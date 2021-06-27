#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Payrolls;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SalaryFixationSettingController : BaseController
    {
        #region -- Constructor

        private readonly ISalaryFixationSettingService _salaryFixationSettingService;

        public SalaryFixationSettingController(ISalaryFixationSettingService salaryFixationSettingService)
        {
            _salaryFixationSettingService = salaryFixationSettingService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_salaryFixationSettingService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_salaryFixationSettingService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SalaryFixationSetting model)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _salaryFixationSettingService.Insert(model, identity.CompanyGroupId);
            return Json(new { SalaryFixationSetting = model, Sequence = _salaryFixationSettingService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SalaryFixationSetting model)
        {
            _salaryFixationSettingService.Update(model);
            return Json(new { Sequence = _salaryFixationSettingService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _salaryFixationSettingService.DeleteMaster(id);
            return Json(new { Sequence = _salaryFixationSettingService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeads(GridParameter parameters)
        {
            return Json(_salaryFixationSettingService.GetSalaryHeads(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadsAncash(GridParameter parameters)
        {
            return Json(_salaryFixationSettingService.GetSalaryHeadsAnCash(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveTypes(GridParameter parameters)
        {
            return Json(_salaryFixationSettingService.GetLeaveTypes(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAnnualNonCash(GridParameter parameters)
        {
            return Json(_salaryFixationSettingService.GetAnnualNonCash(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedLeaveTypes(string salFixSetId)
        {
            return Json(_salaryFixationSettingService.GetSavedLeaveChild(salFixSetId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSalaryHeadDetails(
             IEnumerable<SalaryFixationSettingDetails> monthlyList
            , IEnumerable<SalaryFixationSettingDetails> annualCashList
            , IEnumerable<SalaryFixationSettingDetails> annualCashNonList,
            IEnumerable<SalaryFixationSettingDetails> leaveTypeList, string salFixSetId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _salaryFixationSettingService.InsertOrUpdate(monthlyList, annualCashList, annualCashNonList, leaveTypeList, salFixSetId);
            return Json(new { SalaryFixationSetting = leaveTypeList, Sequence = _salaryFixationSettingService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetChildDataMasterWise(string salFixSetId)
        {
            return Json(_salaryFixationSettingService.GetSavedChildMasterWise(salFixSetId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAnnualCashChildDataMasterWise(string salFixSetId)
        {
            return Json(_salaryFixationSettingService.GetAnnualCashChild(salFixSetId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetNonCashDetailList(string salFixSetId)
        {
            return Json(_salaryFixationSettingService.GetNonCashChild(salFixSetId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}