using System.Web.Mvc;

namespace Aplos.Areas.Commercial
{
    public class CommercialAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Commercial";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Commercial_default",
                "Commercial/{controller}/{action}/{id}",
                new { action = "Aplos", id = UrlParameter.Optional },
                 namespaces: new string[] { "Aplos.Areas.Commercial.Controllers" }
            );
           
        }
    }
}