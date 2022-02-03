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
    public class PerformanceManagementMasterController : BaseController
    {
        PerformanceManagementMasterService ps = new PerformanceManagementMasterService();
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PerformanceManagementMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion Constructor
      
        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        [Authorize, HttpPost]
        public ActionResult getEmployeetype()
        {
            return Json(ps.getEmployeeTypeId(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult GetChildList(string Id)
        {
            try
            {
                var _child = ps.GetChild(Id);
                return Json(new { child = _child }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }      
        [HttpPost, Authorize]
        public ActionResult GetList()
        {
            try
            {
                return Json(ps.GetList(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            try
            {
                return Json(ps.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost,Authorize]
        public JsonResult Create(Dictionary<string, object> datas, List<string> Employee)
        {
            try
            {
                var data = ps.Create(datas, Employee);
                return Json(new { Error = false, Data = data, Sequence = ps.GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            try
            {
                ps.Delete(id);

                return Json(new { Error = false, Sequence = ps.GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }      
    }
}