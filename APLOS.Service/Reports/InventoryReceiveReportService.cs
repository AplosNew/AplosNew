using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Organizations;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Organizations;
using Library.ViewModel.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Library.Service.Reports
{
    public class InventoryReceiveReportService : IInventoryReceiveReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<Plant> _plantRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        public InventoryReceiveReportService(ISqlRepository sqlRepository
            , IRepositoryAsync<Plant> plantRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService)
        {
            _plantRepository = plantRepository;
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }

        public IWorkbook GetInventoryReceiveReport(string companyId, string plantId, string inventoryReceiveId)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                CreateInventoryReceiveReportSheet(ref sheet1, ref sheet2, report, "Inventory Receive", "Summary", companyId, plantId, inventoryReceiveId);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void CreateInventoryReceiveReportSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string inventoryReceiveId)
        {
            var cmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inventoryReceiveId + @"'
                               , @totalBaseAmount DECIMAL(18, 4)=0
                               , @totalReceiveAmount DECIMAL(18, 4)=0
                               , @totalReceiveTaxAmount DECIMAL(18, 4)=0
                               , @totalServiceAmount DECIMAL(18, 4)=0
                               , @totalSvcTaxAmount DECIMAL(18, 4)=0
                              SET @totalBaseAmount=ISNULL((SELECT SUM(TotalMaterialBooksCurrencyAmount) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId),0)
                            SET @totalReceiveAmount=ISNULL((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId),0)
                            SET @totalReceiveTaxAmount=ISNULL((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId IS NULL),0)
                            SET @totalServiceAmount=ISNULL((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId),0)
                            SET @totalSvcTaxAmount=ISNULL((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>''),0)
                           SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId
	                            , MGM.UserName AS MaterialGroupMasterName, p.Code +' - '+ p.UserName Vendor, IR.DocRefNo
	                            , IR.GateEntryNo, IR.Id GRNNo, IR.InvoicingByAddress, IR.DeliveryByAddress
	                            , REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
	                            , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
	                            , MG.UserName StorageLocation, IR.InvoiceNo
	                            , REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy
                                , CASE IR.IsNonCreditable WHEN 1 THEN 'No' ELSE 'Yes' END Creditable
	                            , cp.UserName PartyAccountGroupName,MM.UserName MaterialMasterName
                                , IM.MaterialMasterId, IM.ArticleId, ART.StandardName
                                , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                                , IRD.TransactionQty, IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, CONVERT(decimal(10,2),IRD.MaterialTranRate) as TransactionRate, CU.Code AS CurrencyName
                                , (IRD.TransactionQty*IRD.MaterialTranRate) AS TrnAmount, IRD.TotalTaxAmount AS BaseTaxAmount
	                            , IRD.TotalMaterialBooksCurrencyAmount AS BaseAmount, BaseAmountTotal=@totalBaseAmount-@totalServiceAmount, BaseTaxAmountTotal=@totalReceiveTaxAmount*IR.ToCurrencyRate
	                            , ChargesAmountTotal=@totalServiceAmount
	                            , GrossTotal=CASE WHEN IR.IsNonCreditable=1 THEN @totalBaseAmount ELSE @totalBaseAmount+(@totalReceiveTaxAmount*IR.ToCurrencyRate)+@totalSvcTaxAmount END
	                            , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
	                            , IRD.ChargesTranAmount ChargesAmount
	                            , ServiceCharge=ISNULL((@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount,0)
	                            , TotalSvcTaxAmount=@totalSvcTaxAmount
	                            , ServiceTax=ISNULL((@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount,0)
	                            , IRD.CountryId, IR.EmployeeId, EMP.EmployeeCode, EMP.EmployeeName
                            FROM TRN.InventoryMaterial AS IM
                            JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                            JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                            JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON IR.EmployeeId=EMP.SystemId
                            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            JOIN [HKP].[MaterialStorage] MG ON IR.MaterialStorageId=MG.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            WHERE IRD.InventoryReceiveId=@inventoryReceiveId";
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            var cmdSText = @"SELECT A.Id, A.InventoryReceiveId, A.ServiceMasterId, B.UserName AS ServiceMasterName, A.Amount, A.TotalTaxAmount
                            FROM [TRN].[InventoryService] AS A JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id WHERE A.InventoryReceiveId='" + inventoryReceiveId + "'";
            var inventoryServiceList = _sqlRepository.GetDataTable(cmdSText);
            var empId = inventoryMaterialList.Rows[0]["EmployeeId"].ToString();

            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _row = 5;
            if (!string.IsNullOrEmpty(empId))
            {
                report.SetMasterHeaderText(ref sheet1, _row, 1, "Employee Code");
                report.SetMasterHeaderText(ref sheet2, _row, 1, "Employee Code");
                report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["EmployeeCode"].ToString());
                report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["EmployeeCode"].ToString());
                sheet1.Range[_row, 2, _row, 2].Merge();
                sheet2.Range[_row, 2, _row, 3].Merge();
                _row++;
            }
            report.SetMasterHeaderText(ref sheet1, _row, 1, "Vendor");
            report.SetMasterHeaderText(ref sheet2, _row, 1, "Vendor");
            report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["Vendor"].ToString());
            report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["Vendor"].ToString());
            sheet1.Range[_row, 2, _row, 2].Merge();
            sheet2.Range[_row, 2, _row, 3].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet1, _row, 1, "Invoicing By");
            report.SetMasterHeaderText(ref sheet2, _row, 1, "Invoicing By");
            report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["InvoicingBy"].ToString());
            report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["InvoicingBy"].ToString());
            sheet1.Range[_row, 2, _row, 2].Merge();
            sheet2.Range[_row, 2, _row, 3].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet1, _row, 1, "Delivery By");
            report.SetMasterHeaderText(ref sheet2, _row, 1, "Delivery By");
            report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["DeliveryBy"].ToString());
            report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["DeliveryBy"].ToString());
            sheet1.Range[_row, 2, _row, 2].Merge();
            sheet2.Range[_row, 2, _row, 3].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet1, _row, 1, "Vendor Doc. RefNo");
            report.SetMasterHeaderText(ref sheet2, _row, 1, "Vendor Doc. RefNo");
            report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["DocRefNo"].ToString());
            report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["DocRefNo"].ToString());
            sheet1.Range[_row, 2, _row, 2].Merge();
            sheet2.Range[_row, 2, _row, 3].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet1, _row, 1, "Gate Entry No");
            report.SetMasterHeaderText(ref sheet2, _row, 1, "Gate Entry No");
            report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["GateEntryNo"].ToString());
            report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["GateEntryNo"].ToString());
            sheet1.Range[_row, 2, _row, 2].Merge();
            sheet2.Range[_row, 2, _row, 3].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet1, _row, 1, "GRN No");
            report.SetMasterHeaderText(ref sheet2, _row, 1, "GRN No");
            report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["GRNNo"].ToString());
            report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["GRNNo"].ToString());
            sheet1.Range[_row, 2, _row, 2].Merge();
            sheet2.Range[_row, 2, _row, 3].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet1, _row, 1, "Storage Location");
            report.SetMasterHeaderText(ref sheet2, _row, 1, "Storage Location");
            report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["StorageLocation"].ToString());
            report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["StorageLocation"].ToString());
            sheet1.Range[_row, 2, _row, 2].Merge();
            sheet2.Range[_row, 2, _row, 3].Merge();
            _row++;
            var _rowL = _row;
            var row = _row + 1;
            var _rowR = 5;
            if (!string.IsNullOrEmpty(empId))
            {
                report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Employee Name");
                report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Employee Name");
                report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["EmployeeName"].ToString());
                report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["EmployeeName"].ToString());
                sheet1.Range[_rowR, 4, _rowR, 5].Merge();
                sheet2.Range[_rowR, 5, _rowR, 8].Merge();
                _rowR++;
            }

            report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Invoice No");
            report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Invoice No");
            report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["InvoiceNo"].ToString());
            report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["InvoiceNo"].ToString());
            sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Creditable");
            report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Creditable");
            report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["Creditable"].ToString());
            report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["Creditable"].ToString());
            sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;


            report.SetMasterHeaderText(ref sheet1, _rowR, 3, "A/C Group");
            report.SetMasterHeaderText(ref sheet2, _rowR, 4, "A/C Group");
            report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["PartyAccountGroupName"].ToString());
            report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["PartyAccountGroupName"].ToString());
            sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Invoicing By Address");
            report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Invoicing By Address");
            report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["InvoicingByAddress"].ToString());
            report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["InvoicingByAddress"].ToString());
            sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Delivery By Address");
            report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Delivery By Address");
            report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["DeliveryByAddress"].ToString());
            report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["DeliveryByAddress"].ToString());
            sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Doc Date");
            report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Doc Date");
            report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["DocDate"].ToString());
            report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["DocDate"].ToString());
            sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Entry Date");
            report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Entry Date");
            report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["EntryDate"].ToString());
            report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["EntryDate"].ToString());
            sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            sheet2.Range[_rowR, 5, _rowR, 8].Merge();

            var _rowRN = 5;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "GRND Date");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "GRND Date");
            report.SetText(ref sheet1, _rowRN, 7, inventoryMaterialList.Rows[0]["GRNDate"].ToString());
            report.SetText(ref sheet2, _rowRN, 10, inventoryMaterialList.Rows[0]["GRNDate"].ToString());
            _rowRN++;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Invoice Date");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Invoice Date");
            report.SetText(ref sheet1, _rowRN, 7, inventoryMaterialList.Rows[0]["InvoiceDate"].ToString());
            report.SetText(ref sheet2, _rowRN, 10, inventoryMaterialList.Rows[0]["InvoiceDate"].ToString());
            _rowRN++;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Currency");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Currency");
            report.SetText(ref sheet1, _rowRN, 7, inventoryMaterialList.Rows[0]["CurrencyName"].ToString());
            report.SetText(ref sheet2, _rowRN, 10, inventoryMaterialList.Rows[0]["CurrencyName"].ToString());
            _rowRN++;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Amount (BC)");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Amount (BC)");

            report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            _rowRN++;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Tax (BC)");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Tax (BC)");

            report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseTaxAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseTaxAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            _rowRN++;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Charges (BC)");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Charges (BC)");

            report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["ChargesAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["ChargesAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            _rowRN++;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Charges Tax(BC)");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Charges Tax(BC)");

            report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["TotalSvcTaxAmount"].ToString()), ExcelHAlign.HAlignLeft);
            report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["TotalSvcTaxAmount"].ToString()), ExcelHAlign.HAlignLeft);
            _rowRN++;

            report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Gross Total");
            report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Gross Total");

            report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["GrossTotal"].ToString()), ExcelHAlign.HAlignLeft);
            report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["GrossTotal"].ToString()), ExcelHAlign.HAlignLeft);
            _rowRN++;

            var sheet1headreColIndex = 1;
            var sheet2headreColIndex = 1;
            _rowL += 1;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group", 24);
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Material Group", 24);
            sheet1headreColIndex++;
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Master", 24);
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Material Master", 24);
            sheet1headreColIndex++;
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Article");
            sheet1headreColIndex++;
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty", 18);
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Qty", 18);
            sheet1headreColIndex++;
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Rate");
            sheet1headreColIndex++;
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount (TRN)", ExcelHAlign.HAlignRight);
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Amount (TRN)", ExcelHAlign.HAlignRight);
            sheet1headreColIndex++;
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Tax", 12, ExcelHAlign.HAlignRight);
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Tax", 12, ExcelHAlign.HAlignRight);
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Service Charge", ExcelHAlign.HAlignRight);
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Service Tax", 16, ExcelHAlign.HAlignRight);
            sheet2headreColIndex++;
            report.SetHeaderText(ref sheet2, _rowL, sheet2headreColIndex, "Material Amount (BC)", ExcelHAlign.HAlignRight);

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet2, _rowL, 1, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet2, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["StandardName"].ToString());
                report.SetText(ref sheet2, _rowL, 3, inventoryMaterialList.Rows[n]["StandardName"].ToString());
                report.SetText(ref sheet1, _rowL, 4, Convert.ToDouble(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()) + " " + inventoryMaterialList.Rows[n]["TransactionUoM"].ToString());
                report.SetText(ref sheet2, _rowL, 4, Convert.ToDouble(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()) + " " + inventoryMaterialList.Rows[n]["TransactionUoM"].ToString());
                report.SetText(ref sheet1, _rowL, 5, Convert.ToDouble(inventoryMaterialList.Rows[n]["TransactionRate"].ToString()) + " " + inventoryMaterialList.Rows[n]["CurrencyName"].ToString());
                report.SetText(ref sheet2, _rowL, 5, Convert.ToDouble(inventoryMaterialList.Rows[n]["TransactionRate"].ToString()) + " " + inventoryMaterialList.Rows[n]["CurrencyName"].ToString());
                report.SetText(ref sheet1, _rowL, 6, Convert.ToDouble(inventoryMaterialList.Rows[n]["TrnAmount"].ToString()));
                report.SetText(ref sheet2, _rowL, 6, Convert.ToDouble(inventoryMaterialList.Rows[n]["TrnAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 7, Convert.ToDouble(inventoryMaterialList.Rows[n]["BaseTaxAmount"].ToString()));

                report.SetText(ref sheet2, _rowL, 7, Convert.ToDouble(inventoryMaterialList.Rows[n]["Creditable"].ToString() == "Yes" ? 0 : Convert.ToDouble(inventoryMaterialList.Rows[n]["BaseTaxAmount"].ToString())));
                report.SetText(ref sheet2, _rowL, 8, Convert.ToDouble(inventoryMaterialList.Rows[n]["ServiceCharge"].ToString()));
                report.SetText(ref sheet2, _rowL, 9, Convert.ToDouble(inventoryMaterialList.Rows[n]["Creditable"].ToString() == "Yes" ? 0 : Convert.ToDouble(inventoryMaterialList.Rows[n]["ServiceTax"].ToString())));
                report.SetText(ref sheet2, _rowL, 10, Convert.ToDouble(inventoryMaterialList.Rows[n]["BaseAmount"].ToString()));
            }

            #region sumCalc

            _rowL++;
            sheet1.Range[_rowL, 1, _rowL, 5].Merge();
            sheet2.Range[_rowL, 1, _rowL, 5].Merge();
            report.SetText(ref sheet1, _rowL, 1, "Total :", true);
            report.SetText(ref sheet2, _rowL, 1, "Total :", true);

            var totalCountNeed = 5;
            var sumdrcrCol = 6;
            for (int i = 1; i <= totalCountNeed; i++)
            {
                if (i < 3)
                {
                    sheet1.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                    sheet1.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                    sheet1.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet1.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }

                sheet2.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet2.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                sheet2.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sheet2.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);


                sumdrcrCol++;
            }
            #endregion sumCalc

            sheet2.Range[(row), 1, _rowL, sheet2headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet2.Range[(row), 1, _rowL, sheet2headreColIndex].BorderAround(ExcelLineStyle.Hair);


            _rowL++;
            if (inventoryServiceList.Rows.Count != 0)
            {
                _rowL++;
                var serviceHeadreColIndex = 1;
                report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Service", 32);
                serviceHeadreColIndex++;
                report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Amount (TRN)", 32, ExcelHAlign.HAlignRight);
                serviceHeadreColIndex++;
                report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Total Tax", 26, ExcelHAlign.HAlignRight);


                for (int n = 0; n < inventoryServiceList.Rows.Count; n++)
                {
                    _rowL++;
                    report.SetText(ref sheet1, _rowL, 1, inventoryServiceList.Rows[n]["ServiceMasterName"].ToString());
                    report.SetText(ref sheet1, _rowL, 2, Convert.ToDouble(inventoryServiceList.Rows[n]["Amount"].ToString()));
                    report.SetText(ref sheet1, _rowL, 3, Convert.ToDouble(inventoryServiceList.Rows[n]["TotalTaxAmount"].ToString()));
                }

            }
            #region sum

            _rowL++;
            report.SetText(ref sheet1, _rowL, 1, "Total :", true);

            var loopCount = 2;
            var colNo = 2;
            for (int i = 1; i <= loopCount; i++)
            {
                sheet1.Range[_rowL, colNo].Formula = "=SUM(" + report.GetColumnNameForXls(colNo) + Row_Total_Start + ":" + report.GetColumnNameForXls(colNo) + (_rowL - 1) + ")";
                sheet1.Range[_rowL, colNo].NumberFormat = report.NumberFormatDecimalTwo();
                sheet1.Range[_rowL, colNo].CellStyle.Font.Bold = true;
                sheet1.Range[_rowL, colNo].BorderAround(ExcelLineStyle.Hair);
                colNo++;
            }
            #endregion sumCalc

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            #region Signature

            _rowL = _rowL + 4;
            sheet1.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet1.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet1.Range[_rowL, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            report.SetText(ref sheet1, _rowL, 1, "Prepared By", true);
            report.SetText(ref sheet1, _rowL, 3, "Checked By", true);
            report.SetText(ref sheet1, _rowL, 6, "Authorized By", true);

            sheet2.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet2.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet2.Range[_rowL, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            report.SetText(ref sheet2, _rowL, 1, "Prepared By", true);
            report.SetText(ref sheet2, _rowL, 3, "Checked By", true);
            report.SetText(ref sheet2, _rowL, 6, "Authorized By", true);

            #endregion Signature

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet1, sheet2headreColIndex, sheet1Name, companyId, plantName, null);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

            sheet2.Name = sheet2Name;
            sheet2.UsedRange.WrapText = true;
            sheet2.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet2, sheet2headreColIndex, sheet2Name, companyId, plantName, null);
            report.PageSetup(ref sheet2, 5, ExcelPageOrientation.Landscape);
        }

        #region Payable


        private IEnumerable<InventoryReportViewModel> GetServicePayable(string companyId, string plantId, string serviceAcknowledgementId)
        {
            try
            {
                var sql = @"SELECT  NULL OtherName, TrnType=Case when VD.DrAmount=0 then 'Cr' else 'Dr' End
							, NULL MaterialGroupMasterId, NULL TaxCategoryId,VD.GLGeneralInfoId
							, GL.AccountCode GLGeneralInfoCode
							,GL.UserName  GLGeneralInfoName
							,VD.BudgetMasterId
							,B.Code  BudgetCode
							,B.UserName  BudgetName
							,VD.ActivityId 
							,A.Code ActivityCode
							,A.UserName  ActivityName
							,VD.DrAmount Dr
							,vd.CrAmount Cr
                            ,V.Narration
                        FROM TRN.VoucherDetail VD 
                        LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
                        LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                        LEFT JOIN TRN.Invoice IV ON IV.VoucherId=V.Id
                        LEFT JOIN TRN.[ServiceAcknowledgementMaster] IR ON IR.Id=IV.ServiceAcknowledgementMasterId
                        LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
                        WHERE IV.ServiceAcknowledgementMasterId='" + serviceAcknowledgementId + @"'";
                return _sqlRepository.GetModelCollection<InventoryReportViewModel>(sql);
            }
            catch (CustomException)
            {
                throw;
            }
        }

        #endregion

       
    }
}