#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Materials;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class ServiceGroupGLController : BaseController
    {
        #region Constructor

        private readonly IServiceGroupGLService _serviceGroupGLService;

        public ServiceGroupGLController(
              IServiceGroupGLService serviceGroupGLService
            )
        {
            _serviceGroupGLService = serviceGroupGLService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public JsonResult UpdateServiceGroupDeterminate(IEnumerable<ServiceGroupGL> serviceGroupGL, IEnumerable<ServiceGroupPartyAccountGroupGL> serviceGroupPartyAccountGroupGL)
        {
            _serviceGroupGLService.InsertUpdateServiceGroupDeterminate(serviceGroupGL, serviceGroupPartyAccountGroupGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult GetDataByServiceGroupId(GridParameter parameters, string serviceGroupMasterId, string coaId)
        {
            return Json(_serviceGroupGLService.GetDataByServiceGroupId(parameters, serviceGroupMasterId, coaId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string coaId)
        {
            return Json(_serviceGroupGLService.GetSearchWithCombine(parameters, coaId), JsonRequestBehavior.AllowGet);
        }




        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string coaId)
        {
            return Json(_serviceGroupGLService.GetSearchWithCombineWithAssing(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string coaId)
        {
            return Json(_serviceGroupGLService.GetSearchWithCombineWithNotAssing(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetListWithCombineCoa(GridParameter parameters, string coaId)
        //{
        //    return Json(_serviceGroupGLService.GetSearchWithCombineCoa(parameters, coaId), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineCoa(GridParameter parameters)
        {
            return Json(_serviceGroupGLService.GetSearchWithCombineCoa(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListAccountGroupVendor(GridParameter parameters)
        {
            return Json(_serviceGroupGLService.GetPartyAccountGroup(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyAccountVD(GridParameter parameters)
        {
            return Json(_serviceGroupGLService.GetPartyAccountVD(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(ServiceGroupGL fixedAssetClass)
        {
            _serviceGroupGLService.Update(fixedAssetClass);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _serviceGroupGLService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations




        [HttpGet, Authorize]
        public ActionResult GetServiceGroupGlReport()
        {

            try
            {
                _serviceGroupGLService.ServiceGroupGlReport();
                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }


    }
}