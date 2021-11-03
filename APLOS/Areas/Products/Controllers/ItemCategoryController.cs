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
    /// <summary>
    /// <remark>Author:Mehedi Hasan Tamim;Date:30-12-2015;</remark>
    /// <remark>Modified:Belayet Hossain;Date:6-Jan-2016;</remark>
    /// </summary>
    public class ItemCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The itemCategoryService service. </summary>
        private readonly IItemCategoryService _itemCategoryService;

        public ItemCategoryController(IItemCategoryService itemCategoryService)
        {
            this._itemCategoryService = itemCategoryService;
        }
        #endregion

        #region dll
        //dll

        /// <summary>   Creates a JSON result with the given data as its content. </summary>
        /// <returns>   The currency list. </returns>
        [Authorize]
        public JsonResult GetItemCategoryList()
        {
            return Json(new SelectList(_itemCategoryService.GetItemCategoryList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_itemCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetItemCategory(int pageSize = 10, int pageNumber = 1, string orderBy = "asc")
        {

            var totalCount = _itemCategoryService.Query().Select().Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Json(
                new
                {
                    ItemCategoryData = _itemCategoryService.Query().OrderBy(r => r.OrderBy(x => x.Id)).SelectPage(pageNumber, pageSize, out totalCount),
                    count = totalCount,
                    totalPages
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetItemCategoryById(string id)
        {
            return Json(_itemCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(ItemCategory ItemCategory)
        {
            if (ModelState.IsValid)
            {
                _itemCategoryService.Insert(ItemCategory);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(ItemCategory ItemCategory)
        {
            if (ModelState.IsValid)
            {
                _itemCategoryService.Update(ItemCategory);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _itemCategoryService.Archive(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}