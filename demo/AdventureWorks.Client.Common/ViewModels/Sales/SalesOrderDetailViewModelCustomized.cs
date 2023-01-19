using System;

namespace AdventureWorks.Client.Common.ViewModels
{
    public class SalesOrderDetailViewModelCustomized : SalesOrderDetailViewModel
    {
        public override string BaseTitle => GetString("View_Title", MainObj.SalesOrderNumberProperty.Value);

        public SalesOrderDetailViewModelCustomized(IServiceProvider sp) : base(sp)
        {
        }

        // add custom code here
    }
}