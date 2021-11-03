using System.Web.Mvc;

namespace Aplos.Areas.Setups
{
    /// <summary>
    /// Setups area registration.
    /// </summary>
    public class SetupsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Setups";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Setups",
                "setups/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Setups.Controllers" }
            );
        }
    }
}