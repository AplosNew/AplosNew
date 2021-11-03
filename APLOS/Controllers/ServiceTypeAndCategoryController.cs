using Library.Model.EmployeeServices;
using Library.Service.EmployeeServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;

namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
    public class ServiceTypeAndCategoryController : ApiController
    {
        #region Constructor

        private readonly IServiceTypeAndCategoryService _ServiceTypeAndCategory;

        public ServiceTypeAndCategoryController(
             IServiceTypeAndCategoryService ServiceTypeAndCategory
          )
        {
            _ServiceTypeAndCategory = ServiceTypeAndCategory;
        }


        #endregion Constructor

        [HttpGet]
        public List<ServiceTypeAndCategory> GetList(string Service)
        {
            return _ServiceTypeAndCategory.GetList(Service);
        }

        [HttpGet]
        public List<ServiceCategory> GetCategoryList(string Service)
        {
            return _ServiceTypeAndCategory.GetCategoryList(Service);
        }


        [HttpGet]
        public IHttpActionResult GetAllServices()
        {
            try
            {
                var result = _ServiceTypeAndCategory.GetAllServices();
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
        public IHttpActionResult GetEmpName(string EmpCode)
        {
            try
            {
                var result = _ServiceTypeAndCategory.GetEmpName(EmpCode);
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