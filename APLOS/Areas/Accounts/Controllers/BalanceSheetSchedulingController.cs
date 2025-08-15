using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.ChartOfAccounts;
using Library.Model.Enums;
using Library.Security.Core;
using Library.Service.ChartOfAccounts;
using Library.Service.Helpers;
using Library.ViewModel.Accounts;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class BalanceSheetSchedulingController : BaseController
    {
        string TableName = "dbo.BalanceSheetScheduling";
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public BalanceSheetSchedulingController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion


        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult Report()
        {
            return View();
        }
        public ActionResult SkillUpload()
        {
            return View();
        }
        public ActionResult EmployeeOperationUpload()
        {
            return View();
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,OptionNo AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from " + TableName + " where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM " + TableName + "  order by Id desc";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetBalanceSheetSchedulingList(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM dbo.BalanceSheetScheduling Where Id<>'" + id + "' order by Id desc";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> details)
        {
            try
            {
                DataSet dsMaster, dsDestination = null;
                DataRow drF;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where OptionNo='" + data["OptionNo"].ToString() + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Option No already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where BalanceSheetSchedulingId='" + data["Id"] + "'", out dsDestination, false, "1");
                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                #region NoticePeriodFormulaDetail 

                if (Convert.ToBoolean(data["IsCalculate"]) == true)
                {
                    while (dsDestination.Tables[0].DefaultView.Count > 0)
                        dsDestination.Tables[0].DefaultView[0].Delete();
                    int count = 0;
                    if (details != null)
                    {
                        foreach (var item in details)
                        {
                            drF = dsDestination.Tables[0].NewRow();
                            count++;
                            string pk = _Id + "_" + count;
                            drF["Id"] = pk;
                            drF["BalanceSheetSchedulingId"] = _Id;
                            drF["Sequence"] = item["Sequence"];
                            drF["BalanceSheetSchedulingHeadId"] = item["BalanceSheetSchedulingHeadId"];
                            drF["Component"] = item["Component"];

                            dsDestination.Tables[0].Rows.Add(drF);
                        }

                    }
                }
                #endregion NoticePeriodFormulaDetail 

                Library.Security.Core.clsStaticInfo _info = new Library.Security.Core.clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDestination);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string balanceSheetSchedulingId)
        {

            string sql = @"SELECT D.Sequence,D.BalanceSheetSchedulingHeadId
                            ,SalaryHead= CASE WHEN ISNULL(SD.Id,'')<>'' THEN SD.Id ELSE D.Component END,D.Component,D.BalanceSheetSchedulingId
                            FROM [dbo].[FormulaDetail] D
                            LEFT JOIN dbo.BalanceSheetScheduling SD ON SD.Id=D.BalanceSheetSchedulingHeadId
                            WHERE D.BalanceSheetSchedulingId='" + balanceSheetSchedulingId + "' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
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
                con.CommitTransaction();
                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Id),0) AS Id FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return Library.Security.Core.clsStaticInfo.dbl(dt.Rows[0]["Id"].ToString()) + 1;
            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            IWorkbook workbook = accountsInventoryPayableReportService.GetSampleFileBalanceSheetScheduling(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "BalanceSheetScheduling Data upload Sample File";

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
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<BalanceSheetSchedulingUploadedDataViewModel> data = new List<BalanceSheetSchedulingUploadedDataViewModel>();

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
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
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
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
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

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                BalanceSheetSchedulingUploadedDataViewModel vm = new BalanceSheetSchedulingUploadedDataViewModel();

                                vm.BudgetMasterActivityId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.Level1 = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.Level2 = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.Level3 = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.Level4 = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.GLGeneralInfoCode = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                vm.GLName = dsExcel.Tables[0].Rows[i][6].ToString().Trim();
                                vm.BudgetGroup = dsExcel.Tables[0].Rows[i][7].ToString().Trim();
                                vm.BudgetCategory = dsExcel.Tables[0].Rows[i][8].ToString().Trim();
                                vm.BudgetSubCategory = dsExcel.Tables[0].Rows[i][9].ToString().Trim();
                                vm.Budget = dsExcel.Tables[0].Rows[i][10].ToString().Trim();
                                vm.RefNo = dsExcel.Tables[0].Rows[i][11].ToString().Trim();
                                vm.Activity = dsExcel.Tables[0].Rows[i][12].ToString().Trim();
                                vm.Register = dsExcel.Tables[0].Rows[i][13].ToString().Trim();
                                vm.BalanceSheetSchedulingId = dsExcel.Tables[0].Rows[i][14].ToString().Trim();
                                vm.TaxApplicable = dsExcel.Tables[0].Rows[i][15].ToString().Trim();
                                vm.TaxType = dsExcel.Tables[0].Rows[i][16].ToString().Trim();
                                vm.UserCategory = dsExcel.Tables[0].Rows[i][17].ToString().Trim();
                                vm.UserSubCategory = dsExcel.Tables[0].Rows[i][18].ToString().Trim();
                                vm.UserItem = dsExcel.Tables[0].Rows[i][19].ToString().Trim();
                                vm.UserReport = dsExcel.Tables[0].Rows[i][20].ToString().Trim();
                                vm.IsAllowed = dsExcel.Tables[0].Rows[i][21].ToString().Trim();
                                vm.AllowedDays = Convert.ToInt32(dsExcel.Tables[0].Rows[i][22]);
                                if (!string.IsNullOrEmpty(dsExcel.Tables[0].Rows[i][23].ToString()) | !string.IsNullOrWhiteSpace(dsExcel.Tables[0].Rows[i][23].ToString()))
                                {
                                    vm.MonthDay = Convert.ToInt32(dsExcel.Tables[0].Rows[i][23]);
                                }
                                else
                                {
                                    vm.MonthDay = 0;
                                }
                                vm.UserGroup = dsExcel.Tables[0].Rows[i][24].ToString().Trim();
                                vm.Sequence = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][25]);
                                vm.UserCategorySequence = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][26]);
                                vm.UserSubCategorySequence = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][27]);
                                vm.UserItemSequence = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][28]);
                                vm.Remark = dsExcel.Tables[0].Rows[i][29].ToString().Trim();
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
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
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveBalanceSheetSchedulingUploadedData(IEnumerable<BalanceSheetSchedulingUploadedData> balanceSheetSchedulingUploadedDataList)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.SaveBalanceSheetSchedulingUploadedData(balanceSheetSchedulingUploadedDataList);

            return Json(new { Message = AplosMessage.Updated });
        }

        #region --- Report---
        [HttpPost, Authorize]
        public JsonResult GetReport(string FromDate, string ToDate)
        {
            try
            {
                var workbook = GetReportworkbook(FromDate, ToDate);
                return Json(new { FileName = workbook, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("Id");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["Id"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["Id"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["Id"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "Id = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                            if (dvLocal[0]["Amount"].ToString().Trim() == "")
                            {
                                strTemp = "0";
                            }
                            else
                            {
                                strTemp = dvLocal[0]["Amount"].ToString().Trim();
                            }
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End 

        public string GetReportworkbook(string FromDate, string ToDate)
        {
            #region Variable
            ReportUtility oru = new ReportUtility();
            string yot = string.Empty;//OTConsiderOn
            string tot = string.Empty;//OTConsiderOn
            DataView dvAttn = null;
            DataSet dsFactory = null;
            DataSet dslocal = null;
            DataSet dsCmp = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            var report = new ReportUtility();
            clsReport objRpt = null;
            string sFormulaResult = null;
            DataTable dtValue = new DataTable();
            //dtValue.TableName = "TempTable";
            //dtValue.Columns.Add("Id");
            //dtValue.Columns.Add("Amount");
            //dtValue.Columns.Add("CalAmount");
            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                #region DataSet
                GetReport(FromDate, ToDate, out dslocal);
                dtValue = dslocal.Tables[0];
                for (int i = 0; i < dslocal.Tables[0].Rows.Count; i++)
                {

                    if (!string.IsNullOrEmpty(dslocal.Tables[0].Rows[i]["FormulaDes"].ToString()))
                    {
                        ReLoadFormulaWithValue(dslocal.Tables[0].Rows[i]["FormulaDes"].ToString(), ref dtValue, out string _formulaValue);
                        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("#####");

                        DataView dv = new DataView(dslocal.Tables[0]);
                        dv.RowFilter = "Id='" + dslocal.Tables[0].Rows[i]["Id"].ToString() + "'";
                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;

                            drmo.BeginEdit();
                            if (sFormulaResult == "" || sFormulaResult == "∞" || sFormulaResult == "NaN")
                            {
                                drmo["CalAmount"] = 0;
                            }
                            else
                            {
                                drmo["CalAmount"] = sFormulaResult;
                            }
                            drmo.EndEdit();

                        }

                    }
                }


                objRpt = new clsReport();
                dvAttn = new DataView();
                dvAttn.Table = dslocal.Tables[0];

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

                    int strCount = 0;
                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Id";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "OptionNo";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Type";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Detail Applicable";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Group Sequence";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Group";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "SubGroup Sequence";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "SubGroup";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "User Group";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "User SubGroup";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Item Sequence";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "ItemNo";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int p = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Item";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int q = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sub ItemNo";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 9;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int y = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sub Item";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int yo = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "ScheduleNo";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Schedule Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "User Item";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "User Schedule Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Formula";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Amount";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    #endregion ------------------Column Header------------------


                    for (int i = 0; i < dvAttn.Count; i++)
                    {

                        xlsCol = 1;
                        #region ----------------------Data-----------------------

                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Id"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OptionNo"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Type"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["DetailApplicable"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["GroupSequence"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Group"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;

                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubGroupSequence"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubGroup"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["UserGroup"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["UserSubGroup"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ItemSequence"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ItemNo"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Item"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubItemNo"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubItem"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ScheduleNo"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ScheduleName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["UserItem"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["UserScheduleName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["FormulaDes"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 20;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        if (OTSBD.clsStaticInfo.dbl(dvAttn[i]["CalAmount"].ToString()) == 0)
                        {
                            sheet1.Range[xlsRow, xlsCol].Number = OTSBD.clsStaticInfo.dbl(dvAttn[i]["Amount"].ToString());
                        }
                        else
                        {
                            sheet1.Range[xlsRow, xlsCol].Number = OTSBD.clsStaticInfo.dbl(dvAttn[i]["CalAmount"].ToString());
                        }
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                        xlsRow += 1;

                        #region Line Setup
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;
                        #endregion


                        #endregion ----------------------Data-----------------------

                    }

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
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
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
                    sheet1.Range[xlsRow, 3].Text = "Production Order Rate Report";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "From Date:- " + FromDate + " To Date:- " + ToDate;
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
                    sheet1.FirstVisibleRow = 7;
                    #endregion

                    #region Page Setup

                    sheet1.Name = "RateReport";
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

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

        public void GetReport(string FromDate, string ToDate, out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {

                strSql = @"declare  @fromDate date ='" + FromDate + @"', @todate date ='" + ToDate + @"',@companyId varchar(10)='" + identity.CompanyId + @"',@plantId varchar(10)='" + identity.PlantId + @"'--Cedaar

 SELECT A.Id,A.OptionNo,A.Type,A.DetailApplicable,A.GroupSequence,A.[Group],A.SubGroupSequence,A.SubGroup,A.UserGroup,A.UserSubGroup,A.ItemSequence,A.ItemNo,A.Item,A.SubItemNo,A.SubItem,A.ScheduleNo,A.ScheduleName,A.UserItem,A.UserScheduleName,A.IsActive,SUM(B.Amount)Amount,A.FormulaDes,0 CalAmount FROM [dbo].[BalanceSheetScheduling] A
 LEFT JOIN 
		(SELECT * 
											,ValueDr=CASE WHEN Type='PL' THEN DRForThePeriod ELSE  DRClosingBalance END
										 ,ValueCr=CASE WHEN Type='PL' THEN CRForThePeriod ELSE  CRClosingBalance END
										 ,Amount= CASE WHEN ((CASE WHEN Type='PL' THEN DRForThePeriod ELSE  DRClosingBalance END)>(CASE WHEN Type='PL' THEN CRForThePeriod ELSE  CRClosingBalance END))
										 THEN((CASE WHEN Type='PL' THEN DRForThePeriod ELSE  DRClosingBalance END)-(CASE WHEN Type='PL' THEN CRForThePeriod ELSE  CRClosingBalance END))
										 ELSE((CASE WHEN Type='PL' THEN CRForThePeriod ELSE  CRClosingBalance END)-(CASE WHEN Type='PL' THEN DRForThePeriod ELSE  DRClosingBalance END))
										 END
										 ,[Dr/Cr]=CASE WHEN ((CASE WHEN Type='PL' THEN DRForThePeriod ELSE  DRClosingBalance END)>(CASE WHEN Type='PL' THEN CRForThePeriod ELSE  CRClosingBalance END))
										 THEN 'Dr' ELSE 'Cr' END
                                            FROM(SELECT  
		                                  SuM(OBDRcumulative + FROBDRcumulative) DROpeningBalance, SUM(OBCRcumulative + FROBCRcumulative) CROpeningBalance
										, SUM(DRcumulative) DRForThePeriod, SUM(CRcumulative) CRForThePeriod
                                            , SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) DRClosingBalance, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CRClosingBalance
										   ,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
										 ,ISNULL(BudgetMasterId,'') BudgetMasterId
										 ,Activity,ISNULL(ActivityId,'') ActivityId,Level1,Level2,Level3,Level4,BudgetCategory,BudgetSubCategory,ControlId,BalanceSheetSchedulingId,Type
										 

		                                 FROM
		                                ( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative,0 PDRcumulative,0 PCRcumulative,       
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
                                            A.Id AS ActivityId
											,C1.UserName Level1,C2.UserName Level2,C3.UserName Level3,C4.UserName Level4
											,BCT.UserName BudgetCategory,BSCT.UserName BudgetSubCategory,BMA.Id ControlId
											,BMA.BalanceSheetSchedulingId,BSS.Type
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.ActivityId=A.Id AND BMA.BudgetMasterId=BM.Id
											LEFT JOIN [dbo].[BalanceSheetScheduling] BSS ON BSS.Id=BMA.BalanceSheetSchedulingId
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
											LEFT JOIN [HKP].BudgetCategory BCT ON BCT.Id=BM.BudgetCategoryId
											LEFT JOIN [HKP].BudgetSubCategory BSCT ON BSCT.Id=BM.BudgetSubCategoryId
											LEFT JOIN HKP.COALevel1 C1 ON C1.Id=GL.COALevel1Id
											LEFT JOIN HKP.COALevel2 C2 ON C2.Id=GL.COALevel2Id
											LEFT JOIN HKP.COALevel3 C3 ON C3.Id=GL.COALevel3Id
											LEFT JOIN HKP.COALevel4 C4 ON C4.Id=GL.COALevel4Id
                                            WHERE v.PostingDate < @fromDate and v.CompanyId =@companyId AND V.PlantId=@plantId
                                            AND  v.IsPark=0
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
											,C1.UserName,C2.UserName,C3.UserName ,C4.UserName  ,BCT.UserName,BSCT.UserName,BMA.Id,BMA.BalanceSheetSchedulingId,BSS.Type
											UNION 

										
											   SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,0 OBDRcumulative,0 OBCRcumulative,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS CRcumulative
                                 
                                           , 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative   
										    , SUM(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                           PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, SUM(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative,
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
                                            A.Id AS ActivityId
											,C1.UserName Level1,C2.UserName Level2,C3.UserName Level3,C4.UserName Level4
											,BCT.UserName BudgetCategory,BSCT.UserName BudgetSubCategory,BMA.Id ControlId
											,BMA.BalanceSheetSchedulingId,BSS.Type
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.ActivityId=A.Id AND BMA.BudgetMasterId=BM.Id
											LEFT JOIN [dbo].[BalanceSheetScheduling] BSS ON BSS.Id=BMA.BalanceSheetSchedulingId
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
											LEFT JOIN [HKP].BudgetCategory BCT ON BCT.Id=BM.BudgetCategoryId
											LEFT JOIN [HKP].BudgetSubCategory BSCT ON BSCT.Id=BM.BudgetSubCategoryId
											LEFT JOIN HKP.COALevel1 C1 ON C1.Id=GL.COALevel1Id
											LEFT JOIN HKP.COALevel2 C2 ON C2.Id=GL.COALevel2Id
											LEFT JOIN HKP.COALevel3 C3 ON C3.Id=GL.COALevel3Id
											LEFT JOIN HKP.COALevel4 C4 ON C4.Id=GL.COALevel4Id
                                            WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, @fromDate) AND CONVERT(DATE, @toDate) AND SourceType!='OpeningBalance' AND v.CompanyId =@companyId AND V.PlantId=@plantId
                                            AND  V.IsPark=0
                                           GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
											,C1.UserName,C2.UserName,C3.UserName ,C4.UserName  ,BCT.UserName,BSCT.UserName,BMA.Id,BMA.BalanceSheetSchedulingId,BSS.Type
                                            UNION
                                                        SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBCRcumulative,0 PDRcumulative,0 PCRcumulative
															, ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
                                            A.Id AS ActivityId
											,C1.UserName Level1,C2.UserName Level2,C3.UserName Level3,C4.UserName Level4
											,BCT.UserName BudgetCategory,BSCT.UserName BudgetSubCategory,BMA.Id ControlId
											,BMA.BalanceSheetSchedulingId,BSS.Type
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.ActivityId=A.Id AND BMA.BudgetMasterId=BM.Id
											LEFT JOIN [dbo].[BalanceSheetScheduling] BSS ON BSS.Id=BMA.BalanceSheetSchedulingId
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
											LEFT JOIN [HKP].BudgetCategory BCT ON BCT.Id=BM.BudgetCategoryId
											LEFT JOIN [HKP].BudgetSubCategory BSCT ON BSCT.Id=BM.BudgetSubCategoryId
	                                        LEFT JOIN HKP.COALevel1 C1 ON C1.Id=GL.COALevel1Id
											LEFT JOIN HKP.COALevel2 C2 ON C2.Id=GL.COALevel2Id
											LEFT JOIN HKP.COALevel3 C3 ON C3.Id=GL.COALevel3Id
											LEFT JOIN HKP.COALevel4 C4 ON C4.Id=GL.COALevel4Id
                                                    WHERE V.PostingDate = @fromDate AND V.CompanyId = @companyId AND V.PlantId = @plantId AND v.IsPark = 0 and v.SourceType='OpeningBalance'

                                                
                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,C1.UserName,C2.UserName,C3.UserName ,C4.UserName ,BCT.UserName,BSCT.UserName,BMA.Id,BMA.BalanceSheetSchedulingId,BSS.Type
											) TOTAL	
											GROUP BY AccountCodeId,ParallelCurrencyId,CurrencyCode,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
		                                    ,BudgetMasterId,Activity,ActivityId,Level1,Level2,Level3,Level4,BudgetCategory,BudgetSubCategory,ControlId,BalanceSheetSchedulingId,Type
                                            )ttd 
                                               WHERE ISNULL(DRForThePeriod,0.00) <> 0.00 OR ISNULL(CRForThePeriod,0) <> 0.00 OR
											ISNULL(DROpeningBalance,0.00) <> 0.00 OR ISNULL(CROpeningBalance,0) <> 0.00 OR
											ISNULL(DRClosingBalance,0.00) <> 0.00 OR ISNULL(CRClosingBalance,0) <> 0.00
											)B
											ON A.Id=B.BalanceSheetSchedulingId
											
											Group by A.Id,A.OptionNo,A.Type,A.DetailApplicable,A.GroupSequence,A.[Group],A.SubGroupSequence,A.SubGroup,A.UserGroup,A.UserSubGroup,A.ItemSequence,A.ItemNo,A.Item,A.SubItemNo,A.SubItem,A.ScheduleNo,A.ScheduleName,A.UserItem,A.UserScheduleName,A.IsActive,A.FormulaDes
											Having SUM(B.Amount)>0";

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

        [HttpGet, Authorize]
        public ActionResult GetCheckSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            IWorkbook workbook = accountsInventoryPayableReportService.GetBalanceSheetSchedulingCheckingApprovingData(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Balance Sheet Scheduling Checking upload Sample File";

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

        [HttpGet, Authorize]
        public ActionResult GetApproveSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            IWorkbook workbook = accountsInventoryPayableReportService.GetBalanceSheetSchedulingCheckingApprovingData(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Balance Sheet Scheduling Approving upload Sample File";

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

        #region SkillUpload
        [HttpGet, Authorize]
        public ActionResult GetSkillSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetSampleFileEmployeeSkill(identity.Name);
            var reportFileName = "Employee Skill Data upload Sample File";

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

        public IWorkbook GetSampleFileEmployeeSkill(string Name)
        {
            #region declare
            clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                //IWorksheet sheetSource = null;
                //sheetSource = workbook.Worksheets[1];
                xlsRow = 1;

                #region ------------------Column Header------------------



                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SkillId"); int colSkillId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SkillCode"); int colSkillCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SkillName"); int colSkillName = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SkillGroup"); int colSkillGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SkillCategory"); int colSkillCategory = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MachineApplicable"); int colMachineApplicable = xlsCol; 
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 17.50; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OperationApplicable"); int colOperationApplicable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DashboardApplicable"); int colDashboardApllicable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemId"); int colEmpSystemId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remark"); int colRemark = xlsCol;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                xlsRow++;

                #endregion ------------------Column Header------------------

                DataTable dtSkill = GetSkillData();
                for (int i = 0; i < dtSkill.Rows.Count; i++)
                {
                    sheet1[xlsRow, colSkillId].Text = dtSkill.Rows[i]["SkillId"].ToString();
                    sheet1[xlsRow, colSkillCode].Text = dtSkill.Rows[i]["SkillCode"].ToString();
                    sheet1[xlsRow, colSkillName].Text = dtSkill.Rows[i]["SkillName"].ToString();
                    sheet1[xlsRow, colSkillGroup].Text = dtSkill.Rows[i]["SkillGroup"].ToString();
                    sheet1[xlsRow, colSkillCategory].Text = dtSkill.Rows[i]["SkillCategory"].ToString();
                    sheet1[xlsRow, colMachineApplicable].Text = dtSkill.Rows[i]["MachineApplicable"].ToString();
                    sheet1[xlsRow, colOperationApplicable].Text = dtSkill.Rows[i]["OperationApplicable"].ToString();
                    sheet1[xlsRow, colDashboardApllicable].Text = dtSkill.Rows[i]["DashboardApplicable"].ToString();
                    sheet1[xlsRow, colEmpSystemId].Text = dtSkill.Rows[i]["EmpSystemId"].ToString();
                    sheet1[xlsRow, colRemark].Text = dtSkill.Rows[i]["Remark"].ToString();
                    xlsRow++;
                }

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
        public DataTable GetSkillData()
        {
            var cmdText = @"Select S.Id SkillId,S.Code SkillCode,S.UserName SkillName,SG.UserName SkillGroup,SC.UserName SkillCategory,''EmpSystemId,'' Remark
,MachineApplicable=CASE WHEN IsMachineApplicable=1 THEN 'Yes' ELSE 'No' END
,OperationApplicable=CASE WHEN OperationApplicable=1 THEN 'Yes' ELSE 'No' END
,DashboardApplicable=CASE WHEN DashboardApplicable=1 THEN 'Yes' ELSE 'No' END
from HKP.Skill S
LEFT JOIN [SCS].[SkillGrouping] SG ON SG.Id=S.SkillGroupId
LEFT JOIN HKP.[SkillCategory] SC ON SC.Id=S.SkillCategoryId
UNION ALL
Select S.Id SkillId,S.Code SkillCode,S.UserName SkillName,SG.UserName SkillGroup,SC.UserName SkillCategory,ES.EmpSystemId,ES.Remark
,MachineApplicable=CASE WHEN IsMachineApplicable=1 THEN 'Yes' ELSE 'No' END
,OperationApplicable=CASE WHEN OperationApplicable=1 THEN 'Yes' ELSE 'No' END
,DashboardApplicable=CASE WHEN DashboardApplicable=1 THEN 'Yes' ELSE 'No' END
from [dbo].[EmployeeSkill] ES
LEFT JOIN HKP.Skill S ON S.Id=ES.SkillId
LEFT JOIN [SCS].[SkillGrouping] SG ON SG.Id=S.SkillGroupId
LEFT JOIN HKP.[SkillCategory] SC ON SC.Id=S.SkillCategoryId";
            return _sqlRepository.GetDataTable(cmdText);


        }

        [HttpPost, Authorize]
        public JsonResult ImportSkillData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<EmployeeSkill> data = new List<EmployeeSkill>();

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
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
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
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
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

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                EmployeeSkill vm = new EmployeeSkill();

                                vm.SkillId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.SkillCode = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.SkillName = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.SkillGroup = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.SkillCategory = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.MachineApplicable = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                vm.OperationApplicable = dsExcel.Tables[0].Rows[i][6].ToString().Trim();
                                vm.DashboardApllicable = dsExcel.Tables[0].Rows[i][7].ToString().Trim();
                                vm.EmpSystemId = dsExcel.Tables[0].Rows[i][8].ToString().Trim();
                                vm.Remark = dsExcel.Tables[0].Rows[i][9].ToString().Trim();
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
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
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost,Authorize]
        public ActionResult SaveEmployeeSkillData(IEnumerable<EmployeeSkill> skillDataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string _Id = "";

            foreach (var item in skillDataList)
            {
                string sql = "SELECT * FROM [dbo].[EmployeeSkill] WHERE SkillId='" + item.SkillId + "' AND EmpSystemId='" + item.EmpSystemId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeSkill", out _Id);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = _Id;
                    dr["EmpSystemId"] = item.EmpSystemId;
                    dr["SkillId"] = item.SkillId;
                    dr["Remark"] = item.Remark;

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
                    
                    dr["EmpSystemId"] = item.EmpSystemId;
                    dr["SkillId"] = item.SkillId;
                    dr["Remark"] = item.Remark;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            return Json(new { Message = AplosMessage.Insert });
        }


        #endregion

        #region OperationUpload
        [HttpGet, Authorize]
        public ActionResult GetOperationSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetSampleFileEmployeeOperation(identity.Name);
            var reportFileName = "Employee Operation Data upload Sample File";

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

        public IWorkbook GetSampleFileEmployeeOperation(string Name)
        {
            #region declare
            clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];
                xlsRow = 1;

                #region ------------------Column Header------------------



                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SKillMasterId"); int colOperationMasterId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OperationMaster"); int colOperationMaster = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Skill"); int colSkill = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SkillGroup"); int colSkillGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DesignationGroup"); int colDesignationGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LegalDesignation"); int colLegalDesignation = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Process"); int colProcess = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemId"); int colEmpSystemId = xlsCol; 

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                xlsRow++;

                #endregion ------------------Column Header------------------

                DataTable dtSkill = GetOperationData();
                for (int i = 0; i < dtSkill.Rows.Count; i++)
                {
                    sheet1[xlsRow, colOperationMasterId].Text = dtSkill.Rows[i]["OperationMasterId"].ToString();
                    sheet1[xlsRow, colOperationMaster].Text = dtSkill.Rows[i]["OperationMaster"].ToString();
                    sheet1[xlsRow, colSkill].Text = dtSkill.Rows[i]["Skill"].ToString();
                    sheet1[xlsRow, colSkillGroup].Text = dtSkill.Rows[i]["SkillGroup"].ToString();
                    sheet1[xlsRow, colDesignationGroup].Text = dtSkill.Rows[i]["DesignationGroup"].ToString();
                    sheet1[xlsRow, colLegalDesignation].Text = dtSkill.Rows[i]["LegalDesignation"].ToString();
                    sheet1[xlsRow, colProcess].Text = dtSkill.Rows[i]["Process"].ToString();
                    xlsRow++;
                }

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
        public DataTable GetOperationData()
        {
            var cmdText = @"SELECT OM.Id OperationMasterId,OM.UserName OperationMaster
,S.UserName Skill,SG.UserName SkillGroup,DG.UserName DesignationGroup,LD.UserName LegalDesignation,MM.UserName MachineMaster,P.UserName Process
FROM MST.OperationMaster OM
LEFT JOIN HKP.Skill S ON S.Id=OM.SkillId
LEFT JOIN SCS.SkillGrouping SG ON SG.Id=OM.SkillGroupId
LEFT JOIN HKP.DesignationGroup DG ON DG.Id=OM.DesignationGroupId
LEFT JOIN HKP.LegalDesignation LD ON LD.Id=OM.LegalDesignationId
LEFT JOIN MST.MachineMaster MM ON MM.Id=OM.MachineMasterId
LEFT JOIN HKP.Process P ON P.Id=OM.ProcessId";
            return _sqlRepository.GetDataTable(cmdText);


        }

        [HttpPost, Authorize]
        public JsonResult ImportEmployeeOperationData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<EmployeeOperation> data = new List<EmployeeOperation>();

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
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
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
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
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

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                EmployeeOperation vm = new EmployeeOperation();

                                vm.OperationMasterId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.OperationMaster = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.Skill = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.SkillGroup = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.DesignationGroup = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.LegalDesignation = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                vm.MachineMaster = dsExcel.Tables[0].Rows[i][6].ToString().Trim();
                                vm.Process = dsExcel.Tables[0].Rows[i][7].ToString().Trim();
                                vm.EmpSystemId = dsExcel.Tables[0].Rows[i][8].ToString().Trim();
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
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
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveEmployeeOperationData(IEnumerable<EmployeeOperation> operationDataList)
        {
            EmployeeProfile ef = new EmployeeProfile();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string _Id = "";
            foreach (var item in operationDataList)
            {
                string sql = "SELECT * FROM [dbo].[EmployeeOperation] WHERE OperationMasterId='" + item.OperationMasterId + "' AND EmpSystemId='" + item.EmpSystemId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                DataSet dsSeq;
                ef.GetOperationSequence(item.EmpSystemId, out dsSeq);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Sequence"].ToString());
                if (seq != 0)
                {
                    seq--;
                }

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeOperation", out _Id);
                    seq++;
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = _Id;
                    dr["EmpSystemId"] = item.EmpSystemId;
                    dr["OperationMasterId"] = item.OperationMasterId;
                    dr["Sequence"] = seq;
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

                    dr["EmpSystemId"] = item.EmpSystemId;
                    dr["OperationMasterId"] = item.OperationMasterId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            return Json(new { Message = AplosMessage.Insert });
        }


        #endregion


    }

    public class EmployeeSkill
    {
        public string Id { get; set; }
        public string SkillId { get; set; }
        public string SkillCode { get; set; }
        public string SkillName { get; set; }
        public string SkillGroup { get; set; }
        public string SkillCategory { get; set; }
        public string MachineApplicable { get; set; }
        public string OperationApplicable { get; set; }
        public string DashboardApllicable { get; set; }
        public string EmpSystemId { get; set; }
        public string Remark { get; set; }
    }

}