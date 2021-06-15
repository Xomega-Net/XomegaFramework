using System;
using System.Collections.Specialized;
using Xomega.Framework;

namespace AdventureWorks.Client.ViewModels
{
    public class SalesOrderListViewModelCustomized : SalesOrderListViewModel
    {
        public SalesOrderListViewModelCustomized(IServiceProvider sp) : base(sp)
        {
        }

        public override NameValueCollection LinkDetails_Params(DataRow row)
        {
            var res = base.LinkDetails_Params(row);
            //res[ViewParams.Mode.Param] = ViewParams.Mode.Popup;
            return res;
        }
    }
}