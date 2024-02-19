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
using Syncfusion.Pdf;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using System.Drawing;
using System.IO;
using Syncfusion.DocIO;

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
                string POBOQFromMappingSQL= POBOQMappingSql(POID);
                //Instantiate the Excel application object
                DataTable dtPOTemplateSql = _sqlRepository.GetDataTable(sql);
                DataTable dtPOBOQSql = _sqlRepository.GetDataTable(POBOQFromMappingSQL);
                if (dtPOTemplateSql.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "PO BOQ Report";
                
                var report = new ReportUtility();
                var header = POBOQReportSQL(POID);

                int ROW = 5; int COL = 1;

                #region Header
                report.SetMasterHeaderText(ref sheet, ROW, 1, "PO Number");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["PONumber"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Checked Status");
                sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, header["CheckStatus"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                sheet[ROW, 5].ColumnWidth = 30;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 7, "LC NO");
                sheet[ROW, 7].ColumnWidth = 20;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 7].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 8, header["LCNumber"].ToString());
                sheet[report.GetColumnNameForXls(8) + ROW + ":" + report.GetColumnNameForXls(9) + ROW].Merge();
                sheet[ROW, 8].ColumnWidth = 20;
                sheet.Range[ROW, 8].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Date");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["PODate"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Approved Status");
                sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, header["ApproveStatus"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                sheet[ROW, 5].ColumnWidth = 30;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 7, "Accpt. No");
                sheet[ROW, 7].ColumnWidth = 25;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 7].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 8, header["AcceptanceNo"].ToString());
                sheet[report.GetColumnNameForXls(8) + ROW + ":" + report.GetColumnNameForXls(9) + ROW].Merge();
                sheet[ROW, 8].ColumnWidth = 25;
                sheet.Range[ROW, 8].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "POType");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["POType"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Creditable  Status");
                sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, header["CredtibleStatus"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 5].ColumnWidth = 25;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 7, "Opn. Date");
                sheet[ROW, 7].ColumnWidth = 20;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 7].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 8, header["LCODate"].ToString());
                sheet[report.GetColumnNameForXls(8) + ROW + ":" + report.GetColumnNameForXls(9) + ROW].Merge();
                sheet[ROW, 8].ColumnWidth = 20;
                sheet.Range[ROW, 8].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Accpt. Date");
                sheet[ROW, 1].ColumnWidth = 25;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["AcceptanceDate"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 25;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Opn. Bank");
                sheet[ROW, 4].ColumnWidth = 20;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, header["OpeningBank"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 5].ColumnWidth = 20;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 7, "Contract NO");
                sheet[ROW, 7].ColumnWidth = 25;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 7].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 8, header["ContractNo"].ToString());
                sheet[report.GetColumnNameForXls(8) + ROW + ":" + report.GetColumnNameForXls(9) + ROW].Merge();
                sheet[ROW, 8].ColumnWidth = 25;
                sheet.Range[ROW, 8].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Benf. Bank");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["BeneficiaryBank"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                ROW++;
                ROW++;
                #endregion


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
                ROW+=4;

                int StartCol = 1;
                COL = StartCol;
                sheet[ROW, COL].Text = "PO ID";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPOID = COL;
                COL++;

                sheet[ROW, COL].Text = "BOQ ID";
                sheet[ROW, COL].ColumnWidth = 20;
                int colBOQID = COL;
                COL++;

                sheet[ROW, COL].Text = "Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colItem = COL;
                COL++;
                sheet[ROW, COL].Text = "Criteria Detail";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCriteriaDetail = COL;
                COL++;

                sheet[ROW, COL].Text = "Custoemr ref.";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCustoemrRef = COL;
                COL++;
                sheet[ROW, COL].Text = "Vendor ref";
                sheet[ROW, COL].ColumnWidth = 20;
                int colVendorref = COL;
                COL++;
                sheet[ROW, COL].Text = "Own ref";
                sheet[ROW, COL].ColumnWidth = 20;
                int colOwnRef = COL;
                COL++;
                sheet[ROW, COL].Text = "UoM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colUoM2 = COL;
                COL++;
                sheet[ROW, COL].Text = "Quantity";
                sheet[ROW, COL].ColumnWidth = 10;
                int colQuantity = COL;
                COL++;
                sheet[ROW, COL].Text = "Remark";
                sheet[ROW, COL].ColumnWidth = 20;
                int colRemark = COL;
                endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                StartRow = ROW; //row 20
                for (int i = 0; i < dtPOBOQSql.Rows.Count; i++)
                {
                    sheet[ROW, colPOID].Text = dtPOBOQSql.Rows[i]["POId"].ToString();
                    sheet[ROW, colBOQID].Text = dtPOBOQSql.Rows[i]["BOQId"].ToString();
                    sheet[ROW, colItem].Text = dtPOBOQSql.Rows[i]["Material"].ToString();
                    sheet[ROW, colDescription].Text = dtPOBOQSql.Rows[i]["RMDescription"].ToString();
                    sheet[ROW, colCriteriaDetail].Text = dtPOBOQSql.Rows[i]["CriteriaDetail"].ToString();
                    sheet[ROW, colCustoemrRef].Text = dtPOBOQSql.Rows[i]["CustomerRefNo"].ToString();
                    sheet[ROW, colVendorref].Text = dtPOBOQSql.Rows[i]["VendorRefNo"].ToString();
                    sheet[ROW, colOwnRef].Text = dtPOBOQSql.Rows[i]["OwnReferenceNo"].ToString();
                    sheet[ROW, colUoM2].Text = dtPOBOQSql.Rows[i]["UoM"].ToString();
                    sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtPOBOQSql.Rows[i]["POBOQQty"].ToString());
                    sheet[ROW, colQuantity].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colRemark].Text = dtPOBOQSql.Rows[i]["Remark"].ToString();
                 
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                //sheet.Range[StartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colPOValue, ROW, colPOValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colAcceptanceValue, ROW, colAcceptanceValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colGRNValue, ROW, colGRNValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

              //  sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "PO BOQ Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "POBOQReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }


        public Dictionary<string,object> POBOQReportSQL(string POID)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT PO.Id PONumber
                    ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                    ,POType=CASE WHEN PO.POType='POBOQ' then 'PO BOQ' ELSE 'PO BOQ' END
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
                    ,Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
					,PLC.LCRef LCNumber 
	                ,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
                    ,PLC.BenificiaryBank OpeningBank
                    ,PLC.BenificiaryBank BeneficiaryBank
					,PDA.AcceptanceNo
					,PDA.AcceptanceDate
 	                ,CNO.ContractNo

 	                
                    FROM TRN.PurchaseOrder PO
					LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
                    LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId
                    LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId
					LEFT JOIN TRN.PurchaseDocAcceptanceDetail PDAD on PDAD.POId=PO.Id
					LEFT JOIN TRN.PurchaseDocAcceptance PDA on PDA.Id=PDAD.PurchaseDocAcceptanceId
					LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
                WHERE PO.Id = '" + POID + @"' order by MM.UserName";
                return _sqlRepository.GetData(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

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
					LEFT JOIN TRN.SalesOrder SO on SO.ContractId=CNO.Id
                    LEFT JOIN trn.MasterOrderItem AS moi ON moi.Id=SO.MasterOrderItemId
                    LEFT JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
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
                WHERE PO.Id = '" + POID+@"' order by MM.UserName";
        }


        private string POBOQMappingSql(string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT po.Id POId,BOQ.Id BOQId,BOQ.RMDescription,mm.UserName AS Material,
CriteriaDetail= ISNULL(BOQ.SKUDesc,CONCAT(boq.SalesOrderId,' ',d.UserName,' ',cv1.UserName,' ',cv2.UserName)),
uom.UserName UoM,C.Code Currency,BOQ.Remark,BOQ.OrderQty,p.POBOQQty,BOQ.OwnReferenceNo,BOQ.ItemRefNo,BOQ.RMVendorSpec VendorRefNo
,BOQ.RMCustomerSpec CustomerRefNo

FROM trn.POBOQMAP AS p
LEFT OUTER JOIN BOQ ON BOQ.Id =p.BOQDetailId
LEFT OUTER JOIN trn.PurchaseOrderDetail pod ON pod.Id =p.PODetailId
LEFT OUTER JOIN trn.PurchaseOrder po ON po.Id =pod.InventoryReceiveId
LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=boq.UoMId
LEFT OUTER JOIN scs.Currency AS c ON c.Id=boq.CurrencyId
LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=boq.SalesOrderId
LEFT OUTER JOIN mst.Destination AS d ON d.Id=so.DestinationId
LEFT OUTER JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=boq.FGFirstCharacteristicsValueId
LEFT OUTER JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=boq.FGSecondCharacteristicsValueId
WHERE po.Id='" + POID+@"'";
        }

        class clsStdLib
        {
            public static string passWord = "prodDisplay";
            public clsStdLib()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                if (dayNo.ToString().Length > 1)
                {
                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                        return dayNo + "th";
                }

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";
                }
            }
            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
            ///<summary>
            ///return day difference in integer. 
            ///    Example 1: firstDate[Less Than]lastDate returns positive value
            ///    Example 2: firstDate>lastDate returns negative value
            ///    Example 3: firstDate=lastDate returns 0 [zero]**/
            /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related
            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric
            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string

            #region others
            //public void copyDataset(DataSet source, ref DataSet destination)
            //{
            //    //StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
            //    DataRow drLocal = null;
            //    for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
            //    {
            //        drLocal = destination.Tables[0].NewRow();
            //        for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
            //        {
            //            if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
            //            {
            //                drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
            //            }
            //        }
            //        destination.Tables[0].Rows.Add(drLocal);
            //    }


            //}
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others
            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;
                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }
                return total;
            }
        }




        #region Omar PurchaseOrderBOQReport 

        public void GePurchaseOrderBOQReportWithoutTax(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderBOQId)
        {
            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            fileName = "PurchaseOrderBOQ" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                //var DiscountAmount = "";

                DataTable dsOrderMaster, dsServiceItems, dsTermsAndCondition;
                dsOrderMaster = loadOrderMaster(purchaseOrderBOQId);//sql
                dsTermsAndCondition = TermsAndConditionSQL(purchaseOrderBOQId);

                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);


                dsServiceItems = loadServicerMasterItems(purchaseOrderBOQId);
                var materialTotal = makeMaterialDetailsTable(document, dsOrderMaster, purchaseOrderBOQId);//Material Details 
                var TermsAndCondition = makeTermsAndCondition(purchaseOrderBOQId, document, dsTermsAndCondition);//Terms And Conditions


                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)
                {
                    //{ServiceItems}
                    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderBOQId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);
                }
                var DiscountAmount = "";
                DiscountAmount = dsOrderMaster.Rows[0]["DiscountAmount"].ToString();
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{DiscountAmount}", (DiscountAmount).ToString() + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{AfterDiscountTotal}", ((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                document.Replace("{TrnAmount}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "PurchaseOrderBOQ" + purchaseOrderBOQId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects
            document.Close();
        }

        public double xmakeMaterialDetailsTable(WordDocument document, DataTable dsOrderMaster, string purchaseOrderBOQId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            //clsDataContext data = new clsDataContext();
            dsTax = loadMaterialTax(purchaseOrderBOQId);
            int LasColumnIndex = 14;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
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
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 80;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 60;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN No");
            //range.ApplyCharacterFormat(FontBold);
            //int colHSNCode = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar3].Width = 40;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Description");
            range.ApplyCharacterFormat(FontBold);
            int colMatDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colMatDescription].Width = 100;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            //range.ApplyCharacterFormat(FontBold);
            //int colDescription = COL; COL++;
            //wTable.Rows[ROW].Cells[colDescription].Width = 60;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Reff No");
            range.ApplyCharacterFormat(FontBold);
            int colRefferenceNo = COL; COL++;
            //wTable.Rows[ROW].Cells[colRefferenceNo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Delivery Date");
            range.ApplyCharacterFormat(FontBold);
            int colDeliveryDate = COL; COL++;
            //wTable.Rows[ROW].Cells[colDeliveryDate].Width = 50;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Origin");//TRN.PurchaseOrderDetail ->CountryId
            //range.ApplyCharacterFormat(FontBold);
            //int colOriginCountry = COL; COL++;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL++;
            //wTable.Rows[ROW].Cells[colUOM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 60;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            #endregion column headers
            //if (dv.Count > 0)
            //{
            //    wTable.Rows.Add(TemplateRow);

            //    WTableRow TROW = wTable.LastRow;
            //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
            //    {
            //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
            //        {
            //            item.Text = "";
            //        }
            //        TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
            //    }
            //    for (int i = 0; i < dv.Count; i++)
            //    {

            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //        range.ApplyCharacterFormat(FontBold);
            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //        range.ApplyCharacterFormat(FontBold);
            //    }
            //    ROW++;
            //}
            //else
            //{
            //    ROW++;
            //    wTable.AddRow();

            //}
            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            ROW++;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
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
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SKUDesc"].ToString());
                //TROW.Cells[colHSNCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colMatDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialDescription"].ToString());
                //TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                TROW.Cells[colRefferenceNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["RefferenceNo"].ToString());
                TROW.Cells[colDeliveryDate].AddParagraph().AppendText(dsOrderMaster.Rows[i]["DeliveryDate"].ToString());
                //TROW.Cells[colOriginCountry].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CountryOfOrigin"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUOM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));
                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND PurchaseOrderDetailId='" + dsOrderMaster.Rows[i]["PurchaseOrderDetailId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);



            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colMaterialGroup || C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colUOM || C == colMatDescription || C == colRefferenceNo  || C == colDeliveryDate  || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }

            #endregion Total
            ROW++;
            #region Sub Total
            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());
            #endregion Total
            ROW++;
            #region Total Payable


            #endregion Total Payable
            ROW++;
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



                foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
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
            WTableRow TROWe = wTable.LastRow;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
            {
                TROWe.Cells[i].Width = wTable.Rows[0].Cells[i].Width;
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);
            }
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

        public double makeMaterialDetailsTable(WordDocument document, DataTable dsOrderMaster, string purchaseOrderBOQId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            //clsDataContext data = new clsDataContext();
            dsTax = loadMaterialTax(purchaseOrderBOQId);
            int LasColumnIndex = 12;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        LasColumnIndex++;
            //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
            //        LasColumnIndex++;
            //    }
            //}
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);
            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 110;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 110;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 40;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Description");
            range.ApplyCharacterFormat(FontBold);
            int colMatDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colMatDescription].Width = 120;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            //range.ApplyCharacterFormat(FontBold);
            //int colDescription = COL; COL++;
            //wTable.Rows[ROW].Cells[colDescription].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Reff No");
            range.ApplyCharacterFormat(FontBold);
            int colRefferenceNo = COL; COL++;
            //wTable.Rows[ROW].Cells[colRefferenceNo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Delivery Date");
            range.ApplyCharacterFormat(FontBold);
            int colDeliveryDate = COL; COL++;
            //wTable.Rows[ROW].Cells[colDeliveryDate].Width = 50;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Origin");//TRN.PurchaseOrderDetail ->CountryId
            //range.ApplyCharacterFormat(FontBold);
            //int colOriginCountry = COL; COL++;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL++;
            //wTable.Rows[ROW].Cells[colUOM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 60;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                //for (int i = 0; i < dv.Count; i++)
                //{
                //    try
                //    {
                //        //two columns required for tax
                //        COL++;
                //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //        range.ApplyCharacterFormat(FontBold);
                //        COL++;
                //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //        range.ApplyCharacterFormat(FontBold);
                //    }
                //    catch (Exception ex)
                //    {
                //    }

                //}
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            //if (dv.Count > 0)
            //{
            //    wTable.Rows.Add(TemplateRow);
            //    ROW++;
            //    WTableRow TROW = wTable.LastRow;
            //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
            //    {
            //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
            //        {
            //            item.Text = "";
            //        }
            //        TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
            //    }
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        try
            //        {
            //            range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //            range.ApplyCharacterFormat(FontBold);
            //            range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //            range.ApplyCharacterFormat(FontBold);
            //        }
            //        catch (Exception ex)
            //        {

            //        }

            //    }
            //}
            #endregion column headers
            //if (dv.Count > 0)
            //{
            //    wTable.Rows.Add(TemplateRow);

            //    WTableRow TROW = wTable.LastRow;
            //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
            //    {
            //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
            //        {
            //            item.Text = "";
            //        }
            //        TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
            //    }
            //    for (int i = 0; i < dv.Count; i++)
            //    {

            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //        range.ApplyCharacterFormat(FontBold);
            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //        range.ApplyCharacterFormat(FontBold);
            //    }
            //    ROW++;
            //}
            //else
            //{
            //    ROW++;
            //    wTable.AddRow();

            //}
