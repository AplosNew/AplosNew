using Aplos.Filters;
using System.Web.Mvc;

namespace Aplos
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomActionFilterAttribute());
            filters.Add(new CustomHandleErrorAttribute());
            filters.Add(new CustomAuthorizeAttribute());
        }
    }
}