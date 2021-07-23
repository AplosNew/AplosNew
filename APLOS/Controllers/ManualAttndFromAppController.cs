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

        public ManualAttndFromAppController()
        {

        }       
       
        [HttpGet]        
        public IHttpActionResult GetShiftData(string ShiftId,string Date)
        {
            try
            {
                var result = app.GetShiftData(ShiftId,Date);
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
    }
}
