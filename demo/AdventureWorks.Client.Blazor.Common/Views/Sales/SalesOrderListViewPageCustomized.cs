using Microsoft.AspNetCore.Authorization;

namespace AdventureWorks.Client.Blazor.Views
{
    [Authorize(Policy = "Sales")]
    public partial class SalesOrderListViewPage
    {
    }
}