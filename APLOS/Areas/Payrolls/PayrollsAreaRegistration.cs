using System.Web.Mvc;

namespace Aplos.Areas.Payrolls
{
    public class PayrollsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Payrolls";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Payrolls",
                url: "payrolls/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Payrolls.Controllers" }
            );
        }
    }
}