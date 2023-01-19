using System;
using AdventureWorks.Services.Common;
using AdventureWorks.Services.Common.Enumerations;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Common.DataObjects
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

        // perform post initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            StatusProperty.DisplayFormat = $"{Header.FieldId} - {Header.FieldText}";
            TerritoryIdProperty.SetCascadingProperty(SalesTerritory.Attributes.Group, GlobalRegionProperty);
            SalesPersonIdProperty.SetCascadingProperty(SalesPerson.Attributes.TerritoryId, TerritoryIdProperty);
            SalesPersonIdProperty.NullsMatchAnyCascading = true;
            SalesPersonIdProperty.DisplayListSeparator = "; ";
        }

        public override void Validate(bool force)
        {
            base.Validate(force);
            DateTime? orderDateFrom = OrderDateProperty.Value;
            DateTime? orderDateTo = OrderDate2Property.Value;
            if (orderDateFrom != null && orderDateTo != null && orderDateFrom > orderDateTo)
                validationErrorList.AddValidationError(Messages.OrderFromToDate);
        }
    }
}