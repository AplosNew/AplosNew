using Aplos.Service.Securites;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.HumanResource.UserAppAuthentication;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Security.Core;
using Aplos.Properties;

namespace Aplos.Areas.Securities.Controllers
{
    public class UserAppAuthenticationController : Controller
    {

        // GET: Securities/UserAppAuthentication
        #region Constructor

        private readonly UserAppAuthenticationService ua;
        private readonly SqlRepository _sqlRepository;

        public UserAppAuthenticationController(UserAppAuthenticationService userAppAuthenticationService)
        {
            _sqlRepository = new SqlRepository();
            ua = userAppAuthenticationService;
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet,Authorize]
        public ActionResult getRole()
        {
            string strSql = @"select Id Value,Name Text from SEC.[AppRole]";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }
        /*public ActionResult getuser()
        {
            string strSql = @"select Id Value,Name Text from SEC.[Role]";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }*/

        
        [HttpGet, Authorize]
        public ActionResult getModule()
        {
            string strSql = @"select Id Value,ModuleName Text from dbo.MobileAppModule";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult geticon(string Moduleid)
        {
            string strSql = @"select Id Value,IconName Text , ModuleId from dbo.MobileAppIcon where ModuleId = '" + Moduleid + "'";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }


        #region save
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                string TableNameHead = "SEC.AppRoleDetail";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from SEC.AppRoleDetail where RoleId = '" + data["RoleId"] + "'AND IconId= '" + data["IconId"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from SEC.AppRoleDetail where Id = '" + data["Id"] + "'", out dsMaster, false, "1");
                
                #region Upload HEAD
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                   
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion Upload HEAD
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });


            }
        }
        #endregion save
        #region Create and Edit Default Column

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        #endregion Create and Edit Default Column

        [Authorize, HttpGet]
        public ActionResult Getlist()
        {
            try
            {
                #region commented
                //                string st = @"SELECT p.RoleId AS RoleId, p.ModuleId As ModuleId, p.IconId As IconID, c.name AS RoleName ,  mp.ModuleName As ModuleName, 
                //mi.IconName As IconName
                //FROM SEC.AppRoleDetail AS p
                //LEFT JOIN SEC.AppRole AS c ON p.RoleId = c.id
                //LEFT JOIN dbo.MobileAppModule AS mp ON p.ModuleId = mp.id
                //LEFT JOIN dbo.MobileAppIcon AS mi ON p.IconId = mi.id";
                #endregion commented
                string sql = @"SELECT distinct p.ModuleId, mp.ModuleName  ModuleName, c.name  RoleName 
FROM SEC.AppRoleDetail AS p
LEFT JOIN SEC.AppRole AS c ON p.RoleId = c.id
LEFT JOIN dbo.MobileAppModule AS mp ON p.ModuleId = mp.id
LEFT JOIN dbo.MobileAppIcon AS mi ON p.IconId = mi.id
order by mp.ModuleName";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
            
        }

        [Authorize, HttpGet]
        public ActionResult GetDataById(string moduleid)
        {
            try
            {
                #region commented
                string st = @"SELECT p.RoleId AS RoleId, p.ModuleId As ModuleId, p.IconId As IconID, c.name AS RoleName ,  mp.ModuleName As ModuleName, 
                mi.IconName As IconName
                FROM SEC.AppRoleDetail AS p
                LEFT JOIN SEC.AppRole AS c ON p.RoleId = c.id
                LEFT JOIN dbo.MobileAppModule AS mp ON p.ModuleId = mp.id
                LEFT JOIN dbo.MobileAppIcon AS mi ON p.IconId = mi.id
                where p.ModuleId = '"+ moduleid + @"'
                order by mp.ModuleName
                ";
                #endregion commented

                return Json(_sqlRepository.GetDataCollection(st), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

    }
}