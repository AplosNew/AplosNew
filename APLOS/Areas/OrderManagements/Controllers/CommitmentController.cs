#region Using

using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Aplos.Controllers;

#endregion Using

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class CommitmentController : BaseController
    {
        #region Constructor

        private readonly ICommitmentService _commitmentService;
        private readonly ICommitmentMonthService _commitmentMonthService;

        public CommitmentController(ICommitmentService commitmentService
            , ICommitmentMonthService commitmentMonthService)
        {
            _commitmentService = commitmentService;
            _commitmentMonthService = commitmentMonthService;
        }

        #endregion Constructor

        #region -- Pages

       
        public ActionResult Aplos()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(identity.EmployeeId))
                ViewBag.flag = false;
            else
                ViewBag.flag = true;
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string entityId)
        {
            return Json(_commitmentService.Query(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMonthList(string masterId)
        {
            return Json(_commitmentMonthService.Query(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_commitmentService.GetMaterialMasterList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_commitmentService.GetProductMasterList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult QueryCommitmentValueAdded(string masterId)
        {
            return Json(_commitmentService.QueryCommitmentValueAdded(masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetCommitmentData()
        {
            return Json(_commitmentService.GetCommitmentData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesGroupCbo(string entityId)
        {
            return Json(new SelectList(_commitmentService.GetSalesGroupCbo(entityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_commitmentService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Commitment commitment, IEnumerable<CommitmentMonth> monthList, IEnumerable<CommitmentValueAddedProcess> cvAddedList)
        {
            _commitmentService.Insert(commitment, monthList, cvAddedList);
            return Json(new { commitment, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Commitment commitment, IEnumerable<CommitmentMonth> monthList, IEnumerable<CommitmentValueAddedProcess> cvAddedList)
        {
            _commitmentService.Update(commitment, monthList, cvAddedList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _commitmentService.DeleteMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult DeleteProcess(string id)
        {
            _commitmentService.DeleteProcess(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}