using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using AdventureWorks.Services.Common;
using Xomega.Framework;
using Xomega.Framework.Lookup;
using Xomega.Framework.Properties;
using AdventureWorks.Services.Common.Enumerations;

namespace AdventureWorks.Client.Common.DataObjects
{
    public class SalesOrderCustomerObjectCustomized : SalesOrderCustomerObject
    {
        private LocalLookupCacheLoader AddressLoader;

        public SalesOrderCustomerObjectCustomized()
        {
        }

        public SalesOrderCustomerObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
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

            AddressLoader = new BusinessEntityAddressReadEnumCacheLoader(ServiceProvider);
            BillingAddressObject.AddressIdProperty.LocalCacheLoader = AddressLoader;
            ShippingAddressObject.AddressIdProperty.LocalCacheLoader = AddressLoader;

            StoreIdProperty.AsyncChange += OnCustomerChanged;
            PersonIdProperty.AsyncChange += OnCustomerChanged;
        }

        private async Task OnCustomerChanged(object sender, PropertyChangeEventArgs e, CancellationToken token)
        {
            if (!e.Change.IncludesValue() || Equals(e.OldValue, e.NewValue) ||
                PersonIdProperty.Value == null && StoreIdProperty.Value == null) return;

            var entityId = StoreIdProperty.IsNull() ? // use store or person id
                PersonIdProperty.TransportValue : StoreIdProperty.TransportValue;

            await AddressLoader.SetParametersAsync(new Dictionary<string, object>() {
                { BusinessEntityAddress.Parameters.BusinessEntityId, entityId }
            }, AddressLoader.LocalCache, token);

            await BillingAddressObject.AddressIdProperty.ClearInvalidValues();
            await ShippingAddressObject.AddressIdProperty.ClearInvalidValues();

            var args = new PropertyChangeEventArgs(PropertyChange.Items, null, null, e.Row);
            await BillingAddressObject.AddressIdProperty.FirePropertyChangeAsync(args, token);
            await ShippingAddressObject.AddressIdProperty.FirePropertyChangeAsync(args, token);
        }
    }
}