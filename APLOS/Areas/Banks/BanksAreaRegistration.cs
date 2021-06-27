using System.Web.Mvc;

namespace Aplos.Areas.Banks
{
    public class BanksAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Banks";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Banks",
                url: "banks/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Banks.Controllers" }
            );
        }
    }
}