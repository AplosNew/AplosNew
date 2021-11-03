using Library.Model.Products;
using Library.Service.Products;
using System;
using System.Web.Mvc;
using System.Linq;

using Library.Data;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.Products.Controllers
{
    public class ProductGroupController : BaseController
    {
        #region Constructor
        private readonly IProductGroupService _productGroupService;
        public ProductGroupController(IProductGroupService productGroupService)
        {
            _productGroupService = productGroupService;
        }
        #endregion

        #region dll
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Creates a JSON result with the given data as its content. </summary>
        /// <returns>   The currency list. </returns>
        ///-------------------------------------------------------------------------------------------------
        [Authorize]
        public JsonResult GetProductGroupList()
        {
            return Json(new SelectList(_productGroupService.GetProductGroupList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_productGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetProductGroup(int pageSize = 10, int pageNumber = 1, string orderBy = "asc")
        {

            var totalCount = _productGroupService.Query().Select().Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Json(
                new
                {
                    ProductGroupData = _productGroupService.Query().OrderBy(r => r.OrderBy(x => x.Id)).SelectPage(pageNumber, pageSize, out totalCount),
                    count = totalCount,
                    totalPages
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductGroupById(string id)
        {
            return Json(_productGroupService.Find(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(ProductGroup ProductGroup)
        {
            if (ModelState.IsValid)
            {
                _productGroupService.Insert(ProductGroup);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(ProductGroup ProductGroup)
        {
            if (ModelState.IsValid)
            {
                _productGroupService.Update(ProductGroup);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _productGroupService.Archive(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}