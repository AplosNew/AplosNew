#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using Library.General.AdministrationTasks;
#endregion Using

namespace Aplos.Areas.Administration.Controllers
{
    public class VehicleMovementLocationsController : BaseController
    {
       
        #region Constructor
        VehicleMovementLocationsService vl = new VehicleMovementLocationsService();
        private readonly ISqlRepository _sqlRepository;

        public VehicleMovementLocationsController(ISqlRepository R)
        {
            _sqlRepository = R;
            vl = new VehicleMovementLocationsService();
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
                return Json(vl.GetCbo(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = vl.Get(Id);
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
                return Json(vl.GetList(column, value), JsonRequestBehavior.AllowGet);
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
                return Json(vl.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string ret = vl.Create(data);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Sequence = vl.GetSequence(), Message = AplosMessage.Updated });
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

                string ret = vl.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = vl.GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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