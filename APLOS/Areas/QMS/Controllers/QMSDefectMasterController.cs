using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Newtonsoft.Json;
using OTSBD;

namespace Aplos.Areas.QMS.Controllers
{
    public class QMSDefectMasterController : BaseController
    {
        string TableName = "MST.QMSDefectMaster";
        string TableName1 = "MST.QMSDefectType";
        string TableName2 = "MST.QMSDefectCheckLevel";
        string TableName3 = "MST.QMSDefectZone";
        string TableName4 = "MST.QMSOperationActivity";
        string TableName5 = "MST.QMSProductApplicable";
        string TableName6 = "MST.QMSMaterialApplicable";
        string TableName7 = "MST.QMSProcessApplicable";
        string TableName8 = "MST.QMSQualityActivity";
        string TableName9 = "MST.QMSMachineApplicable";
        string TableName10 = "MST.QMSSkillApplicable";
        string TableName11 = "MST.QMSTestApplicable";
        string TableName12 = "MST.QMSInspectionApplicable";
        string TableName14 = "MST.QMSProcessParameterApplicable";
      

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public QMSDefectMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetRepairTypeList()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName AS Text FROM [hkp].[RepairType]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCriticalityLevel()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value, UserName AS Text FROM HKP.Critical"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from MST.QMSDefectMaster where Id = '" + Id + "' ");


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


