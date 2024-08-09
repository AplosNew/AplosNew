using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.EmployeeServices;

namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class ManualAttndFromAppController : ApiController
    {

        ShiftChangeService app = new ShiftChangeService();

        ManualOTFromAppService _app = new ManualOTFromAppService();

        public ManualAttndFromAppController()
        {
            app = new ShiftChangeService();
            _app = new ManualOTFromAppService();
        }

        #region ShiftChange
        [HttpGet]        
        public IHttpActionResult GetShiftData(string Plant, string Date)
        {
            try
            {
                var result = app.GetShiftData(Plant, Date);
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
        public IHttpActionResult GetExistingShift(string EmpId, string Date)
        {
            try
            {
                var result = app.GetExistingShiftData(EmpId, Date);
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
        public string SaveData([FromBody] List<AttdnManualData> DataToSave)
        {
            try
            {
                string Id = app.SaveData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string Save([FromBody] List<AttendanceProcessNewProcess> data)
        {
            try
            {
                string Id = app.Save(data);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        #endregion

        #region ManualOt
        [HttpGet]
        public IHttpActionResult GetOTConfig()
        {
            try
            {
                var result = _app.GetConfigurationDays();
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
        public string Create([FromBody] IEnumerable<PhysicalVerifyModel> DataToSave)
        {
            try
            {
                string Id = _app.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string BusVerificationFuture([FromBody] IEnumerable<PhysicalVerifyModel> DataToSave)
        {
            try
            {
                string Id = _app.BusVerificationFuture(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string SaveOT([FromBody] List<AttendanceProcessNewProcess> data)
        {
            try
            {
                string Id = _app.Save(data);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion

    }
}
