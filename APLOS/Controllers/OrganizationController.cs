#region Using

using Library.Core;
using Library.Data.Sql;
using Library.Service.Modules;
using Library.Service.Organizations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

#endregion Using

namespace Aplos.Controllers
{
    public class OrganizationController : ApiController
    {
        #region Constructor

        private readonly IModuleAppService _moduleAppService;
        private readonly IDesignationService _designationService;
        private readonly IEntityService _entityService;
        private readonly IStructureRelationshipService _structureRelationshipService;
        private readonly IManpowerBudgetJobDescriptionService _manpowerBudgetJobDescriptionService;
        private readonly IManpowerBudgetService _manpowerBudgetService;
        private readonly IPlantService _plantService;
        private readonly ISqlRepository _sqlRepository;

        public OrganizationController(
            IModuleAppService moduleAppService
            , IEntityService entityService
            , IManpowerBudgetJobDescriptionService manpowerBudgetJobDescriptionService
            , IStructureRelationshipService structureRelationshipService
            , IManpowerBudgetService manpowerBudgetService
            , IPlantService plantService
            , IDesignationService designationService
            , ISqlRepository sqlRepository)
        {
            _moduleAppService = moduleAppService;
            _designationService = designationService;
            _entityService = entityService;
            _manpowerBudgetService = manpowerBudgetService;
            _structureRelationshipService = structureRelationshipService;
            _manpowerBudgetJobDescriptionService = manpowerBudgetJobDescriptionService;
            _plantService = plantService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IHttpActionResult GetEntityList(string companyId)
        {
            try
            {
                var result = _entityService.GetEntityList(companyId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [Obsolete]
        public IHttpActionResult GetEntityRelationship(string companyId)
        {
            try
            {
                var result = _structureRelationshipService.GetEntityRelationship(companyId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetManpowerBudgetRelationship(string companyGroupId, string companyId)
        {
            try
            {
                var result = _structureRelationshipService.GetEntityAndPositionRelationship(companyGroupId, companyId);
                result.Add(new Dictionary<string, object>
                {
                    { "value", "[Id]" },
                    { "name", "Id" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[Code]" },
                    { "name", "Code" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[EntityId]" },
                    { "name", "EntityId" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[Entity]" },
                    { "name", "Entity" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[CompanyId]" },
                    { "name", "CompanyId" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[BudgetedMale]" },
                    { "name", "BudgetedMale" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[BudgetedFemale]" },
                    { "name", "BudgetedFemale" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[BudgetedTotal]" },
                    { "name", "BudgetedTotal" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[OnRuleMale]" },
                    { "name", "OnRuleMale" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[OnRuleFemale]" },
                    { "name", "OnRuleFemale" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[OnRuleTotal]" },
                    { "name", "OnRuleTotal" }
                });

                result.Add(new Dictionary<string, object>
                {
                    { "value", "[EmploymentType]" },
                    { "name", "EmploymentType" }
                });
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetManpowerBudgetDetailById(string id)
        {
            try
            {
                var result = _manpowerBudgetService.GetManpowerBudgetById(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetManpowerBudgetList(string plantId)
        {
            try
            {
                var result = _manpowerBudgetService.GetManpowerBudgetList(plantId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetManpowerBudgetListByPlanning(string planningId)
        {
            try
            {
                var result = _manpowerBudgetService.GetManpowerBudgetListByPlanning(planningId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetManpowerBudgetList(string search, string searchBy, string plantId)
        {
            try
            {
                var result = _manpowerBudgetService.GetManpowerBudgetList(new GridParameter { search = search, searchBy = searchBy }, plantId);
                return Json(result.Rows);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetJDByManpowerBudgetId(string id)
        {
            try
            {
                var result = _manpowerBudgetJobDescriptionService.GetJDByManpowerBudgetId(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetDesignationList(string id)
        {
            try
            {
                return Json(_designationService.GetDesignationList(id));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetAppServiceUrl(string id)
        {
            try
            {
                return Json(_moduleAppService.GetAppServiceUrl(id));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }
        public IHttpActionResult GetAppServiceUrlByCode(string Code)
        {
            try
            {
                SqlRepository _sqlRepository = new SqlRepository();
                DataTable dt = _sqlRepository.GetDataTable("SELECT ServiceUrl FROM [MMS].[ModuleApp] WHERE Code='"+ Code + @"'");
                string ServiceUrl = "";
                if (dt.Rows.Count > 0)
                    ServiceUrl = dt.Rows[0]["ServiceUrl"].ToString();

                return Json(ServiceUrl);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetDesignationUpperList(string id)
        {
            try
            {
                return Json(_designationService.GetDesignationUpperList(id));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        public IHttpActionResult GetPlantList(string companyId)
        {
            try
            {
              

                
                return Json(_plantService.GetCboByCompany(companyId));
                //return Json(_sqlRepository.GetDataTable(@"select UserName AS PlantName  ,Id AS PlantId from Org.Plant AND Active=1 AND Archive=0"));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }
    }
}