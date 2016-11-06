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
        private CreditCardLoader creditCardLoader;

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            creditCardLoader = new CreditCardLoader(this);
        }

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            CustomerObject.PersonIdProperty.Change += creditCardLoader.OnCustomerPersonChanged;
        }

        #region CreditCardLoader class

        class CreditCardLoader : PersonCreditCardReadListCacheLoader
        {
            private SalesOrderObject salesOrder;

            public CreditCardLoader(SalesOrderObject salesOrder)
            {
                this.salesOrder = salesOrder;
            }

            protected override IEnumerable<PersonCreditCard_ReadListOutput> ReadList()
            {
                IPersonCreditCardService svc = DI.Resolve<IPersonCreditCardService>();
                IEnumerable<PersonCreditCard_ReadListOutput> ccList = svc.ReadList(
                    salesOrder.CustomerObject.PersonIdProperty.Value.Value);
                if (svc is IDisposable) ((IDisposable)svc).Dispose();
                return ccList;
            }

            public void OnCustomerPersonChanged(object sender, PropertyChangeEventArgs e)
            {
                if (!e.Change.IncludesValue() || DataProperty.Equals(e.OldValue, e.NewValue)
                     || salesOrder.CustomerObject.PersonIdProperty.Value == null) return;

                LoadCache(Enumerations.PersonCreditCard.EnumName, delegate (LookupTable tbl)
                {
                    EnumProperty p = salesOrder.PaymentObject.CreditCardObject.CreditCardIdProperty;
                    p.SetValue(null);
                    p.LocalLookupTable = tbl;
                    p.FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Items, null, null));
                });
            }
        }
        #endregion
    }
}