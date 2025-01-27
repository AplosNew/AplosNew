#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class SOPDocumentController : BaseController
    {
       string TableName= "hkp.sopdocument";
        #region --Constructor
        private readonly ISOPDocumentService _SOPDocumentService;
        private readonly ISqlRepository _sqlRepository;

        public SOPDocumentController(ISOPDocumentService SOPDocumentService, ISqlRepository R)
        {
            _SOPDocumentService = SOPDocumentService;
            _sqlRepository = R;
        }
        #endregion

        #region dll
        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_SOPDocumentService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCriticalLevelData()
        {
            string query = @"select Id,UserName from hkp.Critical";
            return Json(_sqlRepository.GetDataCollection(query), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDocumentTypeData()
        {
            string query = @"select Id,UserName from hkp.DocumentType";
            return Json(_sqlRepository.GetDataCollection(query), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetModuleList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [MMS].[Module]"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region -- Pages
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);

        }
        // for Sequence
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName);
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        [HttpGet, Authorize]
        public JsonResult GetSOPDocumentList(GridParameter parameters)
        {
            return Json(_SOPDocumentService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSOPDocumentById(string id)
        {
            return Json(_SOPDocumentService.Find(id), JsonRequestBehavior.AllowGet);
        }
        // Save Method
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            SOPDocument sopDocument = new JavaScriptSerializer().Deserialize<SOPDocument>(form["SOPDocument"]);

            _SOPDocumentService.Insert(sopDocument);

            try
            {

                if (file.IsNotNull())
                {
                    var directory = ResourcesPathReader.GetSOPActivityDocumentPath();
                    string path = Path.Combine(directory);

                    for (int i = 0; i < file.Length; i++)
                    {
                        ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                    }

                    var fileId = "";
                    var fileName = "";
                    var filedata = _SOPDocumentService.GetSOPDocumentFile(sopDocument.Id);
                    if (filedata.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                            !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                            fileId = filedata["FileId"].ToString();
                        fileName = filedata["FileName"].ToString();

                        if (fileName != sopDocument.FileName)
                            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                    }

                    foreach (var item in file)
                    {
                        if (item != null)
                        {
                            if (System.IO.File.Exists(path + item.FileName))
                                System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                            item.SaveAs(path + sopDocument.Id + Path.GetExtension(item.FileName));
                        }
                    }

                }

            }
            catch (System.Exception ex)
            {
            }
            return Json(new { SOPDocument = sopDocument, Sequence = _SOPDocumentService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        //Update Method
        [HttpPost]
        public JsonResult Edit(SOPDocument SOPDocument)
        {
            _SOPDocumentService.Update(SOPDocument);
            return Json(new { Sequence = _SOPDocumentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public JsonResult Delete(string id)
        {
            var directory = ResourcesPathReader.GetSOPActivityDocumentPath();
            string path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _SOPDocumentService.GetSOPDocumentFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _SOPDocumentService.Delete(id);
            return Json(new { Sequence = _SOPDocumentService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion



        #region Multiple Value Process selection 
        //Get data Process For Selection
        [HttpGet]
        public JsonResult LoadAllProcessForSelection(string SOPDocumentId) // For Process popUp
        {
            string sql = @"select convert(bit,0) AS isSelected, * from hkp.Process P where P.Id not in (select ProcessId from SOPDocumentProcess where SOPDocumentId='" + SOPDocumentId + @"')";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        //Save Mathod For process, SOPDocumentProcess
        [HttpPost]
        public JsonResult SaveProcess(string SOPDocumentId, List<Dictionary<string, object>> ProcessData)
        {
            try
            {
               
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from SOPDocumentProcess where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < ProcessData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("SOPDocumentProcess", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();
                    dr["ProcessId"] = ProcessData[i]["Id"].ToString(); //save Process field
                    dr["SOPDocumentId"] = SOPDocumentId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Process updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }
        //Loading for GrideView Show from hkp.Process, SOPDocumentProcess
        [HttpGet]
        public JsonResult LoadAllSelectedProcess(string SOPDocumentId)  //LoadAllSelectedDepartment
        {
            string sql = @"select D.Id,P.Id as ProcessId,P.Code,P.UserName,P.ShortName,P.StandardName from [SOPDocumentProcess] D
              left outer join hkp.Process P on p.id=d.processid
              where d.SOPDocumentId='" + SOPDocumentId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteSelectedProcess(string Id)
        {
            try
            {
                string sql = @" delete from SOPDocumentProcess where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Process deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Multiple Value Process selection 

        #region Multiple Value Department selection 
        //Get data DepartmentForSelection
        [HttpGet]
        public JsonResult LoadAllDepartmentForSelection(string SOPDocumentId)
        {
            string sql = @"select convert(bit,0) AS isSelected, * from org.Department D where D.Id not in (select DepartmentId from SOPDocumentDepartment where SOPDocumentId='" + SOPDocumentId + @"')";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //Save Mathod For SOPDocumentDepartment
        [HttpPost]
        public JsonResult SaveDepartment(string SOPDocumentId, List<Dictionary<string, object>> DepartmentData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from SOPDocumentDepartment where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < DepartmentData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("SOPDocumentDepartment", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();
                    dr["DepartmentId"] = DepartmentData[i]["Id"].ToString();
                    dr["SOPDocumentId"] = SOPDocumentId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Department updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }
        //Loading for GrideView Show SOPDocumentDepartment
        [HttpGet]
        public JsonResult LoadAllSelectedDepartment(string SOPDocumentId)  //LoadAllSelectedDepartment
        {
            string sql = @"select DD.Id,dept.Id as DepartmentId,dept.Code,dept.UserName,dept.ShortName,dept.StandardName 
                             from [SOPDocumentDepartment] DD
                             left outer join org.Department dept on dept.id=DD.DepartmentId
                             where DD.SOPDocumentId= '" + SOPDocumentId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //DeleteSelectedDepartment
        // Delete Method for SOPDocumentDepartment
        [HttpGet]
        public JsonResult DeleteSelectedDepartment(string Id)
        {
            try
            {
                string sql = @" delete from SOPDocumentDepartment where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Department deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Multiple Value Department selection

        #region Multiple Value Location selection 
        //Get data LocationForSelection
        [HttpGet]
        public JsonResult LoadAllLocationForSelection(string SOPDocumentId)
        {
            string sql = @"select convert(bit,0) AS isSelected, * from HKP.DocumentLocation D where D.Id not in (select DocumentLocationId from SOPDocumentLocation where SOPDocumentId='" + SOPDocumentId + @"')";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //Save Mathod For SOPDocumentLocation
        [HttpPost]
        public JsonResult SaveLocation(string SOPDocumentId, List<Dictionary<string, object>> LocationData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from SOPDocumentLocation where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < LocationData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("SOPDocumentLocation", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();
                    dr["DocumentLocationId"] = LocationData[i]["Id"].ToString();
                    dr["SOPDocumentId"] = SOPDocumentId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Location updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }
        //Loading for GrideView Show SOPDocumentLocation
        [HttpGet]
        public JsonResult LoadAllSelectedLocation(string SOPDocumentId)  //LoadAllSelectedLocation
        {
            string sql = @"select DD.Id,dept.Id as DocumentLocationId,dept.Code,dept.UserName,dept.ShortName,dept.StandardName 
                             from [SOPDocumentLocation] DD
                             left outer join HKP.DocumentLocation dept on dept.id=DD.DocumentLocationId
                             where DD.SOPDocumentId= '" + SOPDocumentId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //DeleteSelectedLocation
        // Delete Method for SOPDocumentLocation
        [HttpGet]
        public JsonResult DeleteSelectedLocation(string Id)
        {
            try
            {
                string sql = @" delete from SOPDocumentLocation where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Location deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Multiple Value Location selection 

        //Get data DocumentSourceForSelection
        #region Multiple Value DocumentSource selection 
        [HttpGet]
        public JsonResult LoadAllDocumentSourceForSelection(string SOPDocumentId)
        {
            string sql = @" select convert(bit,0) AS isSelected, * from hkp.DocumentSource P where P.Id not in (select DocumentSourceId from SOPDocumentSource where SOPDocumentId='" + SOPDocumentId + @"')";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveDocumentSource(string SOPDocumentId, List<Dictionary<string, object>> DocumentSourceData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from SOPDocumentSource where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < DocumentSourceData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("SOPDocumentSource", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();
                    dr["DocumentSourceId"] = DocumentSourceData[i]["Id"].ToString();
                    dr["SOPDocumentId"] = SOPDocumentId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "DocumentSource updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }
        //Gride view SelectedDocumentSource
        [HttpGet]
        public JsonResult LoadAllSelectedDocumentSource(string SOPDocumentId)  //LoadAllSelectedDepartment
        {
            string sql = @"select D.Id,P.Id as DocumentSourceId,P.Code,P.UserName,P.ShortName,P.StandardName from [SOPDocumentSource] D
              left outer join hkp.DocumentSource P on p.id=d.DocumentSourceid
              where d.SOPDocumentId='" + SOPDocumentId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteSelectedDocumentSource(string Id)
        {
            try
            {
                string sql = @" delete from SOPDocumentSource where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "DocumentSource deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Multiple Value DocumentSource selection 


        //Get DocumentPreparedByForSelection

        [HttpPost, Authorize]
        public ActionResult LoadAllDocumentPreparedByForSelection(string SOPDocumentId, string EntryType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"
                        SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                           
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and  EMP.EmployeeStatus ='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeInformationSystemId,'') from SOPDocumentPreparedBy where SOPDocumentId='" + SOPDocumentId + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            //position
            if (EntryType.ToUpper() != "EMPLOYEE")
            {
                sql = @"SELECT distinct convert(bit,0) AS isSelected,PR.Id,PR.Code,  isnull(DEG.UserName,'') Designation,
                            '' AS EmployeeName,PR.UserName PositionName, 'Active' As EmployeeStatus,
                            DEPT.UserName Department,S.UserName Section,
                            SS.UserName SubSection
                            FROM  ORG.Position PR 
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN HKP.Designation DEG ON PR.DesignationId=DEG.Id
    
                        WHERE PR.CompanyGroupId='" + identity.CompanyGroupId + @"'  and pr.Active=1
                AND ISNULL(pr.Id,'') not in (select ISNULL(PositionId,'') from SOPDocumentPreparedBy where SOPDocumentId='" + SOPDocumentId + @"')";


            }

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        #region Multiple Value DocumentPreparedBy selection 


        [HttpPost]
        public JsonResult SaveDocumentPreparedBy(string SOPDocumentId, string EntryType, List<Dictionary<string, object>> DocumentPreparedByData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from SOPDocumentPreparedBy where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < DocumentPreparedByData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("SOPDocumentPreparedBy", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();
                    if (EntryType.ToUpper() == "EMPLOYEE")
                    {
                        dr["EmployeeInformationSystemId"] = DocumentPreparedByData[i]["Id"].ToString();
                    }
                    else
                    {
                        dr["PositionId"] = DocumentPreparedByData[i]["Id"].ToString();
                    }
                    dr["SOPDocumentId"] = SOPDocumentId;
                    dr["EntryType"] = EntryType;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "DocumentPreparedBy updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        // Gride View SelectedDocumentPreparedBy
        [HttpGet]
        public JsonResult LoadAllSelectedDocumentPreparedBy(string SOPDocumentId)  //LoadAllSelectedDepartment
        {
            string sql = @"  SELECT DISTINCT spb.EntryType AS EntryTypeName, EMP.EmployeeStatus, SPB.Id,EMP.EmployeeCode AS Code, EMP.EmployeeName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName, DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                            INNER JOIN SOPDocumentPreparedBy AS spb ON spb.EmployeeInformationSystemId=emp.SystemId
							WHERE spb.SOPDocumentId='" + SOPDocumentId + @"'
                            UNION ALL 
                            
                            SELECT DISTINCT spb.EntryType AS EntryTypeName,CASE WHEN PR.Active=1 then 'Active' else 'Inactive' end AS EmployeeStatus, SPB.Id,PR.Code, '' AS EmployeeName,isnull(DEG.UserName,'') Designation,
                            pr.UserName AS PositionName,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                            FROM  ORG.Position PR 
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN HKP.Designation DEG ON PR.DesignationId=DEG.Id
                            INNER JOIN SOPDocumentPreparedBy AS spb ON spb.PositionId=PR.Id
                            
                             WHERE spb.SOPDocumentId='" + SOPDocumentId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteSelectedDocumentPreparedBy(string Id)
        {
            try
            {
                string sql = @" delete from SOPDocumentPreparedBy where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "DocumentPreparedBy deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Multiple Value DocumentPreparedBy selection 
    }
}