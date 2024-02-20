using Library.Model.Productions;
using Library.Service.Productions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.Core;
using Library.Model.Productions.ProductionBooking;
using Library.Service.Core;
using Library.OrderManagement.Production;

namespace Aplos.Controllers
{

    public class ProductionSummaryController : ApiController
    {
        #region Constructor
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly IProductionSummaryService _ProductionSummary;

        public ProductionSummaryController(
             IProductionSummaryService ProductionSummary
          )
        {
            _ProductionSummary = ProductionSummary;
        }


        #endregion Constructor

        /// <summary>
        /// monir@s API
        /// </summary>
        ///


        public IHttpActionResult GetProcess(bool IsSysAdmin, string userId, string entityId)
        {
            try
            {
                Library.General.Organization.OrganizationAuthorization orgAuth = new Library.General.Organization.OrganizationAuthorization();
                var result = orgAuth.GetEntityProcessCbo(IsSysAdmin, userId, entityId);

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

        public IHttpActionResult GetEntity(string PlantId, string UserId, bool IsSysAdmin)
        {
            try
            {
                Library.General.Organization.OrganizationAuthorization orgAuth = new Library.General.Organization.OrganizationAuthorization();

                var result = orgAuth.GetEntityByUser(PlantId, UserId, IsSysAdmin);
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

        public IHttpActionResult GetListAPIforProduction(string ProdnDate, string EntityId)
        {
            try
            {
                var result = _ProductionSummary.GetListAPIforProduction(ProdnDate, EntityId);
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
        [HttpPost]
        public string Delete([FromBody] IEnumerable<ProductionSummary> DataToDelete)
        {
            try
            {
                _ProductionSummary.Delete(DataToDelete);
            }
            catch (Exception ex)
            {



                return ex.ToString();



            }
            return "";

        }

        public IHttpActionResult GetLineItemGridSFG(string EntityId, string ProcessId, string ProductionDate, string ProductionShiftId, string WorkCenterMasterId, string ProductionLevel)
        {
            try
            {
                var result = _ProductionSummary.GetLineItemGridSFG(EntityId, ProcessId, ProductionDate, ProductionShiftId, WorkCenterMasterId, ProductionLevel);
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

        public IHttpActionResult GetTotalQty(string salesOrderId, string processId)
        {
            try
            {
                var result = _ProductionSummary.GetTotalQty(salesOrderId, processId);
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

        public IHttpActionResult GetMentorAndRespPersonByWCM(string wcmId)
        {
            try
            {
                var result = _ProductionSummary.GetMentorAndRespPersonByWCM(wcmId);
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

        public IHttpActionResult GetTotalProductionQty(string WorkCenterMasterId, string ProductionDate)
        {
            try
            {
                var result = _ProductionSummary.GetTotalProductionQty(WorkCenterMasterId, ProductionDate);
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

        public IHttpActionResult GetCharacteristicsValueCbo(string soid)
        {
            try
            {
                var result = _ProductionSummary.GetCharacteristicsValueCbo(soid);
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

        [HttpGet]
        public IHttpActionResult Query(string plantId)
        {
            try
            {
                var result = _ProductionSummary.Query(plantId);
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

        public IHttpActionResult GetCbo(string plantId, string ProcessId)
        {
            try
            {
                var result = _ProductionSummary.GetCbo(plantId, ProcessId);
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

        [HttpPost]
        public string Create([FromBody] IEnumerable<ProductionSummary> DataToSave)
        {
            try
            {
                string Id = _ProductionSummary.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        public IHttpActionResult GetSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId)
        {
            try
            {
                var result = _ProductionSummary.GetSOItem(entityid, workCenterMasterId, productionLevel, processId);
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


        public IHttpActionResult GetChar1Info(string id, string soid)
        {
            try
            {
                var result = _ProductionSummary.GetChar1Info(id, soid);
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


        public IHttpActionResult GetCharInfo(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            try
            {
                var result = _ProductionSummary.GetCharInfo(masterid, workdate, mmid, soid, artid, CharCount, CharacteristicsValueId);
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


        public IHttpActionResult GetShiftGroupCbo(string plantId)
        {
            try
            {
                var result = _ProductionSummary.GetShiftGroupCbo(plantId);
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


        public IHttpActionResult GetLineItemGrid(string EntityId, string ProcessId, string ProductionDate, string ProductionShiftId, string WorkCenterMasterId, string ProductionLevel)
        {
            try
            {
                var result = _productionSummaryData.GetLineItemGrid(EntityId, ProcessId, ProductionDate, ProductionShiftId, WorkCenterMasterId, ProductionLevel);
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

        [HttpPost]
        public void SaveSecondDetail([FromBody] IEnumerable<ProductionSummaryDetail> psd, ProductionSummary productionSummary, string companyGroupId, string plantId)
        {
            try
            {
                _ProductionSummary.SaveSecondDetail(psd, productionSummary, companyGroupId, plantId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public void Save([FromBody] ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd)
        {
            try
            {
                _ProductionSummary.Save(ps, psd);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        public void DeleteDetail([FromBody] string masterid)
        {
            try
            {
                _ProductionSummary.DeleteDetail(masterid);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        public void SaveMaster([FromBody] ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string companyGroupId, string ProcessId)
        {
            try
            {
                _ProductionSummary.SaveMaster(ps, psd, companyGroupId, ProcessId,null);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        public void SaveInOutMaster([FromBody] ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string companyGroupId)
        {
            try
            {
                _ProductionSummary.SaveInOutMaster(ps, psd, companyGroupId);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        public void SaveDetail(string psid, IEnumerable<ProductionSummaryDetail> psd)
        {
            try
            {
                _ProductionSummary.SaveDetail(psid, psd);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public IHttpActionResult GetDetailProductionList(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkCenterId, string ProductionOrderId)
        {
            try
            {
                var result = _ProductionSummary.GetDetailProductionList(ProdnDate, EntityId, ProcessId, ShiftId, WkCenterId, ProductionOrderId);
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

        public IHttpActionResult GetListAPIforProduction(string ProdnDate, string EntityId, string ProcessId, string ShiftId)
        {
            try
            {
                var result = _ProductionSummary.GetListAPIforProduction(ProdnDate, EntityId, ProcessId, ShiftId);
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

        [HttpGet]
        public IHttpActionResult GetEntityName(string EntityId, string PlantId)
        {
            try
            {
                var result = _productionSummaryData.GetEntityName(EntityId, PlantId);
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

        public IHttpActionResult GetPRSO(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkCenterId)
        {
            try
            {
                var result = _productionSummaryData.GetPR_SO(ProdnDate, EntityId, ProcessId, ShiftId, WkCenterId);
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


        [HttpGet]
        public IHttpActionResult GetPOCust(string POId)
        {
            try
            {
                var result = _productionSummaryData.GetPOCust(POId);
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



        [HttpGet]
        public IHttpActionResult GetSOCust(string SOId)
        {
            try
            {
                var result = _productionSummaryData.GetSOCust(SOId);
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

        [HttpGet]
        public IHttpActionResult GetSOFields(string SOId)
        {
            try
            {
                var result = _productionSummaryData.GetSOFields(SOId);
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



        [HttpGet]
        public IHttpActionResult GetPOFields(string POId)
        {
            try
            {
                var result = _productionSummaryData.GetPOFields(POId);
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

        [HttpGet]
        public IHttpActionResult GetWk(string plantId, string ProcessId, string EntityId)
        {
            try
            {
                var result = _productionSummaryData.GetWk(plantId, ProcessId, EntityId);
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


    }
}