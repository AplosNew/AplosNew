using System.Web.Mvc;

namespace Aplos.Areas.Recruitments
{
	public class RecruitmentsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Recruitments";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Recruitments",
                "recruitments/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Recruitments.Controllers" }
            );
        }
    }
}