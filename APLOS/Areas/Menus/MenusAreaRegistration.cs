using System.Web.Mvc;

namespace Aplos.Areas.Menus
{
    public class MenusAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Menus";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Menus",
                url: "menus/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Menus.Controllers" }
            );
        }
    }
}