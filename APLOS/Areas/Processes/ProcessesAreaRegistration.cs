using System.Web.Mvc;

namespace Aplos.Areas.Processes
{
    /// <summary>
    /// Processes area registration.
    /// </summary>
    public class ProcessesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Processes";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Processes",
                url: "processes/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Processes.Controllers" }
            );
        }
    }
}