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

            worksheet[ROW, COL].Text = "Particular";
            int colParticular = COL;
            worksheet[ROW, COL].ColumnWidth = 35;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Tax Invoice No";
            int colTaxInvoiceNo = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Tax Invoice Date";
            int colTaxInvoiceDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Line";
            int colLine = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Material";
            int colMaterial = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticle = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "UoM";
            int colUoM = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "GRN Number";
            int colGRNNumber = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Qty";
            int colQty = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "IGST";
            int colIGST = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "CGST";
            int colCGST = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SGST";
            int colSGST = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN Other Charges";
            int colGRNOtherCharges = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Distribution Expenses";
            int colDistributionExpenses = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            worksheet[ROW, COL].Text = "GST Non";
            int colGSTNon = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Igst Third Party";
            int colIgstThirdParty = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Non Rec Taxes Third Party";
            int colNonRecTaxesThirdParty = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Total GRN Amount";
            int colTotalGRNAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "LandedCost";
            int colLandedCost = COL;
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
                worksheet[ROW, colParticular].Text = dtGRNLandedCostData.Rows[i]["Particular"].ToString();
                worksheet[ROW, colTaxInvoiceNo].Text = dtGRNLandedCostData.Rows[i]["TaxInvoiceNo"].ToString();
                worksheet[ROW, colTaxInvoiceDate].Text = dtGRNLandedCostData.Rows[i]["TaxInvoiceDate"].ToString();
                worksheet[ROW, colLine].Text = dtGRNLandedCostData.Rows[i]["Line"].ToString();
                worksheet[ROW, colGRNDate].Text = dtGRNLandedCostData.Rows[i]["GRNDate"].ToString();
                worksheet[ROW, colMaterial].Text = dtGRNLandedCostData.Rows[i]["Material"].ToString();

                worksheet[ROW, colArticle].Text = dtGRNLandedCostData.Rows[i]["Article"].ToString();
                worksheet[ROW, colUoM].Text = dtGRNLandedCostData.Rows[i]["UoM"].ToString();
                worksheet[ROW, colGRNNumber].Text = dtGRNLandedCostData.Rows[i]["GRNNumber"].ToString();

                worksheet[ROW, colQty].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["Qty"].ToString());
                worksheet[ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["Amount"].ToString());
                worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colIGST].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["IGST"].ToString());
                worksheet[ROW, colIGST].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colCGST].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["CGST"].ToString());
                worksheet[ROW, colCGST].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colSGST].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["SGST"].ToString());
                worksheet[ROW, colSGST].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colGRNOtherCharges].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["GRNOtherCharges"].ToString());
                worksheet[ROW, colGRNOtherCharges].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colDistributionExpenses].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["DistributionExpenses"].ToString());
                worksheet[ROW, colDistributionExpenses].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colGSTNon].Text = dtGRNLandedCostData.Rows[i]["GSTNon"].ToString();
                worksheet[ROW, colIgstThirdParty].Text = dtGRNLandedCostData.Rows[i]["IgstThirdParty"].ToString();
                worksheet[ROW, colNonRecTaxesThirdParty].Text = dtGRNLandedCostData.Rows[i]["NonRecTaxesThirdParty"].ToString();
                worksheet[ROW, colTotalGRNAmount].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["TotalGRNAmount"].ToString());
                worksheet[ROW, colTotalGRNAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colLandedCost].Number = clsStaticInfo.dbl(dtGRNLandedCostData.Rows[i]["LandedCost"].ToString());
                worksheet[ROW, colLandedCost].NumberFormat = clsStaticInfo.NumberFormat(2);
                
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
            var cmdText = @"SELECT   Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
,ir.DocRefNo TaxInvoiceNo,  REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS TaxInvoiceDate,'' Line,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
,MM.UserName Material,MMA.StandardName Article
,UoM.UserName UoM,IRD.InventoryReceiveId GRNNumber,IRD.TransactionQty Qty, Amount=IRD.TotalMaterialBooksCurrencyAmount
                                ,ISNULL(IGST.IGSTAmount,0) IGST,ISNULL(CGST.CGSTAmount,0)CGST,ISNULL(SGST.SGSTAmount,0)SGST
								,GRNOtherCharges=ird.ChargesTranAmount
	                            ,(isnull(IDC.ExpensesAmount,0)*ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0))/I.Amount DistributionExpenses
								,ir.IsNonCreditable GSTNon,'' IgstThirdParty,'' NonRecTaxesThirdParty
								, TotalGRNAmount=IRD.TotalMaterialBooksCurrencyAmount+ISNULL(IGST.IGSTAmount,0)+ISNULL(CGST.CGSTAmount,0)+ISNULL(SGST.SGSTAmount,0)
								, LandedCost=IRD.TotalMaterialBooksCurrencyAmount+isnull((isnull(IDC.ExpensesAmount,0)*ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0))/I.Amount,0)
                    FROM TRN.InventoryReceiveDetail IRD 
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
					LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
					LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
					LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
					LEFT JOIN MST.MaterialMaster MM ON MM.Id=MMA.MaterialMasterId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN (SELECT IT.InventoryReceiveDetailId,SUM(ISNULL(IT.TaxAmount,0)) IGSTAmount FROM [TRN].InventoryReceiveTax IT 
										LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
										WHERE TC.Code='IGST'
										group by IT.InventoryReceiveDetailId
										)IGST ON IGST.InventoryReceiveDetailId=IRD.Id
					LEFT JOIN (SELECT IT.InventoryReceiveDetailId,SUM(ISNULL(IT.TaxAmount,0)) CGSTAmount FROM [TRN].InventoryReceiveTax IT 
										LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
										WHERE TC.Code='CGST'
										group by IT.InventoryReceiveDetailId
										)CGST ON CGST.InventoryReceiveDetailId=IRD.Id
					LEFT JOIN (SELECT IT.InventoryReceiveDetailId,SUM(ISNULL(IT.TaxAmount,0)) SGSTAmount FROM [TRN].InventoryReceiveTax IT 
										LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
										WHERE TC.Code='SGST'
										group by IT.InventoryReceiveDetailId
										)SGST ON SGST.InventoryReceiveDetailId=IRD.Id
                    LEFT JOIN TRN.Invoice I ON I.InventoryReceiveId=IR.Id
					 LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IR.VoucherId
					LEFT JOIN (SELECT InvoiceId,SUM(DistributedAmount) ExpensesAmount FROM trn.InvoiceDetailCharges where InvoiceType='InboundInvoice' GROUP BY InvoiceId) IDC ON IDC.InvoiceId=I.Id

					WHERE IR.PlantId='" + plantId + @"' AND CONVERT(DATE, IR.GRNDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"'";
                    
            return _sqlRepository.GetDataTable(cmdText);
        }
    }
}