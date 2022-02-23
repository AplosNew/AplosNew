#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using Library.General.AdministrationTasks;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Administration.Controllers
{

    public class ServicesApprovingAuthorityController : BaseController
    {

        #region Constructor
        ServicesApprovingAuthorityClass sa = new ServicesApprovingAuthorityClass();
        private readonly ISqlRepository _sqlRepository;

        public ServicesApprovingAuthorityController(ISqlRepository R)
        {
            _sqlRepository = R;
            sa = new ServicesApprovingAuthorityClass();
        }

        #endregion Constructor

        #region Views

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region Functions

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            try
            {
                return Json(sa.GetCbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = sa.Get(Id);
                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            try
            {
                return Json(sa.GetList(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            try
            {
                return Json(sa.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string ret = sa.Create(data);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Sequence = sa.GetSequence(), Message = AplosMessage.Updated });
                }
                else
                {
                    return Json(new { Error = true, Message = ret });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            try
            {

                string ret = sa.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = sa.GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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

        #endregion
    }
}