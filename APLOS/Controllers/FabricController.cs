using Library.Model.Materials;
using Library.Service.Employees;
using Library.Service.Materials;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Aplos.Controllers
{
    public class FabricController : ApiController
    {
        #region Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private readonly IEmpReferenceInformationService _eri;
        private readonly IDefectCodeService _defectCodeService;
        private readonly IFabricRollMasterDefectService _fabricRollMasterDefectService;

        public FabricController(
             IFabricRollMasterService fabricRollMasterService,
             IEmpReferenceInformationService eri,
            IDefectCodeService defectCodeService,
            IFabricRollMasterDefectService fabricRollMasterDefectService
          )
        {
            _fabricRollMasterService = fabricRollMasterService;
            _eri = eri;
            _defectCodeService = defectCodeService;
            _fabricRollMasterDefectService = fabricRollMasterDefectService;
        }

        #endregion Constructor

        // GET: api/FrApi
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET: api/FrApi/5
        public IHttpActionResult GetData(string id)
        {
            try
            {
                var result = _fabricRollMasterService.QueryList(id);
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

        public IHttpActionResult GetQueryRollMasterDefectList(string id)
        {
            try
            {
                var result = _fabricRollMasterService.QueryRollMasterDefectList(id);
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

        public IHttpActionResult GetFriPlantConfigInfo()
        {
            try
            {
                var result = _fabricRollMasterService.QueryFriPlantConfigInfo();
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

        public IHttpActionResult GetDefectCboData()
        {
            try
            {
                var result = _fabricRollMasterService.GetDefectCodeList();
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

        // POST: api/FrApi
        public void Post()
        {
        }

        // PUT: api/FrApi/5
        public void Put()
        {
            var responseData = Request.Content;
            var ssss = responseData.ReadAsStringAsync().Result;
            var jObject = JsonConvert.DeserializeObject<FabricRollMaster>(ssss);
            _fabricRollMasterService.Update(jObject);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        public void PutFabricInitial()
        {
            var responseData = Request.Content;
            var ssss = responseData.ReadAsStringAsync().Result;
            var jObject = JsonConvert.DeserializeObject<FabricRollMaster>(ssss);
            _fabricRollMasterService.UpdateFabricInitial(jObject);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        public void PutFabricInspectionWithDefect()
        {
            var responseData = Request.Content;
            var readData = responseData.ReadAsStringAsync().Result;
            var objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);
            var fabricRollMaster = GetData<FabricRollMaster>("fabric", objects);
            var fabricRollDefect = GetData<FabricRollMasterDefect>("fabricDefect", objects);
            _fabricRollMasterService.UpdateFabricInsPectionWithDefect(fabricRollMaster, fabricRollDefect);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        public void PutFabricInspection()
        {
            var responseData = Request.Content;
            var ssss = responseData.ReadAsStringAsync().Result;
            var jObject = JsonConvert.DeserializeObject<FabricRollMaster>(ssss);
            _fabricRollMasterService.UpdateFabricInsPection(jObject);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        private T GetData<T>(string st, Dictionary<string, object> ob)
        {
            var fabricRoll = ob[st];
            var json = JsonConvert.SerializeObject(fabricRoll);
            var fob = JsonConvert.DeserializeObject<T>(json);
            return fob;
        }

        public void PostFabricInspectionDefect()
        {
            var responseData = Request.Content;
            var readData = responseData.ReadAsStringAsync().Result;
            var fabDefect = JsonConvert.DeserializeObject<FabricRollMasterDefect>(readData);
            _fabricRollMasterDefectService.Insert(fabDefect);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        // DELETE: api/FrApi/5
        public void DeleteDefectCode(string id)
        {
            _fabricRollMasterDefectService.DeleteGraph(id);
            Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}