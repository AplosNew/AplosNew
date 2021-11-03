using System.Web.Mvc;

namespace Aplos.Areas.Machines
{
    /// <summary>
    /// Machines area registration.
    /// </summary>
    public class MachinesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Machines";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Machines",
                url: "machines/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Machines.Controllers" }
            );
        }
    }
}