//#endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
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
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                //TROW.Cells[colHSNCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colMatDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialDetail"].ToString());
                //TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                TROW.Cells[colRefferenceNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["BuyerReferenceNo"].ToString());
                TROW.Cells[colDeliveryDate].AddParagraph().AppendText(dsOrderMaster.Rows[i]["DeliveryDate"].ToString());
                //TROW.Cells[colOriginCountry].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CountryOfOrigin"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUOM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));
                //if (dv.Count > 0)
                //{
                //    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                //    //double totalTax = 0;
                //    for (int T = 0; T < dv.Count; T++)
                //    {
                //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND PurchaseOrderDetailId='" + dsOrderMaster.Rows[i]["PurchaseOrderDetailId"].ToString() + "'";
                //        if (dvtax.Count > 0)
                //        {
                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                //        }
                //    }
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
                if (C == colMaterialGroup || C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colUOM || C == colMatDescription || C == colRefferenceNo || C == colDeliveryDate || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }

            #endregion Total
            ROW++;
            #region Sub Total
            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString());
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());
            #endregion Total
            ROW++;
            #region Total Payable


            #endregion Total Payable
            ROW++;
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



                foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


            }

            #endregion paragrpath formats
            #region merging section

            //tax codes merging (horizontal)
            //ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            //ROW++;
            //WTableRow TROWe = wTable.LastRow;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //{
            //    TROWe.Cells[i].Width = wTable.Rows[0].Cells[i].Width;
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);
            //}
            //wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            //IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            //style.CharacterFormat.Bold = true;
            //style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public DataTable loadMaterialTax(string purchaseOrderBOQId)
        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,PO.Id purchaseOrderBOQId,POD.Id PurchaseOrderDetailId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
                            INNER JOIN TRN.PurchaseOrderDetail POD ON POD.InventoryReceiveId = PO.Id
                            Inner join TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryReceiveDetailId = POD.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                            WHERE PO.Id='" + purchaseOrderBOQId + @"' 
							and InventoryReceiveDetailId  is not null and  InventoryServiceId is null AND PODT.Percentage > 0 
							ORDER BY tg.[Sequence] ";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadOrderMaster(string purchaseOrderBOQId)
        {
            string strSQL;
            try
            {
    
     strSQL= @"SELECT PO.Id PONumber
                    ,HSNC.Code HSNCode
 	                ,CNO.ContractNo
 	                ,CNO.Id ContractId
                    --,BuyerReferenceNo=STUFF((SELECT DISTINCT ','+moi.BuyerReferenceNo from
                    --   BOQ boq
                    --   INNER JOin trn.POBOQMAP xboqMap on boq.Id=xboqMap.BOQDetailId
		            --   INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id=xboqMap.PODetailId
		            --   LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=boq.MasterOrderItemId
                    --           WHERE xpod.Id=pod.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
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
                    ,POType=CASE WHEN PO.POType='PO' then 'PO Without Requisition' when PO.POType='POBOQ' then 'PO BOQ' ELSE 'PO With Requisition' END
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
                    ,POD.RefferenceNo BuyerReferenceNo
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
                    --,MRMD.MaterialDetail MaterialDetail
                     ,MaterialDetail=STUFF((SELECT DISTINCT ','+boq.RMDescription from
                            			BOQ boq
                            			INNER JOin trn.POBOQMAP xboqMap on boq.Id=xboqMap.BOQDetailId
										INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id=xboqMap.PODetailId
										LEFT JOIN CostingBOQItems xboqI on xboqI.CostingItemId=boq.CostingItemId
                            			WHERE xpod.Id=pod.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,BuyerPONumber=STUFF((SELECT DISTINCT ','+PO.PONumber from
                            			BOQ boq
                            			INNER JOin trn.POBOQMAP xboqMap on boq.Id=xboqMap.BOQDetailId
										INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id=xboqMap.PODetailId
										--LEFT JOIN CostingBOQItems xboqI on xboqI.CostingItemId=boq.CostingItemId
										LEFT JOIN [TRN].[SalesOrder] AS so ON so.CostingBOQMasterId=boq.CostingBOQMasterId
										LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                            			WHERE xpod.InventoryReceiveId=POD.InventoryReceiveId and boq.[Status]='Approved' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
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
                    --LEFT JOIN trn.MasterOrder AS mo ON mo.Id=cno.MasterOrderId
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
                WHERE PO.Id = '" + purchaseOrderBOQId + @"' order by MM.UserName";
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

        public DataTable TermsAndConditionSQL(string purchaseOrderBOQId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT  ROW_NUMBER() OVER(ORDER BY tac.Sequence) RoWNo, PO.Id POId
                        ,tac.Id TermsAndConditionMasterId,tacc.Id TermsAndConditionPOChildId,tacd.id TermsAndConditionPODetailId,
                        tacc.Title,tacd.HeaderCaption,tacd.DESCRIPTION
                        FROM TRN.PurchaseOrder AS PO
                        LEFT OUTER JOIN HKP.TermsAndConditions AS tac ON PO.TermsAndConditionsId=tac.Id
                        LEFT OUTER JOIN TermsAndConditionsPOChild AS tacc ON tacc.POId=PO.Id
                        LEFT OUTER JOIN TermsAndConditionsPODetails AS tacd ON tacd.TermsAndConditionsPOChildId=tacc.Id
                        WHERE PO.id='" + purchaseOrderBOQId + @"' Order By tac.Sequence,tacc.Id ";

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

        public DataTable loadServicerMasterItems(string purchaseOrderBOQId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT POS.Id ServiceId,SM.UserName  Service , POS.Description, POS.Amount,POS.TotalTaxAmount,Pos.AddedBy,pos.AddedDate,pos.UpdatedBy,pos.UpdatedDate FROM TRN.PurchaseOrder PO
                            INNER join TRN.POService POS ON POS.InventoryReceiveId = PO.Id
                            INNER JOIN HKP.ServiceMaster SM ON POS.ServiceMasterId = SM.Id 
                            where PO.Id = '" + purchaseOrderBOQId + @"'";


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

        

        public double makeTermsAndCondition(string purchaseOrderBOQId, WordDocument document, DataTable dsTermsAndCondition)
        {
            string replaceString = "{TermsAndCondition}";

            WCharacterFormat FontBoldUnderline = new WCharacterFormat(document);
            FontBoldUnderline.Bold = true;
            FontBoldUnderline.UnderlineStyle = UnderlineStyle.Single;

            WCharacterFormat FontBold2 = new WCharacterFormat(document);
            FontBold2.Bold = true;

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            int LasColumnIndex = 2;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            int colTermsAndCondition = COL;
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            string CmpTitile = "";
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            int colHeader = 0;
            int colDescription = 0;

            for (int i = 0; i < dsTermsAndCondition.Rows.Count; i++)
            {
                if (dsTermsAndCondition.Rows[i]["TermsAndConditionPOChildId"].ToString() != CmpTitile)
                {
                    COL = 0;
                    IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dsTermsAndCondition.Rows[i]["Title"].ToString() + ".");
                    range.ApplyCharacterFormat(FontBoldUnderline);

                    //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Header");
                    //range.ApplyCharacterFormat(FontBold);
                    colHeader = COL; COL++;
                    wTable.Rows[ROW].Cells[colHeader].Width = 150;


                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                    //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
                    //range.ApplyCharacterFormat(FontBold);
                    colDescription = COL; COL++;
                    wTable.Rows[ROW].Cells[colDescription].Width = 700;


                    // wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;
                    sl = 0;
                }
                #endregion column headers
                ROW++;
                sl++;
                wTable.AddRow();
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
                IWTextRange A = TROW.Cells[colHeader].AddParagraph().AppendText(sl + "." + dsTermsAndCondition.Rows[i]["HeaderCaption"].ToString() + ".");
                A.ApplyCharacterFormat(FontBold2);
                TROW.Cells[colDescription].AddParagraph().AppendText(sl + "." + dsTermsAndCondition.Rows[i]["DESCRIPTION"].ToString() + ".");
                CmpTitile = dsTermsAndCondition.Rows[i]["TermsAndConditionPOChildId"].ToString();
            }
            ROW++;


            #region Total
            //int TotalRow = ROW;
            //wTable.AddRow();
            //WTableRow _TROW = wTable.LastRow;

            //range.ApplyCharacterFormat(FontBold);
            #endregion Total
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;


            #endregion paragrpath formats

            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            ROW++;
            #endregion merging section


            wTable.TableFormat.Borders.BorderType = BorderStyle.None;

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }

        public double makeServiceDetailsTable(WordDocument document, DataTable dsServiceItems, string purchaseOrderBOQId)
        {
            string replaceString = "{ServiceItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;
            //clsDataContext data = new clsDataContext();
            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            dsTax = loadServiceMasterTax(purchaseOrderBOQId);
            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
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

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Services");
            int colServiceName = COL; COL++;
            range.ApplyCharacterFormat(FontBold);



            // range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            // range.ApplyCharacterFormat(FontBold);
            //var colDescription = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            int colDescription = COL; //COL++;           
            range.ApplyCharacterFormat(FontBold);




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
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);

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
            for (int i = 0; i < dsServiceItems.Rows.Count; i++)
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
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsServiceItems.Rows[i]["Service"].ToString());
                TROW.Cells[colDescription].AddParagraph().AppendText(dsServiceItems.Rows[i]["Description"].ToString());
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString());
                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsServiceItems.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colDescription || dicTaxes.ContainsValue(C))
                    continue;
                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {
                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total
            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsServiceItems.Compute("SUM(Amount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

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
                // TROW.Cells[0].Width = 120;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

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

        public DataTable loadServiceMasterTax(string purchaseOrderBOQId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT InventoryServiceId,PO.Id purchaseOrderBOQId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
                            INNER JOIN TRN.POService POS ON POS.InventoryReceiveId = PO.Id
                            INNER JOIN TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryServiceId = POS.Id
                              LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                                WHERE PO.Id='" + purchaseOrderBOQId + @"' 
								AND InventoryServiceId   IS NOT NULL AND  InventoryReceiveDetailId IS NULL 
								 ORDER BY tg.[Sequence] ";


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

        public DataTable loadServicePOTax(string purchaseOrderBOQId)
        {
            string strSQL;
            try
            {
                strSQL = @"select ServicePODetailId,SPO.Id ServicePOMasterId,SPOD.Id ServicePODetailId1,tg.Code AS TaxCode,SPOTx.Percentage, SPOTx.TaxAmount 
							from [TRN].[ServicePOMaster] SPO
                            Left JOIN 	[TRN].[ServicePODetail] SPOD ON SPOD.ServicePOMasterId = SPO.Id
                            Left join [TRN].[ServicePOTax] SPOTx ON  SPOTx.ServicePODetailId = SPOD.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=SPOTx.TaxCategoryId
                           WHERE SPO.Id='" + purchaseOrderBOQId + @"' 
							and ServicePODetailId  is not null --and  ServiceMasterId is null 
							ORDER BY tg.[Sequence]";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        #region Omar PurchaseOrderBOQReportWithTax 
        public void GePurchaseOrderBOQReportWithTax(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderBOQId)
        {
            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            fileName = "PurchaseOrderBOQ" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                //var DiscountAmount = "";

                DataTable dsOrderMaster, dsServiceItems, dsTermsAndCondition;
                dsOrderMaster = loadOrderMaster(purchaseOrderBOQId);//sql
                dsTermsAndCondition = TermsAndConditionSQL(purchaseOrderBOQId);

                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);


                dsServiceItems = loadServicerMasterItems(purchaseOrderBOQId);
                var materialTotal = makeMaterialDetailsTableWithTax(document, dsOrderMaster, purchaseOrderBOQId);//Material Details 
                var TermsAndCondition = makeTermsAndCondition(purchaseOrderBOQId, document, dsTermsAndCondition);//Terms And Conditions


                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)
                {
                    //{ServiceItems}
                    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderBOQId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);
                }
                var DiscountAmount = "";
                DiscountAmount = dsOrderMaster.Rows[0]["DiscountAmount"].ToString();
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{DiscountAmount}", (DiscountAmount).ToString() + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{AfterDiscountTotal}", ((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                document.Replace("{TrnAmount}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "PurchaseOrderBOQ" + purchaseOrderBOQId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects
            document.Close();
        }
        public double makeMaterialDetailsTableWithTax(WordDocument document, DataTable dsOrderMaster, string purchaseOrderBOQId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            //clsDataContext data = new clsDataContext();
            dsTax = loadMaterialTax(purchaseOrderBOQId);
            int LasColumnIndex = 11;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
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
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 115;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 45;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            //range.ApplyCharacterFormat(FontBold);
            //int colChar3 = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar3].Width = 35;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Description");
            range.ApplyCharacterFormat(FontBold);
            int colMatDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colMatDescription].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Ref No");
            range.ApplyCharacterFormat(FontBold);
            int colRefferenceNo = COL; COL++;
            wTable.Rows[ROW].Cells[colRefferenceNo].Width = 65;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Delivery Date");
            range.ApplyCharacterFormat(FontBold);
            int colDeliveryDate = COL; COL++;
            wTable.Rows[ROW].Cells[colDeliveryDate].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 35;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL++;
            wTable.Rows[ROW].Cells[colUOM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 50;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                       
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            #endregion column headers
            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);

                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);
                }
                ROW++;
            }
            else
            {
                ROW++;
                wTable.AddRow();

            }
            //#endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
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
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                //TROW.Cells[colHSNCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colMatDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialDetail"].ToString());
                //TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                TROW.Cells[colRefferenceNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["RefferenceNo"].ToString());
                TROW.Cells[colDeliveryDate].AddParagraph().AppendText(dsOrderMaster.Rows[i]["DeliveryDate"].ToString());
                //TROW.Cells[colOriginCountry].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CountryOfOrigin"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUOM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));
                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND PurchaseOrderDetailId='" + dsOrderMaster.Rows[i]["PurchaseOrderDetailId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);



            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colMaterialGroup || C == colRate || C == colArticle || C == colChar1 || C == colChar2  || C == colUOM || C == colMatDescription || C == colRefferenceNo ||  C == colDeliveryDate  || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }

            #endregion Total
            ROW++;
            #region Sub Total
            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());
            #endregion Total
            ROW++;
            #region Total Payable


            #endregion Total Payable
            ROW++;
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



                foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
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
            WTableRow TROWe = wTable.LastRow;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
            {
                TROWe.Cells[i].Width = wTable.Rows[0].Cells[i].Width;
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);
            }
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

        #endregion Omar PurchaseOrderBOQReportWithTax 
    }
}
