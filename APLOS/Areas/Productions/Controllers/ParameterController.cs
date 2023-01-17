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
        #region HEADER
        [HttpGet, Authorize]
        public ActionResult GetResponsiblePerson()
        {
            try
            {
                return Json(pc.getResponsiblePerson(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #region Save
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> datas)
        {
            try
            {
                return Json(new { Error = false, Data = pc.Save(datas), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateParameter(string headerid, Dictionary<string, object> parameter)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateParameter(headerid, parameter), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateProductWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateProductWithParameterSetup(headerid, models), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateWorkcenterWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateWorkcenterWithParameterSetup(headerid, models), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion  Save

        #endregion HEADER

        #region GetFun

        [HttpGet, Authorize]
        public ActionResult GetProduct()
        {
            return Json(pc.getProduct(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMachine()
        {
            return Json(pc.getMachine(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetWorkcenter()
        {
            return Json(pc.getWorkcenter(), JsonRequestBehavior.AllowGet);
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

        [HttpGet, Authorize]
        public ActionResult GetSavedProduct(string headerid)
        {
            return Json(pc.GetSavedProduct(headerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedWorkcenter(string headerid)
        {
            return Json(pc.GetSavedWorkcenter(headerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedParameterChild(string headerid)
        {
            return Json(pc.GetSavedParameterChild(headerid), JsonRequestBehavior.AllowGet);
        }

        #endregion GetFun



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
        public ActionResult RemoveProduct(string productid)
        {
            try
            {

                string ret = pc.RemoveProduct(productid);

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

        public ActionResult RemoveWorkcenter(string workcenterid)
        {
            try
            {

                string ret = pc.RemoveProduct(workcenterid);

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