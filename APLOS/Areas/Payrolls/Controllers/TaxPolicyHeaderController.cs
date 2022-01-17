using Aplos.Controllers;
using Aplos.Properties;
using Library.HumanResource.Payroll.Tax;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class TaxPolicyHeaderController : BaseController
    {
        #region Constructor

        TaxPolicyMasterService ds = new TaxPolicyMasterService();
        public TaxPolicyHeaderController()
        {
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

     
        #region PlantChild Actions

        [HttpPost, Authorize]
        public ActionResult getChildData (string MasterId)
        {
            return Json(ds.getChildData(MasterId), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult DeleteChild(string id)
        {
            string jj = ds.DeleteChild(id);
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = id, Message = jj });
            }
        }

        [HttpPost]
        public ActionResult saveChild(Dictionary<string, object> Child)
        {
            try
            {
                var id = ds.saveChild(Child);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

        #region Header Functions

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(ds.GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getMaster()
        {
            return Json(ds.getMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getHeader()
        {
            return Json(ds.getHeader(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequenceHeader()
        {
            return Json(ds.GetSequenceHeader(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult saveHeader(Dictionary<string, object> Header)
        {
            try
            {
                var id = ds.saveHeader(Header);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

        #region Rules Screen Functions

        [HttpPost, Authorize]
        public ActionResult GetEarningMasterList(string Id)
        {
            return Json(ds.GetEarningMasterList(Id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getSalaryHeadList()
        {
            try
            {
                return Json(ds.getSalaryHeadList(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveEarningMaster(Dictionary<string, object> EarningMasterData)
        {
            try
            {
                var id = ds.SaveEarningMasterChild(EarningMasterData);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

    }
}