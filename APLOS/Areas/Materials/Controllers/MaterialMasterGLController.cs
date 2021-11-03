using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialMasterGLController : BaseController
    {
        #region Constructor

        private readonly IMaterialMasterGLService _materialMasterGLService;

        public MaterialMasterGLController(
              IMaterialMasterGLService materialMasterGLService
            )
        {
            _materialMasterGLService = materialMasterGLService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetFixedAssetItemCbo()
        {
            return Json(_materialMasterGLService.GetFixedAssetItemCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateFixedAssetDeterminate(IEnumerable<MaterialMasterGL> materialMasterGL, IEnumerable<MaterialMasterVendorReconGL> materialMasterVendorReconGL)
        {
            _materialMasterGLService.InsertUpdateMaterialMasterGL(materialMasterGL, materialMasterVendorReconGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId)
        {
            return Json(_materialMasterGLService.GetDataByFixedAssetMasterId(parameters, fixedAssetMasterId, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetVendorReconDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId)
        {
            return Json(_materialMasterGLService.GetVendorReconDataByFixedAssetMasterId(parameters, fixedAssetMasterId, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds)
        {
            return Json(_materialMasterGLService.GetSearchWithCombine(parameters, coaId, materialMasterIds, fixedAssetMasterIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds)
        {
            return Json(_materialMasterGLService.GetSearchWithCombineWithAssing(parameters, coaId, materialMasterIds, fixedAssetMasterIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds)
        {
            return Json(_materialMasterGLService.GetSearchWithCombineWithNotAssing(parameters, coaId, materialMasterIds, fixedAssetMasterIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineCoa(GridParameter parameters)
        {
            return Json(_materialMasterGLService.GetSearchWithCombineCoa(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListAccountGroupVendor(GridParameter parameters)
        {
            return Json(_materialMasterGLService.GetPartyAccountGroup(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAccountGroupData()
        {
            return Json(_materialMasterGLService.GetAccountGroupData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAccountGroupData2()
        {
            return Json(_materialMasterGLService.GetAccountGroupData2(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyAccountVD(GridParameter parameters)
        {
            return Json(_materialMasterGLService.GetPartyAccountVD(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyAccountWithAssignList(string partyAcId, string materialMasterGlId)
        {
            return Json(_materialMasterGLService.GetPartyAccountWithAssignList(partyAcId, materialMasterGlId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(MaterialMasterGL fixedAssetClass)
        {
            _materialMasterGLService.Update(fixedAssetClass);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _materialMasterGLService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetActivityCbo(string budgetId)
        {
            return Json(_materialMasterGLService.GetBudgetActivityCbo(budgetId), JsonRequestBehavior.AllowGet);
        }
    }
}