#region Using
using Aplos.Controllers;
using System.Web.Mvc;
using Library.HumanResource.NewAttendanceProcess;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class SandwichProcessController : BaseController
    {
        #region Constructor

        SandwichProcessService ss = new SandwichProcessService();

        public SandwichProcessController(
            )
        {
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string month , string year)
        {
            var jsondata = Json(ss.GetEmployeeInformation(month, year), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult Process(string month, string year)
        {
            var jsondata = Json(ss.GetEmployeeInformation(month, year), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
    }
}