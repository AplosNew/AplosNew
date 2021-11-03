using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmpActiveInActiveController : BaseController
    {
        // GET: Employees/SkillMatrix
        #region Constructor

        private readonly ISkillMatrixService _skillMatrixService;
        //private readonly IPurchaseOrderDetailService _inventoryDetailService;
        //private readonly IInventoryMaterialService _inventoryMaterialService;
        //private readonly IInventoryServiceService _inventoryService;
        // private readonly IInventoryReceiveReportService _inventoryReportService;
        //  private readonly DBService _dbService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeActiveInActiveService _empService;


        public EmpActiveInActiveController(IEmployeeActiveInActiveService empService, ISkillMatrixService skillMatrixService
            , ISqlRepository sqlRepository)
        {
            _skillMatrixService = skillMatrixService;
            _sqlRepository = sqlRepository;
            _empService = empService;
        }

        #endregion Constructor
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetSkillMaster()
        {
            var res = _skillMatrixService.GetSkillMaster();

            //var x= Json(_skillMatrixService.GetSkillMaster(), JsonRequestBehavior.AllowGet);
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //Session["res"] = res.Select(p => new { p.sk, p.Title });
            //foreach (var item in res)
            //{
            //    item.
            //}
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        //[Authorize, HttpGet]
        //[System.Web.Http.FromBody]string queryString, [System.Web.Http.FromBody]string queryStringCaption, [System.Web.Http.FromBody]string queryStringProcess, [System.Web.Http.FromBody]string queryStringSkill, [System.Web.Http.FromBody]string queryStringGrouping, [System.Web.Http.FromBody]string queryStringMachineCategory, [System.Web.Http.FromBody]string queryStringMachineSubCategoryCode, [System.Web.Http.FromBody]string queryStringOnRoll, [System.Web.Http.FromBody]string queryStringTotalPresent, [System.Web.Http.FromBody]string queryStringOnRollShort, [System.Web.Http.FromBody]string queryStringOnRollExcess, [System.Web.Http.FromBody]string queryStringPresentShort, [System.Web.Http.FromBody]string queryStringPresentExcess
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

            //return Json(_empService.GetListForInActive(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);


            JsonResult json = Json(_empService.GetListForInActive(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet]
        public JsonResult GetListForActive()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            JsonResult json = Json(_empService.GetListForActive(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

            //return Json(_empService.GetListForActive(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult InActiveToActive(string SystemId,string reason)
        {

            _empService.InActiveToActive(SystemId, reason);
            return Json(new { Message = "Employee Active" + AplosMessage.Success });
        }
        //[HttpPost]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        //public JsonResult ActiveToInActive(string SystemId)
        //{

        //    _empService.ActiveToInActive(SystemId);
        //    return Json(new { Message = "Employee InActive" + AplosMessage.Success });
        //}

    }


}