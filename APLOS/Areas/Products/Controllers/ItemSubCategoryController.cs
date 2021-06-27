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
    public class ItemSubCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The itemSubCategoryService service. </summary>
        private readonly IItemSubCategoryService _itemSubCategoryService;

        public ItemSubCategoryController(IItemSubCategoryService itemSubCategoryService)
        {
            this._itemSubCategoryService = itemSubCategoryService;
        }
        #endregion

        #region dll
        //dll

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Creates a JSON result with the given data as its content. </summary>
        /// <returns>   The currency list. </returns>
        ///-------------------------------------------------------------------------------------------------
        [Authorize]
        public JsonResult GetItemSubCategoryList()
        {
            return Json(new SelectList(_itemSubCategoryService.GetItemSubCategoryList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_itemSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetItemSubCategory(int pageSize = 10, int pageNumber = 1, string orderBy = "asc")
        {

            var totalCount = _itemSubCategoryService.Query().Select().Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Json(
                new
                {
                    ItemSubCategoryData = _itemSubCategoryService.Query().OrderBy(r => r.OrderBy(x => x.Id)).SelectPage(pageNumber, pageSize, out totalCount),
                    count = totalCount,
                    totalPages
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetItemSubCategoryById(string id)
        {
            return Json(_itemSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(ItemSubCategory ItemSubCategory)
        {
            if (ModelState.IsValid)
            {
                _itemSubCategoryService.Insert(ItemSubCategory);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(ItemSubCategory ItemSubCategory)
        {
            if (ModelState.IsValid)
            {
                _itemSubCategoryService.Update(ItemSubCategory);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _itemSubCategoryService.Archive(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}