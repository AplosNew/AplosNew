using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.HumanResource;
using Aplos.Controllers;
using Aplos.Properties;
using Library.HumanResource.NewAttendanceProcess;
using Library.OrderManagement.Production;

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductiveAllowanceRateSetupController : BaseController
    {
        ProductiveAllowanceRateSetupService pa = new ProductiveAllowanceRateSetupService();
        public ProductiveAllowanceRateSetupController()
        { }

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region ProductiveAllowance

        #region GetOperations

        [Authorize, HttpPost]
        public ActionResult getProcess()
        {
            return Json(pa.getProcess(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            return Json(pa.getEntity(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getMasterData()
        {
            return Json(pa.getMasterData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getPaChildList(string Id)
        {
            return Json(pa.getPaChildList(Id), JsonRequestBehavior.AllowGet);
        }

        #endregion GetOperations


        #region Savings
        [HttpPost]
        public ActionResult saveHeaderPa(Dictionary<string, object> headerData, List<string> process, List<string> entity)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveHeaderPa(headerData, process, entity), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Yes", Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult saveChildPa(List<Dictionary<string, object>> childData, string headerId)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveChildPa(childData, headerId), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Savings

        #endregion ProductiveAllowance

        #region RateSetup

        #region Savings
        [HttpPost]
        public ActionResult saveHeaderRs(Dictionary<string, object> headerData, List<string> process, List<string> entity)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveHeaderRs(headerData, process, entity), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Yes", Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Savings

        #endregion

    }
}