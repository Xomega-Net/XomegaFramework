using System;

namespace AdventureWorks.Client.Objects
{
    public class SalesOrderCriteriaCustomized : SalesOrderCriteria
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
            TerritoryIdProperty.SetCascadingProperty(Enumerations.SalesTerritory.Attributes.Group, GlobalRegionProperty);
            SalesPersonIdProperty.SetCascadingProperty(Enumerations.SalesPerson.Attributes.TerritoryId, TerritoryIdProperty);

            // add custom intialization code here
        }

        public override void Validate(bool force)
        {
            base.Validate(force);
            DateTime? orderDateFrom = OrderDateProperty.Value;
            DateTime? orderDateTo = OrderDate2Property.Value;
            if (orderDateFrom != null && orderDateTo != null && orderDateTo < orderDateFrom)
                validationErrorList.AddError("From Order Date should be earlier than To Order Date");
        }

        // add custom code here
    }
}