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

namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
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
        public IHttpActionResult GetLeaveBalance(string GroupId, string PlantId, string EmpId, string CalId)
        {
            try
            {
                var result = _leaveapp.GetLeaveBalanceType(GroupId, PlantId, EmpId, CalId);
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
    }
}