#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Model.Enums;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Payrolls.SalaryProcess;
using Library.Service.Payrolls.SalaryStructure;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class ExternalDataUploadFromExcelController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly clsTemplateDownloadExternalData _AttendanceManagementService;
        private readonly clsExternalDataUpload _SalaryStructureUploadService;

        public ExternalDataUploadFromExcelController(
               ISqlRepository sqlRepository,
               clsTemplateDownloadExternalData AttendanceManagementService,
               clsExternalDataUpload SalaryStructureUploadService

            )
        {

            _sqlRepository = sqlRepository;
            _AttendanceManagementService = AttendanceManagementService;
            _SalaryStructureUploadService = SalaryStructureUploadService;

        }
        #endregion


        public ActionResult Aplos()
        {
            return View();
        }
        [HttpGet, Authorize]
        public ActionResult LoadData(string SalaryHeadId, string MonthNo, string YearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Sql = string.Empty;


            if (string.IsNullOrEmpty(SalaryHeadId) || SalaryHeadId == "null" || SalaryHeadId == "undefined")
            {
                Sql = @"SELECT EI.SystemId EmpSystemId,EI.PlantId,PLN.UserName AS PlantName,sl.isLocked,EI.EmployeeCode,FORMAT(EI.DOJ,'dd-MMM-yyyy') AS DOJ,FORMAT(EI.DOS,'dd-MMM-yyyy') AS DOS,EI.EmployeeStatus,EI.EmployeeName, sh.SalaryHead,sh.HeadType,c.Name Currency, d.* from dbo.MonthWiseExtraSalaryAmtChild d
                        LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster m on m.SystemID=d.MWESAMasterSystemID
                        Left join EmployeeInformation EI on EI.SystemId=m.EmpInfoSystemID
                        LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=ei.SystemId AND  sl.YearNo=" + YearNo + @" AND sl.MonthNo=" + MonthNo + @"
                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=d.SalaryHeadID
                        LEFT JOIN  SCS.Currency c on c.id=d.EntryCurrencyID
                        left join org.Plant PLN ON PLN.Id=EI.PlantId
                        WHERE m.monthNo=" + MonthNo + @" and m.YearNo=" + YearNo + @" and m.PlantID='" + identity.PlantId + @"' and d.ExtDataUploadApp='XL'
                        ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric ";
            }
            else
            {
                Sql = @"SELECT EI.SystemId EmpSystemId,EI.PlantId,PLN.UserName AS PlantName,sl.isLocked,EI.EmployeeCode,FORMAT(EI.DOJ,'dd-MMM-yyyy') AS DOJ,FORMAT(EI.DOS,'dd-MMM-yyyy') AS DOS,EI.EmployeeStatus,EI.EmployeeName, sh.SalaryHead,sh.HeadType,c.Name Currency, d.* from dbo.MonthWiseExtraSalaryAmtChild d
                        LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster m on m.SystemID=d.MWESAMasterSystemID
                        Left join EmployeeInformation EI on EI.SystemId=m.EmpInfoSystemID
                        LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=ei.SystemId AND  sl.YearNo=" + YearNo + @" AND sl.MonthNo=" + MonthNo + @"
                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=d.SalaryHeadID
                        LEFT JOIN  SCS.Currency c on c.id=d.EntryCurrencyID
                        left join org.Plant PLN ON PLN.Id=EI.PlantId
                        WHERE m.monthNo=" + MonthNo + @" and m.YearNo=" + YearNo + @" and m.PlantID='" + identity.PlantId + @"'
                        and d.SalaryHeadID='" + SalaryHeadId + @"' and d.ExtDataUploadApp='XL'
                        ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric ";





            }

            //_sqlRepository.GetDataCollection(Sql);

            JsonResult json = Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryLock(string EmpSystemId, string MonthNo, string YearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select IsLocked from SalaryLock where EmpSystemId='" + EmpSystemId + "' and YearNo='" + YearNo + "' and MonthNo='" + MonthNo + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadListeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID as Id,SalaryHead+' ['+HeadType+']' as UserName 
                            from [dbo].[SalaryHead]  WHERE ExtDataUpload=1 
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                string pForeignCurRate = "1";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsEmpExtraSalaryAmt objEmpExtAmt = null;
                List<ExternalDataUploadVM> data = new List<ExternalDataUploadVM>();

                //var settings = new JsonSerializerSettings
                //{
                //    NullValueHandling = NullValueHandling.Ignore,
                //    MissingMemberHandling = MissingMemberHandling.Ignore
                //};
                ////var model = JsonConvert.DeserializeObject<PurchaseLC>(form["model"], settings);





                var pre = form["modelNew"];
                var file = Request.Files["file"];


                var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(pre);
                string pSalaryHeadId = getData<string>("SalaryHeadId", _objects);
                string pYearNo = getData<string>("YearNo", _objects);
                string pMonthNo = getData<string>("MonthNo", _objects);


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
                        objEmpExtAmt = new clsEmpExtraSalaryAmt();

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



                        var duplicatedRowsExist = dt.AsEnumerable().GroupBy(x => x[0]).Where(x => x.Count() > 1);


                        int YearNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pYearNo));
                        int MonthNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pMonthNo));
                        //by monir
                        //Delete All Data
                        //clsEmpExtraSalaryAmt objEmpExtAmt = new clsEmpExtraSalaryAmt();
                        //objEmpExtAmt.DeleteOldExtraData(YearNo, MonthNo, identity.PlantId, pSalaryHeadId);

                        objEmpExtAmt.LoadExternalUploadFromExcelOnGrid(identity.PlantId, pSalaryHeadId, YearNo, MonthNo, out dsEmpInfo);
                        dtEmpInfo = dsEmpInfo.Tables[0];
                        dvEmpInfo = new DataView();
                        dvEmpInfo.Table = dtEmpInfo;

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                string strTempEntryAmt = "0.0";
                                string strTempDefineAmt = "0.0";
                                string _empCode = Regex.Replace(dsExcel.Tables[0].Rows[i][0].ToString().Trim(), @"\s", "");
                                //string _empCode = dsExcel.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();
                                strTempEntryAmt = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["Amount"].ToString().Trim();
                                //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["F2"].ToString().Trim();
                                if (_empCode.Trim().Length > 0 && strTempDefineAmt.Trim().Length > 0)
                                {

                                    dvEmpInfo.RowFilter = "EmployeeCode = '" + _empCode + "'";
                                    //if (dvEmpInfo.Count == 1)
                                    if (dvEmpInfo.Count > 0)
                                    {
                                        if (bplib.clsWebLib.GetBoolData(dvEmpInfo[0]["isSalaryLocked"].ToString()) == false)
                                        {
                                            if (dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim() == dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim())
                                            {
                                                strTempDefineAmt = strTempEntryAmt;
                                            }
                                            else
                                            {
                                                strTempDefineAmt = (Convert.ToDecimal(strTempEntryAmt) / Convert.ToDecimal(pForeignCurRate)).ToString("#,##0.0000;(#,##0.0000)");
                                            }

                                            ExternalDataUploadVM vm = new ExternalDataUploadVM();

                                            vm.MWESAMasterSystemID = dvEmpInfo[0]["MWESAMasterSystemID"].ToString().Trim();
                                            vm.MWESAChildSystemID = dvEmpInfo[0]["MWESAChildSystemID"].ToString().Trim();
                                            vm.EmpInfoSystemID = dvEmpInfo[0]["EmpInfoSystemID"].ToString().Trim();
                                            vm.PlantId = dvEmpInfo[0]["PlantId"].ToString().Trim();
                                            vm.PlantName = dvEmpInfo[0]["PlantName"].ToString().Trim();
                                            vm.EmployeeCode = dvEmpInfo[0]["EmployeeCode"].ToString().Trim();
                                            vm.DOJ = dvEmpInfo[0]["DOJ"].ToString().Trim();
                                            vm.DOS = dvEmpInfo[0]["DOS"].ToString().Trim();
                                            vm.EmployeeStatus = dvEmpInfo[0]["EmployeeStatus"].ToString().Trim();
                                            vm.EmployeeName = dvEmpInfo[0]["EmployeeName"].ToString().Trim();
                                            vm.CurrencyRuleSystemID = dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim();
                                            vm.SalaryHeadID = dvEmpInfo[0]["SalaryHeadID"].ToString().Trim();
                                            vm.SalaryHead = dvEmpInfo[0]["SalaryHead"].ToString().Trim();
                                            vm.HeadType = dvEmpInfo[0]["HeadType"].ToString().Trim();
                                            vm.ExistCurrencyID = dvEmpInfo[0]["ExistCurrencyID"].ToString().Trim();
                                            vm.ExistCurrency = dvEmpInfo[0]["ExistCurrency"].ToString().Trim();
                                            vm.ExistAmount = dvEmpInfo[0]["ExistAmount"].ToString().Trim();
                                            vm.EntryCurrencyID = dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim();
                                            vm.EntryCurrency = dvEmpInfo[0]["EntryCurrency"].ToString().Trim();
                                            vm.EntryAmount = strTempEntryAmt.Trim();
                                            vm.DefinitionCurrencyID = dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim();
                                            vm.DefinitionCurrency = dvEmpInfo[0]["DefinitionCurrency"].ToString().Trim();
                                            vm.DefineAmount = strTempDefineAmt.Trim();
                                            vm.AmtDefinationCurrencyID = dvEmpInfo[0]["AmtDefinationCurrencyID"].ToString().Trim();
                                            vm.AmtDefinationRate = dvEmpInfo[0]["AmtDefinationRate"].ToString().Trim();

                                            bool IsDuplicate = false;
                                            foreach (var item in duplicatedRowsExist)
                                            {
                                                if (item.Key.ToString() == _empCode)
                                                {
                                                    IsDuplicate = true;
                                                }
                                            }
                                            if (IsDuplicate)
                                            {
                                                vm.Remarks = "This Employee is Found Multipule Times";
                                            }
                                            else
                                            {
                                                vm.Remarks = "";
                                            }


                                            data.Add(vm);
                                        }
                                        else
                                        {
                                            ExternalDataUploadVM vm = new ExternalDataUploadVM();
                                            vm.MWESAMasterSystemID = dvEmpInfo[0]["MWESAMasterSystemID"].ToString().Trim();
                                            vm.MWESAChildSystemID = dvEmpInfo[0]["MWESAChildSystemID"].ToString().Trim();
                                            vm.EmpInfoSystemID = dvEmpInfo[0]["EmpInfoSystemID"].ToString().Trim();
                                            vm.PlantId = dvEmpInfo[0]["PlantId"].ToString().Trim();
                                            vm.PlantName = dvEmpInfo[0]["PlantName"].ToString().Trim();
                                            vm.EmployeeCode = dvEmpInfo[0]["EmployeeCode"].ToString().Trim();
                                            vm.DOJ = dvEmpInfo[0]["DOJ"].ToString().Trim();
                                            vm.DOS = dvEmpInfo[0]["DOS"].ToString().Trim();
                                            vm.EmployeeStatus = dvEmpInfo[0]["EmployeeStatus"].ToString().Trim();
                                            vm.EmployeeName = dvEmpInfo[0]["EmployeeName"].ToString().Trim();
                                            vm.CurrencyRuleSystemID = dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim();
                                            vm.SalaryHeadID = dvEmpInfo[0]["SalaryHeadID"].ToString().Trim();
                                            vm.SalaryHead = dvEmpInfo[0]["SalaryHead"].ToString().Trim();
                                            vm.HeadType = dvEmpInfo[0]["HeadType"].ToString().Trim();
                                            vm.ExistCurrencyID = dvEmpInfo[0]["ExistCurrencyID"].ToString().Trim();
                                            vm.ExistCurrency = dvEmpInfo[0]["ExistCurrency"].ToString().Trim();
                                            vm.ExistAmount = dvEmpInfo[0]["ExistAmount"].ToString().Trim();
                                            vm.EntryCurrencyID = dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim();
                                            vm.EntryCurrency = dvEmpInfo[0]["EntryCurrency"].ToString().Trim();
                                            vm.EntryAmount = strTempEntryAmt.Trim();
                                            vm.DefinitionCurrencyID = dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim();
                                            vm.DefinitionCurrency = dvEmpInfo[0]["DefinitionCurrency"].ToString().Trim();
                                            vm.DefineAmount = strTempDefineAmt.Trim();
                                            vm.AmtDefinationCurrencyID = dvEmpInfo[0]["AmtDefinationCurrencyID"].ToString().Trim();
                                            vm.AmtDefinationRate = dvEmpInfo[0]["AmtDefinationRate"].ToString().Trim();
                                            vm.Remarks = "Salary has been locked";
                                            data.Add(vm);

                                        }
                                    }
                                    else
                                    {
                                        ExternalDataUploadVM vm = new ExternalDataUploadVM();
                                        vm.MWESAMasterSystemID = "";
                                        vm.MWESAChildSystemID = "";
                                        vm.EmpInfoSystemID = "";
                                        vm.EmployeeCode = _empCode;
                                        vm.EmployeeName = "";
                                        vm.CurrencyRuleSystemID = "";
                                        vm.SalaryHeadID = "";
                                        vm.SalaryHead = "";
                                        vm.HeadType = "";
                                        vm.ExistCurrencyID = "";
                                        vm.ExistCurrency = "";
                                        vm.ExistAmount = "";
                                        vm.EntryCurrencyID = "";
                                        vm.EntryCurrency = "";
                                        vm.EntryAmount = strTempEntryAmt.Trim();
                                        vm.DefinitionCurrencyID = "";
                                        vm.DefinitionCurrency = "";
                                        vm.DefineAmount = "0";
                                        vm.AmtDefinationCurrencyID = "";
                                        vm.AmtDefinationRate = "";
                                        vm.Remarks = "Employee code has not matched with the system database.";
                                        data.Add(vm);
                                    }
                                }
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

        private T getData<T>(string st, Dictionary<string, object> ob)
        {
            var fabricRoll = ob[st];
            var json = JsonConvert.SerializeObject(fabricRoll);
            var fob = JsonConvert.DeserializeObject<T>(json);
            return fob;
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
        public ActionResult SaveExternalData(List<ExternalDataUploadVM> data, string YearNo, string MonthNo, string SalaryHeadId)
        {
            if (data == null)
            {
                throw new Exception("No data found for upload !!!!!");
            }
            string LockedempCodes = "";
            _SalaryStructureUploadService.SaveData(YearNo, MonthNo, SalaryHeadId, data, (CustomIdentity)Thread.CurrentPrincipal.Identity, out LockedempCodes);

            if (LockedempCodes == "")
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { Error = true, Message = "Could not update following employees (Salary is locked):" + LockedempCodes }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = _AttendanceManagementService.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "External Data upload Sample File";
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

        #region Report 

        [HttpPost, Authorize]
        public JsonResult ExternalDataUploadReport(string EmployeeList, string SalaryHeadId, string MonthNo, string YearNo, string SalaryHeadIDs, string HeadType, string CurrencyID, string EntryAmount, string MonthName)
        {
            try
            {
                string fileName = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                fileName = ExternalDataUploadFromExcelReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "External Data Upload", "", EmployeeList, SalaryHeadId, MonthNo, YearNo, SalaryHeadIDs, HeadType, CurrencyID, EntryAmount, MonthName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string ExternalDataUploadFromExcelReport(string CGId, string CompanyId, string PlantId, string SheetName1, string s1, string EmployeeList, string SalaryHeadId, string MonthNo, string YearNo, string SalaryHeadIDs, string HeadType, string CurrencyID, string EntryAmount, string MonthName)
        {
            #region Variable

            clsReport objRpt = null;
            clsReport objRptD = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHeading = null;
            string yot = string.Empty;//OTConsiderOn
            string tot = string.Empty;//OTConsiderOn
            DataSet dsAttn = null;
            DataSet dsEmp = null;
            DataView dvAttn = null;
            DataView dvEmp = null;

            DataSet dsFactory = null;
            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";
            var report = new ReportUtility();

            DataSet dsExtraAbsent = null;
            DataView dvExtraAbsent = null;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyId = identity.CompanyId;
                objRpt = new clsReport();

                var ob = new clsStaticInfo();



                #region DataSet
                getEmployee(CGId, CompanyId, PlantId, EmployeeList, SalaryHeadId, MonthNo, YearNo, SalaryHeadIDs, HeadType, CurrencyID, EntryAmount, out dsAttn);

                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    string strSubSec = "0";
                    string strSec = "0";
                    string strUnit = "0";
                    int strCount = 0;
                    string strLateBy = "00:00:00";

                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sl No.";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8.50;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Section";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sub Section";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Category";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Salary Head";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Head Type";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Currency";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Entry Amount";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    #endregion ------------------Column Header------------------
                    strCount = 0;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {
                        xlsCol = 1;

                        xlsRow += intRow;
                        intRow = 1;


                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;

                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Department"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Designation"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;

                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Section"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubSection"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeCategory"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SalaryHead"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["HeadType"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Currency"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvAttn[i]["EntryAmount"]);
                        sheet1.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        // xlsRow += 1;

                        #endregion ----------------------Data-----------------------


                    }

                    #region Line Setup
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].WrapText = true;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = CmpName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "External Data Upload From Excel Report";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Report of Month:- " + MonthName + ", Year:- " + YearNo;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 9;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    //sheet1.PageSetup.PrintTitleRows = "$1:$2";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId.Trim() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "ExternalDataUpload";
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    // return workbook;

                    var filePath = "";
                    var SheetName = "";
                    //return workbook;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }
        public void getEmployee(string companyGroupId, string companyId, string plantId, string EmployeeList, string SalaryHeadId, string MonthNo, string YearNo, string SalaryHeadIDs, string HeadType, string CurrencyID, string EntryAmount, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string SalayHead = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(SalaryHeadId) || SalaryHeadId == "null" || SalaryHeadId == "undefined")
                {

                }
                else
                {
                    SalayHead = " and d.SalaryHeadID in ('"+ SalaryHeadId + "') ";
                }
                strSql = @"SELECT EI.SystemId,EI.EmployeeCode,EI.EmployeeName, sh.SalaryHead,sh.HeadType,c.Name Currency, d.EntryAmount
                                ,dep.username Department, LG.UserName Designation,L.UserName Line,SS.UserName SubSection,s.UserName Section
                                ,ec.UserName EmployeeCategory
						            from dbo.MonthWiseExtraSalaryAmtChild d
                                            LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster m on m.SystemID=d.MWESAMasterSystemID
                                            Left join EmployeeInformation EI on EI.SystemId=m.EmpInfoSystemID
                                            LEFT JOIN SalaryHead sh on sh.SalaryHeadID=d.SalaryHeadID
                                            LEFT JOIN  SCS.Currency c on c.id=d.EntryCurrencyID
						                    left join mst.ManpowerBudget mp on mp.id=ei.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId                                       
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON EI.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
                        WHERE m.monthNo=" + MonthNo + " and m.YearNo=" + YearNo + " and m.PlantID='" + plantId + @"' and d.ExtDataUploadApp='XL'
                        and Ei.SystemId in (" + EmployeeList + @") " + SalayHead + @"
                        and sh.HeadType in (" + HeadType + ") and d.SalaryHeadID in (" + SalaryHeadIDs + ") and d.EntryAmount in (" + EntryAmount + ") and d.EntryCurrencyID in (" + CurrencyID + @")
                        ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric ";




                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        #endregion

        #region Update
        [HttpPost, Authorize]
        public JsonResult UpdateUpload(ExternalUpload ExternalUploadUpdate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsUpdate;
                DataView _dvSave = null;
                ConnectionManager.DAL.ConManager objCon;
                string sql = "SELECT * FROM [dbo].[MonthWiseExtraSalaryAmtChild] WHERE SystemId='" + ExternalUploadUpdate.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsUpdate, false, "1");
                _dvSave = new DataView(dsUpdate.Tables[0]);
                _dvSave.RowFilter = "SystemId ='" + ExternalUploadUpdate.Id + "'";
                if (_dvSave.Count > 0)
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    dr["EntryAmount"] = ExternalUploadUpdate.Amount;
                    dr["DefineAmount"] = ExternalUploadUpdate.Amount;
                    dr.EndEdit();
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsUpdate);
                return Json(new { Error = false, Data = ExternalUploadUpdate, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion
    }
    public class ExternalUpload
    {
        public string Id { get; set; }
        public string EmpCode { get; set; }
        public string SalaryHead { get; set; }
        public string EmpName { get; set; }
        public string Amount { get; set; }
    }

}
