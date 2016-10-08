
namespace AdventureWorks.Client.Web
{
    static class DependencyInjection
    {
        /// <summary>
        /// Initializes and configures depedency injection container.
        /// </summary>
        public static void Init()
        {
            AdventureWorks.Services.DI.Init();

            AdventureWorks.Entities.ServiceRegistry.RegisterTypes();

        }
    }
}
