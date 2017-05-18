using System.Web.UI;
using System.Web.UI.WebControls;
using Xomega.Framework;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Web
{
    public partial class Errors : UserControl, IErrorPresenter
    {
        public ListView List { get { return errorList; } }

        public void Show(ErrorList errors)
        {
            List.DataSource = errors != null ? errors.Errors : null;
            List.DataBind();
        }
    }
}