            string sql = @"select top 100 * from (select QDM.*,C.UserName as Critical,RT.Username as RepairType
                          from MST.QMSDefectMaster QDM left join  HKP.Critical C 
                          on QDM.CriticalityLevelId=C.Id left join HKP.RepairType RT 
                          on QDM.RepairTypeId=RT.Id) AS TEMP WHERE " + strkey + " order by Sequence ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, IEnumerable<QMSTestApplicable> TestApplicableList, IEnumerable<QMSProductApplicable> ProductApplicableList, IEnumerable<QMSMaterialApplicable> MaterialApplicableList, IEnumerable<QMSProcessApplicable> ProcessApplicableList, 
                                IEnumerable<QMSMachineApplicable> MachineApplicableList, IEnumerable<QMSSkillApplicable> SkillApplicableList, IEnumerable<QMSProcessParameterApplicable> ProcessParameterApplicableList, IEnumerable<QMSDefectType> DefectTypeList, IEnumerable<QMSDefectCheckLevel> DefectCheckList,
                               IEnumerable<QMSDefectZone> DefectZoneList, IEnumerable<QMSOperationActivity> OperationList, IEnumerable<QMSQualityActivity> QualityList, IEnumerable<QMSInspectionApplicable> InspectionList)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsMaster1;
                DataSet dsMaster2;
                DataSet dsMaster3;
                DataSet dsMaster4;
               

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (data["Id"] == null)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where ShortName= '" + data["ShortName"] + "' ", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName= '" + data["UserName"] + "' ", out dsMaster1, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardName= '" + data["StandardName"] + "' ", out dsMaster2, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardCode= '" + data["StandardCode"] + "' ", out dsMaster3, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserCode= '" + data["UserCode"] + "' ", out dsMaster4, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Short Name already exists!!!");
                    if (dsMaster1.Tables[0].Rows.Count > 0)
                        throw new Exception("User Name already exists!!!");
                    if (dsMaster2.Tables[0].Rows.Count > 0)
                        throw new Exception("Standard Name already exists!!!");
                    if (dsMaster3.Tables[0].Rows.Count > 0)
                        throw new Exception("Standard Code already exists!!!");
                    if (dsMaster4.Tables[0].Rows.Count > 0)
                        throw new Exception("User Code already exists!!!");
                }

                else
                {

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserCode='" + data["UserCode"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same User Code already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same User Name already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardName='" + data["StandardName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Standard Name already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where ShortName='" + data["ShortName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Short Name already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardCode='" + data["StandardCode"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Standard Code already exists!!!");

                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "QDM" + _Id;
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
                string masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                CreateSubLocation(TestApplicableList,masterId);
                CreateProductApplicable(ProductApplicableList, masterId);
                CreateMaterialApplicable(MaterialApplicableList, masterId);
                CreateProcessApplicable(ProcessApplicableList, masterId);
                CreateMachineApplicable(MachineApplicableList, masterId);
                CreateSkillApplicable(SkillApplicableList, masterId);
                CreateProcessParameterApplicable(ProcessParameterApplicableList, masterId);
                CreateDefectType(DefectTypeList, masterId);
                CreateDefectCheck(DefectCheckList, masterId);
                CreateDefectZone(DefectZoneList, masterId);
                CreateOperation(OperationList, masterId);
                CreateQuality(QualityList, masterId);
                CreateInspection(InspectionList, masterId);
           

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from [MST].[QMSDefectMaster] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Defect Type");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Defect Check Level");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName3 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Defect Zone");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName4 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Operation Activity");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName5 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Product Master");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName6 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Master");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName7 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Process");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName8 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete QMS Activity Master");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName9 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Machine Master");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName10 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Skill");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName11 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete QMS Testing Master");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName12 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Inspection Master");
                    }
             
                    con.OpenDataSetThroughAdapter("select * from " + TableName14 + " where QMSDefectMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Process Parameter");
                    }
               
                }


                // ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
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


        //      *****************TEST APPLICABLE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetTestApplicableData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.QMSTestingMaster order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsTestApplicablData(string QMSDefectMasterId)
        {
            string sql = @"select qmsta.*,qmstm.Sequence, qmstm.Code, qmstm.ShortName, qmstm.StandardName, qmstm.UserName from MST.QMSTestApplicable qmsta
                           left join HKP.QMSTestingMaster qmstm on qmsta.TestApplicableId=qmstm.Id
                           WHERE qmsta.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteTestApp(string id)
        {
            DeleteTestAppData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteTestAppData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSTestApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetTAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSTestApplicable), out sID);
            return sID;
        }

        private void CreateSubLocation(IEnumerable<QMSTestApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSTestApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetTAPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["TestApplicableId"] = item.TestApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["TestApplicableId"] = item.TestApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************TEST APPLICABLE TAB END *******************

        //      *****************PRODUCT APPLICABLE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetProductApplicableData()
        {
            try
            {
                var sql = @"select 0 Active, * from MST.ProductMaster order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsProductApplicablData(string QMSDefectMasterId)
        {
            string sql = @"select qmspa.*,pm.Sequence, pm.Code, pm.ShortName, pm.StandardName, pm.UserName from MST.QMSProductApplicable qmspa
                           left join MST.ProductMaster pm on qmspa.ProductApplicableId=pm.Id
                           WHERE qmspa.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteProductApp(string id)
        {
            DeleteProductAppData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteProductAppData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSProductApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetPAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSProductApplicable), out sID);
            return sID;
        }

        private void CreateProductApplicable(IEnumerable<QMSProductApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSProductApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetPAPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["ProductApplicableId"] = item.ProductApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["ProductApplicableId"] = item.ProductApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************PRODUCT APPLICABLE TAB END *******************


        //      *****************MATERIAL APPLICABLE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetMaterialApplicableData()
        {
            try
            {
                var sql = @"select 0 Active, * from MST.MaterialMaster order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsMaterialApplicableData(string QMSDefectMasterId)
        {
            string sql = @"select qmsma.*,mm.Sequence, mm.Code, mm.ShortName, mm.StandardName, mm.UserName from MST.QMSMaterialApplicable qmsma
                           left join MST.MaterialMaster mm on qmsma.MaterialApplicableId=mm.Id
                           WHERE qmsma.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteMaterialApp(string id)
        {
            DeleteMaterialAppData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteMaterialAppData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSMaterialApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetMAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSMaterialApplicable), out sID);
            return sID;
        }

        private void CreateMaterialApplicable(IEnumerable<QMSMaterialApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSMaterialApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetMAPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["MaterialApplicableId"] = item.MaterialApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["MaterialApplicableId"] = item.MaterialApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************MATERIAL APPLICABLE TAB END *******************

        //      *****************PROCESS APPLICABLE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetProcessApplicableData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.Process order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsProcessApplicableData(string QMSDefectMasterId)
        {
            string sql = @"select pa.*,p.Sequence, p.Code, p.ShortName, p.StandardName, p.UserName from MST.QMSProcessApplicable pa
                           left join HKP.Process p on pa.ProcessApplicableId=p.Id
                           WHERE pa.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteProcessApp(string id)
        {
            DeleteProcessAppData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteProcessAppData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSProcessApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetProcessAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSProcessApplicable), out sID);
            return sID;
        }

        private void CreateProcessApplicable(IEnumerable<QMSProcessApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSProcessApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetProcessAPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["ProcessApplicableId"] = item.ProcessApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["ProcessApplicableId"] = item.ProcessApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************PROCESS APPLICABLE TAB END *******************

        //      *****************MACHINE APPLICABLE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetMachineApplicableData()
        {
            try
            {
                var sql = @"select 0 Active,mm.Id, mm.Sequence,mm.Code,mm.ShortName,mm.StandardName,mm.UserName,mc.UserName as MachineCategory,msc.UserName as MachineSubCategory
                           from MST.MachineMaster mm left join HKP.MachineCategory mc on mm.MachineCategoryId=mc.Id
                         left join HKP.MachineSubCategory msc on mm.MachineSubCategoryId=msc.Id order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsMachineApplicableData(string QMSDefectMasterId)
        {
            string sql = @"select machapp.*, mm.Sequence,mm.Code,mm.ShortName,mm.StandardName,mm.UserName,mc.UserName as MachineCategory,msc.UserName as MachineSubCategory
                           from MST.QMSMachineApplicable machapp left join MST.MachineMaster mm on machapp.MachineApplicableId=mm.Id
                           left join HKP.MachineCategory mc on mm.MachineCategoryId=mc.Id
                           left join HKP.MachineSubCategory msc on mm.MachineSubCategoryId=msc.Id
                           WHERE machapp.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteMachineApp(string id)
        {
            DeleteMachineAppData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteMachineAppData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSMachineApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetMachineAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSMachineApplicable), out sID);
            return sID;
        }

        private void CreateMachineApplicable(IEnumerable<QMSMachineApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSMachineApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetMachineAPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["MachineApplicableId"] = item.MachineApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["MachineApplicableId"] = item.MachineApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************MACHINE APPLICABLE TAB END *******************

        //      *****************SKILL APPLICABLE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetSkillApplicableData()
        {
            try
            {
                var sql = @"select 0 Active, sk.Id, sk.Sequence,sk.Code,sk.ShortName,sk.StandardName,sk.UserName,skc.UserName as SkillCategory
                           from HKP.Skill sk left join HKP.SkillCategory skc on sk.SkillCategoryId=skc.Id order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsSkillApplicableData(string QMSDefectMasterId)
        {
            string sql = @"select  qmsska.*, sk.Sequence,sk.Code,sk.ShortName,sk.StandardName,sk.UserName,skc.UserName as SkillCategory
                           from MST.QMSSkillApplicable qmsska left join HKP.Skill sk on qmsska.SkillApplicableId=sk.Id
                           left join HKP.SkillCategory skc on sk.SkillCategoryId=skc.Id
                           WHERE qmsska.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteSkillApp(string id)
        {
            DeleteSkillAppData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSkillAppData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSSkillApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetSkillAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSSkillApplicable), out sID);
            return sID;
        }

        private void CreateSkillApplicable(IEnumerable<QMSSkillApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSSkillApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetSkillAPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["SkillApplicableId"] = item.SkillApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["SkillApplicableId"] = item.SkillApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************SKILL APPLICABLE TAB END *******************

        //      *****************PROCESS PARAMETER APPLICABLE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetProcessParameterApplicableData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.ProcessParameter order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsProcessParameterApplicableData(string QMSDefectMasterId)
        {
            string sql = @"select  qmsppa.*, pp.Sequence,pp.Code,pp.ShortName,pp.StandardName,pp.UserName
                           from MST.QMSProcessParameterApplicable qmsppa left join HKP.ProcessParameter pp on qmsppa.ProcessParameterApplicableId=pp.Id
                           WHERE qmsppa.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteProcessParameterApp(string id)
        {
            DeleteProcessParameterAppData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteProcessParameterAppData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSProcessParameterApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetProcessParameterAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSProcessParameterApplicable), out sID);
            return sID;
        }

        private void CreateProcessParameterApplicable(IEnumerable<QMSProcessParameterApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSProcessParameterApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetProcessParameterAPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["ProcessParameterApplicableId"] = item.ProcessParameterApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["ProcessParameterApplicableId"] = item.ProcessParameterApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************PROCESS PARAMETER APPLICABLE TAB END *******************

        //      *****************DEFECT TYPE TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetDefectTypeData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.DefectType order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsDefectTypeData(string QMSDefectMasterId)
        {
            string sql = @"select  qmsdt.*, dft.Sequence,dft.Code,dft.ShortName,dft.StandardName,dft.UserName
                           from MST.QMSDefectType qmsdt left join HKP.DefectType dft on qmsdt.DefectTypeId=dft.Id
                           WHERE qmsdt.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteDefectType(string id)
        {
            DeleteDefectTypeData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteDefectTypeData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSDefectType] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetDefectTypePK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSDefectType), out sID);
            return sID;
        }

        private void CreateDefectType(IEnumerable<QMSDefectType> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSDefectType] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetDefectTypePK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["DefectTypeId"] = item.DefectTypeId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["DefectTypeId"] = item.DefectTypeId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************DEFECT TYPE TAB END *******************

        //      *****************DEFECT Check Level TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetDefectCheckData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.DefectCheckLevel order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsDefectCheckData(string QMSDefectMasterId)
        {
            string sql = @"select qmsdct.*, dfct.Sequence,dfct.Code,dfct.ShortName,dfct.StandardName,dfct.UserName
                           from MST.QMSDefectCheckLevel qmsdct left join HKP.DefectCheckLevel dfct on qmsdct.DefectCheckLevelId=dfct.Id
                           WHERE qmsdct.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteDefectCheck(string id)
        {
            DeleteDefectCheckData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteDefectCheckData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSDefectCheckLevel] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetDefectCheckPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSDefectCheckLevel), out sID);
            return sID;
        }

        private void CreateDefectCheck(IEnumerable<QMSDefectCheckLevel> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSDefectCheckLevel] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetDefectCheckPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["DefectCheckLevelId"] = item.DefectCheckLevelId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["DefectCheckLevelId"] = item.DefectCheckLevelId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************DEFECT Check Level TAB END *******************

        //      *****************DEFECT Zone TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetDefectZoneData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.DefectZone order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsDefectZoneData(string QMSDefectMasterId)
        {
            string sql = @"select qmsdz.*, dfz.Sequence,dfz.Code,dfz.ShortName,dfz.StandardName,dfz.UserName
                           from MST.QMSDefectZone qmsdz left join HKP.DefectZone dfz on qmsdz.DefectZoneId=dfz.Id
                           WHERE qmsdz.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteDefectZone(string id)
        {
            DeleteDefectZoneData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteDefectZoneData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSDefectZone] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetDefectZonePK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSDefectZone), out sID);
            return sID;
        }

        private void CreateDefectZone(IEnumerable<QMSDefectZone> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSDefectZone] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetDefectZonePK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["DefectZoneId"] = item.DefectZoneId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["DefectZoneId"] = item.DefectZoneId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************DEFECT Zone TAB END *******************

        //      *****************OPERATION TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetOperationData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.OperationActivity order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsOperationData(string QMSDefectMasterId)
        {
            string sql = @"select qmsopa.*, opa.Sequence,opa.Code,opa.ShortName,opa.StandardName,opa.UserName
                           from MST.QMSOperationActivity qmsopa left join HKP.OperationActivity opa on qmsopa.OperationActivityId=opa.Id
                           WHERE qmsopa.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteOperation(string id)
        {
            DeleteOperationData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteOperationData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSOperationActivity] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetOperationPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSOperationActivity), out sID);
            return sID;
        }

        private void CreateOperation(IEnumerable<QMSOperationActivity> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSOperationActivity] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetOperationPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["OperationActivityId"] = item.OperationActivityId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["OperationActivityId"] = item.OperationActivityId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************OPERATION TAB END *******************

        //      *****************QUALITY TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetQualityData()
        {
            try
            {
                var sql = @"select 0 Active, qmsam.Id, qmsam.Sequence,qmsam.Code,qmsam.ShortName,qmsam.StandardName,qmsam.UserName,qmsac.UserName as QMSActivityCategory, qact.UserName as QualityActivityCheckType
                          from HKP.QMSActivityMaster qmsam left join HKP.QMSActivityCategory qmsac on qmsam.QMSActivityCategoryId=qmsac.Id
			              left join HKP.QualityActivityCheckType qact on qmsam.QualityActivityCheckTypeId=qact.Id order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsQualityData(string QMSDefectMasterId)
        {
            string sql = @"select qmsqa.*, qmsam.Sequence,qmsam.Code,qmsam.ShortName,qmsam.StandardName,qmsam.UserName,qmsac.UserName as QMSActivityCategory, qact.UserName as QualityActivityCheckType
                          from MST.QMSQualityActivity qmsqa left join HKP.QMSActivityMaster qmsam on qmsqa.QualityActivityId=qmsam.Id
			              left join HKP.QMSActivityCategory qmsac on qmsam.QMSActivityCategoryId=qmsac.Id
			              left join HKP.QualityActivityCheckType qact on qmsam.QualityActivityCheckTypeId=qact.Id
                           WHERE qmsqa.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteQuality(string id)
        {
            DeleteQualityData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteQualityData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSQualityActivity] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetQualityPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSQualityActivity), out sID);
            return sID;
        }

        private void CreateQuality(IEnumerable<QMSQualityActivity> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSQualityActivity] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetQualityPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["QualityActivityId"] = item.QualityActivityId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["QualityActivityId"] = item.QualityActivityId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************QUALITY TAB END *******************

        //      *****************INSPECTION TAB*******************

        [HttpGet, Authorize]
        public ActionResult GetInspectionData()
        {
            try
            {
                var sql = @"select 0 Active, * from HKP.InspectionMaster order by Sequence";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQmsInspectionData(string QMSDefectMasterId)
        {
            string sql = @"select qmsipa.*, ipm.Sequence,ipm.Code,ipm.ShortName,ipm.StandardName,ipm.UserName
                           from MST.QMSInspectionApplicable qmsipa left join HKP.InspectionMaster ipm on qmsipa.InspectionApplicableId=ipm.Id
                           WHERE qmsipa.QMSDefectMasterId='" + QMSDefectMasterId + "' order by Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteInspection(string id)
        {
            DeleteInspectionData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteInspectionData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[QMSInspectionApplicable] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private string GetInspectionPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(QMSInspectionApplicable), out sID);
            return sID;
        }

        private void CreateInspection(IEnumerable<QMSInspectionApplicable> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [MST].[QMSInspectionApplicable] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetInspectionPK();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["InspectionApplicableId"] = item.InspectionApplicableId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["QMSDefectMasterId"] = masterId;
                            dr["InspectionApplicableId"] = item.InspectionApplicableId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        //      *****************INSPECTION TAB END *******************

       

    }
    public class QMSTestApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string TestApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSProductApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string ProductApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSMaterialApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string MaterialApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSProcessApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string ProcessApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSMachineApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string MachineApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSSkillApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string SkillApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSProcessParameterApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string ProcessParameterApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSDefectType : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string DefectTypeId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSDefectCheckLevel : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string DefectCheckLevelId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSDefectZone : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string DefectZoneId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }


    public class QMSOperationActivity : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string OperationActivityId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSQualityActivity : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string QualityActivityId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSInspectionApplicable : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string InspectionApplicableId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }


}