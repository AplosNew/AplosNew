using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.General.Farming;


namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
    public class BookingSaudaController : ApiController
    {
        #region Constructor
        FarmingData _FarmingData = new FarmingData();
         public BookingSaudaController()
        {

        }

        #endregion Constructor


        [HttpGet]
        public IHttpActionResult GetLocation()
        {
            try
            {
                var result = _FarmingData.getLocations();
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
        public IHttpActionResult GetCustomers()
        {
            try
            {
                var result = _FarmingData.getCustomers();
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
        public IHttpActionResult GetICS()
        {
            try
            {
                var result = _FarmingData.getIcsMasterId();
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
        public IHttpActionResult GetFirstPageInfo()
        {
            try
            {
                var result = _FarmingData.GetFirstPageInfo();
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
        public IHttpActionResult GetFarmer(string IcsId)
        {
            try
            {
                var result = _FarmingData.getFarmer(IcsId);
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
        public IHttpActionResult GetCropPlanning(string IcsId)
        {
            try
            {
                var result = _FarmingData.getCropPlanning(IcsId);
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
        public IHttpActionResult GetChildData(string cropPlanningId, string sodaBookingId)
        {
            try
            {
                var result = _FarmingData.getChildData(cropPlanningId, sodaBookingId);
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
        public string Create([FromBody] IEnumerable<FarmingModel> DataToSave)
        {
            try
            {
                string Id = _FarmingData.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string CreateChild([FromBody] IEnumerable<FarmingChildModel> DataToSave)
        {
            try
            {
                string Id = _FarmingData.CreateChild(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


    }
}
