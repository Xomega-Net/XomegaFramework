using AdventureWorks.Services;
using System;
using System.Collections.Generic;
using Xomega.Framework;
using Xomega.Framework.Lookup;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
{
    public class SalesOrderCustomerObjectCustomized : SalesOrderCustomerObject
    {
        private AddressLoader addressLoader;

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            addressLoader = new AddressLoader(this);
        }

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            StoreIdProperty.Change += addressLoader.OnCustomerChanged;
            PersonIdProperty.Change += addressLoader.OnCustomerChanged;
        }

        #region AddressLoader class

        class AddressLoader : BusinessEntityAddressReadListCacheLoader
        {
            private SalesOrderCustomerObject customer;

            public AddressLoader(SalesOrderCustomerObject customer)
            {
                this.customer = customer;
            }

            protected override IEnumerable<BusinessEntityAddress_ReadListOutput> ReadList()
            {
                IBusinessEntityAddressService svc = DI.Resolve<IBusinessEntityAddressService>();
                int businessEntityId = customer.StoreIdProperty.Value == null ?
                    customer.PersonIdProperty.Value.Value : customer.StoreIdProperty.Value.Value;
                IEnumerable<BusinessEntityAddress_ReadListOutput> addrList = svc.ReadList(businessEntityId);
                if (svc is IDisposable) ((IDisposable)svc).Dispose();
                return addrList;
            }

            public void OnCustomerChanged(object sender, PropertyChangeEventArgs e)
            {
                if (!e.Change.IncludesValue() || DataProperty.Equals(e.OldValue, e.NewValue) ||
                    customer.PersonIdProperty.Value == null && customer.StoreIdProperty.Value == null) return;

                LoadCache(Enumerations.BusinessEntityAddress.EnumName, delegate (LookupTable tbl)
                {
                    UpdateProperty(tbl, customer.BillingAddressObject.AddressIdProperty);
                    UpdateProperty(tbl, customer.ShippingAddressObject.AddressIdProperty);
                });
            }

            private void UpdateProperty(LookupTable tbl, EnumProperty ep)
            {
                ep.SetValue(null);
                ep.LocalLookupTable = tbl;
                ep.FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Items, null, null));
            }
        }

        #endregion
    }
}