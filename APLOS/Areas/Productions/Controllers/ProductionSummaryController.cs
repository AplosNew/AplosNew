#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Library.Model.Productions.ProductionBooking;
using Library.Data.Sql;
using Library.OrderManagement.Production;
using System;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionSummaryController : BaseController
    {
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        #region Constructor
        /// <summary>   The ProductionSummaryService service. </summary>
        private readonly IProductionSummaryService _ProductionSummaryService;

        public ProductionSummaryController(IProductionSummaryService ProductionSummaryService, ISqlRepository sqlRepository)
        {
            _ProductionSummaryService = ProductionSummaryService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult AplosSFG()
        {
            return View();
        }

        public ActionResult AplosInOut()
        {
            return View();
        }


        public ActionResult Reject()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueCbo(string soid)
        {
            return Json(_ProductionSummaryService.GetCharacteristicsValueCbo(soid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLotNumberCbo(string SalesOrderId, string ProductionOrderId, string ProcessId, string productionLevel)
        {
            var sql = "";
            if (productionLevel != "ProductionOrder")
            {
                sql = @"SELECT DISTINCT LotNumber [Value],LotNumber [Text] FROM TRN.ProductionSummary Where ISNULL(LotNumber,'')<>'' AND SalesOrderId='" + SalesOrderId + "' AND ProcessId='" + ProcessId + "'";
            }
            else
            {
                sql = @"SELECT DISTINCT LotNumber [Value],LotNumber [Text] FROM TRN.ProductionSummary Where ISNULL(LotNumber,'')<>'' AND ProductionOrderId='" + ProductionOrderId + "' AND ProcessId='" + ProcessId + "'";
            }
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public JsonResult GetWCProcessCbo(string processid, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetCbo(identity.PlantId, processid, entityId,identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBookingLevel(string FromId, string ToId)
        {
            string sql = @"SELECT ProductionBookingLevel FROM MST.SFGMovementEntity WHERE SFGMovementId = 
                            (SELECT Id FROM MST.SFGMovement WHERE ISNULL(FromProcessId,FromSFGInventoryId) = '" + FromId + @"' AND 
                            ISNULL(ToProcessId,ToSFGInventoryId)='" + ToId + "' AND ISNULL(ProductionBookingLevel,'')<>'')";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSFGMovementFromCbo(string entity)
        {
            string sql;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.IsSysAdmin || identity.IsControlAdmin)
            {
                sql = @"SELECT A.* FROM (
                SELECT DISTINCT 'PROCESS' AS Status,  SFGM.FromProcessId AS FromId,  P.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,P.IsFirst,P.IsCrossAllowed          
                FROM MST.SFGMovement AS SFGM
                INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.FromProcessId AND E.EntityId='" + entity + @"'
                LEFT JOIN [HKP].Process P ON SFGM.FromProcessId = P.Id 
                LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.ProcessId=P.Id
                WHERE ISNULL(SFGM.FromProcessId,'')<>'' 
                UNION ALL
                SELECT DISTINCT 'INVENTORY' AS Status, SFGM.FromSFGInventoryId AS FromId, SFGI.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,SFGI.IsFirst,SFGI.IsCrossAllowed        
                FROM MST.SFGMovement AS SFGM 
                INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.FromSFGInventoryId AND E.EntityId='" + entity + @"'
                LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.FromSFGInventoryId = SFGI.Id 
                LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.SFGInventoryId =SFGI.Id
                WHERE ISNULL(SFGM.FromSFGInventoryId,'')<>''
                ) A  Order by A.Sequence";
            }
            else
            {
                sql = @"SELECT A.* FROM (
                         SELECT DISTINCT 'PROCESS' AS Status, SFGM.FromProcessId AS FromId, P.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,P.IsFirst,P.IsCrossAllowed            
                        FROM MST.SFGMovement AS SFGM
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.FromProcessId AND E.EntityId='" + entity + @"'
                        LEFT JOIN [HKP].Process P ON SFGM.FromProcessId = P.Id 
                        LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.ProcessId=P.Id
                        LEFT JOIN SEC.UserProcess U on U.ProcessId= P.Id  AND U.UserId='" + identity.UserId + @"'
						WHERE ISNULL(SFGM.FromProcessId,'')<>'' 
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY' AS Status, SFGM.FromSFGInventoryId AS FromId, SFGI.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,SFGI.IsFirst,SFGI.IsCrossAllowed        
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.FromSFGInventoryId AND E.EntityId='" + entity + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.FromSFGInventoryId = SFGI.Id 
                        LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.SFGInventoryId =SFGI.Id
				        LEFT JOIN SEC.UserSFGInventory U on U.SFGInventoryId=SFGM.FromSFGInventoryId AND U.UserId='" + identity.UserId + @"'
                        WHERE ISNULL(SFGM.FromSFGInventoryId,'')<>''
                        ) A Order by A.Sequence";
            }
            //return Json(_sqlRepository.GetGridData(new GridParameter { CmdText = sql }).Rows, JsonRequestBehavior.AllowGet);
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSFGMovementToCbo(string FromId, string flag, string EntityId)
        {
            string processId = string.Empty;
            string inventoryId = string.Empty;

            if (flag == "PROCESS")
            {
                processId = FromId;
            }
            else
            {
                inventoryId = FromId;
            }

            string sql;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.IsSysAdmin || identity.IsControlAdmin)
            {
                sql = @"SELECT A.* FROM (
                        SELECT DISTINCT  'PROCESS' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToProcessId AS ToId,  P.UserName
                        FROM MST.SFGMovement AS SFGM  
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.ToProcessId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].Process P ON SFGM.ToProcessId = P.Id WHERE ISNULL(SFGM.ToProcessId,'')<>''
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY'as Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToSFGInventoryId AS ToId, SFGI.UserName
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.ToSFGInventoryId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.ToSFGInventoryId = SFGI.Id WHERE ISNULL(SFGM.ToSFGInventoryId,'')<>''
                        ) A WHERE A.FromProcessId = '" + processId + @"' OR A.FromSFGInventoryId = '" + inventoryId + @"' ";
            }
            else
            {
                sql = @"SELECT A.* FROM (
                        SELECT DISTINCT  'PROCESS' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToProcessId AS ToId,  P.UserName 
                        FROM MST.SFGMovement AS SFGM  
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.ToProcessId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].Process p ON SFGM.ToProcessId = P.Id 
                        LEFT JOIN SEC.UserProcess U on U.ProcessId= p.Id AND U.UserId='" + identity.UserId + @"'
                        WHERE ISNULL(SFGM.ToProcessId,'')<>''
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToSFGInventoryId AS ToId, SFGI.UserName
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.ToSFGInventoryId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.ToSFGInventoryId = SFGI.Id 
                        LEFT JOIN SEC.UserSFGInventory U on U.SFGInventoryId= SFGI.Id  AND U.UserId='" + identity.UserId + @"'
                        WHERE ISNULL(SFGM.ToSFGInventoryId,'')<>''
                        ) A WHERE A.FromProcessId = '" + processId + @"' OR A.FromSFGInventoryId = '" + inventoryId + @"' ";
            }
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public JsonResult GetShiftGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetShiftGroupCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCharInfo(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            return Json(_ProductionSummaryService.GetCharInfo(masterid, workdate, mmid, soid, artid, CharCount, CharacteristicsValueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMentorAndRespPersonByWCM(string wcmId)
        {
            return Json(_ProductionSummaryService.GetMentorAndRespPersonByWCM(wcmId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetChar1Info(string masterid, string soid)
        {
            return Json(_ProductionSummaryService.GetChar1Info(masterid, soid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGrid(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel)
        {
            return Json(_productionSummaryData.GetLineItemGrid(entityid, processid, workdate, shiftid, wcid, ProductionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGridInOut(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel)
        {
            return Json(_productionSummaryData.GetLineItemGrid(entityid, processid, workdate, shiftid, wcid, ProductionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGridSFG(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel, string status)
        {

            return Json(_productionSummaryData.GetLineItemGridSFG(entityid, processid, workdate, shiftid, wcid, ProductionLevel, status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGWIPQty(string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            return Json(_productionSummaryData.GetSFGWIPQty(EntityId, processId, workCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult GetEntityProcessOrderTotalQty(string EntityId, string processId, string salesOrderId, string productionOrderId, string status)
        {
            return Json(_productionSummaryData.GetEntityProcessOrderTotalQty(EntityId, processId, salesOrderId, productionOrderId, status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalProductionQty(string wcid, string workdate)
        {
            return Json(_ProductionSummaryService.GetTotalProductionQty(wcid, workdate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalSOQty(string salesOrderId, string processId)
        {
            return Json(_ProductionSummaryService.GetTotalQty(salesOrderId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalPOQty(string productionOrderId, string processId)
        {
            return Json(_productionSummaryData.GetTotalPOQty(productionOrderId, processId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId)
        {
            return Json(_productionSummaryData.GetSOItem(entityid, workCenterMasterId, productionLevel, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId, string status, bool IsFirst)
        {
            return Json(_productionSummaryData.GetSFGSOItem(entityid, workCenterMasterId, productionLevel, processId, status, IsFirst), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ps.PlantId = identity.PlantId;
            _ProductionSummaryService.SaveMaster(ps, psd, identity.CompanyGroupId);
            return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult CreateInOut(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ps.PlantId = identity.PlantId;
            _ProductionSummaryService.SaveInOutMaster(ps, psd, identity.CompanyGroupId);
            return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetWIPQtyForValidation(string Id, string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            return Json(_productionSummaryData.GetWIPQtyForValidation(Id, EntityId, processId, workCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGTotalQty(string salesOrderId, string processId, string status)
        {
            return Json(_productionSummaryData.GetSFGTotalQty(salesOrderId, processId, status), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSFGTotalPOQty(string productionOrderId, string processId, string status)
        {
            return Json(_productionSummaryData.GetSFGTotalPOQty(productionOrderId, processId, status), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult CreateSFG(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string level, string productionOrderId, string salesOrderId, string processId, string status, bool IsFirst, bool IsCrossAllowed)
        {

            try
            {
                if (IsFirst == true && status == "INVENTORY")
                {
                    if (level == "ProductionOrder")
                    {
                        var allData = _productionSummaryData.GetTotalSFGPOQty(ps.Id, productionOrderId, processId, status);
                        if (allData != null)
                        {

                            int TotalSalesOrderQty = Convert.ToInt32(allData["PlannedQty"].ToString());
                            int RemainingQty = Convert.ToInt32(allData["RemainingQty"].ToString());
                            int TotalProductionQty = Convert.ToInt32(allData["TotalProductionQty"].ToString());

                            if (RemainingQty < 0)
                            {
                                throw new Exception("Order Quantity dosen't available.");
                            }

                            if (TotalSalesOrderQty < (TotalProductionQty + ps.Quantity))
                            {
                                throw new Exception("Produced Quantity should less than Order Quantity.");
                            }

                        }

                    }
                    else
                    {
                        var allData = _productionSummaryData.GetTotalSOSFGQty(ps.Id, salesOrderId, processId, status);
                        int TotalSalesOrderQty = Convert.ToInt32(allData["PlannedQty"].ToString());
                        int RemainingQty = Convert.ToInt32(allData["RemainingQty"].ToString());
                        int TotalProductionQty = Convert.ToInt32(allData["TotalProductionQty"].ToString());

                        if (RemainingQty < 0)
                        {
                            throw new Exception("Order Quantity dosen't available.");
                        }

                        if (TotalSalesOrderQty < (TotalProductionQty + ps.Quantity))
                        {
                            throw new Exception("Produced Quantity should less than Sales Order Quantity.");
                        }
                    }
                }

                if (IsFirst == false)
                {
                    var wipData = _productionSummaryData.GetWIPQtyValidation(ps.Id, ps.EntityId, processId, ps.WorkCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed);

                    if (wipData != null)
                    {
                        decimal InQ = Convert.ToDecimal(wipData["InQuantity"].ToString());
                        decimal OutQ = Convert.ToDecimal(wipData["OutQuantity"].ToString());

                        if (InQ - (OutQ + ps.Quantity) < 0)
                        {
                            throw new Exception("Total out quantity is greater than total in quantity.");
                        }
                    }

                }

                if (IsFirst == true && status == "INVENTORY")
                {
                    var wipData = _productionSummaryData.GetWIPQtyValidation(ps.Id, ps.EntityId, processId, ps.WorkCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed);

                    if (wipData != null)
                    {
                        decimal InQ = Convert.ToDecimal(wipData["InQuantity"].ToString());
                        decimal OutQ = Convert.ToDecimal(wipData["OutQuantity"].ToString());

                        if (InQ - (OutQ + ps.Quantity) < 0)
                        {
                            throw new Exception("Total out quantity is greater than total in quantity.");
                        }
                    }

                }




                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ps.PlantId = identity.PlantId;

                _ProductionSummaryService.SaveInOutMaster(ps, psd, identity.CompanyGroupId);
                return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public JsonResult createDetail(string psid, IEnumerable<ProductionSummaryDetail> psd)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveDetail(psid, psd);
            return Json(new { ProductionSummaryDetail = psd, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult createSecondDetail(IEnumerable<ProductionSummaryDetail> psd, ProductionSummary productionSummary)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveSecondDetail(psd, productionSummary, identity.CompanyGroupId, identity.PlantId);
            return Json(new { ProductionSummaryDetail = psd, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public ActionResult DeleteInOut(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult DeleteSFG(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetInWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetWIPInWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }


        #endregion

        

    }
}