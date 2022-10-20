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
    public class MedicineReceiptController : BaseController
    {
        MedicineReceiptService mr = new MedicineReceiptService();
        #region PAGE
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion PAGE

        #region GET FUN
        [Authorize, HttpPost]
        public ActionResult getMedicineData()
        {
            try
            {
                return Json(mr.getMedicineData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult getMedicineReceipt()
        {
            try
            {
                return Json(mr.getMedicineReceipt(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult getPlant()
        {
            try
            {
                return Json(mr.getPlant(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion GET FUN
        #region SEARCH SAVED DATA IN GRID 
        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(mr.GetList(column, value), JsonRequestBehavior.AllowGet);
        }
        #endregion SEARCH SAVED DATA IN GRID
        #region SAVE

        [HttpPost]
        public ActionResult SaveHeader(Dictionary<string, object> data, List<Dictionary<string, object>> medicinelist, string partyId)
        {
            try
            {
                return Json(new { Error = false, Data = mr.SaveHeader(data, medicinelist, partyId), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE
    }
}