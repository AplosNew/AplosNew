using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;
using Library.Accounting.Accounts;
using Library.Data.Sql;
using Library.Core;
using Library.Model.Enums;

namespace Aplos.Areas.Accounts.Controllers
{
    public class PartyReconciliationController : BaseController
    {
        
        private readonly ISqlRepository _sqlRepository;

        public PartyReconciliationController( ISqlRepository sqlRepository )
        {
            _sqlRepository = sqlRepository;
        }


        public ActionResult PartyReconciliation()
        {
            return View("~/Areas/Accounts/Views/PartyReconciliation/PartyReconciliation.cshtml");

        }

        public ActionResult PartyReconciliationDetail()
        {
            return View("~/Areas/Accounts/Views/PartyReconciliation/PartyReconciliationDetail.cshtml");

        }

        

        [Authorize, HttpGet]
        public JsonResult GetPartyDrList(string partyId)
        {
            AccountsPartyReconciliationService _accountsPartyReconciliationService = new AccountsPartyReconciliationService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsPartyReconciliationService.GetPartyDrList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPartyCrList(string partyId)
        {
            AccountsPartyReconciliationService _accountsPartyReconciliationService = new AccountsPartyReconciliationService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsPartyReconciliationService.GetPartyCrList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetPartyReconciliationList(string column, string value)
        
        {
            AccountsPartyReconciliationService _accountsPartyReconciliationService = new AccountsPartyReconciliationService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsPartyReconciliationService.GetPartyReconciliation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, SourceType.PartyReconcilliation), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ReportPartyReconciliation(ReportFormat reportFormat, string voucherId)
        {
            AccountsPartyReconciliationService _accountsPartyReconciliationService = new AccountsPartyReconciliationService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountsPartyReconciliationService.GetPartyReconciliationReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }
    }
}