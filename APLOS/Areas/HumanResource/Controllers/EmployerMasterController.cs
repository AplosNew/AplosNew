#region LIB
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Recruitement;
using Aplos.Properties;
using System.Data;
#endregion LIB

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployerMasterController : Controller
    {
        EmployerMasterService em = new EmployerMasterService();
        public ActionResult Aplos()
        {
            return View();
        }

        #region GET SEQUENCE
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            try
            {
                return Json(em.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion GET SEQUENCE

        #region SEARCH SAVED DATA IN GRID 
        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(em.GetList(column, value), JsonRequestBehavior.AllowGet);
        }
        #endregion SEARCH SAVED DATA IN GRID
        #region SAVE
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = em.Save(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE

        #region DELETE
        public ActionResult Delete(string id)
        {
            try
            {

                string ret = em.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetAutoSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion DELETE

        
    }

    
}