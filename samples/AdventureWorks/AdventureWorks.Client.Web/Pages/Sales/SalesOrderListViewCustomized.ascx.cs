using System;

namespace AdventureWorks.Client.Web
{
    public class SalesOrderListViewCustomized : SalesOrderListView
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ctlGlobalRegion.AutoPostBack = true;
            ctlTerritoryId.AutoPostBack = true;

            // add custom initialization here
        }

        // add custom code here
    }
}