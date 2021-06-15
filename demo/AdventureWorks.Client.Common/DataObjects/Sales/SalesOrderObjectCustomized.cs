using AdventureWorks.Enumerations;
using AdventureWorks.Services;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework;

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

            //Expression<Func<DataProperty, bool>> objEditable = (status) => status.IsNull(null) || 
            //    !status.GetValue(ValueFormat.Transport, null).ToString().Equals(SalesOrderStatus.Shipped);
            //SetComputedEditable(objEditable, StatusProperty);

            CustomerObject.LookupObject.TrackModifications = false;

            var ccProperty = PaymentObject.CreditCardObject.CreditCardIdProperty;
            ccProperty.LocalCacheLoader = new PersonCreditCardReadListCacheLoader(ServiceProvider);
            ccProperty.SetCacheLoaderParameters(PersonCreditCard.Parameters.BusinessEntityId, CustomerObject.PersonIdProperty);
        }

        public override async Task FromDataContractAsync(object dataContract, object options, CancellationToken token = default)
        {
            await base.FromDataContractAsync(dataContract, options, token);
            if (!StatusProperty.IsNull() && StatusProperty.Value.Id == SalesOrderStatus.Shipped)
                Editable = false;
        }
    }
}