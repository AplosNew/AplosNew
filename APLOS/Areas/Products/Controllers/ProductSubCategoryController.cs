using Library.Core;
using Library.Model.Products;
using Library.Service.Products;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.Products.Controllers
{
    public class ProductSubCategoryController : BaseController
    {
        #region -- Constructor
        private readonly IProductSubCategoryService _productSubCategoryService;

        public ProductSubCategoryController(IProductSubCategoryService productSubCategoryService)
        {
            this._productSubCategoryService = productSubCategoryService;
        }
        #endregion

        #region Pages
      
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_productSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_productSubCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_productSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProductSubCategory()
        {
            return Json(_productSubCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProductSubCategoryById(string id)
        {
            return Json(_productSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(ProductSubCategory productSubCategory)
        {
            _productSubCategoryService.Insert(productSubCategory);
            return Json(new { ProductSubCategory = productSubCategory, Sequence = _productSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(ProductSubCategory productSubCategory)
        {
            _productSubCategoryService.Update(productSubCategory);
            return Json(new { Sequence = _productSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _productSubCategoryService.Archive(id);
            return Json(new { Sequence = _productSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}