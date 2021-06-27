using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.IE;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Service.Materials;
using Library.Service.Organizations;
using Library.Service.Processes;
using Syncfusion.XlsIO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.IE.Controllers
{
    public class BulletinController : BaseController
    {
        #region Constructor

        private readonly IBulletinMasterService _bulletinemasterservice;
        private readonly IFGComponentService _fgcomponentservice;
        private readonly IFGZoneService _fgzoneservice;
        private readonly IMaterialMasterService _materialmasterservice;
        private readonly IOperationService _operationService;
        private readonly IDesignationGroupService _designationgroupservice;
        private readonly ICompanyGroupOperationActivityService _companygroupoperationactionservice;

        public BulletinController(
            ICompanyGroupOperationActivityService companygroupoperationactionservice
            , IBulletinMasterService bulletinemasterservice
            , IDesignationGroupService designationgroupservice
            , IFGComponentService fgcomponentservice
            , IFGZoneService fgzoneservice
            , IMaterialMasterService materialmasterservice
            , IOperationService operationService)
        {
            _fgcomponentservice = fgcomponentservice;
            _companygroupoperationactionservice = companygroupoperationactionservice;
            _fgzoneservice = fgzoneservice;
            _operationService = operationService;
            _bulletinemasterservice = bulletinemasterservice;
            _materialmasterservice = materialmasterservice;
            _designationgroupservice = designationgroupservice;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bulletinemasterservice.GetSearchData(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinProcessList(string masterid)
        {
            return Json(_bulletinemasterservice.GetBulletinProcessList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinDetailList(string masterId, string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bulletinemasterservice.GetBulletinDetailList(identity.CompanyGroupId, masterId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinMasterList(string masterid)
        {
            return Json(_bulletinemasterservice.GetBulletinMasterList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBuyerList(GridParameter parameters)
        {
            return Json(_bulletinemasterservice.GetBuyerList(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialMasterList()
        {
            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationList(GridParameter parameters, string processid, string fgcomponentid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationService.GetSearchData(parameters, identity.CompanyGroupId, processid, fgcomponentid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet, Authorize]
        public JsonResult GetZoneCbo()
        {
            return Json(_fgzoneservice.GetFGZoneCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetComponentCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fgcomponentservice.GetFGComponentCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDesignatinGroupCbo()
        {
            return Json(_designationgroupservice.GetCboList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetOperationActionCbo()
        {
            return Json(_companygroupoperationactionservice.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetManpowerTypeCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetMachineExecutionTypeCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateMaster(BulletinMaster master)
        {
            var masterid = string.Empty;
            _bulletinemasterservice.InsertORUpdateMaster(master, out masterid);
            return Json(new { id = masterid, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _bulletinemasterservice.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DeleteMaster(string masterid)
        {
            _bulletinemasterservice.DeleteMasterDetail(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateProcess(string masterId, IEnumerable<BulletinProcess> processList)
        {
            _bulletinemasterservice.InsertProcess(masterId, processList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DeleteProcess(string id)
        {
            _bulletinemasterservice.DeleteProcess(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateDetail(BulletinDetail detail)
        {
            _bulletinemasterservice.InsertORUpdateDetail(detail);
            return Json(new { bulletindetail = detail, Message = AplosMessage.Insert });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DeleteDetail(string detailid)
        {
            _bulletinemasterservice.DeleteDetail(detailid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult GenerateExcel(string masterid)
        {
            var workbook = _bulletinemasterservice.GetWorkBook(out ExcelEngine excelEngine, masterid);
            return excelEngine.SaveAsActionResult(workbook, "output.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.Excel2013);
        }

        #endregion -- Operations
    }
}