using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Properties;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Helpers;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ResidenceMasterController : Controller
    {
        //ResidenceMaseterService rm = new ResidenceMaseterService();
        ResidenceAllocationService rm = new ResidenceAllocationService();
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetResidenceMaster()
        {
            try
            {
                return Json(rm.GetResidenceMaster(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getPlant()
        {
            try
            {
                return Json(rm.getPlant(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getResidenceGroup()
        {
            try
            {
                return Json(rm.getResidenceGroup(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getEmployeeCategory()
        {
            try
            {
                return Json(rm.getEmployeeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getEmpServiceType()
        {
            try 
            {
                return Json(rm.getEmpServiceType(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Save Operations
        [HttpPost]
        public JsonResult Save(Dictionary<string, object> data, string PlantId, string ResidenceGroupId, string Emp, string ServiceTypeId)
        {

            try
            {
                rm.Save(data, PlantId, ResidenceGroupId, Emp, ServiceTypeId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Save Operations

        #region TAB POSITION
        [HttpPost]
        public ActionResult getEntity()
        {
            try
            {
                return Json(rm.getEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getBudgetCode(string entityId)
        {
            try
            {
                return Json(rm.getBudgetCode(entityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getPositionCode(string MPBudgetId)
        {
            try
            {
                return Json(rm.getBudgetCode(MPBudgetId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getPositionTabGridData()
        {
            try
            {
                return Json(rm.getPositionTabGridData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion TAB POSITION
    }
}