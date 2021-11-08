using System.Web.Mvc;

namespace Aplos.Areas.Outsourcing
{
    public class OutsourcingRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Outsourcing";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               name: "Outsourcing",
               url: "Outsourcing/{controller}/{action}/{id}",
               defaults: new { action = "aplos", id = UrlParameter.Optional },
               namespaces: new string[] { "Aplos.Areas.Outsourcing.Controllers" }
           );
        }
    }
}