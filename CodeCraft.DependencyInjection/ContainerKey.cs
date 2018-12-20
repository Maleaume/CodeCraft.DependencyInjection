using System;

namespace CodeCraft.DependencyInjection
{
    public struct ContainerKey
    {
        /// <summary>
        /// is not use for HashCode.
        /// </summary>
        public Type ImplementationType { get; set; }
        public Type InterfaceType { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ContainerKey typedObj))
                return false;
            return typedObj.InterfaceType.Equals(InterfaceType) && 
                   typedObj.Name.Equals(Name);
        }

        public override int GetHashCode() =>
            Name.GetHashCode() ^ InterfaceType.GetHashCode();
    }
}
