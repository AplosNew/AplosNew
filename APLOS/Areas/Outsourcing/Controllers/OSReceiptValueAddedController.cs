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
using Library.Data;

namespace Aplos.Areas.Outsourcing.Controllers
{
	public class OSReceiptValueAddedController : BaseController
	{
		JobWorkReceiptValueAdded R = new JobWorkReceiptValueAdded();

		string TableName = "dbo.JobWorkIssueReturn";
		string TableName1 = "dbo.JobWorkIssueReturnChild";
		string TableName2 = "JobWorkTransformationIssueReturn";

		#region Constructor
		private readonly SqlRepository _sqlRepository;
		public OSReceiptValueAddedController(SqlRepository Repository)
		{
			_sqlRepository = Repository;
			R = new JobWorkReceiptValueAdded();
		}
		#endregion
		#region Pages
		public ActionResult Aplos()
		{
			return View();
		}
		#endregion

		#region Load Data

		[Authorize, HttpGet]
		public JsonResult GetListOfPOGateEntry(string partyCode)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(R.GetListOfPOGateEntry(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyCode), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetListGateEntry(string partyCode)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(R.GetListGateEntry(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyCode), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetIndividualReportData(string Id)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetIndividualReportData(Id), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[Authorize, HttpGet]
		public JsonResult GetIndividualValAddedReportData(string Id, string ReceivedId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetIndividualValAddedReportData(Id, ReceivedId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}


		[Authorize, HttpGet]
		public JsonResult GetReceiptVAChildData(string PKId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetReceiptVAChildData(PKId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[Authorize, HttpGet]
		public JsonResult GetReceiptVAChildDatabyId(string Id)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetReceiptVAChildDatabyId(Id), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetReceiptTransChildData(string PKId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetReceiptTransChildData(PKId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}


		[Authorize, HttpGet]
		public JsonResult GetGradeWiseQuantityList()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				return Json(R.GetGradeWiseQuantityList(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[Authorize, HttpGet]
		public JsonResult GetVAGradeWiseQuantityList(string MasterId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				return Json(R.GetVAGradeWiseQuantityList(MasterId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[Authorize, HttpGet]
		public JsonResult GetTransformationReceiptCurrency(string Id)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				return Json(R.GetTransformationReceiptCurrency(Id), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}


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
			if (Type == "ValueAdded")
			{
				//sql = @"select vac.Id,TabType='Value Added', vac.EntityId,vac.PartyId,vac.Remarks,FORMAT(vac.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime]
				//                                       ,FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
				//                                       FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
				//                                       e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
				//                                       from dbo.OSTransformationPO vac left join ORG.Entity e on e.Id=vac.EntityId
				//                                       left join HKP.Party p on p.Id=vac.PartyId
				//                                       WHERE " + strkey + " and vac.POType='OSValueAddedPO' order by ValueAddedDate desc ";

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
                                     ,isnull(PLC.LCRef,'') as PurchaseLC
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
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
                        left join (Select SUM(IRD.TransactionQty) as ReceivedQty, SUM(IRD.TotalMaterialBooksCurrencyAmount) as TotalAmt,IRD.OSTransformationPOId
						from TRN.InventoryReceive IR left join TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
						group by IRD.OSTransformationPOId) TT on TT.OSTransformationPOId=IR.Id
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
                                     ,isnull(PLC.LCRef,'') as PurchaseLC
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
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
                        left join (Select SUM(IRD.TransactionQty) as ReceivedQty, SUM(IRD.TotalMaterialBooksCurrencyAmount) as TotalAmt,IRD.OSTransformationPOId
						from TRN.InventoryReceive IR left join TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
						group by IRD.OSTransformationPOId) TT on TT.OSTransformationPOId=IR.Id
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
                                    ,isnull(PLC.LCRef,'') as PurchaseLC
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
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
                        left join (Select SUM(IRD.TransactionQty) as ReceivedQty, SUM(IRD.TotalMaterialBooksCurrencyAmount) as TotalAmt,IRD.OSTransformationPOId
						from TRN.InventoryReceive IR left join TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
						group by IRD.OSTransformationPOId) TT on TT.OSTransformationPOId=IR.Id
						Where " + strkey + @" and IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						and IR.POType='OSValueAddedPO'
						) x
						--Order by PODate DESC
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.OSTransformationPO) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc ";

			}
			if (Type == "Transformation")
			{
				//sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime]
				//                                ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
				//                                FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
				//                                e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
				//                                from dbo.OSTransformationPO tc left join ORG.Entity e on e.Id=tc.EntityId
				//					left join HKP.Party p on p.Id=tc.PartyId
				//                                WHERE " + strkey + " and tc.POType='OSTransformationPO' order by tc.PODate desc";

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
                                    ,isnull(PLC.LCRef,'') as PurchaseLC
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
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
                         left join (Select SUM(IRD.TransactionQty) as ReceivedQty, SUM(IRD.TotalMaterialBooksCurrencyAmount) as TotalAmt,IRD.OSTransformationPOId
						from TRN.InventoryReceive IR left join TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
						group by IRD.OSTransformationPOId) TT on TT.OSTransformationPOId=IR.Id
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
                                    ,isnull(PLC.LCRef,'') as PurchaseLC
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
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
                        left join (Select SUM(IRD.TransactionQty) as ReceivedQty, SUM(IRD.TotalMaterialBooksCurrencyAmount) as TotalAmt,IRD.OSTransformationPOId
						from TRN.InventoryReceive IR left join TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
						group by IRD.OSTransformationPOId) TT on TT.OSTransformationPOId=IR.Id
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
                                     ,isnull(PLC.LCRef,'') as PurchaseLC
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
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						left join ORG.Entity E on E.Id=IR.EntityId
                        left join (Select SUM(IRD.TransactionQty) as ReceivedQty, SUM(IRD.TotalMaterialBooksCurrencyAmount) as TotalAmt,IRD.OSTransformationPOId
						from TRN.InventoryReceive IR left join TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
						group by IRD.OSTransformationPOId) TT on TT.OSTransformationPOId=IR.Id
						Where " + strkey + @" and IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + identity.PlantId + @"' 
                        --AND IR.POType='OSTransformationPO'	--AND IR.AddedBy='Shashank'	
                        AND isnull(IR.IsClosed,0)=0 
						and IR.POType='OSTransformationPO'
						) x
						--Order by PODate DESC
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.OSTransformationPO) BD ON BD.Id=x.Id 
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
				//sql = @"select vac.Id,TabType='Value Added', vac.EntityId,vac.PartyId,vac.Remarks,FORMAT(vac.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime]
				//                                ,FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
				//                                FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
				//                                e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
				//                                from dbo.OSTransformationPO vac left join ORG.Entity e on e.Id=vac.EntityId
				//					left join HKP.Party p on p.Id=vac.PartyId
				//                                WHERE vac.Id='" + Id + "' order by ValueAddedDate desc ";

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
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
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
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
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
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
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
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.OSTransformationPO) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc ";
			}
			if (TabType == "Transformation")
			{
				//sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
				//                                FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
				//                                e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
				//					, P.Id InvoicingPartyPlantId
				//					, IPP.UserName AS InvoicingBy
				//					, Am.Address1 InvoicingByAddress
				//					, P.Id DeliveryPartyPlantId
				//					, DPP.UserName AS DeliveryBy
				//					, Am.Address1 DeliveryByAddress
				//                                from dbo.OSTransformationPO tc 
				//					left join ORG.Entity e on e.Id=tc.EntityId
				//					left join HKP.Party p on p.Id=tc.PartyId
				//					LEFT JOIN [HKP].[PartyPlant] AS IPP ON p.Id= IPP.PartyId
				//					LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
				//					LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
				//					LEFT JOIN [HKP].[PartyPlant] AS DPP ON P.Id= DPP.PartyId
				//					LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
				//					LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
				//                                WHERE tc.Id='" + Id + "' order by ValueAddedDate desc";

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
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
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
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
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
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.OSTransformationPOId, SUM(A.Quantity) AS TransactionQty, SUM(A.Quantity * A.RatePerUnit) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM dbo.OSTransformationPODetail AS A
									JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId) AS IRD ON IRD.OSTransformationPOId=IR.Id
						LEFT JOIN (SELECT A.OSTransformationPOId, A.TransactionUoMId FROM dbo.OSTransformationPODetail AS A JOIN OSTransformationPO AS B ON A.OSTransformationPOId=B.Id
									WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.OSTransformationPOId, A.TransactionUoMId HAVING COUNT(A.OSTransformationPOId)> COUNT(A.TransactionUoMId)) AS TU ON TU.OSTransformationPOId=IR.Id
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
                        JOIN (SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from dbo.OSTransformationPO) BD ON BD.Id=x.Id 
						ORDER BY CONVERT(int,Col) desc";
			}

			return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
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
                               PR.SectionId,SS.UserName SubSection
                               ,PL.UserName Plant
                               FROM EmployeeInformation EMP
                               LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                               LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                               LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                               LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                               LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                               LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                               LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                               LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                               LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id

                           WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                      AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeId,'') from TRN.InventoryReceive where Id='" + Id + @"')
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

		[HttpPost]
		public JsonResult Create(Dictionary<string, object> data)
		{
			try
			{
				R.Create(data);
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

		[HttpPost]
		public JsonResult SaveReceiptVAChildTab(IEnumerable<jobworkreceiptvalueaddedchild> ReceiptVAChildData, string MasterId)
		{
			try
			{
				R.SaveReceiptVAChildTab(ReceiptVAChildData, MasterId);
				return Json(new { Error = false, Data = ReceiptVAChildData, Message = AplosMessage.Updated });

			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		// GRADE WISE QUANTITY VALUE ADDED

		[HttpPost]
		public JsonResult SaveGradeWiseValueAdded(IEnumerable<ReceiptValueAddedGradeWise> VAGradeWiseData, string MasterId)
		{
			try
			{
				R.SaveGradeWiseValueAdded(VAGradeWiseData, MasterId);
				return Json(new { Error = false, Data = VAGradeWiseData, Message = AplosMessage.Updated });

			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		// RECEIPT TRANSFORMATION

		[HttpPost, Authorize]
		public ActionResult LoadByWhomDetails(string Id)
		{

			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                           EMP.EmployeeName,EMP.EmployeeCode AS Code,
                           EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                               PR.UserName PositionName,
                               DEPT.UserName DepartmentName,S.UserName Section,
                               PR.SectionId,SS.UserName SubSection
                               ,PL.UserName Plant
                               FROM EmployeeInformation EMP
                               LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                               LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                               LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                               LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                               LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                               LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                               LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                               LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                               LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id

                           WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                           AND isnull(Emp.SystemID,'') not in (select isnull(ByWhomEmployeeId,'') from TRN.InventoryReceive where Id='" + Id + @"')
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

		[HttpPost]
		public JsonResult SaveReceiptTransformation(Dictionary<string, object> data, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
		{
			try
			{

				if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
				{
					CheckedByStatusForNoti = "False";
					ApprovedByStatusForNoti = "False";
				}

				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				data["CompanyGroupId"] = identity.CompanyGroupId;
				data["CompanyId"] = identity.CompanyId;
				data["PlantId"] = identity.PlantId;

				if (identity.EmployeeId == data["CheckedBy"].ToString())
				{
					throw new CustomException("Please select another employee for Check by.");
				}
				else
				{
					if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
					{

						data["AuthorizedBy"] = data["CheckedBy"];
						data["AuthorizedByStatus"] = "For Approval";
						data["CheckedBy"] = null;
						data["CheckedByStatus"] = null;
						data["IsApproved"] = false;
						data["RequiredPosting"] = true;

					}
					else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
					{
						data["CheckedByStatus"] = null;
						data["AuthorizedByStatus"] = null;
						data["CheckedBy"] = null;
						data["AuthorizedBy"] = null;
						data["IsApproved"] = true;
						data["RequiredPosting"] = true;

					}
					else
					{
						data["CheckedBy"] = data["CheckedBy"];
						data["CheckedByStatus"] = "ForChecked";
						data["AuthorizedBy"] = null;
						data["AuthorizedByStatus"] = null;
						data["IsApproved"] = false;
						data["RequiredPosting"] = true;
						
					}
				}
				data["EntryDate"] = System.DateTime.Now.ToString();
				data["InvoiceDate"] = null;
				data["GRNType"] = "GRNBYPO";
				data["ByWhomEmployeeId"] = data["EmpCode"];
				R.SaveReceiptTransformation(data);
				return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
				
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		[HttpPost]
		public JsonResult SaveReceiptTransChildTab(IEnumerable<JobWorkReceiptTransformationChild> ReceiptTransChildData, string MasterId)
		{
			try
			{
				R.SaveReceiptTransChildTab(ReceiptTransChildData, MasterId);
				return Json(new { Error = false, Data = ReceiptTransChildData, Message = AplosMessage.Updated });

			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetGradeQuantityList()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				return Json(R.GetGradeWiseQuantityList(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[Authorize, HttpGet]
		public JsonResult GetTransGradeQuantityList(string MasterId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				return Json(R.GetTransGradeQuantityList(MasterId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[HttpPost]
		public JsonResult SaveGradeWiseTrans(IEnumerable<ReceiptTransformationGradeWise> TransGradeWiseData, string MasterId)
		{
			try
			{
				R.SaveGradeWiseTrans(TransGradeWiseData, MasterId);
				return Json(new { Error = false, Data = TransGradeWiseData, Message = AplosMessage.Updated });

			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetReceiptTransChildDatabyId(string Id)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetReceiptTransChildDatabyId(Id), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		//  BY PRODUCT TAB RECEIPT

		[Authorize, HttpGet]
		public JsonResult GetByProductApplicableList(string Id)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetByProductApplicableList(Id), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpPost]
		public JsonResult SaveByProduct(IEnumerable<JobWorkReceiptTransformationByProduct> ByProductData, string MasterId)
		{
			try
			{
				R.SaveByProduct(ByProductData, MasterId);
				return Json(new { Error = false, Data = ByProductData, Message = AplosMessage.Updated });

			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		// 

		// REPORT FOR TRANSFORMATION Receipt

		#region Reports for Transformation

		private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
		{
			sheet.Range[row, col].Text = txt;
			sheet.Range[row, col].ColumnWidth = width;
			sheet.Range[row, col].CellStyle.Font.Bold = true;
			sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet.Range[row, col].HorizontalAlignment = al;

		}

		[HttpGet, Authorize]
		public ActionResult GetTransformationPrintReport(ReportFormat reportFormat, string PrintTabId, string IssueId)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var reportFileName = " Transformation Job Work Material Receipt Chalaan " + PrintTabId + "";
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

			sheet.Name = "TransformationContractReceiptChalaan";


			int ROW = 6;
			int endCol = 1;
			int COL = 1;


			DataTable data = R.GetTransformationContractReportDataById(PrintTabId, IssueId);
			DataTable TransformationIssueReturnChilddata = R.GetTransformationIssueReturnChildDataById(PrintTabId, IssueId);
			DataTable TransformationByProductData = R.GetTransformationByProductDataById(PrintTabId, IssueId);
			DataTable TransformationWIPData = R.GetTransformationWIPData(PrintTabId,IssueId);
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
				SetHeaderTextTop(ref sheet, ROW, ColIssueIdEnd, "Receipt Id", 20, ExcelHAlign.HAlignLeft);
				ColIssueIdEnd++;
				int ColVAProcessEndDate = ColIssueIdEnd;
				int ColVAProcessEndDateEnd = ColIssueIdEnd + 1;
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["ReceiptId"].ToString();
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				//  ROW++;
				ColVAProcessEndDateEnd++;


				SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "GRN Date", 20, ExcelHAlign.HAlignLeft);
				ColVAProcessEndDateEnd++;
				int ColIssueDate = ColVAProcessEndDateEnd;
				int ColIssueDateEnd = ColVAProcessEndDateEnd + 1;
				sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Text = data.Rows[0]["JWGRNDate"].ToString();
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
				SetHeaderTextTop(ref sheet, ROW, ColIssuebyEnd, "Receipt By", 20, ExcelHAlign.HAlignLeft);
				ColIssuebyEnd++;
				int ColIssueby = ColIssuebyEnd;
				int ColIssueByEnd = ColIssuebyEnd + 1;
				sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Text = data.Rows[0]["EmployeeName"].ToString();
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
				SetHeaderTextTop(ref sheet, ROW, ColIR, "Document Reference No", 15, ExcelHAlign.HAlignLeft);
				ColIR++;
				int ColIssueReturn = ColIR;
				int ColIssueReturnEnd = ColIR + 1;
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Text = data.Rows[0]["DocRefNo"].ToString();
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Merge();
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				//  ROW++;
				ColIssueReturnEnd++;



				SetHeaderTextTop(ref sheet, ROW, ColIssueReturnEnd, "Document Date", 20, ExcelHAlign.HAlignLeft);
				ColIssueReturnEnd++;
				int ColJobWorkLocation = ColIssueReturnEnd;
				int ColJobWorkLocationEnd = ColIssueReturnEnd + 1;
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Text = data.Rows[0]["ReceiveDocumentDate"].ToString();
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Merge();
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ROW++;

				int ColRemarks = 1;
				SetHeaderTextTop(ref sheet, ROW, ColRemarks, "Remarks", 20, ExcelHAlign.HAlignLeft);
				ColRemarks++;
				int ColContractRemarks = ColRemarks;
				int ColContractRemarksEnd = ColRemarks + 1;
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Text = data.Rows[0]["InvoiceNo"].ToString();
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Merge();
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ColContractRemarksEnd++;

				int ColContractIsseStatus = ColContractRemarksEnd + 4;
				SetHeaderTextTop(ref sheet, ROW, ColContractIsseStatus, "Invoice No", 20, ExcelHAlign.HAlignLeft);
				ColContractIsseStatus++;
				int ColIssueStatus = ColContractIsseStatus;
				int ColIssueStatusEnd = ColContractIsseStatus + 1;
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Text = data.Rows[0]["InvoiceNo"].ToString();
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Merge();
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ColIssueStatusEnd++;

				int ColInvoiceNo = ColIssueStatusEnd;
				SetHeaderTextTop(ref sheet, ROW, ColInvoiceNo, "Invoice Date", 20, ExcelHAlign.HAlignLeft);
				ColInvoiceNo++;
				int ColColInvoice = ColInvoiceNo;
				int ColColInvoiceEnd = ColInvoiceNo + 1;
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].Text = data.Rows[0]["ReceiveInvoiceDate"].ToString();
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].Merge();
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ROW++;

				int ColGateEntryNoId = 8;
				SetHeaderTextTop(ref sheet, ROW, ColGateEntryNoId, "Gate Entry No", 20, ExcelHAlign.HAlignLeft);
				ColGateEntryNoId++;
				int ColGateEntryNo = ColGateEntryNoId;
				int ColGateEntryNoEnd = ColGateEntryNoId + 1;
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].Text = data.Rows[0]["GateEntryNo"].ToString();
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].Merge();
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ROW++;

			}

			//       Issue/ Return Child data

			int MPChildROW = ROW + 1;
			int MPChildendCol = 1;
			int MPChildCOL = 1;

			#region Material Planning Child Headers

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Received Quantity", 12, ExcelHAlign.HAlignLeft);
			MPChildROW++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
			int ColJWId = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
			int ColJWOutputItem = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Activity", 12, ExcelHAlign.HAlignLeft);
			int ColJWInputItem = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material", 12, ExcelHAlign.HAlignLeft);
			int ColJWOMMaterial = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Article", 12, ExcelHAlign.HAlignLeft);
			int ColJWArticle = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Plan Quantity", 12, ExcelHAlign.HAlignLeft);
			int ColRequiredQuantity = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Total Received Quantity", 15, ExcelHAlign.HAlignLeft);
			int ColBalanceToIssue = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "To Receive", 12, ExcelHAlign.HAlignLeft);
			int ColTIRCTotalQty = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Received Quantity", 10, ExcelHAlign.HAlignLeft);
			int ColTIRCQty = MPChildCOL;
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

				sheet[MPChildROW, ColJWId].Text = TransformationIssueReturnChilddata.Rows[i]["Id"].ToString();
				sheet[MPChildROW, ColJWOutputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();
				sheet[MPChildROW, ColJWInputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JobWorkActivity"].ToString();
				sheet[MPChildROW, ColJWOMMaterial].Text = TransformationIssueReturnChilddata.Rows[i]["Material"].ToString();
				sheet[MPChildROW, ColJWArticle].Text = TransformationIssueReturnChilddata.Rows[i]["Article"].ToString();
				sheet[MPChildROW, ColBalanceToIssue].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TotalReceivedQty"].ToString());
				sheet[MPChildROW, ColRequiredQuantity].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["PlanQuantity"].ToString());
				sheet[MPChildROW, ColTIRCTotalQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["ToReceive"].ToString());
				sheet[MPChildROW, ColTIRCQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TransactionQty"].ToString());

				sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
				JWOutputItem = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();

				MPChildROW++;
			}

			EndRows = MPChildROW - 1;

			if (RowIndexNo < MPChildROW - 1)
			{
				//sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
				sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			}

			//  WIP QUANTITY

			int WIPROW = MPChildROW + 1;
			int WIPendCol = 1;
			int WIPCOL = 1;

			#region By Product Headers

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "WIP Quantity", 12, ExcelHAlign.HAlignLeft);
			WIPROW++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
			int ColJobWorkTransformationContractChildMasterId = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
			int ColWIPJWOutputItem = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Item Id", 12, ExcelHAlign.HAlignLeft);
			int ColWIPId = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Item", 12, ExcelHAlign.HAlignLeft);
			int ColWIPJWInputItem = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Material", 12, ExcelHAlign.HAlignLeft);
			int ColWIPJWJWInputMaterial = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Article", 12, ExcelHAlign.HAlignLeft);
			int ColWIPJWInputArticle = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Total Planned Quantity", 12, ExcelHAlign.HAlignLeft);
			int ColWIPRequiredQuantity = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Total Issue/ Return Qty", 15, ExcelHAlign.HAlignLeft);
			int ColWIPTIRCTotalQty = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Quantity Used", 12, ExcelHAlign.HAlignLeft);
			int ColWIPQuantityUsed = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Total Quantity Used", 12, ExcelHAlign.HAlignLeft);
			int ColWIPTotalQuantityUsed = WIPCOL;
			WIPCOL++;

			report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "WIP Quantity", 10, ExcelHAlign.HAlignLeft);
			int ColWIPQuantity = WIPCOL;
			WIPROW++;
			WIPendCol = WIPCOL;
			#endregion Headers

			string WIPJWOutputId = "";
			var WIPStartRows = 0;
			var WIPEndRows = 0;
			int WIPRowIndexNo = WIPROW;
			WIPStartRows = WIPROW;

			for (int i = 0; i < TransformationWIPData.Rows.Count; i++)
			{

				if (WIPJWOutputId != TransformationWIPData.Rows[i]["OSTransformationPODetailId"].ToString())
				{

					if (WIPRowIndexNo < WIPROW)
					{
						//sheet.Range[WIPRowIndexNo, ColJobWorkItem, WIPROW - 1, ColJobWorkItem].Merge();
						sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					}
					WIPRowIndexNo = WIPROW;
				}

				sheet[WIPROW, ColJobWorkTransformationContractChildMasterId].Text = TransformationWIPData.Rows[i]["OSTransformationPODetailId"].ToString();
				sheet[WIPROW, ColWIPJWOutputItem].Text = TransformationWIPData.Rows[i]["JWOutputItem"].ToString();
				sheet[WIPROW, ColWIPId].Text = TransformationWIPData.Rows[i]["Id"].ToString();
				sheet[WIPROW, ColWIPJWInputItem].Text = TransformationWIPData.Rows[i]["JWInputItem"].ToString();
				sheet[WIPROW, ColWIPJWJWInputMaterial].Text = TransformationWIPData.Rows[i]["JWInputMaterial"].ToString();
				sheet[WIPROW, ColWIPJWInputArticle].Text = TransformationWIPData.Rows[i]["JWInputArticle"].ToString();
				sheet[WIPROW, ColWIPRequiredQuantity].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["RequiredQuantity"].ToString());
				sheet[WIPROW, ColWIPTIRCTotalQty].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["TIRCTotalQty"].ToString());
				sheet[WIPROW, ColWIPQuantityUsed].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["QuantityUsed"].ToString());
				sheet[WIPROW, ColWIPTotalQuantityUsed].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["TotalQuantityUsed"].ToString());
				sheet[WIPROW, ColWIPQuantity].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["WIPQuantity"].ToString());

				sheet.Range[WIPROW, 1, WIPROW, WIPendCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[WIPROW, 1, WIPROW, WIPendCol].BorderAround(ExcelLineStyle.Hair);
				WIPJWOutputId = TransformationWIPData.Rows[i]["OSTransformationPODetailId"].ToString();

				WIPROW++;
			}

			WIPEndRows = WIPROW - 1;

			if (WIPRowIndexNo < WIPROW - 1)
			{
				//sheet.Range[WIPRowIndexNo, ColJobWorkItem, WIPROW - 1, ColJobWorkItem].Merge();
				sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			}



			// By Product

			// By Product data

			int MPBPChildROW = WIPROW + 1;
			int MPBPChildendCol = 1;
			int BPChildCOL = 1;

			#region By Product Headers

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Quantity", 12, ExcelHAlign.HAlignLeft);
			MPBPChildROW++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "JW By Product Id", 12, ExcelHAlign.HAlignLeft);
			int ColJWBPId = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
			int ColJWBpOutItem = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Item", 12, ExcelHAlign.HAlignLeft);
			int ColJWBPByProductItem = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Material", 12, ExcelHAlign.HAlignLeft);
			int ColJWByProductMaterial = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Article", 12, ExcelHAlign.HAlignLeft);
			int ColJWByProductArticle = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "Total Required Quantity", 12, ExcelHAlign.HAlignLeft);
			int ColTotalReqQty = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "Total Received Quantity", 15, ExcelHAlign.HAlignLeft);
			int ColTotalReceivedQty = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "To Receive", 12, ExcelHAlign.HAlignLeft);
			int ColToReceive = BPChildCOL;
			BPChildCOL++;

			report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "Received Quantity", 10, ExcelHAlign.HAlignLeft);
			int ColReceivedQuantity = BPChildCOL;
			MPBPChildROW++;
			MPBPChildendCol = BPChildCOL;
			#endregion Headers

			string JWBPOutputItem = "";
			var BPStartRows = 0;
			var BPEndRows = 0;
			int BPRowIndexNo = MPBPChildROW;
			BPStartRows = MPBPChildROW;

			for (int i = 0; i < TransformationByProductData.Rows.Count; i++)
			{

				if (JWBPOutputItem != TransformationByProductData.Rows[i]["JWOutputItem"].ToString())
				{

					if (BPRowIndexNo < MPBPChildROW)
					{
						//sheet.Range[BPRowIndexNo, ColJobWorkItem, MPBPChildROW - 1, ColJobWorkItem].Merge();
						sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					}
					BPRowIndexNo = MPBPChildROW;
				}

				sheet[MPBPChildROW, ColJWBPId].Text = TransformationByProductData.Rows[i]["Id"].ToString();
				sheet[MPBPChildROW, ColJWBpOutItem].Text = TransformationByProductData.Rows[i]["JWOutputItem"].ToString();
				sheet[MPBPChildROW, ColJWBPByProductItem].Text = TransformationByProductData.Rows[i]["ByProductItem"].ToString();
				sheet[MPBPChildROW, ColJWByProductMaterial].Text = TransformationByProductData.Rows[i]["ByProductMaterial"].ToString();
				sheet[MPBPChildROW, ColJWByProductArticle].Text = TransformationByProductData.Rows[i]["ByProductArticle"].ToString();
				sheet[MPBPChildROW, ColTotalReqQty].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["TotalReqQty"].ToString());
				sheet[MPBPChildROW, ColTotalReceivedQty].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["TotalReceivedQty"].ToString());
				sheet[MPBPChildROW, ColToReceive].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["ToReceive"].ToString());
				sheet[MPBPChildROW, ColReceivedQuantity].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["TransactionQty"].ToString());

				sheet.Range[MPBPChildROW, 1, MPBPChildROW, MPBPChildendCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[MPBPChildROW, 1, MPBPChildROW, MPBPChildendCol].BorderAround(ExcelLineStyle.Hair);
				JWBPOutputItem = TransformationByProductData.Rows[i]["JWOutputItem"].ToString();

				MPBPChildROW++;
			}

			BPEndRows = MPBPChildROW - 1;

			if (BPRowIndexNo < MPBPChildROW - 1)
			{
				//sheet.Range[BPRowIndexNo, ColJobWorkItem, MPBPChildROW - 1, ColJobWorkItem].Merge();
				sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			}

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			sheet.UsedRange.NumberFormat = "#,##0.000";
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.CellStyle.Font.Size = 8;
			report.CompanyPlantHeader(ref sheet, MPChildendCol + 6, "Transformation Job Work Material Receipt Chalaan", identity.CompanyId, identity.PlantName, null);
			report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
			return workbook;
		}

		#endregion end Reports for Transformation Contract

		// JW Value Added Receipt Report

		#region Reports for Value Added

		[HttpGet, Authorize]
		public ActionResult GetValueAddedPrintReceiptReport(ReportFormat reportFormat, string PrintTabId, string IssueId)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var reportFileName = " ValueAdded Job Work Material Receipt Chalaan " + PrintTabId + "";
			var workbook = GetValueAddedContractReportWorkSheet(PrintTabId, IssueId);
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

		private IWorkbook GetValueAddedContractReportWorkSheet(string PrintTabId, string IssueId)
		{

			var excelEngine = new ExcelEngine();
			var report = new ReportUtility();
			var workbook = report.GetWorkbook(ref excelEngine, 3);
			workbook.Version = ExcelVersion.Excel2016;

			var sheet = workbook.Worksheets[0];

			sheet.Name = "ValueAddedContractReceiptChalaan";


			int ROW = 6;
			int endCol = 1;
			int COL = 1;


			DataTable data = R.GetValueAddedContractReportDataById(PrintTabId, IssueId);
			DataTable TransformationIssueReturnChilddata = R.GetValueAddedIssueReturnChildDataById(PrintTabId, IssueId);
			//DataTable TransformationByProductData = R.GetValueAddedByProductDataById(PrintTabId, IssueId);
			//DataTable TransformationWIPData = R.GetValueAddedWIPData(PrintTabId, IssueId);
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
				SetHeaderTextTop(ref sheet, ROW, ColIssueIdEnd, "Receipt Id", 20, ExcelHAlign.HAlignLeft);
				ColIssueIdEnd++;
				int ColVAProcessEndDate = ColIssueIdEnd;
				int ColVAProcessEndDateEnd = ColIssueIdEnd + 1;
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["ReceiptId"].ToString();
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				//  ROW++;
				ColVAProcessEndDateEnd++;


				SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "GRN Date", 20, ExcelHAlign.HAlignLeft);
				ColVAProcessEndDateEnd++;
				int ColIssueDate = ColVAProcessEndDateEnd;
				int ColIssueDateEnd = ColVAProcessEndDateEnd + 1;
				sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Text = data.Rows[0]["JWGRNDate"].ToString();
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
				SetHeaderTextTop(ref sheet, ROW, ColIssuebyEnd, "Receipt By", 20, ExcelHAlign.HAlignLeft);
				ColIssuebyEnd++;
				int ColIssueby = ColIssuebyEnd;
				int ColIssueByEnd = ColIssuebyEnd + 1;
				sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Text = data.Rows[0]["EmployeeName"].ToString();
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
				SetHeaderTextTop(ref sheet, ROW, ColIR, "Document Reference No", 15, ExcelHAlign.HAlignLeft);
				ColIR++;
				int ColIssueReturn = ColIR;
				int ColIssueReturnEnd = ColIR + 1;
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Text = data.Rows[0]["DocRefNo"].ToString();
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Merge();
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				//  ROW++;
				ColIssueReturnEnd++;



				SetHeaderTextTop(ref sheet, ROW, ColIssueReturnEnd, "Document Date", 20, ExcelHAlign.HAlignLeft);
				ColIssueReturnEnd++;
				int ColJobWorkLocation = ColIssueReturnEnd;
				int ColJobWorkLocationEnd = ColIssueReturnEnd + 1;
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Text = data.Rows[0]["ReceiveDocumentDate"].ToString();
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Merge();
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ROW++;

				int ColRemarks = 1;
				SetHeaderTextTop(ref sheet, ROW, ColRemarks, "Remarks", 20, ExcelHAlign.HAlignLeft);
				ColRemarks++;
				int ColContractRemarks = ColRemarks;
				int ColContractRemarksEnd = ColRemarks + 1;
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Text = data.Rows[0]["InvoiceNo"].ToString();
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Merge();
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ColContractRemarksEnd++;

				int ColContractIsseStatus = ColContractRemarksEnd + 4;
				SetHeaderTextTop(ref sheet, ROW, ColContractIsseStatus, "Invoice No", 20, ExcelHAlign.HAlignLeft);
				ColContractIsseStatus++;
				int ColIssueStatus = ColContractIsseStatus;
				int ColIssueStatusEnd = ColContractIsseStatus + 1;
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Text = data.Rows[0]["InvoiceNo"].ToString();
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Merge();
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ColIssueStatusEnd++;

				int ColInvoiceNo = ColIssueStatusEnd;
				SetHeaderTextTop(ref sheet, ROW, ColInvoiceNo, "Invoice Date", 20, ExcelHAlign.HAlignLeft);
				ColInvoiceNo++;
				int ColColInvoice = ColInvoiceNo;
				int ColColInvoiceEnd = ColInvoiceNo + 1;
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].Text = data.Rows[0]["ReceiveInvoiceDate"].ToString();
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].Merge();
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColColInvoice, ROW, ColColInvoiceEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ROW++;

