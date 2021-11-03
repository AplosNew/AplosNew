using Library.Core;
using Library.Model.Products;
using Library.Service.Products;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.Products.Controllers
{
    public class ProductController : BaseController
    {
        #region -- Constructor
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            this._productService = productService;
        }
        #endregion

        #region Pages
      
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_productService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_productService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_productService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProduct()
        {
            return Json(_productService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Product product)
        {
            _productService.Insert(product);
            return Json(new { Product = product, Sequence = _productService.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(Product product)
        {
            _productService.Update(product);
            return Json(new { Sequence = _productService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _productService.Archive(id);
            return Json(new { Sequence = _productService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}