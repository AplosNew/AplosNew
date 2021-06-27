#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Library.Service.PerformanceManagement;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.PerformanceManagement.Controllers
{
    public class JobEvaluationController : BaseController
    {
        JobEvaluation JE = new JobEvaluation();
       
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public JobEvaluationController(ISqlRepository R)
        {
            _sqlRepository = R;
            JE = new JobEvaluation();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            try
            {
                return Json(JE.GetList(column, value), JsonRequestBehavior.AllowGet);
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
                JE.Create(data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(string Id)
        {
            try
            {
                JE.Delete(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllPositionDetailsForSelection(string Id)
        {
            try
            {
                var jsondata = Json(JE.LoadAllPositionDetailsForSelection(Id), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        // Job Evaluation Child

        [HttpGet, Authorize]
        public ActionResult getjobevalattributelist()
        {
            try
            {
                return Json(JE.getjobevalattributelist(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllJobEvalDetailsForSelection(string MasterId, string JobEvalAttributeId)
        {
            try
            {
                return Json(JE.LoadAllJobEvalDetailsForSelection(MasterId, JobEvalAttributeId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public JsonResult SaveJobEvalChildData(Dictionary<string, object> data, string MasterId)
        {
            try
            {
                JE.SaveJobEvalChildData(data, MasterId);
                return Json(new { Error = false, CData = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet]
        public ActionResult DelJobEChild(string Id)
        {
            try
            {
                JE.DelJobEChild(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public ActionResult getJobEvalChildData(string MasterId)
        {
            try
            {
                return Json(JE.getJobEvalChildData(MasterId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllEvaluatorDetails(string Id)
        {
            try
            {
                var jsondata = Json(JE.LoadAllEvaluatorDetails(Id), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult LoadApprovedbyDetails(string Id)
        {
            try
            {
                var jsondata = Json(JE.LoadApprovedbyDetails(Id), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }


    }
}