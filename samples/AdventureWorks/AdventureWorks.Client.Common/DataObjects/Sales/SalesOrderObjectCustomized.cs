using AdventureWorks.Services;
using System;
using System.Collections.Generic;
using Xomega.Framework;
using Xomega.Framework.Lookup;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
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

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            new PersonCreditCardLoader(this);
            // add custom intialization code here
        }

        // add custom code here
    }

    #region PersonCreditCardLoader

    class PersonCreditCardLoader : PersonCreditCardReadListCacheLoader
    {
        private SalesOrderObject salesOrder;

        public PersonCreditCardLoader(SalesOrderObject salesOrder) : base(salesOrder.ServiceProvider)
        {
            this.salesOrder = salesOrder;
            this.salesOrder.CustomerObject.PersonIdProperty.Change += OnCustomerPersonChanged;
        }

        protected override IEnumerable<PersonCreditCard_ReadListOutput> ReadList(int _businessEntityId)
        {
            return base.ReadList(salesOrder.CustomerObject.PersonIdProperty.Value.Value);
        }

        private void OnCustomerPersonChanged(object sender, PropertyChangeEventArgs e)
        {
            if (!e.Change.IncludesValue() || DataProperty.Equals(e.OldValue, e.NewValue) ||
                salesOrder.CustomerObject.PersonIdProperty.Value == null) return;

            LoadCache(Enumerations.PersonCreditCard.EnumName, delegate (LookupTable tbl)
            {
                salesOrder.PaymentObject.CreditCardObject.CreditCardIdProperty.SetLookupTable(tbl);
            });
        }
    }

    #endregion

}