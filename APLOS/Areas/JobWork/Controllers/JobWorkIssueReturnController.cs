using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using OTSBD;
using Library.MaterialManagement.JobWork;
using Library.Model.Enums;
using Syncfusion.XlsIO;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkIssueReturnController : BaseController
    {
        JobWorkIssueReturn JWTIR = new JobWorkIssueReturn();

        string TableName = "dbo.JobWorkIssueReturn";
        string TableName1 = "dbo.JobWorkIssueReturnChild";
        string TableName2 = "JobWorkTransformationIssueReturn";
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkIssueReturnController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
            JWTIR = new JobWorkIssueReturn();
        }
        #endregion
        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Dropdown Code Area

        //[HttpGet, Authorize]
        //public JsonResult gejobworklocation()
        //{
        //    string sql = "";
        //    sql = @"select Id as Value, LocationName as Text from HKP.JobWorkLocation order by LocationName";

        //    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetMaterialCode()
        {
            string sql = "";
            sql = @"select Id as Value, Code as Text  from MST.MaterialMaster order by Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult gearticlecode(string MaterialCodeId)
        {
            string sql = "";
            sql = @"select Id as Value, StandardName as Text from MST.MaterialMasterArticle where MaterialMasterId='"+ MaterialCodeId + "' order by StandardName ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetIndividualReportData(string Id)
        //{
        //    string sql = "";
        //    sql = @"select distinct tir.Id,tc.Id as ContractId, tir.Date, FORMAT(tir.Date,'dd-MMM-yyyy') as IssueDate, tir.ByWhomId, tir.IssueReturn, tir.JobWorkLocationId, tir.Remarks
        //           ,emp.EmployeeName, emp.EmployeeCode, jl.LocationName
        //            from dbo.JobWorkTransformationIssueReturn tir left join dbo.JobWorkTransformationIssueReturnChild tirc on tir.Id=tirc.TransformationIssueReturnMasterId
        //            left join dbo.EmployeeInformation emp on emp.SystemId=tir.ByWhomId
        //            left join HKP.JobWorkLocation jl on jl.Id=tir.JobWorkLocationId
        //            left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tirc.MaterialInputId
        //            left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
        //            left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
        //            where tc.Id='" + Id + @"' order by tir.Date desc ";

        //    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        //}

        #endregion

        #region Load Data

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.JobWorkValueAddedContract where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value, string Type)
        {
            string sql = "";
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if(Type== "Value Added")
            {
                //sql = @"select vac.Id,TabType='Value Added', vac.EntityId,vac.PartyId,vac.Remarks,FORMAT(vac.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5)
                //                           ,vac.[Time],108)[VACTime],FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate
                //                           ,FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate
                //                           ,e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                //                           from dbo.JWTransformationPurchaseOrder vac left join ORG.Entity e on e.Id=vac.EntityId
                //                           left join HKP.Party p on p.Id=vac.PartyId
                //                           WHERE " + strkey + " and POType='OSValueAddedPO' order by ValueAddedDate desc ";


                sql = @"select * from(
							SELECT  ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Value Added'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='"+identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						WHERE " + strkey + @" and  IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'  --IR.AddedBy='Shashank' And
                        --AND IR.CheckedBy IS NOT NULL 
						AND IR.CheckedByStatus='Pending' 
						AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSValueAddedPO'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Value Added'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where " + strkey + @" and IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
						AND IR.CheckedByStatus IS NULL 
						AND IR.AuthorizedByStatus IS NULL						
						 And IR.PlantId='" + identity.PlantId + @"' 
                       --AND IR.POType='OSTransformationPO'--AND IR.AddedBy='Shashank'

                        AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSValueAddedPO'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Value Added'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where " + strkey + @" and IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						and IR.POType='OSValueAddedPO'
						) x
						--Order by PODate DESC
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.JWTransformationPurchaseOrder) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc ";
            }
            if(Type == "Transformation")
            {
                //       sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime]
                //                           ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                //                           FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                //                           e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                //                           from dbo.JWTransformationPurchaseOrder tc left join ORG.Entity e on e.Id=tc.EntityId
                //left join HKP.Party p on p.Id=tc.PartyId
                //                           WHERE " + strkey + " and POType='OSTransformationPO' order by tc.PODate desc";

                sql = @"	
						select * from(
							SELECT  ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Transformation'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						WHERE " + strkey + @" and  IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'  --IR.AddedBy='Shashank' And
                        --AND IR.CheckedBy IS NOT NULL 
						AND IR.CheckedByStatus='Pending' 
						AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSTransformationPO'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Transformation'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where " + strkey + @" and IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
						AND IR.CheckedByStatus IS NULL 
						AND IR.AuthorizedByStatus IS NULL						
						 And IR.PlantId='" + identity.PlantId + @"' 
                       --AND IR.POType='OSTransformationPO'--AND IR.AddedBy='Shashank'

                        AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSTransformationPO'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Transformation'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where " + strkey + @" and IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						and IR.POType='OSTransformationPO'
						) x
						--Order by PODate DESC
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.JWTransformationPurchaseOrder) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc";
            }
           
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetDataById(string Id, string TabType)
        {
            string sql = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (TabType == "Value Added")
            {
                //       sql = @"select vac.Id,TabType='Value Added', vac.EntityId,vac.PartyId,vac.Remarks,FORMAT(vac.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5)
                //                           ,vac.[Time],108)[VACTime],FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                //                           FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate
                //                           ,e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                //                           from dbo.JWTransformationPurchaseOrder vac left join ORG.Entity e on e.Id=vac.EntityId
                //left join HKP.Party p on p.Id=vac.PartyId
                //                           WHERE vac.Id='" + Id + "' order by ValueAddedDate desc ";

                sql = @"select * from(
							SELECT  ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Value Added'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						WHERE IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'  --IR.AddedBy='Shashank' And
                        --AND IR.CheckedBy IS NOT NULL 
						AND IR.CheckedByStatus='Pending' 
						AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSValueAddedPO'
                        And IR.Id='" + Id + @"'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Value Added'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
						AND IR.CheckedByStatus IS NULL 
						AND IR.AuthorizedByStatus IS NULL						
						 And IR.PlantId='" + identity.PlantId + @"' 
                       --AND IR.POType='OSTransformationPO'--AND IR.AddedBy='Shashank'

                        AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSValueAddedPO'
                        And IR.Id='" + Id + @"'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Value Added'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						and IR.POType='OSValueAddedPO'
                        And IR.Id='" + Id + @"'
						) x
						--Order by PODate DESC
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.JWTransformationPurchaseOrder) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc ";

            }
            if (TabType == "Transformation")
            {
         //       sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
         //                           FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
         //                           e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
         //                           from dbo.JWTransformationPurchaseOrder tc left join ORG.Entity e on e.Id=tc.EntityId
									//left join HKP.Party p on p.Id=tc.PartyId
         //                           WHERE tc.Id='"+ Id + @"' order by tc.PODate desc";

                sql = @"select * from(
							SELECT  ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Transformation'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						WHERE IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'  --IR.AddedBy='Shashank' And
                        --AND IR.CheckedBy IS NOT NULL 
						AND IR.CheckedByStatus='Pending' 
						AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSTransformationPO'
                        And IR.Id='" + Id + @"'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Transformation'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
						AND IR.CheckedByStatus IS NULL 
						AND IR.AuthorizedByStatus IS NULL						
						 And IR.PlantId='" + identity.PlantId + @"' 
                       --AND IR.POType='OSTransformationPO'--AND IR.AddedBy='Shashank'

                        AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC
						and IR.POType='OSTransformationPO'
                        And IR.Id='" + Id + @"'

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id,TabType='Transformation'
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
									JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId) AS IRD ON IRD.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN (SELECT A.JobWorkTransformationContractMasterId, A.TransactionUoMId FROM dbo.JobWorkTransformationContractChild AS A JOIN JWTransformationPurchaseOrder AS B ON A.JobWorkTransformationContractMasterId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.JobWorkTransformationContractMasterId, A.TransactionUoMId HAVING COUNT(A.JobWorkTransformationContractMasterId)> COUNT(A.TransactionUoMId)) AS TU ON TU.JobWorkTransformationContractMasterId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
						Where IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						and IR.POType='OSTransformationPO'
                        And IR.Id='" + Id + @"'
						) x
						--Order by PODate DESC
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.JWTransformationPurchaseOrder) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc";
            }

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetValueAddedChildData(string PKId)
        {
            string sql = "";
 
                sql = @"select distinct vcc.*,vcc.Quantity as VCCQuantity, jwi.UserName as JobWorkItem,jwa.UserName as JobWorkActivity, uom.UserName as OutputUnit,OMM.UserName as OutputMaterial
                                , mma.StandardName as OutputArticle
                               --, vam.RateApplicable as RateApply
							   , c.Code as Currency, emp.EmployeeName as ResponsiblePerson
                               ,owr.Id as OWRId, owr.JobWorkTransformationContractChildMasterId, owr.OrderType,owr.Quantity as OWRQuantity,owr.PlanQuantity
							   ,P.UserName as Customer,mo.MasterOrderNo,mm.UserName as MaterialOrderItem, owruom.UserName as OWRUOM
							  -- ,IssueQuantity=case WHEN vcc.OrderSpecific = 'Yes' THEN (kk.TotalQuantity) ELSE (TQ.TQuantity) END
							 --  ,BalToIssue=case WHEN vcc.OrderSpecific = 'Yes' THEN (owr.Quantity-kk.TotalQuantity) WHEN vcc.OrderSpecific = 'NO' THEN (vcc.Quantity-TQ.TQuantity) ELSE '0' END
                               ,IssueActive='Active'
                               from dbo.JobWorkTransformationContractChild vcc left join HKP.JobWorkItem jwi on jwi.Id=vcc.JobWorkItemMasterId
							   left join hkp.JobWorkActivity jwa on jwa.Id=vcc.JobActivityId
        					   left join SCS.UnitOfMeasurement uom on uom.Id=vcc.OutputMaterialUOMId
        					   left join MST.MaterialMasterArticle mma on mma.Id=vcc.ArticleId
							   left join MST.MaterialMaster OMM on OMM.Id=mma.MaterialMasterId
        					--   left join MST.JobWorkValueAddedMaster vam on vam.Id=vcc.RateApplyId
        					   left join scs.Currency c on c.Id=vcc.CurrencyId
        					   left join dbo.EmployeeInformation emp on emp.SystemId=vcc.ResponsiblePersonId
							   left join dbo.JWTransformationPurchaseOrder vc on vc.Id=vcc.JobWorkTransformationContractMasterId
							   left join dbo.JobWorkTransformationContractChild2 owr on owr.JobWorkTransformationContractChildMasterId=vcc.Id
							   left join HKP.Party P on P.Id=owr.CustomerId
							   left join TRN.MasterOrder mo on mo.Id=owr.MasterOrderNoId												
        					   left join TRN.MasterOrderItem moi on moi.Id=owr.MasterOrderItemId
        			    		left join MST.MaterialMaster mm on mm.Id=moi.MaterialMasterId
        				    	left join SCS.UnitOfMeasurement owruom on owruom.Id=owr.OutputMaterialUOMId
								--left join (	select SUM(quantity) as TotalQuantity,ContractLineItemId,OrderChildId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId,OrderChildId
								--		) kk on kk.ContractLineItemId=vcc.Id and kk.OrderChildId=owr.Id
								--left join (	select SUM(quantity) as TQuantity,ContractLineItemId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId
								--		) TQ on TQ.ContractLineItemId=vcc.Id
        					   where vc.POType='OSValueAddedPO' and vc.Id='" + PKId + @"' ";
            

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTransformationChildData(string PKId)
        {
            string sql = "";

            sql = @"select distinct mp.*, jwi.UserName as JobWorkItem,jwa.UserName as JobWorkActivity--, uom.UserName as OutputUnit
                               ,mm.UserName as Material , mma.StandardName as Article
                               ,c.Code as Currency, emp.EmployeeName as ResponsiblePerson, JL.LocationName as MaterialLocation
                               ,OutputUnit=case when tc.OrderSpecific='Yes' then mmuom.UserName else uom.UserName End
                               from dbo.JobWorkTransformationContractChild mp left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
        					   left join SCS.UnitOfMeasurement uom on uom.Id=mp.OutputMaterialUOMId
                               left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mp.BaseUOMId
        					   left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
							   left join MST.MaterialMaster mm on mm.Id=mp.MaterialMasterId
							   left join scs.Currency c on c.Id=mp.CurrencyId
							   left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
        					   left join dbo.EmployeeInformation emp on emp.SystemId=mp.ResponsiblePersonId
							   --left join HKP.MaterialStorage MS on MS.Id=mp.MaterialLocationId
					   	   	   left join HKP.JobWorkLocation JL on JL.Id=mp.MaterialLocationId
							   left join dbo.JWTransformationPurchaseOrder tc on tc.Id=mp.JobWorkTransformationContractMasterId
        					   where tc.Id='" + PKId + "' ";


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetMaterialInputData(IEnumerable<MaterialPlanning> SelectedMaterialPlanningData, string OrderSpecific,string MaterialStorageIdInventory, string IssueDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(JWTIR.GetMaterialInputData(SelectedMaterialPlanningData, OrderSpecific, MaterialStorageIdInventory, IssueDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetIssuedDetailList(string ArticleId, string MaterialId, string MaterialInputId, string ContractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(JWTIR.GetIssuedDetailList(ArticleId, MaterialId, MaterialInputId, ContractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetLotNoRate(string LotNumber)
        {
            try
            {
  
                return Json(JWTIR.GetLotNoRate(LotNumber), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                           EMP.EmployeeName,EMP.EmployeeCode AS Code,
                           EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                               PR.UserName PositionName,
                               DEPT.UserName DepartmentName,S.UserName Section,
                               EMP.SectionId,SS.UserName SubSection
                               ,PL.UserName Plant
                               FROM EmployeeInformation EMP
                               LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                               LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                               LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                               LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                               LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                               LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                               LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                               LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                               LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id

                           WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                      --AND isnull(Emp.SystemID,'') not in (select isnull(ByWhomId,'') from dbo.JobWorkIssueReturn where Id='" + Id + @"')
                     order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkIssueReturn", out sID);
            return sID;
        }

        //[HttpPost]
        //public JsonResult Create(Dictionary<string, object> data)
        //{
        //    try
        //    {
        //        DataSet dsMaster;
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

        //        con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

        //        string _Id = "";

        //        #region data update
        //        if (dsMaster.Tables[0].Rows.Count == 0)
        //        {
        //            bplib.clsGenID genid = new bplib.clsGenID();
        //            genid.GenID(TableName, out _Id);

        //            data["Id"] = "I" + GetPK();
        //            AddNewRow(dsMaster.Tables[0], data);
        //        }
        //        else
        //        {
        //            _Id = data["Id"].ToString();
        //            EditRow(dsMaster.Tables[0].Rows[0], data);
        //        }
        //        #endregion data update

        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(dsMaster);

        //        return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, string ContractId, string ContractType)
        {
            try
            {
                JWTIR.Create(data, ContractId, ContractType);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
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
                    dr[item] = sourceData[item];
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
                    dr[item] = sourceData[item];
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

        #endregion

        //   // Child data

        private string GetIssueChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkIssueReturnChild", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult SaveIssueChild(IEnumerable<JobWorkIssueReturnChild> IssueChildTabData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;
     
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var MPId = "' '";
                var OWRId = "''";
                foreach (var empitem in IssueChildTabData)
                {
                    MPId += ",'" + empitem.Id + "' ";
                    OWRId += ",'" + empitem.OWRId + "' ";
                }
                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where (ContractLineItemId IN ( " + MPId + " ) or OrderChildId IN (" + OWRId + ")) and JobWorkIssueReturnMasterId='"+ MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in IssueChildTabData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "IC" + GetIssueChildPK();

                        dr["JobWorkIssueReturnMasterId"] = MasterId;

                        dr["ContractLineItemId"] = item.Id;
                        dr["OrderChildId"] = item.OWRId;
                        dr["Quantity"] = item.BalToIssue;

                        dr["Remarks"] = item.Remarks;
                        dr["Active"] = item.IssueActive;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        #region Reports for Value Added Contract

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpGet, Authorize]
        public ActionResult GetValueAddedPrintReport(ReportFormat reportFormat, string PrintTabId, string IssueId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Value Added Job Work Material Issue Chalaan " + PrintTabId + "";
            var workbook = GetContractReportWorkSheet(PrintTabId, IssueId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetContractReportWorkSheet(string PrintTabId, string IssueId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "ValueAddedContractIssueChalaan";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetContractReportDataById(PrintTabId, IssueId);
            DataTable IssueReturnChilddata = GetIssueReturnChildDataById(PrintTabId);
            if (data.Rows.Count > 0)
            {
                int ColValueAddedDateHeader = 1;
                int ColValueAddedDateEnd;
                int ColVACTimeHeader;
                int ColVACTimeEnd;
                int ColVACTimeName;
                int ColEntityHeader;
                int ColEntityEnd;
                int ColEntityName;
                int ColPartyNameHeader;
            //    int ColPartyNameEnd;
                int ColPartyNameName;
                int ColVAProcessStartDateHeader = 1;
                int ColVAProcessStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColValueAddedDateHeader, "Date", 12, ExcelHAlign.HAlignLeft);
                ColValueAddedDateHeader++;
                ColValueAddedDateEnd = ColValueAddedDateHeader + 1;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Text = data.Rows[0]["ValueAddedDate"].ToString();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Merge();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColValueAddedDateEnd++;

                ColEntityHeader = ColValueAddedDateEnd;
                SetHeaderTextTop(ref sheet, ROW, ColEntityHeader, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColEntityHeader++;
                ColEntityEnd = ColEntityHeader + 1;
                ColEntityName = ColEntityHeader;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Merge();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColEntityEnd++;

               

                int ColIssueIdEnd = ColEntityEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIssueIdEnd, "Issue Id", 20, ExcelHAlign.HAlignLeft);
                ColIssueIdEnd++;
                int ColVAProcessEndDate = ColIssueIdEnd;
                int ColVAProcessEndDateEnd = ColIssueIdEnd + 1;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["IssueId"].ToString();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColVAProcessEndDateEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "Issue Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessEndDateEnd++;
                int ColIssueDate = ColVAProcessEndDateEnd;
                int ColIssueDateEnd = ColVAProcessEndDateEnd + 1;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Text = data.Rows[0]["IssueDate"].ToString();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Merge();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //    ColIssueDateEnd++;

                int ColPStartDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColPStartDate, "Process Start Date", 12, ExcelHAlign.HAlignLeft);
                ColPStartDate++;
                ColVAProcessStartDateEnd = ColPStartDate + 1;
                int ColAddress = ColPStartDate;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Text = data.Rows[0]["VAProcessStartDate"].ToString();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Merge();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColVAProcessStartDateEnd++;

           //     int ColPEndDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateEnd, "Process End Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateEnd++;
                int ColProcessEndDate = ColVAProcessStartDateEnd;
                int ColProcessEndDateEnd = ColVAProcessStartDateEnd + 1;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Text = data.Rows[0]["VAProcessEndDate"].ToString();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColProcessEndDateEnd++;

                int ColPrtyName = ColProcessEndDateEnd+1;
                SetHeaderTextTop(ref sheet, ROW, ColPrtyName, "Party Name", 20, ExcelHAlign.HAlignLeft);
                ColPrtyName++;
                int ColPartyName = ColPrtyName;
                int ColPartyNameEnd = ColPrtyName + 1;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Text = data.Rows[0]["PartyName"].ToString();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Merge();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //     ROW++;
                ColPartyNameEnd++;

              
                int ColIssuebyEnd = ColPartyNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColIssuebyEnd, "Issue By", 20, ExcelHAlign.HAlignLeft);
                ColIssuebyEnd++;
                int ColIssueby = ColIssuebyEnd;
                int ColIssueByEnd = ColIssuebyEnd + 1;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Text = data.Rows[0]["ByWhom"].ToString();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Merge();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //  ColIssueByEnd++;

                int ColCCDATe = 1;
                SetHeaderTextTop(ref sheet, ROW, ColCCDATe, "Contract Closing Date", 20, ExcelHAlign.HAlignLeft);
                ColCCDATe++;
                int ColVAContractClosingDate = ColCCDATe;
                int ColVAContractClosingDateEnd = ColCCDATe + 1;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Text = data.Rows[0]["VAContractClosingDate"].ToString();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Merge();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColVAContractClosingDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAContractClosingDateEnd, "User Contract Reference", 20, ExcelHAlign.HAlignLeft);
                ColVAContractClosingDateEnd++;
                int ColUserContractReference = ColVAContractClosingDateEnd;
                int ColUserContractReferenceEnd = ColVAContractClosingDateEnd + 1;
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].Text = data.Rows[0]["UserContractReference"].ToString();
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].Merge();
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColUserContractReferenceEnd++;


                int ColIR = ColUserContractReferenceEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIR, "Issue Type/ Issue Return", 15, ExcelHAlign.HAlignLeft);
                ColIR++;
                int ColIssueReturn = ColIR;
                int ColIssueReturnEnd = ColIR + 1;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Text = data.Rows[0]["IssueReturn"].ToString();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Merge();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColIssueReturnEnd++;



                SetHeaderTextTop(ref sheet, ROW, ColIssueReturnEnd, "Issue Location", 20, ExcelHAlign.HAlignLeft);
                ColIssueReturnEnd++;
                int ColJobWorkLocation = ColIssueReturnEnd;
                int ColJobWorkLocationEnd = ColIssueReturnEnd + 1;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Text = data.Rows[0]["JobWorkLocation"].ToString();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Merge();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                int ColRemarks = 1;
                SetHeaderTextTop(ref sheet, ROW, ColRemarks, "Remarks", 20, ExcelHAlign.HAlignLeft);
                ColRemarks++;
                int ColContractRemarks = ColRemarks;
                int ColContractRemarksEnd = ColRemarks + 1;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Text = data.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Merge();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
       

            }

     //       Issue/ Return Child data

            int MPChildROW = ROW + 1;
            int MPChildendCol = 1;
            int MPChildCOL = 1;

            #region Material Planning Child Headers

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue/ Return Quantity", 12, ExcelHAlign.HAlignLeft);
            MPChildROW++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Job Work Item", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material Type", 8, ExcelHAlign.HAlignLeft);
            int ColMaterialType = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Article Code", 12, ExcelHAlign.HAlignLeft);
            int ColArticleCode = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Output Unit", 8, ExcelHAlign.HAlignLeft);
            int ColOutputUnit = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColVCCQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Specific", 8, ExcelHAlign.HAlignLeft);
            int ColOrderSpecific = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Wise Quantity", 8, ExcelHAlign.HAlignLeft);
            int ColOWRQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Master Order No", 12, ExcelHAlign.HAlignLeft);
            int ColMasterOrderNo = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material Order Item", 10, ExcelHAlign.HAlignLeft);
            int ColMaterialOrderItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Customer", 8, ExcelHAlign.HAlignLeft);
            int ColCustomer = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Plan Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColPlanQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue Quantity", 8, ExcelHAlign.HAlignLeft);
            int ColIssueQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Balance To Issue", 8, ExcelHAlign.HAlignLeft);
            int ColBalToIssue = MPChildCOL;
       //     MPChildCOL++;

            //report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            //int ColMPCRemarks = MPChildCOL;
            MPChildROW++;
            MPChildendCol = MPChildCOL;
            #endregion Headers

            string JobWorkItem = "";
            var StartRows = 0;
            var EndRows = 0;
            int RowIndexNo = MPChildROW;
            StartRows = MPChildROW;

            for (int i = 0; i < IssueReturnChilddata.Rows.Count; i++)
            {

                if (JobWorkItem != IssueReturnChilddata.Rows[i]["JobWorkItem"].ToString())
                {

                    if (RowIndexNo < MPChildROW)
                    {
                        //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndexNo = MPChildROW;
                }

                sheet[MPChildROW, ColMaterialType].Text = IssueReturnChilddata.Rows[i]["MaterialType"].ToString();
                sheet[MPChildROW, ColJobWorkItem].Text = IssueReturnChilddata.Rows[i]["JobWorkItem"].ToString();
                sheet[MPChildROW, ColArticleCode].Text = IssueReturnChilddata.Rows[i]["ArticleCode"].ToString();
                sheet[MPChildROW, ColOutputUnit].Text = IssueReturnChilddata.Rows[i]["OutputUnit"].ToString();
                sheet[MPChildROW, ColVCCQuantity].Number = clsStaticInfo.dbl(IssueReturnChilddata.Rows[i]["VCCQuantity"].ToString());
                sheet[MPChildROW, ColOrderSpecific].Text = IssueReturnChilddata.Rows[i]["OrderSpecific"].ToString();
                sheet[MPChildROW, ColOWRQuantity].Number = clsStaticInfo.dbl(IssueReturnChilddata.Rows[i]["OWRQuantity"].ToString());
                sheet[MPChildROW, ColPlanQuantity].Number = clsStaticInfo.dbl(IssueReturnChilddata.Rows[i]["PlanQuantity"].ToString());
                sheet[MPChildROW, ColCustomer].Text = IssueReturnChilddata.Rows[i]["Customer"].ToString();
        //        sheet[MPChildROW, ColMPCRemarks].Text = IssueReturnChilddata.Rows[i]["Remarks"].ToString();
                sheet[MPChildROW, ColMasterOrderNo].Text = IssueReturnChilddata.Rows[i]["MasterOrderNo"].ToString();
                sheet[MPChildROW, ColMaterialOrderItem].Text = IssueReturnChilddata.Rows[i]["MaterialOrderItem"].ToString();
                sheet[MPChildROW, ColIssueQuantity].Text = IssueReturnChilddata.Rows[i]["IssueQuantity"].ToString();
                sheet[MPChildROW, ColBalToIssue].Text = IssueReturnChilddata.Rows[i]["BalToIssue"].ToString();

                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
                JobWorkItem = IssueReturnChilddata.Rows[i]["JobWorkItem"].ToString();

                MPChildROW++;
            }

            EndRows = MPChildROW - 1;

            if (RowIndexNo < MPChildROW - 1)
            {
                //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            //GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            //GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, MPChildendCol, "Value Added Job Work Material Issue Chalaan", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetContractReportDataById(string PrintTabId, string IssueId)
        {
            var sql = @"select distinct vac.*,TabType='Value Added',FORMAT(vac.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime],FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,ir.Id as IssueId,FORMAT(ir.Date,'dd-MMM-yyyy') as IssueDate,emp.EmployeeName as ByWhom
									,JL.LocationName as JobWorkLocation,ir.IssueReturn
                                    from dbo.JobWorkValueAddedContract vac left join ORG.Entity e on e.Id=vac.EntityId
									left join HKP.Party p on p.Id=vac.VendorPartyId
									left join dbo.JobWorkValueAddedContractChild mp on mp.JobWorkValueAddedContractMasterId=vac.Id
									left join dbo.JobWorkValueAddedContractChild2 owr on owr.JobWorkValueAddedContractChildMasterId=mp.Id
									left join dbo.JobWorkIssueReturnChild irc on irc.ContractLineItemId=mp.Id
									left join dbo.JobWorkIssueReturnChild on irc.OrderChildId=owr.Id
									left join dbo.JobWorkIssueReturn ir on ir.Id=irc.JobWorkIssueReturnMasterId
									left join dbo.EmployeeInformation emp on emp.SystemId=ir.ByWhomId
									left join HKP.JobWorkLocation JL on JL.Id=ir.JobWorkLocationId
                                    where vac.Id = '" + PrintTabId + "' and ir.Id='"+ IssueId + "' ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetIssueReturnChildDataById(string PrintTabId)
        {
            var sql = @"select distinct vcc.*,vcc.Quantity as VCCQuantity, jwi.UserName as JobWorkItem, uom.UserName as OutputUnit, mma.StandardName as ArticleCode, vam.RateApplicable as RateApply, c.Code as Currency, emp.EmployeeName as ResponsiblePerson
                               ,owr.Id as OWRId, owr.JobWorkValueAddedContractChildMasterId, owr.OrderType,owr.Quantity as OWRQuantity,owr.PlanQuantity
							   ,P.UserName as Customer,mo.MasterOrderNo,mm.UserName as MaterialOrderItem, owruom.UserName as OWRUOM
							   ,IssueQuantity=case WHEN vcc.OrderSpecific = 'Yes' THEN (kk.TotalQuantity) ELSE (TQ.TQuantity) END
							   ,BalToIssue=case WHEN vcc.OrderSpecific = 'Yes' THEN (owr.Quantity-kk.TotalQuantity) WHEN vcc.OrderSpecific = 'NO' THEN (vcc.Quantity-TQ.TQuantity) ELSE '0' END
                               ,IssueActive='Active'
                               from dbo.JobWorkValueAddedContractChild vcc left join HKP.JobWorkItem jwi on jwi.Id=vcc.JobWorkItemMasterId
        					   left join SCS.UnitOfMeasurement uom on uom.Id=vcc.OutputMaterialUOMId
        					   left join MST.MaterialMasterArticle mma on mma.Id=vcc.ArticleCodeId
        					   left join MST.JobWorkValueAddedMaster vam on vam.Id=vcc.RateApplyId
        					   left join scs.Currency c on c.Id=vcc.CurrencyId and vcc.CurrencyId=vam.CurrencyId
        					   left join dbo.EmployeeInformation emp on emp.SystemId=vcc.ResponsiblePersonId
							   left join dbo.JobWorkValueAddedContract vc on vc.Id=vcc.JobWorkValueAddedContractMasterId
							   left join dbo.JobWorkValueAddedContractChild2 owr on owr.JobWorkValueAddedContractChildMasterId=vcc.Id
							   left join HKP.Party P on P.Id=owr.CustomerId
							   left join TRN.MasterOrder mo on mo.Id=owr.MasterOrderNoId												
        					   left join TRN.MasterOrderItem moi on moi.Id=owr.MasterOrderItemId
        			    		left join MST.MaterialMaster mm on mm.Id=moi.MaterialMasterId
        				    	left join SCS.UnitOfMeasurement owruom on owruom.Id=owr.OutputMaterialUOMId
								left join (	select SUM(quantity) as TotalQuantity,ContractLineItemId,OrderChildId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId,OrderChildId
										) kk on kk.ContractLineItemId=vcc.Id and kk.OrderChildId=owr.Id
								left join (	select SUM(quantity) as TQuantity,ContractLineItemId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId
										) TQ on TQ.ContractLineItemId=vcc.Id
        					   where vc.Id='" + PrintTabId + "' ";

            return _sqlRepository.GetDataTable(sql);
        }

        #endregion end Reports for Value Added Contract

        // TRANSFORMATION ISSUE

        [HttpPost, Authorize]
        public ActionResult LoadAllResponsiblePersonDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                           EMP.EmployeeName,EMP.EmployeeCode AS Code,
                           EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                               PR.UserName PositionName,
                               DEPT.UserName DepartmentName,S.UserName Section,
                               EMP.SectionId,SS.UserName SubSection
                               ,PL.UserName Plant
                               FROM EmployeeInformation EMP
                               LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                               LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                               LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                               LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                               LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                               LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                               LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                               LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                               LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id

                           WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' 
                      --and EMP.EmpType='Local'
                      --AND isnull(Emp.SystemID,'') not in (select isnull(ByWhomId,'') from dbo.JobWorkTransformationIssueReturn where Id='" + Id + @"')
                     order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getentitylist()
        {
            try
            {

                return Json(JWTIR.getentitylist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult gejobworklocation(string TId)
        {
            try
            {

                return Json(JWTIR.gejobworklocation(TId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult getalljobworklocation()
        {
            try
            {

                return Json(JWTIR.getalljobworklocation(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //private string GetTransformationPK()
        //{
        //    string sID = string.Empty;
        //    bplib.clsGenID objGenID = new bplib.clsGenID();
        //    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkTransformationIssueReturn", out sID);
        //    return sID;
        //}

        //[HttpPost]
        //public JsonResult SaveIssueTransformation(Dictionary<string, object> data)
        //{
        //    try
        //    {
        //        DataSet dsMaster;
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

        //        con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

        //        string _Id = "";

        //        #region data update
        //        if (dsMaster.Tables[0].Rows.Count == 0)
        //        {
        //            bplib.clsGenID genid = new bplib.clsGenID();

        //            genid.GenID(TableName2, out _Id);

        //            data["Id"] = "IT" + GetTransformationPK();
        //            AddNewRow(dsMaster.Tables[0], data);
        //        }
        //        else
        //        {
        //            _Id = data["Id"].ToString();
        //            EditRow(dsMaster.Tables[0].Rows[0], data);
        //        }
        //        #endregion data update

        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(dsMaster);

        //        return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}

        [HttpPost]
        public JsonResult SaveIssueTransformation(Dictionary<string, object> data, string ContractId, string ContractType, IEnumerable<JobWorkTransformationIssueReturnChild> SelectedQuantityData)
        {
            try
            {
                JWTIR.SaveIssueTransformation(data, ContractId, ContractType, SelectedQuantityData);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        //   // Child data

        private string GetTransformationChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkTransformationIssueReturnChild", out sID);
            return sID;
        }

        //[HttpPost]
        //public JsonResult SaveTransformationChild(IEnumerable<JobWorkTransformationIssueReturnChild> SelectedQuantityData, string MasterId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    try
        //    {
        //        DataSet ExistOrNot;

        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
        //        var MatInputId = "' '";
            
        //        foreach (var empitem in SelectedQuantityData)
        //        {
        //            MatInputId += ",'" + empitem.Id + "' ";
                 
        //        }
        //        con.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationIssueReturnChild where MaterialInputId IN ( " + MatInputId + ") and TransformationIssueReturnMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

        //        foreach (var item in SelectedQuantityData)
        //        {
        //            if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
        //            {
        //                DataRow dr = ExistOrNot.Tables[0].NewRow();
        //                dr["Id"] = "TC" + GetTransformationChildPK();

        //                dr["TransformationIssueReturnMasterId"] = MasterId;

        //                dr["MaterialInputId"] = item.Id;
        //                dr["MaterialMasterId"] = item.InputMaterialId;
        //                dr["Quantity"] = item.Quantity;
        //                dr["Remarks"] = item.Remarks;
        //                dr["MaterialMasterArticleId"] = item.MaterialMasterArticleId;
        //                dr["Value"] = item.Value;
        //                dr["LotNumber"] = item.LotNumber;
                     
        //                dr["AddedBy"] = identity.Name;
        //                dr["AddedDate"] = System.DateTime.Now.ToString();
        //                dr["AddedFromIP"] = identity.IPAddress;
        //                dr["UpdatedBy"] = identity.Name;
        //                dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //                dr["UpdatedFromIP"] = identity.IPAddress;

        //                ExistOrNot.Tables[0].Rows.Add(dr);

        //            }

        //        }
        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(ExistOrNot);

        //        return Json(new { Error = false, Message = AplosMessage.Updated });

        //    }


        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}

        [HttpGet, Authorize]
        public ActionResult LoadAllMaterialMstDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select mm.Id, mm.Code, mm.UserName as MaterialName,mc.UserName as MaterialCategory, mgm.UserName as MaterialGroupMaster, buom.UserName as BaseUOM
                                      ,WithSKU=case when mm.WithSKU=0 then 'No' else 'Yes' END
									  ,IsAsset=case when mm.IsAsset=0 then 'No' else 'Yes' END
                                      from MST.MaterialMaster mm left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
									  left join SCS.UnitOfMeasurement buom on buom.Id=mm.BaseUOMId
									  left join HKP.MaterialCategory mc on mc.Id=mm.MaterialCategoryId
                                      WHERE mm.CompanyGroupId='" + identity.CompanyGroupId + @"' order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllMaterialMstArticle(string MaterialMstId, string MaterialInputId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"Select mm.Code as MaterialCode,mm.UserName as Material,mgm.UserName as MaterialGroupMaster,mm.Id MaterialMasterId,mma.Id as ArticleId ,mma.Code as ArticleCode, mma.ShortName, mma.StandardName 
                           from MST.MaterialMasterArticle mma left join MST.MaterialMaster mm on mma.MaterialMasterId=mm.Id
                           left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
                           left join dbo.JobWorkTransformationContractChild3 mi on mi.ArticleId=mma.Id
                            where mm.Id='" + MaterialMstId + @"' and mi.Id='"+ MaterialInputId + @"' order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult GetByDefaultRate(string ArticleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select im.MaterialMasterId, im.ArticleId,SUM(ird.Rate) as Rate 
                           from TRN.InventoryMaterial im
                           left join (Select InventoryMaterialId,(sum( MaterialTranAmount)/sum(TransactionQty)) as Rate from TRN.InventoryReceiveDetail group by InventoryMaterialId)
                           ird on ird.InventoryMaterialId=im.Id
                           where im.ArticleId='"+ ArticleId + @"' and im.PlantId='" + identity.PlantId + @"'
                           group by im.MaterialMasterId, im.ArticleId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLotNumberList(string ArticleId, string MaterialId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select distinct IRD.LotNo Value, IRD.LotNo Text, IM.MaterialMasterId, IM.ArticleId from trn.InventoryReceiveDetail IRD
                                      left join trn.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                                      where PlantId='"+ identity.PlantId + @"' and IM.MaterialMasterId='"+ MaterialId + @"' and IM.ArticleId='"+ ArticleId + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        // REPORT FOR TRANSFORMATION ISSUE

        #region Reports for Transformation Contract

        [HttpGet, Authorize]
        public ActionResult GetTransformationPrintReport(ReportFormat reportFormat, string PrintTabId, string IssueId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Transformation Job Work Material Issue Chalaan " + PrintTabId + "";
            var workbook = GetTransformationContractReportWorkSheet(PrintTabId, IssueId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetTransformationContractReportWorkSheet(string PrintTabId, string IssueId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "TransformationContractIssueChalaan";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetTransformationContractReportDataById(PrintTabId, IssueId);
            DataTable TransformationIssueReturnChilddata = GetTransformationIssueReturnChildDataById(PrintTabId, IssueId);
            DataTable TransformationIssueGRNdata = GetTransformationGRNDataById(IssueId);
            if (data.Rows.Count > 0)
            {
                int ColValueAddedDateHeader = 1;
                int ColValueAddedDateEnd;
                int ColVACTimeHeader;
                int ColVACTimeEnd;
                int ColVACTimeName;
                int ColEntityHeader;
                int ColEntityEnd;
                int ColEntityName;
                int ColPartyNameHeader;
                //    int ColPartyNameEnd;
                int ColPartyNameName;
                int ColVAProcessStartDateHeader = 1;
                int ColVAProcessStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColValueAddedDateHeader, "Date", 12, ExcelHAlign.HAlignLeft);
                ColValueAddedDateHeader++;
                ColValueAddedDateEnd = ColValueAddedDateHeader + 1;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Text = data.Rows[0]["ValueAddedDate"].ToString();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Merge();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColValueAddedDateEnd++;

                ColEntityHeader = ColValueAddedDateEnd;
                SetHeaderTextTop(ref sheet, ROW, ColEntityHeader, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColEntityHeader++;
                ColEntityEnd = ColEntityHeader + 1;
                ColEntityName = ColEntityHeader;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Merge();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColEntityEnd++;



                int ColIssueIdEnd = ColEntityEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIssueIdEnd, "Issue Id", 20, ExcelHAlign.HAlignLeft);
                ColIssueIdEnd++;
                int ColVAProcessEndDate = ColIssueIdEnd;
                int ColVAProcessEndDateEnd = ColIssueIdEnd + 1;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["TransformationIssueId"].ToString();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColVAProcessEndDateEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "Issue Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessEndDateEnd++;
                int ColIssueDate = ColVAProcessEndDateEnd;
                int ColIssueDateEnd = ColVAProcessEndDateEnd + 1;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Text = data.Rows[0]["TransformationDate"].ToString();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Merge();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //    ColIssueDateEnd++;

                int ColPStartDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColPStartDate, "Process Start Date", 12, ExcelHAlign.HAlignLeft);
                ColPStartDate++;
                ColVAProcessStartDateEnd = ColPStartDate + 1;
                int ColAddress = ColPStartDate;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Text = data.Rows[0]["VAProcessStartDate"].ToString();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Merge();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColVAProcessStartDateEnd++;

                //     int ColPEndDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateEnd, "Process End Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateEnd++;
                int ColProcessEndDate = ColVAProcessStartDateEnd;
                int ColProcessEndDateEnd = ColVAProcessStartDateEnd + 1;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Text = data.Rows[0]["VAProcessEndDate"].ToString();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColProcessEndDateEnd++;

                int ColPrtyName = ColProcessEndDateEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColPrtyName, "Party Name", 20, ExcelHAlign.HAlignLeft);
                ColPrtyName++;
                int ColPartyName = ColPrtyName;
                int ColPartyNameEnd = ColPrtyName + 1;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Text = data.Rows[0]["PartyName"].ToString();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Merge();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //     ROW++;
                ColPartyNameEnd++;


                int ColIssuebyEnd = ColPartyNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColIssuebyEnd, "Issue By", 20, ExcelHAlign.HAlignLeft);
                ColIssuebyEnd++;
                int ColIssueby = ColIssuebyEnd;
                int ColIssueByEnd = ColIssuebyEnd + 1;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Text = data.Rows[0]["ByWhom"].ToString();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Merge();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //  ColIssueByEnd++;

                int ColCCDATe = 1;
                SetHeaderTextTop(ref sheet, ROW, ColCCDATe, "Contract Closing Date", 20, ExcelHAlign.HAlignLeft);
                ColCCDATe++;
                int ColVAContractClosingDate = ColCCDATe;
                int ColVAContractClosingDateEnd = ColCCDATe + 1;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Text = data.Rows[0]["VAContractClosingDate"].ToString();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Merge();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColVAContractClosingDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAContractClosingDateEnd, "PO Number", 20, ExcelHAlign.HAlignLeft);
                ColVAContractClosingDateEnd++;
                int ColContractId = ColVAContractClosingDateEnd;
                int ColContractIdEnd = ColVAContractClosingDateEnd + 1;
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].Text = data.Rows[0]["Id"].ToString();
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].Merge();
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColContractIdEnd++;


                int ColIR = ColContractIdEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIR, "Issue Type", 15, ExcelHAlign.HAlignLeft);
                ColIR++;
                int ColIssueReturn = ColIR;
                int ColIssueReturnEnd = ColIR + 1;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Text = data.Rows[0]["IssueType"].ToString();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Merge();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColIssueReturnEnd++;



                SetHeaderTextTop(ref sheet, ROW, ColIssueReturnEnd, "Issue Location", 20, ExcelHAlign.HAlignLeft);
                ColIssueReturnEnd++;
                int ColJobWorkLocation = ColIssueReturnEnd;
                int ColJobWorkLocationEnd = ColIssueReturnEnd + 1;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Text = data.Rows[0]["JobWorkLocation"].ToString();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Merge();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                int ColRemarks = 1;
                SetHeaderTextTop(ref sheet, ROW, ColRemarks, "Remarks", 20, ExcelHAlign.HAlignLeft);
                ColRemarks++;
                int ColContractRemarks = ColRemarks;
                int ColContractRemarksEnd = ColRemarks + 1;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Text = data.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Merge();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColContractRemarksEnd++;

                int ColContractIsseStatus = ColContractRemarksEnd + 4;
                SetHeaderTextTop(ref sheet, ROW, ColContractIsseStatus, "Issue Status", 20, ExcelHAlign.HAlignLeft);
                ColContractIsseStatus++;
                int ColIssueStatus = ColContractIsseStatus;
                int ColIssueStatusEnd = ColContractIsseStatus + 1;
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Text = data.Rows[0]["IssueStatus"].ToString();
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Merge();
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;


            }

            //       Issue/ Return Child data

            int MPChildROW = ROW + 1;
            int MPChildendCol = 1;
            int MPChildCOL = 1;

            #region Material Planning Child Headers

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue/ Return Quantity", 12, ExcelHAlign.HAlignLeft);
            MPChildROW++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColJWOutputItemId = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWOutputItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Input Material", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputMaterial = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Input Article", 12, ExcelHAlign.HAlignLeft);
            int ColArticle = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Required Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColRequiredQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Total Issued Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTIRCTotalQty = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Balance To Issue", 12, ExcelHAlign.HAlignLeft);
            int ColBalanceToIssue = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue UoM", 12, ExcelHAlign.HAlignLeft);
            int ColJWIssueUoM = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issued Quantity", 10, ExcelHAlign.HAlignLeft);
            int ColTIRCQty = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Average Issue Rate", 12, ExcelHAlign.HAlignLeft);
            int ColAvgRate = MPChildCOL;
            MPChildCOL++;

            //report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Base Rate", 12, ExcelHAlign.HAlignLeft);
            //int ColBaseRateeee = MPChildCOL;
            //MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue Amount", 10, ExcelHAlign.HAlignLeft);
            int ColAvgAmount = MPChildCOL;
            MPChildROW++;

            MPChildendCol = MPChildCOL;
            #endregion Headers

            string JWOutputItem = "";
            var StartRows = 0;
            var EndRows = 0;
            int RowIndexNo = MPChildROW;
            StartRows = MPChildROW;

            for (int i = 0; i < TransformationIssueReturnChilddata.Rows.Count; i++)
            {

                if (JWOutputItem != TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString())
                {

                    if (RowIndexNo < MPChildROW)
                    {
                        //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndexNo = MPChildROW;
                }

                sheet[MPChildROW, ColJWOutputItemId].Text = TransformationIssueReturnChilddata.Rows[i]["JWOutputId"].ToString();
                sheet[MPChildROW, ColJWOutputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();
                //sheet[MPChildROW, ColJWInputItemId].Text = TransformationIssueReturnChilddata.Rows[i]["JwInputId"].ToString();
                //sheet[MPChildROW, ColJWInputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JWInputItem"].ToString();
                sheet[MPChildROW, ColJWInputMaterial].Text = TransformationIssueReturnChilddata.Rows[i]["Material"].ToString();
                sheet[MPChildROW, ColArticle].Text = TransformationIssueReturnChilddata.Rows[i]["Article"].ToString();
                sheet[MPChildROW, ColBalanceToIssue].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["BalanceToIssue"].ToString());
                sheet[MPChildROW, ColRequiredQuantity].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["RequiredQuantity"].ToString());
                sheet[MPChildROW, ColTIRCTotalQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TotalIssuedQty"].ToString());
                sheet[MPChildROW, ColTIRCQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TransactionQty"].ToString());

                sheet[MPChildROW, ColAvgRate].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["AveRateeee"].ToString());

          //      sheet[MPChildROW, ColBaseRateeee].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["BaseRateeee"].ToString());

                sheet[MPChildROW, ColAvgAmount].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["AverageAmount"].ToString());

                sheet[MPChildROW, ColJWIssueUoM].Text = TransformationIssueReturnChilddata.Rows[i]["IssueUoM"].ToString();

                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
                JWOutputItem = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();

                MPChildROW++;
            }

            int ColTotal = 1;
            report.SetHeaderText(ref sheet, MPChildROW, ColTotal, "Total", 10, ExcelHAlign.HAlignLeft);
     //       int ColAvgAmount = MPChildCOL;
     //       MPChildROW++;

            // SUM OF TOTAL ISSUED QUANTITY
            int ColTotalIssQty = 9;
            decimal p = 0;
            decimal q = 0;
            decimal r = 0;
            for (int j = 0; j < TransformationIssueReturnChilddata.Rows.Count; j++)
            {

                p = Convert.ToDecimal(TransformationIssueReturnChilddata.Rows[j]["TransactionQty"]);
                r = p + q;
                q = r;
                sheet[MPChildROW, ColTotalIssQty].Number = clsStaticInfo.dbl(q);
                sheet.Range[MPChildROW, ColTotalIssQty].CellStyle.Font.Bold = true;
            }

            // SUM OF TOTAL Amount
            int ColTotalRecQty = 11;
            decimal x = 0;
            decimal y = 0;
            decimal z = 0;
            for (int j = 0; j < TransformationIssueReturnChilddata.Rows.Count; j++)
            {

                x = Math.Round(Convert.ToDecimal(TransformationIssueReturnChilddata.Rows[j]["AverageAmount"]),2);
                z = Math.Round(x,2) + Math.Round(y,2);
                y = Math.Round(z,2);
                sheet[MPChildROW, ColTotalRecQty].Number = Math.Round(clsStaticInfo.dbl(y),2);
                sheet.Range[MPChildROW, ColTotalRecQty].CellStyle.Font.Bold = true;
            }

            EndRows = MPChildROW - 1;

            if (RowIndexNo < MPChildROW - 1)
            {
                //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // GRN DETAILS

            int GRNROW = MPChildROW + 2;
            int GRNendCol = 1;
            int GRNCOL = 1;

            #region GRN DETAILS Headers

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "GRN Details", 12, ExcelHAlign.HAlignLeft);
            GRNROW++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColId= GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "JW Input Material", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputMat= GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "JW Input Article", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputArticle = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "GRN No", 12, ExcelHAlign.HAlignLeft);
            int ColGRNNo = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "GRN Row Id", 12, ExcelHAlign.HAlignLeft);
            int ColGRNRowId = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Issue UoM", 12, ExcelHAlign.HAlignLeft);
            int ColIssueUoM = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Issue Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColGRNIssueQty = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Transaction Currency", 12, ExcelHAlign.HAlignLeft);
            int ColTransactionCurrency = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Transaction Rate", 12, ExcelHAlign.HAlignLeft);
            int ColTransactionRate = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Base Currency", 12, ExcelHAlign.HAlignLeft);
            int ColBaseCurrency = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Base Rate", 10, ExcelHAlign.HAlignLeft);
            int ColBaseRate = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Total Amount", 10, ExcelHAlign.HAlignLeft);
            int ColTotalAmount = GRNCOL;
            GRNROW++;
            GRNendCol = GRNCOL;
            #endregion Headers

            string Id = "";
            var GRNStartRows = 0;
            var GRNEndRows = 0;
            int GRNRowIndexNo = GRNROW;
            GRNStartRows = GRNROW;

            for (int i = 0; i < TransformationIssueGRNdata.Rows.Count; i++)
            {

                if (Id != TransformationIssueGRNdata.Rows[i]["Id"].ToString())
                {

                    if (GRNRowIndexNo < GRNROW)
                    {
                        //sheet.Range[GRNRowIndexNo, ColJobWorkItem, GRNROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[GRNRowIndexNo, ColId, GRNROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[GRNRowIndexNo, ColId, GRNROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    GRNRowIndexNo = GRNROW;
                }

                sheet[GRNROW, ColId].Text = TransformationIssueGRNdata.Rows[i]["Id"].ToString();
                sheet[GRNROW, ColGRNNo].Text = TransformationIssueGRNdata.Rows[i]["GRNNo"].ToString();
                sheet[GRNROW, ColGRNRowId].Text = TransformationIssueGRNdata.Rows[i]["GRNRowId"].ToString();
                
                sheet[GRNROW, ColJWInputMat].Text = TransformationIssueGRNdata.Rows[i]["JWInputMaterial"].ToString();
                sheet[GRNROW, ColJWInputArticle].Text = TransformationIssueGRNdata.Rows[i]["JWInputArticle"].ToString();

                sheet[GRNROW, ColGRNIssueQty].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["GRNIssueQty"].ToString());
                sheet[GRNROW, ColIssueUoM].Text = TransformationIssueGRNdata.Rows[i]["IssueUoM"].ToString();

                sheet[GRNROW, ColTransactionCurrency].Text = TransformationIssueGRNdata.Rows[i]["TransactionCurrency"].ToString();
                sheet[GRNROW, ColTransactionRate].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["TransactionRate"].ToString());

                //    sheet[GRNROW, ColTIRCQty].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["TransactionQty"].ToString());

                sheet[GRNROW, ColBaseCurrency].Text = TransformationIssueGRNdata.Rows[i]["BaseCurrency"].ToString();
                sheet[GRNROW, ColBaseRate].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["BaseRate"].ToString());

                sheet[GRNROW, ColTotalAmount].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["TotalAmount"].ToString());

                sheet.Range[GRNROW, 1, GRNROW, GRNendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[GRNROW, 1, GRNROW, GRNendCol].BorderAround(ExcelLineStyle.Hair);
                Id = TransformationIssueGRNdata.Rows[i]["Id"].ToString();

                GRNROW++;
            }

            int ColGRNTotal = 1;
            report.SetHeaderText(ref sheet, GRNROW, ColGRNTotal, "Total", 10, ExcelHAlign.HAlignLeft);
            //       int ColAvgAmount = MPChildCOL;
            //       GRNROW++;

            // SUM OF TOTAL GRN ISSUED QUANTITY
            int ColTotalGRNIssQty = 7;
            decimal a = 0;
            decimal b = 0;
            decimal c = 0;
            for (int j = 0; j < TransformationIssueGRNdata.Rows.Count; j++)
            {
                    a = Convert.ToDecimal(TransformationIssueGRNdata.Rows[j]["GRNIssueQty"]);
                    c = a + b;
                    b = c;
                    sheet[GRNROW, ColTotalGRNIssQty].Number = clsStaticInfo.dbl(b);
                    sheet.Range[GRNROW, ColTotalGRNIssQty].CellStyle.Font.Bold = true;
            }

            // SUM OF TOTAL GRN Amount
            int ColTotalGRNAmount = 12;
            decimal xx = 0;
            decimal yy = 0;
            decimal zz = 0;
            for (int j = 0; j < TransformationIssueGRNdata.Rows.Count; j++)
            {
                    xx = Math.Round(Convert.ToDecimal(TransformationIssueGRNdata.Rows[j]["TotalAmount"]), 2);
                    zz = Math.Round(xx, 2) + Math.Round(yy, 2);
                    yy = Math.Round(zz, 2);
                    sheet[GRNROW, ColTotalGRNAmount].Number = Math.Round(clsStaticInfo.dbl(yy), 2);
                    sheet.Range[GRNROW, ColTotalGRNAmount].CellStyle.Font.Bold = true;
            }

            GRNEndRows = GRNROW - 1;

            if (GRNRowIndexNo < GRNROW - 1)
            {
                //sheet.Range[GRNRowIndexNo, ColJobWorkItem, GRNROW - 1, ColJobWorkItem].Merge();
                sheet.Range[GRNRowIndexNo, ColId, GRNROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[GRNRowIndexNo, ColId, GRNROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            //GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            //GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, MPChildendCol+6, "Issue Chalaan (Transformation)", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetTransformationContractReportDataById(string PrintTabId, string IssueId)
        {
            //   var sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.VendorPartyId,tc.Remarks,FORMAT(tc.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime]
            //                           ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
            //                           FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
            //                           e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
            //,II.Id as TransformationIssueId,FORMAT(II.IssueDate,'dd-MMM-yyyy') as TransformationDate,emp.EmployeeName as ByWhom
            //,Ms.UserName as JobWorkLocation,II.Types as IssueReturn, II.IssueType
            //,IssueStatus=case when II.IsConfirmed=0 then 'Not Confirmed' else 'Confirmed' End
            //                           from dbo.JobWorkTransformationContract tc left join ORG.Entity e on e.Id=tc.EntityId
            //left join HKP.Party p on p.Id=tc.VendorPartyId
            //   left join TRN.InventoryIssue II on II.JWContractId=tc.Id
            //left join dbo.EmployeeInformation emp on emp.SystemId=II.EmployeeId
            //left join HKP.MaterialStorage Ms on Ms.Id=II.MaterialStorageId
            //                           WHERE tc.Id='"+ PrintTabId + @"' and II.Id='"+ IssueId + @"' ";

            var sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.PartyId,tc.PODate,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime]
                                    ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
									,II.Id as TransformationIssueId,FORMAT(II.IssueDate,'dd-MMM-yyyy') as TransformationDate,emp.EmployeeName as ByWhom
									,Ms.UserName as JobWorkLocation,II.Types as IssueReturn, II.IssueType
									,IssueStatus=case when II.IsConfirmed=0 then 'Not Confirmed' else 'Confirmed' End
                                    ,tc.Remarks
                                    from dbo.JWTransformationPurchaseOrder tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.PartyId
								    left join TRN.InventoryIssue II on II.JWContractId=tc.Id
									left join dbo.EmployeeInformation emp on emp.SystemId=II.EmployeeId
									left join HKP.MaterialStorage Ms on Ms.Id=II.MaterialStorageId
                                    WHERE tc.Id='" + PrintTabId + @"' and II.Id='" + IssueId + @"' ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetTransformationIssueReturnChildDataById(string PrintTabId, string IssueId)
        {
            //            var sql = @"select distinct IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id as JWOutputId, jwi.UserName as JWOutputItem, mi.Id as JwInputId
            //,jwii.UserName as JWInputItem,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
            //							 ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalIssuedQty,'0')),IID.TransactionQty
            //from TRN.InventoryIssueDetail IID left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
            //left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
            //left join dbo.JobWorkTransformationContractChild mp on mp.JobWorkTransformationContractMasterId=II.JWContractId
            //left join (select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article, IM.ArticleId,IID.InventoryMaterialId
            //							            from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
            //                                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
            //                                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
            //                                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
            //										where II.JWContractId='"+ PrintTabId + @"'
            //										group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName)
            //										kk on kk.InventoryMaterialId=IM.Id
            //										left join dbo.JobWorkTransformationContractChild3 mi on mi.JobWorkTransformationContractChildMasterId=mp.Id
            //										left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
            //										left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
            //										where mp.JobWorkTransformationContractMasterId='"+ PrintTabId + @"' and II.Id='"+ IssueId + @"' and mi.Id is not null and II.Types='InventoryJWIssue'
            //										group by IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id, jwi.UserName, mi.Id
            //										,jwii.UserName,mp.Quantity,mi.GrossConsumption,IID.TransactionQty";

            //            var sql = @"select distinct IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id as JWOutputId, jwi.UserName as JWOutputItem
            //,mi.Id as JwInputId,jwii.UserName as JWInputItem
            //,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
            //,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalIssuedQty,'0'))
            //,IID.TransactionQty
            //from TRN.InventoryIssueDetail IID left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
            //left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
            //left join dbo.JobWorkTransformationContractChild mp on mp.JobWorkTransformationContractMasterId=II.JWContractId
            //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
            //left join dbo.JobWorkTransformationContractChild3 mi on mi.JobWorkTransformationContractChildMasterId=mp.Id and mi.ArticleId=IM.ArticleId
            //left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
            //left join (select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article, IM.ArticleId,IID.InventoryMaterialId
            //							            from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
            //                                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
            //                                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
            //                                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
            //										where II.JWContractId='" + PrintTabId + @"'
            //										group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName)
            //										kk on kk.InventoryMaterialId=IM.Id
            //										where mp.JobWorkTransformationContractMasterId='" + PrintTabId + @"' and II.Id='" + IssueId + @"' and mi.Id is not null 
            //										and II.Types='InventoryJWIssue'
            //										group by IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id
            //										, jwi.UserName, mi.Id
            //										,jwii.UserName
            //										,mp.Quantity, mi.GrossConsumption
            //										,IID.TransactionQty";

            //            var sql = @"select distinct IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id as JWOutputId, jwi.UserName as JWOutputItem
            //--,mi.Id as JwInputId,jwii.UserName as JWInputItem
            //,RequiredQuantity=(mp.Quantity * JWMi.GrossConsump)
            //,BalanceToIssue=(mp.Quantity * JWMi.GrossConsump)-(ISNULL(kk.TotalIssuedQty,'0'))
            //,IID.TransactionQty--,IID.AvgRate,IID.AvgAmount
            //,AA.TQty,AA.AverageIssueRate,AverageAmount=(AA.AverageIssueRate * IID.TransactionQty)
            //from TRN.InventoryIssueDetail IID left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
            //left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
            //left join dbo.JobWorkTransformationContractChild mp on mp.JobWorkTransformationContractMasterId=II.JWContractId and mp.Id=IID.JWTCMID
            //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
            //LEFT join (Select Sum(mi.GrossConsumption) GrossConsump,mi.ArticleId,mm.Id as MaterialMstId,mi.JobWorkTransformationContractChildMasterId
            //										from dbo.JobWorkTransformationContractChild3 mi
            //										left join MST.MaterialMasterArticle mma on mma.Id= mi.ArticleId
            //										left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
            //										group by mi.ArticleId,mi.JobWorkTransformationContractChildMasterId,mm.Id)
            //										JWMi on JWMi.ArticleId=IM.ArticleId and JWMi.JobWorkTransformationContractChildMasterId=mp.Id and JWMi.MaterialMstId=IM.MaterialMasterId
            //--left join dbo.JobWorkTransformationContractChild3 mi on mi.JobWorkTransformationContractChildMasterId=mp.Id and mi.ArticleId=IM.ArticleId
            //--left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
            //left join (select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article, IM.ArticleId,IID.InventoryMaterialId
            //							            from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
            //                                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
            //                                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
            //                                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
            //										where II.JWContractId='" + PrintTabId + @"'
            //										group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName)
            //										kk on kk.InventoryMaterialId=IM.Id
            //                                        left join (select Sum(IIH.Qty) as TQty,IIH.InventoryIssueDetailId,Sum(IIH.Rate * IIH.Qty) as TotalAmount
            //										           ,AverageIssueRate=(Sum(IIH.Rate * IIH.Qty) /Sum(IIH.Qty))
            //													from TRN.InventoryIssueHistory IIH left join TRN.InventoryIssueDetail IID on IID.Id=IIH.InventoryIssueDetailId 
            //													group by IIH.InventoryIssueDetailId)
            //													AA on AA.InventoryIssueDetailId=IID.Id
            //										where mp.JobWorkTransformationContractMasterId='" + PrintTabId + @"' and II.Id='" + IssueId + @"' --and mi.Id is not null 
            //										and II.Types='InventoryJWIssue' and JWMi.GrossConsump is not null
            //										group by IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id
            //										, jwi.UserName--, mi.Id
            //										--,jwii.UserName
            //										,mp.Quantity--, mi.GrossConsumption
            //										,IID.TransactionQty
            //										,JWMi.GrossConsump--,IID.AvgRate,IID.AvgAmount
            //                                        ,AA.TQty,AA.AverageIssueRate
            //										order by mp.Id";

            //     var sql = @"select distinct IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id as JWOutputId, jwi.UserName as JWOutputItem
            //                 --, mi.Id as JwInputId,jwii.UserName as JWInputItem
            //                 --,RequiredQuantity = (mp.Quantity * JWMi.GrossConsump)
            //                 ,RequiredQuantity = (mp.Quantity * JWMi.GrossConsumption)
            //                 --,BalanceToIssue = (mp.Quantity * JWMi.GrossConsump) - (ISNULL(kk.TotalIssuedQty, '0'))
            //                 ,BalanceToIssue = (mp.Quantity * JWMi.GrossConsumption) - (ISNULL(kk.TotalIssuedQty, '0'))
            //                 ,IID.TransactionQty--,IID.AvgRate,IID.AvgAmount
            //                 ,AA.TQty,AA.AverageIssueRate--,AverageAmount = round((AA.AverageIssueRate * IID.TransactionQty), 2)
            //                 --,AverageAmount = round((AA.BooksCurrencyBaseRate * IID.TransactionQty), 2),uom.UserName as IssueUoM
            //                 ,AA.BaseRateeee
            //                  --,AverageAmount = round((AA.AverageIssueRate * IID.TransactionQty), 2),uom.UserName as IssueUoM
            //,AverageAmount=round((AA.AverageIssueRate * AA.BaseRateeee * IID.TransactionQty),2)
            //                  ,uom.UserName as IssueUoM
            //                 from TRN.InventoryIssueDetail IID left
            //                 join TRN.InventoryIssue II on II.Id = IID.InventoryIssueId
            //                 left
            //                 join TRN.InventoryMaterial IM on IM.Id = IID.InventoryMaterialId
            //                 left
            //                 join dbo.JobWorkTransformationContractChild mp on mp.JobWorkTransformationContractMasterId = II.JWContractId and mp.Id = IID.JWTCMID
            //                 left join HKP.JobWorkItem jwi on jwi.Id = mp.JobWorkItemMasterId
            //                 LEFT join(Select Sum(mi.GrossConsumption) GrossConsump, mi.GrossConsumption, mi.ArticleId, mm.Id as MaterialMstId,mi.JobWorkTransformationContractChildMasterId
            //                                 from dbo.JobWorkTransformationContractChild3 mi

            //                                 left join MST.MaterialMasterArticle mma on mma.Id = mi.ArticleId

            //                                 left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId

            //                                 group by mi.ArticleId,mi.JobWorkTransformationContractChildMasterId,mm.Id,mi.GrossConsumption)
            //			JWMi on JWMi.ArticleId = IM.ArticleId and JWMi.JobWorkTransformationContractChildMasterId = mp.Id and JWMi.MaterialMstId = IM.MaterialMasterId
            //                                 --left join dbo.JobWorkTransformationContractChild3 mi on mi.JobWorkTransformationContractChildMasterId = mp.Id and mi.ArticleId = IM.ArticleId
            //                                   --left join HKP.JobWorkItem jwii on jwii.Id = mi.JobWorkItemId
            //                             left join(select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article
            //                             , IM.ArticleId,IID.InventoryMaterialId
            //                             ,IID.JWTCMID                                       
            //                             from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id = IID.InventoryIssueId
            //                                 left join TRN.InventoryMaterial IM on IM.Id = IID.InventoryMaterialId
            //                                 left join MST.MaterialMaster mm on mm.Id = IM.MaterialMasterId
            //                                 left join MST.MaterialMasterArticle mma on mma.Id = IM.ArticleId

            //                                 where II.JWContractId = '" + PrintTabId + @"' --and IID.InventoryIssueId='" + IssueId + @"'

            //                                 group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName,IID.JWTCMID)
            //			kk on kk.InventoryMaterialId = IM.Id and kk.JWTCMID=mp.Id
            //                                 left join(select Sum(IIH.Qty) as TQty,IIH.InventoryIssueDetailId--,Sum(IIH.Rate * IIH.Qty) as TotalAmount
            //			           --,AverageIssueRate = (Sum(IIH.Rate * IIH.Qty) / Sum(IIH.Qty))
            //					   ,SUM(IIH.BooksCurrencyBaseRate) as BaseRate,AverageIssueRate=(SUM(IIH.BooksCurrencyBaseRate) / Sum(IIH.Qty))
            //					   --,IIH.BooksCurrencyBaseRate
            //                                             ,IR.ToCurrencyRate as BaseRateeee
            //                                             from TRN.InventoryIssueHistory IIH left join TRN.InventoryIssueDetail IID on IID.Id = IIH.InventoryIssueDetailId
            //                                             left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
            //						left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
            //                                             group by IIH.InventoryIssueDetailId--,IIH.BooksCurrencyBaseRate
            //                                             ,IR.ToCurrencyRate
            //                                              )
            //						AA on AA.InventoryIssueDetailId = IID.Id

            //                                 left join SCS.UnitOfMeasurement uom on uom.Id=IID.TransactionUoMId

            //                                 where mp.JobWorkTransformationContractMasterId = '" + PrintTabId + @"' and II.Id = '" + IssueId + @"'--and mi.Id is not null
            //                                    and II.Types = 'InventoryJWIssue' and JWMi.GrossConsumption is not null-- and JWMi.GrossConsump is not null

            //                                 group by IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id
            //			, jwi.UserName--, mi.Id
            //                                 --,jwii.UserName
            //			,mp.Quantity--, mi.GrossConsumption
            //			,IID.TransactionQty
            //                                 --,JWMi.GrossConsump--,IID.AvgRate,IID.AvgAmount
            //                                 ,AA.TQty,AA.AverageIssueRate,JWMi.GrossConsumption--,AA.BooksCurrencyBaseRate
            //                                 ,uom.UserName,AA.BaseRateeee
            //                                 order by mp.Id";

            var sql = @"select distinct IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id as JWOutputId, jwi.UserName as JWOutputItem
                        --, mi.Id as JwInputId,jwii.UserName as JWInputItem
                        --,RequiredQuantity = (mp.Quantity * JWMi.GrossConsump)
                        ,RequiredQuantity = (mp.Quantity * JWMi.GrossConsumption)
                        --,BalanceToIssue = (mp.Quantity * JWMi.GrossConsump) - (ISNULL(kk.TotalIssuedQty, '0'))
                        ,BalanceToIssue = (mp.Quantity * JWMi.GrossConsumption) - (ISNULL(kk.TotalIssuedQty, '0'))
                        ,IID.TransactionQty--,IID.AvgRate,IID.AvgAmount
                       -- ,AA.TQty,AA.AverageIssueRate--,AverageAmount = round((AA.AverageIssueRate * IID.TransactionQty), 2)
                        --,AverageAmount = round((AA.BooksCurrencyBaseRate * IID.TransactionQty), 2),uom.UserName as IssueUoM
						--,AA.BaseRateeee
                         --,AverageAmount = round((AA.AverageIssueRate * IID.TransactionQty), 2),uom.UserName as IssueUoM
					--	 ,AverageAmount=round((AA.AverageIssueRate * AA.BaseRateeee * IID.TransactionQty),2)
						 ,uom.UserName as IssueUoM
						 ,BB.TotalAmt as AverageAmount
						 ,AveRateeee=(BB.TotalAmt/IID.TransactionQty)
                        from TRN.InventoryIssueDetail IID left
                        join TRN.InventoryIssue II on II.Id = IID.InventoryIssueId
                        left
                        join TRN.InventoryMaterial IM on IM.Id = IID.InventoryMaterialId
                        left
                        join dbo.JobWorkTransformationContractChild mp on mp.JobWorkTransformationContractMasterId = II.JWContractId and mp.Id = IID.JWTCMID
                        left join HKP.JobWorkItem jwi on jwi.Id = mp.JobWorkItemMasterId
                        LEFT join(Select Sum(mi.GrossConsumption) GrossConsump, mi.GrossConsumption, mi.ArticleId, mm.Id as MaterialMstId,mi.JobWorkTransformationContractChildMasterId
                                        from dbo.JobWorkTransformationContractChild3 mi

                                        left join MST.MaterialMasterArticle mma on mma.Id = mi.ArticleId

                                        left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId

                                        group by mi.ArticleId,mi.JobWorkTransformationContractChildMasterId,mm.Id,mi.GrossConsumption)
										JWMi on JWMi.ArticleId = IM.ArticleId and JWMi.JobWorkTransformationContractChildMasterId = mp.Id and JWMi.MaterialMstId = IM.MaterialMasterId
                                    left join(select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article
                                    , IM.ArticleId,IID.InventoryMaterialId
                                    ,IID.JWTCMID                                       
                                    from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id = IID.InventoryIssueId
                                        left join TRN.InventoryMaterial IM on IM.Id = IID.InventoryMaterialId
                                        left join MST.MaterialMaster mm on mm.Id = IM.MaterialMasterId
                                        left join MST.MaterialMasterArticle mma on mma.Id = IM.ArticleId

                                        where II.JWContractId = '" + PrintTabId + @"' --and IID.InventoryIssueId='" + IssueId + @"'

                                        group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName,IID.JWTCMID)
										kk on kk.InventoryMaterialId = IM.Id and kk.JWTCMID=mp.Id
										left join (select Sum(x.TotalAmount) as TotalAmt,x.MaterialId,x.JWInputMaterial,x.ArticleId,x.JWInputArticle,x.Id,x.InventoryMaterialId 
										from (
                        select om.Id,IIH.Qty as GRNIssueQty,IID.InventoryMaterialId,mm.Id as MaterialId,mm.UserName as JWInputMaterial,mma.Id as ArticleId, mma.StandardName as JWInputArticle
                          --,TotalAmount=round((IIH.Rate * IR.ToCurrencyRate * IIH.Qty),2)
                           ,TotalAmount=round(((IIH.Rate/86) * IR.ToCurrencyRate * IIH.Qty),2)
                        from dbo.JobWorkTransformationContractChild om left join TRN.InventoryIssueDetail IID on om.Id=IID.JWTCMID
                        left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
                        left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                        left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
                        where IID.InventoryIssueId='" + IssueId + @"' 
						) x
						group by x.JWInputMaterial,x.ArticleId,x.Id,x.MaterialId,x.JWInputArticle,x.InventoryMaterialId
						)
						BB on BB.Id=mp.Id and BB.InventoryMaterialId=IM.Id

                                        left join SCS.UnitOfMeasurement uom on uom.Id=IID.TransactionUoMId

                                        where mp.JobWorkTransformationContractMasterId = '" + PrintTabId + @"' and II.Id = '" + IssueId + @"'--and mi.Id is not null
                                           and II.Types = 'InventoryJWIssue' and JWMi.GrossConsumption is not null-- and JWMi.GrossConsump is not null

                                        group by IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article, mp.Id
										, jwi.UserName
										,mp.Quantity
										,IID.TransactionQty
										,JWMi.GrossConsumption
                                        ,uom.UserName
										,BB.TotalAmt
                                        order by mp.Id";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetTransformationGRNDataById(string IssueId)
        {
            //var sql = @"select om.Id, IRD.InventoryReceiveId as GRNNo,IRD.Id as GRNRowId,uom.UserName as IssueUoM,IIH.Qty as GRNIssueQty,mm.UserName as JWInputMaterial
            //            , mma.StandardName as JWInputArticle, C.Code as TransactionCurrency, IIH.Rate as TransactionRate, IR.ToCurrencyRate as BaseRate
            //             , CC.Code as BaseCurrency,(IIH.Rate * IIH.Qty) as TotalAmount
            //            from dbo.JobWorkTransformationContractChild om left join TRN.InventoryIssueDetail IID on om.Id=IID.JWTCMID
            //            left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
            //            left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
            //            left join SCS.UnitOfMeasurement uom on uom.Id=IID.BaseUOMId
            //            left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
            //            left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
            //            left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
            //            left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
            //            left join SCS.Currency C on C.Id=IR.CurrencyId
            //            left join SCS.Currency CC on CC.Id=IR.BaseCurrencyId
            //            where IID.InventoryIssueId='" + IssueId + @"'";

            var sql = @"select om.Id, IRD.InventoryReceiveId as GRNNo,IRD.Id as GRNRowId,uom.UserName as IssueUoM,IIH.Qty as GRNIssueQty,mm.UserName as JWInputMaterial
                        , mma.StandardName as JWInputArticle, C.Code as TransactionCurrency--, IIH.Rate as TransactionRate
                        ,TransactionRate=(IIH.Rate/86)
                         --, IR.ToCurrencyRate as BaseRate
                         --,BaseRate=(IIH.Rate * IR.ToCurrencyRate)
                           ,BaseRate=((IIH.Rate/86) * IR.ToCurrencyRate)
                         , CC.Code as BaseCurrency--,(IIH.Rate * IIH.Qty) as TotalAmount
						-- ,TotalAmount=round((IR.ToCurrencyRate * IIH.Qty),2)
                          --,TotalAmount=round((IIH.Rate * IR.ToCurrencyRate),2)
                           --,TotalAmount=round((IIH.Rate * IR.ToCurrencyRate * IIH.Qty),2)
                            ,TotalAmount=round(((IIH.Rate/86) * IR.ToCurrencyRate * IIH.Qty),2)
                        from dbo.JobWorkTransformationContractChild om left join TRN.InventoryIssueDetail IID on om.Id=IID.JWTCMID
                        left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
                        left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
                        left join SCS.UnitOfMeasurement uom on uom.Id=IID.BaseUOMId
                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                        left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
                        left join SCS.Currency C on C.Id=IR.CurrencyId
                        left join SCS.Currency CC on CC.Id=IR.BaseCurrencyId
                        where IID.InventoryIssueId='" + IssueId + @"' and IRD.InventoryReceiveId is not null ";

            return _sqlRepository.GetDataTable(sql);
        }

        #endregion end Reports for Transformation Contract

        // New Changes

        [Authorize, HttpGet]
        public JsonResult GetCostCenterLoadNewFun(string EntityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(JWTIR.GetCostCenterLoadNewFun(EntityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetDataByInventoryIssue(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(JWTIR.GetDataByInventoryIssue(Id, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getStoragloc(string JLId)
        {
            try
            {

                return Json(JWTIR.getStoragloc(JLId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // Inventory Issue Report

        #region Reports for Inventory Issue

        [HttpGet, Authorize]
        public ActionResult GetIIPrintReport(ReportFormat reportFormat, string IssueId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Inventory Issue Chalaan " + IssueId + "";
            var workbook = GetIIReportWorkSheet(IssueId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetIIReportWorkSheet(string IssueId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "InventoryIssueChalaan";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetIIReportDataById(IssueId);
            DataTable TransformationIssueReturnChilddata = GetIIIssueReturnChildDataById(IssueId);
            DataTable TransformationIssueGRNdata = GetIIGRNDataById(IssueId);
            if (data.Rows.Count > 0)
            {
                int ColValueAddedDateHeader = 1;
                int ColValueAddedDateEnd;
                int ColVACTimeHeader;
                int ColVACTimeEnd;
                int ColVACTimeName;
                int ColEntityHeader;
                int ColEntityEnd;
                int ColEntityName;
                int ColPartyNameHeader;
                //    int ColPartyNameEnd;
                int ColPartyNameName;
                int ColVAProcessStartDateHeader = 1;
                int ColVAProcessStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColValueAddedDateHeader, "Issue No.", 12, ExcelHAlign.HAlignLeft);
                ColValueAddedDateHeader++;
                ColValueAddedDateEnd = ColValueAddedDateHeader + 1;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Text = data.Rows[0]["TransformationIssueId"].ToString();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Merge();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColValueAddedDateEnd++;

                ColEntityHeader = ColValueAddedDateEnd;
                SetHeaderTextTop(ref sheet, ROW, ColEntityHeader, "Company", 20, ExcelHAlign.HAlignLeft);
                ColEntityHeader++;
                ColEntityEnd = ColEntityHeader + 1;
                ColEntityName = ColEntityHeader;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Text = data.Rows[0]["Company"].ToString();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Merge();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColEntityEnd++;



                int ColIssueIdEnd = ColEntityEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIssueIdEnd, "Plant", 20, ExcelHAlign.HAlignLeft);
                ColIssueIdEnd++;
                int ColVAProcessEndDate = ColIssueIdEnd;
                int ColVAProcessEndDateEnd = ColIssueIdEnd + 1;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["Plant"].ToString();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColVAProcessEndDateEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessEndDateEnd++;
                int ColIssueDate = ColVAProcessEndDateEnd;
                int ColIssueDateEnd = ColVAProcessEndDateEnd + 1;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Merge();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //    ColIssueDateEnd++;

                int ColPStartDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColPStartDate, "Issue Date", 12, ExcelHAlign.HAlignLeft);
                ColPStartDate++;
                ColVAProcessStartDateEnd = ColPStartDate + 1;
                int ColAddress = ColPStartDate;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Text = data.Rows[0]["TransformationDate"].ToString();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Merge();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColVAProcessStartDateEnd++;

                //     int ColPEndDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateEnd, "Storage Location", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateEnd++;
                int ColProcessEndDate = ColVAProcessStartDateEnd;
                int ColProcessEndDateEnd = ColVAProcessStartDateEnd + 1;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Text = data.Rows[0]["JobWorkLocation"].ToString();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColProcessEndDateEnd++;

                int ColPrtyName = ColProcessEndDateEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColPrtyName, "To Whom", 20, ExcelHAlign.HAlignLeft);
                ColPrtyName++;
                int ColPartyName = ColPrtyName;
                int ColPartyNameEnd = ColPrtyName + 1;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Text = data.Rows[0]["ByWhom"].ToString();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Merge();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //     ROW++;
                ColPartyNameEnd++;


                int ColIssuebyEnd = ColPartyNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColIssuebyEnd, "Issue Type", 20, ExcelHAlign.HAlignLeft);
                ColIssuebyEnd++;
                int ColIssueby = ColIssuebyEnd;
                int ColIssueByEnd = ColIssuebyEnd + 1;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Text = data.Rows[0]["IssueType"].ToString();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Merge();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //  ColIssueByEnd++;

                int ColCCDATe = 1;
                SetHeaderTextTop(ref sheet, ROW, ColCCDATe, "Currency", 20, ExcelHAlign.HAlignLeft);
                ColCCDATe++;
                int ColVAContractClosingDate = ColCCDATe;
                int ColVAContractClosingDateEnd = ColCCDATe + 1;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Text = data.Rows[0]["Currency"].ToString();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Merge();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColVAContractClosingDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAContractClosingDateEnd, "Refference No", 20, ExcelHAlign.HAlignLeft);
                ColVAContractClosingDateEnd++;
                int ColContractId = ColVAContractClosingDateEnd;
                int ColContractIdEnd = ColVAContractClosingDateEnd + 1;
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].Text = data.Rows[0]["RefferenceNo"].ToString();
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].Merge();
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColContractIdEnd++;


                int ColIR = ColContractIdEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIR, "Order specific", 15, ExcelHAlign.HAlignLeft);
                ColIR++;
                int ColIssueReturn = ColIR;
                int ColIssueReturnEnd = ColIR + 1;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Text = data.Rows[0]["Orderspecific"].ToString();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Merge();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //    ROW++;
                ColIssueReturnEnd++;



                SetHeaderTextTop(ref sheet, ROW, ColIssueReturnEnd, "Order Ref No", 20, ExcelHAlign.HAlignLeft);
                ColIssueReturnEnd++;
                int ColJobWorkLocation = ColIssueReturnEnd;
                int ColJobWorkLocationEnd = ColIssueReturnEnd + 1;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Text = data.Rows[0]["OrderRefNo"].ToString();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Merge();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                int ColRemarks = 1;
                SetHeaderTextTop(ref sheet, ROW, ColRemarks, "Production Order Id", 20, ExcelHAlign.HAlignLeft);
                ColRemarks++;
                int ColContractRemarks = ColRemarks;
                int ColContractRemarksEnd = ColRemarks + 1;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Text = data.Rows[0]["ProductionOrderId"].ToString();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Merge();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColContractRemarksEnd++;

                int ColContractIsseStatus = ColContractRemarksEnd;
                SetHeaderTextTop(ref sheet, ROW, ColContractIsseStatus, "Contract Id", 20, ExcelHAlign.HAlignLeft);
                ColContractIsseStatus++;
                int ColIssueStatus = ColContractIsseStatus;
                int ColIssueStatusEnd = ColContractIsseStatus + 1;
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Text = data.Rows[0]["ContractId"].ToString();
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Merge();
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColIssueStatusEnd++;

                int ColIsseStatus = ColIssueStatusEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIsseStatus, "Status", 20, ExcelHAlign.HAlignLeft);
                ColIsseStatus++;
                int ColIIIssueStatus = ColIsseStatus;
                int ColIIIssueStatusEnd = ColIsseStatus + 1;
                sheet.Range[ROW, ColIIIssueStatus, ROW, ColIIIssueStatusEnd].Text = data.Rows[0]["Status"].ToString();
                sheet.Range[ROW, ColIIIssueStatus, ROW, ColIIIssueStatusEnd].Merge();
                sheet.Range[ROW, ColIIIssueStatus, ROW, ColIIIssueStatusEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIIIssueStatus, ROW, ColIIIssueStatusEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColIIIssueStatusEnd++;

                int ColIsseVoucher= ColIIIssueStatusEnd;
                SetHeaderTextTop(ref sheet, ROW, ColIsseVoucher, "Voucher Id", 20, ExcelHAlign.HAlignLeft);
                ColIsseVoucher++;
                int ColIIIssueVoucher = ColIsseVoucher;
                int ColIIIssueVoucherEnd = ColIsseVoucher + 1;
                sheet.Range[ROW, ColIIIssueVoucher, ROW, ColIIIssueVoucherEnd].Text = data.Rows[0]["VoucherId"].ToString();
                sheet.Range[ROW, ColIIIssueVoucher, ROW, ColIIIssueVoucherEnd].Merge();
                sheet.Range[ROW, ColIIIssueVoucher, ROW, ColIIIssueVoucherEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIIIssueVoucher, ROW, ColIIIssueVoucherEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                int ColIssueRequestMasterId = 1;
                SetHeaderTextTop(ref sheet, ROW, ColIssueRequestMasterId, "IssueRequest Master Id", 20, ExcelHAlign.HAlignLeft);
                ColIssueRequestMasterId++;
                int ColissueReq = ColIssueRequestMasterId;
                int ColIssueReqEnd = ColIssueRequestMasterId + 1;
                sheet.Range[ROW, ColissueReq, ROW, ColIssueReqEnd].Text = data.Rows[0]["IssueRequestMasterId"].ToString();
                sheet.Range[ROW, ColissueReq, ROW, ColIssueReqEnd].Merge();
                sheet.Range[ROW, ColissueReq, ROW, ColIssueReqEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColissueReq, ROW, ColIssueReqEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;


            }

            //       Issue/ Return Child data

            int MPChildROW = ROW + 1;
            int MPChildendCol = 1;
            int MPChildCOL = 1;

            #region Material Planning Child Headers

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue/ Return Quantity", 12, ExcelHAlign.HAlignLeft);
            MPChildROW++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Inventory Issue Detail Id", 12, ExcelHAlign.HAlignLeft);
            int ColId = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue No", 12, ExcelHAlign.HAlignLeft);
            int ColJWOutputItemId = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputMaterial = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Article", 12, ExcelHAlign.HAlignLeft);
            int ColArticle = MPChildCOL;
            MPChildCOL++;

            //report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Required Quantity", 12, ExcelHAlign.HAlignLeft);
            //int ColRequiredQuantity = MPChildCOL;
            //MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issued Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTIRCTotalQty = MPChildCOL;
            MPChildCOL++;

            //report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Balance To Issue", 12, ExcelHAlign.HAlignLeft);
            //int ColBalanceToIssue = MPChildCOL;
            //MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue UoM", 12, ExcelHAlign.HAlignLeft);
            int ColJWIssueUoM = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue Quantity", 10, ExcelHAlign.HAlignLeft);
            int ColTIRCQty = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Average Issue Rate", 12, ExcelHAlign.HAlignLeft);
            int ColAvgRate = MPChildCOL;
            MPChildCOL++;

            //report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Base Rate", 12, ExcelHAlign.HAlignLeft);
            //int ColBaseRateeee = MPChildCOL;
            //MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue Amount", 10, ExcelHAlign.HAlignLeft);
            int ColAvgAmount = MPChildCOL;
            MPChildROW++;

            MPChildendCol = MPChildCOL;
            #endregion Headers

            string InventoryIssueId = "";
            var StartRows = 0;
            var EndRows = 0;
            int RowIndexNo = MPChildROW;
            StartRows = MPChildROW;

            for (int i = 0; i < TransformationIssueReturnChilddata.Rows.Count; i++)
            {

                if (InventoryIssueId != TransformationIssueReturnChilddata.Rows[i]["InventoryIssueId"].ToString())
                {

                    if (RowIndexNo < MPChildROW)
                    {
                        //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[RowIndexNo, ColJWOutputItemId, MPChildROW - 1, ColJWOutputItemId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndexNo, ColJWOutputItemId, MPChildROW - 1, ColJWOutputItemId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndexNo = MPChildROW;
                }

                sheet[MPChildROW, ColJWOutputItemId].Text = TransformationIssueReturnChilddata.Rows[i]["InventoryIssueId"].ToString();
                sheet[MPChildROW, ColId].Text = TransformationIssueReturnChilddata.Rows[i]["Id"].ToString();
                //sheet[MPChildROW, ColJWInputItemId].Text = TransformationIssueReturnChilddata.Rows[i]["JwInputId"].ToString();
                //sheet[MPChildROW, ColJWInputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JWInputItem"].ToString();
                sheet[MPChildROW, ColJWInputMaterial].Text = TransformationIssueReturnChilddata.Rows[i]["Material"].ToString();
                sheet[MPChildROW, ColArticle].Text = TransformationIssueReturnChilddata.Rows[i]["Article"].ToString();
         //       sheet[MPChildROW, ColBalanceToIssue].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["BalanceToIssue"].ToString());
         //       sheet[MPChildROW, ColRequiredQuantity].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["RequiredQuantity"].ToString());
                sheet[MPChildROW, ColTIRCTotalQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TotalIssuedQty"].ToString());
                sheet[MPChildROW, ColTIRCQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TransactionQty"].ToString());

                sheet[MPChildROW, ColAvgRate].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["AveRateeee"].ToString());

                //      sheet[MPChildROW, ColBaseRateeee].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["BaseRateeee"].ToString());

                sheet[MPChildROW, ColAvgAmount].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["AverageAmount"].ToString());

                sheet[MPChildROW, ColJWIssueUoM].Text = TransformationIssueReturnChilddata.Rows[i]["IssueUoM"].ToString();

                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
                InventoryIssueId = TransformationIssueReturnChilddata.Rows[i]["InventoryIssueId"].ToString();

                MPChildROW++;
            }

            int ColTotal = 1;
            report.SetHeaderText(ref sheet, MPChildROW, ColTotal, "Total", 10, ExcelHAlign.HAlignLeft);
            //       int ColAvgAmount = MPChildCOL;
            //       MPChildROW++;

            // SUM OF TOTAL ISSUED QUANTITY
            int ColTotalIssQty = 7;
            decimal p = 0;
            decimal q = 0;
            decimal r = 0;
            for (int j = 0; j < TransformationIssueReturnChilddata.Rows.Count; j++)
            {

                p = Convert.ToDecimal(TransformationIssueReturnChilddata.Rows[j]["TransactionQty"]);
                r = p + q;
                q = r;
                sheet[MPChildROW, ColTotalIssQty].Number = clsStaticInfo.dbl(q);
                sheet.Range[MPChildROW, ColTotalIssQty].CellStyle.Font.Bold = true;
            }

            // SUM OF TOTAL Amount
            int ColTotalRecQty = 9;
            decimal x = 0;
            decimal y = 0;
            decimal z = 0;
            for (int j = 0; j < TransformationIssueReturnChilddata.Rows.Count; j++)
            {

                x = Math.Round(Convert.ToDecimal(TransformationIssueReturnChilddata.Rows[j]["AverageAmount"]), 2);
                z = Math.Round(x, 2) + Math.Round(y, 2);
                y = Math.Round(z, 2);
                sheet[MPChildROW, ColTotalRecQty].Number = Math.Round(clsStaticInfo.dbl(y), 2);
                sheet.Range[MPChildROW, ColTotalRecQty].CellStyle.Font.Bold = true;
            }

            EndRows = MPChildROW - 1;

            if (RowIndexNo < MPChildROW - 1)
            {
                //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                sheet.Range[RowIndexNo, ColJWOutputItemId, MPChildROW - 1, ColJWOutputItemId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndexNo, ColJWOutputItemId, MPChildROW - 1, ColJWOutputItemId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // GRN DETAILS

            int GRNROW = MPChildROW + 2;
            int GRNendCol = 1;
            int GRNCOL = 1;

            #region GRN DETAILS Headers

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "GRN Details", 12, ExcelHAlign.HAlignLeft);
            GRNROW++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Inventory Issue Detail Id", 12, ExcelHAlign.HAlignLeft);
            int ColGRNId = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Material", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputMat = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Article", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputArticle = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "GRN No", 12, ExcelHAlign.HAlignLeft);
            int ColGRNNo = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "GRN Row Id", 12, ExcelHAlign.HAlignLeft);
            int ColGRNRowId = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Issue UoM", 12, ExcelHAlign.HAlignLeft);
            int ColIssueUoM = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Issue Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColGRNIssueQty = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Transaction Currency", 12, ExcelHAlign.HAlignLeft);
            int ColTransactionCurrency = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Transaction Rate", 12, ExcelHAlign.HAlignLeft);
            int ColTransactionRate = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Base Currency", 12, ExcelHAlign.HAlignLeft);
            int ColBaseCurrency = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Base Rate", 10, ExcelHAlign.HAlignLeft);
            int ColBaseRate = GRNCOL;
            GRNCOL++;

            report.SetHeaderText(ref sheet, GRNROW, GRNCOL, "Total Amount", 10, ExcelHAlign.HAlignLeft);
            int ColTotalAmount = GRNCOL;
            GRNROW++;
            GRNendCol = GRNCOL;
            #endregion Headers

            string GRNNo = "";
            var GRNStartRows = 0;
            var GRNEndRows = 0;
            int GRNRowIndexNo = GRNROW;
            GRNStartRows = GRNROW;

            for (int i = 0; i < TransformationIssueGRNdata.Rows.Count; i++)
            {

                if (GRNNo != TransformationIssueGRNdata.Rows[i]["GRNNo"].ToString())
                {

                    if (GRNRowIndexNo < GRNROW)
                    {
                        //sheet.Range[GRNRowIndexNo, ColJobWorkItem, GRNROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[GRNRowIndexNo, ColJWInputMat, GRNROW - 1, ColJWInputMat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[GRNRowIndexNo, ColJWInputMat, GRNROW - 1, ColJWInputMat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    GRNRowIndexNo = GRNROW;
                }

                sheet[GRNROW, ColGRNId].Text = TransformationIssueGRNdata.Rows[i]["Id"].ToString();
                sheet[GRNROW, ColGRNNo].Text = TransformationIssueGRNdata.Rows[i]["GRNNo"].ToString();
                sheet[GRNROW, ColGRNRowId].Text = TransformationIssueGRNdata.Rows[i]["GRNRowId"].ToString();

                sheet[GRNROW, ColJWInputMat].Text = TransformationIssueGRNdata.Rows[i]["JWInputMaterial"].ToString();
                sheet[GRNROW, ColJWInputArticle].Text = TransformationIssueGRNdata.Rows[i]["JWInputArticle"].ToString();

                sheet[GRNROW, ColGRNIssueQty].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["GRNIssueQty"].ToString());
                sheet[GRNROW, ColIssueUoM].Text = TransformationIssueGRNdata.Rows[i]["IssueUoM"].ToString();

                sheet[GRNROW, ColTransactionCurrency].Text = TransformationIssueGRNdata.Rows[i]["TransactionCurrency"].ToString();
                sheet[GRNROW, ColTransactionRate].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["TransactionRate"].ToString());

                //    sheet[GRNROW, ColTIRCQty].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["TransactionQty"].ToString());

                sheet[GRNROW, ColBaseCurrency].Text = TransformationIssueGRNdata.Rows[i]["BaseCurrency"].ToString();
                sheet[GRNROW, ColBaseRate].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["BaseRate"].ToString());

                sheet[GRNROW, ColTotalAmount].Number = clsStaticInfo.dbl(TransformationIssueGRNdata.Rows[i]["TotalAmount"].ToString());

                sheet.Range[GRNROW, 1, GRNROW, GRNendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[GRNROW, 1, GRNROW, GRNendCol].BorderAround(ExcelLineStyle.Hair);
                GRNNo = TransformationIssueGRNdata.Rows[i]["GRNNo"].ToString();

                GRNROW++;
            }

            int ColGRNTotal = 1;
            report.SetHeaderText(ref sheet, GRNROW, ColGRNTotal, "Total", 10, ExcelHAlign.HAlignLeft);
            //       int ColAvgAmount = MPChildCOL;
            //       GRNROW++;

            // SUM OF TOTAL GRN ISSUED QUANTITY
            int ColTotalGRNIssQty = 7;
            decimal a = 0;
            decimal b = 0;
            decimal c = 0;
            for (int j = 0; j < TransformationIssueGRNdata.Rows.Count; j++)
            {
                a = Convert.ToDecimal(TransformationIssueGRNdata.Rows[j]["GRNIssueQty"]);
                c = a + b;
                b = c;
                sheet[GRNROW, ColTotalGRNIssQty].Number = clsStaticInfo.dbl(b);
                sheet.Range[GRNROW, ColTotalGRNIssQty].CellStyle.Font.Bold = true;
            }

            // SUM OF TOTAL GRN Amount
            int ColTotalGRNAmount = 12;
            decimal xx = 0;
            decimal yy = 0;
            decimal zz = 0;
            for (int j = 0; j < TransformationIssueGRNdata.Rows.Count; j++)
            {
                xx = Math.Round(Convert.ToDecimal(TransformationIssueGRNdata.Rows[j]["TotalAmount"]), 2);
                zz = Math.Round(xx, 2) + Math.Round(yy, 2);
                yy = Math.Round(zz, 2);
                sheet[GRNROW, ColTotalGRNAmount].Number = Math.Round(clsStaticInfo.dbl(yy), 2);
                sheet.Range[GRNROW, ColTotalGRNAmount].CellStyle.Font.Bold = true;
            }

            GRNEndRows = GRNROW - 1;

            if (GRNRowIndexNo < GRNROW - 1)
            {
                //sheet.Range[GRNRowIndexNo, ColJobWorkItem, GRNROW - 1, ColJobWorkItem].Merge();
                sheet.Range[GRNRowIndexNo, ColJWInputMat, GRNROW - 1, ColJWInputMat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[GRNRowIndexNo, ColJWInputMat, GRNROW - 1, ColJWInputMat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            //GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            //GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, MPChildendCol + 6, "Inventory Issue Report", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetIIReportDataById(string IssueId)
        {

            var sql = @"select II.EntityId ,II.IssueDate,e.UserName as Entity,II.Id as TransformationIssueId,FORMAT(II.IssueDate,'dd-MMM-yyyy') as TransformationDate,emp.EmployeeName as ByWhom
									,Ms.UserName as JobWorkLocation, II.IssueType,II.PlantId,Pl.UserName as Plant, II.CompanyId, Cm.UserName as Company
									,Cr.Code as Currency,II.RefferenceNo,II.Orderspecific,II.OrderRefNo,II.ProductionOrderId,II.ContractId,II.Status,II.VoucherId
									,II.IssueRequestMasterId
                                    from TRN.InventoryIssue II left join ORG.Entity e on II.EntityId=e.Id
									left join ORG.Plant Pl on Pl.Id=II.PlantId
									left join ORG.Company Cm on Cm.Id=II.CompanyId
									left join dbo.EmployeeInformation emp on emp.SystemId=II.EmployeeId
									left join HKP.MaterialStorage Ms on Ms.Id=II.MaterialStorageId
									left join SCS.Currency Cr on Cr.Id=II.CurrencyId
                                    WHERE II.Id='" + IssueId + @"' ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetIIIssueReturnChildDataById(string IssueId)
        {

            var sql = @"select distinct IID.Id,IID.InventoryIssueId,kk.TotalIssuedQty,IID.InventoryMaterialId ,kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article--, mp.Id as JWOutputId
                        -- , jwi.UserName as JWOutputItem
                       -- ,RequiredQuantity = (mp.Quantity * JWMi.GrossConsumption)
                      --  ,BalanceToIssue = (mp.Quantity * JWMi.GrossConsumption) - (ISNULL(kk.TotalIssuedQty, '0'))
                        ,IID.TransactionQty
						 ,uom.UserName as IssueUoM
						 ,BB.TotalAmt as AverageAmount
						 ,AveRateeee=(BB.TotalAmt/IID.TransactionQty)
                        from TRN.InventoryIssueDetail IID left join TRN.InventoryIssue II on II.Id = IID.InventoryIssueId
                        left join TRN.InventoryMaterial IM on IM.Id = IID.InventoryMaterialId
                     --   left  join dbo.JobWorkTransformationContractChild mp on mp.JobWorkTransformationContractMasterId = II.JWContractId and mp.Id = IID.JWTCMID
                      --  left join HKP.JobWorkItem jwi on jwi.Id = mp.JobWorkItemMasterId
          --              LEFT join(Select Sum(mi.GrossConsumption) GrossConsump, mi.GrossConsumption, mi.ArticleId, mm.Id as MaterialMstId,mi.JobWorkTransformationContractChildMasterId
          --                              from dbo.JobWorkTransformationContractChild3 mi

          --                              left join MST.MaterialMasterArticle mma on mma.Id = mi.ArticleId

          --                              left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId

          --                              group by mi.ArticleId,mi.JobWorkTransformationContractChildMasterId,mm.Id,mi.GrossConsumption)
										--JWMi on JWMi.ArticleId = IM.ArticleId and JWMi.JobWorkTransformationContractChildMasterId = mp.Id and JWMi.MaterialMstId = IM.MaterialMasterId
                                    left join(select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article
                                    , IM.ArticleId,IID.InventoryMaterialId
                                   -- ,IID.JWTCMID                                       
                                    from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id = IID.InventoryIssueId
                                        left join TRN.InventoryMaterial IM on IM.Id = IID.InventoryMaterialId
                                        left join MST.MaterialMaster mm on mm.Id = IM.MaterialMasterId
                                        left join MST.MaterialMasterArticle mma on mma.Id = IM.ArticleId

                                      --  where --II.JWContractId = 'undefined' --and 
									--	IID.InventoryIssueId='" + IssueId + @"'
                                        group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName)
										kk on kk.InventoryMaterialId = IM.Id
										left join (select Sum(x.TotalAmount) as TotalAmt,x.MaterialId,x.JWInputMaterial,x.ArticleId,x.JWInputArticle--,x.Id
										,x.InventoryMaterialId 
										from (
                        select --om.Id,
						IIH.Qty as GRNIssueQty,IID.InventoryMaterialId,mm.Id as MaterialId,mm.UserName as JWInputMaterial,mma.Id as ArticleId, mma.StandardName as JWInputArticle
                          --,TotalAmount=round((IIH.Rate * IR.ToCurrencyRate * IIH.Qty),2)
                           ,TotalAmount=round(((IIH.Rate/86) * IR.ToCurrencyRate * IIH.Qty),2)
                        from TRN.InventoryIssue II --dbo.JobWorkTransformationContractChild om 
						left join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
                        left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
                        left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                        left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
                        where IID.InventoryIssueId='" + IssueId + @"' 
						) x
						group by x.JWInputMaterial,x.ArticleId--,x.Id
						,x.MaterialId,x.JWInputArticle,x.InventoryMaterialId
						)
						BB on --BB.=mp.Id and 
						BB.InventoryMaterialId=IM.Id

                                        left join SCS.UnitOfMeasurement uom on uom.Id=IID.TransactionUoMId

                                        where --mp.JobWorkTransformationContractMasterId = 'undefined' and 
										II.Id = '" + IssueId + @"'
                                          -- and II.Types != 'InventoryJWIssue' --and JWMi.GrossConsumption is not null-- and JWMi.GrossConsump is not null

                                        group by IID.InventoryIssueId,kk.TotalIssuedQty, kk.MaterialMasterId, kk.Material,kk.ArticleId,kk.Article--, mp.Id
										--, jwi.UserName
										--,mp.Quantity
										,IID.TransactionQty
										--,JWMi.GrossConsumption
                                        ,uom.UserName
										,BB.TotalAmt
										,IID.InventoryMaterialId,IID.Id
                                        order by IID.Id";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetIIGRNDataById(string IssueId)
        {

            var sql = @"select IID.Id, IRD.InventoryReceiveId as GRNNo,IRD.Id as GRNRowId,uom.UserName as IssueUoM,IIH.Qty as GRNIssueQty,mm.UserName as JWInputMaterial
                        , mma.StandardName as JWInputArticle, C.Code as TransactionCurrency
                        ,TransactionRate=(IIH.Rate/86)
                           ,BaseRate=((IIH.Rate/86) * IR.ToCurrencyRate)
                         , CC.Code as BaseCurrency
                            ,TotalAmount=round(((IIH.Rate/86) * IR.ToCurrencyRate * IIH.Qty),2)
                        from TRN.InventoryIssue II
						left join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
                        left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
                        left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
                        left join SCS.UnitOfMeasurement uom on uom.Id=IID.BaseUOMId
                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                        left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
                        left join SCS.Currency C on C.Id=IR.CurrencyId
                        left join SCS.Currency CC on CC.Id=IR.BaseCurrencyId
                        where IID.InventoryIssueId='" + IssueId + @"' and IRD.InventoryReceiveId is not null
                        order by IID.Id";

            return _sqlRepository.GetDataTable(sql);
        }

        #endregion end Reports for Transformation Contract

    }
}
public class JobWorkIssueReturnChild
{

    #region Scalar Properties

    public string Id { get; set; }
    public string JobWorkIssueReturnMasterId { get; set; }
    public string ContractLineItemId { get; set; }
    public string OrderChildId { get; set; }
    public string Quantity { get; set; }
    public string Remarks { get; set; }
    public string OWRId { get; set; }
    public string OrderSpecific { get; set; }
    public string IssueQuantity { get; set; }
    public string BalToIssue { get; set; }
    public string IssueActive { get; set; }


    #endregion Scalar Properties
}

//public class JobWorkTransformationIssueReturnChild
//{

//    #region Scalar Properties

//    public string Id { get; set; }
//    public string Material { get; set; }
//    public string Article { get; set; }
//    public string InputMaterialId { get; set; }
//    public string MaterialMasterArticleId { get; set; }
//    public string Quantity { get; set; }
//    public string Remarks { get; set; }
//    public string Value { get; set; }
//    public string LotNumber { get; set; }


//    #endregion Scalar Properties
//}