using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmpActiveInActiveNewController : BaseController
    {
        // GET: Employees/SkillMatrix
        #region Constructor

        private readonly ISkillMatrixService _skillMatrixService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeActiveInActiveService _empService;


        public EmpActiveInActiveNewController(IEmployeeActiveInActiveService empService, ISkillMatrixService skillMatrixService
            , ISqlRepository sqlRepository)
        {
            _skillMatrixService = skillMatrixService;
            _sqlRepository = sqlRepository;
            _empService = empService;
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetSkillMaster()
        {
            var res = _skillMatrixService.GetSkillMaster();

            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost,Authorize]
        public JsonResult GetSkillMasterDetails(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillMatrixService.GetSkillMasterDetail(queryString, queryStringProcess, queryStringSkill, queryStringOperationCode, queryStringGrouping, queryStringMachineCategory, queryStringMachineSubCategoryCode, queryStringCaption, queryStringOperationCategoryId, queryStringOnRoll, queryStringTotalPresent, queryStringOnRollShort, queryStringOnRollExcess, queryStringPresentShort, queryStringPresentExcess), JsonRequestBehavior.AllowGet);
            //return null;
        }
        [HttpPost,Authorize]
        public JsonResult GetGraphDetails(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillMatrixService.GetGraphDetails(queryString, queryStringProcess, queryStringSkill, queryStringOperationCode, queryStringGrouping, queryStringMachineCategory, queryStringMachineSubCategoryCode, queryStringCaption, queryStringOperationCategoryId, queryStringOnRoll, queryStringTotalPresent, queryStringOnRollShort, queryStringOnRollExcess, queryStringPresentShort, queryStringPresentExcess), JsonRequestBehavior.AllowGet);
        }
        [HttpPost,Authorize]
        public JsonResult GetGraphDetails1()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillMatrixService.GetGraphDetails1(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcess()
        {
            return Json(new SelectList(_skillMatrixService.GetProcess(), "drpValue", "drpText"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEntity()
        {
            return Json(new SelectList(_skillMatrixService.GetEntity(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }





        //Cs Controller for Active InActive
        [Authorize, HttpGet]
        public JsonResult GetListForInActive()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

             JsonResult json = Json(_empService.GetListForInActive(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public JsonResult GetListForActive()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            JsonResult json = Json(_empService.GetListForActive(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

            //return Json(_empService.GetListForActive(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult InActiveToActive(string SystemId,string reason)
        {
            ActiveInActiveEmpNewProcessService rep = new ActiveInActiveEmpNewProcessService();
            rep.InActiveToActiveNewAttdnProcess(SystemId, reason);
            return Json(new { Message = "Employee Active" + AplosMessage.Success });
        }


      
    }


}