using System.Web.Mvc;

namespace Aplos.Areas.MeetingManagement
{
    public class MeetingManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "MeetingManagement";
            }
        }


        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "MeetingManagement",
                url: "meetingmanagement/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.MeetingManagement.Controllers" }
            );
        }


    }
}