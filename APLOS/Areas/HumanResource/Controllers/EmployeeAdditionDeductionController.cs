using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Employee;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeAdditionDeductionController : BaseController
    {
        #region Constructor

        EmployeeAdditionDeductionService ds = new EmployeeAdditionDeductionService();
        public EmployeeAdditionDeductionController()
        {
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        [HttpPost, Authorize]
        public ActionResult getPlants()
        {
            return Json(ds.getPlants(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getEmpType()
        {
            return Json(ds.getEmpType(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getDesignation(string empType)
        {
            return Json(ds.getDesignation(empType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(ds.GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getEmployees()
        {
            try
            {
                return Json(ds.getEmployees(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getEmploymentType()
        {
            return Json(ds.getEmploymentType(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public ActionResult getAdditionDeductionHead(string Type)
        {
            return Json(ds.getAdditionDeductionHead(Type), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getMaster()
        {
            return Json(ds.getMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getPeriodChildData (string MasterId)
        {
            return Json(ds.getPeriodChildData(MasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getPlantChildData(string MasterId)
        {
            return Json(ds.getPlantChildData(MasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDefaultDayStatus()
        {
            return Json(ds.getDefaultDayStatus(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult saveMaster(Dictionary<string, object> Master)
        {
            try
            {
                var id = ds.saveMaster(Master);
                return Json(new { Error = false, Data = id,  Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
            
        }

        [HttpPost]
        public ActionResult savePeriodChild(List<Dictionary<string, object>> Periods)
        {
            try
            {
                ds.savePeriodChild(Periods);
                return Json(new { Error = false, Data = Periods, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost]
        public ActionResult deleteMaster(string id)
        {
            string jj = ds.deleteMaster(id);
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = id, Message = jj });
            }
        }

        [HttpPost]
        public ActionResult DeleteChild(string id)
        {
            string jj = ds.DeleteChild(id);
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = id, Message = jj });
            }
        }

        [HttpPost]
        public ActionResult savePlantChild(Dictionary<string, object> Child)
        {
            try
            {
                var id = ds.savePlantChild(Child);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }
    }
}