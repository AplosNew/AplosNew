using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.FixedAssets;
using Library.Core;
using Library.Data.Sql;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetMasterGLController : BaseController
    {
        private readonly IFixedAssetMasterGLService _fixedAssetMasterGLService;
        private readonly ISqlRepository _sqlRepository;

        public FixedAssetMasterGLController(IFixedAssetMasterGLService fixedAssetMasterGLService, ISqlRepository sqlRepository)
        {
            _fixedAssetMasterGLService = fixedAssetMasterGLService;
            _sqlRepository = sqlRepository;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return RedirectToAction("FixedAssetMasterGL", "FixedAssetMaster");
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetItemCbo()
        {
            return Json(_fixedAssetMasterGLService.GetFixedAssetItemCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateFixedAssetDeterminate(IEnumerable<FixedAssetMasterGL> fixedAssetMasterGL, IEnumerable<FixedAssetMasterVendorReconGL> fixedAssetMasterVendorReconGL)
        {
            _fixedAssetMasterGLService.InsertUpdateFixedAssetMasterGL(fixedAssetMasterGL, fixedAssetMasterVendorReconGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public ActionResult GetDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId)
        {
            return Json(_fixedAssetMasterGLService.GetDataByFixedAssetMasterId(parameters, fixedAssetMasterId, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetVendorReconDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId)
        {
            return Json(_fixedAssetMasterGLService.GetVendorReconDataByFixedAssetMasterId(parameters, fixedAssetMasterId, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string coaId, string fixedAssetMasterIds)
        {
            FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(fixedAssetQueryService.GetSearchWithCombine(parameters, coaId, fixedAssetMasterIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string coaId, string fixedAssetMasterIds)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);

            return Json(_fixedAssetQueryService.GetSearchWithCombineWithAssing(parameters, coaId, fixedAssetMasterIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string coaId, string fixedAssetMasterIds)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);

            return Json(_fixedAssetQueryService.GetSearchWithCombineWithNotAssing(parameters, coaId, fixedAssetMasterIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineCoa(GridParameter parameters)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);

            return Json(_fixedAssetQueryService.GetSearchWithCombineCoa(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListAccountGroupVendor(GridParameter parameters)
        {
            return Json(_fixedAssetMasterGLService.GetPartyAccountGroup(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAccountGroupData()
        {
            return Json(_fixedAssetMasterGLService.GetAccountGroupData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAccountGroupData2()
        {
            return Json(_fixedAssetMasterGLService.GetAccountGroupData2(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyAccountVD(GridParameter parameters)
        {
            return Json(_fixedAssetMasterGLService.GetPartyAccountVD(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyAccountWithAssignList(string partyAcId, string fixedAssetMasterGlId)
        {
            return Json(_fixedAssetMasterGLService.GetPartyAccountWithAssignList(partyAcId, fixedAssetMasterGlId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetMasterGL fixedAssetClass)
        {
            _fixedAssetMasterGLService.Update(fixedAssetClass);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _fixedAssetMasterGLService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetActivityCbo(string budgetId)
        {
            return Json(_fixedAssetMasterGLService.GetBudgetActivityCbo(budgetId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetReport()
        {

            try
            {
                _fixedAssetMasterGLService.FixedAssetMasterReport();
                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

    }
}