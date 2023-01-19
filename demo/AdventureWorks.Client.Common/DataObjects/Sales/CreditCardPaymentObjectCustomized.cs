using System;
using AdventureWorks.Services.Common.Enumerations;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Common.DataObjects
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

        // perform post initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            CreditCardIdProperty.Change += OnCreditCardChanged;
        }

        private void OnCreditCardChanged(object sender, PropertyChangeEventArgs e)
        {
            if (e.Change.IncludesValue() && !Equals(e.OldValue, e.NewValue))
            {
                Header cc = CreditCardIdProperty.Value;
                CardNumberProperty.SetValue(cc?[PersonCreditCard.Attributes.CardNumber]);
                ExpirationProperty.SetValue(cc == null ? null : cc[PersonCreditCard.Attributes.ExpMonth]
                                                        + "/" + cc[PersonCreditCard.Attributes.ExpYear]);
            }
        }
    }
}