using System;

namespace CodeCraft.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class InjectionAttribute : Attribute
    {
       
        public string Name { get; }
        public InjectionType InjectionType { get; }
        public InjectionAttribute( string name = "default", InjectionType injectionType = InjectionType.NewInstance)
        {
            Name = name;
            InjectionType = injectionType;    
        }
    }
    
    public enum InjectionType
    {
        NewInstance,
        Singleton
    }
}
