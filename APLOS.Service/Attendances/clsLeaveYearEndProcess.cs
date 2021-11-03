//using LeaveService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace OTSBD.clsLeave
{
    public class clsLeaveYearEndProcess
    {
        //clsLeaveServiceAplos objLeaveServiceAplos = new clsLeaveServiceAplos();
        public clsLeaveYearEndProcess()
        {
            // TODO: Add constructor logic here
        }
        public void GetLeaveYearEndProcessDataGrid(string sPlantID, string sLeaveTypeId, string sYearId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                if (sLeaveTypeId == "All")
                {
                    strSql = @"SELECT ELS.[Id]
                                      ,els.EmployeeId
                                      ,EI.EmployeeName
	                                  ,EI.EmployeeCode
                                      ,[CalanderYearId]
                                      ,LT.UserName
                                     ,[LeaveTypeId]
                                      ,[BroughtForward]
                                      ,[CarryForward]
                                      ,[CurrentYearAllocation]
                                      ,[DaysCanBeSanctioned]
                                      ,[CurrentYearAvailedOpeningBalance]
                                      ,[CurrentYearEarnedDaysOpeningBalance]
                                      ,[CarryForwardOpeningBalance]
                                      ,[YearEndLapse]
                                      ,[YearEndEncash]
                                      ,[YearEndEncashCumulative]
                                      ,[YearEndLapseCumulative]
                                      ,[AppliedDays]
                                      ,[AvailedDays]
                                      ,[IsYearlyProcessed]
                                      ,[CalculatedEarningDays]
                                      , '' OpeningBalance
                                      , '' Allocation
                                      , '' TotalAvailed
                                      , '' Balance        
                            FROM [TRN].[EmployeeLeaveSummary] ELS
                                  LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                  LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId
                                  Where ELS.CalanderYearId='" + sYearId + @"'
                                  --AND ELS.EmployeeId=1800028 
                                  and ELS.PlantId='" + sPlantID + @"'
                                  ORDER BY Convert(INT, EI.EmployeeCode)";
                }
                else
                {

                    strSql = @"SELECT ELS.[Id]
                                       ,els.EmployeeId
                                      ,EI.EmployeeName
	                                  ,EI.EmployeeCode
                                      ,[CalanderYearId]
                                      ,LT.UserName
                                     ,[LeaveTypeId]
                                      ,[BroughtForward]
                                      ,[CarryForward]
                                      ,[CurrentYearAllocation]
                                      ,[DaysCanBeSanctioned]
                                      ,[CurrentYearAvailedOpeningBalance]
                                      ,[CurrentYearEarnedDaysOpeningBalance]
                                      ,[CarryForwardOpeningBalance]
                                      ,[YearEndLapse]
                                      ,[YearEndEncash]
                                      ,[YearEndEncashCumulative]
                                      ,[YearEndLapseCumulative]
                                      ,[AppliedDays]
                                      ,[AvailedDays]
                                      ,[IsYearlyProcessed]
                                      ,[CalculatedEarningDays]
                                      , '' OpeningBalance
                                      , '' Allocation
                                      , '' TotalAvailed
                                      , '' Balance        
                                  FROM [TRN].[EmployeeLeaveSummary] ELS
                                  LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                  LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId
                                  Where ELS.CalanderYearId='" + sYearId + @"'
                                  --AND ELS.EmployeeId=1800174 
                                  and ELS.PlantId='" + sPlantID + @"'
                                 and ELS.LeaveTypeId='" + sLeaveTypeId + @"'

                                  ORDER BY Convert(INT, EI.EmployeeCode)";
                }




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
        public void LeaveYearEndProcess(DateTime PDate, string sGroupID, string sPlantID, string sCurrentYearId, string sNextYearId, DataSet sdsLeaveTranInfo, out DataSet dsNewSummary, out DataSet dsOldSummary)
        {
            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            #region variables
            var _count = 0;
            dsNewSummary = null;
            dsOldSummary = null;
            //DataSet dsAllEmpEarningDaysSummary = null;
            //DataSet dsCalandarYearLocal = null;

            string EmpSystemId = string.Empty;
            string LeaveTypeId = string.Empty;
            string CalendarYearId = string.Empty;
            //decimal leaveResult = 0;



            #endregion variables

            try
            {
                //Earning Days
                //objLeaveYearEndProcessData.GetYearlyCalendarsFromDateAndToDateForLeaveYearEndProcess(sGroupID, sPlantID, sCurrentYearId, out dsCalandarYearLocal);
                //objLeaveYearEndProcessData.GetEarningDays(sPlantID, dsCalandarYearLocal.Tables[0].Rows[0]["FromDate"].ToString(), dsCalandarYearLocal.Tables[0].Rows[0]["ToDate"].ToString(), out dsAllEmpEarningDaysSummary);
                //DataView dvAllEmpEarningDaysSummary = null;
                //DataRow drAllEmpEarningDaysSummary = null;


                string EarnleaveID = string.Empty;
                EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();
                //for HR proj starts
                objLeaveYearEndProcessData.InitLeaveSummary(sGroupID, sPlantID, sCurrentYearId, out dsOldSummary);
                objLeaveYearEndProcessData.InitLeaveSummary(sGroupID, sPlantID, sNextYearId, out dsNewSummary);
                DataView dvSaveSummary = null;
                DataRow drSaveSummary = null;

                DataView dvSaveSummaryOld = null;
                DataRow drSaveSummaryOld = null;
                //for HR porj ends
                var _pks = string.Empty;

                if (dsOldSummary.Tables[0].Rows.Count > 0)
                {
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToString("dd-MMM-yyyy"), "LEAVE_SUMMARY_N", out _pks);

                }
                for (int i = 0; i < dsOldSummary.Tables[0].Rows.Count; i++)
                {
                    #region variables
                    string CarryForwardBasedOn = string.Empty;
                    decimal CarryForwardOpeningBalance = 0;
                    decimal CurrentYearEncashedInbetween = 0;
                    decimal CurrentYearAllocation = 0;
                    decimal CurrentYearAvailedOpeningBalance = 0;
                    decimal CurrentYearEarnedDaysOpeningBalance = 0;
                    //decimal CalculatedEarningDays = 0;
                    decimal CarryForward = 0;
                    decimal CarryForwardOld = 0;
                    decimal BroughtForwardOLd = 0;
                    decimal DaysCanBeSanctioned = 0;
                    decimal CarryForwordEncash = 0;
                    decimal CarryForwordLapse = 0;
                    decimal CarryforwardEncashCumulative = 0;
                    decimal CarryForwordLapseCumulative = 0;
                    decimal YearEndEncash = 0;
                    bool NotEncashedButYearEnded = false;
                    bool IsEncashed = false;
                    EmpSystemId = dsOldSummary.Tables[0].Rows[i]["EmployeeId"].ToString();
                    LeaveTypeId = dsOldSummary.Tables[0].Rows[i]["LeaveTypeId"].ToString();

                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CurrentYearEarnedDaysOpeningBalance"].ToString()))
                    {
                        CurrentYearEarnedDaysOpeningBalance = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CurrentYearEarnedDaysOpeningBalance"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CurrentYearAvailedOpeningBalance"].ToString()))
                    {
                        CurrentYearAvailedOpeningBalance = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CurrentYearAvailedOpeningBalance"].ToString());
                    }

                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CarryForwardOpeningBalance"].ToString()))
                    {
                        CarryForwardOpeningBalance = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CarryForwardOpeningBalance"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CarryForward"].ToString()))
                    {
                        CarryForwardOld = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CarryForward"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CurrentYearAllocation"].ToString()))
                    {
                        CurrentYearAllocation = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CurrentYearAllocation"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["BroughtForward"].ToString()))
                    {
                        BroughtForwardOLd = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["BroughtForward"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString()))
                    {
                        DaysCanBeSanctioned = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["EncashedInbetween"].ToString()))
                    {
                        CurrentYearEncashedInbetween = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["EncashedInbetween"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["YearEndEncash"].ToString()))
                    {
                        YearEndEncash = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["YearEndEncash"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["NotEncashedButYearEnded"].ToString()))
                    {
                        NotEncashedButYearEnded = Convert.ToBoolean(dsOldSummary.Tables[0].Rows[i]["NotEncashedButYearEnded"].ToString());
                    }

                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["IsEncashed"].ToString()))
                    {
                        IsEncashed = Convert.ToBoolean(dsOldSummary.Tables[0].Rows[i]["IsEncashed"].ToString());
                    }

                    //CalendarYearId = sdsInitLeaveSummary.Tables[0].Rows[i]["CalendarYearId"].ToString();
                    CalendarYearId = sCurrentYearId;
                    //DaysCanBeSanctioned = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString())- CurrentYearAvailedOpeningBalance;//
                    CarryForword objCarryForword = CheckLeavePolicyDetails(sPlantID, EmpSystemId, LeaveTypeId, BroughtForwardOLd, CarryForwardOpeningBalance, CarryForwardOld, DaysCanBeSanctioned, CurrentYearAllocation, CurrentYearAvailedOpeningBalance, YearEndEncash, CurrentYearEncashedInbetween, sdsLeaveTranInfo, PDate, NotEncashedButYearEnded, IsEncashed, out CarryForwardBasedOn);
                    //leaveResult = CalculateLeave(EmpSystemId, LeaveTypeId, sdsLeaveTranInfo);
                    if (objCarryForword != null)
                    {
                        CarryForward = objCarryForword.CarryForward;
                        CarryForwordEncash = objCarryForword.CarryForwordEncash;
                        CarryForwordLapse = objCarryForword.CarryForwordLapse;
                        CarryforwardEncashCumulative = objCarryForword.CarryforwardEncashCumulative;
                        CarryForwordLapseCumulative = objCarryForword.CarryForwordLapseCumulative;
                    }
                    else
                    {
                        CarryForward = 0;
                        CarryForwordEncash = 0;
                        CarryForwordLapse = 0;
                        CarryforwardEncashCumulative = 0;
                        CarryForwordLapseCumulative = 0;
                    }

                    //if (LeaveTypeId == EarnleaveID)
                    //{
                    //    dvAllEmpEarningDaysSummary = new DataView(dsAllEmpEarningDaysSummary.Tables[0]);
                    //    dvAllEmpEarningDaysSummary.RowFilter = "EmpSystemID='" + EmpSystemId + "'";
                    //    if (dvAllEmpEarningDaysSummary.Count > 0)
                    //    {
                    //        CalculatedEarningDays = Convert.ToDecimal(dvAllEmpEarningDaysSummary[0]["WorkingDays"].ToString());
                    //    }

                    //}

                    #endregion variables



                    #region Database entry
                    if (string.IsNullOrEmpty(sNextYearId))
                    {
                        throw new Exception("Calendar can not be blank...");
                    }
                    if (string.IsNullOrEmpty(sGroupID))
                    {
                        throw new Exception("GroupId can not be blank...");
                    }
                    if (string.IsNullOrEmpty(sPlantID))
                    {
                        throw new Exception("PlantId can not be blank...");
                    }
                    if (string.IsNullOrEmpty(EmpSystemId))
                    {
                        throw new Exception("EmployeeId can not be blank...");
                    }


                    if (EmpSystemId == null || LeaveTypeId == null || CalendarYearId == null)
                    {
                    }
                    else
                    {
                        //new year insert or update
                        dvSaveSummary = new DataView(dsNewSummary.Tables[0]);
                        dvSaveSummary.RowFilter = "EmployeeId='" + EmpSystemId + "' and LeaveTypeId='" + LeaveTypeId + "' and CalanderYearId='" + sNextYearId + "'";
                        if (dvSaveSummary.Count == 0)
                        {
                            //_count++;
                            //drSaveSummary = dsNewSummary.Tables[0].NewRow();
                            //drSaveSummary["Id"] = "LS" + _pks + "-" + _count;
                            //drSaveSummary["EmployeeId"] = EmpSystemId;
                            //drSaveSummary["CalanderYearId"] = sNextYearId;
                            //drSaveSummary["PlantId"] = sPlantID;
                            //drSaveSummary["CompanyGroupId"] = sGroupID;
                            //drSaveSummary["LeaveTypeId"] = LeaveTypeId;
                            //drSaveSummary["CurrentYearAllocation"] = 0;
                            //drSaveSummary["DaysCanBeSanctioned"] = 0;
                            //drSaveSummary["CurrentYearAvailedOpeningBalance"] = 0;
                            //drSaveSummary["CurrentYearEarnedDaysOpeningBalance"] = 0;
                            //drSaveSummary["CarryForwardOpeningBalance"] = 0;
                            //drSaveSummary["CarryForward"] = 0;
                            //drSaveSummary["BroughtForward"] = CarryForward;
                            //drSaveSummary["AppliedDays"] = 0;
                            //drSaveSummary["AvailedDays"] = 0;
                            ////drSaveSummary["PreviousYearCarryForward"] = 0;
                            //drSaveSummary["YearEndEncash"] = 0;
                            //drSaveSummary["YearEndLapse"] = 0;
                            ////drSaveSummary["YearEndEncashCumulative"] = 0;
                            ////drSaveSummary["YearEndLapseCumulative"] = 0;
                            //drSaveSummary["AddedBy"] = "Schedule";
                            //drSaveSummary["AddedDate"] = System.DateTime.Now;
                            //drSaveSummary["AddedFromIP"] = "::1";
                            //drSaveSummary["UpdatedFromIP"] = "::1";
                            //dsNewSummary.Tables[0].Rows.Add(drSaveSummary);
                        }
                        else
                        {
                            drSaveSummary = dvSaveSummary[0].Row;
                            drSaveSummary.BeginEdit();

                            drSaveSummary["BroughtForward"] = CarryForward;                    





                            if (!string.IsNullOrEmpty(CarryForwardBasedOn))
                            {
                                if (CarryForwardBasedOn == "CalanderYear")
                                {
                                    //drSaveSummary["CarryForward"] = 0;
                                    //drSaveSummary["YearEndEncash"] = 0;
                                    //drSaveSummary["YearEndLapse"] = 0;
                                    //drSaveSummary["CurrentYearAllocation"] = 0;
                                    //drSaveSummary["DaysCanBeSanctioned"] = 0;
                                    //drSaveSummary["CurrentYearAvailedOpeningBalance"] = 0;
                                    //drSaveSummary["CurrentYearEarnedDaysOpeningBalance"] = 0;
                                    //drSaveSummary["CarryForwardOpeningBalance"] = 0;
                                }
                            }
                            //drSaveSummary["YearEndEncashCumulative"] = 0;
                            //drSaveSummary["YearEndLapseCumulative"] = 0;


                            drSaveSummary["UpdatedFromIP"] = "::1";
                            drSaveSummary["UpdatedDate"] = System.DateTime.Now;
                            drSaveSummary["UpdatedBy"] = "Schedule";
                            drSaveSummary.EndEdit();
                        }
                        //Old year insert or update
                        dvSaveSummaryOld = new DataView(dsOldSummary.Tables[0]);
                        dvSaveSummaryOld.RowFilter = "EmployeeId='" + EmpSystemId + "' and LeaveTypeId='" + LeaveTypeId + "' and CalanderYearId='" + sCurrentYearId + "'";
                        if (dvSaveSummaryOld.Count == 0)
                        {

                        }
                        else
                        {
                            drSaveSummaryOld = dvSaveSummaryOld[0].Row;
                            drSaveSummaryOld.BeginEdit();

                            if (!string.IsNullOrEmpty(CarryForwardBasedOn))
                            {
                                if (CarryForwardBasedOn == "CalanderYear")
                                {
                                    drSaveSummaryOld["CarryForward"] = CarryForward;
                                    drSaveSummaryOld["YearEndEncash"] = CarryForwordEncash;
                                    drSaveSummaryOld["YearEndLapse"] = CarryForwordLapse;
                                    drSaveSummaryOld["EncashedInbetween"] = CarryForwordEncash;
                                }
                            }



                            //drSaveSummaryOld["YearEndEncashCumulative"] = CarryforwardEncashCumulative;
                            //drSaveSummaryOld["YearEndLapseCumulative"] = CarryForwordLapseCumulative;
                            drSaveSummaryOld["IsYearlyProcessed"] = true;
                            //drSaveSummaryOld["CalculatedEarningDays"] = CalculatedEarningDays;
                            //drSaveSummaryOld["CurrentYearAllocation"] = 0;
                            //drSaveSummaryOld["DaysCanBeSanctioned"] = 0;
                            //drSaveSummaryOld["CurrentYearAvailedOpeningBalance"] = 0;
                            //drSaveSummaryOld["CurrentYearEarnedDaysOpeningBalance"] = 0;
                            //drSaveSummaryOld["CarryForwardOpeningBalance"] = 0;
                            drSaveSummaryOld["UpdatedFromIP"] = "::1";
                            drSaveSummaryOld["UpdatedDate"] = System.DateTime.Now;
                            drSaveSummaryOld["UpdatedBy"] = "Schedule";
                            drSaveSummaryOld.EndEdit();
                        }


                    }//if(empId == null || leaveType == null || CalendarYearId == null)



                    #endregion Database entry
                }//loop dtLeaveInfo



            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function 


        public decimal CalculateLeave(string EmpSystemId, string LeaveTypeId, DataSet sdsLeaveTranInfo)
        {
            decimal result = 0;
            var totalleaveAvailed = string.Empty;
            try
            {

                DataView dv = new DataView(sdsLeaveTranInfo.Tables[0]);
                dv.RowFilter = "EmpSystemID='" + EmpSystemId + "' and LTSystemID='" + LeaveTypeId + "'";
                if (dv.Count > 0)
                {
                    totalleaveAvailed = dv[0]["totalLeave"].ToString();
                }
                if (totalleaveAvailed != "")
                {
                    result = Convert.ToDecimal(totalleaveAvailed);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public void GetNextYearID(string sPlantID, string sYearId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @" select Id,YearNo  from YearlyCalendar where PlantId='" + sPlantID + @"' and YEAR(FromDate) = (select YEAR ( ToDate )+1   from YearlyCalendar where PlantId='" + sPlantID + @"' and id='" + sYearId + @"')";



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

        }

        public void GetYearlyCalendarForLeaveYearEndProcess(string sGroupID, string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (
                                        SELECT Id
                                        , YearNo
                                        , REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate
                                        ,REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate
                                        ,(select Id from dbo.YearlyCalendar where '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate AND PlantId='" + sPlantID + @"') CalendarYear
                                        FROM YearlyCalendar WHERE PlantID = '" + sPlantID + @"' AND CompanyGroupId = '" + sGroupID + @"'
                                    ) AS A";

                //if (sSystemID.Trim() != "")
                //{
                //    strSQL = strSQL + " WHERE Id = '" + sSystemID + @"'";
                //}

                strSQL = strSQL + " ORDER BY YearNo";

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
        public void GetYearlyCalendarsFromDateAndToDateForLeaveYearEndProcess(string sGroupID, string sPlantID, string sYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (
                                        SELECT Id
                                        , YearNo
                                        , REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate
                                        ,REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate
                                        ,(select Id from dbo.YearlyCalendar where '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate AND PlantId='" + sPlantID + @"') CalendarYear,IsYearEndClosed
                                        FROM YearlyCalendar WHERE PlantID = '" + sPlantID + @"' AND CompanyGroupId = '" + sGroupID + @"'
                                    ) AS A
                                        WHERE Id = '" + sYearID + @"'";

                //if (sSystemID.Trim() != "")
                //{
                //    strSQL = strSQL + " WHERE Id = '" + sSystemID + @"'";
                //}

                strSQL = strSQL + " ORDER BY YearNo";

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

        public void GetLeaveTranInfo(string sGroupID, string sPlantID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpSystemID,LTSystemID,sum(d.d) totalLeave FROM [dbo].[LeaveTransaction] m   
                            left join  (
                            select sum(LeaveDuration) d,LvTrnsSystemID from [dbo].[LeaveTransactionDetails]
                            where IsAvailed=1 and WorkDate between '" + sFromDate + "' and '" + sToDate + @"' 
                            group by LvTrnsSystemID
                                ) d on d.LvTrnsSystemID=m.SystemID
                            where  m.GroupID='" + sGroupID + "' and PlantID=" + sPlantID + @"
                            ---and m.EmpSystemID=1800156
                            group by EmpSystemID,LTSystemID ";





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
        //(sPlantID, EmpSystemId, LeaveTypeId, BroughtForwardOLd, DaysCanBeSanctioned, CurrentYearAvailedOpeningBalance, sdsLeaveTranInfo);
        public CarryForword xCheckLeavePolicyDetails(string sPlantID, string sEmployeeId, string LeaveTypeId, decimal BroughtForwardOLd, decimal CarryforwardOB, decimal DaysCanBeSanctioned, decimal CurrentYearAvailedOpeningBalance, DataSet sdsLeaveTranInfo)
        {
            CarryForword objCarryForword = null;
            DataSet dsLeavePld = null;
            decimal LeaveTran = 0;
            decimal newCarryForward = 0;
            decimal Carryforward = 0;
            decimal CarryForwordEncash = 0;
            decimal CarryForwordLapse = 0;




            decimal CarryforwardEncashCumulative = 0;
            decimal CarryForwordLapseCumulative = 0;

            bool IsCFFixed = false;

            decimal CarryForwardMaxDay = 0;
            decimal MaxAllocationLimit = 0;
            bool IsCarryForwardCumulative = false;
            decimal CarryForwardCumulativeMaxLimit = 0;
            bool IsMaxEncashment = false;
            decimal MaxEncashment = 0;
            bool IsMaxEncashmentLapse = false;
            decimal MaxEncashmentLapse = 0;
            string CarryForwardRoundupOption = string.Empty;

            bool IsCFRestEncash = false;
            bool IsCFCRestEncash = false;
            try
            {
                GetLeavePolicyDetails(sPlantID, sEmployeeId, out dsLeavePld);
                DataView dv = new DataView(dsLeavePld.Tables[0]);
                dv.RowFilter = "IsCarryForward=1 and LTSystemID='" + LeaveTypeId + "'";
                if (dv.Count > 0)
                {
                    IsCFFixed = Convert.ToBoolean(dv[0]["IsCFFixed"].ToString());
                    CarryForwardMaxDay = Convert.ToDecimal(dv[0]["CarryForwardDay"].ToString());
                    //IsCarryForwardCumulative = Convert.ToBoolean(dv[0]["IsCarryForwardCumulative"].ToString());
                    //CarryForwardCumulativeMaxLimit = Convert.ToDecimal(dv[0]["CarryForwardCumulative"].ToString());
                    MaxAllocationLimit = Convert.ToDecimal(dv[0]["MaxAllocationLimit"].ToString());
                    IsMaxEncashment = Convert.ToBoolean(dv[0]["IsMaxEncashment"].ToString());
                    MaxEncashment = Convert.ToDecimal(dv[0]["MaxEncashment"].ToString());
                    IsMaxEncashmentLapse = Convert.ToBoolean(dv[0]["IsMaxEncashmentLapse"].ToString());
                    MaxEncashmentLapse = Convert.ToDecimal(dv[0]["MaxEncashmentLapse"].ToString());

                    IsCFRestEncash = Convert.ToBoolean(dv[0]["IsCFRestEncash"].ToString());
                    IsCFCRestEncash = Convert.ToBoolean(dv[0]["IsCFCRestEncash"].ToString());


                    MaxEncashmentLapse = Convert.ToDecimal(dv[0]["MaxEncashmentLapse"].ToString());
                    CarryForwardRoundupOption = dv[0]["CarryForwardRoundupOption"].ToString();

                    LeaveTran = CalculateLeave(sEmployeeId, LeaveTypeId, sdsLeaveTranInfo);
                    newCarryForward = DaysCanBeSanctioned + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance);
                    if (newCarryForward > 0)
                    {
                        objCarryForword = xGetCarryforwardQnty(IsCFFixed, newCarryForward, BroughtForwardOLd, CarryForwardMaxDay, IsCFRestEncash, IsCFCRestEncash, IsCarryForwardCumulative, CarryForwardCumulativeMaxLimit);
                        if (!string.IsNullOrEmpty(CarryForwardRoundupOption))
                        {
                            if (CarryForwardRoundupOption == "Round Up")
                            {
                                Carryforward = Math.Ceiling(objCarryForword.CarryForward);
                            }
                            if (CarryForwardRoundupOption == "Round Down")
                            {
                                Carryforward = Math.Floor(objCarryForword.CarryForward);
                            }
                            if (CarryForwardRoundupOption == "Round")
                            {
                                Carryforward = Math.Round(objCarryForword.CarryForward);
                            }
                        }
                        else
                        {
                            Carryforward = objCarryForword.CarryForward;
                        }
                        CarryForwordEncash = objCarryForword.CarryForwordEncash;
                        CarryForwordLapse = objCarryForword.CarryForwordLapse;
                        CarryforwardEncashCumulative = objCarryForword.CarryforwardEncashCumulative;
                        CarryForwordLapseCumulative = objCarryForword.CarryForwordLapseCumulative;

                    }
                    else
                    {

                        //objCarryForword.Carryforward = 0;
                        //CarryForwordEncash = 0;
                        //CarryForwordLapse = 0;
                        //CarryforwardEncashCumulative = 0;
                        //CarryForwordLapseCumulative = 0;
                        //objCarryForword = null;




                    }


                }



                return objCarryForword;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }

        }


        public CarryForword CheckLeavePolicyDetails(string sPlantID, string sEmployeeId, string LeaveTypeId, decimal BroughtForwardOLd, decimal CarryforwardOB, decimal CarryforwardOld, decimal DaysCanBeSanctioned, decimal CurrentYearAllocation, decimal CurrentYearAvailedOpeningBalance, decimal CurrentYearEncashed, decimal CurrentYearEncashedInbetween, DataSet sdsLeaveTranInfo, DateTime PDate, bool NotEncashedButYearEnded, bool IsEncashed, out string CarryForwardBasedOn)
        {
            CarryForword objCarryForword = null;
            DataSet dsLeavePld = null;
            decimal LeaveTran = 0;
            decimal newCarryForward = 0;
            decimal Carryforward = 0;
            decimal CarryForwordEncash = 0;
            decimal CarryForwordLapse = 0;
            CarryForwardBasedOn = string.Empty;



            decimal CarryforwardEncashCumulative = 0;
            decimal CarryForwordLapseCumulative = 0;
            bool IsLvAvailed = true;
            bool IsCFFixed = false;
            bool IsCarryforward = false;
            decimal CarryForwardMaxDay = 0;
            decimal MaxAllocationLimit = 0;
            bool IsCarryForwardCumulative = false;
            decimal CarryForwardCumulativeMaxLimit = 0;
            bool IsMaxEncashment = false;
            decimal MaxEncashment = 0;
            bool IsMaxEncashmentLapse = false;
            decimal MaxEncashmentLapse = 0;
            string CarryForwardRoundupOption = string.Empty;
            //string CarryForwardBasedOn = string.Empty;


            bool LvAvailedOnDOJ = false;
            bool LvAvailedOnDOC = false;
            double LvCanAvailAfter = 0;
            string CanAvailUOM = string.Empty;


            bool IsCFRestEncash = false;
            bool IsCFCRestEncash = false;
            DateTime DOJorDOC = DateTime.Now;
            try
            {
                GetLeavePolicyDetails(sPlantID, sEmployeeId, out dsLeavePld);
                DataView dv = new DataView(dsLeavePld.Tables[0]);
                dv.RowFilter = "LTSystemID='" + LeaveTypeId + "'";
                if (dv.Count > 0)
                {
                    //CarryForwardBasedOn =dv[0]["EncasementEndDate"].ToString();
                    CarryForwardBasedOn = dv[0]["EncashmentBasis"].ToString();

                    IsCarryforward = Convert.ToBoolean(dv[0]["IsCarryForward"].ToString());
                    IsCFFixed = Convert.ToBoolean(dv[0]["IsCFFixed"].ToString());
                    CarryForwardMaxDay = Convert.ToDecimal(dv[0]["CarryForwardDay"].ToString());
                    //IsCarryForwardCumulative = Convert.ToBoolean(dv[0]["IsCarryForwardCumulative"].ToString());
                    //CarryForwardCumulativeMaxLimit = Convert.ToDecimal(dv[0]["CarryForwardCumulative"].ToString());
                    MaxAllocationLimit = Convert.ToDecimal(dv[0]["MaxAllocationLimit"].ToString());
                    IsMaxEncashment = Convert.ToBoolean(dv[0]["IsMaxEncashment"].ToString());
                    MaxEncashment = Convert.ToDecimal(dv[0]["MaxEncashment"].ToString());
                    IsMaxEncashmentLapse = Convert.ToBoolean(dv[0]["IsMaxEncashmentLapse"].ToString());
                    MaxEncashmentLapse = Convert.ToDecimal(dv[0]["MaxEncashmentLapse"].ToString());

                    IsCFRestEncash = Convert.ToBoolean(dv[0]["IsCFRestEncash"].ToString());
                    IsCFCRestEncash = Convert.ToBoolean(dv[0]["IsCFCRestEncash"].ToString());


                    MaxEncashmentLapse = Convert.ToDecimal(dv[0]["MaxEncashmentLapse"].ToString());
                    CarryForwardRoundupOption = dv[0]["CarryForwardRoundupOption"].ToString();
                    LvAvailedOnDOJ = Convert.ToBoolean(dv[0]["LvAvailedOnDOJ"].ToString());
                    LvAvailedOnDOC = Convert.ToBoolean(dv[0]["LvAvailedOnDOC"].ToString()); ;
                    LvCanAvailAfter = Convert.ToDouble(dv[0]["LvCanAvailAfter"].ToString());
                    CanAvailUOM = dv[0]["CanAvailUOM"].ToString();
                    if (!string.IsNullOrEmpty(dv[0]["DOJorDOC"].ToString()))
                    {
                        DOJorDOC = Convert.ToDateTime(dv[0]["DOJorDOC"].ToString());
                    }

                    if (LvAvailedOnDOJ || LvAvailedOnDOC)
                    {
                        if (CanAvailUOM.ToUpper() == "DAY")
                        {
                            if (DOJorDOC.AddDays(LvCanAvailAfter) > PDate)
                            {
                                IsLvAvailed = false;
                            }

                        };
                        if (CanAvailUOM.ToUpper() == "MONTH")
                        {
                            if (DOJorDOC.AddMonths((int)LvCanAvailAfter) > PDate)
                            {
                                IsLvAvailed = false;
                            }
                        }
                        if (CanAvailUOM.ToUpper() == "YEAR")
                        {
                            if (DOJorDOC.AddYears((int)LvCanAvailAfter) > PDate)
                            {
                                IsLvAvailed = false;
                            }
                        }
                    }



                    LeaveTran = CalculateLeave(sEmployeeId, LeaveTypeId, sdsLeaveTranInfo);


                    #region CalanderYear
                    if (CarryForwardBasedOn == "CalanderYear")
                    {

                        if (IsLvAvailed)
                        {
                            if (NotEncashedButYearEnded)
                            {
                                //newCarryForward = CurrentYearAllocation + CarryforwardOB + BroughtForwardOLd - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);
                                newCarryForward = CurrentYearAllocation + CarryforwardOB + BroughtForwardOLd - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                            }
                            else
                            {
                                //newCarryForward = DaysCanBeSanctioned + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);
                                newCarryForward = DaysCanBeSanctioned + CarryforwardOB+ BroughtForwardOLd - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                            }

                        }
                        else
                        {

                            //newCarryForward = CurrentYearAllocation + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);
                            newCarryForward = CurrentYearAllocation + CarryforwardOB+ BroughtForwardOLd - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);
                        }

                        if (newCarryForward > 0)
                        {

                            objCarryForword = GetCarryforwardQnty(IsCarryforward, IsCFFixed, CarryForwardRoundupOption, newCarryForward, CarryForwardMaxDay, IsCFRestEncash, IsMaxEncashment, MaxEncashment, IsLvAvailed);




                        }
                        else
                        {

                            objCarryForword = new CarryForword();
                        }
                    }
                    #endregion
                    #region DOJ
                    if (CarryForwardBasedOn == "DOJ")
                    {


                        if (IsLvAvailed)
                        {
                            if (NotEncashedButYearEnded)
                            {
                                if (IsEncashed)
                                {
                                    newCarryForward = CurrentYearAllocation + CarryforwardOB + CurrentYearEncashed - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                                }
                                else
                                {
                                    newCarryForward = CurrentYearAllocation + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                                }
                            }
                            else
                            {
                                if (IsEncashed)
                                {
                                    newCarryForward = DaysCanBeSanctioned + CarryforwardOB + CurrentYearEncashed - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                                }
                                else
                                {
                                    newCarryForward = DaysCanBeSanctioned + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                                }
                            }

                        }
                        else
                        {

                            newCarryForward = CurrentYearAllocation + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);
                        }
                        if (newCarryForward > 0)
                        {


                            objCarryForword = new CarryForword();

                            if (IsEncashed)
                            {
                                objCarryForword.CarryForward = newCarryForward + CarryforwardOld;

                            }
                            else
                            {
                                objCarryForword.CarryForward = newCarryForward + BroughtForwardOLd;

                            }
                        }
                        else
                        {
                            objCarryForword = new CarryForword();


                        }

                    }
                    #endregion
                    #region EncashmentDate
                    if (CarryForwardBasedOn == "EncashmentDate")
                    {

                        if (IsLvAvailed)
                        {
                            if (NotEncashedButYearEnded)
                            {
                                if (IsEncashed)
                                {
                                    newCarryForward = CurrentYearAllocation + CarryforwardOB + CurrentYearEncashed + BroughtForwardOLd - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                                }
                                else
                                {
                                    newCarryForward = CurrentYearAllocation + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                                }
                            }
                            else
                            {
                                if (IsEncashed)
                                {
                                    //newCarryForward = DaysCanBeSanctioned + CarryforwardOB + CurrentYearEncashed - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);
                                    newCarryForward = DaysCanBeSanctioned + CarryforwardOld  - LeaveTran;
                                }
                                else
                                {
                                    newCarryForward = DaysCanBeSanctioned + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);

                                }
                            }

                        }
                        else
                        {

                            newCarryForward = CurrentYearAllocation + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance + CurrentYearEncashedInbetween);
                        }
                        if (newCarryForward > 0)
                        {


                            objCarryForword = new CarryForword();
                            //objCarryForword.CarryForward = newCarryForward + CarryforwardOld;
                            if (IsEncashed)
                            {
                                //objCarryForword.CarryForward = newCarryForward + CarryforwardOld;
                                objCarryForword.CarryForward = newCarryForward;
                            }
                            else
                            {
                                objCarryForword.CarryForward = newCarryForward + BroughtForwardOLd;

                            }

                        }
                        else
                        {


                            objCarryForword = new CarryForword();

                        }
                    }
                    #endregion













                }



                return objCarryForword;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }

        }
        public CarryForword xGetCarryforwardQnty(bool IsCFFixed, decimal newCarryForward, decimal BroughtForwardOLd, decimal CarryForwardMaxDay, bool IsCFRestEncash, bool IsCFCRestEncash, bool IsCarryForwardCumulative, decimal CarryForwardCumulativeMaxLimit)
        {

            //DataSet dsSaveSummary = null;
            //DataRow drSaveSummary = null;
            decimal CarryforwardResult = 0;
            decimal CarryForwordEncash = 0;
            decimal CarryForwordLapse = 0;


            decimal newCarryForwardCumulative = 0;
            decimal CarryforwardCumulativeResult = 0;
            decimal CarryforwardEncashCumulative = 0;
            decimal CarryForwordLapseCumulative = 0;
            if (IsCFFixed == true)//fiexd
            {
                //carryforward
                if (newCarryForward <= CarryForwardMaxDay)
                {
                    CarryforwardResult = newCarryForward;
                }
                else
                {
                    CarryforwardResult = CarryForwardMaxDay;
                    var rest = newCarryForward - CarryForwardMaxDay;

                    if (IsCFRestEncash == true)
                    {
                        CarryForwordEncash = rest;
                        CarryForwordLapse = 0;
                    }
                    else
                    {
                        CarryForwordLapse = rest;
                        CarryForwordEncash = 0;
                    }
                }
            }
            else //persent
            {
                CarryforwardResult = (newCarryForward * CarryForwardMaxDay) / 100;
                var rest = newCarryForward - CarryforwardResult;

                if (IsCFCRestEncash == true)
                {
                    CarryForwordEncash = rest;
                    CarryForwordLapse = 0;
                }
                else
                {
                    CarryForwordLapse = rest;
                    CarryForwordEncash = 0;
                }

            }



            //carryforward Cumulative
            if (IsCarryForwardCumulative == true)
            {
                newCarryForwardCumulative = CarryforwardResult + BroughtForwardOLd;
                if (newCarryForwardCumulative <= CarryForwardCumulativeMaxLimit)
                {
                    CarryforwardResult = newCarryForwardCumulative;
                }
                else
                {
                    //carryforward Cumulative
                    CarryforwardResult = CarryForwardCumulativeMaxLimit;
                    var rest = newCarryForwardCumulative - CarryForwardCumulativeMaxLimit;
                    if (IsCFCRestEncash == true)
                    {
                        CarryforwardEncashCumulative = rest;
                        CarryForwordLapseCumulative = 0;
                    }
                    else
                    {
                        CarryforwardEncashCumulative = 0;
                        CarryForwordLapseCumulative = rest;
                    }
                }
            }



            CarryForword obj = new CarryForword();
            obj.CarryForward = CarryforwardResult;
            obj.CarryForwordEncash = CarryForwordEncash;
            obj.CarryForwordLapse = CarryForwordLapse;
            obj.CarryforwardCumulativeResult = CarryforwardCumulativeResult;
            obj.CarryforwardEncashCumulative = CarryforwardEncashCumulative;
            obj.CarryForwordLapseCumulative = CarryForwordLapseCumulative;
            return obj;

        }

        public CarryForword GetCarryforwardQnty(bool IsCarryForward, bool IsCFFixed, string CFRoundupOption, decimal newCarryForward, decimal CarryForwardMaxDay, bool IsCFRestEncash, bool IsCFCRestEncashMaxLimit, decimal CFEncashMaxLimit, bool IsLvAvailed)
        {


            decimal CarryforwardTemp = 0;
            decimal CarryforwardResult = 0;
            decimal CarryForwordEncash = 0;
            decimal CarryForwordLapse = 0;



            decimal CarryforwardCumulativeResult = 0;
            decimal CarryforwardEncashCumulative = 0;
            decimal CarryForwordLapseCumulative = 0;





            if (IsCarryForward == true) //CarryForward and Encash
            {



                if (IsCFFixed == true)//fiexd
                {
                    //carryforward
                    if (newCarryForward <= CarryForwardMaxDay)
                    {
                        CarryforwardResult = GetRoundupOption(CFRoundupOption, newCarryForward);
                    }
                    else
                    {
                        CarryforwardResult = CarryForwardMaxDay;
                        var rest = newCarryForward - CarryForwardMaxDay;

                        if (IsCFRestEncash == true) //Encashment
                        {
                            if (IsCFCRestEncashMaxLimit == true) //Encashment max limit wise
                            {
                                if (rest >= CFEncashMaxLimit)
                                {
                                    CarryForwordEncash = CFEncashMaxLimit;
                                }
                                else
                                {
                                    CarryForwordEncash = rest;
                                }
                            }
                            else //Encashment all
                            {
                                CarryForwordEncash = rest;
                            }

                            CarryForwordLapse = 0;
                        }
                        else //Lapse all
                        {
                            CarryForwordLapse = rest;
                            CarryForwordEncash = 0;
                        }
                    }
                }
                else //persent
                {
                    if (IsLvAvailed)
                    {
                        CarryforwardTemp = (newCarryForward * CarryForwardMaxDay) / 100;
                        CarryforwardResult = GetRoundupOption(CFRoundupOption, CarryforwardTemp);
                        //var resttemp = newCarryForward - CarryforwardTemp;

                        //var rest = GetRoundupOptionForEncashment(CFRoundupOption, resttemp); 


                        var rest = newCarryForward - CarryforwardResult;
                        if (IsCFRestEncash == true)
                        {
                            if (rest > 0)
                            {
                                CarryForwordEncash = rest;
                            }
                            else
                            {
                                CarryForwordEncash = 0;
                            }
                            CarryForwordLapse = 0;
                        }
                        else
                        {
                            if (rest > 0)
                            {
                                CarryForwordLapse = rest;
                            }
                            else
                            {
                                CarryForwordLapse = 0;
                            }
                            //CarryForwordLapse = rest;
                            CarryForwordEncash = 0;
                        }
                    }
                    else
                    {
                        CarryforwardResult = GetRoundupOption(CFRoundupOption, newCarryForward);
                    }


                }

            }
            else// only Encashment
            {
                if (IsCFRestEncash == true) //Encashment
                {
                    if (IsCFCRestEncashMaxLimit == true) //Encashment max limit wise
                    {
                        if (newCarryForward >= CFEncashMaxLimit)
                        {
                            CarryForwordEncash = CFEncashMaxLimit;
                        }
                        else
                        {
                            CarryForwordEncash = newCarryForward;
                        }
                    }
                    else //Encashment all
                    {
                        CarryForwordEncash = newCarryForward;
                    }

                    CarryForwordLapse = 0;
                }
                else //Lapse all
                {
                    CarryForwordLapse = newCarryForward;
                    CarryForwordEncash = 0;
                }
            }









            CarryForword obj = new CarryForword();
            obj.CarryForward = CarryforwardResult;
            obj.CarryForwordEncash = CarryForwordEncash;
            obj.CarryForwordLapse = CarryForwordLapse;
            obj.CarryforwardCumulativeResult = CarryforwardCumulativeResult;
            obj.CarryforwardEncashCumulative = CarryforwardEncashCumulative;
            obj.CarryForwordLapseCumulative = CarryForwordLapseCumulative;
            return obj;

        }


        public decimal GetRoundupOption(string RoundupOption, decimal value)
        {
            decimal result = 0;
            try
            {
                if (!string.IsNullOrEmpty(RoundupOption))
                {
                    if (RoundupOption == "Round Up")
                    {
                        result = Math.Ceiling(value);
                    }
                    if (RoundupOption == "Round Down")
                    {
                        result = Math.Floor(value);
                    }
                    if (RoundupOption == "Round")
                    {
                        result = Math.Round(value);
                    }
                    if (RoundupOption == "Exact")
                    {
                        result = value;
                    }
                }
                else
                {
                    result = value;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return result;

        }



        public decimal GetRoundupOptionForEncashment(string RoundupOption, decimal value)
        {
            decimal result = 0;
            try
            {
                if (!string.IsNullOrEmpty(RoundupOption))
                {
                    if (RoundupOption == "Round Up")
                    {
                        result = Math.Floor(value);
                    }
                    if (RoundupOption == "Round Down")
                    {
                        result = Math.Ceiling(value);
                    }
                    if (RoundupOption == "Round")
                    {
                        result = Math.Round(value);
                    }
                    if (RoundupOption == "Exact")
                    {
                        result = value;
                    }
                }
                else
                {
                    result = value;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return result;

        }

        public void GetLeavePolicyDetails(string sPlantID, string sEmployeeId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @"select lpd.* 
                            ,DOJorDOC=CASE WHEN lpd.LvAvailedOnDOJ=1 THEN emp.DOJ 
                            WHEN  lpd.LvAvailedOnDOC=1 THEN emp.DOC end
                            from dbo.LeavePolicyDetail as lpd
                            LEFT JOIN dbo.LeavePolicyMaster as lpm on lpd.LPMSystemID = lpm.SystemID
                            LEFT JOIN (select * from SCS.DesignationMasterConfiguration where PlantId='" + sPlantID + @"') DC on   lpm.SystemID = DC.LeavePolicyMasterId
                            LEFT JOIN MST.DesignationMaster DM on  DC.DesignationMasterId=DM.Id
                            LEFT JOIN dbo.EmployeeInformation emp on emp.GivenDesignationId=DM.DesignationId
                            where lpd.IsCarryForward=1 and emp.SystemId=" + sEmployeeId;



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

        }



        public void InitLeaveSummary(string CompanyGroupId, string plantId, string calendarYearId, out DataSet dsSaveSummary)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from [TRN].[EmployeeLeaveSummary] 
                                    where PlantId='" + plantId + @"'  
                                    and CalanderYearId = '" + calendarYearId + @"' 
                                    ----AND EmployeeId IN (1800001)
                                    and CompanyGroupId = '" + CompanyGroupId + @"' 
                                    and LeaveTypeId in ( SELECT LTSystemID FROM [LeavePolicyDetail] WHERE IsCarryForward=1 and PlantId='" + plantId + @"'   and GroupId = '" + CompanyGroupId + @"'  )";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsSaveSummary, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        }


        public void GetEarningDays(string PlantID, string earnStartDate, string earnEndDate, out DataSet dsAllEmpEarningDaysSummary)
        {


            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            dsAllEmpEarningDaysSummary = null;
            try
            {

                strSql = @" select count(EmpSystemID) WorkingDays, EmpSystemID from AttdnProcessData
                                    where WorkDate between '" + earnStartDate + @"' and '" + earnEndDate + @"'
                                    and PlantID='" + PlantID + @"' and DayStatus in
                        (select DayType from LeavePolicyWorkingDays where LPDetailID in (SELECT SystemID FROM [LeavePolicyDetail] WHERE  LTSystemID=(select id from LeaveType where LeaveType='Earn'))) Group By EmpSystemID";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsAllEmpEarningDaysSummary, false, false, "", "1");
                //return dsRef.Tables[0];
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }


        }

        public string GetEarnLeaveID()
        {


            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef = null;
            try
            {

                strSql = @"select id from LeaveType where LeaveType='Earn'";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                return dsRef.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }


        }
        public void GetLeaveTypeName(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select id,UserName from LeaveType";

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
        public void GetDOJorDOC(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemId,DOJ,DOC FROM EmployeeInformation";

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
        public void GetYearlyCalendarseForYearEndClosedData(string sGroupID, string sPlantID, string sYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  YearlyCalendar WHERE PlantID = '" + sPlantID + @"' AND CompanyGroupId = '" + sGroupID + @"' AND Id = '" + sYearID + @"'";




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
        public void YearlyCalendarsYearEndClosedProcess(string sGroupID, string sPlantID, string sCurrentYearId, out DataSet dsCalandarYearLocalSummay)
        {
            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            #region variables


            dsCalandarYearLocalSummay = null;
            DataView dvSaveSummary = null;
            DataRow drSaveSummary = null;


            #endregion variables

            try
            {
                //Earning Days
                objLeaveYearEndProcessData.GetYearlyCalendarseForYearEndClosedData(sGroupID, sPlantID, sCurrentYearId, out dsCalandarYearLocalSummay);


                if (dsCalandarYearLocalSummay.Tables[0].Rows.Count > 0)
                {
                    #region variables



                    #endregion variables



                    #region Database entry

                    if (string.IsNullOrEmpty(sGroupID))
                    {
                        throw new Exception("GroupId can not be blank...");
                    }
                    if (string.IsNullOrEmpty(sPlantID))
                    {
                        throw new Exception("PlantId can not be blank...");
                    }
                    if (string.IsNullOrEmpty(sCurrentYearId))
                    {
                        throw new Exception("CurrentYearId can not be blank...");
                    }


                    if (sGroupID == null || sPlantID == null || sCurrentYearId == null)
                    {
                    }
                    else
                    {
                        //new year insert or update
                        dvSaveSummary = new DataView(dsCalandarYearLocalSummay.Tables[0]);
                        dvSaveSummary.RowFilter = " PlantID = '" + sPlantID + "' AND CompanyGroupId = '" + sGroupID + @"' AND Id = '" + sCurrentYearId + @"'";
                        if (dvSaveSummary.Count == 0)
                        {

                        }
                        else
                        {
                            drSaveSummary = dvSaveSummary[0].Row;
                            drSaveSummary.BeginEdit();
                            drSaveSummary["IsYearEndClosed"] = true;

                            drSaveSummary.EndEdit();
                        }
                        //Old year insert or update



                    }


                    #endregion Database entry
                }
                else
                {
                    throw new Exception("Calander Year can not Found...");
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function 

        public void GetBalance(string sGroupID, string sPlantID, string sCurrentYearId, string sLeaveType, out DataSet dsOldSummary)
        {
            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            #region variables
            var _count = 0;
            //dsNewSummary = null;
            dsOldSummary = null;
            DataSet dsAllEmpEarningDaysSummary = null;
            DataSet dsCalandarYearLocal = null;
            DataSet dsProRataPolicy = null;
            DataSet sdsLeaveTranInfo = null;

            string EmpSystemId = string.Empty;
            string LeaveTypeId = string.Empty;
            string CalendarYearId = string.Empty;
            //decimal leaveResult = 0;

            DataView dvProRataPolicy = null;

            #endregion variables

            try
            {
                //Earning Days
                //objLeaveYearEndProcessData.GetYearlyCalendarsFromDateAndToDateForLeaveYearEndProcess(sGroupID, sPlantID, sCurrentYearId, out dsCalandarYearLocal);
                //objLeaveYearEndProcessData.GetEarningDays(sPlantID, dsCalandarYearLocal.Tables[0].Rows[0]["FromDate"].ToString(), dsCalandarYearLocal.Tables[0].Rows[0]["ToDate"].ToString(), out dsAllEmpEarningDaysSummary);
                //DataView dvAllEmpEarningDaysSummary = null;
                //DataRow drAllEmpEarningDaysSummary = null;
                objLeaveYearEndProcessData.GetYearlyCalendarsFromDateAndToDateForLeaveYearEndProcess(sGroupID, sPlantID, sCurrentYearId, out dsCalandarYearLocal);
                //objLeaveYearEndProcessData.InitLeaveSummary(lblGroupID.Text.Trim(), ddlPlant.SelectedValue.ToString().Trim(),  ddlYear.SelectedValue.ToString().Trim(),out dsInitLeaveSummary);
                objLeaveYearEndProcessData.GetLeaveTranInfo(sGroupID, sPlantID, dsCalandarYearLocal.Tables[0].Rows[0]["FromDate"].ToString(), dsCalandarYearLocal.Tables[0].Rows[0]["ToDate"].ToString(), out sdsLeaveTranInfo);

                string EarnleaveID = string.Empty;
                EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();

                //string EarnleaveID = string.Empty;
                //EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();
                //for HR proj starts
                objLeaveYearEndProcessData.GetLeaveYearEndProcessDataGrid(sPlantID, sLeaveType, sCurrentYearId, out dsOldSummary);
                //objLeaveYearEndProcessData.GetProRataPolicy(out dsProRataPolicy);



                DataView dvSaveSummary = null;
                DataRow drSaveSummary = null;

                //DataView dvSaveSummaryOld = null;
                //DataRow drSaveSummaryOld = null;
                //for HR porj ends
                var _pks = string.Empty;


                for (int i = 0; i < dsOldSummary.Tables[0].Rows.Count; i++)
                {
                    #region variables
                    decimal LeaveTran = 0;
                    //decimal CarryForwordEncash = 0;
                    //decimal CarryForwordLapse = 0;
                    //decimal CarryforwardEncashCumulative = 0;
                    //decimal CarryForwordLapseCumulative = 0;
                    decimal LeaveDays = 0;
                    decimal Balance = 0;
                    decimal CarryForward = 0;
                    decimal BroughtForward = 0;
                    //decimal CarryForwardOpeningBalance = 0;
                    decimal CurrentYearAllocation = 0;
                    decimal CurrentYearAvailedOpeningBalance = 0;
                    decimal CurrentYearEarnedDaysOpeningBalance = 0;
                    decimal CalculatedEarningDays = 0;
                    decimal DaysCanBeSanctioned = 0;
                    decimal CarryForwardOpeningBalance = 0;
                    decimal AppliedDays = 0;

                    decimal AvailedDays = 0;

                    decimal OpeningBalance = 0;
                    decimal Allocation = 0;
                    decimal TotalAvailed = 0;
                    //decimal OpeningBalance = 0;

                    EmpSystemId = dsOldSummary.Tables[0].Rows[i]["EmployeeId"].ToString();
                    LeaveTypeId = dsOldSummary.Tables[0].Rows[i]["LeaveTypeId"].ToString();

                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CalculatedEarningDays"].ToString()))
                    {
                        CalculatedEarningDays = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CalculatedEarningDays"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CurrentYearEarnedDaysOpeningBalance"].ToString()))
                    {
                        CurrentYearEarnedDaysOpeningBalance = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CurrentYearEarnedDaysOpeningBalance"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CurrentYearAvailedOpeningBalance"].ToString()))
                    {
                        CurrentYearAvailedOpeningBalance = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CurrentYearAvailedOpeningBalance"].ToString());
                    }

                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CarryForwardOpeningBalance"].ToString()))
                    {
                        CarryForwardOpeningBalance = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CarryForwardOpeningBalance"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CurrentYearAllocation"].ToString()))
                    {
                        CurrentYearAllocation = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CurrentYearAllocation"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["BroughtForward"].ToString()))
                    {
                        BroughtForward = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["BroughtForward"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString()))
                    {
                        DaysCanBeSanctioned = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CarryForward"].ToString()))
                    {
                        CarryForward = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["CarryForward"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["CarryForward"].ToString()))
                    {
                        AvailedDays = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["AvailedDays"].ToString());
                    }
                    if (!string.IsNullOrEmpty(dsOldSummary.Tables[0].Rows[i]["AppliedDays"].ToString()))
                    {
                        AppliedDays = Convert.ToDecimal(dsOldSummary.Tables[0].Rows[i]["AppliedDays"].ToString());
                    }
                    CalendarYearId = sCurrentYearId;
                    LeaveTran = CalculateLeave(EmpSystemId, LeaveTypeId, sdsLeaveTranInfo);

                    Balance = (DaysCanBeSanctioned + BroughtForward) - (LeaveTran + CurrentYearAvailedOpeningBalance);


                    //Opening Balance
                    OpeningBalance = BroughtForward + CarryForwardOpeningBalance;
                    //Total Availe
                    TotalAvailed = LeaveTran + CurrentYearAvailedOpeningBalance;


                    //if (LeaveTypeId == EarnleaveID) //earn leave true
                    //{
                    //    //dvAllEmpEarningDaysSummary = new DataView(dsAllEmpEarningDaysSummary.Tables[0]);
                    //    //dvAllEmpEarningDaysSummary.RowFilter = "EmpSystemID='" + EmpSystemId + "'";
                    //    //if (dvAllEmpEarningDaysSummary.Count > 0)
                    //    //{
                    //    //    CalculatedEarningDays = Convert.ToDecimal(dvAllEmpEarningDaysSummary[0]["WorkingDays"].ToString());
                    //    //}
                    //    LeaveDays = DaysCanBeSanctioned;
                    //    Balance = DaysCanBeSanctioned - AvailedDays;
                    //}
                    //else
                    //{
                    //    dvProRataPolicy = new DataView(dsProRataPolicy.Tables[0]);
                    //    dvProRataPolicy.RowFilter = "IsProratacurrentyear=1 and LTSystemID='" + LeaveTypeId + "'";
                    //    if (dvProRataPolicy.Count == 0) //not prorata
                    //    {
                    //        LeaveDays = CurrentYearAllocation;
                    //        Balance = CurrentYearAllocation - AvailedDays;
                    //    }
                    //    else
                    //    {
                    //        LeaveDays = DaysCanBeSanctioned;
                    //        Balance = DaysCanBeSanctioned - AvailedDays;
                    //    }
                    //}
                    #endregion variables



                    #region Database entry
                    //if (string.IsNullOrEmpty(sNextYearId))
                    //{
                    //    throw new Exception("Calendar can not be blank...");
                    //}
                    //if (string.IsNullOrEmpty(sGroupID))
                    //{
                    //    throw new Exception("GroupId can not be blank...");
                    //}
                    //if (string.IsNullOrEmpty(sPlantID))
                    //{
                    //    throw new Exception("PlantId can not be blank...");
                    //}
                    //if (string.IsNullOrEmpty(EmpSystemId))
                    //{
                    //    throw new Exception("EmployeeId can not be blank...");
                    //}


                    if (EmpSystemId == null || LeaveTypeId == null)
                    {
                    }
                    else
                    {
                        //new year insert or update
                        dvSaveSummary = new DataView(dsOldSummary.Tables[0]);
                        dvSaveSummary.RowFilter = "EmployeeId='" + EmpSystemId + "' and LeaveTypeId='" + LeaveTypeId + "' ";
                        if (dvSaveSummary.Count == 0)
                        {
                            _count++;
                            drSaveSummary = dsOldSummary.Tables[0].NewRow();
                            //drSaveSummary["Id"] = "LS" + _pks + "-" + _count;
                            drSaveSummary["EmployeeId"] = EmpSystemId;
                            drSaveSummary["CalanderYearId"] = sCurrentYearId;
                            drSaveSummary["PlantId"] = sPlantID;
                            drSaveSummary["CompanyGroupId"] = sGroupID;
                            drSaveSummary["LeaveTypeId"] = LeaveTypeId;
                            drSaveSummary["CurrentYearAllocation"] = CurrentYearAllocation;
                            drSaveSummary["DaysCanBeSanctioned"] = DaysCanBeSanctioned;
                            drSaveSummary["CurrentYearAvailedOpeningBalance"] = CurrentYearAvailedOpeningBalance;
                            drSaveSummary["CurrentYearEarnedDaysOpeningBalance"] = CurrentYearEarnedDaysOpeningBalance;
                            drSaveSummary["CarryForwardOpeningBalance"] = CarryForwardOpeningBalance;
                            drSaveSummary["CarryForward"] = CarryForward;
                            drSaveSummary["BroughtForward"] = BroughtForward;
                            drSaveSummary["AppliedDays"] = AppliedDays;
                            drSaveSummary["AvailedDays"] = LeaveTran;
                            drSaveSummary["LeaveDays"] = LeaveDays;
                            drSaveSummary["Balance"] = Balance;
                            drSaveSummary["OpeningBalance"] = OpeningBalance;
                            drSaveSummary["TotalAvailed"] = TotalAvailed;
                            drSaveSummary["Allocation"] = Allocation;
                            //drSaveSummary["PreviousYearCarryForward"] = 0;
                            //drSaveSummary["YearEndEncash"] = 0;
                            //drSaveSummary["YearEndLapse"] = 0;
                            //drSaveSummary["YearEndEncashCumulative"] = 0;
                            //drSaveSummary["YearEndLapseCumulative"] = 0;
                            drSaveSummary["AddedBy"] = "Schedule";
                            drSaveSummary["AddedDate"] = System.DateTime.Now;
                            drSaveSummary["AddedFromIP"] = "::1";
                            drSaveSummary["UpdatedFromIP"] = "::1";
                            dsOldSummary.Tables[0].Rows.Add(drSaveSummary);
                        }
                        else
                        {
                            drSaveSummary = dvSaveSummary[0].Row;
                            drSaveSummary.BeginEdit();
                            //drSaveSummary["CarryForward"] = 0;
                            //drSaveSummary["BroughtForward"] = CarryForward;
                            //drSaveSummary["YearEndEncash"] = 0;
                            //drSaveSummary["YearEndLapse"] = 0;
                            //drSaveSummary["YearEndEncashCumulative"] = 0;
                            //drSaveSummary["YearEndLapseCumulative"] = 0;

                            drSaveSummary["CurrentYearAllocation"] = 0;
                            drSaveSummary["DaysCanBeSanctioned"] = 0;
                            drSaveSummary["CurrentYearAvailedOpeningBalance"] = 0;
                            drSaveSummary["CurrentYearEarnedDaysOpeningBalance"] = 0;
                            drSaveSummary["CarryForwardOpeningBalance"] = 0;
                            //drSaveSummary["UpdatedFromIP"] = "::1";
                            //drSaveSummary["UpdatedDate"] = System.DateTime.Now;
                            //drSaveSummary["UpdatedBy"] = "Schedule";
                            drSaveSummary["EmployeeId"] = EmpSystemId;
                            //drSaveSummary["CalanderYearId"] = sNextYearId;
                            //drSaveSummary["PlantId"] = sPlantID;
                            //drSaveSummary["CompanyGroupId"] = sGroupID;
                            drSaveSummary["LeaveTypeId"] = LeaveTypeId;
                            drSaveSummary["CurrentYearAllocation"] = CurrentYearAllocation;
                            drSaveSummary["DaysCanBeSanctioned"] = DaysCanBeSanctioned;
                            drSaveSummary["CurrentYearAvailedOpeningBalance"] = CurrentYearAvailedOpeningBalance;
                            drSaveSummary["CurrentYearEarnedDaysOpeningBalance"] = CurrentYearEarnedDaysOpeningBalance;
                            drSaveSummary["CarryForwardOpeningBalance"] = CarryForwardOpeningBalance;
                            drSaveSummary["CarryForward"] = CarryForward;
                            drSaveSummary["BroughtForward"] = BroughtForward;
                            drSaveSummary["AppliedDays"] = AppliedDays;
                            drSaveSummary["AvailedDays"] = LeaveTran;
                            //drSaveSummary["LeaveDays"] = LeaveDays;
                            drSaveSummary["Balance"] = Balance;
                            drSaveSummary.EndEdit();
                        }



                    }//if(empId == null || leaveType == null || CalendarYearId == null)



                    #endregion Database entry
                }//loop dtLeaveInfo

            }


            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function 
        public void GetProRataPolicy(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from [LeavePolicyDetail] where IsProratacurrentyear=1";



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
        }



        public void GetLeaveYearEndProcessSummaryDataGrid(string sPlantID, string sLeaveTypeId, string sYearId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                if (sLeaveTypeId == "All")
                {
                    strSql = @"select EmployeeId, EmployeeCode,EmployeeName
,LeaveName
,OpeningBalance
,EarnedDaysOB
,CalculatedEarningDays
,Convert(int,Allocation) Allocation
--,DaysCanBeSanctioned
,AvailedOB
,Availd
,TotalAvailed
,Balance=OpeningBalance + Convert(int,Allocation) - TotalAvailed
,[CarryForward]
,[YearEndLapse]
,[YearEndEncash]
,[YearEndEncashCumulative]
,[YearEndLapseCumulative] 
from
(
SELECT ELS.EmployeeId
,EI.EmployeeCode
,EI.EmployeeName
,LT.UserName LeaveName
,LT.LeaveType
, CarryForwardOpeningBalance+[BroughtForward] OpeningBalance
,ELS.CurrentYearEarnedDaysOpeningBalance EarnedDaysOB
,ELS.CalculatedEarningDays
, Allocation=case when lt.LeaveType='Earn' then (CurrentYearEarnedDaysOpeningBalance+CalculatedEarningDays)/20
else CurrentYearAllocation end
,isnull(CurrentYearAvailedOpeningBalance,0) AvailedOB
,isnull(tr.totalLeave,0) availd
,isnull(tr.totalLeave,0) + isnull(CurrentYearAvailedOpeningBalance,0) TotalAvailed	
,[CarryForward]


,[YearEndLapse]
,[YearEndEncash]
,[YearEndEncashCumulative]
,[YearEndLapseCumulative] 
,[IsYearlyProcessed]
,DaysCanBeSanctioned

                                FROM [TRN].[EmployeeLeaveSummary] ELS
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId
                                left join (
                                SELECT EmpSystemID,LTSystemID,sum(d.d) totalLeave FROM [dbo].[LeaveTransaction] m 
                                left join (
                                select sum(LeaveDuration) d,LvTrnsSystemID from [dbo].[LeaveTransactionDetails]
                                where IsAvailed=1 and WorkDate between '01-jan-2018' and '31-dec-2018' 
                                group by LvTrnsSystemID
                                ) d on d.LvTrnsSystemID=m.SystemID
                                where m.GroupID='CG20181' and PlantID=20188
                                --and m.EmpSystemID=1800029
                                group by EmpSystemID,LTSystemID
                                ) tr on tr.EmpSystemID=ELS.EmployeeId and els.LeaveTypeId=tr.LTSystemID



                                Where ELS.CalanderYearId='" + sYearId + @"'
                                --AND ELS.EmployeeId=1800028 
                                and ELS.PlantId='" + sPlantID + @"'
                                ) x
                                ORDER BY Convert(INT, x.EmployeeCode)";
                }
                else
                {
                    strSql = @"select EmployeeId, EmployeeCode,EmployeeName
,LeaveName
,OpeningBalance
,EarnedDaysOB
,CalculatedEarningDays
,Convert(int,Allocation) Allocation
--,DaysCanBeSanctioned
,AvailedOB
,Availd
,TotalAvailed
,Balance=OpeningBalance + Convert(int,Allocation) - TotalAvailed
,[CarryForward]
,[YearEndLapse]
,[YearEndEncash]
,[YearEndEncashCumulative]
,[YearEndLapseCumulative] 
from
(
SELECT ELS.EmployeeId
,EI.EmployeeCode
,EI.EmployeeName
,LT.UserName LeaveName
,LT.LeaveType
, CarryForwardOpeningBalance+[BroughtForward] OpeningBalance
,ELS.CurrentYearEarnedDaysOpeningBalance EarnedDaysOB
,ELS.CalculatedEarningDays
, Allocation=case when lt.LeaveType='Earn' then (CurrentYearEarnedDaysOpeningBalance+CalculatedEarningDays)/20
else CurrentYearAllocation end
,isnull(CurrentYearAvailedOpeningBalance,0) AvailedOB
,isnull(tr.totalLeave,0) availd
,isnull(tr.totalLeave,0) + isnull(CurrentYearAvailedOpeningBalance,0) TotalAvailed	
,[CarryForward]


,[YearEndLapse]
,[YearEndEncash]
,[YearEndEncashCumulative]
,[YearEndLapseCumulative] 
,[IsYearlyProcessed]
,DaysCanBeSanctioned
                                FROM [TRN].[EmployeeLeaveSummary] ELS
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=ELS.EmployeeId
                                LEFT JOIN LeaveType LT ON LT.Id=ELS.LeaveTypeId
                                left join (
                                SELECT EmpSystemID,LTSystemID,sum(d.d) totalLeave FROM [dbo].[LeaveTransaction] m 
                                left join (
                                select sum(LeaveDuration) d,LvTrnsSystemID from [dbo].[LeaveTransactionDetails]
                                where IsAvailed=1 and WorkDate between '01-jan-2018' and '31-dec-2018' 
                                group by LvTrnsSystemID
                                ) d on d.LvTrnsSystemID=m.SystemID
                                where m.GroupID='CG20181' and PlantID=20188
                                --and m.EmpSystemID=1800029
                                group by EmpSystemID,LTSystemID
                                ) tr on tr.EmpSystemID=ELS.EmployeeId and els.LeaveTypeId=tr.LTSystemID



                                Where ELS.CalanderYearId='" + sYearId + @"'
                                --AND ELS.EmployeeId=1800028 
                                and ELS.LeaveTypeId='" + sLeaveTypeId + @"'
                                and ELS.PlantId='" + sPlantID + @"'
                                ) x
                                ORDER BY Convert(INT, x.EmployeeCode)";

                }




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
    }

    public class CarryForword
    {
        public decimal CarryForward { get; set; }
        public decimal CarryForwordEncash { get; set; }
        public decimal CarryForwordLapse { get; set; }
        public decimal CarryforwardCumulativeResult { get; set; }
        public decimal CarryforwardEncashCumulative { get; set; }
        public decimal CarryForwordLapseCumulative { get; set; }
        public string CarryForwardBasedOn { get; set; }

    }
}