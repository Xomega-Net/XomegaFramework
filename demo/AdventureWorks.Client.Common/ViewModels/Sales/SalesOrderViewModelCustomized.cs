using System;
using System.Collections.Specialized;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.ViewModels
{
    public class SalesOrderViewModelCustomized : SalesOrderViewModel
    {
        public SalesOrderViewModelCustomized(IServiceProvider sp) : base(sp)
        {
        }

        public override NameValueCollection LinkCustomerLookupLookUp_Params()
        {
            var res = base.LinkCustomerLookupLookUp_Params();
            //res[ViewParams.Mode.Param] = ViewParams.Mode.Inline;
            return res;
        }

        public override string BaseTitle => base.BaseTitle +
            (MainObj.IsNew ? "" : " - " + MainObj.SalesOrderNumberProperty.Value);
    }
}