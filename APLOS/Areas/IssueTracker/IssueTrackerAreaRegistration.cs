using System.Web.Mvc;

namespace Aplos.Areas.IssueTracker
{
    public class IssueTrackerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "IssueTracker";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "IssueTracker",
                url: "IssueTracker/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.IssueTracker.Controllers" }
            );
        }

    }
}