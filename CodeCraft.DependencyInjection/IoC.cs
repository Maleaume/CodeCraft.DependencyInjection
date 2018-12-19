using CodeCraft.Core.BaseArchitecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeCraft.DependencyInjection
{

    public class IoCDictionary : Dictionary<NamedInterfaces, (Type, Lazy<object>)>
    {
        public new(Type ImplementationType, Lazy<object> LazyInstance) this[NamedInterfaces key]
        {
            get { return base[key]; }
            set
            {
                if (ContainsKey(key))
                    DisposableRemove(key);
                base[key] = value;
            }
        }

        private void DisposableRemove(NamedInterfaces key)
        {
            (_, var oldLazyInstance) = this[key];
            if (oldLazyInstance.IsValueCreated && oldLazyInstance.Value is IDisposable objDisposable)
                objDisposable.Dispose();
        }

    }


    public class IoC : Singleton<IoC>
    {
      private readonly IoCDictionary LazyImplementations = new IoCDictionary();

        /* public bool IsRegister<Interface>() => IsRegister<Interface>("default");

               public bool IsRegister<Interface>(string name)
               {
                   var registerKey = GenerateRegisterKey<Interface>(name);
                   return IsRegister(registerKey);
               }



               private bool IsRegister(NamedInterfaces key)
                   => Contracts.ContainsKey(key);*/
        private NamedInterfaces GenerateRegisterKey<Interface>(string name)
           => GenerateRegisterKey(typeof(Interface), name);

        private NamedInterfaces GenerateRegisterKey(Type interfaceType, string name)
            => new NamedInterfaces
            {
                InterfaceType = interfaceType,
                Name = name
            };



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
            var implementationType = typeof(Implementation);
            var lazyInstance = new Lazy<object>(() => Instanciate<Interface>(name));

            LazyImplementations[registerKey] = (implementationType, lazyInstance);
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

        public T Resolve<T>(string name) => (T)Resolve(GenerateRegisterKey<T>(name));

        private object Resolve(NamedInterfaces registerKey) => LazyImplementations[registerKey].LazyInstance.Value;

        private T Instanciate<T>(string name) => (T)Instanciate(GenerateRegisterKey<T>(name));

        private object Instanciate(NamedInterfaces registerKey)
        {
            object instance;
            var implementation = LazyImplementations[registerKey].ImplementationType;
            var constructor = implementation.GetConstructors()[0];
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 0)
                instance = Activator.CreateInstance(implementation);
            else
            {
                var parameters = new List<object>(constructorParameters.Length);
                foreach (var parameterInfo in constructorParameters)
                {
                    var registerParamKey = GenerateRegisterKey(parameterInfo.ParameterType, registerKey.Name);
                    parameters.Add(ResolveNewInstance(registerParamKey));
                }
                instance = constructor.Invoke(parameters.ToArray());
            }
            // Dependencies By Injection. 
            return instance;
        }

        public T ResolveNewInstance<T>(string name) => (T)ResolveNewInstance(GenerateRegisterKey<T>(name));

        private object ResolveNewInstance(NamedInterfaces registerKey)
        {

            var implementation = LazyImplementations[registerKey].ImplementationType;
            object instance = Instanciate(registerKey);

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
                var registerParamKey = GenerateRegisterKey(kp.Field.FieldType,   kp.InjectionAttribute.Name);
                var value = Resolve(registerParamKey);
                kp.Field.SetValue(instance, value);
            }
            ///////////////////// 
            return instance;
        }

        public void RegisterInstance<Interface>(Interface implementation, string name)
        {
            if (Equals(implementation, default(Interface))) throw new ArgumentNullException("implementation", "Implementation cannot be null");
            var registerKey = GenerateRegisterKey<Interface>(name);
            LazyImplementations[registerKey] = ( implementation.GetType(), new Lazy<object>(() => implementation));
        }
    }
}
