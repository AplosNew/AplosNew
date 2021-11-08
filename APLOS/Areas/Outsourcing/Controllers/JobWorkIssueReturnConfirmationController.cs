using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.MaterialManagement.JobWork;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkIssueReturnConfirmationController : BaseController
    {
        JobWorkIssueReturnConfirmation IC = new JobWorkIssueReturnConfirmation();

        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkIssueReturnConfirmationController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
            IC = new JobWorkIssueReturnConfirmation();
        }
        #endregion
        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        

        [Authorize, HttpPost]
        public JsonResult GetSearchedData(string FromDate, string ToDate, string Status, string PartyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(IC.GetSearchedData(FromDate, ToDate, Status, PartyId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpPost]
        public JsonResult LoadAllPartyDetailsForSelection(string Id)
        {
            try
            {

                return Json(IC.LoadAllPartyDetailsForSelection(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public JsonResult SaveConfirmedIssueChildTab(IEnumerable<JobWorkConfirmationIssue> ConfirmedIssueChildData, string IsConfirmed)
        {
            try
            {
                IC.SaveConfirmedIssueChildTab(ConfirmedIssueChildData, IsConfirmed);
                return Json(new { Error = false, CData = ConfirmedIssueChildData, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        // TRANSFORMATION CONFIRMATION ISSUE CHILD

        [Authorize, HttpPost]
        public JsonResult GetSearchTransConfirmationIssue(string FromDate, string ToDate, string Status)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(IC.GetSearchTransConfirmationIssue(FromDate, ToDate, Status), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpPost]
        public JsonResult LoadAllPartyVendorForSelection(string Id)
        {
            try
            {

                return Json(IC.LoadAllPartyVendorForSelection(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public JsonResult SaveTransConfirmationIssueChildTab(IEnumerable<JobWorkTransformationConfirmationIssue> TransConfirmedIssueChildData, string IsConfirmed)
        {
            try
            {
                IC.SaveTransConfirmationIssueChildTab(TransConfirmedIssueChildData, IsConfirmed);
                return Json(new { Error = false, CData = TransConfirmedIssueChildData, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }



    }
}