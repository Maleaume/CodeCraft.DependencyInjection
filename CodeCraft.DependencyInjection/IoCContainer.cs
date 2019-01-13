using System;
using System.Collections.Generic;


namespace CodeCraft.DependencyInjection
{
    using IoCDictionary = Dictionary<ContainerKey, (Type, Lazy<object>)>;
    /// <summary>
    /// Container dictionary used to store all instances.
    /// This class remove properly instance by using dispose method if class implements <see cref="IDisposable"/> interface
    /// </summary>
    public class IoCContainer
    {
        private readonly IoCDictionary container = new IoCDictionary();

        public bool IsRegistered(ContainerKey key, string name = "default")
            => container.ContainsKey(key);

        /// <summary>
        /// Use [] operator to simplify code writting, and wrapped those of <see cref="container"/> dictionary.
        /// </summary>
        /// <param name="key">
        /// The key of the value to get or set.</param>
        /// <returns> The value associated with the specified key. If the specified key is not found,
        /// a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        /// a set operation creates a new element with the specified key.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">  
        /// The property is retrieved and key does not exist in the collection.
        /// </exception>
        public (Type ImplementationType, Lazy<object> LazyInstance) this[ContainerKey key]
        {
            get { return container[key]; }
            set
            {
                if (container.ContainsKey(key))
                    DisposableRemove(key);
                container[key] = value;
            }
        } 

        private void DisposableRemove(ContainerKey key)
        {
            (_, var oldLazyInstance) = container[key];
            if (oldLazyInstance.IsValueCreated && oldLazyInstance.Value is IDisposable objDisposable)
                objDisposable.Dispose();
        }
    }
}
 