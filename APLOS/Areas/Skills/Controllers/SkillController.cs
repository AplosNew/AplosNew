#region Using
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Skills;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using System;
using System.Data;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using Library.Data.Sql;
using OTSBD;
using Library.Model.Enums;
#endregion

namespace Aplos.Areas.Skills.Controllers
{
    public class SkillController : Controller
    {
        #region Constructor
        private readonly ISkillService _skillService;
        private readonly ISkillProcessService _skillProcessService;
        private readonly ISqlRepository _sqlRepository;
        public SkillController(
              ISkillService skillService
            , ISkillProcessService skillProcessService,ISqlRepository R)
        {
            _skillService = skillService;
            _skillProcessService = skillProcessService;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
      
        public ActionResult Aplos()
        {
            return View();
        }

       

        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCboWithoutMachineType(string processId)
        {
            return Json(_skillService.GetCboWithoutMachineType(processId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetIsMachineSkillList(GridParameter parameters, string skillProcessIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillService.GetIsMachineSkillList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(skillProcessIds)), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Use in Operation
        /// </summary>
        /// <param name="processIds"></param>
        /// <returns></returns>
        [Authorize]
        public JsonResult GetCommonSkillListByProcess(GridParameter parameters, string processIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillService.GetCommonSkillListByProcess(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(processIds)), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboByProcess( string processIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillService.GetCboByProcess(identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(processIds)), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboByMachineTypeId(string processId, string matchineTypeId)
        {
            return Json(new SelectList(_skillService.GetCboByMachineTypeId(processId, matchineTypeId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_skillService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSkillProcessList(GridParameter parameters, string skillId)
        {
            return Json(_skillProcessService.Query(parameters, skillId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_skillService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Skill entity, IEnumerable<SkillProcess> skillProcess)
        {
            _skillService.InsertGraph(entity, skillProcess);
            return Json(new { Skill = entity, Sequence = _skillService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Skill entity, IEnumerable<SkillProcess> skillProcess)
        {
            _skillService.UpdateGraph(entity, skillProcess);
            return Json(new { Sequence = _skillService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _skillService.DeleteGraph(id);
            return Json(new { Sequence = _skillService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion

    }
}