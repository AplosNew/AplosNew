using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.IO;
using Syncfusion.DocIO.DLS;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;

using Syncfusion.DocIO;

using System.Drawing;

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

                worksheet[ROW, COL].Text = "Capitalize Amount";
                int colCapitalizeAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Total GL Amount";
                int colTotalGLAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
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
                    ,SUM(X.GLAmount) GLAmount,SUM(X.CapitalizeAmount) CapitalizeAmount
					,SUM(X.GLAmount) +SUM(X.CapitalizeAmount) TotalGLAmount
                    ,ISNULL( SUM(X.RegisterAmount),0) RegisterAmount
                    ,ISNULL( SUM(X.SubAssetAmount),0)SubAssetAmount 
                    ,TotalRegisterAmount=ISNULL( (SUM(X.RegisterAmount)+SUM(X.SubAssetAmount)),0)
                    ,Diffrence=ISNULL( SUM(X.GLAmount)+SUM(X.CapitalizeAmount)-(SUM(X.RegisterAmount)+SUM(X.SubAssetAmount)),0)
                    FROM (

                    SELECT FAM.UserName FixedAsset,FAM.Id FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,VD.BudgetMasterId,VD.ActivityId
                    ,ISNULL( SUM(VDC.DrAmount)-SUM(VDC.CrAmount),0) GLAmount,0 CapitalizeAmount
                    ,0 RegisterAmount
                    ,0 SubAssetAmount
 
                    FROM TRN.VoucherDetail VD 
                    join trn.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
					LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                    LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                    LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GL.Id
                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=VD.BudgetMasterId
                    LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId

                    WHERE GLAT.AccountType='Asset' AND V.SourceType in  ('VendorInvoice','EmployeePayable','JournalVoucher','AdvanceJournalVoucher') 
					--AND V.SourceType NOT IN ('OpeningBalance')
                    GROUP BY FAM.UserName,
                    GL.UserName ,B.UserName ,A.UserName ,VD.BudgetMasterId,VD.ActivityId,FAM.Id

					UNION ALL
					  SELECT FAM.UserName FixedAsset,FAM.Id FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,VD.BudgetMasterId,VD.ActivityId
                    ,0 GLAmount
                    ,ISNULL( SUM(VDC.DrAmount)-SUM(VDC.CrAmount),0) CapitalizeAmount
                    ,0 RegisterAmount
                    ,0 SubAssetAmount
 
                    FROM TRN.VoucherDetail VD 
                    join trn.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
					LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                    LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                    LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GL.Id
                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=VD.BudgetMasterId
                    LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId

                    WHERE GLAT.AccountType='Asset' AND V.SourceType NOT in  ('VendorInvoice','EmployeePayable','JournalVoucher','AdvanceJournalVoucher') 
                    GROUP BY FAM.UserName,
                    GL.UserName ,B.UserName ,A.UserName ,VD.BudgetMasterId,VD.ActivityId,FAM.Id

                    UNION ALL

                    SELECT    FAM.UserName FixedAsset,FAR.FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,FAR.FABudgetMasterId BudgetMasterId,FAR.FAActivityId ActivityId
                    ,0 GLAmount,0 CapitalizeAmount
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
			                    ) SR ON SR.FixedAssetRegisterId=FAR.Id AND  FAR.CompanyId='" + companyId + "' AND FAR.PlantId='" + plantId + @"'  AND FAR.IsFinancial=1  
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
                    worksheet[ROW, colCapitalizeAmount].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["CapitalizeAmount"].ToString());
                    worksheet[ROW, colTotalGLAmount].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["TotalGLAmount"].ToString());

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

            report.SetHeaderText(ref sheet, ROW, COL, "Depreciation", 12, ExcelHAlign.HAlignLeft);
            int ColADBaseAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Base Amount", 15, ExcelHAlign.HAlignLeft);
            int ColNetBaseBookValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Negotiation Amount", 17, ExcelHAlign.HAlignLeft);
            int ColNegotiationValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Books Nagotiation Amount", 15, ExcelHAlign.HAlignLeft);
            int ColBaseNagotiationValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vendor", 18, ExcelHAlign.HAlignCenter);
            int ColVendor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Invoice No", 12, ExcelHAlign.HAlignLeft);
            int ColInvoiceNo = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Trn Currency", 12, ExcelHAlign.HAlignLeft);
            //int ColTrnCurrency = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OB", 5, ExcelHAlign.HAlignLeft);
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
                //sheet[ROW, ColTrnCurrency].Text = data.Rows[i]["TrnCurrency"].ToString();
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



        public void FixedAssetDisposed(string fixedAssetRegisterDisposeId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ReportUtility ru = new ReportUtility();

            //tempId = dtLangName.Rows[0]["UserName"].ToString();
            DataTable dtOrderMaster;


            dtOrderMaster = loadGRNMaterialMaster(fixedAssetRegisterDisposeId);
            if(dtOrderMaster.Rows[0]["Status"].ToString()== "Sales")
            {
                fileName = "FixedAssetDisposed" + identity.PlantId + ".docx";
            }
            else
            {
                fileName = "FixedAssetDisposedScrap" + identity.PlantId + ".docx";
            }
            
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                //DataTable dtOrderMaster;


                //dtOrderMaster = loadGRNMaterialMaster(fixedAssetRegisterDisposeId);


                //var invoicePartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dtOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                //document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

                //var vendorPartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                //document.Replace("{VendorAddress}", vendorPartyAddress, false, false);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                var poApprovedStatus = "";
                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                //var dsServiceItems = loadGRNServiceMaster(fixedAssetRegisterDisposeId);
                double materialTotal = 0;
                if (dtOrderMaster.Rows[0]["Status"].ToString() == "Sales")
                {
                    materialTotal = makeOrderDetailsTable(document, dtOrderMaster, fixedAssetRegisterDisposeId);
                }
                else
                {
                    materialTotal = makeScrapDisposedDetailsTable(document, dtOrderMaster, fixedAssetRegisterDisposeId);
                }
               

                document.Replace("{GrandTotal}", ((materialTotal)).ToString("#,##0.00"), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((materialTotal)), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "FixedAssetDisposed" + fixedAssetRegisterDisposeId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);

                document.Close();
            }


            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable loadGRNMaterialMaster(string fixedAssetRegisterDisposeId)
        {
            string strSQL;
            try
            {

                strSQL = @"SELECT FARD.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId,FR.FixedAssetMasterId
                                    , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType
                                    ,c.Code TrnCurrency
									,c.id TrnCurrencyId
	                                ,FORMAT(FAD.DocDate,'dd-MMM-yyyy') DocDate
                                    , ISNULL(FR.Price,0) Price
									,ISNULL(SAR.subAssetAmount,0) SubAssetAmount
									, ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0) PurchasePrice
									 ,ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0) NetBookValue 
								--	, 0 NegotiationValue

								   , BC.Code BaseCurrency
								   ,BC.id BaseCurrencyId
									,isnull(FR.FABaseAmount,0)FABaseAmount
									,ISNULL(SAR.subAssetBaseAmount,0) SubAssetBaseAmount
									,isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0) PurchaseBaseAmount
									,isnull( FR.ADBaseAmount,0)ADBaseAmount
                                    , isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0) NetBaseBookValue 
									,isnull( FARD.NegotiationValue,0) TrnValue,isnull(FARD.BaseNagotiationValue,0) BaseValue
								,UoM.UserName BaseUoM

                                    , MMA.StandardName Article, FR.IsFinancial,IsOpeningBalance=case when FR.IsOpeningBalance=0 then 'No' Else 'Yes' End
                                    , GL.AccountCode GLGeneralInfoCode,GL.UserName GLGeneralInfoName,GL.Id GLGeneralInfoId
									, BM.Id BudgetMasterId,B.UserName BudgetName,BM.RefNo BudgetRefNo
									, A.UserName ActivityName, FR.FAActivityId ActivityId
                                   		,format( FR.CapitalizationDate,'dd-MMM-yyyy')CapitalizationDate
									,format(IR.GRNDate,'dd-MMM-yyyy') PurchaseDate
									,format( ii.IssueDate,'dd-MMM-yyyy')IssueDate
		                            ,FAD.Remarks,
									Customer.UserName CustomerName

									,CU.Code Currency,CU.Id CurrencyId,Plant.UserName Plant,FR.Status
									,FAD.InvoicingByAddress	
									
									,FAD.DeliveryByAddress,VPL.UserName DeliveryParty
									,FAD.Id FixedAssetNo, FAD.PartyPlantId,fad.DeliveryPartyPlantId
									,FAD.AddedBy,CAST(FAD.ToCurrencyRate AS decimal(18,4))ToCurrencyRate
								
									

                                    FROM [TRN].[FixedAssetRegister] FR

                                   LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId= MM.Id
								   LEFT JOIN SCS.UnitOfMeasurement UoM on MM.BaseUOMId=UoM.Id
                                   LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                   LEFT JOIN MST.BudgetMaster BM ON FR.FABudgetMasterId = BM.Id
                                   LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
                                   LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId= FAC.Id
                                   LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId= FASC.Id

	                                LEFT JOIN TRN.FixedAssetRegisterDetail FRD ON FRD.CapitalizeRegisterNo=FR.CapitalizeRegisterNo
									LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FRD.InventoryIssueHistoryId
									LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									left join trn.InventoryIssue II on ii.Id = iid.InventoryIssueId
									LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									left join trn.InventoryReceive IR on IR.Id =  IRD.InventoryReceiveId
									LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId 
                                    LEFT JOIN SCS.Currency C ON C.Id =FR.CurrencyId
                                    LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId

								   LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
								   LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
								   LEFT JOIN HKP.Activity A ON A.Id=FR.FAActivityId
								   LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM
								   TRN.SubFixedAssetRegister 
								   group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
                                    LEFT JOIN TRN.FixedAssetRegisterDisposedDetail FARD ON FARD.FixedAssetRegisterId=FR.Id

                                    LEFT JOIN TRN.FixedAssetRegisterDisposed FAD ON FAD.Id=FARD.FixedAssetRegisterDisposedId
	                                LEFT JOIN HKP.Party Customer ON Customer.Id = FAD.PartyId
                                    LEFT JOIN SCS.Currency CU ON CU.Id =FAD.CurrencyId
									 LEFT JOIN HKP.PartyPlant VPL ON VPL.Id = FAD.DeliveryPartyPlantId
                                    LEFT JOIN ORG.Plant Plant ON Plant.Id =FR.PlantId
                                   WHERE FARD.FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposeId + @"'";
                return _sqlRepository.GetDataTable(strSQL); 
            }  

            catch (System.Exception ex) 
            {
                throw (ex);
            }
            finally 
            {

            }
        }

        public double makeOrderDetailsTable(WordDocument document, DataTable dsOrderMaster, string fixedAssetRegisterDisposeId)
        {
            string replaceString = "{materialItems}";

            ReportUtility ru = new ReportUtility();

            int LasColumnIndex = 5;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            wTable.TableFormat.IsAutoResized = true;

            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Sl.");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Asset No.");
            range.ApplyCharacterFormat(FontBold);
            int colAssetNo = COL; COL++;
            wTable.Rows[ROW].Cells[colAssetNo].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 130;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 180;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Asset Master");
            range.ApplyCharacterFormat(FontBold);
            int colFixedAssetMaster = COL; COL++;
            wTable.Rows[ROW].Cells[colFixedAssetMaster].Width = 110;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTrnAmount = COL; COL++;
            wTable.Rows[ROW].Cells[colTrnAmount].Width = 90;
           

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Base Amount(" + dsOrderMaster.Rows[0]["BaseCurrency"].ToString() + ")");
            //range.ApplyCharacterFormat(FontBold);
            //int colBaseAmount = COL; COL++;
            //wTable.Rows[ROW].Cells[colBaseAmount].Width = 90;


            #endregion column headers

            double totalValue = 0;
            int sl = 0;
            //ROW++;
            //wTable.AddRow();
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                sl++;
                ROW++;
                wTable.AddRow();
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());
                TROW.Cells[colAssetNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["AssetNo"].ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colFixedAssetMaster].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FixedAssetMasterName"].ToString());

                TROW.Cells[colTrnAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnValue"].ToString()).ToString("#,##0.00"));
                //TROW.Cells[colBaseAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["BaseValue"].ToString()).ToString("#,##0.00"));
                totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnValue"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colAssetNo || C == colMaterialGroup || C == colArticle || C == colFixedAssetMaster || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }
            #endregion Total
            // ROW++;
            #region Sub Total
            int SubTotalRow = ROW;
            int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                                   // wTable.AddRow();
                                   // _TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TrnValue)", "").ToString());
            ////- clsStaticInfo.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())


            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["TrnCurrency"].ToString()) + ")");

            #endregion Total
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                // TROW.Cells[0].Width = 120;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                foreach (WParagraph item in TROW.Cells[colTrnAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                //foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                //{
                //    item.ApplyStyle("MyStyleRightAlign");
                //}

            }

            #endregion paragrpath formats
            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;

            //primary cells merging (veritcal)
            ROW++;
            WTableRow TROWe = wTable.LastRow;

            //wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }
        public double makeScrapDisposedDetailsTable(WordDocument document, DataTable dsOrderMaster, string fixedAssetRegisterDisposeId)
        {
            string replaceString = "{materialItems}";

            ReportUtility ru = new ReportUtility();

            int LasColumnIndex = 4;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            wTable.TableFormat.IsAutoResized = true;

            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Sl.");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Asset No.");
            range.ApplyCharacterFormat(FontBold);
            int colAssetNo = COL; COL++;
            wTable.Rows[ROW].Cells[colAssetNo].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 130;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 180;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Asset Master");
            range.ApplyCharacterFormat(FontBold);
            int colFixedAssetMaster = COL; COL++;
            wTable.Rows[ROW].Cells[colFixedAssetMaster].Width = 200;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Transaction Amount(" + dsOrderMaster.Rows[0]["Currency"].ToString() + ")");
            //range.ApplyCharacterFormat(FontBold);
            //int colTrnAmount = COL; COL++;
            //wTable.Rows[ROW].Cells[colTrnAmount].Width = 90;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Base Amount(" + dsOrderMaster.Rows[0]["BaseCurrency"].ToString() + ")");
            //range.ApplyCharacterFormat(FontBold);
            //int colBaseAmount = COL; COL++;
            //wTable.Rows[ROW].Cells[colBaseAmount].Width = 90;


            #endregion column headers

            double totalValue = 0;
            int sl = 0;
            //ROW++;
            //wTable.AddRow();
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                sl++;
                ROW++;
                wTable.AddRow();
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());
                TROW.Cells[colAssetNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["AssetNo"].ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colFixedAssetMaster].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FixedAssetMasterName"].ToString());

                //TROW.Cells[colTrnAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnValue"].ToString()).ToString("#,##0.00"));
                //TROW.Cells[colBaseAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["BaseValue"].ToString()).ToString("#,##0.00"));
                //totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnValue"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

            }

            ROW++;
            #region Total
            //int TotalRow = ROW;
            //wTable.AddRow();
            //WTableRow _TROW = wTable.LastRow;
            //_TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            //for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            //{
            //    if (C == colAssetNo || C == colMaterialGroup || C == colArticle || C == colFixedAssetMaster || dicTaxes.ContainsValue(C))
            //        continue;

            //    double value = 0;
            //    for (int i = startRow; i < TotalRow; i++)
            //    {

            //        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
            //        {
            //            value += clsStaticInfo.dbl(item.Text);
            //        }
            //    }
            //    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            //}
            #endregion Total
            // ROW++;
            #region Sub Total
            int SubTotalRow = ROW;
            int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                                   // wTable.AddRow();
                                   // _TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TrnValue)", "").ToString());
            ////- clsStaticInfo.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())


            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["TrnCurrency"].ToString()) + ")");

            #endregion Total
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                // TROW.Cells[0].Width = 120;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 1; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                //foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                //{
                //    item.ApplyStyle("MyStyleRightAlign");
                //}


                //foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                //{
                //    item.ApplyStyle("MyStyleRightAlign");
                //}

            }

            #endregion paragrpath formats
            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;

            //primary cells merging (veritcal)
            ROW++;
            WTableRow TROWe = wTable.LastRow;

            //wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public double makeInventoryReceiveAdditionalTaxTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{InventoryReceiveAdditionalTax}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadInventoryReceiveAdditionalTax(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                //for (int i = 0; i < dv.Count; i++)
                //{
                //	//two columns required for tax
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //	range.ApplyCharacterFormat(FontBold);
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //}
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            //if (dv.Count > 0)
            //{
            //	for (int i = 0; i < dv.Count; i++)
            //	{

            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //		range.ApplyCharacterFormat(FontBold);
            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //		range.ApplyCharacterFormat(FontBold);

            //	}
            //}

            #endregion column headers
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;


                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }


        public double makeOrderServiceTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadGRNServiceMasterTex(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            //wTable.Rows[ROW].Cells[COL].Width = 100;
            int colServiceName = COL; //COL++;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Service"].ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsOrderMaster.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
            }

            ROW++;

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            //wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;

            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2"));
            }


            #endregion Total


            ROW++;


            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString())
