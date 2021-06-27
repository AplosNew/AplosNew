using System.Web.Mvc;

namespace Aplos.Areas.SalesManagements
{
    public class SalesManagementsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SalesManagements";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SalesManagements",
                "SalesManagements/{controller}/{action}/{id}",
                new { action = "aplos", id = UrlParameter.Optional },
                new string[] { "Aplos.Areas.SalesManagements.Controllers" }
            );
        }
    }
}