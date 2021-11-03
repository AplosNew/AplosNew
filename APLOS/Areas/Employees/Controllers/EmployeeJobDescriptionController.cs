#region Using
using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeJobDescriptionController : BaseController
    {
        #region Constructor
        private readonly IJobDescriptionService _jobDescriptionService;
        public EmployeeJobDescriptionController(IJobDescriptionService jobDescriptionService)
        {
            _jobDescriptionService = jobDescriptionService;
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
        public ActionResult GetJobDescriptionList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionService.GetEmployeeJobDescription(identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetActivityDocumentList(string SOPActivityId)
        {
            return Json(_jobDescriptionService.GetActivityDocumentList(SOPActivityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSOPDocumentList(string SOPItemId)
        {
            return Json(_jobDescriptionService.GetSOPDocumentList(SOPItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFileList(string jdId)
        {
            return Json(_jobDescriptionService.GetFileByJDId(jdId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}