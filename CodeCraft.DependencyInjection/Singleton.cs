namespace CodeCraft.DependencyInjection
{
    /// <summary>
    /// Abstract base class of any Singleton.
    /// Common code for creation and retrieve instance is here
    /// </summary>
    /// <typeparam name="T">object </typeparam>
    public abstract class Singleton<T> : SingletonCollectionBase where T : new()
    {
        /// <summary>
        /// Retrieve or Create new single instance of T object.
        /// </summary>
        public static T Instance => (T)instances.GetOrAdd(typeof(T), new T());
    }
}
