using Aplos.Controllers;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Advances;
using Library.Service.Banks;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Finances;
using Library.Service.FixedAssets;
using Library.Service.Invoices;
using Library.Service.OpeningBalances;
using Library.Service.Organizations;
using Library.MaterialManagement.Reports;
using Library.Service.SalesManagements;
using Library.Service.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Accounting.FixedAssets;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using System.Data;
using OTSBD;
using Library.Service.Helpers;

namespace Aplos.Areas.Products.Controllers
{
    public class LandedcostreportController : BaseController
    {

        private readonly ISqlRepository _sqlRepository;
       
        public LandedcostreportController(ISalesService salesService,
              ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }


        public ActionResult Report()
        {
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult GetLandedCostReport(ReportFormat reportFormat, DateTime fromdate, DateTime todate, string reportType)
        {
            string reportFileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Syncfusion.XlsIO.IWorkbook workbook = null;
            if (reportType == "GRN Landed Cost")
            {
                workbook = GetGRNLandedCostReport(out reportFileName, identity.PlantId, fromdate, todate);
            }
            else
            {
                workbook = GetGRNLandedCostReport(out reportFileName, identity.PlantId, fromdate, todate);
            }
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        public IWorkbook GetGRNLandedCostReport(out string reportFileName, string plantId, DateTime fromDate, DateTime toDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtGRNLandedCostData = GetGRNLandedCostData(plantId, fromDate, toDate);

            worksheet.Name = "GRN Landed Cost Report";
            reportFileName = "GRN Landed Cost Report ";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            //worksheet.Range[ROW - 1, 3].Text = "Posting Date:  From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            worksheet[ROW, COL].Text = "Inventory Receive No";
            int colInventoryReceiveNo = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Particular";
            int colParticular = COL;
            worksheet[ROW, COL].ColumnWidth = 35;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Ref No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Gate Entry No";
            int colGateEntryNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Gate Entry Name";
            int colGateEntryName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Entry Date";
            int colEntryDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrencyCode = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Invoicing By";
            int colInvoicingBy = COL;
            worksheet[ROW, COL].ColumnWidth = 35;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Base Amount";
            int colBaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Expenses Amount";
            int colExpensesAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Landed Cost";
            int colLandedCost = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Invoicing State";
            int colInvoicingState = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Delivery State";
            int colDeliveryState = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Payment Term Name";
            int colPaymentTermName = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Payment Mode";
            int colPaymentMode = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN Type";
            int colGRNType = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtGRNLandedCostData.Rows.Count; i++)
            {
                worksheet[ROW, colInventoryReceiveNo].Text = dtGRNLandedCostData.Rows[i]["Id"].ToString();
                worksheet[ROW, colGRNDate].Text = dtGRNLandedCostData.Rows[i]["GRNDate"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtGRNLandedCostData.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colParticular].Text = dtGRNLandedCostData.Rows[i]["Particular"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtGRNLandedCostData.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colGateEntryNo].Text = dtGRNLandedCostData.Rows[i]["GateEntryNo"].ToString();

                worksheet[ROW, colGateEntryName].Text = dtGRNLandedCostData.Rows[i]["GateEntryName"].ToString();
                worksheet[ROW, colEntryDate].Text = dtGRNLandedCostData.Rows[i]["EntryDate"].ToString();
                worksheet[ROW, colCurrencyCode].Text = dtGRNLandedCostData.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colInvoicingBy].Text = dtGRNLandedCostData.Rows[i]["InvoicingBy"].ToString();

                worksheet[ROW, colBaseAmount].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["BaseAmount"].ToString());
                worksheet[ROW, colBaseAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colExpensesAmount].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["ExpensesAmount"].ToString());
                worksheet[ROW, colExpensesAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colLandedCost].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["LandedCost"].ToString());
                worksheet[ROW, colLandedCost].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colInvoicingState].Text = dtGRNLandedCostData.Rows[i]["InvoicingState"].ToString();
                worksheet[ROW, colDeliveryState].Text = dtGRNLandedCostData.Rows[i]["DeliveryState"].ToString();
                worksheet[ROW, colPaymentTermName].Text = dtGRNLandedCostData.Rows[i]["PaymentTermName"].ToString();
                worksheet[ROW, colPaymentMode].Text = dtGRNLandedCostData.Rows[i]["PaymentMode"].ToString();
                worksheet[ROW, colGRNType].Text = dtGRNLandedCostData.Rows[i]["GRNType"].ToString();


                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "GRN Landed Cost Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }

        public DataTable GetGRNLandedCostData(string plantId, DateTime fromDate, DateTime toDate)
        {
            var cmdText = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate,v.VoucherNo
                                , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                            , IR.DocRefNo
	                            , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
								, CU.Code AS CurrencyCode
								, IPP.UserName AS InvoicingBy
	                            , IRD.BaseAmount,isnull(IDC.ExpensesAmount,0) ExpensesAmount,LandedCost=IRD.BaseAmount+isnull(IDC.ExpensesAmount,0)
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName,PT.PaymentMode
								,IR.GRNType
								
                    FROM [TRN].[InventoryReceive] AS IR 
					LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                   
                    LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
					LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
                    LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount 
					FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    LEFT JOIN TRN.Invoice I ON I.InventoryReceiveId=IR.Id
					 LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IR.VoucherId
					LEFT JOIN (SELECT InvoiceId,SUM(DistributedAmount) ExpensesAmount FROM trn.InvoiceDetailCharges where InvoiceType='InboundInvoice' GROUP BY InvoiceId) IDC ON IDC.InvoiceId=I.Id
					WHERE IR.PlantId='" + plantId + @"' AND CONVERT(DATE, IR.GRNDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"'
                    --and P.UserName='JAIN TRADERS' --AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND IR.PlantId='202034' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL";
            return _sqlRepository.GetDataTable(cmdText);
        }
    }
}