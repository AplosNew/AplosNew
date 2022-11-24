using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.WorkCenters;
using Library.Service.WorkCenters;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class WorkCenterMasterController : BaseController
    {
        #region Constructor

        private readonly IWorkCenterMasterService _workcentermasterservice;

        public WorkCenterMasterController(IWorkCenterMasterService workcentermasterservice)
        {
            _workcentermasterservice = workcentermasterservice;
        }

        #endregion Constructor

        
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet,Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_workcentermasterservice.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListByPlant(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.EmployeeListByPlant(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetWorkCenterWiseShiftList(string workCenterMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(_workcentermasterservice.GetWorkCenterWiseShiftList(identity.CompanyGroupId, identity.PlantId, workCenterMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetWorkCenterMasterSubProcessList(string workCenterMasterId)
        {
            try
            {
                return Json(_workcentermasterservice.GetWorkCenterMasterSubProcessList(workCenterMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetShiftList(GridParameter parameters, string ShiftDefinationIDs)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetShiftList(parameters, identity.CompanyGroupId, identity.PlantId, new JavaScriptSerializer().Deserialize<string[]>(ShiftDefinationIDs)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListForSubProcess(GridParameter parameters, string processId, string WorkCenterMasterId, string subProcessIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetListForSubProcess(parameters, identity.CompanyGroupId, processId, WorkCenterMasterId, new JavaScriptSerializer().Deserialize<string[]>(subProcessIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteShift(string id)
        {
            DeleteShiftData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteShiftData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[WorkCenterWiseShift] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost, Authorize]
        public ActionResult DeleteSP(string id)
        {
            DeleteSPData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSPData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [SCS].[WorkCenterMasterSubProcess] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [Authorize, HttpGet]
        public ActionResult GetMasterList(string masterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetList(masterid, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetEmployeeList(GridParameter parameters, string plantId)
        {
            return Json(_workcentermasterservice.GetEmployeeList(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_workcentermasterservice.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetProductMasterList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboList(string entityId)
        {
            return Json(_workcentermasterservice.GetCboList(entityId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string plantid, string entityid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetListByPlantAndEntity(plantid, entityid, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetDetalsData(string masterId)
        {
            var eDate = _workcentermasterservice.GetEffectiveDateList(masterId);
            var bCode = _workcentermasterservice.GetManpowerBudgetList(masterId);
            var priority = _workcentermasterservice.GetProductPriorityList(masterId);

            return Json(new { eDate, bCode, priority }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMaterialMasterList(GridParameter parameters, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetMaterialMasterList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllWorkCenter(GridParameter parameter)
        {
            return Json(_workcentermasterservice.GetAllWorkCenter(parameter), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getlistbyplant(GridParameter gridparameter, string plantid)
        {
            return Json(_workcentermasterservice.GetListByPlant(gridparameter, plantid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getlistbyplantandprocess(GridParameter gridparameter, string plantid, string processid)
        {
            return Json(_workcentermasterservice.GetListByPlant(gridparameter, plantid, processid), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(WorkCenterMaster master)
        {
            string masterid = string.Empty;
            _workcentermasterservice.InsertORUpdateMaster(master, out masterid);
            return Json(new { id = masterid, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string masterid)
        {
            _workcentermasterservice.DeleteMaster(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DetailSave(string masterId, IEnumerable<WorkCenterMasterEffectiveDate> effectiveDateList, IEnumerable<WorkCenterMasterManpowerBudge> budgetCodeList, IEnumerable<WorkCenterMasterProductPriority> productPriorityList, IEnumerable<WorkCenterWiseShift> shiftList, IEnumerable<WorkCenterMasterSubProcess> subProcessList)
        {
            _workcentermasterservice.InsertUpdateOrDeleteDetails(masterId, effectiveDateList, budgetCodeList, productPriorityList, shiftList, subProcessList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetLineList(GridParameter parameters, string entityId)
        {
            return Json(_workcentermasterservice.GetSearchLine(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}