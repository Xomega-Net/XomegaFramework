using System;
using AdventureWorks.Services.Common;
using AdventureWorks.Services.Common.Enumerations;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Common.DataObjects
{
    public class SalesOrderObjectCustomized : SalesOrderObject
    {
        public SalesOrderObjectCustomized()
        {
        }

        public SalesOrderObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
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
            var ccProp = PaymentObject.CreditCardObject.CreditCardIdProperty;
            ccProp.LocalCacheLoader = new PersonCreditCardReadEnumCacheLoader(ServiceProvider);
            ccProp.SetCacheLoaderParameters(PersonCreditCard.Parameters.BusinessEntityId,
                                            CustomerObject.PersonIdProperty);
        }

        // add custom code here
    }
}