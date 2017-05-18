using System;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
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

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            AddressIdProperty.Change += OnAddressChanged;
        }

        private void OnAddressChanged(object sender, PropertyChangeEventArgs e)
        {
            if (e.Change.IncludesValue() && !DataProperty.Equals(e.OldValue, e.NewValue))
            {
                Header addr = AddressIdProperty.Value;
                AddressLine1Property.SetValue(addr == null ? null : addr[Enumerations.BusinessEntityAddress.Attributes.AddressLine1]);
                AddressLine2Property.SetValue(addr == null ? null : addr[Enumerations.BusinessEntityAddress.Attributes.AddressLine2]);
                CityStateProperty.SetValue(addr == null ? null : addr[Enumerations.BusinessEntityAddress.Attributes.City]
                                                        + ", " + addr[Enumerations.BusinessEntityAddress.Attributes.State]);
                PostalCodeProperty.SetValue(addr == null ? null : addr[Enumerations.BusinessEntityAddress.Attributes.PostalCode]);
                CountryProperty.SetValue(addr == null ? null : addr[Enumerations.BusinessEntityAddress.Attributes.Country]);
            }
        }

    }
}