using Aplos.Core;
using APLOS.Domain.Materials;
using Aplos.Properties;
using APLOS.Service;
//using APLOS.Service.Productions;

using System.Web.Mvc;
using APLOS.Services.Machines;
using APLOS.Domain.IE;
using Aplos.Service.IE;
//using APLOS.Domain.Production;

namespace APLOS.Areas.IE.Controllers
{
    public class ProductionOrderController : Controller
    {
        //private readonly IOperationService _operationService;
        #region Constractor
        /// <summary>   The ProductionOrderController service. </summary>
        //private readonly IProductionOrderService _productionorderservice;
        private readonly IOperationTimeCaptureService _operationtimecaptureservice;

        public ProductionOrderController(IOperationTimeCaptureService operationTimeCaptureService)
        {
            this._operationtimecaptureservice = operationTimeCaptureService;

        }
        #endregion

        #region -- Pages
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        [Authorize]
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        //[Authorize]
        //[HttpGet]
        //public JsonResult GetOperationCbo()
        //{
        //    return Json(_operationService.GetOperationCbo().rows, JsonRequestBehavior.AllowGet);
        //    //return Json(new SelectList(_operationService.GetOperationCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult GetOperationListCbo()
        //{
        //    return Json(new SelectList(_ioperationtimecaptureservice.GetCharacteristicsValueList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}


        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return null;// Json(_operationtimecaptureservice.GetSearchData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductionOrderController(string id)
        {
            return null;//Json(_productionorderservice.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OperationTimeCaptureMaster productionorder)
        {
            if (ModelState.IsValid)
            {
                //_productionorderservice.Insert(productionorder);
                return Json(new { Id = productionorder.Id, Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(OperationTimeCaptureMaster productionorder)
        {
            if (ModelState.IsValid)
            {
                //_productionorderservice.Update(productionorder);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                //_productionorderservice.Archive(id);
                return Json(new { Message = AplosMessage.Delete });
            }
            else
                throw new CustomException(Resources.IdNotFound);
            //if (!string.IsNullOrEmpty(id))
            //{
            //    _ioperationtimecaptureservice.Archive(id);
            //    return Json(new { });
            //}
            //else
            //    throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}