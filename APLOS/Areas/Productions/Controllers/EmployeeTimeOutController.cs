#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.OrderManagement.Production;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class EmployeeTimeOutController : BaseController
    {

        EmployeeTimeOutService et = new EmployeeTimeOutService();

        #region Constructor

        public EmployeeTimeOutController()
        {

        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {

            return View();
        }
        #endregion

        #region GetOperations

        [HttpGet, Authorize]
        public ActionResult getEmployees()
        {
            return Json(et.getEmployees(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getEmpTimeOut(string EmpId , string Date)
        {
            return Json(et.getEmpTimeOut(EmpId , Date), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Saving


        [HttpPost]
        public JsonResult Create(string EmployeeId, string Date, string FromTime, string ToTime )
        {
            et.Create(EmployeeId,  Date,  FromTime,  ToTime);
            return Json(new { Error = false, Message = AplosMessage.Updated });

        }

        #endregion
    }
}