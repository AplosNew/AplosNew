#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Library.ViewModel.Materials;
using Syncfusion.DocIO.DLS;
using Library.Security.Core;
using System.Data;
using System;
using System.Web;
using Library.Model.External;
using System.IO;
using Library.Data;
using System.Configuration;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Helpers;

#endregion using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeUnderstandingHeadController : BaseController
    {
      

        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private SqlRepository _sqlRepository = new SqlRepository();
        //  private readonly IActivityService _activityService;
        public EmployeeUnderstandingHeadController()
        {

        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages


        #region Operation

        [HttpGet, Authorize]
        public ActionResult GetList(string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(employeeId) || employeeId == "undefined" || employeeId == "null")
            {
                employeeId = identity.EmployeeId;
            }
            string sql = @"SELECT euh.Id, euh.EmployeeId, euh.BudgetCode, euh.PositionCode,FORMAT(euh.[Date],'dd-MMM-yyyy') [Date],
                            euh.[Status], euh.Remarks,ei.EmployeeCode ,ei.EmployeeName
                            ,p.Code PCode,MB.Code MBCode
                            FROM EmpUnderstandingHead AS euh 
                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=euh.EmployeeId
                            LEFT JOIN ORG.Position AS p ON p.Id=euh.PositionCode
                            LEFT JOIN MST.ManpowerBudget AS MB ON MB.Id=euh.BudgetCode
                            WHERE euh.EmployeeId='" + employeeId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterDataFromEI(string EmployeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(EmployeeId)|| EmployeeId== "undefined" || EmployeeId == "null")
            {
                EmployeeId = identity.EmployeeId;
            }
            string sql = @"SELECT ''Id,ei.SystemId EmployeeId,ei.EmployeeCode, ei.EmployeeName,B.Code MBCode,P.Code PCode,P.Id PositionCode,ei.BudgetCode
                         FROM  EmployeeInformation AS ei 
                        LEFT OUTER JOIN mst.ManpowerBudget B ON B.Id=ei.BudgetCode
                        LEFT OUTER JOIN org.Position P ON P.Id=B.PositionId
                        WHERE ei.SystemId='" + EmployeeId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetActivityList(string EmpUnderstandingHeadId)
        {
           
            string sql = @"SELECT EUA.Id,eua.EmpUnderstandingHeadId, eua.ActivityName, eua.ActivityDetail,
       eua.PurposeOfTheActivity, eua.ActivityCategory, eua.OtherActivityCategory,
       eua.ActivityClass, eua.Priority, eua.Period, eua.Frequency, eua.AverageTime,
       eua.ActivityImportance, eua.ActivityType, eua.FinancialImpact, eua.Remarks,eua.ApplicableDocument,eua.ApplicableKPI
,CASE eua.ApplicableDocument WHEN 1 THEN 'Yes' ELSE 'No' END IsApplicableDocument
       ,CASE eua.ApplicableKPI WHEN 1 THEN 'Yes' ELSE 'No' END IsApplicableKPI
  FROM EmpUnderstandingActivity EUA WHERE EmpUnderstandingHeadId='" + EmpUnderstandingHeadId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDocumentList(string EmpUnderstandingActivityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM ActivityDocuments WHERE EmpUnderstandingActivityId='" + EmpUnderstandingActivityId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        
        


        [HttpPost, Authorize]
        public ActionResult GetDocumentCategoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM HKP.DocumentCategory";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetKPIList(string EmpUnderstandingActivityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM ActivityKPI WHERE EmpUnderstandingActivityId='" + EmpUnderstandingActivityId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

       
        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from EmpUnderstandingHead where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                     genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmpUnderstandingHead", out _Id);
                   // genid.GenID("EmpUnderstandingHead", out _Id);
                    _Id = "EUH" + _Id;
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


                return Json(new { Error = false,Id=_Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveActivity(Dictionary<string, object> data, string EmpUnderstandingHeadId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from EmpUnderstandingActivity where ActivityName='" + data["ActivityName"] + "' AND  Id<>'" + data["Id"] + "' AND EmpUnderstandingHeadId='" + data["EmpUnderstandingHeadId"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Activity already exists!!!");

                con.OpenDataSetThroughAdapter("select * from EmpUnderstandingActivity where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmpUnderstandingActivity", out _Id);
                    // genid.GenID("EmpUnderstandingActivity", out _Id);
                    _Id = "EUA" + _Id;
                    data["Id"] = _Id;
                    data["EmpUnderstandingHeadId"] = EmpUnderstandingHeadId;
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
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public JsonResult SaveKPI(Dictionary<string, object> data, string EmpUnderstandingActivityId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from ActivityKPI where KPIName='" + data["KPIName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("KPI already exists!!!");

                con.OpenDataSetThroughAdapter("select * from ActivityKPI where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ActivityKPI", out _Id);
                    // genid.GenID("ActivityDocuments", out _Id);
                    _Id = "EUK" + _Id;
                    data["Id"] = _Id;
                    data["EmpUnderstandingActivityId"] = EmpUnderstandingActivityId;
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
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateChild(Dictionary<string, object> data, string UtilityMasterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from UtilityDetail where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "UtilityDetail", out _Id);

                    data["Id"] = _Id;
                    data["UtilityMasterId"] = UtilityMasterId;
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

        public Dictionary<string, object> GetDocFile(string id)
        {
            try
            {
                var sql = @"Select Id,Attachment, FileName From [dbo].[ActivityDocuments]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void SaveDocuments(ActivityDocuments data, out string id)
        {
            id = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from ActivityDocuments where DocumentName='" + data.DocumentName + "'  AND  Id<>'" + data.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    //throw new Exception("Document already exists!!!");
                con.OpenDataSetThroughAdapter("select * from ActivityDocuments where Id='" + data.Id + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ActivityDocuments", out _Id);
                    _Id = "AD" + _Id;
                    data.Id = _Id;

                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = data.Id;

                    dr["EmpUnderstandingActivityId"] = data.EmpUnderstandingActivityId;
                    dr["EmployeeId"] = data.EmployeeId;
                    dr["DocumentPreprationFrequency"] = data.DocumentPreprationFrequency;
                    dr["DocumentType"] = data.DocumentType;
                    dr["DocumentType"] = data.DocumentType;
                    dr["DocumentFormat"] = data.DocumentFormat;
                    dr["DocumentClass"] = data.DocumentClass;
                    dr["DocumentCode"] = data.DocumentCode;
                    dr["DocumentName"] = data.DocumentName;
                    dr["DocumentCategoryId"] = data.DocumentCategoryId;
                    dr["Remarks"] = data.Remarks;
                    dr["Attachment"] = data.FileName;
                    dr["FileName"] = data.FileName;
                    dr["DocumentCategoryId"] = data.DocumentCategoryId;
                    dr["DocumentGeneration"] = data.DocumentGeneration;
                    dr["PreparedBy"] = data.PreparedBy;

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
                    dr["EmployeeId"] = data.EmployeeId;
                    dr["EmpUnderstandingActivityId"] = data.EmpUnderstandingActivityId;
                    dr["DocumentPreprationFrequency"] = data.DocumentPreprationFrequency;
                    dr["DocumentType"] = data.DocumentType;
                    dr["DocumentFormat"] = data.DocumentFormat;
                    dr["DocumentClass"] = data.DocumentClass;
                    dr["DocumentCode"] = data.DocumentCode;
                    dr["DocumentName"] = data.DocumentName;
                    dr["Remarks"] = data.Remarks;
                    dr["Attachment"] = data.FileName;
                    dr["FileName"] = data.FileName;
                    dr["DocumentCategoryId"] = data.DocumentCategoryId;
                    dr["DocumentGeneration"] = data.DocumentGeneration;
                    dr["PreparedBy"] = data.PreparedBy;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveDocument(FormCollection form, HttpPostedFileBase[] file)
        {
            ActivityDocuments documentActivity = new JavaScriptSerializer().Deserialize<ActivityDocuments>(form["documentActivityNew"]);
            var folderName = "";


            var directory = ResourcesPathReader.GetActivityDocumentsPath();


            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var path = Path.Combine(directory);

            var fileId = "";
            var fileName = "";
            var filedata = GetDocFile(documentActivity.Id);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["Attachment"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["Attachment"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != documentActivity.FileName)
                    if (System.IO.File.Exists(path + fileId + System.IO.Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(fileName));
            }
            //string _Id = "";
            //bplib.clsGenID genid = new bplib.clsGenID();
            //genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ActivityDocuments", out _Id);
            //_Id = "AD" + _Id;
            //var docPk = _Id;
            SaveDocuments(documentActivity, out string Id);
            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(item.FileName));
                        item.SaveAs(path + Id + System.IO.Path.GetExtension(item.FileName));
                    }
                }
            }

            if (!string.IsNullOrEmpty(documentActivity.FileName))
            {
                if (!System.IO.File.Exists(path + Id + System.IO.Path.GetExtension(documentActivity.FileName)))
                    throw new CustomException("File didn't saved.");
            }

            // activityService.InsertOrUpdateDocument(documentActivity, docPk);

            return Json(new { DocumentActivity = documentActivity, Message = "Data Saved Successfully" });
        }



        [HttpPost, Authorize]
        public JsonResult DeleteQualification(string id)
        {
            var directory = ResourcesPathReader.GetActivityDocumentsPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = GetDocFile(id);


            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["Id"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }

            return Json(new { Message = AplosMessage.Deleted });
        }


        #endregion
    }

    public class ActivityDocuments : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EmpUnderstandingActivityId { get; set; }
        public string DocumentCategoryId { get; set; }
        public string EmployeeId { get; set; }
        public string DocumentPreprationFrequency { get; set; }
        public string DocumentType { get; set; }
        public string DocumentFormat { get; set; }
        public string DocumentClass { get; set; }
        public string DocumentCode { get; set; }
        public string DocumentName { get; set; }
        public string Remarks { get; set; }
        public string Attachment { get; set; }
        public string FileName { get; set; }
        public string DocumentGeneration { get; set; }
        public string PreparedBy { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Scalar Properties
    }

}