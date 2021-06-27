using System.Web.Mvc;

namespace Aplos.Areas.TaskScheduler
{
    public class TaskSchedulerRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TaskScheduler";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "TaskScheduler",
                "TaskScheduler/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}