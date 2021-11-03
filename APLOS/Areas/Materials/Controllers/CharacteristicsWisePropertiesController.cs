using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Service.Organizations;
using Library.Service.Processes;
using Library.Service.Setups;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Crosscutting.Security;
using System.Threading;

namespace Aplos.Areas.Materials.Controllers
{
    public class CharacteristicsWisePropertiesController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly IUnitService _unitService;
        private readonly IPlantService _plantService;
        private readonly IProcessService _processService;
        private readonly IMaterialMasterService _materialmasterservice;//
        private readonly IUnitOfMeasurementService _unitofmeasurementservice;//ICharacteristicsValueService
        private readonly ICharacteristicsWisePropertiesMasterService _characteristicswisepropertiesmasterservice;//
        private readonly ICharacteristicsWisePropertiesDetailService _characteristicswisepropertiesdetailservice;//
        private readonly ICharacteristicsValueService _characteristicsvalueservice;//

        #region Constructor

        /// <summary>   The OperationTimeCaptureController service. </summary>
        //private readonly ISubsectionStructureMasterService _subsectionstructuremasterservice;

        public CharacteristicsWisePropertiesController(ICharacteristicsWisePropertiesMasterService characteristicswisepropertiesmasterservice, ICharacteristicsWisePropertiesDetailService characteristicswisepropertiesdetailservice, ICharacteristicsValueService characteristicsvalueservice, IUnitOfMeasurementService unitofmeasurementservice, IMaterialMasterService materialmasterservice, IProcessService processService, IPlantService plantService, ICompanyService companyService, IUnitService unitService)
        {
            this._companyService = companyService;
            this._unitService = unitService;
            this._plantService = plantService;
            this._processService = processService;
            this._materialmasterservice = materialmasterservice;
            //this._subsectionstructuremasterservice = _subsectionstructuremasterservice;
            this._unitofmeasurementservice = unitofmeasurementservice;
            this._characteristicswisepropertiesmasterservice = characteristicswisepropertiesmasterservice;
            this._characteristicswisepropertiesdetailservice = characteristicswisepropertiesdetailservice;
            this._characteristicsvalueservice = characteristicsvalueservice;
        }

        #endregion Constructor

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

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string masterid)
        {
            return Json(_characteristicswisepropertiesdetailservice.GetDetailList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetail(string id)
        {
            return Json(_characteristicswisepropertiesdetailservice.GetDetailById(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string masterid)
        {
            return Json(_characteristicswisepropertiesmasterservice.GetList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterId(string materialmasterid)
        {
            return Json(_characteristicswisepropertiesmasterservice.GetMasterId(materialmasterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUOMList(string detailid)
        {
            return Json(_characteristicswisepropertiesmasterservice.GetUOMList(detailid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUOMFactorList(string detailid)
        {
            return Json(_characteristicswisepropertiesmasterservice.GetUOMFactorList(detailid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterList(string masterid)
        {
            return Json(_characteristicswisepropertiesmasterservice.GetList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateMaster(CharacteristicsWisePropertiesMaster master)
        {
            var masterid = string.Empty;
            _characteristicswisepropertiesmasterservice.InsertORUpdate(master, out masterid);
            return Json(new { id = masterid, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult CreateDetail(CharacteristicsWisePropertiesDetail detail, IEnumerable<CharacteristicsWisePropertiesUOM> uomList, IEnumerable<CharacteristicsWisePropertiesUOMFactor> uomfactorlist)
        {
            var masterid = string.Empty;
            _characteristicswisepropertiesmasterservice.InsertORUpdate(detail, uomfactorlist, uomList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult DeleteMaster(string masterid)
        {
            _characteristicswisepropertiesmasterservice.DeleteMasterDetail(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteDetail(string detailid)
        {
            _characteristicswisepropertiesmasterservice.DeleteDetail(detailid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetMaterialMasterList(GridParameter parameters)
        {
            return Json(JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetCharacteristicsList(string mmid)
        {
            return Json(JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetMMUOMList(GridParameter parameters, string mmid)
        {
            return Json(_unitofmeasurementservice.GetMMUOM(parameters, mmid), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetUOMCbo(string materialmasterid)
        {
            return Json(_unitofmeasurementservice.GetCbo(materialmasterid).Rows, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetCharacteristicsValueList(GridParameter parameters, string CharacteristicsId, string ids)
        {
            //string CharacteristicsId = "CH2";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_characteristicsvalueservice.Query(parameters, identity.CompanyGroupId, CharacteristicsId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}