using System;
using Xomega.Framework;
using Xomega.Framework.Properties;
using AdventureWorks.Services.Common.Enumerations;

namespace AdventureWorks.Client.Common.DataObjects
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

        // perform post initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            SalesPersonIdProperty.SetCascadingProperty(SalesPerson.Attributes.TerritoryId, TerritoryIdProperty);
        }

        // add custom code here
    }
}