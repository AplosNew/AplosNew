using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Employee;
using Aplos.Properties;
using System.Data;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class FiveSZoneMasterController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        FiveSZoneService fs = new FiveSZoneService();

        public FiveSZoneMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
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
                var _master = fs.Get(Id);


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
            return Json(fs.GetList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getEmployee()
        {
            try
            {
                return Json(fs.getEmployee(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult Save(Dictionary<string, object> data, string responsiblePerson)
        {
            try
            {
                return Json(new { Error = false, Data = fs.Save(data, responsiblePerson), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       
        public ActionResult Delete(string id)
        {
            try
            {

                string ret = fs.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetAutoSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            try
            {
                return Json(fs.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getResponsiblePersonId(string ResponsiblePersonId)
        {
            try
            {
                return Json(fs.getResponsiblePersonId(ResponsiblePersonId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}