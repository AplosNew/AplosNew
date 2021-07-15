using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
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
    public class DayStatusMasterController : BaseController
    {
        #region Constructor

        DayStatusService ds = new DayStatusService();
        public DayStatusMasterController()
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

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(ds.GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getMaster()
        {
            return Json(ds.getMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getChildData (string MasterId)
        {
            return Json(ds.getChildData(MasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDayTypes()
        {
            return Json(ds.getDayTypes(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDefaultDayStatus()
        {
            return Json(ds.getDefaultDayStatus(), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult saveMaster(Dictionary<string, object> Master)
        //{
        //    try
        //    {
        //        var id = ds.saveMaster(Master);
        //        return Json(new { Error = false, Data = id,  Message = AplosMessage.Success });

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message });
        //    }
            
        //}

        //[HttpPost]
        //public ActionResult deleteMaster(string id)
        //{
        //    string jj = ds.deleteMaster(id);
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
        public ActionResult saveChild(Dictionary<string, object> Child)
        {
            try
            {
                var id = ds.saveChild(Child);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        /// ************************** New Operations 
        /// Getting the Employee List
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


        /// Header Get
        [HttpGet, Authorize]
        public ActionResult getHeader()
        {
            return Json(ds.getHeader(), JsonRequestBehavior.AllowGet);
        }

        /// Header Sequence
        [HttpGet, Authorize]
        public JsonResult GetAutoSequenceHeader()
        {
            return Json(ds.GetSequenceHeader(), JsonRequestBehavior.AllowGet);
        }

        //Header Save
        [HttpPost]
        public ActionResult saveHeader(Dictionary<string, object> Header)
        {
            try
            {
                var id = ds.saveHeader(Header);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }


        // ********************************************* Day Type With Values Functions
        //Getting The Day Type Child List
        [HttpPost, Authorize]
        public ActionResult getDayTypeChild(string Id)
        {
            return Json(ds.getDayTypeChild(Id), JsonRequestBehavior.AllowGet);
        }
        // Saving the Day Type With Values
        [HttpPost]
        public ActionResult saveDayTypeChild(Dictionary<string, object> DayTypeChild)
        {
            try
            {
                var id = ds.saveDayTypeChild(DayTypeChild);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        //Sequence for the Day Status Child
        [HttpGet, Authorize]
        public JsonResult GetAutoSequenceDayStatus()
        {
            return Json(ds.GetAutoSequenceDayStatus(), JsonRequestBehavior.AllowGet);
        }

        //Saving of the Day status Child
        [HttpPost]
        public ActionResult saveDayStatusChild(Dictionary<string, object> DaystatusChild)
        {
            try
            {
                var id = ds.saveDayStatusChild(DaystatusChild);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        //Getting the Day Status Child
        [HttpPost, Authorize]
        public ActionResult getDayStatusChild(string HeaderId)
        {
            return Json(ds.getDayStatusChild(HeaderId), JsonRequestBehavior.AllowGet);
        }
    }
}