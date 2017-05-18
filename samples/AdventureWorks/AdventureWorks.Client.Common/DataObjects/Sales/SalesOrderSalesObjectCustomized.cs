using System;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
{
    public class SalesOrderSalesObjectCustomized : SalesOrderSalesObject
    {
        public SalesOrderSalesObjectCustomized()
        {
        }

        public SalesOrderSalesObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            // add custom construction code here
        }

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            SalesPersonIdProperty.SetCascadingProperty(Enumerations.SalesPerson.Attributes.TerritoryId, TerritoryIdProperty);
            // add custom intialization code here
        }

        // add custom code here
    }
}