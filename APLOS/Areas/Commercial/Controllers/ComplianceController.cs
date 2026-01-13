#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Commercial.Controllers
{
    public class ComplianceController : BaseController
    {
        string TableName = "hkp.ComplianceMaster";
        string TableName1 = "hkp.ComplianceCategoryType";



        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ComplianceController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize]
        public ActionResult Compliance()
        {
            return View();
        }
        [Authorize]
        public ActionResult Group()
        {
            return View();
        }
        [Authorize]
        public ActionResult Category()
        {
            return View();
        }
        [Authorize]
        public ActionResult SubCategory()
        {
            return View();
        }

        public ActionResult Transaction()
        {
            return View();
        }

        public ActionResult Audit()
        {
            return View();
        }


        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select  * from (select 
                    CM.*,
                    G.UserName ComplianceGroup,
                    C.UserName Category,
                    SC.UserName SubCategory,
                    HasFile = CASE WHEN FileName IS NOT NULL AND FileName != '' THEN 1 ELSE 0 END
                    from hkp.ComplianceMaster CM
                    LEFT JOIN hkp.ComplianceCategoryType G ON G.Id=CM.ComplianceGroupId
                    LEFT JOIN hkp.ComplianceCategoryType C ON C.Id=CM.CategoryId
                    LEFT JOIN hkp.ComplianceCategoryType SC ON SC.Id=CM.SubCategoryId) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create()
        {
            try
            {
                // Get form data
                string dataJson = Request.Form["data"] ?? "";

                if (string.IsNullOrEmpty(dataJson))
                {
                    return Json(new { Error = true, Message = "No data received from form" });
                }

                // Parse JSON data
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson) ?? new Dictionary<string, object>();

                // Get ID from data
                string id = "0";
                if (data.ContainsKey("Id") && data["Id"] != null)
                {
                    id = data["Id"].ToString();
                }

                // Check if we should delete existing file
                bool deleteExistingFile = data.ContainsKey("DeleteExistingFile") &&
                                          data["DeleteExistingFile"] != null &&
                                          Convert.ToBoolean(data["DeleteExistingFile"]);

                // Handle file upload
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string extension = Path.GetExtension(fileName).ToLower();

                        // Validate file type
                        string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { Error = true, Message = "Invalid file type. Allowed: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG" });
                        }

                        // Validate file size (2MB limit)
                        if (file.ContentLength > 2097152)
                        {
                            return Json(new { Error = true, Message = "File size exceeds 2MB limit" });
                        }

                        // **YOUR SERVER'S ABSOLUTE PATH**
                        string basePath = @"F:\aPOP\Pratibha\";
                        string uploadFolder = Path.Combine(basePath, "Uploads", "Compliance");

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(uploadFolder))
                        {
                            try
                            {
                                Directory.CreateDirectory(uploadFolder);
                            }
                            catch (Exception dirEx)
                            {
                                return Json(new { Error = true, Message = $"Cannot create upload folder: {dirEx.Message}" });
                            }
                        }

                        // Generate unique filename
                        string uniqueFileName = Guid.NewGuid().ToString() + extension;
                        string fullPhysicalPath = Path.Combine(uploadFolder, uniqueFileName);

                        // Save the file
                        try
                        {
                            file.SaveAs(fullPhysicalPath);
                        }
                        catch (Exception saveEx)
                        {
                            return Json(new { Error = true, Message = $"Cannot save file: {saveEx.Message}" });
                        }

                        // Store file info in data dictionary
                        // Store RELATIVE path for consistency
                        string relativePath = "/Uploads/Compliance/" + uniqueFileName;

                        data["FileName"] = fileName;
                        data["FilePath"] = relativePath; // Store relative path
                        data["FileSize"] = file.ContentLength;
                        data["FileType"] = file.ContentType;

                        // Also store as BLOB for reliability (optional but recommended)
                        using (BinaryReader br = new BinaryReader(file.InputStream))
                        {
                            data["FileContent"] = br.ReadBytes(file.ContentLength);
                        }
                    }
                }
                else if (deleteExistingFile && id != "0")
                {
                    // Delete existing file when user clears attachment
                    DeleteExistingFileOnServer(id);

                    // Clear file fields
                    data["FileName"] = DBNull.Value;
                    data["FilePath"] = DBNull.Value;
                    data["FileSize"] = DBNull.Value;
                    data["FileType"] = DBNull.Value;
                    data["FileContent"] = DBNull.Value;
                }

                // Database operations
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                // Check if record exists
                con.OpenDataSetThroughAdapter($"select * from hkp.ComplianceMaster where Id='{id}'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    // New record
                    bplib.clsGenID genid = new bplib.clsGenID();
                    string newId;
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString(), "hkp.ComplianceMaster", out newId);

                    data["Id"] = newId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    // Update existing record
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                // Save to database
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = "Saved successfully",
                    Id = data["Id"],
                    HasFile = data.ContainsKey("FileName") && data["FileName"] != DBNull.Value
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Error = true,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // Helper to delete existing file from server
        private void DeleteExistingFileOnServer(string id)
        {
            try
            {
                // Get existing file path from database
                string sql = $"SELECT FilePath FROM hkp.ComplianceMaster WHERE Id = '{id}'";
                var dataList = _sqlRepository.GetDataCollection(sql, null);

                if (dataList != null && dataList.Count > 0 && dataList[0].ContainsKey("FilePath"))
                {
                    string filePath = dataList[0]["FilePath"]?.ToString();

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        // Convert to physical path on YOUR server
                        string physicalPath = ConvertRelativeToAbsolutePath(filePath);

                        if (System.IO.File.Exists(physicalPath))
                        {
                            System.IO.File.Delete(physicalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - file deletion failure shouldn't stop main operation
                System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        // Convert relative path to absolute path for YOUR server
        private string ConvertRelativeToAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return relativePath;

            // Clean the path
            relativePath = relativePath.Trim();

            // Remove leading / or ~ if present
            if (relativePath.StartsWith("~/"))
                relativePath = relativePath.Substring(2);
            else if (relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);

            // **YOUR SERVER'S BASE PATH**
            string basePath = @"F:\aPOP\Pratibha\";

            // Combine paths (replace forward slashes with backslashes for Windows)
            return Path.Combine(basePath, relativePath.Replace("/", "\\"));
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
                con.executeQuery("delete from dbo.ComplianceResponsiblePerson where ComplianceMasterId='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult DeleteRP(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.ComplianceResponsiblePersonAndAuditor where Id='" + id + "'");
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
            try
            {
                if (dt == null || sourceData == null)
                {
                    System.Diagnostics.Debug.WriteLine("AddNewRow: dt or sourceData is null");
                    return;
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataRow dr = dt.NewRow();

                System.Diagnostics.Debug.WriteLine($"Adding new row with {sourceData.Count} fields");

                foreach (var item in sourceData.Keys)
                {
                    try
                    {
                        if (sourceData[item] != null && dt.Columns.Contains(item))
                        {
                            dr[item] = sourceData[item];
                            System.Diagnostics.Debug.WriteLine($"Set column {item} = {sourceData[item]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting column {item}: {ex.Message}");
                    }
                }

                // Set audit fields
                dr["AddedBy"] = identity?.Name ?? "System";
                dr["AddedDate"] = DateTime.Now;
                dr["AddedFromIP"] = identity?.IPAddress ?? "127.0.0.1";

                dt.Rows.Add(dr);
                System.Diagnostics.Debug.WriteLine("Row added successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddNewRow: {ex.Message}");
                throw;
            }
        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            try
            {
                if (dr == null || sourceData == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditRow: dr or sourceData is null");
                    return;
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                dr.BeginEdit();

                System.Diagnostics.Debug.WriteLine($"Editing row with {sourceData.Count} fields");

                foreach (var item in sourceData.Keys)
                {
                    try
                    {
                        if (sourceData[item] != null && dr.Table.Columns.Contains(item))
                        {
                            dr[item] = sourceData[item];
                            System.Diagnostics.Debug.WriteLine($"Updated column {item} = {sourceData[item]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating column {item}: {ex.Message}");
                    }
                }

                // Set audit fields
                dr["UpdatedBy"] = identity?.Name ?? "System";
                dr["UpdatedDate"] = DateTime.Now;
                dr["UpdatedFromIP"] = identity?.IPAddress ?? "127.0.0.1";

                dr.EndEdit();
                System.Diagnostics.Debug.WriteLine("Row edited successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EditRow: {ex.Message}");
                throw;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateRP(List<Dictionary<string, object>> RPDataList, string masterId)
        {
            try
            {
                DataSet dsRP;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.ComplianceResponsiblePersonAndAuditor where ComplianceMasterId='" + masterId + "'", out dsRP, false, "1");

                #region RPDataList 
                if (RPDataList != null)
                {
                    foreach (var item in RPDataList)
                    {
                        DataView dv = new DataView(dsRP.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsRP.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRP);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateAD(List<Dictionary<string, object>> RPDataList, string masterId)
        {
            try
            {
                DataSet dsRP;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.ComplianceResponsiblePersonAndAuditor where ComplianceMasterId='" + masterId + "'", out dsRP, false, "1");

                #region RPDataList 
                if (RPDataList != null)
                {
                    foreach (var item in RPDataList)
                    {
                        DataView dv = new DataView(dsRP.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsRP.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRP);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public JsonResult GetRP(string masterId)
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT RP.Id,RP.EmpSystemID
							    	,E.EmployeeName
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,P.UserName Plant
									,SS.UserName SubSection
							    FROM dbo.ComplianceResponsiblePersonAndAuditor RP
                                LEFT JOIN EmployeeInformation E ON E.SystemId=RP.EmpSystemID
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId Where ComplianceMasterId='" + masterId + "' AND SourceType ='ResponsiblePerson'"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAuditorData(string masterId)
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT RP.Id,RP.EmpSystemID
							    	,E.EmployeeName
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,P.UserName Plant
									,SS.UserName SubSection
							    FROM dbo.ComplianceResponsiblePersonAndAuditor RP
                                LEFT JOIN EmployeeInformation E ON E.SystemId=RP.EmpSystemID
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId Where ComplianceMasterId='" + masterId + "' AND SourceType ='Auditor'"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetComplianceCheckPointsData(string masterId)
        {
            string sql = @"SELECT M.Id,CP.Id CheckPointsId,CP.ComplianceMasterId,CP.CheckPointName,CheckMark=CASE WHEN M.CheckMark=1 THEN 'True' WHEN M.CheckMark=0 THEN 'False' ELSE '' END FROM dbo.ComplianceCheckPoints CP
LEFT JOIN [TRN].[ComplianceAuditorMap] M ON  CP.Id=M.CheckPointsId
Where CP.ComplianceMasterId='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult CreateCheckPoint(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.ComplianceCheckPoints where CheckPointName='" + data["CheckPointName"] + "'  AND  ComplianceMasterId='" + data["ComplianceMasterId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Check Point Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.ComplianceCheckPoints where Id='" + data["Id"] + "'", out dsMaster, false, "1");

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
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize]
        public ActionResult DeleteComplianceCheckPoints(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.ComplianceCheckPoints where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetCTList()
        {
            string sql = @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetCTList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select CT.*,CONVERT(varchar(5),ComplianceTime,108)ComTime,FORMAT(CT.ComplianceDate,'dd-MMM-yyyy')ComDate,EmployeeCode=(E.EmployeeCode+'-'+E.EmployeeName),CMV.ComplianceValue,CMR.LocationReference 
from [TRN].[ComplianceTransaction] CT
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=CT.EmployeeId
LEFT JOIN HKP.ComplianceMaster CMV ON CMV.Id=CT.ValueId
LEFT JOIN HKP.ComplianceMaster CMR ON CMR.Id=CT.LocationId) AS TEMP WHERE " + strkey + "";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateTransaction(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[ComplianceTransaction] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

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
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteTransaction(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [TRN].[ComplianceTransaction] where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #region Group
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetGroupCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName1 + " Where EntryType='Group'"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetCategoryCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName1 + " Where EntryType='Category'"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetSubCategoryCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName1 + " Where EntryType='SubCategory'"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDataList(string column, string value, string entryType)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName1 + " Where EntryType='" + entryType + "') AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCategoryTypeAutoSequence(string entryType)
        {
            return Json(GetCategoryTypeSequence(entryType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateData(Dictionary<string, object> data, string entryType)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "' AND EntryType='" + entryType + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "' AND EntryType='" + entryType + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "' AND EntryType='" + entryType + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName1, out _Id);

                    data["Id"] = _Id;
                    data["EntryType"] = entryType;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["EntryType"] = entryType;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetCategoryTypeSequence(entryType), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteData(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName1 + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private double GetCategoryTypeSequence(string entryType)
        {
            DataTable dt = _sqlRepository.GetDataTable(@"SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName1 + " Where EntryType='" + entryType + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion


        [HttpPost, Authorize]
        public ActionResult GetComplianceDataList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select CM.*,G.UserName ComplianceGroup,C.UserName Category,SC.UserName SubCategory 
,RPACount=(Select Count(Id) from dbo.ComplianceResponsiblePersonAndAuditor Where EmpSystemID='" + identity.EmployeeId + @"'AND ComplianceMasterId=CM.Id)
from hkp.ComplianceMaster CM
LEFT JOIN hkp.ComplianceCategoryType G ON G.Id=CM.ComplianceGroupId
LEFT JOIN hkp.ComplianceCategoryType C ON C.Id=CM.CategoryId
LEFT JOIN hkp.ComplianceCategoryType SC ON SC.Id=CM.SubCategoryId
Where CM.Id IN(Select ComplianceMasterId from dbo.ComplianceResponsiblePersonAndAuditor Where EmpSystemID='" + identity.EmployeeId + @"')) AS TEMP WHERE " + strkey + "";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetComplianceAuditDataList(string masterId)
        {

            string sql = @"select CA.Id,CA.ScorePoint,CA.Remark,CM.ComplianceGroupId,CM.Code,CM.CategoryId,CM.SubCategoryId,CM.ItemName,CM.CriticalityLevel,CM.ComplianceValue,CM.Remarks,CM.LocationReference,CM.ScanApplicable,CM.CodeApplicable,G.UserName ComplianceGroup,C.UserName Category,SC.UserName SubCategory,CA.ComplianceMasterId,CM.AuditFrequency,CM.AuditFrequencyUnit 
from TRN.ComplianceAudit CA
LEFT JOIN hkp.ComplianceMaster CM ON CM.Id=CA.ComplianceMasterId
LEFT JOIN hkp.ComplianceCategoryType G ON G.Id=CM.ComplianceGroupId
LEFT JOIN hkp.ComplianceCategoryType C ON C.Id=CM.CategoryId
LEFT JOIN hkp.ComplianceCategoryType SC ON SC.Id=CM.SubCategoryId
Where CM.Id='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateAudit(Dictionary<string, object> data, List<Dictionary<string, object>> CheckPList, string SourceType)
        {
            string tblname = "ComplianceAudit";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster, dsCheckPoint, dsEmp;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from TRN.ComplianceAudit where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from TRN.ComplianceAuditorMap where 1=2", out dsCheckPoint, false, "1");
                con.OpenDataSetThroughAdapter("select * from dbo.ComplianceResponsiblePersonAndAuditor where ComplianceMasterId='" + data["ComplianceMasterId"] + "' AND SourceType='" + SourceType + "' AND EmpSystemId='" + identity.EmployeeId + "'", out dsEmp, false, "1");
                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(tblname), out _Id);

                    data["Id"] = _Id;
                    data["EmpSystemId"] = identity.EmployeeId;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                if (CheckPList != null)
                {
                    foreach (var item in CheckPList)
                    {
                        DataView dv = new DataView(dsCheckPoint.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["ComplianceResponsiblePersonAndAuditorId"] = dsEmp.Tables[0].Rows[0]["Id"].ToString();
                            AddNewRow(dsCheckPoint.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsCheckPoint);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        // Clean SQL input
        private string CleanSqlInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Replace("'", "''");
        }

        [Authorize, HttpGet]
        public ActionResult DownloadFile(string id)
        {
            try
            {
                // 1. Get ONLY what we need
                string sql = "SELECT FileName, FileContent FROM hkp.ComplianceMaster WHERE Id = @Id";
                var dataList = _sqlRepository.GetDataCollection(sql,
                    new Dictionary<string, object> { { "@Id", id } });

                if (dataList == null || dataList.Count == 0)
                    return Content("File not found in database. ID: " + id);

                var data = dataList[0];
                string fileName = data["FileName"]?.ToString() ?? "file_" + id + ".download";

                // 2. Try BLOB first
                if (data["FileContent"] != null && data["FileContent"] is byte[])
                {
                    byte[] bytes = (byte[])data["FileContent"];
                    return File(bytes, "application/octet-stream", fileName);
                }

                // 3. Try ONE specific location
                string fixedPath = @"F:\aPOP\Pratibha\Uploads\Compliance\" + fileName;

                if (System.IO.File.Exists(fixedPath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(fixedPath);
                    return File(bytes, "application/octet-stream", fileName);
                }

                // 4. Last try: Search for any file with this name
                string searchFolder = @"F:\aPOP\Pratibha\Uploads\Compliance\";

                if (System.IO.Directory.Exists(searchFolder))
                {
                    foreach (string file in System.IO.Directory.GetFiles(searchFolder))
                    {
                        if (System.IO.Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            byte[] bytes = System.IO.File.ReadAllBytes(file);
                            return File(bytes, "application/octet-stream", fileName);
                        }
                    }
                }

                // 5. Final error
                return Content(
                    $"<h3>File Not Found</h3>" +
                    $"<p>File: <b>{fileName}</b></p>" +
                    $"<p>Checked location: <b>{fixedPath}</b></p>" +
                    $"<p>Please check if the file exists at that location.</p>",
                    "text/html"
                );
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        private string FindFileAnywhere(string storedPath, string fileName, string id)
        {
            List<string> allPossiblePaths = new List<string>();

            // **1. YOUR EXACT SERVER PATH (From error message)**
            string basePath = @"F:\aPOP\Pratibha\";

            // **2. Generate EVERY possible combination**

            // Combination 1: Direct filename in Uploads/Compliance
            allPossiblePaths.Add(Path.Combine(basePath, "Uploads", "Compliance", fileName));
            allPossiblePaths.Add(Path.Combine(basePath, "Uploads", "Compliance", Path.GetFileName(storedPath)));

            // Combination 2: Various folder structures
            string[] folders = {
        "",
        "Uploads\\",
        "Uploads\\Compliance\\",
        "Content\\Uploads\\",
        "Content\\Uploads\\Compliance\\",
        "App_Data\\Uploads\\",
        "App_Data\\Uploads\\Compliance\\",
        "Files\\",
        "Files\\Compliance\\",
        "Documents\\",
        "Documents\\Compliance\\",
        "Upload\\",
        "Upload\\Compliance\\"
    };

            foreach (string folder in folders)
            {
                allPossiblePaths.Add(Path.Combine(basePath, folder, fileName));
                allPossiblePaths.Add(Path.Combine(basePath, folder, Path.GetFileName(storedPath)));
            }

            // Combination 3: Server.MapPath variations
            try
            {
                // Try original path
                if (!string.IsNullOrEmpty(storedPath))
                {
                    allPossiblePaths.Add(Server.MapPath(storedPath));

                    // Try with ~/ prefix
                    if (!storedPath.StartsWith("~") && !storedPath.StartsWith("/"))
                    {
                        allPossiblePaths.Add(Server.MapPath("~/" + storedPath));
                    }

                    // Try with / prefix
                    if (!storedPath.StartsWith("/"))
                    {
                        allPossiblePaths.Add(Server.MapPath("/" + storedPath));
                    }
                }
            }
            catch { }

            // Combination 4: Application directory
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            allPossiblePaths.Add(Path.Combine(appPath, "Uploads", "Compliance", fileName));
            allPossiblePaths.Add(Path.Combine(appPath, "Uploads", "Compliance", Path.GetFileName(storedPath)));

            // Combination 5: If storedPath looks like a full path, use it
            if (!string.IsNullOrEmpty(storedPath) && storedPath.Length > 3 && storedPath.Contains(":\\"))
            {
                allPossiblePaths.Add(storedPath);
            }

            // Combination 6: Try cleaning the path
            if (!string.IsNullOrEmpty(storedPath))
            {
                string cleaned = storedPath.Replace("~/", "").Replace("/", "\\").TrimStart('\\');
                allPossiblePaths.Add(Path.Combine(basePath, cleaned));
            }

            // **3. Check EVERY path**
            foreach (string path in allPossiblePaths.Distinct())
            {
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ FILE FOUND AT: {path}");
                    return path;
                }
            }

            // **4. Last resort: Search entire drive for the file**
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 Starting deep search for file...");

                // Search in Uploads folder recursively
                string uploadsRoot = Path.Combine(basePath, "Uploads");
                if (Directory.Exists(uploadsRoot))
                {
                    // Search by filename
                    string[] foundFiles = Directory.GetFiles(uploadsRoot, fileName, SearchOption.AllDirectories);
                    if (foundFiles.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Found in deep search: {foundFiles[0]}");
                        return foundFiles[0];
                    }

                    // Search by stored filename
                    string storedFileName = Path.GetFileName(storedPath);
                    if (!string.IsNullOrEmpty(storedFileName))
                    {
                        foundFiles = Directory.GetFiles(uploadsRoot, storedFileName, SearchOption.AllDirectories);
                        if (foundFiles.Length > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ Found in deep search: {foundFiles[0]}");
                            return foundFiles[0];
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private ActionResult CreateFileNotFoundPage(string id, Dictionary<string, object> data, string filePath)
        {
            string fileName = data["FileName"]?.ToString() ?? "unknown";

            string html = @"
    <!DOCTYPE html>
    <html>
    <head>
        <title>File Not Found</title>
        <style>
            body { font-family: Arial, sans-serif; padding: 40px; }
            .error-box { 
                border: 2px solid #ff6b6b; 
                padding: 20px; 
                border-radius: 10px;
                background-color: #fff5f5;
            }
            .success { color: green; font-weight: bold; }
            .fail { color: red; font-weight: bold; }
            ul { background: #f8f9fa; padding: 15px; border-radius: 5px; }
        </style>
    </head>
    <body>
        <div class='error-box'>
            <h2>⚠️ File Download Error</h2>
            <p>The requested file could not be found on the server.</p>
            
            <h3>📊 File Information:</h3>
            <ul>
                <li><b>File ID:</b> " + id + @"</li>
                <li><b>File Name:</b> " + fileName + @"</li>
                <li><b>Path in Database:</b> " + (filePath ?? "NULL") + @"</li>
                <li><b>Has BLOB Data:</b> " + ((data["FileContent"] != null && data["FileContent"] is byte[]) ? "✅ YES" : "❌ NO") + @"</li>
            </ul>
            
            <h3>🔧 Immediate Solutions:</h3>
            <ol>
                <li><b>Option 1:</b> Re-upload the file to this record</li>
                <li><b>Option 2:</b> Check if file exists at: <code>F:\aPOP\Pratibha\Uploads\Compliance\" + fileName + @"</code></li>
                <li><b>Option 3:</b> Contact administrator to restore from backup</li>
            </ol>
            
            <h3>📞 Administrator Notice:</h3>
            <p>Check the server logs for detailed path search results.</p>
            
            <p><a href='javascript:history.back()'>← Go Back</a></p>
        </div>
    </body>
    </html>";

            return Content(html, "text/html");
        }

        [Authorize, HttpGet]
        public ActionResult EmergencyFix(string id)
        {
            try
            {
                // 1. First, let's see what's REALLY in the database
                string sql = "SELECT FileName, FilePath, FileContent FROM hkp.ComplianceMaster WHERE Id = @Id";
                var parameters = new Dictionary<string, object> { { "@Id", id } };
                var dataList = _sqlRepository.GetDataCollection(sql, parameters);

                if (dataList == null || dataList.Count == 0)
                {
                    return Content($"<h2>❌ Record {id} not found in database</h2>", "text/html");
                }

                var data = dataList[0];
                string fileName = data["FileName"]?.ToString() ?? "unknown";
                string filePath = data["FilePath"]?.ToString() ?? "";
                bool hasBlob = data["FileContent"] != null && data["FileContent"] is byte[];

                string result = $"<h2>🚨 Emergency Diagnostics - File ID: {id}</h2>";

                // 2. Check BLOB first
                if (hasBlob)
                {
                    result += $"<p style='color:green; font-weight:bold;'>✅ FILE CAN BE RECOVERED!</p>";
                    result += $"<p>The file exists as BLOB data in the database.</p>";
                    result += $"<p><a href='/Commercial/Compliance/ForceDownloadFromBlob?id={id}'>Click here to download directly from database</a></p>";
                }

                // 3. Check filesystem
                result += $"<h3>📁 Filesystem Check:</h3>";

                string searchPath = @"F:\aPOP\Pratibha\Uploads\Compliance\";
                if (Directory.Exists(searchPath))
                {
                    string[] files = Directory.GetFiles(searchPath);
                    result += $"<p>Found {files.Length} files in {searchPath}</p>";

                    // Check if our file exists
                    bool fileExists = files.Any(f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));

                    if (fileExists)
                    {
                        result += $"<p style='color:green;'>✅ File '{fileName}' FOUND in Uploads/Compliance folder!</p>";
                        string fullPath = Path.Combine(searchPath, fileName);
                        result += $"<p>Full path: {fullPath}</p>";
                        result += $"<p><a href='/Commercial/Compliance/ForceDownload?path={Uri.EscapeDataString(fullPath)}&name={Uri.EscapeDataString(fileName)}'>Download from this location</a></p>";
                    }
                    else
                    {
                        result += $"<p style='color:red;'>❌ File '{fileName}' NOT FOUND in Uploads/Compliance folder</p>";

                        // List similar files
                        var similarFiles = files.Where(f => f.Contains(fileName.Split('.')[0])).ToList();
                        if (similarFiles.Count > 0)
                        {
                            result += "<p>Similar files found:</p><ul>";
                            foreach (string file in similarFiles)
                            {
                                result += $"<li>{Path.GetFileName(file)}</li>";
                            }
                            result += "</ul>";
                        }
                    }
                }
                else
                {
                    result += $"<p style='color:red;'>❌ Directory not found: {searchPath}</p>";
                }

                // 4. Show database record
                result += $"<h3>💾 Database Record:</h3>";
                result += $"<p><b>FileName:</b> {fileName}</p>";
                result += $"<p><b>FilePath:</b> {filePath}</p>";
                result += $"<p><b>Has BLOB:</b> {(hasBlob ? "✅ YES" : "❌ NO")}</p>";

                return Content(result, "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<h2>Error in EmergencyFix:</h2><pre>{ex}</pre>", "text/html");
            }
        }

        [Authorize, HttpGet]
        public ActionResult ForceDownloadFromBlob(string id)
        {
            // Direct BLOB download bypassing all path logic
            string sql = "SELECT FileName, FileContent FROM hkp.ComplianceMaster WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            var dataList = _sqlRepository.GetDataCollection(sql, parameters);

            if (dataList != null && dataList.Count > 0)
            {
                var data = dataList[0];
                string fileName = data["FileName"]?.ToString() ?? "file_" + id + ".download";

                if (data["FileContent"] != null && data["FileContent"] is byte[])
                {
                    byte[] fileBytes = (byte[])data["FileContent"];
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
                    return File(fileBytes, "application/octet-stream");
                }
            }

            return Content("No BLOB data found for this file");
        }

        [Authorize, HttpGet]
        public ActionResult ForceDownload(string path, string name)
        {
            // Direct filesystem download
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                string fileName = name ?? Path.GetFileName(path);
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
                return File(fileBytes, "application/octet-stream");
            }

            return Content("File not found at specified path");
        }

        // Helper: Get alternative file paths to try
        private string[] GetAlternativeFilePaths(string filePath, string fileName)
        {
            var possiblePaths = new List<string>();

            if (string.IsNullOrEmpty(filePath))
                return possiblePaths.ToArray();

            System.Diagnostics.Debug.WriteLine($"=== GENERATING ALTERNATIVE PATHS ===");
            System.Diagnostics.Debug.WriteLine($"Input filePath: {filePath}");
            System.Diagnostics.Debug.WriteLine($"Input fileName: {fileName}");

            // **YOUR SERVER'S BASE PATH**
            string basePath = @"F:\aPOP\Pratibha\";

            // Get just the filename
            string justFileName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(justFileName))
                justFileName = fileName;

            System.Diagnostics.Debug.WriteLine($"Extracted filename: {justFileName}");

            // 1. YOUR EXACT PATH FROM ERROR MESSAGE (HIGHEST PRIORITY)
            string exactPath = Path.Combine(basePath, "Uploads", "Compliance", justFileName);
            possiblePaths.Add(exactPath);
            System.Diagnostics.Debug.WriteLine($"Added path 1: {exactPath}");

            // 2. With provided fileName
            possiblePaths.Add(Path.Combine(basePath, "Uploads", "Compliance", fileName));
            System.Diagnostics.Debug.WriteLine($"Added path 2: {Path.Combine(basePath, "Uploads", "Compliance", fileName)}");

            // 3. Converted path from ConvertRelativeToAbsolutePath
            string convertedPath = ConvertRelativeToAbsolutePath(filePath);
            possiblePaths.Add(convertedPath);
            System.Diagnostics.Debug.WriteLine($"Added path 3: {convertedPath}");

            // 4. Various folder combinations
            string[] folderVariations = {
        "Uploads\\Compliance\\",
        "Content\\Uploads\\Compliance\\",
        "App_Data\\Uploads\\Compliance\\",
        "Files\\Compliance\\",
        "Documents\\Compliance\\",
        "Uploads\\",
        "Files\\"
    };

            foreach (string folder in folderVariations)
            {
                possiblePaths.Add(Path.Combine(basePath, folder, justFileName));
                possiblePaths.Add(Path.Combine(basePath, folder, fileName));
            }

            // 5. Application root combinations
            string appRoot = AppDomain.CurrentDomain.BaseDirectory;
            possiblePaths.Add(Path.Combine(appRoot, "Uploads", "Compliance", justFileName));
            possiblePaths.Add(Path.Combine(appRoot, "Uploads", "Compliance", fileName));

            // 6. Try with just the base path
            possiblePaths.Add(Path.Combine(basePath, justFileName));
            possiblePaths.Add(Path.Combine(basePath, fileName));

            // 7. Try the path as-is (in case it's already absolute)
            if (Path.IsPathRooted(filePath))
            {
                possiblePaths.Add(filePath);
                System.Diagnostics.Debug.WriteLine($"Added path as-is (rooted): {filePath}");
            }

            // Remove duplicates and nulls
            var uniquePaths = possiblePaths
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .ToArray();

            System.Diagnostics.Debug.WriteLine($"Total unique paths generated: {uniquePaths.Length}");

            return uniquePaths;
        }

        // Helper: Get content type
        private string GetContentType(Dictionary<string, object> data, string fileName)
        {
            // First try to get from FileType column
            if (data.ContainsKey("FileType") && data["FileType"] != null && !string.IsNullOrEmpty(data["FileType"].ToString()))
            {
                return data["FileType"].ToString();
            }

            // Fall back to file extension
            return GetMimeType(fileName);
        }

        // Helper: Get MIME type from file extension
        private string GetMimeType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "application/octet-stream";

            string extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".pdf": return "application/pdf";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".jpg": case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".txt": return "text/plain";
                case ".zip": return "application/zip";
                case ".rar": return "application/x-rar-compressed";
                default: return "application/octet-stream";
            }
        }

        [Authorize, HttpPost]
        public JsonResult DeleteFileOnly(string id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DeleteFileOnly called for ID: {id}");

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { Error = true, Message = "ID is required" });
                }

                // Clean input
                id = CleanSqlInput(id);

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                // Get the record
                con.OpenDataSetThroughAdapter($"select * from hkp.ComplianceMaster where Id='{id}'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    return Json(new { Error = true, Message = "Record not found" });
                }

                DataRow row = dsMaster.Tables[0].Rows[0];

                // Delete physical file if exists
                if (row["FilePath"] != DBNull.Value && !string.IsNullOrEmpty(row["FilePath"].ToString()))
                {
                    string filePath = row["FilePath"].ToString();
                    DeleteExistingFile(id);
                }

                // Clear file information in database
                row["FileName"] = DBNull.Value;
                row["FilePath"] = DBNull.Value;
                row["FileContent"] = DBNull.Value;
                row["FileSize"] = DBNull.Value;
                row["FileType"] = DBNull.Value;

                // Save changes
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                System.Diagnostics.Debug.WriteLine("File deleted successfully from database");

                return Json(new
                {
                    Error = false,
                    Message = "File deleted successfully",
                    HasFile = false
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteFileOnly ERROR: {ex.Message}");
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        // Helper method to delete existing file (for DeleteFileOnly method)
        private void DeleteExistingFile(string id)
        {
            try
            {
                // Get existing file path from database
                string sql = $"SELECT FilePath FROM hkp.ComplianceMaster WHERE Id = '{id}'";
                var dataList = _sqlRepository.GetDataCollection(sql, null);

                if (dataList != null && dataList.Count > 0 && dataList[0].ContainsKey("FilePath"))
                {
                    string filePath = dataList[0]["FilePath"]?.ToString();

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        // Convert to physical path on YOUR server
                        string physicalPath = ConvertRelativeToAbsolutePath(filePath);

                        if (System.IO.File.Exists(physicalPath))
                        {
                            System.IO.File.Delete(physicalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        [Authorize, HttpGet]
        public ActionResult CheckFilePath(string id)
        {
            try
            {
                // Query database for file information
                string sql = $@"
            SELECT 
                Id,
                FileName,
                FilePath,
                FileType
            FROM hkp.ComplianceMaster 
            WHERE Id = '{id}'";

                var dataList = _sqlRepository.GetDataCollection(sql, null);

                if (dataList == null || dataList.Count == 0)
                {
                    return Content($"<h3>No record found for ID: {id}</h3>", "text/html");
                }

                var data = dataList[0];
                string fileName = data["FileName"]?.ToString() ?? "unknown";
                string filePath = data["FilePath"]?.ToString() ?? "";

                string result = $"<h3>File Path Diagnostic for ID: {id}</h3>";
                result += $"<p><b>FileName:</b> {fileName}</p>";
                result += $"<p><b>FilePath in DB:</b> {filePath}</p>";

                // Check ConvertRelativeToAbsolutePath result
                string convertedPath = ConvertRelativeToAbsolutePath(filePath);
                result += $"<p><b>Converted Path:</b> {convertedPath}</p>";
                result += $"<p><b>Exists at converted path:</b> {System.IO.File.Exists(convertedPath)}</p>";

                // Check your specific path from error
                string specificPath = @"F:\aPOP\Pratibha\Uploads\Compliance\" + Path.GetFileName(filePath);
                result += $"<p><b>Specific Path (F:\\aPOP\\Pratibha\\Uploads\\Compliance\\):</b> {specificPath}</p>";
                result += $"<p><b>Exists at specific path:</b> {System.IO.File.Exists(specificPath)}</p>";

                // List files in the Uploads/Compliance folder
                string uploadsPath = @"F:\aPOP\Pratibha\Uploads\Compliance\";
                result += $"<h4>Files in {uploadsPath}:</h4>";
                result += "<ul>";

                try
                {
                    if (Directory.Exists(uploadsPath))
                    {
                        foreach (string file in Directory.GetFiles(uploadsPath))
                        {
                            result += $"<li>{Path.GetFileName(file)}</li>";
                        }
                    }
                    else
                    {
                        result += $"<li>Directory does not exist: {uploadsPath}</li>";
                    }
                }
                catch (Exception ex)
                {
                    result += $"<li>Error reading directory: {ex.Message}</li>";
                }

                result += "</ul>";

                return Content(result, "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<h3>Error:</h3><p>{ex.Message}</p>", "text/html");
            }
        }
    }
}