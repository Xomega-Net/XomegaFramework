using AdventureWorks.Client.Objects;
using AdventureWorks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.UI.WebControls;
using Xomega.Framework;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Web
{
    public class LoginViewCustomized : LoginView
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            btn_Save.Text = "Login";
            pnlMain.Width = new Unit("500px");

            if (Page.User.Identity.IsAuthenticated)
            {
                // user that is authenticated, but not authorized to view a page will be redirected
                // to this view, so display an appropriate message (instead of the login form) here
                lblLoginViewTitle.Text = "Unauthorized";
                ErrorList el = new ErrorList();
                el.AddError(ErrorType.Security, "You are not authorized to view this page");
                Model.Errors = el;
                pnl_Object.Visible = false;
                btn_Save.Visible = false;
            }
        }

        protected override void OnViewEvents(object sender, ViewEvent e)
        {
            base.OnViewEvents(sender, e);

            if (e.IsSaved()) // authenticated successfully
            {
                DetailsViewModel dvm = sender as DetailsViewModel;
                AuthenticationObject authObj = dvm == null ? null : dvm.DetailsObject as AuthenticationObject;
                ClaimsIdentity ci = null;
                if (authObj != null && authObj.EmailProperty.Value != null)
                {
                    PersonInfo userInfo = ServiceProvider.GetService<IPersonService>().Read(authObj.EmailProperty.Value);
                    ci = SecurityManager.CreateIdentity(CookieAuthenticationDefaults.AuthenticationType, userInfo);
                }
                if (ci != null)
                {
                    HttpContext.Current.GetOwinContext().Authentication.SignIn(ci);
                    string url = HttpContext.Current.Request.QueryString[CookieAuthenticationDefaults.ReturnUrlParameter];
                    if (url != null)
                        Response.Redirect(url);
                }
            }
        }
    }
}