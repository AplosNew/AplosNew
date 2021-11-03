using System.Web.Mvc;

namespace Aplos.Areas.TaskManagement
{
    public class TaskManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TaskManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "TaskManagement",
                "TaskManagement/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.TaskManagement.Controllers" }
            );
        }
    }
}