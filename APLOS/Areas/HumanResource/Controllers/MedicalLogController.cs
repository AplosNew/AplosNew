#region LIB
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Employee;
using Aplos.Properties;
using System.Data;
#endregion LIB

namespace Aplos.Areas.HumanResource.Controllers
{
    public class MedicalLogController : BaseController
    {
        MedicalLogServce ml = new MedicalLogServce();
        #region Views
        public ActionResult Aplos()
        {
            return View();
        }

        #region GET SEQUENCE
        [HttpPost, Authorize]
        public JsonResult CountEmpVisits(string empsystemCode)
        {
            try
            {
                return Json(ml.CountEmpVisits(empsystemCode), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion GET SEQUENCE

        #endregion Views
        [Authorize, HttpPost]
        public ActionResult getMedicineList()
        {
            try
            {
                return Json(ml.GetMedicineList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpPost]
        public ActionResult getMedicineByReceipt(string medicinemasterId)
        {
            try
            {
                return Json(ml.GetMedicineByReceipt(medicinemasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult medicallogGridView()
        {
            try
            {
               // ml.MedicallogGridView()
                //var data = _sqlRepository.GetDataCollection(sql);
                JsonResult json = Json(ml.MedicallogGridView(), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
               // return Json(ml.MedicallogGridView(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult getSicknessType()
        {
            try
            {
                return Json(ml.GetSicknessType(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult getEmployee()
        {
            try
            {
                return Json(ml.GetEmployee(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpGet]
        public ActionResult GetMedicineChildForUpdate(string masterId)
        {
            try
            {
                return Json(ml.GetMedicineChildForUpdate(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpGet]
        public ActionResult GetSicknessChildForUpdate(string masterId)
        {
            try
            {
                return Json(ml.GetSicknessChildForUpdate(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #region SEARCH SAVED DATA IN GRID 
        [HttpPost, Authorize]
        public ActionResult getSearchSicknessData(string column, string value)
        {
            return Json(ml.getSearchSicknessData(column, value), JsonRequestBehavior.AllowGet);
        }
        #endregion SEARCH SAVED DATA IN GRID

        #region SAVE
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> data, List<Dictionary<string, object>> medicinepurposelist, List<Dictionary<string, object>> medicinelist, string empSystemId)
        {
            try
            {
                return Json(new { Error = false, Data = ml.Save(data, medicinepurposelist, medicinelist, empSystemId), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE

        [HttpGet, Authorize]
        public ActionResult CountEmployeeVisiting(string empSytemId)
        {
            return Json(ml.CountEmployeeVisiting(empSytemId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult getSearchedEmployee(string column, string value)
        {
            try
            {
                return Json(ml.getSearchedEmployee(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}