using Microsoft.Practices.Unity;

namespace AdventureWorks.Services
{
    /// <summary>
    /// Wrapper class for a dependency injection container.
    /// </summary>
    public static class DI
    {
        #region Members

        /// <summary>
        /// DI container.
        /// </summary>
        private static UnityContainer container = null;

        #endregion
        #region Initialization

        /// <summary>
        /// Initializes the container.
        /// </summary>
        public static void Init()
        {
            container = new UnityContainer();
        }

        #endregion
        #region Container

        /// <summary>
        /// DI container accessor.
        /// </summary>
        public static IUnityContainer Container { get { return container; } }

        #endregion
        #region Methods

        /// <summary>
        /// Registers a type mapping with the container.
        /// </summary>
        public static void RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            container.RegisterType<TFrom, TTo>();
        }

        /// <summary>
        /// Resolves an instance of the default requested type from the container.
        /// </summary>
        public static T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        #endregion
    }
}
