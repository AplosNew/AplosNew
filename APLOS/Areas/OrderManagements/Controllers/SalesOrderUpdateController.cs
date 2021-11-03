#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Service.OrderManagements;
using Library.Service.Parties;
using Library.Service.Productions;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Data.Sql;
using OTSBD;
using Library.Service.Helpers;
using System.Collections.Specialized;
using Library.Service.Enums;
using Aplos.Helpers;
using System.Web;
#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SalesOrderUpdateController : BaseController
    {
        #region -- Constructor

        private readonly string ExchangeRateTableName = "MasterOrderExchangeRates";

        private readonly IMasterOrderService _masterOrderService;
        private readonly IPartyService _partyService;
        private readonly ICustomerPOService _customerPOService;
        private readonly ISqlRepository _sqlRepository;
        public SalesOrderUpdateController(IMasterOrderService masterOrderService, IPartyService partyService, ICustomerPOService customerPOService, ISqlRepository R)
        {
            _masterOrderService = masterOrderService;
            _partyService = partyService;
            _customerPOService = customerPOService;
            _sqlRepository = R;
        }

        #endregion
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region -- Operations

        [HttpPost]
        public JsonResult UpdateSODate(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSODate(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UpdateSORate(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSORate(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UpdateSOQTY(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSOQTY(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UpdateSOStatus(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSOStatus(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

   [HttpGet,Authorize]
        public JsonResult GetSOBookedQtyAndLevel(string salesOrderId)
        {
            return Json(_masterOrderService.GetSOBookedQtyAndLevel(salesOrderId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPOBookedQtyAndLevel(string salesOrderId)
        {
            return Json(_masterOrderService.GetPOBookedQtyAndLevel(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Contract
        [Authorize, HttpGet]
        public ActionResult GetMasterOrderData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId,FORMAT( A.AddedDate,'dd-MMM_yyyy') CreationDate
                                    , a.AddedBy AS CreatedBy
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId,B.UserName Buyer
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId,OC.UserName AS OrderCategory, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' AND PlantId=A.PlantId)
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus,FORMAT( A.AddedDate,'dd-MMM_yyyy') AddedDate,A.AddedBy
                                      ,A.OwnReferenceNo,A.BuyerReferenceNo,A.PaymentTermId,A.PaymentTermDays,A.ExceptionalProcessId,A.ExceptionalSubProcessId
                                    ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                    ContractNo=STUFF((select distinct ','+CNT.ContractNo from dbo.Contract CNT
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.ContractId=CNT.Id	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									MasterLCNo=STUFF((select distinct ','+MLC.LCRef from dbo.Contract CNT
															INNER JOIN TRN.MasterOrderItem XMOI  ON XMOI.ContractId=CNT.Id
															LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            															
                            FROM [TRN].[MasterOrder] AS A
						
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=a.OrderCategoryId
                            LEFT JOIN HKP.Buyer B ON B.Id=A.BuyerId
                            WHERE A.CompanyId='" + identity.CompanyId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetSOData(string MasterOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT SO.Id,SO.ParentId
                            , SO.MasterOrderItemId
                            , MOI.MaterialMasterId
                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
                            , SO.DestinationId, D.UserName Destination
                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
                            , SO.ShipmentModeId
                            , SO.CustomerPOId
		                    , po.PONumber
                            ,MOI.TotalQty MOIQty
                            ,SO.DestinationDescription
                            , SO.OrderStatusId, SO.OrderCategoryId
                            , SO.SOType, SO.ResponsiblePersonId
                            , SO.UpCharge, SO.Qty, SO.Rate, SO.IsFirstEntry,SO.Discount,EMP.EmployeeName ResponsiblePersonName
                            ,FORMAT (SO.LSD, 'dd-MMM-yyyy') as LSD ,FORMAT (SO.MainRawMaterialInhouseDate, 'dd-MMM-yyyy') as MainRawMaterialInhouseDate
                            ,FORMAT (SO.OtherRawMaterialInhouseDate, 'dd-MMM-yyyy') as OtherRawMaterialInhouseDate
                            ,FORMAT (SO.PlanExFactoryDate, 'dd-MMM-yyyy') as PlanExFactoryDate
                            , hasFirst=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId=SO.Id)                            ,(SELECT ISNULL(sum(Qty),0) FROM TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId) SKUQty
                            , isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id)
                            ,ISNULL(POD.ProductionOrderId,'') ProductionOrderId,SO.Reason,SO.Description,SO.CM,SO.SalesOrderYear,SO.WeekNo
                            ,SO.ProductionBookedQty,SO.ProductionBookingLevel,SO.SalesExpense,C.Code As Currency
							
                    FROM [TRN].[SalesOrder] AS SO
                   -- LEFT JOIN TRN.FirstCharacteristics SKU ON SKU.SalesOrderId=SO.Id
                    JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
					left outer join TRN.MasterOrder MO on Mo.Id=MOI.MasterOrderId
					left outer join SCS.Currency C on C.Id=MO.CurrencyId
                    LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                    LEFT JOIN dbo.EmployeeInformation AS EMP ON EMP.SystemId = SO.ResponsiblePersonId
                    LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
                    LEFT JOIN [MST].[Destination] D ON D.Id=SO.DestinationId
                    WHERE  MOI.MasterOrderId='" + MasterOrderId + @"' ORDER BY SO.DeliveryDate";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion


    }
}