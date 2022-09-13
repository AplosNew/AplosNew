using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.HumanResource.Employee;
using Aplos.Properties;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SurveyandFeedbackController : BaseController
    {
        SurveyAndFeedbackService sd = new SurveyAndFeedbackService();

       
        public ActionResult Aplos()
        {
            return View();
        }

        #region Save Operations
        [HttpPost, Authorize]
        public JsonResult Save(Dictionary<string, object> data)
        {

            try
            {
                return Json(new { Error = "No", Data = sd.Save(data), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Save Operations
    }
}