#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Service.PerformanceManagement;


#endregion Using

namespace Aplos.Areas.PerformanceManagement.Controllers
{
    public class JobEvaluationMasterController : BaseController
    {
        JobEvaluationMaster JEM = new JobEvaluationMaster();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public JobEvaluationMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
            JEM = new JobEvaluationMaster();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult getemployeecategorylist()
        {
            try
            {
                return Json(JEM.getemployeecategorylist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public JsonResult getperformanceattributelist()
        {
            try
            {
                return Json(JEM.getperformanceattributelist(), JsonRequestBehavior.AllowGet);
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
                return Json(JEM.GetList(column, value), JsonRequestBehavior.AllowGet);
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
                JEM.Create(data);
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
                JEM.Delete(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public JsonResult SaveDimensionDetails(Dictionary<string, object> data, Dictionary<string, object> JEChildData)
        {
            try
            {
                JEM.SaveDimensionDetails(data, JEChildData);
                return Json(new { Error = false, Data = data, CData = JEChildData, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getJEMChildData(string JobEvaluationMasterId)
        {
            try
            {
                return Json(JEM.getJEMChildData(JobEvaluationMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult DeleteJEMChild(string Id)
        {
            try
            {
                JEM.DeleteJEMChild(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        // Save Job Evaluation Child 2

        [HttpPost]
        public JsonResult CreateDimDetails(Dictionary<string, object> data, Dictionary<string, object> JEMChildDetails)
        {
            try
            {
                JEM.CreateDimDetails(data, JEMChildDetails);
                return Json(new { Error = false, Data = data, CData = JEMChildDetails, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getJEMChildDetails(string JobEvaluationMasterId)
        {
            try
            {
                return Json(JEM.getJEMChildDetails(JobEvaluationMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult DelJEMChild2(string Id)
        {
            try
            {
                JEM.DelJEMChild2(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        // Employee Category

        [HttpPost, Authorize]
        public ActionResult LoadAllEmpCatForSelection(string JobEvaluationMasterId)
        {
            try
            {
                return Json(JEM.LoadAllEmpCatForSelection(JobEvaluationMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveEmpCatTab(string JobEvaluationMasterId, List<Dictionary<string, object>> EmpCatTabData)
        {
            try
            {
                JEM.SaveEmpCatTab(JobEvaluationMasterId, EmpCatTabData);
                return Json(new { Error = false, CData = EmpCatTabData, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult LoadAllSelectedEmpCatTab(string JobEvaluationMasterId)
        {
            try
            {
                return Json(JEM.LoadAllSelectedEmpCatTab(JobEvaluationMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DelECat(string Id)
        {
            try
            {
                JEM.DelECat(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }


    }

}