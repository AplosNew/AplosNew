using Library.Data.Sql;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Data;

namespace Library.Service.Reports
{
    public class InventoryReportService : IInventoryReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;

        public InventoryReportService(
            ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            )
        {
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
        }
        public IWorkbook GetInventoryReport(string companyGroupId, string companyId, string plantId, string materialId, string articleId)
        {
            try
            {
                var row = 4;
                var colLast = row;

                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();

                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);

                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];

                sheet.Name = "Inventory Report";
                var sheetHeader = "Inventory Report";

                int colMaterial = 0,colArticle = 0, colGRNDate = 0, colReceiveQty = 0, colReceiveRate = 0, colReceiveAmnt = 0, colIssueDate = 0, colIssueQty = 0, colIssueAmount = 0, colBalanceQty = 0, colBalanceAmnt = 0;

                var xlsCol = 1;
                var endXlsCol = 0;
                row++;
                // Set Header
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Material", ExcelHAlign.HAlignCenter); colMaterial = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Article", ExcelHAlign.HAlignCenter); colArticle = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GNR Date", ExcelHAlign.HAlignCenter); colGRNDate = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Receive Qty"); colReceiveQty = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Receive Rate"); colReceiveRate = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Receive Amnt"); colReceiveAmnt = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Issue Date"); colIssueDate = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Issue Qty"); colIssueQty = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Issue Amount"); colIssueAmount = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance Qty"); colBalanceQty = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance Amnt"); colBalanceAmnt = xlsCol; endXlsCol = xlsCol;
                // Set Row Header End
                row++;
                var formulaBalanceQuantity = "";
                var formulaBalanceAmount = "";

                var InventoryData = GetInventoryRecieveData(companyGroupId, companyId, plantId, materialId, articleId);

                var xlsTotalFormulaRow = row;
                
                    if (InventoryData.Rows.Count > 0)
                    {
                        for (int i = 0; i < InventoryData.Rows.Count; i++)
                        {                        
                            reportUtility.SetText(ref sheet, row, colMaterial, InventoryData.Rows[i]["MaterialMasterId"].ToString());
                            reportUtility.SetText(ref sheet, row, colArticle, InventoryData.Rows[i]["ArticleId"].ToString());
                            reportUtility.SetText(ref sheet, row, colGRNDate, InventoryData.Rows[i]["GRNDate"].ToString());
                            reportUtility.SetText(ref sheet, row, colReceiveQty, Convert.ToDouble(InventoryData.Rows[i]["RecieveQty"].ToString()));
                            reportUtility.SetText(ref sheet, row, colReceiveRate, Convert.ToDouble(InventoryData.Rows[i]["RecieveRate"].ToString()));
                            reportUtility.SetText(ref sheet, row, colReceiveAmnt, Convert.ToDouble(InventoryData.Rows[i]["RecieveAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colIssueDate, InventoryData.Rows[i]["IssueDate"].ToString());
                            reportUtility.SetText(ref sheet, row, colIssueQty, Convert.ToDouble(InventoryData.Rows[i]["IssueQty"].ToString()));
                            reportUtility.SetText(ref sheet, row, colIssueAmount,0 );//Convert.ToDouble(InventoryData.Rows[i]["IssueAmount"].ToString())

                            formulaBalanceQuantity = reportUtility.GetColumnNameForXls(colReceiveQty) + row + " - " + reportUtility.GetColumnNameForXls(colIssueQty) + row;

                            reportUtility.SetFormula(ref sheet, row, colBalanceQty, formulaBalanceQuantity,true);
                            formulaBalanceAmount = reportUtility.GetColumnNameForXls(colReceiveRate) + row + "*" + reportUtility.GetColumnNameForXls(colBalanceQty) + row;
                            reportUtility.SetFormula(ref sheet, row, colBalanceAmnt, formulaBalanceAmount,true);

                            row++;
                        }
                    }
                reportUtility.SetText(ref sheet, row, colGRNDate, "Total",true);

                var frmTotalRecieveQty = "=sum("+ reportUtility.GetColumnNameForXls(colReceiveQty) + xlsTotalFormulaRow + ":" + reportUtility.GetColumnNameForXls(colIssueQty) + (row-1)+")";
                var frmTotalRecieveRate = "=sum(" + reportUtility.GetColumnNameForXls(colReceiveRate) + xlsTotalFormulaRow + ":" + reportUtility.GetColumnNameForXls(colReceiveRate) + (row - 1) + ")";
                var frmTotalRecieveAmnt = "=sum(" + reportUtility.GetColumnNameForXls(colReceiveAmnt) + xlsTotalFormulaRow + ":" + reportUtility.GetColumnNameForXls(colReceiveAmnt) + (row - 1) + ")";
                var frmTotalIssueQty = "=sum(" + reportUtility.GetColumnNameForXls(colIssueQty) + xlsTotalFormulaRow + ":" + reportUtility.GetColumnNameForXls(colIssueQty) + (row - 1) + ")";
                var frmTotalIssueAmt = "=sum(" + reportUtility.GetColumnNameForXls(colIssueAmount) + xlsTotalFormulaRow + ":" + reportUtility.GetColumnNameForXls(colIssueAmount) + (row - 1) + ")";
                var frmTotalBalanceQty = "=sum(" + reportUtility.GetColumnNameForXls(colBalanceQty) + xlsTotalFormulaRow + ":" + reportUtility.GetColumnNameForXls(colBalanceQty) + (row - 1) + ")";
                var frmTotalBalanceAmnt = "=sum(" + reportUtility.GetColumnNameForXls(colBalanceAmnt) + xlsTotalFormulaRow + ":" + reportUtility.GetColumnNameForXls(colBalanceAmnt) + (row - 1) + ")";

                reportUtility.SetFormula(ref sheet, row, colReceiveQty, frmTotalRecieveQty, true);
                reportUtility.SetFormula(ref sheet, row, colReceiveRate, frmTotalRecieveRate, true);
                reportUtility.SetFormula(ref sheet, row, colReceiveAmnt, frmTotalRecieveAmnt, true);
                reportUtility.SetFormula(ref sheet, row, colIssueQty, frmTotalIssueQty, true);
                reportUtility.SetFormula(ref sheet, row, colIssueAmount, frmTotalIssueAmt, true);
                reportUtility.SetFormula(ref sheet, row, colBalanceQty, frmTotalBalanceQty, true);
                reportUtility.SetFormula(ref sheet, row, colBalanceAmnt, frmTotalBalanceAmnt, true);



                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;

                if (!string.IsNullOrEmpty(plantId))
                    reportUtility.PlantHeader(ref sheet, colBalanceAmnt, sheetHeader, plantId);
                else
                    reportUtility.MainCompanyGroupHeader(ref sheet, colBalanceAmnt, sheetHeader, companyGroupId);
                //sheet.Range[reportUtility.GetColumnNameForXls(colArticle) + 5 + ":" + reportUtility.GetColumnNameForXls(endXlsCol)].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private DataTable GetInventoryRecieveData(string companyGroupId, string companyId, string plantId, string materialId, string articleId)
        {
            var article = string.Empty;
            if (articleId != "null")
            {
                article = "AND IM.ArticleId = '" + articleId + @"'";

            }

            var cmdText = @"SELECT IM.MaterialMasterId, IM.ArticleId ArticleId, IRD.Id
	                            , REPLACE(CONVERT(VARCHAR(11), IR.[GRNDate], 106), ' ', '-') AS GRNDate	
	                            , IRD.BaseQty AS RecieveQty
	                            , IRD.BaseAmount/IRD.BaseQty AS RecieveRate
	                            , ISNULL(IRD.BaseAmount,0) AS RecieveAmount
	                            , ISNULL(IRD.IssueQty, 0) AS IssueQty
	                            , ISNULL(IssueAmount, 0) AS IssueAmount
	                            , REPLACE(CONVERT(VARCHAR(11), IIS.IssueDate, 106), ' ', '-') IssueDate 
                            FROM [TRN].[InventoryMaterial] IM 
                            Left JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IM.Id = IRD.InventoryMaterialId 
                            JOIN [TRN].[InventoryReceive] AS IR ON IR.Id = IRD.InventoryReceiveId	
                            LEFT JOIN (SELECT (SUM(Qty*Rate)/SUM(Qty)) AS IssueAmount, InventoryReceiveDetailId, InventoryIssueDetailId FROM [TRN].[InventoryIssueHistory] GROUP BY InventoryReceiveDetailId, InventoryIssueDetailId)
	                            AS IH ON IH.InventoryReceiveDetailId = IRD.Id
                            LEFT JOIN [TRN].[InventoryIssueDetail] AS IISD ON IH.InventoryIssueDetailId = IISD.Id
                            LEFT JOIN TRN.[InventoryIssue] AS IIS ON IISD.InventoryIssueId=IIS.Id
                            WHERE IM.CompanyGroupId = '"+ companyGroupId + @"' AND IM.CompanyId = '"+ companyId + @"' AND IM .PlantId = '"+plantId+ @"' AND MaterialMasterId='" + materialId + @"' "+article+@"
                            GROUP BY IM.[CompanyGroupId], IM.[CompanyId], IM.[PlantId], IR.[GRNDate], IRD.BaseQty,IRD.IssueQty,IRD.BaseAmount, IRD.BaseQty,IM.ArticleId, IM.MaterialMasterId,IRD.ID
                            , IssueAmount,IIS.IssueDate";     
            return _sqlRepository.GetDataTable(cmdText);
        }
    }
}