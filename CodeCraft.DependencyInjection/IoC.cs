using CodeCraft.Core.BaseArchitecture;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeCraft.DependencyInjection
{
    public class IoC : Singleton<IoC>
    {
        private IDictionary<NamedInterfaces, Type> Contracts = new Dictionary<NamedInterfaces, Type>();
        private IDictionary<NamedInterfaces, object> Implementations = new Dictionary<NamedInterfaces, object>();

        public bool IsRegister<Interface>()
         => IsRegister<Interface>("default");

        public bool IsRegister<Interface>(string name)
            => IsRegister(new NamedInterfaces()
            {
                InterfaceType = typeof(Interface),
                Name = name
            });

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

            var namedInterface = new NamedInterfaces()
            {
                InterfaceType = typeof(Interface),
                Name = name
            };
            if (!Contracts.ContainsKey(namedInterface))
                Contracts[namedInterface] = typeof(Implementation);
        }

        bool CheckConsistency<Interface, Implementation>()
        {
            Type interfaceType = typeof(Interface);
            Type implementationType = typeof(Implementation);
            if (!interfaceType.IsInterface) throw new TypeAccessException();
            if (implementationType.IsAbstract) throw new TypeAccessException();
            return interfaceType.Equals(implementationType.GetInterface(interfaceType.Name));
        }

        public T Resolve<T>() => Resolve<T>("default");

        public T Resolve<T>(string name) => (T)Resolve(typeof(T), name);

        private object Resolve(Type contract, string name)
        {
            var namedInterface = new NamedInterfaces()
            {
                InterfaceType = contract,
                Name = name
            };

            if (!Implementations.ContainsKey(namedInterface))
            {
                Type implementation = Contracts[namedInterface];
                var constructor = implementation.GetConstructors()[0];
                ParameterInfo[] constructorParameters = constructor.GetParameters();
                if (constructorParameters.Length == 0)
                    Implementations[namedInterface] = Activator.CreateInstance(implementation);
                else
                {
                    List<object> parameters = new List<object>(constructorParameters.Length);
                    foreach (ParameterInfo parameterInfo in constructorParameters)
                    {
                        parameters.Add(Resolve(parameterInfo.ParameterType, name));
                    }

                    Implementations[namedInterface] = constructor.Invoke(parameters.ToArray());
                }
            }
            return Implementations[namedInterface];
        }
    }
}
