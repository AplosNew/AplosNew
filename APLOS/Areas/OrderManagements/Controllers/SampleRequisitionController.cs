using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Web.Mvc;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SampleRequisitionController : BaseController
    {
        #region -- Constructor
        private readonly ISampleRequisitionService _sampleRequisitionService;

        public SampleRequisitionController(
              ISampleRequisitionService sampleRequisitionService
            )
        {
            _sampleRequisitionService = sampleRequisitionService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_sampleRequisitionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        //[HttpGet, Authorize]
        //public JsonResult GetRequisitionFinishGoods(string masterId)
        //{
        //    return Json(_sampleRequisitionService.GetRequisitionFinishGoods(masterId), JsonRequestBehavior.AllowGet);
        //}
        [HttpPost]
        public JsonResult Create(SampleRequisition entity)
        {
            _sampleRequisitionService.InsertGraph(entity);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult Edit(SampleRequisition entity)
        {
            _sampleRequisitionService.UpdateGraph(entity);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult Delete(string id)
        {
            _sampleRequisitionService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}