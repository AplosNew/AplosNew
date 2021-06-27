#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class CompanyTaxContributionController : BaseController
    {
        #region Constructor
        private readonly ICompanyTaxContributionService _companyTaxContributionService;

        public CompanyTaxContributionController(ICompanyTaxContributionService companyTaxContributionService)
        {
            _companyTaxContributionService = companyTaxContributionService;
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
        [HttpGet, Authorize]
        public ActionResult GetAllEmployee(GridParameter parameters, string plantId)
        {
            return Json(_companyTaxContributionService.GetAllEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CompanyTaxContribution model)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _companyTaxContributionService.Insert(model, identity.CompanyGroupId);
            return Json(new { CompanyTaxContribution = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CompanyTaxContribution model)
        {
            _companyTaxContributionService.Update(model);
            return Json(new { CompanyTaxContribution = model, Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _companyTaxContributionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedData(GridParameter parameters, string empId, string plantId, string taxYearId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyTaxContributionService.Query(parameters, empId, plantId, taxYearId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBasicData(GridParameter parameters)
        {
            return Json(_companyTaxContributionService.BasicQuery(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getemployeeData(GridParameter parameters, string empId, string plantId)
        {
            return Json(_companyTaxContributionService.Query(parameters, empId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getemployeeDataTaxYearly(GridParameter parameters, string empId, string plantId,string taxYearId)
        {
            return Json(_companyTaxContributionService.Query(parameters, empId, plantId, taxYearId), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}