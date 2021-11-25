using Aplos.Helpers;
using Aplos.Properties;
using ConnectionManager;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Logs;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Securites;
using Library.Service.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Aplos.Controllers
{
    public class UPanelController : BaseController
    {
        public UPanelController()
        {
          
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Aplos()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return Redirect("~/home/Error?message=" + ex.Message);
            }
        }
        [HttpGet, Authorize]
        public ActionResult NotificationURL()
        {

            try
            {
               
                return Json(null, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpGet, AllowAnonymous]
        public ActionResult ULayout()
        {
#if DEBUG
            ViewBag.BasePath = "/applicationpanel";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/applicationpanel";
            else
                ViewBag.BasePath = "/" + appName + "/applicationpanel";
#endif
            try
            {
               
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Login), "Account", new { servicepanel = "uPanel" });
            }
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Dashboard()
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, "Status: Ready To Process");

                }
                catch (Exception ex)
                {

                }
             
                return View();
            });
        }

        [HttpGet, Authorize]
        public ActionResult PlantSelection()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult PlantSelection(string plantId, string plantName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SetAuthentication(identity.UserId, identity.Name, identity.FullName, false, identity.IPAddress, identity.AuthenticationToken, identity.CompanyGroupId, identity.CompanyGroupName, false, identity.CompanyId, identity.CompanyName, identity.IsSysAdmin, identity.IsPowerUser, identity.EmployeeId, plantId, plantName, new Dictionary<string, object>());


            return Json(new
            {

                Message = AplosMessage.Success
            });
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string authToken, string groupId)
        {
            try
            {
                if (!UserValidate(PanelConst.uPanel.ToLower(), authToken, groupId))
                    return Redirect("~/portal?authToken=" + authToken + "&groupId=" + groupId + "&invalidPanel=Access denied!");
                return View();
            }
            catch (Exception ex)
            {
                return Redirect("~/home/Error?message=" + ex.Message);
            }
        }

        [HttpPost, AllowAnonymous]
        public JsonResult Login(string timezoneoffset, string userId, string password, string remember, string authToken, string groupId, string groupName, string companyId, string companyName, string plantId)
        {
            
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public void AddNewRow<T>(DataTable dt, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dt.Rows.Add(dr);
        }
        public void SaveDataSets(params System.Data.DataSet[] dsRef)
        {
            clsConnection objCon = null;
            try
            {
                objCon = new clsConnection();
                objCon.BeginTransaction();
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveData(ref dsRef[i]);
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        } // End Function
        private void SetAuthentication(string id, string userId, string fullName, bool isRemember, string ip, string authenticationToken, string companyGroupId, string companyGroupName, bool concurrentuser, string companyId, string companyName, bool isSysAdmin, bool isPowerUser, string employeeId, string plantId, string plantName, Dictionary<string, object> result)
        {
            if (concurrentuser)
            {
                var rdnumber = new Random();
                userId = userId + rdnumber.Next(0, 99999);
            }
            var basicTicket = CustomIdentity.CreateBasicTicket(
                id, userId, fullName, companyGroupId, companyGroupName,
                companyId, companyName, plantId, plantName, employeeId, ip, false, isSysAdmin,
                isPowerUser, authenticationToken, null);
            var roleTicket = CustomIdentity.CreateRoleTicket(new[] { "NoAction" });
            var timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
            var authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), isRemember, basicTicket);
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            HttpContext.Response.Cookies.Add(new HttpCookie("Identity", basicTicket));
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            HttpContext.Application["BasicTicket" + userId] = basicTicket;
            HttpContext.Application["RoleTicket" + userId] = roleTicket;
            HttpContext.Session["Panel"] = PanelEnum.uPanel.ToString();

            result.Add("Cookie", ".ASPXAUTH=" + encTicket);
            result.Add("Identity", basicTicket);
        }

        private bool UserValidate(string servicepanel, string authenticationToken, string groupId)
        {
            
            return true;
        }
    }
}