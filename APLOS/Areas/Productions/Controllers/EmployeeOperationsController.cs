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
using Library.OrderManagement.Production;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class EmployeeOperationsController : BaseController
    {

        EmployeeOperationsService eo = new EmployeeOperationsService();
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public EmployeeOperationsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }
      
        [HttpGet , Authorize]
        public ActionResult GetWorkCenter()
        {
            return Json(eo.GetWorkCenter(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcess()
        {
            return Json(eo.GetProcess(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet, Authorize]
        public ActionResult GetPeriod()
        {
            return Json(eo.GetPeriod(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetShift()
        {
            return Json(eo.GetShift(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPOs(string wk)
        {
            return Json(eo.GetPOs(wk), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetOperationsData(string PId , string Period)
        {
            return Json(eo.GetOperationsData(PId , Period) , JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getReportView()
        {
            return Json( new {Data = eo.getReportView(out List<string> Cols) , Cols = Cols } , JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult saveData(List<Dictionary<string, object>> data , string WorkCenter , string ProcessId ,  string ShiftId , string POId , string Date , string PeriodId)
        {
            try
            {
                eo.saveData( data,  WorkCenter,  ProcessId,  ShiftId,  POId,  Date , PeriodId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
    }
}