#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using Library.Service.EmployeeServices;
#endregion Using

namespace Aplos.Areas.Leave.Controllers
{
    public class LeaveYearDefinationController  : BaseController
    {
        string TableName = "LeaveYearDefination";

        #region Constructor

        LeaveYearDefinationService _leave = new LeaveYearDefinationService();
        private readonly ISqlRepository _sqlRepository;

        public LeaveYearDefinationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
     
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_leave.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _leave.Get(Id);
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
            return Json(_leave.GetList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string ret = _leave.Create(data);
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

        [HttpGet, Authorize]
        public ActionResult GetEmps()
        {
            try 
            {                
                return Json(_leave.GetEmps(), JsonRequestBehavior.AllowGet);
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
                string ret = _leave.Delete(id);

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