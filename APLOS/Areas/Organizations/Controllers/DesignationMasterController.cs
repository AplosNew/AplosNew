#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Organizations;
using Library.Service.Organizations;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class DesignationMasterController : BaseController
    {
        #region Constructor

        private readonly IDesignationMasterService _designationMasterService;
        private readonly IDesignationMasterLegalDesignationService _legalDesignationService;
        private readonly IOrganizationReportService _organizationReportService;

        public DesignationMasterController(
              IDesignationMasterService designationMasterService
            , IDesignationMasterLegalDesignationService legalDesignationService
            , IOrganizationReportService organizationReportService)
        {
            _designationMasterService = designationMasterService;
            _legalDesignationService = legalDesignationService;
            _organizationReportService = organizationReportService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult DesignationGroup()
        {
            return View();
        }

        [Authorize]
        public ActionResult Designation()
        {
            return View();
        }

        [Authorize]
        public ActionResult LegalDesignation()
        {
            return View();
        }

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_designationMasterService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LegalDesignationListById(GridParameter parameters, string legalDesiIds)
        {
            return Json(_legalDesignationService.LegalDesignationListById(parameters, new JavaScriptSerializer().Deserialize<string[]>(legalDesiIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult DesMstLegalDesignation(GridParameter parameters, string desMstId)
        {
            return Json(_legalDesignationService.Query(parameters, desMstId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListForComDesignation(GridParameter parameters, string companyId)
        {
            return Json(_designationMasterService.GetListForComDesignation(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDesignationMaster(string id)
        {
            return Json(_designationMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DesignationMaster designationMaster, IEnumerable<DesignationMasterLegalDesignation> legalDesig)
        {
            _designationMasterService.InsertGraph(designationMaster, legalDesig);
            return Json(new { DesignationMaster = designationMaster, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(DesignationMaster designationMaster, IEnumerable<DesignationMasterLegalDesignation> legalDesig)
        {
            _designationMasterService.UpdateGraph(designationMaster, legalDesig);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _designationMasterService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult LegalDesignationDelete(string id)
        {
            _legalDesignationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        public ActionResult DesignationMasterReport()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Designation Master Report.xlsx";
            var workbook = _organizationReportService.GetDesignationMaster(identity.CompanyGroupId);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
    }
}