using Library.Service.EmployeeServices;
using Library.Service.Setups;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.Service.Helpers;
using System.Data;
using Library.Data.UnitOfWorks;
using Library.Service.Biometrics;
using APLOS;
using Library.HumanResource.Leave;
using Library.Service.Leave;

namespace Aplos.Controllers
{
    //[BasicAuthenticationAttribute]
    public class LeaveSystemController : ApiController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailSenderService _mailSenderService;
        private readonly ILeaveTransactionDetailsService _leaveTransactionDetailsService;

        LeaveApplicationData _leaveapp;

        public LeaveSystemController(IMailSenderService mailSenderService, IUnitOfWork unitOfWork,
            ILeaveTransactionDetailsService leaveTransactionDetailsService)
        {
            _mailSenderService = mailSenderService;
            _leaveTransactionDetailsService = leaveTransactionDetailsService;
            _unitOfWork = unitOfWork;
            _leaveapp = new LeaveApplicationData(_mailSenderService, _unitOfWork, _leaveTransactionDetailsService);
        }

        #endregion Constructor

        #region Leave Application

        [HttpGet]
        public IHttpActionResult GetEmpInfo(string EmpId)
        {
            try
            {
                var result = _leaveapp.GetEmpInfo(EmpId);
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
        public IHttpActionResult GetCalendar(string PlantId)
        {
            try
            {
                var result = _leaveapp.GetCalender(PlantId);
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
        public IHttpActionResult GetLeaveType(string PlantId, string EmpId, string GroupId)
        {
            try
            {
                var result = _leaveapp.GetLeaveType(PlantId, EmpId, GroupId);
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
        public string Create([FromBody] IEnumerable<LeaveData> DataToSave)
        {
            try
            {
                string Id = _leaveapp.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpGet]
        public IHttpActionResult GetLeaveBalance(string EmpId, string CalId)
        {
            try
            {
                clsLeaveBalanceToDate app = new clsLeaveBalanceToDate();
                var result = app.GetLeaveBalanceTypeApp(EmpId, CalId);
                //var result = _leaveapp.GetLeaveBalanceType(GroupId, PlantId, EmpId, CalId);
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
        public IHttpActionResult GetEmp(string EmpId)
        {
            try
            {
                var result = _leaveapp.GetEmp(EmpId);
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
        public IHttpActionResult GetHistory(string CGId, string CompId, string plantId, string EmpId)
        {
            try
            {
                var result = _leaveapp.Query(CGId, CompId, plantId, EmpId);
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

        #endregion

        #region Leave Approval

        [HttpGet]
        public IHttpActionResult GetLeaveApprovalList(string plantId, bool isControlAdmin, bool isSysAdmin, string EmpId, string CompId)
        {
            try
            {
                var result = _leaveapp.GetApprovalList(plantId,isControlAdmin,isSysAdmin,EmpId, CompId,EmpId);
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
        public IHttpActionResult GetEmpLeaveBalanceForApprovalScreen(string EmpId,string CalId)
        {
            try
            {              
                clsLeaveBalanceToDate app = new clsLeaveBalanceToDate();
                var result = app.GetLeaveBalanceType(EmpId, CalId);
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
        public string LeaveApprove([FromBody] IEnumerable<LeaveVM> DataToSave)
        {
            try
            {
                string Id = _leaveapp.SaveLeaveApproval(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        [Route("api/Reject/Reason/{Reason}")]
        public string LeaveReject([FromBody] IEnumerable<LeaveVM> DataToSave,[FromUri] string Reason)
        {
            try
            {
                string Id = _leaveapp.SaveLeaveReject(DataToSave,Reason);
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