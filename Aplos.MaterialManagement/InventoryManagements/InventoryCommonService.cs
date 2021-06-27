using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;

using Library.Service.Accounts;

using Library.Service.Enums;
using Library.Service.Helpers;

using Library.Service.Organizations;


using OTSBD;

using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
            

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PR No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Row No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

           

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PR Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

           

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref No");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Grn Doc Date Difference");
            //sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Tran Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Material Tran Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Credtible Status");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Credtible Status";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Tax Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Tax Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Charge");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Charge";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Tax");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Tax";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Books Currency Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Material Books Currency Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Trn Currency Base Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Currency Base Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Books Currency Base Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Currency Base Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "MMIsAsset");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "MMIsAsset";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Storage Location");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Storage Location";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PR Row ID");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PR Row ID";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Prepared By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Status";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Checking Name");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Checking Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Approving Name");

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approving Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["GRNId"].ToString());
               // report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n][""].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["GRNEntryDate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["PartyName"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["DocDate"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 13, OTSBD.clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["UOM"].ToString());
                report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["CredtibleStatus"].ToString());
                report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TaxAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceCharge"].ToString()));
                report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceTax"].ToString()));
                report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialBaseAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TrnCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BooksCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                report.SetText(ref sheet1, _rowL, 26, inventoryMaterialList.Rows[n]["StorageLocation"].ToString());
                report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["GrnDetailId"].ToString());
                //report.SetText(ref sheet1, _rowL, 44, inventoryMaterialList.Rows[n]["GRNId"].ToString());
                report.SetText(ref sheet1, _rowL, 28, inventoryMaterialList.Rows[n]["AddedBy"].ToString());
                report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["GRNCheckStatus"].ToString());
                report.SetText(ref sheet1, _rowL, 30, inventoryMaterialList.Rows[n]["CheckedBY"].ToString());
                report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString());

            }



            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);


            //_rowL++;


            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);



            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }

		

	}
}
