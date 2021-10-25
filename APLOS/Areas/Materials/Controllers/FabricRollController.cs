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

        [Authorize]
        public ActionResult Aplos()
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
            string sql = @"select top 100 * from (SELECT              IR.Id GRNNo
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
		public JsonResult Create(FormCollection form)
		{
			var pre = form["FabricRollFile"];
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};
			var Manualfile = JsonConvert.DeserializeObject<ManualAttdnFile>(pre, settings);
			var file = Request.Files["file"];
			if (file != null)
			{
				var extension = Path.GetExtension(file.FileName);
				if (extension.ToLower() != ".xls" && extension.ToLower() != ".xlsx")
				{
					throw new CustomException(Resources.ImageUploadError);
				}


				clsManualAttendanceFileUpload p = new clsManualAttendanceFileUpload();
				p.Save(file.FileName, extension, Manualfile, out DataSet dsMaster);
				var path = Path.Combine(ResourcesPathReader.GetManualAttendanceFilePath(), dsMaster.Tables[0].Rows[0]["FileId"].ToString());

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



	}
}