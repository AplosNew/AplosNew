using System.Web.Mvc;

namespace Aplos.Areas.Materials
{
    public class MaterialsAreaRegistration : AreaRegistration
    {


        public override string AreaName
        {
            get
            {
                return "Materials";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Materials",
                url: "materials/{controller}/{action}/{id}",
                defaults: new { action = "aplos", id = UrlParameter.Optional },
                namespaces: new string[] { "Aplos.Areas.Materials.Controllers" }
            );
        }


    }
}