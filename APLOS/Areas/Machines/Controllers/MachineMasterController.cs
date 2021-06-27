//using Library.Model.IE;
//using Aplos.Properties;
//using Library.Data;
//using Library.Service.IEnumerable;
//using Library.Service.Machines;
//using Library.Core;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Script.Serialization;
//using Library.Service.Helpers;
//using System.Threading;
//using Library.Crosscutting.Security;
//using Library.Service.IE;
//using Library.Model.Inventory;
//using Library.Service.Systems;
//using Library.Service.Enums;
//using Library.Model.Machines;
//using Aplos.Controllers;

//namespace Aplos.Areas.IE.Controllers
//{
//    public class MachineMasterController : BaseController
//    {
//        #region Constructor



//        private readonly IOperationMasterService _operationMasterService;
//        private readonly IMachineMasterService _machineMasterService;
//        private readonly IOperationService _operationService;
//        private readonly IOperationVariationService _operationStepService;

//        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
//        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;
//        private readonly IOperationPositionMPBudgetService _OperationPositionMPBudgetService;



//        public MachineMasterController(
//            IOperationMasterService operationMasterService
//            , IMachineMasterService machineMasterService
//            , IOperationTimeCaptureMasterService operationTimeCaptureService
//            , IOperationTimeCaptureDetailService operationtimecapturedetailservice
//            , IOperationService operationService
//            , IOperationVariationService operationStepService
//            , IOperationPositionMPBudgetService OperationPositionMPBudgetService

//            )
//        {
//            _operationStepService = operationStepService;
//            _operationtimecapturedetailservice = operationtimecapturedetailservice;
//            _operationService = operationService;
//            _ioperationtimecaptureservice = operationTimeCaptureService;
//            _operationMasterService = operationMasterService;
//            _machineMasterService = machineMasterService;
//            _OperationPositionMPBudgetService = OperationPositionMPBudgetService;
//        }

//        #endregion Constructor

//        #region -- Pages

//        [Authorize, HttpGet]
//        public ActionResult Aplos()
//        {
//            return View();
//        }

//        #endregion -- Pages

//        #region -- Operations for OperationMaster

//        [Authorize, HttpGet]
//        public JsonResult GetCboOperationActivity()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCboCompanyGroup(), JsonRequestBehavior.AllowGet);
//        }
//        [Authorize, HttpGet]
//        public JsonResult GetCboOperationType()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCboOperationType(), JsonRequestBehavior.AllowGet);
//        }


//        public JsonResult GetCboOperationCategory()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCboOperationCategory(), JsonRequestBehavior.AllowGet);
//        }

//        public JsonResult GetCboSkill()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCboSkill(), JsonRequestBehavior.AllowGet);
//        }


//        public JsonResult GetCboMachineMaster()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCboMachineMaster(), JsonRequestBehavior.AllowGet);
//        }



//        public JsonResult GetCboSkillGrouping()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCboSkillGrouping(), JsonRequestBehavior.AllowGet);
//        }

//        public JsonResult GetCbolegalDesignation()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCbolegalDesignation(), JsonRequestBehavior.AllowGet);
//        }
//        public JsonResult GetCboProcess()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetCboProcess(), JsonRequestBehavior.AllowGet);
//        }




//        #endregion


//        #region Grid data for Operation Master UI
//        [Authorize, HttpGet]
//        public JsonResult GetOperationMaster()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_machineMasterService.GetOperationMaster(), JsonRequestBehavior.AllowGet);
//        }
//        [Authorize, HttpGet]
//        public JsonResult GetOperationPositionMPBudget()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_OperationPositionMPBudgetService.GetOperationPositionMPBudgetService(), JsonRequestBehavior.AllowGet);
//        }

//        #endregion


//        #region GetData by Operation Master Id
//        //[Authorize, HttpGet]
//        public JsonResult GetDataByMasterOrderId(string id)
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_operationMasterService.GetDataByMasterOrderId(id), JsonRequestBehavior.AllowGet);
//        }
//        [Authorize, HttpGet]
//        public JsonResult GetDataByMasterOrderIdMP(string id) 
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return Json(_OperationPositionMPBudgetService.GetDataByMasterOrderIdMP(id), JsonRequestBehavior.AllowGet);
//        }

