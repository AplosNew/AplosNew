using System.Web.Mvc;

namespace Aplos.Areas.Products
{
    /// <summary>
    /// Products area registration.
    /// </summary>
    public class ProductsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Products";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Products",
                "products/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
               namespaces: new string[] { "Aplos.Areas.Products.Controllers" }
            );
        }
    }
}