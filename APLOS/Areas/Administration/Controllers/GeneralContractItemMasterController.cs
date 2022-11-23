using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.Service.Administration.Contract;
using Aplos.Properties;
using System.Data;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralContractItemMasterController : BaseController
    {
        GeneralContractService gc = new GeneralContractService();
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
                return Json(gc.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion GET SEQUENCE

        #region SEARCH SAVED DATA IN GRID 
        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(gc.GetList(column, value), JsonRequestBehavior.AllowGet);
        }

        
        #endregion SEARCH SAVED DATA IN GRID
        #region SAVE
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = gc.Save(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE

        #region DELETE
        public ActionResult Delete(string id)
        {
            try
            {

                string ret = gc.Delete(id);

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