using System;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
{
    public class CreditCardPaymentObjectCustomized : CreditCardPaymentObject
    {
        public CreditCardPaymentObjectCustomized()
        {
        }

        public CreditCardPaymentObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
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
            CreditCardIdProperty.Change += OnCreditCardChanged;
        }

        private void OnCreditCardChanged(object sender, PropertyChangeEventArgs e)
        {
            if (e.Change.IncludesValue() && !DataProperty.Equals(e.OldValue, e.NewValue))
            {
                Header cc = CreditCardIdProperty.Value;
                CardNumberProperty.SetValue(cc == null ? null : cc[Enumerations.PersonCreditCard.Attributes.CardNumber]);
                ExpirationProperty.SetValue(cc == null ? null : cc[Enumerations.PersonCreditCard.Attributes.ExpMonth]
                                                        + "/" + cc[Enumerations.PersonCreditCard.Attributes.ExpYear]);
            }
        }

        // add custom code here
    }
}