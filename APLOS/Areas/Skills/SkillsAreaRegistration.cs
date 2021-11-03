using System.Web.Mvc;

namespace Aplos.Areas.Skills
{
    public class SkillsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Skills";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Skills",
                url: "skills/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Skills.Controllers" }
            );
        }
    }
}