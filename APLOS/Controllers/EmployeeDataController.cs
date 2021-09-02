using APLOS;
using Library.Model.EmployeeServices;
using Library.Service.EmployeeServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
    public class EmployeeDataController : ApiController
    {
        #region Constructor

        private readonly IEmployeeDataService _EmployeeData;

        public EmployeeDataController(
             IEmployeeDataService EmployeeData
          )
        {
            _EmployeeData = EmployeeData;
        }


        #endregion Constructor

        [HttpGet]
        public IHttpActionResult GetList(string AddedBy)
        {
            try
            {
                var result = _EmployeeData.GetList(AddedBy);
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
        public IHttpActionResult GetShift(string PlantId)
        {
            try
            {
                var result = _EmployeeData.GetShiftMaster(PlantId);
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
        public string Delete([FromBody] IEnumerable<EmployeeData> DataToDelete)
        {
            try
            {
                _EmployeeData.Delete(DataToDelete);
            }
            catch (Exception ex)
            {

                return ex.ToString();

            }
            return "true";

        }


        [HttpPost]
        public string Create([FromBody] IEnumerable<EmployeeData> DataToSave)
        {
            try
            {
                string Id = _EmployeeData.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {                
                return ex.ToString();
            }
        }

        [HttpGet]
        public IHttpActionResult GetEmpCodeId(string CompanyGroupId)
        {
            try
            {
                var result = _EmployeeData.EmpCodeId(CompanyGroupId);
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
        public IHttpActionResult GetCount(string EmpId, string Service)
        {
            try
            {
                var result = _EmployeeData.GetCount(EmpId, Service);
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
        public IHttpActionResult GetDeduction(string EmpId, string Service)
        {
            try
            {
                var result = _EmployeeData.GetDeduction(EmpId, Service);
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
        public IHttpActionResult GetEmpType(string EmpId)
        {
            try
            {
                var result = _EmployeeData.GetEmpType(EmpId);
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
        public IHttpActionResult GetUpdateDeduction(string EmpId, string Service)
        {
            try
            {
                var result = _EmployeeData.GetUpdatedDeduction(EmpId, Service);
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