#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
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
    public class EmployeeSeperationSetupController : BaseController
    {
        //abcd
        //this is my code from tarek
        string TableName = "hkp.EmployeeSeperationSetup";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public EmployeeSeperationSetupController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT SalaryHeadID as Value,SalaryHead AS Text FROM dbo.SalaryHead Where IsApplicableInFinalSettlement=1"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.EmployeeSeperationSetup wher Id = '" + Id + "' ");


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
        public JsonResult CreateEmpSeperationEmployeeType(Dictionary<string, object> data, string masterId)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsEmpCat, dsDD;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                con.OpenDataSetThroughAdapter("select * from [dbo].[EmpSeperationEmployeeType] where EmployeeTypeId='" + data["EmployeeTypeId"] + "' AND EmployeeSeperationSetupId='" + masterId + "' AND  Id<>'" + data["Id"] + "'", out dsEmpCat, false, "1");
                if (dsEmpCat.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Employee Type already exists!!!");
                con.OpenDataSetThroughAdapter("select * from [dbo].[EmpSeperationEmployeeType] where Id='" + data["Id"] + "'", out dsEmpCat, false, "1");

                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmpSeperationEmployeeType] where EmployeeSeperationSetupId='" + masterId + "'", out dsDD, false, "1");
                int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());
                string Id = "";
                #region data update
                if (dsEmpCat.Tables[0].Rows.Count == 0)
                {
                    ccount++;
                    DataRow dr;
                    dr = dsEmpCat.Tables[0].NewRow();

                    dr["Id"] = materialCommonService.MakePK(masterId, ccount, 2);
                    dr["EmployeeSeperationSetupId"] = masterId;
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
        public ActionResult GetEmpSeperationEmployeeTypeData(string masterId)
        {
            try
            {
                var sql = @"select ec.Sequence,ec.Code,ec.ShortName,ec.StandardName,ec.UserName as EmployeeCategory,glmec.*
                            from [dbo].[EmpSeperationEmployeeType] glmec 
                            left join [HKP].[EmployeeCategory] ec on ec.Id=glmec.EmployeeTypeId
							where glmec.EmployeeSeperationSetupId = '" + masterId + "' ";

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
                strSQL = "DELETE FROM [dbo].[EmpSeperationEmployeeType] WHERE Id = '" + id + "'";

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
        public ActionResult GetDesignationGroupData()
        {
            try
            {
                var sql = @"select Flag=CAST(0 AS bit),* from HKP.DesignationGroup Where Active=1 Order By UserName";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpSepDesignationGroupData(string masterId)
        {
            try
            {
                var sql = @"select ec.Sequence,ec.Code,ec.ShortName,ec.StandardName,ec.UserName,glmec.*
                            from [dbo].EmpSeperationDesignationGroup glmec 
                            left join [HKP].[DesignationGroup] ec on ec.Id=glmec.DesignationGroupId
							where glmec.EmployeeSeperationSetupId = '" + masterId + "' Order By ec.UserName";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDesignationGroup(string id)
        {
            DeleteEmpSeperationDesignationGroupData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteEmpSeperationDesignationGroupData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[EmpSeperationDesignationGroup] WHERE Id = '" + id + "'";

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
        public JsonResult CreateDesignationGroup(List<Dictionary<string, object>> data, string masterId)
        {
            try
            {
                DataSet dsDesignation, dsDD;
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[EmpSeperationDesignationGroup] where EmployeeSeperationSetupId='" + masterId + "'", out dsDesignation, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmpSeperationDesignationGroup] where EmployeeSeperationSetupId='" + masterId + "'", out dsDD, false, "1");
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
                        item["EmployeeSeperationSetupId"] = masterId;

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

            string sql = @"SELECT N.* FROM [dbo].[EmployeeSeperationItem] N Where EmployeeSeperationSetupId='" + masterId + "' Order By N.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHeaderItemCbo(string id, string masterId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value, UserName AS Text FROM [dbo].[EmployeeSeperationItem] WHERE Id<>'" + id + "' AND EmployeeSeperationSetupId='" + masterId + "' Order By Sequence"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateSeperationItem(Dictionary<string, object> data, List<Dictionary<string, object>> details)
        {
            try
            {
                SaveSeperationItemData(data, details);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult CreateSeperationItemWithDefault(Dictionary<string, object> data, List<Dictionary<string, object>> details, List<Dictionary<string, object>> Itemdetails)
        {
            try
            {
                SaveSeperationItemWithDefaultData(data, details, Itemdetails);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveSeperationItemWithDefaultData(Dictionary<string, object> data, List<Dictionary<string, object>> details, List<Dictionary<string, object>> Itemdetails)
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

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.EmployeeSeperationItem WHERE EmployeeSeperationSetupId='" + data["EmployeeSeperationSetupId"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where EmployeeSeperationItemId='" + data["Id"] + "'", out dsDestination, false, "1");

                    con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmployeeSeperationItem] where EmployeeSeperationSetupId='" + data["EmployeeSeperationSetupId"] + "'", out dsID, false, "1");
                    int ccount = Convert.ToInt32(dsID.Tables[0].Rows[0]["countId"].ToString());

                    if (Itemdetails != null)
                    {
                        foreach (var item in Itemdetails)
                        {
                            ccount++;
                            //genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeSeperationItem", out _Id);

                            drF = dsMaster.Tables[0].NewRow();

                            drF["Id"] = materialCommonService.MakePK(data["EmployeeSeperationSetupId"].ToString(), ccount, 2);
                            drF["Sequence"] = item["Sequence"];
                            drF["EmployeeSeperationSetupId"] = item["EmployeeSeperationSetupId"];
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
                        data["Id"] = materialCommonService.MakePK(data["EmployeeSeperationSetupId"].ToString(), ccount, 2);
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
                                drF["EmployeeSeperationItemId"] = _Id;
                                drF["Sequence"] = item["Sequence"];
                                drF["EmployeeSeperationItemHeadId"] = item["EmployeeSeperationItemHeadId"];
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

        private void SaveSeperationItemData(Dictionary<string, object> data, List<Dictionary<string, object>> details)
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
                    con.OpenDataSetThroughAdapter("select * from EmployeeSeperationItem where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "' AND EmployeeSeperationSetupId='" + data["EmployeeSeperationSetupId"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("UserName already exists!!!");


                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.EmployeeSeperationItem WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where EmployeeSeperationItemId='" + data["Id"] + "'", out dsDestination, false, "1");

                    con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmployeeSeperationItem] where EmployeeSeperationSetupId='" + data["EmployeeSeperationSetupId"] + "'", out dsID, false, "1");
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

                        data["Id"] = materialCommonService.MakePK(data["EmployeeSeperationSetupId"].ToString(), ccount, 2);
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
                                drF["EmployeeSeperationItemId"] = _Id;
                                drF["Sequence"] = item["Sequence"];
                                drF["EmployeeSeperationItemHeadId"] = item["EmployeeSeperationItemHeadId"];
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

            string sql = @"SELECT D.Sequence,D.EmployeeSeperationItemHeadId
                            ,SalaryHead= CASE WHEN ISNULL(SD.UserName,'')<>'' THEN SD.UserName ELSE D.Component END,D.Component,D.EmployeeSeperationItemId
                            FROM [dbo].[FormulaDetail] D
                            LEFT JOIN dbo.EmployeeSeperationItem SD ON SD.Id=D.EmployeeSeperationItemHeadId
                            WHERE EmployeeSeperationItemId='" + ItemId + "' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetSeperationItemAutoSequence(string masterId)
        {
            return Json(GetSeperationItemSequence(masterId), JsonRequestBehavior.AllowGet);
        }
        private double GetSeperationItemSequence(string masterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  ISNULL(Max(Sequence),0) AS Sequence FROM dbo.EmployeeSeperationItem Where EmployeeSeperationSetupId='" + masterId + "'");
            double seq = 0;
            if (dt.Rows.Count > 0)
            {
                if (clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) == 0)
                {
                    seq = 8;
                }
                else
                {
                    seq = clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
                }
            }
            return seq;
        }

        [HttpPost, Authorize]
        public JsonResult DeleteEmployeeSeperationItem(string id)
        {
            DeleteEmployeeSeperationItemData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteEmployeeSeperationItemData(string SystemID)
        {
            string strSQL, strFSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.EmployeeSeperationItem WHERE Id = '" + SystemID + "'";
                strFSQL = "DELETE FROM dbo.FormulaDetail WHERE EmployeeSeperationItemId = '" + SystemID + "'";
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



    }
}