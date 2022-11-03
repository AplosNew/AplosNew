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
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceRawDataUploadController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;

        public AttendanceRawDataUploadController(
               ISqlRepository sqlRepository,
               IAttendanceManagementService AttendanceManagementService

            )
        {

            _sqlRepository = sqlRepository;
            _AttendanceManagementService = AttendanceManagementService;

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
                List<AttendanceRawDataUploadVM> data = new List<AttendanceRawDataUploadVM>();

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
                                string strTempPDate = "";
                                string strTempPTimee = "";
                                string strTempPType = "";
                                //string strTempDefineAmt = "0.0";
                                string _empEmpSystemId = Regex.Replace(dsExcel.Tables[0].Rows[i][0].ToString().Trim(), @"\s", "");
                                //string _empCode = dsExcel.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();
                               
                                strTempPDate = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                strTempPTimee = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                strTempPType = dsExcel.Tables[0].Rows[i][3].ToString().Trim().ToUpper();
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

                                if (_empEmpSystemId.Trim().Length > 0 && strTempPType.Trim().Length > 0)
                                {
                                    dvEmpInfo.Table = dtEmpInfo;
                                    dvEmpInfo.RowFilter = "SystemId = '" + _empEmpSystemId+"'";
                                    //if (dvEmpInfo.Count == 1)
                                   

                                    if (dvEmpInfo.Count > 0)
                                    {
                                        bool isValid = false;
                                        if (Convert.ToDateTime(strTempPDate) <= DateTime.Now
                                            && Convert.ToDateTime(strTempPDate) >= Convert.ToDateTime(dvEmpInfo[0]["DOJ"].ToString().Trim()))
                                        {
                                            isValid = true;
                                        }
                                        if (!string.IsNullOrEmpty(dvEmpInfo[0]["DOS"].ToString().Trim()))
                                        {
                                            if (Convert.ToDateTime(strTempPDate) <= Convert.ToDateTime(dvEmpInfo[0]["DOS"].ToString().Trim()))
                                            {
                                                isValid = true;
                                            }
                                            else
                                            {
                                                isValid = false;
                                            }
                                        }
                                        AttendanceRawDataUploadVM vm = new AttendanceRawDataUploadVM();
                                        if (isValid)
                                        {
                                            vm.LogDownLoadNum = _empEmpSystemId.ToString().Trim();
                                            //vm.EmployeeCode = dvEmpInfo[0]["EmployeeCode"].ToString().Trim();
                                            vm.PType = strTempPType.Trim();
                                            vm.PDate = strTempPDate.Trim();
                                            vm.PTime = strTempPTimee.Trim();                                            
                                            vm.Remarks = "";
                                            data.Add(vm);
                                        }
                                        else
                                        {
                                            vm.LogDownLoadNum = _empEmpSystemId;
                                            //vm.EmployeeCode = _empEmpSystemId;
                                            vm.PType = strTempPType.Trim();
                                            vm.PDate = strTempPDate.Trim();
                                            vm.PTime = strTempPTimee.Trim();
                                            vm.Remarks = "Invalid work date ";
                                            data.Add(vm);
                                        }
                                        
                                    }
                                    else
                                    {
                                        
                                        AttendanceRawDataUploadVM vm = new AttendanceRawDataUploadVM();
                                        vm.LogDownLoadNum = _empEmpSystemId;                                        
                                        //vm.EmployeeCode = _empEmpSystemId;
                                        vm.PType = strTempPType.Trim();
                                        vm.PDate = strTempPDate.Trim();
                                        vm.PTime = strTempPTimee.Trim();
                                        vm.Remarks = "Employee code has not matched with the system database.";
                                        //if (dvEmpInfo.Count == 0)
                                        //{
                                        //    vm.Remarks = "Employee code has not matched with the system database.";
                                        //}
                                        //else if(isValidDate == false)
                                        //{
                                        //    vm.Remarks = "Date format is not valid.";
                                        //}
                                        //else if (isValidDateTime == false)
                                        //{
                                        //    vm.Remarks = "Date-time format is not valid.";
                                        //}

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
        public ActionResult SaveAttendanceRawData(List<AttendanceRawDataUploadVM> AttendanceRawData)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string DeviceId = "";
            string EmpSytemId = "";
          
            DataSet dsGetdataRef = null;
            DataSet dsGetdataDeviceRef = null;
          
            string strSQL1;
           
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //for (int i = 0; i < AttendanceRawData.Count; i++)
                //{
                //    if (AttendanceRawDataId == "")
                //        AttendanceRawDataId = "'" + AttendanceRawData[i].Id.ToString() + "'";
                //    else
                //        AttendanceRawDataId = AttendanceRawDataId + ",'" + AttendanceRawData[i].Id.ToString() + "'";
                //}

                //for (int i = 0; i < AttendanceRawData.Count; i++)
                //{
                //    if (EmpSytemId == "")
                //        EmpSytemId = "'" + AttendanceRawData[i].SystemId.ToString() + "'";
                //    //else
                //    //    EmpSytemId = EmpSytemId + ",'" + AttendanceRawData[i].SystemId.ToString() + "'";
                //}
                //clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                //DateTime FromDate = Convert.ToDateTime(pFromDate);
                //DateTime ToDate = Convert.ToDateTime(pToDate);
                //try
                //{

                //    if (EmpSytemId != "")
                //    {
                //        obj.LockValidation(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);
                //    }
                //}
                //catch (Exception ex)
                //{

                //    throw ex;
                //}


                strSQL1 = @"SELECT * FROM AttdnRawData WHERE Id IN ('') AND PlantID='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL1, out dsGetdataRef, false, "1");



               string strSQL = @"SELECT Id FROM mst.AccessControllerList WHERE PlantId='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsGetdataDeviceRef, false, "1");
                if (dsGetdataDeviceRef.Tables[0].Rows.Count>0)
                {
                    DeviceId = dsGetdataDeviceRef.Tables[0].Rows[0]["Id"].ToString();
                }
                else
                {
                    Exception ex = new Exception("No Access Controller List Found...");
                    throw (ex);
                }



                bplib.clsGenID objGenID = new bplib.clsGenID();
                string sID = string.Empty;
                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AttdnRawDataUpload", out sID);


                DataView dvSaveSummary = new DataView(dsGetdataRef.Tables[0]);
                for (int i = 0; i < AttendanceRawData.Count; i++)
                {


                    dvSaveSummary.RowFilter = " Id ='" + AttendanceRawData[i].LogDownLoadNum + "' AND PlantID = '" + identity.PlantId + @"'";

                    if (dvSaveSummary.Count == 0)
                    {
                       
                        //bplib.clsGenID objGenID = new bplib.clsGenID();
                        //objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AttdnRawDataUpload", out sID);
                        DataRow dr = dsGetdataRef.Tables[0].NewRow();
                        dr["Id"] = "AU" + sID+"_"+i;
                        //dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[i]["DeviceID"];
                        dr["DevSystemID"] = DeviceId;
                        dr["LogDownLoadNum"] = AttendanceRawData[i].LogDownLoadNum;
                        dr["PDate"] = AttendanceRawData[i].PDate;

                        TimeSpan time = new TimeSpan(0, Convert.ToDateTime(AttendanceRawData[i].PTime).TimeOfDay.Hours, Convert.ToDateTime(AttendanceRawData[i].PTime).TimeOfDay.Minutes, Convert.ToDateTime(AttendanceRawData[i].PTime).TimeOfDay.Seconds);

                        dr["PTime"] =AttendanceRawData[i].PDate+" " + time;
                        dr["PType"] = AttendanceRawData[i].PType;
                        dr["ProcessedFlag"] = false;
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId.ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = System.DateTime.Now.ToString();
                        //dr["BackupType"] = "RAWDATADELETE";
                        dsGetdataRef.Tables[0].Rows.Add(dr);

                    }
                    //else
                    //{
                    //    DataRow dr = dvSaveSummary[0].Row;
                    //    dr.BeginEdit();
                    //    dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[i]["DeviceID"];
                    //    dr["DevSystemID"] = dsGetdataRef.Tables[0].Rows[i]["DevSystemID"];
                    //    dr["LogDownLoadNum"] = dsGetdataRef.Tables[0].Rows[i]["LogDownLoadNum"];
                    //    dr["PDate"] = dsGetdataRef.Tables[0].Rows[i]["PDate"];
                    //    dr["PTime"] = dsGetdataRef.Tables[0].Rows[i]["PTime"];
                    //    dr["PType"] = dsGetdataRef.Tables[0].Rows[i]["PType"];
                    //    dr["ProcessedFlag"] = dsGetdataRef.Tables[0].Rows[i]["ProcessedFlag"];
                    //    dr["GroupID"] = identity.CompanyGroupId;
                    //    dr["PlantID"] = identity.PlantId.ToString();
                    //    dr["UpdatedBy"] = identity.Name;
                    //    dr["DateUpdated"] = System.DateTime.Now.ToString();
                    //    //dr["BackupType"] = "RAWDATADELETE";
                    //    dr.EndEdit();

                    //}
                    dvSaveSummary.RowFilter = null;
                    //Old year insert 
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsGetdataRef);







                //while (FromDate <= ToDate)
                //{

                //    ReturnType r = obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), EmpSytemId, false);//laila                 
                //    FromDate = FromDate.AddDays(1);
                //}


            }
            catch (Exception ex)
            {
                //throw (ex);
               return Json(new { Message = ex.Message, Error = true}, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objCon = null;
            }





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

        [HttpGet,Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Attendance Data upload Sample File";
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

        public IWorkbook GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                var iName = 0;
                var iEmployeeCode = 0;
                var iOffDuration = 0;
                var iInFoType = 0;
                var iShiftName = 0;
                var iShiftOutTime = 0;
                var iLunchOutTime = 0;
                var iShiftInTime = 0;
                var iLunchInTime = 0;
                var iDOS = 0;
                var iDOJ = 0;
                var iBreakInTime = 0;
                var iBreakOutTime = 0;
                var iPunchInTime = 0;
                var iPunchOutTime = 0;
                var iLateInTolarance = 0;
                var iEarlyOutTole = 0;
                var iLateInApp = 0;
                var iEarlyOutApp = 0;
                var iWorkDate = 0;
                var isl = 0;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                xlsRow = 1;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "EmployeeSystemId";
                sheet1.Range[xlsRow, isl].ColumnWidth = 18;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "WorkDate";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 12;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "PunchTime";
                sheet1.Range[xlsRow, iName].ColumnWidth = 18;

                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "PunchType";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                //xlsCol += 1;
                //iDOJ = xlsCol;
                //sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                //sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                //xlsCol += 1;
                //iDOS = xlsCol;
                //sheet1.Range[xlsRow, iDOS].Text = "DOS";
                //sheet1.Range[xlsRow, iDOS].ColumnWidth = 20;


                //xlsCol += 1;
                //iInFoType = xlsCol;
                //sheet1.Range[xlsRow, iInFoType].Text = "InFo Type";
                //sheet1.Range[xlsRow, iInFoType].ColumnWidth = 14;

                //xlsCol += 1;
                //iOffDuration = xlsCol;
                //sheet1.Range[xlsRow, iOffDuration].Text = "Off Duration";
                //sheet1.Range[xlsRow, iOffDuration].ColumnWidth = 16;

                //xlsCol += 1;
                //iLunchInTime = xlsCol;
                //sheet1.Range[xlsRow, iLunchInTime].Text = "Lunch In Time";
                //sheet1.Range[xlsRow, iLunchInTime].ColumnWidth = 12;

                //xlsCol += 1;
                //iLunchOutTime = xlsCol;
                //sheet1.Range[xlsRow, iLunchOutTime].Text = "Lunch Out Time";
                //sheet1.Range[xlsRow, iLunchOutTime].ColumnWidth = 12;


                //xlsCol += 1;
                //iShiftName = xlsCol;
                //sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                //sheet1.Range[xlsRow, iShiftName].ColumnWidth = 16;

                //xlsCol += 1;
                //iShiftInTime = xlsCol;
                //sheet1.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                //sheet1.Range[xlsRow, iShiftInTime].ColumnWidth = 11;

                //xlsCol += 1;
                //iShiftOutTime = xlsCol;
                //sheet1.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                //sheet1.Range[xlsRow, iShiftOutTime].ColumnWidth = 11;

                //xlsCol += 1;
                //iBreakInTime = xlsCol;
                //sheet1.Range[xlsRow, iBreakInTime].Text = "Break Start Time";
                //sheet1.Range[xlsRow, iBreakInTime].ColumnWidth = 11;

                //xlsCol += 1;
                //iBreakOutTime = xlsCol;
                //sheet1.Range[xlsRow, iBreakOutTime].Text = "Break End Time";
                //sheet1.Range[xlsRow, iBreakOutTime].ColumnWidth = 11;

                //xlsCol += 1;
                //iPunchInTime = xlsCol;
                //sheet1.Range[xlsRow, iPunchInTime].Text = "Punch In Time";
                //sheet1.Range[xlsRow, iPunchInTime].ColumnWidth = 11;

                //xlsCol += 1;
                //iPunchOutTime = xlsCol;
                //sheet1.Range[xlsRow, iPunchOutTime].Text = "Punch Out Time";
                //sheet1.Range[xlsRow, iPunchOutTime].ColumnWidth = 11;

                //xlsCol += 1;
                //iLateInTolarance = xlsCol;
                //sheet1.Range[xlsRow, iLateInTolarance].Text = "Late In ToleranceMargin";
                //sheet1.Range[xlsRow, iLateInTolarance].ColumnWidth = 11;

                //xlsCol += 1;
                //iLateInApp = xlsCol;
                //sheet1.Range[xlsRow, iLateInApp].Text = "Late In Applicable";
                //sheet1.Range[xlsRow, iLateInApp].ColumnWidth = 11;

                //xlsCol += 1;
                //iEarlyOutApp = xlsCol;
                //sheet1.Range[xlsRow, iEarlyOutApp].Text = "Early Out Applicable";
                //sheet1.Range[xlsRow, iEarlyOutApp].ColumnWidth = 11;

                //xlsCol += 1;
                //iEarlyOutTole = xlsCol;
                //sheet1.Range[xlsRow, iEarlyOutTole].Text = "Early Out Applicable";
                //sheet1.Range[xlsRow, iEarlyOutTole].ColumnWidth = 11;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup


                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    public class AttendanceRawDataUploadVM
    {
        
        public string PDate { get; set; }
        public string PType { get; set; }
        public string Id { get; set; }
        public string PTime { get; set; }
        public bool ProcessedFlag { get; set; }
        public string LogDownLoadNum { get; set; }
        public string EmployeeCode { get; set; }
        public string Remarks { get; set; }
    }
}