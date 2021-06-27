using System.Web.Mvc;

namespace Aplos.Areas.Costings
{
    public class CostingsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Costings";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            
            context.MapRoute(
                name: "Costings",
                url: "Costings/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Costings.Controllers" }
            );
        }
    }
}