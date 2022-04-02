using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Aplos.Properties;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Security.Core;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PerformanceGradeMasterController : Controller
    {
        //string TableName = "dbo.PerformanceAttributeMaster";

        PerformanceGradeMasterService pg = new PerformanceGradeMasterService();

        private readonly ISqlRepository _sqlRepository;
        public PerformanceGradeMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
            pg = new PerformanceGradeMasterService();

        }

        public ActionResult Aplos()
        {
            return View();
        }



        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = pg.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(pg.GetList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string ret = pg.Create(data);
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

        public ActionResult Delete(string id)
        {
            try
            {

                string ret = pg.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
    }
}