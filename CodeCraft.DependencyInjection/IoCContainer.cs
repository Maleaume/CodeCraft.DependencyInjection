using System;
using   System.Collections.Generic; 

namespace CodeCraft.DependencyInjection
{
    /// <summary>
    /// Container dictionary used to store all instances.
    /// This class remove properly instance by using dispose method if class implements <see cref="IDisposable"/> interface
    /// </summary>
    public class IoCContainer  
    {
        private readonly Dictionary<NamedInterfaces, (Type, Lazy<object>)> container = new Dictionary<NamedInterfaces, (Type, Lazy<object>)>();

        /// <summary>
        /// override 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (Type ImplementationType, Lazy<object> LazyInstance) this[NamedInterfaces key]
        {
            get { return container[key]; }
            set
            {
                if (container.ContainsKey(key))
                    DisposableRemove(key);
                container[key] = value;
            }
        }

        private void DisposableRemove(NamedInterfaces key)
        {
            (_, var oldLazyInstance) = this[key];
            if (oldLazyInstance.IsValueCreated && oldLazyInstance.Value is IDisposable objDisposable)
                objDisposable.Dispose();
        }
    }
}
