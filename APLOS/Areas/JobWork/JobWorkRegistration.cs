using System.Web.Mvc;

namespace Aplos.Areas.JobWork
{
    public class JobWorkRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "JobWork";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               name: "JobWork",
               url: "JobWork/{controller}/{action}/{id}",
               defaults: new { action = "aplos", id = UrlParameter.Optional },
               namespaces: new string[] { "Aplos.Areas.JobWork.Controllers" }
           );
        }
    }
}