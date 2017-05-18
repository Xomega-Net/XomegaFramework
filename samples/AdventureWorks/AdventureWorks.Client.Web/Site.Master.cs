using System;
using System.Web;
using System.Web.UI;

namespace AdventureWorks.Client.Web
{
    public partial class SiteMaster : MasterPage
    {
        protected void HeadLoginStatus_LoggedOut(object sender, EventArgs e)
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut();
            if (Session != null) Session.Clear();
        }
    }
}