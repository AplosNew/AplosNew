using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Properties;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.Model.Currencies;

namespace Library.Accounting.FixedAssets
{
    public class FixedAssetReportService
    {
        private readonly ISqlRepository _sqlRepository;
        public FixedAssetReportService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository; 

        }

        #region GL vs FA
        public IWorkbook GLVSfaReport(ExcelEngine excelEngine, string companyId, string plantId) 
        {
            excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                worksheet.Name = "GL VS FA";

                int COL = 1; int ROW = 6;
                int startCol = COL;
                worksheet[ROW, COL].Text = "SL. No";
                int colSLNO = COL;
                worksheet[ROW, COL].ColumnWidth = 7;
                COL++;

                worksheet[ROW, COL].Text = "FA Master Id";
                //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colFixedAssetMasterId = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Fixed Asset";
                int colFixedAsset = COL;
                worksheet[ROW, COL].ColumnWidth = 35;
                COL++;

                worksheet[ROW, COL].Text = "GL";
                int colGLName = COL;
                worksheet[ROW, COL].ColumnWidth = 35;
                COL++;

                worksheet[ROW, COL].Text = "Budget Master Id";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colBudgetMasterId = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++; 

                worksheet[ROW, COL].Text = "Budget";
               // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBudgetName = COL;
                worksheet[ROW, COL].ColumnWidth = 35;
                COL++;

                worksheet[ROW, COL].Text = "Activity Id";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colActivityId = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Activity";
                int colActivityName = COL;
                worksheet[ROW, COL].ColumnWidth = 35;
                COL++;

                worksheet[ROW, COL].Text = "GL Amount";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGLAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Register Amount";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRegisterAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "SubAsset Amount";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSubAssetAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Total Reg. Amount";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalRegisterAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Diffrence";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDiffrence = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
              //  COL++;

                int endCol = COL;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Size = 12;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Bold = true;

                //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                worksheet.Range[ROW, startCol, ROW, COL].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, startCol, ROW, COL].BorderInside(ExcelLineStyle.Hair);

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                string sql = @"SELECT X.FixedAsset,x.FixedAssetMasterId,X.GLName,X.BudgetName,X.ActivityName,X.BudgetMasterId,X.ActivityId
                    ,SUM(X.GLAmount) GLAmount
                    ,ISNULL( SUM(X.RegisterAmount),0) RegisterAmount
                    ,ISNULL( SUM(X.SubAssetAmount),0)SubAssetAmount 
                    ,TotalRegisterAmount=ISNULL( (SUM(X.RegisterAmount)+SUM(X.SubAssetAmount)),0)
                    ,Diffrence=ISNULL( SUM(X.GLAmount)-(SUM(X.RegisterAmount)+SUM(X.SubAssetAmount)),0)
                    FROM (

                    SELECT FAM.UserName FixedAsset,FAM.Id FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,VD.BudgetMasterId,VD.ActivityId
                    ,ISNULL( SUM(VDC.DrAmount)-SUM(VDC.CrAmount),0) GLAmount
                    ,0 RegisterAmount
                    ,0 SubAssetAmount
 
                    FROM TRN.VoucherDetail VD 
                    join trn.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                    LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                    LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GL.Id
                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=VD.BudgetMasterId
                    LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId

                    WHERE GLAT.AccountType='Asset' 
                    GROUP BY FAM.UserName,
                    GL.UserName ,B.UserName ,A.UserName ,VD.BudgetMasterId,VD.ActivityId,FAM.Id

                    UNION ALL

                    SELECT    FAM.UserName FixedAsset,FAR.FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,FAR.FABudgetMasterId BudgetMasterId,FAR.FAActivityId ActivityId
                    ,0 GLAmount
                    ,ISNULL( FAR.FABaseAmount,0) RegisterAmount
                    ,ISNULL( SR.SubAssetAmount,0) SubAssetAmount
			                    FROM [TRN].[FixedAssetRegister] FAR
			                    LEFT JOIN MST.BudgetMaster BM ON FAR.FABudgetMasterId = BM.Id
			                    LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
			                    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
			                    LEFT JOIN HKP.Activity A ON A.Id=FAR.FAActivityId
			                    LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAR.FixedAssetMasterId

			                    LEFT JOIN (SELECT FixedAssetRegisterId,SUM(Amount) SubAssetAmount FROM TRN.SubFixedAssetRegister 
			                    GROUP BY FixedAssetRegisterId
			                    ) SR ON SR.FixedAssetRegisterId=FAR.Id AND  FAR.CompanyId='"+companyId+"' AND FAR.PlantId='"+plantId+@"'  AND FAR.IsFinancial=1  
			                     WHERE  far.DisposedVoucherId is null
			                    --GROUP BY FAM.UserName , GL.UserName ,B.UserName ,A.UserName 
			                    ) X
			                    GROUP BY X.FixedAsset,x.FixedAssetMasterId,X.GLName,X.BudgetName,X.ActivityName,X.BudgetMasterId,X.ActivityId";


                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsData, false, "1"); ;


