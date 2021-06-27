using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class LineController : BaseController
    {
        private readonly ILineService _lineService;
        private readonly ICompanyGroupLineService _companyGroupLineService;
        private readonly ICompanyLineService _companyLineService;

        public LineController(
            ILineService lineService
            , ICompanyGroupLineService companyGroupLineService
            , ICompanyLineService companyLineService
            )
        {
            _lineService = lineService;
            _companyGroupLineService = companyGroupLineService;
            _companyLineService = companyLineService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_lineService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupLineService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompany(string companyId)
        {
            return Json(_companyLineService.GetCboList(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupLineService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLineList(GridParameter parameters, string entityId)
        {
            return Json(_lineService.Query(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListLineWithCompnay(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupLineService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Edit(Line line, IEnumerable<LocalLanguage> localLanguages)
        {
            _lineService.Update(line, localLanguages);
            return Json(new { Sequence = _lineService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _lineService.Delete(id);
                return Json(new { Sequence = _lineService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost]
        public JsonResult Create(Line line, IEnumerable<LocalLanguage> localLanguages)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _lineService.Insert(line, localLanguages);
            return Json(new { Line = line, Sequence = _lineService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult Get(string id)
        {
            return Json(_lineService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_lineService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
    }
}