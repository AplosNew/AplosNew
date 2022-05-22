using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Properties;
using Library.Data.Sql;
using Library.HumanResource.Employee;
using Library.Security.Core;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class TrainingMasterController : Controller
    {
        string TableName = "HKP.TrainingMaster";
        TrainingMasterService tm = new TrainingMasterService();
        private readonly ISqlRepository _sqlRepository;
        public TrainingMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(tm.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        /*[Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = tm.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }*/



        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(tm.GetList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getAllEmployee()
        {
            try
            {
                var jsondata = Json(tm.getAllEmployee(), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult save(Dictionary<string, object> data, string EmpSystemId)
        {
            try
            {
                string ret = tm.save(data, EmpSystemId);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
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

        public ActionResult Delete(string id)
        {
            try
            {

                string ret = tm.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }
}