using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Logs;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.JobWork
{
    public class JobWorkCommon
    {
        private readonly SqlRepository _sqlRepository = new SqlRepository();
        string TableName = "JWTransformationPurchaseOrder";

        public JobWorkCommon()
        {
            _sqlRepository = new SqlRepository();
        }
        public enum JobWorkType
        {
            [Description("ValueAdded")]
            ValueAdded,
            [Description("Transformation")]
            Transformation
        }

        public List<Dictionary<string, object>> EmployeeListAll()
        {
            try
            {
                string strSql = "";
                strSql = @"SELECT Emp.SystemId,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE  EMP.EmployeeStatus='Active' ORDER BY ISNULL(EMP.EmployeeCodePreFix,''),ISNULL(EMP.EmployeeCodeNumeric,0)";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> GetPOTypeList(string plantId, string POTypeStatus)
        {
            if (POTypeStatus == "")
            {
                POTypeStatus = "Pending";
            }
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var Sql = "";
                if (POTypeStatus == "Pending")
                {
                    Sql = @"	
						select * from(
							SELECT  ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
									, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
									--,IR.PODate
									, IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
									, CP.UserName AS PartyAccountGroupName
									,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									--, IR.GateEntryNo
									--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
									, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.FixedAssetOrInventory, IR.PODepended
									--, IR.AlongwithInvoice
									--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
									, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount
									,ROUND(IRD.BaseAmount, 2) BaseAmount
									, IR.ToCurrencyRate
									, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
									,isnull(pgl.CtnId,0) CtnId
									,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									,ISNULL(Cn.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							        ,ISNULL(MLC.LCRef,'') LCRef
									--,Par1.UserName Customer
									,IR.CheckedByStatus AS CheckedByStatus
									,IR.AuthorizedByStatus AS AuthorizedByStatus
                                  ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
									,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
									,Par.UserName CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
						,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
						FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
									ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                         --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
						LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
						LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
						LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
						LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
						LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
						LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId

						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						WHERE  IR.PlantId='"+ plantId + @"' 
                        --AND IR.POType='OSTransformationPO'  --IR.AddedBy='Shashank' And
                        --AND IR.CheckedBy IS NOT NULL 
						AND IR.CheckedByStatus='Pending' 
						AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
									, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
									--,IR.PODate
									, IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
									, CP.UserName AS PartyAccountGroupName
									,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									--, IR.GateEntryNo
									--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
									, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.FixedAssetOrInventory, IR.PODepended
									--, IR.AlongwithInvoice
									--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
									, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount
									,ROUND(IRD.BaseAmount, 2) BaseAmount
									, IR.ToCurrencyRate
									, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
									,isnull(pgl.CtnId,0) CtnId
									,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									,ISNULL(cn.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							        ,ISNULL(MLC.LCRef,'') LCRef
									--,Par1.UserName Customer
									,IR.CheckedByStatus AS CheckedByStatus
									,IR.AuthorizedByStatus AS AuthorizedByStatus
                             ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
									,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
									,Par.UserName CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
						,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
						FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
									ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                        --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
						LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
						LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
						LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
						LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
						LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
						LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
						AND IR.CheckedByStatus IS NULL 
						AND IR.AuthorizedByStatus IS NULL						
						 And IR.PlantId='"+ plantId + @"' 
                       --AND IR.POType='OSTransformationPO'--AND IR.AddedBy='Shashank'

                        AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
									, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
									--,IR.PODate
									, IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
									, CP.UserName AS PartyAccountGroupName
									,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									--, IR.GateEntryNo
									--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
									, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.FixedAssetOrInventory, IR.PODepended
									--, IR.AlongwithInvoice
									--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
									, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount
									,ROUND(IRD.BaseAmount, 2) BaseAmount
									, IR.ToCurrencyRate
									, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
									,isnull(pgl.CtnId,0) CtnId
									,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									,ISNULL(cn.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							        ,ISNULL(MLC.LCRef,'') LCRef
									--,Par1.UserName Customer
									,IR.CheckedByStatus AS CheckedByStatus
									,IR.AuthorizedByStatus AS AuthorizedByStatus
                                   ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
									,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
									,Par.UserName CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
						,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
						FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
									ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                         --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
                        
						--LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
						LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
						LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
						LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
						LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
						LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
						LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='"+ plantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						) x
						Order by PODate DESC";
                }
                else if (POTypeStatus == "CheckedHoldRej")
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='"+ plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                ,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount
                                    , IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,IR.AddedBy
                                 --   ,PLC.LCANo PurchaseLC
							--		,ISNULL(Ctc.ContractNo,'') ContractNo
								--	, ISNULL(MLC.Id,'') MasterLCNo
					--		,ISNULL(MLC.LCRef,'') LCRef
						--			,Par.UserName Customer
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                           ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                        ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
                        
					--	LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
		                            JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
                        WHERE  IR.PlantId='"+ plantId + @"' AND IR.CheckedBy IS NOT NULL AND IR.AuthorizedBy IS NOT NULL AND IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' 
                        --AND IR.POType='OSTransformationPO' 
                        AND IR.PlantId='"+ plantId + @"'   AND isnull(IR.IsClosed,0)=0 Order by IR.PODate DESC";//IR.AddedBy='" + identity.Name + "' And

                }
                else if (POTypeStatus == "Checked")
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                ,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount
                                    , IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									--,ISNULL(Ctc.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							,ISNULL(MLC.LCRef,'') LCRef
									--,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                         ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId

                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
		                            JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
                         WHERE IR.PlantId='" + plantId + @"' 
                         AND IR.CheckedBy IS NOT NULL 
                         AND IR.AuthorizedBy IS NOT NULL  
                         AND IR.CheckedByStatus='Checked' 
                         AND IR.AuthorizedByStatus='For Approval'  
                         --AND IR.POType='OSTransformationPO'  		
                         AND ISNULL(IR.IsClosed,0)=0 Order by IR.PODate DESC";


                }
                return _sqlRepository.GetDataCollection(Sql);
            }

            catch (Exception ex)
            {
                throw ex;

            }
        }

        public IEnumerable<object> GetListForHoldRejectApproved(string plantId, string ApproveRejectHold)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var Sql = "";
                if (ApproveRejectHold == "Approved")
                {
                    Sql = @"select * from
											(
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                ,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount
                                    , IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									--,ISNULL(Ctc.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							,ISNULL(MLC.LCRef,'') LCRef
									--,Par.UserName Customer
                               --     ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                         ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId

                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
		                            JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE  
                                            --IR.POType='OSTransformationPO' AND 
                                            IR.PlantId='"+ plantId + @"' 
											AND IR.Id in(Select distinct POId from trn.InventoryReceive where POId is not null)--and RequisitionId='110232'
											AND IR.CheckedByStatus IS NULL
											AND IR.AuthorizedByStatus IS NULL
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASC

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                ,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount
                                    , IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									--,ISNULL(Ctc.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							,ISNULL(MLC.LCRef,'') LCRef
									--,Par.UserName Customer
                                --    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                         ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId

                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
		                            JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE IR.PlantId='"+ plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus='Approved'
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASCr

                                             UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                ,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount
                                    , IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									--,ISNULL(Ctc.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							,ISNULL(MLC.LCRef,'') LCRef
									--,Par.UserName Customer
                                --    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                         ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId

                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
		                            JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE IR.PlantId='"+ plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus Is null
											AND isnull(IR.IsClosed,0)=0 

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                ,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount
                                    , IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									--,ISNULL(Ctc.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							,ISNULL(MLC.LCRef,'') LCRef
									--,Par.UserName Customer
                               --     ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                         ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId

                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
		                            JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE IR.PlantId='"+ plantId + @"' 
											AND IR.CheckedByStatus='Checked'
											AND IR.AuthorizedByStatus='Approved'
											AND isnull(IR.IsClosed,0)=0 
											)x Order by PODate ASC";
                }
                else
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                          SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId,    IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                ,    IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount
                                    , IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,IR.AddedBy
                                    --,PLC.LCANo PurchaseLC
									--,ISNULL(Ctc.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							,ISNULL(MLC.LCRef,'') LCRef
									--,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                         ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),IR.[Time],108)[TConTime],FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM JWTransformationPurchaseOrder AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          --LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						--LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						--LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId

                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.JobWorkTransformationContractChild AS A
		                            JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
                        WHERE IR.PlantId='"+ plantId + @"'  AND IR.CheckedBy IS NOT NULL AND IR.CheckedByStatus='Checked' 
						AND IR.AuthorizedBy IS NOT NULL  AND IR.AuthorizedByStatus<>'Approved'  AND IR.AuthorizedByStatus <> 'For Approval'   
						AND isnull(IR.IsClosed,0)=0 Order by IR.PODate ASC ";

                }
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetBOQItems(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataTable dtJWPODetail = new DataTable();
            if (!String.IsNullOrEmpty(JWPODId))
            {
                string strSql = "";

                strSql = @"SELECT * FROM JWTransformationPurchaseOrderDetail WHERE JWTransformationPurchaseOrderId = '" + JWPOId + @"'";

                dtJWPODetail = _sqlRepository.GetDataTable(strSql);

            }

            if (IsOwnVendor == "OwnVendor")
            {
                try
                {



                    var sql = "";
                    sql = @"SELECT NULL AS uoMList, b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
					    --,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						--,uom1.UserName AlternateUOM
						,b.RequiredQty
						--,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
						,uom.UserName BOQUOM
						--,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,b.POUoMId FromPoUomId
					    ,b.POUoMId
						--,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,b.RequiredQtyPO 
						,b.RequiredQtyPO RequiredQtyPOOrginal
						,TransactionUoMId=CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'')+'-'+ISNULL(moi.BuyerReferenceNo,'')
						,mm.BaseUOMId
						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=POD.JWTransformationPurchaseOrderId
									WHERE POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=POD.JWTransformationPurchaseOrderId
									WHERE POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
                        LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						--LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE moi.ContractId='" + ContractId + @"' AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null)
						--AND  b.id in(select ParentId from BOQ where ISNULL(ParentId,'')<>'' 
                        --and ProcessId IN (Select ProcessId from JWActivity where Id IN (" + jwActivityId + @"))) --and isChild=0

                            AND  b.id in(select ParentId from BOQ where ISNULL(ParentId,'')<>'' )

						ORDER BY b.Sequence, b.SalesOrderId";//b.MaterialMasterId,


                    var Data = _sqlRepository.GetDataCollection(sql);

                    for (int i = 0; i < Data.Count; i++)
                    {

                        Data[i].Add("CombinationKey", MakeKey(Data[i]));

                    }


                    for (int i = 0; i < dtJWPODetail.Rows.Count; i++)
                    {
                        string jwPodetailCom = "";//dtJWPODetail

                        jwPodetailCom = MakeKey(dtJWPODetail.Rows[i]);

                        var x = Data.Where(xx => xx["CombinationKey"].ToString() == jwPodetailCom).ToList();
                        foreach (var item in x)
                        {
                            Data.Remove(item);
                        }
                    }


                    StringCollection strCol = new StringCollection();
                    string MaterialMasterList = "''";
                    for (int i = 0; i < Data.Count; i++)
                    {
                        if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                            continue;
                        strCol.Add(Data[i]["MaterialMasterId"].ToString());
                        MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

                    }

                    var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

                    for (int i = 0; i < Data.Count; i++)
                    {
                        var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                        Data[i]["uoMList"] = temp;
                    }

                    return Data;
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }
            else if (IsOwnVendor == "OtherVendor")
            {
                try
                {
                    var sql = "";
                    sql = @"SELECT  b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						,uom1.UserName AlternateUOM
						,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN b.RequiredQty ELSE AUOM.BaseUOMFactor END
						,uom.UserName BOQUOM
						,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '/' + ISNULL(mo.BuyerReferenceNo,'') +'/'+ ISNULL(moi.OwnReferenceNo,'')+'/'+ISNULL(moi.BuyerReferenceNo,'')

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
						LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE moi.ContractId='" + ContractId + @"' AND b.VendorId<>'" + VendorId + @"' 
						AND b.isParent=0 --and isChild=0
						ORDER BY b.MaterialMasterId,b.SalesOrderId";
                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }
            else
            {
                try
                {
                    var sql = "";
                    sql = @"SELECT  b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer
						,b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						,uom1.UserName AlternateUOM
						,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
						,uom.UserName BOQUOM
						,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '/' + ISNULL(mo.BuyerReferenceNo,'') +'/'+ ISNULL(moi.OwnReferenceNo,'')+'/'+ISNULL(moi.BuyerReferenceNo,'')

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
						LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE moi.ContractId='" + ContractId + @"' AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null) AND b.isParent=1 
						ORDER BY b.MaterialMasterId,b.SalesOrderId";
                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }

        }


        public IEnumerable<object> GetBOQItemsForUpdate(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataTable dtJWPODetail = new DataTable();
            if (!String.IsNullOrEmpty(JWPODId))
            {
                string strSql = "";

                strSql = @"SELECT * FROM JWTransformationPurchaseOrderDetail WHERE JWTransformationPurchaseOrderId = '" + JWPOId + @"' AND Id = '" + JWPODId + @"'";

                dtJWPODetail = _sqlRepository.GetDataTable(strSql);

            }

            if (IsOwnVendor == "OwnVendor")
            {
                try
                {
                    var sql = "";
                    sql = @"SELECT NULL AS uoMList, b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty--,C.Id
						,CONVERT(BIT,CASE WHEN ISNULL(JWPOBOQMAP.Id,'')<>'' THEN 1 ELSE 0 END) CheckedStatus ,NULL TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal,JWPOBOQMAP.TransactionQty
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
					    --,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						--,uom1.UserName AlternateUOM
						,b.RequiredQty
						--,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
						,uom.UserName BOQUOM
						--,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,b.POUoMId FromPoUomId
					    ,b.POUoMId
						--,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,b.RequiredQtyPO 
						,b.RequiredQtyPO RequiredQtyPOOrginal
						,TransactionUoMId=CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'')+'-'+ISNULL(moi.BuyerReferenceNo,'')
						,mm.BaseUOMId
						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
											FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN JWTransformationPurchaseOrderDetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=JWPPOD.JWTransformationPurchaseOrderId
									where POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
											FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN JWTransformationPurchaseOrderDetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=JWPPOD.JWTransformationPurchaseOrderId
									where POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
                        LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
                        LEFT JOIN JWPOBOQMAP JWPOBOQMAP ON JWPOBOQMAP.BOQDetailId=b.Id AND JWPOBOQMAP.JWPODetailId IN (select Id from JWTransformationPurchaseOrderDetail where JWTransformationPurchaseOrderId='" + JWPOId + @"')
						--LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE moi.ContractId='" + ContractId + @"' AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null)
						AND  b.id in(select ParentId from BOQ where ISNULL(ParentId,'')<>'' and ProcessId IN (Select ProcessId from JWActivity where Id IN (" + jwActivityId + @"))) --and isChild=0
						ORDER BY JWPOBOQMAP.BOQDetailId DESC, b.Sequence, b.SalesOrderId";//b.MaterialMasterId,


                    var Data = _sqlRepository.GetDataCollection(sql);

                    for (int i = 0; i < Data.Count; i++)
                    {
                        Data[i].Add("Id", JWPODId);
                        Data[i].Add("CombinationKey", MakeKey(Data[i]));

                    }


                    for (int i = 0; i < dtJWPODetail.Rows.Count; i++)
                    {
                        string jwPodetailCom = "";//dtJWPODetail

                        jwPodetailCom = MakeKey(dtJWPODetail.Rows[i]);

                        var x = Data.Where(xx => xx["CombinationKey"].ToString() != jwPodetailCom).ToList();
                        foreach (var item in x)
                        {
                            Data.Remove(item);
                        }
                    }


                    StringCollection strCol = new StringCollection();
                    string MaterialMasterList = "''";
                    for (int i = 0; i < Data.Count; i++)
                    {
                        if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                            continue;
                        strCol.Add(Data[i]["MaterialMasterId"].ToString());
                        MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";


                    }

                    var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

                    for (int i = 0; i < Data.Count; i++)
                    {
                        var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                        Data[i]["uoMList"] = temp;
                    }

                    return Data;
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }
            else if (IsOwnVendor == "OtherVendor")
            {
                try
                {
                    var sql = "";
                    sql = @"SELECT  b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,C.Id
						,NULL CheckedStatus   ,NULL TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						,uom1.UserName AlternateUOM
						,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN b.RequiredQty ELSE AUOM.BaseUOMFactor END
						,uom.UserName BOQUOM
						,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '/' + ISNULL(mo.BuyerReferenceNo,'') +'/'+ ISNULL(moi.OwnReferenceNo,'')+'/'+ISNULL(moi.BuyerReferenceNo,'')

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
						LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE moi.ContractId='" + ContractId + @"' AND b.VendorId<>'" + VendorId + @"' 
						AND b.isParent=0 --and isChild=0
						ORDER BY b.MaterialMasterId,b.SalesOrderId";
                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }
            else
            {
                try
                {
                    var sql = "";
                    sql = @"SELECT  b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer
						,b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty,JWPOBOQMAP.TransactionQty
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						,uom1.UserName AlternateUOM
						,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
						,uom.UserName BOQUOM
						,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '/' + ISNULL(mo.BuyerReferenceNo,'') +'/'+ ISNULL(moi.OwnReferenceNo,'')+'/'+ISNULL(moi.BuyerReferenceNo,'')

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=POD.JWTransformationPurchaseOrderId
									WHERE POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=POD.JWTransformationPurchaseOrderId
									WHERE POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
						LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE moi.ContractId='" + ContractId + @"' AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null) AND b.isParent=1 
						ORDER BY b.MaterialMasterId,b.SalesOrderId";
                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }

        }



        //string inveReveiveId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId
        public IEnumerable<object> GetBOQItemsListForUpdate(string JWPOId, string JWPODId)
        {
            try
            {
                var _sql = @"SELECT Distinct b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END  
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,b.RequiredQty,uom.UserName AS UOM,C.Id
						,b.UoMId TransactionUoMId,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
                        ,Isnull(POMAP.TransactionQty,0) PORaisedQry,POMAP.PODetailId InventoryReceiveDetailId,Isnull(POMAP.TransactionQty,0) TransactionQty
						,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'') +'-'+ISNULL(moi.BuyerReferenceNo,'')
						,POMAP.TransactionRate,POMAP.DeliveryDate
						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId
						LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
						--LEFT JOIN(Select  PODetailId,BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId,PODetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=POD.JWTransformationPurchaseOrderId
									WHERE POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN JWTransformationPurchaseOrder POM ON POM.Id=POD.JWTransformationPurchaseOrderId
									WHERE POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
						where POMAP.PODetailId='" + JWPODId + @"'
						ORDER BY b.Sequence, b.SalesOrderId";
                //WHERE IM.MaterialMasterId='" + MaterialMasterId + "' and ArticleId='" + ArticleId + "' and IM.FirstCharacteristicsValueId='" + FirstCharacteristicsValueId + "' And IM.PORcvQty=0";
                return _sqlRepository.GetDataCollection(_sql);
                //string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        private void SaveJWTransformationPurchaseOrderChildMaterial(List<Dictionary<string, object>> data, string JWPODetailId, string JWActivityId, Library.General.Conversions.UOMConversion Conversion, out DataSet dsJwChildMaterial)
        {
            dsJwChildMaterial = new DataSet();
            try
            {
                DataSet dsBOQChild;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string parentBOQId = "' '";
                for (int si = 0; si < data.Count; si++)
                {
                    parentBOQId += ",'" + data[si]["BOQId"].ToString() + "'";
                }

                con.OpenDataSetThroughAdapter("SELECT * FROM BOQ  b WHERE b.Id In  (" + parentBOQId + @")", out DataSet dsBOQParentItems, false, "1");
                string sqlboqChild = @"SELECT * FROM BOQ  b WHERE b.ParentId In  (" + parentBOQId + @") AND                         
                             b.ProcessId IN (Select ProcessId from JWActivity where Id IN (" + JWActivityId + @"))";
                con.OpenDataSetThroughAdapter(sqlboqChild, out dsBOQChild, false, "1");

                con.OpenDataSetThroughAdapter("SELECT * FROM JWTransformationPurchaseOrderChildMaterial WHERE JWPODetailId ='" + JWPODetailId + "'", out dsJwChildMaterial, false, "1");

                for (int i = 0; i < dsJwChildMaterial.Tables[0].Rows.Count; i++)
                {
                    dsBOQChild.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(dsJwChildMaterial.Tables[0].Rows[i]["BOQDetailId"]).ToString() + "'";
                    dsJwChildMaterial.Tables[0].Rows[0].Delete();
                }

                for (int i = 0; i < dsBOQChild.Tables[0].Rows.Count; i++)
                {
                    dsJwChildMaterial.Tables[0].DefaultView.RowFilter = "BOQDetailId='" + bplib.clsWebLib.RetValidLen(dsBOQChild.Tables[0].Rows[i]["Id"]).ToString() + "'";

                    string _Id = "";

                    var PData = data.Where(ee => ee["BOQId"].ToString() == dsBOQChild.Tables[0].Rows[i]["ParentId"].ToString()).ToList();

                    if (dsJwChildMaterial.Tables[0].DefaultView.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("JWTransformationPurchaseOrderChildMaterial", out _Id);

                        DataRow dr = dsJwChildMaterial.Tables[0].NewRow();
                        CopyRow(dsBOQChild.Tables[0].Rows[i], ref dr);
                        dr["Id"] = "JWPCM" + _Id;
                        dr["JWPODetailId"] = JWPODetailId;
                        dr["BOQDetailId"] = dsBOQChild.Tables[0].Rows[i]["Id"];

                        Calculations(data, dsBOQParentItems, dsBOQChild.Tables[0].Rows[i], dr, Conversion);

                        dsJwChildMaterial.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsJwChildMaterial.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        Calculations(data, dsBOQParentItems, dsBOQChild.Tables[0].Rows[i], dr, Conversion);

                        dr.EndEdit();
                    }
                }


            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }

        private void SaveJWTransformationPurchaseOrderInputMaterial(List<Dictionary<string, object>> data, string JWActivityId, Library.General.Conversions.UOMConversion Conversion, out DataSet dsInputMaterialChild)
        {
            dsInputMaterialChild = new DataSet();

            try
            {
                DataSet dsTransInputMaterail = null;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                string strSql = @"SELECT * FROM JWInputMaterial where JWTransformationMasterId = '" + data[0]["JWTransformationMasterId"] + "' ";
                string strSql2 = @"SELECT * FROM JWTransformationPurchaseOrderInputChildMaterial where JWTransformationMasterId = '" + data[0]["JWTransformationMasterId"] + "' and  JWPODetailId = '" + data[0]["Id"] + @"'";

                con.OpenDataSetThroughAdapter(strSql, out dsTransInputMaterail, false, "1");
                con.OpenDataSetThroughAdapter(strSql2, out dsInputMaterialChild, false, "1");


                for (int i = 0; i < dsTransInputMaterail.Tables[0].Rows.Count; i++)
                {
                    dsInputMaterialChild.Tables[0].DefaultView.RowFilter = "JWTransformationMasterId='" + bplib.clsWebLib.RetValidLen(dsTransInputMaterail.Tables[0].Rows[i]["JWTransformationMasterId"]).ToString() + "'";

                    string _Id = "";

                    if (dsInputMaterialChild.Tables[0].DefaultView.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("JWTransformationPurchaseOrderChildMaterial", out _Id);

                        DataRow dr = dsInputMaterialChild.Tables[0].NewRow();
                        //  CopyRow(dsBOQChild.Tables[0].Rows[i], ref dr);
                        dr["Id"] = "JWPCM" + _Id;
                        dr["JWPODetailId"] = data[0]["Id"].ToString();
                        dr["JWInputMaterialId"] = dsTransInputMaterail.Tables[0].Rows[i]["Id"];
                        dr["JWTransformationMasterId"] = dsTransInputMaterail.Tables[0].Rows[i]["JWTransformationMasterId"];

                        CalculationsNonOrderSpecific(data, dsInputMaterialChild, dsTransInputMaterail.Tables[0].Rows[i], dr, Conversion);

                        dsInputMaterialChild.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsInputMaterialChild.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();


                        dr.EndEdit();
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void SaveJWTransformationPurchaseOrderByProductMaterial(List<Dictionary<string, object>> data, string JWActivityId, Library.General.Conversions.UOMConversion Conversion, out DataSet dsJwChildJWByProduct)
        {
            dsJwChildJWByProduct = new DataSet();

            try
            {
                DataSet dsJWTransByProduct = null;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                string strSql = @"select * from JWByProduct where JWTransformationMasterId = '" + data[0]["JWTransformationMasterId"] + "' ";
                string strSql2 = @"select * from JWTransformationPurchaseOrderByProductChildMaterial where JWTransformationMasterId = '" + data[0]["JWTransformationMasterId"] + "' and  JWPODetailId = '" + data[0]["Id"] + @"'";

                con.OpenDataSetThroughAdapter(strSql, out dsJWTransByProduct, false, "1");
                con.OpenDataSetThroughAdapter(strSql2, out dsJwChildJWByProduct, false, "1");


                for (int i = 0; i < dsJWTransByProduct.Tables[0].Rows.Count; i++)
                {
                    dsJwChildJWByProduct.Tables[0].DefaultView.RowFilter = "JWTransformationMasterId='" + bplib.clsWebLib.RetValidLen(dsJWTransByProduct.Tables[0].Rows[i]["JWTransformationMasterId"]).ToString() + "'";

                    string _Id = "";

                    if (dsJwChildJWByProduct.Tables[0].DefaultView.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("JWTransformationPurchaseOrderByProductChildMaterial", out _Id);

                        DataRow dr = dsJwChildJWByProduct.Tables[0].NewRow();

                        dr["Id"] = "JWPCM" + _Id;
                        dr["JWPODetailId"] = data[0]["Id"].ToString();
                        dr["JWByProductId"] = dsJWTransByProduct.Tables[0].Rows[i]["Id"];
                        dr["JWTransformationMasterId"] = dsJWTransByProduct.Tables[0].Rows[i]["JWTransformationMasterId"];

                        CalculationsNonOrderSpecific(data, dsJWTransByProduct, dsJWTransByProduct.Tables[0].Rows[i], dr, Conversion);

                        dsJwChildJWByProduct.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsJwChildJWByProduct.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();


                        dr.EndEdit();
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Calculation For UOM Convertions

        private void Calculations(List<Dictionary<string, object>> ParentData, DataSet dsBOQParentItems, DataRow drChildItem, DataRow POChildRow, Library.General.Conversions.UOMConversion Conversion)
        {
            //add param for uom conversion later
            try
            {


                dsBOQParentItems.Tables[0].DefaultView.RowFilter = "Id='" + drChildItem["ParentId"].ToString() + "'";
                var PData = ParentData.Where(ee => ee["BOQId"].ToString() == drChildItem["ParentId"].ToString()).ToList();


                POChildRow["BaseUoMId"] = drChildItem["BaseUoMId"];

                double ParentTotalRequiredQty = Conversion.Convert(PData[0]["MaterialMasterId"].ToString(),
                    PData[0]["TransactionUoMId"].ToString(), bplib.clsWebLib.RetValidLen(PData[0]["BaseUOMId"]).ToString(), clsStaticInfo.dbl(PData[0]["TransactionQty"].ToString()));

                double ChildRequiredQty = ParentTotalRequiredQty * (clsStaticInfo.dbl(drChildItem["Consumption"].ToString())) * (1 + (clsStaticInfo.dbl(drChildItem["WastagePer"].ToString()) / 100));
                POChildRow["TransactionUoMId"] = drChildItem["UOMId"];//please check all column names accordingly
                POChildRow["TransactionQty"] = ChildRequiredQty;//please check all column names accordingly


                POChildRow["POUoMId"] = drChildItem["POUoMId"];
                POChildRow["POBOQQty"] = Conversion.Convert(drChildItem["MaterialMasterId"].ToString(),
                   drChildItem["UoMId"].ToString(), PData[0]["POUoMId"].ToString(), ChildRequiredQty);//please check all column names accordingly

                POChildRow["BaseUoMId"] = drChildItem["BaseUoMId"];
                POChildRow["BaseQty"] = Conversion.Convert(drChildItem["MaterialMasterId"].ToString(),
                   drChildItem["UoMId"].ToString(), bplib.clsWebLib.RetValidLen(PData[0]["BaseUOMId"]).ToString(), ChildRequiredQty);//please check all column names accordingly
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private void CalculationsNonOrderSpecific(List<Dictionary<string, object>> ParentData, DataSet dsBOQParentItems, DataRow drChildItem, DataRow POChildRow, Library.General.Conversions.UOMConversion Conversion)
        {
            //add param for uom conversion later
            try
            {


                dsBOQParentItems.Tables[0].DefaultView.RowFilter = "JWInputMaterialId='" + drChildItem["Id"].ToString() + "'";
                var PData = ParentData.Where(ee => ee["JWTransformationMasterId"].ToString() == drChildItem["JWTransformationMasterId"].ToString()).ToList();


                POChildRow["BaseUoMId"] = drChildItem["UOMId"];

                double ParentTotalRequiredQty = Conversion.Convert(PData[0]["MaterialMasterId"].ToString(),
                    PData[0]["JWItemUOMId"].ToString(), bplib.clsWebLib.RetValidLen(PData[0]["JWItemUOMId"]).ToString(), clsStaticInfo.dbl(PData[0]["TransactionQty"].ToString()));

                double ChildRequiredQty = ParentTotalRequiredQty * (clsStaticInfo.dbl(drChildItem["GrossConsumption"].ToString()));// * (1 + (clsStaticInfo.dbl(drChildItem["WastagePer"].ToString()) / 100));
                POChildRow["TransactionUoMId"] = drChildItem["UOMId"];//please check all column names accordingly
                POChildRow["TransactionQty"] = ChildRequiredQty;//please check all column names accordingly


                POChildRow["BaseUoMId"] = drChildItem["BaseUoMId"];
                POChildRow["BaseQty"] = Conversion.Convert(drChildItem["MaterialMasterId"].ToString(),
                   drChildItem["UOMId"].ToString(), bplib.clsWebLib.RetValidLen(PData[0]["BaseUOMId"]).ToString(), ChildRequiredQty);//please check all column names accordingly
            }
            catch (Exception ex)
            {


            }

        }

        #endregion

        #region Add Edit Copy Dataset
        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = bplib.clsWebLib.RetValidLen(drSource[drSource.Table.Columns[COL].ColumnName].ToString());

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }



        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item.ToUpper() == "TRANSACTIONAMOUNT")
                    {

                    }
                    if (item.ToUpper() == "ID")
                        continue;
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion

        #region JW PO Creation
        public Dictionary<string, object> XCreate(Dictionary<string, object> data, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, List<string> ActivityList, List<Dictionary<string, object>> ItemList)
        {
            string JWPOId = "";
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM JWTransformationPurchaseOrder WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
            if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
            {
                CheckedByStatusForNoti = "False";
                ApprovedByStatusForNoti = "False";
            }
            try
            {
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("JWPurchaseOrder", out _Id);
                    data["Id"] = "JWP" + _Id;
                    JWPOId = data["Id"].ToString();

                    data["CompanyGroupId"] = identity.CompanyGroupId;
                    data["CompanyId"] = identity.CompanyId;
                    data["PlantId"] = identity.PlantId;

                    if (!string.IsNullOrEmpty(identity.EmployeeId))
                    {
                        if (identity.EmployeeId == bplib.clsWebLib.RetValidLen(data["CheckedBy"]).ToString())
                        {
                            throw new CustomException("Please select another employee for Check by.");
                        }
                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                    {
                        data["AuthorizedBy"] = data["CheckedBy"];
                        data["AuthorizedByStatus"] = "For Approval";
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = "PO";

                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                    {
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = "PO";
                    }
                    else
                    {

                        data["CheckedBy"] = identity.EmployeeId;
                        data["CheckedByStatus"] = "Pending";
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["POType"] = "PO";

                    }

                    data["IsApproved"] = false;
                    data["IsClosed"] = false;
                    data["IsClosed"] = null;

                    data["EmployeeId"] = identity.EmployeeId;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    JWPOId = data["Id"].ToString();

                    data["CompanyGroupId"] = identity.CompanyGroupId;
                    data["CompanyId"] = identity.CompanyId;
                    data["PlantId"] = identity.PlantId;

                    if (!string.IsNullOrEmpty(identity.EmployeeId))
                    {
                        if (identity.EmployeeId == bplib.clsWebLib.RetValidLen(data["CheckedBy"]).ToString())
                        {
                            throw new CustomException("Please select another employee for Check by.");
                        }
                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                    {
                        data["AuthorizedBy"] = data["CheckedBy"];
                        data["AuthorizedByStatus"] = "For Approval";
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = "PO";

                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                    {
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = "PO";
                    }
                    else
                    {

                        data["CheckedBy"] = 1900109;//identity.EmployeeId; //data["CheckedBy"];
                        data["CheckedByStatus"] = "Pending";
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["POType"] = "PO";

                    }

                    data["IsApproved"] = false;
                    data["IsClosed"] = false;
                    data["IsClosed"] = null;

                    data["EmployeeId"] = identity.EmployeeId;

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                #region Activity
                string sql = "";
                string _activityId = "";
                DataSet dsActivity = null;
                sql = "SELECT * FROM JWTransformationPOActivity WHERE JWTransformationPurchaseOrderId='" + JWPOId + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsActivity, false, "1");


                if (ActivityList != null)
                {
                    for (int i = 0; i < dsActivity.Tables[0].Rows.Count; i++)
                    {
                        var containsActivity = ActivityList.Contains(dsActivity.Tables[0].Rows[i]["JWActivityId"].ToString());
                        if (containsActivity)
                            continue;
                        else
                            dsActivity.Tables[0].Rows[i].Delete();
                    }
                    for (int i = 0; i < ActivityList.Count; i++)
                    {
                        dsActivity.Tables[0].DefaultView.RowFilter = "JWActivityId='" + ActivityList[i] + "'";
                        //if (Convert.ToBoolean(ActivityList[i]["isToBeSelect"]))
                        //{

                        if (dsActivity.Tables[0].DefaultView.Count == 0)
                        {

                            if (_activityId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("JWPOActivity", out _activityId);
                                _activityId = "PA" + _activityId;
                            }
                            DataRow dr = dsActivity.Tables[0].NewRow();
                            dr["Id"] = _activityId + "-" + (i + 1).ToString();

                            dr["JWTransformationPurchaseOrderId"] = JWPOId;

                            dr["JWActivityId"] = ActivityList[i];

                            dsActivity.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsActivity.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["JWActivityId"] = bplib.clsWebLib.RetValidLen(ActivityList[i]);


                            dr.EndEdit();

                        }
                        //}
                        //else
                        //{
                        //    if (dsActivity.Tables[0].DefaultView.Count > 0)
                        //        dsActivity.Tables[0].DefaultView[0].Delete();
                        //}
                    }
                }


                #endregion

                #region Item              
                //string sqlItem = "";
                //string _itemId = "";
                //sql = "SELECT * FROM JWTransformationPOActivity WHERE JWTransformationPurchaseOrderId='" + JWPOId + "'";
                //con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter(sql, out dsActivity, false, "1");


                //if (ItemList != null)
                //{
                //    for (int i = 0; i < dsActivity.Tables[0].Rows.Count; i++)
                //    {
                //        var containsActivity = ItemList.FirstOrDefault(x => x.ContainsKey("JWActivityId")).Values.Contains(dsActivity.Tables[0].Rows[i]["JWActivityId"].ToString());
                //        if (containsActivity)
                //            continue;
                //        else
                //            dsActivity.Tables[0].Rows[i].Delete();
                //    }
                //    for (int i = 0; i < ActivityList.Count; i++)
                //    {
                //        dsActivity.Tables[0].DefaultView.RowFilter = "JWActivityId='" + ItemList[i]["JWActivityId"] + "'";
                //        if (Convert.ToBoolean(ItemList[i]["isToBeSelect"]))
                //        {

                //            if (dsActivity.Tables[0].DefaultView.Count == 0)
                //            {

                //                if (_itemId == "")
                //                {
                //                    bplib.clsGenID id = new bplib.clsGenID();
                //                    id.GenID("JWTransformationPOItem", out _activityId);
                //                    _itemId = "PI" + _activityId;
                //                }
                //                DataRow dr = dsActivity.Tables[0].NewRow();
                //                dr["Id"] = _itemId + "-" + (i + 1).ToString();

                //                dr["JWLocationId"] = JWPOId;

                //                dr["JWItemId"] = ItemList[i]["JWItemId"];



                //                dsActivity.Tables[0].Rows.Add(dr);
                //            }
                //            else
                //            {
                //                DataRow dr = dsActivity.Tables[0].DefaultView[0].Row;

                //                dr.BeginEdit();

                //                dr["JWActivityId"] = bplib.clsWebLib.RetValidLen(ItemList[i]["JWActivityId"]);



                //                dr.EndEdit();

                //            }
                //        }
                //        else
                //        {
                //            if (dsActivity.Tables[0].DefaultView.Count > 0)
                //                dsActivity.Tables[0].DefaultView[0].Delete();
                //        }
                //    }
                //}




                #endregion


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsActivity);
                return data;// Json(new { data = data, Message = AplosMessage.Success + " PO no <b>" + data["Id"] + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Create(Dictionary<string, object> data, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            string JWPOId = "";
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM JWTransformationPurchaseOrder WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
            if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
            {
                CheckedByStatusForNoti = "False";
                ApprovedByStatusForNoti = "False";
            }
            try
            {
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("JWPurchaseOrder", out _Id);
                    data["Id"] = "JWP" + _Id;
                    JWPOId = data["Id"].ToString();

                    data["CompanyGroupId"] = identity.CompanyGroupId;
                    data["CompanyId"] = identity.CompanyId;
                    data["PlantId"] = identity.PlantId;

                    if (!string.IsNullOrEmpty(identity.EmployeeId))
                    {
                        if (identity.EmployeeId == bplib.clsWebLib.RetValidLen(data["CheckedBy"]).ToString())
                        {
                            throw new CustomException("Please select another employee for Check by.");
                        }
                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                    {
                        data["AuthorizedBy"] = data["CheckedBy"];
                        data["AuthorizedByStatus"] = "For Approval";
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];

                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                    {
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];
                    }
                    else
                    {

                        data["CheckedBy"] = data["CheckedBy"];//identity.EmployeeId;
                        data["CheckedByStatus"] = "Pending";
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["POType"] = data["POType"];

                    }

                    data["IsApproved"] = false;
                    data["IsClosed"] = false;
                    data["IsClosed"] = null;

                    data["EmployeeId"] = identity.EmployeeId;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    JWPOId = data["Id"].ToString();

                    data["CompanyGroupId"] = identity.CompanyGroupId;
                    data["CompanyId"] = identity.CompanyId;
                    data["PlantId"] = identity.PlantId;

                    if (!string.IsNullOrEmpty(identity.EmployeeId))
                    {
                        if (identity.EmployeeId == bplib.clsWebLib.RetValidLen(data["CheckedBy"]).ToString())
                        {
                            throw new CustomException("Please select another employee for Check by.");
                        }
                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                    {
                        data["AuthorizedBy"] = data["CheckedBy"];
                        data["AuthorizedByStatus"] = "For Approval";
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];

                    }
                    else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                    {
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];
                    }
                    else
                    {

                        data["CheckedBy"] = data["CheckedBy"]; //identity.EmployeeId; //data["CheckedBy"];
                        data["CheckedByStatus"] = "Pending";
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["POType"] = data["POType"];

                    }

                    data["IsApproved"] = false;
                    data["IsClosed"] = false;
                    data["IsClosed"] = null;

                    data["EmployeeId"] = identity.EmployeeId;

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                      

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return data;// Json(new { data = data, Message = AplosMessage.Success + " PO no <b>" + data["Id"] + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region delete PO
        public void Delete(string id)
        {
            // string sql = @"select * from [HKP].[HourlyLeaveReason] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con2 = new ConnectionManager.DAL.ConManager("1");

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con2.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationContractChild where JobWorkTransformationContractMasterId='" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Output Data");
                    }
                }

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                

                //   con.executeQuery("delete from JWTransformationPurchaseOrderDetail where JWTransformationPurchaseOrderId='" + id + "'");
                con.executeQuery("delete from JWTransformationPurchaseOrder where Id='" + id + "'");

                con.CommitTransaction();

                //return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion

        #region delete PODetail
        public void DeleteDetail(string id)
        {
            // string sql = @"select * from [HKP].[HourlyLeaveReason] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con2 = new ConnectionManager.DAL.ConManager("1");

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                if (!string.IsNullOrEmpty(id))
                {
                    con2.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationContractChild3 where JobWorkTransformationContractChildMasterId='" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Input Data");
                    }
                }

                

                //con.executeQuery("DELETE JWTransformationPurchaseOrderTax where JWTransformationPurchaseOrderDetailId = '" + id + "'");
                //con.executeQuery("DELETE JWPOBOQMAP where JWPODetailId = '" + id + "'");
                //con.executeQuery("DELETE JWTransformationPurchaseOrderChildMaterial where JWPODetailId = '" + id + "'");
                //con.executeQuery("DELETE JWTransformationPurchaseOrderTax where JWTransformationPurchaseOrderDetailId = '" + id + "'");
                //con.executeQuery("DELETE JWTransformationPurchaseOrderInputChildMaterial where JWPODetailId = '" + id + "'");
                //con.executeQuery("DELETE JWTransformationPurchaseOrderByProductChildMaterial where JWPODetailId = '" + id + "'");

                //con.executeQuery("DELETE JWTransformationPurchaseOrderTax where JWTransformationPurchaseOrderDetailId = '" + id + "'");
                //con.executeQuery("DELETE from  JWTransformationPurchaseOrderDetail where id='" + id + "'");

                con.executeQuery("delete from dbo.JWTransformationPurchaseOrderTax where JWTransformationPurchaseOrderDetailId='" + id + @"' ");
                con.executeQuery("delete from dbo.JobWorkTransformationContractChild where Id='" + id + "' ");

                con.CommitTransaction();

                //return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion


        public Dictionary<string, object> ServiceChargeCreate(Dictionary<string, object> data, List<Dictionary<string, object>> TaxList)
        {

            string JWPODSId = "";
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM JWTransformationPurchaseOrderService WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

            try
            {
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("JWTransformationPurchaseOrderService", out _Id);
                    data["Id"] = "JPOS" + _Id;
                    JWPODSId = data["Id"].ToString();
                    //dr["JWTransformationPurchaseOrderServiceId"] = data["ServiceMasterId"];
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                #region Tax
                string sql = "";
                string _activityId = "";
                DataSet dsTax = null;
                sql = "SELECT * FROM JWTransformationPurchaseOrderTax WHERE JWTransformationPurchaseOrderId='" + data["JWTransformationPurchaseOrderId"] + "' and  JWTransformationPurchaseOrderDetailId='" + data["Id"] + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsTax, false, "1");
                if (TaxList != null)
                {
                    for (int i = 0; i < dsTax.Tables[0].Rows.Count; i++)
                    {
                        var k = TaxList.Where(ee => ee["JWTransformationPurchaseOrderDetailId"].ToString() == dsTax.Tables[0].Rows[i]["JWTransformationPurchaseOrderId"].ToString()).ToList();
                        if (k.Count == 0)
                        {
                            dsTax.Tables[0].Rows[i].Delete();
                        }
                    }

                    for (int i = 0; i < TaxList.Count; i++)
                    {
                        dsTax.Tables[0].DefaultView.RowFilter = "JWTransformationPurchaseOrderDetailId='" + TaxList[i]["JWTransformationPurchaseOrderDetailId"] + "'";


                        if (dsTax.Tables[0].DefaultView.Count == 0)
                        {

                            if (_activityId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("JWTransformationPurchaseOrderTax", out _activityId);
                                _activityId = "JTX" + _activityId;
                            }
                            DataRow dr = dsTax.Tables[0].NewRow();
                            dr["Id"] = _activityId + "-" + (i + 1).ToString();

                            dr["JWTransformationPurchaseOrderId"] = data["JWTransformationPurchaseOrderId"];
                            dr["ServiceMasterId"] = data["ServiceMasterId"];



                            dr["TaxCategoryId"] = TaxList[i]["TaxCategoryId"];
                            dr["HSNCodeId"] = TaxList[i]["HSNCodeId"];
                            dr["Percentage"] = TaxList[i]["Percentage"];
                            dr["TaxAmount"] = TaxList[i]["TaxAmount"];




                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dsTax.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsTax.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["JWTransformationPurchaseOrderId"] = data["JWTransformationPurchaseOrderId"];

                            dr["ServiceMasterId"] = data["ServiceMasterId"];
                            dr["TaxCategoryId"] = TaxList[i]["TaxCategoryId"];
                            dr["HSNCodeId"] = TaxList[i]["HSNCodeId"];
                            dr["Percentage"] = TaxList[i]["Percentage"];
                            dr["TaxAmount"] = TaxList[i]["TaxAmount"];


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();

                        }

                    }
                }


                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsTax);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region JW PO Detail : Tax, BoQChild Creation
        //public List<Dictionary<string, object>> detailcreate(List<Dictionary<string, object>> data, string JWPurchaseOrderId, string JWActivityId, string userName, string IPAddress, string OrderSpecific, string type)
        //{
        //    string JWPODId = "";
        //    DataSet dsMaster; DataSet dsPOBOQMap; DataSet dsJwChildMaterial;
        //    DataSet dsJwChildJWInputMaterial = new DataSet();
        //    DataSet dsJwChildJWByProduct = new DataSet();

        //    dsPOBOQMap = new DataSet();
        //    dsJwChildMaterial = new DataSet();
        //    string sql = "";
        //    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
        //    if (String.IsNullOrEmpty(JWPurchaseOrderId))
        //    {
        //        JWPurchaseOrderId = data[0]["JWTransformationPurchaseOrderId"].ToString();
        //    }

        //    con.OpenDataSetThroughAdapter("SELECT * FROM JWTransformationPurchaseOrderDetail WHERE JWTransformationPurchaseOrderId='" + JWPurchaseOrderId + "'", out dsMaster, false, "1");

        //    List<Dictionary<string, object>> dataBoq = new List<Dictionary<string, object>>();
        //    //List<Dictionary<string, object>> dataDetail = new List<Dictionary<string, object>>();

        //    DataSet dsTax = null;
        //    sql = "SELECT * FROM JWTransformationPurchaseOrderTax WHERE JWTransformationPurchaseOrderId='" + JWPurchaseOrderId + "'";
        //    con = new ConnectionManager.DAL.ConManager("1");
        //    con.OpenDataSetThroughAdapter(sql, out dsTax, false, "1");

        //    try
        //    {
        //        Library.General.Conversions.UOMConversion Conversion = new Library.General.Conversions.UOMConversion();
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //        if (OrderSpecific == "Yes")
        //        {
        //            dataBoq = data;
        //            data = MakePodetail(data);
        //            //dataBoq = new List<Dictionary<string, object>>(data);
        //            #region Validation

        //            for (int i = 0; i < data.Count; i++)
        //            {
        //                if (type == "PODETAILLIST")
        //                {
        //                    if (data[i]["TransactionRate"] == null)
        //                        throw new Exception("Rate Can not be Empty.");
        //                }
        //                if (data[i].ContainsKey("RequiredQty"))
        //                {
        //                    if (clsStaticInfo.dbl(data[i]["TransactionQty"].ToString()) + clsStaticInfo.dbl(data[i]["OtherPOQty"].ToString()) > clsStaticInfo.dbl(data[i]["RequiredQty"].ToString()))
        //                    {
        //                        throw new Exception("Current Qty can't be Greater then Transaction Qty.");
        //                    }
        //                }

        //                var _locUOM = data.Where(ee => ee["TransactionUoMId"].ToString().Trim() != data[i]["TransactionUoMId"].ToString().Trim()).ToList();
        //                if (_locUOM == null)
        //                    continue;

        //                if (_locUOM.Count >= 1)
        //                    throw new Exception("UoM can't be Different.");

        //            }
        //            #endregion


        //            for (int i = 0; i < data.Count; i++)
        //            {
        //                dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";

        //                string _Id = "";

        //                if (dsMaster.Tables[0].DefaultView.Count == 0)
        //                {
        //                    bplib.clsGenID genid = new bplib.clsGenID();
        //                    genid.GenID("JWTransformationPurchaseOrderDetail", out _Id);
        //                    data[i]["Id"] = "JWPD" + _Id;
        //                    JWPODId = data[i]["Id"].ToString();
        //                    data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;
        //                    if (OrderSpecific == "Yes")
        //                    {
        //                        data[i]["ReferenceNo"] = data[i]["ReferenceNoM"];

        //                    }
        //                    AddNewRow(dsMaster.Tables[0], data[i]);


        //                }
        //                else
        //                {

        //                    data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;

        //                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
        //                }


        //            }
        //        }


        //        else
        //        {
        //            for (int i = 0; i < data.Count; i++)
        //            {


        //                dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";

        //                string _Id = "";

        //                if (dsMaster.Tables[0].DefaultView.Count == 0)
        //                {
        //                    //if (data[i]["Id"] == null)
        //                    //{
        //                    //    TaxList = null;
        //                    //}

        //                    bplib.clsGenID genid = new bplib.clsGenID();
        //                    genid.GenID("JWTransformationPurchaseOrderDetail", out _Id);
        //                    data[i]["Id"] = "JWPD" + _Id;
        //                    JWPODId = data[i]["Id"].ToString();
        //                    data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;

        //                    AddNewRow(dsMaster.Tables[0], data[i]);


        //                }
        //                else
        //                {

        //                    data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;

        //                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
        //                }


        //            }
        //        }

        //        if (data != null)
        //        {
        //            clsStaticInfo _info = new clsStaticInfo();
        //            if (OrderSpecific == "Yes")
        //            {
        //                if (data[0].ContainsKey("BOQId"))
        //                {

        //                    for (int i = 0; i < data.Count; i++)
        //                    {
        //                        SaveJWTransformationPurchaseOrderChildMaterial(dataBoq, data[i]["Id"].ToString(), JWActivityId, Conversion, out dsJwChildMaterial);

        //                        JPOBOQMAPCreate(dataBoq, data[i]["Id"].ToString(), userName, IPAddress, out dsPOBOQMap);
        //                    }
        //                    _info.SaveDataSets(dsMaster);
        //                    _info.SaveDataSets(dsPOBOQMap, dsJwChildMaterial);
        //                }
        //                _info.SaveDataSets(dsMaster);

        //            }
        //            else
        //            {

        //                //for (int i = 0; i < data.Count; i++)
        //                //{
        //                //    SaveJWTransformationPurchaseOrderInputMaterial(data, JWActivityId, Conversion, out dsJwChildJWInputMaterial);

        //                //    SaveJWTransformationPurchaseOrderByProductMaterial(data, JWActivityId, Conversion, out dsJwChildJWByProduct);
        //                //}

        //                _info.SaveDataSets(dsMaster, dsJwChildJWInputMaterial, dsJwChildJWByProduct);

        //            }
        //        }

        //        return data;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public List<Dictionary<string, object>> detailcreate(List<Dictionary<string, object>> data, string JWPurchaseOrderId, string JWActivityId, string userName, string IPAddress, string OrderSpecific, string type, List<Dictionary<string, object>> taxCategoryList)
        {
            string JWPODId = "";
            DataSet dsMaster; DataSet dsPOBOQMap; DataSet dsJwChildMaterial;
            DataSet dsJwChildJWInputMaterial = new DataSet();
            DataSet dsJwChildJWByProduct = new DataSet();

            dsPOBOQMap = new DataSet();
            dsJwChildMaterial = new DataSet();
            string sql = "";
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            if (String.IsNullOrEmpty(JWPurchaseOrderId))
            {
                JWPurchaseOrderId = data[0]["JWTransformationPurchaseOrderId"].ToString();
            }

            con.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationContractChild where JobActivityId='" + data[0]["JobActivityId"] + "' and JobWorkItemMasterId='" + data[0]["JobWorkItemMasterId"] + "' and ArticleCodeId='" + data[0]["ArticleId"] + "' and MaterialMasterId='"+data[0]["MaterialMasterId"] + "' and JobWorkTransformationContractMasterId='" + data[0]["JWTransformationPurchaseOrderId"] + "' AND  Id<>'" + data[0]["Id"] + "' ", out dsMaster, false, "1");
            if (dsMaster.Tables[0].Rows.Count > 0)
            {
                throw new Exception("Same Activity, JW Output Item, Material and Article already exist.");
            }

            con.OpenDataSetThroughAdapter("SELECT * FROM JobWorkTransformationContractChild WHERE JobWorkTransformationContractMasterId='" + JWPurchaseOrderId + "'", out dsMaster, false, "1");

            List<Dictionary<string, object>> dataBoq = new List<Dictionary<string, object>>();
            //List<Dictionary<string, object>> dataDetail = new List<Dictionary<string, object>>();

            DataSet dsTax = null;
            sql = "SELECT * FROM JWTransformationPurchaseOrderTax WHERE JWTransformationPurchaseOrderId='" + JWPurchaseOrderId + "'";
            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(sql, out dsTax, false, "1");

            try
            {
                Library.General.Conversions.UOMConversion Conversion = new Library.General.Conversions.UOMConversion();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (OrderSpecific == "Yes")
                {
                    dataBoq = data;
                    data = MakePodetail(data);
                    //dataBoq = new List<Dictionary<string, object>>(data);
                    #region Validation

                    for (int i = 0; i < data.Count; i++)
                    {
                        if (type == "PODETAILLIST")
                        {
                            if (data[i]["TransactionRate"] == null)
                                throw new Exception("Rate Can not be Empty.");
                        }
                        if (data[i].ContainsKey("RequiredQty"))
                        {
                            if (clsStaticInfo.dbl(data[i]["TransactionQty"].ToString()) + clsStaticInfo.dbl(data[i]["OtherPOQty"].ToString()) > clsStaticInfo.dbl(data[i]["RequiredQty"].ToString()))
                            {
                                throw new Exception("Current Qty can't be Greater then Transaction Qty.");
                            }
                        }

                        var _locUOM = data.Where(ee => ee["TransactionUoMId"].ToString().Trim() != data[i]["TransactionUoMId"].ToString().Trim()).ToList();
                        if (_locUOM == null)
                            continue;

                        if (_locUOM.Count >= 1)
                            throw new Exception("UoM can't be Different.");

                    }
                    #endregion


                    for (int i = 0; i < data.Count; i++)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";

                        string _Id = "";

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("JWTransformationPurchaseOrderDetail", out _Id);
                            data[i]["Id"] = "JWPD" + _Id;
                            JWPODId = data[i]["Id"].ToString();
                            data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;
                            if (OrderSpecific == "Yes")
                            {
                                data[i]["ReferenceNo"] = data[i]["ReferenceNoM"];

                            }
                            AddNewRow(dsMaster.Tables[0], data[i]);


                        }
                        else
                        {

                            data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;

                            EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
                        }


                    }
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {


                        dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";
                        dsTax.Tables[0].DefaultView.RowFilter = "JWTransformationPurchaseOrderDetailId='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";

                        string _Id = "";

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            //if (data[i]["Id"] == null)
                            //{
                            //    TaxList = null;
                            //}

                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("JobWorkTransformationContractChild", out _Id);
                            data[i]["Id"] = "JWPD" + _Id;
                            JWPODId = data[i]["Id"].ToString();
                            //data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;
                            data[i]["JobWorkTransformationContractMasterId"] = JWPurchaseOrderId;
                            data[i]["Quantity"] = data[i]["TransactionQty"];

                            AddNewRow(dsMaster.Tables[0], data[i]);



                        }


                        else
                        {

                            //data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;
                            data[i]["JobWorkTransformationContractMasterId"] = JWPurchaseOrderId;
                            data[i]["Quantity"] = data[i]["TransactionQty"];
                            EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
                        }
                        string DetailIdid = dsMaster.Tables[0].Rows[i]["Id"].ToString();
                        for (int i1 = 0; i1 < taxCategoryList.Count; i1++)
                        {


                            if (dsTax.Tables[0].DefaultView.Count == 0)
                            {
                                

                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("JWTransformationPurchaseOrderTax", out _Id);
                                taxCategoryList[i1]["Id"] = "JWPDT" + _Id;
                                //JWPODId = taxCategoryList[i1]["Id"].ToString();
                                //data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;
                                taxCategoryList[i1]["JobWorkTransformationContractMasterId"] = JWPurchaseOrderId;
                                taxCategoryList[i1]["JWTransformationPurchaseOrderDetailId"] = DetailIdid;
                                //data[i]["Quantity"] = data[i]["TransactionQty"];

                                AddNewRow(dsTax.Tables[0], taxCategoryList[i1]);



                            }


                            else
                            {

                                //data[i]["JWTransformationPurchaseOrderId"] = JWPurchaseOrderId;
                                taxCategoryList[i1]["JobWorkTransformationContractMasterId"] = JWPurchaseOrderId;
                                taxCategoryList[i1]["JWTransformationPurchaseOrderDetailId"] = DetailIdid;
                                taxCategoryList[i1]["Quantity"] = data[i]["TransactionQty"];
                                EditRow(dsTax.Tables[0].DefaultView[0].Row, taxCategoryList[i1]);
                            }
                        }

                    }

                }

                if (data != null)
                {
                    clsStaticInfo _info = new clsStaticInfo();
                    if (OrderSpecific == "Yes")
                    {
                        if (data[0].ContainsKey("BOQId"))
                        {

                            for (int i = 0; i < data.Count; i++)
                            {
                                SaveJWTransformationPurchaseOrderChildMaterial(dataBoq, data[i]["Id"].ToString(), JWActivityId, Conversion, out dsJwChildMaterial);

                                JPOBOQMAPCreate(dataBoq, data[i]["Id"].ToString(), userName, IPAddress, out dsPOBOQMap);
                            }
                            _info.SaveDataSets(dsMaster);
                            _info.SaveDataSets(dsPOBOQMap, dsJwChildMaterial);
                        }
                        _info.SaveDataSets(dsMaster);

                    }
                    else
                    {

                        //for (int i = 0; i < data.Count; i++)
                        //{
                        //    SaveJWTransformationPurchaseOrderInputMaterial(data, JWActivityId, Conversion, out dsJwChildJWInputMaterial);

                        //    SaveJWTransformationPurchaseOrderByProductMaterial(data, JWActivityId, Conversion, out dsJwChildJWByProduct);
                        //}

                        _info.SaveDataSets(dsMaster, dsTax);//, dsJwChildJWInputMaterial, dsJwChildJWByProduct,

                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> SaveTaxList(List<Dictionary<string, object>> data, List<Dictionary<string, object>> TaxList, string userName, string IPAddress)
        {
            DataSet dsTax = null;
            string sql = "";
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            sql = "SELECT * FROM JWTransformationPurchaseOrderTax WHERE JWTransformationPurchaseOrderId='" + data[0]["JWTransformationPurchaseOrderId"] + "'";
            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(sql, out dsTax, false, "1");

            try
            {
                string _taxId = "";
                if (TaxList != null)
                {
                    for (int tc = 0; tc < dsTax.Tables[0].Rows.Count; tc++)
                    {

                        var k = TaxList.Where(ee => ee["JWTransformationPurchaseOrderDetailId"].ToString() == dsTax.Tables[0].Rows[tc]["JWTransformationPurchaseOrderDetailId"].ToString() && ee["Id"].ToString() == dsTax.Tables[0].Rows[tc]["Id"].ToString()).ToList();

                        if (k.Count == 0)
                        {
                            dsTax.Tables[0].Rows[tc].Delete();
                        }
                    }


                    for (int t = 0; t < TaxList.Count; t++)
                    {

                        #region duplicate Check
                        //List<string> _loc = TaxList.Where(ee => ee.Selected == true && ee.Action.Trim() == menuAction[i].Action.Trim()).ToList();
                        var _loc = TaxList.Where(ee => ee["TaxCategoryId"].ToString() == TaxList[t]["TaxCategoryId"].ToString()).ToList();

                        if (_loc == null)
                            continue;

                        if (_loc.Count > 1)
                            throw new Exception("Same Tax Cannot be added.");
                        if (TaxList[t]["TaxAmount"] == null)
                        {
                            throw new Exception("TaxAmount cannot be blank for the " + TaxList[t]["UserName"] + ".");

                        }
                        #endregion


                        dsTax.Tables[0].DefaultView.RowFilter = "JWTransformationPurchaseOrderDetailId='" + TaxList[t]["JWTransformationPurchaseOrderDetailId"] + "' and Id = '" + TaxList[t]["Id"] + "'  ";


                        if (dsTax.Tables[0].DefaultView.Count == 0)
                        {

                            if (_taxId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("JWTransformationPurchaseOrderTax", out _taxId);
                                _taxId = "JTX" + _taxId;
                            }
                            DataRow dr = dsTax.Tables[0].NewRow();
                            dr["Id"] = _taxId + "-" + (t + 1).ToString();

                            dr["JWTransformationPurchaseOrderId"] = data[0]["JWTransformationPurchaseOrderId"];
                            dr["JWTransformationPurchaseOrderDetailId"] = data[0]["Id"];
                            dr["TaxCategoryId"] = TaxList[t]["TaxCategoryId"];
                            if (TaxList[t].ContainsKey("HSNCodeId"))
                            {
                                if (TaxList[t]["HSNCodeId"] == null)
                                {

                                    dr["HSNCodeId"] = null;
                                }
                                else
                                {
                                    if (TaxList[t]["HSNCodeId"].ToString() == "")
                                    {
                                        dr["HSNCodeId"] = null;
                                    }
                                    else
                                    {
                                        dr["HSNCodeId"] = TaxList[t]["HSNCodeId"];

                                    }

                                }
                            }
                            else
                            {
                                dr["HSNCodeId"] = null;
                            }
                            dr["Percentage"] = bplib.clsWebLib.RetValidLen(TaxList[t]["Percentage"]);
                            dr["TaxAmount"] = TaxList[t]["TaxAmount"];
                            dr["AddedBy"] = userName;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = IPAddress;

                            dsTax.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsTax.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["JWTransformationPurchaseOrderId"] = TaxList[t]["JWTransformationPurchaseOrderId"];
                            dr["JWTransformationPurchaseOrderDetailId"] = TaxList[t]["JWTransformationPurchaseOrderDetailId"];
                            if (TaxList[t].ContainsKey("HSNCodeId"))
                            {
                                if (TaxList[t]["HSNCodeId"] == null)
                                {

                                    dr["HSNCodeId"] = null;
                                }
                                else
                                {
                                    if (TaxList[t]["HSNCodeId"].ToString() == "")
                                    {
                                        dr["HSNCodeId"] = null;
                                    }
                                    else
                                    {
                                        dr["HSNCodeId"] = TaxList[t]["HSNCodeId"];

                                    }

                                }
                            }
                            else
                            {
                                dr["HSNCodeId"] = null;
                            }
                            dr["TaxCategoryId"] = bplib.clsWebLib.RetValidLen(TaxList[t]["TaxCategoryId"]);
                            dr["Percentage"] = bplib.clsWebLib.RetValidLen(TaxList[t]["Percentage"]);
                            dr["TaxAmount"] = TaxList[t]["TaxAmount"];
                            dr["UpdatedBy"] = userName;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = IPAddress;
                            dr.EndEdit();
                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTax);
                return TaxList;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        #endregion

        public void JPOBOQMAPCreate(List<Dictionary<string, object>> data, string JWPODetailId, string userName, string IPAddress, out DataSet dsPOboq)
        {
            try
            {
                dsPOboq = new DataSet();
                string sql = "";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string _poboqId = "";
                sql = "SELECT * FROM JWPOBOQMAP WHERE JWPODetailId='" + JWPODetailId + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsPOboq, false, "1");
                Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();


                for (int i = 0; i < data.Count; i++)
                {
                    dsPOboq.Tables[0].DefaultView.RowFilter = "BOQDetailId = '" + data[i]["BOQId"] + "'  ";

                    if (dsPOboq.Tables[0].DefaultView.Count == 0)
                    {
                        if (_poboqId == "")
                        {
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID("JWPOBOQMAP", out _poboqId);
                            _poboqId = "JPB" + _poboqId;

                        }
                        DataRow dr = dsPOboq.Tables[0].NewRow();

                        double conversiongroupListData = conversion.Convert(data[i]["MaterialMasterId"].ToString(), bplib.clsWebLib.RetValidLen(data[i]["TransactionUoMId"]).ToString(), bplib.clsWebLib.RetValidLen(data[i]["BaseUOMId"]).ToString(), clsStaticInfo.dbl(data[i]["TransactionQty"]));
                        dr["BaseQty"] = Convert.ToDecimal(conversiongroupListData);

                        dr["Id"] = _poboqId + "-" + (i + 1).ToString();

                        dr["JWPODetailId"] = JWPODetailId;
                        dr["BOQDetailId"] = data[i]["BOQId"];
                        dr["TransactionQty"] = data[i]["TransactionQty"];
                        dr["TransactionUoMId"] = data[i]["TransactionUoMId"];

                        dr["BaseUoMId"] = bplib.clsWebLib.RetValidLen(data[i]["BaseUOMId"]).ToString() == "" ? null : bplib.clsWebLib.RetValidLen(data[i]["BaseUOMId"]).ToString();
                        dr["POBOQQty"] = conversion.Convert(data[i]["MaterialMasterId"].ToString(), bplib.clsWebLib.RetValidLen(data[i]["TransactionUoMId"]).ToString(), bplib.clsWebLib.RetValidLen(data[i]["POUoMId"]).ToString(), clsStaticInfo.dbl(data[i]["TransactionQty"]));

                        dr["POUoMId"] = data[i]["POUoMId"];

                        dr["AddedBy"] = userName;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = IPAddress;

                        dsPOboq.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsPOboq.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["JWPODetailId"] = JWPODetailId;
                        dr["BOQDetailId"] = data[i]["BOQId"];
                        dr["TransactionQty"] = data[i]["TransactionQty"];
                        dr["TransactionUoMId"] = data[i]["TransactionUoMId"];
                        dr["BaseUoMId"] = bplib.clsWebLib.RetValidLen(data[i]["BaseUOMId"]).ToString() == "" ? null : bplib.clsWebLib.RetValidLen(data[i]["BaseUOMId"]).ToString();
                        dr["POBOQQty"] = conversion.Convert(data[i]["MaterialMasterId"].ToString(), bplib.clsWebLib.RetValidLen(data[i]["TransactionUoMId"]).ToString(), bplib.clsWebLib.RetValidLen(data[i]["POUoMId"]).ToString(), clsStaticInfo.dbl(data[i]["TransactionQty"]));

                        dr["POUoMId"] = data[i]["POUoMId"];
                        dr["UpdatedBy"] = userName;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = IPAddress;
                        dr.EndEdit();
                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public string GetJWPOChildList(string jwpoId)
        {
            string strkey = "1=1";


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //    string sql = @" SELECT JWTPD.*,ISNULL(JWI.UserName,'') JWItemName,ISNULL(JWItemUOM.Code,'') JWItemUOM ,ISNULL(MM.UserName,'') MaterialMasterName
            //                        ,ISNULL(MMA.ShortName,'') ArticleName
            //                        ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
            //                         ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
            //                         ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
            //                         ,ISNULL(BaseUOM.Code,'') BaseUOM,ISNULL(TransactionUoM.Code,'') TransactionUoM
            //                         ,ISNULL(Country.UserName,'') Country
            //                         --JWTransfromation Detail 
            //                         ,JWA.UserName JWActivity,
            // JWTPD.ResponsiblePersonId
            //                         ,EEI.EmployeeName ResponsiblePersonName 
            //                         , JWTPD.JobWorkItemMasterId, JWI.UserName OutputMaterial, JWTPD.OutputMaterialUOMId
            //                         , JWTPD.RateApplyOn,JWTPD.CurrencyId, CURR.Code CURR--, JWTPD.MinRate, JWTM.MaxRate
            //                         , JWTPD.ByProductApplicable



            //                        , ReferenceNo,BaseAmount
            //                        , jwtax.TaxAmount
            //                        FROM JWTransformationPurchaseOrderDetail JWTPD 

            //                        LEFT JOIN HKP.JobWorkItem JWI ON JWI.Id = JWTPD.JobWorkItemMasterId
            //                        LEFT JOIN HKP.JobWorkActivity JWA ON JWA.Id = JWTPD.JobWorkItemMasterId
            //                        LEFT JOIN SCS.UnitOfMeasurement JWItemUOM  ON JWItemUOM.Id = JWTPD.OutputMaterialUOMId
            //                        LEFT JOIN MST.MaterialMaster MM  ON MM.Id = JWTPD.MaterialMasterId
            //                        LEFT JOIN MST.MaterialMasterArticle MMA  ON MMA.Id = JWTPD.ArticleId
            //                        LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = JWTPD.FirstCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = JWTPD.FirstCharacteristicsValueId
            //                        LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = JWTPD.SecondCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = JWTPD.SecondCharacteristicsValueId
            //                        LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = JWTPD.ThirdCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = JWTPD.ThirdCharacteristicsValueId
            //                        LEFT JOIN SCS.UnitOfMeasurement BaseUOM  ON BaseUOM.Id = JWTPD.BaseUOMId
            //                        LEFT JOIN SCS.UnitOfMeasurement TransactionUoM  ON TransactionUoM.Id = JWTPD.TransactionUoMId
            //                        LEFT JOIN SCS.Country Country  ON Country.Id = JWTPD.CountryId
            //                        LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWTPD.ResponsiblePersonId


            //                        LEFT JOIN SCS.Currency CURR ON CURR.Id = JWTPD.CurrencyId

            //                     LEFT JOIN (select Sum(TaxAmount) TaxAmount,JWTransformationPurchaseOrderDetailId,JWTransformationPurchaseOrderId from JWTransformationPurchaseOrderTax  where  JWTransformationPurchaseOrderId = 'JWP17' GROUP BY JWTransformationPurchaseOrderId, JWTransformationPurchaseOrderDetailId ) jwtax 
            //ON jwtax.JWTransformationPurchaseOrderId  = JWTPD.JWTransformationPurchaseOrderId and  jwtax.JWTransformationPurchaseOrderDetailId  = JWTPD.Id 
            //                   WHERE " + strkey + "  and JWTPD.JWTransformationPurchaseOrderId = '" + jwpoId + @"'";
            string sql = @"SELECT JWA.Id JobWorkActivityId, JWA.UserName JobWorkActivity,JWTPD.*,ISNULL(JWI.UserName,'') JWItemName,ISNULL(JWItemUOM.Code,'') JWItemUOM ,MM.Id MaterialMasterId ,ISNULL(MM.UserName,'') MaterialMasterName
                            ,MMA.Id ArticleId,ISNULL(MMA.ShortName,'') ArticleName
                            ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
                                ,ISNULL(BaseUOM.Code,'') BaseUOM,ISNULL(TransactionUoM.Code,'') TransactionUoM
                                ,ISNULL(Country.UserName,'') Country
                                --JWTransfromation Detail 
                                ,JWA.UserName JWActivity,
	                            JWTPD.ResponsiblePersonId
                                ,EEI.EmployeeName ResponsiblePersonName 
                                , JWTPD.JobWorkItemMasterId, JWI.UserName OutputMaterial, JWTPD.OutputMaterialUOMId
                                , JWTPD.RateApplyOn,JWTPD.CurrencyId, CURR.Code CURR--, JWTPD.MinRate, JWTM.MaxRate
                                , JWTPD.ByProductApplicable 
                                ,JWTPD.Quantity	TransactionQty	
	                            ,JWTPD.RatePerUnit	TransactionRate
	                            ,(JWTPD.Quantity*JWTPD.RatePerUnit) TransactionAmount
                            , JWTPD.ReferenceNo,((JWTPD.Quantity*JWTPD.RatePerUnit)*po.ToCurrencyRate) BaseAmount
                            , jwtax.TaxAmount,JWTPD.TransactionUoMId,TransactionUoM.Code TransactionUoM,JWTPD.BaseUOMId,BaseUOM.Code BaseUOM
                            ,MS.Id MaterialStorageId,MS.UserName MaterialStorage,EEI.EmployeeName ResponsiblePerson ,ISNULL(MM.UserName,'') MaterialName
                            FROM JobWorkTransformationContractChild JWTPD      
                            left JOIN [dbo].[JWTransformationPurchaseOrder] PO On PO.Id=JWTPD.JobWorkTransformationContractMasterId
                            LEFT JOIN HKP.JobWorkItem JWI ON JWI.Id = JWTPD.JobWorkItemMasterId
                            LEFT JOIN HKP.JobWorkActivity JWA ON JWA.Id = JWTPD.JobActivityId
                            LEFT JOIN SCS.UnitOfMeasurement JWItemUOM  ON JWItemUOM.Id = JWTPD.OutputMaterialUOMId
                            LEFT JOIN MST.MaterialMaster MM  ON MM.Id = JWTPD.MaterialMasterId
                            LEFT JOIN MST.MaterialMasterArticle MMA  ON MMA.Id = JWTPD.ArticleId
                            LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = JWTPD.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = JWTPD.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = JWTPD.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = JWTPD.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = JWTPD.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = JWTPD.ThirdCharacteristicsValueId
                            LEFT JOIN SCS.UnitOfMeasurement BaseUOM  ON BaseUOM.Id = JWTPD.BaseUOMId
                            LEFT JOIN SCS.UnitOfMeasurement TransactionUoM  ON TransactionUoM.Id = JWTPD.TransactionUoMId
                            LEFT JOIN SCS.Country Country  ON Country.Id = JWTPD.CountryId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWTPD.ResponsiblePersonId
                              
                           
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = JWTPD.CurrencyId
    
                            LEFT JOIN (select Sum(TaxAmount) TaxAmount,JWTransformationPurchaseOrderDetailId,JWTransformationPurchaseOrderId from JWTransformationPurchaseOrderTax  where  JWTransformationPurchaseOrderId = '" + jwpoId + @"' GROUP BY JWTransformationPurchaseOrderId, JWTransformationPurchaseOrderDetailId ) jwtax 
                            ON jwtax.JWTransformationPurchaseOrderId  = JWTPD.JobWorkTransformationContractMasterId and  jwtax.JWTransformationPurchaseOrderDetailId  = JWTPD.Id 
                            left join hkp.MaterialStorage MS ON MS.Id=JWTPD.MaterialLocationId
                            WHERE " + strkey + "  and JWTPD.JobWorkTransformationContractMasterId = '" + jwpoId + @"'";
            return sql;
		}

        public string GetJwPoDetailByProduct(string jwPODetailId)
        {
            string strSql = "";

            strSql = @"SELECT JWI.UserName ByProduct
                          FROM JWTransformationPurchaseOrderByProductChildMaterial JWTPOB
                        
                          Left Join JWByProduct JWBP ON JWBP.Id =  JWTPOB.JWByProductId
                          left join JWItem JWI ON JWI.Id =JWBP.MaterialId
                            where JWTPOB.JWPODetailId = '" + jwPODetailId + @"'";

            return strSql;
        }
        public string GetJwTransPoDetailInputMaterial(string jwPODetailId)
        {
            string strSql = "";

            strSql = @"SELECT JWI.UserName InputItem
                          FROM JWTransformationPurchaseOrderInputChildMaterial JWTPOI
                        
                          Left Join JWInputMaterial JWIM ON JWIM.Id =  JWTPOI.JWInputMaterialId
                          left join JWItem JWI ON JWI.Id =JWIM.MaterialId
                            where JWTPOI.JWPODetailId = '" + jwPODetailId + @"'";

            return strSql;
        }

        public string GetJWPOChildListAll()
        {
            string strkey = "1=1";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //    string sql = @" SELECT JWTPD.*,ISNULL(JWI.UserName,'') JWItemName,ISNULL(JWItemUOM.Code,'') JWItemUOM 
            //                        ,ISNULL(MM.UserName,'') MaterialMasterName
            //                        ,ISNULL(MMA.ShortName,'') ArticleName
            //                        ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
            //                         ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
            //                         ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
            //                         ,ISNULL(BaseUOM.Code,'') BaseUOM,ISNULL(TransactionUoM.Code,'') TransactionUoM
            //                         ,ISNULL(Country.UserName,'') Country

            //                         ,JWA.UserName JWActivity, JWTM.ResponsiblePersonId,ISNULL(JWTM.ProcessId,'') ProcessId
            //                         , Process.UserName Process,EEI.EmployeeName ResponsiblePersonName 
            //                         , JWTM.OutputMaterialId, OUM.UserName OutputMaterial, JWTM.OutputMaterialUOMId,UOM.ShortName UOM
            //                         , JWTM.RateApplicableOn,JWTM.CurrencyId, CURR.Code CurrencyName, JWTM.MinRate, JWTM.MaxRate
            //                         , JWTM.CycleTimeDays, JWTM.ByProductApplicable
            //                         , ISNULL(SM.UserName,'') JWServiceName, ISNULL(SM.Id,'') ServiceId                    
            //                        , ReferenceNo,BaseAmount
            //                        , ISNULL(jwtax.TaxAmount,0),JWTPD.TransactionRate
            //,ISNULL(JWTPD.TransactionAmount,0) TransactionAmount,ISNULL(JWTPD.TransactionAmount,0) + ISNULL(jwtax.TaxAmount,0) TotalAmount
            //                        FROM JWTransformationPurchaseOrderDetail JWTPD 
            //                        LEFT JOIN JWTransformationMaster JWTM ON JWTM.Id = JWTPD.JWTransformationMasterId
            //                        LEFT JOIN JWItem JWI ON JWI.Id = JWTPD.JWItemId
            //                        LEFT JOIN JWActivity JWA ON JWA.Id = JWTM.JWActivityId
            //                        LEFT JOIN SCS.UnitOfMeasurement JWItemUOM  ON JWItemUOM.Id = JWTPD.JWItemUOMId
            //                        LEFT JOIN MST.MaterialMaster MM  ON MM.Id = JWTPD.MaterialMasterId
            //                        LEFT JOIN MST.MaterialMasterArticle MMA  ON MMA.Id = JWTPD.ArticleId
            //                        LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = JWTPD.FirstCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = JWTPD.FirstCharacteristicsValueId
            //                        LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = JWTPD.SecondCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = JWTPD.SecondCharacteristicsValueId
            //                        LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = JWTPD.ThirdCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = JWTPD.ThirdCharacteristicsValueId
            //                        LEFT JOIN SCS.UnitOfMeasurement BaseUOM  ON BaseUOM.Id = JWTPD.BaseUOMId
            //                        LEFT JOIN SCS.UnitOfMeasurement TransactionUoM  ON TransactionUoM.Id = JWTPD.TransactionUoMId
            //                        LEFT JOIN SCS.Country Country  ON Country.Id = JWTPD.CountryId
            //                        LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWTM.ResponsiblePersonId
            //                        LEFT JOIN JWItem OUM ON OUM.Id = JWTM.OutputMaterialId
            //                        LEFT JOIN [SCS].[UnitOfMeasurement] UOM  oN UOM.Id = JWTM.OutputMaterialUOMId
            //                        LEFT JOIN [HKP].[Process] Process  oN Process.Id = JWTM.ProcessId
            //                        LEFT JOIN SCS.Currency CURR ON CURR.Id = JWTM.CurrencyId
            //                        Left Join HKP.ServiceMaster SM ON JWA.ServiceId = SM.Id
            //                     LEFT JOIN (select Sum(TaxAmount) TaxAmount,JWTransformationPurchaseOrderDetailId,JWTransformationPurchaseOrderId from JWTransformationPurchaseOrderTax   GROUP BY JWTransformationPurchaseOrderId, JWTransformationPurchaseOrderDetailId ) jwtax 
            //on jwtax.JWTransformationPurchaseOrderId  = JWTPD.JWTransformationPurchaseOrderId and  jwtax.JWTransformationPurchaseOrderDetailId  = JWTPD.Id 
            //                     WHERE " + strkey + "  ";


            //    string sql = @"SELECT JWTPD.*,ISNULL(JWI.UserName,'') JWItemName,ISNULL(JWItemUOM.Code,'') JWItemUOM 
            //                        ,ISNULL(MM.UserName,'') MaterialMasterName
            //                        ,ISNULL(MMA.ShortName,'') ArticleName
            //                        ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
            //                         ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
            //                         ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
            //                         ,ISNULL(BaseUOM.Code,'') BaseUOM,ISNULL(TransactionUoM.Code,'') TransactionUoM
            //                         ,ISNULL(Country.UserName,'') Country

            //                         --,JWA.UserName JWActivity
            // , JWTPD.ResponsiblePersonId
            // --,ISNULL(JWTM.ProcessId,'') ProcessId
            //                         --, Process.UserName Process
            // ,EEI.EmployeeName ResponsiblePersonName 
            //                         , OUM. Id OutputMaterialId, OUM.UserName OutputMaterial,  OUM.UOMId OutputMaterialUOMId,UOM.ShortName UOM
            //                         , JWTPD.RateApplyOn,JWTPD.CurrencyId, CURR.Code CurrencyName, JWTPD.RateApplyOn, JWTPD.RatePerUnit

            // ,  JWTPD.ByProductApplicable
            //                         , ISNULL(SM.UserName,'') JWServiceName, ISNULL(SM.Id,'') ServiceId                    
            //                        , ReferenceNo,BaseAmount
            //                        , ISNULL(jwtax.TaxAmount,0),JWTPD.RatePerUnit as TransactionRate
            //,ISNULL(JWTPD.Quantity * JWTPD.RatePerUnit,0) TransactionAmount,ISNULL(JWTPD.Quantity * JWTPD.RatePerUnit,0) + ISNULL(jwtax.TaxAmount,0) TotalAmount
            //                        FROM dbo.JobWorkTransformationContractChild JWTPD 
            //                       -- LEFT JOIN MST.JobWorkTransformationMaster JWTM ON JWTM.Id = JWTPD.JWTransformationMasterITId
            //                        LEFT JOIN HKP.JobWorkItem JWI ON JWI.Id = JWTPD.JobWorkItemMasterId
            //                       -- LEFT JOIN HKP.JOBWORKActivity JWA ON JWA.Id = JWTPD.JWActivityId
            //                        LEFT JOIN SCS.UnitOfMeasurement JWItemUOM  ON JWItemUOM.Id = JWTPD.OutputMaterialUOMId
            //                        LEFT JOIN MST.MaterialMaster MM  ON MM.Id = JWTPD.MaterialMasterId
            //                        LEFT JOIN MST.MaterialMasterArticle MMA  ON MMA.Id = JWTPD.ArticleId
            //                        LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = JWTPD.FirstCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = JWTPD.FirstCharacteristicsValueId
            //                        LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = JWTPD.SecondCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = JWTPD.SecondCharacteristicsValueId
            //                        LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = JWTPD.ThirdCharacteristicsId
            //                        LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = JWTPD.ThirdCharacteristicsValueId
            //                        LEFT JOIN SCS.UnitOfMeasurement BaseUOM  ON BaseUOM.Id = JWTPD.BaseUOMId
            //                        LEFT JOIN SCS.UnitOfMeasurement TransactionUoM  ON TransactionUoM.Id = JWTPD.TransactionUoMId
            //                        LEFT JOIN SCS.Country Country  ON Country.Id = JWTPD.CountryId
            //                        LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWTPD.ResponsiblePersonId
            //                        LEFT JOIN HKP.JobWorkItem OUM ON OUM.Id = JWTPD.JobActivityId
            //                        LEFT JOIN [SCS].[UnitOfMeasurement] UOM  oN UOM.Id = OUM.UOMId
            //                        --LEFT JOIN [HKP].[Process] Process  oN Process.Id = JWTM.ProcessId
            //                        LEFT JOIN SCS.Currency CURR ON CURR.Id = JWTPD.CurrencyId
            //                        Left Join HKP.ServiceMaster SM ON JWTPD.ServiceId = SM.Id
            //                     LEFT JOIN (select Sum(TaxAmount) TaxAmount,JWTransformationPurchaseOrderDetailId,JWTransformationPurchaseOrderId from JWTransformationPurchaseOrderTax   GROUP BY JWTransformationPurchaseOrderId, JWTransformationPurchaseOrderDetailId ) jwtax 
            //on jwtax.JWTransformationPurchaseOrderId  = JWTPD.JobWorkTransformationContractMasterId and  jwtax.JWTransformationPurchaseOrderDetailId  = JWTPD.Id 
            //                     WHERE  " + strkey + " ";

            string sql = @"SELECT JWA.Id JobWorkActivityId, JWA.UserName JobWorkActivity,JWTPD.*,ISNULL(JWI.UserName,'') JWItemName,ISNULL(JWItemUOM.Code,'') JWItemUOM ,MM.Id MaterialMasterId ,ISNULL(MM.UserName,'') MaterialMasterName
                            ,MMA.Id ArticleId,ISNULL(MMA.ShortName,'') ArticleName
                            ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
                                ,ISNULL(BaseUOM.Code,'') BaseUOM,ISNULL(TransactionUoM.Code,'') TransactionUoM
                                ,ISNULL(Country.UserName,'') Country
                                --JWTransfromation Detail 
                                ,JWA.UserName JWActivity,
	                            JWTPD.ResponsiblePersonId
                                ,EEI.EmployeeName ResponsiblePersonName 
                                , JWTPD.JobWorkItemMasterId, JWI.UserName OutputMaterial, JWTPD.OutputMaterialUOMId
                                , JWTPD.RateApplyOn,JWTPD.CurrencyId, CURR.Code CURR--, JWTPD.MinRate, JWTM.MaxRate
                                , JWTPD.ByProductApplicable 
                                ,JWTPD.Quantity	TransactionQty	
	                            ,JWTPD.RatePerUnit	TransactionRate
	                            ,(JWTPD.Quantity*JWTPD.RatePerUnit) TransactionAmount
                            , JWTPD.ReferenceNo,((JWTPD.Quantity*JWTPD.RatePerUnit)*po.ToCurrencyRate) BaseAmount
                            , jwtax.TaxAmount,JWTPD.TransactionUoMId,TransactionUoM.Code TransactionUoM,JWTPD.BaseUOMId,BaseUOM.Code BaseUOM
                            ,MS.Id MaterialStorageId,MS.UserName MaterialStorage,EEI.EmployeeName ResponsiblePerson ,ISNULL(MM.UserName,'') MaterialName
                            FROM JobWorkTransformationContractChild JWTPD      
                            left JOIN [dbo].[JWTransformationPurchaseOrder] PO On PO.Id=JWTPD.JobWorkTransformationContractMasterId
                            LEFT JOIN HKP.JobWorkItem JWI ON JWI.Id = JWTPD.JobWorkItemMasterId
                            LEFT JOIN HKP.JobWorkActivity JWA ON JWA.Id = JWTPD.JobActivityId
                            LEFT JOIN SCS.UnitOfMeasurement JWItemUOM  ON JWItemUOM.Id = JWTPD.OutputMaterialUOMId
                            LEFT JOIN MST.MaterialMaster MM  ON MM.Id = JWTPD.MaterialMasterId
                            LEFT JOIN MST.MaterialMasterArticle MMA  ON MMA.Id = JWTPD.ArticleId
                            LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = JWTPD.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = JWTPD.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = JWTPD.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = JWTPD.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = JWTPD.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = JWTPD.ThirdCharacteristicsValueId
                            LEFT JOIN SCS.UnitOfMeasurement BaseUOM  ON BaseUOM.Id = JWTPD.BaseUOMId
                            LEFT JOIN SCS.UnitOfMeasurement TransactionUoM  ON TransactionUoM.Id = JWTPD.TransactionUoMId
                            LEFT JOIN SCS.Country Country  ON Country.Id = JWTPD.CountryId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWTPD.ResponsiblePersonId
                              
                           
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = JWTPD.CurrencyId
    
                            LEFT JOIN (select Sum(TaxAmount) TaxAmount,JWTransformationPurchaseOrderDetailId,JWTransformationPurchaseOrderId from JWTransformationPurchaseOrderTax GROUP BY JWTransformationPurchaseOrderId, JWTransformationPurchaseOrderDetailId ) jwtax 
                            ON jwtax.JWTransformationPurchaseOrderId  = JWTPD.JobWorkTransformationContractMasterId and  jwtax.JWTransformationPurchaseOrderDetailId  = JWTPD.Id 
                            left join hkp.MaterialStorage MS ON MS.Id=JWTPD.MaterialLocationId
                            WHERE " + strkey + " ";

            return sql;
        }

        public string GetList(string column, string value)
        {

            string strkey = "1 =1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"SELECT JWTM.Id, JWTM.Sequence, JWTM.JWActivityId, JWA.UserName JWActivity, JWTM.ResponsiblePersonId,ISNULL(JWTM.ProcessId,'') ProcessId
                               , Process.UserName Process
                            ,EEI.EmployeeName ResponsiblePersonName, 
                            JWTM.OutputMaterialId, OUM.UserName OutputMaterial, JWTM.OutputMaterialUOMId,UOM.ShortName UOM, JWTM.RateApplicableOn, 
                            JWTM.CurrencyId, CURR.Code CURR, JWTM.MinRate, JWTM.MaxRate, 
                            JWTM.CycleTimeDays, JWTM.ByProductApplicable, JWTM.Remarks
                            FROM dbo.JWTransformationMaster JWTM
                            LEFT JOIN JWActivity JWA ON JWA.Id = JWTM.JWActivityId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWTM.ResponsiblePersonId
                            LEFT JOIN JWItem OUM ON OUM.Id = JWTM.OutputMaterialId
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM  oN UOM.Id = JWTM.OutputMaterialUOMId
                            LEFT JOIN [HKP].[Process] Process  oN Process.Id = JWTM.ProcessId

                            LEFT JOIN SCS.Currency CURR ON CURR.Id = JWTM.CurrencyId
                             WHERE " + strkey + " ";

            return sql;
        }


        public string GetJWTransformationPurchaseOrderServiceList(string jwpoId)
        {
            string strkey = "1=1";


            string sql = @" SELECT A.Id
                        , A.JWTransformationPurchaseOrderId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount
                        , POT.Amount-A.Amount AS  Bal
                        , POT.Amount As POAmount
                        --, A.TotalTaxAmount
                        ,A.POID
						,A.POServiceId,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].[JWTransformationPurchaseOrderService] AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                        left JOIN (select Id, Amount from TRN.POService) AS POT on A.POServiceId=POT.Id
                        left join ( Select JWTransformationPurchaseOrderId, sum(TaxAmount) TaxAmount FROM  JWTransformationPurchaseOrderTax group by ServiceMasterId where JWTransformationPurchaseOrderDetailId is null) IRT On IRT.JWTransformationPurchaseOrderId=A.Id
                        WHERE A.InventoryReceiveId='" + jwpoId + "'";

            return sql;

        }

        public string GetJWItemMAList(string ActivityId)
        {
            string strkey = "1 = 1";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //     string sql = @"SELECT  JWI.UserName JWItemName,JWI.Id JWItemId,JTM.Id  JWTransformationMasterId
            //,JTM.JWActivityId,JWA.UserName JWActivity,MM.UserName MaterialMaster,MM.Id MaterialMasterId
            //,MM.WithSKU, ISNULL(ART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
            //, hasInventory=CASE WHEN IM.Id<>'' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MM.IsOriginApplicable

            //, UOM.UserName UOM,UOM.Id UOMId
            //,JTM.ProcessId, Process.UserName Process, SM.Id ServiceId , SM.UserName ServiceName
            //FROM JWTransformationMaster  JTM 
            //LEFT JOIN JWActivity JWA On JWA.Id =JTM.JWActivityId
            //LEFT JOIN HkP.Process Process On Process.Id =JTM.ProcessId

            //LEFT JOIN JWItem JWI On JWI.Id =JTM.OutputMaterialId
            //                     LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id = JWI.MaterialMasterId
            //                     LEFT JOIN HKP.ServiceMaster SM ON SM.Id = JWA.ServiceId

            //                     LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JTM.ResponsiblePersonId
            //                    LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
            //                     , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
            //                     FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS ART ON ART.MaterialMasterId=MM.Id
            //                         LEFT JOIN TRN.InventoryMaterial AS IM ON IM.MaterialMasterId=MM.Id
            // LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id = JWI.UOMId  where JTM.JWActivityId IN(" + ActivityId + @")";
            string sql = @"SELECT  JWI.UserName JWItemName,JWI.Id JWItemId,JTM.Id  JWTransformationMasterId
		                ,JTM.JobWorkActivityId,JWA.UserName JWActivity,MM.UserName MaterialMaster,MM.Id MaterialMasterId
		                ,MM.WithSKU, ISNULL(ART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
		                , hasInventory=CASE WHEN IM.Id<>'' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MM.IsOriginApplicable

		                , UOM.UserName UOM,UOM.Id UOMId
		                ,Process.ProcessId, p.UserName Process, SM.Id ServiceId , SM.UserName ServiceName
		                FROM MST.JobWorkTransformationMaster  JTM 
		                LEFT JOIN HKP.JobWorkActivity JWA On JWA.Id =JTM.JobWorkActivityId
		
		                LEFT JOIN MSt.JobWorkTransformationMasterProcess Process ON Process.JobWorkTransformationMasterId=JTM.Id
		                LEFT JOIN [HKP].[Process] p on P.Id=Process.ProcessId

		                LEFT JOIN HKP.JobWorkItem JWI On JWI.Id =JTM.JobWorkActivityChildId
                        LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id = JWI.MaterialMasterId
                        LEFT JOIN HKP.ServiceMaster SM ON SM.Id = JTM.ServiceId

                        LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JTM.ResponsiblePersonId
                        LEFT JOIN(SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
                                  ,HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                                   FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId
				                  )AS ART ON ART.MaterialMasterId=MM.Id
		                LEFT JOIN TRN.InventoryMaterial AS IM ON IM.MaterialMasterId=MM.Id
		                LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id = JWI.UOMId  
		                where JTM.JobWorkActivityId IN(" + ActivityId + @")";
            return sql;
        }


        public string GetJWItemList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"SELECT [isToBeSelect] = Convert(bit, 'False'),  JWI.Id,JWI.MaterialMasterId,JWI.ResponsiblePersonId,JWI.UOMId UOMId,JWI.Code
                            ,JWI.Sequence,JWI.ShortName,JWI.StandardName,JWI.UserName,JWI.Remarks,MM.UserName MaterialMaster
                            ,UOM.ShortName UOM,EEI.EmployeeName ResponsiblePersonName FROM JWItem JWI 
                            LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id = JWI.MaterialMasterId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWI.ResponsiblePersonId
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id = JWI.UOMId WHERE " + strkey + " order by JWI.sequence";

            return sql;
        }

    
        public IEnumerable<object> GetJWServiceTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN JWTransformationPurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN JWTransformationPurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, HP.[Percentage] AS [Percentage], NULL TaxAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public string GetServiceChargeList(string jwpoId)
        {
            var sql = @"SELECT A.Id, A.JWTransformationPurchaseOrderId  InventoryReceiveId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.TransactionAmount
                            --, A.TotalTaxAmount
                            ,POT.TaxAmount As TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM 
                           JWTransformationPurchaseOrderService AS A 
                            INner JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (SELECT ServiceMasterId,Sum(TaxAmount) as TaxAmount  from JWTransformationPurchaseOrderTax 
                            WHERE  ISNULL(JWTransformationPurchaseOrderId,'') ='" + jwpoId + @"' group by ServiceMasterId
                            ) AS POT on A.ServiceMasterId=POT.ServiceMasterId
                            WHERE A.JWTransformationPurchaseOrderId='" + jwpoId + @"' ";

            return sql;//.Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public string GetPODetailServiceChargeList(string jwpoId, string jwpodId)
        {
            var sql = @"SELECT A.Id, A.JWTransformationPurchaseOrderId  InventoryReceiveId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.TransactionAmount
                            --, A.TotalTaxAmount
                            ,POT.TaxAmount As TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM 
                           JWTransformationPurchaseOrderService AS A 
                            INner JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (select ServiceMasterId InventoryServiceId,Sum(TaxAmount) as TaxAmount  from JWTransformationPurchaseOrderTax group by ServiceMasterId 
                            where ISNULL(JWTransformationPurchaseOrderDetailId,'') = '" + jwpodId + @"' and ISNULL(JWTransformationPurchaseOrderId,'') ='" + jwpoId + @"' 
                             ) AS POT on A.id=POT.InventoryServiceId
                            WHERE A.JWTransformationPurchaseOrderId='" + jwpoId + @"'";

            return sql;
        }

        public string GetServiceTaxList(string serviceId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.JWTransformationPurchaseOrderId,A.JWTransformationPurchaseOrderDetailId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM JWTransformationPurchaseOrderTax AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.JWTransformationPurchaseOrderId='" + serviceId + @"' AND A.JWTransformationPurchaseOrderDetailId IS NULL ORDER BY TC.[Sequence]";
                return sql;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public string GetPODetailTaxList(string jwPOId, string jwPoDetailId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.JWTransformationPurchaseOrderId,A.JWTransformationPurchaseOrderDetailId, A.TaxCategoryId, TC.UserName, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM JWTransformationPurchaseOrderTax AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.JWTransformationPurchaseOrderId='" + jwPOId + @"' AND A.JWTransformationPurchaseOrderDetailId = '" + jwPoDetailId + @"' ORDER BY TC.[Sequence]";
                return sql;

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private string MakeKey(DataRow dr)
        {
            StringCollection strCol = new StringCollection();
            strCol.Add("MaterialMasterId");
            strCol.Add("ArticleId");
            strCol.Add("FirstCharacteristicsValueId");
            strCol.Add("SecondCharacteristicsValueId");
            strCol.Add("ThirdCharacteristicsValueId");

            string _key = "";
            for (int i = 0; i < strCol.Count; i++)
            {
                _key += "-" + dr[strCol[i].ToString()];
            }

            return _key;
        }
        private string MakeKey(Dictionary<string, object> dr)
        {
            try
            {
                StringCollection strCol = new StringCollection();
                strCol.Add("MaterialMasterId");
                strCol.Add("ArticleId");
                strCol.Add("FirstCharacteristicsValueId");
                strCol.Add("SecondCharacteristicsValueId");
                strCol.Add("ThirdCharacteristicsValueId");

                string _key = "";
                for (int i = 0; i < strCol.Count; i++)
                {
                    _key += "-" + bplib.clsWebLib.RetValidLen(dr[strCol[i].ToString()]).ToString();
                }

                return _key;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<Dictionary<string, object>> MakePodetail(List<Dictionary<string, object>> data)
        {
            List<Dictionary<string, object>> dataInserted = new List<Dictionary<string, object>>();

            StringCollection strKey = new StringCollection();
            string referrenceNo = "";
            for (int i = 0; i < data.Count; i++)
            {
                //dataInserted.Add(data[i]);
                string CurrentKey = MakeKey(data[i]);

                if (strKey.Contains(CurrentKey) == true)
                    continue;
                strKey.Add(CurrentKey);

                dataInserted.Add(new Dictionary<string, object>(data[i]));
                dataInserted[dataInserted.Count - 1]["TransactionQty"] = 0;
                dataInserted[dataInserted.Count - 1]["ReferenceNoM"] = "";
                referrenceNo = "";
                for (int KK = 0; KK < dataInserted.Count; KK++)
                {
                    referrenceNo = "";
                    if (CurrentKey == MakeKey(dataInserted[KK]))
                    {
                        StringCollection strTemp = new StringCollection();
                        for (int M = 0; M < data.Count; M++)
                        {
                            if (CurrentKey == MakeKey(data[M]))
                            {
                                dataInserted[KK]["TransactionQty"] = clsStaticInfo.dbl(dataInserted[KK]["TransactionQty"]) + clsStaticInfo.dbl(data[M]["TransactionQty"]);

                                if (data[M].ContainsKey("OwnItemReferenceNo"))
                                {
                                    if (strTemp.Contains(data[M]["OwnItemReferenceNo"].ToString()) == false)
                                    {
                                        strTemp.Add(data[M]["OwnItemReferenceNo"].ToString());
                                        if (dataInserted[KK]["ReferenceNoM"].ToString() == "")
                                            dataInserted[KK]["ReferenceNoM"] = data[M]["OwnItemReferenceNo"].ToString();
                                        else
                                            dataInserted[KK]["ReferenceNoM"] += "," + data[M]["OwnItemReferenceNo"].ToString();
                                    }
                                }

                            }
                        }
                    }
                }
            }
            return dataInserted;
        }


        public IEnumerable<object> GetJWPODTChildMaterials(Dictionary<string, object> data)
        {
            try
            {
                string wc = "AND 1 = 1";

                if (data["MaterialId"] != null)
                {
                    wc += "AND MM.Id = '" + data["MaterialId"].ToString() + @"'";
                }
                if (data["ArticleId"] != null && data["ArticleId"].ToString() != "")
                {
                    wc += "AND MMA.Id = '" + data["ArticleId"].ToString() + @"'";
                }
                if (data["FirstCharacteristicsValueID"] != null && data["FirstCharacteristicsValueID"].ToString() != "")
                {
                    wc += "AND v1.Id = '" + data["FirstCharacteristicsValueID"].ToString() + @"'";
                }
                if (data["SecondCharacteristicsValueId"] != null && data["SecondCharacteristicsValueId"].ToString() != "")
                {
                    wc += "AND v2.Id = '" + data["SecondCharacteristicsValueId"].ToString() + @"'";
                }
                if (data["ThirdCharacteristicsValueId"] != null && data["ThirdCharacteristicsValueId"].ToString() != "")
                {
                    wc += "AND v3.Id = '" + data["ThirdCharacteristicsValueId"].ToString() + @"'";
                }
                var sql = "";
                sql = @"  SELECT JWPOBOQMAP.TransactionQty ParentQty,JWPOBOQMAP.TransactionUoMId,JWTPODUOM.UserName ParentUoM,JWTPOD.TransactionRate,JWTPOD.TransactionAmount, JWTPOCM.JWPODetailId ,JWTPOCM.BOQDetailId ,MM.UserName Material,MMA.StandardName Article
                            ,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
                            ,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
                            ,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue
                            , BOQ.Consumption , BOQ.WastagePer
                            ,JWTPOCM.TransactionQty ,JWTPOCM.TransactionUoMId , TRUOM.UserName TRUOM ,JWTPOCM.BaseQty
                            ,JWTPOCM.BaseUoMId ,BSUOM.UserName BSUOM ,JWTPOCM.POBOQQty ,JWTPOCM.POUoMId ,POUOM.UserName POUOM
                            
                            FROM [dbo].[JWTransformationPurchaseOrderChildMaterial] JWTPOCM
                            LEFT JOIN SCS.UnitOfMeasurement TRUOM ON JWTPOCM.TransactionUoMId = TRUOM.Id
                            LEFT JOIN SCS.UnitOfMeasurement BSUOM ON JWTPOCM.BaseUoMId = BSUOM.Id
                            LEFT JOIN SCS.UnitOfMeasurement POUOM ON JWTPOCM.POUoMId = POUOM.Id
                            LEFT JOIN BOQ AS bT ON bT.Id=JWTPOCM.BOQDetailId
                            LEFT JOIN JWPOBOQMAP ON JWPOBOQMAP.JWPODetailId = JWTPOCM.JWPODetailId and bT.ParentId = JWPOBOQMAP.BOQDetailId
                            Left join JWTransformationPurchaseOrderDetail JWTPOD ON JWTPOD.Id =JWTPOCM.JWPODetailId
                            LEFT JOIN SCS.UnitOfMeasurement JWTPODUOM ON JWTPODUOM.Id = JWPOBOQMAP.TransactionUoMId
                            LEFT JOIN BOQ ON BOQ.Id = JWTPOCM.BOQDetailId
                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=BOQ.MaterialMasterId
                            LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=BOQ.ArticleId
                            LEFT JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=BOQ.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=BOQ.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=BOQ.ThirdCharacteristicsValueId
                        
                          WHERE JWTPOCM.JWPODetailId = '" + data["JWPODetailId"] + @"' " + wc + @" ";//b.MaterialMasterId,


                var Data = _sqlRepository.GetDataCollection(sql);

                return Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetJWPODTChildMaterialsSummary(string JWPODId)
        {
            try
            {
                var sql = "";
                sql = @"   SELECT SUM(JWPOBOQMAP.TransactionQty) ParentQty,JWPOBOQMAP.TransactionUoMId,JWTPODUOM.UserName ParentUoM,JWTPOD.TransactionRate,JWTPOD.TransactionAmount, JWTPOCM.JWPODetailId  ,MM.UserName Material,MM.Id MaterialId,MMA.StandardName Article,MMA.Id  ArticleId
                            ,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
                            ,IsNULL(v1.Id,'') AS FirstCharacteristicsValueID
                            ,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
                            ,IsNULL(v2.Id,'') AS SecondCharacteristicsValueId
                            ,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue
                            ,IsNULL(v3.Id,'') AS ThirdCharacteristicsValueId

                            , BOQ.Consumption , BOQ.WastagePer
                            ,SUM(JWTPOCM.TransactionQty)TransactionQty ,JWTPOCM.TransactionUoMId , TRUOM.UserName TRUOM ,SUM(JWTPOCM.BaseQty)BaseQty
                            ,JWTPOCM.BaseUoMId ,BSUOM.UserName BSUOM ,SUM(JWTPOCM.POBOQQty) POBOQQty,JWTPOCM.POUoMId ,POUOM.UserName POUOM
                            
                            FROM [dbo].[JWTransformationPurchaseOrderChildMaterial] JWTPOCM
                            LEFT JOIN SCS.UnitOfMeasurement TRUOM ON JWTPOCM.TransactionUoMId = TRUOM.Id
                            LEFT JOIN SCS.UnitOfMeasurement BSUOM ON JWTPOCM.BaseUoMId = BSUOM.Id
                            LEFT JOIN SCS.UnitOfMeasurement POUOM ON JWTPOCM.POUoMId = POUOM.Id
                            LEFT JOIN BOQ AS bT ON bT.Id=JWTPOCM.BOQDetailId
                            LEFT JOIN JWPOBOQMAP ON JWPOBOQMAP.JWPODetailId = JWTPOCM.JWPODetailId and bT.ParentId = JWPOBOQMAP.BOQDetailId
                            Left join JWTransformationPurchaseOrderDetail JWTPOD ON JWTPOD.Id =JWTPOCM.JWPODetailId
                            LEFT JOIN SCS.UnitOfMeasurement JWTPODUOM ON JWTPODUOM.Id = JWPOBOQMAP.TransactionUoMId
                            LEFT JOIN BOQ ON BOQ.Id = JWTPOCM.BOQDetailId
                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=BOQ.MaterialMasterId
                            LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=BOQ.ArticleId
                            LEFT JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=BOQ.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=BOQ.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=BOQ.ThirdCharacteristicsValueId
							where JWTPOCM.JWPODetailId = '" + JWPODId + @"'

							Group by JWPOBOQMAP.TransactionUoMId,JWTPODUOM.UserName ,JWTPOD.TransactionRate,JWTPOD.TransactionAmount, JWTPOCM.JWPODetailId  ,MM.UserName ,MMA.StandardName 
                            ,v1.UserName,v1.Id ,v2.UserName,v2.Id,v3.UserName,v3.Id,mm.Id,mma.Id, BOQ.Consumption , BOQ.WastagePer
                            ,JWTPOCM.TransactionUoMId , TRUOM.UserName  ,JWTPOCM.BaseUoMId ,BSUOM.UserName  ,JWTPOCM.POUoMId ,POUOM.UserName  ";


                var Data = _sqlRepository.GetDataCollection(sql);

                return Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public IEnumerable<object> GetJWPOActivityService(string JWPODId)
        {
            try
            {
                var sql = "";
                sql = @" select Distinct SM.UserName ServiceName,SM.Id from JWTransformationPOActivity JWPA 
							 left join JWActivity JWA ON JWA.Id = JWPA.JWActivityId
							 left join HKP.ServiceMaster SM ON JWA.ServiceId = SM.Id
							 where JWPA.JWTransformationPurchaseOrderId = '" + JWPODId + @"' ";

                var Data = _sqlRepository.GetDataCollection(sql);

                return Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllEntity(string PlantId)
        {
            try
            {
                var sql = "";
                sql = @" select Id as Value, UserName as Text from ORG.Entity where PlantId='" + PlantId + "' order by UserName ";

                var Data = _sqlRepository.GetDataCollection(sql);

                return Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMaterialfromJW(string JobWorkItemId)
        {
            try
            {
                var sql = "";
                sql = @"select mm.Id, mm.Code, mm.UserName as Material,mm.BaseUOMId, mmuom.UserName as BaseUom,jwi.UOMId, uom.UserName as JWIUom
                     ,UnitId=case when jwi.MaterialMasterId is not null then mm.BaseUOMId else jwi.UOMId End
                     from HKP.JobWorkItem jwi left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
                     left join scs.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
					 left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                     where jwi.Id='" + JobWorkItemId + @"' ";

                var Data = _sqlRepository.GetDataCollection(sql);

                return Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> LoadInputArticle(string MaterialMstId)
        {
            try
            {
                var sql = "";
                sql = @"Select mm.Code as MaterialCode,mm.UserName as Material,mgm.UserName as MaterialGroupMaster,mma.Id as ArticleId ,mma.Code as ArticleCode, mma.ShortName, mma.StandardName 
                           from MST.MaterialMasterArticle mma left join MST.MaterialMaster mm on mma.MaterialMasterId=mm.Id
                           left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
                            where mm.Id='" + MaterialMstId + @"'
                            order by mm.Code ";

                var Data = _sqlRepository.GetDataCollection(sql);

                return Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}
