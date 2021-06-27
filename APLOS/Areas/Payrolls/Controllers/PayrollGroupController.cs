#region Using
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class PayrollGroupController : BaseController
    {
        #region Constructor
        private readonly IPayrollGroupService _payrollGroupService;
        private readonly ICompanyGroupPayrollGroupService _companyGroupPayrollGroupService;

        public PayrollGroupController(IPayrollGroupService payrollGroupService, ICompanyGroupPayrollGroupService companyGroupPayrollGroupService)
        {
            _payrollGroupService = payrollGroupService;
            _companyGroupPayrollGroupService = companyGroupPayrollGroupService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(new SelectList(_companyGroupPayrollGroupService.GetCbo(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupPayrollGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_payrollGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PayrollGroup payrollGroup)
        {
            _payrollGroupService.Insert(payrollGroup);
            return Json(new { PayrollGroup= payrollGroup, Sequence=_payrollGroupService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(PayrollGroup payrollGroup)
        {
            _payrollGroupService.Update(payrollGroup);
            return Json(new { Sequence = _payrollGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _payrollGroupService.DeleteGraph(id);
            return Json(new { Sequence = _payrollGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}