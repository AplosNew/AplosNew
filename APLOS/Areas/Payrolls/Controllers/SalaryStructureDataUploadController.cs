#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
using System.IO;
using Library.Data;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System.Data.OleDb;
using Syncfusion.XlsIO;
using System.Text.RegularExpressions;
using System.Globalization;
using Library.Model.Enums;
using Library.Service.HumanResources;
using Library.Service.Payrolls.SalaryStructure;
#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryStructureDataUploadController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly clsTemplateDownloadSalaryStructure _AttendanceManagementService;
        private readonly clsSalaryStructureUpload _SalaryStructureUploadService;
        
        public SalaryStructureDataUploadController(
               ISqlRepository sqlRepository,
               clsTemplateDownloadSalaryStructure AttendanceManagementService,
               clsSalaryStructureUpload SalaryStructureUploadService

            )
        {

            _sqlRepository = sqlRepository;
            _AttendanceManagementService = AttendanceManagementService;
            _SalaryStructureUploadService = SalaryStructureUploadService;

        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                List<SalaryStructureUploadVM> data = new List<SalaryStructureUploadVM>();

                var pre = form["modelNew"]; 
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                        //cost.FileName = extension;
                        //if (!string.IsNullOrEmpty(cost.FileName))
                        //    cost.FileName = cost.Id.ToString() + cost.FileName;
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, /*cost.FileName*/file.FileName);
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




                FileInfo docFile;

                DataSet dsEmpInfo = null;
                DataTable dtEmpInfo = null;
                DataView dvEmpInfo = null;

               


                string exception = "\r\n";
                try
                {
                    //string path = Server.MapPath("TempExcelFile") + "/" + FileUpload1.FileName; exception += "\r\n" + path;
                    try
                    {

                        
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }





                        GetEmployeeInfo(out dsEmpInfo);
                        dtEmpInfo = dsEmpInfo.Tables[0];
                        dvEmpInfo = new DataView();

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                               
                                string SalaryRuleMasterSystemID = "";
                                string EffectiveDate = "";
                                string NextDueDate = "";
                                string SalaryHeadID = "";
                                string EntryAmount = "";
                             
                                string _empEmpSystemId = Regex.Replace(dsExcel.Tables[0].Rows[i][0].ToString().Trim(), @"\s", "");
                                //string _empCode = dsExcel.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();

                                SalaryRuleMasterSystemID = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                EffectiveDate = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                NextDueDate = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                SalaryHeadID = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                EntryAmount = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["Amount"].ToString().Trim();
                                //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["F2"].ToString().Trim();


                                //DateTime dtPDate;

                                //bool isValidDate = DateTime.TryParseExact(
                                //    strTempPDate,
                                //    "dd-MMM-yyyy",
                                //    CultureInfo.InvariantCulture,
                                //    DateTimeStyles.None,
                                //    out dtPDate);
                                //DateTime dtPTime;

                                //bool isValidDateTime = DateTime.TryParseExact(
                                //    strTempPTimee,
                                //    "dd-MMM-yyyy hh:mm:ss",
                                //    CultureInfo.InvariantCulture,
                                //    DateTimeStyles.None,
                                //    out dtPTime);

                                if (_empEmpSystemId.Trim().Length > 0 )
                                {
                                    dvEmpInfo.Table = dtEmpInfo;
                                    dvEmpInfo.RowFilter = "SystemId = " + _empEmpSystemId;
                                    //if (dvEmpInfo.Count == 1)
                                   

                                    if (dvEmpInfo.Count > 0 
                                        && !string.IsNullOrEmpty(SalaryRuleMasterSystemID)
                                        && !string.IsNullOrEmpty(EffectiveDate)
                                        && !string.IsNullOrEmpty(NextDueDate)
                                        && !string.IsNullOrEmpty(SalaryHeadID)
                                        && !string.IsNullOrEmpty(EntryAmount)
                                        )
                                    {
                                       
                                       
                                        SalaryStructureUploadVM vm = new SalaryStructureUploadVM();

                                        vm.EmpSystemID = _empEmpSystemId.ToString().Trim();
                                        vm.SalaryRuleMasterSystemID = SalaryRuleMasterSystemID.Trim();
                                        vm.EffectiveDate = EffectiveDate.Trim();
                                        vm.NextDueDate = NextDueDate.Trim();
                                        vm.SalaryHeadID = SalaryHeadID.Trim();
                                        vm.EntryAmount = EntryAmount.Trim();
                                        vm.Remarks = "";
                                        data.Add(vm);
                                      
                                        
                                    }
                                    else
                                    {

                                        SalaryStructureUploadVM vm = new SalaryStructureUploadVM();
                                        vm.EmpSystemID = _empEmpSystemId.ToString().Trim();
                                        vm.SalaryRuleMasterSystemID = SalaryRuleMasterSystemID.Trim();
                                        vm.EffectiveDate = EffectiveDate.Trim();
                                        vm.NextDueDate = NextDueDate.Trim();
                                        vm.SalaryHeadID = SalaryHeadID.Trim();
                                        vm.EntryAmount = EntryAmount.Trim();
                                        vm.Remarks = "Some required data are missing.";


                                        data.Add(vm);
                                    }
                                }//blank checking


                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
                        }
                        // }
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
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }








                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;

                //return Json(new { Error = false, data , Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
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

        public void GetEmployeeInfo(out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT SystemId, EmployeeCode, PlantId, DOJ, DOS  FROM EmployeeInformation WHERE PlantId = '" + identity.PlantId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//



        [HttpPost]
        public ActionResult SaveAttendanceRawData(List<SalaryStructureUploadVM> data)
        {
            //List<SalaryStructureUploadVM> data = new List<SalaryStructureUploadVM>();
            //SalaryStructureUploadVM o = new SalaryStructureUploadVM();
            //o.EmpSystemID = "1";
            //o.SalaryHeadID = "A";
            //o.EntryAmount = "2000";
            //data.Add(o);


            //SalaryStructureUploadVM o1 = new SalaryStructureUploadVM();
            //o1.EmpSystemID = "1";
            //o1.SalaryHeadID = "B";
            //o1.EntryAmount = "5000";
            //data.Add(o1);

            //SalaryStructureUploadVM o2 = new SalaryStructureUploadVM();
            //o2.EmpSystemID = "2";
            //o2.SalaryHeadID = "B";
            //o2.EntryAmount = "5000";
            //data.Add(o2);
            _SalaryStructureUploadService.SalaryStructureUpload(data, (CustomIdentity)Thread.CurrentPrincipal.Identity);
            


            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetSampleFile()
        //{
        //    try
        //    {
        //        CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        clsReport objRpt = null;
        //        DataSet dsCmp = null;
        //        DataSet dsFactory = null;
        //        ExcelEngine excelEngine = null;
        //        IApplication application = null;
        //        IWorkbook workbook = null;
        //        IWorksheet sheet1 = null;
        //        var formulaStartRow = 0;
        //        var endXlsRow = 0;
        //        int xlsRow = 1, xlsCol = 1;
        //        int endXlsCol = 1;
        //        DataTable dtEmpInfo = new DataTable();
        //        dtEmpInfo = null;

        //        //get ds

        //        objRpt = new clsReport();

        //        //get ds

        //        var colSrNo = 0;
        //        var colPaycode = 0;
        //        var colPFUANNo = 0;
        //        var colEmployeeName = 0;
        //        var colDays = 0;
        //        var colWagesAmount = 0;
        //        var colEmployeeShare12parcent = 0;
        //        var colVPF = 0;
        //        var col3point67parcent = 0;
        //        var colFPFEmployersShare8point33percent = 0;
        //        var colTotal = 0;
        //        var colWAGES8point33percent = 0;
        //        var colWagesAbove15000 = 0;
        //        var colRemarksDOL = 0;
        //        var colAge = 0;
        //        var slCount = 0;


        //        excelEngine = new ExcelEngine();
        //        application = excelEngine.Excel;
        //        workbook = application.Workbooks.Create(1);

        //        sheet1 = workbook.Worksheets[0];
        //        sheet1.IsGridLinesVisible = true;

        //        xlsRow = 1;

        //        //	#region ------------------Column Header------------------

        //        var ru = new ReportUtility();

        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "S.No", 4, 25, ExcelHAlign.HAlignCenter); colSrNo = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "EmpCode", 8, 25, ExcelHAlign.HAlignCenter); colPaycode = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "PF UAN No.", 12, 25, ExcelHAlign.HAlignCenter); colPFUANNo = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Name of the Employee", 21, 25, ExcelHAlign.HAlignCenter); colEmployeeName = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Age", 5, 25, ExcelHAlign.HAlignCenter); colAge = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Days", 5, 25, ExcelHAlign.HAlignCenter); colDays = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Wages Amount", 8, 25, ExcelHAlign.HAlignCenter); colWagesAmount = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Employee's Share 12%", 10, 25, ExcelHAlign.HAlignCenter); colEmployeeShare12parcent = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "VPF", 5, 25, ExcelHAlign.HAlignRight); colVPF = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "3.67%", 8, 25, ExcelHAlign.HAlignRight); col3point67parcent = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "FPF Employers Share 8.33%", 9, 25, ExcelHAlign.HAlignRight); colFPFEmployersShare8point33percent = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Total", 8, 25, ExcelHAlign.HAlignRight); colTotal = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "EDLI Amt.", 9, 25, ExcelHAlign.HAlignRight); colWAGES8point33percent = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "WAGES ABOVE 15000", 9, 25, ExcelHAlign.HAlignRight); colWagesAbove15000 = xlsCol; xlsCol++;
        //            SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Remarks DOL", 9, 25, ExcelHAlign.HAlignRight); colRemarksDOL = xlsCol;
        //            endXlsCol = xlsCol;
        //            //xlsRow++;

        //        #region UsedRange Alignment

        //        sheet1.UsedRange.WrapText = true;
        //        sheet1.UsedRange.CellStyle.Font.Size = 8;
        //        sheet1.Range["A1"].CellStyle.Font.Size = 14;
        //        sheet1.Range["A2"].CellStyle.Font.Size = 10;
        //        sheet1.Range[1, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
        //        sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

        //        #endregion UsedRange Alignment

        //        #region Page Setup	
        //        sheet1.PageSetup.PrintTitleRows = "$A$6:$IV$6";
        //        sheet1.PageSetup.TopMargin = 0.5;
        //        sheet1.PageSetup.BottomMargin = 0.7;
        //        sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
        //        sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
        //        sheet1.PageSetup.LeftMargin = 0.5;
        //        sheet1.PageSetup.RightMargin = 0.2;
        //        sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
        //        sheet1.PageSetup.FitToPagesTall = 0;
        //        sheet1.PageSetup.FitToPagesWide = 1;
        //        sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
        //        #endregion Page Setup

        //        workbook.Version = ExcelVersion.Excel2013;
        //        string fileName =  "Provident Fund Statement" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
        //        string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
        //        workbook.SaveAs(fullPath);
        //        return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
        //        // throw ex;
        //    }
        //}

        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = _AttendanceManagementService.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Salary Structure Data upload Sample File";
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



    }

    
}