//        #endregion

//        [HttpGet, Authorize]
//        public JsonResult GetAutoSequence()
//        {
//            return Json(_machineMasterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
//        }
//        [HttpGet, Authorize]
//        public JsonResult GetAutoSequenceForManPower()
//        {
//            return Json(_OperationPositionMPBudgetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
//        }
//        private string GetPK()
//        {
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            return _machineMasterService.GetAutoNumber(nameof(OperationMaster), PKGeneratorEnum.Yearly, identity.CompanyGroupId, DateTime.Now);
//        }
//        [HttpPost]
//        public JsonResult Create(MachineMaster model)
//        {
//            try
//            {
//                _machineMasterService.Check(model);
//            }
//            catch(CustomException)
//            {
//                throw;
//            }
          
//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            model.Id = "OP" + GetPK();
//            model.CompanyGroupID = identity.CompanyGroupId;
//            ////if(model.Type=="ACTIVITY")
//            //{
//            //    model.MachineMasterId = null;
//            //    model.Skillid = model.Skillid;
//            //}
//            //else if (model.Type == "OPERATION")
//            //{
//            //    model.MachineMasterId = model.MachineMasterId;
//            //    model.Skillid = null;
//            //}
//            //else if (model.Active)
//            //{
//            //    model.Active=true;
//            //}
//            //else if (!model.Active)
//            //{
//            //    model.Active = false;
//            //}

//            _machineMasterService.Insert(model);
//            return Json(new { OperationMaster = model, Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
//        }
//        [HttpPost]
//        public JsonResult CreateManpower(OperationPositionMPBudget model)
//        {
//            try
//            {
//                _OperationPositionMPBudgetService.Check(model);
//            }
//            catch (CustomException)
//            {
//                throw;
//            }

//            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//            model.Id = "OPP" + GetPK();
//            model.CompanyGroupID = identity.CompanyGroupId;            
//           if (model.Active)
//            {
//                model.Active = true;
//            }
//            else if (!model.Active)
//            {
//                model.Active = false;
//            }

//            _OperationPositionMPBudgetService.Insert(model);
//            return Json(new { OperationMaster = model, Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
//        }
//        [HttpPost]
//        //public JsonResult Edit(MachineMaster model)
//        //{
//        //    try
//        //    {
//        //        _machineMasterService.Check(model);
//        //    }
//        //    catch (CustomException)
//        //    {
//        //        throw;
//        //    }
//        //    if (model.Type == "ACTIVITY")
//        //    {
//        //        model.MachineMasterId = null;
//        //    }
//        //    else if (model.Type == "OPERATION")
//        //    {
//        //        model.MachineMasterId = model.MachineMasterId;
//        //    }
//        //    else if (model.Active)
//        //    {
//        //        model.Active = true;
//        //    }
//        //    else if (!model.Active)
//        //    {
//        //        model.Active = false;
//        //    }
//        //    _machineMasterService.Update(model);
//        //    return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
//        //}
//        //[HttpPost]
//        public JsonResult EditManpower(OperationPositionMPBudget model)
//        {
//            try
//            {
//                _OperationPositionMPBudgetService.Check(model);
//            }
//            catch (CustomException)
//            {
//                throw;
//            }            
//           if (model.Active)
//            {
//                model.Active = true;
//            }
//            else if (!model.Active)
//            {
//                model.Active = false;
//            }
//            _OperationPositionMPBudgetService.Update(model);
//            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
//        }
//        [HttpPost]
//        public ActionResult Delete(string id)
//        {
//            _operationMasterService.Delete(id);
//            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
//        }
//        [HttpPost]
//        public ActionResult DeleteManpower(string id)
//        {
//            _OperationPositionMPBudgetService.Delete(id);
//            return Json(new { Sequence = _operationMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
//        }

//    }

//}