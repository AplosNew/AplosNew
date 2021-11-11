using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Helpers;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.FixedAssets
{
    public class AssetWIPQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public AssetWIPQueryService(ISqlRepository sqlRepository )
        {
            _sqlRepository = sqlRepository;
           
        }

		public List<Dictionary<string, object>> GetFixedAssetWIPstatusSQL()
		{
			var sql = @"select  isnull(MM.UserName,'') MaterialMasterName	
							, MM.Id	MaterialMasterId	
							, isnull( ART.StandardName,'') ArticleName	
							, ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							, MS.UserName MaterialStorageLocation	
							,GL.UserName GL
							,A.UserName Activity
							,IRD.InventoryReceiveId GRNNo,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							,V.VoucherNo,CU.Code Currency
							,IRD.TotalMaterialBooksCurrencyAmount Amount,IRD.BaseQty,IRD.IssueQty 
							,V.Id VoucherId,GL.Id GlId,A.Id ActivityId
from TRN.InventoryReceiveDetail IRD 
LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
 left join [HKP].[MaterialStorage] MS on ms.id=IR.MaterialStorageId
 LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
 LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostDrGLGeneralInfoId
 LEFT JOIN HKP.Activity A ON A.Id=IRD.PostDrActivityId
 LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
 LEFT JOIN SCS.Currency CU ON CU.Id=IR.CurrencyId
 LEFT JOIN (SELECT InventoryReceiveDetailId,SUM(Qty) IssueQty FROM  TRN.InventoryIssueHistory group by InventoryReceiveDetailId) IIH ON IIH.InventoryReceiveDetailId=IRD.Id
WHERE IR.VoucherId<>'' AND IRD.IsAsset=1 and (isnull(IRD.BaseQty,0)-isnull(IIH.IssueQty,0))>0";
			return _sqlRepository.GetDataCollection(sql);

		}

       private DataTable GetFixedAssetWIPstatusReportSQL(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        {
            var sql = @"select  isnull(MM.UserName,'') MaterialMasterName	
							, MM.Id	MaterialMasterId	
							, isnull( ART.StandardName,'') ArticleName	
							, ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							, MS.UserName MaterialStorageLocation	
							,GL.UserName GL
							,A.UserName Activity
							,IRD.InventoryReceiveId GRNNo,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							,V.VoucherNo,CU.Code Currency
							,IRD.TotalMaterialBooksCurrencyAmount Amount,IRD.BaseQty,IRD.IssueQty 
							,V.Id VoucherId,GL.Id GlId,A.Id ActivityId
from TRN.InventoryReceiveDetail IRD 
LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
 left join [HKP].[MaterialStorage] MS on ms.id=IR.MaterialStorageId
 LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
 LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostDrGLGeneralInfoId
 LEFT JOIN HKP.Activity A ON A.Id=IRD.PostDrActivityId
 LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
 LEFT JOIN SCS.Currency CU ON CU.Id=IR.CurrencyId
 LEFT JOIN (SELECT InventoryReceiveDetailId,SUM(Qty) IssueQty FROM  TRN.InventoryIssueHistory group by InventoryReceiveDetailId) IIH ON IIH.InventoryReceiveDetailId=IRD.Id
WHERE IR.VoucherId<>'' AND IRD.IsAsset=1 and (isnull(IRD.BaseQty,0)-isnull(IIH.IssueQty,0))>0
AND ART.Id IN("+materialMasterArticleId+ @")
AND IR.VoucherId IN (" + voucherId + @")
AND MM.Id IN (" + materialMasterId + @")
AND IRD.InventoryReceiveId IN (" + grnNo + @")
AND IRD.PostDrGLGeneralInfoId IN (" + glId + @")
AND IRD.PostDrActivityId IN( " + activityId + @")";

            return _sqlRepository.GetDataTable(sql);

        }


        public IWorkbook AssetWIPstatusList(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        {

            //Start EmployeeAdvanceDueList


            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtGatenntryRegisterList = GetFixedAssetWIPstatusReportSQL( materialMasterId,  materialMasterArticleId,  voucherId,  grnNo,  glId,  activityId);


            if (dtGatenntryRegisterList.Rows.Count == 0)
                throw new Exception("No data found");
            // throw new Exception("To date must be above or equal to From Date.");

            worksheet.Name = "Asset WIP Status Report";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;

            worksheet[ROW, COL].Text = "MaterialMasterName";
            int colMaterialMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticleName = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "SKU1";
            int colFirstCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU2";
            int colSecondCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU3";
            int colThirdCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Storage Location";
            int colMaterialStorageLocation = COL;
            worksheet[ROW, COL].ColumnWidth = 14;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL";
            int colGL = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity";
            int colActivity = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN No";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Base Qty";
            int colBaseQty = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Issue Qty";
            int colIssueQty = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            ROW++;

            for (int i = 0; i < dtGatenntryRegisterList.Rows.Count; i++)
            {
                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
                // worksheet[ROW, colIsOpeningBalance].Number =clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
                worksheet[ROW, colArticleName].Text = (dtGatenntryRegisterList.Rows[i]["ArticleName"].ToString());
                worksheet[ROW, colFirstCharacteristicsValue].Text = dtGatenntryRegisterList.Rows[i]["FirstCharacteristicsValue"].ToString();
                worksheet[ROW, colSecondCharacteristicsValue].Text = dtGatenntryRegisterList.Rows[i]["SecondCharacteristicsValue"].ToString();
                worksheet[ROW, colThirdCharacteristicsValue].Text = dtGatenntryRegisterList.Rows[i]["ThirdCharacteristicsValue"].ToString();
                worksheet[ROW, colMaterialStorageLocation].Text = dtGatenntryRegisterList.Rows[i]["MaterialStorageLocation"].ToString();

                worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
                worksheet[ROW, colGL].Text = dtGatenntryRegisterList.Rows[i]["GL"].ToString();
                worksheet[ROW, colActivity].Text = dtGatenntryRegisterList.Rows[i]["Activity"].ToString();
                
                worksheet[ROW, colGRNNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["GRNNo"].ToString());
                worksheet[ROW, colGRNDate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["GRNDate"].ToString());
                //worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();

                worksheet[ROW, colVoucherNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["VoucherNo"].ToString());
                worksheet[ROW, colCurrency].Text = dtGatenntryRegisterList.Rows[i]["Currency"].ToString();
                worksheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["Amount"].ToString());
                worksheet[ROW, colBaseQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BaseQty"].ToString());
                worksheet[ROW, colIssueQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["IssueQty"].ToString());
                
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, "Asset WIP Status Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze penes
            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;
            #endregion

            return workbook;
        }


    }
}
