using Aplos.App_Start;
using Library.Crosscutting.Security;
using System;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Aplos.Helpers;
using Library.Core;
using Library.Service.Core;

namespace Aplos
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //GlobalConfiguration.Configuration.MessageHandlers.Add(new CorsHandler());

            try
            {
                for (int i = 0; i < ValueProviderFactories.Factories.Count; i++)
                {
                    if (ValueProviderFactories.Factories[i].GetType() == typeof(System.Web.Mvc.JsonValueProviderFactory))
                    {
                        ValueProviderFactories.Factories.RemoveAt(i);
                        break;
                    }
                }

                ValueProviderFactories.Factories.Add(new AplosJsonValueProviderFactory());
            }
            catch (Exception ex)
            {

            }

        }
        protected void Application_AcquireRequestState(object sender, EventArgs e)

        {
            try
            {
                var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null || authCookie.Value == "") return;
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket == null) return;
                var identity = new CustomIdentity(ticket.UserData);
                var basicTicket = Application["BasicTicket" + identity.Name];
                var roleTicket = Application["RoleTicket" + identity.Name];
                if (basicTicket != null && roleTicket != null && basicTicket.ToString() == ticket.UserData)
                {
                    identity.SetRoles(roleTicket.ToString());
                    var principal = new CustomPrincipal(identity);
                    HttpContext.Current.User = principal;
                    Thread.CurrentPrincipal = principal;
                    var timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
                    var authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), ticket.IsPersistent, ticket.UserData);
                    var encTicket = FormsAuthentication.Encrypt(authTicket);
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
                }
                else
                {
                    authCookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Current.Response.Cookies.Add(authCookie);
                    Application["BasicTicket" + identity.Name] = null;
                    Application["RoleTicket" + identity.Name] = null;
                    HttpContext.Current.Response.Redirect("/");
                }
            }
            catch (Exception)
            {
                FormsAuthentication.SignOut();
            }
        }
    }
}
