using System.Web.Mvc;

namespace Aplos.Areas.MIS
{
    public class MISAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "MIS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                name:"MIS",
                url: "MIS/{controller}/{action}/{id}",
                 defaults: new { action = "aplos", id = UrlParameter.Optional },
                 namespaces: new string[] { "Aplos.Areas.MIS.Controllers" }
            );
        }
    }
}