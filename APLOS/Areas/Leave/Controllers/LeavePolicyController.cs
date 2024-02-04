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

namespace Aplos.Areas.Leave.Controllers
{
    public class LeavePolicyController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;

        public LeavePolicyController(
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
        public ActionResult LeavePolicy()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        #region All Grid 

        #region Master Grid Load------start

        [HttpPost]
        public ActionResult getlist(string PlantID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PolicyCode,PolicyName,DefaultPolicy,SystemID, PlantID
                            ,Policy=case
                            when DefaultPolicy='1' then 'YES' ELSE 'NO' END
                            ,Format(DateAdded,'dd-MMM-yyyy')DateAdded,p.CompanyId
							FROM LeavePolicyMaster 
							left join ORG.Plant p on p.Id  = LeavePolicyMaster.PlantID
                            where PlantID='" + PlantID + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Details Grid-------start

        [HttpGet, Authorize]
        public ActionResult getdetailslist(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT LPD.SystemID, LPD.LPMSystemID, LT.Id LTSystemID, LT.UserName, LT.[Description], LPD.LeaveDays, LPD.CarryForwardDay,LPD.MaxEncashment, LPD.MaxEncashmentLapse,
                   ISNULL(IsPrecedingWeekoff, 0) IsPrecedingWeekoff, ISNULL(IsPrecedingHoliday, 0) IsPrecedingHoliday, 
                   ISNULL(IsSucceedignWeekoff, 0) IsSucceedignWeekoff, ISNULL(IsSucceedignHoliday, 0) IsSucceedignHoliday,
                   ISNULL(InBetweenWeekoff, 0) InBetweenWeekoff, ISNULL(InBetweenHoliday, 0) InBetweenHoliday,
                   ISNULL(IsExcessAllow, 0) IsExcessAllow ,LPD.CarryForwardRoundupOption
                        ,LPD.LvEncashmentFormulaDesID
                        ,LPD.FormulaDescription
                        ,IsAllowed=case when LPD.IsAllowed=1 then 'true' else 'false' end 
                        ,LvCalculationOnDOJOrDoc = CASE WHEN LPD.LvCalculationOnDOJ=1 THEN 'CalculateDoj' when LPD.LvCalculationOnDOC=1 then 'CalculateDoc' end 
                        ,LPD.EncashmentBasis
                        ,LvAvailedOnDOJorDoc = CASE WHEN LPD.LvAvailedOnDOJ=1 then 'CalAvailDoj' when LPD.LvAvailedOnDOC=1 then 'CalAvailDoc' end 
                        ,IsCFRestEncash=case when LPD.IsCFRestEncash=1 then 'true' else 'false' end 
                        ,IsCFCRestEncash=case when LPD.IsCFCRestEncash=1 then 'true' else 'false' end 
                        ,IsCFFixed=case when LPD.IsCFFixed=1 then 'true' else 'false' end 
                        ,IsProrataMonthly=case when LPD.IsProrataMonthly=1 then 'true' else 'false' end
                        ,LPD.AllowedAfterDays,LPD.IsAllowedonspecialappeal
                        ,LPD.IsProratacurrentyear
                        ,LPD.IsPostApplicationAllowed,LPD.MaxAllocationLimit,LPD.IsProofDocRequired
                        ,LPD.IsAvailExceptionAllowedOnSpecialAppeal,LPD.IsExceptionAllowed,LPD.IsSubjectToApproval,LPD.ProofDocReqAfterDays
                        ,LPD.LvCanAvailAfter,LPD.CanAvailUOM,LPD.EncashEarnLeaveQty,LPD.EncashWorkingDaysQty
                        ,LPD.IsCarryForward,LPD.IsMaxEncashment,lpd.EncashmentSpecificDay,lpd.EncashmentSpecificMonth,lpd.LeaveCalculationRoundOption
                        ,LPD.LvAvailedOnFixedOrPercentage,LPD.LvCanAvailQuantity, LPD.IsAsperEntryOnW,LPD.IsNoLeaveOnW,LPD.IsAsperEntryOnH,LPD.IsNoLeaveOnH
                        ,LPD.IsBackDatePosting,ISNULL(LPD.BackDatePostingAllowedDays, 0) BackDatePostingAllowedDays,LPD.EmpCatId,LPD.MinAllocationLimit
                     FROM dbo.LeavePolicyDetail LPD
                        LEFT JOIN  dbo.LeaveType LT ON LPD.LTSystemID = LT.ID                       
						where
                         LPD.LPMSystemID='" + MasterId + @"'					
                 and  LT.CompanyGroupId = '" + identity.CompanyGroupId + @"' 				
                    ORDER BY LT.UserName
                                    ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Day Type Grid --------start

        [HttpGet, Authorize]
        public ActionResult getDayTypeDatalist(string SystemID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	select  Active=case when t.Id is null then  CONVERT(bit,0) else  CONVERT(bit,1) end, d.DayType,Category ,t.Id
                                    from DayType d
                                left join (select * from [LeavePolicyWorkingDays] where LPDetailID='" + SystemID + @"')t on t.DayType=d.DayType
                                WHERE  Category in ('Present','Late')  ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        [HttpGet, Authorize]
        public ActionResult GetEmployeeCategory()
        {
            string Sql = @"select Id,UserName from [HKP].[EmployeeCategory]";
            return Json(_sqlRepository.GetDataCollection(Sql),JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(LeavePolicyMaster LeavePolicy)
        {
            string MasterId = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeavePolicy obj = new clsLeavePolicy();
            LeavePolicy.AddedBy = identity.Name;
            LeavePolicy.DateAdded = DateTime.Now;
            LeavePolicy.GroupID = identity.CompanyGroupId;
            LeavePolicy.DateUpdated = DateTime.Now;
            LeavePolicy.UpdatedBy = identity.Name;
            MasterId = obj.SaveMasterAndDetailForLeavePolicy(LeavePolicy);
            return Json(new { MasterId, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveDetails(LeavePolicyDetails LeavePolicyDetails, string MasterId, string Id, List<LeavePolicyDayType> leavePolicyDayType)
        {

            DataSet dsYrCalFromDate = null;
            DataSet dsEarnType = null;
            GetCalendarYearStartDate(DateTime.Now.Year.ToString(), out dsYrCalFromDate);
            GetLeaveType(LeavePolicyDetails.LTSystemID, out dsEarnType);

            if (dsEarnType.Tables[0].Rows[0]["LeaveType"].ToString() == "Earn" && LeavePolicyDetails.CanAvailUOM == null)
            {
                throw new Exception("select Can Avail After dropdown Value");
            }
            if (string.IsNullOrEmpty(LeavePolicyDetails.EncasementEndDate.ToString())|| LeavePolicyDetails.EncasementEndDate.ToString()=="0")
            {
                LeavePolicyDetails.EncasementEndDate = false;
            }
            
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeavePolicy obj = new clsLeavePolicy();
            LeavePolicyDetails.AddedBy = identity.Name;
            LeavePolicyDetails.DateAdded = DateTime.Now;
            //LeavePolicyDetails.PlantID = identity.PlantId;
            LeavePolicyDetails.GroupID = identity.CompanyGroupId;
            LeavePolicyDetails.DateUpdated = DateTime.Now;
            LeavePolicyDetails.UpdatedBy = identity.Name;
            LeavePolicyDetails.LPMSystemID = MasterId;
            LeavePolicyDetails.StartDate = Convert.ToDateTime(dsYrCalFromDate.Tables[0].Rows[0]["FromDate"].ToString());
            LeavePolicyDetails.EndDate = null;
            obj.SaveDetailForLeavePolicy(LeavePolicyDetails, MasterId, Id, leavePolicyDayType);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void GetCalendarYearStartDate(string strSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT ISNULL(MIN(FromDate), '01-01-2000') FromDate 
                                    FROM dbo.YearlyCalendar
                                WHERE ID = '" + strSystemID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        public void GetLeaveType(string LeaveTypeId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveType
                                WHERE ID = '" + LeaveTypeId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        [HttpPost]
        public JsonResult Edit(MaternityLeavePolicy maternityLeavePolicy)
        {
            _LeavePolicyMaster.Update(maternityLeavePolicy);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult Delete(string SystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM LeavePolicyMaster WHERE SystemID='" + SystemID + @"'";
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
        public ActionResult DeleteDetails(string SystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sqlDay = @"Delete FROM LeavePolicyDetail WHERE SystemID='" + SystemID + @"'";
                string sql = @"Delete FROM LeavePolicyWorkingDays WHERE LPDetailID='" + SystemID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
                objCon.OpenDataSetThroughAdapter(sqlDay, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct UserName,Id,LeaveType
									from LeaveType lt
									LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
									  LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + identity.PlantId + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
									 where CompanyGroupId='" + identity.CompanyGroupId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID,SalaryHead from SalaryHead WHERE GroupID='" + identity.CompanyGroupId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #endregion -- Operations  
    }
}