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
//using clsAttendance;
using Library.Data.Sql;
using OTSBD.clsLeave;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class LeaveYearEndProcessController : BaseController
    {
        #region Constructor
        private readonly clsLeaveYearEndProcess _LeaveYearEndProcess;
        private readonly ISqlRepository _sqlRepository;


        public LeaveYearEndProcessController(
              clsLeaveYearEndProcess oLeaveYearEndProcess, ISqlRepository sqlRepository
            )
        {
            _LeaveYearEndProcess = oLeaveYearEndProcess;
            _sqlRepository = sqlRepository;
        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult LoadYearlyCalendar()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"select * from YearlyCalendar where  PlantId='" + identity.PlantId + @"'";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LeaveYearEndProcess(string YearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            DataSet dsLocal = null;
            DataSet dsCalandarYearLocal = null;
            DataSet dsOldSummary = null;

            DataSet dsNewSummary = null;
            DataSet dsLeaveTranInfo = null;
            DataSet dsCalandarYearLocalSummay = null;

            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            //clsLeaveServiceAplos objLeaveServiceAplos;
            //objLeaveServiceAplos = new clsLeaveServiceAplos();
            try
            {

                objLeaveYearEndProcessData.validateIndividualYearEnd(identity.PlantId, YearId);
                #region Validation


                if (string.IsNullOrEmpty(YearId.ToString().Trim()) == true)
                {
                    Exception ex = new Exception("Select Year First...");
                    throw (ex);
                }

                objLeaveYearEndProcessData.GetNextYearID(identity.PlantId, YearId.ToString().Trim(), out dsLocal);
             
                if (dsLocal.Tables[0].Rows.Count > 0)
                {

                    objLeaveYearEndProcessData.GetYearlyCalendarsFromDateAndToDateForLeaveYearEndProcess(identity.CompanyGroupId, identity.PlantId, YearId.ToString().Trim(), out dsCalandarYearLocal);
                    DataView dv = new DataView(dsCalandarYearLocal.Tables[0]);
                    dv.RowFilter = "IsYearEndClosed=1 ";
                    if (dv.Count > 0)
                    {
                        Exception ex = new Exception("Year End Closed for this Calendar Year.");
                        throw (ex);
                    }



                    objLeaveYearEndProcessData.GetLeaveTranInfo(identity.CompanyGroupId, identity.PlantId, dsCalandarYearLocal.Tables[0].Rows[0]["FromDate"].ToString(), dsCalandarYearLocal.Tables[0].Rows[0]["ToDate"].ToString(), out dsLeaveTranInfo);
                    objLeaveYearEndProcessData.LeaveYearEndProcess(Convert.ToDateTime(dsCalandarYearLocal.Tables[0].Rows[0]["ToDate"].ToString()), identity.CompanyGroupId, identity.PlantId, YearId.ToString().Trim(), dsLocal.Tables[0].Rows[0]["Id"].ToString(), dsLeaveTranInfo, out dsNewSummary, out dsOldSummary);
                    objLeaveYearEndProcessData.YearlyCalendarsYearEndClosedProcess(identity.CompanyGroupId, identity.PlantId, YearId.ToString().Trim(), out dsCalandarYearLocalSummay);




                    foreach (DataRow item in dsOldSummary.Tables[0].Rows)
                        item["IsYearlyProcessed"] = true;

                    objStatic.SaveDataSets(dsNewSummary, dsOldSummary, dsCalandarYearLocalSummay);


                }
                else
                {

                    //Exception ex = new Exception("Calendar Year " + (Convert.ToInt32(ddlYear.Items[ddlYear.SelectedIndex].Text) + 1).ToString()+" Not found ");
                    Exception ex = new Exception("Calendar Year Not found after ");
                    throw (ex);
                }


                #endregion Validation
                //-----
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }


            //var data = _sqlRepository.GetDataCollection(sql);
            //JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            //json.MaxJsonLength = int.MaxValue;
            //return json;



            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LeaveYearEndProcessIndividual(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            DataSet dsOldSummary = null;

            DataSet dsNewSummary = null;
            DataSet dsLeaveTranInfo = null;

            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            //clsLeaveServiceAplos objLeaveServiceAplos;
            //objLeaveServiceAplos = new clsLeaveServiceAplos();
            try
            {


                #region Validation


                if (string.IsNullOrEmpty(ToDate.ToString().Trim()) == true)
                {
                    Exception ex = new Exception("Select date First...");
                    throw (ex);
                }






                objLeaveYearEndProcessData.GetLeaveTranInfoIndividual(identity.PlantId, ToDate, out dsLeaveTranInfo);
                objLeaveYearEndProcessData.LeaveYearEndProcessIndividual(identity.PlantId, ToDate.ToString().Trim(), dsLeaveTranInfo, out dsNewSummary, out dsOldSummary);




                foreach (DataRow item in dsOldSummary.Tables[0].Rows)
                    item["IsYearlyProcessed"] = true;

                objStatic.SaveDataSets(dsNewSummary, dsOldSummary);




                #endregion Validation
                //-----
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }


            //var data = _sqlRepository.GetDataCollection(sql);
            //JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            //json.MaxJsonLength = int.MaxValue;
            //return json;



            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult LeaveYearEndProcessNew(string YearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            DataSet dsLocal = null;
            DataSet dsCalandarYearLocal = null;
            DataSet dsOldSummary = null;

            DataSet dsNewSummary = null;
            DataSet dsLeaveTranInfo = null;
            DataSet dsCalandarYearLocalSummay = null;

            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            //clsLeaveServiceAplos objLeaveServiceAplos;
            //objLeaveServiceAplos = new clsLeaveServiceAplos();
            try
            {
                objLeaveYearEndProcessData.validateIndividualYearEnd(identity.PlantId, YearId);

                #region Validation


                if (string.IsNullOrEmpty(YearId.ToString().Trim()) == true)
                {
                    Exception ex = new Exception("Select Year First...");
                    throw (ex);
                }

                objLeaveYearEndProcessData.GetNextYearID(identity.PlantId, YearId.ToString().Trim(), out dsLocal);

                if (dsLocal.Tables[0].Rows.Count > 0)
                {

                    objLeaveYearEndProcessData.GetYearlyCalendarsFromDateAndToDateForLeaveYearEndProcess(identity.CompanyGroupId, identity.PlantId, YearId.ToString().Trim(), out dsCalandarYearLocal);
                    DataView dv = new DataView(dsCalandarYearLocal.Tables[0]);
                    dv.RowFilter = "IsYearEndClosed=1 ";
                    if (dv.Count > 0)
                    {
                        Exception ex = new Exception("Year End Closed for this Calendar Year.");
                        throw (ex);
                    }



                    objLeaveYearEndProcessData.GetLeaveTranInfoNew(identity.CompanyGroupId, identity.PlantId, dsCalandarYearLocal.Tables[0].Rows[0]["FromDate"].ToString(), dsCalandarYearLocal.Tables[0].Rows[0]["ToDate"].ToString(), out dsLeaveTranInfo);
                    objLeaveYearEndProcessData.LeaveYearEndProcess(Convert.ToDateTime(dsCalandarYearLocal.Tables[0].Rows[0]["ToDate"].ToString()), identity.CompanyGroupId, identity.PlantId, YearId.ToString().Trim(), dsLocal.Tables[0].Rows[0]["Id"].ToString(), dsLeaveTranInfo, out dsNewSummary, out dsOldSummary);
                    objLeaveYearEndProcessData.YearlyCalendarsYearEndClosedProcess(identity.CompanyGroupId, identity.PlantId, YearId.ToString().Trim(), out dsCalandarYearLocalSummay);



                    foreach (DataRow item in dsOldSummary.Tables[0].Rows)
                        item["IsYearlyProcessed"] = true;
                    objStatic.SaveDataSets(dsNewSummary, dsOldSummary, dsCalandarYearLocalSummay);


                }
                else
                {

                    //Exception ex = new Exception("Calendar Year " + (Convert.ToInt32(ddlYear.Items[ddlYear.SelectedIndex].Text) + 1).ToString()+" Not found ");
                    Exception ex = new Exception("Calendar Year Not found after ");
                    throw (ex);
                }


                #endregion Validation
                //-----
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }


            //var data = _sqlRepository.GetDataCollection(sql);
            //JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            //json.MaxJsonLength = int.MaxValue;
            //return json;



            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LeaveYearEndProcessIndividualNew(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            DataSet dsOldSummary = null;

            DataSet dsNewSummary = null;
            DataSet dsLeaveTranInfo = null;

            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            //clsLeaveServiceAplos objLeaveServiceAplos;
            //objLeaveServiceAplos = new clsLeaveServiceAplos();
            try
            {


                #region Validation


                if (string.IsNullOrEmpty(ToDate.ToString().Trim()) == true)
                {
                    Exception ex = new Exception("Select date First...");
                    throw (ex);
                }






                objLeaveYearEndProcessData.GetLeaveTranInfoIndividualNew(identity.PlantId, ToDate, out dsLeaveTranInfo);
                objLeaveYearEndProcessData.LeaveYearEndProcessIndividual(identity.PlantId, ToDate.ToString().Trim(), dsLeaveTranInfo, out dsNewSummary, out dsOldSummary);





                foreach (DataRow item in dsOldSummary.Tables[0].Rows)
                    item["IsYearlyProcessed"] = true;

                objStatic.SaveDataSets(dsNewSummary, dsOldSummary);





                #endregion Validation
                //-----
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }


            //var data = _sqlRepository.GetDataCollection(sql);
            //JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            //json.MaxJsonLength = int.MaxValue;
            //return json;



            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetLeaveYearEndProcessSummaryData(string sYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            #region obsolete
            sql = @"select EmployeeId, EmployeeCode,EmployeeName,EmployeeCodeNumeric,EmployeeCodePreFix
                                ,LeaveName
                                ,OpeningBalance
                                ,EarnedDaysOB
                                ,CalculatedEarningDays
                                
                                ,Allocation DaysCanBeSanctioned
                                ,AvailedOB
                                ,Availd
                                ,TotalAvailed
                                ,Balance=OpeningBalance +Allocation - TotalAvailed - EncashedInbetween
                                ,[CarryForward]
                                ,[YearEndLapse]
                                ,[YearEndEncash]
                                ,[YearEndEncashCumulative]
                                ,[YearEndLapseCumulative] ,EncashedInbetween
                                from
                                (
                                SELECT ELS.EmployeeId
                                ,EI.EmployeeCode
                                ,EI.EmployeeName,EI.EmployeeCodeNumeric,EI.EmployeeCodePreFix
                                ,LT.UserName LeaveName
                                ,LT.LeaveType
                                --,isnull(CarryForwardOpeningBalance,0) +isnull([BroughtForward],0)  OpeningBalance
                                ,OpeningBalance=CASE WHEN IsEncashed =1 THEN ISNULL(CarryForward, 0)+ISNULL(EncashedInbetween, 0) ELSE ISNULL(BroughtForward, 0)+isnull(CarryForwardOpeningBalance,0) END
                                ,isnull(ELS.CurrentYearEarnedDaysOpeningBalance,0) EarnedDaysOB
                                ,isnull(ELS.CalculatedEarningDays,0) CalculatedEarningDays
                             
                                ,Allocation=CASE WHEN LT.LeaveType='Earn' THEN  
												CASE WHEN LvPolicyDetail.DOJorDOC> els.ToDate then isnull(CurrentYearAllocation,0)
													ELSE CASE WHEN ELS.NotEncashedButYearEnded=1 THEN isnull(CurrentYearAllocation,0) ELSE isnull(DaysCanBeSanctioned,0) END   
												END
                                            ELSE isnull(DaysCanBeSanctioned,0) END
                                
                                ,isnull(CurrentYearAvailedOpeningBalance,0) AvailedOB
                                ,isnull((SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                  INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                  where D.WorkDate BETWEEN yc.FromDate and yc.ToDate
                                AND m.EmpSystemID=els.EmployeeId AND m.LTSystemID=els.LeaveTypeId),0) availd
                                ,isnull((SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                  INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                  where D.WorkDate BETWEEN yc.FromDate and yc.ToDate
                                AND m.EmpSystemID=els.EmployeeId AND m.LTSystemID=els.LeaveTypeId),0) + isnull(CurrentYearAvailedOpeningBalance,0) TotalAvailed	
                                ,isnull([CarryForward],0) [CarryForward]


                                ,isnull([YearEndLapse],0) [YearEndLapse]
                                ,isnull([YearEndEncash],0) [YearEndEncash]
                                ,isnull([YearEndEncashCumulative],0) [YearEndEncashCumulative]
                                ,isnull([YearEndLapseCumulative],0) [YearEndLapseCumulative] 
                                ,isnull([IsYearlyProcessed],0) [IsYearlyProcessed]
                                ,isnull(DaysCanBeSanctioned,0)   DaysCanBeSanctioned
                                ,isnull(EncashedInbetween,0)   EncashedInbetween
                                FROM [TRN].[EmployeeLeaveSummary] ELS
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId
								INNER JOIN YearlyCalendar AS yc ON yc.Id=els.CalanderYearId

                             

-------------------------LvPolicyDetail start---------------------------
							LEFT JOIN ( select lpd.EncashmentBasis,lpd.LTSystemID, 
                             DOJorDOC=CASE WHEN lpd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  lpd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
										   	 END
                            ,emp.SystemID EmpSystemId,lpd.IsCarryForward
                            from dbo.LeavePolicyDetail as lpd
                            LEFT JOIN dbo.LeavePolicyMaster as lpm on lpd.LPMSystemID = lpm.SystemID
                            LEFT JOIN (select * from SCS.DesignationMasterConfiguration where PlantId='" + identity.PlantId + @"') DC on   lpm.SystemID = DC.LeavePolicyMasterId
                            LEFT JOIN MST.DesignationMaster DM on  DC.DesignationMasterId=DM.Id
                            LEFT JOIN dbo.EmployeeInformation emp on emp.GivenDesignationId=DM.DesignationId
                            
								) AS  LvPolicyDetail   ON LvPolicyDetail.LTSystemID=els.LeaveTypeId AND  LvPolicyDetail.IsCarryForward=1  AND LvPolicyDetail.EmpSystemId= ELS.EmployeeId                
                         
                             -------------------------LvPolicyDetail end---------------------------


                                Where ELS.CalanderYearId='" + sYearId + @"'
                                --AND ELS.EmployeeId=1800028 
                                and EI.PlantId='" + identity.PlantId + @"'
                                AND LvPolicyDetail.EncashmentBasis='CalanderYear'
                                
                                ) x
                                ORDER BY  x.EmployeeCodePreFix,x.EmployeeCodeNumeric ";

            #endregion obsolete

            sql = @"select EmployeeId, EmployeeCode,EmployeeName,EmployeeCodeNumeric,EmployeeCodePreFix,DOJorDOC,FromDate,x.ToDate
                                ,LeaveName
                                ,OpeningBalance
                                ,EarnedDaysOB
                                ,CalculatedEarningDays,
                                x.IsCarryForward, x.CarryForwardDay,
                                x.IsMaxEncashment, x.MaxEncashment,
                                x.IsMaxEncashmentLapse, x.MaxEncashmentLapse
                                ,Allocation DaysCanBeSanctioned
                                ,AvailedOB
                               ,availd
                                ,TotalAvailed
                                ,Balance=OpeningBalance +Allocation - TotalAvailed - EncashedInbetween
                                ,[CarryForward]
                                ,[YearEndLapse]
                                ,[YearEndEncash]
                                ,[YearEndEncashCumulative]
                                ,[YearEndLapseCumulative] ,EncashedInbetween
                                from
                                (
                                SELECT ELS.EmployeeId,FORMAT(els.FromDate,'dd-MMM-yyyy') AS FromDate,FORMAT(els.ToDate,'dd-MMM-yyyy') AS ToDate
                                ,EI.EmployeeCode,LvPolicyDetail.IsCarryForward,FORMAT(LvPolicyDetail.DOJorDOC,'dd-MMM-yyyy') AS DOJorDOC,
                                LvPolicyDetail.CarryForwardDay,
                                LvPolicyDetail.IsMaxEncashment,
                                LvPolicyDetail.MaxEncashment,
                                LvPolicyDetail.IsMaxEncashmentLapse,
                                LvPolicyDetail.MaxEncashmentLapse
                                ,EI.EmployeeName,EI.EmployeeCodeNumeric,EI.EmployeeCodePreFix
                                ,LT.UserName LeaveName
                                ,LT.LeaveType
                                --,isnull(CarryForwardOpeningBalance,0) +isnull([BroughtForward],0)  OpeningBalance
                                ,OpeningBalance=ISNULL(ELS.BroughtForward, 0)+isnull(ELS.CarryForwardOpeningBalance,0)
                                ,isnull(ELS.CurrentYearEarnedDaysOpeningBalance,0) EarnedDaysOB
                                ,isnull(ELS.CalculatedEarningDays,0) CalculatedEarningDays
                             
                                ,Allocation= isnull(ELS.CurrentYearAllocation,0)
            
                                ,isnull(ELS.CurrentYearAvailedOpeningBalance,0) AvailedOB
                                ,isnull((SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                  INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND m.EmpSystemID=els.EmployeeId AND m.LTSystemID=els.LeaveTypeId AND m.PlantID=els.PlantId),0) availd
                                ,isnull((SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                  INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND m.EmpSystemID=els.EmployeeId AND m.LTSystemID=els.LeaveTypeId AND m.PlantID=els.PlantId),0) + isnull(ELS.CurrentYearAvailedOpeningBalance,0) TotalAvailed	
                                ,isnull(ELS.[CarryForward],0) [CarryForward]


                                ,isnull(ELS.[YearEndLapse],0) [YearEndLapse]
                                ,isnull(ELS.[YearEndEncash],0) [YearEndEncash]
                                ,isnull(ELS.[YearEndEncashCumulative],0) [YearEndEncashCumulative]
                                ,isnull(ELS.[YearEndLapseCumulative],0) [YearEndLapseCumulative] 
                                ,isnull(ELS.[IsYearlyProcessed],0) [IsYearlyProcessed]
                                ,isnull(ELS.DaysCanBeSanctioned,0)   DaysCanBeSanctioned
                                ,isnull(ELS.EncashedInbetween,0)   EncashedInbetween
                                FROM [TRN].[EmployeeLeaveSummary] ELS
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId      
								INNER JOIN YearlyCalendar AS yc ON yc.Id=els.CalanderYearId   

-------------------------LvPolicyDetail start---------------------------
							LEFT JOIN ( select lpd.EncashmentBasis,lpd.LTSystemID,lpd.IsCarryForward,
							                   lpd.CarryForwardDay,IsMaxEncashment,	MaxEncashment,	IsMaxEncashmentLapse,	MaxEncashmentLapse,

                             DOJorDOC=CASE WHEN lpd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  lpd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
										   	 END
                            ,emp.SystemID EmpSystemId
                            from dbo.LeavePolicyDetail as lpd
                            LEFT JOIN dbo.LeavePolicyMaster as lpm on lpd.LPMSystemID = lpm.SystemID
                            LEFT JOIN (select * from SCS.DesignationMasterConfiguration where PlantId='" + identity.PlantId + @"') DC on   lpm.SystemID = DC.LeavePolicyMasterId
                            LEFT JOIN MST.DesignationMaster DM on  DC.DesignationMasterId=DM.Id
                            LEFT JOIN dbo.EmployeeInformation emp on emp.GivenDesignationId=DM.DesignationId
							) AS  LvPolicyDetail   ON LvPolicyDetail.LTSystemID=els.LeaveTypeId AND  LvPolicyDetail.IsCarryForward=1  AND LvPolicyDetail.EmpSystemId= ELS.EmployeeId                
                        
                             -------------------------LvPolicyDetail end---------------------------


                             Where ELS.CalanderYearId='" + sYearId + @"'
                                --AND ELS.EmployeeId=1800028 
                                and isnull(ELS.IsEncashed,0)=0
                                and EI.PlantId='" + identity.PlantId + @"'
                                AND LvPolicyDetail.EncashmentBasis='CalanderYear'
                                ) x
                                ORDER BY  x.EmployeeCodePreFix,x.EmployeeCodeNumeric ";
            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }//End Function 
        [HttpGet]
        public ActionResult GetLeaveYearEndProcessSummaryDataIndividual(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;

            sql = @"select EmployeeId, EmployeeCode,EmployeeName,EmployeeCodeNumeric,EmployeeCodePreFix,DOJorDOC,FromDate,x.ToDate
                                ,LeaveName
                                ,OpeningBalance
                                ,EarnedDaysOB
                                ,CalculatedEarningDays,
                                x.IsCarryForward, x.CarryForwardDay,
                                x.IsMaxEncashment, x.MaxEncashment,
                                x.IsMaxEncashmentLapse, x.MaxEncashmentLapse
                                ,Allocation DaysCanBeSanctioned
                                ,AvailedOB
                               ,availd
                                ,TotalAvailed
                                ,Balance=OpeningBalance +Allocation - TotalAvailed - EncashedInbetween
                                ,[CarryForward]
                                ,[YearEndLapse]
                                ,[YearEndEncash]
                                ,[YearEndEncashCumulative]
                                ,[YearEndLapseCumulative] ,EncashedInbetween
                                from
                                (
                                SELECT ELS.EmployeeId,FORMAT(els.FromDate,'dd-MMM-yyyy') AS FromDate,FORMAT(els.ToDate,'dd-MMM-yyyy') AS ToDate
                                ,EI.EmployeeCode,LvPolicyDetail.IsCarryForward,FORMAT(LvPolicyDetail.DOJorDOC,'dd-MMM-yyyy') AS DOJorDOC,
                                LvPolicyDetail.CarryForwardDay,
                                LvPolicyDetail.IsMaxEncashment,
                                LvPolicyDetail.MaxEncashment,
                                LvPolicyDetail.IsMaxEncashmentLapse,
                                LvPolicyDetail.MaxEncashmentLapse
                                ,EI.EmployeeName,EI.EmployeeCodeNumeric,EI.EmployeeCodePreFix
                                ,LT.UserName LeaveName
                                ,LT.LeaveType
                                --,isnull(CarryForwardOpeningBalance,0) +isnull([BroughtForward],0)  OpeningBalance
                                ,OpeningBalance=ISNULL(ELS.BroughtForward, 0)+isnull(ELS.CarryForwardOpeningBalance,0)
                                ,isnull(ELS.CurrentYearEarnedDaysOpeningBalance,0) EarnedDaysOB
                                ,isnull(ELS.CalculatedEarningDays,0) CalculatedEarningDays
                             
                                ,Allocation= isnull(ELS.CurrentYearAllocation,0)
            --                    CASE WHEN LT.LeaveType='Earn' THEN  
												--CASE WHEN LvPolicyDetail.DOJorDOC> els.ToDate then isnull(ELS.CurrentYearAllocation,0)
												--	ELSE CASE WHEN ELS.NotEncashedButYearEnded=1 THEN isnull(ELS.CurrentYearAllocation,0) ELSE isnull(ELS.DaysCanBeSanctioned,0) END   
												--END
            --                                ELSE isnull(ELS.DaysCanBeSanctioned,0) END
                                
                                ,isnull(ELS.CurrentYearAvailedOpeningBalance,0) AvailedOB
                                ,isnull((SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                  INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND m.EmpSystemID=els.EmployeeId AND m.LTSystemID=els.LeaveTypeId AND m.PlantID=els.PlantId),0) availd
                                ,isnull((SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                  INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND m.EmpSystemID=els.EmployeeId AND m.LTSystemID=els.LeaveTypeId AND m.PlantID=els.PlantId),0) + isnull(ELS.CurrentYearAvailedOpeningBalance,0) TotalAvailed	
                                ,isnull(ELS.[CarryForward],0) [CarryForward]


                                ,isnull(ELS.[YearEndLapse],0) [YearEndLapse]
                                ,isnull(ELS.[YearEndEncash],0) [YearEndEncash]
                                ,isnull(ELS.[YearEndEncashCumulative],0) [YearEndEncashCumulative]
                                ,isnull(ELS.[YearEndLapseCumulative],0) [YearEndLapseCumulative] 
                                ,isnull(ELS.[IsYearlyProcessed],0) [IsYearlyProcessed]
                                ,isnull(ELS.DaysCanBeSanctioned,0)   DaysCanBeSanctioned
                                ,isnull(ELS.EncashedInbetween,0)   EncashedInbetween
                                FROM [TRN].[EmployeeLeaveSummary] ELS
                               
                                JOIN [TRN].[EmployeeLeaveSummary] ELX ON els.Id=elx.Id and elx.Id=(SELECT TOP 1 Id FROM [TRN].[EmployeeLeaveSummary] X 
                                                                                                   WHERE x.EmployeeId=els.EmployeeId
																							AND x.LeaveTypeId=els.LeaveTypeId AND X.ToDate<='" + ToDate + @"' ORDER BY x.ToDate DESC )
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId         

-------------------------LvPolicyDetail start---------------------------
							LEFT JOIN ( select lpd.EncashmentBasis,lpd.LTSystemID,lpd.IsCarryForward,
							                   lpd.CarryForwardDay,IsMaxEncashment,	MaxEncashment,	IsMaxEncashmentLapse,	MaxEncashmentLapse,

                             DOJorDOC=CASE WHEN lpd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  lpd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
										   	 END
                            ,emp.SystemID EmpSystemId
                            from dbo.LeavePolicyDetail as lpd
                            LEFT JOIN dbo.LeavePolicyMaster as lpm on lpd.LPMSystemID = lpm.SystemID
                            LEFT JOIN (select * from SCS.DesignationMasterConfiguration where PlantId='" + identity.PlantId + @"') DC on   lpm.SystemID = DC.LeavePolicyMasterId
                            LEFT JOIN MST.DesignationMaster DM on  DC.DesignationMasterId=DM.Id
                            LEFT JOIN dbo.EmployeeInformation emp on emp.GivenDesignationId=DM.DesignationId
							) AS  LvPolicyDetail   ON LvPolicyDetail.LTSystemID=els.LeaveTypeId AND  LvPolicyDetail.IsCarryForward=1  AND LvPolicyDetail.EmpSystemId= ELS.EmployeeId                
                        
                             -------------------------LvPolicyDetail end---------------------------


                          Where ELS.ToDate<='" + ToDate + @"' 
                                and EI.PlantId='" + identity.PlantId + @"'
                                and isnull(ELS.IsEncashed,0)=0
                                AND ISNULL(els.IsYearlyProcessed,0)=0
                                AND LvPolicyDetail.EncashmentBasis<>'CalanderYear'
                                ) x
                                ORDER BY  x.EmployeeCodePreFix,x.EmployeeCodeNumeric ";

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }//End Function 

        [HttpGet]
        public ActionResult GetLeaveYearEndProcessSummaryDataNew(string sYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
         

            sql = @"select EmployeeId, EmployeeCode,EmployeeName,EmployeeCodeNumeric,EmployeeCodePreFix,DOJorDOC,FromDate,x.ToDate
                                ,LeaveName
                                ,OpeningBalance
                                ,EarnedDaysOB
                                ,CalculatedEarningDays,
                                x.IsCarryForward, x.CarryForwardDay,
                                x.IsMaxEncashment, x.MaxEncashment,
                                x.IsMaxEncashmentLapse, x.MaxEncashmentLapse
                                ,Allocation DaysCanBeSanctioned
                                ,AvailedOB
                               ,availd
                                ,TotalAvailed
                                ,Balance=OpeningBalance +Allocation - TotalAvailed - EncashedInbetween
                                ,[CarryForward]
                                ,[YearEndLapse]
                                ,[YearEndEncash]
                                ,[YearEndEncashCumulative]
                                ,[YearEndLapseCumulative] ,EncashedInbetween
                                from
                                (
                                SELECT ELS.EmployeeId,FORMAT(els.FromDate,'dd-MMM-yyyy') AS FromDate,FORMAT(els.ToDate,'dd-MMM-yyyy') AS ToDate
                                ,EI.EmployeeCode,LvPolicyDetail.IsCarryForward,FORMAT(LvPolicyDetail.DOJorDOC,'dd-MMM-yyyy') AS DOJorDOC,
                                LvPolicyDetail.CarryForwardDay,
                                LvPolicyDetail.IsMaxEncashment,
                                LvPolicyDetail.MaxEncashment,
                                LvPolicyDetail.IsMaxEncashmentLapse,
                                LvPolicyDetail.MaxEncashmentLapse
                                ,EI.EmployeeName,EI.EmployeeCodeNumeric,EI.EmployeeCodePreFix
                                ,LT.UserName LeaveName
                                ,LT.LeaveType
                                --,isnull(CarryForwardOpeningBalance,0) +isnull([BroughtForward],0)  OpeningBalance
                                ,OpeningBalance=ISNULL(ELS.BroughtForward, 0)+isnull(ELS.CarryForwardOpeningBalance,0)
                                ,isnull(ELS.CurrentYearEarnedDaysOpeningBalance,0) EarnedDaysOB
                                ,isnull(ELS.CalculatedEarningDays,0) CalculatedEarningDays
                             
                                ,Allocation= isnull(ELS.CurrentYearAllocation,0)
            
                                ,isnull(ELS.CurrentYearAvailedOpeningBalance,0) AvailedOB
                                ,isnull((SELECT sum(d.LvValue) FROM AttdnProcessData D 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND D.EmpSystemID=els.EmployeeId AND D.LTSystemID=els.LeaveTypeId and D.PlantID=els.PlantId),0) availd
                                ,isnull((SELECT sum(d.LvValue) FROM AttdnProcessData D 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND D.EmpSystemID=els.EmployeeId AND D.LTSystemID=els.LeaveTypeId and D.PlantID=els.PlantId),0) + isnull(ELS.CurrentYearAvailedOpeningBalance,0) TotalAvailed	
                                ,isnull(ELS.[CarryForward],0) [CarryForward]


                                ,isnull(ELS.[YearEndLapse],0) [YearEndLapse]
                                ,isnull(ELS.[YearEndEncash],0) [YearEndEncash]
                                ,isnull(ELS.[YearEndEncashCumulative],0) [YearEndEncashCumulative]
                                ,isnull(ELS.[YearEndLapseCumulative],0) [YearEndLapseCumulative] 
                                ,isnull(ELS.[IsYearlyProcessed],0) [IsYearlyProcessed]
                                ,isnull(ELS.DaysCanBeSanctioned,0)   DaysCanBeSanctioned
                                ,isnull(ELS.EncashedInbetween,0)   EncashedInbetween
                                FROM [TRN].[EmployeeLeaveSummary] ELS
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId      
								INNER JOIN YearlyCalendar AS yc ON yc.Id=els.CalanderYearId   

-------------------------LvPolicyDetail start---------------------------
							LEFT JOIN ( select lpd.EncashmentBasis,lpd.LTSystemID,lpd.IsCarryForward,
							                   lpd.CarryForwardDay,IsMaxEncashment,	MaxEncashment,	IsMaxEncashmentLapse,	MaxEncashmentLapse,

                             DOJorDOC=CASE WHEN lpd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  lpd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
										   	 END
                            ,emp.SystemID EmpSystemId
                            from dbo.LeavePolicyDetail as lpd
                            LEFT JOIN dbo.LeavePolicyMaster as lpm on lpd.LPMSystemID = lpm.SystemID
                            LEFT JOIN (select * from SCS.DesignationMasterConfiguration where PlantId='" + identity.PlantId + @"') DC on   lpm.SystemID = DC.LeavePolicyMasterId
                            LEFT JOIN MST.DesignationMaster DM on  DC.DesignationMasterId=DM.Id
                            LEFT JOIN dbo.EmployeeInformation emp on emp.GivenDesignationId=DM.DesignationId
							) AS  LvPolicyDetail   ON LvPolicyDetail.LTSystemID=els.LeaveTypeId AND  LvPolicyDetail.IsCarryForward=1  AND LvPolicyDetail.EmpSystemId= ELS.EmployeeId                
                        
                             -------------------------LvPolicyDetail end---------------------------


                             Where ELS.CalanderYearId='" + sYearId + @"'
                                --AND ELS.EmployeeId=1800028 
                                and isnull(ELS.IsEncashed,0)=0
                                and EI.PlantId='" + identity.PlantId + @"'
                                AND LvPolicyDetail.EncashmentBasis='CalanderYear'
                                ) x
                                ORDER BY  x.EmployeeCodePreFix,x.EmployeeCodeNumeric ";
            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }//End Function 
        [HttpGet]
        public ActionResult GetLeaveYearEndProcessSummaryDataIndividualNew(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;

            sql = @"select EmployeeId, EmployeeCode,EmployeeName,EmployeeCodeNumeric,EmployeeCodePreFix,DOJorDOC,FromDate,x.ToDate
                                ,LeaveName
                                ,OpeningBalance
                                ,EarnedDaysOB
                                ,CalculatedEarningDays,
                                x.IsCarryForward, x.CarryForwardDay,
                                x.IsMaxEncashment, x.MaxEncashment,
                                x.IsMaxEncashmentLapse, x.MaxEncashmentLapse
                                ,Allocation DaysCanBeSanctioned
                                ,AvailedOB
                               ,availd
                                ,TotalAvailed
                                ,Balance=OpeningBalance +Allocation - TotalAvailed - EncashedInbetween
                                ,[CarryForward]
                                ,[YearEndLapse]
                                ,[YearEndEncash]
                                ,[YearEndEncashCumulative]
                                ,[YearEndLapseCumulative] ,EncashedInbetween
                                from
                                (
                                SELECT ELS.EmployeeId,FORMAT(els.FromDate,'dd-MMM-yyyy') AS FromDate,FORMAT(els.ToDate,'dd-MMM-yyyy') AS ToDate
                                ,EI.EmployeeCode,LvPolicyDetail.IsCarryForward,FORMAT(LvPolicyDetail.DOJorDOC,'dd-MMM-yyyy') AS DOJorDOC,
                                LvPolicyDetail.CarryForwardDay,
                                LvPolicyDetail.IsMaxEncashment,
                                LvPolicyDetail.MaxEncashment,
                                LvPolicyDetail.IsMaxEncashmentLapse,
                                LvPolicyDetail.MaxEncashmentLapse
                                ,EI.EmployeeName,EI.EmployeeCodeNumeric,EI.EmployeeCodePreFix
                                ,LT.UserName LeaveName
                                ,LT.LeaveType
                                --,isnull(CarryForwardOpeningBalance,0) +isnull([BroughtForward],0)  OpeningBalance
                                ,OpeningBalance=ISNULL(ELS.BroughtForward, 0)+isnull(ELS.CarryForwardOpeningBalance,0)
                                ,isnull(ELS.CurrentYearEarnedDaysOpeningBalance,0) EarnedDaysOB
                                ,isnull(ELS.CalculatedEarningDays,0) CalculatedEarningDays
                             
                                ,Allocation= isnull(ELS.CurrentYearAllocation,0)
            --                    CASE WHEN LT.LeaveType='Earn' THEN  
												--CASE WHEN LvPolicyDetail.DOJorDOC> els.ToDate then isnull(ELS.CurrentYearAllocation,0)
												--	ELSE CASE WHEN ELS.NotEncashedButYearEnded=1 THEN isnull(ELS.CurrentYearAllocation,0) ELSE isnull(ELS.DaysCanBeSanctioned,0) END   
												--END
            --                                ELSE isnull(ELS.DaysCanBeSanctioned,0) END
                                
                                ,isnull(ELS.CurrentYearAvailedOpeningBalance,0) AvailedOB
                                ,isnull((SELECT sum(d.LvValue) FROM AttdnProcessData D 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND D.EmpSystemID=els.EmployeeId AND D.LTSystemID=els.LeaveTypeId AND D.PlantID=els.PlantId),0) availd
                                ,isnull((SELECT sum(d.LvValue) FROM AttdnProcessData D 
                                  where D.WorkDate BETWEEN ELS.FromDate and ELS.ToDate
                                AND D.EmpSystemID=els.EmployeeId AND D.LTSystemID=els.LeaveTypeId AND D.PlantID=els.PlantId),0) + isnull(ELS.CurrentYearAvailedOpeningBalance,0) TotalAvailed	
                                ,isnull(ELS.[CarryForward],0) [CarryForward]


                                ,isnull(ELS.[YearEndLapse],0) [YearEndLapse]
                                ,isnull(ELS.[YearEndEncash],0) [YearEndEncash]
                                ,isnull(ELS.[YearEndEncashCumulative],0) [YearEndEncashCumulative]
                                ,isnull(ELS.[YearEndLapseCumulative],0) [YearEndLapseCumulative] 
                                ,isnull(ELS.[IsYearlyProcessed],0) [IsYearlyProcessed]
                                ,isnull(ELS.DaysCanBeSanctioned,0)   DaysCanBeSanctioned
                                ,isnull(ELS.EncashedInbetween,0)   EncashedInbetween
                                FROM [TRN].[EmployeeLeaveSummary] ELS
                               
                                JOIN [TRN].[EmployeeLeaveSummary] ELX ON els.Id=elx.Id and elx.Id=(SELECT TOP 1 Id FROM [TRN].[EmployeeLeaveSummary] X 
                                                                                                   WHERE x.EmployeeId=els.EmployeeId
																							AND x.LeaveTypeId=els.LeaveTypeId AND X.ToDate<='" + ToDate + @"' ORDER BY x.ToDate DESC )
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId         

-------------------------LvPolicyDetail start---------------------------
							LEFT JOIN ( select lpd.EncashmentBasis,lpd.LTSystemID,lpd.IsCarryForward,
							                   lpd.CarryForwardDay,IsMaxEncashment,	MaxEncashment,	IsMaxEncashmentLapse,	MaxEncashmentLapse,

                             DOJorDOC=CASE WHEN lpd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  lpd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN lpd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN lpd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN lpd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
										   	 END
                            ,emp.SystemID EmpSystemId
                            from dbo.LeavePolicyDetail as lpd
                            LEFT JOIN dbo.LeavePolicyMaster as lpm on lpd.LPMSystemID = lpm.SystemID
                            LEFT JOIN (select * from SCS.DesignationMasterConfiguration where PlantId='" + identity.PlantId + @"') DC on   lpm.SystemID = DC.LeavePolicyMasterId
                            LEFT JOIN MST.DesignationMaster DM on  DC.DesignationMasterId=DM.Id
                            LEFT JOIN dbo.EmployeeInformation emp on emp.GivenDesignationId=DM.DesignationId
							) AS  LvPolicyDetail   ON LvPolicyDetail.LTSystemID=els.LeaveTypeId AND  LvPolicyDetail.IsCarryForward=1  AND LvPolicyDetail.EmpSystemId= ELS.EmployeeId                
                        
                             -------------------------LvPolicyDetail end---------------------------


                          Where ELS.ToDate<='" + ToDate + @"' 
                                and EI.PlantId='" + identity.PlantId + @"'
                                and isnull(ELS.IsEncashed,0)=0
                                AND ISNULL(els.IsYearlyProcessed,0)=0
                                AND LvPolicyDetail.EncashmentBasis<>'CalanderYear'
                                ) x
                                ORDER BY  x.EmployeeCodePreFix,x.EmployeeCodeNumeric ";

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }//End Function 



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


    }
}