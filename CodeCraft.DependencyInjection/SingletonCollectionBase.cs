using System;
using System.Collections.Concurrent;

namespace CodeCraft.DependencyInjection
{
    /// <summary>
    /// Abstract base class use to store all singleton instance function of type.
    /// </summary>
    public abstract class SingletonCollectionBase
    {
        /// <summary>
        /// Dictionary between type of singleton and single instance of it.
        /// </summary>
        protected readonly static ConcurrentDictionary<Type, object> instances = new ConcurrentDictionary<Type, object>();
    }
}
