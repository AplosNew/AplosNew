using System.Web.Mvc;

namespace Aplos.Areas.OrderManagements
{
    /// <summary>
    /// OrderManagements area registration.
    /// </summary>
    public class OrderManagementsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "OrderManagements";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "OrderManagements",
                "ordermanagements/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.OrderManagements.Controllers" }
            );
        }
    }
}