#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Securites;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Library.Service.Securites;
using Microsoft.ReportingServices.Diagnostics.Internal;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    public class UserController : BaseController
    {
        #region Constructor

        private readonly IUserService _userService;
        private readonly IPasswordChangeService _passwordChangeService;
        private readonly ICompanyGroupService _companyGroupService;
        private readonly IUserSalesGroupService _userSalesGroupService;
        private readonly IUserPurchaseGroupService _userPurchaseGroupService;

        public UserController(
            IUserService userService,
            IPasswordChangeService passwordChangeService,
            IUserSalesGroupService userSalesGroupService,
            IUserPurchaseGroupService userPurchaseGroupService,
            ICompanyGroupService companyGroupService)
        {
            _userSalesGroupService = userSalesGroupService;
            _userPurchaseGroupService = userPurchaseGroupService;
            _userService = userService;
            _passwordChangeService = passwordChangeService;
            _companyGroupService = companyGroupService;
        }

        #endregion Constructor

        #region GetFullNameByUserId

        public ActionResult GetFullName(string id)
        {
            return Json(_userService.GetFullName(id), JsonRequestBehavior.AllowGet);
        }

        #endregion GetFullNameByUserId

        #region DDL

        [Authorize]
        public JsonResult GetUserList()
        {
            return Json(new SelectList(_userService.GetUserList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetUserListWithoutSysAdmin()
        {
            return Json(new SelectList(_userService.GetUserListWithoutSysAdmin(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        #endregion DDL

        [HttpGet]
        public ActionResult UserList()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AuthTokenChange()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Reset()
        {
            return View();
        }

        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_userService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        #region Get

        [HttpGet, Authorize]
        public JsonResult Get(string id)
        {
            return Json(_userService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAuth()
        {
            try
            {
                var auth = Guid.NewGuid().ToString();
                return Json(new { Auth = auth });
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message + " Call Stack: " + ex.StackTrace);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetUserAccessFromEmp()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(_companyGroupService.Query(r => r.Id == identity.CompanyGroupId).Select(r => r.IsUserAccessFromEmployee).FirstOrDefault(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message + " Call Stack: " + ex.StackTrace);
            }
        }

        #endregion Get

        #region CRUD

        [HttpGet]
        public JsonResult CreateAuth()
        {
            return Json(Guid.NewGuid(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult CreatePin()
        {
            var r = new Random();
            var randomPinNo = r.Next(100000, 999999);
            return Json(randomPinNo, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            var sg = form["userSalesGroup"];
            var pg = form["userPurchaseGroup"];
            var sk = form["sectionList"];
            var payroll = form["payrollGroupList"];
            var userProcess = form["userProcessList"];
            var userGate = form["userGateList"];
            var userSFGInventory = form["userSFGInventoryList"];
            var userReportGroup = form["userReportGroupList"];

            List<UserSalesGroup> userSalesGroup = null;
            List<UserPurchaseGroup> userPurchaseGroup = null;
            List<UserSection> userSection = null;
            List<UserPayrollGroup> payrollGroup = null;
            List<UserProcess> userProcessList = null;
            List<UserPlantGate> userGateList = null;
            List<UserSFGInventory> userSFGInventoryList = null;
            List<UserReportGroup> userReportGroupList = null;
            var user = new JavaScriptSerializer().Deserialize<User>(form["user"]);
            if (sg != null)
                userSalesGroup = new JavaScriptSerializer().Deserialize<List<UserSalesGroup>>(form[nameof(userSalesGroup)]);
            if (pg != null)
                userPurchaseGroup = new JavaScriptSerializer().Deserialize<List<UserPurchaseGroup>>(form[nameof(userPurchaseGroup)]);
            if (sk != null)
                userSection = new JavaScriptSerializer().Deserialize<List<UserSection>>(form["sectionList"]);
            if (payroll != null)
                payrollGroup = new JavaScriptSerializer().Deserialize<List<UserPayrollGroup>>(form["payrollGroupList"]);

            if (userProcess != null)
                userProcessList = new JavaScriptSerializer().Deserialize<List<UserProcess>>(form["userProcessList"]);

            if (userGate != null)
                userGateList = new JavaScriptSerializer().Deserialize<List<UserPlantGate>>(form["userGateList"]);

            if (userSFGInventory != null)
                userSFGInventoryList = new JavaScriptSerializer().Deserialize<List<UserSFGInventory>>(form["userSFGInventoryList"]);

            var file = Request.Files["file"];
            var extension = string.Empty;
            if (file != null)
            {
                extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() != ".jpg" && extension.ToLower() != ".png")
                    throw new CustomException(Resources.ImageUploadError);
            }
            _userService.Insert(user, userSalesGroup, userPurchaseGroup, userSection, payrollGroup, userProcessList, userGateList, userSFGInventoryList, userReportGroupList, extension);
            if (file != null)
            {
                var path = Path.Combine(ResourcesPathReader.GetUserPicUrl(), user.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            else if (!string.IsNullOrEmpty(user.EmployeeId) && !string.IsNullOrEmpty(user.Image))
            {
                var fromdirectory = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath(), user.Image);
                if (System.IO.File.Exists(fromdirectory))
                {
                    var todirectory = Path.Combine(ResourcesPathReader.GetUserPicUrl(), user.Image);
                    System.IO.File.Copy(fromdirectory, todirectory, true);
                }
            }
            return Json(new { User = user, AuthToken = Guid.NewGuid(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form)
        {
            var user = new JavaScriptSerializer().Deserialize<User>(form["user"]);
            var userSalesGroup = new JavaScriptSerializer().Deserialize<List<UserSalesGroup>>(form["userSalesGroup"]);
            var userPurchaseGroup = new JavaScriptSerializer().Deserialize<List<UserPurchaseGroup>>(form["userPurchaseGroup"]);
            var userSection = new JavaScriptSerializer().Deserialize<List<UserSection>>(form["sectionList"]);
            var payroll = new JavaScriptSerializer().Deserialize<List<UserPayrollGroup>>(form["payrollGroupList"]);
            var userProcessList = new JavaScriptSerializer().Deserialize<List<UserProcess>>(form["userProcessList"]);
            var userGateList = new JavaScriptSerializer().Deserialize<List<UserPlantGate>>(form["userGateList"]);
            var userSFGInventoryList = new JavaScriptSerializer().Deserialize<List<UserSFGInventory>>(form["userSFGInventoryList"]);
            var userReportGroupList = new JavaScriptSerializer().Deserialize<List<UserReportGroup>>(form["userReportGroupList"]);

            var file = Request.Files["file"];
            var imageExtension = string.Empty;
            var oldUser = _userService.Find(user.Id);
            var oldImage = oldUser.Image;
            var oldEmployeeId = oldUser.EmployeeId;

            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() != ".jpg" && extension.ToLower() != ".png")
                    throw new CustomException(Resources.ImageUploadError);
                imageExtension = extension.ToLower();
            }
            //user.EmployeeId = null;

            _userService.Update(oldUser, user, userSalesGroup, userPurchaseGroup, userSection, payroll, userProcessList, userGateList, userSFGInventoryList, userReportGroupList, imageExtension);
            if (file != null)
            {
                // Delete Old image in employee case.
                if (!string.IsNullOrEmpty(oldImage))
                {
                    var pathOld = Path.Combine(ResourcesPathReader.GetUserPicUrl(), oldImage);
                    if (System.IO.File.Exists(pathOld))
                        System.IO.File.Delete(pathOld);
                }
                var path = Path.Combine(ResourcesPathReader.GetUserPicUrl(), user.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            else
            {
                if (!string.IsNullOrEmpty(oldImage) && string.IsNullOrEmpty(user.Image))
                {
                    var path = Path.Combine(ResourcesPathReader.GetUserPicUrl(), oldImage);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                //else
                if (!string.IsNullOrEmpty(user.EmployeeId) && oldEmployeeId != user.EmployeeId && !string.IsNullOrEmpty(user.Image))
                {
                    var fromdirectory = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath(), user.Image);
                    if (System.IO.File.Exists(fromdirectory))
                    {
                        //throw new CustomException("Employee picture not exist.");
                        var todirectory = Path.Combine(ResourcesPathReader.GetUserPicUrl(), user.Image);
                        System.IO.File.Copy(fromdirectory, todirectory, true);
                    }
                }
            }
            return Json(new { User = user, AuthToken = Guid.NewGuid(), Message = AplosMessage.Insert });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (id == identity.UserId) throw new Exception("You can not delete this user.........!");
            _userService.ArchiveGraph(id);
            return Json(new { AuthToken = Guid.NewGuid(), Message = AplosMessage.Deleted });
        }

        #endregion CRUD

        [HttpPost]
        public JsonResult AuthTokenChange(User user)
        {
            _userService.UpdateAuthToken(user);
            return Json(new { Message = AplosMessage.Success });
        }

        #region PasswordChange

        [HttpGet, Authorize]
        public JsonResult GetForPasswordChange(string id)
        {
            var data = _userService.FindForUserPasswordChange(id);
            using (var embeddedTool = new EmbeddedTool())
            {
                data.Password = embeddedTool.Decrypt(data.Password);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult PasswordChange()
        {
            return View();
        }

        //[HttpPost]
        //public JsonResult PasswordChange(string id, string userPassword)
        //{
        //    _passwordChangeService.AddAndUpdate(id, userPassword, null);
        //    return Json(new { Message = AplosMessage.Updated });
        //}

        [HttpPost, Authorize]
        public JsonResult PasswordChange(User user)
        {
            _userService.PasswordChange(user);
            return Json(new { Message = AplosMessage.Updated });
        }


        #endregion PasswordChange

        #region Auth Token Lock

        public ActionResult AuthTokenLockDateWithoutSyAdmin(GridParameter parameters)
        {
            return Json(_userService.AuthTokenLockDateWithoutSyAdmin(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AuthTokenUnLock()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AuthTokenUnLock(string id)
        {
            _userService.AuthTokenLockUpdate(id);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion Auth Token Lock

        #region User Lock

        public ActionResult UserLockDateWithoutSyAdmin(GridParameter parameters)
        {
            return Json(_userService.UserLockDateWithoutSyAdmin(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult UserUnLock()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserUnLock(string id)
        {
            _userService.UpdateUserLockUnLock(id);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion User Lock

        [HttpPost, Authorize]
        public JsonResult Reset(User user)
        {
            _userService.PasswordChange(user);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetUserSalesGroupList(string userId)
        {
            return Json(_userSalesGroupService.List(userId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_userSalesGroupService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUserPurchaseGroupList(string userId)
        {
            return Json(_userPurchaseGroupService.List(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUserSectionList(string userId)
        {
            return Json(_userService.UserSectionList(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult UserPayrollGroupList(string userId)
        {
            return Json(_userService.UserPayrollGroupList(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUserProcessList(string userId)
        {
            return Json(_userService.GetUserProcessList(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult SaveShowHideFavouriteMenu(bool ShowFavouriteMenu)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSql = "select * from UserFavoriteMaster where UserId='" + identity.Name + "'";
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(strSql, out System.Data.DataSet dsRef, false, "1");
            if (dsRef.Tables[0].Rows.Count == 0)
            {
                DataRow dr = dsRef.Tables[0].NewRow();
                dr["UserId"] = identity.Name;
                dr["ShowFavoriteMenu"] = ShowFavouriteMenu;
                dsRef.Tables[0].Rows.Add(dr);

            }
            else
            {
                DataRow dr = dsRef.Tables[0].Rows[0];

                dr.BeginEdit();
                dr["ShowFavoriteMenu"] = ShowFavouriteMenu;
                dr.EndEdit();
            }

            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsRef);

            return Json(ShowFavouriteMenu, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ShowHideFavouriteMenu()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSql = "select * from UserFavoriteMaster where UserId='" + identity.Name + "'";
            SqlRepository repo = new SqlRepository();

            return Json(repo.GetData(strSql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult SaveFavorite(string MenuMasterId)
        {
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            string strSql = "select Id from MST.MenuMaster where Href='" + MenuMasterId + "'";
            objCon.OpenDataSetThroughAdapter(strSql, out System.Data.DataSet dsRefMenu, false, "1");
            if (dsRefMenu.Tables[0].Rows.Count == 0)
                return null;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            strSql = "select * from UserFavoriteMenu where MenuMasterId='" + dsRefMenu.Tables[0].Rows[0]["Id"].ToString() + "' and UserId='" + identity.Name + "'";

            objCon.OpenDataSetThroughAdapter(strSql, out System.Data.DataSet dsRef, false, "1");




            if (dsRef.Tables[0].Rows.Count == 0)
            {
                DataRow dr = dsRef.Tables[0].NewRow();
                dr["UserId"] = identity.Name;
                dr["MenuMasterId"] = dsRefMenu.Tables[0].Rows[0]["Id"].ToString();
                dsRef.Tables[0].Rows.Add(dr);

            }


            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsRef);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult DeleteFavorite(string MenuMasterId)
        {
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            string strSql = "select Id from MST.MenuMaster where Href='" + MenuMasterId + "'";
            objCon.OpenDataSetThroughAdapter(strSql, out System.Data.DataSet dsRefMenu, false, "1");
            if (dsRefMenu.Tables[0].Rows.Count == 0)
                return null;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            strSql = "select * from UserFavoriteMenu where MenuMasterId='" + dsRefMenu.Tables[0].Rows[0]["Id"].ToString() + "' and UserId='" + identity.Name + @"'";

            objCon.OpenDataSetThroughAdapter(strSql, out System.Data.DataSet dsRef, false, "1");

            if (dsRef.Tables[0].Rows.Count > 0)
            {
                dsRef.Tables[0].Rows[0].Delete();

            }


            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsRef);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult UserFavoriteMenu()
        {
            SqlRepository repo = new SqlRepository();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSql = @"SELECT distinct NULL as MenuList, m.ModuleId,m.MenuFrameId, mf.UserName+'('+ D.Code+')' AS ModuleCode,d.Sequence,mf.Sequence
                                  FROM UserFavoriteMenu MM
                                INNER JOIN mst.MenuMaster AS m ON m.Id=mm.MenuMasterId
                                INNER JOIN mms.Module AS D ON d.Id=m.ModuleId
                                INNER JOIN mms.MenuFrame AS mf ON mf.Id=m.MenuFrameId
                                WHERE UserId='" + identity.Name + @"' 
                                ORDER BY d.Sequence,mf.Sequence ";

            List<Dictionary<string, object>> ModuleData = repo.GetDataCollection(strSql);


            strSql = @"SELECT m.ModuleId,m.MenuFrameId, mm.MenuMasterId,D.Code,m.Href,m.[Description],m.[Image],M.Sequence
                              FROM UserFavoriteMenu MM
                            INNER JOIN mst.MenuMaster AS m ON m.Id=mm.MenuMasterId
                            INNER JOIN mms.Module AS D ON d.Id=m.ModuleId
                            INNER JOIN mms.MenuFrame AS mf ON mf.Id=m.MenuFrameId
                            WHERE UserId='" + identity.Name + "' ORDER BY m.Sequence ";

            List<Dictionary<string, object>> MenuData = repo.GetDataCollection(strSql);

            for (int i = 0; i < ModuleData.Count; i++)
            {
                var k = MenuData.Where(x => clsStaticInfo.nullrecorder(x["ModuleId"]) == ModuleData[i]["ModuleId"].ToString()
                && clsStaticInfo.nullrecorder(x["MenuFrameId"]) == ModuleData[i]["MenuFrameId"].ToString()).ToList();
                ModuleData[i]["MenuList"] = k;
            }


            return Json(ModuleData, JsonRequestBehavior.AllowGet);
        }

    }
}