				int ColGateEntryNoId = 8;
				SetHeaderTextTop(ref sheet, ROW, ColGateEntryNoId, "Gate Entry No", 20, ExcelHAlign.HAlignLeft);
				ColGateEntryNoId++;
				int ColGateEntryNo = ColGateEntryNoId;
				int ColGateEntryNoEnd = ColGateEntryNoId + 1;
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].Text = data.Rows[0]["GateEntryNo"].ToString();
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].Merge();
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[ROW, ColGateEntryNo, ROW, ColGateEntryNoEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
				ROW++;

			}

			//       Issue/ Return Child data

			int MPChildROW = ROW + 1;
			int MPChildendCol = 1;
			int MPChildCOL = 1;

			#region Material Planning Child Headers

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Received Quantity", 12, ExcelHAlign.HAlignLeft);
			MPChildROW++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
			int ColJWId = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
			int ColJWOutputItem = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Activity", 12, ExcelHAlign.HAlignLeft);
			int ColJWInputItem = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material", 12, ExcelHAlign.HAlignLeft);
			int ColJWOMMaterial = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Article", 12, ExcelHAlign.HAlignLeft);
			int ColJWArticle = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Plan Quantity", 12, ExcelHAlign.HAlignLeft);
			int ColRequiredQuantity = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Total Received Quantity", 15, ExcelHAlign.HAlignLeft);
			int ColBalanceToIssue = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "To Receive", 12, ExcelHAlign.HAlignLeft);
			int ColTIRCTotalQty = MPChildCOL;
			MPChildCOL++;

			report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Received Quantity", 10, ExcelHAlign.HAlignLeft);
			int ColTIRCQty = MPChildCOL;
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

				sheet[MPChildROW, ColJWId].Text = TransformationIssueReturnChilddata.Rows[i]["Id"].ToString();
				sheet[MPChildROW, ColJWOutputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();
				sheet[MPChildROW, ColJWInputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JobWorkActivity"].ToString();
				sheet[MPChildROW, ColJWOMMaterial].Text = TransformationIssueReturnChilddata.Rows[i]["Material"].ToString();
				sheet[MPChildROW, ColJWArticle].Text = TransformationIssueReturnChilddata.Rows[i]["Article"].ToString();
				sheet[MPChildROW, ColBalanceToIssue].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TotalReceivedQty"].ToString());
				sheet[MPChildROW, ColRequiredQuantity].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["PlanQuantity"].ToString());
				sheet[MPChildROW, ColTIRCTotalQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["ToReceive"].ToString());
				sheet[MPChildROW, ColTIRCQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TransactionQty"].ToString());

				sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
				JWOutputItem = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();

				MPChildROW++;
			}

			EndRows = MPChildROW - 1;

			if (RowIndexNo < MPChildROW - 1)
			{
				//sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
				sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			}

			//  WIP QUANTITY

			//int WIPROW = MPChildROW + 1;
			//int WIPendCol = 1;
			//int WIPCOL = 1;

			//#region By Product Headers

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "WIP Quantity", 12, ExcelHAlign.HAlignLeft);
			//WIPROW++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
			//int ColJobWorkTransformationContractChildMasterId = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPJWOutputItem = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Item Id", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPId = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Item", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPJWInputItem = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Material", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPJWJWInputMaterial = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "JW Input Article", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPJWInputArticle = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Total Planned Quantity", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPRequiredQuantity = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Total Issue/ Return Qty", 15, ExcelHAlign.HAlignLeft);
			//int ColWIPTIRCTotalQty = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Quantity Used", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPQuantityUsed = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "Total Quantity Used", 12, ExcelHAlign.HAlignLeft);
			//int ColWIPTotalQuantityUsed = WIPCOL;
			//WIPCOL++;

			//report.SetHeaderText(ref sheet, WIPROW, WIPCOL, "WIP Quantity", 10, ExcelHAlign.HAlignLeft);
			//int ColWIPQuantity = WIPCOL;
			//WIPROW++;
			//WIPendCol = WIPCOL;
			//#endregion Headers

			//string WIPJWOutputId = "";
			//var WIPStartRows = 0;
			//var WIPEndRows = 0;
			//int WIPRowIndexNo = WIPROW;
			//WIPStartRows = WIPROW;

			//for (int i = 0; i < TransformationWIPData.Rows.Count; i++)
			//{

			//	if (WIPJWOutputId != TransformationWIPData.Rows[i]["OSTransformationPODetailId"].ToString())
			//	{

			//		if (WIPRowIndexNo < WIPROW)
			//		{
			//			//sheet.Range[WIPRowIndexNo, ColJobWorkItem, WIPROW - 1, ColJobWorkItem].Merge();
			//			sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//			sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//		}
			//		WIPRowIndexNo = WIPROW;
			//	}

			//	sheet[WIPROW, ColJobWorkTransformationContractChildMasterId].Text = TransformationWIPData.Rows[i]["OSTransformationPODetailId"].ToString();
			//	sheet[WIPROW, ColWIPJWOutputItem].Text = TransformationWIPData.Rows[i]["JWOutputItem"].ToString();
			//	sheet[WIPROW, ColWIPId].Text = TransformationWIPData.Rows[i]["Id"].ToString();
			//	sheet[WIPROW, ColWIPJWInputItem].Text = TransformationWIPData.Rows[i]["JWInputItem"].ToString();
			//	sheet[WIPROW, ColWIPJWJWInputMaterial].Text = TransformationWIPData.Rows[i]["JWInputMaterial"].ToString();
			//	sheet[WIPROW, ColWIPJWInputArticle].Text = TransformationWIPData.Rows[i]["JWInputArticle"].ToString();
			//	sheet[WIPROW, ColWIPRequiredQuantity].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["RequiredQuantity"].ToString());
			//	sheet[WIPROW, ColWIPTIRCTotalQty].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["TIRCTotalQty"].ToString());
			//	sheet[WIPROW, ColWIPQuantityUsed].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["QuantityUsed"].ToString());
			//	sheet[WIPROW, ColWIPTotalQuantityUsed].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["TotalQuantityUsed"].ToString());
			//	sheet[WIPROW, ColWIPQuantity].Number = clsStaticInfo.dbl(TransformationWIPData.Rows[i]["WIPQuantity"].ToString());

			//	sheet.Range[WIPROW, 1, WIPROW, WIPendCol].BorderInside(ExcelLineStyle.Hair);
			//	sheet.Range[WIPROW, 1, WIPROW, WIPendCol].BorderAround(ExcelLineStyle.Hair);
			//	WIPJWOutputId = TransformationWIPData.Rows[i]["OSTransformationPODetailId"].ToString();

			//	WIPROW++;
			//}

			//WIPEndRows = WIPROW - 1;

			//if (WIPRowIndexNo < WIPROW - 1)
			//{
			//	//sheet.Range[WIPRowIndexNo, ColJobWorkItem, WIPROW - 1, ColJobWorkItem].Merge();
			//	sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//	sheet.Range[WIPRowIndexNo, ColJobWorkTransformationContractChildMasterId, WIPROW - 1, ColJobWorkTransformationContractChildMasterId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//}



			//// By Product

			//// By Product data

			//int MPBPChildROW = WIPROW + 1;
			//int MPBPChildendCol = 1;
			//int BPChildCOL = 1;

			//#region By Product Headers

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Quantity", 12, ExcelHAlign.HAlignLeft);
			//MPBPChildROW++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "JW By Product Id", 12, ExcelHAlign.HAlignLeft);
			//int ColJWBPId = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
			//int ColJWBpOutItem = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Item", 12, ExcelHAlign.HAlignLeft);
			//int ColJWBPByProductItem = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Material", 12, ExcelHAlign.HAlignLeft);
			//int ColJWByProductMaterial = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "By Product Article", 12, ExcelHAlign.HAlignLeft);
			//int ColJWByProductArticle = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "Total Required Quantity", 12, ExcelHAlign.HAlignLeft);
			//int ColTotalReqQty = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "Total Received Quantity", 15, ExcelHAlign.HAlignLeft);
			//int ColTotalReceivedQty = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "To Receive", 12, ExcelHAlign.HAlignLeft);
			//int ColToReceive = BPChildCOL;
			//BPChildCOL++;

			//report.SetHeaderText(ref sheet, MPBPChildROW, BPChildCOL, "Received Quantity", 10, ExcelHAlign.HAlignLeft);
			//int ColReceivedQuantity = BPChildCOL;
			//MPBPChildROW++;
			//MPBPChildendCol = BPChildCOL;
			//#endregion Headers

			//string JWBPOutputItem = "";
			//var BPStartRows = 0;
			//var BPEndRows = 0;
			//int BPRowIndexNo = MPBPChildROW;
			//BPStartRows = MPBPChildROW;

			//for (int i = 0; i < TransformationByProductData.Rows.Count; i++)
			//{

			//	if (JWBPOutputItem != TransformationByProductData.Rows[i]["JWOutputItem"].ToString())
			//	{

			//		if (BPRowIndexNo < MPBPChildROW)
			//		{
			//			//sheet.Range[BPRowIndexNo, ColJobWorkItem, MPBPChildROW - 1, ColJobWorkItem].Merge();
			//			sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//			sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//		}
			//		BPRowIndexNo = MPBPChildROW;
			//	}

			//	sheet[MPBPChildROW, ColJWBPId].Text = TransformationByProductData.Rows[i]["Id"].ToString();
			//	sheet[MPBPChildROW, ColJWBpOutItem].Text = TransformationByProductData.Rows[i]["JWOutputItem"].ToString();
			//	sheet[MPBPChildROW, ColJWBPByProductItem].Text = TransformationByProductData.Rows[i]["ByProductItem"].ToString();
			//	sheet[MPBPChildROW, ColJWByProductMaterial].Text = TransformationByProductData.Rows[i]["ByProductMaterial"].ToString();
			//	sheet[MPBPChildROW, ColJWByProductArticle].Text = TransformationByProductData.Rows[i]["ByProductArticle"].ToString();
			//	sheet[MPBPChildROW, ColTotalReqQty].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["TotalReqQty"].ToString());
			//	sheet[MPBPChildROW, ColTotalReceivedQty].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["TotalReceivedQty"].ToString());
			//	sheet[MPBPChildROW, ColToReceive].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["ToReceive"].ToString());
			//	sheet[MPBPChildROW, ColReceivedQuantity].Number = clsStaticInfo.dbl(TransformationByProductData.Rows[i]["TransactionQty"].ToString());

			//	sheet.Range[MPBPChildROW, 1, MPBPChildROW, MPBPChildendCol].BorderInside(ExcelLineStyle.Hair);
			//	sheet.Range[MPBPChildROW, 1, MPBPChildROW, MPBPChildendCol].BorderAround(ExcelLineStyle.Hair);
			//	JWBPOutputItem = TransformationByProductData.Rows[i]["JWOutputItem"].ToString();

			//	MPBPChildROW++;
			//}

			//BPEndRows = MPBPChildROW - 1;

			//if (BPRowIndexNo < MPBPChildROW - 1)
			//{
			//	//sheet.Range[BPRowIndexNo, ColJobWorkItem, MPBPChildROW - 1, ColJobWorkItem].Merge();
			//	sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//	sheet.Range[BPRowIndexNo, ColJWOutputItem, MPBPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//}

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			sheet.UsedRange.NumberFormat = "#,##0.000";
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.CellStyle.Font.Size = 8;
			report.CompanyPlantHeader(ref sheet, MPChildendCol + 6, "Value Added Job Work Material Receipt Chalaan", identity.CompanyId, identity.PlantName, null);
			report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
			return workbook;
		}

		#endregion end Reports for Value Added Contract

		// GET Issued Material data

		[Authorize, HttpGet]
		public JsonResult GetIfIssuedOrNot(string JWOutputId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetIfIssuedOrNot(JWOutputId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetIssuedMatInputList(string JWPOId, string JWOutputId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetIssuedMatInputList(JWPOId, JWOutputId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetIfIssuedOrNotValAdded(string JWPOId, string JWOutputId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetIfIssuedOrNotValAdded(JWPOId, JWOutputId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetIssuedMatInputListValAdded(string JWPOId, string JWOutputId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(R.GetIssuedMatInputListValAdded(JWPOId, JWOutputId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		// Template Report

		[Authorize, HttpGet]
		public ActionResult GRNReport(string grnId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				R.GRNReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

				return null;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[Authorize, HttpGet]
		public ActionResult ValAddedGRNReport(string grnId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				R.ValAddedGRNReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

				return null;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpPost]
		public JsonResult SaveTransformationWOMaterial(Dictionary<string, object> data, string ContractId, string PartyId, IEnumerable<JobWorkTransformationReceiptWOMaterial> SelectedQtyDataWOMat, IEnumerable<JWTransformationReceiptWOMaterialByProduct> SelectedByProductQtyDataWOMat)
		{
			try
			{
				R.SaveTransformationWOMaterial(data, ContractId, PartyId, SelectedQtyDataWOMat, SelectedByProductQtyDataWOMat);
				return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}


	}
}