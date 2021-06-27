#region Using

using Aplos.App_Start;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Security.Core;
using System;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Unity;

#endregion Using

namespace Aplos.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        //  private static readonly IUnityContainer Unitycontainer = UnityConfig.GetConfiguredContainer();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var identity = Thread.CurrentPrincipal.Identity;
            if (!identity.IsAuthenticated)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { status = "401" }
                    };
                    //xhr status code 401 to redirect
                    filterContext.HttpContext.Response.StatusCode = 401;
                    return;
                }
                //else
                //    filterContext.Result = new RedirectToRouteResult(
                //                       new RouteValueDictionary
                //                       {
                //                           { "action", "Aplos" },
                //                           { "controller", "Home" }
                //                       });
            }
            else
            {
                var customIdentity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (filterContext.ActionDescriptor.IsDefined(typeof(AuthorizeAttribute), true) ||
                    filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                    return;
                else if (customIdentity.IsControlAdmin || customIdentity.IsSysAdmin)
                    return;
                else
                {
                    var targetPage = filterContext.RequestContext.RouteData.Values["controller"] as string;
                    var targetAction = filterContext.RequestContext.RouteData.Values["action"] as string;
                    if (filterContext.ActionDescriptor.IsDefined(typeof(ChaildActionAttribute), true))
                    {
                        var attr = filterContext.ActionDescriptor.GetCustomAttributes(typeof(ChaildActionAttribute), true);
                        targetAction = ((ChaildActionAttribute[])attr)[0].ParentActionName;
                    }

                    UserAccessService _userRoleService = new UserAccessService();
                    if (!_userRoleService.IsAuthoorized(customIdentity.CompanyGroupId, customIdentity.CompanyId, customIdentity.UserId, customIdentity.EmployeeId, targetPage, targetAction))
                    {
                        filterContext.HttpContext.Response.StatusCode = 406;//406: Not Acceptable
                        filterContext.Result = new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { status = "406" }
                        };
                    }
                }
                base.OnAuthorization(filterContext);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //Returns HTTP 401 - see comment in HttpUnauthorizedResult.cs.
            if (filterContext.HttpContext.Response.StatusCode == 403)
                throw new HttpException((int)HttpStatusCode.Forbidden, "Forbidden");
            if (filterContext.HttpContext.Response.StatusCode == 406)
                throw new HttpException((int)HttpStatusCode.Forbidden, "You don't have permission to perform this action!");
            else
                throw new HttpException((int)HttpStatusCode.ServiceUnavailable, "Session Timeout");
        }
    }
}