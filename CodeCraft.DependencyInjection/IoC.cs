using CodeCraft.Core.BaseArchitecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeCraft.DependencyInjection
{
    public class IoC : Singleton<IoC>
    {
        private readonly IDictionary<NamedInterfaces, Type> Contracts = new Dictionary<NamedInterfaces, Type>();
        private readonly IDictionary<NamedInterfaces, Lazy<object>> LazyImplementations = new Dictionary<NamedInterfaces, Lazy<object>>();

        public bool IsRegister<Interface>() => IsRegister<Interface>("default");

        public bool IsRegister<Interface>(string name)
        {
            var registerKey = GenerateRegisterKey<Interface>(name);
            return IsRegister(registerKey);
        }

        private NamedInterfaces GenerateRegisterKey<Interface>(string name)
            => GenerateRegisterKey(typeof(Interface), name);

        private NamedInterfaces GenerateRegisterKey(Type interfaceType, string name)
            => new NamedInterfaces
            {
                InterfaceType = interfaceType,
                Name = name
            };

        private bool IsRegister(NamedInterfaces key)
         => Contracts.ContainsKey(key);

        public void RegisterType<Interface, Implementation>()
           where Interface : class
           where Implementation : class
        {
            RegisterType<Interface, Implementation>("default");
        }

        public void RegisterType<Interface, Implementation>(string name)
            where Interface : class
            where Implementation : class
        {
            if (!CheckConsistency<Interface, Implementation>()) throw new TypeAccessException();

            var registerKey = GenerateRegisterKey<Interface>(name);
            Contracts[registerKey] = typeof(Implementation);
            LazyImplementations[registerKey] = new Lazy<object>(() => Instanciate<Interface>(name));
        }

        bool CheckConsistency<Interface, Implementation>()
        {
            if (!IsInterface<Interface>()) throw new TypeAccessException();
            if (IsAbstract<Implementation>()) throw new TypeAccessException();
            return IsImplementedBy<Interface, Implementation>();
        }


        bool IsInterface<Interface>() => typeof(Interface).IsInterface;
        bool IsAbstract<Implementation>() => typeof(Implementation).IsAbstract;
        bool IsImplementedBy<Interface, Implementation>()
        {
            var interfaceType = typeof(Interface);
            var implementationType = typeof(Implementation);
            return interfaceType.Equals(implementationType.GetInterface(interfaceType.Name));
        }

        public T Resolve<T>() => Resolve<T>("default");

        public T Resolve<T>(string name) => (T)Resolve(typeof(T), name);

        private object Resolve(Type contract, string name)
        {
            var registerKey = GenerateRegisterKey(contract, name);
            return LazyImplementations[registerKey].Value;

        }


        private T Instanciate<T>(string name) => (T)Instanciate(typeof(T), name);

        private object Instanciate(Type contract, string name)
        {
            var registerKey = GenerateRegisterKey(contract, name);
            object instance;
            var implementation = Contracts[registerKey];
            var constructor = implementation.GetConstructors()[0];
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 0)
                instance = Activator.CreateInstance(implementation);
            else
            {
                var parameters = new List<object>(constructorParameters.Length);
                foreach (var parameterInfo in constructorParameters)
                {
                    parameters.Add(ResolveNewInstance(parameterInfo.ParameterType, name));
                }
                instance = constructor.Invoke(parameters.ToArray());
            }

            // Dependencies By Injection.

            return instance;
        }


        public T ResolveNewInstance<T>(string name) => (T)ResolveNewInstance(typeof(T), name);

        private object ResolveNewInstance(Type contract, string name)
        {
            var registerKey = GenerateRegisterKey(contract, name);
            var implementation = Contracts[registerKey];

            object instance = Instanciate(contract, name);

            /////////////////////////////////////

            var t = implementation.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(field => new
                {
                    Field = field,
                    InjectionAttribute = field.GetCustomAttribute<FieldInjectionAttribute>()
                })
                .Where(x => x.InjectionAttribute != null);

            foreach (var kp in t)
            {
                var value = Resolve(kp.Field.FieldType, kp.InjectionAttribute.Name);
                kp.Field.SetValue(instance, value);

            }
            /////////////////////
             
            return instance;
        }


        public void RegisterInstance<Interface>(Interface implementation, string name)
        {
            if (Equals(implementation, default(Interface))) throw new ArgumentNullException("implementation", "Implementation cannot be null");
            var registerKey = GenerateRegisterKey<Interface>(name);
            LazyImplementations[registerKey] = new Lazy<object>(() => implementation);
        }

    }
}
