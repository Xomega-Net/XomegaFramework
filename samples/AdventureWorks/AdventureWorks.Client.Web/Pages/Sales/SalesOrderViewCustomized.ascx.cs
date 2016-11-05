using System;

namespace AdventureWorks.Client.Web
{
    public class SalesOrderViewCustomized : SalesOrderView
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // add custom initialization here
            ctlSalesTerritoryId.AutoPostBack = true;
            ctlBillingAddressAddressId.AutoPostBack = true;
            ctlShippingAddressAddressId.AutoPostBack = true;
        }

        // add custom code here
    }
}