using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Service.IE;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Service.Enums;
using Library.Service.Securites;
using System.Data;
using Library.Security.Core;
using Library.Data.Sql;

namespace Aplos.Areas.IE.Controllers
{
    public class MachineMasterUIController : Controller
    {
        #region Constructor


        private readonly IUserService _userService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationMasterService _operationMasterService;
        private readonly IMachineMasterUIService _machineMasterUIService;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;

        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;
        private readonly IOperationPositionMPBudgetService _OperationPositionMPBudgetService; 

        

        public MachineMasterUIController(
            IOperationMasterService operationMasterService
            ,IMachineMasterUIService machineMasterUIService
            , IOperationTimeCaptureMasterService operationTimeCaptureService
            , IOperationTimeCaptureDetailService operationtimecapturedetailservice
            , IOperationService operationService
            , IOperationVariationService operationStepService
            , IOperationPositionMPBudgetService OperationPositionMPBudgetService
            , ISqlRepository sqlRepository

            )
        {
            _machineMasterUIService = machineMasterUIService;
            _operationStepService = operationStepService;
            _operationtimecapturedetailservice = operationtimecapturedetailservice;
            _operationService = operationService;
            _ioperationtimecaptureservice = operationTimeCaptureService;
            _operationMasterService = operationMasterService;
            _OperationPositionMPBudgetService = OperationPositionMPBudgetService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

  
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations for OperationMaster

        [Authorize, HttpGet]
        public JsonResult GetCboOperationActivity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboCompanyGroup(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboOperationType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboOperationType(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]

        public JsonResult GetCboMachineCategory()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboMachineCategory(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboMachineSubCategory()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboMachineSubCategory(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboSkill()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboSkill(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboMachineMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboMachineMaster(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]

        public JsonResult GetCboSkillGrouping()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboSkillGrouping(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbolegalDesignation()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCbolegalDesignation(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboProcess()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboProcess(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboEntity() 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetCboEntity(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboPosition()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetCboPosition(), JsonRequestBehavior.AllowGet);
        }




        #endregion


        #region Grid data for Operation Master UI
         [Authorize, HttpGet]
        public JsonResult GetMachineMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetMachineMaster(), JsonRequestBehavior.AllowGet);
        }
        

        #endregion

        [HttpPost]
        public JsonResult Create(MachineMasterUI model)
        {
            try
            {
                _machineMasterUIService.Check(model);
                // _machineMasterUIService.Check(model);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                model.Id = "MM" + GetPK();
                model.CompanyGroupId = identity.CompanyGroupId;
                
                if (model.Active)
                {
                    model.Active = true;
                }
                else if (!model.Active)
                {
                    model.Active = false;
                }

                _machineMasterUIService.Insert(model);
                return Json(new { OperationMaster = model, model.Id, Sequence = _machineMasterUIService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult CreateProcess(List<Dictionary<string, object>> data,string machineMasterId)
        {
            try
            {
                SaveData(data, machineMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        private void SaveData(List<Dictionary<string, object>> data,string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM MachineMasterProcess WHERE MachineMasterId ='"+ machineMasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string MachineMasterProcessId = "";

                
                foreach (var item in data)
                {
                   
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MachineMasterProcess", out MachineMasterProcessId);                      

                        item["Id"] = "M-" + MachineMasterProcessId + "-" + (1);
                        item["MachineMasterId"] = machineMasterId;
                        item["ProcessId"] = item["ProcessId"];
                        
                        AddNewRow(dsMasterOrder.Tables[0], item);
                    }
                    
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [Authorize, HttpPost]
        public ActionResult getProcess(string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MMP.Id,P.Sequence,P.Code,P.ShortName,P.StandardName,P.Id ProcessId,P.UserName Process
			                            from MachineMasterProcess MMP
			                            left join HKP.Process P on P.Id=MMP.ProcessId
										where MMP.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize,HttpPost]
        public ActionResult ProcessDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from MachineMasterProcess where Id ='" + id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        [HttpPost]
        public JsonResult Edit(MachineMasterUI model)
        {
            try
            {
                _machineMasterUIService.Check(model);
                if (model.Active)
                {
                    model.Active = true;
                }
                else if (!model.Active)
                {
                    model.Active = false;
                }
                _machineMasterUIService.Update(model);
                return Json(new { Sequence = _machineMasterUIService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _machineMasterUIService.Delete(id);
            return Json(new { Sequence = _machineMasterUIService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #region GetData by Operation Master Id
        [Authorize, HttpGet]
        public JsonResult GetDataByMasterOrderId(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetDataByMasterOrderId(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetDataByMasterOrderIdMP(string id) 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetDataByMasterOrderIdMP(id), JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_machineMasterUIService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        //public JsonResult GetAutoSequenceForManPower()
        //{
        //    return Json(_OperationPositionMPBudgetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}
        private string GetPK()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return _machineMasterUIService.GetAutoNumber(nameof(MachineMasterUI), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"Select e.Id as EntityId, e.UserName as EntityName , p.UserName as Plant, c.UserName as Company from org.Entity e
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
    }

}