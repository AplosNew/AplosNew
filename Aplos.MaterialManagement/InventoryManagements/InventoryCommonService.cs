
#region Using
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using Library.Service.Enums;
using Library.Service.Logs;

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
    public class InventoryCommonService
    {
        private readonly ISqlRepository _sqlRepository;

        #region Constructor
        public InventoryCommonService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion Constructor


        public IEnumerable<object> GetMaterialTransferRegister(string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                //if (Type == "Posted")
                //{
                sql = @"SELECT 
						    REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,IR.Id As GRNId
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,IRD.Id As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,MT.UserName MaterialType
						  ,MGM.UserName AS MaterialGroupMasterName
						  ,IM.MaterialMasterId
						  ,MM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, ART.StandardName ArticleName
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
						,IRD.ShortageQty
						,IRD.ShortageRatePercent
						,IRD.ShortageValue
						,IRD.RejectionQty
						,IRD.RejectRatePercent
						,IRD.RejectValue
						,IRD.RejectClamPercent
						,IRD.ApprovedQty
						,IR.IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
						,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
						,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                         ,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,Case When IR.IsNonCreditable = 1 
							then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						when IR.IsNonCreditable = 0
							then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						end TotalMaterialTranAmount
                       ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approval' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,IGL.UserName AS GL
						,IA.UserName Activity
						,B.UserName AS Budget
                        ,IGL1.UserName AS CGL
						,IA1.UserName AS CActivity
						,B1.UserName AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IR.POId
						,IRD.PODetailsId AS PORowId
                        ,MS.UserName as StorageLocation
						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                        ,isnull(p.TINNO,'') GSTINNo
					from TRN.InventoryMaterial AS IM
					JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId  
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
                    LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
	                LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
					LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
                    where GRNType='MaterialTransfer' AND IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' ORDER BY IR.GRNDate ASC";
                return _sqlRepository.GetDataCollection(sql);




            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IWorkbook CreateMaterialTransferExcelReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var Head = "Material Transfer";
                CreateMaterialTransferExcelReportQuery(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void CreateMaterialTransferExcelReportQuery(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            var cmdText = "";
            cmdText = @"SELECT 
						    REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,IR.Id As GRNId
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,IRD.Id As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,MT.UserName MaterialType
						  ,MGM.UserName AS MaterialGroupMasterName
						  ,IM.MaterialMasterId
						  ,MM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, ART.StandardName ArticleName
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
						,IRD.ShortageQty
						,IRD.ShortageRatePercent
						,IRD.ShortageValue
						,IRD.RejectionQty
						,IRD.RejectRatePercent
						,IRD.RejectValue
						,IRD.RejectClamPercent
						,IRD.ApprovedQty
						,IR.IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
						,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
						,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                         ,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,Case When IR.IsNonCreditable = 1 
							then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						when IR.IsNonCreditable = 0
							then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						end TotalMaterialTranAmount
                       ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approval' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,IGL.UserName AS GL
						,IA.UserName Activity
						,B.UserName AS Budget
                        ,IGL1.UserName AS CGL
						,IA1.UserName AS CActivity
						,B1.UserName AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IR.POId
						,IRD.PODetailsId AS PORowId
						,aa.FromStorageName FromStorageLocation
                        ,MS.UserName as ToStorageLocation
						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                        ,isnull(p.TINNO,'') GSTINNo
						,IR.PlantId FromPlant,IR.ToPlantId
						,FP.UserName FromPlantName
						,TP.UserName ToPlantName
					from TRN.InventoryMaterial AS IM
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					left JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId  
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
                    LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
	                LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
					LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
					left join(select IRD.Id,IRD.MaterialStorageId,MS.UserName FromStorageName 
					          from trn.InventoryReceiveDetail IRD
							  left join trn.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
					          Left JOIN hkp.MaterialStorage MS ON MS.Id=IRD.MaterialStorageId
							  ) aa ON aa.Id=IRD.TransferedFromGrnId

                    left join org.plant FP on FP.Id=IR.PlantId
					left join org.plant TP on Tp.Id=IR.ToPlantId
                    where GRNType='MaterialTransfer' AND IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' ORDER BY IR.GRNDate ASC";

            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");


            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {


                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.UsedRange.CellStyle.Font.Bold = true;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();


            }

            var _rows = 6;
            sheet1[_rows, 6].Text = "Report Ref No: ";
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1.Range[_rows, 3, _rows, 6].CellStyle.Font.Bold = false;

            var _row = 7;
            var _rowL = _row;
            var row = _row + 1;
            var sheet1headreColIndex = 1;
            _rowL += 1;

            int ROW = 8;
            int COL = 1;


            sheet1.Range[ROW, COL].Text = "Sl No.";
            sheet1.Range[ROW, COL].ColumnWidth = 6;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colSLNo = COL;
            COL++;


            sheet1.Range[ROW, COL].Text = "GRN Row No";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colGRNRowNo = COL;
            COL++;



            sheet1.Range[ROW, COL].Text = "PR Date";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colPRDate = COL;

            COL++;

            sheet1.Range[ROW, COL].Text = "Party";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colParty = COL;
            COL++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref No");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Doc Ref No";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colDOcRef = COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref Date");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Doc Ref Date";
            sheet1.Range[ROW, COL].ColumnWidth = 12;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colDocRefDate= COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Grn Doc Date Difference");
            //sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Material Type";
            sheet1.Range[ROW, COL].ColumnWidth = 30;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colMaterialType= COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Material Group";
            sheet1.Range[ROW, COL].ColumnWidth = 30;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colMaterialGroup= COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Material";
            sheet1.Range[ROW, COL].ColumnWidth = 30;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colMaterial = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Article";
            sheet1.Range[ROW, COL].ColumnWidth = 30;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colArticle = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "SKU1";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colSKU1 = COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "SKU2";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colSKU2 = COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "SKU3";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colSKU3= COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Qty");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Transaction Qty";
            sheet1.Range[ROW, COL].ColumnWidth = 15;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colTransactionQty = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "UoM";
            sheet1.Range[ROW, COL].ColumnWidth = 8;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colUoM = COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Rate");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Transaction Rate";
            sheet1.Range[ROW, COL].ColumnWidth = 15;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colTransactionRate = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Amount");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Transaction Amount";
            sheet1.Range[ROW, COL].ColumnWidth = 15;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colTransactionAmount = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Tran Amount");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Total Material Tran Amount";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colTotalMaterialTranAmount = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Credtible Status");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Credtible Status";
            sheet1.Range[ROW, COL].ColumnWidth = 15;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colCredtibleStatus = COL;

            COL++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Tax Amount");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Tax Amount";
            sheet1.Range[ROW, COL].ColumnWidth = 15;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colTaxAmount = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Charge");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Service Charge";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colServiceCharge = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Tax");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Service Tax";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colServiceTax = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Books Currency Amount");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Total Material Books Currency Amount";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colTotalMaterialBooksCurrencyAmount = COL;


            COL++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Trn Currency Base Rate");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Trn Currency Base Rate";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colTrnCurrencyBaseRate = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Books Currency Base Rate");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Books Currency Base Rate";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colBooksCurrencyBaseRate = COL;

            COL++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "MMIsAsset");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "MMIsAsset";
            sheet1.Range[ROW, COL].ColumnWidth = 15;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colMMIsAsset = COL;

            COL++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Storage Location");
            //sheet1headreColIndex++;
            sheet1.Range[ROW, COL].Text = "From Plant";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colFromPlant = COL;
            COL++;
            sheet1.Range[ROW, COL].Text = "From Storage Location";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colFromStorageLocation = COL;
            COL++;
            sheet1.Range[ROW, COL].Text = "To Plant";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colToPlant = COL;
            

            COL++;
            sheet1.Range[ROW, COL].Text = "To Storage Location";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colToStorageLocation = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PR Row ID");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "PR Row ID";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colPRRowID = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Prepared By";
            sheet1.Range[ROW, COL].ColumnWidth = 10;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colPreparedBy = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Status";
            sheet1.Range[ROW, COL].ColumnWidth = 15;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colStatus = COL;

            COL++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Checking Name");
            //sheet1headreColIndex++;

            sheet1.Range[ROW, COL].Text = "Checking Name";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colCheckingName = COL;

            COL++;


            sheet1.Range[ROW, COL].Text = "Approving Name";
            sheet1.Range[ROW, COL].ColumnWidth = 20;
            sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, COL].CellStyle.Font.Bold = true;
            int colApprovingName = COL;



            int endCol = COL;

            //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            //sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
       
            sheet1.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet1.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[ROW, 1, ROW, COL].CellStyle.Font.Size = 10;
            sheet1.Range[ROW, 1, ROW, COL].RowHeight = 22;
            ROW++;
            int StartRow = ROW;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                report.SetText(ref sheet1, ROW, colSLNo, (n+1).ToString());
                report.SetText(ref sheet1, ROW, colGRNRowNo, inventoryMaterialList.Rows[n]["GRNId"].ToString());
                report.SetText(ref sheet1, ROW, colPRDate, inventoryMaterialList.Rows[n]["GRNEntryDate"].ToString());
                report.SetText(ref sheet1, ROW, colParty, inventoryMaterialList.Rows[n]["PartyName"].ToString());
                report.SetText(ref sheet1, ROW, colFromPlant, inventoryMaterialList.Rows[n]["FromPlantName"].ToString());
                report.SetText(ref sheet1, ROW, colToPlant, inventoryMaterialList.Rows[n]["ToPlantName"].ToString());
                report.SetText(ref sheet1, ROW, colDOcRef, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
                report.SetText(ref sheet1, ROW, colDocRefDate, inventoryMaterialList.Rows[n]["DocDate"].ToString());
                report.SetText(ref sheet1, ROW, colMaterialType, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, ROW, colMaterialGroup, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, ROW, colMaterial, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, ROW, colArticle, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, ROW, colSKU1, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, ROW, colSKU2, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, ROW, colSKU3, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, ROW, colTransactionQty, OTSBD.clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
                report.SetText(ref sheet1, ROW, colUoM, inventoryMaterialList.Rows[n]["UOM"].ToString());
                report.SetText(ref sheet1, ROW, colTransactionRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranRate"].ToString()));
                report.SetText(ref sheet1, ROW, colTransactionAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, ROW, colTotalMaterialTranAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, ROW, colCredtibleStatus, inventoryMaterialList.Rows[n]["CredtibleStatus"].ToString());
                report.SetText(ref sheet1, ROW, colTaxAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TaxAmount"].ToString()));
                report.SetText(ref sheet1, ROW, colServiceCharge, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceCharge"].ToString()));
                report.SetText(ref sheet1, ROW, colServiceTax, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceTax"].ToString()));
                report.SetText(ref sheet1, ROW, colTotalMaterialBooksCurrencyAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialBaseAmount"].ToString()));
                report.SetText(ref sheet1, ROW, colTrnCurrencyBaseRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TrnCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, ROW, colBooksCurrencyBaseRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BooksCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, ROW, colMMIsAsset, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                report.SetText(ref sheet1, ROW, colFromStorageLocation, inventoryMaterialList.Rows[n]["FromStorageLocation"].ToString());
                report.SetText(ref sheet1, ROW, colFromStorageLocation, inventoryMaterialList.Rows[n]["ToStorageLocation"].ToString());
                report.SetText(ref sheet1, ROW, colPRRowID, inventoryMaterialList.Rows[n]["GrnDetailId"].ToString());
                //report.SetText(ref sheet1ROWwL, 44, inventoryMaterialList.Rows[n]["GRNId"].ToString());
                report.SetText(ref sheet1, ROW, colPreparedBy, inventoryMaterialList.Rows[n]["AddedBy"].ToString());
                report.SetText(ref sheet1, ROW, colStatus, inventoryMaterialList.Rows[n]["GRNCheckStatus"].ToString());
                report.SetText(ref sheet1, ROW, colCheckingName, inventoryMaterialList.Rows[n]["CheckedBY"].ToString());
                report.SetText(ref sheet1, ROW, colApprovingName, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString());

               
                ROW++;

            }
            sheet1.Range[StartRow, 1, ROW-1, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[StartRow, 1, ROW-1, endCol].BorderInside(ExcelLineStyle.Hair);
            //sheet1.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8;
            //sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet1.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
            //sheet1.Name = sheet1Name;
            //sheet1.UsedRange.WrapText = true;
            ////sheet1.UsedRange.CellStyle.Font.Size = 8;
            //sheet1.IsGridLinesVisible = false;
            //report.PlantHeader(ref sheet1, endCol, sheet1Name, plantId);
            //report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);





            sheet1.IsGridLinesVisible = false;

            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet1.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet1["A" + StartRow.ToString()].FreezePanes();


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet1, endCol, sheet1Name, plantId);
            reportUtility.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet1.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

          


        }



    }
}
