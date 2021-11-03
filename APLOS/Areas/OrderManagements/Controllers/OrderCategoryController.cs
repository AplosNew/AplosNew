using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The OrderCategoryService service. </summary>
        private readonly IOrderCategoryService _orderCategoryService;
        private readonly ICompanyGroupOrderCategoryService _companyGroupOrderCategoryService;

        public OrderCategoryController(IOrderCategoryService orderCategoryService, ICompanyGroupOrderCategoryService companyGroupOrderCategoryService)
        {
            _orderCategoryService = orderCategoryService;
            _companyGroupOrderCategoryService = companyGroupOrderCategoryService;
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
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupOrderCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupOrderCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_orderCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OrderCategory orderCategory)
        {
            _orderCategoryService.Insert(orderCategory);
            return Json(new { OrderCategory = orderCategory, PlanningPriority = _orderCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(OrderCategory orderCategory)
        {
            _orderCategoryService.Update(orderCategory);
            return Json(new { PlanningPriority = _orderCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _orderCategoryService.DeleteGraph(id);
            return Json(new { PlanningPriority = _orderCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion

    }
}