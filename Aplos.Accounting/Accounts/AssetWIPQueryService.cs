using ConnectionManager.DAL;
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
using System.IO;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.FixedAssets
{
    public class AssetWIPQueryService
    {
        ISqlRepository _sqlRepository;
        public AssetWIPQueryService()
        {
            _sqlRepository = new SqlRepository();
        }
        public List<Dictionary<string, object>> GetFixedAssetWIPstatusSQL()
		{
			var sql = @"select  IRD.Id InventoryReceiveDetailId, isnull(MM.UserName,'') MaterialMasterName	
							, MM.Id	MaterialMasterId	
							, isnull( ART.StandardName,'') ArticleName	
							, ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							, MS.UserName MaterialStorageLocation,FAM.UserName AssetMaster	
							,GL.UserName GL,B.UserName Budget
							,A.UserName Activity
							,IRD.InventoryReceiveId GRNNo,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							,V.VoucherNo
							,IRD.TransactionQty,TUOM.UserName TrnUOM
							,IRD.MaterialTranRate TrnRate
							,CU.Code Currency
							,IRD.TotalMaterialTranAmount TrnAmount
							,IRD.BaseQty,BUOM.UserName BaseUOM,IRD.BooksCurrencyBaseRate BaseRate
							,IRD.TotalMaterialBooksCurrencyAmount BooksAmount
							,isnull(IIH.IssueQty,0) as IssueQty
							,V.Id VoucherId,GL.Id GlId,A.Id ActivityId,(isnull(IRD.BaseQty,0)-isnull(IIH.IssueQty,0)) BalanceQty
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
 LEFT JOIN SCS.UnitOfMeasurement TUOM ON TUOM.Id=IRD.TransactionUoMId
 LEFT JOIN SCS.UnitOfMeasurement BUOM ON BUOM.Id=IRD.BaseUOMId
 LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostDrGLGeneralInfoId
 LEFT JOIN MST.BudgetMaster BM ON BM.Id=IRD.PostDrBudgetMasterId
 LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
 LEFT JOIN HKP.Activity A ON A.Id=IRD.PostDrActivityId
 LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=MM.BudgetMasterId
 LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId
 LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IRD.InventoryReceiveId
 LEFT JOIN TRN.Voucher V ON V.Id=CASE WHEN IR.EmployeeId<>'' THEN EP.VoucherId ELSE IR.VoucherId  END
 LEFT JOIN SCS.Currency CU ON CU.Id=IR.CurrencyId
 LEFT JOIN (SELECT InventoryReceiveDetailId,SUM(Qty) IssueQty FROM  TRN.InventoryIssueHistory group by InventoryReceiveDetailId) IIH ON IIH.InventoryReceiveDetailId=IRD.Id
WHERE 
IR.[Status]='Posting' and IRD.IsAsset=1  
and (isnull(IRD.BaseQty,0)-isnull(IIH.IssueQty,0))>0";
			return _sqlRepository.GetDataCollection(sql);

		}

       private DataTable GetFixedAssetWIPstatusReportSQL(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        {
            var sql = @"select  IRD.Id InventoryReceiveDetailId, isnull(MM.UserName,'') MaterialMasterName	
							, MM.Id	MaterialMasterId	
							, isnull( ART.StandardName,'') ArticleName	
							, ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							, MS.UserName MaterialStorageLocation,FAM.UserName AssetMaster	
							,GL.UserName GL,B.UserName Budget
							,A.UserName Activity
							,IRD.InventoryReceiveId GRNNo,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							,V.VoucherNo
							,IRD.TransactionQty,TUOM.UserName TrnUOM
							,IRD.MaterialTranRate TrnRate
							,CU.Code Currency
							,IRD.TotalMaterialTranAmount TrnAmount
							,IRD.BaseQty,BUOM.UserName BaseUOM,IRD.BooksCurrencyBaseRate BaseRate
							,IRD.TotalMaterialBooksCurrencyAmount BooksAmount
							,isnull(IIH.IssueQty,0) as IssueQty
							,V.Id VoucherId,GL.Id GlId,A.Id ActivityId,(isnull(IRD.BaseQty,0)-isnull(IIH.IssueQty,0)) BalanceQty
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
 LEFT JOIN SCS.UnitOfMeasurement TUOM ON TUOM.Id=IRD.TransactionUoMId
 LEFT JOIN SCS.UnitOfMeasurement BUOM ON BUOM.Id=IRD.BaseUOMId
 LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostDrGLGeneralInfoId
 LEFT JOIN MST.BudgetMaster BM ON BM.Id=IRD.PostDrBudgetMasterId
 LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
 LEFT JOIN HKP.Activity A ON A.Id=IRD.PostDrActivityId
 LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=MM.BudgetMasterId
 LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId
 LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IRD.InventoryReceiveId
 LEFT JOIN TRN.Voucher V ON V.Id=CASE WHEN IR.EmployeeId<>'' THEN EP.VoucherId ELSE IR.VoucherId  END
 LEFT JOIN SCS.Currency CU ON CU.Id=IR.CurrencyId
 LEFT JOIN (SELECT InventoryReceiveDetailId,SUM(Qty) IssueQty FROM  TRN.InventoryIssueHistory group by InventoryReceiveDetailId) IIH ON IIH.InventoryReceiveDetailId=IRD.Id
WHERE 
IR.[Status]='Posting' and IRD.IsAsset=1  
and (isnull(IRD.BaseQty,0)-isnull(IIH.IssueQty,0))>0";

            return _sqlRepository.GetDataTable(sql);

        }


        public string AssetWIPstatusList(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        {

            ExcelEngine excelEngine = new ExcelEngine();
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

            worksheet[ROW, COL].Text = "Material";
            int colMaterialMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticleName = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "SKU1";
            int colFirstCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU2";
            int colSecondCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU3";
            int colThirdCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Storage Location";
            int colMaterialStorageLocation = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Asset Master";
            int colAssetMaster = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL";
            int colGL = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity";
            int colActivity = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN No";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Qty";
            int colTransactionQty = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Transaction UoM";
            int colTrnUOM = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Rate";
            int colTrnRate = COL;
            worksheet[ROW, COL].ColumnWidth =12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Amount";
            int colTrnAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Base Qty";
            int colBaseQty = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Base UOM";
            int colBaseUOM = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Base Rate";
            int colBaseRate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Books Amount";
            int colBooksAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Issue Qty";
            int colIssueQty = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Balance Qty";
            int colBalanceQty = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ROW++;
            int StartRow = ROW;
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
                worksheet[ROW, colAssetMaster].Text = dtGatenntryRegisterList.Rows[i]["AssetMaster"].ToString();

                worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
                worksheet[ROW, colGL].Text = dtGatenntryRegisterList.Rows[i]["GL"].ToString();
                worksheet[ROW, colActivity].Text = dtGatenntryRegisterList.Rows[i]["Activity"].ToString();
                
                worksheet[ROW, colGRNNo].Text = dtGatenntryRegisterList.Rows[i]["GRNNo"].ToString();
                worksheet[ROW, colGRNDate].Text = dtGatenntryRegisterList.Rows[i]["GRNDate"].ToString();
                worksheet[ROW, colVoucherNo].Text =dtGatenntryRegisterList.Rows[i]["VoucherNo"].ToString();

                worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TransactionQty"].ToString());
                worksheet[ROW, colTransactionQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colTrnUOM].Text = dtGatenntryRegisterList.Rows[i]["TrnUOM"].ToString();

                worksheet[ROW, colTrnRate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TrnRate"].ToString());
                worksheet[ROW, colTrnRate].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colCurrency].Text = dtGatenntryRegisterList.Rows[i]["Currency"].ToString();
                worksheet[ROW, colTrnAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TrnAmount"].ToString());
                worksheet[ROW, colTrnAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colBaseQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BaseQty"].ToString());
                worksheet[ROW, colBaseQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colBaseUOM].Text = dtGatenntryRegisterList.Rows[i]["BaseUOM"].ToString();
                worksheet[ROW, colBaseRate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BaseRate"].ToString());
                worksheet[ROW, colBaseRate].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colBooksAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BooksAmount"].ToString());
                worksheet[ROW, colBooksAmount].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colIssueQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["IssueQty"].ToString());
                worksheet[ROW, colBalanceQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BalanceQty"].ToString());

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }



           worksheet.UsedRange.WrapText = true;
           worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
           worksheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
       
           worksheet["A" + StartRow.ToString()].FreezePanes();



             var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "Asset WIP Status Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //worksheet.Range[6, 1, 7, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[6, 1, 7, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


            // return workbook;

            var filePath = "";
            var SheetName = "";
            //return workbook;
            workbook.Version = ExcelVersion.Excel97to2003;
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }

        public List<Dictionary<string, object>> GetIssueQtyList(string InventoryReceiveDetailId)
        {
            try
            {
                string sql = IssueQtySql(InventoryReceiveDetailId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string IssueQtySql(string InventoryReceiveDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select iid.InventoryIssueId IssueNo,FORMAT(ii.IssueDate,'dd-MMM-yyyy') IssueDate,SUM(isnull(iih.Qty,0)) Qty, CU.Code Currency
            ,SUM(isnull(iih.TotalMaterialBooksCurrencyAmount,0)) Amount,uom.UserName UOM,ii.VoucherId,v.VoucherNo 
            from trn.InventoryIssueHistory iih 
            left join TRN.InventoryIssueDetail iid on iid.Id=iih.InventoryIssueDetailId 
            left join trn.InventoryIssue ii on ii.Id=iid.InventoryIssueId
            left join TRN.Voucher v on v.Id=ii.VoucherId
            left join SCS.UnitOfMeasurement uom on uom.Id=iid.BaseUOMId
            left join TRN.InventoryReceiveDetail ird on ird.Id=iih.InventoryReceiveDetailId
			left join SCS.Currency CU on CU.Id=ii.CurrencyId  
            where iih.InventoryReceiveDetailId='" + InventoryReceiveDetailId + @"'
            group by iid.InventoryIssueId ,ii.IssueDate,ii.VoucherId,v.VoucherNo ,uom.UserName,CU.Code";
              
        }


        public List<Dictionary<string, object>> GetNonRegisterAssetSQL()
        {
            var sql = @"select  IIHD.IssueNo,IIHD.IssueDate,isnull(IIHD.IssueQty,0) as IssueQty,IIHD.IssueCurrency,IIHD.IssueAmount,IIHD.VoucherId IssueVoucherId,IIHD.IssueVoucherNo,IRD.Id InventoryReceiveDetailId, isnull(MM.UserName,'') MaterialMasterName	
							, MM.Id	MaterialMasterId	
							, isnull( ART.StandardName,'') ArticleName	
							, ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							, MS.UserName MaterialStorageLocation,FAM.UserName AssetMaster	
							,GL.UserName GL,B.UserName Budget
							,A.UserName Activity
							,IRD.InventoryReceiveId GRNNo,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							,V.VoucherNo
							,IRD.TransactionQty,TUOM.UserName TrnUOM
							,IRD.MaterialTranRate TrnRate
							,CU.Code Currency,IR.ToCurrencyRate  ExchangeRate
							,IRD.TotalMaterialTranAmount TrnAmount
							,IRD.BaseQty,BUOM.UserName BaseUOM,IRD.BooksCurrencyBaseRate BaseRate
							,IRD.TotalMaterialBooksCurrencyAmount BooksAmount
							,V.Id VoucherId,GL.Id GlId,A.Id ActivityId
							,case when IIHD.IsCapitalize=1 then 'Yes' else 'No' end Capitalized,(isnull(IRD.BaseQty,0)-isnull(IIHD.IssueQty,0)) BalanceQty
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
 LEFT JOIN SCS.UnitOfMeasurement TUOM ON TUOM.Id=IRD.TransactionUoMId
 LEFT JOIN SCS.UnitOfMeasurement BUOM ON BUOM.Id=IRD.BaseUOMId
 LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostDrGLGeneralInfoId
 LEFT JOIN MST.BudgetMaster BM ON BM.Id=IRD.PostDrBudgetMasterId
 LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
 LEFT JOIN HKP.Activity A ON A.Id=IRD.PostDrActivityId
 LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=MM.BudgetMasterId
 LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId
 LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IRD.InventoryReceiveId
 LEFT JOIN TRN.Voucher V ON V.Id=CASE WHEN IR.EmployeeId<>'' THEN EP.VoucherId ELSE IR.VoucherId  END
 LEFT JOIN SCS.Currency CU ON CU.Id=IR.CurrencyId
inner JOIN (select iih.InventoryReceiveDetailId,iid.InventoryIssueId IssueNo,FORMAT(ii.IssueDate,'dd-MMM-yyyy') IssueDate,SUM(isnull(iih.Qty,0)) IssueQty, CU.Code IssueCurrency
            ,SUM(isnull(iih.TotalMaterialBooksCurrencyAmount,0)) IssueAmount,uom.UserName UOM,ii.VoucherId,v.VoucherNo IssueVoucherNo,IIH.IsRegister,IIH.IsCapitalize,ird.IsAsset
            from trn.InventoryIssueHistory iih 
            left join TRN.InventoryIssueDetail iid on iid.Id=iih.InventoryIssueDetailId 
            left join trn.InventoryIssue ii on ii.Id=iid.InventoryIssueId
            left join TRN.Voucher v on v.Id=ii.CapitalizeVoucherId
            left join SCS.UnitOfMeasurement uom on uom.Id=iid.BaseUOMId
            left join TRN.InventoryReceiveDetail ird on ird.Id=iih.InventoryReceiveDetailId
			left join SCS.Currency CU on CU.Id=ii.CurrencyId  
			where ird.IsAsset=1 and ii.IssueType='Capital'
            group by iih.InventoryReceiveDetailId,iid.InventoryIssueId ,ii.IssueDate,ii.VoucherId,v.VoucherNo ,uom.UserName,CU.Code,IIH.IsRegister,IIH.IsCapitalize,ird.IsAsset) IIHD ON IIHD.InventoryReceiveDetailId=IRD.Id
WHERE IR.[Status]='Posting' AND IIHD.IsAsset=1 AND ird.IsAsset=1 and isnull(IIHD.IssueQty,0)>0 and IIHD.IsRegister=0";
            return _sqlRepository.GetDataCollection(sql);

        }


        private DataTable GetNonRegisterAssetReportSQL(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        {
            var sql = @"select  IIHD.IssueNo,IIHD.IssueDate,isnull(IIHD.IssueQty,0) as IssueQty,IIHD.IssueCurrency,IIHD.IssueAmount,IIHD.VoucherId IssueVoucherId,IIHD.IssueVoucherNo,IRD.Id InventoryReceiveDetailId, isnull(MM.UserName,'') MaterialMasterName	
							, MM.Id	MaterialMasterId	
							, isnull( ART.StandardName,'') ArticleName	
							, ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							, MS.UserName MaterialStorageLocation,FAM.UserName AssetMaster	
							,GL.UserName GL,B.UserName Budget
							,A.UserName Activity
							,IRD.InventoryReceiveId GRNNo,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							,V.VoucherNo
							,IRD.TransactionQty,TUOM.UserName TrnUOM
							,IRD.MaterialTranRate TrnRate
							,CU.Code Currency,IR.ToCurrencyRate  ExchangeRate
							,IRD.TotalMaterialTranAmount TrnAmount
							,IRD.BaseQty,BUOM.UserName BaseUOM,IRD.BooksCurrencyBaseRate BaseRate
							,IRD.TotalMaterialBooksCurrencyAmount BooksAmount
							,V.Id VoucherId,GL.Id GlId,A.Id ActivityId
							,case when IIHD.IsCapitalize=1 then 'Yes' else 'No' end Capitalized,(isnull(IRD.BaseQty,0)-isnull(IIHD.IssueQty,0)) BalanceQty
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
 LEFT JOIN SCS.UnitOfMeasurement TUOM ON TUOM.Id=IRD.TransactionUoMId
 LEFT JOIN SCS.UnitOfMeasurement BUOM ON BUOM.Id=IRD.BaseUOMId
 LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostDrGLGeneralInfoId
 LEFT JOIN MST.BudgetMaster BM ON BM.Id=IRD.PostDrBudgetMasterId
 LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
 LEFT JOIN HKP.Activity A ON A.Id=IRD.PostDrActivityId
 LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=MM.BudgetMasterId
 LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId
 LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IRD.InventoryReceiveId
 LEFT JOIN TRN.Voucher V ON V.Id=CASE WHEN IR.EmployeeId<>'' THEN EP.VoucherId ELSE IR.VoucherId  END
 LEFT JOIN SCS.Currency CU ON CU.Id=IR.CurrencyId
inner JOIN (select iih.InventoryReceiveDetailId,iid.InventoryIssueId IssueNo,FORMAT(ii.IssueDate,'dd-MMM-yyyy') IssueDate,SUM(isnull(iih.Qty,0)) IssueQty, CU.Code IssueCurrency
            ,SUM(isnull(iih.TotalMaterialBooksCurrencyAmount,0)) IssueAmount,uom.UserName UOM,ii.VoucherId,v.VoucherNo IssueVoucherNo,IIH.IsRegister,IIH.IsCapitalize,ird.IsAsset
            from trn.InventoryIssueHistory iih 
            left join TRN.InventoryIssueDetail iid on iid.Id=iih.InventoryIssueDetailId 
            left join trn.InventoryIssue ii on ii.Id=iid.InventoryIssueId
            left join TRN.Voucher v on v.Id=ii.CapitalizeVoucherId
            left join SCS.UnitOfMeasurement uom on uom.Id=iid.BaseUOMId
            left join TRN.InventoryReceiveDetail ird on ird.Id=iih.InventoryReceiveDetailId
			left join SCS.Currency CU on CU.Id=ii.CurrencyId  
			where ird.IsAsset=1 and ii.IssueType='Capital'
            group by iih.InventoryReceiveDetailId,iid.InventoryIssueId ,ii.IssueDate,ii.VoucherId,v.VoucherNo ,uom.UserName,CU.Code,IIH.IsRegister,IIH.IsCapitalize,ird.IsAsset) IIHD ON IIHD.InventoryReceiveDetailId=IRD.Id
WHERE IR.[Status]='Posting' AND IIHD.IsAsset=1 AND ird.IsAsset=1 and isnull(IIHD.IssueQty,0)>0 and IIHD.IsRegister=0";

            return _sqlRepository.GetDataTable(sql);

        }

        public string NonRegisterAssetList(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        {

            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtGatenntryRegisterList = GetNonRegisterAssetReportSQL(materialMasterId, materialMasterArticleId, voucherId, grnNo, glId, activityId);


            if (dtGatenntryRegisterList.Rows.Count == 0)
                throw new Exception("No data found");
            // throw new Exception("To date must be above or equal to From Date.");

            worksheet.Name = "Non Register Asset";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;

            worksheet[ROW, COL].Text = "Issue No";
            int colIssueNo = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue Date";
            int colIssueDate = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Material";
            int colMaterialMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticleName = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "SKU1";
            int colFirstCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU2";
            int colSecondCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU3";
            int colThirdCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            
            worksheet[ROW, COL].Text = "Asset Master";
            int colAssetMaster = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue Qty";
            int colIssueQty = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Transaction UoM";
            int colTrnUOM = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Exchange Rate";
            int colExchangeRate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Issue VoucherNo";
            int colIssueVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue Currency";
            int colIssueCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue Amount";
            int colIssueAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Storage Location";
            int colMaterialStorageLocation = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            

            worksheet[ROW, COL].Text = "GL";
            int colGL = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            //worksheet[ROW, COL].Text = "Activity";
            //int colActivity = COL;
            //worksheet[ROW, COL].ColumnWidth = 12;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "GRN No";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Transaction Qty";
            //int colTransactionQty = COL;
            //worksheet[ROW, COL].ColumnWidth = 12;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            

            worksheet[ROW, COL].Text = "Transaction Rate";
            int colTrnRate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Amount";
            int colTrnAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Base Qty";
            //int colBaseQty = COL;
            //worksheet[ROW, COL].ColumnWidth = 12;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "Base UOM";
            int colBaseUOM = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Base Rate";
            //int colBaseRate = COL;
            //worksheet[ROW, COL].ColumnWidth = 12;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            worksheet[ROW, COL].Text = "Books Amount";
            int colBooksAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            


            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ROW++;
            int StartRow = ROW;
            for (int i = 0; i < dtGatenntryRegisterList.Rows.Count; i++)
            {
                worksheet[ROW, colIssueNo].Text = dtGatenntryRegisterList.Rows[i]["IssueNo"].ToString();
                worksheet[ROW, colIssueDate].Text = dtGatenntryRegisterList.Rows[i]["IssueDate"].ToString();
                worksheet[ROW, colIssueQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["IssueQty"].ToString());
                worksheet[ROW, colIssueCurrency].Text = dtGatenntryRegisterList.Rows[i]["IssueCurrency"].ToString();
                worksheet[ROW, colIssueAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["IssueAmount"].ToString());
                worksheet[ROW, colExchangeRate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["ExchangeRate"].ToString());
                worksheet[ROW, colIssueVoucherNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["IssueVoucherId"].ToString()); 

                 // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                 worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
                // worksheet[ROW, colIsOpeningBalance].Number =clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
                worksheet[ROW, colArticleName].Text = (dtGatenntryRegisterList.Rows[i]["ArticleName"].ToString());
                worksheet[ROW, colFirstCharacteristicsValue].Text = dtGatenntryRegisterList.Rows[i]["FirstCharacteristicsValue"].ToString();
                worksheet[ROW, colSecondCharacteristicsValue].Text = dtGatenntryRegisterList.Rows[i]["SecondCharacteristicsValue"].ToString();
                worksheet[ROW, colThirdCharacteristicsValue].Text = dtGatenntryRegisterList.Rows[i]["ThirdCharacteristicsValue"].ToString();
                worksheet[ROW, colMaterialStorageLocation].Text = dtGatenntryRegisterList.Rows[i]["MaterialStorageLocation"].ToString();
                worksheet[ROW, colAssetMaster].Text = dtGatenntryRegisterList.Rows[i]["AssetMaster"].ToString();

                worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
                worksheet[ROW, colGL].Text = dtGatenntryRegisterList.Rows[i]["GL"].ToString();
                //worksheet[ROW, colActivity].Text = dtGatenntryRegisterList.Rows[i]["Activity"].ToString();

                worksheet[ROW, colGRNNo].Text = dtGatenntryRegisterList.Rows[i]["GRNNo"].ToString();
                worksheet[ROW, colGRNDate].Text = dtGatenntryRegisterList.Rows[i]["GRNDate"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtGatenntryRegisterList.Rows[i]["VoucherNo"].ToString();

                //worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TransactionQty"].ToString());
                //worksheet[ROW, colTransactionQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colTrnUOM].Text = dtGatenntryRegisterList.Rows[i]["TrnUOM"].ToString();

                worksheet[ROW, colTrnRate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TrnRate"].ToString());
                worksheet[ROW, colTrnRate].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colCurrency].Text = dtGatenntryRegisterList.Rows[i]["Currency"].ToString();
                worksheet[ROW, colTrnAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TrnAmount"].ToString());
                worksheet[ROW, colTrnAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                //worksheet[ROW, colBaseQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BaseQty"].ToString());
                //worksheet[ROW, colBaseQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colBaseUOM].Text = dtGatenntryRegisterList.Rows[i]["BaseUOM"].ToString();
                //worksheet[ROW, colBaseRate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BaseRate"].ToString());
                //worksheet[ROW, colBaseRate].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colBooksAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BooksAmount"].ToString());
                worksheet[ROW, colBooksAmount].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }



            worksheet.UsedRange.WrapText = true;
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            worksheet["A" + StartRow.ToString()].FreezePanes();



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "Asset WIP Status Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //worksheet.Range[6, 1, 7, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[6, 1, 7, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


            // return workbook;

            var filePath = "";
            var SheetName = "";
            //return workbook;
            workbook.Version = ExcelVersion.Excel97to2003;
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }

        public List<Dictionary<string, object>> GetIssueQtyList2(string InventoryReceiveDetailId)
        {
            try
            {
                string sql = IssueQtySql(InventoryReceiveDetailId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
