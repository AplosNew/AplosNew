using System.Web.Mvc;

namespace Aplos.Areas.Parties
{
    public class PartiesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Parties";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Parties",
                "parties/{controller}/{action}/{id}",
                new { action = "aplos", id = UrlParameter.Optional },
                new[] { "Aplos.Areas.Parties.Controllers" }
            );
        }
    }
}