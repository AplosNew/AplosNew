using System.Web.Mvc;

namespace Aplos.Areas.Farming
{
    public class FarmingAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Farming";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Farming",
                url: "farming/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Farming.Controllers" }
            );
        }
    }
}