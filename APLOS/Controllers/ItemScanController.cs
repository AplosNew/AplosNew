using System;
using Library.Service.EmployeeServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;

namespace Aplos.Controllers
{
   // [BasicAuthenticationAttribute]
    public class ItemScanController : ApiController
    {
        ItemScanService _scan= new ItemScanService();
        public ItemScanController()
        {
            _scan = new ItemScanService();
        }
       
       
        [HttpGet]
        public IHttpActionResult GetFromLoc(string Entity,string Purp)
        {
            try
            {
                var result = _scan.FromLoc(Entity,Purp);
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
        public IHttpActionResult GetToLoc(string Entity, string Purp, string From)
        {
            try
            {
                var result = _scan.ToLoc(Entity, Purp, From);
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
        public string SaveHeader([FromBody] IEnumerable<ItemScanData> DataToSave)
        {
            try
            {
                string Id = _scan.SaveHeader(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpGet]
        public IHttpActionResult GetShift(string PlantId)
        {
            try
            {
                var result = _scan.GetShiftMaster(PlantId);
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
        [Route("api/Scan/MId/{MId}")]
        public string Create([FromUri] string MId, [FromBody] IEnumerable<ItemScanChildData> DataToSave)
        {
            try
            {
                string Id = _scan.Create(MId, DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpGet]
        public IHttpActionResult GetPurp(string Entity)
        {
            try
            {
                var result = _scan.GetPurpose(Entity);
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


        // Dispatch & Booking


        [HttpGet]
        public IHttpActionResult GetPackingId(string Cust,string User)
        {
            try
            {
                var result = _scan.GetPackingId(Cust,User);
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
        public IHttpActionResult GetCustomer()
        {
            try
            {
                var result = _scan.GetCust();
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
        public IHttpActionResult GetSO(string PId)
        {
            try
            {
                var result = _scan.GetSO(PId);
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
        public IHttpActionResult GetProdCode(string PL)
        {
            try
            {
                var result = _scan.GetProdCode(PL);
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
        public IHttpActionResult GetPO(string PL, string Prod)
        {
            try
            {
                var result = _scan.GetPO(PL,Prod);
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
        public IHttpActionResult GetLot(string PL, string Prod, string PO)
        {
            try
            {
                var result = _scan.GetLotId(PL,  Prod,PO);
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
        public IHttpActionResult GetBookQty(string Lot, string Prod, string PO,string Pqty,string PoLotRefernceId)
        {
            try
            {
                var result = _scan.GetBookedQty(Lot, Prod, PO,Pqty,PoLotRefernceId);
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
        public IHttpActionResult GetBookQtyMsg(string PackingId)
        {
            try
            {
                var result = _scan.GetBookedQtyMsg(PackingId);
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
        public string CreateDispatch([FromBody] IEnumerable<ItemScanChildData> DataToSave)
        {
            try
            {
                string Id = _scan.CreateDispatch(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
             
    }
}
