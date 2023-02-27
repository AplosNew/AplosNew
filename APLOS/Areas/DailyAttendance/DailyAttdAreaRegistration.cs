using System.Web.Mvc;

namespace Aplos.Areas.Recruitments
{
	public class DailyAttdAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DailyAttendance";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DailyAttendance",
                "dailyattendance/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.DailyAttendance.Controllers" }
            );
        }
    }
}