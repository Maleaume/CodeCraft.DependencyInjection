using CodeCraft.Core.BaseArchitecture;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CodeCraft.DependencyInjection
{
    public class IoC : Singleton<IoC>
    {
        private IDictionary<NamedInterfaces, Type> Contracts = new Dictionary<NamedInterfaces, Type>();
        private IDictionary<NamedInterfaces, object> Instances = new Dictionary<NamedInterfaces, object>();

        public void RegisterType<Interface, Implementation>(string name)
            where Interface : class
            where Implementation : class
        {
            if (!typeof(Interface).IsInterface) throw new TypeAccessException();
            var namedInterface = new NamedInterfaces()
            {
                InterfaceType = typeof(Interface),
                Name = name
            };

            Contracts[namedInterface] = typeof(Implementation);
        }

        public T Resolve<T>(string name) => (T)Resolve(typeof(T), name);
        private object Resolve(Type contract, string name)
        {
            var namedInterface = new NamedInterfaces()
            {
                InterfaceType = contract,
                Name = name
            };

            if (!Instances.ContainsKey(namedInterface))
            {
                Type implementation = Contracts[namedInterface];
                var constructor = implementation.GetConstructors()[0];
                ParameterInfo[] constructorParameters = constructor.GetParameters();
                if (constructorParameters.Length == 0)
                    Instances[namedInterface] = Activator.CreateInstance(implementation);
                else
                {
                    List<object> parameters = new List<object>(constructorParameters.Length);
                    foreach (ParameterInfo parameterInfo in constructorParameters)
                    {
                        parameters.Add(Resolve(parameterInfo.ParameterType, name));
                    }

                    Instances[namedInterface] = constructor.Invoke(parameters.ToArray());
                }
            }
            return Instances[namedInterface];
        }
    }

    public struct NamedInterfaces
    {
        public Type InterfaceType { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is NamedInterfaces typedObj))
                return false;
            return typedObj.InterfaceType.Equals(InterfaceType) &&
                   typedObj.Name.Equals(Name);
        }

        public override int GetHashCode() => Name.GetHashCode() ^ InterfaceType.GetHashCode();
    }
}
