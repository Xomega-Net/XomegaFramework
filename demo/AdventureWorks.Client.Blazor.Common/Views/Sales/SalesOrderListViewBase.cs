//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Syncfusion Blazor Views" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using AdventureWorks.Client.Common.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Blazor.Views;
using Xomega.Framework.Views;
using Xomega._Syncfusion.Blazor;

namespace AdventureWorks.Client.Blazor.Common.Views
{
    public partial class SalesOrderListViewBase : XSfSearchView
    {
        [Inject] protected SalesOrderListViewModel VM { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            BindTo(VM);
        }

        public override void BindTo(ViewModel viewModel)
        {
            VM = viewModel as SalesOrderListViewModel;
            base.BindTo(viewModel);
        }

        protected SalesOrderView cvSalesOrderView;

        protected override BlazorView[] ChildViews => new BlazorView[]
        {
            cvSalesOrderView,
        };
    
        protected virtual async Task LinkDetails_ClickAsync(DataRow row, CancellationToken token = default)
        {
            if (VM != null && VM.LinkDetails_Enabled(row))
                await VM.LinkDetails_CommandAsync(cvSalesOrderView, cvSalesOrderView.Visible ? cvSalesOrderView : null, row, token);
        }
    
        protected virtual async Task LinkNew_ClickAsync(CancellationToken token = default)
        {
            if (VM != null && VM.LinkNew_Enabled())
                await VM.LinkNew_CommandAsync(cvSalesOrderView, cvSalesOrderView.Visible ? cvSalesOrderView : null, token);
        }
    }
}
