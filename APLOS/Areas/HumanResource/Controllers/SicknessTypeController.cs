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
    public class SicknessTypeController : BaseController
    {
        
        SicknessTypeService st = new SicknessTypeService();

        #region const
        public SicknessTypeController()
        {

        }
        #endregion const

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region GET
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = st.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion GET

        #region SAVE
        [HttpPost, Authorize]
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = st.Save(data), Message = AplosMessage.Success });
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

                string ret = st.Delete(id);

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