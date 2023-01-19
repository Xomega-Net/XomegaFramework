using System;

namespace AdventureWorks.Client.Common.ViewModels
{
    public class SalesOrderViewModelCustomized : SalesOrderViewModel
    {
        public SalesOrderViewModelCustomized(IServiceProvider sp) : base(sp)
        {
        }

        public override string BaseTitle => base.BaseTitle +
            (MainObj.IsNew ? "" : " - " + MainObj.SalesOrderNumberProperty.Value);
    }
}