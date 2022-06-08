#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Costings;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class BOQStatusReportController : BaseController
    {


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BOQStatusReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        //Current Fund Position start//

        [HttpGet]
        public ActionResult getCurrentFundPositionlist(DateTime PostingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT ROW_NUMBER() OVER (ORDER BY  AccountTitle) AS SLNo,Category,AccountTitle Bank_CashName,AccountNumber,Currency,SUM(DrAmount) - SUM(CrAmount) AS Amount
                         ,LimitAmount,0 TotalAvailableAmount,'' Remark,PDCOverDue,PDCInNext_7_Days,0 PaymentOverdue,0 PaymentOverdueInNext_7_Days
						 ,0 Surplus_Short_AsOnDate,0 Short_SurplusInNext_7_Days
						FROM (
                        SELECT  'Bank' Category,BM.AccountTitle,BM.AccountNumber,CU.Code Currency,BM.LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,PDCOverDue=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<0 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
                         ,PDCInNext_7_Days=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)>=0 AND DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<7 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
						 
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
						LEFT JOIN TRN.PostDepositCheque PDC ON PDC.BankMasterId=BM.Id
						LEFT JOIN SCS.Currency CU ON CU.Id=BM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'
						AND V.PostingDate <= '" + PostingDate + @"' and VD.BankMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,BM.AccountTitle,BM.AccountNumber,CU.Code,BM.LimitAmount,PDC.PostingDate,VD.BankMasterId,PDC.Amount

						UNION
						SELECT 'Cash' Category,CM.UserName AccountTitle, '' AccountNumber,CU.Code Currency,0 LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,0 PDCOverDue,0PDCInNext_7_Days
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.CashMaster CM ON CM.Id=VD.CashMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=CM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.CashMasterId=VD.CashMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' 
						AND V.PostingDate <= '" + PostingDate + @"' and VD.CashMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,CM.UserName,CU.Code
						  ) AS X GROUP BY X.CompanyCurrencyId,X.AccountTitle,x.Currency,x.LimitAmount,x.Category,x.AccountNumber,x.PDCOverDue,X.PDCInNext_7_Days";

            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetCurrentFundPositionReport(DateTime PostingDate)
        {
            try
            {
                //AccountsBankService accountsBankService = new AccountsBankService(_sqlRepository);
                string fileName = "";
                fileName = CurrentFundPositionReport(PostingDate, "Post Date Cheque Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public string CurrentFundPositionReport(DateTime PostingDate, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "CurrentFundPositionReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                CurrentFundPositionSQL(PostingDate, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "SL No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSLNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank CashName";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBank_CashName = COL;
                COL++;

                sheet[ROW, COL].Text = "Account Number";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAccountNumber = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Limit Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLimitAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Available Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTotalAvailableAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "PDC Over Due";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPDCOverDue = COL;
                COL++;

                sheet[ROW, COL].Text = "PDC In Next 7 Days";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPDCInNext_7_Days = COL;
                COL++;

                //sheet[ROW, COL].Text = "Payment Over Due";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColPaymentOverdue = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Payment Over Due In Next 7 Days";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColPaymentOverdueInNext_7_Days = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Surplus Short As On Date";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSurplus_Short_AsOnDate = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Short Surplus In Next 7 Days";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColShort_SurplusInNext_7_Days = COL;
                //COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColSLNo].Text = data.Rows[i]["SLNo"].ToString();
                    sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                    sheet[ROW, ColBank_CashName].Text = data.Rows[i]["Bank_CashName"].ToString();
                    sheet[ROW, ColAccountNumber].Text = data.Rows[i]["AccountNumber"].ToString();
                    sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
                    sheet[ROW, ColLimitAmount].Number = clsStaticInfo.dbl(data.Rows[i]["LimitAmount"].ToString());
                    sheet[ROW, ColTotalAvailableAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalAvailableAmount"].ToString());
                    sheet[ROW, ColPDCOverDue].Number = clsStaticInfo.dbl(data.Rows[i]["PDCOverDue"].ToString());
                    sheet[ROW, ColPDCInNext_7_Days].Number = clsStaticInfo.dbl(data.Rows[i]["PDCInNext_7_Days"].ToString());
                    //sheet[ROW, ColPaymentOverdue].Number = clsStaticInfo.dbl(data.Rows[i]["PaymentOverdue"].ToString());
                    //sheet[ROW, ColPaymentOverdueInNext_7_Days].Number = clsStaticInfo.dbl(data.Rows[i]["PaymentOverdueInNext_7_Days"].ToString());
                    //sheet[ROW, ColSurplus_Short_AsOnDate].Number = clsStaticInfo.dbl(data.Rows[i]["Surplus_Short_AsOnDate"].ToString());
                    //sheet[ROW, ColShort_SurplusInNext_7_Days].Number = clsStaticInfo.dbl(data.Rows[i]["Short_SurplusInNext_7_Days"].ToString());
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remark"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Current Fund Position Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CurrentFundPositionSQL(DateTime PostingDate, out DataTable data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string strSQL = @"SELECT ROW_NUMBER() OVER (ORDER BY  AccountTitle) AS SLNo,Category,AccountTitle Bank_CashName,AccountNumber,Currency,SUM(DrAmount) - SUM(CrAmount) AS Amount
                         ,LimitAmount,0 TotalAvailableAmount,'' Remark,PDCOverDue,PDCInNext_7_Days,0 PaymentOverdue,0 PaymentOverdueInNext_7_Days
						 ,0 Surplus_Short_AsOnDate,0 Short_SurplusInNext_7_Days
						FROM (
                        SELECT  'Bank' Category,BM.AccountTitle,BM.AccountNumber,CU.Code Currency,BM.LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,PDCOverDue=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<0 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
                         ,PDCInNext_7_Days=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)>=0 AND DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<7 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
						 
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
						LEFT JOIN TRN.PostDepositCheque PDC ON PDC.BankMasterId=BM.Id
						LEFT JOIN SCS.Currency CU ON CU.Id=BM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'
						--AND VD.BankMasterId='20199' 
						AND V.PostingDate <= '" + PostingDate + @"' and VD.BankMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,BM.AccountTitle,BM.AccountNumber,CU.Code,BM.LimitAmount,PDC.PostingDate,VD.BankMasterId,PDC.Amount

						UNION
						SELECT 'Cash' Category,CM.UserName AccountTitle, '' AccountNumber,CU.Code Currency,0 LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,0 PDCOverDue,0PDCInNext_7_Days
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.CashMaster CM ON CM.Id=VD.CashMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=CM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.CashMasterId=VD.CashMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' 
						--AND VD.BankMasterId='20199' 
						AND V.PostingDate <= '" + PostingDate + @"' and VD.CashMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,CM.UserName,CU.Code
						  ) AS X GROUP BY X.CompanyCurrencyId,X.AccountTitle,x.Currency,x.LimitAmount,x.Category,x.AccountNumber,x.PDCOverDue,X.PDCInNext_7_Days";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        //Current Fund Position end//

    }

}