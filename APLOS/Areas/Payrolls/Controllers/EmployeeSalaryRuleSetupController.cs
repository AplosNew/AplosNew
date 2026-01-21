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
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
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

        public ActionResult SalaryProcess()
        {
            return View();
        }

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
                            drF["SandardName"] = item["SandardName"];
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

                    DataSet dsMaster, dsDestination, dsID = null;
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
	,E.PlantId,C.CompanyGroupId,SH.Sequence,E.GivenDesignationId,E.LegalDesignationId,SLID.EffectiveDate,SLID.NextDueDate
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
                DataSet dsMaster,dsChild, dsIH;
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
                obj.SaveDataSets(dsMaster,dsChild, dsIH);

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

    }
}