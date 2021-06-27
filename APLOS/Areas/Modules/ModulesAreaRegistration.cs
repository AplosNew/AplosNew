using System.Web.Mvc;

namespace Aplos.Areas.Modules
{
    public class ModulesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Modules";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Modules",
                url: "modules/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Modules.Controllers" }
            );
        }
    }
}