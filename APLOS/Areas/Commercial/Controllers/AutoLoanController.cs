#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Finances;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class AutoLoanController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IAutoLoanService _autoLoanService;

        public AutoLoanController( ISqlRepository R
           , IAutoLoanService autoLoanService
            )
        {
            _sqlRepository = R;
            _autoLoanService = autoLoanService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult AutoLoanPost()
        {
            return View();
        }
        #endregion

        [Authorize, HttpGet]
        public JsonResult GetAutoLoanAvailableList(bool dateRange,string fromDate, string toDate)
        {
            AccountsAutoLoanService accountsAutoLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsAutoLoanService.GetAutoLoanAvailableList(identity.PlantId,dateRange,fromDate,toDate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAutoLoanPostableList()
        {
            AccountsAutoLoanService accountsAutoLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsAutoLoanService.GetAutoLoanPostableList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [Authorize, HttpGet]
        public JsonResult GetAutoLoanPostableDetailList(string LoanAgainstAcceptanceMasterId, string SourceType)
        {
            AccountsAutoLoanService accountsAutoLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsAutoLoanService.GetAutoLoanPostableDetailList(identity.PlantId, LoanAgainstAcceptanceMasterId, SourceType), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpPost]
        public JsonResult SaveAutoLoan(List<Dictionary<string, object>> autoLoanData,Dictionary<string,object> LCModel)
        {
            try
            {
                #region Validation
                string LC = "";
                for (int i = 0; i < autoLoanData.Count; i++)
                {
                    if (i == 0 ||LC == autoLoanData[i]["PurchaseLCNo"].ToString())
                    {
                        LC = autoLoanData[i]["PurchaseLCNo"].ToString();
                    }
                    else
                    {
                        throw new Exception("LC should be matched with " + LC + " ");
                    }
                }
                if (string.IsNullOrEmpty(LCModel["LoanDate"].ToString()))
                {
                    throw new Exception("Insert Loan date");
                }
                if (string.IsNullOrEmpty(LCModel["LoanNo"].ToString()))
                {
                    throw new Exception("Insert Loan no");
                }
                #endregion
                SaveLoanAgainstAcceptance(autoLoanData, LCModel);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "LoanAgainstAcceptanceMaster", out sID);
            return sID;
        }
        private void SaveLoanAgainstAcceptance(List<Dictionary<string, object>> data, Dictionary<string, object> LCModel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster,dsDetails; DataRow drSave, drMSave;
                    string MasterId = string.Empty; int count = 0;
                    
                    string sql = "SELECT * FROM [LoanAgainstAcceptanceMaster] WHERE 1=2";
                    string sql2 = "SELECT * FROM [LoanAgainstAcceptanceDetail] WHERE 1=2";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sql2, out dsDetails, false, "1");

                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        drMSave = dsMaster.Tables[0].NewRow();
                        drMSave["Id"] = GetPK();
                        MasterId = drMSave["Id"].ToString();

                        drMSave["VoucherId"] = null;
                        drMSave["CompanyGroupId"] = identity.CompanyGroupId;
                        drMSave["CompanyId"] = identity.CompanyId;
                        drMSave["PlantId"] = identity.PlantId;
                        //drMSave["EntityId"] = data[0]["EntityId"];
                        drMSave["CurrencyId"] = data[0]["CurrencyId"];
                        drMSave["PartyType"] = "Vendor";
                        drMSave["PartyId"] = data[0]["PartyId"];
                        drMSave["PartyPlantId"] = data[0]["PartyPlantId"];
                        drMSave["PaymentSource"] = "Bank";
                        drMSave["TransactionType"] = "LoanTaken";
                        drMSave["Amount"] = LCModel["Amount"];
                        drMSave["LoanDate"] = LCModel["LoanDate"];
                        drMSave["LoanNo"] = LCModel["LoanNo"];
                        drMSave["IsPark"] = true;

                        drMSave["AddedBy"] = identity.Name;
                        drMSave["AddedDate"] = DateTime.Now;
                        drMSave["AddedFromIP"] = identity.IPAddress;

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(drMSave);

                    }
                    foreach (var item in data)
                    {
                        dsDetails.Tables[0].DefaultView.RowFilter = "Id = '" + item["PurchaseDocAcceptanceId"] + "'";
                        if (dsDetails.Tables[0].DefaultView.Count == 0)
                        {
                            count++;
                            drSave = dsDetails.Tables[0].NewRow();
                            drSave["Id"] =  MasterId + count;
                            drSave["LoanAgainstAcceptanceMasterId"] = MasterId;
                            if(item["SourceType"].ToString() == "Acceptance")
                            {
                                drSave["PurchaseDocAcceptanceId"] = item["PurchaseDocAcceptanceId"];
                            }
                            else
                            {
                                drSave["InvoiceId"] = item["PurchaseDocAcceptanceId"];
                            }
                            
                            drSave["BankMasterId"] = item["BankMasterId"];

                            drSave["AddedBy"] = identity.Name;
                            drSave["AddedDate"] = DateTime.Now;
                            drSave["AddedFromIP"] = identity.IPAddress;

                            drSave["UpdatedBy"] = identity.Name;
                            drSave["UpdatedDate"] = DateTime.Now;
                            drSave["UpdatedFromIP"] = identity.IPAddress;
                            dsDetails.Tables[0].Rows.Add(drSave);

                        }
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDetails);
                    //foreach (var item in data)
                    //{
                    //    string sql = "SELECT * FROM [trn].[LoanAgainstAcceptance] WHERE Id='" + item["Id"] + "'";
                    //    objCon = new ConnectionManager.DAL.ConManager("1");
                    //    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                    //    if (dsMaster.Tables[0].Rows.Count == 0)
                    //    {
                    //        DataRow dr = dsMaster.Tables[0].NewRow();
                    //        dr["Id"] = GetPK();
                    //        dr["PurchaseDocAcceptanceId"] = item["PurchaseDocAcceptanceId"];
                    //        dr["VoucherId"] = null;
                    //        dr["BankMasterId"] = item["BankMasterId"];
                    //        dr["CompanyGroupId"] = identity.CompanyGroupId;
                    //        dr["CompanyId"] = identity.CompanyId;
                    //        dr["PlantId"] = identity.PlantId;
                    //        //dr["EntityId"] = identity.EntityId;
                    //        dr["CurrencyId"] = item["CurrencyId"];
                    //        dr["PartyType"] = "Vendor";
                    //        dr["PartyId"] = item["PartyId"];
                    //        dr["PartyPlantId"] = item["PartyPlantId"];
                    //        dr["Amount"] = item["Amount"];
                    //        dr["PaymentSource"] = "Bank";
                    //        dr["TransactionType"] = "LoanTaken";
                    //        dr["LoanDate"] = item["LoanDate"];
                    //        dr["LoanNo"] = item["LoanNo"];
                    //        dr["IsPark"] = true;

                    //        dr["AddedBy"] = identity.Name;
                    //        dr["AddedDate"] = DateTime.Now;
                    //        dr["AddedFromIP"] = identity.IPAddress;

                    //        dsMaster.Tables[0].Rows.Add(dr);
                    //    }
                    //    else
                    //    {
                    //        //edit
                    //        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    //        dr.BeginEdit();

                    //        dr["PurchaseDocAcceptanceId"] = item["PurchaseDocAcceptanceId"];
                    //        dr["VoucherId"] = null;
                    //        dr["BankMasterId"] = item["BankMasterId"];
                    //        dr["CompanyGroupId"] = identity.CompanyGroupId;
                    //        dr["CompanyId"] = identity.CompanyId;
                    //        dr["PlantId"] = identity.PlantId;
                    //        //dr["EntityId"] = identity.EntityId;
                    //        dr["CurrencyId"] = item["CurrencyId"];
                    //        dr["PartyType"] = "Vendor";
                    //        dr["PartyId"] = item["PartyId"];
                    //        dr["PartyPlantId"] = item["PartyPlantId"];
                    //        dr["Amount"] = item["Amount"];
                    //        dr["PaymentSource"] = "Bank";
                    //        dr["TransactionType"] = "LoanTaken";
                    //        dr["LoanDate"] = item["LoanDate"];
                    //        dr["LoanNo"] = item["LoanNo"];
                    //        dr["IsPark"] = true;

                    //        dr["AddedBy"] = identity.Name;
                    //        dr["AddedDate"] = DateTime.Now;
                    //        dr["AddedFromIP"] = identity.IPAddress;

                    //        dr.EndEdit();
                    //    }
                    //    clsStaticInfo obj = new clsStaticInfo();
                    //    obj.SaveDataSets(dsMaster);
                    //}
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetSaveData()
        {
            try
            {
                AccountsAutoLoanService _accountsLoanService = new AccountsAutoLoanService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(_accountsLoanService.GetMaster(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetAutoLoanList(GridParameter parameters)
        {
            AccountsAutoLoanService _accountsLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.LoanQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.AutoLoan), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AutoLoanPost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = false;
            voucherVM.SourceType = SourceType.AutoLoan.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherDetailVMList.FirstOrDefault().CompanyCurrencyRate < 0 || voucherDetailVMList.FirstOrDefault().CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
         
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }
            if (voucherVM.SettlementType == "Acceptance")
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _autoLoanService.ParkAutoLoan(voucherVM, voucherDetailVMList, existingLoanList, loanRepaymentSchedulelist)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _autoLoanService.ParkAutoLoanInvoice(voucherVM, voucherDetailVMList, existingLoanList, loanRepaymentSchedulelist)) });
            }
        }


        //[HttpGet, Authorize]
        //public ActionResult GetAutoLoanReport(ReportFormat reportFormat, string voucherId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var workbook = _autoLoanService.GetEmployeePayment(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcelx(workbook, reportFileName);

        //        default:
        //            return View();
        //    }
        //}


        //public IWorkbook GetAutoLoanReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        //{
        //    var reportUtility = new ReportUtility();
        //    var excelEngine = new ExcelEngine();
        //    var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
        //    workbook.Version = ExcelVersion.Excel2016;
        //    var sheet = workbook.Worksheets[0];
        //    sheet.Name = "Voucher";

        //    var header = GetEmployeePaymentHeader(companyGroupId, companyId, plantId, voucherId, SourceType.EmployeePayment);

        //    reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

        //    var dsLocal = GetEmployeePaymentVoucher(companyId, voucherId);

        //    var transcationCurrency = header["CurrencyId"].ToString();
        //    _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

        //    var row = 5;
        //    var colLast = 1;
        //    int xlsCol = 1;

        //    int colinrDebit = 0;
        //    int colinrCredit = 0;
        //    int colusdDebit = 0;
        //    int colusdCradit = 0;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
        //    sheet[row, 1].ColumnWidth = 20;
        //    sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
        //    sheet[row, 2].ColumnWidth = 10;
        //    sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

        //    sheet[row, 3].ColumnWidth = 10;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Voucher Date");
        //    reportUtility.SetText(ref sheet, row, 7, header["VoucherDate"].ToString());
        //    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    row++;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
        //    reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
        //    sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "DocDate");
        //    reportUtility.SetText(ref sheet, row, 7, header["DocDate"].ToString());
        //    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    row++;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee:");
        //    reportUtility.SetText(ref sheet, row, 2, header["EmployeeCode"].ToString() + " - " + header["EmployeeName"].ToString());
        //    sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Doc Ref");
        //    reportUtility.SetText(ref sheet, row, 7, header["DocRefNo"].ToString());
        //    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    row++;

        //    //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
        //    // reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
        //    reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
        //    sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
        //    sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Status");
        //    reportUtility.SetText(ref sheet, row, 7, header["Status"].ToString());
        //    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

        //    //row++;
        //    row++;  //10
        //    colLast = companyCurrencyId == transcationCurrency ? 7 : 9;
        //    if (companyCurrencyId == transcationCurrency)
        //    {
        //        reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
        //        sheet[row, 6, row, 7].Merge();
        //    }
        //    else
        //    {
        //        reportUtility.SetHeaderText(ref sheet, row, 6, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
        //        sheet[row, 6, row, 7].Merge();

        //        reportUtility.SetHeaderText(ref sheet, row, 8, companyCurrencyCode, ExcelHAlign.HAlignCenter);
        //        sheet[row, 8, row, 9].Merge();
        //    }
        //    //sheet[row, 6].RowHeight = 15;

        //    sheet.Range[row, 6, row, colLast].BorderAround(ExcelLineStyle.Hair);
        //    sheet.Range[row, 6, row, colLast].BorderInside(ExcelLineStyle.Hair);
        //    row++;

        //    int colGl = 0;
        //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
        //    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

        //    xlsCol++; //clo3

        //    xlsCol++; //cloDNaration
        //    int colDnaration = 0;
        //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Detail Narration"); colDnaration = xlsCol;
        //    sheet[row, 4].ColumnWidth = 40;
        //    //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

        //    xlsCol++; //clo5
        //    int colApprovedBy = 0;
        //    colApprovedBy = xlsCol;
        //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Approved By");
        //    sheet[row, colApprovedBy].ColumnWidth = 20;
        //    //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
        //    xlsCol++;

        //    //xlsCol++;

        //    if (companyCurrencyId != transcationCurrency)
        //    {
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
        //        colLast = xlsCol; //col9

        //        sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
        //        sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
        //        //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    }
        //    else
        //    {

        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
        //        colLast = xlsCol;

        //        //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
        //        //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

        //        sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
        //        sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
        //        //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    }


        //    int formulaStartRow = 0;
        //    int formulaEndRow = 0;

        //    if (dsLocal.Rows.Count > 0)
        //    {
        //        double totalTranAmount = 0;
        //        double totalBookCurrencyAmount = 0;
        //        row++; //?? 12

        //        formulaStartRow = row;
        //        for (int i = 0; i < dsLocal.Rows.Count; i++)
        //        {
        //            var glName = dsLocal.Rows[i]["Budget"].ToString();
        //            // glName = string.Empty;
        //            reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
        //            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();

        //            reportUtility.SetText(ref sheet, row, colDnaration, dsLocal.Rows[i]["DetailNarration"].ToString());
        //            sheet[row, colDnaration].RowHeight = 25;
        //            sheet[row, colDnaration].WrapText = true;

        //            reportUtility.SetText(ref sheet, row, colApprovedBy, dsLocal.Rows[i]["ApprovedBy"].ToString());

        //            if (companyCurrencyId != transcationCurrency)
        //            {
        //                reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
        //                totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
        //            }
        //            else
        //            {
        //                reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
        //            }
        //            totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

        //            sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
        //            sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

        //            // glName = string.Empty;

        //            // sheet.AutofitRow(3);



        //            row++;
        //        }

        //        formulaEndRow = row - 1;
        //        reportUtility.SetText(ref sheet, row, 5, "Total: ", true);

        //        if (companyCurrencyId != transcationCurrency)
        //        {
        //            //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
        //            //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
        //            //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
        //            //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
        //            //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

        //            sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
        //            sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
        //            sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
        //            sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
        //            sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
        //        }
        //        else
        //        {
        //            sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
        //            sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
        //            sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
        //        }

        //        sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
        //        sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

        //        row += 2;
        //        reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

        //        if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
        //        {
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
        //            // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

        //            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
        //            row++;

        //        }

        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //        // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
        //        sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

        //        //sheet.UsedRange.AutofitColumns();

        //        sheet.UsedRange.CellStyle.Font.Size = 8;
        //        row += 4;

        //        reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
        //        sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //        reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
        //        sheet[row, 1].ColumnWidth = 21;

        //        // reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["AddedBy"].ToString());
        //        sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //        reportUtility.SetTextMiddle(ref sheet, row, 3, "Received By", true);
        //        //sheet[row, 3].ColumnWidth = 15;



        //        reportUtility.SetSignatureText(ref sheet, row - 1, 5, header["PostedBy"].ToString());
        //        sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //        reportUtility.SetTextMiddle(ref sheet, row, 5, "Checked By", true);
        //        //sheet[row, 5].ColumnWidth = 15;

        //        sheet.Range[row, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //        reportUtility.SetTextMiddle(ref sheet, row, 7, "Authorized By", true);
        //        sheet[row, 6].ColumnWidth = 15;
        //        sheet[row, 7].ColumnWidth = 15;

        //        sheet[row, 8].ColumnWidth = 15;
        //        sheet[row, 9].ColumnWidth = 15;


        //        reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
        //        reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

        //        //    //else
        //        //    //{
        //        //    //    sheet.UsedRange.WrapText = true;
        //        //    //    sheet.UsedRange.CellStyle.Font.Size = 8;
        //        //    //    reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
        //        //    //    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
        //    }
        //    else
        //    {
        //        sheet.UsedRange.WrapText = true;
        //        sheet.UsedRange.CellStyle.Font.Size = 8;
        //        reportUtility.CompanyPlantHeader(ref sheet, 9, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
        //        reportUtility.PageSetup(ref sheet, 9, ExcelPageOrientation.Portrait);
        //    }

        //    return workbook;
        //}
    }


}