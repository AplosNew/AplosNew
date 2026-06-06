using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Addresses;
using Library.Model.Organizations;
using Library.Service.Organizations;
using Library.Service.Setups;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class EntityController : BaseController
    {
        private readonly IEntityService _entityService;
        private readonly IEntityAllowanceService _entityAllowanceService;
        private readonly IOrganizationReportService _organizationReportService;
        private readonly ISqlRepository _sqlRepository;
        public EntityController(
              IEntityService entityService
            , IEntityAllowanceService entityAllowanceService
            , IOrganizationReportService organizationReportService
            , ISqlRepository R
            )
        {
            _organizationReportService = organizationReportService;
            _entityService = entityService;
            _entityAllowanceService = entityAllowanceService;
            _sqlRepository = R;
        }

        [HttpGet, Authorize]
        public JsonResult UseChecking(string id)
        {
            return Json(_entityService.UseChecking(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public FileResult Report(string companyId)
        {
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = companyId + ".xlsx";
            return File(_entityService.Report(companyId).ToArray(), contentType, fileName);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult EntityRelationship()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_entityService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompany(string companyGroupId, string companyId)
        {
            return Json(_entityService.GetCbo(companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByPlant(string companyGroupId, string companyId, string plantId)
        {
            //if (string.IsNullOrEmpty(plantId))
            //{
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            //}
            //if (string.IsNullOrEmpty(companyGroupId))
            //{
            companyGroupId = identity.CompanyGroupId;
            //}
            //if (string.IsNullOrEmpty(companyId))
            //{
            companyId = identity.CompanyId;
            //}
            return Json(_entityService.GetCbo(companyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboByPlantAdmin(string companyGroupId, string companyId, string plantId)
        {
            return Json(GetCboDataByPlantId(companyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetCboDataByPlantId(string companyGroupId, string companyId, string plantId)
        {
            if (string.IsNullOrEmpty(plantId) || plantId == "undefined")
            {
                return _sqlRepository.GetDataCollection(@"SELECT UserName AS [Text],Id AS Value FROM ORG.Entity  AS L 
                WHERE CompanyGroupId = '" + companyGroupId + "' AND CompanyId = '" + companyId + "'");
            }
            else
            {
                return _sqlRepository.GetDataCollection(@"SELECT UserName AS [Text],Id AS Value FROM ORG.Entity  AS L 
                WHERE CompanyGroupId = '" + companyGroupId + "' AND CompanyId = '" + companyId + "' AND PlantId = '" + plantId + @"' ");
            }

        }

        [HttpGet, Authorize]
        public JsonResult GetEntityCboByPlant(string companyGroupId, string companyId, string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            if (string.IsNullOrEmpty(companyGroupId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupId = identity.CompanyGroupId;
            }
            if (string.IsNullOrEmpty(companyId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyId = identity.CompanyId;
            }
            return Json(_sqlRepository.GetDataCollection(@"select Id Value,UserName Text from ORG.Entity Where PlantId='" + plantId + "'"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityByUser()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
            {

                return Json(_entityService.GetEntityCbobyUser(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(_entityService.GetCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);

            }
        }
        [HttpGet, Authorize]
        public JsonResult GetEntityByGeneralUser()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!identity.IsSysAdmin)
            {
                return Json(_entityService.GetEntityByGeneralUser(identity.UserId), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(_entityService.GetCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEntityDivisionList(string companyGroupId, string companyId, string plantId)
        {
            return Json(_entityService.GetCboEntityDivisionList(companyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEntitySubDivisionList(string companyGroupId, string companyId, string plantId, string divisionId)
        {
            return Json(_entityService.GetCboEntitySubDivisionList(companyGroupId, companyId, plantId, divisionId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEntityUnitList(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId)
        {
            return Json(_entityService.GetCboEntityUnitList(companyGroupId, companyId, plantId, divisionId, subDivisionId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboWithEmployee(string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_entityService.GetCboWithEmployee(companyId, companyGroupId, identity.EmployeeId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboInterEntity(string companyGroupId, string companyId, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_entityService.GetCboInterEntity(companyId, companyGroupId, Id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExceptionCboByCompany(string companyId)
        {
            return Json(new SelectList(_entityService.GetExceptionCboByCompany(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string companyId)
        {
            return Json(_entityService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEntityList(GridParameter parameters, string companyId)
        {
            return Json(_entityService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult QueryEntityByBuyer(GridParameter parameters, string companyId, string buyerMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
            {
                companyId = identity.CompanyId;
            }
            return Json(_entityService.QueryEntityByBuyer(parameters, companyId, buyerMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult WithinCompany(GridParameter parameters, string companyId, string entityId)
        {
            return Json(_entityService.WithinCompany(parameters, companyId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult WithinGroup(GridParameter parameters, string companyGroupId, string companyId, string entityId)
        {
            return Json(_entityService.WithinGroup(parameters, companyGroupId, companyId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(Entity entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _entityService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        public JsonResult Delete(string id)
        {
            _entityService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(Entity entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _entityService.Insert(entity);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult Get(string id)
        {
            return Json(_entityService.GetData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetById(string companyId, string id)
        {
            return Json(_entityService.Get(companyId, id), JsonRequestBehavior.AllowGet);
        }
        #region Allowance

        [HttpGet, Authorize]
        public ActionResult Allowance()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetAllowanceList(GridParameter parameters)
        {
            return Json(_entityAllowanceService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEffectiveDateList(GridParameter parameters, string entityId, string designationId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_entityAllowanceService.GetEffectiveDateList(parameters, identity.CompanyGroupId, entityId, designationId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAllowanceCbo()
        {
            return Json(_entityAllowanceService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEntityAllowance()
        {
            return Json(_entityAllowanceService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEntityAllowanceById(string id)
        {
            return Json(_entityAllowanceService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateAllowance(EntityAllowance entityAllowance)
        {
            _entityAllowanceService.Insert(entityAllowance);
            return Json(new { EntityAllowance = entityAllowance, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditAllowance(EntityAllowance entityAllowance)
        {
            _entityAllowanceService.Update(entityAllowance);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult DeleteAllowance(string id)
        {
            _entityAllowanceService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Allowance

        [HttpGet, Authorize]
        public ActionResult EntityReport(string companyId)
        {
            var fileName = "Entity Report.xlsx";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _organizationReportService.GetEntity(identity.CompanyGroupId, companyId);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
    }
}