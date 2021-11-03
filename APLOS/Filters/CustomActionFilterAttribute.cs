using Aplos.App_Start;
using Library.Service.Logs;
using System;
using System.Threading;
using System.Web.Mvc;
using Unity;

namespace Aplos.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CustomActionFilterAttribute : ActionFilterAttribute, IActionFilter
    {
        private readonly IUnityContainer container = UnityConfig.GetConfiguredContainer();

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var actionLog = container.Resolve<IActionLogService>();
            string userId = null;
            try
            {
                var identity = Thread.CurrentPrincipal.Identity;
                if (identity.IsAuthenticated)
                    userId = identity.Name;
                //else
                //    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            catch { }
            //SaveLog(filterContext, actionLog, userId);
        }

        private void SaveLog(ActionExecutingContext filterContext, IActionLogService actionLog, string userId)
        {
            actionLog.Insert(
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                filterContext.ActionDescriptor.ActionName,
                filterContext.HttpContext.Timestamp,
                userId,
                filterContext.HttpContext.Request.UserHostAddress
                );
            OnActionExecuting(filterContext);
        }
    }
}