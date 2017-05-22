using AdventureWorks.Services;
using System;
using System.Threading;
using Xomega.Framework;

namespace AdventureWorks.Client.Objects
{
    public class SalesOrderCriteriaCustomized : SalesOrderCriteria
    {
        public SalesOrderCriteriaCustomized()
        {
        }

        public SalesOrderCriteriaCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
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
            StatusProperty.DisplayFormat = Header.FieldId + " - " + Header.FieldText;
            TerritoryIdProperty.SetCascadingProperty(Enumerations.SalesTerritory.Attributes.Group, GlobalRegionProperty);
            SalesPersonIdProperty.SetCascadingProperty(Enumerations.SalesPerson.Attributes.TerritoryId, TerritoryIdProperty);

            if (Thread.CurrentPrincipal.IsStoreContact() || Thread.CurrentPrincipal.IsIndividualCustomer())
            {
                CustomerStoreOperatorProperty.AccessLevel = AccessLevel.None;
                CustomerNameOperatorProperty.AccessLevel = AccessLevel.None;
            }
        }

        public override void Validate(bool force)
        {
            base.Validate(force);
            DateTime? orderDateFrom = OrderDateProperty.Value;
            DateTime? orderDateTo = OrderDate2Property.Value;
            if (orderDateFrom != null && orderDateTo != null && orderDateTo < orderDateFrom)
                validationErrorList.AddError(ErrorType.Validation, "From Order Date should be earlier than To Order Date");
        }
    }
}