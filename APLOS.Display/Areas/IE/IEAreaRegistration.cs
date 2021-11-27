using System.Web.Mvc;

namespace Aplos.Areas.IE
{
    public class IEAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "IE";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "IE",
                url: "ie/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.IE.Controllers" }
            );
        }
    }
}