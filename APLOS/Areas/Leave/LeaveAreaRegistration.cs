using System.Web.Mvc;

namespace Aplos.Areas.Leave
{
    public class LeaveAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Leave";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               name: "Leave",
               url: "Leave/{controller}/{action}/{id}",
               defaults: new { action = "aplos", id = UrlParameter.Optional },
               namespaces: new string[] { "Aplos.Areas.Leave.Controllers" }
           );
        }
    }
}