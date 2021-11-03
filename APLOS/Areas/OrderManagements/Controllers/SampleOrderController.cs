using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Model.Parties;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SampleOrderController : BaseController
    {
        #region -- Constructor

        private readonly ISampleOrderService _sampleOrderService;
        private readonly ISampleOrderSubMaterialService _subMaterialService;
        private readonly ISampleOrderSubMaterialValueService _sampleOrderValueService;
        private readonly ISampleOrderPartnerFunctionService _partnerFunctionService;

        public SampleOrderController(
              ISampleOrderService sampleOrderService
            , ISampleOrderSubMaterialService subMaterialService
            , ISampleOrderSubMaterialValueService sampleOrderValueService
            , ISampleOrderPartnerFunctionService partnerFunctionService
            )
        {
            _sampleOrderService = sampleOrderService;
            _subMaterialService = subMaterialService;
            _sampleOrderValueService = sampleOrderValueService;
            _partnerFunctionService = partnerFunctionService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string plantId)
        {
            return Json(_sampleOrderService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAttributeByMgm(string materialGroupMasterId, string subMaterialId)
        {
            return Json(_sampleOrderValueService.GetAttributeByMgm(materialGroupMasterId, subMaterialId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSubMaterial(string masterId)
        {
            return Json(_subMaterialService.Query(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterByCustomer(GridParameter parameters, string partyId, string sampleOrderSubMaterialIds)
        {
            return Json(_subMaterialService.GetMaterialMasterByCustomer(parameters, partyId, new JavaScriptSerializer().Deserialize<string[]>(sampleOrderSubMaterialIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSampleOrderPartnerFunction(string masterId)
        {
            return Json(_partnerFunctionService.Query(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCustomerBySPF(GridParameter parameters)
        {
            return Json(_partnerFunctionService.GetCustomerBySPF(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SampleOrder entity, IEnumerable<SampleOrderSubMaterial> details, IEnumerable<SampleOrderPartnerFunction> partnerFunctions)
        {
            _sampleOrderService.InsertGraph(entity, details, partnerFunctions);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SampleOrder entity, IEnumerable<SampleOrderSubMaterial> details, IEnumerable<SampleOrderPartnerFunction> partnerFunctions)
        {
            _sampleOrderService.UpdateGraph(entity, details, partnerFunctions);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _sampleOrderService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}