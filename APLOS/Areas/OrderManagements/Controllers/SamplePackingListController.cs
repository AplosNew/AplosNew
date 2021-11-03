using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SamplePackingListController : BaseController
    {
        #region -- Constructor
        private readonly ISamplePackingListService _samplePackingListService;
        private readonly ISamplePackingListFormService _samplePackingListFormService;
        private readonly ISamplePackingListMaterialDetailsService _samplePackingListMaterialService;
        public SamplePackingListController(
              ISamplePackingListService samplePackingListService
            , ISamplePackingListFormService samplePackingListFormService
            , ISamplePackingListMaterialDetailsService samplePackingListMaterialService
            )
        {
            _samplePackingListService = samplePackingListService;
            _samplePackingListFormService = samplePackingListFormService;
            _samplePackingListMaterialService = samplePackingListMaterialService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region List
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameter)
        {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_samplePackingListService.Query(parameter, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingListByMaterialGroupMaster(string materialGroupMasterId)
        {
            return Json(_samplePackingListService.GetPackingListByMaterialGroupMaster(materialGroupMasterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult Get2ndPackingListByMaterialGroupMaster(string firstFormId)
        {
            return Json(_samplePackingListService.Get2ndPackingListByMaterialGroupMaster(firstFormId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingFormList(string masterId)
        {
            return Json(_samplePackingListFormService.GetPackingFormList(masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackLessMaterialList(string masterId)
        {
            return Json(_samplePackingListMaterialService.GetPackLessMaterialList(masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAllMaterialList(string masterId)
        {
            return Json(_samplePackingListMaterialService.GetAllMaterialList(masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetViewMaterialList(string firstFormId,string smpMaterialId)
        {
            return Json(_samplePackingListMaterialService.GetViewMaterialList(firstFormId, smpMaterialId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingMaterial(string firstFormId)
        {
            return Json(_samplePackingListMaterialService.GetPackingMaterial(firstFormId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFirstPackingForm(string id, string samplePackingMaterialId, string materialGroupMstId)
        {
            return Json(_samplePackingListFormService.GetFirstPackingForm(id, samplePackingMaterialId, materialGroupMstId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSecondPackByFirstPackId(string firstFormId,string samplePackingListMaterialId)
        {
            return Json(_samplePackingListFormService.GetSecondPackByFirstPackId(firstFormId, samplePackingListMaterialId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Operation
        [HttpPost]
        public JsonResult Create(SamplePackingList entity)
        {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.PlantId = identity.PlantId;
			_samplePackingListService.Insert(entity);
            return Json(new {entity.Id, Message = AplosMessage.Insert });
        }
        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult PackingFormCreate(IEnumerable<SamplePackingListMaterialDetails> materialList, IEnumerable<SamplePackingListForm> firstPackingList)
        {
            _samplePackingListService.InsertPackingForm(materialList, firstPackingList);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult SecondPackingFormCreate(IEnumerable<SamplePackingListForm> secondPackingList)
        {
            _samplePackingListFormService.InsertOrUpdateSecondPackingForm(secondPackingList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SamplePackingList entity)
        {
            _samplePackingListService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult PackingFormEdit(IEnumerable<SamplePackingListMaterialDetails> materialList, IEnumerable<SamplePackingListForm> firstPackingList)
        {
            _samplePackingListService.UpdatePackingForm(materialList, firstPackingList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult PackLessMaterialEdit(IEnumerable<SamplePackingListMaterialDetails> materialList)
        {
            _samplePackingListMaterialService.UpdatePackLessMaterial(materialList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _samplePackingListService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, ChaildAction(ParentActionName = "Delete")]
        public JsonResult PackingFormDelete(string id)
        {
            _samplePackingListService.DeletePackingForm(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, ChaildAction(ParentActionName = "Delete")]
        public JsonResult PackLessMaterialDelete(string id)
        {
            _samplePackingListMaterialService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}