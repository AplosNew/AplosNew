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
    public class ResidenceStatusLocationController : Controller
    {
        ResidenceStatusLocationService rsl = new ResidenceStatusLocationService();
        private readonly ISqlRepository _sqlRepository;
        public ResidenceStatusLocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult getPlant()
        {
            return Json(rsl.getPlant(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getLocation(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getLocation(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceGroup()
        {
            return Json(rsl.getResidenceGroup(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceCategory(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getResidenceCategory(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceSubCategory(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getResidenceSubCategory(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getBlock(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getBlock(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getRoom(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getRoom(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getEmployeeType()
        {
            return Json(rsl.getEmployeeType(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceNumber(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getResidenceNumber(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getFloor(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getFloor(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidentType(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getResidentType(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getAssetName(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getAssetName(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        #region Save Operations
        [HttpPost]
        public JsonResult Save(Dictionary<string, object> data)
        {

            try
            {
                return Json(new { Error = "No", Data = rsl.Save(data), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getData()
        {
            try
            {
                return Json(rsl.getData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            try
            {
                rsl.delete(id);
                return Json(new { Message = "Data deleted successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion Save Operations
    }
}