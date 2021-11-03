using Library.Service.Helpers;
using Library.Service.Organizations;
using System;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    [AllowAnonymous]
    public class PortalExtendedController : BaseController
    {
        private readonly ICompanyGroupService _companyGroupService;

        public PortalExtendedController(
            ICompanyGroupService companyGroupService
            )
        {
            _companyGroupService = companyGroupService;
        }

        [HttpGet]
        public ActionResult Aplos(string authToken, string groupId, string invalidPanel)
        {
#if DEBUG
            ViewBag.BasePath = "/";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/";
            else
                ViewBag.BasePath = "/" + appName + "/";
#endif
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            ViewBag.AuthToken = authToken;
            ViewBag.GroupId = groupId;
            if (!string.IsNullOrEmpty(invalidPanel))
            {
                ViewBag.InvalidPanel = invalidPanel;
                return View();
            }
            else
                return View();
        }

        [HttpGet]
        public ActionResult PortalExtended(string authenticationToken, string groupId, string invalidPanel)
        {
            try
            {
                ViewBag.CompanyGroupName = _companyGroupService.GetCompanyGroupDisplayName(groupId);
                if (!string.IsNullOrEmpty(invalidPanel))
                {
                    ViewBag.InvalidPanel = invalidPanel;
                    return View();
                }
                else
                    return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { error = ex.Message });
            }
        }

        public ActionResult Error(string message)
        {
            ViewBag.Error = message;
            return View();
        }

        public string GetROOT_FOLDER()
        {
            return ResourcesPathReader.GetROOT_FOLDER();
        }

        public string GetVirtualDirectory()
        {
            return ResourcesPathReader.GetVirtualDirectory();
        }

        public string GetEmployeeFingerPrintPath()
        {
            return ResourcesPathReader.GetEmployeeFingerPrintPath();
        }

        public PartialViewResult ConfirmBox()
        {
            return PartialView("_ConfirmBox");
        }
    }
}