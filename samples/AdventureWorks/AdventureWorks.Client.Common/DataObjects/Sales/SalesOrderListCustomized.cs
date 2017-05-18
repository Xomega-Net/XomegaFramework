using System;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
{
    public class SalesOrderListCustomized : SalesOrderList
    {
        public SalesOrderListCustomized()
        {
        }

        public SalesOrderListCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            SalesOrderIdProperty.IsKey = true;
        }

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            // add custom intialization code here
        }

        // add custom code here
    }
}