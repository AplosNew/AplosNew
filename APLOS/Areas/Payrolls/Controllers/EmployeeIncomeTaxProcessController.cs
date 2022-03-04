using Aplos.Controllers;
using System;
using System.Web.Mvc;
using Library.HumanResource.Payroll.Tax;
using Aplos.Properties;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class EmployeeIncomeTaxProcessController : BaseController
    {
        #region Constructor

        IncomeTaxProcessService rep = new IncomeTaxProcessService();

        public EmployeeIncomeTaxProcessController()
        {
            rep = new IncomeTaxProcessService();
        }
        #endregion

        #region View
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        [HttpPost, Authorize]
        public JsonResult GetData(string PolicyId, string Earning, string PlantId)
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.getGridData(PolicyId,Earning,PlantId) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetPlants()
        {
            try
            {
                return Json(rep.getPlants(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult ProcessFunction(string PolicyId, string YearId, string PlantId,string EmpId)
        {
            try
            {
                rep.ProcessIncomeTax(PolicyId, YearId,PlantId,EmpId);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

    }
}
 