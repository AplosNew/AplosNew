using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets
{
    /// <summary>
    /// Fixed assets area registration
    /// </summary>
    public class FixedAssetsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "FixedAssets";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "FixedAssets",
                url: "fixedassets/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.FixedAssets.Controllers" }
            );
        }
    }
}