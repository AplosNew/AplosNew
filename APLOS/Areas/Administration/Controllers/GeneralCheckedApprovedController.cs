using Aplos.Controllers;
using Library.Data.Sql;
using System.Web.Mvc;
using Library.Service.Administration.Contract;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Accounting.Accounts;
using Aplos.Properties;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralCheckedApprovedController : BaseController
    {
        //GeneralContractCheckService gc = new GeneralContractCheckService();
        private readonly SqlRepository _sqlRepository;
        public GeneralCheckedApprovedController()
        {
            _sqlRepository = new SqlRepository();
        }
        [Authorize]
        public ActionResult GeneralApproved()
        {
            return View();
        }


        [HttpGet, Authorize]
        public JsonResult GetUNApprovalList(string POTypeApprovalStatus)
        {
            CheckQueryService checkQueryService = new CheckQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(checkQueryService.GetUNApprovalList(identity.PlantId, POTypeApprovalStatus), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetApprovedList()
        {
            CheckQueryService checkQueryService = new CheckQueryService(_sqlRepository);
            return Json(checkQueryService.GetApprovedList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult UpdateApprovalStatus(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string ApproveRejectReason)
        {
            CheckQueryService checkQueryService = new CheckQueryService(_sqlRepository);

            checkQueryService.UpdateApprovalStatus(PoId, PoValue, CheckedStataus, AuthorizedBy, ApproveRejectReason);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }
    }
}