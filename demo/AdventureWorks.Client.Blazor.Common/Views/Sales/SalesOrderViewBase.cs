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
    public partial class SalesOrderViewBase : XSfDetailsView
    {
        [Inject] protected SalesOrderViewModel VM { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            BindTo(VM);
        }

        public override void BindTo(ViewModel viewModel)
        {
            VM = viewModel as SalesOrderViewModel;
            base.BindTo(viewModel);
        }

        protected CustomerListView cvCustomerListView;
        protected SalesOrderDetailView cvSalesOrderDetailView;

        protected override BlazorView[] ChildViews => new BlazorView[]
        {
            cvCustomerListView,
            cvSalesOrderDetailView,
        };
    
        protected virtual async Task LinkCustomerLookupLookUp_ClickAsync(CancellationToken token = default)
        {
            if (VM != null && VM.LinkCustomerLookupLookUp_Enabled())
                await VM.LinkCustomerLookupLookUp_CommandAsync(cvCustomerListView, cvCustomerListView.Visible ? cvCustomerListView : null, token);
        }
    
        protected virtual async Task LinkDetailDetails_ClickAsync(DataRow row, CancellationToken token = default)
        {
            if (VM != null && VM.LinkDetailDetails_Enabled(row))
                await VM.LinkDetailDetails_CommandAsync(cvSalesOrderDetailView, cvSalesOrderDetailView.Visible ? cvSalesOrderDetailView : null, row, token);
        }
    
        protected virtual async Task LinkDetailNew_ClickAsync(CancellationToken token = default)
        {
            if (VM != null && VM.LinkDetailNew_Enabled())
                await VM.LinkDetailNew_CommandAsync(cvSalesOrderDetailView, cvSalesOrderDetailView.Visible ? cvSalesOrderDetailView : null, token);
        }
    }
}