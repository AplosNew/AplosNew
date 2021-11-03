using Library.Model.QMS;
using Library.Service.QMS;
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
    public class QMSRejectionController : ApiController
    {
        #region Constructor

        private readonly IQMSRejectionService _QMSRejection;

        public QMSRejectionController(
             IQMSRejectionService QMSRejection
          )
        {
            _QMSRejection = QMSRejection;
        }


        #endregion Constructor

        /// <summary>
        /// monir@s API
        /// </summary>
        ///

        [HttpGet]
        public List<QMSRejection> GetList(string Date,string LocationId)
        {
            return _QMSRejection.GetList(Date,LocationId);
        }

        [HttpGet]
        public List<QMSRejection> GetDelete(string strkey)
        {
            return _QMSRejection.GetDelete(strkey);
        }

        [HttpGet]
        public IHttpActionResult GetShiftGroupCbo(string plantId)
        {
            try
            {
                var result = _QMSRejection.GetShiftGroupCbo(plantId);
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
        public IHttpActionResult Get(string Id)
        {
            try
            {
                var result = _QMSRejection.Get(Id);
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

        public IHttpActionResult GetProcess()
        {
            try
            {
                var result = _QMSRejection.GetProcess();
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

        public IHttpActionResult GetShiftMaster()
        {
            try
            {
                var result = _QMSRejection.GetShiftMaster();
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

        public IHttpActionResult GetProductionReference()
        {
            try
            {
                var result = _QMSRejection.GetProductionReference();
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

        public IHttpActionResult GetSKUList()
        {
            try
            {
                var result = _QMSRejection.GetSKUList();
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

        public IHttpActionResult GetLocationList()
        {
            try
            {
                var result = _QMSRejection.GetLocationList();
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


        public IHttpActionResult GetGradeList()
        {
            try
            {
                var result = _QMSRejection.GetGradeList();
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


        public IHttpActionResult GetDefectMasterList()
        {
            try
            {
                var result = _QMSRejection.GetDefectMasterList();
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
                var result = _QMSRejection.GetCustomer();
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
        public IHttpActionResult GetRejectionMasterId(string MasterId)
        {
            try
            {
                var result = _QMSRejection.GetRejectionMasterId(MasterId);
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
        public string Create([FromBody] IEnumerable<QMSRejection> DataToSave)
        {
            try
            {
              string Id= _QMSRejection.Create(DataToSave);
                return Id;
             
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                return "";
            }
        }

        [HttpPost]
        [Route("api/Rej/MasterId/{MasterId}")]
        public void CreateRejectionChild([FromBody] IEnumerable<QMSRejectionChild> ChildData, [FromUri] string MasterId)
        {
            try
            {
              _QMSRejection.CreateRejectionChild(ChildData, MasterId);

               // return Id;
            }
            catch (Exception ex)
            {
                throw ex;
                // return "";
            }
        }

        [HttpPost]
        public void Delete([FromBody] IEnumerable<QMSRejection> DataToDelete)
        {
            try
            {
                _QMSRejection.Delete(DataToDelete);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        //[HttpPost]
        //[Route("api/Rej/MasterId/{MasterId}")]
        //public string SaveDetail([FromUri] string MasterId, [FromBody]IEnumerable<QMSRejectionChild> ChildData)
        //{

        //    try
        //    {
        //        _QMSRejection.SaveDetail(MasterId, ChildData);

        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //        throw ex;
        //    }
        //    return "";
        //}



        //[HttpPost]
        //[Route("api/Rej/MasterId/{MasterId}")]
        //public string CreateRejectionChild([FromBody] IEnumerable<QMSRejectionChild> ChildData,[FromUri] string MasterId)
        //{
        //    try
        //    {
        //       string Id= _QMSRejection.CreateRejectionChild(ChildData, MasterId);

        //       return Id;
        //    }
        //    catch (Exception)
        //    {

        //        return "";
        //    }
        //}


        public IHttpActionResult GetListRejectionChild(string MasterId)
        {
            try
            {
                var result = _QMSRejection.GetListRejectionChild(MasterId);
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
        public IHttpActionResult LoadAllResPersonDetailsForSelection(string CompanyGroupId)
        {
            try
            {
                var result = _QMSRejection.LoadAllResPersonDetailsForSelection(CompanyGroupId);
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
        public List<EmployeeInformation> LoadAllEmpDetailsForSelection(string CompanyGroupId, string Id)
        {
            try
            {
                return _QMSRejection.LoadAllEmpDetailsForSelection(CompanyGroupId, Id);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}