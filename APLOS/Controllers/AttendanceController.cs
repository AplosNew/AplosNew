using APLOS;
using Library.Model.Attendances;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Aplos.Controllers
{
    public class AttendanceController : ApiController
    {
        private readonly IAttdnRawDataFromAppService _raw_fromApp;
        private readonly IEmployeeProfileService _IEmployeeProfileService;
        private readonly IEmployeeMobileAppsAuthorizationService _empAuthService;
        private readonly IEmployeeFPInformationService _eri;

        public AttendanceController(IEmployeeProfileService IEmployeeProfileService
            , IEmployeeFPInformationService eri
            , IEmployeeMobileAppsAuthorizationService empAuthService
            , IAttdnRawDataFromAppService raw_fromApp
            )
        {
            _raw_fromApp = raw_fromApp;
            _eri = eri;
            _IEmployeeProfileService = IEmployeeProfileService;
            _empAuthService = empAuthService;
        }

        public IHttpActionResult GetJobCard()
        {
            try
            {
                string employeeId = "1800001";
                string fromDate = "01-Nov-2018";
                string toDate = "30-Nov-2018";
                //var result = "code";//_fabricRollMasterService.QueryList(id);
                var result = _IEmployeeProfileService.ShowJobCard(employeeId, fromDate, toDate);
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

        public IHttpActionResult GetDailyAttendance(string employeeId, string FromDate, string ToDate)
        {
            try
            {
                //GetEmployeeInformation
                //string employeeId, string SelectedDate
                //Shift Name	Shift InTime	Shift OutTime	Least Punch Time	InTime	OutTime	Day Status
                //Late By	Duration	Short Leave	Leave Type

                //employeeId = "1800001";
                //SelectedDate = "01-Nov-2018";
                var result = _IEmployeeProfileService.ShowDailyAttendance(employeeId, FromDate, ToDate);
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

        public IHttpActionResult GetDailyAttendance(string employeeId, string workingDate)
        {
            try
            {
                //GetEmployeeInformation
                //string employeeId, string SelectedDate
                //Shift Name	Shift InTime	Shift OutTime	Least Punch Time	InTime	OutTime	Day Status
                //Late By	Duration	Short Leave	Leave Type

                //employeeId = "1800001";
                //SelectedDate = "01-Nov-2018";
                var result = _IEmployeeProfileService.ShowDailyAttendance(employeeId, workingDate);
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

        public IHttpActionResult GetEmployeeInformation(string employeeId)
        {
            try
            {
                //GetEmployeeInformation
                //string employeeId, string SelectedDate
                //Shift Name	Shift InTime	Shift OutTime	Least Punch Time	InTime	OutTime	Day Status
                //Late By	Duration	Short Leave	Leave Type

                //employeeId = "1800001";
                //SelectedDate = "01-Nov-2018";
                var result = _eri.GetEmployeeInformation(employeeId);
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

        public IHttpActionResult GetLoginStatus(string employeeId, string password)
        {
            try
            {
                var result = _empAuthService.Login(employeeId, password);
                //if (result["Status"].ToString() == "Success")
                return Json(result);

                //var result = _eri.GetEmployeeInformation(employeeId);
                //return Json(result);
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

        public void xPutRemoteAttendance()
        {
            var responseData = Request.Content;
            var ssss = responseData.ReadAsStringAsync().Result;
            var jObject = JsonConvert.DeserializeObject<AttdnRawDataFromApp>(ssss);
            _raw_fromApp.SaveAttdnRawDataFromApp(jObject);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        public void xPostRemoteAttendance()
        {
            var responseData = Request.Content;
            var readData = responseData.ReadAsStringAsync().Result;
            var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);

            var ui = getData<AttdnRawDataFromApp>("ob", _objects);
            _raw_fromApp.SaveAttdnRawDataFromApp(ui);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        public void PostRemoteAttendance([FromBody]AttdnRawDataFromApp ob)
        {
            //var responseData = Request.Content;
            //var readData = responseData.ReadAsStringAsync().Result;
            //var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);

            //var ui = getData<AttdnRawDataFromApp>("ob", _objects);
            _raw_fromApp.SaveAttdnRawDataFromApp(ob);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        private T getData<T>(string st, Dictionary<string, object> ob)
        {
            var fabricRoll = ob[st];
            var json = JsonConvert.SerializeObject(fabricRoll);
            var fob = JsonConvert.DeserializeObject<T>(json);
            return fob;
        }

        [HttpGet, BasicAuthenticationAttribute]
        public IHttpActionResult GetAttnd(string EmpId)
        {
            try
            {
                var result = _raw_fromApp.GetAttnd(EmpId);
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

        [HttpPost, BasicAuthenticationAttribute]
        public string SaveData([FromBody] IEnumerable<AttdnRawDataFromApp> DataToSave)
        {
            try
            {
                string Id = _raw_fromApp.SaveData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();



            }
        }

    }
}