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
    public class QMSInspectionController : ApiController
    {
        #region Constructor

        private readonly IQMSInspectionService _QMSInspection;

        public QMSInspectionController(
             IQMSInspectionService QMSInspection
          )
        {
            _QMSInspection = QMSInspection;
        }


        #endregion Constructor

        /// <summary>
        /// monir@s API
        /// </summary>
        ///

        [HttpGet]
        public IHttpActionResult GetShiftGroupCbo(string plantId)
        {
            try
            {
                var result = _QMSInspection.GetShiftGroupCbo(plantId);
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
        public List<QMSInspection> GetList(string Date, string LocationId)
        {
            return _QMSInspection.GetList(Date,LocationId);
        }

        [HttpGet]
        public List<QMSInspection> GetDelete(string strkey)
        {
            return _QMSInspection.GetDelete(strkey);
        }
        public IHttpActionResult Get(string Id)
        {
            try
            {
                var result = _QMSInspection.Get(Id);
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
                var result = _QMSInspection.GetProcess();
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

        public IHttpActionResult GetInspectionLevel(string InspectionMasterId)
        {
            try
            {
                var result = _QMSInspection.GetInspectionLevel(InspectionMasterId);
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

        public IHttpActionResult GetInspectionMasterList()
        {
            try
            {
                var result = _QMSInspection.GetInspectionMasterList();
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
        public IHttpActionResult GetProductionReference()
        {
            try
            {
                var result = _QMSInspection.GetProductionReference();
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
                var result = _QMSInspection.GetShiftMaster();
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

        public IHttpActionResult GetInspectionType()
        {
            try
            {
                var result = _QMSInspection.GetInspectionType();
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
                var result = _QMSInspection.GetLocationList();
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
        public IHttpActionResult GetStatusList()
        {
            try
            {
                var result = _QMSInspection.GetStatusList();
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


        public IHttpActionResult Getdefectmasterlist()
        {
            try
            {
                var result = _QMSInspection.Getdefectmasterlist();
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


        public IHttpActionResult Getdefectzonelist()
        {
            try
            {
                var result = _QMSInspection.Getdefectzonelist();
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


        public IHttpActionResult Getskilllist()
        {
            try
            {
                var result = _QMSInspection.Getskilllist();
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
                var result = _QMSInspection.GetCustomer();
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

        //[HttpPost]
        //public void SaveDetail([FromUri] string inpid, [FromBody] IEnumerable<QMSInspectionChild> ChildData)
        //{
        //    try
        //    {
        //        _QMSInspection.SaveDetail(inpid, ChildData);
        //    }

        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

      
        [HttpPost]
        public string Create([FromBody] IEnumerable<QMSInspection> DataToSave)
        {

            try
            {
                string Id = _QMSInspection.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {


                Console.WriteLine(ex);
                return "";
            }
        }

        [HttpPost]
        [Route("api/Ins/MasterId/{MasterId}")]
        public void CreateInspectionChild([FromBody] IEnumerable<QMSInspectionChild> ChildData, [FromUri] string MasterId)
        {
            try
            {
                 _QMSInspection.CreateInspectionChild(ChildData, MasterId);

               // return Id;
            }
            catch (Exception ex)
            {
                throw ex;
               // return "";
            }
        }

        //[HttpPost]
        //[Route("api/Insp/MasterId/{MasterId}")]
        //public string SaveDetail([FromUri] string MasterId, [FromBody]IEnumerable<QMSInspectionChild> ChildData)
        //{

        //    try
        //    {
        //        _QMSInspection.SaveDetail(MasterId, ChildData);

        //    }
        //    catch (Exception ex)
        //    {
        //      //  throw ex;
        //      //  throw ex;
        //        return ex.ToString();
        //    }
        //    return "";
        //}

        [HttpPost]
        public void Delete([FromBody] IEnumerable<QMSInspection> DataToDelete)
        {
            try
            {
                _QMSInspection.Delete(DataToDelete);
            }
            catch (Exception)
            {

                throw;
            }
        }

       
        public IHttpActionResult GetListInspectionChild(string QMSInspectionId)
        {
            try
            {
                var result = _QMSInspection.GetListInspectionChild(QMSInspectionId);
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
                var result = _QMSInspection.LoadAllResPersonDetailsForSelection(CompanyGroupId);
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
                return _QMSInspection.LoadAllEmpDetailsForSelection(CompanyGroupId, Id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public List<EmployeeInformation> LoadAllDefResPonDetailsForSelection(string CompanyGroupId, string Id)
        {
            try
            {
                return _QMSInspection.LoadAllDefResPonDetailsForSelection(CompanyGroupId, Id);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}