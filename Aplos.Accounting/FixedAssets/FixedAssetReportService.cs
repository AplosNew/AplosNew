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
    }
}
