using Aplos.Helpers;
using Library.Crosscutting.Security;
using Library.Service.Helpers;
using Library.Service.Securites;
using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    [AllowAnonymous]
    public class DownloadController : BaseController
    {
        private readonly IUserService _userService;

        public DownloadController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult AuthToken()
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
            return View();
        }

        [HttpPost]
        public ActionResult AuthToken(string username, string password, DateTime dateOfBirth, string email, string captcha)
        {
            var msg = "";
            var error = false;
            try
            {
                var http = System.Web.HttpContext.Current;
                var ip = AccessInfo.GetWorkstationIP(http);
                var captchaHelper = new CaptchaHelper();
                var success = captchaHelper.Verify(captcha);
                if (success)
                {
                    var userInfo = _userService.GetDataForDownloadAuth(username, password, dateOfBirth, email, ip, Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    using (var embeddedTool = new EmbeddedTool())
                    {
                        var authToken = embeddedTool.Encrypt(userInfo.AuthToken);
                        using (var embeddedTool1 = new EmbeddedTool())
                        {
                            var companyGroup = embeddedTool1.Encrypt(userInfo.CompanyGroupId);
                            var byteArray = System.Text.Encoding.ASCII.GetBytes(authToken + "\r\n" + companyGroup);
                            var stream = new System.IO.MemoryStream(byteArray);
                            //return Json(new { Error = error, Message = msg });
                            return File(stream, "text/plain", "secureclientconfig");
                        }
                    }
                }
                else
                {
                    error = true;
                    msg = "Error: captcha is not valid.";
                }
                return Json(new { Error = error, Message = msg });
            }
            catch (Exception ex)
            {
                error = true;
                msg = "Invalid information!";
                return Json(new { Error = error, Message = msg, hala = ex.Message + "- --- -" + ex.StackTrace });
            }
        }

    }
}