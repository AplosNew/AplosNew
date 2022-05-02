using Aplos.Properties;
using Library.HumanResource.NewAttendanceProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ScatteredWeekOffController : Controller
    {
        ScatteredWeekService sc = new ScatteredWeekService();
        public ActionResult Aplos()
        {
            return View();
        }

        #region Scattered Week Definition
        [HttpGet, Authorize]
        public ActionResult getWeeksList()
        {
            return Json(sc.getWeeksList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getCurrentWeekDef()
        {
            return Json(sc.getCurrentWeekDef(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveAllDef(Dictionary<string, string> data)
        {
            try
            {
                string ret = sc.SaveAllDef(data);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
                }
                else
                {
                    return Json(new { Error = true, Message = ret });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        #endregion

        #region Scattered Week Master

        [HttpGet, Authorize]
        public ActionResult getCompany()
        {
            return Json(sc.getCompany(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getPlants(string cmp)
        {
            return Json(sc.getPlants(cmp), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getCurrWeeksList(string HeaderId)
        {
            return Json(sc.getCurrWeeksList(HeaderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getMasterData()
        {
            return Json(sc.getMasterData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, string> masterData, List<Dictionary<string, string>> childData)
        {
            try
            {
                string ret = sc.Create(masterData, childData);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = masterData, Message = AplosMessage.Updated });
                }
                else
                {
                    return Json(new { Error = true, Message = ret });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult DeleteChild(string id)
        {
            string jj = sc.DeleteChild(id);
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = id, Message = jj });
            }
        }
        #endregion
    }
}