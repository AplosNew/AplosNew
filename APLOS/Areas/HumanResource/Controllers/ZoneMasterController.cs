#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.Employee;


#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ZoneMasterController : Controller
    {
        ZoneMasterService f = new ZoneMasterService();
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ZoneMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult GetMaster(string Id)
        {
            try
            {
                return Json(f.GetMaster(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        
        [HttpPost]
        public ActionResult GetList()
        {
            try
            {
                return Json(f.GetList(), JsonRequestBehavior.AllowGet);
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
                return Json(f.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult getEmployee()
        {
            try
            {
                var jsondata = Json(f.getEmployee(), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> datas, string Employee)

        {
            try
            {
                var data = f.Save(datas, Employee);
                return Json(new { Error = false, Data = data, Sequence = f.GetSequence(), Message = AplosMessage.Updated });

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
                f.Delete(id);

                return Json(new { Error = false, Sequence = f.GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
    }
}