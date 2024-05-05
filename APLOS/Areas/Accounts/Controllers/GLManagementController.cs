#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Setups;
using Library.Service.Systems;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class GLManagementController : BaseController
    {
        string TableName = "hkp.GeneralAccountDeterminate";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IPKGeneratorService _pkGeneratorService;
        public GLManagementController(IMaterialMasterService materialMasterService, ISqlRepository R, IPKGeneratorService pkGeneratorService)
        {
            _materialMasterService = materialMasterService;
            _sqlRepository = R;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor


        public ActionResult GLManagement()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetList(string column, string value, string COAId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (
                            SELECT GAD.*,C.UserName COA,(GL.AccountCode +'-'+ GL.UserName) GLGeneralInfo,B.UserName Budget,A.UserName Activity
                            FROM [HKP].[GeneralAccountDeterminate] GAD
                            LEFT JOIN [HKP].[COA] C ON C.Id=GAD.COAId
                            LEFT JOIN [HKP].[GLGeneralInfo] GL ON GL.Id=GAD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] BM ON BM.Id=GAD.BudgetMasterId
                            LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                            LEFT JOIN [HKP].[Activity] A ON A.Id=GAD.ActivityId
                            WHERE GAD.COAId='" + COAId + @"'
                            ) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {


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


                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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

        #region GL Control
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [MST].[GLControlMaster]"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateGlManagementHeader(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagement] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _detaliId = null;
                string _Id = "";
                bplib.clsGenID genid = new bplib.clsGenID();

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenerateIDYearly(DateTime.Now.ToString(), "GLManagement", out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateGlManagementEmployeeCategory(Dictionary<string, object> data, string GlManagementId)
        {
            try
            {
                DataSet dsEmpCat;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementEmployeeCategory] where Id='" + data["Id"] + "'", out dsEmpCat, false, "1");

                string Id = "";
                #region data update
                if (dsEmpCat.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementEmployeeCategory", out Id);

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataRow dr;
                    dr = dsEmpCat.Tables[0].NewRow();

                    dr["Id"] = Id;
                    dr["EmployeeCategoryId"] = data["EmployeeCategoryId"];
                    dr["GlManagementId"] = GlManagementId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmpCat.Tables[0].Rows.Add(dr);
                }
                else
                {
                    Id = data["Id"].ToString();
                    EditRow(dsEmpCat.Tables[0].Rows[0], data);
                }

                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpCat);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementDesignation(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsDesignation;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementDesignation] where GlManagementId='" + GlManagementId + "'", out dsDesignation, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementDesignation", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsDesignation.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["GlManagementId"] = GlManagementId;
                        AddNewRow(dsDesignation.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDesignation);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementDepartment(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsDepartment;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementDepartment] where GlManagementId='" + GlManagementId + "'", out dsDepartment, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementDepartment", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsDepartment.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["GlManagementId"] = GlManagementId;
                        AddNewRow(dsDepartment.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDepartment);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementPositionCode(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsPCode;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementPositionCode] where GlManagementId='" + GlManagementId + "'", out dsPCode, false, "1");
                string Id = "";

                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementPositionCode", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsPCode.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsPCode.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPCode);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementBudgetCode(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsBudCode;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementBudgetCode] where GlManagementId='" + GlManagementId + "'", out dsBudCode, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementBudgetCode", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsBudCode.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsBudCode.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update  
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsBudCode);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public JsonResult CreateGlManagementEmployee(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsEmp;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementEmployee] where GlManagementId='" + GlManagementId + "'", out dsEmp, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementEmployee", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsEmp.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["EmpSystemId"] = item["SystemID"];
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsEmp.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmp);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public JsonResult CreateGlManagementControlDrCr(List<Dictionary<string, object>> data, string GlManagementId, string TabName)
        {
            try
            {
                DataSet dsDr, dsCr;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string Id = "";
                #region data update

                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementControlDrCr] where GlManagementId='" + GlManagementId + "'", out dsDr, false, "1");
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementControlDrCr", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsDr.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        if (Convert.ToBoolean(item["ControlDr"]) == true)
                        {
                            item["BudgetMasterActivityIdDr"] = item["BudgetMasterActivityId"];

                        }
                        if (Convert.ToBoolean(item["ControlCr"]) == true)
                        {
                            item["BudgetMasterActivityIdCr"] = item["BudgetMasterActivityId"];

                        }
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsDr.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false && item["Id"].ToString() != null)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDr);
               
                #endregion data update 


                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public JsonResult CreateGlManagementActionBy(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsAB;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementActionBy] where GlManagementId='" + GlManagementId + "'", out dsAB, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementActionBy", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsAB.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ActionById"] = item["SystemID"];
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsAB.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsAB);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public JsonResult CreateGlManagementApproveBy(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsAPB;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementApproveBy] where GlManagementId='" + GlManagementId + "'", out dsAPB, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementApproveBy", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsAPB.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ApproveById"] = item["SystemID"];
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsAPB.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsAPB);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public JsonResult CreateGlManagementResponsiblePersosn(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsRP;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementResponsiblePerson] where GlManagementId='" + GlManagementId + "'", out dsRP, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementResponsiblePerson", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsRP.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ResponsiblePersonId"] = item["SystemID"];
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsRP.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRP);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public JsonResult CreateGlManagementAccessControl(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsProcess;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementProcess] where GlManagementId='" + GlManagementId + "'", out dsProcess, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementProcess", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsProcess.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsProcess.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProcess);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateGlManagementProcess(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsPCode;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementProcess] where GlManagementId='" + GlManagementId + "'", out dsPCode, false, "1");
                string Id = "";

                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementProcess", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsPCode.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsPCode.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPCode);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [MST].[GLControlMaster]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public ActionResult DeleteGlControl(string id)
        {
            string sqlChild = @"select * from [MST].[GLControlDetail] where GLControlId = '" + id + "'";
            string sql = @"select * from [MST].[GLControlMaster] where Id = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [MST].[GLControlDetail] where GLControlId='" + id + "'");
                con.executeQuery("delete from [MST].[GLControlMaster] where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetGlManagementList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM [HKP].[GLManagement]) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion GL Control


        [HttpGet, Authorize]
        public JsonResult GetMaterialList(GridParameter parameters, string GlControlId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.MaterialQueryForGLControl(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult selectIDs(string materialType, string materialGroup, string materialMasterId)
        {
            try
            {
                var sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                             mm.Id as MaterialMasterId,
                             mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId, bah.Id
                            from MST.MaterialMaster mm
                            left join mst.MaterialGroupMaster mgm on mgm.Id = mm.MaterialGroupMasterId	
                            left join hkp.materialtype mt on mt.Id =  mgm.materialtypeid                                                       
                            left join trn.BinAllocationHead bah on bah.MaterialMasterId = mm.Id
                             where mt.Id = '" + materialType + "' and mgm.Id = '" + materialGroup + "'" +
                             "and mm.Id = '" + materialMasterId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                //return Json(sba.selectIDs(materialType, materialGroup, material, storagelevel), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult GetMaterialData(string glManagementId)
        {
            try
            {
                var sql = @"select ec.UserName as EmployeeCategory,glmec.EmployeeCategoryId
                            from [HKP].[GLManagementEmployeeCategory] glmec 
                            left join [HKP].[EmployeeCategory] ec on ec.Id=glmec.EmployeeCategoryId
							where glmec.GLManagementId = '" + glManagementId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetDesignationInformation(string glManagementId)
        {
            string sql = @"SELECT CheckBoxSelect=cast(case when gld.Id is null then 0 else 1 end as bit),gld.Id,D.Id DesignationId,                    D.Sequence,D.Code,D.ShortName,D.StandardName,D.UserName
                            FROM [HKP].[Designation] D   
                            left join(select * from  [HKP].[GLManagementDesignation] where GlManagementId='" + glManagementId + @"') gld on gld.DesignationId = D.Id
                            WHERE Active = 1";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDepartmentInformation(string glManagementId)
        {
            string sql = @"select CheckBoxSelect=cast(case when gld.Id is null then 0 else 1 end as bit),gld.Id,D.Id    DepartmentId,D.Code,D.Sequence,D.ShortName,D.StandardName,D.UserName DepartmentName,D.Description,D.Remarks 
						                from ORG.Department D
										left join(select * from  [HKP].[GLManagementDepartment] where GlManagementId='" + glManagementId + @"') gld on gld.DepartmentId = D.Id";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteEmployeeCategory(string Id)
        {
            string sql = @"select * from [HKP].[GLManagementEmployeeCategory] where EmployeeCategoryId = '" + Id + @"'";
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [HKP].[GLManagementEmployeeCategory] where EmployeeCategoryId = '" + Id + @"'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetPositionCode(string GlManagementId)
        {
            string designationsql = @"select DesignationId from  [HKP].[GLManagementDesignation] where GlManagementId='"+ GlManagementId + "'";
            var tempDesignationData = _sqlRepository.GetDataCollection(designationsql);
            var tempDesignationQuery = "";
            if (tempDesignationData.Count > 0)
            {
                tempDesignationQuery = @"AND P.DesignationId IN (select DesignationId from  [HKP].[GLManagementDesignation] where GlManagementId='" + GlManagementId + "')";
            }
            string departmentsql = @"select DepartmentId from  [HKP].[GLManagementDepartment] where GlManagementId='" + GlManagementId + "'";
            var tempDepartmentData = _sqlRepository.GetDataCollection(departmentsql);
            var tempDepartmentQuery = "";
            if (tempDepartmentData.Count > 0)
            {
                tempDepartmentQuery = @"AND P.DepartmentId IN (select DepartmentId from  [HKP].[GLManagementDepartment] where GlManagementId='" + GlManagementId + "')";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select CheckBoxSelect=cast(case when glpc.Id is null then 0 else 1 end as bit),glpc.Id,P.Id PositionCodeId,P.Code,P.UserName Position,P.Activity,
                        DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection
                        from ORG.Position P	
                        LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                        LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
                        LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
                        left outer join MST.DesignationMaster DM ON DM.DesignationId=P.DesignationId
                        left join(select * from  [HKP].[GLManagementPositionCode] where GlManagementId='" + GlManagementId + @"') glpc on glpc.PositionCodeId=p.Id
						where P.Active = 1  "+ tempDesignationQuery + "  "+ tempDepartmentQuery + "";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult Getemployeelist(string GlManagementId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string designationsql = @"select DesignationId from  [HKP].[GLManagementDesignation] where GlManagementId='" + GlManagementId + "'";
                var tempDesignationData = _sqlRepository.GetDataCollection(designationsql);
                var tempDesignationQuery = "";
                if (tempDesignationData.Count > 0)
                {
                    tempDesignationQuery = @" AND  EMP.GivenDesignationId IN (select DesignationId from  [HKP].[GLManagementDesignation] where GlManagementId='" + GlManagementId + "')";
                }
                string departmentsql = @"select DepartmentId from  [HKP].[GLManagementDepartment] where GlManagementId='" + GlManagementId + "'";
                var tempDepartmentData = _sqlRepository.GetDataCollection(departmentsql);
                var tempDepartmentQuery = "";
                if (tempDepartmentData.Count > 0)
                {
                    tempDepartmentQuery = @" AND EMP.DepartmentId IN (select DepartmentId from  [HKP].[GLManagementDepartment] where GlManagementId='" + GlManagementId + "')";
                }

                string positionsql = @"select PositionCodeId from  [HKP].[GLManagementPositionCode] where GlManagementId='" + GlManagementId + "'";
                var tempPositionData = _sqlRepository.GetDataCollection(positionsql);
                var tempPositionQuery = "";
                if (tempPositionData.Count > 0)
                {
                    tempPositionQuery = @" AND EMP.PositionId IN (select PositionCodeId from  [HKP].[GLManagementBudgetCode] where GlManagementId='" + GlManagementId + "')";
                }

                string manpowerbudgetsql = @"select BudgetCodeId from  [HKP].[GLManagementBudgetCode] where GlManagementId='" + GlManagementId + "'";
                var tempManpowerbudgetData = _sqlRepository.GetDataCollection(positionsql);
                var tempManpowerbudgetQuery = "";
                if (tempManpowerbudgetData.Count > 0)
                {
                    tempManpowerbudgetQuery = @" AND EMP.BudgetCode IN (select BudgetCodeId from  [HKP].[GLManagementPositionCode] where GlManagementId='" + GlManagementId + "')";
                }

                var sql = @"SELECT CheckBoxSelect=cast(case when glme.Id is null then 0 else 1 end as bit),glme.Id,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,ISNULL(DeM.UserName,'') Designation,
                                        ISNULL(PR.UserName,'') PositionName,ISNULL(DEG.UserName,'') GivenDesignation,ISNULL(DEPT.UserName,'') Department,SE.UserName Section,EMP.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DeM.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EJ.JobLcSystemID,FORMAT(EJ.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                        ,C.UserName Company,AM.Address1,EMP.PresentAddress1,EMP.CellPhnNo,EC.UserName EmployeeCategory,LPM.PolicyName
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN ORG.Company C ON C.Id=EMP.CompanyId
                                        LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        left join ORG.Section SE on SE.Id=PR.SectionId
										LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=DeM.DesignationId
                                        left join [MST].[DesignationMaster] DM on DM.DesignationId=EMP.GivenDesignationId
										left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=emp.PlantId                
										left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=emp.PlantID
                                        left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN dbo.EmpDateWiseJobLocation EJ ON EJ.EmpsystemId=EMP.SystemId
										 AND EJ.SystemId=(Select top(1) SystemId from dbo.EmpDateWiseJobLocation JB Where JB.EmpSystemID=EMP.SystemId Order by EffectiveDate desc)
                                        left join(select * from  [HKP].[GLManagementEmployee] where GlManagementId='" + GlManagementId + @"') glme on glme.EmpSystemId=EMP.SystemId
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' 
                                        " + tempDesignationQuery + "  " + tempDepartmentQuery + "  " + tempPositionQuery + " " + tempManpowerbudgetQuery + "  ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetBudgetCodeList(string GlManagementId)
        {
            string designationsql = @"select DesignationId from  [HKP].[GLManagementDesignation] where GlManagementId='" + GlManagementId + "'";
            var tempDesignationData = _sqlRepository.GetDataCollection(designationsql);
            var tempDesignationQuery = "";
            if (tempDesignationData.Count > 0)
            {
                tempDesignationQuery = @" AND  PRD.DesignationId IN (select DesignationId from  [HKP].[GLManagementDesignation] where GlManagementId='" + GlManagementId + "')";
            }
            string departmentsql = @"select DepartmentId from  [HKP].[GLManagementDepartment] where GlManagementId='" + GlManagementId + "'";
            var tempDepartmentData = _sqlRepository.GetDataCollection(departmentsql);
            var tempDepartmentQuery = "";
            if (tempDepartmentData.Count > 0)
            {
                tempDepartmentQuery = @" AND PRD.DepartmentId IN (select DepartmentId from  [HKP].[GLManagementDepartment] where GlManagementId='" + GlManagementId + "')";
            }

            string positionsql = @"select PositionCodeId from  [HKP].[GLManagementPositionCode] where GlManagementId='" + GlManagementId + "'";
            var tempPositionData = _sqlRepository.GetDataCollection(positionsql);
            var tempPositionQuery = "";
            if (tempPositionData.Count > 0)
            {
                tempPositionQuery = @" AND PMB.PositionId IN (select PositionCodeId from  [HKP].[GLManagementPositionCode] where GlManagementId='" + GlManagementId + "')";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when BC.Id is null then 0 else 1 end as bit),BC.Id,PMB.Id BudgetCodeId, PMB.Code, PMB.EntityId, ERD.UserName AS EntityName, PMB.PositionId, PRD.UserName AS PositionName,PRD.Code PositionCode,ERD.Code EntityCode, PMB.EmploymentType, PMB.IsOTEntitled, PMB.PayrollGroupId, PMB.WorkGroupId, PMB.Deployment, PRD.IsDirect , ERD.PlantId,(SELECT UserName FROM  [ORG].[Plant] WHERE Id=ERD.PlantId) AS [Plant], ERD.DivisionId, (SELECT UserName FROM  [ORG].[Division] 
                    WHERE Id=ERD.DivisionId) AS [Division], ERD.SubDivisionId, (SELECT UserName FROM  [ORG].[SubDivision] WHERE Id=ERD.SubDivisionId) AS [SubDivision], ERD.UnitId,(SELECT UserName FROM  [ORG].[Unit] WHERE Id=ERD.UnitId) AS [Unit], PRD.DepartmentId,
                    (SELECT UserName FROM [ORG].[Department] WHERE Id=PRD.DepartmentId) AS [Department], PRD.SectionId,
                    (SELECT UserName FROM [ORG].[Section] WHERE Id=PRD.SectionId) AS [Section], PRD.SubSectionId,
                    (SELECT UserName FROM [ORG].[SubSection] WHERE Id=PRD.SubSectionId) AS [SubSection], PMB.LineId, 
                    (SELECT UserName FROM  [ORG].[Line] WHERE Id=PMB.LineId) AS [Line] , PMB.ShiftDefinationId, 
                    (SELECT UserName FROM  [dbo].[ShiftDefination] WHERE SystemID=PMB.ShiftDefinationId) AS [ShiftDefination] , PRD.DesignationId,
                    (SELECT UserName FROM [HKP].[Designation] WHERE Id=PRD.DesignationId) AS [Designation]  
                    FROM [MST].[ManpowerBudget] AS PMB INNER JOIN ORG.Entity AS ERD ON PMB.EntityId=ERD.Id 
                    INNER JOIN ORG.Position AS PRD ON PMB.PositionId = PRD.Id 
                    LEFT JOIN (select * from  [HKP].[GLManagementBudgetCode] where GlManagementId='" + GlManagementId + @"') BC ON BC.BudgetCodeId=PMB.Id                 WHERE PMB.Active=1 AND PMB.Archive=0 AND ERD.CompanyGroupId='" + identity.CompanyGroupId + @"' AND ERD.CompanyId='" + identity.CompanyId + @"'
                    AND ERD.PlantId='" + identity.PlantId + @"' AND ERD.Active=1 AND ERD.Archive=0  "+ tempDesignationQuery + "  "+ tempDepartmentQuery + "  "+ tempPositionQuery + "";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetControlDrlist(string GlManagementId, string tabName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";

                sql = @"SELECT  CheckBoxSelect=cast(CASE WHEN GLDr.BudgetMasterActivityIdDr<>'' THEN 1  WHEN GLCr.BudgetMasterActivityIdCr<>'' THEN 1 ELSE 0 END as bit),
                                    AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BMA.BudgetMasterId, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName, BMA.ActivityId, A.Code ActivityCode, A.UserName ActivityName 
									,BMA.Active,BMA.Id BudgetMasterActivityId,Id=CASE WHEN gldr.BudgetMasterActivityIdDr<>'' THEN GLDr.Id ELSE GLCr.Id END
									,GLDr.BudgetMasterActivityIdDr,GLCr.BudgetMasterActivityIdCr,ControlDr=cast(CASE WHEN GLDr.BudgetMasterActivityIdDr<>'' THEN 1 ELSE 0 END as bit)
									,ControlCr=cast(CASE WHEN GLCr.BudgetMasterActivityIdCr<>'' THEN 1 ELSE 0 END as bit),Remarks=case when GLDr.Remarks<>'' then GLDr.Remarks else GLCr.Remarks end
                                    FROM [MST].[BudgetMasterActivity] BMA
									 JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
									LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
									 JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
									left join(select * from  [HKP].[GLManagementControlDrCr] where GlManagementId='" + GlManagementId + @"') GLDr on GLDr.BudgetMasterActivityIdDr=BMA.Id
									left join(select * from  [HKP].[GLManagementControlDrCr] where GlManagementId= '" + GlManagementId + @"') GLCr on GLCr.BudgetMasterActivityIdCr=BMA.Id
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND  GLCI.CompanyId='" + identity.CompanyId + @"' 
									AND BMA.Active=1 AND BM.Active=1";


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetActionBylist(string GlManagementId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when gla.Id is null then 0 else 1 end as bit),gla.Id,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,DeM.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,SE.UserName Section,EMP.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DeM.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EJ.JobLcSystemID,FORMAT(EJ.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                        ,C.UserName Company,AM.Address1,EMP.PresentAddress1,EMP.CellPhnNo,EC.UserName EmployeeCategory,LPM.PolicyName
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN ORG.Company C ON C.Id=EMP.CompanyId
                                        LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        left join ORG.Section SE on SE.Id=PR.SectionId
										LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=DeM.DesignationId
                                        left join [MST].[DesignationMaster] DM on DM.DesignationId=EMP.GivenDesignationId
										left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=emp.PlantId                
										left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=emp.PlantID
                                        left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN dbo.EmpDateWiseJobLocation EJ ON EJ.EmpsystemId=EMP.SystemId
										 AND EJ.SystemId=(Select top(1) SystemId from dbo.EmpDateWiseJobLocation JB Where JB.EmpSystemID=EMP.SystemId Order by EffectiveDate desc)
                                        left join(select * from  [HKP].[GLManagementActionBy] where GlManagementId='" + GlManagementId + @"') gla on gla.ActionById=EMP.SystemId
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetApproveBylist(string GlManagementId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when glap.Id is null then 0 else 1 end as bit),glap.Id,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,DeM.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,SE.UserName Section,EMP.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DeM.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EJ.JobLcSystemID,FORMAT(EJ.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                        ,C.UserName Company,AM.Address1,EMP.PresentAddress1,EMP.CellPhnNo,EC.UserName EmployeeCategory,LPM.PolicyName
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN ORG.Company C ON C.Id=EMP.CompanyId
                                        LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        left join ORG.Section SE on SE.Id=PR.SectionId
										LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=DeM.DesignationId
                                        left join [MST].[DesignationMaster] DM on DM.DesignationId=EMP.GivenDesignationId
										left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=emp.PlantId                
										left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=emp.PlantID
                                        left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN dbo.EmpDateWiseJobLocation EJ ON EJ.EmpsystemId=EMP.SystemId
										 AND EJ.SystemId=(Select top(1) SystemId from dbo.EmpDateWiseJobLocation JB Where JB.EmpSystemID=EMP.SystemId Order by EffectiveDate desc)
                                        left join(select * from  [HKP].[GLManagementApproveBy] where GlManagementId='" + GlManagementId + @"') glap on glap.ApproveById=EMP.SystemId
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetResponsiblePersonlist(string GlManagementId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when glrp.Id is null then 0 else 1 end as bit),glrp.Id,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,DeM.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,SE.UserName Section,EMP.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DeM.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EJ.JobLcSystemID,FORMAT(EJ.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                        ,C.UserName Company,AM.Address1,EMP.PresentAddress1,EMP.CellPhnNo,EC.UserName EmployeeCategory,LPM.PolicyName
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN ORG.Company C ON C.Id=EMP.CompanyId
                                        LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        left join ORG.Section SE on SE.Id=PR.SectionId
										LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=DeM.DesignationId
                                        left join [MST].[DesignationMaster] DM on DM.DesignationId=EMP.GivenDesignationId
										left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=emp.PlantId                
										left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=emp.PlantID
                                        left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN dbo.EmpDateWiseJobLocation EJ ON EJ.EmpsystemId=EMP.SystemId
										 AND EJ.SystemId=(Select top(1) SystemId from dbo.EmpDateWiseJobLocation JB Where JB.EmpSystemID=EMP.SystemId Order by EffectiveDate desc)
                                        left join(select * from  [HKP].[GLManagementResponsiblePerson] where GlManagementId='" + GlManagementId + @"') glrp on glrp.ResponsiblePersonId=EMP.SystemId
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetEmployeeData(string glManagementId)
        {
            try
            {
                var sql = @"select CheckBoxSelect=cast(case when GLME.Id is null then 0 else 1 end as bit),GLME.Id,GLME.EmpSystemId,EI.EmployeeCode,EI.EmployeeName
                            from [HKP].[GLManagementEmployee] GLME
                            left join dbo.EmployeeInformation EI on EI.SystemId=GLME.EmpSystemId
							where GLME.GLManagementId = '" + glManagementId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetGLManagementProcessData(string glManagementId)
        {
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when GMP.Id is null then 0 else 1 end as bit),GMP.Id,VT.UserName VoucherType,VTM.SourceType Process
										FROM SCS.VoucherTypeMatrix VTM
										LEFT JOIN SCS.VoucherType VT ON VT.Id=VTM.VoucherTypeId
										LEFT JOIN (SELECT * FROM  [HKP].[GLManagementProcess] WHERE GlManagementId='" + glManagementId + @"') GMP  ON VTM.SourceType=GMP.Process order by VTM.SourceType ";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetGLManagementViewData(string glManagementId)
        {
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when GMP.Id is null then 0 else 1 end as bit),GMP.Id,VT.UserName VoucherType,VTM.SourceType Process
										FROM SCS.VoucherTypeMatrix VTM
										LEFT JOIN SCS.VoucherType VT ON VT.Id=VTM.VoucherTypeId
										LEFT JOIN (SELECT * FROM  [HKP].[GLManagementProcess] WHERE GlManagementId='" + glManagementId + @"') GMP  ON VTM.SourceType=GMP.Process order by VTM.SourceType ";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetGLControlReport(string glControlId)
        {
            try
            {
                string fileName = "";
                fileName = GLControlReport(glControlId, "GL Control Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GLControlReport(string glControlId, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Material";
                sheet = workbook.Worksheets[0];

                DataTable headerData, data, typeData;
                GLControlHeaderSql(glControlId, out headerData);
                GlControlReportSQL(glControlId, out data);
                GlControlTypeReportSQL(glControlId, out typeData);


                if (headerData.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                int ROW = 6; int COL = 1;
                sheet.Range[ROW, COL].Text = "Sequence";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Sequence"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Code";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Code"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "User Defined Name";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["UserName"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Description";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Description"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                sheet.Range[6, 1, ROW, 3].CellStyle.Font.Bold = true;
                sheet.Range[6, 1, ROW, 3].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[6, 1, ROW, 3].BorderInside(ExcelLineStyle.Hair);


                ROW = 6; COL = 4;
                sheet.Range[ROW, COL].Text = "Short Name";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["ShortName"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Standard Name";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["StandardName"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Remarks";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                sheet.Range[6, 4, ROW, 6].CellStyle.Font.Bold = true;
                sheet.Range[6, 4, ROW, 6].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[6, 4, ROW, 6].BorderInside(ExcelLineStyle.Hair);

                ROW = 10; COL = 1;
                ROW++;

                #region Material

                #region columns
                sheet[ROW, COL].Text = "Material Type";
                sheet[ROW, COL].ColumnWidth = 18;
                int ColMaterialType = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Group";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColMaterialGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMaterialCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Name";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColMaterialName = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;


                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColMaterialType].Text = data.Rows[i]["Type"].ToString();
                    sheet[ROW, ColMaterialGroup].Text = data.Rows[i]["MaterialGroup"].ToString();
                    sheet[ROW, ColMaterialCode].Text = data.Rows[i]["MaterialCode"].ToString();
                    sheet[ROW, ColMaterialName].Text = data.Rows[i]["MaterialName"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();
                #endregion Material


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "GL Control Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                #region Material Type

                workbook.Worksheets[1].Name = "Material Type";
                sheet = workbook.Worksheets[1];

                ROW = 6; COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Material Type";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColMaterialTypes = COL;
                COL++;

                sheet[ROW, COL].Text = "GL Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColGLCode = COL;
                COL++;

                sheet[ROW, COL].Text = "GL";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColGL = COL;
                COL++;

                sheet[ROW, COL].Text = "Budget";
                sheet[ROW, COL].ColumnWidth = 18;
                int ColBudget = COL;
                COL++;

                sheet[ROW, COL].Text = "Activity";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "BudgetRefNo";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColBudgetRefNo = COL;

                #endregion columns

                int endsCol = COL;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endsCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                startRow = ROW;

                for (int i = 0; i < typeData.Rows.Count; i++)
                {
                    sheet[ROW, ColMaterialTypes].Text = typeData.Rows[i]["MaterialType"].ToString();
                    sheet[ROW, ColGLCode].Text = typeData.Rows[i]["GLGeneralInfoCode"].ToString();
                    sheet[ROW, ColGL].Text = typeData.Rows[i]["GL"].ToString();
                    sheet[ROW, ColBudget].Text = typeData.Rows[i]["BudgetName"].ToString();
                    sheet[ROW, ColActivity].Text = typeData.Rows[i]["Activity"].ToString();
                    sheet[ROW, ColBudgetRefNo].Text = typeData.Rows[i]["BudgetRefNo"].ToString();

                    sheet.Range[ROW, 1, ROW, endsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "GL Control Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                #endregion Material Type

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GLControlHeaderSql(string glControlId, out DataTable headerData)
        {
            try
            {
                string strSQL = @"select * from mst.GLControlMaster
							where Id ='" + glControlId + @"'";
                headerData = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void GlControlReportSQL(string glControlId, out DataTable data)
        {
            try
            {
                string strSQL = @"select MT.UserName [Type],MGM.UserName MaterialGroup,MM.Code MaterialCode,MM.UserName MaterialName
                                                from mst.MaterialMaster MM
												left join mst.GLControlMaster GLM on MM.GLControlMasterId=GLM.Id
                                                left join MST.MaterialGroupMaster MGM on MGM.Id=MM.MaterialGroupMasterId
                                                left join hkp.materialType MT on MT.Id=MGM.MaterialTypeId
							                    where MM.GLControlMasterId = '" + glControlId + @"'";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void GlControlTypeReportSQL(string glControlId, out DataTable typeData)
        {
            try
            {
                string strSQL = @"select GLD.[Type] MaterialType,GL.AccountCode AS GLGeneralInfoCode,GL.UserName GL,B.BudgetName,A.UserName Activity,B.BudgetRefNo 
                                                    from  MST.GLControlDetail GLD 
                                                    left join [HKP].[GLGeneralInfo] GL on GL.Id=GLD.GLGeneralInfoId
                                                    left join [HKP].[Activity] A on A.Id=GLD.ActivityId
                                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.RefNo BudgetRefNo
	                                                    FROM [HKP].[Budget] AS B
                                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                                    ) AS B ON B.BudgetMasterId=GLD.BudgetMasterId
							                        where GLD.GLControlId ='" + glControlId + @"'";

                typeData = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetGlManagementAccessControllist(string GlManagementId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                sql = @"select distinct x.EmployeeName,x.*,GMDC.BudgetMasterActivityIdDr ControlDrId,B.UserName BudgetMasterActivityDr,GMDC.BudgetMasterActivityIdCr ControlCrId,BB.UserName BudgetMasterActivityCr
					,GMAB.ActionById,EIAB.EmployeeName ActionBy,GMAPB.ApproveById,EIAPB.EmployeeName ApproveBy,GMRP.ResponsiblePersonId,EIRP.EmployeeName ResponsiblePerson
					,CheckBoxSelect=cast(case when glrp.Id is null then 0 else 1 end as bit),glrp.Id
					
                    from(select DISTINCT GEC.EmployeeCategoryId,EC.UserName EmployeeCategorys
					,GMD.DesignationId,DE.UserName Designation,GMDP.DepartmentId,DP.UserName Department,GMPC.PositionCodeId,PO.UserName Position
					,GMBC.BudgetCodeId,GME.EmpSystemId EmpId,EI.EmployeeName,MB.Code BudgetCode,GLM.Id GLManagementId
                    from HKP.GLManagement GLM 
                    LEFT JOIN HKP.GLManagementEmployeeCategory GEC ON GEC.GLManagementId=GLM.Id
                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=GEC.EmployeeCategoryId
					LEFT JOIN [HKP].[GLManagementEmployee] GME ON GME.GLManagementId=GLM.Id
                    LEFT JOIN [DBO].[EmployeeInformation] EI ON EI.SystemId=GME.EmpSystemId 
										   
					LEFT JOIN [HKP].[GLManagementPositionCode] GMPC ON GMPC.GLManagementId=GLM.Id AND GMPC.PositionCodeId=EI.PositionID
                    LEFT JOIN ORG.Position PO  ON PO.Id=GMPC.PositionCodeId  
                    LEFT JOIN [HKP].[GLManagementDesignation] GMD ON GMD.GLManagementId=GLM.Id AND PO.DesignationId=GMD.DesignationId AND EI.GivenDesignationId=GMD.DesignationId
                    LEFT JOIN [HKP].[Designation] DE ON DE.Id=GMD.DesignationId AND PO.DesignationId=GMD.DesignationId
                    LEFT JOIN [HKP].[GLManagementDepartment] GMDP ON GMDP.GLManagementId=GLM.Id AND PO.DepartmentId=GMDP.DepartmentId
                    LEFT JOIN [ORG].[Department] DP ON DP.Id=GMDP.DepartmentId AND PO.DepartmentId=GMDP.DepartmentId AND EI.DepartmentId=GMDP.DepartmentId
					 LEFT JOIN [HKP].[GLManagementBudgetCode] GMBC ON GMBC.GLManagementId=GLM.Id    AND EI.BudgetCode=GMBC.BudgetCodeId 
                    LEFT JOIN [MST].ManpowerBudget MB ON MB.Id=GMBC.BudgetCodeId AND MB.PositionId=PO.Id 
                   WHERE GLM.Id='" + GlManagementId + @"')x 					

                    LEFT JOIN [HKP].[GLManagementControlDrCr] GMDC ON GMDC.GLManagementId=x.GLManagementId
                    LEFT JOIN [MST].[BudgetMasterActivity] BMA on BMA.Id=GMDC.BudgetMasterActivityIdDr
                    LEFT JOIN [MST].[BudgetMaster] BM on BM.Id=BMA.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] B on B.Id=BM.BudgetId
                    LEFT JOIN [MST].[BudgetMasterActivity] BMAC on BMAC.Id=GMDC.BudgetMasterActivityIdCr
                    LEFT JOIN [MST].[BudgetMaster] BMM on BMM.Id=BMAC.BudgetMasterId
					LEFT JOIN [HKP].[Budget] BB on BB.Id=BMM.BudgetId

					LEFT JOIN [HKP].[GLManagementActionBy] GMAB ON GMAB.GLManagementId=x.GLManagementId
                    LEFT JOIN [DBO].[EmployeeInformation] EIAB ON EIAB.SystemId=GMAB.ActionById
					LEFT JOIN [HKP].[GLManagementApproveBy] GMAPB ON GMAPB.GLManagementId=x.GLManagementId
                    LEFT JOIN [DBO].[EmployeeInformation] EIAPB ON EIAPB.SystemId=GMAPB.ApproveById 
					LEFT JOIN [HKP].[GLManagementResponsiblePerson] GMRP ON GMRP.GLManagementId=x.GLManagementId
                    LEFT JOIN [DBO].[EmployeeInformation] EIRP ON EIRP.SystemId=GMRP.ResponsiblePersonId 
                    left join(select * from  [HKP].[GLManagementProcess] where GlManagementId='" + GlManagementId + @"') glrp on glrp.GLManagementId=x.GLManagementId
                    WHERE x.GLManagementId='" + GlManagementId + @"'"; 
                //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}