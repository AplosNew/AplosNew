using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Setups.Controllers
{
    public class ServiceMasterController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IServiceMasterService _serviceMasterService;

        public ServiceMasterController(IServiceMasterService serviceMasterService, ISqlRepository R)
        {
            this._serviceMasterService = serviceMasterService;
            _sqlRepository = R;
        }

        #endregion -- Constructor

        #region Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters, string ids)
        {
            return Json(_serviceMasterService.Query(parameters, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceMasterList(GridParameter parameters)
        {
            return Json(_serviceMasterService.QueryServiceMaster(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHSNCodeByServiceGroupId(string groupId)
        {
            var sql = @"SELECT Code FROM HKP.HSNCode WHERE Id =(SELECT HSNCodeId FROM [HKP].[ServiceGroup] WHERE Id='"+ groupId + "')";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_serviceMasterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ServiceMaster serviceMaster)
        {
            _serviceMasterService.Insert(serviceMaster);
            return Json(new { ServiceMaster = serviceMaster, Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ServiceMaster serviceMaster)
        {
            _serviceMasterService.Update(serviceMaster);
            return Json(new { Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _serviceMasterService.Delete(id);
                return Json(new { Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}