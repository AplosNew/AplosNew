using System.Web.Mvc;

namespace Aplos.Areas.PerformanceManagement
{
    public class PerformanceManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PerformanceManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "PerformanceManagement",
                url: "performancemanagement/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.PerformanceManagement.Controllers" }
            );
        }
    }
}