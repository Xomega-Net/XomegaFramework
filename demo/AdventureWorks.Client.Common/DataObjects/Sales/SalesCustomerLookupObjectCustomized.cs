using System;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Common.DataObjects
{
    public class SalesCustomerLookupObjectCustomized : SalesCustomerLookupObject
    {
        public SalesCustomerLookupObjectCustomized()
        {
        }

        public SalesCustomerLookupObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            TrackModifications = false;
        }

        // perform post initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            // add custom initialization code here
        }

        // add custom code here
    }
}