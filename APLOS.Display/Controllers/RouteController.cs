using Library.Core;
using System.Web.Mvc;
using System.Web.Routing;

namespace Aplos.Controllers
{
    public class RouteController : Controller
    {
        public ActionResult Route(string controllerName, string actionName, string p)
        {
            var routeValues = new RouteValueDictionary
                {
                    { "controller", controllerName },
                    { "action", actionName}
                };
            if (p.IsNotNullOrEmpty())
            {
                var parameters = p.Split('~');
                foreach (var para in parameters)
                {
                    var param = para.Split('=');
                    if (param.Length == 2)
                        routeValues.Add(param[0], param[1]);
                }
            }
            var redirect = new RedirectToRouteResult(routeValues);
            return redirect;
        }
    }
}