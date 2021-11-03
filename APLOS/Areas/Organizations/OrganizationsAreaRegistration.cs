using System.Web.Mvc;

namespace Aplos.Areas.Organizations
{
    public class OrganizationsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Organizations";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Organizations",
                url: "organizations/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Organizations.Controllers" }
            );
        }
    }
}