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


            if (string.IsNullOrEmpty(SalaryHeadId) || SalaryHeadId=="null" || SalaryHeadId == "undefined")
            {
                Sql = @"SELECT EI.EmployeeCode,EI.EmployeeName, sh.SalaryHead,sh.HeadType,c.Name Currency, d.* from dbo.MonthWiseExtraSalaryAmtChild d
                        LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster m on m.SystemID=d.MWESAMasterSystemID
                        Left join EmployeeInformation EI on EI.SystemId=m.EmpInfoSystemID
                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=d.SalaryHeadID
                        LEFT JOIN  SCS.Currency c on c.id=d.EntryCurrencyID
                        WHERE m.monthNo=" + MonthNo + @" and m.YearNo=" + YearNo + @" and m.PlantID='" + identity.PlantId + @"' and d.ExtDataUploadApp='XL'
                        ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric ";
            }
            else
            {
                Sql = @"SELECT EI.EmployeeCode,EI.EmployeeName, sh.SalaryHead,sh.HeadType,c.Name Currency, d.* from dbo.MonthWiseExtraSalaryAmtChild d
                        LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster m on m.SystemID=d.MWESAMasterSystemID
                        Left join EmployeeInformation EI on EI.SystemId=m.EmpInfoSystemID
                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=d.SalaryHeadID
                        LEFT JOIN  SCS.Currency c on c.id=d.EntryCurrencyID
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

                        //LoadDataSetFromDataGrid(ref dgEmpSalaryDefine, out dsGrd);
                        //dtGrd = dsGrd.Tables[0];
                        //dvGrd = new DataView();
                        //dvGrd.Table = dtGrd;
                        //dsGrd.Tables[0].DefaultView.RowFilter = "EmployeeCode='102841'";
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
                                    dvEmpInfo.Table = dtEmpInfo;
                                    dvEmpInfo.RowFilter = "EmployeeCode = '" + _empCode + "'";
                                    //if (dvEmpInfo.Count == 1)
                                    if (dvEmpInfo.Count > 0)
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
                                        vm.EmployeeCode = dvEmpInfo[0]["EmployeeCode"].ToString().Trim();
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
                                }//blank checking

                                //dgEmpSalaryDefine.DataSource = dtGrd;
                                //dgEmpSalaryDefine.DataBind();
                                //PanEmpSalaryDefine.Visible = true;
                                //Button_save.Visible = true;
                                //Button_save.Enabled = true;
                                //Session["VERIFICATION_STATE"] = 2;
                                //lblInfo.Text = "The entry form is in Add Mode. A new data is going to create on press the [create] button below after finish the entry.";
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




        //private void Import_Excel_File()
        //{
        //    FileInfo docFile;

        //    DataSet dsEmpInfo = null;
        //    DataTable dtEmpInfo = null;
        //    DataView dvEmpInfo = null;

        //    DataSet dsGrd = null;
        //    DataTable dtGrd = null;
        //    DataView dvGrd = null;
        //    DataRow drGrd = null;

        //    clsEmpExtraSalaryAmt objEmpExtAmt = null;
        //    string exception = "\r\n";
        //    try
        //    {
        //        #region CHECK EDIT/UPDATE ACCESS

        //        if (lblAccessEdit.Text == "NO")
        //        {
        //            Exception ex = new Exception("Access Denied for EDIT/UPDATE ... !!!");
        //            throw (ex);
        //        }

        //        #endregion //End CHECK EDIT/UPDATE ACCESS

        //        objEmpExtAmt = new clsEmpExtraSalaryAmt();

        //        #region Validation

        //        if (string.IsNullOrEmpty(ddlPlant.SelectedValue.Trim()) == true)
        //        {
        //            ddlPlant.Focus();
        //            Exception ex = new Exception("Please select Plant...");
        //            throw (ex);
        //        }
        //        if (string.IsNullOrEmpty(ddlYearNo.SelectedValue.Trim()) == true)
        //        {
        //            ddlYearNo.Focus();
        //            Exception ex = new Exception("Please select year No...");
        //            throw (ex);
        //        }
        //        if (string.IsNullOrEmpty(ddlMonthName.SelectedValue.Trim()) == true)
        //        {
        //            ddlMonthName.Focus();
        //            Exception ex = new Exception("Please select month name...");
        //            throw (ex);
        //        }
        //        if (string.IsNullOrEmpty(ddExtraSlrHd.SelectedValue.Trim()) == true)
        //        {
        //            ddExtraSlrHd.Focus();
        //            Exception ex = new Exception("Please select salary head...");
        //            throw (ex);
        //        }
        //        if (string.IsNullOrEmpty(txtForeignCurRate.Text.Trim()) == true)
        //        {
        //            txtForeignCurRate.Text = "0.0";
        //        }

        //        if (txtForeignCurRate.Text.Trim().Length > 20 || bplib.clsWebLib.IsNumeric(txtForeignCurRate.Text.Trim()) == false)
        //        {
        //            txtForeignCurRate.Focus();
        //            Exception ex = new Exception("Invalid / Blank Data not allowed for 'Amount Definition Currency Rate'. \n Please Enter Numeric data Only");
        //            throw (ex);
        //        }

        //        #endregion Validation

        //        int YearNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(ddlYearNo.Text));
        //        int MonthNo = ddlMonthName.SelectedIndex;

        //        if (FileUpload1.HasFile)
        //        {
        //            string path = Server.MapPath("TempExcelFile") + "/" + FileUpload1.FileName; exception += "\r\n" + path;
        //            try
        //            {

        //                FileUpload1.PostedFile.SaveAs(path);

        //                string ext = Path.GetExtension(FileUpload1.PostedFile.FileName); exception += "\r\n" + path;

        //                if (ext.ToLower() == ".xls" || ext.ToLower() == ".xlsx")
        //                {
        //                    string connString = string.Empty;

        //                    /
        //                    ExcelEngine excelEngine = null;
        //                    IApplication application = null;
        //                    IWorkbook workbook = null;

        //                    excelEngine = new ExcelEngine();
        //                    application = excelEngine.Excel;
        //                    workbook = excelEngine.Excel.Workbooks.Open(path);

        //                    DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);


        //                    DataSet dsExcel = new DataSet();
        //                    dsExcel.Tables.Add(dt);


        //                    docFile = new FileInfo(path);
        //                    if (docFile.Exists)
        //                    {
        //                        exception += "\r\nTrying to delete";
        //                        docFile.Delete();
        //                    }

        //                    //by monir
        //                    //Delete All Data
        //                    //clsEmpExtraSalaryAmt objEmpExtAmt = new clsEmpExtraSalaryAmt();
        //                    objEmpExtAmt.DeleteOldExtraData(YearNo, MonthNo, ddlPlant.SelectedValue.Trim(), ddExtraSlrHd.SelectedValue);

        //                    objEmpExtAmt.LoadExternalUploadFromExcelOnGrid(ddlPlant.SelectedValue.Trim(), ddExtraSlrHd.SelectedValue.Trim(), YearNo, MonthNo, out dsEmpInfo);
        //                    dtEmpInfo = dsEmpInfo.Tables[0];
        //                    dvEmpInfo = new DataView();

        //                    LoadDataSetFromDataGrid(ref dgEmpSalaryDefine, out dsGrd);
        //                    dtGrd = dsGrd.Tables[0];
        //                    dvGrd = new DataView();
        //                    dvGrd.Table = dtGrd;
        //                    //dsGrd.Tables[0].DefaultView.RowFilter = "EmployeeCode='102841'";
        //                    if (dsExcel.Tables[0].Rows.Count > 0)
        //                    {
        //                        for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
        //                        {
        //                            string strTempEntryAmt = "0.0";
        //                            string strTempDefineAmt = "0.0";
        //                            string _empCode = Regex.Replace(dsExcel.Tables[0].Rows[i][0].ToString().Trim(), @"\s", "");
        //                            //string _empCode = dsExcel.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();
        //                            strTempEntryAmt = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
        //                            //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["Amount"].ToString().Trim();
        //                            //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["F2"].ToString().Trim();
        //                            if (_empCode.Trim().Length > 0 && strTempDefineAmt.Trim().Length > 0)
        //                            {
        //                                dvEmpInfo.Table = dtEmpInfo;
        //                                dvEmpInfo.RowFilter = "EmployeeCode = '" + _empCode + "'";
        //                                //if (dvEmpInfo.Count == 1)
        //                                if (dvEmpInfo.Count > 0)
        //                                {
        //                                    if (dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim() == dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim())
        //                                    {
        //                                        strTempDefineAmt = strTempEntryAmt;
        //                                    }
        //                                    else
        //                                    {
        //                                        strTempDefineAmt = (Convert.ToDecimal(strTempEntryAmt) / Convert.ToDecimal(txtForeignCurRate.Text)).ToString("#,##0.0000;(#,##0.0000)");
        //                                    }

        //                                    drGrd = dtGrd.NewRow();
        //                                    drGrd["MWESAMasterSystemID"] = dvEmpInfo[0]["MWESAMasterSystemID"].ToString().Trim();
        //                                    drGrd["MWESAChildSystemID"] = dvEmpInfo[0]["MWESAChildSystemID"].ToString().Trim();
        //                                    drGrd["EmpInfoSystemID"] = dvEmpInfo[0]["EmpInfoSystemID"].ToString().Trim();
        //                                    drGrd["EmployeeCode"] = dvEmpInfo[0]["EmployeeCode"].ToString().Trim();
        //                                    drGrd["EmployeeName"] = dvEmpInfo[0]["EmpInfoSystemID"].ToString().Trim();
        //                                    drGrd["CurrencyRuleSystemID"] = dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim();
        //                                    drGrd["SalaryHeadID"] = dvEmpInfo[0]["SalaryHeadID"].ToString().Trim();
        //                                    drGrd["SalaryHead"] = dvEmpInfo[0]["SalaryHead"].ToString().Trim();
        //                                    drGrd["HeadType"] = dvEmpInfo[0]["HeadType"].ToString().Trim();
        //                                    drGrd["ExistCurrencyID"] = dvEmpInfo[0]["ExistCurrencyID"].ToString().Trim();
        //                                    drGrd["ExistCurrency"] = dvEmpInfo[0]["ExistCurrency"].ToString().Trim();
        //                                    drGrd["ExistAmount"] = dvEmpInfo[0]["ExistAmount"].ToString().Trim();
        //                                    drGrd["EntryCurrencyID"] = dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim();
        //                                    drGrd["EntryCurrency"] = dvEmpInfo[0]["EntryCurrency"].ToString().Trim();
        //                                    drGrd["EntryAmount"] = strTempEntryAmt.Trim();
        //                                    drGrd["DefinitionCurrencyID"] = dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim();
        //                                    drGrd["DefinitionCurrency"] = dvEmpInfo[0]["DefinitionCurrency"].ToString().Trim();
        //                                    drGrd["DefineAmount"] = strTempDefineAmt.Trim();
        //                                    drGrd["AmtDefinationCurrencyID"] = dvEmpInfo[0]["AmtDefinationCurrencyID"].ToString().Trim();
        //                                    drGrd["AmtDefinationRate"] = dvEmpInfo[0]["AmtDefinationRate"].ToString().Trim();
        //                                    drGrd["Remarks"] = "";
        //                                    dtGrd.Rows.Add(drGrd);
        //                                }
        //                                else
        //                                {
        //                                    drGrd = dtGrd.NewRow();
        //                                    drGrd["MWESAMasterSystemID"] = "";
        //                                    drGrd["MWESAChildSystemID"] = "";
        //                                    drGrd["EmpInfoSystemID"] = "";
        //                                    drGrd["EmployeeCode"] = _empCode;
        //                                    drGrd["EmployeeName"] = "";
        //                                    drGrd["CurrencyRuleSystemID"] = "";
        //                                    drGrd["SalaryHeadID"] = "";
        //                                    drGrd["SalaryHead"] = "";
        //                                    drGrd["HeadType"] = "";
        //                                    drGrd["ExistCurrencyID"] = "";
        //                                    drGrd["ExistCurrency"] = "";
        //                                    drGrd["ExistAmount"] = "";
        //                                    drGrd["EntryCurrencyID"] = "";
        //                                    drGrd["EntryCurrency"] = "";
        //                                    drGrd["EntryAmount"] = strTempEntryAmt.Trim();
        //                                    drGrd["DefinitionCurrencyID"] = "";
        //                                    drGrd["DefinitionCurrency"] = "";
        //                                    drGrd["DefineAmount"] = "0";
        //                                    drGrd["AmtDefinationCurrencyID"] = "";
        //                                    drGrd["AmtDefinationRate"] = "";
        //                                    drGrd["Remarks"] = "Employee code has not matched with the system database.";
        //                                    dtGrd.Rows.Add(drGrd);
        //                                }
        //                            }//blank checking

        //                            dgEmpSalaryDefine.DataSource = dtGrd;
        //                            dgEmpSalaryDefine.DataBind();
        //                            PanEmpSalaryDefine.Visible = true;
        //                            Button_save.Visible = true;
        //                            Button_save.Enabled = true;
        //                            Session["VERIFICATION_STATE"] = 2;
        //                            lblInfo.Text = "The entry form is in Add Mode. A new data is going to create on press the [create] button below after finish the entry.";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        throw new Exception("Please Select File");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //                docFile = new FileInfo(path);
        //                if (docFile.Exists)
        //                {
        //                    docFile.Delete();
        //                }
        //                throw (ex);
        //            }
        //        }
        //        //TxtMsgBox.Text += "\r\nGlobal.ExcelFilePath" + Global.ExcelFilePath;
        //    }
        //    catch (Exception ex)
        //    {

        //        //throw ex;
        //        ShowLog(exception.ToString());

        //        TxtMsgBox.Text += "\r\n1ERROR: With Inverted Comma Only: " + Server.MapPath("");
        //        TxtMsgBox.Text += "\r\n2ERROR With ~" + Server.MapPath("~");
        //        TxtMsgBox.Text += "\r\n3ERROR: With slash/" + Server.MapPath("/");
        //        TxtMsgBox.Text += "\r\n4ERROR TempExcelFile" + Server.MapPath("TempExcelFile");
        //        TxtMsgBox.Text += "\r\n5ERROR:/TempExcelFile" + Server.MapPath("/TempExcelFile");

        //    }
        //    finally
        //    {
        //    }
        //}






        [HttpPost]
        public ActionResult SaveExternalData(List<ExternalDataUploadVM> data, string YearNo, string MonthNo, string SalaryHeadId)
        {
            if (data == null)
            {
                throw new Exception("No data found for upload !!!!!");
            }

            _SalaryStructureUploadService.SaveData(YearNo, MonthNo, SalaryHeadId, data, (CustomIdentity)Thread.CurrentPrincipal.Identity);



            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
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



    }


}