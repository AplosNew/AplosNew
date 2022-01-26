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
using Library.Service.Currencies;
using Library.Service.Finances;
using Library.Service.Helpers;
using Library.Service.Organizations;
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
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

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


        [HttpGet, Authorize]
        public ActionResult GetAutoLoanReport(ReportFormat reportFormat, string LCId,string LoanAgainstAcceptanceMasterId, string SourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetAutoLoanReportFormat(out string reportFileName, LCId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, LoanAgainstAcceptanceMasterId, SourceType);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return View();
            }
        }


        public IWorkbook GetAutoLoanReportFormat(out string reportFileName, string lcId, string companyGroupId, string companyId, string plantId ,string LoanAgainstAcceptanceMasterId,string SourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetAutoLoanHeader();

            reportFileName = "Auto Loan Report";

            DataTable data = GetAutoLoanQuery(LoanAgainstAcceptanceMasterId, SourceType);

            int ROW = 6;
            int COL = 1;


            //sheet[ROW, COL].Text = "LoanNo";
            //sheet[ROW, COL].ColumnWidth = 10;
            //int colLoanNo = COL;
            //COL++;

            //sheet[ROW, COL].Text = "Loan Date";
            //sheet[ROW, COL].ColumnWidth = 10;
            //int colNewLoanDate = COL;
            //COL++;
            //sheet[ROW, COL].Text = "Source Type";
            //sheet[ROW, COL].ColumnWidth = 10;
            //int colSourceType = COL;
            //COL++;
            //sheet[ROW, COL].Text = "LC No";
            //sheet[ROW, COL].ColumnWidth = 14;
            //int colPurchaseLCNo = COL;
            //COL++;

            //sheet[ROW, COL].Text = "Bank Master";
            //sheet[ROW, COL].ColumnWidth = 10;
            //int colAccountTitle = COL;
            //COL++;
            //sheet[ROW, COL].Text = "Currency";
            //sheet[ROW, COL].ColumnWidth = 10;
            //int colCurrencyCode = COL;
            //COL++;
            //sheet[ROW, COL].Text = "Amount";
            //sheet[ROW, COL].ColumnWidth = 10;
            //int colAmount = COL;
            sheet[ROW, COL].Text = "Acceptance / Invoice No.";
            sheet[ROW, COL].ColumnWidth = 15;
            int colAcceptanceNo = COL;
            COL++;
            
            sheet[ROW, COL].Text = "Amount";
            sheet[ROW, COL].ColumnWidth = 15;
            int colAmount = COL;

            int endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

            var startRow = 0;
            int RowIndex = ROW;
            int StartRow = ROW; //row 20
            ROW++;


            for (int i = 0; i < data.Rows.Count; i++)
            {

                //sheet[ROW, colLoanNo].Text = data.Rows[i]["LoanNo"].ToString();
                //sheet[ROW, colNewLoanDate].Text = data.Rows[i]["date"].ToString();
                //sheet[ROW, colSourceType].Text = data.Rows[i]["SourceType"].ToString();
                //sheet[ROW, colPurchaseLCNo].Text = data.Rows[i]["PurchaseLCNo"].ToString();
                //sheet[ROW, colAccountTitle].Text = data.Rows[i]["AccountTitle"].ToString();
                //sheet[ROW, colCurrencyCode].Text = data.Rows[i]["CurrencyCode"].ToString();
                sheet[ROW, colAcceptanceNo].Text = data.Rows[i]["AcceptanceNo"].ToString();
                sheet[ROW, colAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
                sheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            //sheet.Range[StartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[StartRow, colPOValue, ROW, colPOValue].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[StartRow, colAcceptanceValue, ROW, colAcceptanceValue].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[StartRow, colGRNValue, ROW, colGRNValue].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.IsGridLinesVisible = false;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet["A" + StartRow.ToString()].FreezePanes();


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Auto Loan", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;


            return workbook;
        }

        private Dictionary<string, object> GetAutoLoanHeader()
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string strSQL = string.Empty;
                strSQL = @"SELECT * FROM
						(SELECT 'Acceptance' SourceType,LAA.Id LoanAgainstAcceptanceId,LAA.Id, LAA.CompanyGroupId, LAA.CompanyId, LAA.PlantId, LAA.EntityId, LAA.CurrencyId, 
						LAA.VoucherId, LAA.PartyType, LAA.PartyId, LAA.PartyPlantId, LAA.TransactionType, LAA.PaymentSource, LAA.LoanDate, 
						LAA.LoanNo, LAA.Amount, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,(SELECT TOP 1  LAAD.BankMasterId
						FROM LoanAgainstAcceptanceDetail LAAD 
						LEFT JOIN TRN.PurchasedocAcceptance AS PDA ON PDA.Id=LAAD.PurchasedocAcceptanceId
						WHERE LAAD.LoanAgainstAcceptanceMasterId=LAA.Id) BankMasterId
						,(SELECT TOP 1  BM.AccountTitle 
						FROM LoanAgainstAcceptanceDetail LAAD 
						LEFT JOIN TRN.PurchasedocAcceptance AS PDA ON PDA.Id=LAAD.PurchasedocAcceptanceId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.BankMasterId
						WHERE LAAD.LoanAgainstAcceptanceMasterId=LAA.Id)AccountTitle
						,PurchaseLCNo= ISNULL(STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON XP.Id=LAAD.PurchaseDocAcceptanceId
													where	LAAD.LoanAgainstAcceptanceMasterId=LAA.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),(STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD 
														LEFT JOIN TRN.Invoice I ON XVD.Id=I.PurchaseLCId
														LEFT JOIN LoanAgainstAcceptanceDetail LAADI ON I.Id=LAADI.InvoiceId
													where	LAADI.LoanAgainstAcceptanceMasterId=LAA.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')))
							,PINo= STUFF((select distinct ','+XVD.PINo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON XP.Id=LAAD.PurchaseDocAcceptanceId
													where	LAAD.LoanAgainstAcceptanceMasterId=LAA.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						
						FROM LoanAgainstAcceptanceMaster LAA 
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.IsPark=1  AND LAA.VoucherId IS NULL
						UNION ALL
						SELECT 'Invoice' SourceType, LAA.Id LoanAgainstAcceptanceId,LAA.Id, LAA.CompanyGroupId, LAA.CompanyId, LAA.PlantId, LAA.EntityId, LAA.CurrencyId, LAA.VoucherId, 'Vendor' PartyType,LAA.PartyId, LAA.PartyPlantId,'LoanTaken' TransactionType,'Bank' PaymentSource , LAA.LoanDate, LAA.LoanNo, LAA.Amount, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,LAA.BankMasterId, BM.AccountTitle, XVD.LCRef,XVD.PINo
						FROM InvoiceTaggingWithLCMaster LAA 
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN dbo.PurchaseLC XVD ON XVD.Id=LAA.PurchaseLCId
						WHERE LAA.IsLoan=1 AND  
						 LAA.VoucherId IS NULL)X
						WHERE X.PlantId='" + identity.PlantId + "'";
                return _sqlRepository.GetData(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private DataTable GetAutoLoanQuery(string LoanAgainstAcceptanceMasterId, string SourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            if (SourceType == "Acceptance")
            {
                sql = @"SELECT LAA.Id LoanAgainstAcceptanceId,LAA.CurrencyId, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,IVD.GLGeneralInfoId,IVD.BudgetMasterId,IVD.ActivityId,IVD.InvoiceId,IVD.Id InvoiceDetailId,IV.Amount
						,IV.CompanyCurrencyRate,BM.AccountTitle 
						,PDA.AcceptanceNo,LAAD.BankMasterId
						FROM LoanAgainstAcceptanceMaster LAA 
						LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON LAA.Id=LAAD.LoanAgainstAcceptanceMasterId
						INNER JOIN TRN.PurchasedocAcceptance AS PDA ON PDA.Id=LAAD.PurchasedocAcceptanceId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.BankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN TRN.Invoice IV ON IV.PurchaseDocAcceptanceId=LAAD.PurchaseDocAcceptanceId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.IsPark=1 AND LAA.PlantId='" + identity.PlantId + "' AND LAAD.LoanAgainstAcceptanceMasterId='" + LoanAgainstAcceptanceMasterId + @"'  AND LAA.VoucherId IS NULL 
						UNION ALL 
						SELECT LAA.Id LoanAgainstAcceptanceId,LAA.CurrencyId, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,IVD.GLGeneralInfoId,IVD.BudgetMasterId,IVD.ActivityId,IVD.InvoiceId,IVD.Id InvoiceDetailId,IV.Amount
						,IV.CompanyCurrencyRate,BM.AccountTitle 
						,IV.DocRefNo  AcceptanceNo,LAAD.BankMasterId
						FROM LoanAgainstAcceptanceMaster LAA 
						LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON LAA.Id=LAAD.LoanAgainstAcceptanceMasterId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.BankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN TRN.Invoice IV ON IV.Id=LAAD.InvoiceId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.IsPark=1 AND LAA.PlantId='" + identity.PlantId + "' AND LAAD.LoanAgainstAcceptanceMasterId='" + LoanAgainstAcceptanceMasterId + @"'  AND LAA.VoucherId IS NULL";
            }
            else
            {
                sql = @"SELECT LAA.Id LoanAgainstAcceptanceId,LAA.CurrencyId, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,IVD.GLGeneralInfoId,IVD.BudgetMasterId,IVD.ActivityId,IVD.InvoiceId,IVD.Id InvoiceDetailId,IV.Amount
						,IV.CompanyCurrencyRate,BM.AccountTitle 
						,IV.DocRefNo  AcceptanceNo,LAA.BankMasterId
						FROM InvoiceTaggingWithLCMaster LAA 
						LEFT JOIN InvoiceTaggingWithLCDetail LAAD ON LAA.Id=LAAD.InvoiceTaggingWithLCMasterId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN TRN.Invoice IV ON IV.Id=LAAD.InvoiceId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.PlantId='" + identity.PlantId + "' AND LAAD.InvoiceTaggingWithLCMasterId='" + LoanAgainstAcceptanceMasterId + "'  AND LAA.VoucherId IS NULL";
            }

            return _sqlRepository.GetDataTable(sql);
        }
    }


}