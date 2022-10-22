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

        
        #endregion Views
        [Authorize, HttpPost]
        public ActionResult getMedicineList()
        {
            try
            {
                return Json(ml.getMedicineList(), JsonRequestBehavior.AllowGet);
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
                return Json(ml.getMedicineByReceipt(medicinemasterId), JsonRequestBehavior.AllowGet);
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
                return Json(ml.medicallogGridView(), JsonRequestBehavior.AllowGet);
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
                return Json(ml.getSicknessType(), JsonRequestBehavior.AllowGet);
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
                return Json(ml.getEmployee(), JsonRequestBehavior.AllowGet);
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

    }
}