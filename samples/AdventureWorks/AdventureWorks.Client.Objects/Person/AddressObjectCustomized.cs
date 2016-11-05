using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
{
    public class AddressObjectCustomized : AddressObject
    {
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
            if (!e.Change.IncludesValue() || DataProperty.Equals(e.OldValue, e.NewValue)) return;

            Header h = AddressIdProperty.Value;
            AddressLine1Property.SetValue(h == null ? null : h[Enumerations.BusinessEntityAddress.Attributes.AddressLine1]);
            AddressLine2Property.SetValue(h == null ? null : h[Enumerations.BusinessEntityAddress.Attributes.AddressLine2]);
            CityStateProperty.SetValue(h == null ? null : h[Enumerations.BusinessEntityAddress.Attributes.City]
                                                 + ", " + h[Enumerations.BusinessEntityAddress.Attributes.State]);
            PostalCodeProperty.SetValue(h == null ? null : h[Enumerations.BusinessEntityAddress.Attributes.PostalCode]);
            CountryProperty.SetValue(h == null ? null : h[Enumerations.BusinessEntityAddress.Attributes.Country]);
        }
    }
}