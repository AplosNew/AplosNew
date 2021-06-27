using System.Web.Mvc;

namespace Aplos.Areas.Logs
{
    public class LogsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Logs";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Logs",
                url: "logs/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Logs.Controllers" }
            );
        }
    }
}