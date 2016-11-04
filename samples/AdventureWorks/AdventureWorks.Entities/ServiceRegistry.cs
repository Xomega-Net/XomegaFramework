using AdventureWorks.Services;

namespace AdventureWorks.Entities
{
    /// <summary>
    /// DI configuration for service implementations.
    /// </summary>
    public static class ServiceRegistry
    {
        /// <summary>
        /// Registers service types with the DI container.
        /// </summary>
        public static void RegisterTypes()
        {
            DI.RegisterType<ISalesOrderService, AdventureWorks.Entities.Services.SalesOrderService>();
            DI.RegisterType<ISalesPersonService, AdventureWorks.Entities.Services.SalesPersonService>();
            DI.RegisterType<ISalesTerritoryService, AdventureWorks.Entities.Services.SalesTerritoryService>();
            DI.RegisterType<IProductService, AdventureWorks.Entities.Services.ProductService>();
            DI.RegisterType<ISpecialOfferService, AdventureWorks.Entities.Services.SpecialOfferService>();
            DI.RegisterType<IShipMethodService, AdventureWorks.Entities.Services.ShipMethodService>();
            DI.RegisterType<ISalesReasonService, AdventureWorks.Entities.Services.SalesReasonService>();
            DI.RegisterType<ICustomerService, AdventureWorks.Entities.Services.CustomerService>();
            // GENPOINT - generated code will be inserted here. DO NOT REMOVE this line!
        }
    }
}
