using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.IE;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.OrderManagements;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Library.Service.Systems;
using Aplos.MaterialManagement.MaterialQuery;
using System.Drawing.Printing;
using Zen.Barcode;
using System.Text;
using Aplos.Areas.Materials.Controllers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Web.Hosting;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;


namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductionOrderController : BaseController
    {
        #region Constructor
        string LineItemReference, SKUColor, SKUSize, Qty = null;
        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProductionBulletinTemplateDetail> _bulletinDetailRepository;
        private readonly IRepositoryAsync<ProductionBulletinTemplateMaster> _bulletinProcessRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<ProductionOrderFirstProcessWorkCenter> _fpworkCenterRepository;

        public ProductionOrderController(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R
            , IRepositoryAsync<ProductionBulletinTemplateDetail> bulletinDetailRepository
            , IRepositoryAsync<ProductionBulletinTemplateMaster> bulletinProcessRepository
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<ProductionOrderFirstProcessWorkCenter> fpworkCenterRepository)
        {
            _unitOfWork = U;
            _sqlRepository = R;
            _productionOrderService = productionOrderService;
            _bulletinDetailRepository = bulletinDetailRepository;
            _bulletinProcessRepository = bulletinProcessRepository;
            _pkGeneratorService = pkGeneratorService;
            _fpworkCenterRepository = fpworkCenterRepository;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult Type2()
        {
            return View();
        }
        [Authorize]
        public ActionResult PO()
        {
            return View();
        }

        [Authorize]
        public ActionResult SubPO()
        {
            return View();
        }

        [Authorize]
        public ActionResult Schedulle()
        {
            return View();
        }

        public ActionResult SKURegistration()
        {
            return View();
        }

        public ActionResult PackingBooking()
        {
            return View();
        }

        public ActionResult PackerCategory()
        {
            return View();
        }

        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value, string OrderType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select top 100 * from ( " + new Library.OrderManagement.Production.ProductionOrder().ProductionOrderList() + @"
                            WHERE PO.PlantId='" + identity.PlantId + "' OR EN.PlantId='" + identity.PlantId + "') AS TEMP WHERE OrderType = '" + OrderType + @"' AND " + strkey + " ORDER BY AddedDate DESC";


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetPlanningTypeProcessCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT distinct P.Id,P.UserName FROM PlanningTypes AS pt 
INNER JOIN hkp.Process AS p ON p.Id=pt.BaseProcessId
WHERE PT.PlanningType='PlanningType1' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "' AND pt.PlantId='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetMasterOrderEntityCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT distinct E.Id,E.UserName FROM TRN.MasterOrder M
JOIN ORG.Entity E ON E.Id=M.EntityId
Where M.OrderStatusId='Active' AND M.PlantId='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlanningType2ProcessCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT distinct P.Id,P.UserName FROM PlanningTypes AS pt 
INNER JOIN hkp.Process AS p ON p.Id=pt.BaseProcessId
WHERE PT.PlanningType='PlanningType2' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlanningTypeEntityCbo(string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT E.Id,E.UserName FROM PlanningTypes AS pt 
INNER JOIN org.Entity E on e.Id=pt.EntityId
WHERE PT.PlanningType='PlanningType1' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "' AND PT.BaseProcessId='" + processId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlanningType2EntityCbo(string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT E.Id,E.UserName FROM PlanningTypes AS pt 
INNER JOIN org.Entity E on e.Id=pt.EntityId
WHERE PT.PlanningType='PlanningType2' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "' AND PT.BaseProcessId='" + processId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionHistory(string ProductionOrderId)
        {

            try
            {

                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                IWorkbook workbook = order.GetProductionHistory(ProductionOrderId);

                string strFileName = "Production Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

            return null;
        }

        [Authorize, HttpGet]
        public ActionResult GetSalesOrderListSearch(string column, string value, string productionorderid, string EntityId, string ProcessId, string moentity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            string Entity = "'" + moentity.Replace(",", "','") + "'";//replaced with ""

            string activeStatus = "";
            string plantSql = @"select * from scs.PlantConfig where plantid='" + identity.PlantId + "'";
            DataTable dtPlantConfig = _sqlRepository.GetDataTable(plantSql);
            if (dtPlantConfig.Rows.Count > 0)
                if (bplib.clsWebLib.GetBoolData(dtPlantConfig.Rows[0]["IsProductionOrderCreatedAfterConfirmationOfSO"].ToString()))
                    activeStatus = " AND isnull(SO.IsConfirm,0)=1 ";


            string sql = @"SELECT 
                             MO.Type,isnull(moi.Consignment,0) AS Consignment,
                             CASE WHEN ISNULL(eout.Id,'')<>'' OR ISNULL(TOUT.Id,'')<>'' THEN CONCAT(POWN.UserName,'(',EOWN.UserName,')') ELSE '' END AS OrderOwner
                            ,TEMP.* FROM (SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Checked,null AS Id,null AS ProductionOrderId
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
                       FROM [TRN].[SalesOrder] AS SO 
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
                       LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)

                       WHERE   
                       (
                            --if there is no jobwork, i can create my own production order
                       	    (ISNULL(moi.JobWorkType,'')='' AND MO.PlantId='" + identity.PlantId + @"' )
                                OR 
                       	    (ISNULL(moi.JobWorkType,'')<>'' AND EOUT.PlantId='" + identity.PlantId + @"' )
                       )
                       AND (OS.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' " + activeStatus + @" and  SO.Id not IN (SELECT DISTINCT SalesOrderId FROM [TRN].[ProductionOrderDetail])) AND MOI.ArticleId<>''
                        
						UNION
						
						SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Checked,POD.Id,POD.ProductionOrderId
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
                       FROM [TRN].[SalesOrder] AS SO 
                        left outer join [TRN].[ProductionOrderDetail] POD on POD.SalesOrderId=SO.Id and POD.ProductionOrderID='" + productionorderid + @"'
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
                       WHERE  SO.Id IN (SELECT DISTINCT SalesOrderId FROM [TRN].[ProductionOrderDetail] WHERE ProductionOrderID='" + productionorderid + @"')
                        
						
						
						) AS TEMP 

                            LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON TEMP.MasterOrderItemId=MOI.Id
                            LEFT JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
							LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

WHERE " + strkey + "  and MO.PlantId='" + identity.PlantId + @"' AND MO.EntityId IN(" + Entity + @")  ORDER BY  TEMP.ProductionGrouping,TEMP.MaterialMasterId,TEMP.ArticleId";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult GetSalesOrderList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productionOrderService.GetSalesOrderList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionRecipeMaterialList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionRecipeMaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderType2MaterialList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderType2MaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public JsonResult GetPacketRegistrationDetailList(string masterId)
        {
            return Json(_productionOrderService.GetPacketRegistrationDetailList(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCartonDetailList(string masterId)
        {
            return Json(_productionOrderService.GetCartonDetailList(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderProcessSetList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderProcessSetList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderEntityList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderEntityList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWorkCenterListByEntity(string entityId)
        {
            return Json(_productionOrderService.GetWorkCenterListByEntity(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWorkCenterListByEntityandFirstProcess(string entityId, string processId, string productionOrderId)
        {
            return Json(_productionOrderService.GetWorkCenterListByEntityandFirstProcess(entityId, processId, productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSavedWorkCenterListByEntityandFirstProcess(string productionOrderId)
        {
            return Json(_productionOrderService.GetSavedWorkCenterListByEntityandFirstProcess(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSavedType2WorkCenterListByEntityandFirstProcess(string productionOrderId)
        {
            return Json(_productionOrderService.GetSavedType2WorkCenterListByEntityandFirstProcess(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderWorkCenterList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderWorkCenterList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderType2WorkCenterList(int productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderType2WorkCenterList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRunningOrderType2WorkCenterList(int productionOrderId)
        {
            return Json(_productionOrderService.GetRunningOrderType2WorkCenterList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
            , IEnumerable<ProductionOrderProcessSet> processSetlist
            , IEnumerable<ProductionOrderEntity> entitylist
            , IEnumerable<ProductionOrderWorkCenter> workcenterlist
             , IEnumerable<ProductionOrderFirstProcessWorkCenter> fpworkcenterlist)
        {

            try
            {
                DataTable dtRunningOrderPreference = new DataTable();


                DataTable dtUserDefineLotNo = _sqlRepository.GetDataTable("SELECT * FROM TRN.ProductionOrder where  Id <> '" + master.Id + "' AND UserDefineLotNo = '" + master.UserDefineLotNo + "'");
                if (dtUserDefineLotNo.Rows.Count > 0)
                {
                    throw new CustomException("This Lot No is already exists.");
                }

                DataTable dtTemp = _sqlRepository.GetDataTable("select top 1 * from Trn.ProductionOrder where ID='" + master.Id + "'");
                if (dtTemp.Rows.Count > 0)
                {
                    if (dtTemp.Rows[0]["EntityId"].ToString() != master.EntityId)
                    {
                        //throw new Exception("Planning has been done therefore cannot change entity");
                    }
                    else
                    {
                        #region default running order workcenters
                        //here validate and localize running order workcenters
                        //List<RunningOrderWorkCenter> runningworkcenterlist = new List<RunningOrderWorkCenter>();


                        if (string.IsNullOrEmpty(master.ProductionStatusId))
                            throw new Exception("Production order status cannot be blank");

                        DataTable dtTempStatus = _sqlRepository.GetDataTable("SELECT * FROM hkp.ProductionStatus AS ps WHERE ps.Id='" + master.ProductionStatusId + "'");

                        master.ClosingDate = null;
                        if (dtTempStatus.Rows[0]["StandardName"].ToString().ToUpper() == productionOrderSchedulingParametersType1Controller.PlanningStatus.CLOSED.ToString())
                            master.ClosingDate = DateTime.Now;

                        if (dtTempStatus.Rows[0]["StandardName"].ToString().ToUpper() == productionOrderSchedulingParametersType1Controller.PlanningStatus.RUNNING.ToString())
                        {

                            if (workcenterlist == null)
                            {
                                dtTempStatus = _sqlRepository.GetDataTable("SELECT * FROM trn.RunningOrderWorkCenter WHERE ProductionOrderId='" + master.Id + "'");
                                if (dtTempStatus.Rows.Count == 0)
                                {
                                    // now check whether plan has been simulated or not
                                    dtRunningOrderPreference = _sqlRepository.GetDataTable("SELECT DISTINCT ppt.WorkCenterMasterId FROM ProductionPlanningType1 AS ppt WHERE ppt.ProductionOrderID='" + master.Id + "'");
                                    if (dtRunningOrderPreference.Rows.Count == 0)
                                    {
                                        //no simulation found, now check for line preference for active order
                                        dtRunningOrderPreference = _sqlRepository.GetDataTable("SELECT DISTINCT ppt.WorkCenterMasterId FROM trn.ProductionOrderWorkCenter AS ppt WHERE ppt.ProductionOrderID='" + master.Id + "'");
                                        if (dtRunningOrderPreference.Rows.Count == 0)
                                            throw new Exception("Please provide running order line preference as the production order has been marked as 'Running' and no plan data/line preference found to generate 'running line preference' for this order");

                                    }
                                }
                            }

                        }

                        #endregion default running order workcenters

                    }
                }

                string s = "''";
                foreach (ProductionOrderDetail item in detaillist)
                {
                    s += ",'" + item.SalesOrderId + "'";
                }

                if (s == "''")
                    throw new Exception("No sales order data found, cannot save");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                master.PlantId = identity.PlantId;
                dtTemp = _sqlRepository.GetDataTable("select Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate  from trn.SalesOrder where id in (" + s + ")");
                if (dtTemp.Rows.Count > 0)
                {
                    master.Lsd = null;
                    master.CommitmentDate = null;
                    if (bplib.clsWebLib.RetValidLen(dtTemp.Rows[0]["LSD"].ToString()).ToString() != "")
                        master.Lsd = Convert.ToDateTime(dtTemp.Rows[0]["LSD"].ToString());

                    if (bplib.clsWebLib.RetValidLen(dtTemp.Rows[0]["CommitmentDate"].ToString()).ToString() != "")
                        master.CommitmentDate = Convert.ToDateTime(dtTemp.Rows[0]["CommitmentDate"].ToString());

                }

                DataTable dtPlanQty = _sqlRepository.GetDataTable(@"SELECT SUM(qty) AS Qty,CEILING( SUM( (isnull(qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
                                                                      FROM trn.SalesOrder AS so
                                                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                                    WHERE so.Id IN (" + s + @")");
                if (dtPlanQty.Rows.Count > 0)
                {
                    master.PlannedQty = clsStaticInfo.dbl(dtPlanQty.Rows[0]["PlannedQty"].ToString());
                    master.Qty = clsStaticInfo.dbl(dtPlanQty.Rows[0]["Qty"].ToString());
                }


                //string sqlRow = ProductionOrderList() + " where PO.Id='" + master.Id + "'";
                Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();

                DataTable dtMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[ProductionOrder] where id='" + master.Id + "'");
                if (dtMaster.Rows.Count > 0)
                {
                    master.AddedBy = identity.UserId;
                    master.AddedDate = System.DateTime.Now;
                    master.AddedFromIP = identity.IPAddress;

                    master.UpdatedBy = identity.UserId;
                    master.UpdatedDate = System.DateTime.Now;
                    master.UpdatedFromIP = identity.IPAddress;

                    _productionOrderService.UpdateGraph(master, detaillist, processSetlist, entitylist, workcenterlist, dtRunningOrderPreference, fpworkcenterlist);

                    mo.GenerateLogForTnA(master.Id, TaskAppliedOnEnum.ProductionOrder);
                }
                else
                {
                    master.UpdatedBy = identity.UserId;
                    master.UpdatedDate = System.DateTime.Now;
                    master.UpdatedFromIP = identity.IPAddress;

                    _productionOrderService.InsertGraph(master, detaillist, processSetlist, entitylist, workcenterlist, dtRunningOrderPreference);
                    mo.GenerateLogForTnA(master.Id, TaskAppliedOnEnum.ProductionOrder);

                }

                Library.Service.TaskScheduler.TaskScheduler schedule = new Library.Service.TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.UpdateTaskStatus();
                //Production Order Related Tasks

                string sql = @"SELECT distinct TaskTemplateMasterId FROM trn.MasterOrder AS mo 
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE so.id IN(" + s + ")";
                DataTable dtSO = _sqlRepository.GetDataTable(sql);
                string TaskTemplateMasterId = dtSO.Rows[0]["TaskTemplateMasterId"].ToString();

                DataTable dt = schedule.GetDataSourceProdOrderNew(master.Id, master.EntityId, TaskAppliedOnEnum.ProductionOrder);
                if (dt.Rows.Count > 0)
                    schedule.MakeTNAMaster(dt, master.Id, TaskAppliedOnEnum.ProductionOrder);


                return Json(new { Message = AplosMessage.Insert, DATA = master.Id });
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true });
            }

        }

        //[HttpPost]
        //public JsonResult Edit(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
        //     , IEnumerable<ProductionOrderProcessSet> processSetlist
        //    , IEnumerable<ProductionOrderEntity> entitylist
        //    , IEnumerable<ProductionOrderWorkCenter> workcenterlist)
        //{
        //    _productionOrderService.UpdateGraph(master, detaillist, processSetlist, entitylist, workcenterlist);
        //    return Json(new { Message = AplosMessage.Insert });
        //}

        [HttpPost]
        public JsonResult Delete(string masterid)
        {
            //_productionOrderService.DeleteGraph(masterid);
            //return Json(new { Message = AplosMessage.Deleted });
            try
            {
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //objCon.ExecuteNonQueryWrapper(@"delete from ExpectedSOWiseProductionCompletion where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningType1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningType1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningSnapshotType1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningSnapshot2Type1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionOrderSchedulingParametersType1 where ProductionOrderId='" + masterid + "'", true, "1");


                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderSubprocessSet where ProductionOrderId='" + masterid + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderProcessSet where ProductionOrderId='" + masterid + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderProcessCriteria where ProductionOrderId='" + masterid + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderWorkCenter where ProductionOrderId='" + masterid + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderDetail where ProductionOrderId='" + masterid + "'", true, "1");

                objCon.ExecuteNonQueryWrapper(@"delete from [TRN].[ProductionBulletinTemplateDetail] 
                                                Where ProductionBulletinTemplateMasterId in (Select Id from [TRN].[ProductionBulletinTemplateMaster] 
                                                Where ProductionBulletinTemplateId=(Select Id from [TRN].[ProductionBulletinTemplate] where ProductionOrderId='" + masterid + "'))", true, "1");

                objCon.ExecuteNonQueryWrapper(@"delete from [TRN].[ProductionBulletinTemplateMaster] 
                                               Where ProductionBulletinTemplateId=(Select Id from [TRN].[ProductionBulletinTemplate] where ProductionOrderId='" + masterid + "')", true, "1");

                objCon.ExecuteNonQueryWrapper(@"delete from [TRN].[ProductionBulletinTemplate] where ProductionOrderId='" + masterid + "'", true, "1");

                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrder where Id='" + masterid + "'", true, "1");




                objCon.CommitTransaction();
                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {

                return Json(new { Message = "Production Order might have Planning, TNA or Production Data, therefore can not delete!", Error = true });
            }

        }



        [HttpGet, Authorize]
        public ActionResult GetSampleReport()
        {

            var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            var workbook = application.Workbooks.Create(3);
            var sheet1 = workbook.Worksheets[0];

            sheet1[1, 1].Text = "Tarek";
            workbook.Version = ExcelVersion.Excel2013;


            workbook.SaveAs(DateTime.Now.ToString("yyMMdd") + " Payment Receipt Voucher.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);


            return null;
        }

        [HttpPost, Authorize]
        public JsonResult SaveWCFPData(List<Dictionary<string, object>> data, string productionOrderId)
        {
            try
            {
                DataSet dsMaster = null;
                if (data != null)
                {
                    var count = _fpworkCenterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM dbo.ProductionOrderFirstProcessWorkCenter WHERE ProductionOrderId='{productionOrderId}'").First();
                    ConnectionManager.DAL.ConManager objCon;
                    foreach (var item in data)
                    {
                        string sql = "select * from ProductionOrderFirstProcessWorkCenter  Where ProductionOrderId='" + productionOrderId + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            count++;
                            item["Id"] = _pkGeneratorService.MakePK(productionOrderId, count, 2);
                            item["ProductionOrderId"] = productionOrderId;

                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);

                        }

                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteFPWCDetail(string id)
        {
            DeleteFPWCDetailData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteFPWCDetailData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[ProductionOrderFirstProcessWorkCenter] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

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

        #region upload product picture
        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the production order first");




                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.GetProductImagePath(), fileName);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetProductImagePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetProductImagePath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from trn.productionorder where id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["PicFileName"] = fileName;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }
        [Authorize]
        public ActionResult RemoveDefault(string[] fileNames)
        {
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        #endregion upload product picture

        #region Production Bulletin

        [HttpPost, Authorize]
        public JsonResult EditProductionBulletin(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM TRN.ProductionBulletinTemplate WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                    #endregion data update

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinDataByProductMaster()
        {
            string sql = @"Select BT.Id, BT.CompanyGroupId, BT.BulletinName, BT.AlternativeName, BT.ByWhom, BT.ProductMasterId, BT.SizeGroupId,BT.PicFileName
                         ,PM.UserName ProductMaster, SG.UserName SizeGroup
						  ,Buyer=REPLACE(REPLACE(
										 STUFF((select distinct ', '+B.UserName FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
										JOIN HKP.Buyer B ON B.Id=BTB.BuyerId
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')				 
			         	,BuyerItemRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.BuyerStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
			        	,OwnStyleRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.OwnStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')										
						,Process=REPLACE(REPLACE(
										STUFF((select distinct ', '+P.UserName FROM 
                                       [MST].[BulletinTemplateMaster] BTP 
									   join HKP.Process P ON P.Id=BTP.ProcessId
                                        WHERE BT.Id=BTP.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                         FROM [MST].[BulletinTemplate] BT
                         LEFT JOIN MST.ProductMaster PM ON PM.Id=BT.ProductMasterId
                         LEFT JOIN HKP.SizeGroup SG ON SG.Id=BT.SizeGroupId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionBulletinDataByProductionOrder(string productionOrderId)
        {
            string sql = @"Select BT.*,PM.UserName ProductMaster, SG.UserName SizeGroup FROM [TRN].[ProductionBulletinTemplate] BT
                         LEFT JOIN MST.ProductMaster PM ON PM.Id=BT.ProductMasterId
                         LEFT JOIN HKP.SizeGroup SG ON SG.Id=BT.SizeGroupId
                         WHERE BT.ProductionOrderId='" + productionOrderId + @"' ORDER BY BT.BulletinName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionBulletinProcess(string bulletinTemplateId)
        {
            try
            {
                string sql = @"SELECT  0 HasProcess,P.UserName Process, BTM.* FROM [TRN].[ProductionBulletinTemplateMaster] BTM
                             LEFT JOIN HKP.Process P ON P.Id=BTM.ProcessId
                             WHERE BTM.ProductionBulletinTemplateId='" + bulletinTemplateId + "'  Order By P.[Sequence]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetProductionBulletinDetailData(string bulletinTemplateMasterId)
        {
            try
            {
                string sql = @"SELECT BTD.Id,BTD.ProductionBulletinTemplateMasterId,BTD.Sequence,BTD.OperationVariationId,BTD.OperationGroup,BTD.SkillMasterId,BTD.MachineVarientId,BTD.FGZoneId,BTD.FGComponentId,BTD.IsLastOperation
                            ,CONVERT(NUMERIC(10,2),BTD.AdditionalSPT) AdditionalSPT, CONVERT(NUMERIC(10,2),BTD.TotalSPT) TotalSPT, CONVERT(NUMERIC(10,2),BTD.AllotedWorkstation) AllotedWorkstation
                            , CONVERT(NUMERIC(10,2),BTD.AllotedManpower) AllotedManpower, BTD.AttachmentId,BTD.GaugeFolderId,BTD.OperationConsumptionId,BTD.OperationTypeId,CONVERT(NUMERIC(10,2),BTD.Frequency) Frequency
                            ,BTD.Remark,BTD.OperationCategoryId,BTD.QualityLevel,CONVERT(NUMERIC(10,2),BTD.AvgAllotedTime) AvgAllotedTime,CONVERT(NUMERIC(10,0),BTD.OperationTargetPerHr) OperationTargetPerHr
                            ,CONVERT(NUMERIC(10,0),BTD.RequiredManPower) RequiredManPower
                            ,OV.Code OperationCode, OV.UserName OperationVariation, FZ.UserName FGZone, FC.UserName FGComponent, A.UserName Attachment,
                             GF.UserName GaugeFolder, OC.UserName OperationConsumption, OT.UserName OperationType, OV.OperationId, MMA.StandardName MachineName
                            ,0 AvgAllotedTime, OperationSPT=BTD.TotalSPT-BTD.AdditionalSPT, MM.UserName MaterialMaster, 0 IsMaxAllottedTime 
                            , OM.UserName AS SkillName,OPP.BasicProcessTime,OPP.AssociateProcessTime,OPP.PersonalAllowance,OV.MachineAllowance,OPP.Frequency,OPP.SPI OperationSPI,OV.TotalSAM, OV.AdditionalSAMSymbol,OV.SubOperationSAM,OV.AdditionalSAM
                            ,BTD.SPI,BTD.NoOfStitch,BTD.OperationLength,BTD.StitchCodeId,BTD.FabricWidth,OV.AdditionalAllowance,ISNULL(OV.VASSAMSOURCE,'') VASSAMSOURCE,0 DelFlag,CONVERT(NUMERIC(10,2),BTD.AdditionalWorkstation) AdditionalWorkstation, CONVERT(NUMERIC(10,2),BTD.AdditionalManpower) AdditionalManpower,BTD.AreaCode
                             FROM [TRN].[ProductionBulletinTemplateDetail] BTD
                             LEFT JOIN [MST].[OperationVariation] OV ON OV.Id=BTD.OperationVariationId
                             LEFT JOIN (SELECT OP.Id,ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime
                                     ,ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance, ISNULL(OP.MachineAllowance, 0) AS MachineAllowance
                                     ,OP.Frequency, OP.SPI FROM [MST].[Operation] OP) OPP ON OPP.Id =OV.OperationId
                             LEFT JOIN HKP.FGZone FZ ON FZ.Id=BTD.FGZoneId
                             LEFT JOIN HKP.FGComponent FC ON FC.Id=BTD.FGComponentId
                             LEFT JOIN HKP.Attachment A ON A.Id=BTD.AttachmentId
                             LEFT JOIN HKP.GaugeFolder GF ON GF.Id=BTD.GaugeFolderId
                             LEFT JOIN HKP.OperationConsumption OC ON OC.Id=BTD.OperationConsumptionId
                             LEFT JOIN HKP.OperationType OT ON OT.Id=BTD.OperationTypeId
                             LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = BTD.MachineVarientId
                             LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=MMA.MaterialMasterId
							 LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = BTD.SkillMasterId
                             WHERE BTD.ProductionBulletinTemplateMasterId='" + bulletinTemplateMasterId + "' ORDER BY BTD.Sequence ";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinMachineOperation(string bulletinTemplateMasterId)
        {
            try
            {
                string sql = @"SELECT BTD.Id,BTD.ProductionBulletinTemplateMasterId,BTD.Sequence,BTD.OperationVariationId,BTD.OperationGroup,BTD.SkillId,BTD.MachineVarientId,BTD.FGZoneId,BTD.FGComponentId
                            ,CONVERT(NUMERIC(10,2),BTD.AdditionalSPT) AdditionalSPT, CONVERT(NUMERIC(10,2),BTD.TotalSPT) TotalSPT, CONVERT(NUMERIC(10,2),BTD.AllotedWorkstation) AllotedWorkstation
                            , CONVERT(NUMERIC(10,2),BTD.AllotedManpower) AllotedManpower, BTD.AttachmentId,BTD.GaugeFolderId,BTD.OperationConsumptionId,BTD.OperationTypeId,CONVERT(NUMERIC(10,2),BTD.Frequency) Frequency
                            ,BTD.Remark,BTD.OperationCategoryId,BTD.QualityLevel,CONVERT(NUMERIC(10,2),BTD.AvgAllotedTime) AvgAllotedTime,CONVERT(NUMERIC(10,0),BTD.OperationTargetPerHr) OperationTargetPerHr
                            ,CONVERT(NUMERIC(10,0),BTD.RequiredManPower) RequiredManPower
                            ,OV.Code OperationCode, OV.UserName OperationVariation, FZ.UserName FGZone, FC.UserName FGComponent, A.UserName Attachment,
                             GF.UserName GaugeFolder, OC.UserName OperationConsumption, OT.UserName OperationType, OV.OperationId, MMA.StandardName MachineName
                            ,0 AvgAllotedTime, OperationSPT=BTD.TotalSPT-BTD.AdditionalSPT, MM.UserName MaterialMaster, 0 IsMaxAllottedTime 
                            , SK.UserName AS SkillName,OPP.BasicProcessTime,OPP.AssociateProcessTime,OPP.PersonalAllowance,OPP.MachineAllowance,OPP.Frequency,OPP.SPI OperationSPI,OV.TotalSAM, OV.AdditionalSAMSymbol,OV.SubOperationSAM,OV.AdditionalSAM
							,BTD.SPI,BTD.NoOfStitch,BTD.OperationLength,BTD.StitchCodeId,BTD.FabricWidth,BTD.NeedleDescription,BTD.NeedleMaterialMasterId,MMN.UserName NeedleMaterialMaster, BTD.NeedleArticleId,MMNA.ShortName NeedleArticle
							,BTD.BobbinDescription,BTD.BobbinMaterialMasterId,MMB.UserName BobbinMaterialMaster,BTD.BobbinArticleId,MMBA.ShortName BobbinArticle
							,BTD.LooperDescription,BTD.LooperMaterialMasterId,MML.UserName LooperMaterialMaster,BTD.LooperArticleId,MMLA.ShortName LooperArticle,SC.userName StitchCode
                            ,BTD.SPIConsumption,BTD.NeedleConsumption,BTD.BobbinConsumption,BTD.LooperConsumption,BTD.Consumption
                             FROM [TRN].[ProductionBulletinTemplateDetail] BTD
                             LEFT JOIN [MST].[OperationVariation] OV ON OV.Id=BTD.OperationVariationId
                             LEFT JOIN (SELECT OP.Id,ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime
                                     ,ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance, ISNULL(OP.MachineAllowance, 0) AS MachineAllowance
                                     ,OP.Frequency, OP.SPI FROM [MST].[Operation] OP) OPP ON OPP.Id =OV.OperationId
                             LEFT JOIN HKP.FGZone FZ ON FZ.Id=BTD.FGZoneId
                             LEFT JOIN HKP.FGComponent FC ON FC.Id=BTD.FGComponentId
                             LEFT JOIN HKP.Attachment A ON A.Id=BTD.AttachmentId
                             LEFT JOIN HKP.GaugeFolder GF ON GF.Id=BTD.GaugeFolderId
                             LEFT JOIN HKP.OperationConsumption OC ON OC.Id=BTD.OperationConsumptionId
                             LEFT JOIN HKP.OperationType OT ON OT.Id=BTD.OperationTypeId
                             LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = BTD.MachineVarientId
                             LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=MMA.MaterialMasterId
							 LEFT JOIN [HKP].[Skill] AS SK ON BTD.SkillId=Sk.Id
                             LEFT JOIN [HKP].StitchCode AS SC ON BTD.StitchCodeId=SC.Id

                             LEFT JOIN [MST].[MaterialMaster] MMN ON MMN.Id=BTD.NeedleMaterialMasterId
							 LEFT JOIN [MST].[MaterialMasterArticle] MMNA ON MMNA.Id = BTD.NeedleArticleId

							  LEFT JOIN [MST].[MaterialMaster] MMB ON MMB.Id=BTD.BobbinMaterialMasterId
							 LEFT JOIN [MST].[MaterialMasterArticle] MMBA ON MMBA.Id = BTD.BobbinArticleId

							 LEFT JOIN [MST].[MaterialMaster] MML ON MML.Id=BTD.LooperMaterialMasterId
							 LEFT JOIN [MST].[MaterialMasterArticle] MMLA ON MMLA.Id = BTD.LooperArticleId
                             WHERE BTD.ProductionBulletinTemplateMasterId='" + bulletinTemplateMasterId + "'  AND MM.Id <>'' ORDER BY BTD.Sequence ";

                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetThreadMatrixData(string bulletinTemplateMasterId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT SUM(A.NeedleConsumption) NeedleConsumption, SUM(A.BobbinConsumption) BobbinConsumption, SUM(A.LooperConsumption) LooperConsumption,A.ArticleId, A.Thread
                                FROM 
                                (
                                SELECT BTD.NeedleArticleId ArticleId, NMA.ShortName Thread,SUM(BTD.NeedleConsumption) NeedleConsumption,0 BobbinConsumption, 0 LooperConsumption 
                                FROM [TRN].[ProductionBulletinTemplateDetail] BTD 
                                LEFT JOIN MST.MaterialMaster NM ON NM.Id=BTD.NeedleMaterialMasterId
                                JOIN MST.MaterialMasterArticle NMA ON NMA.Id=BTD.NeedleArticleId
                                WHERE ProductionBulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.NeedleArticleId, NMA.ShortName,BTD.BobbinArticleId
                                UNION ALL

                                select BTD.BobbinArticleId, BMA.ShortName BobbinArticle,0 NeedleConsumption,SUM(BTD.BobbinConsumption) BobbinConsumption, 0 LooperConsumption 
                                from [TRN].[ProductionBulletinTemplateDetail] BTD 
                                LEFT JOIN MST.MaterialMaster BM ON BM.Id=BTD.BobbinMaterialMasterId
                                JOIN MST.MaterialMasterArticle BMA ON BMA.Id=BTD.BobbinArticleId
                                Where ProductionBulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.BobbinArticleId, BMA.ShortName
                                UNION ALL
                                select BTD.LooperArticleId, LMA.ShortName LooperArticle,0 NeedleConsumption,0 BobbinConsumption,SUM(BTD.LooperConsumption) LooperConsumption
                                from [TRN].[ProductionBulletinTemplateDetail] BTD 
                                LEFT JOIN MST.MaterialMaster LM ON LM.Id=BTD.LooperMaterialMasterId
                                JOIN MST.MaterialMasterArticle LMA ON LMA.Id=BTD.LooperArticleId
                                Where ProductionBulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.LooperArticleId, LMA.ShortName
                                ) AS A 
                                GROUP BY A.ArticleId, A.Thread";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationData(string processId, string bulletinTemplateId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT 0 Active
                           	,OV.Id OperationVariationId
                           	,OV.Code OperationCode
                           	,OV.[Sequence]
                           	,A.Id MachineVarientId
							,MM.UserName MaterialMaster
                           	,A.StandardName Article
                           	,OV.UserName OperationVariation
                           	,OV.SubOperationSAM
                           	,OV.AdditionalSAM
                           	,OV.SPI,ISNULL(OV.VASSAMSOURCE,'') VASSAMSOURCE
                           	,ISNULL(OV.VASFINALSAM,OV.TotalSAM) TtalSAM
							,TotalSAM=CASE WHEN ISNULL(OV.VASSAMSOURCE,'')='' THEN OV.TotalSAM ELSE OV.VASFINALSAM END
                           	,OV.Frequency
                            ,OT.Id OperationTypeId
                            ,OV.AdditionalSAMSymbol
                            ,OV.OperationId
                            ,OCT.Id OperationCategoryId
							,OCT.UserName OperationCategory
							,OM.Id SkillMasterId, OM.UserName SkillName
                            ,SC.Id StitchCodeId ,SC.UserName StitchCode,O.OperationLength,OV.AreaCode
                           FROM [MST].[OperationVariation] OV
                           LEFT JOIN [MST].[MaterialMasterArticle] A ON A.Id = OV.ArticleId
						   LEFT JOIN MST.OperationMaster AS OM ON OM.Id=OV.OperationMasterId
                           LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=A.MaterialMasterId 
                           LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                           LEFT JOIN [HKP].[OperationType] OT ON OT.Id = O.OperationTypeId
                           LEFT JOIN [HKP].[OperationCategory] OCT ON OCT.Id = O.OperationCategoryId
                            LEFT JOIN [HKP].[StitchCode] SC ON SC.Id = A.StitchCodeId
						   LEFT JOIN (SELECT * FROM [MST].[OperationProcess] WHERE ProcessId='" + processId + @"')OP ON OP.OperationId=OV.OperationId
                           WHERE OV.CompanyGroupId = '" + identity.CompanyGroupId + @"' 
                           --AND OV.Id NOT IN (SELECT OperationVariationId FROM [TRN].[ProductionBulletinTemplateDetail] PBTD
						   --LEFT JOIN [TRN].[ProductionBulletinTemplateMaster] PBTM ON PBTM.Id=PBTD.ProductionBulletinTemplateMasterId
					       --WHERE PBTM.ProductionBulletinTemplateId='" + bulletinTemplateId + @"')
                           ORDER BY OV.UserName";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private DataSet CheckBulletinInProduction(string ProductionOrderId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM [TRN].[ProductionBulletinTemplate] WHERE ProductionOrderId='" + ProductionOrderId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        [HttpPost, Authorize]
        public JsonResult CreateProductionBulletin(ProductionBulletinTemplate entity)
        {

            try
            {
                var checkBull = CheckBulletinInProduction(entity.ProductionOrderId);
                if (checkBull.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This bulletin has already been taken for this Production.");
                }
                CopyBulletinTemplate(entity);

                return Json(new { Message = "Bulletin Template copied successfully." });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        public void CopyBulletinTemplate(ProductionBulletinTemplate entity)
        {
            DataSet BulletinTemplate;
            DataSet BulletinTemplateMaster;
            DataSet BulletinTemplateDetail;
            DataSet BulletinCalculation;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionBulletinTemplate] where 1=2", out BulletinTemplate, false, "1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionBulletinTemplateMaster] where 1=2", out BulletinTemplateMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionBulletinTemplateDetail] where 1=2", out BulletinTemplateDetail, false, "1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[ProducitonBulletinCalculation] where 1=2", out BulletinCalculation, false, "1");

                DataTable Master = _sqlRepository.GetDataTable("select * from [MST].[BulletinTemplate] WHERE Id='" + entity.Id + "'");
                DataTable Detail = _sqlRepository.GetDataTable("select * from [MST].[BulletinTemplateMaster] WHERE BulletinTemplateId='" + entity.Id + "'");
                DataTable Process = _sqlRepository.GetDataTable("select * from [MST].[BulletinTemplateDetail] WHERE BulletinTemplateMasterId IN (SELECT Id FROM [MST].[BulletinTemplateMaster] WHERE BulletinTemplateId='" + entity.Id + "')");
                DataTable ProBulCal = _sqlRepository.GetDataTable("select * from [dbo].[BulletinCalculation] WHERE BulletinTemplateMasterId IN (SELECT Id FROM [MST].[BulletinTemplateMaster] WHERE BulletinTemplateId='" + entity.Id + "')");

                NewId = GetGeneralPK();
                DataRow drBOMDestination = BulletinTemplate.Tables[0].NewRow();
                CopyRow(Master.Rows[0], ref drBOMDestination);
                drBOMDestination["Id"] = NewId;

                drBOMDestination["ProductionOrderId"] = entity.ProductionOrderId;
                drBOMDestination["BulletinTemplateId"] = entity.Id;
                BulletinTemplate.Tables[0].Rows.Add(drBOMDestination);

                for (int i = 0; i < Detail.Rows.Count; i++)
                {
                    DataRow drDetailDestination = BulletinTemplateMaster.Tables[0].NewRow();
                    CopyRow(Detail.Rows[i], ref drDetailDestination);
                    drDetailDestination["Id"] = NewId + "-" + (i + 1);
                    drDetailDestination["ProductionBulletinTemplateId"] = NewId;
                    BulletinTemplateMaster.Tables[0].Rows.Add(drDetailDestination);


                    if (ProBulCal.Rows.Count > 0)
                    {
                        DataRow drBCDestination = BulletinCalculation.Tables[0].NewRow();
                        CopyRow(ProBulCal.Rows[0], ref drBCDestination);
                        drBCDestination["ProductionBulletinTemplateMasterId"] = drDetailDestination["Id"];
                        BulletinCalculation.Tables[0].Rows.Add(drBCDestination);
                    }

                    Process.DefaultView.RowFilter = "BulletinTemplateMasterId='" + Detail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < Process.DefaultView.Count; K++)
                    {
                        //GetOperationMasterByOperationVariation(Process.DefaultView[K].Row["OperationVariationId"].ToString(), out DataSet dsOperationMaster);

                        DataRow drDetailSKUDestination = BulletinTemplateDetail.Tables[0].NewRow();
                        CopyRow(Process.DefaultView[K].Row, ref drDetailSKUDestination);
                        drDetailSKUDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drDetailSKUDestination["ProductionBulletinTemplateMasterId"] = NewId + "-" + (i + 1);
                        //if (string.IsNullOrEmpty(dsOperationMaster.Tables[0].Rows[0]["SkillMasterId"].ToString()))
                        //{
                        //    drDetailSKUDestination["SkillMasterId"] = DBNull.Value;
                        //}
                        //else
                        //{
                        //    drDetailSKUDestination["SkillMasterId"] = dsOperationMaster.Tables[0].Rows[0]["SkillMasterId"].ToString();
                        //}


                        BulletinTemplateDetail.Tables[0].Rows.Add(drDetailSKUDestination);
                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(BulletinTemplate, BulletinTemplateMaster, BulletinCalculation, BulletinTemplateDetail);

                MoveImage(entity.Id, entity.PicFileName, NewId);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public static void MoveImage(string fromName, string toName, string NewBulletinId)
        {
            var Fromdirectory = ResourcesPathReader.GetBulletinImagePath();
            var Todirectory = ResourcesPathReader.GetProductionBulletinImagePath();
            if (!string.IsNullOrEmpty(fromName))
            {
                string path = Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName));
                //var path = Path.Combine(Fromdirectory, fromName);
                if (System.IO.File.Exists(path))
                {
                    //File.Copy(Path.Combine(Fromdirectory, fromName), Path.Combine(Todirectory, NewBulletinId), true);
                    System.IO.File.Copy(Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName)), Path.Combine(Todirectory, NewBulletinId + Path.GetExtension(toName)), true);
                }
            }
        }

        [HttpPost]
        public JsonResult CreateProductionBulletinTemplateMaster(ProductionBulletinTemplateMaster entity)
        {

            try
            {
                // string ProductionBulletinId;
                SaveProcessData(entity);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetProcessPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ProductionBulletinTemplateMaster), out sID);
            return sID;

        }

        private DataSet GetProductionBulletinTemplateMaster(string id, string ProductionBulletinTemplateId, string processId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM [TRN].[ProductionBulletinTemplateMaster]  WHERE Id<>'" + id + "' AND ProductionBulletinTemplateId='" + ProductionBulletinTemplateId + "' AND ProcessId='" + processId + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        private void SaveProcessData(ProductionBulletinTemplateMaster data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                var checkProcess = GetProductionBulletinTemplateMaster(data.Id, data.ProductionBulletinTemplateId, data.ProcessId);
                if (checkProcess.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("Process should be unique.");

                }
                else
                {
                    string sql = "SELECT * FROM [TRN].[ProductionBulletinTemplateMaster] WHERE Id='" + data.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = "PM" + GetProcessPK();

                        dr["ProductionBulletinTemplateId"] = data.ProductionBulletinTemplateId;
                        dr["ProcessId"] = data.ProcessId;
                        dr["RequiredStdTarget"] = data.RequiredStdTarget;
                        dr["PlannedHoursPerDay"] = data.PlannedHoursPerDay;
                        dr["MaxNoOfWS"] = data.MaxNoOfWS;
                        dr["BottleNeckPercentage"] = data.BottleNeckPercentage;
                        dr["SPT"] = data.SPT;
                        dr["BuildUp"] = data.BuildUp;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        dr["ProcessId"] = data.ProcessId;
                        dr["RequiredStdTarget"] = data.RequiredStdTarget;
                        dr["PlannedHoursPerDay"] = data.PlannedHoursPerDay;
                        dr["MaxNoOfWS"] = data.MaxNoOfWS;
                        dr["BottleNeckPercentage"] = data.BottleNeckPercentage;
                        dr["SPT"] = data.SPT;
                        dr["BuildUp"] = data.BuildUp;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        private string GetOperationPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ProductionBulletinTemplateDetail), out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public JsonResult CreateOperation(IEnumerable<ProductionBulletinTemplateDetail> entities, string productionBulletinTemplateMasterId, Dictionary<string, object> calculateddata, string pId)
        {
            try
            {
                SaveOperationData(entities, productionBulletinTemplateMasterId, calculateddata, pId
                    );
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        public void GetAutoSequence(string ProductionBulletinTemplateMasterId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL((MAX(Sequence)+1),0) Sequence FROM [TRN].[ProductionBulletinTemplateDetail] Where ProductionBulletinTemplateMasterId='" + ProductionBulletinTemplateMasterId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetOperationMasterByOperationVariation(string OperationVariationId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT OperationMasterId FROM MST.OperationVariation Where Id='" + OperationVariationId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void EvaluateSPI(IEnumerable<ProductionBulletinTemplateDetail> ItemDetail)
        {
            string StitchCodeIds = "''"; string SPIs = "NULL";
            foreach (var item in ItemDetail)
            {
                if (string.IsNullOrEmpty(item.StitchCodeId) == false)
                {
                    StitchCodeIds += ",'" + item.StitchCodeId + "'";
                    SPIs += "," + item.SPI + "";

                }

            }

            DataTable dtFormula = _sqlRepository.GetDataTable(@" SELECT f.*,SC.Needle,sc.Bobbin,sc.Looper FROM   [dbo].[SPIFormula]  F
                                    inner join hkp.StitchCode SC ON SC.id=f.StitchCodeId  WHERE f.StitchCodeId in (" + StitchCodeIds + ") and f.SPI IN (" + SPIs + ")");

            string Formula;
            double SPI, FabricWht, SPIConsumption, NeedleConsumption, BobbinConsumption, LooperConsumption;
            decimal Consumption;
            foreach (var item in ItemDetail)
            {
                dtFormula.DefaultView.RowFilter = "StitchCodeId='" + item.StitchCodeId + "' AND SPI=" + item.SPI;
                if (dtFormula.DefaultView.Count > 0)
                {
                    Formula = bplib.clsWebLib.GetBoolData(dtFormula.DefaultView[0]["isFormula"].ToString()) == true ? dtFormula.DefaultView[0]["Formula"].ToString() : dtFormula.DefaultView[0]["FixedValue"].ToString();
                    SPI = item.SPI;
                    FabricWht = (double)item.FabricWidth;

                    Library.General.FormulaEvaluator.ThreadConsumption threadConsumption = new Library.General.FormulaEvaluator.ThreadConsumption(
                        new Library.General.FormulaEvaluator.ThreadConsumption.FKeys { Key = Library.General.FormulaEvaluator.ThreadConsumption.FxKeys.SPI, Value = SPI },
                        new Library.General.FormulaEvaluator.ThreadConsumption.FKeys { Key = Library.General.FormulaEvaluator.ThreadConsumption.FxKeys.FabWht, Value = FabricWht }
                        );

                    SPIConsumption = threadConsumption.ExecuteFunction(Formula);
                    Consumption = (item.OperationLength * (decimal)SPIConsumption * item.NoOfStitch) / 100;
                    //NeedleConsumption = threadConsumption.ExecuteFunction(Formula,clsStaticInfo.dbl(dtFormula.DefaultView[0]["Needle"].ToString()));
                    //BobbinConsumption = threadConsumption.ExecuteFunction(Formula, clsStaticInfo.dbl(dtFormula.DefaultView[0]["Bobbin"].ToString()));
                    //LooperConsumption= threadConsumption.ExecuteFunction(Formula, clsStaticInfo.dbl(dtFormula.DefaultView[0]["Looper"].ToString()));

                    NeedleConsumption = threadConsumption.ExecuteFunction(Consumption.ToString(), clsStaticInfo.dbl(dtFormula.DefaultView[0]["Needle"].ToString()));
                    BobbinConsumption = threadConsumption.ExecuteFunction(Consumption.ToString(), clsStaticInfo.dbl(dtFormula.DefaultView[0]["Bobbin"].ToString()));
                    LooperConsumption = threadConsumption.ExecuteFunction(Consumption.ToString(), clsStaticInfo.dbl(dtFormula.DefaultView[0]["Looper"].ToString()));



                    item.SPIConsumption = (decimal)SPIConsumption;
                    item.Consumption = (decimal)Consumption;
                    item.NeedleConsumption = (decimal)NeedleConsumption;
                    item.BobbinConsumption = (decimal)BobbinConsumption;
                    item.LooperConsumption = (decimal)LooperConsumption;
                    //item.PerOperationConsumption = (decimal)PerOperationConsumption;

                }
            }

        }
        public void GetBulletinCalculation(string bulletinTemplateMasterId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT * FROM [dbo].[ProducitonBulletinCalculation] Where ProductionBulletinTemplateMasterId='" + bulletinTemplateMasterId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void SaveOperationData(IEnumerable<ProductionBulletinTemplateDetail> data, string productionBulletinTemplateMasterId, Dictionary<string, object> bulletinCalculation, string pId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsEO = null;
                DataSet dsBC = null;
                DataSet dsMaster = null;
                GetBulletinCalculation(productionBulletinTemplateMasterId, out dsBC);

                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;


                    DataSet dsSeq;
                    GetAutoSequence(productionBulletinTemplateMasterId, out dsSeq);
                    decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Sequence"].ToString());
                    if (seq != 0)
                    {
                        seq--;
                    }
                    EvaluateSPI(data);
                    string sql = "SELECT * FROM [TRN].[ProductionBulletinTemplateDetail] WHERE ProductionBulletinTemplateMasterId='" + productionBulletinTemplateMasterId + "'";
                    string esql = "select * from dbo.EmployeeOperationWip Where ProductionOrderId='" + pId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(esql, out dsEO, false, "1");
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item.Id + "' AND ProductionBulletinTemplateMasterId='" + productionBulletinTemplateMasterId + "'";

                        if (dv.Count == 0)
                        {
                            seq++;
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = "PD" + GetOperationPK();

                            dr["ProductionBulletinTemplateMasterId"] = productionBulletinTemplateMasterId;
                            dr["Sequence"] = seq;
                            dr["AreaCode"] = item.AreaCode;
                            dr["OperationVariationId"] = item.OperationVariationId;
                            dr["OperationGroup"] = item.OperationGroup;
                            dr["SkillMasterId"] = item.SkillMasterId;
                            dr["MachineVarientId"] = item.MachineVarientId;
                            dr["FGZoneId"] = item.FGZoneId;
                            dr["FGComponentId"] = item.FGComponentId;
                            dr["AdditionalSPT"] = item.AdditionalSPT;
                            dr["TotalSPT"] = item.TotalSPT;
                            dr["AvgAllotedTime"] = item.AvgAllotedTime;
                            dr["AllotedWorkstation"] = item.AllotedWorkstation;
                            dr["AllotedManpower"] = item.AllotedManpower;
                            dr["AdditionalWorkstation"] = item.AdditionalWorkstation;
                            dr["AdditionalManpower"] = item.AdditionalManpower;
                            dr["AttachmentId"] = item.AttachmentId;
                            dr["GaugeFolderId"] = item.GaugeFolderId;
                            dr["OperationConsumptionId"] = item.OperationConsumptionId;
                            dr["OperationTypeId"] = item.OperationTypeId;
                            dr["Frequency"] = item.Frequency;
                            dr["Remark"] = item.Remark;

                            dr["OperationCategoryId"] = item.OperationCategoryId;
                            dr["QualityLevel"] = item.QualityLevel;
                            dr["OperationTargetPerHr"] = item.OperationTargetPerHr;
                            dr["RequiredManPower"] = item.RequiredManPower;

                            dr["SPI"] = item.SPI;
                            dr["NoOfStitch"] = item.NoOfStitch;
                            dr["OperationLength"] = item.OperationLength;
                            dr["StitchCodeId"] = item.StitchCodeId;
                            dr["FabricWidth"] = item.FabricWidth;
                            dr["NeedleDescription"] = item.NeedleDescription;
                            dr["NeedleMaterialMasterId"] = item.NeedleMaterialMasterId;
                            dr["NeedleArticleId"] = item.NeedleArticleId;
                            dr["BobbinDescription"] = item.BobbinDescription;
                            dr["BobbinMaterialMasterId"] = item.BobbinMaterialMasterId;
                            dr["BobbinArticleId"] = item.BobbinArticleId;
                            dr["LooperDescription"] = item.LooperDescription;
                            dr["LooperMaterialMasterId"] = item.LooperMaterialMasterId;
                            dr["LooperArticleId"] = item.LooperArticleId;

                            dr["Consumption"] = item.Consumption;
                            dr["SPIConsumption"] = item.SPIConsumption;
                            dr["NeedleConsumption"] = item.NeedleConsumption;
                            dr["BobbinConsumption"] = item.BobbinConsumption;
                            dr["LooperConsumption"] = item.LooperConsumption;
                            dr["IsLastOperation"] = item.IsLastOperation;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIp"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataView dveo = new DataView(dsEO.Tables[0]);
                            dveo.RowFilter = "OperationVariationId='" + item.OperationVariationId + "'";

                            if (dveo.Count > 0)
                            {
                                if (Convert.ToDecimal(dveo[0]["OperationSequence"]) != item.Sequence)
                                {
                                    throw new Exception("Operation Sequence " + item.Sequence + " change not accepted as it is used in Employee Production.");
                                }
                            }

                            //edit
                            DataRow dr = dv[0].Row;

                            dr.BeginEdit();

                            dr["ProductionBulletinTemplateMasterId"] = productionBulletinTemplateMasterId;
                            dr["Sequence"] = item.Sequence;
                            dr["OperationVariationId"] = item.OperationVariationId;
                            dr["AreaCode"] = item.AreaCode;
                            dr["OperationGroup"] = item.OperationGroup;
                            dr["SkillMasterId"] = item.SkillMasterId;
                            dr["MachineVarientId"] = item.MachineVarientId;
                            dr["FGZoneId"] = item.FGZoneId;
                            dr["FGComponentId"] = item.FGComponentId;
                            dr["AdditionalSPT"] = item.AdditionalSPT;
                            dr["TotalSPT"] = item.TotalSPT;
                            dr["AvgAllotedTime"] = item.AvgAllotedTime;
                            dr["AllotedWorkstation"] = item.AllotedWorkstation;
                            dr["AllotedManpower"] = item.AllotedManpower;
                            dr["AdditionalWorkstation"] = item.AdditionalWorkstation;
                            dr["AdditionalManpower"] = item.AdditionalManpower;
                            dr["AttachmentId"] = item.AttachmentId;
                            dr["GaugeFolderId"] = item.GaugeFolderId;
                            dr["OperationConsumptionId"] = item.OperationConsumptionId;
                            dr["OperationTypeId"] = item.OperationTypeId;
                            dr["Frequency"] = item.Frequency;
                            dr["Remark"] = item.Remark;

                            dr["OperationCategoryId"] = item.OperationCategoryId;
                            dr["QualityLevel"] = item.QualityLevel;

                            dr["OperationTargetPerHr"] = item.OperationTargetPerHr;
                            dr["RequiredManPower"] = item.RequiredManPower;

                            dr["SPI"] = item.SPI;
                            dr["NoOfStitch"] = item.NoOfStitch;
                            dr["OperationLength"] = item.OperationLength;
                            dr["StitchCodeId"] = item.StitchCodeId;
                            dr["FabricWidth"] = item.FabricWidth;
                            dr["NeedleDescription"] = item.NeedleDescription;
                            dr["NeedleMaterialMasterId"] = item.NeedleMaterialMasterId;
                            dr["NeedleArticleId"] = item.NeedleArticleId;
                            dr["BobbinDescription"] = item.BobbinDescription;
                            dr["BobbinMaterialMasterId"] = item.BobbinMaterialMasterId;
                            dr["BobbinArticleId"] = item.BobbinArticleId;
                            dr["LooperDescription"] = item.LooperDescription;
                            dr["LooperMaterialMasterId"] = item.LooperMaterialMasterId;
                            dr["LooperArticleId"] = item.LooperArticleId;

                            dr["Consumption"] = item.Consumption;
                            dr["SPIConsumption"] = item.SPIConsumption;
                            dr["NeedleConsumption"] = item.NeedleConsumption;
                            dr["BobbinConsumption"] = item.BobbinConsumption;
                            dr["LooperConsumption"] = item.LooperConsumption;
                            dr["IsLastOperation"] = item.IsLastOperation;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIp"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                    }

                }

                if (bulletinCalculation != null)
                {
                    if (dsBC.Tables[0].Rows.Count == 0)
                    {
                        AddNewRow(dsBC.Tables[0], bulletinCalculation);
                    }
                    else
                    {
                        EditRow(dsBC.Tables[0].Rows[0], bulletinCalculation);
                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsBC);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public JsonResult UpdateMachine(ProductionBulletinTemplateDetail machine)
        {
            UpdateOperationMachine(machine);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpPost]
        public JsonResult UpdateSequence(ProductionBulletinTemplateDetail bulletinTemplateDetail)
        {
            UpdateOperationSequence(bulletinTemplateDetail);
            return Json(new { Message = AplosMessage.Updated });
        }

        public void UpdateOperationSequence(ProductionBulletinTemplateDetail entity)
        {
            try
            {
                var dblist = _bulletinDetailRepository.Find(entity.Id);

                dblist.Sequence = entity.Sequence;

                AuditService.UpdatedLog(dblist);
                _bulletinDetailRepository.Update(dblist);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void UpdateOperationMachine(ProductionBulletinTemplateDetail entity)
        {
            try
            {
                var dblist = _bulletinDetailRepository.Find(entity.Id);

                dblist.MachineVarientId = entity.MachineVarientId;
                dblist.SkillMasterId = entity.SkillMasterId;
                //dblist.OperationMasterId = null;

                AuditService.UpdatedLog(dblist);
                _bulletinDetailRepository.Update(dblist);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private DataSet GetProductionBulletinTemplateDetail(string bulletinTemplateMasterId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"Select * from [TRN].[ProductionBulletinTemplateDetail]  where ProductionBulletinTemplateMasterId='" + bulletinTemplateMasterId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public ActionResult DeleteProcess(string id)
        {
            DeleteProductionProcess(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteProductionProcess(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                var data = GetProductionBulletinTemplateDetail(id);
                if (data.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This process has operation, first delete it's operation.");
                }

                strSQL = "DELETE FROM TRN.ProductionBulletinTemplateMaster WHERE Id = '" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }

        }//End of function

        public ActionResult DeleteOperation(string id)
        {
            DeleteProductionOperation(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteProductionOperation(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM TRN.ProductionBulletinTemplateDetail WHERE Id = '" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function


        [HttpPost, Authorize]
        public ActionResult DeleteProductionBulletin(string ProductionOrderId)
        {
            DeleteProductionBulletinData(ProductionOrderId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        string GetProductionBulletinTemplate(string ProductionOrderId)
        {
            try
            {
                var _sql = @"Select Id FROM [TRN].[ProductionBulletinTemplate] where ProductionOrderId = '" + ProductionOrderId + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);
                if (list.Count > 0)
                {
                    return list[0]["Id"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteProductionBulletinData(string ProductionOrderId)
        {
            string strSQL, strBSQL, strOSQL, strBCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                string PBId = GetProductionBulletinTemplate(ProductionOrderId);
                strOSQL = @"DELETE FROM TRN.ProductionBulletinTemplateDetail WHERE[ProductionBulletinTemplateMasterId] 
                            in (SELECT Id FROM TRN.ProductionBulletinTemplateMaster WHERE ProductionBulletinTemplateId = '" + PBId + "')";

                strBCSQL = @"DELETE FROM dbo.ProducitonBulletinCalculation WHERE[ProductionBulletinTemplateMasterId] 
                            in (SELECT Id FROM TRN.ProductionBulletinTemplateMaster WHERE ProductionBulletinTemplateId = '" + PBId + "')";

                strBSQL = @"DELETE FROM TRN.ProductionBulletinTemplateMaster WHERE ProductionBulletinTemplateId = '" + PBId + "'";

                strSQL = @"DELETE FROM [TRN].[ProductionBulletinTemplate] where Id = '" + PBId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [Authorize, HttpGet]
        public ActionResult GetProductionOrderAndProdBulletinData()
        {
            string CmdText = @"SELECT  PO.Id ProductionOrderId,ISNULL(PS.UserName,'') ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, ISNULL(PD.Product,'')Product,ISNULL(PD.ProductCategory,'')ProductCategory,ISNULL(PD.Buyer,'')Buyer,ISNULL(PD.Customer,'')Customer 
                                   ,ISNULL(PD.BuyerRefNo,'')BuyerRefNo,ISNULL(PD.OwnRefNo,'')OwnRefNo,ISNULL(PD.StyleNo,'')StyleNo,ISNULL(PD.OwnStyleNo,'')OwnStyleNo,ISNULL(PD.Description,'')Description,ISNULL(PD.PONumber,'')PONumber,PBT.Id ProductionBulletinTemplateId
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN [TRN].[ProductionBulletinTemplate] PBT ON PBT.ProductionOrderId=PO.Id
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,
								   
								   Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
										 ,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                                           ,BuyerRefNo = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', ''), 

                                                    OwnRefNo =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	, 

													StyleNo=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	, 
	                                                
                                                    OwnStyleNo=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 ,
                                                PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
										, Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PBT.Id<>'' ";
            return Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult CopyProductionBulletin(string Id, string ProductionOrderId)
        {
            try
            {
                var checkBull = CheckBulletinInProduction(ProductionOrderId);
                if (checkBull.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This bulletin has already been taken for this Production.");
                }
                CopyProductionBulletinTemplate(Id, ProductionOrderId);

                return Json(new { Error = false, Message = "Production Bulletin copied successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }
        private string GetGeneralPK()
        {
            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            string idFromDB;
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PB", out idFromDB);
            string systemID = idFromDB;
            string sID = systemID.Trim();
            return sID;

        }
        public static void MoveProductionBulletinImage(string fromName, string toName, string NewBulletinId)
        {
            var Fromdirectory = ResourcesPathReader.GetProductionBulletinImagePath();
            var Todirectory = ResourcesPathReader.GetProductionBulletinImagePath();
            if (!string.IsNullOrEmpty(fromName))
            {
                string path = Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName));
                //var path = Path.Combine(Fromdirectory, fromName);
                if (System.IO.File.Exists(path))
                {
                    //File.Copy(Path.Combine(Fromdirectory, fromName), Path.Combine(Todirectory, NewBulletinId), true);
                    System.IO.File.Copy(Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName)), Path.Combine(Todirectory, NewBulletinId + Path.GetExtension(toName)), true);
                }
            }
        }

        public void CopyProductionBulletinTemplate(string MasterId, string ProductionOrderId)
        {
            DataSet ProductionBulletinTemplate;
            DataSet ProductionBulletinTemplateMaster;
            DataSet ProductionBulletinTemplateDetail;
            DataSet ProductionBulletinTemplateCal;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionBulletinTemplate] where 1=2", out ProductionBulletinTemplate, false, "1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionBulletinTemplateMaster] where 1=2", out ProductionBulletinTemplateMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionBulletinTemplateDetail] where 1=2", out ProductionBulletinTemplateDetail, false, "1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[ProducitonBulletinCalculation] where 1=2", out ProductionBulletinTemplateCal, false, "1");

                DataTable Master = _sqlRepository.GetDataTable("select * from [TRN].[ProductionBulletinTemplate] WHERE Id='" + MasterId + "'");
                DataTable Detail = _sqlRepository.GetDataTable("select * from [TRN].[ProductionBulletinTemplateMaster] WHERE ProductionBulletinTemplateId='" + MasterId + "'");
                DataTable Process = _sqlRepository.GetDataTable("select * from [TRN].[ProductionBulletinTemplateDetail] WHERE ProductionBulletinTemplateMasterId IN (SELECT Id FROM [TRN].[ProductionBulletinTemplateMaster] WHERE ProductionBulletinTemplateId='" + MasterId + "')");
                DataTable btcal = _sqlRepository.GetDataTable("select * from [dbo].[ProducitonBulletinCalculation] WHERE ProductionBulletinTemplateMasterId IN (SELECT Id FROM [TRN].[ProductionBulletinTemplateMaster] WHERE ProductionBulletinTemplateId='" + MasterId + "')");

                NewId = GetGeneralPK();
                DataRow drBOMDestination = ProductionBulletinTemplate.Tables[0].NewRow();
                CopyRow(Master.Rows[0], ref drBOMDestination);
                drBOMDestination["Id"] = NewId;
                drBOMDestination["ParentId"] = MasterId;
                drBOMDestination["ProductionOrderId"] = ProductionOrderId;
                drBOMDestination["BulletinTemplateId"] = DBNull.Value;
                ProductionBulletinTemplate.Tables[0].Rows.Add(drBOMDestination);

                for (int i = 0; i < Detail.Rows.Count; i++)
                {
                    DataRow drDetailDestination = ProductionBulletinTemplateMaster.Tables[0].NewRow();
                    CopyRow(Detail.Rows[i], ref drDetailDestination);
                    drDetailDestination["Id"] = NewId + "-" + (i + 1);
                    drDetailDestination["ProductionBulletinTemplateId"] = NewId;
                    ProductionBulletinTemplateMaster.Tables[0].Rows.Add(drDetailDestination);

                    if (btcal.Rows.Count > 0)
                    {
                        DataRow drBCDestination = ProductionBulletinTemplateCal.Tables[0].NewRow();
                        CopyRow(btcal.Rows[0], ref drBCDestination);
                        drBCDestination["ProductionBulletinTemplateMasterId"] = drDetailDestination["Id"];
                        ProductionBulletinTemplateCal.Tables[0].Rows.Add(drBCDestination);
                    }

                    Process.DefaultView.RowFilter = "ProductionBulletinTemplateMasterId='" + Detail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < Process.DefaultView.Count; K++)
                    {
                        //GetOperationMasterByOperationVariation(Process.DefaultView[K].Row["OperationVariationId"].ToString(), out DataSet dsOperationMaster);

                        DataRow drDetailSKUDestination = ProductionBulletinTemplateDetail.Tables[0].NewRow();
                        CopyRow(Process.DefaultView[K].Row, ref drDetailSKUDestination);
                        drDetailSKUDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drDetailSKUDestination["ProductionBulletinTemplateMasterId"] = NewId + "-" + (i + 1);

                        //if (string.IsNullOrEmpty(dsOperationMaster.Tables[0].Rows[0]["OperationMasterId"].ToString()))
                        //{
                        //    drDetailSKUDestination["OperationMasterId"] = DBNull.Value;
                        //}
                        //else
                        //{
                        //    drDetailSKUDestination["OperationMasterId"] = dsOperationMaster.Tables[0].Rows[0]["OperationMasterId"].ToString();
                        //}

                        ProductionBulletinTemplateDetail.Tables[0].Rows.Add(drDetailSKUDestination);
                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ProductionBulletinTemplate, ProductionBulletinTemplateMaster, ProductionBulletinTemplateDetail);

                MoveProductionBulletinImage(MasterId, Master.Rows[0]["PicFileName"].ToString(), NewId);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

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

        [HttpPost, Authorize]
        public ActionResult UpdateOperationMaster(ProductionBulletinTemplateDetail entity)
        {
            UpdateOperationMasterData(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        public void UpdateOperationMasterData(ProductionBulletinTemplateDetail entity)
        {
            try
            {
                var dblist = _bulletinDetailRepository.Find(entity.Id);

                dblist.SkillMasterId = entity.SkillMasterId;

                AuditService.UpdatedLog(dblist);
                _bulletinDetailRepository.Update(dblist);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public ActionResult DeleteMultiOperation(string id)
        {
            DeleteMultiProductionOperation(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteMultiProductionOperation(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM TRN.ProductionBulletinTemplateDetail WHERE Id " + id + "";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #region upload Production Bulletin picture
        [HttpPost, Authorize]
        public ActionResult SaveBulletinDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the Production Bulletin first");

                foreach (var file in UploadDefault)
                {
                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetProductionBulletinImagePath(), fileName);

                    var directory = ResourcesPathReader.GetProductionBulletinImagePath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetProductionBulletinImagePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetProductionBulletinImagePath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from [TRN].[ProductionBulletinTemplate] where id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["PicFileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["PicFileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);
                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("SELECT PicFileName FROM [TRN].[ProductionBulletinTemplate] WHERE Id ='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion upload product picture

        #region Multi Operation Add
        [HttpPost, Authorize]
        public JsonResult InsertMultiOperation(string Code, string processId, string bulletinTemplateMasterId, IEnumerable<MultiCode> MultiCodeList)
        {

            //string str = Code.Replace(" ", ",");
            string codes = "'" + Code.Replace(" ", "','") + "'";//replaced with ""
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };
            SaveMultiOperation(para, codes, processId, bulletinTemplateMasterId, MultiCodeList);
            return Json(new { Message = AplosMessage.Success });
        }

        private DataSet GetOperationDataByCode(string companyGroupId, string Code, string processId, string bulletinTemplateMasterId)
        {
            try
            {
                GridParameter parameters;
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT CONVERT (bit,0) Active
                           	,OV.Id OperationVariationId
                           	,OV.Code OperationCode
                           	,OV.[Sequence]
                           	,A.Id MachineVarientId
							,MM.UserName MaterialMaster
                           	,A.StandardName Article
							,S.Id SkillId
                           	,S.UserName Skill
                           	,OV.UserName OperationVariation
                           	,OV.SubOperationSAM
                           	,OV.AdditionalSAM
                           	,OV.SPI,OV.VASSAMSOURCE
                           	,ISNULL(OV.VASFINALSAM,OV.TotalSAM) TtalSAM
							,TotalSAM=CASE WHEN ISNULL(OV.VASSAMSOURCE,'')='' THEN OV.TotalSAM ELSE OV.VASFINALSAM END
                           	,OV.Frequency
                            ,OT.Id OperationTypeId
                            ,OV.AdditionalSAMSymbol
                            ,OV.OperationId
                            ,OCT.Id OperationCategoryId
							,OCT.UserName OperationCategory
                            ,SC.Id StitchCodeId ,SC.UserName StitchCode,O.OperationLength
                           FROM [MST].[OperationVariation] OV
                           LEFT JOIN [MST].[MaterialMasterArticle] A ON A.Id = OV.ArticleId
                           LEFT JOIN [HKP].[Skill] S ON S.Id = OV.SkillId
                           LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=A.MaterialMasterId AND MM.SkillId=S.Id
                           LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                           LEFT JOIN [HKP].[OperationType] OT ON OT.Id = O.OperationTypeId
                           LEFT JOIN [HKP].[OperationCategory] OCT ON OCT.Id = O.OperationCategoryId
                           LEFT JOIN [HKP].[StitchCode] SC ON SC.Id = A.StitchCodeId
						   INNER JOIN (Select * from [MST].[OperationProcess] WHERE ProcessId='" + processId + @"')OP ON OP.OperationId=OV.OperationId
                           WHERE OV.CompanyGroupId = '" + companyGroupId + @"' AND OV.Code IN (" + Code + @") "
                };


                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public void SaveMultiOperation(IdentityParameter para, string Code, string processId, string bulletinTemplateMasterId, IEnumerable<MultiCode> MultiCodeList)
        {
            try
            {
                DataSet dataSet = GetOperationDataByCode(para.CompanyGroupId, Code, processId, bulletinTemplateMasterId);
                ConnectionManager.DAL.ConManager objCon;
                var id = GetOperationPK();
                string sql = "SELECT * FROM [TRN].[ProductionBulletinTemplateDetail] WHERE Id=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsOperation, false, "1");
                int count = 0;


                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        //var filteredSeq = MultiCodeList.Where(p => dataSet.Tables[0].Rows[i]["OperationCode"] = p.OperationCode.Contains(p.Sequenc.ToString()));
                        var filteredSeq = MultiCodeList.Where(p => p.OperationCode == dataSet.Tables[0].Rows[i]["OperationCode"].ToString()).Select(p => p.Sequenc).FirstOrDefault();

                        count++;

                        DataRow dr = dsOperation.Tables[0].NewRow();

                        dr["Id"] = id + "-" + count;
                        dr["ProductionBulletinTemplateMasterId"] = bulletinTemplateMasterId;
                        dr["Sequence"] = filteredSeq;
                        dr["OperationVariationId"] = dataSet.Tables[0].Rows[i]["OperationVariationId"];
                        dr["OperationGroup"] = null;
                        dr["SkillId"] = dataSet.Tables[0].Rows[i]["SkillId"];
                        dr["MachineVarientId"] = dataSet.Tables[0].Rows[i]["MachineVarientId"];
                        dr["FGZoneId"] = null;
                        dr["FGComponentId"] = null;
                        dr["AdditionalSPT"] = dataSet.Tables[0].Rows[i]["AdditionalSAM"];
                        dr["TotalSPT"] = dataSet.Tables[0].Rows[i]["TotalSAM"];
                        dr["AllotedWorkstation"] = 0;
                        dr["AllotedManpower"] = 0;
                        dr["AdditionalWorkstation"] = 0;
                        dr["AdditionalManpower"] = 0;
                        dr["AvgAllotedTime"] = 0;
                        dr["AttachmentId"] = null;
                        dr["GaugeFolderId"] = null;
                        dr["OperationConsumptionId"] = null;
                        dr["OperationTypeId"] = dataSet.Tables[0].Rows[i]["OperationTypeId"];
                        dr["Frequency"] = dataSet.Tables[0].Rows[i]["Frequency"];
                        dr["Remark"] = null;
                        dr["OperationCategoryId"] = dataSet.Tables[0].Rows[i]["OperationCategoryId"];
                        dr["QualityLevel"] = null;
                        dr["OperationTargetPerHr"] = 0;
                        dr["RequiredManPower"] = 0;

                        dr["SPI"] = dataSet.Tables[0].Rows[i]["SPI"];
                        dr["NoOfStitch"] = 0;
                        dr["OperationLength"] = dataSet.Tables[0].Rows[i]["OperationLength"];
                        dr["StitchCodeId"] = dataSet.Tables[0].Rows[i]["StitchCodeId"];
                        dr["FabricWidth"] = 0;
                        dr["NeedleDescription"] = null;
                        dr["NeedleMaterialMasterId"] = null;
                        dr["NeedleArticleId"] = null;
                        dr["BobbinMaterialMasterId"] = null;
                        dr["BobbinArticleId"] = null;
                        dr["LooperDescription"] = null;
                        dr["LooperMaterialMasterId"] = null;
                        dr["LooperArticleId"] = null;
                        dr["SPIConsumption"] = 0;
                        dr["NeedleConsumption"] = 0;
                        dr["BobbinConsumption"] = 0;
                        dr["LooperConsumption"] = 0;
                        dr["Consumption"] = 0;

                        dr["AddedBy"] = para.AddedBy;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = para.AddedFromIP;


                        dsOperation.Tables[0].Rows.Add(dr);
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsOperation);
                }
                else
                {
                    throw new Exception("Wrong Operation Code !!!.");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        #endregion Multi Operation Add

        #region Change OPCode
        [Authorize, HttpPost]
        public JsonResult UpdateOperationVaiationCode(ProductionBulletinTemplateDetail bulletinTemplateDetail, string processId, string bulletinTemplateMasterId)
        {

            string Code = "'" + bulletinTemplateDetail.OperationCode.Replace(" ", "','") + "'";//replaced with ""
            //string Code = bulletinTemplateDetail.OperationCode;
            decimal Sequence = bulletinTemplateDetail.Sequence;
            DeleteProductionOperation(bulletinTemplateDetail.Id);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };

            ReplaceOperation(para, Code, Sequence, processId, bulletinTemplateMasterId);
            return Json(new { Message = AplosMessage.Updated });
        }

        public void ReplaceOperation(IdentityParameter para, string Code, decimal Sequence, string processId, string bulletinTemplateMasterId)
        {
            try
            {
                DataSet ds = GetOperationDataByCode(para.CompanyGroupId, Code, processId, bulletinTemplateMasterId);
                SaveReplacedBulletinDetailData(ds, para, bulletinTemplateMasterId, Sequence);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void SaveReplacedBulletinDetailData(DataSet dataSet, IdentityParameter para, string BulletinTemplateMasterId, decimal Sequence)
        {
            ConnectionManager.DAL.ConManager objCon;
            var id = GetOperationPK();
            string sql = "SELECT * FROM [TRN].[ProductionBulletinTemplateDetail] WHERE Id=''";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out DataSet dsOperation, false, "1");
            int count = 0;


            if (dataSet.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    count++;

                    DataRow dr = dsOperation.Tables[0].NewRow();

                    dr["Id"] = id + "-" + count;
                    dr["ProductionBulletinTemplateMasterId"] = BulletinTemplateMasterId;
                    dr["Sequence"] = Sequence;
                    dr["OperationVariationId"] = dataSet.Tables[0].Rows[i]["OperationVariationId"];
                    dr["OperationGroup"] = null;
                    dr["SkillId"] = dataSet.Tables[0].Rows[i]["SkillId"];
                    dr["MachineVarientId"] = dataSet.Tables[0].Rows[i]["MachineVarientId"];
                    dr["FGZoneId"] = null;
                    dr["FGComponentId"] = null;
                    dr["AdditionalSPT"] = dataSet.Tables[0].Rows[i]["AdditionalSAM"];
                    dr["TotalSPT"] = dataSet.Tables[0].Rows[i]["TotalSAM"];
                    dr["AllotedWorkstation"] = 0;
                    dr["AllotedManpower"] = 0;
                    dr["AdditionalWorkstation"] = 0;
                    dr["AdditionalManpower"] = 0;
                    dr["AvgAllotedTime"] = 0;
                    dr["AttachmentId"] = null;
                    dr["GaugeFolderId"] = null;
                    dr["OperationConsumptionId"] = null;
                    dr["OperationTypeId"] = dataSet.Tables[0].Rows[i]["OperationTypeId"];
                    dr["Frequency"] = dataSet.Tables[0].Rows[i]["Frequency"];
                    dr["Remark"] = null;
                    dr["OperationCategoryId"] = dataSet.Tables[0].Rows[i]["OperationCategoryId"];
                    dr["QualityLevel"] = null;
                    dr["OperationTargetPerHr"] = 0;
                    dr["RequiredManPower"] = 0;

                    dr["SPI"] = dataSet.Tables[0].Rows[i]["SPI"];
                    dr["NoOfStitch"] = 0;
                    dr["OperationLength"] = dataSet.Tables[0].Rows[i]["OperationLength"];
                    dr["StitchCodeId"] = dataSet.Tables[0].Rows[i]["StitchCodeId"];
                    dr["FabricWidth"] = 0;
                    dr["NeedleDescription"] = null;
                    dr["NeedleMaterialMasterId"] = null;
                    dr["NeedleArticleId"] = null;
                    dr["BobbinMaterialMasterId"] = null;
                    dr["BobbinArticleId"] = null;
                    dr["LooperDescription"] = null;
                    dr["LooperMaterialMasterId"] = null;
                    dr["LooperArticleId"] = null;
                    dr["SPIConsumption"] = 0;
                    dr["NeedleConsumption"] = 0;
                    dr["BobbinConsumption"] = 0;
                    dr["LooperConsumption"] = 0;
                    dr["Consumption"] = 0;

                    dr["AddedBy"] = para.AddedBy;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = para.AddedFromIP;


                    dsOperation.Tables[0].Rows.Add(dr);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsOperation);
            }
            else
            {
                throw new Exception("Wrong Operation Code !!!.");
            }

        }

        #endregion

        #endregion



        //tarek 

        [HttpGet, Authorize]
        public JsonResult GetPOLotControlData(string poId, string entityId)
        {
            try
            {
                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                return Json(order.GetPOLotControlData(poId, entityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetPOLotContSettingsData(string poId, string entityId)
        {
            List<Dictionary<string, object>> data = null;
            try
            {
                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                return Json(order.GetPOLotControlSettingData(entityId, poId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public JsonResult GetPOLotControlSettingsData(string entityId, string poId, string userLotNo)
        {
            List<Dictionary<string, object>> data = null;
            try
            {
                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                DataTable dtData = order.GetPOLotControlSettingsData(entityId, poId, userLotNo);

                data = new List<Dictionary<string, object>>();
                foreach (DataRow row in dtData.Rows)
                {
                    Dictionary<string, object> dictionary = Enumerable.Range(0, dtData.Columns.Count).ToDictionary(i => dtData.Columns[i].ColumnName, i => row.ItemArray[i]);
                    data.Add(dictionary);
                }

                SaveLotSettingData(data, poId, userLotNo);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SaveLotSettingData(List<Dictionary<string, object>> data, string poId, string userLotNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsUserDefineLotNo;
            try
            {
                string sql = "SELECT * FROM TRN.ProductionOrder WHERE Id='" + poId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM TRN.ProductionOrder where  Id<>'" + poId + "' AND UserDefineLotNo='" + userLotNo + "'", out dsUserDefineLotNo, false, "1");
                if (dsUserDefineLotNo.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This Lot No is already exists.");
                }

                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["UserDefineLotNo"] = userLotNo;
                    dr["IsPreDefineLotApplicable"] = true;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                #region LotControlSetting 

                // objCon.executeQuery("DELETE FROM dbo.LotControlSetting WHERE ProductionOrderId='" + poId + "'");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.LotControlSetting where  ProductionOrderId='" + poId + "'", out dsChild, false, "1");
                for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                {
                    dsChild.Tables[0].Rows[i].Delete();
                }

                if (data != null)
                {
                    int c = 0;
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"].ToString() + "'";


                        if (dv.Count == 0)
                        {
                            c++;
                            item["Id"] = poId + " - " + c;
                            item["ProductionOrderId"] = poId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [HttpPost, Authorize]
        public JsonResult CreateLotControl(List<Dictionary<string, object>> data, string poId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsEntity;
            string _Id = string.Empty;
            int c = 0;
            try
            {
                if (string.IsNullOrEmpty(poId))
                {
                    throw new CustomException("Select Production Order.");
                }
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionOrderLotControl where  ProductionOrderId='" + poId + "'", out dsEntity, false, "1");
                //for (int i = 0; i < dsEntity.Tables[0].Rows.Count; i++)
                //{
                //    dsEntity.Tables[0].Rows[i].Delete();
                //}

                if (data != null)
                {
                    //genid.GenID("ProductionOrderLotControl", out _Id);
                    foreach (var item in data)
                    {
                        c++;
                        DataView dv = new DataView(dsEntity.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = poId + "-" + c;
                            item["ProductionOrderId"] = poId;
                            item["IsDefault"] = true;
                            AddNewRow(dsEntity.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsEntity);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public ActionResult SetAreaCode(string id, string areacode)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("Update [TRN].[ProductionBulletinTemplateDetail] set AreaCode=" + areacode + " Where Id= '" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        #region ProductionOrderType2
        [HttpPost]
        public JsonResult CreateProductionOrderType2(Dictionary<string, object> data, List<Dictionary<string, object>> detaillist, List<Dictionary<string, object>> processSetlist, List<Dictionary<string, object>> workcenterlist, List<Dictionary<string, object>> fpworkcenterlist)
        {
            try
            {

                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                DataSet dsProcDetail;
                DataSet dsWCDetail;
                DataSet dsFPWCDetail;
                DataSet dsDD = null;
                DataSet dsPDD = null;
                DataSet dsWCDD = null;
                DataSet dsFPWCDD = null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionOrder] where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                #region data update
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {

                    bplib.clsGenID objID = new bplib.clsGenID();
                    objID.GenHRID(System.DateTime.Now.ToShortDateString(), "PRODUCTION ORDER", out _Id);
                    data["Id"] = _Id;
                    data["OrderType"] = "PlanningType2";
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update

                #region ProductionOrderDetail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionOrderDetail] where ProductionOrderId='" + data["Id"] + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from [TRN].[ProductionOrderDetail] where ProductionOrderId='" + data["Id"] + "'", out dsDD, false, "1");
                int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());
                if (detaillist != null)
                {
                    foreach (var item in detaillist)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            item["Id"] = detailid;
                            item["ProductionOrderId"] = _MasterId;


                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["SalesOrderId"] = item["SalesOrderId"];
                            drmo.EndEdit();
                        }
                    }
                }
                #endregion ProductionOrderDetail

                #region ProductionOrderProcessSet


                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionOrderProcessSet] where ProductionOrderId='" + data["Id"] + "'", out dsProcDetail, false, "1");
                con.OpenDataSetThroughAdapter("select CAST((RIGHT(ISNULL(MAX(CAST(Id AS INT)), 0),2)) AS INT) countId from [TRN].[ProductionOrderProcessSet] where ProductionOrderId='" + data["Id"] + "'", out dsPDD, false, "1");
                int pcount = Convert.ToInt32(dsPDD.Tables[0].Rows[0]["countId"].ToString());
                int sq = 0;
                if (processSetlist != null)
                {
                    foreach (var item in processSetlist)
                    {
                        sq++;
                        DataView dv = new DataView(dsProcDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            pcount++;
                            string detailid = materialCommonService.MakePK(_MasterId, pcount, 2);
                            item["Id"] = detailid;
                            item["ProductionOrderId"] = _MasterId;
                            item["Sequence"] = sq;
                            item["IsCompleted"] = 0;


                            materialCommonService.AddNewRowD(dsProcDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["ProcessId"] = item["ProcessId"];
                            drmo["IsBaseProcess"] = item["IsBaseProcess"];
                            drmo["ProductionBookingLevel"] = item["ProductionBookingLevel"];
                            drmo["IsInventory"] = item["IsInventory"];
                            drmo["Days"] = item["Days"];
                            drmo["ProductionCycleTime"] = item["ProductionCycleTime"];
                            drmo["Qty"] = item["Qty"];
                            drmo["UOMId"] = item["UOMId"];
                            drmo["IsProductionVerification"] = item["IsProductionVerification"];
                            drmo["Sequence"] = sq;
                            drmo.EndEdit();
                        }
                    }
                }
                #endregion ProductionOrderProcessSet

                #region ProductionOrderType2WorkCenter


                con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionOrderType2WorkCenter] where ProductionOrderId='" + data["Id"] + "'", out dsWCDetail, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from [TRN].[ProductionOrderType2WorkCenter] where ProductionOrderId='" + data["Id"] + "'", out dsWCDD, false, "1");
                int wccount = Convert.ToInt32(dsWCDD.Tables[0].Rows[0]["countId"].ToString());
                if (workcenterlist != null)
                {
                    foreach (var item in workcenterlist)
                    {
                        DataView dv = new DataView(dsWCDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            wccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, wccount, 2);
                            item["Id"] = detailid;
                            item["ProductionOrderId"] = _MasterId;


                            materialCommonService.AddNewRowD(dsWCDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["WorkCenterMasterId"] = item["WorkCenterMasterId"];
                            drmo.EndEdit();
                        }
                    }
                }
                #endregion ProductionOrderType2WorkCenter

                #region ProductionOrderType2FirstProcessWorkCenter


                con.OpenDataSetThroughAdapter("select * from [dbo].[ProductionOrderType2FirstProcessWorkCenter] where ProductionOrderId='" + data["Id"] + "'", out dsFPWCDetail, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[ProductionOrderType2FirstProcessWorkCenter] where ProductionOrderId='" + data["Id"] + "'", out dsFPWCDD, false, "1");
                int fpwccount = Convert.ToInt32(dsFPWCDD.Tables[0].Rows[0]["countId"].ToString());
                if (fpworkcenterlist != null)
                {
                    foreach (var item in fpworkcenterlist)
                    {
                        DataView dv = new DataView(dsWCDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            fpwccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, fpwccount, 2);
                            item["Id"] = detailid;
                            item["ProductionOrderId"] = _MasterId;


                            materialCommonService.AddNewRowD(dsWCDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["WorkCenterMasterId"] = item["WorkCenterMasterId"];
                            drmo["ProcessId"] = item["ProcessId"];
                            drmo.EndEdit();
                        }
                    }
                }
                #endregion ProductionOrderType2FirstProcessWorkCenter

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail, dsProcDetail, dsWCDetail, dsFPWCDetail);
                return Json(new { Error = false, DATA = _Id, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetType2List(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select top 100 * from ( " + new Library.OrderManagement.Production.ProductionOrder().ProductionOrderList() + @"
                            WHERE PO.PlantId='" + identity.PlantId + "' OR EN.PlantId='" + identity.PlantId + "') AS TEMP WHERE OrderType = 'PlanningType2' AND " + strkey + " ORDER BY AddedDate DESC";


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteType2(string masterid)
        {
            //_productionOrderService.DeleteGraph(masterid);
            //return Json(new { Message = AplosMessage.Deleted });
            try
            {
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //objCon.ExecuteNonQueryWrapper(@"delete from ExpectedSOWiseProductionCompletion where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningType1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningType1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningSnapshotType1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionPlanningSnapshot2Type1 where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from ProductionOrderSchedulingParametersType1 where ProductionOrderId='" + masterid + "'", true, "1");


                //objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderType2SubprocessSet where ProductionOrderId='" + masterid + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderProcessSet where ProductionOrderId='" + masterid + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderType2ProcessCriteria where ProductionOrderId='" + masterid + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderType2WorkCenter where ProductionOrderId='" + masterid + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderDetail where ProductionOrderId='" + masterid + "'", true, "1");

                //objCon.ExecuteNonQueryWrapper(@"delete from [TRN].[ProductionBulletinTemplateDetail] 
                //                                Where ProductionBulletinTemplateMasterId in (Select Id from [TRN].[ProductionBulletinTemplateMaster] 
                //                                Where ProductionBulletinTemplateId=(Select Id from [TRN].[ProductionBulletinTemplate] where ProductionOrderId='" + masterid + "'))", true, "1");

                //objCon.ExecuteNonQueryWrapper(@"delete from [TRN].[ProductionBulletinTemplateMaster] 
                //                               Where ProductionBulletinTemplateId=(Select Id from [TRN].[ProductionBulletinTemplate] where ProductionOrderId='" + masterid + "')", true, "1");

                //objCon.ExecuteNonQueryWrapper(@"delete from [TRN].[ProductionBulletinTemplate] where ProductionOrderId='" + masterid + "'", true, "1");

                objCon.ExecuteNonQueryWrapper(@"delete from trn.ProductionOrderType2 where Id='" + masterid + "'", true, "1");




                objCon.CommitTransaction();
                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {

                return Json(new { Message = "Production Order might have Planning, TNA or Production Data, therefore can not delete!", Error = true });
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetProductionOredrList(string id)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetPOData(id), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetSKUData(string poId, bool SKU1, bool SKU2, bool Both)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetSKUData(poId, SKU1, SKU2, Both), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        [HttpPost, Authorize]
        public ActionResult GetSavedSKUData(string poId)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetSavedSKUData(poId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetWorkCenterGroup()
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetWorkCenterGroup(), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public JsonResult SaveSPO(List<Dictionary<string, object>> data, string POId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                #region Entity 

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionOrderSchedulingParametersType2 Where ProductionOrderID='" + POId + "'", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "ID='" + Convert.ToInt64(item["ID"]) + "'";

                        if (dv.Count == 0)
                        {
                            item["ProductionOrderID"] = POId;
                            item["Qty"] = item["PlanQty"];
                            item["Color"] = DBNull.Value;
                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            item["Qty"] = item["PlanQty"];
                            EditRow(drmo, item);
                        }
                    }


                }

                #endregion
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }



        [HttpPost, Authorize]
        public JsonResult SavePreferenceWorkCenter(List<Dictionary<string, object>> workcenterlist, int spoId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsWorkcenter = null;
                string sql = "SELECT * FROM [TRN].[ProductionOrderType2WorkCenter] where ProductionOrderID=" + spoId + "";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsWorkcenter, false, "1");

                string SystemID = "";
                for (int i = 0; i < workcenterlist.Count; i++)
                {
                    dsWorkcenter.Tables[0].DefaultView.RowFilter = "WorkcenterMasterID='" + workcenterlist[i]["WorkCenterMasterId"] + "'";
                    if (dsWorkcenter.Tables[0].DefaultView.Count == 0)
                    {
                        if (SystemID == "")
                        {
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID(System.DateTime.Now.ToShortDateString(), "PRTWC", out SystemID);
                        }
                        DataRow dr = dsWorkcenter.Tables[0].NewRow();

                        dr["id"] = SystemID + "-" + (i + 1).ToString();
                        dr["ProductionOrderID"] = spoId;
                        dr["WorkcenterMasterID"] = workcenterlist[i]["WorkCenterMasterId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsWorkcenter.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsWorkcenter);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveType2RunningWorkCenter(List<Dictionary<string, object>> workcenterlist, int spoId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsWorkcenter = null;
                string sql = "SELECT * FROM [TRN].[RunningOrderType2WorkCenter] where ProductionOrderID=" + spoId + "";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsWorkcenter, false, "1");

                string SystemID = "";
                for (int i = 0; i < workcenterlist.Count; i++)
                {
                    dsWorkcenter.Tables[0].DefaultView.RowFilter = "WorkcenterMasterID='" + workcenterlist[i]["WorkCenterMasterId"] + "'";
                    if (dsWorkcenter.Tables[0].DefaultView.Count == 0)
                    {
                        if (SystemID == "")
                        {
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID(System.DateTime.Now.ToShortDateString(), "PRTWC", out SystemID);
                        }
                        DataRow dr = dsWorkcenter.Tables[0].NewRow();

                        dr["id"] = SystemID + "-" + (i + 1).ToString();
                        dr["ProductionOrderID"] = spoId;
                        dr["WorkcenterMasterID"] = workcenterlist[i]["WorkCenterMasterId"];
                        dr["isResidualApplicable"] = bplib.clsWebLib.GetBoolData(workcenterlist[i]["isResidualApplicable"]);
                        dr["Qty"] = Convert.IsDBNull(workcenterlist[i]["Qty"]) ? DBNull.Value : workcenterlist[i]["Qty"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsWorkcenter.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsWorkcenter.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["Qty"] = Convert.IsDBNull(workcenterlist[i]["Qty"]) ? DBNull.Value : workcenterlist[i]["Qty"];
                        dr["isResidualApplicable"] = bplib.clsWebLib.GetBoolData(workcenterlist[i]["isResidualApplicable"]);
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsWorkcenter);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult DeleteType2Process(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from trn.ProductionOrderProcessSet where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        [HttpPost, Authorize]
        public ActionResult DeleteRunningWC(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [TRN].[RunningOrderType2WorkCenter] where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult DeletePreferenceWC(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [TRN].[ProductionOrderType2WorkCenter] where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost, Authorize]
        public JsonResult GetProductionPlanningData(string planrowid, string ProductionOrderId, string processid)
        {
            string SelectedProductionOrder = ProductionOrderId;
            //CONVERT(INT, pt.WorkCenterMasterId)
            if (string.IsNullOrEmpty(planrowid) == false)
            {
                DataTable dt = _sqlRepository.GetDataTable("select top 1 ProductionOrderID from ProductionPlanningType2  where id = '" + planrowid + @"'");
                if (dt.Rows.Count > 0)
                    ProductionOrderId = dt.Rows[0]["ProductionOrderID"].ToString();
            }

            string sqlWCDATA = @"SELECT WC.Sequence, t1.ProductionOrderID, t1.WorkCenterMasterId, wc.UserName AS WorkCenter,e.username as Entity,
							FORMAT(po.Lsd,'dd-MMM-yyyy') AS LSD,
                            FORMAT(po.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,
							DATEDIFF(DAY,po.LSD,MIN(t1.ProductionDate)) AS DIFF,
							case when DATEDIFF(DAY,po.CommitmentDate,MAX(t1.ProductionDate))>0 THEN  DATEDIFF(DAY,po.CommitmentDate,MAX(t1.ProductionDate)) ELSE NULL END AS DelayedProductionDaysOnCommitmentDate,
                            FORMAT(MIN(t1.ProductionDate),'dd-MMM-yyyy') AS FromDate,
                            FORMAT(MAX(t1.ProductionDate),'dd-MMM-yyyy') AS ToDate,
                            SUM(t1.Quantity) AS PlannedQuantity

                            FROM ProductionPlanningType2 T1
							LEFT OUTER JOIN ProductionOrderSchedulingParametersType2 AS po ON po.ID = t1.ProductionOrderID
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            WHERE t1.ProductionOrderID='" + ProductionOrderId + @"' --AND ProductionDate>=GETDATE()
                            GROUP BY  WC.Sequence,e.username,t1.ProductionOrderID, po.Lsd,po.CommitmentDate, t1.WorkCenterMasterId, wc.UserName
                            order by WC.Sequence
                            ";


            string sqlPRODDATA = @"SELECT  WC.Sequence,t1.SubProductionOrderId,t1.WorkCenterMasterId, wc.UserName AS WorkCenter,e.username as Entity,
                            FORMAT(MIN(t1.ProductionDate),'dd-MMM-yyyy') AS FromDate,
                            FORMAT(MAX(t1.ProductionDate),'dd-MMM-yyyy') AS ToDate,
                            SUM(t1.Quantity) AS ProductionQuantity

                            FROM 
                            trn.ProductionSummary T1
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            WHERE t1.SubProductionOrderId='" + ProductionOrderId + @"' AND WC.ProcessId='" + processid + @"' 
                            GROUP BY e.username, WC.Sequence,t1.SubProductionOrderId,t1.WorkCenterMasterId, wc.UserName
                            order by WC.Sequence
                            ";

            string sqlRowData = @"select WC.Sequence,T1.Id, mm.UserName AS Material,t1.WorkCenterMasterId,T1.ProductionOrderId,WC.entityid,wc.UserName AS WorkCenter,
    PM.UserName AS ProductName,PD.ProductMasterId,T1.Quantity,T1.ProductionHours,e.username as Entity,
    FORMAT(t1.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,SO.Qty AS TotalQuantity,mm.[Image] AS MaterialImage
FROM ProductionPlanningType2 T1
LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
left join org.entity e on e.id=wc.entityid
LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t1.MaterialMasterId
LEFT OUTER JOIN [TRN].[ProductDefinition] PD ON mm.Id=pd.MaterialMasterId
LEFT OUTER JOIN [MST].[ProductMaster] PM on pm.ID=PD.ProductMasterId
LEFT OUTER  JOIN (SELECT T2.ID SubProductionOrderId,SUM(so.Qty) AS Qty
                    FROM trn.SalesOrder AS so
INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
INNER JOIN ProductionOrderSchedulingParametersType2 T2 ON T2.ProductionOrderId=pod.ProductionOrderId
GROUP BY T2.ID) 
AS SO ON so.SubProductionOrderId=T1.ProductionOrderId
                            WHERE t1.ID='" + planrowid + @"' 
                            order by WC.Sequence";



            string sqlPRDATA = @"select T1.*,upper(ps.username) AS PlanningStatus,PO.PicFileName from ProductionOrderSchedulingParametersType2 T1
                                inner join trn.productionorder po on po.id=t1.productionorderID
                                 LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            where T1.ID = '" + ProductionOrderId + @"'";

            string sqlSTYLEDATA = @"  SELECT distinct moi.BuyerReferenceNo
            FROM trn.ProductionOrderDetail AS pod
            INNER JOIN ProductionOrderSchedulingParametersType2 T2 ON T2.ProductionOrderId=pod.ProductionOrderId
          LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
          LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
          WHERE isnull(moi.BuyerReferenceNo,'')<>''
          AND T2.Id='" + ProductionOrderId + @"'";

            Library.Planning.PlanningType1.PlanningType1Scheduler scheduler = new Library.Planning.PlanningType1.PlanningType1Scheduler();
            string SameDayPlanningData = scheduler.GetSameDayPlanningType2Summary(planrowid, SelectedProductionOrder);

            return Json(new
            {
                WCDATA = _sqlRepository.GetDataCollection(sqlWCDATA),
                WPRODDATA = _sqlRepository.GetDataCollection(sqlPRODDATA),
                WSTYLEDATA = _sqlRepository.GetDataCollection(sqlSTYLEDATA),
                SAMEDAYDATA = _sqlRepository.GetDataCollection(SameDayPlanningData),

                ROWDATA = _sqlRepository.GetDataCollection(sqlRowData),
                PRDATA = _sqlRepository.GetDataCollection(sqlPRDATA)
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SKURegistration

        [HttpGet, Authorize]
        public ActionResult GetSalesOrderFilterData()
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetSalesOrderFilterData(), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetPacketRegistrationMasterList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select M.*,E.EmployeeName from PacketRegistrationMaster M
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.EmployeeId) AS TEMP WHERE " + strkey + " order by UserName";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreatePacketRegistrationMaster(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from PacketRegistrationMaster where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("UserName already exists!!!");
                con.OpenDataSetThroughAdapter("select * from PacketRegistrationMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PacketRegistrationMaster", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Error = false, Data = _Id, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public ActionResult DeletePRM(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from PacketRegistrationMaster where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetSRSalesOrderListSearch(string column, string value, string packetRegistrationMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string activeStatus = "";
            string plantSql = @"select * from scs.PlantConfig where plantid='" + identity.PlantId + "'";
            DataTable dtPlantConfig = _sqlRepository.GetDataTable(plantSql);
            if (dtPlantConfig.Rows.Count > 0)
                if (bplib.clsWebLib.GetBoolData(dtPlantConfig.Rows[0]["IsProductionOrderCreatedAfterConfirmationOfSO"].ToString()))
                    activeStatus = " AND isnull(SO.IsConfirm,0)=1 ";


            string sql = @"SELECT 
                             MO.Type,isnull(moi.Consignment,0) AS Consignment,
                             CASE WHEN ISNULL(eout.Id,'')<>'' OR ISNULL(TOUT.Id,'')<>'' THEN CONCAT(POWN.UserName,'(',EOWN.UserName,')') ELSE '' END AS OrderOwner
                            ,TEMP.* FROM (SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Checked,null AS Id,null AS PacketRegistrationMasterId
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
                       FROM [TRN].[SalesOrder] AS SO 
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
                       LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)

                       WHERE   
                       (
                            --if there is no jobwork, i can create my own production order
                       	    (ISNULL(moi.JobWorkType,'')='' AND MO.PlantId='" + identity.PlantId + @"' )
                                OR 
                       	    (ISNULL(moi.JobWorkType,'')<>'' AND EOUT.PlantId='" + identity.PlantId + @"' )
                       )
                       AND (OS.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' " + activeStatus + @" and  SO.Id not IN (SELECT DISTINCT SalesOrderId FROM dbo.PacketRegistrationDetail)) AND MOI.ArticleId<>''
                        
						UNION
						
						SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Checked,POD.Id,POD.PacketRegistrationMasterId
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
                       FROM [TRN].[SalesOrder] AS SO 
                        left outer join dbo.PacketRegistrationDetail POD on POD.SalesOrderId=SO.Id and POD.PacketRegistrationMasterId='" + packetRegistrationMasterId + @"'
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
                       WHERE  SO.Id IN (SELECT DISTINCT SalesOrderId FROM dbo.PacketRegistrationDetail WHERE PacketRegistrationMasterId='" + packetRegistrationMasterId + @"')						
						) AS TEMP 
                            LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON TEMP.MasterOrderItemId=MOI.Id
                            LEFT JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
							LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

WHERE " + strkey + "  and MO.PlantId='" + identity.PlantId + @"' AND  TEMP.SalesOrderId IN(Select SalesOrderId from TRN.SecondCharacteristics) ORDER BY  TEMP.ProductionGrouping,TEMP.MaterialMasterId,TEMP.ArticleId";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult SavePacketRegistrationDetail(List<Dictionary<string, object>> details, string packetRegistrationMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsSO = null;
                string sql = "SELECT * FROM dbo.PacketRegistrationDetail where packetRegistrationMasterId=" + packetRegistrationMasterId + "";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsSO, false, "1");

                string SystemID = "";
                for (int i = 0; i < details.Count; i++)
                {
                    dsSO.Tables[0].DefaultView.RowFilter = "SalesOrderId='" + details[i]["SalesOrderId"] + "'";
                    if (dsSO.Tables[0].DefaultView.Count == 0)
                    {

                        DataRow dr = dsSO.Tables[0].NewRow();

                        dr["Id"] = packetRegistrationMasterId + "-" + (i + 1).ToString();
                        dr["PacketRegistrationMasterId"] = packetRegistrationMasterId;
                        dr["SalesOrderId"] = details[i]["SalesOrderId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsSO.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsSO);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult DeleteSO(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from PacketRegistrationDetail where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPacketRegistrationTypeList(string masterId)
        {
            try
            {
                string sql = @"DECLARE @PacketRegistrationMasterId varchar(30) = '" + masterId + @"';
WITH PackingCategory AS
(
    SELECT 1 AS SortOrder, 'Individual' AS PackingCategory
    UNION ALL
    SELECT 2, 'Prepack'
    UNION ALL
    SELECT 3, 'Bulk Pack'
    UNION ALL
    SELECT 4, 'CartonPack'
)
SELECT
    Flag = CAST(CASE WHEN D.Id IS NULL THEN 0 ELSE 1 END AS bit),P.PackingCategory,D.Id,D.PacketRegistrationMasterId,D.PackingTypeId,
    NoOfUnitPerPack =CASE WHEN D.Id IS NULL AND P.PackingCategory = 'Individual' THEN 1 ELSE D.NoOfUnitPerPack END,
    NoOfPack =(SO.Qty * CM.PlanPercentage / 100) + SO.Qty,D.AddedBy,D.AddedDate,D.AddedFromIP,D.UpdatedBy,D.UpdatedDate,D.UpdatedFromIP,PT.PackingType
    ,LineItemReference = COALESCE(D.LineItemReference,CM.LineItemReference,MOI.BuyerReferenceNo)
FROM PackingCategory P
LEFT JOIN PacketRegistrationType D ON D.PackingCategory = P.PackingCategory AND D.PacketRegistrationMasterId = @PacketRegistrationMasterId
LEFT JOIN PacketRegistrationDetail PD ON PD.PacketRegistrationMasterId = @PacketRegistrationMasterId
LEFT JOIN PacketRegistrationMaster CM ON CM.Id = PD.PacketRegistrationMasterId
LEFT JOIN TRN.SalesOrder SO ON SO.Id = PD.SalesOrderId
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId
LEFT JOIN HKP.PackingType PT ON PT.Id=D.PackingTypeId
ORDER BY P.SortOrder;";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SavePackingCategory(List<Dictionary<string, object>> packCatlist, string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dspackCat = null;
                string sql = "SELECT * FROM [dbo].[PacketRegistrationType] where PacketRegistrationMasterId='" + masterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dspackCat, false, "1");

                for (int i = 0; i < packCatlist.Count; i++)
                {
                    dspackCat.Tables[0].DefaultView.RowFilter = "Id='" + packCatlist[i]["Id"] + "'";
                    if (dspackCat.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dspackCat.Tables[0].NewRow();

                        dr["Id"] = masterId + "-" + (i + 1).ToString();
                        dr["PacketRegistrationMasterId"] = masterId;
                        dr["PackingTypeId"] = packCatlist[i]["PackingTypeId"];
                        dr["PackingCategory"] = packCatlist[i]["PackingCategory"];
                        dr["NoOfUnitPerPack"] = packCatlist[i]["NoOfUnitPerPack"];
                        dr["NoOfPack"] = packCatlist[i]["NoOfPack"];
                        dr["LineItemReference"] = packCatlist[i]["LineItemReference"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;


                        dspackCat.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dspackCat.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["PackingTypeId"] = packCatlist[i]["PackingTypeId"];
                        dr["NoOfUnitPerPack"] = packCatlist[i]["NoOfUnitPerPack"];
                        dr["NoOfPack"] = packCatlist[i]["NoOfPack"];
                        dr["LineItemReference"] = packCatlist[i]["LineItemReference"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dspackCat);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SavePacketRegistration(List<Dictionary<string, object>> packregilist, string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dspackCat = null;
                string sql = "SELECT * FROM [dbo].[PacketRegistration] where PacketRegistrationTypeId='" + masterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dspackCat, false, "1");

                for (int i = 0; i < packregilist.Count; i++)
                {
                    dspackCat.Tables[0].DefaultView.RowFilter = "Id='" + packregilist[i]["Id"] + "'";
                    if (dspackCat.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dspackCat.Tables[0].NewRow();

                        dr["Id"] = masterId + "-" + (i + 1).ToString();
                        dr["PacketRegistrationTypeId"] = masterId;
                        dr["SalesOrderId"] = packregilist[i]["SalesOrderId"];
                        dr["SKU1Id"] = packregilist[i]["SKU1Id"];
                        dr["SKU2Id"] = packregilist[i]["SKU2Id"];
                        dr["UnitPerPack"] = packregilist[i]["UnitPerPack"] == null || packregilist[i]["UnitPerPack"] == DBNull.Value ? DBNull.Value : packregilist[i]["UnitPerPack"];
                        dr["NoOfUnit"] = packregilist[i]["NoOfUnit"];
                        //if (packregilist[i]["UnitPerPack"] != null)
                        //{
                        //    dr["NoOfPack"] = Convert.ToDecimal(packregilist[i]["NoOfUnit"]) / Convert.ToDecimal(packregilist[i]["UnitPerPack"] == null || packregilist[i]["UnitPerPack"] == DBNull.Value ? DBNull.Value : packregilist[i]["UnitPerPack"]);
                        //}
                        dr["NoOfPack"] = packregilist[i]["NoOfPack"];
                        dr["LineItemReference"] = packregilist[i]["LineItemReference"];
                        dr["ComboRefNo"] = packregilist[i]["ComboRefNo"];
                        dr["BarCode"] = packregilist[i]["BarCode"];
                        dr["QRCode"] = packregilist[i]["QRCode"];
                        dr["RFID"] = packregilist[i]["RFID"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;


                        dspackCat.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dspackCat.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["UnitPerPack"] = packregilist[i]["UnitPerPack"] == null || packregilist[i]["UnitPerPack"] == DBNull.Value ? DBNull.Value : packregilist[i]["UnitPerPack"];
                        dr["NoOfUnit"] = packregilist[i]["NoOfUnit"];
                        //if (packregilist[i]["UnitPerPack"] != null)
                        //{
                        //    dr["NoOfPack"] = Convert.ToDecimal(packregilist[i]["NoOfUnit"]) / Convert.ToDecimal(packregilist[i]["UnitPerPack"] == null || packregilist[i]["UnitPerPack"] == DBNull.Value ? DBNull.Value : packregilist[i]["UnitPerPack"]);
                        //}
                        dr["NoOfPack"] = packregilist[i]["NoOfPack"];
                        dr["ComboRefNo"] = packregilist[i]["ComboRefNo"];
                        dr["BarCode"] = packregilist[i]["BarCode"];
                        dr["QRCode"] = packregilist[i]["QRCode"];
                        dr["RFID"] = packregilist[i]["RFID"];
                        dr["LineItemReference"] = packregilist[i]["LineItemReference"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dspackCat);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveComboPacketRegistration(Dictionary<string, object> packrdata, List<Dictionary<string, object>> packregilist, string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dspcsku, dspcr, dsPCRID = null;
                objCon = new ConnectionManager.DAL.ConManager("1");

                string pcr = @"select * from PackingComboReference Where PacketRegistrationTypeId='" + masterId + "'";
                objCon.OpenDataSetThroughAdapter(pcr, out dspcr, false, "1");

                objCon.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[PackingComboReference] where PacketRegistrationTypeId='" + masterId + "'", out dsPCRID, false, "1");
                int pcount = Convert.ToInt32(dsPCRID.Tables[0].Rows[0]["countId"].ToString());

                string pcsku = @"select * from PackingComboSKUDetail Where PackingComboReferenceId='" + packrdata["Id"] + "'";
                objCon.OpenDataSetThroughAdapter(pcsku, out dspcsku, false, "1");


                string prmasterId = null;

                dspcr.Tables[0].DefaultView.RowFilter = "PacketRegistrationTypeId ='" + masterId + "' AND Id ='" + packrdata["Id"] + "'";
                if (dspcr.Tables[0].DefaultView.Count == 0)
                {
                    DataRow dr = dspcr.Tables[0].NewRow();

                    dr["Id"] = masterId + "-" + (pcount + 1).ToString();
                    prmasterId = masterId + "-" + (pcount + 1).ToString();
                    dr["PacketRegistrationTypeId"] = masterId;

                    dr["ColorSizeQty"] = packrdata["ColorSizeQty"];
                    dr["NoOfPack"] = packrdata["NoOfPack"];
                    dr["ComboRefNo"] = packrdata["ComboRefNo"];
                    dr["PackRefQty"] = packrdata["PackRefQty"];
                    dr["ComboQty"] = packrdata["ComboQty"];
                    dr["BarCode"] = packrdata["BarCode"];
                    dr["QRCode"] = packrdata["QRCode"];
                    dr["RFID"] = packrdata["RFID"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;


                    dspcr.Tables[0].Rows.Add(dr);
                }

                for (int i = 0; i < packregilist.Count; i++)
                {

                    dspcsku.Tables[0].DefaultView.RowFilter = "Id =''";
                    if (dspcsku.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow drc = dspcsku.Tables[0].NewRow();

                        drc["PackingComboReferenceId"] = prmasterId;
                        drc["SKU1Id"] = packregilist[i]["SKU1Id"];
                        drc["SKU2Id"] = packregilist[i]["SKU2Id"];
                        drc["QtyPerPack"] = packregilist[i]["UnitPerPack"];
                        drc["NoOfPack"] = packregilist[i]["PlanPack"];
                        drc["ComboQty"] = Convert.ToInt32(packregilist[i]["UnitPerPack"]) * Convert.ToInt32(packregilist[i]["PlanPack"]);

                        drc["AddedBy"] = identity.Name;
                        drc["AddedDate"] = System.DateTime.Now.ToString();
                        drc["AddedFromIP"] = identity.IPAddress;


                        dspcsku.Tables[0].Rows.Add(drc);
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dspcr, dspcsku);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetComboData(string packetRegistrationTypeId)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetComboData(packetRegistrationTypeId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpGet, Authorize]
        public ActionResult GetComboRefNo(string comboNo, string packetRegistrationTypeId)
        {
            try
            {
                string sql = "";
                if (!string.IsNullOrEmpty(comboNo))
                {
                    sql = "select * from PackingComboReference Where ComboRefNo=" + comboNo + " AND PacketRegistrationTypeId='" + packetRegistrationTypeId + "'";
                }
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetComboPackingSKUData(string soId, string packetRegistrationTypeId)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetComboPackingSKUData(soId, packetRegistrationTypeId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetPackingResigtationSKUData(string soId, string packetRegistrationTypeId)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetPackingResigtationSKUData(soId, packetRegistrationTypeId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetCartonList(string masterId)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetCartonList(masterId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetComboCartonList(string masterId)
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetComboCartonList(masterId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public JsonResult GenerateCarton(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dspackCat = null;
                string sql = "SELECT * FROM [dbo].[CartonGeneration] where PacketRegistrationId='" + data["PacketRegistrationId"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dspackCat, false, "1");

                int totalCartons = Convert.ToInt32(data["NoOfPack"]);
                int existingCartons = dspackCat.Tables[0].AsEnumerable().Count(r => r["PacketRegistrationId"].ToString() == data["PacketRegistrationId"].ToString());

                if (existingCartons < totalCartons)
                {
                    for (int i = existingCartons; i < totalCartons; i++)
                    {
                        DataRow dr = dspackCat.Tables[0].NewRow();

                        dr["Id"] = data["PacketRegistrationId"] + "-" + (i + 1);
                        dr["PacketRegistrationId"] = data["PacketRegistrationId"];
                        dr["NoOfPcs"] = data["NoOfPcs"];
                        dr["CartonNo"] = i + 1;
                        dr["Code"] = data["PacketRegistrationId"] + "-" + (i + 1);

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dspackCat.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dspackCat);

                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult GenerateComboCarton(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dspackCat = null;
                string sql = "SELECT * FROM [dbo].[CartonGeneration] where PackingComboReferenceId='" + data["PackingComboReferenceId"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dspackCat, false, "1");

                int totalCartons = Convert.ToInt32(data["NoOfPack"]);
                int existingCartons = dspackCat.Tables[0].AsEnumerable().Count(r => r["PackingComboReferenceId"].ToString() == data["PackingComboReferenceId"].ToString());

                if (existingCartons < totalCartons)
                {
                    for (int i = existingCartons; i < totalCartons; i++)
                    {
                        DataRow dr = dspackCat.Tables[0].NewRow();

                        dr["Id"] = data["PackingComboReferenceId"] + "-" + (i + 1);
                        dr["PackingComboReferenceId"] = data["PackingComboReferenceId"];
                        dr["NoOfPcs"] = data["NoOfPcs"];
                        dr["CartonNo"] = i + 1;
                        dr["Code"] = data["PackingComboReferenceId"] + "-" + (i + 1);

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dspackCat.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dspackCat);

                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private List<QRCodeItem> qrCodeItems = new List<QRCodeItem>();
        private int currentQRCodeIndex = 0;
          
        #endregion

        #region PackerCategory

        string TableName = "HKP.PackerCategory";

        [Authorize, HttpGet]
        public JsonResult GetPackerCategoryCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM HKP.PackerCategory"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPackerCategoryList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPackerCategoryAutoSequence()
        {
            return Json(GetPackerCategorySequence(), JsonRequestBehavior.AllowGet);
        }
        private double GetPackerCategorySequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        [HttpPost]
        public JsonResult CreatePackerCategory(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetPackerCategorySequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeletePackerCategory(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetPackerCategorySequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion

        #region QRCode
      
        [Authorize]
        public ActionResult GenerateQRCode(Dictionary<string, object> data)
        {
            try
            {
                string packetRegistrationId = Convert.ToString(data["PacketRegistrationId"]);

                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                DataTable dt = order.GetQRCodeData(packetRegistrationId);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new { Error = true, Message = "No carton data found." });
                }

                List<QRCodeItem> qrCodeItems = dt.AsEnumerable().Select(row => new QRCodeItem
                {
                    PackingName = Convert.ToString(row["PackingName"]),
                    Customer = Convert.ToString(row["Customer"]),
                    SOId = Convert.ToString(row["SOId"]),
                    Color = Convert.ToString(row["Color"]),
                    Size = Convert.ToString(row["Size"]),
                    NoOfPcs = Convert.ToString(row["NoOfPcs"]),
                    CartonNo = Convert.ToString(row["CartonNo"])
                }).ToList();

                byte[] pdfBytes = BuildQRCodePdf(qrCodeItems);

                // -----------------------------------------
                // Save to a temp folder on the server, same
                // way your other download-generating actions do
                // -----------------------------------------
                //string fileName = $"QRCode_{packetRegistrationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string fileName = $"QRCode_{packetRegistrationId}.pdf";
                //string folderPath = Server.MapPath("~/POPResources/QRCode"); // adjust to whatever folder your downloadgriddataUrlPath action already reads from
                //if (!Directory.Exists(folderPath))
                //{
                //    Directory.CreateDirectory(folderPath);
                //}
                // string fullPath = Path.Combine(folderPath, fileName);
                //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~/") + fileName);
                System.IO.File.WriteAllBytes(fullPath, pdfBytes);

                return Json(new
                {
                    Error = false,
                    Message = "QR Code generated successfully.",
                    FileName = fullPath   // matches response.data.FileName your JS already expects
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize]
        public ActionResult GenerateComboQRCode(Dictionary<string, object> data)
        {
            try
            {
                string Id = Convert.ToString(data["Id"]);

                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                DataTable dt = order.GetComboQRCodeData(Id);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new { Error = true, Message = "No carton data found." });
                }

                List<QRCodeItem> qrCodeItems = dt.AsEnumerable().Select(row => new QRCodeItem
                {
                    PackingName = Convert.ToString(row["PackingName"]),
                    Customer = Convert.ToString(row["Customer"]),
                    ColorSizeQty = Convert.ToString(row["ColorSizeQty"]),
                    NoOfPcs = Convert.ToString(row["NoOfPcs"]),
                    CartonNo = Convert.ToString(row["CartonNo"])
                }).ToList();

                byte[] pdfBytes = BuildComboQRCodePdf(qrCodeItems);

                // -----------------------------------------
                // Save to a temp folder on the server, same
                // way your other download-generating actions do
                // -----------------------------------------
                //string fileName = $"QRCode_{packetRegistrationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string fileName = $"QRCode_{Id}.pdf";
               
                //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~/") + fileName);
                System.IO.File.WriteAllBytes(fullPath, pdfBytes);

                return Json(new
                {
                    Error = false,
                    Message = "QR Code generated successfully.",
                    FileName = fullPath   // matches response.data.FileName your JS already expects
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        private byte[] BuildQRCodePdf(List<QRCodeItem> qrCodeItems)
        {
            //const float pageWidth = 4f * 72f;    // 288pt (4in wide)
            //const float cellPadding = 4f;        // small inner padding, not a margin
            //const float qrSize = 95f;            // was 110f — a bit smaller
            //const float textFontSize = 5f;
            //const float textLineHeight = textFontSize + 3f;

            const float cellPadding = 4f;
            const float qrSize = 95f;
            const float textFontSize = 8f;
            const float textLineHeight = textFontSize + 3f;
            float pageWidth = cellPadding + qrSize + cellPadding;   // content-sized width

            #region QRCodePerPage
            //using (PdfDocument document = new PdfDocument())
            //{
            //    const float cellPadding = 4f;
            //    const float qrSize = 95f;
            //    const float textFontSize = 8f;
            //    const float textLineHeight = textFontSize + 3f;
            //    const int reservedLineCount = 3;   // fixed — matches your physical label height

            //    float pageWidth = cellPadding + qrSize + cellPadding;

            //    // ---- fixed height, same for every page, matches the physical sticker size ----
            //    float pageHeight =
            //        cellPadding + qrSize + 3f + (textLineHeight * reservedLineCount) + cellPadding;

            //    PdfFont font = new PdfStandardFont(PdfFontFamily.Courier, textFontSize, PdfFontStyle.Bold);
            //    PdfBrush brush = PdfBrushes.Black;

            //    PdfStringFormat leftFormat = new PdfStringFormat
            //    {
            //        Alignment = PdfTextAlignment.Left,
            //        LineAlignment = PdfVerticalAlignment.Top
            //    };

            //    float textAreaWidth = pageWidth - (cellPadding * 2);

            //    foreach (QRCodeItem item in qrCodeItems)
            //    {
            //        string qrData = string.Concat(item.CartonNo);

            //        string[] rawLines =
            //            {
            //    $"{item.PackingName}",
            //    $"{item.Color}_{item.Size}",
            //    $"{item.NoOfPcs}_{item.CartonNo}"
            //    };

            //        List<string> wrappedLines = new List<string>();
            //        foreach (string raw in rawLines)
            //        {
            //            wrappedLines.AddRange(WrapTextToWidth(raw, font, textAreaWidth));
            //        }

            //        // If this item genuinely needs MORE lines than reserved, it will still overflow —
            //        // that's a data problem (label text too long for the sticker), not a layout bug.
            //        // Consider truncating or shrinking font for these specific outliers if it happens often.

            //        PdfSection section = document.Sections.Add();
            //        section.PageSettings.Size = new SizeF(pageWidth, pageHeight);   // same every time
            //        section.PageSettings.Margins.All = 0;

            //        PdfPage page = section.Pages.Add();
            //        PdfGraphics g = page.Graphics;

            //        CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;

            //        using (Image barcodeImg = qrCode.Draw(qrData, 200, 2))
            //        using (MemoryStream imgStream = new MemoryStream())
            //        {
            //            barcodeImg.Save(imgStream, ImageFormat.Png);
            //            imgStream.Position = 0;

            //            PdfBitmap pdfImage = new PdfBitmap(imgStream);

            //            float qrX = cellPadding;
            //            float qrY = cellPadding;
            //            g.DrawImage(pdfImage, qrX, qrY, qrSize, qrSize);

            //            float textAreaX = cellPadding;
            //            float textY = qrY + qrSize + 3f;

            //            for (int li = 0; li < wrappedLines.Count; li++)
            //            {
            //                RectangleF textRect = new RectangleF(
            //                    textAreaX, textY + li * textLineHeight, textAreaWidth, textLineHeight);

            //                g.DrawString(wrappedLines[li], font, brush, textRect, leftFormat);
            //            }
            //        }
            //    }

            //    using (MemoryStream output = new MemoryStream())
            //    {
            //        document.Save(output);
            //        return output.ToArray();
            //    }
            //} 
            #endregion

            using (PdfDocument document = new PdfDocument())
            {

                PdfFont font = new PdfStandardFont(PdfFontFamily.Courier, textFontSize, PdfFontStyle.Bold);
                PdfBrush brush = PdfBrushes.Black;

                PdfStringFormat leftFormat = new PdfStringFormat
                {
                    Alignment = PdfTextAlignment.Left,
                    LineAlignment = PdfVerticalAlignment.Top
                };

                float textAreaWidth = pageWidth - (cellPadding * 2);   // now == qrSize

                for (int pairStart = 0; pairStart < qrCodeItems.Count; pairStart += 2)
                {
                    List<QRCodeItem> pair = qrCodeItems.Skip(pairStart).Take(2).ToList();

                    List<List<string>> wrappedLinesPerItem = new List<List<string>>();
                    List<float> blockHeights = new List<float>();

                    foreach (QRCodeItem item in pair)
                    {
                        string[] rawLines =
                        {
                $"{item.PackingName}",
                $"{item.Color}_{item.Size}",
                $"{item.NoOfPcs}_{item.CartonNo}",
                $""
            };

                        List<string> wrappedLines = new List<string>();
                        foreach (string raw in rawLines)
                        {
                            wrappedLines.AddRange(WrapTextToWidth(raw, font, textAreaWidth));
                        }
                        wrappedLinesPerItem.Add(wrappedLines);

                        float blockHeight =
                            cellPadding + qrSize + 3f + (textLineHeight * wrappedLines.Count) + cellPadding;

                        blockHeights.Add(blockHeight);
                    }

                    float pageHeight = blockHeights.Sum();

                    PdfSection section = document.Sections.Add();
                    section.PageSettings.Size = new SizeF(pageWidth, pageHeight);
                    section.PageSettings.Margins.All = 0;

                    PdfPage page = section.Pages.Add();
                    PdfGraphics g = page.Graphics;

                    float cursorY = 0f;

                    for (int p = 0; p < pair.Count; p++)
                    {
                        QRCodeItem item = pair[p];
                        List<string> wrappedLines = wrappedLinesPerItem[p];

                        string qrData = string.Concat(item.CartonNo);

                        CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;

                        using (Image barcodeImg = qrCode.Draw(qrData, 200, 2))
                        using (MemoryStream imgStream = new MemoryStream())
                        {
                            barcodeImg.Save(imgStream, ImageFormat.Png);
                            imgStream.Position = 0;

                            PdfBitmap pdfImage = new PdfBitmap(imgStream);

                            float qrX = cellPadding;
                            float qrY = cursorY + cellPadding;
                            g.DrawImage(pdfImage, qrX, qrY, qrSize, qrSize);

                            float textAreaX = cellPadding;
                            float textY = qrY + qrSize + 3f;

                            for (int li = 0; li < wrappedLines.Count; li++)
                            {
                                RectangleF textRect = new RectangleF(
                                    textAreaX, textY + li * textLineHeight, textAreaWidth, textLineHeight);

                                g.DrawString(wrappedLines[li], font, brush, textRect, leftFormat);
                            }
                        }

                        cursorY += blockHeights[p];
                    }
                }

                using (MemoryStream output = new MemoryStream())
                {
                    document.Save(output);
                    return output.ToArray();
                }
            }


        }

        private byte[] BuildComboQRCodePdf(List<QRCodeItem> qrCodeItems)
        {

            const float cellPadding = 4f;
            const float qrSize = 95f;
            const float textFontSize = 8f;
            const float textLineHeight = textFontSize + 3f;
            float pageWidth = cellPadding + qrSize + cellPadding;   // content-sized width
                       

            using (PdfDocument document = new PdfDocument())
            {

                PdfFont font = new PdfStandardFont(PdfFontFamily.Courier, textFontSize, PdfFontStyle.Bold);
                PdfBrush brush = PdfBrushes.Black;

                PdfStringFormat leftFormat = new PdfStringFormat
                {
                    Alignment = PdfTextAlignment.Left,
                    LineAlignment = PdfVerticalAlignment.Top
                };

                float textAreaWidth = pageWidth - (cellPadding * 2);   // now == qrSize

                for (int pairStart = 0; pairStart < qrCodeItems.Count; pairStart += 2)
                {
                    List<QRCodeItem> pair = qrCodeItems.Skip(pairStart).Take(2).ToList();

                    List<List<string>> wrappedLinesPerItem = new List<List<string>>();
                    List<float> blockHeights = new List<float>();

                    foreach (QRCodeItem item in pair)
                    {
                        string[] rawLines =
                        {
                $"{item.PackingName}",
                $"{item.ColorSizeQty}",
                $"{item.NoOfPcs}_{item.CartonNo}",
                $""
            };

                        List<string> wrappedLines = new List<string>();
                        foreach (string raw in rawLines)
                        {
                            wrappedLines.AddRange(WrapTextToWidth(raw, font, textAreaWidth));
                        }
                        wrappedLinesPerItem.Add(wrappedLines);

                        float blockHeight =
                            cellPadding + qrSize + 3f + (textLineHeight * wrappedLines.Count) + cellPadding;

                        blockHeights.Add(blockHeight);
                    }

                    float pageHeight = blockHeights.Sum();

                    PdfSection section = document.Sections.Add();
                    section.PageSettings.Size = new SizeF(pageWidth, pageHeight);
                    section.PageSettings.Margins.All = 0;

                    PdfPage page = section.Pages.Add();
                    PdfGraphics g = page.Graphics;

                    float cursorY = 0f;

                    for (int p = 0; p < pair.Count; p++)
                    {
                        QRCodeItem item = pair[p];
                        List<string> wrappedLines = wrappedLinesPerItem[p];

                        string qrData = string.Concat(item.CartonNo);

                        CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;

                        using (Image barcodeImg = qrCode.Draw(qrData, 200, 2))
                        using (MemoryStream imgStream = new MemoryStream())
                        {
                            barcodeImg.Save(imgStream, ImageFormat.Png);
                            imgStream.Position = 0;

                            PdfBitmap pdfImage = new PdfBitmap(imgStream);

                            float qrX = cellPadding;
                            float qrY = cursorY + cellPadding;
                            g.DrawImage(pdfImage, qrX, qrY, qrSize, qrSize);

                            float textAreaX = cellPadding;
                            float textY = qrY + qrSize + 3f;

                            for (int li = 0; li < wrappedLines.Count; li++)
                            {
                                RectangleF textRect = new RectangleF(
                                    textAreaX, textY + li * textLineHeight, textAreaWidth, textLineHeight);

                                g.DrawString(wrappedLines[li], font, brush, textRect, leftFormat);
                            }
                        }

                        cursorY += blockHeights[p];
                    }
                }

                using (MemoryStream output = new MemoryStream())
                {
                    document.Save(output);
                    return output.ToArray();
                }
            }


        }

        private List<string> WrapTextToWidth(string text, PdfFont font, float maxWidth)
        {
            List<string> lines = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                lines.Add(string.Empty);
                return lines;
            }

            string[] words = text.Split(' ');
            string currentLine = string.Empty;

            foreach (string word in words)
            {
                string candidate = currentLine.Length == 0 ? word : currentLine + " " + word;
                SizeF size = font.MeasureString(candidate);

                if (size.Width <= maxWidth || currentLine.Length == 0)
                {
                    currentLine = candidate;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine);
            }

            return lines;
        }

        private static string EscapePdfText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        #endregion

    }


}



public class QRCodeItem
{
    public string PackingName { get; set; }
    public string ColorSizeQty { get; set; }
    public string Customer { get; set; }
    public string SOId { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
    public string NoOfPcs { get; set; }
    public string CartonNo { get; set; }
}

// =====================================================================
// Minimal hand-written PDF builder — no external package required.
// Supports: multiple pages, JPEG images (DCTDecode), vector rectangles,
// and text in the standard Courier-Bold font.
// =====================================================================
public class MinimalPdfBuilder
{
    private readonly List<byte[]> _objects = new List<byte[]>();
    private readonly List<int> _pageObjNums = new List<int>();
    private readonly double _pageWidth;
    private readonly double _pageHeight;

    private int _catalogObj;
    private int _pagesObj;
    private int _fontObj;

    public MinimalPdfBuilder(double pageWidthPt, double pageHeightPt)
    {
        _pageWidth = pageWidthPt;
        _pageHeight = pageHeightPt;

        _objects.Add(null);              // placeholder for Catalog (obj 1)
        _objects.Add(null);              // placeholder for Pages   (obj 2)
        _catalogObj = 1;
        _pagesObj = 2;

        _fontObj = AddRaw("<< /Type /Font /Subtype /Type1 /BaseFont /Courier-Bold >>");
    }

    private int AddRaw(string dictContent)
    {
        _objects.Add(Encoding.ASCII.GetBytes(dictContent));
        return _objects.Count;
    }


    public int AddJpegImage(byte[] jpegBytes, int widthPx, int heightPx)
    {
        string dict = string.Format(CultureInfo.InvariantCulture,
            "<< /Type /XObject /Subtype /Image /Width {0} /Height {1} " +
            "/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {2} >>\nstream\n",
            widthPx, heightPx, jpegBytes.Length);

        var head = Encoding.ASCII.GetBytes(dict);
        var tail = Encoding.ASCII.GetBytes("\nendstream");
        var combined = new byte[head.Length + jpegBytes.Length + tail.Length];
        Buffer.BlockCopy(head, 0, combined, 0, head.Length);
        Buffer.BlockCopy(jpegBytes, 0, combined, head.Length, jpegBytes.Length);
        Buffer.BlockCopy(tail, 0, combined, head.Length + jpegBytes.Length, tail.Length);

        _objects.Add(combined);
        return _objects.Count;
    }

    public int AddPage(string contentStream, Dictionary<string, int> imageResources)
    {
        var contentBytes = Encoding.ASCII.GetBytes(contentStream);
        var streamDict = string.Format(CultureInfo.InvariantCulture,
            "<< /Length {0} >>\nstream\n", contentBytes.Length);

        var head = Encoding.ASCII.GetBytes(streamDict);
        var tail = Encoding.ASCII.GetBytes("\nendstream");
        var combined = new byte[head.Length + contentBytes.Length + tail.Length];
        Buffer.BlockCopy(head, 0, combined, 0, head.Length);
        Buffer.BlockCopy(contentBytes, 0, combined, head.Length, contentBytes.Length);
        Buffer.BlockCopy(tail, 0, combined, head.Length + contentBytes.Length, tail.Length);

        _objects.Add(combined);
        int contentObjNum = _objects.Count;

        var xObjEntries = string.Join(" ", imageResources.Select(kv => $"/{kv.Key} {kv.Value} 0 R"));

        string pageDict = string.Format(CultureInfo.InvariantCulture,
            "<< /Type /Page /Parent {0} 0 R /MediaBox [0 0 {1} {2}] " +
            "/Resources << /Font << /F1 {3} 0 R >> /XObject << {4} >> >> " +
            "/Contents {5} 0 R >>",
            _pagesObj, _pageWidth, _pageHeight, _fontObj, xObjEntries, contentObjNum);

        _objects.Add(Encoding.ASCII.GetBytes(pageDict));
        int pageObjNum = _objects.Count;
        _pageObjNums.Add(pageObjNum);
        return pageObjNum;
    }

    public byte[] Build()
    {
        var kids = string.Join(" ", _pageObjNums.Select(n => $"{n} 0 R"));
        _objects[_pagesObj - 1] = Encoding.ASCII.GetBytes(
            $"<< /Type /Pages /Kids [{kids}] /Count {_pageObjNums.Count} >>");
        _objects[_catalogObj - 1] = Encoding.ASCII.GetBytes(
            $"<< /Type /Catalog /Pages {_pagesObj} 0 R >>");

        using (var buffer = new MemoryStream())
        {
            void WriteAscii(string s)
            {
                var b = Encoding.ASCII.GetBytes(s);
                buffer.Write(b, 0, b.Length);
            }

            WriteAscii("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

            var offsets = new long[_objects.Count + 1]; // 1-based

            for (int i = 0; i < _objects.Count; i++)
            {
                offsets[i + 1] = buffer.Position;
                WriteAscii($"{i + 1} 0 obj\n");
                buffer.Write(_objects[i], 0, _objects[i].Length);
                WriteAscii("\nendobj\n");
            }

            long xrefStart = buffer.Position;
            WriteAscii($"xref\n0 {_objects.Count + 1}\n");
            WriteAscii("0000000000 65535 f \n");
            for (int i = 1; i <= _objects.Count; i++)
            {
                WriteAscii(offsets[i].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
            }

            WriteAscii($"trailer\n<< /Size {_objects.Count + 1} /Root {_catalogObj} 0 R >>\n");
            WriteAscii($"startxref\n{xrefStart}\n%%EOF");

            return buffer.ToArray();
        }
    }
}

public class _QRCodeItem
{
    public string PackingName { get; set; }
    public string Customer { get; set; }
    public string SOId { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
    public string NoOfPcs { get; set; }
    public string CartonNo { get; set; }
}


public class MultiCode
{
    public string Sequenc { get; set; }
    public string OperationCode { get; set; }
}
