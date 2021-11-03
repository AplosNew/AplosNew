using System.Web.Mvc;

namespace Aplos.Areas.Attendances
{
    public class AttendancesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Attendances";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Attendances",
                url: "Attendances/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Attendances.Controllers" }
            );
        }
    }
}