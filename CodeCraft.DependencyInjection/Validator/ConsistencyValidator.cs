using System;

namespace CodeCraft.DependencyInjection.Validator
{
    class ConsistencyValidator : IConsistencyValidator
    {
        public bool CheckConsistency<Interface, Implementation>()
            where Interface : class
            where Implementation : class
        {
            if (!IsInterface<Interface>()) throw new TypeAccessException();
            if (IsAbstract<Implementation>()) throw new TypeAccessException();
            return IsImplementedBy<Interface, Implementation>();
        }

        private bool IsInterface<Interface>() => typeof(Interface).IsInterface;
        private bool IsAbstract<Implementation>() => typeof(Implementation).IsAbstract;
        private bool IsImplementedBy<Interface, Implementation>()
        {
            var interfaceType = typeof(Interface);
            var implementationType = typeof(Implementation);
            return interfaceType.Equals(implementationType.GetInterface(interfaceType.Name));
        }
    }
}
