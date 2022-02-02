#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Web.Script.Serialization;
using System;
using Newtonsoft.Json;
using Library.Data;
using System.IO;
using Library.HumanResource.Attendance.Manual;
using Library.Service.Helpers;
using System.Data;
using Library.OrderManagement.FabricRollClass;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using OTSBD;
using Library.Service.HumanResources.Profile;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class FabricRollController : BaseController
    {
        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private SqlRepository _sqlRepository = new SqlRepository();
        public FabricRollController(IFabricRollMasterService fabricRollMasterService)
        {
            _fabricRollMasterService = fabricRollMasterService;
        }

        #endregion -- Constructor

        #region Pages

     
        public ActionResult Aplos()
        {
            return View();
        }

	
		public ActionResult Aplos1()
		{
			return View();
		}

		#endregion Pages

		#region -- Operations

		[HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string paidHours)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fabricRollMasterService.Query(parameters, identity.CompanyGroupId, paidHours, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFabricIncrementValue()
        {
            return Json(_fabricRollMasterService.InsertOrUpdateGraphIncrement(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(IEnumerable<FabricRollMaster> entities)
        {
            _fabricRollMasterService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }


		[HttpPost]
		public JsonResult Update(List<Dictionary<string, object>> FabricRollData, string PackingForm)
		{
			_fabricRollMasterService.UpdateFabricRoll(FabricRollData,PackingForm);
			return Json(new { Message = AplosMessage.Updated });
		}

	

		[HttpPost, Authorize]
		public JsonResult GetRoll( int NoofRolls,Dictionary<string, object> SelectedRow,double Width,string PackingForm)
		{
			_fabricRollMasterService.CreateRoll( NoofRolls, SelectedRow, Width, PackingForm);
			return Json(new { Message = AplosMessage.Insert });
		}

		public ActionResult Delete(string id)
		{
			try
			{
				if (string.IsNullOrEmpty(id))
					throw new Exception("Select entry first");
				ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
				con.BeginTransaction();
				con.executeQuery("delete from TRN.FabricRollMaster where id='" + id + "'");
				con.CommitTransaction();
				return Json(new { Error = false,/* Sequence = GetSequence(),*/ Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}
		[HttpGet, Authorize]
        public JsonResult GetGRNList(GridParameter parameters)
        {
            return Json(_fabricRollMasterService.GetGRNList(parameters, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetGRNDetailList(GridParameter parameters, string inventoryReceiveId)
        {
            return Json(_fabricRollMasterService.GetGRNDetailList(parameters, inventoryReceiveId, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFABRollList(GridParameter parameters, string inventoryReceiveDetailId)
        {
            return Json(_fabricRollMasterService.GetFABRollList(parameters, inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBarCideList(string inventoryReceiveDetailId)
        {
            return Json(_fabricRollMasterService.GetBarCideList(inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations




        [HttpPost, Authorize]
        public ActionResult GRNList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT IR.Id GRNNo
                                    ,IR.Status GRNStatus
                                    ,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,PO.PODate
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
									,PO.VendorRefNo,PO.PINo,PO.PurchaseLCNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    
									,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
							FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='20171' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='20171' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
					
						

                         LEFT JOIN(SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,VendorRefNo=STUFF((select distinct ','+xpo.DocRefNo  from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,PurchaseLCNo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,PINo=STUFF((select distinct ','+PLC.PINo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), xpo.PODate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate,PODate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
							LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId

							LEFT JOIN TRN.InventoryReceiveDetail IRD1 ON IR.Id=IRD1.InventoryReceiveId
							LEFT JOIN TRN.PurchaseOrder po1 on po1.id=IRD1.POId
							LEFT JOIN SCS.Currency C ON IR.CurrencyId=C.Id
							LEFT JOIN TRN.InventoryMaterial IM ON IRD1.InventoryMaterialId=IM.Id
							LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
							LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
                        WHERE BP.BusinessProcessName='FabricRollManagement'
					  and IR.GRNType in('GRNBYPO','GRN' ,'EMPGRN')) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

		[HttpPost, Authorize]
		public ActionResult MaterialList(string inventoryReceiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"SELECT 
DISTINCT IRD.Id,IRD.InventoryReceiveId,IRD.TransactionQty,IRD.TransactionUoMId,Isnull(FRM.SplitCount,0)SplitCount
,ISNULL(FRM.TotalDistributeQty,0)TotalDistributeQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,PL.FabRollPrefix,IM.PlantId,IM.MaterialMasterId,IM.ArticleId
,IM.FirstCharacteristicsId SKUId,MM.UserName MaterialMasterName,MMA.StandardName ArticleName
,C.UserName SKU1,C2.UserName SKU2,C3.UserName SKU3,CV.UserName SKUValue,CV2.UserName SKUValue2,CV3.UserName SKUValue3, C.UserName +':'+CV.UserName SKUInfo,CU.Code
,MGM.UserName MaterialGroup
FROM [TRN].[InventoryReceiveDetail] IRD
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
										--LEFT JOIN ORG.Plant PL ON IM.PlantId= PL.Id
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
										LEFT JOIN scs.PlantConfig PL ON  PL.PlantId=IM.PlantId
                                        LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
                                        LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id

                                        LEFT JOIN MST.MaterialGroupMaster MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id

                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN HKP.Characteristics C2 ON IM.SecondCharacteristicsId=C2.Id
                                        LEFT JOIN HKP.Characteristics C3 ON IM.ThirdCharacteristicsId=C3.Id

                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV2 ON IM.SecondCharacteristicsValueId=CV2.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV3 ON IM.ThirdCharacteristicsValueId=CV3.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
										LEFT JOIN (SELECT COUNT(Id) SplitCount,Sum(VendorQty) TotalDistributeQty
										,InventoryReceiveDetailId FROM TRN.FabricRollMaster 
										GROUP BY InventoryReceiveDetailId) FRM ON IRD.Id=FRM.InventoryReceiveDetailId
WHERE BP.BusinessProcessName='FabricRollManagement' AND IRD.InventoryReceiveId='" + inventoryReceiveId + @"'";

			            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

		}

		[HttpPost, Authorize]
		public ActionResult GetMaterialListData(string inventoryReceiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"SELECT 
DISTINCT IRD.Id,IRD.InventoryReceiveId,IRD.TransactionQty,IRD.TransactionUoMId,Isnull(FRM.SplitCount,0)SplitCount
,ISNULL(FRM.TotalDistributeQty,0)TotalDistributeQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,PL.FabRollPrefix,IM.PlantId,IM.MaterialMasterId,IM.ArticleId
,IM.FirstCharacteristicsId SKUId,MM.UserName MaterialMasterName,MMA.StandardName ArticleName
,C.UserName SKU1,C2.UserName SKU2,C3.UserName SKU3,CV.UserName SKUValue,CV2.UserName SKUValue2,CV3.UserName SKUValue3, C.UserName +':'+CV.UserName SKUInfo,CU.Code
,MGM.UserName MaterialGroup
FROM [TRN].[InventoryReceiveDetail] IRD
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
										--LEFT JOIN ORG.Plant PL ON IM.PlantId= PL.Id
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
										LEFT JOIN scs.PlantConfig PL ON  PL.PlantId=IM.PlantId
                                        LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
                                        LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id

                                        LEFT JOIN MST.MaterialGroupMaster MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id

                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN HKP.Characteristics C2 ON IM.SecondCharacteristicsId=C2.Id
                                        LEFT JOIN HKP.Characteristics C3 ON IM.ThirdCharacteristicsId=C3.Id

                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV2 ON IM.SecondCharacteristicsValueId=CV2.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV3 ON IM.ThirdCharacteristicsValueId=CV3.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
										LEFT JOIN (SELECT COUNT(Id) SplitCount,Sum(VendorQty) TotalDistributeQty
										,InventoryReceiveDetailId FROM TRN.FabricRollMaster 
										GROUP BY InventoryReceiveDetailId) FRM ON IRD.Id=FRM.InventoryReceiveDetailId
WHERE BP.BusinessProcessName='FabricRollManagement' AND IRD.InventoryReceiveId='" + inventoryReceiveId + @"'";

			return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

		}

		[HttpPost, Authorize]
		public ActionResult FabricRollList(string inventoryReceiveDetailId)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"select * from TRN.FabricRollMaster where InventoryReceiveDetailId='" + inventoryReceiveDetailId + @"'";

			return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

		}
		[HttpGet, Authorize]
		public ActionResult DownloadRollReport(string inventoryReceiveDetailId)
		{
			try
			{
				Library.OrderManagement.FabricRollClass.FabricRollClass RollReport = new Library.OrderManagement.FabricRollClass.FabricRollClass();
				RollReport.DownloadReport(inventoryReceiveDetailId);

				return null;
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}


		#region Upload Roll Data

		[HttpPost]
		public JsonResult CreateRollFile(FormCollection form)
		{
			var pre = form["FabricRollFile"];
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};
			var FabricRollFile = JsonConvert.DeserializeObject<FabricRollFile>(pre, settings);
			var file = Request.Files["file"];
			if (file != null)
			{
				var extension = Path.GetExtension(file.FileName);
				if (extension.ToLower() != ".xls" && extension.ToLower() != ".xlsx")
				{
					throw new CustomException(Resources.ImageUploadError);
				}


				FabricRollClass Clsss = new FabricRollClass();
				//clsManualAttendanceFileUpload p = new clsManualAttendanceFileUpload();
				Clsss.Save(file.FileName, extension, FabricRollFile, out DataSet dsMaster);
				var path = Path.Combine(ResourcesPathReader.GetFabricRollFilePath(), dsMaster.Tables[0].Rows[0]["FileId"].ToString());

				if (System.IO.File.Exists(path))
				{
					System.IO.File.Delete(path);
					file.SaveAs(path);
				}
				else
				{
					file.SaveAs(path);
				}
			}
			return Json(new { Message = AplosMessage.Success });
		}

		[HttpGet, Authorize]
		public ActionResult GetMaster()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				clsManualAttendanceFileUpload ep = new clsManualAttendanceFileUpload();
				return Json(ep.GetMaster(identity.PlantId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}
		#endregion
		public void SaveFile(out string path)
		{
			path = "";
			try
			{
				var file = Request.Files["file"];
				if (file != null)
				{
					var extension = Path.GetExtension(file.FileName);
					if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
					{
					}
					else
						throw new CustomException(Resources.ExcelUploadError);
				}
				if (file != null)
				{
					path = Path.Combine(ResourcesPathReader.GetFabricRollData(), file.FileName);
					if (System.IO.File.Exists(path))
					{
						System.IO.File.Delete(path);
						file.SaveAs(path);
					}
					else
					{
						file.SaveAs(path);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		#region SampleFile
		[HttpGet, Authorize]
		public ActionResult GetSampleFile(ReportFormat reportFormat,int rollNo)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			IWorkbook workbook = GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, rollNo);
			var reportFileName = "Fabric Roll Management Template";
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

        public IWorkbook GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, int rollNo)
        {
            #region declare
            clsReport objRpt = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
          
            int maxRow = 5001;

            #endregion
            try
            {
                //sorting
                //lock               

                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 6, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];
               


				#region ------------------Column Header------------------

				ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sequence");
				sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@"; xlsCol += 1;

				ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRNRowId");
              
                sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LotNo");
                sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;
             
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shade"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MarkarCode"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricGroup"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Length"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Weight"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shrinkage"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Qty"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "QtyUoM"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ActualQty"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "InvoiceQty"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierRollNo"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OwnRollNo"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BuyerRollNo"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Grouping"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks");
                 xlsCol += 1;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

				#endregion ------------------Column Header------------------
				int count =0;
                #region DataPlot
                for (int i = 0; i < rollNo; i++)
                {
					count++;
					xlsCol = 1;
					sheet1[xlsRow, xlsCol].Number = count;
					xlsRow++;
				}

				xlsRow++;

				#endregion

				#region UsedRange Alignment

				sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                //sheetSource.Protect("2020", ExcelSheetProtection.Content);


                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

		#endregion

		#region MyRegion

		[HttpPost, Authorize]
		public JsonResult ImportData()
		{
			string path;
			clsTemplateReadProfile objR = null;
			try
			{
				objR = new clsTemplateReadProfile();
				var file = Request.Files["file"];
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				SaveFiles(out path);
				var data = ReadData(identity.PlantId, path);
				JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
				json.MaxJsonLength = int.MaxValue;
				return json;
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}
		public void SaveFiles(out string path)
		{
			path = "";
			try
			{
				//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				//GetPlantwiseData(identity.PlantId);
				var file = Request.Files["file"];
				if (file != null)
				{
					var extension = Path.GetExtension(file.FileName);
					if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
					{
					}
					else
						throw new CustomException(Resources.ExcelUploadError);
				}
				if (file != null)
				{
					path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
					if (System.IO.File.Exists(path))
					{
						System.IO.File.Delete(path);
						file.SaveAs(path);
					}
					else
					{
						file.SaveAs(path);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		 public List<FabricRollTemplate> ReadData(string plantid, string path)
        {
            List<FabricRollTemplate> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<FabricRollTemplate>();
                //SaveFile(out path);
                ReadFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<FabricRollTemplate>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

		public void ReadFile(string path, out DataSet dsExcel)
		{
			FileInfo docFile;
			dsExcel = null;
			try
			{
				ExcelEngine excelEngine = null;
				IApplication application = null;
				IWorkbook workbook = null;
				excelEngine = new ExcelEngine();
				application = excelEngine.Excel;
				workbook = excelEngine.Excel.Workbooks.Open(path);
				//DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
				DataTable dt = workbook.Worksheets[0].ExportDataTable(6,1,5000,18, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(Sequence,'')<>''";
                dt = dt.DefaultView.ToTable();
                dsExcel = new DataSet();
				dsExcel.Tables.Add(dt);
				docFile = new FileInfo(path);
				if (docFile.Exists)
				{
					//exception += "\r\nTrying to delete";
					docFile.Delete();
				}
			}
			catch (Exception ex)
			{
				docFile = new FileInfo(path);
				if (docFile.Exists)
				{
					docFile.Delete();
				}
				throw (ex);
			}
		}

		public void Validation(DataSet dsExcel, string plantid)
		{
			
			try
			{
			
				if (dsExcel.Tables[0].Rows.Count > 0)
				{
					if (false)
					{
						for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
						{
							string strTempPDate = "";
							string strTempPTimee = "";
							string strTempPType = "";

							strTempPDate = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
							strTempPTimee = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
							strTempPType = dsExcel.Tables[0].Rows[i][3].ToString().Trim().ToUpper();

						}//for

					}

				}
				else
				{
					throw new Exception("Please Select File");
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		[HttpPost]
		public JsonResult CreateFabricRollManage(Dictionary<string, object> data, List<Dictionary<string, object>> WorkDayList)
		{
            SaveFabricRollManageData(data, WorkDayList);
			return Json(new { Data = data, Message = AplosMessage.Insert });
		}


		private void SaveFabricRollManageData(Dictionary<string, object> data, List<Dictionary<string, object>> grnDetailList)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			try
			{
				DataSet dsMaster, dsDetail;
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FabricRollManagementMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

				string _Id, _detailId = "";
				string masterId = "";

				if (dsMaster.Tables[0].Rows.Count == 0)
				{
					bplib.clsGenID genid = new bplib.clsGenID();
					genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchMaster", out _Id);

					data["Id"] =_Id;
					data["PlantId"] = identity.PlantId;
					AddNewRow(dsMaster.Tables[0], data);
				}
				else
				{
					_Id = data["Id"].ToString();
					data["PlantId"] = identity.PlantId;
					EditRow(dsMaster.Tables[0].Rows[0], data);
				}

				masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

				
				con.OpenDataSetThroughAdapter("SELECT * FROM BPDT.FabricRollManagementChild WHERE FabricRollManagementMasterId ='" + masterId + "'", out dsDetail, false, "1");

				int count = 0;
				foreach (var item in grnDetailList)
				{
					count++;
					DataView dv = new DataView(dsDetail.Tables[0]);
					dv.RowFilter = "Id='" + item["Id"] + "'";

					if (dv.Count == 0)
					{
						item["Id"] = masterId+"-"+count;
						item["FabricRollManagementMasterId"] = masterId;

						AddNewRow(dsDetail.Tables[0], item);
					}
					else
					{
						DataRow drmo = dv[0].Row;
						EditRow(drmo, item);
					}
				}


				clsStaticInfo obj = new clsStaticInfo();
				obj.SaveDataSets(dsMaster, dsDetail);

			}
			catch (Exception ex)
			{
				throw (ex);
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

	}


	public class FabricRollTemplate
	{
		
		public string Sequence { get; set; }
		public string GRNRowId { get; set; }
		public string LotNo { get; set; }
		public string Shade { get; set; }
		public string MarkarCode { get; set; }
		public string FabricGroup { get; set; }
		public string Length { get; set; }

		public string Weight { get; set; }
		public string Shrinkage { get; set; }
		public string Qty { get; set; }
		public string QtyUoM { get; set; }
		public string ActualQty { get; set; }
		public string InvoiceQty { get; set; }

		public string SupplierRollNo { get; set; }
		public string OwnRollNo { get; set; }
		public string BuyerRollNo { get; set; }
		public string Grouping { get; set; }
		public string Remarks { get; set; }

	}
}