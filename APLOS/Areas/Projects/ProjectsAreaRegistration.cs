using System.Web.Mvc;

namespace Aplos.Areas.Projects
{
    public class ProjectsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Projects";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Projects",
                "projects/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Projects.Controllers" }

            );
        }
    }
}