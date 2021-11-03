using Library.Core;
using Library.Model.Products;
using Library.Data;
using Library.Service.Products;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.Products.Controllers
{
    public class ProductCategoryController : BaseController
    {
        #region -- Constructor
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoryController(IProductCategoryService productCategoryService)
        {
            this._productCategoryService = productCategoryService;
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
            return Json(_productCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_productCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_productCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProductCategory()
        {
            return Json(_productCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProductCategoryById(string id)
        {
            return Json(_productCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(ProductCategory productCategory)
        {
            _productCategoryService.Insert(productCategory);
            return Json(new { ProductCategory = productCategory, Sequence = _productCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(ProductCategory productCategory)
        {
            _productCategoryService.Update(productCategory);
            return Json(new { Sequence = _productCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _productCategoryService.Archive(id);
                return Json(new { Sequence = _productCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}