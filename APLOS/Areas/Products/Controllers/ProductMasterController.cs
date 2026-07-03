using Library.Core;
using Library.Model.Products;
using Library.Service.Products;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;

namespace Aplos.Areas.Products.Controllers
{
    public class ProductMasterController : BaseController
    {
        #region -- Constructor
        private readonly IProductMasterService _productMasterService;

        public ProductMasterController(IProductMasterService productMasterService)
        {
            this._productMasterService = productMasterService;
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
            return Json(_productMasterService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult ProductMasterWithDetails(string productMasterId)
        {
            return Json(_productMasterService.ProductMasterWithDetails(productMasterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult ProductMasterComminationData(string productMasterId)
        {
            return Json(_productMasterService.ProductMasterComminationData(productMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productMasterService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetPMCbo()
        {
            return Json(_productMasterService.GetPMCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_productMasterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductMaster()
        {
            return Json(_productMasterService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductMasterById(string id)
        {
            return Json(_productMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEfficencyList(string masterId)
        {
            return Json(_productMasterService.GetEfficencyList(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductMasterAltUomList(string productMasterId)
        {
            return Json(_productMasterService.GetProductMasterAltUomList(productMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductMaster productMaster, IEnumerable<ProductMasterAttributeValue> productMasterAttributeValue, IEnumerable<ProductMasterEfficency> efficencyList, IEnumerable<ProductMasterAlternativeUOM> materialMasterAlternativeUOM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _productMasterService.Insert(productMaster, productMasterAttributeValue, efficencyList, materialMasterAlternativeUOM);
            return Json(new { ProductMaster = productMaster, Sequence = _productMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(ProductMaster productMaster)
        {
            _productMasterService.Update(productMaster);
            return Json(new { Sequence = _productMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _productMasterService.Archive(id);
            return Json(new { Sequence = _productMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}