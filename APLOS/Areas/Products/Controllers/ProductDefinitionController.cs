using Library.Service.Products;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Model.Products;
using Aplos.Properties;
using Library.Model.Materials;
using System.Collections.Generic;
using Library.Service.Materials;

namespace Aplos.Areas.Products.Controllers
{
    public class ProductDefinitionController : BaseController
    {
        #region -- Constructor
        readonly IProductDefinitionService _productDefinitionService;
        readonly IMaterialMasterArticleService _articleService;
        public ProductDefinitionController(IProductDefinitionService productDefinitionService, IMaterialMasterArticleService articleService)
        {
            _productDefinitionService = productDefinitionService;
            _articleService = articleService;
        }
        #endregion

        #region Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        [HttpGet]
        public ActionResult MaterialMasterWithProductMaster()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterList(GridParameter parameters, string paramList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productDefinitionService.GetMaterialMasterList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(paramList)), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEfficencyList(string masterId)
        {
            return Json(_productDefinitionService.GetEfficencyList(masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetUnSavedMaterialMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productDefinitionService.GetMaterialMasterList(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSavedData()
        {
            return Json(_productDefinitionService.GetSavedData(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string tempParam)
        {
            return Json(_productDefinitionService.Query(parameters, new JavaScriptSerializer().Deserialize<string[]>(tempParam)), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateProductDefinition(IEnumerable<ProductDefinition> entities)
        {
            _productDefinitionService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Create(ProductDefinition product, IEnumerable<MaterialMasterArticle> articleList, IEnumerable<ProductDefinitionEfficency> efficencyList)
        {
            _productDefinitionService.InsertGraph(product, articleList, efficencyList);
            return Json(new { Product = product, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ProductDefinition product, IEnumerable<MaterialMasterArticle> articleList, IEnumerable<ProductDefinitionEfficency> efficencyList)
        {
            _productDefinitionService.UpdateGraph(product, articleList, efficencyList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _productDefinitionService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DeleteArticleProcess(string id)
        {
            _articleService.DeleteArticleProcess(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}