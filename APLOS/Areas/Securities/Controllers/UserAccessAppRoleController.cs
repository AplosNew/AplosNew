using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Securities.Controllers
{
    public class UserAccessAppRoleController : Controller
    {
        // GET: Securities/UserAccessAppRole
        private readonly SqlRepository _sqlRepository;

        public UserAccessAppRoleController()
        {
            _sqlRepository = new SqlRepository();
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getRole()
        {
            string strSql = @"select Id Value,Name Text from SEC.[AppRole]";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getUser()
        {
            string strSql = @"select ''Id, E.EmployeeCode, U.Id AS UserId, E.SystemId EmployeeId, U.UserId AS [User], U.FullName, U.AuthToken , U.Active 
from EmployeeInformation E
left join SEC.[User] U  on E.SystemId = U.EmployeeId
where E.EmployeeStatus = 'Active'";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }

        #region save
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                string TableNameHead = "SEC.AppRoleMapping";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from SEC.AppRoleMapping where EmployeeId = '" + data["EmployeeId"] + "' AND RoleId= '" + data["RoleId"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                #region Upload HEAD
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);
                    data["Id"] = _Id;
                    data["CompanyGroupId"] = identity.CompanyGroupId;
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

        public ActionResult GetUserAccessedIcon(string employeeId)
        {
            try
            {
                var sql = @"select A.Id, A.EmployeeId , A.UserId ,EI.EmployeeName, A.RoleId , AR.[Name] IconName from SEC.AppRoleMapping A
                            left join SEC.AppRole AR on AR.Id = A.RoleId
                            left join EmployeeInformation EI on EI.SystemId = A.EmployeeId
                            where A.EmployeeId = '" + employeeId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
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

        #region DELETE
        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                
                string TableName = "SEC.AppRoleMapping";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        #endregion DELETE

    }
}