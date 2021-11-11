using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Logs;
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

        //public IWorkbook FixedAssetRegisterList(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo,string glId, string activityId)
        //{

        //    //Start EmployeeAdvanceDueList


        //    ExcelEngine excelEngine = new ExcelEngine();
        //    //Instantiate the Excel application object
        //    IApplication application = excelEngine.Excel;

        //    //Set the default application version
        //    application.DefaultVersion = ExcelVersion.Excel2013;

        //    //Load the existing Excel workbook into IWorkbook
        //    IWorkbook workbook = application.Workbooks.Create(1);

        //    //Get the first worksheet in the workbook into IWorksheet
        //    IWorksheet worksheet = workbook.Worksheets[0];
        //    DataTable dtGatenntryRegisterList = GetRegisterReportData(companyGroupId, companyId, plantId, MaterialMasterId, MaterialMasterArticleId, fixedAssetMasterId, vendorId);


        //    if (dtGatenntryRegisterList.Rows.Count == 0)
        //        throw new Exception("No data found");
        //    // throw new Exception("To date must be above or equal to From Date.");

        //    worksheet.Name = "FixedAssetsRegisterReport";

        //    int COL = 1; int ROW = 5;
        //    int startCol = COL;

        //    // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
        //    // worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    //  ROW++;

        //    worksheet[ROW, COL].Text = "SerialNo";
        //    int colSerialNo = COL;
        //    worksheet[ROW, COL].ColumnWidth = 12;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "AssetNo";
        //    int colAssetNo = COL;
        //    worksheet[ROW, COL].ColumnWidth = 10;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;



        //    worksheet[ROW, COL].Text = "Entity";
        //    int colEntity = COL;
        //    worksheet[ROW, COL].ColumnWidth = 12;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Department";
        //    int colDepartment = COL;
        //    worksheet[ROW, COL].ColumnWidth = 12;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Model";
        //    int colModel = COL;
        //    worksheet[ROW, COL].ColumnWidth = 16;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Capitalization Date";
        //    int colCapitalizationDate = COL;
        //    worksheet[ROW, COL].ColumnWidth = 14;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Fixed Asset Master";
        //    int colFixedAssetMasterName = COL;
        //    worksheet[ROW, COL].ColumnWidth = 25;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Material Master";
        //    int colMaterialMasterName = COL;
        //    worksheet[ROW, COL].ColumnWidth = 25;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Article";
        //    int colArticle = COL;
        //    worksheet[ROW, COL].ColumnWidth = 25;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Description";
        //    int colDescription = COL;
        //    worksheet[ROW, COL].ColumnWidth = 40;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Depreciation Rules";
        //    int colDepreciationRules = COL;
        //    worksheet[ROW, COL].ColumnWidth = 25;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Pur. Currency";
        //    int colPurchaseCurrency = COL;
        //    worksheet[ROW, COL].ColumnWidth = 10;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Quantity";
        //    int colQuantity = COL;
        //    worksheet[ROW, COL].ColumnWidth = 8;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Purchase Price";
        //    int colPurchasePrice = COL;
        //    worksheet[ROW, COL].ColumnWidth = 15;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;


        //    worksheet[ROW, COL].Text = "Base Currency";
        //    int colBaseCurrency = COL;
        //    worksheet[ROW, COL].ColumnWidth = 10;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "FA Base Amount";
        //    int colFABaseAmount = COL;
        //    worksheet[ROW, COL].ColumnWidth = 15;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "SubAsset Base Amount";
        //    int colSubAssetAmount = COL;
        //    worksheet[ROW, COL].ColumnWidth = 15;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Total Base Amount";
        //    int colTotalAmount = COL;
        //    worksheet[ROW, COL].ColumnWidth = 15;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "AD Base Amount";
        //    int colADBaseAmount = COL;
        //    worksheet[ROW, COL].ColumnWidth = 15;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

        //    COL++;
        //    worksheet[ROW, COL].Text = "Net FABase Amount";
        //    int colNetFixedAssetsBaseAmount = COL;
        //    worksheet[ROW, COL].ColumnWidth = 15;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Invoice No.";
        //    int colInvoiceNo = COL;
        //    worksheet[ROW, COL].ColumnWidth = 17;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    COL++;

        //    worksheet[ROW, COL].Text = "GRN No.";
        //    int colGRNNo = COL;
        //    worksheet[ROW, COL].ColumnWidth = 10;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "PO No.";
        //    int colPONo = COL;
        //    worksheet[ROW, COL].ColumnWidth = 10;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;



        //    worksheet[ROW, COL].Text = "Vendor";
        //    int colVendorName = COL;
        //    worksheet[ROW, COL].ColumnWidth = 32;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Life Time";
        //    int colLifeTime = COL;
        //    worksheet[ROW, COL].ColumnWidth = 10;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Origin";
        //    int colOriginName = COL;
        //    worksheet[ROW, COL].ColumnWidth = 10;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Year Of Installation";
        //    int colYearOfInstallation = COL;
        //    worksheet[ROW, COL].ColumnWidth = 12;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    COL++;

        //    worksheet[ROW, COL].Text = "Opening Balance";
        //    int colIsOpeningBalance = COL;
        //    worksheet[ROW, COL].ColumnWidth = 12;
        //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
        //    // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //    //COL++;


        //    int endCol = COL;
        //    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
        //    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
        //    worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
        //    ROW++;

        //    for (int i = 0; i < dtGatenntryRegisterList.Rows.Count; i++)
        //    {
        //        // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
        //        worksheet[ROW, colSerialNo].Text = dtGatenntryRegisterList.Rows[i]["SerialNo"].ToString();
        //        worksheet[ROW, colAssetNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["AssetNo"].ToString());
        //        worksheet[ROW, colGRNNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["GRNNo"].ToString());
        //        worksheet[ROW, colPONo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PONo"].ToString());
        //        // worksheet[ROW, colIsOpeningBalance].Number =clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
        //        worksheet[ROW, colIsOpeningBalance].Text = (dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
        //        worksheet[ROW, colModel].Text = dtGatenntryRegisterList.Rows[i]["Model"].ToString();
        //        worksheet[ROW, colCapitalizationDate].Text = dtGatenntryRegisterList.Rows[i]["CapitalizationDate"].ToString();
        //        worksheet[ROW, colInvoiceNo].Text = dtGatenntryRegisterList.Rows[i]["InvoiceNo"].ToString();
        //        worksheet[ROW, colFixedAssetMasterName].Text = dtGatenntryRegisterList.Rows[i]["FixedAssetMasterName"].ToString();

        //        worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
        //        worksheet[ROW, colArticle].Text = dtGatenntryRegisterList.Rows[i]["Article"].ToString();
        //        worksheet[ROW, colEntity].Text = dtGatenntryRegisterList.Rows[i]["Entity"].ToString();
        //        worksheet[ROW, colDepartment].Text = dtGatenntryRegisterList.Rows[i]["Department"].ToString();
        //        worksheet[ROW, colDescription].Text = dtGatenntryRegisterList.Rows[i]["Description"].ToString();
        //        worksheet[ROW, colDepreciationRules].Text = dtGatenntryRegisterList.Rows[i]["DepreciationRules"].ToString();
        //        worksheet[ROW, colPurchaseCurrency].Text = dtGatenntryRegisterList.Rows[i]["PurchaseCurrency"].ToString();
        //        worksheet[ROW, colBaseCurrency].Text = dtGatenntryRegisterList.Rows[i]["BaseCurrency"].ToString();

        //        worksheet[ROW, colQuantity].Text = dtGatenntryRegisterList.Rows[i]["Quantity"].ToString();

        //        worksheet[ROW, colPurchasePrice].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PurchasePrice"].ToString());
        //        worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();
        //        worksheet[ROW, colVendorName].Text = dtGatenntryRegisterList.Rows[i]["VendorName"].ToString();
        //        worksheet[ROW, colLifeTime].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["LifeTime"].ToString());
        //        worksheet[ROW, colOriginName].Text = dtGatenntryRegisterList.Rows[i]["OriginName"].ToString();
        //        worksheet[ROW, colYearOfInstallation].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["YearOfInstallation"].ToString());
        //        worksheet[ROW, colPurchasePrice].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PurchasePrice"].ToString());
        //        worksheet[ROW, colFABaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["FABaseAmount"].ToString());
        //        worksheet[ROW, colSubAssetAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["SubAssetBaseAmount"].ToString());
        //        worksheet[ROW, colTotalAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TotalBaseAmount"].ToString());

        //        worksheet[ROW, colADBaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["ADBaseAmount"].ToString());
        //        worksheet[ROW, colNetFixedAssetsBaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["NetFixedAssetsBaseAmount"].ToString());

        //        worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
        //        worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

        //        ROW++;

        //    }

        //    worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
        //    worksheet.UsedRange.CellStyle.Font.Size = 8f;


        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    ReportUtility reportUtility = new ReportUtility();

        //    reportUtility.PlantHeader(ref worksheet, endCol, "Fixed Assets Register", identity.PlantId);
        //    reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
        //    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //    // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //    worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

        //    worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
        //    worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
        //    worksheet.IsGridLinesVisible = false;

        //    #region Freeze penes
        //    worksheet.IsDisplayZeros = false;
        //    worksheet.UsedRange["A6"].FreezePanes();
        //    worksheet.FirstVisibleColumn = 1;
        //    worksheet.FirstVisibleRow = 6;
        //    #endregion

        //    return workbook;
        //}


    }
}
