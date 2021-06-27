#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class OTManagementController : BaseController
    {
        #region Constructor
        private readonly IOTManagementService _OTManagementService;
        private readonly ISqlRepository _sqlRepository;
        public OTManagementController(
              IOTManagementService oTManagementService, ISqlRepository sqlRepository
            )
        {
            _OTManagementService = oTManagementService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        [Authorize]
        public ActionResult OTConfirmation()
        {
            return View();
        }

        [Authorize]
        public ActionResult EmployeeDevice()
        {
            return View();
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeForOTConfirmation(string ProcDate, string OTvalCons)
        {
            DataSet dsLocalHRMSSetting = null;
            DataSet dsOTSlabDefineGeneral = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();

            decimal NWDayType = 0;
            decimal HDayType = 0;
            decimal WDayType = 0;

            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAfterLock = false;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
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
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                {
                    IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                }
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                {
                    IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                }

            }
            GetOTSlabDefineGeneral(identity.CompanyGroupId, identity.PlantId, ProcDate, out dsOTSlabDefineGeneral);
            if (dsOTSlabDefineGeneral.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsOTSlabDefineGeneral.Tables[0].Rows.Count; i++)
                {
                    if (dsOTSlabDefineGeneral.Tables[0].Rows[i]["DayType"].ToString().Trim() == "NW")
                    {
                        if (!string.IsNullOrEmpty(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString()))
                        {
                            NWDayType = Convert.ToDecimal(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString().Trim());
                        }
                        
                    }
                    if (dsOTSlabDefineGeneral.Tables[0].Rows[i]["DayType"].ToString().Trim() == "H")
                    {
                        if (!string.IsNullOrEmpty(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString()))
                        {
                            HDayType = Convert.ToDecimal(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString().Trim());
                        }
                    }
                        
                    if (dsOTSlabDefineGeneral.Tables[0].Rows[i]["DayType"].ToString().Trim() == "W")
                    {
                        if (!string.IsNullOrEmpty(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString()))
                        {
                            WDayType = Convert.ToDecimal(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString().Trim());
                        }
                        
                    }
                }
            }




            JsonResult json = Json(new
            {
                EmpMaternityWithOT = _OTManagementService.LoadEmpMaternityWithOT(identity.CompanyGroupId, identity.PlantId, ProcDate, OTvalCons),
                data = _OTManagementService.LoadEmpForOTConfirmation(identity.CompanyGroupId, identity.PlantId, ProcDate, OTvalCons),
                ShowOTValue = _OTManagementService.ShowOTValueFromHRMSSetting(identity.CompanyGroupId, identity.PlantId),
                MinimumOTMinute,
                OTConsiderOn,
                OTFractionCalculate,
                IsPunchBasedOT,
                IsPreallocationBasedOT,
                NWDayType,
                HDayType,
                WDayType,
                IsOTConfirmationAutoForZeroAuto,
                IsOTConfirmationAfterLock
            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public ActionResult GetConfirmedEmployeeDataForGrid(string ProcDate, string OTvalCons)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();


            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
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

            }
            JsonResult json = Json(new
            {
                data = _OTManagementService.LoadConfirmedEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, ProcDate, OTvalCons),
                ShowOTValue = _OTManagementService.ShowOTValueFromHRMSSetting(identity.CompanyGroupId, identity.PlantId),
                MinimumOTMinute,
                OTConsiderOn,
                OTFractionCalculate,
                IsPunchBasedOT,
                IsPreallocationBasedOT
            }, JsonRequestBehavior.AllowGet);


            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetPostDeviationEmployeeDataForGrid(string ProcDate, string OTvalCons)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();


            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
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

            }
            JsonResult json = Json(new
            {
                data = _OTManagementService.LoadPostDeviationEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, ProcDate, OTvalCons),
                ShowOTValue = _OTManagementService.ShowOTValueFromHRMSSetting(identity.CompanyGroupId, identity.PlantId),
                MinimumOTMinute,
                OTConsiderOn,
                OTFractionCalculate,
                IsPunchBasedOT,
                IsPreallocationBasedOT

            }, JsonRequestBehavior.AllowGet);


            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public ActionResult GetMissPunchEmployeeDataForGrid(string ProcDate, string OTvalCons)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();


            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
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

            }
            JsonResult json = Json(new
            {
                data = _OTManagementService.LoadMissPunchEmployeeDataForGrid(identity.CompanyGroupId, identity.PlantId, ProcDate, OTvalCons),
                ShowOTValue = _OTManagementService.ShowOTValueFromHRMSSetting(identity.CompanyGroupId, identity.PlantId),
                MinimumOTMinute,
                OTConsiderOn,
                OTFractionCalculate,
                IsPunchBasedOT,
                IsPreallocationBasedOT

            }, JsonRequestBehavior.AllowGet);


            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpPost, Authorize]
        public ActionResult SaveOTConfirmation(string ProcDate, IEnumerable<OTOTConfirmation> employeeOTInformation)
        {

            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //OTOTConfirmation view = serializer.Deserialize<OTOTConfirmation>(employeeOTInformation);

            DataSet dsEmployeeOTInformation = Library.Service.Helpers.DataTableExtensions.ToDataSet<OTOTConfirmation>(employeeOTInformation);
            //DataSet dsEmployeeOTInformation = null;
            _OTManagementService.SaveData(ProcDate, dsEmployeeOTInformation);
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveEmpWiseOTConfirmation(string ProcDate, OTOTConfirmation employeeOTInformation)
        {
            try
            {
                List<OTOTConfirmation> ot = new List<OTOTConfirmation>();
                ot.Add(employeeOTInformation);
                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                //OTOTConfirmation view = serializer.Deserialize<OTOTConfirmation>(employeeOTInformation);

                DataSet dsEmployeeOTInformation = Library.Service.Helpers.DataTableExtensions.ToDataSet<OTOTConfirmation>(ot);

                _OTManagementService.SaveData(ProcDate, dsEmployeeOTInformation);
                return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        [HttpPost]
        public ActionResult GetEmployeeWiseDataForOTConfirmation(string EmpId, string FDate, string TDate, string OTvalCons)
        {
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();


            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;
            bool IsOTConfirmationAfterLock = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
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
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                {
                    IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                }
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                {
                    IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                }

            }
            JsonResult json = Json(new
            {
                data = _OTManagementService.LoadEmpWiseDataForOTConfirmation(identity.CompanyGroupId, identity.PlantId, EmpId, FDate, TDate, OTvalCons),
                ShowOTValue = _OTManagementService.ShowOTValueFromHRMSSetting(identity.CompanyGroupId, identity.PlantId),
                MinimumOTMinute,
                OTConsiderOn,
                OTFractionCalculate,
                IsPunchBasedOT,
                IsPreallocationBasedOT,
                IsOTConfirmationAfterLock,
                IsOTConfirmationAutoForZeroAuto

            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public ActionResult GetMaternityDetailsForOTConfirmation(string EmpId, string WDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='' AND EmpSystemId=''
            string sql = @"SELECT apd.EmpSystemID,apd.MaternityStatus, apd.OTHr, apd.DayStatus
                                       ,ei.EmployeeCode,FORMAT(ei.DOJ,'dd-MMM-yyyy')  DOJ , ei.DOB, ei.EmployeeName, ei.GenderID,ei.EmpPicPath
                                       , FORMAT(lt.FromDate,'dd-MMM-yyyy') FromDate, FORMAT(lt.ToDate,'dd-MMM-yyyy') ToDate, FORMAT(lt.ExpectedDelivaryDate,'dd-MMM-yyyy') ExpectedDelivaryDate
                                       ,mlp.ChildNo,mlp.MaternityStartDay,mlp.MaternityEndDay,
                                       mlp.MaternityLeaveStartDay, mlp.MaternityLeaveEndDay,CASE WHEN mlp.IsNoBenefit=0 THEN 'YES' ELSE 'NO' END as IsNoBenefit
                                FROM AttdnProcessData AS apd
                                LEFT JOIN EmployeeInformation AS ei ON ei.SystemId = apd.EmpSystemID
                                LEFT JOIN LeaveTransaction AS lt ON lt.EmpSystemID = ei.SystemId ---AND '13-Oct-2019' BETWEEN lt.FromDate AND lt.FromDate
                                LEFT JOIN [MST].[MaternityLeavePolicy] as mlp ON mlp.Id = lt.MaternityLeavePolicyId 
                                WHERE apd.EmpSystemID='" + EmpId + @"' AND apd.WorkDate=" + WDate + @"  AND apd.PlantID='" + identity.PlantId + @"'
                                AND( DATEADD(DAY
			                                ,CASE WHEN apd.MaternityStatus='PRE' THEN mlp.MaternityStartDay WHEN apd.MaternityStatus='POST' THEN -mlp.MaternityEndDay ELSE 0 END
			                                ,apd.WorkDate ) BETWEEN lt.FromDate AND lt.toDate )
                                AND lt.LTSystemID IN (SELECT id FROM LeaveType WHERE LeaveType='Maternity')";
            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }



        public void GetOTSlabDefineGeneral(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.OTSlabDefineGeneral
                           WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate AND GroupID = '" + sGroupID + @"' 
                                 AND PlantID = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function 
        //[HttpGet, Authorize]
        //public JsonResult GetEmployeeRelatedDevices(string systemId)
        //{
        //    return Json(_OTManagementService.GetEmployeeRelatedDevices(systemId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public JsonResult GetEmployeeDevicesList(string deviceId)
        //{
        //    return Json(_OTManagementService.GetEmployeeDevicesList(deviceId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public JsonResult Create(IEnumerable<AccessControllerEmployeeTag> AccessControllerEmployeeTags, string empId, bool registerProximate, bool registerFP)
        //{
        //    _OTManagementService.InsertOrUpdateGraph(AccessControllerEmployeeTags, empId, registerProximate, registerFP);
        //    return Json(new { AccessControllerEmployeeTag = AccessControllerEmployeeTags, Message = AplosMessage.Insert });
        //}
        //[HttpPost]
        //public JsonResult CreateEmloyeeDevice(IEnumerable<AccessControllerEmployeeTag> AccessControllerEmployeeTags, bool registerProximate, bool registerFP,string deviceId)
        //{
        //    _OTManagementService.InsertOrUpdateEmployeeDevice(AccessControllerEmployeeTags, registerProximate, registerFP, deviceId);
        //    return Json(new { AccessControllerEmployeeTag = AccessControllerEmployeeTags, Message = AplosMessage.Insert });
        //}

        //public ActionResult Delete(string id)
        //{
        //    _OTManagementService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}
    }
}