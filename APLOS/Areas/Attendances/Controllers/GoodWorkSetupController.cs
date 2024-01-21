#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.General.Commercial;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Productions;
using Library.Model.Setups;
using Library.OrderManagement.Sales;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class GoodWorkSetupController : BaseController
    {
        #region Constructor
        private readonly IAuthorizationConfigService _authorizationConfigService;
        private readonly ISqlRepository _sqlRepository;
        clsContract clsCon = new clsContract();
        public GoodWorkSetupController(IAuthorizationConfigService authorizationConfigService, ISqlRepository R)
        {
            _authorizationConfigService = authorizationConfigService;
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select * from (SELECT G.*,E.EmployeeName ResponsiblePerson,E.EmployeeCode ResponsiblePersonCode from  [dbo].[GoodWorkSetup] G
LEFT JOIN dbo.EmployeeInformation E on E.SystemId=G.ResponsiblePersonId) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkSetup] where UserCode='" + data["UserCode"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkSetup] where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkSetup] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("GoodWorkSetup", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }



        [HttpPost, Authorize]
        public JsonResult CreateEntity(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsBC;
            string _Id = string.Empty;
            int c = 0;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkEntitySetup where  GoodWorkSetUpId='" + goodWorkSetupId + "'", out dsBC, false, "1");
                if (data != null)
                {
                    genid.GenID("GoodWorkEntitySetup", out _Id);
                    foreach (var item in data)
                    {
                        c++;
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id + "-" + c;
                            item["GoodWorkSetUpId"] = goodWorkSetupId;

                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [Authorize, HttpPost]
        public ActionResult EntityDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkEntitySetup where Id = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkEntitySetup where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkEntitySetupData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkEntitySetupData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult Delete(string id)
        {
            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[GoodWorkSetup] where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateBudgetCode(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkBudgetSetup where  GoodWorkSetupId='" + goodWorkSetupId + "'", out dsBC, false, "1");
                if (data != null)
                {
                    int idcount = 0;
                    genid.GenID("GoodWorkBudgetSetup", out _Id);
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "BudgetId='" + item["BudgetId"] + "'";
                        idcount++;
                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id + "-" + idcount;
                            item["GoodWorkSetupId"] = goodWorkSetupId;

                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkBudgetCodeSetupData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkBudgetCodeSetupData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult BudgetCodeDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkBudgetSetUp where BudgetId in (" + Id + @")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkBudgetSetUp where BudgetId in (" + Id + ")");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeData()
        {
            JsonResult json = Json(_authorizationConfigService.GetAllEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string actionStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_authorizationConfigService.Query(identity.CompanyId, identity.PlantId, actionStatus), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateAuthority(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsAuthority;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkAuthoritySetup where  GoodWorkSetupId='" + goodWorkSetupId + "'", out dsAuthority, false, "1");
                if (data != null)
                {
                    genid.GenID("GoodWorkAuthoritySetup", out _Id);
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsAuthority.Tables[0]);
                        dv.RowFilter = "AuthorityId='" + item["AuthorityId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id;
                            item["GoodWorkSetupId"] = goodWorkSetupId;

                            AddNewRow(dsAuthority.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsAuthority);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkAuthorityData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkAuthorityData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult AuthorityDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkAuthoritySetUp where Id = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkAuthoritySetUp where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateCheckBy(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsCheckBy;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkCheckBySetUp where  GoodWorkSetupId='" + goodWorkSetupId + "'", out dsCheckBy, false, "1");
                if (data != null)
                {
                    genid.GenID("GoodWorkCheckBySetUp", out _Id);
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsCheckBy.Tables[0]);
                        dv.RowFilter = "CheckById='" + item["CheckById"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id;
                            item["GoodWorkSetupId"] = goodWorkSetupId;

                            AddNewRow(dsCheckBy.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsCheckBy);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetCheckByData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkCheckByData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult CheckByDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkCheckBySetUp where Id = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkCheckBySetUp where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

    }
}