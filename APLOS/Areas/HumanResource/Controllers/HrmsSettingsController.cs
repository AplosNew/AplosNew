using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class HrmsSettingsController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IHrmsSettingsService _hrmsSettingsService;
        private readonly IOTManagementService _OTManagementService;
        public HrmsSettingsController(
              IHrmsSettingsService hrmsSettingsService, IOTManagementService OTManagementService, ISqlRepository sqlRepository
            )
        {
            _hrmsSettingsService = hrmsSettingsService;
            _OTManagementService = OTManagementService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        //plant wise lock
        [Authorize]
        public ActionResult PlantWiseAttendanceLock()
        {
            return View();
        }
        [Authorize]
        public ActionResult PlantWiseAttendanceUnLock()
        {
            return View();
        }

        [Authorize]
        public ActionResult EmployeeWiseAttendanceLock()
        {
            return View();
        }
        [Authorize]
        public ActionResult EmployeeWiseAttendanceUnLock()
        {
            return View();
        }
        //employee wise 
        [Authorize]
        public ActionResult EmployeeAndPlantWiseAttendanceUnLock()
        {
            return View();
        }
        [Authorize]
        public ActionResult IndividualAttendanceLock()
        {
            return View();
        }

        [Authorize]
        public ActionResult IndividualAttendanceUnLock()
        {
            return View();
        }



        [Authorize]
        public ActionResult DateRangeWiseAttendanceUnLock()
        {
            return View();
        }


        #endregion -- Pages

        #region -- Operations

        #region Lock and Un-Lock
        //plant wise lock
        [HttpPost]
        public JsonResult GetEmployeeData(string lockDate)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();


            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;
            bool IsOTConfirmationAuto = false;
            bool IsOutMissingValidationRequired = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAfterLock = false;
            IEnumerable<object> OutPunchMissingData = null;
            ////IEnumerable<object> EmpListForOTConfirmation = null;
            IEnumerable<object> OTUnConfirmedEmployees = null;
            IEnumerable<object> OutPunchMissingDataForAlert = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out DataSet dsLocalHRMSSetting);
            if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
            {
                MinimumOTMinute = dsLocalHRMSSetting.Tables[0].Rows[0]["MinimumOTMinute"].ToString().Trim();
                OTConsiderOn = dsLocalHRMSSetting.Tables[0].Rows[0]["OTConsiderOn"].ToString().Trim();
                OTFractionCalculate = dsLocalHRMSSetting.Tables[0].Rows[0]["OTFractionCalculation"].ToString().Trim();
                //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                if (IsPreallocationBasedOT)
                {
                    IsPunchBasedOT = false;
                }
                IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                {
                    IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                }
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                {
                    IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                }

            }






            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var UnApprovedEmployees = _hrmsSettingsService.GetUnApprovedEmployeeListData(lockDate);
            //var OTUnConfirmedEmployees = _hrmsSettingsService.GetOTConfirmationData(lockDate);
            var ShiftNotAssignEmployees = _hrmsSettingsService.GetShiftNotAssignData(lockDate);
            var AttdencenotNotProcEmployees = _hrmsSettingsService.GetAttdencenotNotProcData(lockDate);
            var LastLockDate = _hrmsSettingsService.GetLastLockDate();
            if (IsOutMissingValidationRequired)
            {
                OutPunchMissingData = _hrmsSettingsService.GetOutPunchMissingData(lockDate);
            }

            if (IsOTConfirmationAfterLock == false)
            {

                if (IsOTConfirmationAuto == false)
                {
                    if (IsOTConfirmationAutoForZeroAuto)
                    {
                        OTUnConfirmedEmployees = _hrmsSettingsService.GetOTConfirmationDataForZeroAuto(lockDate);

                    }
                    else
                    {
                        OTUnConfirmedEmployees = _hrmsSettingsService.GetOTConfirmationData(lockDate);
                    }

                    // OutPunchMissingData = _hrmsSettingsService.GetOutPunchMissingData(lockDate);
                    //EmpListForOTConfirmation = _OTManagementService.LoadEmpForOTConfirmation(identity.CompanyGroupId, identity.PlantId, lockDate,"" /*OTvalCons*/);
                }

            }






            OutPunchMissingDataForAlert = _hrmsSettingsService.GetOutPunchMissingDataForAlert(lockDate);
            //var LockEmpList = _hrmsSettingsService.GetLockEmployeeListData(lockDate, identity.PlantId);
            //var ToBeLockEmpList = _hrmsSettingsService.GetTobeLockEmployeeListData(lockDate, identity.PlantId);

            JsonResult json = Json(new
            {
                UnApprovedEmployees
                ,
                OTUnConfirmedEmployees
                ,
                ShiftNotAssignEmployees
                ,
                AttdencenotNotProcEmployees
                ,
                LastLockDate/*, LockEmpList, ToBeLockEmpList*/
                ,
                OutPunchMissingData
                ,
                OutPunchMissingDataForAlert
                ,
                IsOutMissingValidationRequired,
                IsOTConfirmationAuto,
                IsOTConfirmationAfterLock
            }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        //[HttpGet]  //employee wise
        //public JsonResult xGetLockEmployeeList(string lockDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var LockEmployees = _hrmsSettingsService.GetLockEmployeeList(lockDate,"", identity);
        //    var ReLockEmployees = _hrmsSettingsService.GetReLockEmployeeList(lockDate,"", identity);
        //    JsonResult json = Json(new { LockEmployees, ReLockEmployees }, JsonRequestBehavior.AllowGet);
        //    json.MaxJsonLength = int.MaxValue;
        //    return json;

        //}
        //[HttpGet, Authorize]
        //public JsonResult xGetReLockEmployeeList(string lockDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var LockEmployees = _hrmsSettingsService.GetLockEmployeeList(lockDate,"", identity);
        //    var ReLockEmployees = _hrmsSettingsService.GetReLockEmployeeList(lockDate,"", identity);
        //    JsonResult json = Json(new { LockEmployees, ReLockEmployees }, JsonRequestBehavior.AllowGet);
        //    json.MaxJsonLength = int.MaxValue;
        //    return json;

        //}




        [HttpGet, Authorize]
        public JsonResult GetLastLockDate()
        {


            var LastLockDate = _hrmsSettingsService.GetLastLockDate();
            JsonResult json = Json(new { LastLockDate }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpGet, Authorize]
        public JsonResult GetLockDateList()
        {


            var LastLockDate = _hrmsSettingsService.GetLockDateList();
            JsonResult json = Json(new { LastLockDate }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpGet, Authorize]
        public JsonResult GetUnLockDateList()
        {


            var LastLockDate = _hrmsSettingsService.GetUnLockDateList();
            JsonResult json = Json(new { LastLockDate }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }


        [HttpPost, Authorize]
        public JsonResult GetAllEmployeeList(string fromdate, string todate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Employees = _hrmsSettingsService.GetAllEmployeeListData(fromdate, todate, identity.PlantId);
            JsonResult json = Json(new { Employees }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }


        [HttpPost, Authorize]
        public JsonResult GetEmployeeWiseLockData(string empsystemid, string fromdate, string todate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Employees = _hrmsSettingsService.GetEmployeeWiseLockData(empsystemid, fromdate, todate, identity.PlantId);
            JsonResult json = Json(new { Employees }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }


        //[HttpPost, Authorize]
        //public JsonResult XCreateLockData(string lockDate, string[] LockDateWiseEmployeeList)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    _hrmsSettingsService.CreateLockDataDateWise(lockDate, LockDateWiseEmployeeList, identity.Name, DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
        //    //_hrmsSettingsService.CreateLockData(lockDate);
        //    return Json(new { Message = AplosMessage.Success });

        //}



        #region plant wise
        [HttpPost]  //plant wise lock
        public JsonResult CreateLockData(string lockDate)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAuto = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAutoException = false;
            bool IsOTConfirmationAfterLock = false;
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim()))
                    {
                        IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());

                    }
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                    {
                        IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                    }
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                    {
                        IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                    }
                }



                if (IsOTConfirmationAuto)///Is OT Confirmation Auto
                {
                    var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                    if (MissPunchEmployeeListAuto.Count() > 0)
                    {
                        IsOTConfirmationAutoException = true;
                        return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
                    }

                    DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                    if (employeeOTInformationAuto.Tables[0].Rows.Count > 0)
                    {
                        _OTManagementService.SaveData(lockDate, employeeOTInformationAuto);
                    }
                    DataSet employeeOTInformationReconfirmAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                    if (employeeOTInformationReconfirmAuto.Tables[0].Rows.Count > 0)
                    {
                        _OTManagementService.SaveData(lockDate, employeeOTInformationReconfirmAuto);
                    }


                    //DataSet employeeMaternityWithOTInformationAuto = _OTManagementService.LoadEmpMaternityWithOTAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                    //if (employeeMaternityWithOTInformationAuto.Tables[0].Rows.Count > 0)
                    //{
                    //    string message = string.Empty;
                    //    for (int i = 0; i < employeeMaternityWithOTInformationAuto.Tables[0].Rows.Count; i++)
                    //    {
                    //        //message += employeeMaternityWithOTInformationAuto.Tables[0].Rows[0]["EmployeeCode"].ToString();


                    //        if (message == "")
                    //            message = "'" + employeeMaternityWithOTInformationAuto.Tables[0].Rows[0]["EmployeeCode"].ToString() + "'";
                    //        else
                    //            message = message + ",'" + employeeMaternityWithOTInformationAuto.Tables[0].Rows[0]["EmployeeCode"].ToString() + "'";


                    //    }
                    //    throw new Exception("OT is not Confirmed. Employee Code [ " + message + " ].");

                    //}
                    DataSet employeeMaternityWithOTInformationAuto = _OTManagementService.LoadEmpMaternityWithOTAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                    //employeeMaternityWithOTInformationAuto.Tables[0].DefaultView.RowFilter = "OTHrInMin=0";
                    DataView dv = new DataView(employeeMaternityWithOTInformationAuto.Tables[0]);
                    dv.RowFilter = "OTHrInMin=0";

                    DataSet newDs = new DataSet();
                    newDs.Tables.Add(dv.ToTable());

                    if (newDs.Tables[0].Rows.Count > 0)
                    {
                        _OTManagementService.SaveData(lockDate, newDs);
                    }

                    dv.RowFilter = null;
                    dv.RowFilter = "OTHrInMin>0";


                    if (dv.Count > 0)
                    {
                        string message = string.Empty;
                        for (int i = 0; i < dv.Count; i++)
                        {
                            //message += employeeMaternityWithOTInformationAuto.Tables[0].Rows[0]["EmployeeCode"].ToString();


                            if (message == "")
                                message = "'" + dv[i]["EmployeeCode"].ToString() + "'";
                            else
                                message = message + ",'" + dv[i]["EmployeeCode"].ToString() + "'";


                        }
                        throw new Exception("OT is not Confirmed. Employee Code [ " + message + " ].");

                    }




                }
                else ///OT Confirmation Auto False
                {
                    if (IsOTConfirmationAfterLock == false)
                    {
                        if (IsOTConfirmationAutoForZeroAuto)///OT Confirmation Auto For Zero Auto
                        {
                            var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                            if (MissPunchEmployeeListAuto.Count() > 0)
                            {
                                IsOTConfirmationAutoException = true;
                                return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
                            }

                            DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");

                            if (employeeOTInformationAuto.Tables[0].Rows.Count > 0)
                            {
                                _OTManagementService.SaveData(lockDate, employeeOTInformationAuto);
                            }
                            DataSet employeeOTInformationReconfirmAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                            if (employeeOTInformationReconfirmAuto.Tables[0].Rows.Count > 0)
                            {
                                _OTManagementService.SaveData(lockDate, employeeOTInformationReconfirmAuto);
                            }


                            DataSet employeeMaternityWithOTInformationAuto = _OTManagementService.LoadEmpMaternityWithOTAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
                            //employeeMaternityWithOTInformationAuto.Tables[0].DefaultView.RowFilter = "OTHrInMin=0";
                            DataView dv = new DataView(employeeMaternityWithOTInformationAuto.Tables[0]);
                            dv.RowFilter = "OTHrInMin=0";

                            DataSet newDs = new DataSet();
                            newDs.Tables.Add(dv.ToTable());

                            if (newDs.Tables[0].Rows.Count > 0)
                            {
                                _OTManagementService.SaveData(lockDate, newDs);
                            }

                            dv.RowFilter = null;
                            dv.RowFilter = "OTHrInMin>0";


                            if (dv.Count > 0)
                            {
                                string message = string.Empty;
                                for (int i = 0; i < dv.Count; i++)
                                {
                                    //message += employeeMaternityWithOTInformationAuto.Tables[0].Rows[0]["EmployeeCode"].ToString();


                                    if (message == "")
                                        message = "'" + dv[i]["EmployeeCode"].ToString() + "'";
                                    else
                                        message = message + ",'" + dv[i]["EmployeeCode"].ToString() + "'";


                                }
                                throw new Exception("OT is not Confirmed. Employee Code [ " + message + " ].");

                            }
                        }
                    }
                }



                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //_hrmsSettingsService.CreateLockDataDateWise(lockDate, LockDateWiseEmployeeList, identity.Name, DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
                _hrmsSettingsService.CreateLockData(lockDate);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json(new { IsOTConfirmationAutoException, Message = AplosMessage.Success });

        }
        [HttpPost] //plant wise
        public JsonResult CreateUnLockData(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _hrmsSettingsService.CreateUnLockData(lockDate, identity.PlantId);
            return Json(new { Message = AplosMessage.Success });
        }
        #endregion


        #region Employee wise
        //[HttpPost, Authorize]
        //public JsonResult xCreateUnLockDataEmployeeWise(string lockDate, string[] UnLockEmployeeList)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    _hrmsSettingsService.CreateUnLockDataEmployeeWise(lockDate, UnLockEmployeeList, (CustomIdentity)Thread.CurrentPrincipal.Identity);
        //    //_hrmsSettingsService.CreateLockData(lockDate);
        //    return Json(new { Message = AplosMessage.Success });

        //}
        //[HttpPost, Authorize]
        //public JsonResult xCreateReLockDataEmployeeWise(string lockDate, string[] ReLockEmployeeList)
        //{

        //    DataSet dsLocalHRMSSetting = null;
        //    clsStaticInfo objStatic = null;
        //    objStatic = new clsStaticInfo();
        //    bool IsOTConfirmationAuto = false;
        //    bool IsOTConfirmationAutoException = false;
        //    bool IsOutMissingValidationRequired = false;
        //    try
        //    {


        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
        //        if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
        //        {
        //            IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
        //            IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
        //        }



        //        if (IsOTConfirmationAuto)
        //        {
        //            var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, lockDate, "");
        //            if (MissPunchEmployeeListAuto.Count() > 0)
        //            {
        //                string message = string.Empty;
        //                foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
        //                {
        //                    if (message == "")
        //                        message = "'" + item["EmployeeCode"].ToString() + "'";
        //                    else
        //                        message = message + ",'" + item["EmployeeCode"].ToString() + "'";
        //                }
        //                //for (int i = 0; i < MissPunchEmployeeListAuto.Count(); i++)
        //                //{

        //                //}

        //                throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");
        //                //IsOTConfirmationAutoException = true;
        //                //return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
        //            }

        //            DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
        //            string EmpCade = "";
        //            foreach (var item in ReLockEmployeeList)
        //            {
        //                if (EmpCade == "")
        //                    EmpCade = "'" + item.ToString() + "'";
        //                else
        //                    EmpCade = EmpCade + ",'" + item.ToString() + "'";
        //            }
        //            employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";

        //            DataSet dsTemp = new DataSet();
        //            DataTable dtTemp = new DataTable("TEMP");
        //            dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
        //            dsTemp.Tables.Add(dtTemp);
        //            if (dsTemp.Tables[0].Rows.Count > 0)
        //            {
        //                _OTManagementService.SaveData(lockDate, dsTemp);
        //            }
        //            DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, lockDate, "");
        //            employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";
        //            DataSet dsTempPD = new DataSet();
        //            DataTable dtTempPD = new DataTable("TEMPPD");
        //            dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
        //            dsTempPD.Tables.Add(dtTempPD);


        //            if (dsTempPD.Tables[0].Rows.Count > 0)
        //            {
        //                _OTManagementService.SaveData(lockDate, dsTempPD);
        //            }
        //        }
        //        if (IsOutMissingValidationRequired)
        //        {
        //            //OutPunchMissingData = _hrmsSettingsService.GetOutPunchMissingData(lockDate);


        //            List<Dictionary<string, object>> OutPunchMissingData = (List<Dictionary<string, object>>)_hrmsSettingsService.GetOutPunchMissingData(lockDate);
        //            if (OutPunchMissingData.Count() > 0)
        //            {
        //                for (int i = 0; i < ReLockEmployeeList.Length; i++)
        //                {
        //                    List<Dictionary<string, object>> OT = OutPunchMissingData.Where(ee => ee["EmpSystemId"].ToString() == ReLockEmployeeList[i].ToString()).ToList();
        //                    if (OT.Count() > 0)
        //                    {
        //                        throw new Exception("Please Confirmed  Out Punch Missing. Employee Code [ " + _hrmsSettingsService.GetEmpCode(ReLockEmployeeList[i].ToString()) + " ].");
        //                    }
        //                }
        //            }

        //        }
        //        //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        _hrmsSettingsService.CreateReLockDataEmployeeWise(lockDate, ReLockEmployeeList, (CustomIdentity)Thread.CurrentPrincipal.Identity);
        //        //_hrmsSettingsService.CreateLockData(lockDate);


        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //    return Json(new { Message = AplosMessage.Success });

        //}


        [HttpPost, Authorize]
        public JsonResult CreateLockDataEmpWise(string EmpSystemId, string[] EmployeeWiseLockDateList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _hrmsSettingsService.CreateLockDataEmpWise(EmpSystemId, EmployeeWiseLockDateList, identity.Name, DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
            return Json(new { Message = AplosMessage.Success });

        }
        #endregion
        #endregion
        #region Individual Attendance Lock 
        [HttpGet]
        public JsonResult LoadSeparatedEmployeeList(string FromDate, string ToDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                --,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,ld.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,SE.UserName Section,SuS.UserName SubSection,1 IsSeparatedPart,0 MLVPart, EC.UserName EmpCategoryName  
								FROM  EmployeeInformation EI 
                                ----LEFT JOIN HKP.EmployeeCategory AS EC ON EI.EmployeeCategorySystemID = EC.Id 
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId 
                                LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                LEFT JOIN [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId                                        
								WHERE EI.EmployeeStatus='separated'   AND EI.DOS BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
                                AND  EI.PlantId='" + identity.PlantId + @"'
                                ---AND EI.SystemId NOT IN (SELECT EmpSystemId FROM IndividualEmployeeAttendancelock WHERE WorkDate ='')
                                ORDER BY CONVERT(DATE,EI.DOS) ";

            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }

        [HttpGet, Authorize]
        public JsonResult LoadWorkDateSeparatedEmployee(string FromDate, string ToDate, string EmpSystemId)
        {

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEmp = null; ;





            string EMPsql = @"select EmployeeStatus from EmployeeInformation where SystemID='" + EmpSystemId + @"'";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(EMPsql, out dsEmp, false, "1");


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = string.Empty;
            if (dsEmp.Tables[0].Rows[0]["EmployeeStatus"].ToString().ToUpper() == "SEPARATED")
            {
                sql = @"Select 0 CheckBoxSelect
                                ,apd.EmpSystemId,FORMAT(apd.WorkDate,'dd-MMM-yyyy')   WorkDate,apd.DayStatus
                                ,LockedStatus=CASE WHEN IAL.WorkDate=apd.WorkDate THEN 'Lock' ELSE 'Un-lock' END
								FROM AttdnProcessData AS apd
								LEFT JOIN IndividualEmployeeAttendancelock AS IAL ON  IAL.EmpSystemID = apd.EmpSystemID AND IAL.WorkDate = apd.WorkDate           
								WHERE apd.EmpSystemId='" + EmpSystemId + @"'   AND apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
                                AND  apd.PlantId='" + identity.PlantId + @"' ---AND (apd.MaternityStatus !='MLV' or apd.MaternityStatus is NULL)";
            }
            else
            {
                sql = @"Select 0 CheckBoxSelect
                                ,apd.EmpSystemId,FORMAT(apd.WorkDate,'dd-MMM-yyyy')   WorkDate,apd.DayStatus
                                ,LockedStatus=CASE WHEN IAL.WorkDate=apd.WorkDate THEN 'Lock' ELSE 'Un-lock' END
								FROM AttdnProcessData AS apd
								LEFT JOIN IndividualEmployeeAttendancelock AS IAL ON  IAL.EmpSystemID = apd.EmpSystemID AND IAL.WorkDate = apd.WorkDate           
								WHERE apd.EmpSystemId='" + EmpSystemId + @"'   AND apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
                                AND  apd.PlantId='" + identity.PlantId + @"' AND (apd.MaternityStatus !='MLV' or apd.MaternityStatus is NULL)";
            }


            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }




        [HttpPost]
        public JsonResult CreateSeparatedEmployeeAttendanceLock(string EmployeeSystemId, string[] LockDates)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAuto = false;
            //bool IsOTConfirmationAutoException = false;
            bool IsOutMissingValidationRequired = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAfterLock = false;
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                    IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                    {
                        IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                    }
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                    {
                        IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                    }
                }



                if (IsOTConfirmationAuto)
                {
                    foreach (var LockDate in LockDates)
                    {

                        var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                        if (MissPunchEmployeeListAuto.Count() > 0)
                        {
                            //string message = string.Empty;
                            //foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                            //{
                            //    if (message == "")
                            //        message = "'" + item["EmployeeCode"].ToString() + "'";
                            //    else
                            //        message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                            //}


                            //throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");

                            string message = string.Empty;
                            foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                            {

                                if (item["EmployeeCode"].ToString() == EmployeeSystemId)
                                    throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");

                            }
                        }
                    }
                    foreach (var LockDate in LockDates)
                    {

                        DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                        string EmpCade = "";

                        employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")";

                        DataSet dsTemp = new DataSet();
                        DataTable dtTemp = new DataTable("TEMP");
                        dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                        dsTemp.Tables.Add(dtTemp);
                        if (dsTemp.Tables[0].Rows.Count > 0)
                        {
                            _OTManagementService.SaveData(LockDate, dsTemp);
                        }
                        DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                        employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")";
                        DataSet dsTempPD = new DataSet();
                        DataTable dtTempPD = new DataTable("TEMPPD");
                        dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                        dsTempPD.Tables.Add(dtTempPD);


                        if (dsTempPD.Tables[0].Rows.Count > 0)
                        {
                            _OTManagementService.SaveData(LockDate, dsTempPD);
                        }
                    }
                }
                else ///OT Confirmation Auto False
                {
                    if (IsOTConfirmationAfterLock == false)
                    {
                        if (IsOTConfirmationAutoForZeroAuto)///OT Confirmation Auto For Zero Auto
                        {


                            foreach (var LockDate in LockDates)
                            {

                                var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                                if (MissPunchEmployeeListAuto.Count() > 0)
                                {
                                    //string message = string.Empty;
                                    //foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                                    //{
                                    //    if (message == "")
                                    //        message = "'" + item["EmployeeCode"].ToString() + "'";
                                    //    else
                                    //        message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                                    //}
                                    string message = string.Empty;
                                    foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                                    {
                                        if (item["EmployeeCode"].ToString() == EmployeeSystemId)
                                            throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");

                                    }

                                }
                            }
                            foreach (var LockDate in LockDates)
                            {

                                DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                                string EmpCade = "";

                                employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")  and OTHrInMin=0";

                                DataSet dsTemp = new DataSet();
                                DataTable dtTemp = new DataTable("TEMP");
                                dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                                dsTemp.Tables.Add(dtTemp);
                                if (dsTemp.Tables[0].Rows.Count > 0)
                                {
                                    _OTManagementService.SaveData(LockDate, dsTemp);
                                }
                                DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                                employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")  and OTHrInMin=0";
                                DataSet dsTempPD = new DataSet();
                                DataTable dtTempPD = new DataTable("TEMPPD");
                                dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                                dsTempPD.Tables.Add(dtTempPD);


                                if (dsTempPD.Tables[0].Rows.Count > 0)
                                {
                                    _OTManagementService.SaveData(LockDate, dsTempPD);
                                }
                            }












                            //var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                            //if (MissPunchEmployeeListAuto.Count() > 0)
                            //{
                            //    string message = string.Empty;
                            //    foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                            //    {
                            //        if (message == "")
                            //            message = "'" + item["EmployeeCode"].ToString() + "'";
                            //        else
                            //            message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                            //    }
                            //    //for (int i = 0; i < MissPunchEmployeeListAuto.Count(); i++)
                            //    //{

                            //    //}

                            //    throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");
                            //    //IsOTConfirmationAutoException = true;
                            //    //return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
                            //}

                            //DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                            //string EmpCade = "";
                            //foreach (var item in ReLockEmployeeList)
                            //{
                            //    if (EmpCade == "")
                            //        EmpCade = "'" + item.ToString() + "'";
                            //    else
                            //        EmpCade = EmpCade + ",'" + item.ToString() + "'";
                            //}
                            //employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @") and OTHrInMin=0";

                            //DataSet dsTemp = new DataSet();
                            //DataTable dtTemp = new DataTable("TEMP");
                            //dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                            //dsTemp.Tables.Add(dtTemp);
                            //if (dsTemp.Tables[0].Rows.Count > 0)
                            //{
                            //    _OTManagementService.SaveData(sFromDate.ToString("dd-MMM-yyyy"), dsTemp);
                            //}

                            //DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                            //employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @") and OTHrInMin=0";
                            //DataSet dsTempPD = new DataSet();
                            //DataTable dtTempPD = new DataTable("TEMPPD");
                            //dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                            //dsTempPD.Tables.Add(dtTempPD);


                            //if (dsTempPD.Tables[0].Rows.Count > 0)
                            //{
                            //    _OTManagementService.SaveData(sFromDate.ToString("dd-MMM-yyyy"), dsTempPD);
                            //}
                        }
                    }
                }





                if (IsOutMissingValidationRequired)
                {

                    foreach (var LockDate in LockDates)
                    {
                        List<Dictionary<string, object>> OutPunchMissingData = (List<Dictionary<string, object>>)_hrmsSettingsService.GetOutPunchMissingData(LockDate);
                        if (OutPunchMissingData.Count() > 0)
                        {

                            List<Dictionary<string, object>> OT = OutPunchMissingData.Where(ee => ee["EmpSystemId"].ToString() == EmployeeSystemId.ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed  Out Punch Missing. Employee Code [ " + _hrmsSettingsService.GetEmpCode(EmployeeSystemId.ToString()) + " ].");
                            }

                        }
                    }



                }

                _hrmsSettingsService.CreateEmployeeIndividualAttendanceLock(EmployeeSystemId, LockDates, "SEPARATED", (CustomIdentity)Thread.CurrentPrincipal.Identity);



            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json(new { Message = AplosMessage.Success });

        }


        [HttpGet, Authorize]
        public JsonResult LoadMLVEmployeeList(string FromDate, string ToDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                --,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,ld.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section,FORMAT(lv.FromDate,'dd-MMM-yyyy') FromDate,FORMAT(lv.ToDate,'dd-MMM-yyyy') ToDate,lv.BabyNo,1 IsSeparatedPart,0 MLVPart  , EC.UserName EmpCategoryName  
								FROM  EmployeeInformation EI 
                                LEFT JOIN HKP.EmployeeCategory AS EC ON EI.EmployeeCategorySystemID = EC.Id 
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId  
                                LEFT JOIN (SELECT EmpSystemID,FromDate,ToDate,BabyNo  FROM LeaveTransaction WHERE LTSystemID=(SELECT id FROM LeaveType WHERE LeaveType='Maternity' ) AND DATEADD(DAY,-1,FromDate)  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"') lv ON lv.EmpSystemID=EI.SystemId     
								WHERE  EI.PlantId='" + identity.PlantId + @"'
                                AND EI.SystemId  IN (SELECT EmpSystemID FROM LeaveTransaction WHERE LTSystemID=(SELECT id FROM LeaveType WHERE LeaveType='Maternity' ) AND DATEADD(DAY,-1,FromDate)  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"')
                                ORDER BY CONVERT(DATE,lv.FromDate) ";

            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }






        [HttpPost, Authorize]
        public JsonResult CreateMLVEmployeeAttendanceLock(string EmployeeSystemId, string[] LockDates)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAuto = false;
            //bool IsOTConfirmationAutoException = false;
            bool IsOutMissingValidationRequired = false;
            //bool IsOutMissingValidationRequired = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAfterLock = false;
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                    IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                    {
                        IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                    }
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                    {
                        IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                    }
                }



                if (IsOTConfirmationAuto)
                {
                    foreach (var LockDate in LockDates)
                    {

                        var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                        if (MissPunchEmployeeListAuto.Count() > 0)
                        {
                            string message = string.Empty;
                            foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                            {
                                if (message == "")
                                    message = "'" + item["EmployeeCode"].ToString() + "'";
                                else
                                    message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                            }


                            throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");

                        }
                    }
                    foreach (var LockDate in LockDates)
                    {

                        DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                        string EmpCade = "";

                        employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")";

                        DataSet dsTemp = new DataSet();
                        DataTable dtTemp = new DataTable("TEMP");
                        dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                        dsTemp.Tables.Add(dtTemp);
                        if (dsTemp.Tables[0].Rows.Count > 0)
                        {
                            _OTManagementService.SaveData(LockDate, dsTemp);
                        }
                        DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                        employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")";
                        DataSet dsTempPD = new DataSet();
                        DataTable dtTempPD = new DataTable("TEMPPD");
                        dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                        dsTempPD.Tables.Add(dtTempPD);


                        if (dsTempPD.Tables[0].Rows.Count > 0)
                        {
                            _OTManagementService.SaveData(LockDate, dsTempPD);
                        }
                    }
                }
                else ///OT Confirmation Auto False
                {
                    if (IsOTConfirmationAfterLock == false)
                    {
                        if (IsOTConfirmationAutoForZeroAuto)///OT Confirmation Auto For Zero Auto
                        {


                            foreach (var LockDate in LockDates)
                            {

                                var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                                if (MissPunchEmployeeListAuto.Count() > 0)
                                {
                                    string message = string.Empty;
                                    foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                                    {
                                        if (message == "")
                                            message = "'" + item["EmployeeCode"].ToString() + "'";
                                        else
                                            message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                                    }


                                    throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");

                                }
                            }
                            foreach (var LockDate in LockDates)
                            {

                                DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                                string EmpCade = "";

                                employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")  and OTHrInMin=0";

                                DataSet dsTemp = new DataSet();
                                DataTable dtTemp = new DataTable("TEMP");
                                dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                                dsTemp.Tables.Add(dtTemp);
                                if (dsTemp.Tables[0].Rows.Count > 0)
                                {
                                    _OTManagementService.SaveData(LockDate, dsTemp);
                                }
                                DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                                employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmployeeSystemId + @")  and OTHrInMin=0";
                                DataSet dsTempPD = new DataSet();
                                DataTable dtTempPD = new DataTable("TEMPPD");
                                dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                                dsTempPD.Tables.Add(dtTempPD);


                                if (dsTempPD.Tables[0].Rows.Count > 0)
                                {
                                    _OTManagementService.SaveData(LockDate, dsTempPD);
                                }
                            }

                        }
                    }
                }








                if (IsOutMissingValidationRequired)
                {

                    foreach (var LockDate in LockDates)
                    {
                        List<Dictionary<string, object>> OutPunchMissingData = (List<Dictionary<string, object>>)_hrmsSettingsService.GetOutPunchMissingData(LockDate);
                        if (OutPunchMissingData.Count() > 0)
                        {

                            List<Dictionary<string, object>> OT = OutPunchMissingData.Where(ee => ee["EmpSystemId"].ToString() == EmployeeSystemId.ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed  Out Punch Missing. Employee Code [ " + _hrmsSettingsService.GetEmpCode(EmployeeSystemId.ToString()) + " ].");
                            }

                        }
                    }



                }

                _hrmsSettingsService.CreateEmployeeIndividualAttendanceLock(EmployeeSystemId, LockDates, "MLV", (CustomIdentity)Thread.CurrentPrincipal.Identity);



            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json(new { Message = AplosMessage.Success });

        }





        [HttpGet, Authorize]
        public JsonResult LoadWorkDateForUnLock(string FromDate, string ToDate, string EmpSystemId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect
                                ,apd.EmpSystemId,FORMAT(apd.WorkDate,'dd-MMM-yyyy')   WorkDate,apd.DayStatus
                                ,LockedStatus=CASE WHEN IAL.WorkDate=apd.WorkDate THEN 'Lock' ELSE 'Un-lock' END
								FROM AttdnProcessData AS apd
								LEFT JOIN IndividualEmployeeAttendancelock AS IAL ON  IAL.EmpSystemID = apd.EmpSystemID AND IAL.WorkDate = apd.WorkDate           
								WHERE apd.EmpSystemId='" + EmpSystemId + @"'   AND apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
                                AND  apd.PlantId='" + identity.PlantId + @"' AND (apd.MaternityStatus !='MLV' or apd.MaternityStatus is NULL) AND  IAL.WorkDate IS NOT NULL";

            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpPost]
        public JsonResult CreateIndividualUnLock(string EmployeeSystemId, string[] LockDates)
        {

            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                _hrmsSettingsService.CreateEmployeeIndividualAttendanceUnLock(EmployeeSystemId, LockDates, (CustomIdentity)Thread.CurrentPrincipal.Identity);






            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json(new { Message = AplosMessage.Deleted });

        }





        [HttpGet]
        public JsonResult LoadEmployeeIndividualAttendanceUnLock(string lockDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section
								FROM AttdnProcessData AS apd
								LEFT JOIN  EmployeeInformation EI ON apd.EmpSystemID=EI.SystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId                          
								WHERE EI.EmployeeStatus='separated'   AND apd.WorkDate='" + lockDate + @"'
                                AND  EI.PlantId='" + identity.PlantId + @"'
                                AND EI.SystemId NOT IN (SELECT EmpSystemId FROM IndividualEmployeeAttendancelock WHERE WorkDate ='" + lockDate + @"')";

            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpGet, Authorize]
        public JsonResult LoadEmployeeIndividualAttendanceLock(string lockDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section
								FROM AttdnProcessData AS apd
								LEFT JOIN  EmployeeInformation EI ON apd.EmpSystemID=EI.SystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId                          
								WHERE EI.EmployeeStatus='separated'   AND apd.WorkDate='" + lockDate + @"'
                                AND  EI.PlantId='" + identity.PlantId + @"' 
                                AND EI.SystemId  IN (SELECT EmpSystemId FROM IndividualEmployeeAttendancelock WHERE WorkDate ='" + lockDate + @"')";

            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpPost]
        public JsonResult CreateEmployeeIndividualAttendanceLock(string LockDate, string[] EmployeeSystemIds)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAuto = false;
            bool IsOTConfirmationAutoException = false;
            bool IsOutMissingValidationRequired = false;
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                    IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
                }



                if (IsOTConfirmationAuto)
                {
                    var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                    if (MissPunchEmployeeListAuto.Count() > 0)
                    {
                        string message = string.Empty;
                        foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                        {
                            if (message == "")
                                message = "'" + item["EmployeeCode"].ToString() + "'";
                            else
                                message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                        }
                        //for (int i = 0; i < MissPunchEmployeeListAuto.Count(); i++)
                        //{

                        //}

                        throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");
                        //IsOTConfirmationAutoException = true;
                        //return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
                    }

                    DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                    string EmpCade = "";
                    foreach (var item in EmployeeSystemIds)
                    {
                        if (EmpCade == "")
                            EmpCade = "'" + item.ToString() + "'";
                        else
                            EmpCade = EmpCade + ",'" + item.ToString() + "'";
                    }
                    employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";

                    DataSet dsTemp = new DataSet();
                    DataTable dtTemp = new DataTable("TEMP");
                    dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                    dsTemp.Tables.Add(dtTemp);
                    if (dsTemp.Tables[0].Rows.Count > 0)
                    {
                        _OTManagementService.SaveData(LockDate, dsTemp);
                    }
                    DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                    employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";
                    DataSet dsTempPD = new DataSet();
                    DataTable dtTempPD = new DataTable("TEMPPD");
                    dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                    dsTempPD.Tables.Add(dtTempPD);


                    if (dsTempPD.Tables[0].Rows.Count > 0)
                    {
                        _OTManagementService.SaveData(LockDate, dsTempPD);
                    }
                }
                if (IsOutMissingValidationRequired)
                {
                    //OutPunchMissingData = _hrmsSettingsService.GetOutPunchMissingData(lockDate);


                    List<Dictionary<string, object>> OutPunchMissingData = (List<Dictionary<string, object>>)_hrmsSettingsService.GetOutPunchMissingData(LockDate);
                    if (OutPunchMissingData.Count() > 0)
                    {
                        for (int i = 0; i < EmployeeSystemIds.Length; i++)
                        {
                            List<Dictionary<string, object>> OT = OutPunchMissingData.Where(ee => ee["EmpSystemId"].ToString() == EmployeeSystemIds[i].ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed  Out Punch Missing. Employee Code [ " + _hrmsSettingsService.GetEmpCode(EmployeeSystemIds[i].ToString()) + " ].");
                            }
                        }
                    }

                }
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                // _hrmsSettingsService.CreateEmployeeIndividualAttendanceLock(LockDate, EmployeeSystemIds, (CustomIdentity)Thread.CurrentPrincipal.Identity);
                //_hrmsSettingsService.CreateLockData(lockDate);


            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json(new { Message = AplosMessage.Success });

        }
        [HttpPost, Authorize]
        public JsonResult CreateEmployeeIndividualAttendanceUnLock(string LockDate, string[] EmployeeSystemIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _hrmsSettingsService.CreateEmployeeIndividualAttendanceUnLock(LockDate, EmployeeSystemIds, (CustomIdentity)Thread.CurrentPrincipal.Identity);
            return Json(new { Message = AplosMessage.Success });

        }
        #endregion


        #region MLV Attendance Lock 
        [HttpGet]
        public JsonResult LoadEmployeeMLVAttendanceUnLock(string lockDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section
								FROM AttdnProcessData AS apd
								LEFT JOIN  EmployeeInformation EI ON apd.EmpSystemID=EI.SystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId                          
								WHERE EI.EmployeeStatus='Active' AND apd.WorkDate='" + lockDate + @"' AND EI.SystemId IN (SELECT  EmpSystemID  FROM LeaveTransaction WHERE LTSystemID=(SELECT Id FROM LeaveType WHERE LeaveType='Maternity')) 
                                AND  EI.PlantId='" + identity.PlantId + @"'
                                AND EI.SystemId NOT IN (SELECT EmpSystemId FROM IndividualEmployeeAttendancelock WHERE WorkDate ='" + lockDate + @"')";

            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpGet, Authorize]
        public JsonResult LoadEmployeeMLVAttendanceLock(string lockDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section
								FROM AttdnProcessData AS apd
								LEFT JOIN  EmployeeInformation EI ON apd.EmpSystemID=EI.SystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId                          
								WHERE EI.EmployeeStatus='separated'   AND apd.WorkDate='" + lockDate + @"'
                                AND  EI.PlantId='" + identity.PlantId + @"' 
                                AND EI.SystemId  IN (SELECT EmpSystemId FROM IndividualEmployeeAttendancelock WHERE WorkDate ='" + lockDate + @"')";

            var data = _sqlRepository.GetDataCollection(sql);



            JsonResult json = Json(new { data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpPost]
        public JsonResult CreateEmployeeMLVAttendanceLock(string LockDate, string[] EmployeeSystemIds)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAuto = false;
            bool IsOTConfirmationAutoException = false;
            bool IsOutMissingValidationRequired = false;
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                    IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
                }



                if (IsOTConfirmationAuto)
                {
                    var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                    if (MissPunchEmployeeListAuto.Count() > 0)
                    {
                        string message = string.Empty;
                        foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                        {
                            if (message == "")
                                message = "'" + item["EmployeeCode"].ToString() + "'";
                            else
                                message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                        }
                        //for (int i = 0; i < MissPunchEmployeeListAuto.Count(); i++)
                        //{

                        //}

                        throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");
                        //IsOTConfirmationAutoException = true;
                        //return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
                    }

                    DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                    string EmpCade = "";
                    foreach (var item in EmployeeSystemIds)
                    {
                        if (EmpCade == "")
                            EmpCade = "'" + item.ToString() + "'";
                        else
                            EmpCade = EmpCade + ",'" + item.ToString() + "'";
                    }
                    employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";

                    DataSet dsTemp = new DataSet();
                    DataTable dtTemp = new DataTable("TEMP");
                    dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                    dsTemp.Tables.Add(dtTemp);
                    if (dsTemp.Tables[0].Rows.Count > 0)
                    {
                        _OTManagementService.SaveData(LockDate, dsTemp);
                    }
                    DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, LockDate, "");
                    employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";
                    DataSet dsTempPD = new DataSet();
                    DataTable dtTempPD = new DataTable("TEMPPD");
                    dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                    dsTempPD.Tables.Add(dtTempPD);


                    if (dsTempPD.Tables[0].Rows.Count > 0)
                    {
                        _OTManagementService.SaveData(LockDate, dsTempPD);
                    }
                }






                if (IsOutMissingValidationRequired)
                {
                    //OutPunchMissingData = _hrmsSettingsService.GetOutPunchMissingData(lockDate);


                    List<Dictionary<string, object>> OutPunchMissingData = (List<Dictionary<string, object>>)_hrmsSettingsService.GetOutPunchMissingData(LockDate);
                    if (OutPunchMissingData.Count() > 0)
                    {
                        for (int i = 0; i < EmployeeSystemIds.Length; i++)
                        {
                            List<Dictionary<string, object>> OT = OutPunchMissingData.Where(ee => ee["EmpSystemId"].ToString() == EmployeeSystemIds[i].ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed  Out Punch Missing. Employee Code [ " + _hrmsSettingsService.GetEmpCode(EmployeeSystemIds[i].ToString()) + " ].");
                            }
                        }
                    }

                }
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //_hrmsSettingsService.CreateEmployeeIndividualAttendanceLock(LockDate, EmployeeSystemIds, (CustomIdentity)Thread.CurrentPrincipal.Identity);
                //_hrmsSettingsService.CreateLockData(lockDate);


            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json(new { Message = AplosMessage.Success });

        }
        [HttpPost, Authorize]
        public JsonResult CreateEmployeeMLVAttendanceUnLock(string LockDate, string[] EmployeeSystemIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _hrmsSettingsService.CreateEmployeeIndividualAttendanceUnLock(LockDate, EmployeeSystemIds, (CustomIdentity)Thread.CurrentPrincipal.Identity);
            return Json(new { Message = AplosMessage.Success });

        }
        #endregion




        #region Date Range Wise Attendance & UnLock 
        [HttpGet]  //employee wise
        public JsonResult GetLockEmployeeList(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var LockEmployees = _hrmsSettingsService.GetLockEmployeeList(FromDate, ToDate, identity);
            var ReLockEmployees = _hrmsSettingsService.GetReLockEmployeeList(FromDate, ToDate, identity);
            JsonResult json = Json(new { LockEmployees, ReLockEmployees }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpGet, Authorize]
        public JsonResult GetReLockEmployeeList(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var LockEmployees = _hrmsSettingsService.GetLockEmployeeList(FromDate, ToDate, identity);
            var ReLockEmployees = _hrmsSettingsService.GetReLockEmployeeList(FromDate, ToDate, identity);
            JsonResult json = Json(new { LockEmployees, ReLockEmployees }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        [HttpPost, Authorize]
        public JsonResult CreateUnLockDataEmployeeWise(string FromDate, string ToDate, string[] UnLockEmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _hrmsSettingsService.CreateUnLockDataRangeWise(FromDate, ToDate, UnLockEmployeeList, (CustomIdentity)Thread.CurrentPrincipal.Identity);
            //_hrmsSettingsService.CreateLockData(lockDate);
            return Json(new { Message = AplosMessage.Success });

        }
        [HttpPost, Authorize]
        public JsonResult CreateReLockDataEmployeeWise(string FromDate, string ToDate, string[] ReLockEmployeeList)
        {

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAuto = false;
            //bool IsOTConfirmationAutoException = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAfterLock = false;
            bool IsOutMissingValidationRequired = false;


            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                    IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());


                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                    {
                        IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                    }
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                    {
                        IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                    }
                }







                DateTime sFromDate = Convert.ToDateTime(FromDate.ToString());
                DateTime sToDate = Convert.ToDateTime(ToDate.ToString());
                while (sFromDate <= sToDate)//date wise loop
                {
                    //===================

                    if (IsOTConfirmationAuto)
                    {
                        var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                        if (MissPunchEmployeeListAuto.Count() > 0)
                        {
                            string message = string.Empty;
                            foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                            {
                                if (message == "")
                                    message = "'" + item["EmployeeCode"].ToString() + "'";
                                else
                                    message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                            }
                            //for (int i = 0; i < MissPunchEmployeeListAuto.Count(); i++)
                            //{

                            //}

                            throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");
                            //IsOTConfirmationAutoException = true;
                            //return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
                        }

                        DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                        string EmpCade = "";
                        foreach (var item in ReLockEmployeeList)
                        {
                            if (EmpCade == "")
                                EmpCade = "'" + item.ToString() + "'";
                            else
                                EmpCade = EmpCade + ",'" + item.ToString() + "'";
                        }
                        employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";

                        DataSet dsTemp = new DataSet();
                        DataTable dtTemp = new DataTable("TEMP");
                        dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                        dsTemp.Tables.Add(dtTemp);
                        if (dsTemp.Tables[0].Rows.Count > 0)
                        {
                            _OTManagementService.SaveData(sFromDate.ToString("dd-MMM-yyyy"), dsTemp);
                        }
                        DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                        employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @")";
                        DataSet dsTempPD = new DataSet();
                        DataTable dtTempPD = new DataTable("TEMPPD");
                        dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                        dsTempPD.Tables.Add(dtTempPD);


                        if (dsTempPD.Tables[0].Rows.Count > 0)
                        {
                            _OTManagementService.SaveData(sFromDate.ToString("dd-MMM-yyyy"), dsTempPD);
                        }
                    }
                    else ///OT Confirmation Auto False
                    {
                        if (IsOTConfirmationAfterLock == false)
                        {
                            if (IsOTConfirmationAutoForZeroAuto)///OT Confirmation Auto For Zero Auto
                            {
                                var MissPunchEmployeeListAuto = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                                if (MissPunchEmployeeListAuto.Count() > 0)
                                {
                                    string message = string.Empty;
                                    foreach (IDictionary<string, object> item in MissPunchEmployeeListAuto)
                                    {
                                        if (message == "")
                                            message = "'" + item["EmployeeCode"].ToString() + "'";
                                        else
                                            message = message + ",'" + item["EmployeeCode"].ToString() + "'";
                                    }
                                    //for (int i = 0; i < MissPunchEmployeeListAuto.Count(); i++)
                                    //{

                                    //}

                                    throw new Exception("Can not be locked because OT Confirmation is Auto. Please Confirmed  Out Punch Missing. Employee Code [" + message + "].");
                                    //IsOTConfirmationAutoException = true;
                                    //return Json(new { MissPunchEmployeeListAuto, IsOTConfirmationAutoException, Message = AplosMessage.Success });
                                }

                                DataSet employeeOTInformationAuto = _OTManagementService.LoadEmpForOTConfirmationAuto(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                                string EmpCade = "";
                                foreach (var item in ReLockEmployeeList)
                                {
                                    if (EmpCade == "")
                                        EmpCade = "'" + item.ToString() + "'";
                                    else
                                        EmpCade = EmpCade + ",'" + item.ToString() + "'";
                                }
                                employeeOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @") and OTHrInMin=0";

                                DataSet dsTemp = new DataSet();
                                DataTable dtTemp = new DataTable("TEMP");
                                dtTemp = employeeOTInformationAuto.Tables[0].DefaultView.ToTable();
                                dsTemp.Tables.Add(dtTemp);
                                if (dsTemp.Tables[0].Rows.Count > 0)
                                {
                                    _OTManagementService.SaveData(sFromDate.ToString("dd-MMM-yyyy"), dsTemp);
                                }

                                DataSet employeePostDeviationOTInformationAuto = _OTManagementService.LoadPostDeviationEmployeeDataForGridAuto(identity.CompanyGroupId, identity.PlantId, sFromDate.ToString("dd-MMM-yyyy"), "");
                                employeePostDeviationOTInformationAuto.Tables[0].DefaultView.RowFilter = "SystemID IN (" + EmpCade + @") and OTHrInMin=0";
                                DataSet dsTempPD = new DataSet();
                                DataTable dtTempPD = new DataTable("TEMPPD");
                                dtTempPD = employeePostDeviationOTInformationAuto.Tables[0].DefaultView.ToTable();
                                dsTempPD.Tables.Add(dtTempPD);


                                if (dsTempPD.Tables[0].Rows.Count > 0)
                                {
                                    _OTManagementService.SaveData(sFromDate.ToString("dd-MMM-yyyy"), dsTempPD);
                                }
                            }
                        }
                    }





                    if (IsOutMissingValidationRequired)
                    {
                        //OutPunchMissingData = _hrmsSettingsService.GetOutPunchMissingData(lockDate);


                        List<Dictionary<string, object>> OutPunchMissingData = (List<Dictionary<string, object>>)_hrmsSettingsService.GetOutPunchMissingData(sFromDate.ToString("dd-MMM-yyyy"));
                        if (OutPunchMissingData.Count() > 0)
                        {
                            for (int i = 0; i < ReLockEmployeeList.Length; i++)
                            {
                                List<Dictionary<string, object>> OT = OutPunchMissingData.Where(ee => ee["EmpSystemId"].ToString() == ReLockEmployeeList[i].ToString()).ToList();
                                if (OT.Count() > 0)
                                {
                                    throw new Exception("Please Confirmed  Out Punch Missing. Employee Code [ " + _hrmsSettingsService.GetEmpCode(ReLockEmployeeList[i].ToString()) + " ].");
                                }
                            }
                        }

                    }

                    _hrmsSettingsService.CreateReLockDataEmployeeWise(sFromDate.ToString("dd-MMM-yyyy"), ReLockEmployeeList, (CustomIdentity)Thread.CurrentPrincipal.Identity);


                    //====================




                    //date increment
                    sFromDate = sFromDate.AddDays(1);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json(new { Message = AplosMessage.Success });

        }


        [HttpPost, Authorize]
        public JsonResult GetOutPunchMissingDataForAlert(string FromDate, string ToDate, string[] ReLockEmployeeList)
        {

            string EmpCade = "";
            foreach (var item in ReLockEmployeeList)
            {
                if (EmpCade == "")
                    EmpCade = "'" + item.ToString() + "'";
                else
                    EmpCade = EmpCade + ",'" + item.ToString() + "'";
            }
            JsonResult json = Json(GetOutPunchMissingDataForAlertData(FromDate, ToDate, EmpCade), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        public IEnumerable<object> GetOutPunchMissingDataForAlertData(string FromDate, string ToDate, string EmpSystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"  SELECT AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,FORMAT(AP.InTime,'HH:mm tt') InTime
                                ,FORMAT(AP.OutTime,'HH:mm tt') OutTime
                                ,AP.DayStatus
                                ,EI.EmployeeCode,EI.EmployeeName 
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation                              
                                ,ld.UserName LegalDesignation
                                ,se.UserName Section
                                ,Sus.UserName SubSection
                                from AttdnProcessData AP
                                INNER JOIN EmployeeInformation EI ON EI.SystemId=AP.EmpSystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId 
                                LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                LEFT JOIN [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID
                                LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                WHERE AP.OutTime IS NOT NULL AND AP.DayStatus='A' 
								AND AP.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
								AND AP.EmpSystemID IN (" + EmpSystemId + @")
                                AND AP.PlantID='" + identity.PlantId + "'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion



        [HttpPost, Authorize]
        public JsonResult GetOutPunchMissingDataForAlertEmpWise(string EmployeeSystemId, string[] LockDates)
        {

            string Dates = "";
            foreach (var item in LockDates)
            {
                if (Dates == "")
                    Dates = "'" + item.ToString() + "'";
                else
                    Dates = Dates + ",'" + item.ToString() + "'";
            }
            JsonResult json = Json(GetOutPunchMissingDataForAlertDataEmpWise(EmployeeSystemId, Dates), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        public IEnumerable<object> GetOutPunchMissingDataForAlertDataEmpWise(string EmployeeSystemId, string Dates)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"  SELECT AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,FORMAT(AP.InTime,'HH:mm tt') InTime
                                ,FORMAT(AP.OutTime,'HH:mm tt') OutTime
                                ,AP.DayStatus
                                ,EI.EmployeeCode,EI.EmployeeName 
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation                              
                                ,ld.UserName LegalDesignation
                                ,se.UserName Section
                                ,Sus.UserName SubSection
                                from AttdnProcessData AP
                                INNER JOIN EmployeeInformation EI ON EI.SystemId=AP.EmpSystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId 
                                LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                LEFT JOIN [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID
                                LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                WHERE AP.OutTime IS NOT NULL AND AP.DayStatus='A' 
								AND AP.WorkDate IN (" + Dates + @") 
								AND AP.EmpSystemID ='" + EmployeeSystemId + @"'
                                AND AP.PlantID='" + identity.PlantId + "'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        #endregion -- Operations
    }
}