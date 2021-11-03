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

namespace Aplos.Areas.IE.Controllers
{
    public class SkillGroupingController : Controller
    {
        #region Constructor



        private readonly IOperationMasterService _operationMasterService;
        private readonly IMachineMasterUIService _machineMasterUIService;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;

        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;
        private readonly IOperationPositionMPBudgetService _OperationPositionMPBudgetService;

        private readonly ISkillGroupingService _SkillGroupingService;



        public SkillGroupingController(
            IOperationMasterService operationMasterService
            ,IMachineMasterUIService machineMasterUIService
            , IOperationTimeCaptureMasterService operationTimeCaptureService
            , IOperationTimeCaptureDetailService operationtimecapturedetailservice
            , IOperationService operationService
            , IOperationVariationService operationStepService
            , IOperationPositionMPBudgetService OperationPositionMPBudgetService

            ,ISkillGroupingService skillGroupingService
            

            )
        {
            _machineMasterUIService = machineMasterUIService;
            _operationStepService = operationStepService;
            _operationtimecapturedetailservice = operationtimecapturedetailservice;
            _operationService = operationService;
            _ioperationtimecaptureservice = operationTimeCaptureService;
            _operationMasterService = operationMasterService;
            _OperationPositionMPBudgetService = OperationPositionMPBudgetService;
            _SkillGroupingService = skillGroupingService; 
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
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_SkillGroupingService.GetCbo(), JsonRequestBehavior.AllowGet);
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


        #region Grid data for Skill grouping
        [Authorize, HttpGet]
        public JsonResult GetSkillgrouping()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_SkillGroupingService.GetSkillgrouping(), JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region GetData by Skill Grouping Id
        [Authorize, HttpGet]
        public JsonResult GetDataBySkillGroupingId(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_SkillGroupingService.GetDataBySkillGroupingId(id), JsonRequestBehavior.AllowGet);
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
            return Json(_SkillGroupingService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        
        //public JsonResult GetAutoSequenceForManPower()
        //{
        //    return Json(_OperationPositionMPBudgetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}
        private string GetPK()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return _SkillGroupingService.GetAutoNumber(nameof(SkillGrouping), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        [HttpPost]
        public JsonResult Create(SkillGrouping model)
        {
            try
            {
                _SkillGroupingService.Check(model);
               // _machineMasterUIService.Check(model);
            }
            catch (CustomException)
            {
                throw;
            }  
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.Id = "SG" + GetPK();
            model.CompanyGroupID = identity.CompanyGroupId;
            //if(model.Type=="ACTIVITY")
            //{
            //    model.MachineMasterId = null;
            //    model.Skillid = model.Skillid;
            //}
            //else if (model.Type == "OPERATION")
            //{
            //    model.MachineMasterId = model.MachineMasterId;
            //    model.Skillid = null;
            //}
            if (model.Active)
            {
                model.Active = true;
            }
            else if (!model.Active)
            {
                model.Active = false;
            }

            _SkillGroupingService.Insert(model);
            return Json(new { SkillGrouping = model,model.Id, Sequence = _SkillGroupingService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SkillGrouping model)
        {
            try
            {
                _SkillGroupingService.Check(model);
            }
            catch (CustomException)
            {
                throw;
            }
            if (model.Active)
            {
                model.Active = true;
            }
            else if (!model.Active)
            {
                model.Active = false;
            }
            _SkillGroupingService.Update(model);
            return Json(new { Sequence = _SkillGroupingService.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        [HttpPost]

     
        public ActionResult Delete(string id)
        {
            _SkillGroupingService.Delete(id);
            return Json(new { Sequence = _SkillGroupingService.GetAutoSequence(), Message = AplosMessage.Deleted });
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
            model.Id = "OPP" + GetPK();
            model.CompanyGroupID = identity.CompanyGroupId;            
           if (model.Active)
            {
                model.Active = true;
            }
            else if (!model.Active)
            {
                model.Active = false;
            }
            else if (model.PositionId==null)
            {
                throw new CustomException("Please select position");
            }
            
            else if (model.PositionId == null)
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
            _OperationPositionMPBudgetService.Insert(model);
            return Json(new { MachineMasterUI = model, Sequence = _machineMasterUIService.GetAutoSequence(), Message = AplosMessage.Insert });
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
           if (model.Active)
            {
                model.Active = true;
            }
            else if (!model.Active)
            {
                model.Active = false;
            }
            else if (model.PositionId == null)
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
            _OperationPositionMPBudgetService.Update(model);
            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        
        [HttpPost]
        public ActionResult DeleteManpower(string id)
        {
            _OperationPositionMPBudgetService.Delete(id);
            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

    }

}