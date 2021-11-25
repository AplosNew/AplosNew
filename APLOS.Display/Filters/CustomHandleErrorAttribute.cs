using Aplos.App_Start;
using Aplos.Controllers;
using Library.Data;
using Library.Service.Logs;
using System;
using System.Reflection;
using System.Web.Mvc;
using Unity;

namespace Aplos.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class CustomHandleErrorAttribute : FilterAttribute, IExceptionFilter
    {
        private readonly IUnityContainer container = UnityConfig.GetConfiguredContainer();

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;
            SaveError(filterContext);
            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { Error = true, filterContext.Exception.Message }
                };
                filterContext.ExceptionHandled = true;
            }
            else
            {
                filterContext.ExceptionHandled = true;
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { Error = true, filterContext.Exception.Message }
                };
            }
        }

        private void SaveError(ExceptionContext filterContext)
        {
            if (filterContext.Exception.GetType() == typeof(CustomException))
            {
                var exception = (CustomException)filterContext.Exception;
                if (exception.ErrorInfo != null)
                {
                    var logger = container.Resolve<ILogger>();
                    exception.ErrorInfo.AppVersion = Assembly.GetAssembly(typeof(HomeController)).GetName().Version.ToString();
                    exception.ErrorInfo.ControllerName = filterContext.RouteData.Values["controller"].ToString();
                    exception.ErrorInfo.ActionName = filterContext.RouteData.Values["action"].ToString();
                    logger.Log(exception.ErrorInfo);
                }
            }
        }
    }
}