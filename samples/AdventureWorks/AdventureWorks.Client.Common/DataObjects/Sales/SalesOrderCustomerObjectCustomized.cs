using AdventureWorks.Services;
using System;
using Xomega.Framework;
using System.Collections.Generic;
using Xomega.Framework.Lookup;

namespace AdventureWorks.Client.Objects
{
    public class SalesOrderCustomerObjectCustomized : SalesOrderCustomerObject
    {
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
            new AddressLoader(this);
        }

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            // add custom intialization code here
        }

        #region AddressLoader

        class AddressLoader : BusinessEntityAddressReadListCacheLoader
        {
            private SalesOrderCustomerObject customer;
            public AddressLoader(SalesOrderCustomerObject customer) : base(customer.ServiceProvider)
            {
                this.customer = customer;
                this.customer.StoreIdProperty.Change += OnCustomerChanged;
                this.customer.PersonIdProperty.Change += OnCustomerChanged;
            }
            protected override IEnumerable<BusinessEntityAddress_ReadListOutput> ReadList(int _businessEntityId)
            {
                int id = customer.StoreIdProperty.Value == null ? // use store or person id
                    customer.PersonIdProperty.Value.Value : customer.StoreIdProperty.Value.Value;
                return base.ReadList(id);
            }
            private void OnCustomerChanged(object sender, PropertyChangeEventArgs e)
            { // if store or person id value changed and one of them is set
                if (!e.Change.IncludesValue() || DataProperty.Equals(e.OldValue, e.NewValue) ||
                    customer.PersonIdProperty.Value == null && customer.StoreIdProperty.Value == null) return;
                
                // reload lookup table for bill-to and ship-to addresses
                LoadCache(Enumerations.BusinessEntityAddress.EnumName, delegate (LookupTable tbl)
                {
                    customer.BillingAddressObject.AddressIdProperty.SetLookupTable(tbl);
                    customer.ShippingAddressObject.AddressIdProperty.SetLookupTable(tbl);
                });
            }
        }
        #endregion
    }
}