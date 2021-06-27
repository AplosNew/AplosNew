using System.Web.Mvc;

namespace Aplos.Areas.Currencies
{
    /// <summary>
    /// Expenses area registration.
    /// </summary>
    public class CurrenciesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Currencies";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Currencies",
                url: "currencies/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Currencies.Controllers" }
            );
        }
    }
}