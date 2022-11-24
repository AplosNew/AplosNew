using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Parameter;
using Aplos.Properties;
using System.Data;

namespace Aplos.Areas.Productions.Controllers
{
    public class ParameterController : BaseController
    {
        ParameterService ps = new ParameterService();
        ParameterChild pc = new ParameterChild();
        public ActionResult Aplos()
        {
            return View();
        }
        [HttpGet, Authorize]
        public ActionResult GetParameterMaster()
        {
            return Json(ps.GetParameter(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetMachineMaster()
        {
            return Json(ps.GetMachineMaster(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            return Json(pc.GetList(), JsonRequestBehavior.AllowGet);
        }
        #region SAVE
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = pc.Save(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE

        #region Update
        [HttpPost]
        public ActionResult Update(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = pc.Update(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Update

        #region DELETE
        public ActionResult Delete(string id)
        {
            try
            {

                string ret = pc.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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