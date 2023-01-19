using System;
using AdventureWorks.Services.Common.Enumerations;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Common.DataObjects
{
    public class AddressObjectCustomized : AddressObject
    {
        public AddressObjectCustomized()
        {
        }

        public AddressObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
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
            AddressIdProperty.Change += OnAddressChanged;
        }

        private void OnAddressChanged(object sender, PropertyChangeEventArgs e)
        {
            if (!e.Change.IncludesValue() || Equals(e.OldValue, e.NewValue)) return;

            Header addr = AddressIdProperty.Value;
            AddressLine1Property.SetValue(addr?[BusinessEntityAddress.Attributes.AddressLine1]);
            AddressLine2Property.SetValue(addr?[BusinessEntityAddress.Attributes.AddressLine2]);
            CityStateProperty.SetValue(addr == null ? null : addr[BusinessEntityAddress.Attributes.City]
                                                    + ", " + addr[BusinessEntityAddress.Attributes.State]);
            PostalCodeProperty.SetValue(addr?[BusinessEntityAddress.Attributes.PostalCode]);
            CountryProperty.SetValue(addr?[BusinessEntityAddress.Attributes.Country]);
        }
    }
}