using System.Web.Mvc;

namespace Aplos.Areas.Employees
{
    /// <summary>
    /// Employees area registration.
    /// </summary>
    public class EmployeesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Employees";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Employees",
                url: "employees/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Employees.Controllers" }
            );
        }
    }
}