using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class EmployeeWiseFixedOTSettingController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private DataSet dsRef;

        public EmployeeWiseFixedOTSettingController(
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        
        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Save(EmpWiseFixedOT EmpWiseFOTSetting)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOffDDutyHours obj = new clsOffDDutyHours();
                EmpWiseFOTSetting.AddedBy = identity.Name;
                EmpWiseFOTSetting.AddedDate = DateTime.Now;
                EmpWiseFOTSetting.AddedFromIP = identity.IPAddress;
                EmpWiseFOTSetting.PlantId = identity.PlantId;
                EmpWiseFOTSetting.CompanyId = identity.CompanyId;              
                obj.SaveEmpWiseFixedOT(EmpWiseFOTSetting);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
               
        [HttpGet]
        public ActionResult GetEmpWiseFOT(string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	 select MinimumOT,Id
                                ,ExcessAllowed = case when IsExcessAllowed=1 then 'Yes' else 'No' End ,IsExcessAllowed
                                from EmployeeWiseFixedOTSetting 
                                where PlantId='" + identity.PlantId+"' and CompanyId='"+identity.CompanyId+"' and EmpSystemId='"+empId+"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM EmployeeWiseFixedOTSetting WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations  
    }
}