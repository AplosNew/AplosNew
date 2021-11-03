#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Model.Costings;
using Library.Service.Costings;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.Costings.Controllers
{
    public class CostingItemController : BaseController
    {
        #region Constructor
        private readonly ICostingItemService _CostingItemService;
        private readonly ISqlRepository _sqlRepository;

        public CostingItemController(ICostingItemService CostingItemService, ISqlRepository sqlRepository)
        {
            _CostingItemService = CostingItemService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages
    
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_CostingItemService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSubProcessCbo(string processId)
        {
            return Json(_sqlRepository.GetDataCollection("  SELECT * FROM hkp.SubProcess AS sp WHERE sp.ProcessId='" + processId + "' ORDER BY sp.Sequence,sp.UserName"), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetCostingSubCategory()
        {
            string sql = @"select * from HKP.CostingSubCategory";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetPurchaseGroups()
        {
            string sql = @"select* from org.PurchaseGroup";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_CostingItemService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetMaterialGroupList()
        {
            return Json(new Library.MaterialManagement.InventoryManagements.MaterialCommonService(_sqlRepository).GetMaterialGroupList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_CostingItemService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CostingItem entity)
        {
            _CostingItemService.Insert(entity);
            return Json(new { CostingItem = entity, Sequence = _CostingItemService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CostingItem entity)
        {
            _CostingItemService.Update(entity);
            return Json(new { Sequence = _CostingItemService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var entity = _CostingItemService.Find(id);
            _CostingItemService.Delete(entity);
            return Json(new { Sequence = _CostingItemService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion
    }
}