using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.DailyAttendance.Controllers
{
	public class PreDashBoardController : BaseController
	{
		private readonly IPreRecruitmentDashboardService _dashboardService;

		public PreDashBoardController(IPreRecruitmentDashboardService PreRecruitmenDashboardService)
		{
			_dashboardService = PreRecruitmenDashboardService;
		}

		//GET: Recruitments/PreRecruitment
		
		public ActionResult Aplos()
		{
			return View();
		}

		[HttpPost, Authorize]
		public ActionResult GetOrgStrunctureList()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.OrgStructureList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult OverAllStatus()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var k = _dashboardService.OverAllStatus(identity.CompanyGroupId, identity.CompanyId);

			var NotSelDoc = _dashboardService.NotSelDoc(identity.CompanyGroupId, identity.CompanyId);
			var selDoc = _dashboardService.SelDoc(identity.CompanyGroupId, identity.CompanyId);
			var selDocOVD = _dashboardService.SelDocOVD(identity.CompanyGroupId, identity.CompanyId);

			var LoggedInDoc = _dashboardService.LoggedInDoc(identity.CompanyGroupId, identity.CompanyId);
			var LoggedInDocOVD = _dashboardService.LoggedInDocOVD(identity.CompanyGroupId, identity.CompanyId);

			var NotLoggedInDoc = _dashboardService.NotLoggedInDoc(identity.CompanyGroupId, identity.CompanyId);
			var NotLoggedInDocOVD = _dashboardService.NotLoggedInDocOverDue(identity.CompanyGroupId, identity.CompanyId);

			var PreDocNotSubmitted = _dashboardService.PreDocNotSubmitted(identity.CompanyGroupId, identity.CompanyId);
			var PreDocSubmitted = _dashboardService.PreDocSubmitted(identity.CompanyGroupId, identity.CompanyId);

			return Json(new { fg = k, selDoc, selDocOVD, LoggedInDoc, NotLoggedInDoc, NotLoggedInDocOVD, LoggedInDocOVD, PreDocNotSubmitted, PreDocSubmitted }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult NotConfirmeddoc()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.NotConfirmedDoc(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult IntervieweeDocuments(string EmpId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.EmployeeWiseDoument(EmpId, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult IntervieweeDocumentsDept(string EmpId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.EmployeeWiseDoumentDept(EmpId, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		//
		[HttpPost, Authorize]
		public ActionResult GetIntervieweeDocumentsSelfNU(string EmpId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.EmployeeWiseNotUploadedDoumentSelf(EmpId, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult GetIntervieweeDocumentsDeptNU(string EmpId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.EmployeeWiseNotUploadedDoumentDept(EmpId, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		//
		[HttpGet, Authorize]
		public ActionResult GetListSelTotalInterviewee(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.ListSelTotalInterviewee(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetListNotSelectedEmp(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.ListNotSelectedEmp(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetListOverDueTotalInterviewee(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.ListOverDueTotalInterviewee(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult SubmittedButNotConfirmed(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.SubmittedButNotConfirmed(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetListLoggedInInterviewee(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.ListLoggedInInterviewee(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetListODLoggedInInterviewee(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.ListODLoggedInInterviewee(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetListNotLoggedInInterviewee(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.ListNotoggedInInterviewee(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetListODNotLoggedInInterviewee(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.ListODNotoggedInInterviewee(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult GetDocumentUploadingStatus(string status)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_dashboardService.DocumentUploadingStatus(identity.CompanyGroupId, identity.CompanyId, status), JsonRequestBehavior.AllowGet);
		}
	}
}