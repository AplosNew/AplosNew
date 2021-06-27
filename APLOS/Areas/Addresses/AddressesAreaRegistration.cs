using System.Web.Mvc;

namespace Aplos.Areas.Addresses
{
    /// <summary>
    /// Addresses area registration.
    /// </summary>
    public class AddressesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Addresses";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Addresses",
                url: "addresses/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Addresses.Controllers" }
            );
        }
    }
}