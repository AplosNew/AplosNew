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
using Library.HumanResource.NewAttendanceProcess;
#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PerformancePeriodController : BaseController
    {
        PerformancePeriodMasterService pp = new PerformancePeriodMasterService();
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PerformancePeriodController(ISqlRepository R)
        {
            _sqlRepository = R;
            pp = new PerformancePeriodMasterService();
        }       

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion 

        #region Functions
     
        [HttpPost]
        public ActionResult GetList()
        {
            try
            {
                return Json(pp.GetList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult Delete(string SystemId)
        {
            try
            {
                pp.Delete(SystemId);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost,Authorize]
        public JsonResult Create(Dictionary<string, object> Data)
        {
            try
            {
                var data = pp.Create(Data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        
        #endregion
    }
}


