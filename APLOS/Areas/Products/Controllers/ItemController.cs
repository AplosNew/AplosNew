using Library.Model.Products;
using Library.Data;
using Library.Service.Products;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.Products.Controllers
{
    /// <summary>
    /// <remark>Author:Mehedi Hasan Tamim;Date:30-12-2015;</remark>
    /// <remark>Modified:Belayet Hossain;Date:6-Jan-2016;</remark>
    /// </summary>
    public class ItemController : BaseController
    {
        #region Constructor
        /// <summary>   The itemService service. </summary>
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            this._itemService = itemService;
        }
        #endregion

        #region dll
        //dll

        /// <summary>   Creates a JSON result with the given data as its content. </summary>
        /// <returns>   The currency list. </returns>
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_itemService.GetItemList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_itemService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetItem(int pageSize = 10, int pageNumber = 1, string orderBy = "asc")
        {
            var totalCount = _itemService.Query().Select().Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Json(
                new
                {
                    ItemData = _itemService.Query().OrderBy(r => r.OrderBy(x=> x.Id)).SelectPage(pageNumber, pageSize, out totalCount),
                    count = totalCount,
                    totalPages
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetItemById(string id)
        {
            return Json(_itemService.Find(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(Item Item)
        {
            if (ModelState.IsValid)
            {
                _itemService.Insert(Item);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(Item Item)
        {
            if (ModelState.IsValid)
            {
                _itemService.Update(Item);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _itemService.Archive(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}