using System.Web.Mvc;
using System.Web.Routing;

namespace Aplos
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            try
            {
                AreaRegistration.RegisterAllAreas();
                routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
                routes.LowercaseUrls = true;
                routes.MapRoute(
                    name: "Default",
                    url: "{controller}/{action}/{id}",
                    defaults: new { controller = "Portal", action = "aplos", id = UrlParameter.Optional },
                    namespaces: new string[] { "Aplos.Controllers" }
                );
            }
            catch (System.Exception)
            {

                throw;
            }
            
        }
    }
}