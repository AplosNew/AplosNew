#region using
using Aplos.Properties;
using Aplos.Controllers;
using Library.Model.Products;
using Library.Service.Products;
using System.Collections.Generic;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.Products.Controllers
{
    public class ProductSubCategoryAttributeController : BaseController
    {
        #region -- Constructor
        private readonly IProductSubCategoryAttributeService _productSubCategoryAttributeService;

        public ProductSubCategoryAttributeController(
            IProductSubCategoryAttributeService productSubCategoryAttributeService)
        {
            this._productSubCategoryAttributeService = productSubCategoryAttributeService;
        }
        #endregion

        #region --pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(string productSubCategoryId)
        {
            return Json(_productSubCategoryAttributeService.GetSearchData(productSubCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAttribute(string productSubCategoryId, string productMasterId)
        {
            return Json(_productSubCategoryAttributeService.GetAttribute(productSubCategoryId, productMasterId), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet, Authorize]
        public JsonResult GetProductSubCategoryAttributeId(string id)
        {
            return Json(_productSubCategoryAttributeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<ProductSubCategoryAttribute> productSubCategoryAttributes)
        {
            _productSubCategoryAttributeService.Insert(productSubCategoryAttributes);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ProductSubCategoryAttribute productSubCategoryAttribute)
        {
            _productSubCategoryAttributeService.Update(productSubCategoryAttribute);
            return Json(new { ProductSubCategoryAttribute = productSubCategoryAttribute, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _productSubCategoryAttributeService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}