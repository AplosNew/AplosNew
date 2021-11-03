using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Web.Mvc;
using Library.Data;

using System;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderControlStageController : BaseController
    {
        #region Constructor
        private readonly IOrderControlStageService _orderControlStageService;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <param name="orderControlStageService"> The order control stage service. </param>
        /// <param name="companyService">           The company service. </param>
        ///-------------------------------------------------------------------------------------------------

        public OrderControlStageController(IOrderControlStageService orderControlStageService)
        {
            this._orderControlStageService = orderControlStageService;
        }
        #endregion

        #region GetmachineCategoryList
        //dll

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the item list. </summary>
        /// <returns>   JsonResult. </returns>
        ///-------------------------------------------------------------------------------------------------

        [Authorize]
        public JsonResult GetmachineCategoryList()
        {
            return Json(new SelectList(_orderControlStageService.GetOrderControlStageList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Aplos
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_orderControlStageService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetorderControlStage(int pageSize = 10, int pageNumber = 1, string orderBy = "asc")
        {
            var totalCount = _orderControlStageService.Query().Select().Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Json(
               new
               {
                   OrderControlStageData = _orderControlStageService.Query().OrderBy(r => r.OrderBy(x => x.Id)).SelectPage(pageNumber, pageSize, out totalCount),
                   count = totalCount,
                   totalPages
               }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetorderControlStageById(string id)
        {
            return Json(_orderControlStageService.Find(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(OrderControlStage orderControlStage)
        {
            if (ModelState.IsValid)
            {
                _orderControlStageService.Insert(orderControlStage);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(OrderControlStage orderControlStage)
        {
            if (ModelState.IsValid)
            {
                _orderControlStageService.Update(orderControlStage);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _orderControlStageService.Archive(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion

    }
}