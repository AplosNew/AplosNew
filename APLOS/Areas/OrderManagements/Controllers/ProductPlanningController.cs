using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductPlanningController : BaseController
    {
        #region Constructor

        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public ProductPlanningController(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
            _productionOrderService = productionOrderService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PO.*,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.PlantId='" + identity.PlantId + "'";

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
        public JsonResult GetProductionOrderProcessSetList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderProcessSetList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderEntityList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderEntityList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetWorkCenterList(string entityIds)
        //{
        //    return Json(_productionOrderService.GetWorkCenterList(new JavaScriptSerializer().Deserialize<string[]>(entityIds)), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderWorkCenterList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderWorkCenterList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Create(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
        //    , IEnumerable<ProductionOrderProcessSet> processSetlist
        //    , IEnumerable<ProductionOrderEntity> entitylist
        //    , IEnumerable<ProductionOrderWorkCenter> workcenterlist)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    master.PlantId = identity.PlantId;


        //    DataTable dtMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[ProductionOrder] where id='" + master.Id + "'");
        //    if (dtMaster.Rows.Count > 0)
        //        _productionOrderService.UpdateGraph(master, detaillist, processSetlist, entitylist, workcenterlist);
        //    else
        //        _productionOrderService.InsertGraph(master, detaillist, processSetlist, entitylist, workcenterlist);
        //    return Json(new { Message = AplosMessage.Insert });
        //}

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
            _productionOrderService.DeleteGraph(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult Menu()
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT 'FRAME-'+id AS id,UserName AS MenuText FROM [MMS].[MenuFrame]";
                var _dataM = _sqlRepository.GetDataCollection(sql);

                sql = @"SELECT 'GROUP-'+mg.id AS id,'FRAME-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.MenuFrame M
CROSS JOIN [MMS].[MenuGroup] MG
UNION ALL
SELECT 'SUBGROUP-'+mg.id AS id,'GROUP-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.[MenuGroup] M
CROSS JOIN [MMS].[MenuSubGroup] MG
UNION ALL
SELECT mm.MenuId AS id,'FRAME-'+mm.MenuFrameId AS pid,m.UserName AS MenuText
  FROM mst.MenuMaster AS mm 
  INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')='' AND ISNULL(mm.MenuSubGroupId,'')=''
UNION ALL
SELECT mm.MenuId AS id,'GROUP-'+mm.MenuGroupId AS pid,m.UserName AS MenuText
  FROM mst.MenuMaster AS mm 
  INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')=''
UNION ALL
SELECT mm.MenuId AS id,'SUBGROUP-'+mm.MenuSubGroupId AS pid,m.UserName AS MenuText
  FROM mst.MenuMaster AS mm 
  INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')<>''";
                var _dataC = _sqlRepository.GetDataCollection(sql);

                return Json(new { MASTER = _dataM, DATA = _dataC, Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
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

        #endregion
    }
}