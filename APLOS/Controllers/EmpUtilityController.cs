using System;
using Library.Service.EmployeeServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
    public class EmpUtilityController : ApiController
    {
        EmpUtilityService _emp= new EmpUtilityService();
        public EmpUtilityController()
        {

        }
       
        [HttpPost]
        public string Create([FromBody] IEnumerable<PhysicalVerifyModel> DataToSave)
        {
            try
            {
                string Id = _emp.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        #region Nitesh
        [Route("api/EmpUtiliy/PostCreate")]
        [HttpPost, AllowAnonymous]
        public string PostCreate([FromBody] IEnumerable<CreateDetentionList> DataToSave)
        {
            try
            {

                string Id = _emp.CreateDetentionLog(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion Nitesh

        [HttpGet]
        
        public IHttpActionResult GetEmpInfo(string Code)
        {
            try
            {
                var result = _emp.GetEmpInfo(Code);
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
        public IHttpActionResult GetEmpCode(string GpId, string CompId, string PlantId)
        {
            try
            {
                var result = _emp.GetEmpCode(GpId,CompId,PlantId);
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
        public IHttpActionResult GetBudgetData(string GpId, string CompId, string PlantId,string Code)
        {
            try
            {
                var result = _emp.GetBudgetData(GpId, CompId, PlantId,Code);
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
        public IHttpActionResult GetBudgetCode(string GpId, string CompId,string PlntId)
        {
            try
            {
                var result = _emp.GetBudgetCode(GpId, CompId,PlntId);
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
        public IHttpActionResult GetDesgGp(string DesgId)
        {
            try
            {
                var result = _emp.GetDesignationGroup(DesgId);
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
                string Id = _emp.SaveData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string UpdateBudgetCode([FromBody] IEnumerable<EmployeeInformationViewModel> DataToSave)
        {
            try
            {
                string Id = _emp.Createx(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        
        /// For Status Check 
            
        [HttpGet]
        public IHttpActionResult GetSeniorCode(string EmpId)
        {
            try
            {
                var result = _emp.GetSeniorBudgetCode(EmpId);
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
        public IHttpActionResult GetROEmp(string BudgetId, string FromDate,string ToDate)
        {
            try
            {
                var result = _emp.GetROEmp(BudgetId, FromDate,ToDate);
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
        public IHttpActionResult GetPREmp(string BudgetId, string FromDate,string ToDate)
        {
            try
            {
                var result = _emp.GetPREmp(BudgetId, FromDate,ToDate);
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

        /// OT 
        [HttpGet]
        public IHttpActionResult GetDepartment()
        {
            try
            {
                var result = _emp.GetDepartment();
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
        public IHttpActionResult GetSubSection()
        {
            try
            {
                var result = _emp.GetSubSection();
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
        public IHttpActionResult GetSection()
        {
            try
            {
                var result = _emp.GetSection();
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
       
       // New 
        [HttpGet]
        public IHttpActionResult GetUpdOTEmpCode(string GpId, string CompId, string PlantId, string Date, string DepId, string SId, string SsId)
        {
            try
            {
                var result = _emp.GetUpdOTEmpCode(GpId, CompId, PlantId, Date, DepId, SId, SsId);
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
        public IHttpActionResult GetAttndLock(string PlantId, string Date)
        {
            try
            {
                var result = _emp.GetAttndLock(PlantId,Date);
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
        public IHttpActionResult GetAttndUnLock(string EmpId, string Date)
        {
            try
            {
                var result = _emp.GetAttndUnLock(EmpId, Date);
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
        public IHttpActionResult GetAttndStatus(string EmpId, string Date)
        {
            try
            {
                var result = _emp.GetAttndStatus(EmpId, Date);
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

        // Shift & Attnd
        [HttpGet]
        public IHttpActionResult GetShiftAttnd(string EmpId, string Date)
        {
            try
            {
                var result = _emp.GetShiftAttnd(EmpId, Date);
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
        public IHttpActionResult GetShiftTimings(string PlantId)
        {
            try
            {
                var result = _emp.GetShiftTimings(PlantId);
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
        public IHttpActionResult GetPartShift(string PlantId,string Id)
        {
            try
            {
                var result = _emp.GetPartShift(PlantId,Id);
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
        public IHttpActionResult CheckOTEligible(string EmpId)
        {
            try
            {
                var result = _emp.CheckOTEligible(EmpId);
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
        public string CreateOT([FromBody] IEnumerable<PhysicalVerifyModel> DataToSave)
        {
            try
            {
                string Id = _emp.CreateOT(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string SaveManualOT([FromBody] IEnumerable<AttendanceProcessNewProcess> DataToSave)
        {
            try
            {
                string Id = _emp.SaveManualOT(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


        [HttpGet]
        public IHttpActionResult GetOTId(string EmpId, string Date)
        {
            try
            {
                var result = _emp.GetOTId(EmpId, Date);
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
        public IHttpActionResult GetOTEmpCode(string GpId, string CompId, string PlantId, string Date)
        {
            try
            {
                var result = _emp.GetOTEmpCode(GpId, CompId, PlantId, Date);
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
        public IHttpActionResult EmpCompCode(string CGId, string CId)
        {
            try
            {
                var result = _emp.EmpCompCode(CGId, CId);
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
        public IHttpActionResult GetUserId(string UserId)
        {
            try
            {
                var result = _emp.GetuserInfo(UserId);
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
        public IHttpActionResult CheckOD(string EmpId, string Date)
        {
            try
            {
                var result = _emp.CheckOD(EmpId, Date);
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

        // New 
             
        [HttpGet]
        public IHttpActionResult CheckSalaryLock(string EmpId, string Month, string Year)
        {
            try
            {
                var result = _emp.CheckSalaryLock(EmpId, Month, Year);
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
        public IHttpActionResult UpdateInFinal(string EmpId,string WKdate,string Shift)
        {
            try
            {
                var result = _emp.UpdateInLive(EmpId,WKdate,Shift);
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
