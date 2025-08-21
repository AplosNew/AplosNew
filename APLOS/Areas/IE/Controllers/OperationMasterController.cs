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
using Library.Data.Sql;
using Library.Data.Repositories;
using System.Linq;
using Library.Model.Enums;
using Aplos.Controllers;
using System.Data;

namespace Aplos.Areas.IE.Controllers
{
    public class OperationMasterController : BaseController
    {
        #region Constructor



        private readonly IOperationMasterService _operationMasterService;

        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;

        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;
        private readonly IOperationPositionMPBudgetService _OperationPositionMPBudgetService;
        private readonly IRepositoryAsync<OperationPositionMPBudget> _OperationPositionMPBudgetRepository;
        private readonly ISqlRepository _sqlRepository;


        public OperationMasterController(
            IOperationMasterService operationMasterService
            , IOperationTimeCaptureMasterService operationTimeCaptureService
            , IOperationTimeCaptureDetailService operationtimecapturedetailservice
            , IOperationService operationService
            , IOperationVariationService operationStepService
            , IOperationPositionMPBudgetService OperationPositionMPBudgetService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<OperationPositionMPBudget> OperationPositionMPBudgetRepository
            )
        {
            _operationStepService = operationStepService;
            _operationtimecapturedetailservice = operationtimecapturedetailservice;
            _operationService = operationService;
            _ioperationtimecaptureservice = operationTimeCaptureService;
            _operationMasterService = operationMasterService;
            _OperationPositionMPBudgetService = OperationPositionMPBudgetService;
            _sqlRepository = sqlRepository;
            _OperationPositionMPBudgetRepository = OperationPositionMPBudgetRepository;
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
            return Json(_operationMasterService.GetCboCompanyGroup(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboOperationType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboOperationType(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboOperationCategory()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboOperationCategory(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboSkill()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboSkill(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboSkillCboByMachine(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboSkillCboByMachine(Id), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetCbolegalDesignation(string designationGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_operationMasterService.GetCbolegalDesignation(), JsonRequestBehavior.AllowGet);
            string sql = @"Select LD.Id,LD.UserName  FROM MST.DesignationMaster DM
LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId AND DC.PlantId = '" + identity.PlantId + @"'
LEFT JOIN [MST].[DesignationMasterLegalDesignation] DMLD ON DMLD.DesignationMasterId=DM.Id
LEFT JOIN HKP.LegalDesignation LD ON DMLD.LegalDesignationId=LD.Id
LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
WHERE DM.DesignationGroupId='" + designationGroupId + "' AND LD.Id<>''";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

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
        public JsonResult GetCboShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetCboShift(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]

        public JsonResult GetCboPosition()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetCboPosition(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetDesignationGroupCbo()
        {
            try
            {
                string sql = @"Select Id,UserName from HKP.DesignationGroup Where Active=1";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region Grid data for Operation Master UI
        [Authorize, HttpGet]
        public JsonResult GetOperationMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetOperationMaster(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetOperationPositionMPBudget(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetOperationPositionMPBudgetService(id), JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region GetData by Operation Master Id
        [Authorize, HttpGet]
        public JsonResult GetDataByMasterOrderId(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetDataByMasterOrderId(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetDataByMasterOrderIdMP(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetDataByMasterOrderIdMP(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetDataByMasterOrderIdMP1(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetDataByMasterOrderIdMP1(id), JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_operationMasterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequenceForManPower(string OMId)
        {
            return Json(_OperationPositionMPBudgetService.GetAutoSequence(OMId), JsonRequestBehavior.AllowGet);
        }
        private string GetPK()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return _operationMasterService.GetAutoNumber(nameof(OperationMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetPK1()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return _operationMasterService.GetAutoNumber(nameof(OperationPositionMPBudget), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        [HttpPost]
        public JsonResult Create(OperationMaster model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupID = identity.CompanyGroupId;
            try
            {
                _operationMasterService.Check(model);
            }
            catch (CustomException)
            {
                throw;
            }

            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.Id = "OP" + GetPK();
            model.CompanyGroupID = identity.CompanyGroupId;
            if (model.Type == "ACTIVITY")
            {
                model.MachineMasterId = null;
                model.Skillid = model.Skillid;
            }
            else if (model.Type == "OPERATION")
            {
                model.MachineMasterId = model.MachineMasterId;
                model.Skillid = null;
            }
            else if (model.Active)
            {
                model.Active = true;
            }
            else if (!model.Active)
            {
                model.Active = false;
            }

            _operationMasterService.Insert(model);
            return Json(new { OperationMaster = model, model.Id, Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(OperationMaster model)
        {
            //try
            //{
            //    _operationMasterService.Check(model);
            //}
            //catch (CustomException)
            //{
            //    throw;
            //}
            if (model.Type == "ACTIVITY")
            {
                model.MachineMasterId = null;
            }
            else if (model.Type == "OPERATION")
            {
                model.MachineMasterId = model.MachineMasterId;
            }
            else if (model.Active)
            {
                model.Active = true;
            }
            else if (!model.Active)
            {
                model.Active = false;
            }
            _operationMasterService.Update(model);
            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
        }


        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                // var res = _OperationPositionMPBudgetRepository.SqlQuery<int>($"select id=case when Id is not null then 1 else 0 end from Mst.OperationPositionMPBudget where OperationMasterId='{model.OperationMasterId}' AND EntityId= '{model.EntityId}' and PositionId='{model.PositionId}' and Caption='{model.Caption}'").FirstOrDefault();
                var res = _OperationPositionMPBudgetRepository.SqlQuery<int>($"select distinct id=case when Id is not null then 1 else 0 end 	from Mst.OperationPositionMPBudget where OperationMasterId='{id}'").FirstOrDefault();
                if (res == 1)
                {
                    throw new CustomException("Operation Master has got Manpower Budget!");
                }
                else
                {

                    _operationMasterService.Delete(id);
                }

            }
            catch (CustomException)
            {
                throw new CustomException("Operation Master has got Manpower Budget!");
            }
            // _operationMasterService.Delete(id);
            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        public bool CheckUsing(object id)
        {
            try
            {
                var sql = "IF EXISTS(SELECT 1 FROM( " +
                $"SELECT COALevel1Id AS CheckingColumn FROM [HKP].[GLGeneralInfo] " +
                $") A WHERE CheckingColumn = '{id}') SELECT 1 ELSE SELECT 0 RETURN ";
                return Convert.ToBoolean(_OperationPositionMPBudgetRepository.SqlQuery<int>(sql).Single());
            }
            catch (Exception)
            {
                return false;
            }
        }
        [HttpPost]
        public JsonResult CreateManpower(OperationPositionMPBudget model)
        {
            try
            {
                _OperationPositionMPBudgetService.Check(model);
            }
            catch (CustomException)
            {
                throw;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.Id = "OPP" + GetPK1();
            model.CompanyGroupID = identity.CompanyGroupId;
            if (model.PositionId == null)
            {
                throw new CustomException("Please select position");
            }
            else if (model.Caption == null)
            {
                throw new CustomException("Please input Caption");
            }
            else if (model.ManpowerBudget == '0')
            {
                throw new CustomException("Please input Manpower Budget");
            }
            else if (model.Active)
            {
                model.Active = true;
            }
            else if (!model.Active)
            {
                model.Active = false;
            }
            var res = _OperationPositionMPBudgetRepository.SqlQuery<int>($"select id=case when Id is not null then 1 else 0 end from Mst.OperationPositionMPBudget where OperationMasterId='{model.OperationMasterId}' AND EntityId= '{model.EntityId}' and PositionId='{model.PositionId}' and Caption='{model.Caption}' and ShiftId='{model.ShiftId}'").FirstOrDefault();

            if (res == 1)
            {
                throw new CustomException("Combination already exist!");
            }
            else
            {

                _OperationPositionMPBudgetService.Insert(model);
            }

            //_OperationPositionMPBudgetService.Insert(model);
            return Json(new { OperationMaster = model, Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditManpower(OperationPositionMPBudget model)
        {
            try
            {
                _OperationPositionMPBudgetService.Check(model);
            }
            catch (CustomException)
            {
                throw;
            }
            try
            {


                if (model.PositionId == null)
                {
                    throw new CustomException("Please select position");
                }
                else if (model.Caption == null)
                {
                    throw new CustomException("Please input Caption");
                }
                else if (model.ManpowerBudget == '0')
                {
                    throw new CustomException("Please input Manpower Budget");
                }
                else if (model.Active)
                {
                    model.Active = true;
                }
                else if (!model.Active)
                {
                    model.Active = false;
                }

                _OperationPositionMPBudgetService.Update(model);
                return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult DeleteManpower(string id)
        {
            _OperationPositionMPBudgetService.Delete(id);
            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult SaveSkillMachine(List<Dictionary<string, object>> machineList, string SkillMasterId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string _Id = "";
                string sql = "";
                string EmpSystemId = "";

                sql = "SELECT * FROM [dbo].[SkillMasterMachine] WHERE SkillMasterId='" + SkillMasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                foreach (var item in machineList)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["SkillMasterId"] = item["SkillMasterId"];
                        dr["ArticleId"] = item["ArticleId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetSkillMasterMachineData(string OMId)
        {
            return Json(_operationMasterService.GetSkillMasterMachineData(OMId), JsonRequestBehavior.AllowGet);
        }



        #region Operation Master Report 

        [HttpGet, Authorize]
        public ActionResult OperationMasterReports(ReportFormat reportFormat, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Operation Master Reports  ";
            var workbook = _operationMasterService.CreateOperationMasterReports(identity.CompanyId, plantId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        #endregion Operation Master Report 
    }

}