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
    public class PerformanceAttributeMasterController : Controller
    {
        string TableName = "dbo.PerformanceAttributeMaster";
        PerformaceAttributeMasterService pa = new PerformaceAttributeMasterService();
        private readonly ISqlRepository _sqlRepository;
        public PerformanceAttributeMasterController(ISqlRepository R)
        { _sqlRepository = R; }
        // GET: HumanResource/PerformanceAttributeMaster
        public ActionResult Aplos()
        {
            return View();
        }

       

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = pa.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        
        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string ret = pa.Create(data);
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

                string ret = pa.Delete(id);

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