using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
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
    public class ShiftCreationController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;

        public ShiftCreationController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult ShiftCreation()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        #region All Grid 

        #region Master Grid Load------start

        [HttpGet]
        public ActionResult getShiftlist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  SELECT SystemID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo
                            ,FORMAT(InTime,'hh:mm tt') AS InTime,INAfterOUTAsOTStart
                            ,FORMAT(OutTime,'hh:mm tt') AS OutTime
                            ,FORMAT( BreakStratTime, 'hh:mm tt') AS BreakStratTime
                            ,FORMAT(BreakEndTime, 'hh:mm tt') AS BreakEndTime
                            ,InTimeStartMargin, LateMargin, AbsentEndMargin, LateInToleranceMargin,
                            OutTimeEndMargin, OTStartTime, LateMarginSeconds,
                            BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
               ,IsActives=case when IsActive=1 then 'True' else 'False' end
               ,DefaultShifts=case when DefaultShift=1 then 'True' else 'False' End
               ,IsGapIncludes=case when IsGapInclude=1 then 'True' else 'False' End 
                    ,EarlyIn,LateIn,LateInMargin,EarlyOut,EarlyOutMargin,EarlyInMargin,LateOutMargin,LateOut,LateOutRoundMargin,LateInRoundMargin
                    ,EarlyOutRoundMargin,EarlyInRoundMargin,LateOutRoundMarginType,LateInRoundMarginType,EarlyOutRoundMarginType,EarlyInRoundMarginType
                    ,IncludeBreakTimeInOT,HalfDayAbsentMaxLimit,LateInMargin,EarlyOutMaxLimit,IsLunchOutApplicable,IsEarlyOutApplicable,EarlyOutToleranceMargin
                        ,LateInMaxLimit ,IsLateInApplicable ,RawINDefinitionFrom ,RawINDefinitionTo ,RawOUTDefinitionFrom ,RawOUTDefinitionTo ,ShiftLateOutMargin,ShiftLateInMargin,ShiftEarlyOutMargin,ShiftEarlyInMargin,
        ShiftDuration,FullDayDuration,HalfDayDuration,ShortDuration,MaxOutDuration,HoursWithoutOT, DateAdded,AddedBy
               FROM ShiftDefination WHERE GroupID = '" + identity.CompanyGroupId + "' AND PlantID = '" + identity.PlantId + "' Order By ShiftDefinationName";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        [HttpPost]
        public ActionResult Save(ShiftCreationMaster ShiftCreationData)
        {
            if (ShiftCreationData.LateMarginSeconds > 60)
            {
                Exception ex = new Exception("Seconds Can't Be Bigger Then 60");
                throw (ex);
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsShiftCreation obj = new clsShiftCreation(_sqlRepository);
            ShiftCreationData.PlantID = identity.PlantId;
            ShiftCreationData.GroupID = identity.CompanyGroupId;
            ShiftCreationData.DateUpdated = DateTime.Now;
            ShiftCreationData.UpdatedBy = identity.Name;
            ShiftCreationData.UserName = ShiftCreationData.ShiftDefinationName;

            obj.SaveShiftCreationMaster(ShiftCreationData);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(string SystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM ShiftDefination WHERE SystemID='" + SystemID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT isnull(Max(SequenceNo),0)+1 SequenceNo FROM ShiftDefination ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        #endregion -- Operations  
    }
}