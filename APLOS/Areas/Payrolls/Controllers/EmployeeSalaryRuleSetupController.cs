#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Payrolls.SalaryStructure;
using Library.Service.Setups;
using Library.Service.TaskScheduler;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Payrolls.Controllers
{
    public class EmployeeSalaryRuleSetupController : BaseController
    {

        string TableName = "hkp.EmployeeSalaryRuleSetup";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        clsSalaryStructureUpload clsSSU = new clsSalaryStructureUpload();
        public EmployeeSalaryRuleSetupController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult SalaryStructure()
        {
            return View();
        }

     

        public async Task<ActionResult> SalaryProcess()
        {
            return await Task.Factory.StartNew(() =>
            {

                var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SendNotification("Status: Ready To Process");

                return View();
            });

        }//

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT SalaryHeadID as Value,SalaryHead AS Text FROM dbo.SalaryHead"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.EmployeeSalaryRuleSetup wher Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

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

        public ActionResult Delete(string id)
        {
            string sql = @"select * from '" + TableName + "' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpPost, Authorize]
        public JsonResult CreateSalaryRuleEmployeeType(Dictionary<string, object> data, string masterId)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsEmpCat, dsDD;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                con.OpenDataSetThroughAdapter("select * from [dbo].[SalaryRuleEmployeeType] where EmployeeTypeId='" + data["EmployeeTypeId"] + "' AND EmployeeSalaryRuleSetupId='" + masterId + "' AND  Id<>'" + data["Id"] + "'", out dsEmpCat, false, "1");
                if (dsEmpCat.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Employee Type already exists!!!");
                con.OpenDataSetThroughAdapter("select * from [dbo].[SalaryRuleEmployeeType] where Id='" + data["Id"] + "'", out dsEmpCat, false, "1");

                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[SalaryRuleEmployeeType] where EmployeeSalaryRuleSetupId='" + masterId + "'", out dsDD, false, "1");
                int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());
                string Id = "";
                #region data update
                if (dsEmpCat.Tables[0].Rows.Count == 0)
                {
                    ccount++;
                    DataRow dr;
                    dr = dsEmpCat.Tables[0].NewRow();

                    dr["Id"] = materialCommonService.MakePK(masterId, ccount, 2);
                    dr["EmployeeSalaryRuleSetupId"] = masterId;
                    dr["EmployeeTypeId"] = data["EmployeeTypeId"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
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

        [HttpGet, Authorize]
        public ActionResult GetSalaryRuleEmployeeTypeData(string masterId)
        {
            try
            {
                var sql = @"select ec.Sequence,ec.Code,ec.ShortName,ec.StandardName,ec.UserName as EmployeeCategory,glmec.*
                            from [dbo].[SalaryRuleEmployeeType] glmec 
                            left join [HKP].[EmployeeCategory] ec on ec.Id=glmec.EmployeeTypeId
							where glmec.EmployeeSalaryRuleSetupId = '" + masterId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteEmployeeCategory(string id)
        {
            DeleteEmployeeCategoryData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteEmployeeCategoryData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[SalaryRuleEmployeeType] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function


        [HttpGet, Authorize]
        public ActionResult GetDesignationData(string ecId)
        {
            try
            {
                var sql = @"SELECT distinct DG.Id,DG.Sequence,DG.Code,DG.ShortName,DG.UserName,DG.StandardName, Flag=CAST(0 AS bit),EC.UserName EmployeeCategory 
FROM HKP.Designation DG
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=DG.Id
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
WHERE DG.Active=1 AND DG.Id NOT IN(SELECT DesignationId FROM [dbo].[SalaryRuleDesignation]) 
AND EC.Id " + ecId + @"";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryRuleDesignationData(string masterId)
        {
            try
            {
                var sql = @"select ec.Sequence,ec.Code,ec.ShortName,ec.StandardName,ec.UserName,glmec.*
                            from [dbo].SalaryRuleDesignation glmec 
                            left join [HKP].[Designation] ec on ec.Id=glmec.DesignationId
							where glmec.EmployeeSalaryRuleSetupId = '" + masterId + "' Order By ec.UserName";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDesignation(string id)
        {
            DeleteSalaryRuleDesignationData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteSalaryRuleDesignationData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[SalaryRuleDesignation] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function


        [HttpPost, Authorize]
        public JsonResult CreateDesignation(List<Dictionary<string, object>> data, string masterId)
        {
            try
            {
                DataSet dsDesignation, dsDD;
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[SalaryRuleDesignation] where EmployeeSalaryRuleSetupId='" + masterId + "'", out dsDesignation, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[SalaryRuleDesignation] where EmployeeSalaryRuleSetupId='" + masterId + "'", out dsDD, false, "1");
                int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());

                string Id = "";

                #region data update
                foreach (var item in data)
                {

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsDesignation.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        ccount++;
                        item["Id"] = materialCommonService.MakePK(masterId, ccount, 2);
                        item["EmployeeSalaryRuleSetupId"] = masterId;

                        AddNewRow(dsDesignation.Tables[0], item);
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

        [HttpGet, Authorize]
        public ActionResult GetProcessParameterList(string masterId)
        {

            string sql = @"SELECT N.*,AG.UserName AS DrAccountGroupName,CAG.UserName AS CrAccountGroupName, DrA.UserName DrActivityName, CrA.UserName CrActivityName  FROM [dbo].[EmployeeSalaryRuleItem] N
LEFT JOIN [MST].[BudgetMasterActivity] Dr ON Dr.Id=N.DrBudgetMasterActivityId
LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=Dr.BudgetMasterId
LEFT JOIN [HKP].[Activity] DrA ON DrA.Id=Dr.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [MST].[BudgetMasterActivity] Cr ON Cr.Id=N.CrBudgetMasterActivityId
									LEFT JOIN [HKP].[Activity] CrA ON CrA.Id=Cr.ActivityId
                                    LEFT JOIN [MST].[BudgetMaster] AS CBM ON CBM.Id=Cr.BudgetMasterId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS CGLGI ON CGLGI.Id=CBM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS CGLCI ON CGLCI.GLGeneralInfoId=CGLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS CAG ON CAG.Id=CGLGI.AccountGroupId
Where N.EmployeeSalaryRuleSetupId='" + masterId + "' Order By N.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHeaderItemCbo(string id, string masterId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value, UserName AS Text FROM [dbo].[EmployeeSalaryRuleItem] WHERE Id<>'" + id + "' AND EmployeeSalaryRuleSetupId='" + masterId + "' Order By Sequence"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateSalaryRuleItem(Dictionary<string, object> data, List<Dictionary<string, object>> details)
        {
            try
            {
                SaveSalaryRuleItemData(data, details);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult CreateSalaryRuleItemWithDefault(Dictionary<string, object> data, List<Dictionary<string, object>> details, List<Dictionary<string, object>> Itemdetails)
        {
            try
            {
                SaveSalaryRuleItemWithDefaultData(data, details, Itemdetails);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveSalaryRuleItemWithDefaultData(Dictionary<string, object> data, List<Dictionary<string, object>> details, List<Dictionary<string, object>> Itemdetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster, dsDestination, dsID = null;
                    DataRow drF;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    bplib.clsGenID genid = new bplib.clsGenID();
                    MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.EmployeeSalaryRuleItem WHERE EmployeeSalaryRuleSetupId='" + data["EmployeeSalaryRuleSetupId"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where EmployeeSalaryRuleItemId='" + data["Id"] + "'", out dsDestination, false, "1");

                    con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmployeeSalaryRuleItem] where EmployeeSalaryRuleSetupId='" + data["EmployeeSalaryRuleSetupId"] + "'", out dsID, false, "1");
                    int ccount = Convert.ToInt32(dsID.Tables[0].Rows[0]["countId"].ToString());

                    if (Itemdetails != null)
                    {
                        foreach (var item in Itemdetails)
                        {
                            ccount++;

                            drF = dsMaster.Tables[0].NewRow();

                            drF["Id"] = materialCommonService.MakePK(data["EmployeeSalaryRuleSetupId"].ToString(), ccount, 2);
                            drF["Sequence"] = item["Sequence"];
                            drF["EmployeeSalaryRuleSetupId"] = item["EmployeeSalaryRuleSetupId"];
                            drF["DrBudgetMasterActivityId"] = item["DrBudgetMasterActivityId"];
                            drF["CrBudgetMasterActivityId"] = item["CrBudgetMasterActivityId"];
                            drF["UserName"] = item["UserName"].ToString().Trim();
                            drF["StandardName"] = item["StandardName"];
                            drF["Active"] = item["Active"];
                            drF["IsReportItem"] = item["IsReportItem"];
                            drF["ViewItem"] = item["ViewItem"];
                            drF["IsDefault"] = item["IsDefault"];
                            drF["EntryState"] = item["EntryState"];
                            drF["FormulaId"] = item["FormulaId"];
                            drF["Formula"] = item["Formula"];
                            drF["AddedBy"] = identity.Name;
                            drF["AddedDate"] = DateTime.Now;
                            drF["AddedFromIP"] = identity.IPAddress; ;

                            dsMaster.Tables[0].Rows.Add(drF);
                        }
                        ccount++;
                    }

                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        data["Id"] = materialCommonService.MakePK(data["EmployeeSalaryRuleSetupId"].ToString(), ccount, 2);
                        data["Sequence"] = 5;
                        AddNewRow(dsMaster.Tables[0], data);
                    }

                    string Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    #region NoticePeriodFormulaDetail 

                    if (data["EntryState"].ToString() == "Calculate")
                    {
                        while (dsDestination.Tables[0].DefaultView.Count > 0)
                            dsDestination.Tables[0].DefaultView[0].Delete();
                        int count = 0;
                        if (details != null)
                        {

                            foreach (var item in details)
                            {
                                drF = dsDestination.Tables[0].NewRow();
                                count++;
                                string pk = _Id + "_" + count;
                                drF["Id"] = pk;
                                drF["EmployeeSalaryRuleItemId"] = _Id;
                                drF["Sequence"] = item["Sequence"];
                                drF["EmployeeSalaryRuleItemHeadId"] = item["EmployeeSalaryRuleItemHeadId"];
                                drF["Component"] = item["Component"];

                                dsDestination.Tables[0].Rows.Add(drF);
                            }

                        }
                    }
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDestination);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveSalaryRuleItemData(Dictionary<string, object> data, List<Dictionary<string, object>> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster, dsDestination, dsID, dsFormulaID = null;
                    DataRow drF;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                    con.OpenDataSetThroughAdapter("select * from EmployeeSalaryRuleItem where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "' AND EmployeeSalaryRuleSetupId='" + data["EmployeeSalaryRuleSetupId"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("UserName already exists!!!");


                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.EmployeeSalaryRuleItem WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where EmployeeSalaryRuleItemId='" + data["Id"] + "'", out dsDestination, false, "1");

                    con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmployeeSalaryRuleItem] where EmployeeSalaryRuleSetupId='" + data["EmployeeSalaryRuleSetupId"] + "'", out dsID, false, "1");
                    int ccount = Convert.ToInt32(dsID.Tables[0].Rows[0]["countId"].ToString());


                    con.OpenDataSetThroughAdapter("SELECT count(Id) countId FROM dbo.FormulaDetail Where EmployeeSalaryRuleItemId IN(SELECT Id FROM dbo.EmployeeSalaryRuleItem Where EmployeeSalaryRuleSetupId='" + data["EmployeeSalaryRuleSetupId"] + "')", out dsFormulaID, false, "1");
                    int count = Convert.ToInt32(dsFormulaID.Tables[0].Rows[0]["countId"].ToString());

                    if (data["EntryState"].ToString() == "Entry")
                    {
                        data["Formula"] = DBNull.Value;
                        data["FormulaId"] = DBNull.Value;


                        while (dsDestination.Tables[0].DefaultView.Count > 0)
                            dsDestination.Tables[0].DefaultView[0].Delete();
                    }
                    data["UserName"] = data["UserName"].ToString().Replace(" ", "");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        ccount++;

                        data["Id"] = materialCommonService.MakePK(data["EmployeeSalaryRuleSetupId"].ToString(), ccount, 3);
                        _Id = data["Id"].ToString();
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();

                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                    string Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    #region NoticePeriodFormulaDetail 

                    if (data["EntryState"].ToString() == "Calculate")
                    {
                        while (dsDestination.Tables[0].DefaultView.Count > 0)
                            dsDestination.Tables[0].DefaultView[0].Delete();

                        if (details != null)
                        {

                            foreach (var item in details)
                            {
                                drF = dsDestination.Tables[0].NewRow();
                                count++;
                                string pk = materialCommonService.MakePK(_Id, count, 3);
                                drF["Id"] = pk;
                                drF["EmployeeSalaryRuleItemId"] = _Id;
                                drF["Sequence"] = item["Sequence"];
                                drF["EmployeeSalaryRuleItemHeadId"] = item["EmployeeSalaryRuleItemHeadId"];
                                drF["Component"] = item["Component"];

                                dsDestination.Tables[0].Rows.Add(drF);
                            }

                        }
                    }
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDestination);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string ItemId)
        {

            string sql = @"SELECT D.Sequence,D.EmployeeSalaryRuleItemHeadId
                            ,SalaryHead= CASE WHEN ISNULL(SD.UserName,'')<>'' THEN SD.UserName ELSE D.Component END,D.Component,D.EmployeeSalaryRuleItemId
                            FROM [dbo].[FormulaDetail] D
                            LEFT JOIN dbo.EmployeeSalaryRuleItem SD ON SD.Id=D.EmployeeSalaryRuleItemHeadId
                            WHERE EmployeeSalaryRuleItemId='" + ItemId + "' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetSalaryRuleItemAutoSequence(string masterId)
        {
            return Json(GetSalaryRuleItemSequence(masterId), JsonRequestBehavior.AllowGet);
        }
        private double GetSalaryRuleItemSequence(string masterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  ISNULL(Max(Sequence),0) AS Sequence FROM dbo.EmployeeSalaryRuleItem Where EmployeeSalaryRuleSetupId='" + masterId + "'");
            double seq = 0;
            if (dt.Rows.Count > 0)
            {
                if (clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) == 0)
                {
                    seq = 15;
                }
                else
                {
                    seq = clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
                }
            }
            return seq;
        }

        [HttpPost, Authorize]
        public JsonResult DeleteEmployeeSalaryRuleItem(string id)
        {
            DeleteEmployeeSalaryRuleItemData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteEmployeeSalaryRuleItemData(string SystemID)
        {
            string strSQL, strFSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.EmployeeSalaryRuleItem WHERE Id = '" + SystemID + "'";
                strFSQL = "DELETE FROM dbo.FormulaDetail WHERE EmployeeSalaryRuleItemId = '" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper(strFSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function


        [Authorize, HttpGet]
        public ActionResult GetControlDrlist(string tabName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                if (tabName == "ControlDr")
                {
                    sql = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BMA.BudgetMasterId, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName, BMA.ActivityId, A.Code ActivityCode, A.UserName ActivityName 
									,BMA.Active,BMA.Id BudgetMasterActivityId
                                    FROM [MST].[BudgetMasterActivity] BMA
									 JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
									LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
									 JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
									WHERE GLGI.Archive=0 AND GLGI.Active=1 AND  GLCI.CompanyId='" + identity.CompanyId + @"' AND BMA.Active=1 AND BM.Active=1";
                }
                else
                {
                    sql = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BMA.BudgetMasterId, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName, BMA.ActivityId, A.Code ActivityCode, A.UserName ActivityName 
									,BMA.Active,BMA.Id BudgetMasterActivityId
                                    FROM [MST].[BudgetMasterActivity] BMA
									 JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
									LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
									 JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
									WHERE GLGI.Archive=0 AND GLGI.Active=1 AND  GLCI.CompanyId='" + identity.CompanyId + @"' AND BMA.Active=1 AND BM.Active=1";
                }

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region New SalaryStructure

        [HttpGet, Authorize]
        public ActionResult GetEmployeeSalaryData(string empId, string designationId)
        {
            try
            {
                var sql = @"Select distinct SG.SalaryHeadID,SH.SalaryHead,SLID.SystemID,IH.SystemID IncrementHistoryId,
	HeadType = CASE WHEN SH.HeadType = 'D' THEN 'Deduction' WHEN SH.HeadType = 'E' THEN 'Earning'  ELSE '' END
	,E.SystemId EmpInfoSystemID,E.EmployeeName,E.EmployeeCode,E.BudgetCode,EN.UserName Entity, P.UserName Position,LD.UserName LegalDesignation,DG.UserName DesignationGroup,D.UserName GivenDesignation,SG.EmployeeSalaryRuleSetupId
	,E.PlantId,C.CompanyGroupId,SH.Sequence,E.GivenDesignationId,E.LegalDesignationId,SLID.EffectiveDate,SLID.NextDueDate,FORMAT(E.DOJ,'dd-MMM-yyyy')DOJ
		FROM dbo.EmployeeSalaryRuleItem SG
	INNER JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
	LEFT JOIN (
		SELECT SDM.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.SalaryIncrementSystemID, SDM.EmployeeSalaryRuleSetupId, 
				SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
				SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,FORMAT(SDM.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
				,FORMAT(SDM.NextDueDate,'dd-MMM-yyyy')NextDueDate
		FROM SalaryInfoDefineMaster SDM
							INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
		WHERE SDM.EmpInfoSystemID = '" + empId + @"'
					AND SDM.EffectiveDate IN (
											SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
												WHERE EmpInfoSystemID = '" + empId + @"'
													--AND EffectiveDate = '01-Jan-2026'
											)
		) SLID ON SG.EmployeeSalaryRuleSetupId = SLID.EmployeeSalaryRuleSetupId 
		LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=" + empId + @"
		LEFT JOIN MST.ManpowerBudget MB ON MB.Id=E.BudgetCode
		LEFT JOIN ORG.Position P ON P.Id=MB.PositionId
		LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
		LEFT JOIN HKP.LegalDesignation LD ON LD.Id=E.LegalDesignationId
		LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationId
		LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
        LEFT JOIN HKP.Designation D ON D.id=E.GivenDesignationId
        LEFT JOIN ORG.Company C ON C.id=E.CompanyId
LEFT JOIN dbo.IncrementHistory IH ON IH.EmpSystemId=E.SystemId AND IH.SystemId=(Select top 1 SystemId From dbo.IncrementHistory Where EmpSystemId='2500255' Order by ToEffectiveDate DESC) 
	WHERE SG.EmployeeSalaryRuleSetupId=(Select EmployeeSalaryRuleSetupId from SalaryRuleDesignation Where DesignationId IN('" + designationId + @"'))
	AND SG.EntryState='Entry'  Order by SH.SalaryHead";
                var salaryItem = _sqlRepository.GetDataCollection(sql);

                string ssql = @"select SystemID,EmpInfoSystemID,GroupID,PlantID,FORMAT(EffectiveDate,'dd-MMM-yyyy')EffectiveDate,FORMAT(NextDueDate,'dd-MMM-yyyy')NextDueDate,EmployeeSalaryRuleSetupId,IsApproved from dbo.SalaryInfoDefineMaster Where EmpInfoSystemID='" + empId + @"'";
                var salaryData = _sqlRepository.GetDataCollection(ssql);
                return Json(new { salaryItem, salaryData }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryInfoData(string SalaryID)
        {
            try
            {
                string sql = @"Select h.SalaryHead,s.DefineAmount Amount,S.SystemID,s.SalaryID,
HeadType = CASE WHEN h.HeadType = 'D' THEN 'Deduction' WHEN h.HeadType = 'E' THEN 'Earning'  ELSE '' END
from dbo.SalaryInfoDefine s
left join dbo.SalaryHead h on h.SalaryHeadID=s.SalaryHeadID
where s.SalaryID='" + SalaryID + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSalary(Dictionary<string, object> master, List<Dictionary<string, object>> data, Dictionary<string, object> IncrementHistory)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsChild, dsIH;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalaryInfoDefineMaster where  SystemID='" + master["SystemID"] + "'", out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalaryInfoDefine where  SalaryID='" + master["SystemID"] + "'", out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.IncrementHistory where  SystemID='" + master["IncrementHistoryId"] + "'", out dsIH, false, "1");
                bplib.clsGenID objGenID = new bplib.clsGenID();
                string strSystemID = null;
                string strIHSystemID = null;
                if (master != null)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "SystemID='" + master["SystemID"] + "'";

                    if (dv.Count == 0)
                    {
                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARY_INFO", out strSystemID);
                        strSystemID = "SALR" + strSystemID;

                        master["SystemID"] = strSystemID;
                        NewAddRow(dsMaster.Tables[0], master);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        NewEditRow(drmo, master);
                    }


                }

                string _PK_SLrDef = string.Empty;
                int count = 0;
                if (data != null)
                {
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARYINFODEFINE", out _PK_SLrDef);

                    foreach (var item in data)
                    {

                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "SystemID='" + item["SystemID"] + "'";

                        if (dv.Count == 0)
                        {
                            count++;
                            item["SystemID"] = bplib.clsWebLib.RetValidLen("SD" + _PK_SLrDef + "-" + count);
                            item["SalaryID"] = strSystemID;

                            NewAddRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            NewEditRow(drmo, item);
                        }
                    }


                }

                if (IncrementHistory != null)
                {
                    DataView dv = new DataView(dsIH.Tables[0]);
                    dv.RowFilter = "SystemID='" + IncrementHistory["SystemID"] + "'";

                    if (dv.Count == 0)
                    {
                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "IncrementHistory", out strIHSystemID);

                        IncrementHistory["SystemID"] = strIHSystemID;
                        IncrementHistory["ToSalaryId"] = strSystemID;

                        AddNewRow(dsIH.Tables[0], IncrementHistory);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, IncrementHistory);
                    }


                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild, dsIH);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }


        private void NewAddRow(DataTable dt, Dictionary<string, object> sourceData)
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
            dr["DateAdded"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

        private void NewAddedRow(DataTable dt, Dictionary<string, object> sourceData)
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
            dr["DateAdded"] = System.DateTime.Now.ToString();


            dt.Rows.Add(dr);
        }
        private void NewAddedLogRow(DataTable dt, Dictionary<string, object> sourceData)
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


            dt.Rows.Add(dr);
        }
        private void NewEditLogRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr.EndEdit();
        }
        private void NewEditRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["DateUpdated"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        #endregion


        #region SalaryProcess

        public DataTable GetDataTable(string fromDate, string toDate, string empId)
        {
            try
            {

                string year = DateTime.Now.Year.ToString();

                string sql = @"SELECT E.SystemId EmpSystemId,SS.Id,OL.Id EmployeeSalaryRuleItemId,OL.UserName,OL.Formula,OL.FormulaId,'' SystemID
,SH.HeadCategory,SS.SalaryCalculationDays,OL.SalaryHeadID,SH.HeadType,SIDM.SystemID SalaryID,SIDM.GroupID,SIDM.PlantID,SS.PFLimit,SS.ESICLimit,SS.AgeLimit
,Value= ISNULL(CASE WHEN OL.UserName='WeekOff' THEN APD.WeekOffValue
			 WHEN OL.UserName='Leave' THEN APD.LvValue
			 WHEN OL.UserName='HoliDay' THEN APD.HoliDayValue
			 WHEN OL.UserName='PayDay' THEN APD.PayDayValue
			 WHEN OL.UserName='NetDay' THEN APD.PayDayValue
			 WHEN OL.UserName='NightShiftDays' THEN NAPD.NightShiftDays
			 WHEN OL.UserName='ShortDuration' THEN APD.CountedShortLeave
			 WHEN OL.UserName='LateIN' THEN APD.LateIN
			 WHEN OL.UserName='EarlyOut' THEN APD.EarlyOut
			 WHEN OL.UserName='HalfDuration' THEN APD.HalfDuration
			 WHEN OL.SalaryHeadID<>'' THEN CAST(cast(SD.DefineAmount AS decimal(18,0)) AS varchar(100))
             ELSE CAST(0 AS varchar(100)) END,0)

,DATEDIFF(YEAR, E.DOB, GETDATE()) - CASE  WHEN DATEADD(YEAR, DATEDIFF(YEAR, E.DOB, GETDATE()), E.DOB) > GETDATE()  THEN 1  ELSE 0  END AS Age
,OL.EntryState,CO.BaseCurrencyId ,E.GivenDesignationId DesignationId,E.LegalDesignationId
,E.PaymentMode,E.BudgetCode,EB.BankAccNo,EB.BankBranchId,EB.BankSystemId,EB.MICRCode,EB.IFSCCode,EB.SalaryPercentage,DM.EmployeeCategoryId, LSGD.LegalSalaryGradeId
FROM EmployeeSalaryRuleItem AS OL
LEFT JOIN HKP.EmployeeSalaryRuleSetup SS ON SS.Id=OL.EmployeeSalaryRuleSetupId
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId IN(" + empId + @")
LEFT JOIN  dbo.EmployeeBankInfo EB ON EB.EmpSystemID=E.SystemId AND RowID in ( select top(1) RowID from dbo.EmployeeBankInfo where EB.EmpSystemID=EmpSystemID AND IsApproved=1 order by DateAdded desc)
LEFT JOIN MST.DesignationMaster DM ON DM.Id=E.GivenDesignationId
LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId=E.LegalDesignationId
LEFT JOIN (Select sum(PresentValue)PresentValue,SUM(LateValue)LateValue,SUM(LvValue)LvValue,SUM(WeekOffValue)WeekOffValue,SUM(PayDayValue)PayDayValue,SUM(HoliDayValue)HoliDayValue,ISNULL(SUM(CountedShortLeave),0)CountedShortLeave
,Count(APD.LateIn)LateIn,Count(APD.EarlyOut)EarlyOut, HalfDuration= CASE WHEN LeaveDuration=0.5 THEN CounT(LeaveDuration) ELSE 0 END,APD.EmpSystemID from dbo.AttdnProcessData APD where APD.EmpSystemID IN(" + empId + @") AND WorkDate between '" + fromDate + "' AND '" + toDate + @"'  Group by APD.EmpSystemID,LeaveDuration)APD ON APD.EmpSystemID=E.SystemId
LEFT JOIN(Select COUNT(DayStatus)NightShiftDays,APD.EmpSystemID from dbo.AttdnProcessData APD 
LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID=APD.ShiftSystemID
where APD.EmpSystemID IN(" + empId + @") AND SD.ShiftType='Night Shift' AND DayStatus='P' AND APD.WorkDate between '" + fromDate + "' AND '" + toDate + @"'
Group By APD.EmpSystemID) NAPD ON NAPD.EmpSystemID=e.SystemId

 LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = E.SystemId  AND SIDM.EmployeeSalaryRuleSetupId=OL.EmployeeSalaryRuleSetupId
 LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SIDM.SystemID AND OL.SalaryHeadID = SD.SalaryHeadID 
 LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
 LEFT JOIN ORG.Plant PL ON PL.Id=SIDM.PlantID
 LEFT JOIN ORG.Company CO ON CO.Id=PL.CompanyId
Where SIDM.isapproved=1 and OL.EmployeeSalaryRuleSetupId IN
(Select EmployeeSalaryRuleSetupId from dbo.SalaryRuleDesignation Where DesignationId IN (select GivenDesignationId from [dbo].EmployeeInformation Where SystemId IN(" + empId + @")))
ORDER BY E.SystemId,OL.Sequence";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "EmployeeSalaryRuleItemId = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                            strTemp = dvLocal[0]["Value"].ToString().Trim();
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End 

        private void SendNotification(string Message)
        {
            try
            {
                var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, Message);

            }
            catch (Exception ex)
            {

            }

        }
        string GetBankInfo(DataSet ds)
        {
            string r = string.Empty;
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (r.Length == 0)
                    {
                        r = "Employee [" + ds.Tables[0].Rows[i]["EmployeeCode"].ToString() + "] " + ds.Tables[0].Rows[i]["Remark"].ToString() + Environment.NewLine;
                    }
                    else
                    {
                        r += ", Employee [" + ds.Tables[0].Rows[i]["EmployeeCode"].ToString() + "] " + ds.Tables[0].Rows[i]["Remark"].ToString() + Environment.NewLine;
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void ValidationBank(string emplist, string plantid)
        {
            clsEmployeeLoad objel = null;
            try
            {
                objel = new clsEmployeeLoad();
                DataSet dsBankInof;
                objel.GetBankInfo(plantid, emplist, out dsBankInof);

                string r = GetBankInfo(dsBankInof);
                if (r.Length > 0)
                {
                    throw new Exception(r);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetAttendanceTobelocked(DataSet ds)
        {
            string r = string.Empty;
            try
            {
                DataTable dt = new DataView(ds.Tables[0]).ToTable(true, "EmployeeCode");

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (r.Length == 0)
                    {
                        r = "Attendance is not locked (individual) for the following Employees:-" + Environment.NewLine;
                        r += " Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }
                    else
                    {
                        r += ", Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }

                }

                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void ValidationAttendance(string emplist, string plantid, string fromdate, string todate)
        {
            clsEmployeeLoad objel = null;
            try
            {
                objel = new clsEmployeeLoad();
                DataSet dsAttInfo;
                objel.GetAttendanceLockInfo(plantid, fromdate, todate, emplist, out dsAttInfo);
                string r = GetAttendanceTobelocked(dsAttInfo);
                if (r.Length > 0)
                {
                    throw new Exception(r);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public async Task<JsonResult> ProcessAsync(Dictionary<string, object> data, List<Dictionary<string, object>> alldataset)
        {
            string empId = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.SalaryProcess, "", 60);
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    string tempEmpSysId = "''";
                    for (int i = alldataset.Count - 1; i >= 0; i--)
                    {
                        var item = alldataset.ElementAt(i);
                        tempEmpSysId += ",'" + item["EmpSystemID"] + "'";
                    }

                    DataSet dsMaster, dsProChild, dsSlaProLogDetail, dsSPAttdnProc, dsMMDSSI = null;

                    DataTable dtSPAttdnProc = null;
                    DataView dvSPAttdnProc = null;
                    DataRow drSPAttdnProc = null;
                    string esql = "";
                    var empIds = "' '";

                    SendNotification("Validating Bank Accounts");
                    ValidationBank(tempEmpSysId, identity.PlantId);

                    SendNotification("Validating Attendance Lock");
                    ValidationAttendance(tempEmpSysId, identity.PlantId, Convert.ToDateTime(data["FromDate"]).ToString("yyyyMMMdd"), Convert.ToDateTime(data["ToDate"]).ToString("yyyyMMMdd"));
                    _lock.LockProcess();
                    
                    clsFinalSettlement clsFS = new clsFinalSettlement();
                    MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    clsSalaryProc objSlrProc = new clsSalaryProc();

                    string tempDelEmpSysId = @"   EmpInfoSystemID IN (" + tempEmpSysId + ")";
                    string tempSlaProAttEmpSysId = @"   EmpSystemID IN (" + tempEmpSysId + ")";
                    objSlrProc.DeleteSlrProcChild(Convert.ToInt32(Convert.ToDateTime(data["ToDate"]).ToString("MM")), Convert.ToInt32(Convert.ToDateTime(data["ToDate"]).ToString("yyyy")), tempDelEmpSysId);
                    objSlrProc.GetSalaryProceAttdnData(Convert.ToInt32(Convert.ToDateTime(data["ToDate"]).ToString("MM")), Convert.ToInt32(Convert.ToDateTime(data["ToDate"]).ToString("yyyy")), tempSlaProAttEmpSysId, out dsSPAttdnProc);
                    dtSPAttdnProc = dsSPAttdnProc.Tables[0];
                    #region Attendance

                    objSlrProc.GetAttdnDataForMonthlyProc(tempSlaProAttEmpSysId, Convert.ToDateTime(data["FromDate"]).ToString("yyyyMMMdd"), Convert.ToDateTime(data["ToDate"]).ToString("yyyyMMMdd"), out dsMMDSSI);

                    #endregion
                    con.OpenDataSetThroughAdapter("select * from SalaryProcMaster where SystemID='" + data["SystemID"] + "'", out dsMaster, false, "1");
                    DataTable dtData = GetDataTable(Convert.ToDateTime(data["FromDate"]).ToString("yyyyMMMdd"), Convert.ToDateTime(data["ToDate"]).ToString("yyyyMMMdd"), tempEmpSysId);

                    string _Id = "";
                    string idFromDB = "";
                    string _childPK_seed_fromDB = "";
                    bplib.clsGenID genid = new bplib.clsGenID();
                    #region data master
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        string strCurCode = "";

                        genid.GenID(DateTime.Now.ToShortDateString().ToString(), "SlrProc", out _Id);
                        strCurCode = "M" + "-" + strCurCode;
                        strCurCode = Convert.ToDateTime(data["FromDate"]).ToString("yyyyMMMdd") + "SP" + Convert.ToDateTime(data["ToDate"]).ToString("MMMdd");
                        data["SystemID"] = _Id;
                        data["SalaryProcID"] = strCurCode;
                        data["SalaryProcDate"] = System.DateTime.Now.ToString();
                        data["YearNo"] = Convert.ToDateTime(data["ToDate"]).ToString("yyyy");
                        data["MonthNo"] = Convert.ToDateTime(data["ToDate"]).ToString("MM");
                        data["AmtDefinitionCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];
                        data["LocalCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];
                        data["AmtDefinitionCurrencyRate"] = 1;
                        data["IsCompleteMonth"] = 1;
                        data["SalaryView"] = 0;
                        data["SalaryProcID"] = strCurCode;
                        NewAddedRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["SystemID"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update
                    #region data Detail
                    con.OpenDataSetThroughAdapter("select * from [dbo].[SalaryProcChild] where SlrProcMstSystemID='" + data["SystemID"] + "'", out dsProChild, false, "1");
                    con.OpenDataSetThroughAdapter("select * from [dbo].[SalaryProcessLogDetail] where SalaryProcessId  in (select SystemID from SalaryProcMaster m where m.MonthNo= " + Convert.ToDateTime(data["ToDate"]).ToString("MM") + " AND M.YearNo=" + Convert.ToDateTime(data["ToDate"]).ToString("yyyy") + ") and EmpSystemId in (" + tempEmpSysId + ")", out dsSlaProLogDetail, false, "1");


                    genid.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SAL_PROC_CHILD_PK", out _childPK_seed_fromDB);
                    int _child_emp_seed = 0;
                    double tempBasicValue = 0;
                    double tempGrossValue = 0;

                    decimal tempWeekOff = 0; decimal tempLeave = 0; decimal tempHoliDay = 0; decimal tempPayDay = 0; decimal tempNetDay = 0; decimal tempNightShiftDays = 0; decimal tempShortDuration = 0; decimal tempHalfDuration = 0
                               ; decimal tempTotalWorkingDays = 0; decimal tempSalaryCalculationDays = 0; decimal tempPFLimit = 0; decimal tempESICLimit; decimal tempAgeLimit = 0; decimal OTHDay = 0; decimal NorOTHDay = 0; decimal ExtOTHDay = 0;

                    foreach (var item in alldataset)
                    {
                        empId = item["EmpSystemID"].ToString();
                        SendNotification(item["EmployeeCode"].ToString()+"-"+item["EmployeeName"].ToString()+ " "+ "Process is going on ...");

                      
                        string sql = string.Empty;
                        DataTable dtValue = new DataTable();
                        DataRow dtValueRow = dtValue.NewRow();
                        dtValue.TableName = "TempTable";
                        dtValue.Columns.Add("EmployeeSalaryRuleItemId");
                        dtValue.Columns.Add("Value");
                        dtValue.Columns.Add("EntryAmount");
                        dtValue.Columns.Add("SalaryHeadID");
                        dtValue.Columns.Add("SystemID");
                        dtValue.Columns.Add("HeadType");
                        dtValue.Columns.Add("PFLimit");
                        dtValue.Columns.Add("ESICLimit");
                        dtValue.Columns.Add("AgeLimit");
                        dtValue.Columns.Add("Age");
                        double sFormulaResult = 0.00;

                        DataView dvEmpWise = new DataView(dtData);
                        //DataView dvEmpSalary = new DataView(dtData);
                        DataView dvslaProLogDet = new DataView(dsSlaProLogDetail.Tables[0]);
                        dvEmpWise.RowFilter = "EmpSystemId = '" + empId + "'";
                        //dvEmpSalary.RowFilter = "EmpSystemId = '" + empId + "' AND  SalaryHeadID<>''";
                        if (dvEmpWise.Count > 0)
                        {

                            #region Salary Proc child
                            DataView dvchild = new DataView(dsProChild.Tables[0]);
                            dvchild.RowFilter = "SystemID='" + dvEmpWise[0]["SystemID"].ToString() + "'";
                            Dictionary<string, object> spc = new Dictionary<string, object>();
                            for (int i = 0; i < dvEmpWise.Count; i++)
                            {
                                if (dvEmpWise[i]["UserName"].ToString() == "WeekOff")
                                    tempWeekOff = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "Leave")
                                    tempLeave = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "HoliDay")
                                    tempHoliDay = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "PayDay")
                                    tempPayDay = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "NetDay")
                                    tempNetDay = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "NightShiftDays")
                                    tempNightShiftDays = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "NightShiftDays")
                                    tempShortDuration = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "HalfDuration")
                                    tempHalfDuration = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());
                                else if (dvEmpWise[i]["UserName"].ToString() == "TotalWorkingDays")
                                    tempTotalWorkingDays = Convert.ToDecimal(dvEmpWise[i]["Value"].ToString());

                                if (dvEmpWise[i]["UserName"].ToString() == "Basic")
                                {
                                    tempSalaryCalculationDays = Convert.ToDecimal(dvEmpWise[i]["SalaryCalculationDays"].ToString());
                                    tempPFLimit = Convert.ToDecimal(dvEmpWise[i]["PFLimit"].ToString());
                                    tempESICLimit = Convert.ToDecimal(dvEmpWise[i]["ESICLimit"].ToString());
                                    tempAgeLimit = Convert.ToDecimal(dvEmpWise[i]["AgeLimit"].ToString());
                                }


                                if (string.IsNullOrEmpty(dvEmpWise[i]["FormulaId"].ToString()) && !string.IsNullOrEmpty(dvEmpWise[i]["SalaryHeadID"].ToString()))
                                {
                                    dtValueRow = dtValue.NewRow();
                                    dtValueRow["EmployeeSalaryRuleItemId"] = dvEmpWise[i]["EmployeeSalaryRuleItemId"].ToString().Trim();
                                    dtValueRow["SystemID"] = dvEmpWise[i]["SystemID"].ToString().Trim();
                                    dtValueRow["SalaryHeadID"] = dvEmpWise[i]["SalaryHeadID"].ToString().Trim();
                                    dtValueRow["EntryAmount"] = dvEmpWise[i]["Value"].ToString().Trim();
                                    dtValueRow["HeadType"] = dvEmpWise[i]["HeadType"].ToString().Trim();
                                    dtValueRow["Value"] = Math.Round((Convert.ToDecimal(dvEmpWise[i]["Value"].ToString()) / tempSalaryCalculationDays) * tempPayDay, 0);

                                    dtValue.Rows.Add(dtValueRow);

                                    DataView dvPC = new DataView(dsProChild.Tables[0]);
                                    dvPC.RowFilter = "SystemID='" + dtValue.Rows[0]["SystemID"] + "'";

                                    if (dvPC.Count == 0)
                                    {
                                        _child_emp_seed++;
                                        spc["SystemID"] = _childPK_seed_fromDB + _child_emp_seed;
                                        spc["SlrProcMstSystemID"] = _Id;
                                        spc["DefineAmount"] = dtValueRow["Value"];
                                        if (!string.IsNullOrEmpty(dtValueRow["HeadType"].ToString()))
                                            spc["DisbusmentAmount"] = dtValueRow["HeadType"].ToString() == "D" ? Convert.ToDecimal(dtValueRow["Value"]) * (-1) : dtValueRow["Value"];
                                        else
                                            spc["DisbusmentAmount"] = 0;
                                        spc["EmpInfoSystemID"] = empId;
                                        spc["PlantID"] = dtData.Rows[0]["PlantID"];
                                        spc["GroupID"] = dtData.Rows[0]["GroupID"];
                                        spc["EntryCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];
                                        spc["DefineCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];
                                        spc["DisbusmentCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];
                                        spc["AcltExcDisbSlrHDID"] = dtValueRow["SalaryHeadID"];
                                        spc["SalaryHeadID"] = dtValueRow["SalaryHeadID"];
                                        spc["SalaryID"] = dtData.Rows[0]["SalaryID"];
                                        spc["EntryAmount"] = dtValueRow["EntryAmount"];

                                        NewAddedRow(dsProChild.Tables[0], spc);
                                    }
                                }

                                else if (!string.IsNullOrEmpty(dvEmpWise[i]["FormulaId"].ToString()) && !string.IsNullOrEmpty(dvEmpWise[i]["SalaryHeadID"].ToString()))
                                {
                                    dtValueRow = dtValue.NewRow();
                                    ReLoadFormulaWithValue(dvEmpWise[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                                    //sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("###0");
                                    sFormulaResult = clsSalaryStructureAplos.EvaluateUpto2Decimal(_formulaValue);

                                    dtValueRow["EmployeeSalaryRuleItemId"] = dvEmpWise[i]["EmployeeSalaryRuleItemId"].ToString().Trim();
                                    dtValueRow["SalaryHeadID"] = dvEmpWise[i]["SalaryHeadID"].ToString().Trim();
                                    dtValueRow["HeadType"] = dvEmpWise[i]["HeadType"].ToString().Trim();


                                    if (dvEmpWise[i]["HeadCategory"].ToString().Trim() == "Basic")
                                    {
                                        tempBasicValue = Math.Round(sFormulaResult, 0);
                                    }
                                    else if (dvEmpWise[i]["HeadCategory"].ToString().Trim() == "GROSS")
                                    {
                                        tempGrossValue = Math.Round(sFormulaResult, 0);
                                    }

                                    if (dvEmpWise[i]["HeadCategory"].ToString().Trim() == "ESIC Employee Contribution" || dvEmpWise[i]["HeadCategory"].ToString().Trim() == "ESIC Employer Contribution")
                                    {
                                        if (tempGrossValue <= Convert.ToDouble(dvEmpWise[i]["ESICLimit"].ToString().Trim()))
                                        {
                                            dtValueRow["EntryAmount"] = dvEmpWise[i]["Value"].ToString().Trim();
                                            dtValueRow["Value"] = Math.Round(sFormulaResult, 0);
                                        }
                                    }
                                    else if (dvEmpWise[i]["HeadCategory"].ToString().Trim() == "PF Employer Contribution" || dvEmpWise[i]["HeadCategory"].ToString().Trim() == "PF Employee Contribution")
                                    {
                                        if (tempBasicValue <= Convert.ToDouble(dvEmpWise[i]["PFLimit"].ToString().Trim()) && Convert.ToDouble(dvEmpWise[i]["AgeLimit"].ToString().Trim()) >= Convert.ToDouble(dvEmpWise[i]["Age"].ToString().Trim())) //TDDO: employee age
                                        {
                                            dtValueRow["EntryAmount"] = dvEmpWise[i]["Value"].ToString().Trim();
                                            dtValueRow["Value"] = Math.Round(sFormulaResult, 0);
                                        }
                                        else
                                        {
                                            dtValueRow["EntryAmount"] = 0;
                                            dtValueRow["Value"] = 0;
                                        }
                                    }
                                    else
                                    {
                                        dtValueRow["EntryAmount"] = dvEmpWise[i]["Value"].ToString().Trim();
                                        dtValueRow["Value"] = Math.Round(sFormulaResult, 0);
                                    }


                                    dtValue.Rows.Add(dtValueRow);

                                    DataView dvPC = new DataView(dsProChild.Tables[0]);
                                    dvPC.RowFilter = "SystemID='" + dtValue.Rows[0]["SystemID"] + "'";

                                    if (dvPC.Count == 0)
                                    {
                                        _child_emp_seed++;
                                        spc["SystemID"] = _childPK_seed_fromDB + _child_emp_seed;
                                        spc["SlrProcMstSystemID"] = _Id;
                                        spc["DefineAmount"] = dtValueRow["Value"];
                                        spc["AcltExcDisbSlrHDID"] = dtValueRow["SalaryHeadID"];


                                        if (!string.IsNullOrEmpty(dtValueRow["HeadType"].ToString()))
                                            spc["DisbusmentAmount"] = dtValueRow["HeadType"].ToString() == "D" ? Convert.ToDecimal(dtValueRow["Value"]) * (-1) : Convert.ToDecimal(dtValueRow["Value"]);
                                        else
                                            spc["DisbusmentAmount"] = 0;

                                        spc["EmpInfoSystemID"] = empId;
                                        spc["PlantID"] = dtData.Rows[0]["PlantID"];
                                        spc["GroupID"] = dtData.Rows[0]["GroupID"];
                                        spc["EntryCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];
                                        spc["DefineCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];
                                        spc["DisbusmentCurrencyID"] = dtData.Rows[0]["BaseCurrencyId"];

                                        spc["SalaryHeadID"] = dtValueRow["SalaryHeadID"];
                                        spc["SalaryID"] = dtData.Rows[0]["SalaryID"];
                                        spc["EntryAmount"] = dtValueRow["EntryAmount"];

                                        NewAddedRow(dsProChild.Tables[0], spc);
                                    }
                                }
                            }

                            #endregion Salary Proc child

                            #region Log Data
                            Dictionary<string, object> logData = new Dictionary<string, object>();

                            logData["PlantId"] = dvEmpWise[0]["PlantID"];
                            logData["CompanyGroupId"] = dvEmpWise[0]["GroupID"];
                            logData["SalaryProcessId"] = _Id;
                            logData["EmpSystemId"] = empId;
                            logData["Flag"] = item["Flag"].ToString();
                            logData["DesignationId"] = dvEmpWise[0]["DesignationId"];
                            logData["LegalDesignationId"] = dvEmpWise[0]["LegalDesignationId"];
                            logData["BankSystemId"] = dvEmpWise[0]["BankSystemId"];
                            logData["PaymentMode"] = dvEmpWise[0]["PaymentMode"];
                            logData["BudgetCode"] = dvEmpWise[0]["BudgetCode"];
                            logData["BankAccNo"] = dvEmpWise[0]["BankAccNo"];
                            logData["BankBranchId"] = dvEmpWise[0]["BankBranchId"];
                            logData["MICRCode"] = dvEmpWise[0]["MICRCode"];
                            logData["IFSCCode"] = dvEmpWise[0]["IFSCCode"];
                            logData["SalaryPercentage"] = dvEmpWise[0]["SalaryPercentage"];
                            logData["EmployeeCategoryId"] = dvEmpWise[0]["EmployeeCategoryId"];
                            logData["LegalSalaryGradeId"] = dvEmpWise[0]["LegalSalaryGradeId"];

                            if (logData != null)
                            {
                                DataView dvSLD = new DataView(dsSlaProLogDetail.Tables[0]);
                                dvSLD.RowFilter = "EmpSystemId='" + empId + "'";

                                if (dvSLD.Count == 0)
                                {
                                    genid.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARY_LOG_DETAIL", out idFromDB);
                                    logData["Id"] = idFromDB;
                                    NewAddedLogRow(dsSlaProLogDetail.Tables[0], logData);
                                }
                                else
                                {
                                    DataRow drmo = dvSLD[0].Row;
                                    NewEditLogRow(drmo, logData);
                                }

                                logData = null;
                            }

                            #endregion


                            #region Salary Proc Attendence Summary
                            List<dicMMDSSI_New> dicMMDSSI = new List<dicMMDSSI_New>();
                            if (dsMMDSSI.Tables[0].Rows.Count > 0)
                                dicMMDSSI = dsMMDSSI.Tables[0].ToList<dicMMDSSI_New>();
                            var dicMMDSSI_Sub = dicMMDSSI.Find(x => x.EmpSystemID == empId);


                            dvSPAttdnProc = new DataView();
                            dvSPAttdnProc.Table = dtSPAttdnProc;
                            dvSPAttdnProc.RowFilter = "EmpSystemID = '" + empId + "'";

                            FunctionPara para = new FunctionPara();
                            para.FromDate = Convert.ToDateTime(data["FromDate"]).ToString("yyyyMMMdd");
                            para.ToDate = Convert.ToDateTime(data["ToDate"]).ToString("yyyyMMMdd");
                            para.GroupId = dtData.Rows[0]["GroupID"].ToString();
                            para.IsOTEntitled = false;
                            para.OTRate = 0;
                            para.USER = identity.Name;
                            para.lblSalaryProcSystemId = _Id;
                            if (dvSPAttdnProc.Count == 0)
                            {
                                drSPAttdnProc = dtSPAttdnProc.NewRow();

                                UpdateSlrProcAttdenDataRow("ADDNEW", para, empId, dvEmpWise[0]["PlantID"].ToString(), OTHDay, NorOTHDay, ExtOTHDay, dicMMDSSI_Sub, ref drSPAttdnProc);
                                dtSPAttdnProc.Rows.Add(drSPAttdnProc);
                            }
                            else
                            {
                                drSPAttdnProc = dvSPAttdnProc[0].Row;
                                drSPAttdnProc.BeginEdit();
                                UpdateSlrProcAttdenDataRow("EDIT", para, empId, dvEmpWise[0]["PlantID"].ToString(), OTHDay, NorOTHDay, ExtOTHDay, dicMMDSSI_Sub, ref drSPAttdnProc);
                                drSPAttdnProc.EndEdit();
                            }

                            #endregion Salary Proc Attendence

                        }


                    }
                    SendNotification("Status: Process Completed");
                    #endregion data update 
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsProChild, dsSlaProLogDetail, dsSPAttdnProc);

                    _lock.UnlockProcess();
                    return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

                }
                catch (Exception ex)
                {
                    _lock.UnlockProcess();
                    return Json(new { Error = true, Message = ex.Message + " " + empId });

                }
            });
        }

        private void UpdateSlrProcAttdenDataRow(string OPN_FLAG, FunctionPara fpara, string sEmpSysID, string sPlantID, decimal OTHDay, decimal NorOTHDay, decimal ExtOTHDay, dicMMDSSI_New dicMMDSSI_Sub, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(sEmpSysID.Trim());

                    drLocal["AddedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["SlrProcMstSystemID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcSystemId.Trim());

                drLocal["MonthNo"] = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Month;
                drLocal["YearNo"] = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Year;
                drLocal["GroupID"] = bplib.clsWebLib.RetValidLen(fpara.GroupId.ToString().Trim());
                drLocal["PlantID"] = bplib.clsWebLib.RetValidLen(sPlantID.Trim());

                drLocal["FromDate"] = fpara.FromDate.Trim();
                drLocal["ToDate"] = fpara.ToDate.Trim();

                drLocal["IsOTEntitled"] = fpara.IsOTEntitled;
                drLocal["OTRate"] = fpara.OTRate;

                drLocal["TotalProcDate"] = dicMMDSSI_Sub.TotalProcDate;
                drLocal["TotalPresent"] = dicMMDSSI_Sub.TotalPresent;

                drLocal["TotalLate"] = dicMMDSSI_Sub.TotalLate;
                drLocal["TotalAbsent"] = dicMMDSSI_Sub.TotalAbsent;
                drLocal["TotalLWP"] = dicMMDSSI_Sub.TotalLWP;
                drLocal["TotalLVWithPay"] = dicMMDSSI_Sub.TotalLVWithPay;

                drLocal["TotalLv"] = dicMMDSSI_Sub.TotalLv;
                drLocal["TotalMLv"] = dicMMDSSI_Sub.TotalMLv;

                drLocal["TotalCompAssignLv"] = dicMMDSSI_Sub.TotalCompAssignLv;
                drLocal["TotalWeekOff"] = dicMMDSSI_Sub.TotalWeekOff;

                drLocal["TotalHoliDay"] = dicMMDSSI_Sub.TotalHoliDay;
                drLocal["TotalWeekOffHoliDay"] = dicMMDSSI_Sub.TotalWeekOffHoliDay;

                drLocal["TotalPayDay"] = dicMMDSSI_Sub.TotalPayDay;
                drLocal["TotalNonPayDay"] = dicMMDSSI_Sub.TotalNonPayDay;
                drLocal["TotalWorkingDay"] = dicMMDSSI_Sub.TotalWorkingDay;
                drLocal["ActualWorkingDay"] = dicMMDSSI_Sub.TotalActualWorkingDay;

                drLocal["WeekoffDays"] = dicMMDSSI_Sub.WeekoffDays;

                drLocal["TotalOTHr"] = OTHDay;
                drLocal["TotalNormalOTHr"] = NorOTHDay;
                drLocal["TotalExtraOTHr"] = ExtOTHDay;

                drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function

        private class dicMMDSSI_New
        {
            public string EmpSystemID { get; set; } = "";
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int TotalProcDate { get; set; } = 0;
            public decimal TotalPresent { get; set; } = 0;
            public decimal TotalLate { get; set; } = 0;
            public decimal TotalAbsent { get; set; } = 0;
            public decimal TotalLv { get; set; } = 0;
            public decimal TotalLWP { get; set; } = 0;
            public decimal TotalMLv { get; set; } = 0;
            public decimal TotalWeekOff { get; set; } = 0;
            public decimal TotalCompAssignLv { get; set; } = 0;
            public decimal TotalHoliDay { get; set; } = 0;
            public decimal TotalWeekOffHoliDay { get; set; } = 0;
            public decimal TotalOTHr { get; set; } = 0;
            public decimal TotalNormalOTHr { get; set; } = 0;
            public decimal TotalExtraOTHr { get; set; } = 0;
            public decimal TotalLVWithPay { get; set; } = 0;
            public decimal TotalPayDay { get; set; } = 0;
            public decimal TotalNonPayDay { get; set; } = 0;
            public decimal TotalWorkingDay { get; set; } = 0;
            public decimal TotalActualWorkingDay { get; set; } = 0;
            public decimal WeekoffDays { get; set; } = 0;

            //public string PlantID { get; set; } = "";
        }
        #endregion
    }
}