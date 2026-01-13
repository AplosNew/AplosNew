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
                // **START DEBUG LOG**
                System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD FILE REQUEST STARTED ===");
                System.Diagnostics.Debug.WriteLine($"Request Time: {DateTime.Now}");
                System.Diagnostics.Debug.WriteLine($"File ID: {id}");

                if (string.IsNullOrEmpty(id))
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: File ID is null or empty");
                    return Content("Error: File ID is required");
                }

                // Clean input to prevent SQL injection
                id = CleanSqlInput(id);
                System.Diagnostics.Debug.WriteLine($"Cleaned ID: {id}");

                // **USE PARAMETERIZED QUERY (SECURITY FIX)**
                string sql = @"
            SELECT 
                Id,
                FileName,
                FilePath,
                FileContent,
                FileType,
                FileSize
            FROM hkp.ComplianceMaster 
            WHERE Id = @Id";

                var parameters = new Dictionary<string, object>
        {
            { "@Id", id }
        };

                var dataList = _sqlRepository.GetDataCollection(sql, parameters);

                if (dataList == null || dataList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: No data found for ID: {id}");
                    return Content("Error: File record not found in database");
                }

                var data = dataList[0];

                // **DEBUG: LOG ALL DATA FROM DATABASE**
                System.Diagnostics.Debug.WriteLine($"=== DATABASE RECORD FOUND ===");
                foreach (var key in data.Keys)
                {
                    if (key == "FileContent" && data[key] != null && data[key] is byte[])
                    {
                        byte[] bytes = (byte[])data[key];
                        System.Diagnostics.Debug.WriteLine($"{key}: [BLOB DATA - {bytes.Length} bytes]");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{key}: {data[key]}");
                    }
                }

                // Get file name
                string fileName = "download.file";
                if (data.ContainsKey("FileName") && data["FileName"] != null && !string.IsNullOrEmpty(data["FileName"].ToString()))
                {
                    fileName = data["FileName"].ToString();
                    System.Diagnostics.Debug.WriteLine($"Final FileName: {fileName}");
                }

                // **OPTION 1: First try to get file from BLOB (Most Reliable)**
                if (data.ContainsKey("FileContent") && data["FileContent"] != null && data["FileContent"] is byte[])
                {
                    byte[] fileBytes = (byte[])data["FileContent"];
                    string contentType = GetContentType(data, fileName);

                    System.Diagnostics.Debug.WriteLine($"=== USING BLOB DATA ===");
                    System.Diagnostics.Debug.WriteLine($"BLOB Size: {fileBytes.Length} bytes");
                    System.Diagnostics.Debug.WriteLine($"Content-Type: {contentType}");
                    System.Diagnostics.Debug.WriteLine($"Returning file: {fileName}");
                    System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD COMPLETE (BLOB) ===");

                    return File(fileBytes, contentType, fileName);
                }

                // **OPTION 2: Try to get file from file system**
                if (data.ContainsKey("FilePath") && data["FilePath"] != null && !string.IsNullOrEmpty(data["FilePath"].ToString()))
                {
                    string filePath = data["FilePath"].ToString();

                    System.Diagnostics.Debug.WriteLine($"=== ATTEMPTING FILE SYSTEM DOWNLOAD ===");
                    System.Diagnostics.Debug.WriteLine($"Original FilePath from DB: {filePath}");
                    System.Diagnostics.Debug.WriteLine($"FileName to use: {fileName}");

                    // **STRATEGY 1: Convert relative path to YOUR server's absolute path**
                    string physicalPath = ConvertRelativeToAbsolutePath(filePath);
                    System.Diagnostics.Debug.WriteLine($"Strategy 1 - Converted Path: {physicalPath}");
                    System.Diagnostics.Debug.WriteLine($"Strategy 1 - File Exists: {System.IO.File.Exists(physicalPath)}");

                    if (System.IO.File.Exists(physicalPath))
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
                        string contentType = GetContentType(data, fileName);

                        System.Diagnostics.Debug.WriteLine($"=== FILE FOUND VIA STRATEGY 1 ===");
                        System.Diagnostics.Debug.WriteLine($"File Size: {fileBytes.Length} bytes");
                        System.Diagnostics.Debug.WriteLine($"Content-Type: {contentType}");
                        System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD COMPLETE (Strategy 1) ===");

                        return File(fileBytes, contentType, fileName);
                    }

                    // **STRATEGY 2: Try Server.MapPath**
                    try
                    {
                        string mappedPath = Server.MapPath(filePath);
                        System.Diagnostics.Debug.WriteLine($"Strategy 2 - Server.MapPath: {mappedPath}");
                        System.Diagnostics.Debug.WriteLine($"Strategy 2 - File Exists: {System.IO.File.Exists(mappedPath)}");

                        if (System.IO.File.Exists(mappedPath))
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(mappedPath);
                            string contentType = GetContentType(data, fileName);

                            System.Diagnostics.Debug.WriteLine($"=== FILE FOUND VIA STRATEGY 2 ===");
                            System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD COMPLETE (Strategy 2) ===");

                            return File(fileBytes, contentType, fileName);
                        }
                    }
                    catch (Exception mapEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Strategy 2 - Server.MapPath Error: {mapEx.Message}");
                    }

                    // **STRATEGY 3: Try with ~ prefix**
                    try
                    {
                        if (!filePath.StartsWith("~"))
                        {
                            string mappedPath = Server.MapPath("~/" + filePath);
                            System.Diagnostics.Debug.WriteLine($"Strategy 3 - Server.MapPath(~): {mappedPath}");
                            System.Diagnostics.Debug.WriteLine($"Strategy 3 - File Exists: {System.IO.File.Exists(mappedPath)}");

                            if (System.IO.File.Exists(mappedPath))
                            {
                                byte[] fileBytes = System.IO.File.ReadAllBytes(mappedPath);
                                string contentType = GetContentType(data, fileName);

                                System.Diagnostics.Debug.WriteLine($"=== FILE FOUND VIA STRATEGY 3 ===");
                                System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD COMPLETE (Strategy 3) ===");

                                return File(fileBytes, contentType, fileName);
                            }
                        }
                    }
                    catch (Exception mapEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Strategy 3 - Server.MapPath(~) Error: {mapEx.Message}");
                    }

                    // **STRATEGY 4: Try alternative paths on YOUR server**
                    System.Diagnostics.Debug.WriteLine($"=== TRYING ALTERNATIVE PATHS ===");
                    string[] alternativePaths = GetAlternativeFilePaths(filePath, fileName);

                    foreach (string altPath in alternativePaths)
                    {
                        System.Diagnostics.Debug.WriteLine($"Checking: {altPath}");
                        bool exists = System.IO.File.Exists(altPath);
                        System.Diagnostics.Debug.WriteLine($"Exists: {exists}");

                        if (exists)
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(altPath);
                            string contentType = GetContentType(data, fileName);

                            System.Diagnostics.Debug.WriteLine($"=== FILE FOUND VIA ALTERNATIVE PATH ===");
                            System.Diagnostics.Debug.WriteLine($"Alternative Path Used: {altPath}");
                            System.Diagnostics.Debug.WriteLine($"File Size: {fileBytes.Length} bytes");
                            System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD COMPLETE (Alternative Path) ===");

                            return File(fileBytes, contentType, fileName);
                        }
                    }

                    // **STRATEGY 5: Check if file exists by filename only in Uploads/Compliance**
                    string simplePath = @"F:\aPOP\Pratibha\Uploads\Compliance\" + Path.GetFileName(filePath);
                    System.Diagnostics.Debug.WriteLine($"Strategy 5 - Simple Path: {simplePath}");
                    System.Diagnostics.Debug.WriteLine($"Strategy 5 - File Exists: {System.IO.File.Exists(simplePath)}");

                    if (System.IO.File.Exists(simplePath))
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(simplePath);
                        string contentType = GetContentType(data, fileName);

                        System.Diagnostics.Debug.WriteLine($"=== FILE FOUND VIA SIMPLE PATH ===");
                        System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD COMPLETE (Simple Path) ===");

                        return File(fileBytes, contentType, fileName);
                    }

                    // **FINAL: File not found anywhere**
                    System.Diagnostics.Debug.WriteLine($"=== FILE NOT FOUND ===");
                    System.Diagnostics.Debug.WriteLine($"Last attempted path: {simplePath}");

                    // Create detailed error message
                    string errorMessage = $"Error: File not found on server.<br/><br/>";
                    errorMessage += $"<b>Details:</b><br/>";
                    errorMessage += $"• File ID: {id}<br/>";
                    errorMessage += $"• File Name: {fileName}<br/>";
                    errorMessage += $"• Path in DB: {filePath}<br/>";
                    errorMessage += $"• Expected location: F:\\aPOP\\Pratibha\\Uploads\\Compliance\\<br/><br/>";
                    errorMessage += $"<b>Troubleshooting:</b><br/>";
                    errorMessage += $"1. Check if file exists in F:\\aPOP\\Pratibha\\Uploads\\Compliance\\<br/>";
                    errorMessage += $"2. Verify file permissions<br/>";
                    errorMessage += $"3. Check if file was moved or renamed<br/>";

                    return Content(errorMessage, "text/html");
                }

                System.Diagnostics.Debug.WriteLine($"ERROR: No file attached to record");
                return Content("Error: No file attached to this record");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== CRITICAL ERROR ===");
                System.Diagnostics.Debug.WriteLine($"Error Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"=== DOWNLOAD FAILED ===");

                return Content($"Error downloading file: {ex.Message}");
            }
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