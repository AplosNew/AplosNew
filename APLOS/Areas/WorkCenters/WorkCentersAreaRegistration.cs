using System.Web.Mvc;

namespace Aplos.Areas.WorkCenters
{
    /// <summary>
    /// WorkCenters area registration.
    /// </summary>
    public class WorkCentersAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "WorkCenters";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "WorkCenter",
                url: "workcenters/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.WorkCenters.Controllers" }
            );
        }
    }
}