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

using Library.Core;
using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.Service.Helpers;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Inventory;
using Library.ViewModel.Setup;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Library.MaterialManagement.Inventory;

namespace Library.MaterialManagement.JobWork
{
    public class OSCommon
    {
        private readonly SqlRepository _sqlRepository = new SqlRepository();
        string TableName = "OSTransformationPO";

        public OSCommon()
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
									,isnull(PLC.LCRef,'') as PurchaseLC
									--,Par1.UserName Customer
									,IR.CheckedByStatus AS CheckedByStatus
									,IR.AuthorizedByStatus AS AuthorizedByStatus
                                  ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
									,IR.ContractId
									,IR.OrderSpecific,IR.ProcessId
									--,IR.PurchaseLCId
									,Par.UserName CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
						,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
						FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
						left join dbo.PurchaseLC PLC on PLC.Id=IR.PurchaseLCId
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						WHERE  IR.PlantId='" + plantId + @"' 
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
									,isnull(PLC.LCRef,'') as PurchaseLC
									--,Par1.UserName Customer
									,IR.CheckedByStatus AS CheckedByStatus
									,IR.AuthorizedByStatus AS AuthorizedByStatus
                             ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
									,IR.ContractId
									,IR.OrderSpecific,IR.ProcessId
									--,IR.PurchaseLCId
									,Par.UserName CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
						,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
						FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
						left join dbo.PurchaseLC PLC on PLC.Id=IR.PurchaseLCId
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
						AND IR.CheckedByStatus IS NULL 
						AND IR.AuthorizedByStatus IS NULL						
						 And IR.PlantId='" + plantId + @"' 
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
									,isnull(PLC.LCRef,'') as PurchaseLC
									--,Par1.UserName Customer
									,IR.CheckedByStatus AS CheckedByStatus
									,IR.AuthorizedByStatus AS AuthorizedByStatus
                                   ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
									,IR.ContractId
									,IR.OrderSpecific,IR.ProcessId
									--,IR.PurchaseLCId
									,Par.UserName CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
						,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
						FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
						left join dbo.PurchaseLC PLC on PLC.Id=IR.PurchaseLCId
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + plantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						) x
						--Order by PODate DESC
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.OSTransformationPO) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc";
                }
                else if (POTypeStatus == "CheckedHoldRej")
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
						,ISNULL(Cn.ContractNo,'') ContractNo
									, ISNULL(MLC.Id,'') MasterLCNo
							        ,ISNULL(MLC.LCRef,'') LCRef
									,isnull(PLC.LCRef,'') as PurchaseLC
									,Par.UserName CustomerName
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                           ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific,IR.ProcessId
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                        ,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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

					--	LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						left join dbo.PurchaseLC PLC on PLC.Id=IR.PurchaseLCId
                        LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
		                            JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
                        LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.OSTransformationPO) BD ON BD.Id=IR.Id 
                        WHERE  IR.PlantId='" + plantId + @"' AND IR.CheckedBy IS NOT NULL AND IR.AuthorizedBy IS NOT NULL AND IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' 
                        --AND IR.POType='OSTransformationPO' 
                        AND IR.PlantId='" + plantId + @"'   AND isnull(IR.IsClosed,0)=0 
                        --Order by IR.PODate DESC
                         ORDER BY CONVERT(int,Col) desc";//IR.AddedBy='" + identity.Name + "' And

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

									,ISNULL(Cn.ContractNo,'') ContractNo
									,isnull(PLC.LCRef,'') as PurchaseLC
									,Par.UserName CustomerName

                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI.EmployeeName CheckedBy
									,eI1 .EmployeeName ApprovedBy
                                    ,IR.ContractId
									,IR.OrderSpecific,IR.ProcessId
									--,IR.PurchaseLCId
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        ,IR.DeliveryInstruction,IR.SpecialInstruction
                         ,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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

						--	LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                   --     LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cn.MasterLCId
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						left join dbo.PurchaseLC PLC on PLC.Id=IR.PurchaseLCId

                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
		                            JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
                        LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
                         JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.OSTransformationPO) BD ON BD.Id=IR.Id
                         WHERE IR.PlantId='" + plantId + @"' 
                         AND IR.CheckedBy IS NOT NULL 
                         AND IR.AuthorizedBy IS NOT NULL  
                         AND IR.CheckedByStatus='Checked' 
                         AND IR.AuthorizedByStatus='For Approval'  
                         --AND IR.POType='OSTransformationPO'  		
                         AND ISNULL(IR.IsClosed,0)=0 
                         --Order by IR.PODate DESC
                           ORDER BY CONVERT(int,Col) desc ";


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
                         ,IR.EntityId,E.UserName as Entity,CONVERT(varchar(5),FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
		                            JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
                        LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE  
                                            --IR.POType='OSTransformationPO' AND 
                                            IR.PlantId='" + plantId + @"' 
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
                         ,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
		                            JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
                        LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE IR.PlantId='" + plantId + @"' 
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
                         ,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
		                            JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
                        LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE IR.PlantId='" + plantId + @"' 
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
                         ,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
		                            JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
                        LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
											WHERE IR.PlantId='" + plantId + @"' 
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
                         ,IR.EntityId,E.UserName as Entity,FORMAT(IR.ProcessStartDate,'dd-MMM-yyyy') as TConProcessStartDate,
                        FORMAT(IR.ProcessEndDate,'dd-MMM-yyyy') as TConProcessEndDate,FORMAT(IR.ContractClosingDate,'dd-MMM-yyyy') as TConContractClosingDate
						,IR.ContractStatus, IR.Remarks,IR.POType
                        FROM OSTransformationPO AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
		                            JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
                        LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        left join ORG.Entity E on E.Id=IR.EntityId
                        WHERE IR.PlantId='" + plantId + @"'  AND IR.CheckedBy IS NOT NULL AND IR.CheckedByStatus='Checked' 
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

        public IEnumerable<object> GetBOQItems(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId, string POType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataTable dtJWPODetail = new DataTable();
            if (!String.IsNullOrEmpty(JWPODId))
            {
                string strSql = "";

                //strSql = @"SELECT * FROM OSTransformationPODetail WHERE OSTransformationPOId = '" + JWPOId + @"'";
                strSql = @"SELECT * FROM dbo.OSTransformationPODetail WHERE OSTransformationPOId = '" + JWPOId + @"'";

                dtJWPODetail = _sqlRepository.GetDataTable(strSql);

            }

            if (IsOwnVendor == "OwnVendor")
            {
                if (POType == "OSTransformationPO")
                {
                    try
                    {

                        var sql = "";
                        sql = @"SELECT NULL AS uoMList, NULL as BOQserviceCboList, b.Id BOQId,b.Sequence Sequence1
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
						,Isnull(POMAP.TransactionQty,0) PORaisedQry--,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
                        ,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal
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
						,mm.BaseUOMId--,BalanceQuantity=b.RequiredQtyPO - ISNULL(OtherPOData.TransactionQty,0)
                        ,BalanceQuantity=b.RequiredQtyPO - ISNULL(kk.OtherPOQuantity,0)
                        ,TransactionQty=b.RequiredQtyPO - ISNULL(kk.OtherPOQuantity,0)
                        ,ISNULL(kk.OtherPOQuantity,'0') as OtherPOQty
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
									FROM OSPOBOQMAP POBOQMAP1
									LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.OSTransformationPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									WHERE POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM OSPOBOQMAP POBOQMAP1
									LEFT JOIN  dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.OSTransformationPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									WHERE POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
                                    left join (select Sum(boqmap.TransactionQty) as OtherPOQuantity,B.MaterialMasterId,B.ArticleId,SO.Id as SalesOrderId,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                                          ,B.ThirdCharacteristicsValueId,boqmap.BOQDetailId
                                          from dbo.OSPOBOQMAP boqmap left join dbo.OSTransformationPODetail om on om.Id=boqmap.OSTransformationPODetailId
                                          left join dbo.BOQ B on B.Id=boqmap.BOQDetailId
                                          left join trn.SalesOrder SO on SO.Id=B.SalesOrderId
                                          left join dbo.OSTransformationPO po on po.Id=om.OSTransformationPOId
										  where po.POType='OSTransformationPO'
                                          group by B.MaterialMasterId,B.ArticleId,SO.Id,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                                          ,B.ThirdCharacteristicsValueId,boqmap.BOQDetailId)
										  kk on kk.SalesOrderId=so.Id and kk.BOQDetailId=b.Id
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

                        var JWServiceList = _sqlRepository.GetDataCollection(@"select Id as Value, UserName as Text from HKP.ServiceMaster order by UserName");
                        for (int i = 0; i < Data.Count; i++)
                        {
                            Data[i]["BOQserviceCboList"] = JWServiceList;
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
                else
                {
                    try
                    {



                        var sql = "";
                        sql = @"SELECT NULL AS uoMList, NULL as BOQserviceCboList, b.Id BOQId,b.Sequence Sequence1
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
						,Isnull(POMAP.TransactionQty,0) PORaisedQry--,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
                        ,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal
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
						,mm.BaseUOMId--,BalanceQuantity=b.RequiredQtyPO - ISNULL(OtherPOData.TransactionQty,0)
                        ,BalanceQuantity=b.RequiredQtyPO - ISNULL(kk.OtherPOQuantity,0)
                        ,TransactionQty=b.RequiredQtyPO - ISNULL(kk.OtherPOQuantity,0)
                        ,ISNULL(kk.OtherPOQuantity,'0') as OtherPOQty
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
									FROM OSPOBOQMAP POBOQMAP1
									LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.OSTransformationPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									WHERE POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM OSPOBOQMAP POBOQMAP1
									LEFT JOIN  dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.OSTransformationPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									WHERE POM.Id !='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
                                    left join (select Sum(boqmap.TransactionQty) as OtherPOQuantity,B.MaterialMasterId,B.ArticleId,SO.Id as SalesOrderId,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                                          ,B.ThirdCharacteristicsValueId,boqmap.BOQDetailId
                                          from dbo.OSPOBOQMAP boqmap left join dbo.OSTransformationPODetail om on om.Id=boqmap.OSTransformationPODetailId
                                          left join dbo.BOQ B on B.Id=boqmap.BOQDetailId
                                          left join trn.SalesOrder SO on SO.Id=B.SalesOrderId
                                          left join dbo.OSTransformationPO po on po.Id=om.OSTransformationPOId
										  where po.POType='OSValueAddedPO'
                                          group by B.MaterialMasterId,B.ArticleId,SO.Id,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                                          ,B.ThirdCharacteristicsValueId,boqmap.BOQDetailId)
										  kk on kk.SalesOrderId=so.Id and kk.BOQDetailId=b.Id
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

                        var JWServiceList = _sqlRepository.GetDataCollection(@"select Id as Value, UserName as Text from HKP.ServiceMaster order by UserName");
                        for (int i = 0; i < Data.Count; i++)
                        {
                            Data[i]["BOQserviceCboList"] = JWServiceList;
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
									--FROM [TRN].[POBOQMAP] POBOQMAP1
                                    FROM JWPOBOQMAP POBOQMAP1
									--LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									--LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
                                    LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									where POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									--FROM [TRN].[POBOQMAP] POBOQMAP1
                                    FROM JWPOBOQMAP POBOQMAP1
									--LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									--LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
                                    LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
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
									--FROM [TRN].[POBOQMAP] POBOQMAP1
									--LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									--LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
                                    FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									where POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									--FROM [TRN].[POBOQMAP] POBOQMAP1
									--LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									--LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
                                    FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
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


        public IEnumerable<object> GetBOQItemsForUpdate(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId, string MaterialId, string ArticleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataTable dtJWPODetail = new DataTable();
            if (!String.IsNullOrEmpty(JWPODId))
            {
                string strSql = "";

                //strSql = @"SELECT * FROM JWTransformationPurchaseOrderDetail WHERE OSTransformationPOId = '" + JWPOId + @"' AND Id = '" + JWPODId + @"'";
                strSql = @"SELECT* FROM dbo.OSTransformationPODetail WHERE OSTransformationPOId = '" + JWPOId + @"' AND Id = '" + JWPODId + @"'";

                dtJWPODetail = _sqlRepository.GetDataTable(strSql);

            }

            if (IsOwnVendor == "OwnVendor")
            {
                try
                {
                    var sql = "";
                    //              sql = @"SELECT NULL AS uoMList,NULL as BOQserviceCboList, b.Id BOQId,b.Sequence Sequence1
                    //,b.MasterOrderItemId
                    //,moi.MasterOrderId
                    //,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
                    //,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

                    //,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
                    //,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
                    //, b.VendorId
                    //,b.SalesOrderId
                    //,mm.Id MaterialMasterId,mma.Id ArticleId
                    //,IsNULL(mm.UserName,'') AS UserName
                    //,IsNULL(mma.StandardName,'') AS StandardName
                    //,IsNULL(p.UserName,'') AS Vendor
                    //,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
                    //,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
                    //,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

                    //,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
                    //,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
                    //,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
                    //,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
                    //,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
                    //,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                    //b.BOMQty--,C.Id
                    //,CONVERT(BIT,CASE WHEN ISNULL(JWPOBOQMAP.Id,'')<>'' THEN 1 ELSE 0 END) CheckedStatus ,NULL TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
                    //,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal,JWPOBOQMAP.TransactionQty
                    //,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
                    //,ISNULL(cpo.PONumber,'') PONumber
                    //   --,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
                    //--,uom1.UserName AlternateUOM
                    //,b.RequiredQty
                    //--,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
                    //,uom.UserName BOQUOM
                    //--,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
                    //,b.POUoMId FromPoUomId
                    //   ,b.POUoMId
                    //--,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
                    //,b.RequiredQtyPO 
                    //,b.RequiredQtyPO RequiredQtyPOOrginal
                    //,TransactionUoMId=CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
                    //,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'')+'-'+ISNULL(moi.BuyerReferenceNo,'')
                    //,mm.BaseUOMId,POMAP.RatePerUnit as TransactionRate, IsEditMode=1,POMAP.ServiceId
                    //FROM BOQ AS b
                    //LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                    //LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                    //LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                    //LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                    //LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                    //LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                    //LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
                    //left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

                    //LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                    //LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                    //LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

                    //LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
                    //LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
                    //LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

                    //LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
                    //--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
                    //LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,JWPPOD.Id,sum(POBOQMAP1.TransactionQty) TransactionQty,JWPPOD.RatePerUnit,JWPPOD.ServiceId
                    //					FROM JWPOBOQMAP POBOQMAP1
                    //			--LEFT JOIN JWTransformationPurchaseOrderDetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
                    //			--LEFT JOIN OSTransformationPO POM ON POM.Id=JWPPOD.OSTransformationPOId

                    //                              LEFT JOIN dbo.OSTransformationPODetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
                    //			LEFT JOIN OSTransformationPO POM ON POM.Id=JWPPOD.OSTransformationPOId
                    //			where POM.Id ='" + JWPOId + @"'
                    //			GROUP by POBOQMAP1.BOQDetailId,JWPPOD.Id,JWPPOD.RatePerUnit,JWPPOD.ServiceId					
                    //			)POMAP ON POMAP.BOQDetailId=b.Id
                    //LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,JWPPOD.Id,sum(POBOQMAP1.POBOQQty) TransactionQty,JWPPOD.RatePerUnit
                    //					FROM JWPOBOQMAP POBOQMAP1
                    //			--LEFT JOIN JWTransformationPurchaseOrderDetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
                    //			--LEFT JOIN OSTransformationPO POM ON POM.Id=JWPPOD.OSTransformationPOId
                    //                              LEFT JOIN OSTransformationPODetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
                    //			LEFT JOIN OSTransformationPO POM ON POM.Id=JWPPOD.OSTransformationPOId
                    //			where POM.Id !='" + JWPOId + @"'
                    //			GROUP by POBOQMAP1.BOQDetailId,JWPPOD.Id,JWPPOD.RatePerUnit
                    //		) OtherPOData ON OtherPOData.BOQDetailId=b.Id
                    //                  LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
                    //                  --LEFT JOIN JWPOBOQMAP JWPOBOQMAP ON JWPOBOQMAP.BOQDetailId=b.Id AND JWPOBOQMAP.JWPODetailId IN (select Id from JWTransformationPurchaseOrderDetail where OSTransformationPOId='" + JWPOId + @"')
                    //                    LEFT JOIN JWPOBOQMAP JWPOBOQMAP ON JWPOBOQMAP.BOQDetailId=b.Id AND JWPOBOQMAP.JWPODetailId IN (select Id from OSTransformationPODetail where OSTransformationPOId='" + JWPOId + @"')
                    //--LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
                    //WHERE moi.ContractId='" + ContractId + @"' AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null)
                    //  --AND  b.id in(select ParentId from BOQ where ISNULL(ParentId,'')<>'' and ProcessId IN (Select ProcessId from JWActivity where Id IN (" + jwActivityId + @"))) --and isChild=0
                    //                   AND  b.id in(select ParentId from BOQ where ISNULL(ParentId,'')<>'' 
                    //--and ProcessId IN (Select ProcessId from JWActivity where Id IN ('')) 
                    //) --and isChild=0
                    //and b.MaterialMasterId='"+ MaterialId + @"' and b.ArticleId='"+ ArticleId + @"'
                    //                  --and POMAP.Id='"+ JWPODId + @"'
                    //ORDER BY JWPOBOQMAP.BOQDetailId DESC, b.Sequence, b.SalesOrderId";//b.MaterialMasterId,

                    //              sql = @"SELECT NULL AS uoMList,NULL as BOQserviceCboList, b.Id BOQId,b.Sequence Sequence1
                    //,b.MasterOrderItemId
                    //,moi.MasterOrderId
                    //,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
                    //,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

                    //,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
                    //,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
                    //, b.VendorId
                    //,b.SalesOrderId
                    //,mm.Id MaterialMasterId,mma.Id ArticleId
                    //,IsNULL(mm.UserName,'') AS UserName
                    //,IsNULL(mma.StandardName,'') AS StandardName
                    //,IsNULL(p.UserName,'') AS Vendor
                    //,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
                    //,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
                    //,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

                    //,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
                    //,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
                    //,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
                    //,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
                    //,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
                    //,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                    //b.BOMQty--,C.Id
                    //,CONVERT(BIT,CASE WHEN ISNULL(JWPOBOQMAP.Id,'')<>'' THEN 1 ELSE 0 END) CheckedStatus ,NULL TaxList,MM.HSNCodeId	,MM.IsOriginApplicable

                    //,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
                    //,ISNULL(cpo.PONumber,'') PONumber
                    //,b.RequiredQty
                    //,uom.UserName BOQUOM
                    //,b.POUoMId FromPoUomId
                    //   ,b.POUoMId
                    //,b.RequiredQtyPO 
                    //,b.RequiredQtyPO RequiredQtyPOOrginal
                    //,TransactionUoMId=CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
                    //,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'')+'-'+ISNULL(moi.BuyerReferenceNo,'')
                    //,mm.BaseUOMId
                    //,POMAP.RatePerUnit as TransactionRate
                    //, IsEditMode=1
                    //,POMAP.ServiceId
                    //,Sum(Isnull(POMAP.TransactionQty,0)) PORaisedQry
                    //--,Sum(ISNULL(OtherPOData.TransactionQty,0)) OtherPOQty
                    //--,Sum(ISNULL(OtherPOData.TransactionQty,0)) OtherPOQtyOrginal
                    //--,Sum(JWPOBOQMAP.TransactionQty) TransactionQty
                    //                  ,POMAP.TransactionQty--,BalanceQuantity=b.RequiredQtyPO - ISNULL(OtherPOData.TransactionQty,0)
                    //                  ,BalanceQuantity=b.RequiredQtyPO - ISNULL(kk.OtherPOQuantity,0)
                    //,ISNULL(kk.OtherPOQuantity,'0') as OtherPOQty
                    //                  ,ISNULL(kk.OtherPOQuantity,'0') as OtherPOQtyOrginal
                    //FROM BOQ AS b
                    //LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                    //LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                    //LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                    //LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                    //LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                    //LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                    //LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
                    //left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

                    //LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                    //LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                    //LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

                    //LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
                    //LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
                    //LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

                    //LEFT JOIN [dbo].[Contract] C ON C.Id=moi.ContractId
                    //LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,JWPPOD.Id,sum(POBOQMAP1.TransactionQty) TransactionQty,JWPPOD.RatePerUnit,JWPPOD.ServiceId
                    //					FROM JWPOBOQMAP POBOQMAP1

                    //                              LEFT JOIN dbo.OSTransformationPODetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
                    //			LEFT JOIN OSTransformationPO POM ON POM.Id=JWPPOD.OSTransformationPOId
                    //			where POM.Id ='" + JWPOId + @"'
                    //			GROUP by POBOQMAP1.BOQDetailId,JWPPOD.Id,JWPPOD.RatePerUnit,JWPPOD.ServiceId					
                    //			)POMAP ON POMAP.BOQDetailId=b.Id
                    //LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,JWPPOD.Id,sum(POBOQMAP1.POBOQQty) TransactionQty,JWPPOD.RatePerUnit
                    //					FROM JWPOBOQMAP POBOQMAP1
                    //                              LEFT JOIN OSTransformationPODetail JWPPOD ON JWPPOD.Id=POBOQMAP1.JWPODetailId
                    //			LEFT JOIN OSTransformationPO POM ON POM.Id=JWPPOD.OSTransformationPOId
                    //			where POM.Id !='"+ JWPOId + @"'
                    //			GROUP by POBOQMAP1.BOQDetailId,JWPPOD.Id,JWPPOD.RatePerUnit
                    //		) OtherPOData ON OtherPOData.BOQDetailId=b.Id
                    //                                    left join (select Sum(boqmap.TransactionQty) as OtherPOQuantity,B.MaterialMasterId,B.ArticleId,SO.Id as SalesOrderId,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                    //                                    ,B.ThirdCharacteristicsValueId
                    //                                    from dbo.JWPOBOQMAP boqmap --left join dbo.OSTransformationPODetail om on om.Id=boqmap.JWPODetailId
                    //                                    left join dbo.BOQ B on B.Id=boqmap.BOQDetailId
                    //                                    left join trn.SalesOrder SO on SO.Id=B.SalesOrderId
                    //                                    group by B.MaterialMasterId,B.ArticleId,SO.Id,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                    //                                    ,B.ThirdCharacteristicsValueId)
                    //				  kk on kk.SalesOrderId=so.Id
                    //                  LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
                    //                    LEFT JOIN JWPOBOQMAP JWPOBOQMAP ON JWPOBOQMAP.BOQDetailId=b.Id AND JWPOBOQMAP.JWPODetailId IN (select Id from OSTransformationPODetail where OSTransformationPOId='" + JWPOId + @"')
                    //WHERE moi.ContractId='"+ ContractId + @"' AND (b.VendorId='"+ VendorId + @"' OR b.VendorId is null)
                    //                   AND  b.id in(select ParentId from BOQ where ISNULL(ParentId,'')<>'' 
                    //) 
                    //and b.MaterialMasterId='"+ MaterialId + @"' and b.ArticleId='"+ ArticleId + @"'
                    //                  --and POMAP.Id='JWPD49'
                    //--ORDER BY JWPOBOQMAP.BOQDetailId DESC, b.Sequence, b.SalesOrderId
                    //group by
                    // b.Id,b.Sequence 
                    //,b.MasterOrderItemId
                    //,moi.MasterOrderId
                    //,ISNULL(mo.OwnReferenceNo,'') 
                    //,ISNULL(mo.BuyerReferenceNo,'') 

                    //,ISNULL(moi.OwnReferenceNo,'') 
                    //,ISNULL(moi.BuyerReferenceNo,'') 
                    //, b.VendorId
                    //,b.SalesOrderId
                    //,mm.Id 
                    //,mma.Id 
                    //,IsNULL(mm.UserName,'')  
                    //,IsNULL(mma.StandardName,'')  
                    //,IsNULL(p.UserName,'')  
                    //,IsNULL(v1.UserName,'')  
                    //,IsNULL(v2.UserName,'')  
                    //,IsNULL(v3.UserName,'')  

                    //,b.FirstCharacteristicsValueId,FC.Id 
                    //,b.SecondCharacteristicsValueId,SC.Id 
                    //,b.ThirdCharacteristicsValueId,TC.Id 
                    //,b.RequiredQtyApproved
                    //,b.IncompleteMaterial
                    //,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                    //b.BOMQty--,C.Id
                    //,JWPOBOQMAP.Id  ,MM.HSNCodeId	,MM.IsOriginApplicable

                    //,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-')   
                    //,ISNULL(cpo.PONumber,'') 
                    //,b.RequiredQty
                    //,uom.UserName 
                    //,b.POUoMId 
                    //   ,b.POUoMId
                    //,b.RequiredQtyPO 
                    //,b.RequiredQtyPO 
                    //,b.POUoMId
                    //,mo.OwnReferenceNo
                    //,mm.BaseUOMId,b.POUoMId,b.UoMId
                    //,POMAP.RatePerUnit  

                    //,POMAP.ServiceId
                    //                  ,POMAP.TransactionQty
                    //                  --,OtherPOData.TransactionQty
                    //                  ,kk.OtherPOQuantity";

                    sql = @"SELECT NULL AS uoMList,NULL as BOQserviceCboList, b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo, '') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo, '') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo, '') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo, '') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId, mma.Id ArticleId
                         , IsNULL(mm.UserName, '') AS UserName
                           , IsNULL(mma.StandardName, '') AS StandardName
                             , IsNULL(p.UserName, '') AS Vendor
                               , IsNULL(v1.UserName, '') AS FirstCharacteristicsValue
                                 , IsNULL(v2.UserName, '') AS SecondCharacteristicsValue
                                   , IsNULL(v3.UserName, '') AS ThirdCharacteristicsValue

                                     , b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
                                      , b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
                                       , b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
                                        , RequiredQtyApproved = Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))= 0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial = CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))= 1 THEN 'Yes' ELSE 'No' END
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty--,C.Id
						,CONVERT(BIT, CASE WHEN ISNULL(JWPOBOQMAP.Id, '') <> '' THEN 1 ELSE 0 END) CheckedStatus ,NULL TaxList, MM.HSNCodeId ,MM.IsOriginApplicable
						
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                          , ISNULL(cpo.PONumber, '') PONumber
						,b.RequiredQty
						,uom.UserName BOQUOM
                        , b.POUoMId FromPoUomId
                         , b.POUoMId
						,b.RequiredQtyPO
						,b.RequiredQtyPO RequiredQtyPOOrginal
                        , TransactionUoMId = CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
                           , RefferenceNo = ISNULL(mo.OwnReferenceNo, '') + '-' + ISNULL(mo.BuyerReferenceNo, '') + '-' + ISNULL(moi.OwnReferenceNo, '') + '-' + ISNULL(moi.BuyerReferenceNo, '')
                           , mm.BaseUOMId
                            --,POMAP.RatePerUnit as TransactionRate
						, IsEditMode = 1
                        --,POMAP.ServiceId
						,Sum(Isnull(POMAP.TransactionQty, 0)) PORaisedQry
                         --,Sum(ISNULL(OtherPOData.TransactionQty, 0)) OtherPOQty
                          --,Sum(ISNULL(OtherPOData.TransactionQty, 0)) OtherPOQtyOrginal
                           --,Sum(JWPOBOQMAP.TransactionQty) TransactionQty
                        ,POMAP.TransactionQty--,BalanceQuantity = b.RequiredQtyPO - ISNULL(OtherPOData.TransactionQty, 0)
                        ,BalanceQuantity = b.RequiredQtyPO - ISNULL(kk.OtherPOQuantity, 0)
						,ISNULL(kk.OtherPOQuantity, '0') as OtherPOQty
                        ,ISNULL(kk.OtherPOQuantity, '0') as OtherPOQtyOrginal
						,OtM.RatePerUnit as TransactionRate,OtM.ServiceId
                        FROM BOQ AS b
                        LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id = b.MaterialMasterId

                        LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id = b.ArticleId

                        LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id = b.UoMId

                        LEFT OUTER JOIN HKP.Party P ON p.Id = b.VendorId

                        LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id = b.SalesOrderId

                        LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id = b.MasterOrderItemId

                        LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id = moi.MasterOrderId

                        left outer join[TRN].[CustomerPO] cpo On cpo.Id = so.CustomerPOId


                        LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = b.FirstCharacteristicsValueId

                        LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = b.SecondCharacteristicsValueId

                        LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = b.ThirdCharacteristicsValueId


                        LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId

                        LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId

                        LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId


                        LEFT JOIN[dbo].[Contract] C ON C.Id = moi.ContractId

                        LEFT JOIN(SELECT POBOQMAP1.BOQDetailId--,JWPPOD.Id
						,sum(POBOQMAP1.TransactionQty) TransactionQty--,JWPPOD.RatePerUnit,JWPPOD.ServiceId
                                            FROM JWPOBOQMAP POBOQMAP1

                                    LEFT JOIN dbo.OSTransformationPODetail JWPPOD ON JWPPOD.Id = POBOQMAP1.JWPODetailId

                                    LEFT JOIN OSTransformationPO POM ON POM.Id = JWPPOD.OSTransformationPOId

                                    where POM.Id = '" + JWPOId + @"'

                                    GROUP by POBOQMAP1.BOQDetailId--,JWPPOD.Id,JWPPOD.RatePerUnit,JWPPOD.ServiceId
									)POMAP ON POMAP.BOQDetailId = b.Id

                        LEFT JOIN(SELECT POBOQMAP1.BOQDetailId--,JWPPOD.Id
						,sum(POBOQMAP1.POBOQQty) TransactionQty--,JWPPOD.RatePerUnit
                                            FROM JWPOBOQMAP POBOQMAP1
                                    LEFT JOIN OSTransformationPODetail JWPPOD ON JWPPOD.Id = POBOQMAP1.JWPODetailId

                                    LEFT JOIN OSTransformationPO POM ON POM.Id = JWPPOD.OSTransformationPOId

                                    where POM.Id != '" + JWPOId + @"'

                                    GROUP by POBOQMAP1.BOQDetailId--,JWPPOD.Id
                                    --,JWPPOD.RatePerUnit
								) OtherPOData ON OtherPOData.BOQDetailId = b.Id
                                          left join(select Sum(boqmap.TransactionQty) as OtherPOQuantity,B.MaterialMasterId,B.ArticleId,SO.Id as SalesOrderId,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                                          ,B.ThirdCharacteristicsValueId,boqmap.BOQDetailId
                                          from dbo.JWPOBOQMAP boqmap left join dbo.OSTransformationPODetail om on om.Id = boqmap.JWPODetailId
                                          left join dbo.BOQ B on B.Id = boqmap.BOQDetailId
                                          left join trn.SalesOrder SO on SO.Id = B.SalesOrderId
                                          group by B.MaterialMasterId,B.ArticleId,SO.Id,B.FirstCharacteristicsValueId,B.SecondCharacteristicsValueId
                                          ,B.ThirdCharacteristicsValueId,boqmap.BOQDetailId)
										  kk on kk.SalesOrderId = so.Id and kk.BOQDetailId = b.Id

                                          left join(Select outm.RatePerUnit, outm.ServiceId, boqm.BOQDetailId from dbo.OSTransformationPODetail outm

                                          left join dbo.JWPOBOQMAP boqm on boqm.JWPODetailId= outm.Id  where outm.Id= '" + JWPODId + @"')

                                          OtM on OtM.BOQDetailId = b.Id
                        LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId = mm.Id
                          LEFT JOIN JWPOBOQMAP JWPOBOQMAP ON JWPOBOQMAP.BOQDetailId = b.Id AND JWPOBOQMAP.JWPODetailId= '" + JWPODId + @"' --IN(select Id from OSTransformationPODetail where OSTransformationPOId = '" + JWPOId + @"')

                        WHERE moi.ContractId = '" + ContractId + @"' AND(b.VendorId = '" + VendorId + @"' OR b.VendorId is null)
                         AND b.id in(select ParentId from BOQ where ISNULL(ParentId, '') <> ''
						) 
						and b.MaterialMasterId = '" + MaterialId + @"' and b.ArticleId = '" + ArticleId + @"'
                          --and POMAP.Id = 'JWPD49'
                          --ORDER BY JWPOBOQMAP.BOQDetailId DESC, b.Sequence, b.SalesOrderId
                          group by

                         b.Id,b.Sequence
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo, '')
						,ISNULL(mo.BuyerReferenceNo, '')

						,ISNULL(moi.OwnReferenceNo, '')
						,ISNULL(moi.BuyerReferenceNo, '')
						, b.VendorId
						,b.SalesOrderId
						,mm.Id
						,mma.Id
						,IsNULL(mm.UserName, '')
						,IsNULL(mma.StandardName, '')
						,IsNULL(p.UserName, '')
						,IsNULL(v1.UserName, '')
						,IsNULL(v2.UserName, '')
						,IsNULL(v3.UserName, '')

						,b.FirstCharacteristicsValueId,FC.Id
						,b.SecondCharacteristicsValueId,SC.Id
						,b.ThirdCharacteristicsValueId,TC.Id
						,b.RequiredQtyApproved
						,b.IncompleteMaterial
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty--,C.Id
						,JWPOBOQMAP.Id  ,MM.HSNCodeId   ,MM.IsOriginApplicable
						
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106), ' ', '-')
						,ISNULL(cpo.PONumber, '')
						,b.RequiredQty
						,uom.UserName
						,b.POUoMId
                        --,b.POUoMId
						,b.RequiredQtyPO
                        --,b.RequiredQtyPO
                        --,b.POUoMId
						,mo.OwnReferenceNo
						,mm.BaseUOMId,b.POUoMId,b.UoMId
                        --,POMAP.RatePerUnit
                        --,POMAP.ServiceId
                        ,POMAP.TransactionQty
                        --,OtherPOData.TransactionQty
                        ,kk.OtherPOQuantity
						,OtM.RatePerUnit
						,OtM.ServiceId";


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

                    var JWServiceList = _sqlRepository.GetDataCollection(@"select Id as Value, UserName as Text from HKP.ServiceMaster order by UserName");
                    for (int i = 0; i < Data.Count; i++)
                    {
                        Data[i]["BOQserviceCboList"] = JWServiceList;
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
									--FROM [TRN].[POBOQMAP] POBOQMAP1
									--LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									--LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
                                    FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									where POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									--FROM [TRN].[POBOQMAP] POBOQMAP1
									--LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									--LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
                                    FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
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
                        --,JWPOBOQMAP.TransactionQty
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
									--LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									--LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
                                    LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									WHERE POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									--LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									--LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
                                    LEFT JOIN dbo.OSTransformationPODetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
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
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
									WHERE POM.Id ='" + JWPOId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM JWPOBOQMAP POBOQMAP1
									LEFT JOIN  JWTransformationPurchaseOrderDetail POD ON POD.Id=POBOQMAP1.JWPODetailId
									LEFT JOIN OSTransformationPO POM ON POM.Id=POD.OSTransformationPOId
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
            con.OpenDataSetThroughAdapter("SELECT * FROM OSTransformationPO WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
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
                sql = "SELECT * FROM JWTransformationPOActivity WHERE OSTransformationPOId='" + JWPOId + "'";
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

                            dr["OSTransformationPOId"] = JWPOId;

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
                //sql = "SELECT * FROM JWTransformationPOActivity WHERE OSTransformationPOId='" + JWPOId + "'";
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
        private string GetOSPOProductionOrderMapPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OSPOProductionOrderMap", out sID);
            return sID;
        }
        public Dictionary<string, object> Create(Dictionary<string, object> data, List<Dictionary<string, object>> mapdata, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            string JWPOId = "";
            DataSet dsMaster;
            DataSet dsMapMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM OSTransformationPO WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
            con.OpenDataSetThroughAdapter("SELECT * FROM OSPOProductionOrderMap WHERE OSTransformationPOId='" + data["Id"] + "'", out dsMapMaster, false, "1");

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
                    data["Id"] = "OSP" + _Id;
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
                    if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                    {
                        data["AuthorizedBy"] = data["CheckedBy"];
                        data["AuthorizedByStatus"] = "For Approval";
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];
                        data["IsApproved"] = false;

                    }
                    if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                    {
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];
                        data["IsApproved"] = true;
                    }
                    else
                    {

                        data["CheckedBy"] = data["CheckedBy"];//identity.EmployeeId;
                        data["CheckedByStatus"] = "Pending";
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["POType"] = data["POType"];
                        data["IsApproved"] = false;

                    }

                    data["ProcessId"] = data["ProcessId"]; 
                    data["IsClosed"] = false;

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
                    if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                    {
                        data["AuthorizedBy"] = data["CheckedBy"];
                        data["AuthorizedByStatus"] = "For Approval";
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];
                        data["IsApproved"] = false;

                    }
                    if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                    {
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["CheckedBy"] = null;
                        data["CheckedByStatus"] = null;
                        data["POType"] = data["POType"];
                        data["IsApproved"] = true;
                    }
                    else
                    {

                        data["CheckedBy"] = data["CheckedBy"]; //identity.EmployeeId; //data["CheckedBy"];
                        data["CheckedByStatus"] = "Pending";
                        data["AuthorizedBy"] = null;
                        data["AuthorizedByStatus"] = null;
                        data["POType"] = data["POType"];
                        data["IsApproved"] = false;

                    }


                    data["IsClosed"] = false;

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                foreach (var item in mapdata)
                {
                    dsMapMaster.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + dsMaster.Tables[0].Rows[0]["Id"].ToString() + "' ";

                    if (dsMapMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMapMaster.Tables[0].NewRow();
                        dr["Id"] = GetOSPOProductionOrderMapPK();

                        dr["OSTransformationPOId"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                        dr["ProductionOrderId"] = item["POId"].ToString();
                        //dr["Remarks"] = item["Remarks"];
                        dsMapMaster.Tables[0].Rows.Add(dr);
                    }
                    
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMapMaster);
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
                    con2.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPODetail where OSTransformationPOId='" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Output Data");
                    }
                }

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();



                //   con.executeQuery("delete from JWTransformationPurchaseOrderDetail where OSTransformationPOId='" + id + "'");
                con.executeQuery("delete from OSTransformationPO where Id='" + id + "'");

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
        public void DeleteDetail(string id, string OrderSpecific)
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

                if (OrderSpecific == "No")
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        con2.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPOMasterOrderItem where OSTransformationPODetailId='" + id + "' ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            throw new Exception("First Delete Order Wise Data");
                        }

                        con2.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + id + "' ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            throw new Exception("First Delete Material Input Data");
                        }

                    }

                    //con.executeQuery("DELETE OSTransformationPOTax where OSTransformationPODetailId = '" + id + "'");
                    //con.executeQuery("DELETE JWPOBOQMAP where JWPODetailId = '" + id + "'");
                    //con.executeQuery("DELETE JWTransformationPurchaseOrderChildMaterial where JWPODetailId = '" + id + "'");
                    //con.executeQuery("DELETE OSTransformationPOTax where OSTransformationPODetailId = '" + id + "'");
                    //con.executeQuery("DELETE JWTransformationPurchaseOrderInputChildMaterial where JWPODetailId = '" + id + "'");
                    //con.executeQuery("DELETE JWTransformationPurchaseOrderByProductChildMaterial where JWPODetailId = '" + id + "'");

                    //con.executeQuery("DELETE OSTransformationPOTax where OSTransformationPODetailId = '" + id + "'");
                    //con.executeQuery("DELETE from  JWTransformationPurchaseOrderDetail where id='" + id + "'");

                    con.executeQuery("delete from dbo.OSTransformationPOTax where OSTransformationPODetailId='" + id + @"' ");
                    con.executeQuery("delete from dbo.OSTransformationPODetail where Id='" + id + "' ");
                }
                else
                {
                    //       con.executeQuery("delete from dbo.JWPOBOQMAP where JWPODetailId='" + id + @"' ");
                    con.executeQuery("delete from dbo.OSPOBOQMAP where OSTransformationPODetailId='" + id + @"' ");
                    con.executeQuery("delete from dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + id + @"' ");
                    con.executeQuery("delete from dbo.OSTransformationPOTax where OSTransformationPODetailId='" + id + @"' ");
                    con.executeQuery("delete from dbo.OSTransformationPODetail where Id='" + id + "' ");
                }



                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;

            }


        }
        #endregion


        public Dictionary<string, object> ServiceChargeCreate(Dictionary<string, object> data, List<Dictionary<string, object>> TaxList)
        {

            string JWPODSId = "";
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM OSTransformationPOService WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

            try
            {
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("OSTransformationPOService", out _Id);
                    data["Id"] = "OPS" + _Id;
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
                sql = "SELECT * FROM OSTransformationPOTax WHERE OSTransformationPOId='" + data["OSTransformationPOId"] + "' and  OSTransformationPOServiceId='" + data["Id"] + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsTax, false, "1");
                if (TaxList != null)
                {
                    for (int i = 0; i < dsTax.Tables[0].Rows.Count; i++)
                    {
                        var k = TaxList.Where(ee => ee["OSTransformationPOServiceId"].ToString() == dsTax.Tables[0].Rows[i]["OSTransformationPOServiceId"].ToString()).ToList();
                        if (k.Count == 0)
                        {
                            dsTax.Tables[0].Rows[i].Delete();
                        }
                    }

                    for (int i = 0; i < TaxList.Count; i++)
                    {
                        dsTax.Tables[0].DefaultView.RowFilter = "Id='" + TaxList[i]["Id"] + "'";


                        if (dsTax.Tables[0].DefaultView.Count == 0)
                        {

                            if (_activityId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("OSTransformationPOTax", out _activityId);
                                _activityId = "JTX" + _activityId;
                            }
                            DataRow dr = dsTax.Tables[0].NewRow();
                            dr["Id"] = _activityId + "-" + (i + 1).ToString();

                            dr["OSTransformationPOId"] = data["OSTransformationPOId"];
                            dr["ServiceMasterId"] = data["ServiceMasterId"];
                            dr["OSTransformationPOServiceId"] = data["Id"];



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

                            dr["OSTransformationPOId"] = data["OSTransformationPOId"];
                            dr["OSTransformationPOServiceId"] = data["Id"];

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
        //        JWPurchaseOrderId = data[0]["OSTransformationPOId"].ToString();
        //    }

        //    con.OpenDataSetThroughAdapter("SELECT * FROM JWTransformationPurchaseOrderDetail WHERE OSTransformationPOId='" + JWPurchaseOrderId + "'", out dsMaster, false, "1");

        //    List<Dictionary<string, object>> dataBoq = new List<Dictionary<string, object>>();
        //    //List<Dictionary<string, object>> dataDetail = new List<Dictionary<string, object>>();

        //    DataSet dsTax = null;
        //    sql = "SELECT * FROM OSTransformationPOTax WHERE OSTransformationPOId='" + JWPurchaseOrderId + "'";
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
        //                    data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
        //                    if (OrderSpecific == "Yes")
        //                    {
        //                        data[i]["ReferenceNo"] = data[i]["ReferenceNoM"];

        //                    }
        //                    AddNewRow(dsMaster.Tables[0], data[i]);


        //                }
        //                else
        //                {

        //                    data[i]["OSTransformationPOId"] = JWPurchaseOrderId;

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
        //                    data[i]["OSTransformationPOId"] = JWPurchaseOrderId;

        //                    AddNewRow(dsMaster.Tables[0], data[i]);


        //                }
        //                else
        //                {

        //                    data[i]["OSTransformationPOId"] = JWPurchaseOrderId;

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

        private string GetOSoutputmatPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OSTransformationPODetail", out sID);
            return sID;
        }

        private string GetOSPOTaxPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OSTransformationPOTax", out sID);
            return sID;
        }

        public List<Dictionary<string, object>> detailcreate(List<Dictionary<string, object>> data, string JWPurchaseOrderId, string JWActivityId, string userName, string IPAddress, string OrderSpecific, string type, List<Dictionary<string, object>> taxCategoryList, string JWPOToCurrencyRate, string JWPOIsNonCreditable, string JWPODate, string JWPOType)
        {
            string JWOutId = " ";
            //string  JWBOQId = " ";
            string JWBOQId = "' '";
            string ABC = "";
            string JWBOQReqQty = "";
            string TRate = "";
            string TQty = "";
            //  var JWBOQChildId = "' '";

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
                JWPurchaseOrderId = data[0]["OSTransformationPOId"].ToString();
            }
            if (type != "BOQ" && type != "PODETAILLIST")
            {
                //con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPODetail where JobActivityId='" + data[0]["JobActivityId"] + "' and JobWorkItemMasterId='" + data[0]["JobWorkItemMasterId"] + "' and ArticleId='" + data[0]["ArticleId"] + "' and MaterialMasterId='" + data[0]["MaterialMasterId"] + "' and OSTransformationPOId='" + data[0]["OSTransformationPOId"] + "' AND  Id<>'" + data[0]["Id"] + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Same Activity, JW Output Item, Material and Article already exist.");
                //}

                con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPODetail where ArticleId='" + data[0]["ArticleId"] + "' and MaterialMasterId='" + data[0]["MaterialMasterId"] + "' and OSTransformationPOId='" + data[0]["OSTransformationPOId"] + "' AND  Id<>'" + data[0]["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Material and Article already exist.");
                }
            }


            con.OpenDataSetThroughAdapter("SELECT * FROM OSTransformationPODetail WHERE OSTransformationPOId='" + JWPurchaseOrderId + "'", out dsMaster, false, "1");

            List<Dictionary<string, object>> dataBoq = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> detailBoq = new List<Dictionary<string, object>>();
            //List<Dictionary<string, object>> dataDetail = new List<Dictionary<string, object>>();

            DataSet dsTax = null;
            sql = "SELECT * FROM OSTransformationPOTax WHERE OSTransformationPOId='" + JWPurchaseOrderId + "'";
            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(sql, out dsTax, false, "1");

            try
            {
                Library.General.Conversions.UOMConversion Conversion = new Library.General.Conversions.UOMConversion();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (OrderSpecific == "Yes")
                {
                    dataBoq = data;
                    detailBoq = data;
                    if (data[0]["BOQId"] != null)
                    {
                        data = MakePodetail(data);
                    }

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
                            dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";
                            if (dsMaster.Tables[0].DefaultView.Count == 0)
                            {
                                if (clsStaticInfo.dbl(data[i]["TransactionQty"].ToString()) + clsStaticInfo.dbl(data[i]["OtherPOQty"].ToString()) > clsStaticInfo.dbl(data[i]["RequiredQty"].ToString()))
                                {
                                    throw new Exception("Current Qty can't be Greater then Transaction Qty.");
                                }
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
                            genid.GenID("OSTransformationPODetail", out _Id);
                            // data[i]["Id"] = "JWPD" + _Id;
                            data[i]["Id"] = "JWPD" + GetOSoutputmatPK();
                            JWPODId = data[i]["Id"].ToString();
                            data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                            data[i]["Quantity"] = data[i]["TransactionQty"];
                            data[i]["RatePerUnit"] = data[i]["TransactionRate"];
                            data[i]["ServiceId"] = data[i]["ServiceId"];
                            data[i]["TaxAmount"] = 0;

                            if (JWPOIsNonCreditable == "False")
                            {
                                //    decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Qty = Convert.ToDecimal(data[i]["TransactionQty"]);
                                decimal Rate = Convert.ToDecimal(data[i]["RatePerUnit"]);
                                decimal Amt = Convert.ToDecimal(Qty * Rate);
                                decimal BAmt = Convert.ToDecimal(Amt);
                                data[i]["TransactionAmount"] = Amt;
                                data[i]["BaseAmount"] = BAmt;
                            }

                            if (JWPOIsNonCreditable == "True")
                            {
                                //   decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Qty = Convert.ToDecimal(data[i]["TransactionQty"]);
                                decimal Rate = Convert.ToDecimal(data[i]["RatePerUnit"]);
                                decimal Amt = Convert.ToDecimal(Qty * Rate);
                                decimal BAmt = Convert.ToDecimal(Amt + 0);
                                data[i]["TransactionAmount"] = Amt;
                                data[i]["BaseAmount"] = BAmt;
                            }

                            if (OrderSpecific == "Yes")
                            {
                                data[i]["ReferenceNo"] = data[i]["BuyerItemReferenceNo"];

                            }
                            //JWOutId += ",'" + data[i]["Id"].ToString() + "' ";
                            //JWBOQId += ",'" + data[i]["BOQId"].ToString() + "' ";

                            JWOutId = data[i]["Id"].ToString();
                            JWBOQId += ", '" + data[i]["BOQId"].ToString() + "' ";
                            ABC = data[i]["BOQId"].ToString();
                            JWBOQReqQty = data[i]["RequiredQtyPO"].ToString();
                            TQty = data[i]["TransactionQty"].ToString();
                            TRate = data[i]["RatePerUnit"].ToString();
                            dataBoq = data;

                            AddNewRow(dsMaster.Tables[0], data[i]);

                            clsStaticInfo _info = new clsStaticInfo();
                            _info.SaveDataSets(dsMaster);
                            SaveJWBOQChild(JWOutId, JWBOQId, JWBOQReqQty, ABC, JWPOType);
                            SaveJWServiceTaxes(JWOutId, JWPurchaseOrderId, JWPODate, TQty, TRate, JWPOIsNonCreditable);
                            JPOBOQMAPCreate(dataBoq, JWOutId, userName, IPAddress, out dsPOBOQMap, detailBoq);

                        }
                        else
                        {

                            //data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                            //data[i]["Quantity"] = data[i]["TransactionQty"];
                            //data[i]["RatePerUnit"] = data[i]["TransactionRate"];
                            //data[i]["TransactionAmount"] = data[i]["TransactionAmount"];
                            //data[i]["TaxAmount"] = data[i]["TaxAmount"];
                            //    data[i]["TaxAmount"] = data[i]["JWTaxAmount"];

                            data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                            data[i]["Quantity"] = data[i]["TransactionQty"];
                            data[i]["RatePerUnit"] = data[i]["TransactionRate"];
                            data[i]["ServiceId"] = data[i]["ServiceId"];
                            data[i]["TaxAmount"] = 0;

                            if (JWPOIsNonCreditable == "False")
                            {
                                //    decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Qty = Convert.ToDecimal(data[i]["TransactionQty"]);
                                decimal Rate = Convert.ToDecimal(data[i]["RatePerUnit"]);
                                decimal Amt = Convert.ToDecimal(Qty * Rate);
                                decimal BAmt = Convert.ToDecimal(Amt);
                                data[i]["TransactionAmount"] = Amt;
                                data[i]["BaseAmount"] = BAmt;
                            }

                            if (JWPOIsNonCreditable == "True")
                            {
                                //   decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Qty = Convert.ToDecimal(data[i]["TransactionQty"]);
                                decimal Rate = Convert.ToDecimal(data[i]["RatePerUnit"]);
                                decimal Amt = Convert.ToDecimal(Qty * Rate);
                                decimal TAmt = Convert.ToDecimal(data[i]["TaxAmount"]);
                                decimal BAmt = Convert.ToDecimal(Amt + TAmt);
                                data[i]["TransactionAmount"] = Amt;
                                data[i]["BaseAmount"] = BAmt;
                            }

                            if (OrderSpecific == "Yes" && data[i]["BOQId"] != null)
                            {
                                data[i]["ReferenceNo"] = data[i]["BuyerItemReferenceNo"];

                            }
                            else
                            {
                                data[i]["ReferenceNo"] = data[i]["ReferenceNo"];
                            }

                            //JWOutId += ",'" + data[i]["Id"].ToString() + "' ";
                            //JWBOQId += ",'" + data[i]["BOQId"].ToString() + "' ";

                            JWOutId = data[i]["Id"].ToString();
                            if (data[i]["BOQId"] != null)
                            {
                                JWBOQId += ", '" + data[i]["BOQId"].ToString() + "' ";
                                ABC = data[i]["BOQId"].ToString();
                                JWBOQReqQty = data[i]["RequiredQtyPO"].ToString();
                                TQty = data[i]["TransactionQty"].ToString();
                                TRate = data[i]["RatePerUnit"].ToString();
                                dataBoq = data;
                            }


                            EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);

                            if (data[i]["BOQId"] != null)
                            {
                                clsStaticInfo _info = new clsStaticInfo();
                                _info.SaveDataSets(dsMaster);
                                SaveJWBOQChild(JWOutId, JWBOQId, JWBOQReqQty, ABC, JWPOType);
                                SaveJWServiceTaxes(JWOutId, JWPurchaseOrderId, JWPODate, TQty, TRate, JWPOIsNonCreditable);
                                JPOBOQMAPCreate(dataBoq, JWOutId, userName, IPAddress, out dsPOBOQMap, detailBoq);
                            }

                        }
                    }

                }
                else
                {
                    decimal SumTax = 0;
                    if (taxCategoryList != null)
                    {

                        for (int a = 0; a < taxCategoryList.Count; a++)
                        {
                            decimal T1 = Convert.ToDecimal(taxCategoryList[a]["TaxAmount"]);
                            SumTax = T1 + SumTax;
                        }
                    }


                    for (int i = 0; i < data.Count; i++)
                    {


                        dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";
                        dsTax.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";
                        // dsTax.Tables[0].DefaultView.RowFilter = "Id='" + bplib.clsWebLib.RetValidLen(data[i]["Id"]).ToString() + "'";

                        string _Id = "";

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            //if (data[i]["Id"] == null)
                            //{
                            //    TaxList = null;
                            //}

                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("OSTransformationPODetail", out _Id);
                            //   data[i]["Id"] = "JWPD" + _Id;
                            data[i]["Id"] = "JWPD" + GetOSoutputmatPK();
                            JWPODId = data[i]["Id"].ToString();
                            //data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                            data[i]["TransactionUoMId"] = data[i]["OutputMaterialUOMId"];
                            data[i]["BaseUOMId"] = data[i]["OutputMaterialUOMId"];

                            data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                            data[i]["Quantity"] = data[i]["TransactionQty"];
                            data[i]["TaxAmount"] = SumTax;

                            if (JWPOIsNonCreditable == "False")
                            {
                                //    decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Amt = Convert.ToDecimal(data[i]["TransactionAmount"]);
                                decimal BAmt = Convert.ToDecimal(Amt);
                                data[i]["TransactionAmount"] = data[i]["TransactionAmount"];
                                data[i]["BaseAmount"] = BAmt;
                            }

                            if (JWPOIsNonCreditable == "True")
                            {
                                //   decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Amt = Convert.ToDecimal(data[i]["TransactionAmount"]);
                                decimal BAmt = Convert.ToDecimal(Amt + SumTax);
                                data[i]["TransactionAmount"] = data[i]["TransactionAmount"];
                                data[i]["BaseAmount"] = BAmt;
                            }

                            AddNewRow(dsMaster.Tables[0], data[i]);



                        }


                        else
                        {

                            //data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                            data[i]["TransactionUoMId"] = data[i]["OutputMaterialUOMId"];
                            data[i]["BaseUOMId"] = data[i]["OutputMaterialUOMId"];

                            data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                            data[i]["Quantity"] = data[i]["TransactionQty"];
                            data[i]["TaxAmount"] = SumTax;

                            if (JWPOIsNonCreditable == "False")
                            {
                                //   decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Amt = Convert.ToDecimal(data[i]["TransactionAmount"]);
                                decimal BAmt = Convert.ToDecimal(Amt);
                                data[i]["TransactionAmount"] = data[i]["TransactionAmount"];
                                data[i]["BaseAmount"] = BAmt;
                            }

                            if (JWPOIsNonCreditable == "True")
                            {
                                //   decimal PORate = Convert.ToDecimal(JWPOToCurrencyRate);
                                decimal Amt = Convert.ToDecimal(data[i]["TransactionAmount"]);
                                decimal BAmt = Convert.ToDecimal(Amt + SumTax);
                                data[i]["TransactionAmount"] = data[i]["TransactionAmount"];
                                data[i]["BaseAmount"] = BAmt;
                            }

                            EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
                        }
                        string DetailIdid = dsMaster.Tables[0].Rows[i]["Id"].ToString();

                        if (taxCategoryList != null)
                        {
                            for (int i1 = 0; i1 < taxCategoryList.Count; i1++)
                            {


                                if (dsTax.Tables[0].DefaultView.Count == 0)
                                {


                                    bplib.clsGenID genid = new bplib.clsGenID();
                                    genid.GenID("OSTransformationPOTax", out _Id);
                                    //  taxCategoryList[i1]["Id"] = "JWPDT" + _Id;
                                    taxCategoryList[i1]["Id"] = "JWPDT" + GetOSPOTaxPK();
                                    //JWPODId = taxCategoryList[i1]["Id"].ToString();
                                    //data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                                    taxCategoryList[i1]["OSTransformationPOId"] = JWPurchaseOrderId;
                                    taxCategoryList[i1]["OSTransformationPODetailId"] = DetailIdid;
                                    //data[i]["Quantity"] = data[i]["TransactionQty"];

                                    AddNewRow(dsTax.Tables[0], taxCategoryList[i1]);



                                }


                                else
                                {



                                    //data[i]["OSTransformationPOId"] = JWPurchaseOrderId;
                                    //taxCategoryList[i1]["OSTransformationPOId"] = JWPurchaseOrderId;
                                    //taxCategoryList[i1]["OSTransformationPODetailId"] = DetailIdid;

                                    //taxCategoryList[i1]["TaxCategoryId"] = dsTax.Tables[0].Rows[i1][""];
                                    //taxCategoryList[i1]["Percentage"] = DetailIdid;
                                    //taxCategoryList[i1]["TaxAmount"] = JWPurchaseOrderId;
                                    // taxCategoryList[i1]["Quantity"] = data[i]["TransactionQty"];
                                    // EditRow(dsTax.Tables[0].DefaultView[0].Row, taxCategoryList[i1]);

                                    // edit

                                    dsTax.Tables[0].DefaultView.RowFilter = "Id ='" + taxCategoryList[i1]["Id"] + "' ";
                                    if (dsTax.Tables[0].DefaultView.Count == 0)
                                    {
                                        bplib.clsGenID genid = new bplib.clsGenID();
                                        genid.GenID("OSTransformationPOTax", out _Id);
                                        taxCategoryList[i1]["Id"] = "JWPDT" + _Id;
                                        taxCategoryList[i1]["OSTransformationPOId"] = JWPurchaseOrderId;
                                        taxCategoryList[i1]["OSTransformationPODetailId"] = DetailIdid;

                                        AddNewRow(dsTax.Tables[0], taxCategoryList[i1]);
                                    }
                                    else
                                    {
                                        DataRow dr = dsTax.Tables[0].DefaultView[0].Row;

                                        dr.BeginEdit();
                                        dr["OSTransformationPOId"] = JWPurchaseOrderId;
                                        dr["OSTransformationPODetailId"] = DetailIdid;
                                        dr["TaxCategoryId"] = taxCategoryList[i1]["TaxCategoryId"];
                                        dr["Percentage"] = taxCategoryList[i1]["Percentage"];
                                        dr["TaxAmount"] = taxCategoryList[i1]["TaxAmount"];
                                        dr["HSNCodeId"] = taxCategoryList[i1]["HSNCodeId"];

                                        dr["AddedBy"] = identity.Name;
                                        dr["AddedDate"] = DateTime.Now.ToString();
                                        dr["AddedFromIP"] = identity.IPAddress;
                                        dr["UpdatedBy"] = identity.Name;
                                        dr["UpdatedDate"] = DateTime.Now.ToString();
                                        dr["UpdatedFromIP"] = identity.IPAddress;

                                        dr.EndEdit();
                                    }

                                }
                            }
                        }

                    }

                }

                if (data != null)
                {
                    clsStaticInfo _info = new clsStaticInfo();
                    if (OrderSpecific == "Yes")
                    {
                        if (data[0].ContainsKey("BOQId") && data[0]["BOQId"] != null)
                        {

                            for (int i = 0; i < data.Count; i++)
                            {
                                SaveJWTransformationPurchaseOrderChildMaterial(dataBoq, data[i]["Id"].ToString(), JWActivityId, Conversion, out dsJwChildMaterial);

                                //JPOBOQMAPCreate(dataBoq, data[i]["Id"].ToString(), userName, IPAddress, out dsPOBOQMap);
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

        // BOQ Child Material

        private string GetMaterialInputPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OSTransformationPOInputMaterial", out sID);
            return sID;
        }

        public void SaveJWBOQChild(string JWOutId, string JWBOQId, string JWBOQReqQty, string ABC, string JWPOType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var JWBOQChildId = "' '";
            var BB = "''";
            BB += ",'" + JWBOQId + "' ";
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (JWPOType == "OSTransformationPO")
                {
                    DataSet JWBOQ;
                    DataSet JWOutMat;
                    DataSet DelJWMatInput;

                    if (!string.IsNullOrEmpty(JWOutId))
                    {
                        con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + JWOutId + "' ", out DelJWMatInput, false, "1");
                        if (DelJWMatInput.Tables[0].Rows.Count > 0)
                        {
                            // throw new Exception("First Delete Material Output Data");
                            con.BeginTransaction();

                            con.executeQuery("delete from dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + JWOutId + @"' ");

                            con.CommitTransaction();
                        }
                    }

                    //con.OpenDataSetThroughAdapter("select * from BOQ where ParentId='"+ JWBOQId + @"'  ", out JWBOQ, false, "1");
                    //con.OpenDataSetThroughAdapter("select * from BOQ where ParentId IN ( " + JWBOQId + " )  ", out JWBOQ, false, "1");
                    con.OpenDataSetThroughAdapter("select * from BOQ where ParentId IN (" + ABC + ")  ", out JWBOQ, false, "1");

                    for (var i = 0; i < JWBOQ.Tables[0].DefaultView.Count; i++)
                    {
                        JWBOQChildId += ",'" + JWBOQ.Tables[0].Rows[i]["Id"];
                    }
                    con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + JWOutId + "' ", out JWOutMat, false, "1");
                    string BoqChildId = "";
                    decimal Consumption = 0;
                    decimal WastagePer = 0;
                    string BOQArticleId = "";

                    for (var j = 0; j < JWBOQ.Tables[0].DefaultView.Count; j++)
                    {
                        //  JWBOQChildId += ",'" + data[i]["Id"].ToString() + "' ";
                        //        JWBOQ.Tables[0].DefaultView.RowFilter = "ParentId ='" + data + "' and WorkDate='" + item.WorkDate + "' ";

                        BoqChildId = JWBOQ.Tables[0].Rows[j]["Id"].ToString();
                        Consumption = Convert.ToDecimal(JWBOQ.Tables[0].Rows[j]["Consumption"]);
                        WastagePer = Convert.ToDecimal(JWBOQ.Tables[0].Rows[j]["WastagePer"]);
                        decimal ReqQuantity = (Convert.ToDecimal(JWBOQReqQty) * Consumption) * (1 + (WastagePer / 100));

                        decimal GrossConsumption = (Consumption * (1 + (WastagePer / 100)));

                        BOQArticleId = JWBOQ.Tables[0].Rows[j]["ArticleId"].ToString();

                        JWOutMat.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + JWOutId + "' and BOQChildId='" + bplib.clsWebLib.RetValidLen(JWBOQ.Tables[0].Rows[j]["Id"]).ToString() + "' ";


                        if (JWOutMat.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = JWOutMat.Tables[0].NewRow();
                            dr["Id"] = "MI" + GetMaterialInputPK();

                            dr["OSTransformationPODetailId"] = JWOutId;

                            //dr["JobWorkItemId"] = item.JobWorkItemId;
                            //dr["ItemSpecification"] = item.ItemSpecification;

                            dr["NetConsumption"] = Consumption;
                            dr["Rejection"] = 0;
                            dr["ValueLoss"] = WastagePer;
                            dr["GrossConsumption"] = GrossConsumption;

                            dr["BOQRequiredQuantity"] = ReqQuantity;
                            dr["BOQChildId"] = JWBOQ.Tables[0].Rows[j]["Id"];
                            dr["ArticleId"] = BOQArticleId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            JWOutMat.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit

                            JWOutMat.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + JWOutId + "' and BOQChildId='" + bplib.clsWebLib.RetValidLen(JWBOQ.Tables[0].Rows[j]["Id"]).ToString() + "' ";

                            if (JWOutMat.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow drr = JWOutMat.Tables[0].NewRow();
                                drr["Id"] = "MI" + GetMaterialInputPK();

                                drr["OSTransformationPODetailId"] = JWOutId;

                                drr["NetConsumption"] = Consumption;
                                drr["Rejection"] = 0;
                                drr["ValueLoss"] = WastagePer;
                                drr["GrossConsumption"] = GrossConsumption;
                                drr["BOQRequiredQuantity"] = ReqQuantity;
                                drr["BOQChildId"] = JWBOQ.Tables[0].Rows[j]["Id"];
                                drr["ArticleId"] = BOQArticleId;

                                drr["AddedBy"] = identity.Name;
                                drr["AddedDate"] = System.DateTime.Now.ToString();
                                drr["AddedFromIP"] = identity.IPAddress;

                                JWOutMat.Tables[0].Rows.Add(drr);
                            }
                            else if (JWOutMat.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow drr = JWOutMat.Tables[0].DefaultView[0].Row;

                                drr.BeginEdit();

                                drr["OSTransformationPODetailId"] = JWOutId;

                                drr["NetConsumption"] = Consumption;
                                drr["Rejection"] = 0;
                                drr["ValueLoss"] = WastagePer;
                                drr["GrossConsumption"] = GrossConsumption;
                                drr["BOQRequiredQuantity"] = ReqQuantity;
                                drr["BOQChildId"] = JWBOQ.Tables[0].Rows[j]["Id"];
                                drr["ArticleId"] = BOQArticleId;

                                drr["UpdatedBy"] = identity.Name;
                                drr["UpdatedDate"] = System.DateTime.Now.ToString();
                                drr["UpdatedFromIP"] = identity.IPAddress;

                                drr.EndEdit();
                            }
                        }
                        //clsStaticInfo _info = new clsStaticInfo();
                        //_info.SaveDataSets(JWOutMat);
                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(JWOutMat);

                }
            }
            catch (Exception ex)
            {

                throw ex;

            }

        }

        // JW SERVICE TAXES(BOQ POP UP)

        private string GetJWTaxesPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OSTransformationPOTax", out sID);
            return sID;
        }

        public void SaveJWServiceTaxes(string JWOutId, string JWPurchaseOrderId, string JWPODate, string TQty, string TRate, string JWPOIsNonCreditable)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //      var JWBOQChildId = "' '";
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet JWTaxExist;
                DataSet JWGetSerTaxes;
                DataSet JWOutMatExist;
                string hsnCodeId = "";

                con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPOTax where OSTransformationPOId='" + JWPurchaseOrderId + @"' and OSTransformationPODetailId='" + JWOutId + @"'  ", out JWTaxExist, false, "1");
                con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPODetail where Id='" + JWOutId + @"' ", out JWOutMatExist, false, "1");
                //for (var i = 0; i < JWBOQ.Tables[0].DefaultView.Count; i++)
                //{
                //    JWBOQChildId += ",'" + JWBOQ.Tables[0].Rows[i]["Id"];
                //}

                con.OpenDataSetThroughAdapter("DECLARE @receiveId varchar(10)='" + JWPurchaseOrderId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + identity.PlantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN OSTransformationPO AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN OSTransformationPO AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage],'0') AS [Percentage], 0 TaxAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + JWPODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + identity.CompanyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence] ", out JWGetSerTaxes, false, "1");

                //string BoqChildId = "";
                //decimal Consumption = 0;
                //decimal WastagePer = 0;

                decimal SumTax = 0;
                decimal TrAmt = 0;
                decimal TaxAmt = 0;
                //for (int a = 0; a < JWGetSerTaxes.Tables[0].DefaultView.Count; a++)
                //{
                //    decimal T1 = Convert.ToDecimal(JWGetSerTaxes.Tables[0].Rows[a]["TaxAmount"]);
                //    SumTax = T1 + SumTax;
                //}

                for (var j = 0; j < JWGetSerTaxes.Tables[0].DefaultView.Count; j++)
                {
                    if (JWGetSerTaxes.Tables[0].Rows[j]["Percentage"] != null && Convert.ToDecimal(JWGetSerTaxes.Tables[0].Rows[j]["Percentage"]) != 0)
                    {
                        decimal P1 = Convert.ToDecimal(JWGetSerTaxes.Tables[0].Rows[j]["Percentage"]);
                        TrAmt = Convert.ToDecimal(TQty) * Convert.ToDecimal(TRate);
                        TaxAmt = ((TrAmt * P1) / 100);
                        SumTax = TaxAmt + SumTax;

                    }

                    //BoqChildId = JWBOQ.Tables[0].Rows[j]["Id"].ToString();
                    //Consumption = Convert.ToDecimal(JWBOQ.Tables[0].Rows[j]["Consumption"]);
                    //WastagePer = Convert.ToDecimal(JWBOQ.Tables[0].Rows[j]["WastagePer"]);
                    //decimal ReqQuantity = (Convert.ToDecimal(JWBOQReqQty) * Consumption) * (1 + (WastagePer / 100));

                    //decimal GrossConsumption = (Consumption * (1 + (WastagePer / 100)));

                    JWTaxExist.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + JWPurchaseOrderId + "' and OSTransformationPODetailId ='" + JWOutId + "' and TaxCategoryId='" + bplib.clsWebLib.RetValidLen(JWGetSerTaxes.Tables[0].Rows[j]["TaxCategoryId"]).ToString() + "' ";


                    if (JWTaxExist.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = JWTaxExist.Tables[0].NewRow();
                        dr["Id"] = "JTX" + GetJWTaxesPK();

                        dr["OSTransformationPOId"] = JWPurchaseOrderId;

                        dr["OSTransformationPODetailId"] = JWOutId;

                        dr["TaxCategoryId"] = JWGetSerTaxes.Tables[0].Rows[j]["TaxCategoryId"].ToString();
                        //dr["HSNCodeId"] = JWGetSerTaxes.Tables[0].Rows[j]["HSNCodeId"].ToString();
                        dr["Percentage"] = Convert.ToDecimal(JWGetSerTaxes.Tables[0].Rows[j]["Percentage"]);
                        dr["TaxAmount"] = TaxAmt;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        JWTaxExist.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit

                        JWTaxExist.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + JWPurchaseOrderId + "' and OSTransformationPODetailId ='" + JWOutId + "' and TaxCategoryId='" + bplib.clsWebLib.RetValidLen(JWGetSerTaxes.Tables[0].Rows[j]["TaxCategoryId"]).ToString() + "' ";

                        if (JWTaxExist.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow drr = JWTaxExist.Tables[0].NewRow();
                            drr["Id"] = "JTX" + GetJWTaxesPK();

                            drr["OSTransformationPOId"] = JWPurchaseOrderId;

                            drr["OSTransformationPODetailId"] = JWOutId;

                            drr["TaxCategoryId"] = JWGetSerTaxes.Tables[0].Rows[j]["TaxCategoryId"].ToString();
                            //drr["HSNCodeId"] = JWGetSerTaxes.Tables[0].Rows[j]["HSNCodeId"].ToString();
                            drr["Percentage"] = Convert.ToDecimal(JWGetSerTaxes.Tables[0].Rows[j]["Percentage"]);
                            drr["TaxAmount"] = TaxAmt;

                            drr["AddedBy"] = identity.Name;
                            drr["AddedDate"] = System.DateTime.Now.ToString();
                            drr["AddedFromIP"] = identity.IPAddress;

                            JWTaxExist.Tables[0].Rows.Add(drr);
                        }
                        else if (JWTaxExist.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow drr = JWTaxExist.Tables[0].DefaultView[0].Row;

                            drr.BeginEdit();

                            drr["OSTransformationPOId"] = JWPurchaseOrderId;

                            drr["OSTransformationPODetailId"] = JWOutId;

                            drr["TaxCategoryId"] = JWGetSerTaxes.Tables[0].Rows[j]["TaxCategoryId"].ToString();
                            //drr["HSNCodeId"] = JWGetSerTaxes.Tables[0].Rows[j]["HSNCodeId"].ToString();
                            drr["Percentage"] = Convert.ToDecimal(JWGetSerTaxes.Tables[0].Rows[j]["Percentage"]);
                            drr["TaxAmount"] = TaxAmt;

                            drr["UpdatedBy"] = identity.Name;
                            drr["UpdatedDate"] = System.DateTime.Now.ToString();
                            drr["UpdatedFromIP"] = identity.IPAddress;

                            drr.EndEdit();
                        }
                    }
                    //clsStaticInfo _info = new clsStaticInfo();
                    //_info.SaveDataSets(JWOutMat);
                }

                JWOutMatExist.Tables[0].DefaultView.RowFilter = "Id ='" + JWOutId + "' ";
                if (JWOutMatExist.Tables[0].DefaultView.Count > 0)
                {
                    DataRow drr = JWOutMatExist.Tables[0].DefaultView[0].Row;

                    drr.BeginEdit();

                    TrAmt = Convert.ToDecimal(TQty) * Convert.ToDecimal(TRate);
                    if (JWPOIsNonCreditable == "False")
                    {
                        drr["TransactionAmount"] = TrAmt;
                        drr["BaseAmount"] = TrAmt;
                    }

                    if (JWPOIsNonCreditable == "True")
                    {
                        drr["TransactionAmount"] = TrAmt;
                        drr["BaseAmount"] = TrAmt + SumTax;
                    }
                    drr["TaxAmount"] = SumTax;

                    drr["UpdatedBy"] = identity.Name;
                    drr["UpdatedDate"] = System.DateTime.Now.ToString();
                    drr["UpdatedFromIP"] = identity.IPAddress;

                    drr.EndEdit();
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(JWTaxExist, JWOutMatExist);


            }
            catch (Exception ex)
            {

                throw ex;

            }

        }

        public List<Dictionary<string, object>> SaveTaxList(List<Dictionary<string, object>> data, List<Dictionary<string, object>> TaxList, string userName, string IPAddress)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataSet dsTax = null;
            DataSet OutputMat = null;
            string sql = "";
            string sql2 = "";

            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            //sql = "SELECT * FROM OSTransformationPOTax WHERE OSTransformationPOId='" + data[0]["OSTransformationPOId"] + "'";
            sql = "SELECT * FROM OSTransformationPOTax WHERE OSTransformationPODetailId='" + data[0]["Id"] + "'";
            sql2 = "SELECT * FROM dbo.OSTransformationPODetail WHERE Id='" + data[0]["Id"] + "'";

            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(sql, out dsTax, false, "1");
            con.OpenDataSetThroughAdapter(sql2, out OutputMat, false, "1");

            try
            {
                string _taxId = "";
                if (TaxList != null)
                {
                    for (int tc = 0; tc < dsTax.Tables[0].Rows.Count; tc++)
                    {

                        var k = TaxList.Where(ee => ee["OSTransformationPODetailId"].ToString() == dsTax.Tables[0].Rows[tc]["OSTransformationPODetailId"].ToString() && ee["Id"].ToString() == dsTax.Tables[0].Rows[tc]["Id"].ToString()).ToList();

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


                        dsTax.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + TaxList[t]["OSTransformationPODetailId"] + "' and Id = '" + TaxList[t]["Id"] + "'  ";
                        OutputMat.Tables[0].DefaultView.RowFilter = "Id='" + data[0]["Id"] + "' ";

                        if (dsTax.Tables[0].DefaultView.Count == 0)
                        {

                            if (_taxId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("OSTransformationPOTax", out _taxId);
                                _taxId = "JTX" + _taxId;
                            }
                            DataRow dr = dsTax.Tables[0].NewRow();
                            dr["Id"] = _taxId + "-" + (t + 1).ToString();

                            dr["OSTransformationPOId"] = data[0]["OSTransformationPOId"];
                            dr["OSTransformationPODetailId"] = data[0]["Id"];
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

                            dr["OSTransformationPOId"] = TaxList[t]["OSTransformationPOId"];
                            dr["OSTransformationPODetailId"] = TaxList[t]["OSTransformationPODetailId"];
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

                        decimal SumTax = 0;
                        if (TaxList != null)
                        {

                            for (int a = 0; a < TaxList.Count; a++)
                            {
                                decimal T1 = Convert.ToDecimal(TaxList[a]["TaxAmount"]);
                                SumTax = T1 + SumTax;
                            }
                        }

                        if (OutputMat.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = OutputMat.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();
                            dr["TaxAmount"] = SumTax;

                            //dr["AddedBy"] = identity.Name;
                            //dr["AddedDate"] = DateTime.Now.ToString();
                            //dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTax, OutputMat);
                return TaxList;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        #endregion

        public void JPOBOQMAPCreate(List<Dictionary<string, object>> data, string JWPODetailId, string userName, string IPAddress, out DataSet dsPOboq, List<Dictionary<string, object>> detailBoq)
        {
            try
            {
                DataSet DelJWPOBOQMap;
                DataSet DetailJWPOBOQ;
                dsPOboq = new DataSet();
                string sql = "";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (!string.IsNullOrEmpty(JWPODetailId))
                {
                    con.OpenDataSetThroughAdapter("SELECT * FROM OSPOBOQMAP WHERE OSTransformationPODetailId='" + JWPODetailId + "' ", out DelJWPOBOQMap, false, "1");
                    if (DelJWPOBOQMap.Tables[0].Rows.Count > 0)
                    {
                        // throw new Exception("First Delete Material Output Data");
                        con.BeginTransaction();

                        con.executeQuery("delete from OSPOBOQMAP where OSTransformationPODetailId='" + JWPODetailId + @"' ");

                        con.CommitTransaction();
                    }
                }

                string _poboqId = "";
                string MaterialId = "";
                string ArticleId = "";
                string SKU1Id = "0";
                string SKU2Id = "0";
                string SKU3Id = "0";
                string SOId = "";
                string DetailSKU1Id = "0";
                string DetailSKU2Id = "0";
                string DetailSKU3Id = "0";
                sql = "SELECT * FROM OSPOBOQMAP WHERE OSTransformationPODetailId='" + JWPODetailId + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsPOboq, false, "1");
                Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();


                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i]["Id"].ToString() == JWPODetailId)
                    {
                        MaterialId = data[i]["MaterialMasterId"].ToString();
                        ArticleId = data[i]["ArticleId"].ToString();

                        if (data[i]["FirstCharacteristicsValueId"] != null)
                        {
                            SKU1Id = data[i]["FirstCharacteristicsValueId"].ToString();
                        }

                        if (data[i]["SecondCharacteristicsValueId"] != null)
                        {
                            SKU2Id = data[i]["SecondCharacteristicsValueId"].ToString();
                        }

                        if (data[i]["ThirdCharacteristicsValueId"] != null)
                        {
                            SKU3Id = data[i]["ThirdCharacteristicsValueId"].ToString();
                        }


                        SOId = data[i]["SalesOrderId"].ToString();

                    }
                }

                for (int a = 0; a < detailBoq.Count; a++)
                {

                    if (detailBoq[a]["FirstCharacteristicsValueId"] != null)
                    {
                        DetailSKU1Id = detailBoq[a]["FirstCharacteristicsValueId"].ToString();
                    }

                    if (detailBoq[a]["SecondCharacteristicsValueId"] != null)
                    {
                        DetailSKU2Id = detailBoq[a]["SecondCharacteristicsValueId"].ToString();
                    }

                    if (detailBoq[a]["ThirdCharacteristicsValueId"] != null)
                    {
                        DetailSKU3Id = detailBoq[a]["ThirdCharacteristicsValueId"].ToString();
                    }

                    if (detailBoq[a]["MaterialMasterId"].ToString() == MaterialId && detailBoq[a]["ArticleId"].ToString() == ArticleId && DetailSKU1Id == SKU1Id && DetailSKU2Id == SKU2Id && DetailSKU3Id == SKU3Id)
                    {

                        dsPOboq.Tables[0].DefaultView.RowFilter = "BOQDetailId = '" + detailBoq[a]["BOQId"] + "'  ";
                        //  dsPOboq.Tables[0].DefaultView.RowFilter = "BOQDetailId IN (" + BOQId + ") ";

                        if (dsPOboq.Tables[0].DefaultView.Count == 0)
                        {
                            if (_poboqId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("OSPOBOQMAP", out _poboqId);
                                _poboqId = "JPB" + _poboqId;

                            }
                            DataRow dr = dsPOboq.Tables[0].NewRow();

                            double conversiongroupListData = conversion.Convert(detailBoq[a]["MaterialMasterId"].ToString(), bplib.clsWebLib.RetValidLen(detailBoq[a]["TransactionUoMId"]).ToString(), bplib.clsWebLib.RetValidLen(detailBoq[a]["BaseUOMId"]).ToString(), clsStaticInfo.dbl(detailBoq[a]["TransactionQty"]));
                            dr["BaseQty"] = Convert.ToDecimal(conversiongroupListData);

                            dr["Id"] = _poboqId + "-" + (a + 1).ToString();

                            dr["OSTransformationPODetailId"] = JWPODetailId;
                            dr["BOQDetailId"] = detailBoq[a]["BOQId"];
                            dr["TransactionQty"] = detailBoq[a]["TransactionQty"];
                            dr["TransactionUoMId"] = detailBoq[a]["TransactionUoMId"];

                            dr["BaseUoMId"] = bplib.clsWebLib.RetValidLen(detailBoq[a]["BaseUOMId"]).ToString() == "" ? null : bplib.clsWebLib.RetValidLen(detailBoq[a]["BaseUOMId"]).ToString();
                            dr["POBOQQty"] = conversion.Convert(detailBoq[a]["MaterialMasterId"].ToString(), bplib.clsWebLib.RetValidLen(detailBoq[a]["TransactionUoMId"]).ToString(), bplib.clsWebLib.RetValidLen(detailBoq[a]["POUoMId"]).ToString(), clsStaticInfo.dbl(detailBoq[a]["TransactionQty"]));

                            dr["POUoMId"] = detailBoq[a]["POUoMId"];

                            dr["AddedBy"] = userName;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = IPAddress;

                            dsPOboq.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsPOboq.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["OSTransformationPODetailId"] = JWPODetailId;
                            dr["BOQDetailId"] = detailBoq[a]["BOQId"];
                            dr["TransactionQty"] = detailBoq[a]["TransactionQty"];
                            dr["TransactionUoMId"] = detailBoq[a]["TransactionUoMId"];
                            dr["BaseUoMId"] = bplib.clsWebLib.RetValidLen(detailBoq[a]["BaseUOMId"]).ToString() == "" ? null : bplib.clsWebLib.RetValidLen(detailBoq[a]["BaseUOMId"]).ToString();
                            dr["POBOQQty"] = conversion.Convert(detailBoq[a]["MaterialMasterId"].ToString(), bplib.clsWebLib.RetValidLen(detailBoq[a]["TransactionUoMId"]).ToString(), bplib.clsWebLib.RetValidLen(detailBoq[a]["POUoMId"]).ToString(), clsStaticInfo.dbl(detailBoq[a]["TransactionQty"]));

                            dr["POUoMId"] = detailBoq[a]["POUoMId"];
                            dr["UpdatedBy"] = userName;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = IPAddress;
                            dr.EndEdit();

                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPOboq);

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

            //                     LEFT JOIN (select Sum(TaxAmount) TaxAmount,OSTransformationPODetailId,OSTransformationPOId from OSTransformationPOTax  where  OSTransformationPOId = 'JWP17' GROUP BY OSTransformationPOId, OSTransformationPODetailId ) jwtax 
            //ON jwtax.OSTransformationPOId  = JWTPD.OSTransformationPOId and  jwtax.OSTransformationPODetailId  = JWTPD.Id 
            //                   WHERE " + strkey + "  and JWTPD.OSTransformationPOId = '" + jwpoId + @"'";
            string sql = @"SELECT JWA.Id JobWorkActivityId, JWA.UserName JobWorkActivity,JWTPD.*,ISNULL(JWI.UserName,'') JWItemName,ISNULL(JWItemUOM.Code,'') JWItemUOM,MM.Code as MaterialCode
                          -- ,MM.Id MaterialMasterId 
						   ,ISNULL(MM.UserName,'') MaterialMasterName
                            --,MMA.Id ArticleId
							,ISNULL(MMA.ShortName,'') ArticleName, MMA.Code as ArticleCode
                            ,isnull(FChar.ValueAssignmentLevel,'') ValueAssignmentLevel
                            ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue,ISNULL(FCharValue.Code,'') SKU1ValueCode
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue,ISNULL(SCharValue.Code,'') SKU2ValueCode
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue,ISNULL(TCharValue.Code,'') SKU3ValueCode
                                ,ISNULL(BaseUOM.Code,'') BaseUOM,ISNULL(TransactionUoM.Code,'') TransactionUoM
                                ,ISNULL(Country.UserName,'') Country
                                --JWTransfromation Detail 
                                ,JWA.UserName JWActivity,
	                            JWTPD.ResponsiblePersonId
                                ,EEI.EmployeeName ResponsiblePerson,EEI.EmployeeCode
                                , JWTPD.JobWorkItemMasterId, JWI.UserName OutputMaterial, JWTPD.OutputMaterialUOMId
                                , JWTPD.RateApplyOn,JWTPD.CurrencyId, CURR.Code CURR--, JWTPD.MinRate, JWTM.MaxRate
                                , JWTPD.ByProductApplicable 
                                ,JWTPD.Quantity	TransactionQty	
	                            ,JWTPD.RatePerUnit	TransactionRate
	                            ,(JWTPD.Quantity*JWTPD.RatePerUnit) TransactionAmount
                            , JWTPD.ReferenceNo,((JWTPD.Quantity*JWTPD.RatePerUnit)*po.ToCurrencyRate) BaseAmount
                            , jwtax.TaxAmount,JWTPD.TransactionUoMId,TransactionUoM.Code TransactionUoM,JWTPD.BaseUOMId,BaseUOM.Code BaseUOM
                            ,MS.Id MaterialStorageId,MS.UserName MaterialStorage,EEI.EmployeeName ResponsiblePerson ,ISNULL(MM.UserName,'') MaterialName
                            ,SerM.UserName as JWService, NULL as BOQId
                            --,FORMAT(JWTPD.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate
                            FROM OSTransformationPODetail JWTPD      
                            left JOIN [dbo].[OSTransformationPO] PO On PO.Id=JWTPD.OSTransformationPOId
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
    
                            LEFT JOIN (select Sum(TaxAmount) TaxAmount,OSTransformationPODetailId,OSTransformationPOId from OSTransformationPOTax  where  OSTransformationPOId = '" + jwpoId + @"' GROUP BY OSTransformationPOId, OSTransformationPODetailId ) jwtax 
                            ON jwtax.OSTransformationPOId  = JWTPD.OSTransformationPOId and  jwtax.OSTransformationPODetailId  = JWTPD.Id 
                            left join HKP.JobWorkLocation JL on JL.Id=JWTPD.MaterialLocationId
							left join hkp.MaterialStorage MS ON MS.Id=JL.StoreLocationId
                            left join hkp.ServiceMaster SerM on SerM.Id=JWTPD.ServiceId
                            WHERE " + strkey + "  and JWTPD.OSTransformationPOId = '" + jwpoId + @"'";
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
            //                     LEFT JOIN (select Sum(TaxAmount) TaxAmount,OSTransformationPODetailId,OSTransformationPOId from OSTransformationPOTax   GROUP BY OSTransformationPOId, OSTransformationPODetailId ) jwtax 
            //on jwtax.OSTransformationPOId  = JWTPD.OSTransformationPOId and  jwtax.OSTransformationPODetailId  = JWTPD.Id 
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
            //                        FROM dbo.OSTransformationPODetail JWTPD 
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
            //                     LEFT JOIN (select Sum(TaxAmount) TaxAmount,OSTransformationPODetailId,OSTransformationPOId from OSTransformationPOTax   GROUP BY OSTransformationPOId, OSTransformationPODetailId ) jwtax 
            //on jwtax.OSTransformationPOId  = JWTPD.OSTransformationPOId and  jwtax.OSTransformationPODetailId  = JWTPD.Id 
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
                            FROM OSTransformationPODetail JWTPD      
                            left JOIN [dbo].[OSTransformationPO] PO On PO.Id=JWTPD.OSTransformationPOId
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
    
                            LEFT JOIN (select Sum(TaxAmount) TaxAmount,OSTransformationPODetailId,OSTransformationPOId from OSTransformationPOTax GROUP BY OSTransformationPOId, OSTransformationPODetailId ) jwtax 
                            ON jwtax.OSTransformationPOId  = JWTPD.OSTransformationPOId and  jwtax.OSTransformationPODetailId  = JWTPD.Id 
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
                        , A.OSTransformationPOId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount
                        , POT.Amount-A.Amount AS  Bal
                        , POT.Amount As POAmount
                        --, A.TotalTaxAmount
                        ,A.POID
						,A.POServiceId,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].[OSTransformationPOService] AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                        left JOIN (select Id, Amount from TRN.POService) AS POT on A.POServiceId=POT.Id
                        left join ( Select OSTransformationPOId, sum(TaxAmount) TaxAmount FROM  OSTransformationPOTax group by ServiceMasterId where OSTransformationPODetailId is null) IRT On IRT.OSTransformationPOId=A.Id
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
                                                    JOIN OSTransformationPO AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN OSTransformationPO AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage],'0') AS [Percentage], 0 TaxAmount
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
            var sql = @"SELECT A.Id, A.OSTransformationPOId  InventoryReceiveId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.TransactionAmount
                            --, A.TotalTaxAmount
                            ,POT.TaxAmount As TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM 
                           OSTransformationPOService AS A 
                            INner JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (SELECT ServiceMasterId,Sum(TaxAmount) as TaxAmount  from OSTransformationPOTax 
                            WHERE  ISNULL(OSTransformationPOId,'') ='" + jwpoId + @"' group by ServiceMasterId
                            ) AS POT on A.ServiceMasterId=POT.ServiceMasterId
                            WHERE A.OSTransformationPOId='" + jwpoId + @"' ";

            return sql;//.Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public string GetPODetailServiceChargeList(string jwpoId, string jwpodId)
        {
            var sql = @"SELECT A.Id, A.OSTransformationPOId  InventoryReceiveId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.TransactionAmount
                            --, A.TotalTaxAmount
                            ,POT.TaxAmount As TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM 
                           OSTransformationPOService AS A 
                            INner JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (select ServiceMasterId InventoryServiceId,Sum(TaxAmount) as TaxAmount  from OSTransformationPOTax group by ServiceMasterId 
                            where ISNULL(OSTransformationPODetailId,'') = '" + jwpodId + @"' and ISNULL(OSTransformationPOId,'') ='" + jwpoId + @"' 
                             ) AS POT on A.id=POT.InventoryServiceId
                            WHERE A.OSTransformationPOId='" + jwpoId + @"'";

            return sql;
        }

        public string GetServiceTaxList(string serviceId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.OSTransformationPOId,A.OSTransformationPODetailId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM OSTransformationPOTax AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.OSTransformationPOId='" + serviceId + @"' AND A.OSTransformationPODetailId IS NULL ORDER BY TC.[Sequence]";
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
                var sql = @"SELECT A.Id,A.OSTransformationPOId,A.OSTransformationPODetailId, A.TaxCategoryId, TC.UserName, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM OSTransformationPOTax AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.OSTransformationPOId='" + jwPOId + @"' AND A.OSTransformationPODetailId = '" + jwPoDetailId + @"' ORDER BY TC.[Sequence]";
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
                dataInserted[dataInserted.Count - 1]["RequiredQty"] = 0;
                dataInserted[dataInserted.Count - 1]["ReferenceNoM"] = "";
                dataInserted[dataInserted.Count - 1]["BuyerItemReferenceNo"] = "";
                dataInserted[dataInserted.Count - 1]["BOQId"] = "' '";
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
                                dataInserted[KK]["RequiredQty"] = clsStaticInfo.dbl(dataInserted[KK]["RequiredQty"]) + clsStaticInfo.dbl(data[M]["RequiredQty"]);

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

                                if (data[M].ContainsKey("BuyerItemReferenceNo"))
                                {
                                    if (strTemp.Contains(data[M]["BuyerItemReferenceNo"].ToString()) == false)
                                    {
                                        strTemp.Add(data[M]["BuyerItemReferenceNo"].ToString());
                                        if (dataInserted[KK]["BuyerItemReferenceNo"].ToString() == "")
                                            dataInserted[KK]["BuyerItemReferenceNo"] = data[M]["BuyerItemReferenceNo"].ToString();
                                        else
                                            dataInserted[KK]["BuyerItemReferenceNo"] += "," + data[M]["BuyerItemReferenceNo"].ToString();
                                    }
                                }

                                if (data[M].ContainsKey("BOQId"))
                                {
                                    if (strTemp.Contains(data[M]["BOQId"].ToString()) == false)
                                    {
                                        strTemp.Add(data[M]["BOQId"].ToString());
                                        //if (dataInserted[KK]["BOQId"].ToString() == "")
                                        //    dataInserted[KK]["BOQId"] = data[M]["BOQId"].ToString();
                                        //else
                                        //    dataInserted[KK]["BOQId"] += ",'" + data[M]["BOQId"].ToString() + "' ";
                                        dataInserted[KK]["BOQId"] += ",'" + data[M]["BOQId"].ToString() + "' ";
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
							 where JWPA.OSTransformationPOId = '" + JWPODId + @"' ";

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
                //          sql = @"select mm.Id, mm.Code, mm.UserName as Material,mm.BaseUOMId, mmuom.UserName as BaseUom,jwi.UOMId, uom.UserName as JWIUom
                //               ,UnitId=case when jwi.MaterialMasterId is not null then mm.BaseUOMId else jwi.UOMId End
                //               ,UOM=case when jwi.MaterialMasterId is not null then mmuom.UserName else uom.UserName End
                //,AlternateUoM=case when jwi.MaterialMasterId is not null then U.UserName End
                //               from HKP.JobWorkItem jwi left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
                //               left join scs.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                //left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                //left join MST.MaterialMasterAlternativeUOM mauom on mauom.MaterialMasterId=mm.Id
                //left join SCS.UnitOfMeasurement U on U.Id=mauom.AlternativeUOMId
                //               where jwi.Id='" + JobWorkItemId + @"' ";

                sql = @"select --mm.BaseUOMId UoMId,mmuom.UserName UoM
                        mm.Id, mm.Code, mm.UserName as Material,mm.WithSKU,jwi.UOMId, uom.UserName as JWIUom
                         ,Value=case when jwi.MaterialMasterId is not null then mm.BaseUOMId else jwi.UOMId End
                         ,Text=case when jwi.MaterialMasterId is not null then mmuom.UserName else uom.UserName End
                        -- ,AlternateUoM=case when jwi.MaterialMasterId is not null then U.UserName End
                        from HKP.JobWorkItem jwi left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
                        left join scs.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                        left join MST.MaterialMasterAlternativeUOM mauom on mauom.MaterialMasterId=mm.Id
                        left join SCS.UnitOfMeasurement U on U.Id=mauom.AlternativeUOMId
                        where jwi.Id='" + JobWorkItemId + @"'

                         UNION ALL
                        select
                        --mauom.AlternativeUOMId UoMId,uom1.UserName UoM
                        mm.Id, mm.Code, mm.UserName as Material,mm.WithSKU,jwi.UOMId, uom.UserName as JWIUom
                         ,Value=case when jwi.MaterialMasterId is not null then mauom.AlternativeUOMId else jwi.UOMId End
                         ,Text=case when jwi.MaterialMasterId is not null then uom1.UserName else uom.UserName End
                        -- -- ,AlternateUoM=case when jwi.MaterialMasterId is not null then uom1.UserName End
                        from HKP.JobWorkItem jwi left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
                        left join scs.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                        left join MST.MaterialMasterAlternativeUOM mauom on mauom.MaterialMasterId=mm.Id
                        left join SCS.UnitOfMeasurement uom1 on uom1.Id=mauom.AlternativeUOMId
                        left join SCS.UnitOfMeasurement U on U.Id=mauom.AlternativeUOMId
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

        public IEnumerable<object> GetJWMaterialStorage(string JWLocId)
        {
            try
            {
                var sql = "";
                sql = @"select jl.Id as Value, jl.LocationName as Text, StoreLocationId,ms.UserName as MaterialStorage
                       from HKP.JobWorkLocation jl left join HKP.MaterialStorage ms on ms.Id=jl.StoreLocationId
                       where jl.Id='" + JWLocId + @"' order by LocationName ";

                var Data = _sqlRepository.GetDataCollection(sql);

                return Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GePurchaseOrderReport(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderId, string POType)
        {
            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            if (POType == "OSTransformationPO")
            {
                fileName = "JWPurchaseOrder" + plantId + ".docx";
            }
            else
            {
                fileName = "JWValAddedPurchaseOrder" + plantId + ".docx";
            }

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

                DataTable dsOrderMaster, dsServiceItems;
                dsOrderMaster = loadOrderMaster(purchaseOrderId);//sql
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
                dsServiceItems = loadServicerMasterItems(purchaseOrderId);
                var materialTotal = makeMaterialDetailsTable(document, dsOrderMaster, purchaseOrderId);//Material Details 
                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)
                {
                    //{ServiceItems}
                    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);
                }
                var DiscountAmount = "";
                DiscountAmount = dsOrderMaster.Rows[0]["DiscountAmount"].ToString();
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{DiscountAmount}", (DiscountAmount).ToString() + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{AfterDiscountTotal}", ((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

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
                string Prefix = "PurchaseOrder" + purchaseOrderId;
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

        public DataTable loadOrderMaster(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT PO.Id PONumber
                    ,HSNC.Code HSNCode
 	                ,CNO.ContractNo
					,PLC.LCRef LCNumber 
                    ,PLC.BenificiaryBank BeneficiaryBank
                    ,PLC.BenificiaryBank OpeningBank
					--,B.UserName BeneficiaryBank
					--,B.UserName OpeningBank
                    ,PO.CompanyGroupId
                  --  ,PO.CompanyId
                    ,Plant.GSTIN
	                ,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
                    ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                   -- ,POType=CASE WHEN PO.POType='PO' then 'PO Without Requisition' ELSE 'PO With Requisition' END
                    ,POType=CASE WHEN PO.POType='OSTransformationPO' then 'Transformation PO' ELSE 'Value Added PO' END
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
                    ,POD.MaterialMasterId MaterialMasterId
                    ,PO.DocRefNo
                    ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
                    ,CheckedBy=CASE WHEN PO.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                    ,AuthorizedBy=CASE When PO.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                    ,AddedBy=CASE When PO.CheckedByStatus='pending' OR PO.CheckedByStatus='Hold' OR PO.CheckedByStatus='Reject' OR PO.CheckedByStatus='Checked'then eI3.EmployeeName else PO.AddedBy  END 
                    ,PO.AddedDate
                    ,PO.UpdatedBy
                    ,PO.UpdatedDate
                    ,PO.IsApproved
                    ,PO.PartyId
                   -- ,POD.RefferenceNo
				    --,POD.ReferenceNo as RefferenceNo
					,RefferenceNo=case when POD.ReferenceNo is not null then POD.ReferenceNo else POD.MaterialReference End
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
                    ,ROUND(POD.Quantity, 2) POTransactionQty
                    ,ROUND(POD.RatePerUnit, 4) TransactionRate
                    ,ROUND((POD.Quantity * POD.RatePerUnit), 2) AS TrnAmount
                    ,POD.BaseAmount
                   -- ,POD.TotalTaxAmount AS BaseTaxAmount
				    ,BaseTaxAmount= (
                    SELECT SUM(TaxAmount)
                    FROM [TRN].[PurchaseOrderTax]
                    WHERE InventoryReceiveDetailId = POD.Id
                    )
                    ,REPLACE(Convert(VARCHAR(11), POD.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                    ,TaxAmount = (
                    SELECT SUM(TaxAmount)
                    FROM [TRN].[PurchaseOrderTax]
                    WHERE InventoryReceiveDetailId = POD.Id
                    )
                    ,ServiceTaxAmount = (
                    SELECT SUM(TotalTaxAmount)
                    FROM [TRN].[POService]
                   -- WHERE InventoryReceiveId = POD.InventoryReceiveId
				   WHERE InventoryReceiveId = POD.OSTransformationPOId
                    )
                    --,POD.Description
					,Description=case when POD.Description is not null then POD.Description else POD.MaterialSpecification End
                    --,POD.ChargesAmount
					,POD.BaseAmount as ChargesAmount
                    ,POD.CountryId
                    ,POCountry.UserName CountryOfOrigin
                    ,POD.Id PurchaseOrderDetailId
                    ,POD.TransactionUoMId
                    -- ,TUoM.ShortName AS TransactionUoM
					,TransactionUoM=case when POD.TransactionUoMId is not null then TUoM.ShortName else UOM.UserName End
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
                    FROM dbo.OSTransformationPO PO --TRN.PurchaseOrder PO
                    LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                 --   LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                    LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                    LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                    LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                    LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                    LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                    LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId
                --    LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId
				   LEFT JOIN dbo.OSTransformationPODetail POD ON PO.Id = POD.OSTransformationPOId
					LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
					LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
	               -- LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                    LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
                    LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId
                  --  LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId
				    LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.MaterialMasterId
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
					left join SCS.UnitOfMeasurement UOM on UOM.Id=POD.OutputMaterialUOMId
                 --   LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id=POD.RequisitionDetailId
                    LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.MaterialMasterId=POD.MaterialMasterId and MRMD.ArticleId=POD.ArticleId
                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=PO.CheckedBy
                    LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=PO.AuthorizedBy
                    left join [SEC].[User] U on U.UserId=PO.AddedBy
                    LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
                WHERE PO.Id = '" + purchaseOrderId + @"' ";
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

        public DataTable loadServicerMasterItems(string purchaseOrderId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT POS.Id ServiceId,SM.UserName  Service , POS.Description--, POS.Amount
                             ,POS.TransactionAmount as Amount
                            ,POS.TotalTaxAmount,Pos.AddedBy,pos.AddedDate,pos.UpdatedBy,pos.UpdatedDate 
                            FROM dbo.OSTransformationPO PO --TRN.PurchaseOrder PO
                            --INNER join TRN.POService POS ON POS.InventoryReceiveId = PO.Id
							left join dbo.OSTransformationPOService POS ON POS.OSTransformationPOId = PO.Id
                            left JOIN HKP.ServiceMaster SM ON POS.ServiceMasterId = SM.Id 
                            where PO.Id = '" + purchaseOrderId + @"'";


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

        public DataTable loadMaterialTax(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"select   PODT.ServiceMasterId InventoryServiceId,
                            PO.Id PurchaseOrderId,POD.Id PurchaseOrderDetailId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount 
                            from dbo.OSTransformationPO PO --TRN.PurchaseOrder PO
                            --INNER JOIN TRN.PurchaseOrderDetail POD ON POD.InventoryReceiveId = PO.Id
							left JOIN dbo.OSTransformationPODetail POD ON POD.OSTransformationPOId = PO.Id
                            --Inner join TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryReceiveDetailId = POD.Id
							LEFT join dbo.OSTransformationPOTax PODT ON PODT.OSTransformationPOId = PO.Id and PODT.OSTransformationPODetailId = POD.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                            WHERE PO.Id='" + purchaseOrderId + @"' 
							--and InventoryReceiveDetailId  is not null and  InventoryServiceId is null AND PODT.Percentage > 0 
							and PODT.OSTransformationPODetailId  is not null and PODT.ServiceMasterId is null 
							AND PODT.Percentage > 0 
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

        public double makeMaterialDetailsTable(WordDocument document, DataTable dsOrderMaster, string purchaseOrderId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            //clsDataContext data = new clsDataContext();
            dsTax = loadMaterialTax(purchaseOrderId);
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

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 75;



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
            wTable.Rows[ROW].Cells[colChar3].Width = 35;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN No");
            //range.ApplyCharacterFormat(FontBold);
            //int colHSNCode = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar3].Width = 40;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Description");
            range.ApplyCharacterFormat(FontBold);
            int colMatDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colMatDescription].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            range.ApplyCharacterFormat(FontBold);
            int colDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colDescription].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Reff No");
            range.ApplyCharacterFormat(FontBold);
            int colRefferenceNo = COL; COL++;
            //wTable.Rows[ROW].Cells[colRefferenceNo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Delivery Date");
            range.ApplyCharacterFormat(FontBold);
            int colDeliveryDate = COL; COL++;
            //wTable.Rows[ROW].Cells[colDeliveryDate].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Origin");//TRN.PurchaseOrderDetail ->CountryId
            range.ApplyCharacterFormat(FontBold);
            int colOriginCountry = COL; COL++;
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
            //    #endregion column headers

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
                TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                TROW.Cells[colRefferenceNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["RefferenceNo"].ToString());
                TROW.Cells[colDeliveryDate].AddParagraph().AppendText(dsOrderMaster.Rows[i]["DeliveryDate"].ToString());
                TROW.Cells[colOriginCountry].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CountryOfOrigin"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUOM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());
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
                if (C == colMaterialGroup || C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colUOM || C == colMatDescription || C == colRefferenceNo || C == colDescription || C == colDeliveryDate || C == colOriginCountry || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }

            #endregion Total
            ROW++;
            #region Sub Total
            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());
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

        public double makeServiceDetailsTable(WordDocument document, DataTable dsServiceItems, string purchaseOrderId)
        {
            string replaceString = "{ServiceItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;
            //clsDataContext data = new clsDataContext();
            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            dsTax = loadServiceMasterTax(purchaseOrderId);
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
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsServiceItems.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStaticInfo.dbl(dsServiceItems.Rows[i]["Amount"].ToString());
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
                        value += clsStaticInfo.dbl(item.Text);
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

            double total = clsStaticInfo.dbl(dsServiceItems.Compute("SUM(Amount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

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

        public DataTable loadServiceMasterTax(string purchaseOrderId)
        {
            string strSQL;

            try
            {
                //        strSQL = @"SELECT InventoryServiceId,PO.Id PurchaseOrderId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
                //                    INNER JOIN TRN.POService POS ON POS.InventoryReceiveId = PO.Id
                //                    INNER JOIN TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryServiceId = POS.Id
                //                      LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                //                        WHERE PO.Id='" + purchaseOrderId + @"' 
                //AND InventoryServiceId   IS NOT NULL AND  InventoryReceiveDetailId IS NULL 
                // ORDER BY tg.[Sequence] ";

                strSQL = @"SELECT PODT.ServiceMasterId InventoryServiceId,
                            PO.Id PurchaseOrderId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount 
                            from dbo.OSTransformationPO PO --TRN.PurchaseOrder PO
                            --INNER JOIN TRN.POService POS ON POS.InventoryReceiveId = PO.Id
							left JOIN dbo.OSTransformationPOService POS ON POS.OSTransformationPOId = PO.Id
                            --INNER JOIN TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryServiceId = POS.Id
							left JOIN dbo.OSTransformationPODetail POD ON POD.OSTransformationPOId = PO.Id
							 LEFT JOIN dbo.OSTransformationPOTax PODT ON PODT.OSTransformationPOId = PO.Id and PODT.OSTransformationPODetailId = POD.Id
                              LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                                WHERE PO.Id='" + purchaseOrderId + @"' 
								--AND InventoryServiceId   IS NOT NULL 
								-- and InventoryReceiveDetailId is null
								AND PODT.ServiceMasterId   IS NOT NULL 
								AND  PODT.OSTransformationPODetailId IS NULL 
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

        // DOCUMENT SAVE

        private string OSPODocumentMap()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(OSPODocumentMap), out sID);
            return sID;
        }

        public void InsertPODocMap(PODocumentMap entity, string POId, out string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [dbo].[OSPODocumentMap] WHERE Id='" + entity.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = entity.POId + "-" + OSPODocumentMap();
                    var createdId = dr["Id"];
                    dr["POId"] = entity.POId;

                    dr["CompanyGroupId"] = entity.CompanyGroupId;

                    dr["UserFilename"] = entity.UserFilename;
                    dr["SystemFileName"] = createdId + Path.GetExtension(entity.UserFilename);
                    dr["Description"] = entity.Description;
                    dr["Remarks"] = entity.Remarks;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                //else
                //{
                //    //edit
                //    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                //    dr.BeginEdit();

                //    dr["FileName"] = data.FileName;
                //    dr["Description"] = data.Description;
                //    dr["IssueTransactionId"] = data.IssueTransactionId;

                //    dr["UpdatedBy"] = identity.EmployeeId;
                //    dr["UpdatedFromIP"] = identity.IPAddress;
                //    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                //    dr.EndEdit();
                //}

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public IEnumerable<object> PODocumentMapData(string POID)
        {
            try
            {
                var _sql = @"SELECT
								Id
							  ,CompanyGroupId
							  
							  ,POId
							  ,UserFilename 
							  ,SystemFileName
							  ,Description
							  ,Remarks
							  ,AddedBy
							  ,AddedDate
							  ,AddedFromIP
							  ,UpdatedBy
							  ,UpdatedDate
							  ,UpdatedFromIP
						  FROM dbo.OSPODocumentMap 
							where POId='" + POID + @"'
							ORDER BY UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GRNImageDelete(string Id)
        {
            try
            {
                var _sql = @" Delete from [dbo].[OSPODocumentMap] where Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> PODocumentMapDataAll(string POID)
        {
            try
            {
                var _sql = @"DECLARE @pathval varchar(200)='POPResources/PurchaseOrder'
							SELECT POId,Remarks,'<a href='''  + @pathval+'/'+SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>' As UserFilename,Description
							--stuff(
							--(
							--  SELECT '<a href=''' + SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>'
							--  FROM [TRN].[GRNDocumentMap] 	 WHERE GRNId = t.GRNId FOR XML path('')
							--),1,1,' ') UserFilename
							FROM (select Id,CompanyGroupId	,POId,UserFilename ,SystemFileName,Description,Remarks,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP 
							FROM [dbo].[OSPODocumentMap] )t
							ORDER BY t.UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> getMatInputListBOQData(string Id)
        {
            try
            {
                //var _sql = @"select mi.*
                //                ,mm.Id as MaterialMasterId
                //                ,mm.UserName as Material, mm.Code as MaterialCode
                //                , mma.StandardName as Article, mma.Code as ArticleCode
                //                ,uom.UserName as MatBaseUoM,mm.BaseUOMId
                //                ,CurrentReqQty=(om.Quantity * mi.NetConsumption) * (1 + (mi.ValueLoss/100))
                //                from dbo.JobWorkTransformationContractChild3 mi 
                //                left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                //                left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                //                left join SCS.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                //                left join dbo.OSTransformationPODetail om on om.Id=mi.OSTransformationPODetailId
                //                where mi.OSTransformationPODetailId='" + Id + @"' ";

                var _sql = @"select mm.Id as MaterialMasterId,mi.NetConsumption as NETCon,mi.Rejection as Rej, mi.ValueLoss as ValLoss--, mi.GrossConsumption as GrConsump
                                  ,ROUND(mi.GrossConsumption,4) as GrConsump
                                 ,mi.BOQRequiredQuantity
                                ,mm.UserName as Material, mm.Code as MaterialCode
                                , mma.StandardName as Article, mma.Code as ArticleCode
                                ,uom.UserName as MatBaseUoM,mm.BaseUOMId
                                --,CurrentReqQty=(om.Quantity * KK.NetConsumption) * (1 + (KK.ValueLoss/100))
                                ,CurrentReqQty=(om.Quantity * mi.NetConsumption) * (1 + (mi.ValueLoss/100))
								,KK.BOQReqQty,KK.NetConsumption,kk.Rejection,KK.ValueLoss,KK.GrossConsumption
                                from dbo.OSTransformationPOInputMaterial mi 
                                left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                                left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                                left join SCS.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                                left join dbo.OSTransformationPODetail om on om.Id=mi.OSTransformationPODetailId
								left join (Select ArticleId,SUM(BOQRequiredQuantity) as BOQReqQty, Sum(NetConsumption) as NetConsumption, Sum(Rejection) as Rejection
								,Sum(ValueLoss) ValueLoss, Sum(GrossConsumption) GrossConsumption from dbo.OSTransformationPOInputMaterial group by ArticleId)
								KK on KK.ArticleId=mma.Id
                                where mi.OSTransformationPODetailId='" + Id + @"'
								group by mm.Id,mma.Code,mm.UserName,mma.StandardName,mm.Code,uom.UserName
								,mm.BaseUOMId,om.Quantity,KK.BOQReqQty,KK.NetConsumption,kk.Rejection,KK.ValueLoss,KK.GrossConsumption
                                ,mi.NetConsumption,mi.Rejection, mi.ValueLoss, mi.GrossConsumption,mi.BOQRequiredQuantity ";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DelMaterialInputBOQ(string Id)
        {
            try
            {
                //       DataSet dsMaster;
                ConnectionManager.DAL.ConManager con2 = new ConnectionManager.DAL.ConManager("1");

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                //if (!string.IsNullOrEmpty(Id))
                //{
                //    con2.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPOMasterOrderItem where OSTransformationPODetailId='" + id + "' ", out dsMaster, false, "1");
                //    if (dsMaster.Tables[0].Rows.Count > 0)
                //    {
                //        throw new Exception("First Delete Order Wise Data");
                //    }

                //    con2.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationContractChild3 where OSTransformationPODetailId='" + id + "' ", out dsMaster, false, "1");
                //    if (dsMaster.Tables[0].Rows.Count > 0)
                //    {
                //        throw new Exception("First Delete Material Input Data");
                //    }

                //}

                con.executeQuery("delete from dbo.JobWorkTransformationContractChild3 where Id='" + Id + @"' ");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public IEnumerable<object> GetSalesOrderData(string Id)
        {
            try
            {
                var _sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,POD.ProductionOrderId
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,CASE WHEN isnull(so.WeekNo,0)=0 THEN  DATEPART(week,so.DeliveryDate) ELSE so.WeekNo END AS DeliveryWeek
	                            , Flag = CAST(0 AS BIT),SO.DestinationDescription
								,CN.ContractNo,MLC.LCRef MasterLCNo, Uom.UserName as MasterOrderUoM
                       FROM [TRN].[SalesOrder] AS SO 
                        left outer join [TRN].[ProductionOrderDetail] POD on POD.SalesOrderId=SO.Id 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       LEFT JOIN dbo.[Contract] AS CN ON CN.Id=MOI.ContractId
                       LEFT JOIN dbo.MasterLC AS MLC ON MLC.Id=CN.MasterLCId
					   left join SCS.UnitOfMeasurement Uom on Uom.Id=MO.TotalQtyUOMId ";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> LoadAllSKU(string MaterialMstId, string assignment, string charId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = "";
                //var _sql = @"select cv.Id,cv.Sequence,cv.Code,cv.ShortName,cv.StandardName,cv.UserName,cv.CharacteristicsId,C.UserName as FirstCharacteristics,cv.MaterialMasterId,mm.UserName as MaterialMst
                //                from HKP.CharacteristicsValue cv left join HKP.Characteristics C on cv.CharacteristicsId=C.Id
                //                left join MST.MaterialMaster mm on mm.Id=cv.MaterialMasterId
                //                where MaterialMasterId='"+ MaterialMstId + @"' order by cv.Sequence ";

                if (assignment == ValueAssignmentEnum.General.ToString())
                    _sql = @"SELECT Id AS CharacteristicsValueId, CompanyGroupId, CharacteristicsId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.CharacteristicsValue WHERE CompanyGroupId='" + identity.CompanyGroupId + @"' AND CharacteristicsId='" + charId + "' AND SourceType='" + assignment + "'";
                else
                    _sql = @"SELECT Id AS CharacteristicsValueId, CompanyGroupId, CharacteristicsId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.CharacteristicsValue WHERE CompanyGroupId='" + identity.CompanyGroupId + @"' AND CharacteristicsId='" + charId + "' AND MaterialMasterId='" + MaterialMstId + "' AND SourceType='" + assignment + "'";
                //      return _sqlRepository.GetGridData(parameters);

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> GetProductionOredrList(string entityid, string processid, string column, string value)
        {
            string sql = @"select * from (SELECT Flag=CAST(0 AS BIT), so.Customer,so.Article,so.StyleNo, so.OwnStyleNo, so.Product,
                            PO.Id POId,s.UserName AS POStatus,so.SONo,SO.SOQuantity SOQty,ISNULL(PO.Qty,0) AS POQuantity, So.LineItemId,SO.SOStatus
                            FROM [TRN].[ProductionOrder] AS PO  JOIN TRN.ProductionOrderProcessSet POP ON POP.ProductionOrderId=PO.Id                          
                            LEFT OUTER  JOIN (select pod.ProductionOrderId, sum(so.Qty) AS SOQuantity,
                                                    LineItemId=STUFF((select distinct ','+XMOI.Id from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
													SOStatus=STUFF((select distinct ','+OS.UserName from 
								                                 HKP.OrderStatus OS 
								                                INNER JOIN trn.SalesOrder AS sox on OS.Id=SOX.OrderStatusId
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                                   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													
                                                     ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													
                            from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId
                            ) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.entityid='" + entityid + "' AND POP.ProcessId='"+processid+@"' AND S.UserName<>'Closed') AS TEMP";

            return _sqlRepository.GetDataCollection(sql, null);
        }
        public IEnumerable<object> GetProductionOredrMapList(string entityid, string processid, string osPOId)
        {
            string sql = @"select * from (SELECT Flag=CAST(0 AS BIT), so.Customer,so.Article,so.StyleNo, so.OwnStyleNo, so.Product,
                            PO.Id POId,s.UserName AS POStatus,so.SONo,SO.SOQuantity SOQty,ISNULL(PO.Qty,0) AS POQuantity, So.LineItemId,SO.SOStatus
                            FROM [TRN].[ProductionOrder] AS PO  
                            JOIN TRN.ProductionOrderProcessSet POP ON POP.ProductionOrderId=PO.Id  
                            JOIN  [dbo].[OSPOProductionOrderMap] MAP ON MAP.ProductionOrderId=PO.Id
                            LEFT OUTER  JOIN (select pod.ProductionOrderId, sum(so.Qty) AS SOQuantity,
                                                    LineItemId=STUFF((select distinct ','+XMOI.Id from 
								                            TRN.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
													SOStatus=STUFF((select distinct ','+OS.UserName from 
								                                 HKP.OrderStatus OS 
								                                INNER JOIN trn.SalesOrder AS sox on OS.Id=SOX.OrderStatusId
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                                   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													
                                                     ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													
                            from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId
                            ) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.entityid='" + entityid + "' AND POP.ProcessId='" + processid + "' AND  MAP.OSTransformationPOId='"+osPOId+@"' AND S.UserName<>'Closed') AS TEMP";

            return _sqlRepository.GetDataCollection(sql, null);
        }
    }
}
