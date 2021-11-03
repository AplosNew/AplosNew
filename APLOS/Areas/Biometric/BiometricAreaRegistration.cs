using System.Web.Mvc;

namespace Aplos.Areas.Biometric
{
    public class BiometricAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Biometric";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            //  context.MapRoute(
            //    name: "Biometric_default",
            //    url: "biometric/{controller}/{action}/{id}",
            //    defaults: new { action = "Aplos", id = UrlParameter.Optional },
            //    namespaces: new string[] { "Aplos.Areas.Biometric.Controllers" }
            //);

            context.MapRoute(
               "Biometric_default",
               "Biometric/{controller}/{action}/{id}",
               new { action = "Aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Biometric.Controllers" }
           );
        }
    }
}