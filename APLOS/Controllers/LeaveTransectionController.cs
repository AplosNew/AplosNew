using Library.Model.Biometrics;
using Library.Service.Biometrics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Aplos.Controllers
{
    public class LeaveTransectionController : ApiController
    {
        #region Constructor

        private readonly ILeaveTransectionService _leaveTransection;

        public LeaveTransectionController(
             ILeaveTransectionService leaveTransection
          )
        {
            _leaveTransection = leaveTransection;
        }

        #endregion Constructor

        /// <summary>
        /// monir@s API
        /// </summary>
        ///

        public IHttpActionResult GetAllEmployeeInfo(string companyId)
        {
            try
            {
                var result = _leaveTransection.EmployeeInfo();
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

        public IHttpActionResult GetLeaveType(string plantid, string lvpolicymastersystemid, string employeeId)
        {
            try
            {
                var result = _leaveTransection.LoadLvPolicyWiseLeaveTypeCmb(plantid, lvpolicymastersystemid, employeeId);
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

        private T getData<T>(string st, Dictionary<string, object> ob)
        {
            var fabricRoll = ob[st];
            var json = JsonConvert.SerializeObject(fabricRoll);
            var fob = JsonConvert.DeserializeObject<T>(json);
            return fob;
        }

        public void PostLeaveTransaction()
        {
            try
            {
                var responseData = Request.Content;
                var readData = responseData.ReadAsStringAsync().Result;
                var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);

                var ui_data = getData<LeaveTransaction>("data", _objects);
                _leaveTransection.SaveLeaveData(ui_data);
                Request.CreateResponse(HttpStatusCode.OK);
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

        //       public IHttpActionResult GetPlantList(string companyId)
        //	{
        //		try
        //		{
        //			var result = _plants.GetCboByCompany(companyId);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //       public IHttpActionResult GetEmployeeProfile(string id, string plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetEmployeeInformation(id, plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetGroupPrefix(string empid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetGroupPrefix(empid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetAccessControllerList(string Plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetAccessControllerList(Plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetShortLeaveSetting(string Plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetShortLeaveSettings(Plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetIndviEmployeeInformation(string plantid, string cardNumber)
        //	{
        //		try
        //		{
        //			var result = _eri.GetIndviEmployeeInformation(plantid, cardNumber);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetIndviSupVisEmpInfo(string plantid, string cardNumber)
        //	{
        //		try
        //		{
        //			var result = _eri.GetIndviSupVisEmpInfo(plantid, cardNumber);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetPlantWiseShortLeaveKioskDetails(string plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetPlantWiseShortLeaveKioskDetails(plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetFPEngineParameterForWithOutBlackListedEmpInfoViaUSBRd(string plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetFPEngineParameterForWithOutBlackListedEmpInfoViaUSBRd(plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetAccessControllerEmployeeTag(string Plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetAccessControllerEmployeeTag(Plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetAccessControllerEmployeeTagList(string Plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetAccessControllerEmployeeTagList(Plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetSlvAvailedB4SlvApp(string plantId, string empSystemID, string slvDate)
        //	{
        //		try
        //		{
        //			var result = _eri.GetSlvAvailedB4SlvApp(plantId, empSystemID, slvDate);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetShortLeaveAllocation(string plantid)
        //	{
        //		try
        //		{
        //			var result = _eri.GetShortLeaveAllocation(plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetEmployeePin(string employeeid,string pin)
        //	{
        //		try
        //		{
        //			var result = _eri.GetEmployeePin(employeeid, pin);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetSLAPK()
        //	{
        //		try
        //		{
        //			var result = _eri.GetSLAPK();
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	/// <summary>
        //	/// attendance
        //	/// </summary>
        //	public IHttpActionResult GetAttendanceRawDataForSave(string plantid)
        //	{
        //		try
        //		{
        //			var result = "";// _plants.GetCboByCompany(plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetAttendanceLogForSave(string plantid)
        //	{
        //		try
        //		{
        //			var result = "";// _plants.GetCboByCompany(plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetProximityInfo(string plantid, string _date)
        //	{
        //		try
        //		{
        //			var result = _ars.AttendanceProximityInfo(plantid, _date);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}
        //	public IHttpActionResult GetAttendanceLogMaxDate(string plantid)
        //	{
        //		try
        //		{
        //			var result = _addl.AttendanceLogMaxDate(plantid);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public IHttpActionResult GetAccessControllerSingle(string plantid, string ip)
        //	{
        //		try
        //		{
        //			var result = _accessControllerListService.LoadAttdnRawData(plantid, ip);
        //			return Json(result);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //			{
        //				ReasonPhrase = ex.Message
        //			};
        //			throw new HttpResponseException(resp);
        //		}
        //	}

        //	public void PostEmpDeviceTag()
        //	{
        //		var responseData = Request.Content;
        //		var readData = responseData.ReadAsStringAsync().Result;
        //		var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);

        //	    var ui_list = getData<List<AccessControllerEmployeeTag>>("edlist", _objects);
        //	    _edt.SaveList(ui_list);
        //		Request.CreateResponse(HttpStatusCode.OK);
        //	}

        //	//public void PostShortLeaveAllocation()
        //	//{
        //	//	var responseData = Request.Content;
        //	//	var readData = responseData.ReadAsStringAsync().Result;
        //	//	var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);

        //	//	var ui_data = getData<ShortLeaveAllocation>("data", _objects);
        //	//	_shortLeaveAllocationService.SaveData(ui_data);
        //	//	Request.CreateResponse(HttpStatusCode.OK);
        //	//}

        //	public void PostProximityCard()
        //	{
        //		try
        //		{
        //			var responseData = Request.Content;
        //			var readData = responseData.ReadAsStringAsync().Result;
        //			var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);

        //		    var empid = getData<string>("empid", _objects);
        //			var cardnum = getData<string>("cardnum", _objects);
        //		    _eri.SaveProximityCard(empid, cardnum);
        //			var response = new HttpResponseMessage(HttpStatusCode.OK);
        //			// return Task.FromResult(new AplosReturnType() { Status = 1, Message = "Success" });
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.NotFound);
        //			resp.ReasonPhrase = ex.Message;

        //			throw new HttpResponseException(resp);

        //		}
        //	}
        //	public void PostFabricInspection()
        //	{
        //		var responseData = Request.Content;
        //		var readData = responseData.ReadAsStringAsync().Result;
        //		var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);

        //	    var ui = getData<EmployeeFPInformation>("ob", _objects);
        //	    _eri.Save(ui);
        //		Request.CreateResponse(HttpStatusCode.OK);
        //	}
        //	private T getData<T>(string st, Dictionary<string, object> ob)
        //	{
        //		var fabricRoll = ob[st];
        //		var json = JsonConvert.SerializeObject(fabricRoll);
        //		var fob = JsonConvert.DeserializeObject<T>(json);
        //		return fob;
        //	}

        //	///Save Raw Attendance Data
        //	public void PostAttendanceRawData()
        //	{
        //		//string _plantid, _deviceid, _mindate, _maxdate = string.Empty;
        //		try
        //		{
        //			var responseData = Request.Content;
        //			var readData = responseData.ReadAsStringAsync().Result;
        //			var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);
        //			var ui_list = getData<List<AttdnRawData>>("edlist", _objects);

        //			var _plantid = getData<string>("plantid", _objects);
        //			var _deviceid = getData<string>("deviceid", _objects);
        //			var _mindate = getData<string>("mindate", _objects);
        //			var _maxdate = getData<string>("maxdate", _objects);
        //			var _groupid = getData<string>("groupid", _objects);
        //			//string cardnum = getData<string>("cardnum", _objects);

        //			_ars.SaveAttdnRawData(_plantid, _deviceid, _mindate, _maxdate, _groupid, ui_list);
        //			var response = new HttpResponseMessage(HttpStatusCode.OK);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.NotFound);
        //			resp.ReasonPhrase = ex.Message;

        //			throw new HttpResponseException(resp);

        //		}
        //	}
        //	public void PostAttendanceLog()
        //	{
        //		try
        //		{
        //			var responseData = Request.Content;
        //			var readData = responseData.ReadAsStringAsync().Result;
        //			var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);
        //			var ui_list = getData<List<AttdnDataDownLoadLog>>("edlist", _objects);

        //			var _plantid = getData<string>("plantid", _objects);
        //			//string cardnum = getData<string>("cardnum", _objects);

        //			_addl.SaveAttdnDataDownLoadLog(_plantid, ui_list);
        //			var response = new HttpResponseMessage(HttpStatusCode.OK);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.NotFound);
        //			resp.ReasonPhrase = ex.Message;

        //			throw new HttpResponseException(resp);

        //		}
        //	}
        //	public void PostAttendanceProcess()
        //	{
        //		try
        //		{
        //			var responseData = Request.Content;
        //			var readData = responseData.ReadAsStringAsync().Result;
        //			var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);
        //			//List<AttdnDataDownLoadLog> ui_list = getData<List<AttdnDataDownLoadLog>>("edlist", _objects);

        //			var _plantid = getData<string>("plantid", _objects);
        //			//string cardnum = getData<string>("cardnum", _objects);

        //			_apd.SaveTotal(_plantid);
        //			var response = new HttpResponseMessage(HttpStatusCode.OK);
        //		}
        //		catch (Exception ex)
        //		{
        //			var resp = new HttpResponseMessage(HttpStatusCode.NotFound);
        //			resp.ReasonPhrase = ex.Message;

        //			throw new HttpResponseException(resp);

        //		}
        //	}
    }
}