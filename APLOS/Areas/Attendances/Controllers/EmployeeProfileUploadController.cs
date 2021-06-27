#region Using
using Aplos.Controllers;
using Aplos.Properties;
using System.Web.Mvc;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using System;
using Library.Data.Sql;
using System.IO;
using Library.Data;
using Library.Service.Helpers;
using System.Data.OleDb;
using Syncfusion.XlsIO;
using Library.Model.Enums;
using Library.Service.HumanResources;
using Library.Service.HumanResources.Profile;
using Library.Service.Organizations;
using Library.Service.HumanResources.Shift;
using Library.HumanResource.Leave.LeaveUploadXL;
using Library.HumanResource.Employee.BankInformationUploadXL;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class EmployeeProfileUploadController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IManpowerBudgetService _IManpowerBudgetService;

        public EmployeeProfileUploadController(
               ISqlRepository sqlRepository,
               IManpowerBudgetService IManpowerBudgetService,
               IAttendanceManagementService AttendanceManagementService
            )
        {
            _sqlRepository = sqlRepository;
            _IManpowerBudgetService = IManpowerBudgetService;
            _AttendanceManagementService = AttendanceManagementService;

        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        #region Profile
        [HttpPost, Authorize]
        public JsonResult ImportData()
        {
            string path;
            clsTemplateReadProfile objR = null;
            try
            {
                objR = new clsTemplateReadProfile();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = objR.ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //GetPlantwiseData(identity.PlantId);
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private DataSet ReadExcelToTable(string path)
        {

            //Connection String

            //string connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=NO;IMEX=1';";
            //the same name 
            string connstring = "Provider = Microsoft.JET.OLEDB.4.0; Data Source = " + path + "; Extended Properties = 'Excel 8.0;HDR=NO;IMEX=1'; ";

            using (OleDbConnection conn = new OleDbConnection(connstring))
            {
                conn.Open();
                //Get All Sheets Name
                DataTable sheetsName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });

                //Get the First Sheet Name
                string firstSheetName = sheetsName.Rows[0][2].ToString();
                firstSheetName = "Sheet1$";
                //Query String 
                string sql = string.Format("SELECT * FROM [{0}]", firstSheetName);
                OleDbDataAdapter ada = new OleDbDataAdapter(sql, connstring);
                DataSet set = new DataSet();
                ada.Fill(set);
                return set;
            }
        }
        [HttpPost]
        public ActionResult SaveProfileData(List<EmployeeProfileUploadTemplate> epList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsTemplateSaveProfile sp = new clsTemplateSaveProfile();
            sp.SaveData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, epList);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            clsTemplateDownloadProfile obj = new clsTemplateDownloadProfile();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = obj.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Employee Profile Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        } 
        #endregion

        #region Shift
        [HttpPost, Authorize]
        public JsonResult ImportDataShift()
        {
            string path;
            clsTemplateReadShiftAssignment objR = null;
            try
            {
                objR = new clsTemplateReadShiftAssignment();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = objR.ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSampleFileShift(ReportFormat reportFormat)
        {
            clsTemplateDownloadShiftAssignment obj = new clsTemplateDownloadShiftAssignment();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = obj.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Shift Assignment Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }
        [HttpPost]
        public ActionResult SaveShiftData(List<EmployeeShiftUploadTemplate> epList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsTemplateSaveShiftAssignment sp = new clsTemplateSaveShiftAssignment();
            sp.SaveData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, epList);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        } 
        #endregion

        #region Weekoff
        [HttpGet, Authorize]
        public ActionResult LoadEmployeelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'),
                         E.SystemId, e.EmployeeCode,e.EmployeeName ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ,format(e.DOS,'dd-MMM-yyyy')DOS
                         , EC.UserName EmpCategoryName
                         ,ld.UserName Designation
                         ,U.UserName Unit
                         ,Dv.UserName Division
                         ,Dp.UserName Department
                         ,Se.UserName Section
                         ,SB.UserName SubSection
                         ,L.UserName Line
                          from  EmployeeInformation e 
 
                          LEFT JOIN MST.ManpowerBudget PMB ON e.BudgetCode=PMB.Id
                          LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                          LEFT JOIN ORG.Entity En ON PMB.EntityId=En.Id
                         LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                         LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                         LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                         LEFT JOIN ORG.Section Se ON PR.SectionID = Se.Id
                         LEFT JOIN ORG.SubSection SB ON PR.SubSectionID = SB.Id
                         LEFT JOIN ORG.Line L ON PMB.LineID = L.Id
                         LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                         LEFT JOIN HKP.LegalDesignation AS ld ON e.LegalDesignationId = ld.Id
                         LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                         LEFT JOIN HKP.EmployeeCategory AS EC ON dm.EmployeeCategoryId  = EC.Id     			
                         WHERE  E.PlantId='" + identity.PlantId + @"' and e.EmployeeStatus='Active'  ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult ImportDataWeekOff()
        {
            string path;
            clsTemplateReadEmployeeWeekOff objR = null;
            try
            {
                objR = new clsTemplateReadEmployeeWeekOff();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = objR.ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSampleFileWeeOff(ReportFormat reportFormat)
        {
            clsTemplateDownloadEmployeeWeekOff obj = new clsTemplateDownloadEmployeeWeekOff();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = obj.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Weekoff Assignment Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }
        [HttpPost]
        public ActionResult SaveWeekOffData(List<EmployeeWeekOffUploadTemplate> empList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsTemplateSaveEmployeeWeekOff sp = new clsTemplateSaveEmployeeWeekOff();
            sp.SaveData(identity, empList);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Leave
        [HttpPost, Authorize]
        public JsonResult ImportDataLeave()
        {
            string path;
            clsTemplateReadLeave objR = null;
            try
            {
                objR = new clsTemplateReadLeave();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = objR.ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSampleFileLeave(ReportFormat reportFormat)
        {
            clsTemplateDownloadLeave obj = new clsTemplateDownloadLeave();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = obj.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Leave Info Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }
        [HttpPost]
        public ActionResult SaveLeaveData(List<LeaveUploadTemplate> epList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsTemplateSaveLeave sp = new clsTemplateSaveLeave();
            sp.SaveData(identity, epList);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Bank
        [HttpPost, Authorize]
        public JsonResult ImportDataBank()
        {
            string path;
            clsTemplateReadBankInformation objR = null;
            try
            {
                objR = new clsTemplateReadBankInformation();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = objR.ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSampleFileBank(ReportFormat reportFormat)
        {
            clsTemplateDownloadBankInformation obj = new clsTemplateDownloadBankInformation();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = obj.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Bank Info Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }
        [HttpPost, Authorize]
        public ActionResult SaveBankData(List<BankInformationUploadTemplate> epList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsTemplateSaveBankInformation sp = new clsTemplateSaveBankInformation();
            sp.SaveData(identity, epList);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }


}