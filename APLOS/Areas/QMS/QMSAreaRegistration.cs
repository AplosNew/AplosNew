using System.Web.Mvc;

namespace Aplos.Areas.QMS
{
    public class QMSAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "QMS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                      "QMS",
                      "QMS/{controller}/{action}/{id}",
                      defaults: new { action = "aplos", id = UrlParameter.Optional },
                      namespaces: new string[] { "Aplos.Areas.QMS.Controllers" }

                );

        }
    }
}