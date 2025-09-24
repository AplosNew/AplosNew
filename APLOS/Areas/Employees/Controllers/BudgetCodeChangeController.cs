#region Using
using System;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Syncfusion.XlsIO;
using Library.Data.Sql;
using Library.Model.Enums;
using System.Data;
using Library.Security.Core;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class BudgetCodeChangeController : BaseController
    {
        #region Constructor

        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public BudgetCodeChangeController(
              IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeProfileService employeeProfileService
            , ISqlRepository sqlRepository
           )
        {
            _employeeInformationService = employeeInformationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        [HttpGet, Authorize]
        public JsonResult GetEmployeeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployeeService.Query(parameters, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Update(EmployeeInformation employeeInformation)
        {
            _employeeProfileService.UpdateBudgetCode(employeeInformation);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public JsonResult GetGivenDesignationByLegalDesignationCbo(string legalDesignationId, string BudgetCode)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT DISTINCT B.DesignationId, C.UserName FROM [MST].[DesignationMasterLegalDesignation] A
                            INNER JOIN  [MST].[DesignationMaster] B ON B.Id=A.DesignationMasterId
                            INNER JOIN HKP.Designation C ON C.Id=B.DesignationId
                            WHERE A.LegalDesignationId IN(select LegalDesignationId from [MST].[DesignationMasterLegalDesignation] where DesignationMasterId=
(select Id from MST.DesignationMaster where 
DesignationId=(select DesignationId from  ORG.Position where Id =(select PositionId from mst.ManpowerBudget where id='" + BudgetCode + "'))))";

                strSQL = @"SELECT B.DesignationId, C.UserName FROM [MST].[DesignationMasterLegalDesignation] A
                            INNER JOIN  [MST].[DesignationMaster] B ON B.Id=A.DesignationMasterId
                            INNER JOIN HKP.Designation C ON C.Id=B.DesignationId
                            WHERE A.LegalDesignationId='" + legalDesignationId + "'";
                return Json(_sqlRepository.GetCombo(strSQL, "DesignationId", "UserName"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetInActiveLegalDesignaion(string legalDesignationId)
        {
            string sql;

            try
            {
                sql = @"SELECT Active FROM  [HKP].[LegalDesignation] WHERE id='" + legalDesignationId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetLegalSalaryGradeDesignation(string legalDesignationId)
        {
            string sql;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                sql = @"SELECT LegalDesignationId FROM [MST].[LegalSalaryGradeDesignation] WHERE PlantId='" + identity.PlantId + "' AND LegalDesignationId='" + legalDesignationId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SyncGivenDesignation()
        {
            _employeeProfileService.UpdateGivenDesignation();
            return Json(new { Message = AplosMessage.Updated });
        }

        #region Tab 2 Operations

        #region GettingOperations

        [HttpGet, Authorize]
        public ActionResult GetCurrentFileList()
        {
            var str = @"Select EmployeeCode, EmployeeName , BudgetCode from dbo.EmployeeInformation Where EmployeeStatus='Active' AND EmpType<>'Guest'";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SampleDownload
        [HttpGet, Authorize]
        public ActionResult GetSampleReport(string plantId, string name, ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = "BudgetUpload-" + name + "-" + date;
            var workbook = GetBudgetWorkSheet(plantId);
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

        private IWorkbook GetBudgetWorkSheet(string plantId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            //DataTable data = eob.getEmployeeOperationBudgetFile(plantId);

            sheet.Name = "BudgetUpload";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCode", 8, ExcelHAlign.HAlignLeft);
            int ColEmpId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "BudgetCode", 8, ExcelHAlign.HAlignLeft);
            int ColBudgetCode = COL;
            COL++;

            endCol = COL;
            #endregion Headers
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            //for (int i = 0; i < data.Rows.Count; i++)
            //{
            //    sheet[ROW, ColBudgetCode].Text = data.Rows[i]["BudgetCode"].ToString();
            //    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            //    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            //    ROW++;

            //}
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion

        #region IMporting the File
        [HttpPost, Authorize]
        public ActionResult ImportData()
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                //string plantId = Request.Files["plantId"].ToString();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = ReadData(path);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<Dictionary<string, string>> ReadData(string path)
        {
            DataSet dsExcel = null;
            try
            {
                List<eobud> data = new List<eobud>();
                List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
                ReadFile(path, out dsExcel);
                DataTable dtId = GetBudgets();
                DataTable dtEd = GetEmployees();
                data = dsExcel.Tables[0].ToList<eobud>();


                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {


                        dtId.DefaultView.RowFilter = @"BudgetCode='" + data[i].BudgetCode + "'";
                        dtEd.DefaultView.RowFilter = @"EmployeeCode='" + data[i].EmployeeCode + "'";
                        if (dtEd.DefaultView.Count > 0)
                        {
                            Dictionary<string, string> jj = new Dictionary<string, string>();
                            jj.Add("BudgetId", dtId.DefaultView[0]["BudgetId"].ToString());
                            jj.Add("BudgetCode", dtId.DefaultView[0]["BudgetCode"].ToString());
                            jj.Add("EmployeeId", dtEd.DefaultView[0]["EmployeeId"].ToString());
                            jj.Add("EmployeeCode", dtEd.DefaultView[0]["EmployeeCode"].ToString());
                            ret.Add(jj);
                        }
                        else
                        {
                            //throw new Exception("The Employee Code or the BudgetCode at Line no - " + i + 1 + " doesn't exist!! Please Check Again!!");
                            throw new Exception("The Employee Code '" + data[i].EmployeeCode + "' at Line no - " + i + " doesn't exist!! Please Check Again!!");
                        }

                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);

                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    //exception += "\r\nTrying to delete";
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }


        public void SaveFile(out string path)
        {
            path = "";
            try
            {
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
                    path = Path.Combine(ResourcesPathReader.GetOTManualFile(), file.FileName);
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

        private DataTable GetBudgets()
        {
            var str = @"Select Id as BudgetId , Code as BudgetCode from mst.ManpowerBudget Where Active=1";
            return _sqlRepository.GetDataTable(str);
        }

        private DataTable GetEmployees()
        {
            var str = @"Select SystemId as EmployeeId , EmployeeCode  from dbo.EmployeeInformation  where EmpType<>'Guest' AND EmployeeStatus='Active' ";
            return _sqlRepository.GetDataTable(str);
        }

        public class eobud
        {
            public string BudgetCode { get; set; }
            public string EmployeeCode { get; set; }

        }
        #endregion

        #region Saving

        [HttpPost]
        public ActionResult SaveFileList(List<Dictionary<string, string>> data)
        {
            try
            {
                SaveFileListAll(data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        private void SaveFileListAll(List<Dictionary<string, string>> data)
        {
            try
            {

                string empsList = "''";
                for (int i = 0; i < data.Count; i++)
                {
                    empsList += ",'" + data[i]["EmployeeId"] + "'";
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.EmployeeInformation";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SystemId in (" + empsList + ")", out dsMaster, false, "1");

                for (int i = 0; i < data.Count; i++)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"SystemId='" + data[i]["EmployeeId"] + "'";

                    dsMaster.Tables[0].DefaultView[0].Row.BeginEdit();
                    dsMaster.Tables[0].DefaultView[0]["BudgetCode"] = data[i]["BudgetId"];
                    dsMaster.Tables[0].DefaultView[0]["UpdatedBy"] = identity.Name;
                    dsMaster.Tables[0].DefaultView[0]["DateUpdated"] = System.DateTime.Now.ToString();
                    //dsMaster.Tables[0].DefaultView[0]["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].DefaultView[0].Row.EndEdit();
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
        #endregion

    }
}