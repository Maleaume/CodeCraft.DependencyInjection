using System;

namespace CodeCraft.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FieldInjectionAttribute : Attribute
    {
        public string Name { get; }

        public FieldInjectionAttribute(string name = "")
            => Name = name;
    }

    public class SingletonInectionAttribute : Attribute { }
}
