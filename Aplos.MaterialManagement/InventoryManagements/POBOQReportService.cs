using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
#region Using
using Syncfusion.DocIO.DLS;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Collections.Specialized;
using System.Linq;
using Syncfusion.XlsIO;
using Library.Service.Helpers;

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
    public class POBOQReportService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructors
        public POBOQReportService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        #region Report
        public void POBOQReport(string POID)
        {
            try
            {

                //if (string.IsNullOrEmpty(entityid) || entityid == "''")
                //    throw new Exception("Select entity");

                string sql = POTemplateSql(POID);

                //Instantiate the Excel application object
                DataTable dtPOTemplateSql = _sqlRepository.GetDataTable(sql);
                if (dtPOTemplateSql.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "PO BOQ Report";

                int ROW = 6;
                int COL = 1;

                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 20;
                int colMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 20;
                int colArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "SKU 1";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSKU1 = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU 2";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSKU2 = COL;
                COL++;

                sheet[ROW, COL].Text = "SKU 3";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSKU3 = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Description";
                sheet[ROW, COL].ColumnWidth = 20;
                int colMaterialDescription = COL;
                COL++;
                sheet[ROW, COL].Text = "Description";
                sheet[ROW, COL].ColumnWidth = 20;
                int colDescription = COL;
                COL++;
                sheet[ROW, COL].Text = "Ref No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Description";
                sheet[ROW, COL].ColumnWidth = 20;
                int colSODesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Origin";
                sheet[ROW, COL].ColumnWidth = 14;
                int colOrigin = COL;
                COL++;
                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].ColumnWidth = 10;
                int colQty = COL;
                COL++;
                sheet[ROW, COL].Text = "UoM";
                sheet[ROW, COL].ColumnWidth = 8;
                int colUoM = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 10;
                int colRate = COL;
                COL++;
                sheet[ROW, COL].Text = "Total Amount";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalAmount = COL;
              
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
             
                int StartRow = ROW; //row 20
                for (int i = 0; i < dtPOTemplateSql.Rows.Count; i++)
                {
                    sheet[ROW, colMaterial].Text = dtPOTemplateSql.Rows[i]["MaterialMaster"].ToString();
                    sheet[ROW, colArticle].Text = dtPOTemplateSql.Rows[i]["Article"].ToString();
                    sheet[ROW, colSKU1].Text = dtPOTemplateSql.Rows[i]["FirstCharacteristicsValue"].ToString();
                    sheet[ROW, colSKU2].Text = dtPOTemplateSql.Rows[i]["SecondCharacteristicsValue"].ToString();
                    sheet[ROW, colSKU3].Text = dtPOTemplateSql.Rows[i]["ThirdCharacteristicsValue"].ToString();
                    sheet[ROW, colMaterialDescription].Text =dtPOTemplateSql.Rows[i]["MaterialDetail"].ToString();
                    sheet[ROW, colDescription].Text = dtPOTemplateSql.Rows[i]["Description"].ToString();
                    sheet[ROW, colRefNo].Text = dtPOTemplateSql.Rows[i]["RefferenceNo"].ToString();
                    sheet[ROW, colDeliveryDate].Text = dtPOTemplateSql.Rows[i]["DeliveryDate"].ToString();
                    sheet[ROW, colOrigin].Text = dtPOTemplateSql.Rows[i]["CountryOfOrigin"].ToString();
                    sheet[ROW, colQty].Number =clsStaticInfo.dbl( dtPOTemplateSql.Rows[i]["POTransactionQty"].ToString());
                    sheet[ROW, colQty].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colUoM].Text = dtPOTemplateSql.Rows[i]["TransactionUoM"].ToString();
                    sheet[ROW, colRate].Number = clsStaticInfo.dbl(dtPOTemplateSql.Rows[i]["TransactionRate"].ToString());
                    sheet[ROW, colRate].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colTotalAmount].Number = clsStaticInfo.dbl(dtPOTemplateSql.Rows[i]["TrnAmount"].ToString());
                    sheet[ROW, colTotalAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                //sheet.Range[StartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colPOValue, ROW, colPOValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colAcceptanceValue, ROW, colAcceptanceValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colGRNValue, ROW, colGRNValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Bulletin Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "BulletinReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        private string POTemplateSql(string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT PO.Id PONumber
                    ,HSNC.Code HSNCode
 	                ,CNO.ContractNo
 	                ,CNO.Id ContractId
                    ,mo.BuyerReferenceNo 
					,PLC.LCRef LCNumber 
                    ,PLC.BenificiaryBank BeneficiaryBank
                    ,PLC.BenificiaryBank OpeningBank
					--,B.UserName BeneficiaryBank
					--,B.UserName OpeningBank
                    ,PO.CompanyGroupId
                    ,PO.CompanyId
                    ,Plant.GSTIN
	                ,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
                    ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                    ,POType=CASE WHEN PO.POType='PO' then 'PO Without Requisition' ELSE 'PO With Requisition' END
                    ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                    ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
                    ,PO.InvoicingPartyPlantId
                    ,INVPARTYPL.UserName InvoicingPartyName
                    ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                    ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                    ,ISNULL(PO.InvoicingByAddress,'') InvoicingByAddress
                    ,PO.DeliveryByAddress
                    ,DPARTYPL.UserName DeliveryParty
                    ,PO.DeliveryPartyPlantId
                    ,POD.InventoryMaterialId MaterialMasterId
                    ,PO.DocRefNo
                    ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
                    ,CheckedBy=CASE WHEN PO.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                    ,AuthorizedBy=CASE When PO.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                    ,AddedBy=CASE When PO.CheckedByStatus='pending' OR PO.CheckedByStatus='Hold' OR PO.CheckedByStatus='Reject' OR PO.CheckedByStatus='Checked'then eI3.EmployeeName else PO.AddedBy  END 
                    ,PO.AddedDate
                    ,PO.UpdatedBy
                    ,PO.UpdatedDate
                    ,PO.IsApproved
                    ,PO.PartyType
                    ,PO.PartyId
                    ,POD.RefferenceNo
                    ,isnull(PO.DiscountAmount,0) DiscountAmount
                    ,ISNULL(PO.DeliveryInstruction,'') DeliveryInstruction
                    ,ISNULL(PO.SpecialInstruction,'') SpecialInstruction
                    ,Party.UserName VendorName
                    ,Party.AddressMasterId VendorAddressMasterId
                    ,Party.TINNO VendorGSTIN
                    ,Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                    ,PO.CurrencyId
                    ,CRNC.Code AS CurrencyName
                    ,PO.ToCurrencyRate
                    ,BASECRNC.Code AS BaseCurrencyName
                    ,PayTerm.UserName PaymentTerm
                    ,MM.UserName MaterialMaster
                    ,MM.MaterialGroupMasterId
                    ,MGM.UserName MaterialGroupMaster
                    ,POD.ArticleId
                    ,MMA.StandardName Article
                    ,FC.Id FirstCharId
                    ,FC.UserName FirstChar
                    ,POD.FirstCharacteristicsValueId
                    ,FCV.UserName AS FirstCharacteristicsValue
                    ,POD.SecondCharacteristicsValueId
                    ,SCV.UserName AS SecondCharacteristicsValue
                    ,POD.ThirdCharacteristicsValueId
                    ,TCV.UserName AS ThirdCharacteristicsValue
                    ,SC.Id SecondCharId
                    ,SC.UserName SecondChar
                    ,TC.Id ThirdCharId
                    ,TC.UserName ThirdChar
                    ,ROUND(POD.TransactionQty, 2) POTransactionQty
                    ,ROUND(POD.TransactionRate, 4) TransactionRate
                    ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
                    ,POD.BaseAmount
                    ,POD.TotalTaxAmount AS BaseTaxAmount
                    ,REPLACE(Convert(VARCHAR(11), POD.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                    ,TaxAmount = (
                    SELECT SUM(TaxAmount)
                    FROM [TRN].[PurchaseOrderTax]
                    WHERE InventoryReceiveDetailId = POD.Id
                    )
                    ,ServiceTaxAmount = (
                    SELECT SUM(TotalTaxAmount)
                    FROM [TRN].[POService]
                    WHERE InventoryReceiveId = POD.InventoryReceiveId
                    )
                    ,POD.Description
                    ,POD.ChargesAmount
                    ,POD.CountryId
                    ,POCountry.UserName CountryOfOrigin
                    ,POD.Id PurchaseOrderDetailId
                    ,POD.TransactionUoMId
                     ,TUoM.ShortName AS TransactionUoM
                    ,MRMD.MaterialDetail MaterialDetail
                    ,CheckStatus= CASE when PO.CheckedByStatus='pending' Then 'To be checked'
                    when PO.CheckedByStatus='Hold' Then 'Hold'
                    when PO.CheckedByStatus='Reject' Then 'Reject'
                    when PO.CheckedByStatus='Checked' Then 'Checked'
                    else ''
                    END
                    ,ApproveStatus= CASE
                    when PO.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                    when PO.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                    when PO.AuthorizedByStatus='For Approval' Then 'To be Approval'
                    when PO.AuthorizedByStatus='Approved' Then 'Approved'
                    else ''
                    END
                    FROM TRN.PurchaseOrder PO
                    LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                    LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                    LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                    LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                    LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                    LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                    LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                    LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId
                    LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId
					LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
                    LEFT JOIN trn.MasterOrder AS mo ON mo.Id=cno.MasterOrderId
					LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
	               -- LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                    LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
                    LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId
                    LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId
	                LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                    LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POD.ArticleId
                    LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                    LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id=POD.RequisitionDetailId
                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=PO.CheckedBy
                    LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=PO.AuthorizedBy
                    left join [SEC].[User] U on U.UserId=PO.AddedBy
                    LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
                WHERE PO.Id = '"+POID+@"' order by MM.UserName";
        }
    }
}
