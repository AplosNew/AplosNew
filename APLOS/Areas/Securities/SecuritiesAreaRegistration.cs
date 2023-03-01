using System.Web.Mvc;

namespace Aplos.Areas.Securities
{
    public class CPanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "controlpanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "controlpanel",
                "controlpanel/{controller}/{action}/{id}",
                new { controller = "CPanel", action = "clayout", id = UrlParameter.Optional },
                new string[] { "Aplos.Controllers" });
        }
    }

    public class APanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "administrationpanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "administrationpanel",
                "administrationpanel/{controller}/{action}/{id}",
                new { controller = "APanel", action = "alayout", id = UrlParameter.Optional },
                new string[] { "Aplos.Controllers" }
            );
        }
    }

    public class MPanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "masterpanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "masterpanel",
                "masterpanel/{controller}/{action}/{id}",
                new { controller = "MPanel", action = "mlayout", id = UrlParameter.Optional },
                new[] { "Aplos.Controllers" }
            );
        }
    }

    public class UPanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "applicationpanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "applicationpanel",
                "applicationpanel/{controller}/{action}/{id}",
                new { controller = "UPanel", action = "ulayout", id = UrlParameter.Optional },
                new[] { "Aplos.Controllers" }
            );
        }
    }

    public class EPanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "epanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "epanel",
                "epanel/{controller}/{action}/{id}",
                new { controller = "MyApp", action = "elayout", id = UrlParameter.Optional },
                new string[] { "Aplos.Controllers" });
        }
    }

    public class TPanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "tpanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "tpanel",
                "tpanel/{controller}/{action}/{id}",
                new { controller = "DailyAttdStatus", action = "tlayout", id = UrlParameter.Optional },
                new string[] { "Aplos.Controllers" });
        }
    }

    public class PPanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ppanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ppanel",
                "ppanel/{controller}/{action}/{id}",
                new { controller = "MyParents", action = "playout", id = UrlParameter.Optional },
                new string[] { "Aplos.Controllers" });
        }
    }

    public class SecuritiesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Securities";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Securities",
                "securities/{controller}/{action}/{id}",
                new { action = "aplos", id = UrlParameter.Optional },
                new string[] { "Aplos.Areas.Securities.Controllers" }
            );
        }
    }
}