//- clsStaticInfo.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
+ clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());



            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;

            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;


            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public DataTable loadGRNServiceMasterTex(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,IR.Id PurchaseOrderId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount
                    from TRN.InventoryReceive IR
                              INNER JOIN trn.InventoryService ISER ON ISER.InventoryReceiveId = IR.Id
                              Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id and IRT.InventoryServiceId = ISER.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                WHERE IR.Id='" + OrderMasterID + @"'
								and InventoryServiceId  is not null and   InventoryReceiveDetailId is null 
								 ORDER BY tg.[Sequence]";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadOrderMasterItems(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        public DataTable loadGRNServiceMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TotalTaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.InventoryReceive   IR
                            INNER join trn.inventoryservice IOS ON IOS.InventoryReceiveId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        public DataTable loadInventoryReceiveAdditionalTax(string grnId)
        {
            string strSQL;

            try
            {
                strSQL = @"Select TxC.UserName Taxname  ,IRAT.ID ,IRAT.TaxCodeId TaxCode,IRAT.TaxAmount,IRAT.Percentage   from [TRN].[InventoryReceiveAdditionalTax] IRAT
						LEFT JOIN TRN.InventoryReceive IR ON IR.ID= IRAT.InventoryReceiveId
						LEFT JOIN [MST].[TaxCode] TxC ON TxC.Id= IRAT.TaxCodeId
                        where IR.Id = '" + grnId + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        public void loadGRNShortageTable(WordDocument document, string grnId)
        {
            string replaceString = "{shortage}";

            DataTable dtlOrderItems;

            dtlOrderItems = loadGRNShortageMaster(grnId);
            if (dtlOrderItems.Rows.Count > 0)
            {
                document.Replace("{ShortageDetails}", "Shortage Details", true, true);

                //dsTax = loadOrderMasterTax(grnId);

                int LasColumnIndex = 6;
                Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

                WTable wTable = new WTable(document);
                wTable.TableFormat.Borders.LineWidth = 1;
                wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
                int ROW = 0; int COL = 0;
                wTable.ResetCells(1, LasColumnIndex + 1);

                WTableRow TemplateRow = wTable.Rows[0].Clone();

                #region column headers
                document.EnsureMinimal();
                //wTable.Title = "Material Details";
                //wTable.Description = "This table shows the price details of PI";
                //wTable.IndentFromLeft = 10;


                //string UOM = dsOrderMaster.Tables[0].Rows[0]["UOM"].ToString();
                //string Currency = dsOrderMaster.Tables[0].Rows[0]["Currency"].ToString();
                WCharacterFormat FontBold = new WCharacterFormat(document);
                FontBold.Bold = true;
                // = true;




                IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
                range.ApplyCharacterFormat(FontBold);
                int colRowIdShort = COL; COL++;
                wTable.Rows[ROW].Cells[COL].Width = 50;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
                wTable.Rows[ROW].Cells[COL].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                int colMaterial = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colArticle = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("InvoiceRate");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterialTranRate = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colShortageQty = COL; COL++;



                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate(%)");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colShortageRatePercent = COL;
                COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value (" + dtlOrderItems.Rows[0]["Code"].ToString() + ")");
                wTable.Rows[ROW].Cells[COL].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                int colShortageValue = COL;



                //int colTotalTaxableAmount = COL;
                //if (dv.Count > 0)
                //{
                //    COL++;
                //    colTotalTaxableAmount = COL;
                //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                //    range.ApplyCharacterFormat(FontBold);
                //    //COL++;
                //    //for (int i = 0; i < dv.Count; i++)
                //    //{
                //    //    //two columns required for tax
                //    //    COL++;
                //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //    //    range.ApplyCharacterFormat(FontBold);

                //    //    COL++;
                //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //    //}
                //}
                //else
                //{
                //    COL++;
                //    colTotalTaxableAmount = COL;
                //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Value");
                //}


                //wTable.Rows.Add(TemplateRow);
                //ROW++;

                //if (dv.Count > 0)
                //{
                //    for (int i = 0; i < dv.Count; i++)
                //    {

                //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                //        range.ApplyCharacterFormat(FontBold);
                //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                //        range.ApplyCharacterFormat(FontBold);

                //    }
                //}
                #endregion column headers
                double totalValue = 0;
                int startRow = ROW + 1;
                for (int i = 0; i < dtlOrderItems.Rows.Count; i++)
                {
                    //if (Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageQty"]) > 0)
                    //{



                    ROW++;

                    wTable.AddRow();
                    WTableRow TROW = wTable.LastRow;

                    // WTableRow TROW = wTable.Rows[1].Clone();
                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.Text = "";
                        }
                    }
                    TROW.Cells[colRowIdShort].AddParagraph().AppendText(dtlOrderItems.Rows[i]["InventoryReceiveDetailsId"].ToString());
                    TROW.Cells[colMaterial].AddParagraph().AppendText(dtlOrderItems.Rows[i]["MaterialMaster"].ToString());
                    TROW.Cells[colArticle].AddParagraph().AppendText(dtlOrderItems.Rows[i]["Article"].ToString());

                    TROW.Cells[colMaterialTranRate].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["MaterialTranRate"]).ToString("F2"));
                    //TROW.Cells[colMaterialTranRate].Width = 60;
                    TROW.Cells[colShortageQty].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageQty"]).ToString("F2"));
                    //TROW.Cells[colShortageQty].Width = 60;
                    TROW.Cells[colShortageRatePercent].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageRatePercent"]).ToString("F2"));
                    //TROW.Cells[colShortageRatePercent].Width = 60;
                    TROW.Cells[colShortageValue].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageValue"]).ToString("F2"));
                    //TROW.Cells[colShortageValue].Width = 60;

                    //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dtOrderItems.Rows[i]["TrnAmount"].ToString()).ToString("F2"));

                    //totalValue += clsStaticInfo.dbl(dtOrderItems.Rows[i]["TrnAmount"].ToString());

                    //if (dv.Count > 0)
                    //{
                    //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //    //double totalTax = 0;

                    //    for (int T = 0; T < dv.Count; T++)
                    //    {
                    //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                    //        if (dvtax.Count > 0)
                    //        {
                    //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                    //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                    //        }
                    //    }
                    //}
                    //}
                }

                ROW++;
                #region Total
                int TotalRow = ROW;
                wTable.AddRow();
                WTableRow _TROW = wTable.LastRow;
                _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


                for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
                {
                    if (C == colMaterialTranRate || C == colShortageRatePercent || C == colRowIdShort || dicTaxes.ContainsValue(C))
                        continue;

                    double value = 0;
                    for (int i = startRow; i < TotalRow; i++)
                    {

                        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                        {
                            value += OTSBD.clsStaticInfo.dbl(item.Text);
                        }
                    }
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
                }
                #endregion Total


                ROW++;
                //#region Sub Total
                //int SubTotalRow = ROW;
                //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                //wTable.AddRow();
                //_TROW = wTable.LastRow;

                //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

                //double total = clsStaticInfo.dbl(dtlOrderItems.Compute("SUM(ShortageQty)", "").ToString())

                //+ clsStaticInfo.dbl(dtlOrderItems.Compute("SUM(ShortageValue)", "").ToString());

                //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

                //#endregion Total


                ROW++;
                #region Total Payable
                //int TotalPayableRow = ROW;
                //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
                //wTable.AddRow();
                //_TROW = wTable.LastRow;

                //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
                //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

                #endregion Total Payable

                ROW++;


                #region paragrpath formats
                IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyleS");
                //Sets the formatting of the style
                myStyle.CharacterFormat.FontSize = 8f;
                myStyle.CharacterFormat.TextColor = Color.Black;
                myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                for (int R = 0; R < wTable.Rows.Count; R++)
                {
                    WTableRow TROW = wTable.Rows[R];
                    TROW.Cells[0].Width = 50;

                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.ApplyStyle("MyStyleS");
                        }
                    }

                }

                #endregion paragrpath formats

                //#region paragrpath formats
                //Adds a new paragraph style named "MyStyle"
                //IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyles");
                ////Sets the formatting of the style
                //myStyle.CharacterFormat.FontSize = 8f;
                //myStyle.CharacterFormat.TextColor = Color.Black;
                //myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                //for (int R = 0; R < wTable.Rows.Count; R++)
                //{
                //    WTableRow TROW = wTable.Rows[R];
                //    TROW.Cells[0].Width = 35;
                //    //if (dv.Count < 3)
                //    //    TROW.Cells[0].Width = 70 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                //    {
                //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                //        {
                //            item.ApplyStyle("MyStyles");
                //        }
                //    }
                //}

                //#endregion paragrpath formats

                #region
                //tax codes merging (horizontal)
                ROW = 0;
                //for (int i = 0; i < dv.Count; i++)
                //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

                //primary cells merging (veritcal)
                //ROW++;
                //for (int i = 0; i <= colTotalTaxableAmount; i++)
                //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


                //WParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
                //style.CharacterFormat.Bold = true;
                //style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;I
                //Adds new paragraph to the section


                //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
                //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
                //        PARA.ApplyStyle("SubTotalStyle");

                //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
                #endregion merging section

                TextBodyPart textBodyPart = new TextBodyPart(document);
                textBodyPart.BodyItems.Add(wTable);
                document.Replace(replaceString, textBodyPart, true, true);

                //return total;
            }
        }


        public void loadGRNRejectionTable(WordDocument document, string grnId)
        {
            string replaceString = "{rejection}";



            DataTable dtOrderItems, dsTax;
            dtOrderItems = loadGRNRejectionMaster(grnId);
            if (dtOrderItems.Rows.Count > 0)
            {
                document.Replace("{RejectionDetails}", "Rejection Details", true, true);


                //  dsTax = loadOrderMasterTax(grnId);

                int LasColumnIndex = 6;
                Dictionary<string, int> dicTaxes = new Dictionary<string, int>();


                WTable wTable = new WTable(document);
                wTable.TableFormat.Borders.LineWidth = 1;
                wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
                int ROW = 0; int COL = 0;
                wTable.ResetCells(1, LasColumnIndex + 1);

                WTableRow TemplateRow = wTable.Rows[0].Clone();

                #region column headers
                document.EnsureMinimal();

                WCharacterFormat FontBold = new WCharacterFormat(document);
                FontBold.Bold = true;
                IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
                range.ApplyCharacterFormat(FontBold);
                int colRowIdRej = COL; COL++;
                wTable.Rows[ROW].Cells[COL].Width = 50;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterial = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colArticle = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("InvoiceRate");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterialTranRate = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty ");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colRejectionQty = COL; COL++;



                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate(%) ");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colRejectRatePercent = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value (" + dtOrderItems.Rows[0]["Code"].ToString() + ")");
                wTable.Rows[ROW].Cells[COL].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                int colRejectValue = COL;

                #endregion column headers

                double totalValue = 0;
                int startRow = ROW + 1;
                for (int i = 0; i < dtOrderItems.Rows.Count; i++)
                {
                    ROW++;
                    wTable.AddRow();
                    WTableRow TROW = wTable.LastRow;
                    // WTableRow TROW = wTable.Rows[1].Clone();
                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.Text = "";
                        }
                    }

                    TROW.Cells[colRowIdRej].AddParagraph().AppendText(dtOrderItems.Rows[i]["InventoryReceiveDetailsId"].ToString());
                    TROW.Cells[colMaterial].AddParagraph().AppendText(dtOrderItems.Rows[i]["MaterialMaster"].ToString());
                    TROW.Cells[colArticle].AddParagraph().AppendText(dtOrderItems.Rows[i]["Article"].ToString());
                    TROW.Cells[colMaterialTranRate].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["MaterialTranRate"]).ToString());
                    //TROW.Cells[colMaterialTranRate].Width = 60;
                    TROW.Cells[colRejectionQty].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectionQty"]).ToString());
                    //TROW.Cells[colRejectionQty].Width = 60;
                    TROW.Cells[colRejectRatePercent].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectRatePercent"]).ToString());
                    //TROW.Cells[colRejectRatePercent].Width = 60;
                    TROW.Cells[colRejectValue].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectValue"]).ToString());
                    //TROW.Cells[colRejectValue].Width = 60;


                }

                ROW++;
                #region Total
                int TotalRow = ROW;
                wTable.AddRow();
                WTableRow _TROW = wTable.LastRow;
                _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


                for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
                {
                    if (C == colMaterialTranRate || C == colRejectRatePercent || dicTaxes.ContainsValue(C))
                        continue;

                    double value = 0;
                    for (int i = startRow; i < TotalRow; i++)
                    {

                        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                        {
                            value += clsStaticInfo.dbl(item.Text);
                        }
                    }
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
                }
                #endregion Total


                ROW++;
                //#region Sub Total
                //int SubTotalRow = ROW;
                //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                //wTable.AddRow();
                //_TROW = wTable.LastRow;

                //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

                //double total = clsStaticInfo.dbl(dtOrderItems.Compute("SUM(RejectValue)", "").ToString());
                ////- clsStaticInfo.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                ////+ clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

                //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

                //#endregion Total




                ROW++;

                #region paragrpath formats
                IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyleR");
                //Sets the formatting of the style
                myStyle.CharacterFormat.FontSize = 8f;
                myStyle.CharacterFormat.TextColor = Color.Black;
                myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                for (int R = 0; R < wTable.Rows.Count; R++)
                {
                    WTableRow TROW = wTable.Rows[R];
                    TROW.Cells[0].Width = 50;

                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.ApplyStyle("MyStyleR");
                        }
                    }

                }

                #endregion paragrpath formats


                #region merging section


                //tax codes merging (horizontal)
                ROW = 0;

                ROW++;

                #endregion merging section

                TextBodyPart textBodyPart = new TextBodyPart(document);
                textBodyPart.BodyItems.Add(wTable);
                document.Replace(replaceString, textBodyPart, true, true);
            }

        }

        public DataTable loadGRNRejectionMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select IRD.Id AS InventoryReceiveDetailsId,IRD.MaterialTranRate
	                                ,IRD.RejectionQty
	                                ,IRD.RejectRatePercent
	                                ,IRD.RejectValue
									,C.Code
                                    ,MM.UserName MaterialMaster
									,MMA.StandardName Article
                                FROM trn.InventoryReceiveDetail IRD
                                LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
								 LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
								 LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
								LEFT JOIn trn.InventoryReceive IR ON IR.Id=InventoryReceiveId
								LEFT JOIN scs.Currency C on C.Id=IR.CurrencyId
                        where IRD.InventoryReceiveId='" + OrderMasterID + "'and isnull(IRD.ShortageQty,0)>0";


                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadGRNShortageMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select IRD.Id AS InventoryReceiveDetailsId,IRD.MaterialTranRate
	                                ,IRD.ShortageQty
	                                ,IRD.ShortageRatePercent
	                                ,IRD.ShortageValue 
									,C.Code
									,MM.UserName MaterialMaster
									,MMA.StandardName Article
                                FROM trn.InventoryReceiveDetail IRD
								 LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
								 LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
								 LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
								LEFT JOIn trn.InventoryReceive IR ON IR.Id=InventoryReceiveId
								LEFT JOIN scs.Currency C on C.Id=IR.CurrencyId
                                where IRD.InventoryReceiveId='" + OrderMasterID + "'and isnull(IRD.ShortageQty,0)>0";


                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public IEnumerable<object> getGLVSfaListSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT X.FixedAsset,x.FixedAssetMasterId,X.GLName,X.BudgetName,X.ActivityName,X.BudgetMasterId,X.ActivityId
                    ,SUM(X.GLAmount) GLAmount,SUM(X.CapitalizeAmount) CapitalizeAmount
					,SUM(X.GLAmount) +SUM(X.CapitalizeAmount) TotalGLAmount
                    ,ISNULL( SUM(X.RegisterAmount),0) RegisterAmount
                    ,ISNULL( SUM(X.SubAssetAmount),0)SubAssetAmount 
                    ,TotalRegisterAmount=ISNULL( (SUM(X.RegisterAmount)+SUM(X.SubAssetAmount)),0)
                    ,Diffrence=ISNULL( SUM(X.GLAmount)+SUM(X.CapitalizeAmount)-(SUM(X.RegisterAmount)+SUM(X.SubAssetAmount)),0)
                    FROM (

                    SELECT FAM.UserName FixedAsset,FAM.Id FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,VD.BudgetMasterId,VD.ActivityId
                    ,ISNULL( SUM(VDC.DrAmount)-SUM(VDC.CrAmount),0) GLAmount,0 CapitalizeAmount
                    ,0 RegisterAmount
                    ,0 SubAssetAmount
 
                    FROM TRN.VoucherDetail VD 
                    join trn.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
					LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                    LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                    LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GL.Id
                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=VD.BudgetMasterId
                    LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId

                    WHERE GLAT.AccountType='Asset' AND V.SourceType in  ('VendorInvoice','EmployeePayable','JournalVoucher','AdvanceJournalVoucher') 
					--AND V.SourceType NOT IN ('OpeningBalance')
                    GROUP BY FAM.UserName,
                    GL.UserName ,B.UserName ,A.UserName ,VD.BudgetMasterId,VD.ActivityId,FAM.Id

					UNION ALL
					  SELECT FAM.UserName FixedAsset,FAM.Id FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,VD.BudgetMasterId,VD.ActivityId
                    ,0 GLAmount
                    ,ISNULL( SUM(VDC.DrAmount)-SUM(VDC.CrAmount),0) CapitalizeAmount
                    ,0 RegisterAmount
                    ,0 SubAssetAmount
 
                    FROM TRN.VoucherDetail VD 
                    join trn.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
					LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                    LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                    LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GL.Id
                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=VD.BudgetMasterId
                    LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId

                    WHERE GLAT.AccountType='Asset' AND V.SourceType NOT in  ('VendorInvoice','EmployeePayable','JournalVoucher','AdvanceJournalVoucher') 
                    GROUP BY FAM.UserName,
                    GL.UserName ,B.UserName ,A.UserName ,VD.BudgetMasterId,VD.ActivityId,FAM.Id

                    UNION ALL

                    SELECT    FAM.UserName FixedAsset,FAR.FixedAssetMasterId,
                    GL.UserName GLName,B.UserName BudgetName,A.UserName ActivityName ,FAR.FABudgetMasterId BudgetMasterId,FAR.FAActivityId ActivityId
                    ,0 GLAmount,0 CapitalizeAmount
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
			                    ) SR ON SR.FixedAssetRegisterId=FAR.Id AND  FAR.CompanyId='" + identity.CompanyId + @"' AND FAR.PlantId='" + identity.PlantId +@"'  AND FAR.IsFinancial=1  
			                     WHERE  far.DisposedVoucherId is null
			                    --GROUP BY FAM.UserName , GL.UserName ,B.UserName ,A.UserName 
			                    ) X
			                    GROUP BY X.FixedAsset,x.FixedAssetMasterId,X.GLName,X.BudgetName,X.ActivityName,X.BudgetMasterId,X.ActivityId";

            return _sqlRepository.GetDataCollection(sql);
        }
    }
}
