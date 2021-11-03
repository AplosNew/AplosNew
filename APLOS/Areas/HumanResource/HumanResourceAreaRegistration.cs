using System.Web.Mvc;

namespace Aplos.Areas.HumanResource
{
    public class HumanResourceAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "HumanResource";
            }
        }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HumanResource",
                "HumanResource/{controller}/{action}/{id}",
                new { action = "aplos", id = UrlParameter.Optional },
                new string[] { "Aplos.Areas.HumanResource.Controllers" }
            );
        }
    }
}