using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Attendances;
using Library.HumanResource.Shift;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
//using TBS;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class WeeklyOffController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        WeeklyOffService rs = new WeeklyOffService();
        public WeeklyOffController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion



        #region Aplos       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        //#region -- Operations


        [HttpGet, Authorize]
        public ActionResult getMaster()
        {
            return Json(rs.getMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getChilds(string Id)
        {
            var data = rs.getDateChild(Id);
            var data1 = rs.getDayChild(Id);
            return Json(new { Dates = data, Days = data1 }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult saveMasters(Dictionary<string, object> Master, List<Dictionary<string, object>> Effective)
        {
            try
            {
                string id = rs.saveMasters(Master, Effective);
                return Json(new { Error = false, Data = Master, ids = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        //[HttpPost]
        //public ActionResult deleteMaster(string id)
        //{
        //    string jj = rs.deleteMaster(id);
        //    if (jj == "Success")
        //    {
        //        return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
        //    }
        //    else
        //    {
        //        return Json(new { Error = true, Data = id, Message = jj });
        //    }
        //}

        [HttpPost]
        public JsonResult SaveDays(List<Dictionary<string, object>> Week , string HeaderId)
        {
            try
            {
                rs.SaveDays(Week , HeaderId);
                return Json(new { Error = false, Data = Week, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
    }
}