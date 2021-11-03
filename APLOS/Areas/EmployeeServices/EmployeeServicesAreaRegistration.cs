using System.Web.Mvc;

namespace Aplos.Areas.EmployeeServices
{
    public class EmployeeServicesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "EmployeeServices";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                   "EmployeeServices",
                      "EmployeeServices/{controller}/{action}/{id}",
                      defaults: new { action = "aplos", id = UrlParameter.Optional },
                      namespaces: new string[] { "Aplos.Areas.EmployeeServices.Controllers" }
            );
        }
    }
}