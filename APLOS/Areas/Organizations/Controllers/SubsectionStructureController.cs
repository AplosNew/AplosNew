using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.IE;
using Library.Service.IEnumerable;
using Library.Service.Organizations;
using Library.Service.Processes;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class SubsectionStructureController : BaseController
    {
        #region Constructor

        private readonly ICompanyService _companyService;
        private readonly IUnitService _unitService;
        private readonly IProcessService _processService;
        private readonly ICompanyDivisionService _divisionservice;
        private readonly ICompanySubDivisionService _subdivisionservice;
        private readonly ICompanySectionService _sectionservice;
        private readonly ICompanySubSectionService _subsectionservice;
        private readonly ICompanyLineService _lineservice;
        private readonly ISubsectionStructureMasterService _subsectionstructuremasterservice;

        public SubsectionStructureController(ICompanyLineService lineservice
            , ICompanySubSectionService subsectionservice
            , ICompanySectionService sectionservice
            , ICompanyDivisionService divisionservice
            , ICompanySubDivisionService subdivisionservice
            , ISubsectionStructureMasterService subsectionstructuremasterservice
            , IProcessService processService
            , ICompanyService companyService
            , IUnitService unitService)
        {
            _companyService = companyService;
            _unitService = unitService;
            _processService = processService;
            _subsectionstructuremasterservice = subsectionstructuremasterservice;
            _lineservice = lineservice;
            _subdivisionservice = subdivisionservice;
            _divisionservice = divisionservice;
            _sectionservice = sectionservice;
            _subsectionservice = subsectionservice;
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

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_subsectionstructuremasterservice.GetSearchData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDetailList(string masterid)
        {
            return Json(_subsectionstructuremasterservice.GetDetailList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMasterList(string masterid)
        {
            return Json(_subsectionstructuremasterservice.GetMasterList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMachineTypeList(GridParameter parameters)
        {
            return Json(_companyService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateMaster(SubsectionStructureMaster master)
        {
            string masterid = string.Empty;
            _subsectionstructuremasterservice.InsertORUpdate(master, out masterid);
            return Json(new { id = masterid, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult DeleteMaster(string masterid)
        {
            _subsectionstructuremasterservice.DeleteMasterDetail(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteDetail(string detailid)
        {
            _subsectionstructuremasterservice.DeleteDetail(detailid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult CreateDetail(SubsectionStructureDetail detail)
        {
            _subsectionstructuremasterservice.InsertORUpdateDetail(detail);
            return Json(new { bulletindetail = detail, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_subsectionstructuremasterservice.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {
            _subsectionstructuremasterservice.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetDivisionListCbo(string CompanyId)
        {
            return Json(_divisionservice.GetCboList(CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetSubdivisionListCbo(string CompanyId)
        {
            return Json(_subdivisionservice.GetCboList(CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetSectionListCbo(string CompanyId)
        {
            return Json(_sectionservice.GetCboList(CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetSubsectionListCbo(string CompanyId)
        {
            return Json(_subsectionservice.GetCboList(CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetLineListCbo(string CompanyId)
        {
            return Json(_lineservice.GetCboList(CompanyId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}