                if (dsData.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("No Data Found");
                }

                ROW++;
                int StartDataRow = ROW;
           
               // int SerialNumber = 0;
                for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                {
                    //SerialNumber++;
                    // worksheet[ROW, colSLNO, ROW , colSLNO].Merge();

                    worksheet[ROW, colSLNO].Number = (i + 1);
                    worksheet[ROW, colFixedAssetMasterId].Text = dsData.Tables[0].Rows[i]["FixedAssetMasterId"].ToString();
                    worksheet[ROW, colFixedAsset].Text = dsData.Tables[0].Rows[i]["FixedAsset"].ToString();
                    worksheet[ROW, colGLName].Text = dsData.Tables[0].Rows[i]["GLName"].ToString();
                    
                    worksheet[ROW, colBudgetName].Text = dsData.Tables[0].Rows[i]["BudgetName"].ToString();
                    worksheet[ROW, colBudgetMasterId].Text = dsData.Tables[0].Rows[i]["BudgetMasterId"].ToString();

                    worksheet[ROW, colActivityName].Text = dsData.Tables[0].Rows[i]["ActivityName"].ToString();
                    worksheet[ROW, colActivityId].Text = dsData.Tables[0].Rows[i]["ActivityId"].ToString();
     
                    worksheet[ROW, colGLAmount].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["GLAmount"].ToString());
                    worksheet[ROW, colRegisterAmount].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["RegisterAmount"].ToString());
                    worksheet[ROW, colSubAssetAmount].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["SubAssetAmount"].ToString());
                    worksheet[ROW, colTotalRegisterAmount].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["TotalRegisterAmount"].ToString());
                    worksheet[ROW, colDiffrence].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["Diffrence"].ToString());
                    
                    ROW++;
                }

                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                //worksheet[StartDataRow, colSalesOrderValue, ROW - 1, colSalesOrderValue].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet[StartDataRow, colContractFundCommission, ROW - 1, colContractFundCommission].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet[StartDataRow, colContractFundUtilization, ROW - 1, colContractFundUtilization].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet[StartDataRow, colContractFundPercentage, ROW - 1, colContractFundPercentage].NumberFormat = "#,##0.00;(#,##0.00)";


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeader(ref worksheet, endCol, "GL VS FA", identity.CompanyId, identity.PlantName, "");
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }
        }
        #endregion GL vs FA

        public IWorkbook FixedAssetDisposedReportWorkSheet(string fixedAssetRegisterDisposeId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "Fixed asset dispose all";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            FixedAssetDisposeService fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataTable data = fixedAssetDisposeService.GetFixedAssetDisposeServiceData(fixedAssetRegisterDisposeId);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Customer:", 15, ExcelHAlign.HAlignLeft);
            int ColCustomer = COL;
            COL++;
            sheet[ROW, COL].Text = data.Rows[0]["CustomerName"].ToString();
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 15, ExcelHAlign.HAlignLeft);
            int ColCurrency = COL;
            COL++;
            sheet[ROW, COL].Text = data.Rows[0]["Currency"].ToString();

            COL = 1;
            ROW = 8;
            report.SetHeaderText(ref sheet, ROW, COL, "Asset No", 10, ExcelHAlign.HAlignLeft);
            int ColFixedAssetRegisterId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Serial No", 10, ExcelHAlign.HAlignLeft);
            int ColSerialNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material", 30, ExcelHAlign.HAlignLeft);
            int ColActivityName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 22, ExcelHAlign.HAlignLeft);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Fixed Asset Master", 28, ExcelHAlign.HAlignLeft);
            int ColFixedAssetMasterName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Purchase Date", 15, ExcelHAlign.HAlignLeft);
            int ColPurchaseDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Capitalization Date", 18, ExcelHAlign.HAlignLeft);
            int ColCapitalizationDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "FA Base Amount", 15, ExcelHAlign.HAlignLeft);
            int ColFABaseAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Base SubAsset", 12, ExcelHAlign.HAlignLeft);
            int ColSubAssetBaseAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Base Amount", 18, ExcelHAlign.HAlignLeft);
            int ColPurchaseBaseAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Depreciation", 15, ExcelHAlign.HAlignLeft);
            int ColADBaseAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Base Amount", 15, ExcelHAlign.HAlignLeft);
            int ColNetBaseBookValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Negotiation Amount", 18, ExcelHAlign.HAlignLeft);
            int ColNegotiationValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Books Nagotiation Amount", 22, ExcelHAlign.HAlignLeft);
            int ColBaseNagotiationValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vendor", 8, ExcelHAlign.HAlignCenter);
            int ColVendor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Invoice No", 12, ExcelHAlign.HAlignLeft);
            int ColInvoiceNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Trn Currency", 12, ExcelHAlign.HAlignLeft);
            int ColTrnCurrency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OB", 8, ExcelHAlign.HAlignLeft);
            int ColIsOpeningBalance = COL;
            COL++;


            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColFixedAssetRegisterId].Text = data.Rows[i]["FixedAssetRegisterId"].ToString();
                sheet[ROW, ColSerialNo].Text = data.Rows[i]["SerialNo"].ToString();
                sheet[ROW, ColActivityName].Text = data.Rows[i]["ActivityName"].ToString();
                sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColFixedAssetMasterName].Text = data.Rows[i]["FixedAssetMasterName"].ToString();
                sheet[ROW, ColPurchaseDate].Text = data.Rows[i]["PurchaseDate"].ToString();
                sheet[ROW, ColCapitalizationDate].Text = data.Rows[i]["CapitalizationDate"].ToString();

                sheet[ROW, ColFABaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["FABaseAmount"].ToString());
                sheet[ROW, ColFABaseAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColSubAssetBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["SubAssetBaseAmount"].ToString());
                sheet[ROW, ColSubAssetBaseAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColPurchaseBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["PurchaseBaseAmount"].ToString());
                sheet[ROW, ColPurchaseBaseAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColADBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["ADBaseAmount"].ToString());
                sheet[ROW, ColADBaseAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColNetBaseBookValue].Number = clsStaticInfo.dbl(data.Rows[i]["NetBaseBookValue"].ToString());
                sheet[ROW, ColNetBaseBookValue].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColNegotiationValue].Number = clsStaticInfo.dbl(data.Rows[i]["NegotiationValue"].ToString());
                sheet[ROW, ColNegotiationValue].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColBaseNagotiationValue].Number = clsStaticInfo.dbl(data.Rows[i]["BaseNagotiationValue"].ToString());
                sheet[ROW, ColBaseNagotiationValue].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColVendor].Text = data.Rows[i]["Vendor"].ToString();
                sheet[ROW, ColInvoiceNo].Text = data.Rows[i]["InvoiceNo"].ToString();
                sheet[ROW, ColTrnCurrency].Text = data.Rows[i]["TrnCurrency"].ToString();
                sheet[ROW, ColIsOpeningBalance].Text = data.Rows[i]["IsOpeningBalance"].ToString();




                //sheet[ROW, ColSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                //sheet[ROW, ColSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet[ROW, ColSPT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet[ROW, ColSPT].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                //sheet[ROW, ColReqMP].Number = Convert.ToDouble(data.Rows[i]["RequiredManPower"].ToString());
                //sheet[ROW, ColReqMP].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet[ROW, ColReqMP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet[ROW, ColReqMP].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                //sheet[ROW, ColAllocatedMP].Number = Convert.ToDouble(data.Rows[i]["AllotedManpower"].ToString());
                //sheet[ROW, ColAllocatedMP].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet[ROW, ColAllocatedMP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet[ROW, ColAllocatedMP].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                //sheet[ROW, ColNoofWS].Number = Convert.ToDouble(data.Rows[i]["AllotedWorkstation"].ToString());
                //sheet[ROW, ColNoofWS].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet[ROW, ColNoofWS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet[ROW, ColNoofWS].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                //sheet[ROW, ColFixedAssetCategory].Text = data.Rows[i]["FixedAssetCategory"].ToString();

                //sheet[ROW, ColFixedAssetCategoryId].Text = data.Rows[i]["FixedAssetCategoryId"].ToString();
                //sheet[ROW, ColFixedAssetSubCategoryId].Text = data.Rows[i]["FixedAssetSubCategoryId"].ToString();
                //sheet[ROW, ColAssetType].Text = data.Rows[i]["AssetType"].ToString();


                //sheet[ROW, ColIsFinancial].Text = data.Rows[i]["IsFinancial"].ToString();

                //sheet[ROW, ColGLGeneralInfoCode].Text = data.Rows[i]["GLGeneralInfoCode"].ToString();
                //sheet[ROW, ColGLGeneralInfoName].Text = data.Rows[i]["GLGeneralInfoName"].ToString();
                //sheet[ROW, ColGLGeneralInfoId].Text = data.Rows[i]["GLGeneralInfoId"].ToString();
                //sheet[ROW, ColBudgetMasterId].Text = data.Rows[i]["BudgetMasterId"].ToString();
                //sheet[ROW, ColBudgetName].Text = data.Rows[i]["BudgetName"].ToString();
                //sheet[ROW, ColBudgetRefNo].Text = data.Rows[i]["BudgetRefNo"].ToString();

                //sheet[ROW, ColActivityId].Text = data.Rows[i]["ActivityId"].ToString();


                //sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Fixed Asset Disposed", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }
    }
}
