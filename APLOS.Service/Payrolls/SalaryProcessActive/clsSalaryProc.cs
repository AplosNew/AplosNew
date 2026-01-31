using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI.WebControls;

namespace OTSBD
{
    public class clsSalaryProc
    {
        public clsSalaryProc()
        {
            // TODO: Add constructor logic here
        }

        public void SaveDataSetsForSalaryProcess(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();//

                //objCon.ExecuteNonQueryWrapper("DELETE FROM SalaryProcChild WHERE MonthNo = " + intMonthNo + " AND YearNo = " + intYearNo + " AND IsDisbursed = 0 AND (" + strEmp + ")", true, "1");

                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }

            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }

        }//End Function

        public void SaveDataSets(params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function//


        public void GetSlrProcMst(int intMonthNo, int intYearNo, string sSystemID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryProcMaster WHERE MonthNo = " + intMonthNo + @" 
                                AND YearNo = " + intYearNo + @" AND SystemID = '" + sSystemID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetArrearProcMst(int intMonthNo, int intYearNo, string sSystemID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM ArrearProcMaster WHERE MonthNo = " + intMonthNo + @" 
                                AND YearNo = " + intYearNo + @" AND SystemID = '" + sSystemID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetAttdnDataMonthlySummary(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM [dbo].[AttdnDataMonthlySummary] WHERE MonthNo = " + intMonthNo + @" 
                                AND YearNo = " + intYearNo + @" AND IsDisbusted = 0 AND (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetSalaryProceAttdnData(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM [dbo].[SalaryProceAttdnData] 
                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @" AND (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetExtraAbsent(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT convert(numeric(18,2),count(id))  ExtraAbsent,EmpSystemID
                              FROM [SCS].[WeeklyAbsentismAssignment]
                              WHERE month(WorkingDate) = " + intMonthNo + @" AND YEAR(WorkingDate) = " + intYearNo + @" AND  (" + sEmpInfo + @")
                              group by EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetExtraAbsentHoliday(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT convert(numeric(18,2),count(id))  ExtraAbsent,EmpSystemID
                              FROM [trn].[HolidayAbsentismAssignment]
                              WHERE month(WorkDate) = " + intMonthNo + @" AND YEAR(WorkDate) = " + intYearNo + @" AND  (" + sEmpInfo + @")
                              group by EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void xxGetAttdnDataForMonthlyProc(string sEmpSystemID, string sfrmDate, string sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                //clsCrossModule ob = new clsCrossModule();
                GenericAttendance.clsCrossModule ob = new GenericAttendance.clsCrossModule();
                strSQL = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(TotalPresent, 0)) TotalPresent, SUM(ISNULL(TotalLate, 0)) TotalLate, 
		                            SUM(ISNULL(TotalAbsent, 0)) TotalAbsent, SUM(ISNULL(TotalLv, 0)) TotalLv, 
		                            SUM(ISNULL(TotalMLv, 0)) TotalMLv, SUM(ISNULL(TotalWeekOff, 0)) TotalWeekOff, SUM(ISNULL(TotalCompAssignLv, 0))  TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay, 0)) TotalHoliDay, SUM(ISNULL(TotalWeekOffHoliDay, 0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(OTHr, 0)) TotalOTHr, 0.00 TotalNormalOTHr, 0.00 TotalExtraOTHr, PlantID  ,
                                    SUM(ISNULL(TotalLWP, 0)) TotalLWP   
                            FROM (SELECT EmpSystemID, WorkDate, PlantID,
			                             " + ob.GetAttSum() + @"
                                        OTHr
	                             FROM dbo.AttdnProcessData 
                                WHERE WorkDate BETWEEN '" + sfrmDate + @"'
                                    AND '" + sToDate + @"'  
                                    AND (" + sEmpSystemID + @")) A
                            GROUP BY EmpSystemID, PlantID";

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
        public void GetAttdnDataForMonthlyProc(string sEmpSystemID, string sfrmDate, string sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //clsCrossModule ob = new clsCrossModule();
                GenericAttendance.clsCrossModule ob = new GenericAttendance.clsCrossModule();
                strSQL = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, 
                                   CAST(COUNT(WorkDate) As int) TotalProcDate,
		                            SUM(ISNULL(CAST(WorkingDayValue As decimal(18, 2)), '0.00')) TotalWorkingDay,
                                SUM(ISNULL(CAST(ActualWorkingDayValue As decimal(18, 2)), '0.00')) TotalActualWorkingDay,
                                SUM(ISNULL(CAST(PayDayValue As decimal(18, 2)), '0.00')) TotalPayDay,
                                SUM(ISNULL(CAST(NonPayDayValue As decimal(18, 2)), '0.00')) TotalNonPayDay,
                                SUM(ISNULL(CAST(PresentValue As decimal(18, 2)), '0.00')) TotalPresent,
                                SUM(ISNULL(CAST(LateValue As decimal(18, 2)), '0.00')) TotalLate,
                                SUM(ISNULL(CAST(AbsentValue As decimal(18, 2)), '0.00')) TotalAbsent,
                                SUM(ISNULL(CAST(LvValue As decimal(18, 2)), '0.00')) TotalLv,
                                SUM(ISNULL(CAST((CASE WHEN ISNULL(lt.LeaveType,'')='' THEN 0 ELSE 1 END) As decimal(18, 2)),'0.00')) TotalMLv,
                                SUM(ISNULL(CAST(WeekOffValue As decimal(18, 2)),'0.00')) TotalWeekOff,
                                SUM(ISNULL(CAST(WeekOffValue As decimal(18, 2)),'0.00')) WeekoffDays,
                                SUM(ISNULL(CAST(0 As decimal(18, 2)),'0.00')) TotalCompAssignLv,
                                SUM(ISNULL(CAST(HoliDayValue As decimal(18, 2)),'0.00')) TotalHoliDay,
                                SUM(ISNULL(CAST(0 As decimal(18, 2)),'0.00')) TotalWeekOffHoliDay,
                                SUM(ISNULL(CAST(OTHr As decimal(18, 2)), '0.00')) TotalOTHr,
                                0.00 TotalNormalOTHr,
                                0.00 TotalExtraOTHr, 
                                SUM(ISNULL(CAST((CASE WHEN ISNULL(ds.PayDay,0)=0 THEN l.AvailedValue ELSE 0 END) As decimal(18, 2)), '0.00')) TotalLWP,  
                                SUM(ISNULL(CAST((CASE WHEN ISNULL(ds.PayDay,0)>0 THEN l.AvailedValue ELSE 0 END) As decimal(18, 2)), '0.00')) TotalLVWithPay 
                             
			                            
	                             FROM dbo.AttdnProcessData apd left join daytype p on apd.DayStatus=p.DayType
								 LEFT JOIN LeaveType AS lt ON lt.Id=apd.LTSystemID
								 LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=apd.EmpSystemID
                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=apd.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
								LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id AND l.LeaveTypeId=apd.LTSystemID
                                WHERE WorkDate BETWEEN '" + sfrmDate + @"'
                                    AND '" + sToDate + @"'  
                                    AND (" + sEmpSystemID + @")
                            GROUP BY EmpSystemID";

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

        public void xGetAttdnDataForMonthlyProc(string sEmpSystemID, string sfrmDate, string sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(TotalPresent, 0)) TotalPresent, SUM(ISNULL(TotalLate, 0)) TotalLate, 
		                            SUM(ISNULL(TotalAbsent, 0)) TotalAbsent, SUM(ISNULL(TotalLv, 0)) TotalLv, 
		                            SUM(ISNULL(TotalMLv, 0)) TotalMLv, SUM(ISNULL(TotalWeekOff, 0)) TotalWeekOff, SUM(ISNULL(TotalCompAssignLv, 0))  TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay, 0)) TotalHoliDay, SUM(ISNULL(TotalWeekOffHoliDay, 0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(OTHr, 0)) TotalOTHr, 0.00 TotalNormalOTHr, 0.00 TotalExtraOTHr, PlantID   
                            FROM (SELECT EmpSystemID, WorkDate, PlantID,
			                            TotalPresent = CASE WHEN DayStatus = 'P' THEN 1 
                                                       WHEN DayStatus = 'WP' THEN 1 
						                               WHEN DayStatus = 'HP' THEN 1  
                                                       WHEN DayStatus = 'WHP' THEN 1 
						                               WHEN DayStatus = 'HWP' THEN 1  
                                                       ELSE 0 END,
			                            TotalLate = CASE WHEN DayStatus = 'L' THEN 1
                                                       WHEN DayStatus = 'WL' THEN 1 
						                               WHEN DayStatus = 'HL' THEN 1  
                                                       WHEN DayStatus = 'WHL' THEN 1 
						                               WHEN DayStatus = 'HWL' THEN 1  
                                                       ELSE 0 END,
			                            TotalAbsent = CASE WHEN DayStatus = 'A' THEN 1 ELSE 0 END,
			                            TotalLv = CASE WHEN DayStatus = 'LV' THEN 1 
						                               WHEN DayStatus = 'LVP' THEN 1 
						                               WHEN DayStatus = 'LVL' THEN 1
						                               WHEN DayStatus = 'WLV' THEN 1 
						                               WHEN DayStatus = 'HLV' THEN 1  
						                               WHEN DayStatus = 'WLVP' THEN 1 
						                               WHEN DayStatus = 'HLVP' THEN 1  
						                               WHEN DayStatus = 'WLVL' THEN 1 
						                               WHEN DayStatus = 'HLVL' THEN 1  
                                                       WHEN DayStatus = 'WHLV' THEN 1 
                                                       WHEN DayStatus = 'WHLVP' THEN 1 
                                                       WHEN DayStatus = 'WHLVL' THEN 1 
						                               WHEN DayStatus = 'HWLV' THEN 1  
						                               WHEN DayStatus = 'HWLVP' THEN 1  
						                               WHEN DayStatus = 'HWLVL' THEN 1  
						                               ELSE 0 END,
			                            TotalMLv = CASE WHEN DayStatus = 'MLV' THEN 1 
						                                WHEN DayStatus = 'MLVP' THEN 1 
						                                WHEN DayStatus = 'MLVL' THEN 1
						                                WHEN DayStatus = 'WMLV' THEN 1 
						                                WHEN DayStatus = 'HMLV' THEN 1  
						                                WHEN DayStatus = 'WMLVP' THEN 1 
						                                WHEN DayStatus = 'HMLVP' THEN 1  
						                                WHEN DayStatus = 'WMLVL' THEN 1 
						                                WHEN DayStatus = 'HMLVL' THEN 1  
                                                        WHEN DayStatus = 'WHMLV' THEN 1 
                                                        WHEN DayStatus = 'WHMLVP' THEN 1 
                                                        WHEN DayStatus = 'WHMLVL' THEN 1 
						                                WHEN DayStatus = 'HWMLV' THEN 1  
						                                WHEN DayStatus = 'HWMLVP' THEN 1  
						                                WHEN DayStatus = 'HWMLVL' THEN 1  
						                                ELSE 0 END,
			                            TotalWeekOff = CASE WHEN DayStatus = 'W' THEN 1 
						                                ELSE 0 END,
			                            TotalHoliDay = CASE WHEN DayStatus = 'H' THEN 1 
						                               ELSE 0 END,
                                        TotalWeekOffHoliDay = CASE WHEN DayStatus = 'WH' THEN 1 
							                            WHEN DayStatus = 'HW' THEN 1 
						                               ELSE 0 END,
                                        TotalCompAssignLv = CASE WHEN DayStatus = 'CAL' THEN 1
						                               ELSE 0 END,
                                        OTHr
	                             FROM dbo.AttdnProcessData 
                                WHERE WorkDate BETWEEN '" + sfrmDate + @"'
                                    AND '" + sToDate + @"'  
                                    AND (" + sEmpSystemID + @")) A
                            GROUP BY EmpSystemID, PlantID";

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

        public void GetSalaryRuleDayStatusOnlyShiftType(string sEmpInfo, string sAllSalaryID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {

                if (sAllSalaryID != "''")
                {
                    sAllSalaryID = " Where (" + sAllSalaryID + ") ";
                }
                else
                {
                    sAllSalaryID = "";
                }
                strSql = @"SELECT Att.EmpSystemID, COUNT(Att.DayStatus) DayStatus 
                                FROM 
			                        (
			                         SELECT * FROM 
						                        (
							                     SELECT SD.SystemID AS SlrInfoDefSystemID, SLM.EmpInfoSystemID, SLM.SalaryRuleMasterSystemID, 
								                        SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, 
								                        SRDSM.LeaveType, SRDSST.ShiftDefinitionID 
							                     FROM 
													 (
													  SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
															 AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
													  FROM SalaryInfoDefine
													 ) SD
								                    INNER JOIN 
															(
															 SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
															 FROM SalaryInfoDefineMaster
															) SLM ON SLM.SystemID = SD.SalaryID
								                    INNER JOIN SalaryRuleMaster SRM ON SLM.SalaryRuleMasterSystemID = SRM.SystemID
								                    INNER JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SRDSM.SalaryHeadID
								                    INNER JOIN SalaryRuleDayStatusShiftType SRDSST ON SRDSM.SalaryRuleDayStatusSystemID = SRDSST.SalaryRuleDayStatusSystemID
												
													" + sAllSalaryID + @"
												 ) A WHERE (" + sEmpInfo + @")
						            ) A INNER JOIN (
				                                    SELECT * FROM [dbo].[AttdnProcessData] 
									                        WHERE WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
						                           ) Att ON A.EmpInfoSystemID = Att.EmpSystemID AND A.ShiftDefinitionID = Att.ShiftSystemID
                        GROUP BY Att.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetSalaryRuleDayStatusOnlyDayType(string sEmpInfo, string sAllSalaryID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT Att.EmpSystemID, COUNT(Att.DayStatus) DayStatus FROM 
			                            (
			                             SELECT * FROM 
						                            (
							                        SELECT SD.SystemID AS SlrInfoDefSystemID, SLM.EmpInfoSystemID, SLM.SalaryRuleMasterSystemID, 
								                            SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
								                            SRDSDT.DayType DayStatus  
							                        FROM (
														  SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
																 AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
														  FROM SalaryInfoDefine
														) SD
								                        INNER JOIN 
															    (
															     SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
															     FROM SalaryInfoDefineMaster
															    ) SLM ON SLM.SystemID = SD.SalaryID
								                                INNER JOIN SalaryRuleMaster SRM ON SLM.SalaryRuleMasterSystemID = SRM.SystemID
								                                INNER JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
																                                AND SD.SalaryHeadID = SRDSM.SalaryHeadID
								                                INNER JOIN SalaryRuleDayStatusDayType SRDSDT ON SRDSM.SalaryRuleDayStatusSystemID = SRDSDT.SalaryRuleDayStatusSystemID
								                      WHERE (" + sAllSalaryID + @")
						                            ) A WHERE (" + sEmpInfo + @")
			                            ) A
				                            INNER JOIN (
				                                        SELECT * FROM [dbo].[AttdnProcessData] 
									                            WHERE WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
						                               ) Att ON A.EmpInfoSystemID = Att.EmpSystemID AND A.DayStatus = Att.DayStatus
                            GROUP BY Att.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetSalaryRuleDayStatusOnlyLeaveType(string sEmpInfo, string sAllSalaryID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT Att.EmpSystemID, COUNT(Att.DayStatus) DayStatus FROM 
			                            (
			                             SELECT * FROM 
													(
						                             SELECT SD.SystemID AS SlrInfoDefSystemID, SLM.EmpInfoSystemID, SLM.SalaryRuleMasterSystemID, 
															SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
															SRDSLT.LeaveTypeID, SRDSLT.IsPostApplied
						                             FROM (
														   SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
																  AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
														   FROM SalaryInfoDefine
															--UNION
															--(
															--SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
															--		AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated                   
															--FROM SalaryInfoBack
															--)
														  ) SD
								                    INNER JOIN 
															(
															 SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
															 FROM SalaryInfoDefineMaster
															--  UNION 
															-- (
															--  SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
															--		 IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
															--  FROM SalaryInfoBackMaster
															-- )
															) SLM ON SLM.SystemID = SD.SalaryID
													INNER JOIN SalaryRuleMaster SRM ON SLM.SalaryRuleMasterSystemID = SRM.SystemID
													INNER JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
																					AND SD.SalaryHeadID = SRDSM.SalaryHeadID
													INNER JOIN SalaryRuleDayStatusLeaveType SRDSLT ON SRDSM.SalaryRuleDayStatusSystemID = SRDSLT.SalaryRuleDayStatusSystemID
						                            WHERE (" + sAllSalaryID + @")
													) A WHERE (" + sEmpInfo + @")
						                        ) A
				                        INNER JOIN 
						                        (
				                                    SELECT APD.*, L.EmpInfoSystemID, L.IsPostApplied FROM [dbo].[AttdnProcessData] APD
						                            INNER JOIN (
									                            SELECT LT.EmpSystemID EmpInfoSystemID, LT.LTSystemID, LT.IsPostApplied, LTD.WorkDate, LTD.DayType, LTD.LeaveStatus
									                            FROM [dbo].[LeaveTransaction] LT 
												                            INNER JOIN [dbo].[LeaveTransactionDetails] LTD ON LT.SystemID = LTD.LvTrnsSystemID AND LTD.IsAvailed = 1
									                            WHERE LT.ComAssignLvSystemID IS NULL
									                        ) L ON APD.WorkDate = L.WorkDate AND APD.LTSystemID = L.LTSystemID
								                        WHERE APD.WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
                                                              AND (" + sEmpInfo + @")
						                        ) Att ON A.EmpInfoSystemID = Att.EmpSystemID AND A.LeaveTypeID = Att.LTSystemID 
								                            AND A.IsPostApplied = Att.IsPostApplied
                        GROUP BY Att.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetSalaryRuleDayStatusShiftTypeDayType(string sEmpInfo, string sAllSalaryID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT Att.EmpSystemID, COUNT(Att.DayStatus) DayStatus FROM 
			                            (
			                             SELECT * FROM 
						                           (
							                        SELECT SD.SystemID AS SlrInfoDefSystemID, SLM.EmpInfoSystemID, SLM.SalaryRuleMasterSystemID, 
								                           SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
								                           SRDSST.ShiftDefinitionID, SRDSDT.DayType DayStatus  
							                        FROM (
														   SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
																  AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
														   FROM SalaryInfoDefine
															
														  ) SD
														INNER JOIN 
																(
																 SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																 FROM SalaryInfoDefineMaster
																
																) SLM ON SLM.SystemID = SD.SalaryID
								                        INNER JOIN SalaryRuleMaster SRM ON SLM.SalaryRuleMasterSystemID = SRM.SystemID
								                        INNER JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
																                        AND SD.SalaryHeadID = SRDSM.SalaryHeadID
								                        INNER JOIN SalaryRuleDayStatusShiftType SRDSST ON SRDSM.SalaryRuleDayStatusSystemID = SRDSST.SalaryRuleDayStatusSystemID
								                        INNER JOIN SalaryRuleDayStatusDayType SRDSDT ON SRDSM.SalaryRuleDayStatusSystemID = SRDSDT.SalaryRuleDayStatusSystemID
								                     WHERE (" + sAllSalaryID + @")
						                        ) A WHERE (" + sEmpInfo + @")
			                        ) A
				                        INNER JOIN (
				                                    SELECT * FROM [dbo].[AttdnProcessData] 
									                        WHERE WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
						                           ) Att ON A.EmpInfoSystemID = Att.EmpSystemID AND A.ShiftDefinitionID = Att.ShiftSystemID
									                        AND A.DayStatus = Att.DayStatus
                        GROUP BY Att.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetSalaryRuleDayStatusOnlyShiftTypeLeaveType(string sEmpInfo, string sAllSalaryID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT Att.EmpSystemID, COUNT(Att.DayStatus) DayStatus FROM 
			                        (
			                         SELECT * FROM 
						                        (
						                         SELECT SD.SystemID AS SlrInfoDefSystemID, SLM.EmpInfoSystemID, SLM.SalaryRuleMasterSystemID, 
								                        SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
								                        SRDSST.ShiftDefinitionID, SRDSLT.LeaveTypeID, SRDSLT.IsPostApplied
						                         FROM (
														SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
																AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
														FROM SalaryInfoDefine
														--UNION
														--(
														--SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
														--		AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated                   
														--FROM SalaryInfoBack
														--)
													) SD
														INNER JOIN 
																(
																 SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																 FROM SalaryInfoDefineMaster
																--  UNION 
																-- (
																--  SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																--		 IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																--  FROM SalaryInfoBackMaster
																-- )
																) SLM ON SLM.SystemID = SD.SalaryID
								                        INNER JOIN SalaryRuleMaster SRM ON SLM.SalaryRuleMasterSystemID = SRM.SystemID
								                        INNER JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
																                        AND SD.SalaryHeadID = SRDSM.SalaryHeadID
								                        INNER JOIN SalaryRuleDayStatusShiftType SRDSST ON SRDSM.SalaryRuleDayStatusSystemID = SRDSST.SalaryRuleDayStatusSystemID
								                        INNER JOIN SalaryRuleDayStatusLeaveType SRDSLT ON SRDSM.SalaryRuleDayStatusSystemID = SRDSLT.SalaryRuleDayStatusSystemID
						                         WHERE (" + sAllSalaryID + @")
												 ) A WHERE (" + sEmpInfo + @")
						                        ) A
				                        INNER JOIN 
						                        (
				                                 SELECT APD.*, L.EmpInfoSystemID, L.IsPostApplied FROM [dbo].[AttdnProcessData] APD
						                         INNER JOIN (
									                         SELECT LT.EmpSystemID EmpInfoSystemID, LT.LTSystemID, LT.IsPostApplied, LTD.WorkDate, LTD.DayType, LTD.LeaveStatus
									                           FROM [dbo].[LeaveTransaction] LT 
												                         INNER JOIN [dbo].[LeaveTransactionDetails] LTD ON LT.SystemID = LTD.LvTrnsSystemID AND LTD.IsAvailed = 1
									                           WHERE LT.ComAssignLvSystemID IS NULL
									                        ) L ON APD.WorkDate = L.WorkDate AND APD.LTSystemID = L.LTSystemID
								                        WHERE APD.WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
                                                              AND (" + sEmpInfo + @")
						                        ) Att ON A.EmpInfoSystemID = Att.EmpSystemID AND A.ShiftDefinitionID = Att.ShiftSystemID
								                         AND A.LeaveTypeID = Att.LTSystemID AND A.IsPostApplied = Att.IsPostApplied
                        GROUP BY Att.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetSalaryRuleDayStatusOnlyDayTypeLeaveType(string sEmpInfo, string sAllSalaryID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT Att.EmpSystemID, COUNT(Att.DayStatus) DayStatus FROM 
			                        (
			                         SELECT * FROM 
						                        (
						                         SELECT SD.SystemID AS SlrInfoDefSystemID, SLM.EmpInfoSystemID, SLM.SalaryRuleMasterSystemID, 
								                        SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
								                        SRDSDT.DayType DayStatus, SRDSLT.LeaveTypeID, SRDSLT.IsPostApplied
						                         FROM (
														SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
																AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
														FROM SalaryInfoDefine
													--	UNION
													--	(
													--	SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
													--			AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated                   
													--	FROM SalaryInfoBack
													--	)
													) SD
														INNER JOIN 
																(
																 SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																 FROM SalaryInfoDefineMaster
																--  UNION 
																-- (
																--  SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																--		 IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																--  FROM SalaryInfoBackMaster
																-- )
																) SLM ON SLM.SystemID = SD.SalaryID
								                        INNER JOIN SalaryRuleMaster SRM ON SLM.SalaryRuleMasterSystemID = SRM.SystemID
								                        INNER JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
																                        AND SD.SalaryHeadID = SRDSM.SalaryHeadID
								                        INNER JOIN SalaryRuleDayStatusDayType SRDSDT ON SRDSM.SalaryRuleDayStatusSystemID = SRDSDT.SalaryRuleDayStatusSystemID
								                        INNER JOIN SalaryRuleDayStatusLeaveType SRDSLT ON SRDSM.SalaryRuleDayStatusSystemID = SRDSLT.SalaryRuleDayStatusSystemID
						                         WHERE (" + sAllSalaryID + @")
												) A WHERE (" + sEmpInfo + @")
						                ) A
				                        INNER JOIN 
						                        (
				                                 SELECT APD.*, L.EmpInfoSystemID, L.IsPostApplied FROM [dbo].[AttdnProcessData] APD
						                         INNER JOIN (
									                         SELECT LT.EmpSystemID EmpInfoSystemID, LT.LTSystemID, LT.IsPostApplied, LTD.WorkDate, LTD.DayType, LTD.LeaveStatus
									                           FROM [dbo].[LeaveTransaction] LT 
												                         INNER JOIN [dbo].[LeaveTransactionDetails] LTD ON LT.SystemID = LTD.LvTrnsSystemID AND LTD.IsAvailed = 1
									                           WHERE LT.ComAssignLvSystemID IS NULL
									                        ) L ON APD.WorkDate = L.WorkDate AND APD.LTSystemID = L.LTSystemID
								                        WHERE APD.WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
                                                              AND (" + sEmpInfo + @")
						                        ) Att ON A.EmpInfoSystemID = Att.EmpSystemID AND A.DayStatus = Att.DayStatus
								                         AND A.LeaveTypeID = Att.LTSystemID AND A.IsPostApplied = Att.IsPostApplied
                        GROUP BY Att.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetLoanAdvanceMonthly(string sPlantID, string sEmpInfo, int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM (SELECT LAM.EmpInfoSystemID, E.PlantID, LAM.SystemID AS MSTSystemID, LAC.SystemID AS CHDSystemID, 
                                LAM.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, LAM.EntryCurrencyID, ECR.Code AS EntryCurrency, LAM.AdvanceAmount, 
                                LAM.DefineCurrencyID AS DefinitionCurrencyID, DECR.Code AS DefinitionCurrency, LAM.DefineAmount,
                                LAM.DisbustCurrencyID, DICR.Code AS DisbustCurrency, LAC.MonthlyAdjAmount, 
                                AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
                                                            THEN CRC.AccumulateExchangeSalaryHeadID
                                                         ELSE LAM.SalaryHeadID END, LAM.DefineCurrencyID AmtDefinitionCurrencyID, LAM.DefineAmount AmtDefinitionRate,
                                LAM.FromMonthNo, LAM.FromYearNo, LAM.PaidAmount, LAM.InterestPercentageAmount, 
                                LAM.InstallmentAmount, LAM.InstallmentMonth, LAC.MonthNo, LAC.YearNo,  
                                LAC.PaidAmount AS WithThisMonthPaidAmt, LAC.BalanceAmount AS AftThisMonthBalAmt, LAC.IsDisbusted, 
                                CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
                            FROM LoanAdvanceMaster LAM
	                                INNER JOIN EmployeeInformation E ON LAM.EmpInfoSystemID = E.SystemID
                                    INNER JOIN LoanAdvanceChild LAC ON LAM.SystemID = LAC.LoanMstSystemID 
                                    INNER JOIN SalaryHead SH ON LAM.SalaryHeadID = SH.SalaryHeadID 
                                    INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
	                                INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID 
					                                AND LAM.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON LAM.EntryCurrencyID = ECR.Id
                                    LEFT JOIN scs.Currency DECR ON LAM.DefineCurrencyID = DECR.Id
                                    LEFT JOIN scs.Currency DICR ON LAM.DisbustCurrencyID = DICR.Id) A
                                WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @" AND (" + sEmpInfo + @")";

                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSql = strSql + " AND PlantID = '" + sPlantID + @"'";
                }

                strSql = strSql + " ORDER BY PlantID, EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void xGetMonthWiseExtraSalaryAmt(string sPlantID, string sEmpInfo, int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM (SELECT MESAM.EmpInfoSystemID, MESAM.PlantID, MESAM.SystemID AS MSTSystemID, MESAC.SystemID AS CHDSystemID, 
                                            MESAC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, MESAC.EntryCurrencyID, ECR.Code AS EntryCurrency, 
                                            MESAC.DefineCurrencyID AS DefinitionCurrencyID, DECR.Code AS DefinitionCurrency, MESAC.DefineAmount,
                                            CRC.AmtDisbusmentCurrency AS DisbustCurrencyID, DICR.Code AS DisbustCurrency,  
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
                                                                        THEN CRC.AccumulateExchangeSalaryHeadID
                                                                     ELSE MESAC.SalaryHeadID END, MESAC.AmtDefinitionCurrencyID, MESAC.DefineAmount AmtDefinitionRate,
                                            MESAM.MonthNo, MESAM.YearNo,  MESAM.IsDisbusted, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo 
                                            FROM MonthWiseExtraSalaryAmtMaster MESAM
                                                INNER JOIN EmployeeInformation E ON MESAM.EmpInfoSystemID = E.SystemID
                                                INNER JOIN MonthWiseExtraSalaryAmtChild MESAC ON MESAM.SystemID = MESAC.MWESAMasterSystemID 
                                                INNER JOIN SalaryHead SH ON MESAC.SalaryHeadID = SH.SalaryHeadID 
                                                INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID 
                                                                AND MESAC.SalaryHeadID = CRC.SalaryHeadID
                                                LEFT JOIN scs.Currency ECR ON MESAC.EntryCurrencyID = ECR.Id
                                                LEFT JOIN scs.Currency DECR ON MESAC.DefineCurrencyID = DECR.Id
                                                LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id) A
                                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + " AND (" + sEmpInfo + @")";

                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSql = strSql + " AND PlantID = '" + sPlantID + @"'";
                }

                strSql = strSql + " ORDER BY PlantID, EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetMonthWiseExtraSalaryAmt(string sEmpInfo, int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM (SELECT MESAM.EmpInfoSystemID, MESAM.PlantID, MESAM.SystemID AS MSTSystemID, MESAC.SystemID AS CHDSystemID, 
                                            MESAC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, MESAC.EntryCurrencyID, ECR.Code AS EntryCurrency, 
                                            MESAC.DefineCurrencyID AS DefinitionCurrencyID, DECR.Code AS DefinitionCurrency, MESAC.DefineAmount,
                                            CRC.AmtDisbusmentCurrency AS DisbustCurrencyID, DICR.Code AS DisbustCurrency,  
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
                                                                        THEN CRC.AccumulateExchangeSalaryHeadID
                                                                     ELSE MESAC.SalaryHeadID END, MESAC.AmtDefinitionCurrencyID, MESAC.DefineAmount AmtDefinitionRate,
                                            MESAM.MonthNo, MESAM.YearNo,  MESAM.IsDisbusted, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo 
                                            FROM (select * from MonthWiseExtraSalaryAmtMaster WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @" AND EmpInfoSystemID in (" + sEmpInfo + @")) MESAM
                                                left JOIN (select SystemID,SalaryRuleMasterSystemID from EmployeeInformation where systemid in (" + sEmpInfo + @")) E ON MESAM.EmpInfoSystemID = E.SystemID
                                                INNER JOIN MonthWiseExtraSalaryAmtChild MESAC ON MESAM.SystemID = MESAC.MWESAMasterSystemID 
                                                INNER JOIN SalaryHead SH ON MESAC.SalaryHeadID = SH.SalaryHeadID 
                                                INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID 
                                                                AND MESAC.SalaryHeadID = CRC.SalaryHeadID
                                                LEFT JOIN scs.Currency ECR ON MESAC.EntryCurrencyID = ECR.Id
                                                LEFT JOIN scs.Currency DECR ON MESAC.DefineCurrencyID = DECR.Id
                                                LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id) A
                                             ";

                //if (sPlantID != "ALL" & sPlantID != "")
                //{
                //    strSql = strSql + " AND PlantID = '" + sPlantID + @"'";
                //}

                strSql = strSql + " ORDER BY PlantID, EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetPaymentModeWiseHeadAmount(string sPlantID, string CompanyGroupId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM (SELECT
                                            p.Id AS CHDSystemID, sm.SystemID SalaryRuleMasterSystemId,p.PaymentMode,p.PlantId,p.Amount,
                                            p.SalaryHeadId SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, CRC.AmtEntryCurrency EntryCurrencyID, ECR.Code AS EntryCurrency, 
                                            CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.Code AS DefinitionCurrency,
                                            CRC.AmtDisbusmentCurrency AS DisbustCurrencyID, DICR.Code AS DisbustCurrency,  
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
                                                                        THEN CRC.AccumulateExchangeSalaryHeadID
                                                                     ELSE p.SalaryHeadID END, CRC.AmtDefinitionCurrency AmtDefinitionCurrencyID,
                                                --p.Amount AmtDefinitionRate,
                                                convert(decimal,1) AmtDefinitionRate,
                                             CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo 
                                            FROM  [SalaryHeadWisePaymentModePolicy] p  
                                                INNER JOIN SalaryHead SH ON p.SalaryHeadId = SH.SalaryHeadID
                                                INNER JOIN CurrencyRuleChild CRC ON p.SalaryHeadId = CRC.SalaryHeadId   
												left join SalaryRuleMaster sm on sm.CurrencyRuleSystemID=crc.MstSystemID                                                             
                                                LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
                                                LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency  = DECR.Id
                                                LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
												  WHERE p.PlantId='" + sPlantID + @"' 
												) A";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmpIncomeTax(string sPlantID, string LocalCurID, string sEmpInfo, int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            //if HeadCategory = 'Tax' it will give result
            try
            {
                //strSql = @"SELECT * FROM 
                //              (SELECT E.SystemID EmpInfoSystemID, E.PlantID, TAX.MonthlyTaxSystemID, TAX.TaxDefineMasterSystemID, 
                //                SH.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, CRC.AmtEntryCurrency EntryCurrencyID, 
                //                ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency DefinitionCurrencyID, 
                //                DECR.CurrencyDesc AS DefinitionCurrency, CRC.AmtDisbusmentCurrency DisbustCurrencyID, 
                //                DICR.CurrencyDesc AS DisbustCurrency, TAX.ActualTaxAmount, Tax.DefinitionCurrencyID AS AmtDefinitionCurrencyID, 
                //                            Tax.AmtDefinitionRate, 
                //                AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
                //                          THEN CRC.AccumulateExchangeSalaryHeadID
                //                          ELSE SH.SalaryHeadID END  
                //               FROM EmployeeInformation E
                //                 INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                //                 INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID
                //                 INNER JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID AND SH.HeadCategory = 'Tax'
                //                 LEFT JOIN Currency ECR ON CRC.AmtEntryCurrency = ECR.CurrencyCode
                //                 LEFT JOIN Currency DECR ON CRC.AmtDefinitionCurrency = DECR.CurrencyCode
                //                 LEFT JOIN Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.CurrencyCode
                //                 INNER JOIN (SELECT TaxMon.SystemID MonthlyTaxSystemID, TaxMon.EmpInfoSystemID, TaxMon.TaxDefineMasterSystemID, 
                //                      TaxMon.TaxPolicyMstID, TaxMon.TaxGroupID, TaxMon.TaxPeriodSystemID, 
                //                      TaxMon.TaxPayablePeriod, TaxMon.ActualTaxAmount, TaxSH.DefinitionCurrencyID, TaxSH.AmtDefinitionRate
                //                    FROM TaxDeductionInfoMonthWise TaxMon
                //                       INNER JOIN (SELECT EmpInfoSystemID, TaxDefineMasterSystemID, DefinitionCurrencyID, 
                //                          MAX(ConvertionRate) AmtDefinitionRate FROM TaxableIncomeSalaryHeadWise
                //                            WHERE DefinitionCurrencyID = '" + LocalCurID + @"'
                //                           GROUP BY EmpInfoSystemID, TaxDefineMasterSystemID, DefinitionCurrencyID) TaxSH 
                //                      ON TaxMon.TaxDefineMasterSystemID = TaxSH.TaxDefineMasterSystemID
                //			                            AND TaxMon.EmpInfoSystemID = TaxSH.EmpInfoSystemID
                //                       INNER JOIN (
                //				  SELECT * FROM TaxYearPeriod 
                //				  WHERE SystemID IN (
                //									 SELECT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment 
                //									 WHERE CompanyID IN (
                //														 SELECT CompanyID FROM PlantAndCompanyAssignment 
                //														  WHERE PlantID = '" + sPlantID + @"'
                //														 )
                //									)
                //	            ) FWP ON TaxMon.TaxPeriodSystemID = FWP.SystemID AND MONTH(FWP.StartDate) = " + intMonthNo + @" 
                //                                                                                AND YEAR(FWP.StartDate) = " + intYearNo + @") TAX
                //                 ON E.SystemID = EmpInfoSystemID) A
                //                WHERE (" + sEmpInfo + @")";

                strSql = @"SELECT * FROM 
		        (SELECT E.SystemID EmpInfoSystemID, E.PlantID, TAX.MonthlyTaxSystemID, TAX.TaxDefineMasterSystemID, 
				        SH.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, CRC.AmtEntryCurrency EntryCurrencyID, 
				        ECR.Name AS EntryCurrency, CRC.AmtDefinitionCurrency DefinitionCurrencyID, 
				        DECR.Name AS DefinitionCurrency, CRC.AmtDisbusmentCurrency DisbustCurrencyID, 
				        DICR.Name AS DisbustCurrency, TAX.ActualTaxAmount, Tax.DefinitionCurrencyID AS AmtDefinitionCurrencyID, 
                        Tax.AmtDefinitionRate, 
				        AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
														        THEN CRC.AccumulateExchangeSalaryHeadID
													            ELSE SH.SalaryHeadID END  
			        FROM EmployeeInformation E
					        INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
					        INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID
					        INNER JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID AND SH.HeadCategory = 'Tax'
					        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
					        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
					        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        INNER JOIN (SELECT TaxMon.SystemID MonthlyTaxSystemID, TaxMon.EmpInfoSystemID, TaxMon.TaxDefineMasterSystemID, 
										        TaxMon.TaxPolicyMstID, TaxMon.TaxGroupID, TaxMon.TaxPeriodSystemID, 
										        TaxMon.TaxPayablePeriod, TaxMon.ActualTaxAmount, TaxSH.DefinitionCurrencyID, TaxSH.AmtDefinitionRate
								        FROM TaxDeductionInfoMonthWise TaxMon
										            INNER JOIN (SELECT EmpInfoSystemID, TaxDefineMasterSystemID, DefinitionCurrencyID, 
														        MAX(ConvertionRate) AmtDefinitionRate FROM TaxableIncomeSalaryHeadWise
															            WHERE DefinitionCurrencyID = '" + LocalCurID + @"'
															        GROUP BY EmpInfoSystemID, TaxDefineMasterSystemID, DefinitionCurrencyID) TaxSH 
										        ON TaxMon.TaxDefineMasterSystemID = TaxSH.TaxDefineMasterSystemID
																			        AND TaxMon.EmpInfoSystemID = TaxSH.EmpInfoSystemID
										            INNER JOIN (
																SELECT * FROM scs.TaxYearPeriod 
																WHERE ID IN (																					
																					select p.Id from [SCS].[CompanyTaxYearPeriod] cp
																						left outer join [SCS].[TaxYearPeriod] p on p.Id=cp.TaxYearPeriodId
																						left outer join [SCS].[CompanyTaxYear] cy on cy.Id=cp.CompanyTaxYearId
																					WHERE cy.CompanyID IN (
																										SELECT CompanyID FROM org.Plant 
																										WHERE Id = '" + sPlantID + @"'
																										)
																				)
															) FWP ON TaxMon.TaxPeriodSystemID = FWP.Id AND MONTH(FWP.StartDate) = " + intMonthNo + @" 
                                                                            AND YEAR(FWP.StartDate) = " + intYearNo + @") TAX
					        ON E.SystemID = EmpInfoSystemID) A
            WHERE (" + sEmpInfo + @")";

                strSql = strSql + @" 
                                    ORDER BY PlantID, EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void DeleteSlrProcChild(int intMonthNo, int intYearNo, string sEmpInfo)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            //string strSql = "";

            try
            {
                //                strSql = @"SELECT * FROM SalaryProcChild WHERE MonthNo = " + intMonthNo + @" 
                //                                AND YearNo = " + intYearNo + @" AND IsDisbursed = 0 AND (" + strEmp + @")";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM SalaryProcChild WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                                                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @")
                                                                                  AND IsApproved = 0 AND IsDisbursed = 0 
                                                                                  AND (" + sEmpInfo + @")", true, "1");

                objCon.ExecuteNonQueryWrapper(@"DELETE FROM SalaryProcChildTemp WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                                                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @")
                                                                                  AND (" + sEmpInfo + @")", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                //throw (ex);
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }

                throw (ex);
            }
            finally
            {
                //objCon = null;
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void DeleteArrearProcChild(int intMonthNo, int intYearNo, string sEmpInfo)
        {
            ConnectionManager.DAL.ConManager objCon = null;

            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM ArrearProcChild WHERE SlrProcMstSystemID IN (SELECT SystemID FROM ArrearProcMaster 
                                                                                                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @")
                                                                                  AND IsApproved = 0 AND IsDisbursed = 0 
                                                                                  AND (" + sEmpInfo + @")", true, "1");

                objCon.ExecuteNonQueryWrapper(@"DELETE FROM SalaryProcChildTemp WHERE SlrProcMstSystemID IN (SELECT SystemID FROM ArrearProcMaster 
                                                                                                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @")
                                                                                  AND (" + sEmpInfo + @")", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                //throw (ex);
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }

                throw (ex);
            }
            finally
            {
                //objCon = null;
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void DeleteCarryForwardSalary(int intMonthNo, int intYearNo, string sEmpInfo)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            //string strSql = "";

            try
            {
                //                strSql = @"SELECT * FROM SalaryProcChild WHERE MonthNo = " + intMonthNo + @" 
                //                                AND YearNo = " + intYearNo + @" AND IsDisbursed = 0 AND (" + strEmp + @")";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM CarryForwardSalary WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                                                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @")
                                                                                  AND IsApproved = 0 AND IsDisbursed = 0 
                                                                                  AND (" + sEmpInfo + @")", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                //throw (ex);
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }

                throw (ex);
            }
            finally
            {
                //objCon = null;
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void GetSlrProcChild(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryProcChild 
                                WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                    WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @") 
                                      AND IsApproved = 0 AND IsDisbursed = 0 
                                      AND (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetArrearProcChild(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM ArrearProcChild 
                                WHERE SlrProcMstSystemID IN (SELECT SystemID FROM ArrearProcMaster 
                                                                    WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @") 
                                      AND IsApproved = 0 AND IsDisbursed = 0 
                                      AND (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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


        public void GetCarryForwardSalary(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM CarryForwardSalary 
                                WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                    WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @") 
                                      AND IsApproved = 0 AND IsDisbursed = 0 
                                      AND (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetCarryForwardSalary(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM CarryForwardSalary 
                                WHERE SystemID ='' ";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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


        public void GetSlrProcChild(int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryProcChild 
                                WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                    WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @") 
                                      AND IsApproved = 0 AND IsDisbursed = 0  ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetSlrProcChild(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryProcChildTemp 
                                WHERE SystemID ='' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetArrearProcChild(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM ArrearProcChildtemp 
                                WHERE SystemID ='' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetSalarySetting(string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT  *
                            FROM PlantWiseHRMSSetting
                                WHERE plantid='" + plantid + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetMaxToDate(string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"                           
                            select max(ToDate) ToDate,EmpInfoSystemID SystemId,e.EmployeeName,EmployeeCode from 
                            (
                            select distinct m.SystemID,m.FromDate,m.ToDate,c.EmpInfoSystemID from SalaryProcMaster m
                            left outer join SalaryProcChild c on m.SystemID=c.SlrProcMstSystemID
                            where c.PlantID='" + plantid + @"'
                            ) x
                            left outer join (select * from EmployeeInformation where PlantId='" + plantid + @"' ) e on x.EmpInfoSystemID=e.SystemId
                            group by EmpInfoSystemID,e.EmployeeName,EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetAlreadyProcessedMaster(int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryProcMaster WHERE MonthNo = " + intMonthNo + @" 
                                AND YearNo = " + intYearNo + @" ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetSlrProcChildDisbursed(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryProcChild 
                                WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                    WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @") 
                                      AND IsApproved = 1 AND IsDisbursed = 0 
                                      AND (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetCompanyOffDay(string sPlantID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM [scs].[OffDayDetail]  
                            WHERE OffDayDate BETWEEN '" + sFromDate + @"' 
                                  AND '" + sToDate + "' AND PlantID = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetCompanyWeekOffDay(string sPlantID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT B.* FROM [scs].[OffDayDetail] B 
								INNER JOIN [scs].[OffDayMaster] A ON B.OffDayMasterId = A.Id AND A.OffDayType = 'W'
                            WHERE B.OffDayDate BETWEEN '" + sFromDate + @"' 
                                  AND '" + sToDate + "' AND B.PlantID = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetLocalCurrencyForSlrProc(string sPlantID, string sGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSQL = @"SELECT F.LocalCurrency, C.CurrencyDesc AS Currency FROM Factory F 
                                LEFT JOIN Currency C ON F.LocalCurrency= C.CurrencyCode
                                WHERE  F.PlantID = '" + sPlantID + @"'";
                }
                else
                {
                    strSQL = @"SELECT TOP(1) F.LocalCurrency, C.CurrencyDesc AS Currency FROM Factory F 
                                LEFT JOIN Currency C ON F.LocalCurrency= C.CurrencyCode
                                WHERE F.PlantID IN (SELECT DISTINCT PlantID FROM EmployeeInformation 
                                WHERE UserGroupSystemID = '" + sGroupID + @"')";
                }

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
        public void LoadSalaryRuleInfo(string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SM.SystemID SalaryRuleMasterSystemID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.AmtEntryCurrency,
		                            CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency FROM SalaryRuleMaster SM
	                            INNER JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                            WHERE SM.PlantID = '" + sPlantID + @"'";

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
        public void GetCurrencyInfo(string sCurrencyID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EWER.CurrencyCode, CR.CurrencyDesc, EWER.ExchangeRate, EWER.BuyingRate, EWER.SellingRate 
	                            FROM EntityWiseExchangeRate EWER
		                            INNER JOIN Currency CR ON EWER.CurrencyCode = CR.CurrencyCode
	                            WHERE EWER.CurrencyCode = '" + sCurrencyID + @"'";

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
        public void GetSalaryHeadTobeExcluded(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM SalaryRuleGeneral g
									   LEFT OUTER JOIN SalaryHead h ON h.SalaryHeadID = g.SalaryHeadID
									   WHERE --SalaryRuleMasterSystemID='SRSI-2018-3'
									    --AND
											( --2
											(IsFixedDisbus = 1 AND IsGNRNetPayEffect = 1)											
											)--2
											";

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
        public void xGetEntityCurrencyRateInfo(string sCurrencyID, string sPlantID, string sFromDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.SystemID, A.FromCurrencyUnit, A.FromCurrencyCode, FR.CurrencyDesc FromCurrencyDesc, A.ToCurrencyBuying, A.ToCurrencySelling, 
	                               A.ToCurrencyCode, LR.CurrencyDesc ToCurrencyDesc, A.FromDate 
                            FROM [dbo].[ExchangerateDateWiseForHR] A
			                            LEFT JOIN Currency FR ON A.FromCurrencyCode = FR.CurrencyCode
			                            LEFT JOIN Currency LR ON A.ToCurrencyCode = LR.CurrencyCode
                            WHERE PlantID = '" + sPlantID + @"'
                            GROUP BY A.SystemID, A.FromCurrencyUnit, A.FromCurrencyCode, FR.CurrencyDesc, A.ToCurrencyBuying, A.ToCurrencySelling, 
	                               A.ToCurrencyCode, LR.CurrencyDesc, A.FromDate 
                            Having Max(A.FromDate) <= '" + sFromDate + @"' ";

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
        public void GetSlrProcChildEmpWise(string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM [dbo].[SalaryProcMaster] 
                                WHERE SystemID IN (
                                                    SELECT SlrProcMstSystemID FROM [dbo].[SalaryProcChild] 
                                                    WHERE EmpInfoSystemID = '" + sEmpInfo + @"'
                                                  )";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetTaxDeductionInfoMonthWise(string sPlantID, string sEmpInfo, int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise 
                //            WHERE TaxPeriodSystemID IN (SELECT DISTINCT SystemID FROM TaxYearPeriod 
                //                                     WHERE SystemID IN (
                //                                          SELECT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment 
                //                                          WHERE CompanyID IN (
                //                                               SELECT CompanyID FROM PlantAndCompanyAssignment 
                //                                               WHERE PlantID = '" + sPlantID + @"'
                //                                               )
                //                                         )
                //    AND MONTH(StartDate) = " + intMonthNo + @" AND YEAR(StartDate) = " + intYearNo + @") AND (" + sEmpInfo + @")";

                strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise 
                            WHERE TaxPeriodSystemID IN 
							(
							SELECT DISTINCT Id FROM scs.TaxYearPeriod 
	                                                    WHERE ID IN (																					
																					select p.Id from [SCS].[CompanyTaxYearPeriod] cp
																						left outer join [SCS].[TaxYearPeriod] p on p.Id=cp.TaxYearPeriodId
																						left outer join [SCS].[CompanyTaxYear] cy on cy.Id=cp.CompanyTaxYearId
																					WHERE cy.CompanyID IN (
																										SELECT CompanyID FROM org.Plant 
																										WHERE Id = '" + sPlantID + @"'
																										)
																				)
														AND MONTH(StartDate) =  " + intMonthNo + @" AND YEAR(StartDate) = " + intYearNo + @"
						  ) AND (" + sEmpInfo + @")";

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
        public void GetTaxDefineMaster(string sPlantID, string sEmp, int intMonthNo, int intYearNo, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //    strSQL = @"SELECT A.* FROM TaxDefineMaster A
                //                        INNER JOIN (
                //                                    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate
                //                                     FROM TaxDefineMaster 
                //                                     WHERE EffectiveDate <= '" + sDate + @"'
                //                                           AND (" + sEmp + @")
                //GROUP BY EmpInfoSystemID
                //                                   ) B ON A.EmpInfoSystemID = B.EmpInfoSystemID AND A.EffectiveDate = B.EffectiveDate
                //                    WHERE A.SystemID IN (
                //                                         SELECT TaxDefineMasterSystemID FROM TaxDeductionInfoMonthWise WHERE  
                //                                            TaxPeriodSystemID IN (SELECT DISTINCT SystemID FROM TaxYearPeriod 
                //                                         WHERE SystemID IN (
                //                                              SELECT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment 
                //                                              WHERE CompanyID IN (
                //                                                   SELECT CompanyID FROM PlantAndCompanyAssignment 
                //                                                   WHERE PlantID = '" + sPlantID + @"'
                //                                                   )
                //                                               )
                //                                            AND MONTH(StartDate) = " + intMonthNo + @" AND YEAR(StartDate) = " + intYearNo + @") 
                //                                                  AND (" + sEmp + @")
                //                                        )";
                strSQL = @"SELECT A.* FROM TaxDefineMaster A
                                    INNER JOIN (
                                                SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate
                                                 FROM TaxDefineMaster 
                                                 WHERE EffectiveDate <= '" + sDate + @"'
                                                       AND (" + sEmp + @")
												GROUP BY EmpInfoSystemID
                                               ) B ON A.EmpInfoSystemID = B.EmpInfoSystemID AND A.EffectiveDate = B.EffectiveDate
                                WHERE A.SystemID IN (
                                                     SELECT TaxDefineMasterSystemID FROM TaxDeductionInfoMonthWise WHERE  
                                                        TaxPeriodSystemID IN (SELECT DISTINCT ID FROM scs.TaxYearPeriod 
	                                                    WHERE ID IN (																					
																					select p.Id from [SCS].[CompanyTaxYearPeriod] cp
																						left outer join [SCS].[TaxYearPeriod] p on p.Id=cp.TaxYearPeriodId
																						left outer join [SCS].[CompanyTaxYear] cy on cy.Id=cp.CompanyTaxYearId
																					WHERE cy.CompanyID IN (
																										SELECT CompanyID FROM org.Plant 
																										WHERE Id = '" + sPlantID + @"'
																										)
																				)
		                                                      AND MONTH(StartDate) = " + intMonthNo + @" AND YEAR(StartDate) = " + intYearNo + @") 
                                                              AND (" + sEmp + @")
                                                    )";

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
        public void GetTaxDefineMasterAfter(string sPlantID, string sEmp, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.* FROM TaxDefineMaster A
                                    INNER JOIN (
                                                SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate
                                                 FROM TaxDefineMaster 
                                                 WHERE EffectiveDate > '" + sDate + @"'
                                                       AND (" + sEmp + @")
												GROUP BY EmpInfoSystemID
                                               ) B ON A.EmpInfoSystemID = B.EmpInfoSystemID AND A.EffectiveDate = B.EffectiveDate";

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

        public void GetTaxDefineMasterSave(string sPlantID, string sTaxYearID, string sEmpInfo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxDefineMaster 
                                    WHERE TaxPolicyMstID IN (SELECT SystemID FROM TaxPolicyMaster 
                                            WHERE PlantID = '" + sPlantID + @"' AND TaxYearID = '" + sTaxYearID + @"')
                                         AND TaxYearID = '" + sTaxYearID + @"' AND (" + sEmpInfo + @")";

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
        public void GetTaxableIncomeSalaryHeadWise(string sPlantID, string sTaxYearID, string sEmpInfo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxableIncomeSalaryHeadWise WHERE EmpInfoSystemID IN (SELECT DISTINCT SystemID 
                                FROM EmployeeInformation WHERE PlantID = '" + sPlantID + @"') 
                                    AND  TaxYearID = '" + sTaxYearID + @"' AND (" + sEmpInfo + @")";

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
        public void GetTaxableYearlyActualIncomeSalaryHeadWise(string sPlantID, string sTaxYearID, string sEmpInfo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxableYearlyActualIncomeSalaryHeadWise 
		                            WHERE EmpInfoSystemID IN (SELECT DISTINCT SystemID 
                           FROM EmployeeInformation WHERE PlantID = '" + sPlantID + @"') 
                                AND TaxYearID = '" + sTaxYearID + @"' AND (" + sEmpInfo + @")";

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
        public void GetTaxDeductionInfoMonthWise(string sPlantID, string sTaxYearID, string sFromDate, string sEmpInfo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE 
                            TaxPeriodSystemID IN (SELECT DISTINCT Id FROM scs.TaxYearPeriod 
	                                                    WHERE ID IN (																					
																					select p.Id from [SCS].[CompanyTaxYearPeriod] cp
																						left outer join [SCS].[TaxYearPeriod] p on p.Id=cp.TaxYearPeriodId
																						left outer join [SCS].[CompanyTaxYear] cy on cy.Id=cp.CompanyTaxYearId
																					WHERE cy.CompanyID IN (
																										SELECT CompanyID FROM org.Plant 
																										WHERE Id = '" + sPlantID + @"'
																										)
																				)
                                AND TaxYearID = '" + sTaxYearID + @"' AND StartDate > DATEADD(m, 1,'" + sFromDate + @"')) AND (" + sEmpInfo + @")";

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
        public void GetTaxPolicyMaster(string sPlantID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM TaxPolicyMaster 
                          WHERE TaxYearID = '" + sTaxYearID + "' AND PlantID = '" + sPlantID + @"'";

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
        public void GetTaxPolicyGeneralWithYearlyActualTax(string sTaxYearID, string sEmpInfo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM (SELECT TAYAISHW.SystemID, TAYAISHW.EmpInfoSystemID, TAYAISHW.TaxPolicyMstID, TAYAISHW.TaxGroupID, 
                                    TAYAISHW.SalaryHeadID, SH.SalaryHead, TAYAISHW.YearlyIncome, TPG.IsExemption, TPG.IsExmWhichEverLess, 
									TPG.IsMaxExmpAmt, TPG.TaxMaxExmpAmt, TPG.IsExmBaseOnActual, TPG.IsExmBaseOnOtherSlrHd, 
									TPG.ExmSalaryHeadID, TPG.PercentageExmAmtOtherSlrHd, TPG.IsTaxable,  
                                    TPG.IsFixedTaxGeneral, TPG.TaxFixedGeneral, TPG.IsPercentageTaxGeneral, TPG.TaxPercentageGeneral, 
                                    ISNULL(TaxExp.TaxExemptedAmt, 0) TaxExemptedAmt, TAYAISHW.YearlyTaxableIncome
                            FROM TaxableYearlyActualIncomeSalaryHeadWise TAYAISHW
                                 LEFT JOIN SalaryHead SH ON TAYAISHW.SalaryHeadID = SH.SalaryHeadID
                                 LEFT JOIN TaxPolicyGeneral TPG ON TAYAISHW.SalaryHeadID = TPG.SalaryHeadID
                                                AND TAYAISHW.TaxPolicyMstID = TPG.TaxPolicyMstID
                                 LEFT JOIN TaxExemptedAmtSalaryHeadWise TaxExp ON TAYAISHW.EmpInfoSystemID = TaxExp.EmpInfoSystemID
					                            AND TAYAISHW.TaxPolicyMstID = TaxExp.TaxPolicyMstID AND 
					                            TAYAISHW.TaxGroupID = TaxExp.TaxGroupID AND TaxExp.TaxYearID = '" + sTaxYearID + @"'
                            WHERE TAYAISHW.TaxYearID = '" + sTaxYearID + @"') A WHERE (" + sEmpInfo + @")";

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
        public void GetTaxSlab(string sPlantID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxSlabDefine WHERE 
                                    TaxPolicyMstID IN (SELECT SystemID 
                                                                FROM TaxPolicyMaster 
                                WHERE TaxYearID = '" + sTaxYearID + @"' AND PlantID = '" + sPlantID + @"')";

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

        public void GetBonusAmount(string sEmpInfo, int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM (SELECT BPA.EmpSystemID, E.PlantID, BPA.BnsMstSystemID, BPA.SystemID AS CHDSystemID, 
                                BPAM.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, BPAM.EntryCurrencyID, ECR.Code AS EntryCurrency, 
                                BPAM.DefineCurrencyID AS DefinationCurrencyID, DECR.Code AS DefinationCurrency, 
                                BPAM.DisbustCurrencyID, DICR.Code AS DisbustCurrency, BPAM.DisbustSalaryHeadID, BPA.BonusAmount, 
                                AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
                                                            THEN CRC.AccumulateExchangeSalaryHeadID
                                                         ELSE BPAM.SalaryHeadID END, BPAM.AmtDefinationCurrencyID, BPAM.AmtDefinationRate,
                                BPA.SlrProcMonthNo, BPA.SlrProcYearNo, BPA.IsDisbused, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
                            FROM BonusPaymentActual BPA
                                INNER JOIN EmployeeInformation E ON BPA.EmpSystemID = E.SystemID
                                INNER JOIN BonusPaymentActualMaster BPAM ON BPAM.SystemID = BPA.BnsMstSystemID 
                                INNER JOIN SalaryHead SH ON BPAM.SalaryHeadID = SH.SalaryHeadID 
                                INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID 
                                    AND BPAM.SalaryHeadID = CRC.SalaryHeadID
                                LEFT JOIN scs.Currency ECR ON BPAM.EntryCurrencyID = ECR.Id
                                LEFT JOIN scs.Currency DECR ON BPAM.DefineCurrencyID = DECR.Id
                                LEFT JOIN scs.Currency DICR ON BPAM.DisbustCurrencyID = DICR.Id) A
                        WHERE SlrProcMonthNo = " + intMonthNo + @" AND SlrProcYearNo = " + intYearNo + @" AND IsDisbused  = 1
                              AND (" + sEmpInfo + @")";

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

        ///
        ///By monir
        ///
        public void GetEntityCurrencyRateInfo(string sCurrencyID, string sPlantID, string sFromDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT A.SystemID, A.FromCurrencyUnit, A.FromCurrencyCode, FR.CurrencyDesc FromCurrencyDesc, A.ToCurrencyBuying, A.ToCurrencySelling, 
                //                A.ToCurrencyCode, LR.CurrencyDesc ToCurrencyDesc, A.FromDate 
                //            FROM [dbo].[ExchangerateDateWiseForHR] A
                //               LEFT JOIN Currency FR ON A.FromCurrencyCode = FR.CurrencyCode
                //               LEFT JOIN Currency LR ON A.ToCurrencyCode = LR.CurrencyCode
                //            WHERE PlantID = '" + sPlantID + @"'
                //            GROUP BY A.SystemID, A.FromCurrencyUnit, A.FromCurrencyCode, FR.CurrencyDesc, A.ToCurrencyBuying, A.ToCurrencySelling, 
                //                A.ToCurrencyCode, LR.CurrencyDesc, A.FromDate 
                //            Having Max(A.FromDate) <= '" + sFromDate + @"' ";

                strSQL = @"SELECT A.SystemID
	                                        ,A.FromCurrencyUnit
	                                        ,A.FromCurrencyCode
	                                        ,FR.Code FromCurrencyDesc
	                                        ,A.ToCurrencyBuying
	                                        ,A.ToCurrencySelling
	                                        ,A.ToCurrencyCode
	                                        ,LR.Code ToCurrencyDesc
	                                        ,A.FromDate
                                        FROM [dbo].[ExchangerateDateWiseForHR] A
                                        LEFT JOIN scs.Currency FR ON A.FromCurrencyCode = FR.Id
                                        LEFT JOIN scs.Currency LR ON A.ToCurrencyCode = LR.Id
                                        WHERE PlantID = '" + sPlantID + @"'
                                        GROUP BY A.SystemID
	                                        ,A.FromCurrencyUnit
	                                        ,A.FromCurrencyCode
	                                        ,FR.Code
	                                        ,A.ToCurrencyBuying
	                                        ,A.ToCurrencySelling
	                                        ,A.ToCurrencyCode
	                                        ,LR.Code
	                                        ,A.FromDate
                                        HAVING Max(A.FromDate) <= '" + sFromDate + @"'";
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
        public void GetCompanyContributedTax(string sEmpInfo, string sFromDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select cc.EmpSystemId,cc.IsFixed,cc.Amount from 
                            [MST].[CompanyTaxContribution] cc
                            left outer join scs.TaxYear t on  cc.TaxYearId=t.Id
                            where EmpSystemId in (" + sEmpInfo + @") and t.Id in
                            (select Id from scs.TaxYear  where '" + sFromDate + "' between StartDate and EndDate)";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetCurrencyRuleChildWithSlrHDCat(string strCurrencyRuleID, string sPlantID, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Cr.SystemID,	Cr.MstSystemID,	Cr.SalaryHeadID, Cr.AmtEntryCurrency, Cr.AmtDefinitionCurrency,	
	                              Cr.AmtDisbusmentCurrency,	Cr.AccumulateExchangeRate,	Cr.AccumulateExchangeSalaryHeadID,	
	                              Cr.IntegerInDisb, SH.HeadCategory, CR.DecimalNo, CR.RoundOption, CR.IsDecimalInDisb
                            FROM CurrencyRuleChild Cr
	                             INNER JOIN CurrencyRuleMaster Cm ON Cr.MstSystemID = Cm.SystemID AND Cm.PlantID = '" + sPlantID + @"'
                                 LEFT JOIN SalaryHead SH ON CR.SalaryHeadID = SH.SalaryHeadID";

                if (strCurrencyRuleID != "")
                {
                    strSQL = strSQL + @" WHERE Cr.MstSystemID = '" + strCurrencyRuleID + @"'";
                }

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
        }//End of function

        public void GetSelectedEmployee(string sEmpInfo, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //       strSQL = @"SELECT DISTINCT E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName, E.PlantID, P.UserName PlantName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ, 
                //                         REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS, E.EmployeeStatus,
                //                         E.DesignationGroupID, E.SalaryRuleMasterSystemID, E.GivenDesignationId,isnull(e.PaymentMode,'Cash') PaymentMode
                // --BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment' ELSE 'Cash Payment' END
                //                        ,BankAccountStatus=isnull(e.PaymentMode,'Bank')
                // --,BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment' ELSE 'Cash Payment' END

                //                  FROM EmployeeInformation E
                //                           LEFT JOIN org.Plant P ON E.PlantID = P.ID
                //LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                //                  WHERE SystemID IN (" + sEmpInfo + @")";
                strSQL = @"SELECT DISTINCT E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName, E.PlantID, P.UserName PlantName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ, 
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,
								  --DOS=case when isnull(x.EmpSystemID,'')<>'' then format(FromDate,'dd-MMM-yyyy') else format(dos,'dd-MMM-yyyy') end,
								   E.EmployeeStatus,
                                  E.DesignationGroupID, E.SalaryRuleMasterSystemID, E.GivenDesignationId,isnull(e.PaymentMode,'Cash') PaymentMode
								  --BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment' ELSE 'Cash Payment' END
                                 ,BankAccountStatus=isnull(e.PaymentMode,'Bank')
								 ,x.EmpSystemID mlvempid,format(x.FromDate,'dd-MMM-yyyy') FromDate
								 ,format(y.ToDate,'dd-MMM-yyyy') ToDate

                           FROM EmployeeInformation E
                                    LEFT JOIN org.Plant P ON E.PlantID = P.ID
									LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
									left join (
									select EmpSystemID,FromDate from LeaveTransaction where DATEADD(DAY,-1,FromDate) between 
                                        '" + sFromDate + @"' and '" + sToDate + @"'
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')									
									) x on x.EmpSystemID=e.SystemId

                                    left join (
									select EmpSystemID,ToDate from LeaveTransaction where DATEADD(DAY,1,ToDate)  between 
                                       '" + sFromDate + @"' and '" + sToDate + @"'
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')									
									) y on y.EmpSystemID=e.SystemId

                           WHERE SystemID IN (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetEmployeeWiseDesignationMasterSetting(string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT E.SystemId EmpSystemID, D.SalaryRuleMasterId, D.IsOTEntitled, D.AttdnBonusPmtPolicyMasterId, D.PFPolicyMasterID
                            FROM EmployeeInformation E
                            LEFT OUTER JOIN (SELECT DC.PlantId,DC.LeavePolicyMasterId,DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.AttdnBonusPmtPolicyMasterId,DC.PFPolicyMasterID,DM.DesignationId
							 FROM MST.DesignationMaster DM
							LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) D ON D.DesignationId = E.GivenDesignationId AND E.PlantId=D.PlantId
                            WHERE E.SystemId IN (" + sEmpInfo + @") 
                            ORDER BY E.SystemId";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseAttdnBonus(string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT DM.EmpSystemID, ABPD.AttdnBonusPmtPolicyID AttdnBonusPmtPolicyMasterId, ABPD.ID, DM.SalaryRuleMasterId, --ABPDT.DayType,  
	                              --ABPDT.DayTypeOperator, ABPDT.DayTypeOperatorValue, ABPLT.LeaveTypeID, ABPLT.ApprovalType, 
                                  ABPD.IsFixed, ABPD.FixedValue,  
	                              ABPD.IsFormula, ABPD.FormulaDes, ABPD.FormulaDesID, SRATBM.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory,  	
	                              CRC.AmtEntryCurrency EntryCurrencyID, ECR.Name AS EntryCurrency, CRC.AmtDefinitionCurrency DefineCurrencyID, 
	                              DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
	                              AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
								                            ELSE SRATBM.SalaryHeadID END,
	                              CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, 
                                  ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo
                            FROM [dbo].[AttdnBonusPmtPolicyDetails] ABPD
				                            --LEFT JOIN [dbo].[AttdnBonusPmtPolicyDayType] ABPDT ON ABPD.ID = ABPDT.AttdnBonusPmtPolicyDetailsID
				                            --LEFT JOIN [dbo].[AttdnBonusPmtPolicyLeaveType] ABPLT ON ABPD.ID = ABPLT.AttdnBonusPmtPolicyDetailsID
				                            INNER JOIN (
							                            SELECT E.SystemId EmpSystemID, D.SalaryRuleMasterId, D.IsOTEntitled, D.AttdnBonusPmtPolicyMasterId, D.PFPolicyMasterID
								                            FROM EmployeeInformation E
												                            INNER JOIN  (SELECT dc.plantid,DC.LeavePolicyMasterId,DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.AttdnBonusPmtPolicyMasterId,DC.PFPolicyMasterID,DM.DesignationId
							 FROM MST.DesignationMaster DM
							LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) D ON D.DesignationId = E.GivenDesignationId and d.plantid=e.plantid
							                            WHERE D.AttdnBonusPmtPolicyMasterId IS NOT NULL AND E.SystemId IN (" + sEmpInfo + @") 
							                            ) DM ON ABPD.AttdnBonusPmtPolicyID = DM.AttdnBonusPmtPolicyMasterId
				                            LEFT JOIN SalaryRuleMaster SRM ON DM.SalaryRuleMasterId = SRM.SystemID
				                            LEFT JOIN [dbo].[SalaryRuleAttdnBonusPmtMaster] SRATBM ON SRM.SystemID = SRATBM.SalaryRuleMasterSystemID
				                            LEFT JOIN SalaryHead SH ON SRATBM.SalaryHeadID = SH.SalaryHeadID
				                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SRATBM.SalaryHeadID = CRC.SalaryHeadID
			                                LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                                LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                                LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                            ORDER BY DM.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseAttdnBonusDayType(string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                
                strSql = @"SELECT ABPD.ID AttdnBonusPmtPolicyDetailsID
                                            ,IsEarlyOutApplicable
                                            ,IsLunchOutApplicable
                                            ,IsLateInApplicable
                                            ,IsAbsentApplicable
                                            ,IsLateApplicable
                                            ,IsLeaveApplicable
                                            ,IsLeaveWithOutPayApplicable
                                            ,EOLIFromValue
                                            ,EOLIToValue
                                            ,LunchOutFromValue
                                            ,LunchOutToValue
                                            ,AbsentFromValue
                                            ,AbsentToValue
                                            ,LateFromValue
                                            ,LateToValue
                                            ,LeaveFromValue
                                            ,LeaveToValue
                                            ,LeaveWithOutPayFromValue
                                            ,LeaveWithOutPayToValue
                                            ,FixedOrFormula
                                            ,IsRouteApplicableForLate
                                 FROM [dbo].[AttdnBonusPmtPolicyDetails] ABPD
				                            INNER JOIN (
							                            SELECT E.SystemId EmpSystemID, D.SalaryRuleMasterId, D.IsOTEntitled, D.AttdnBonusPmtPolicyMasterId, D.PFPolicyMasterID
								                            FROM EmployeeInformation E
												            INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.AttdnBonusPmtPolicyMasterId,DC.PFPolicyMasterID,DM.DesignationId,DC.PlantId
							 FROM MST.DesignationMaster DM
							LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) D ON D.DesignationId = E.GivenDesignationId AND E.PlantId=D.PlantId
							                            WHERE D.AttdnBonusPmtPolicyMasterId IS NOT NULL AND E.SystemId IN (" + sEmpInfo + @") 
							                            ) DM ON ABPD.AttdnBonusPmtPolicyID = DM.AttdnBonusPmtPolicyMasterId
                            ORDER BY ABPD.Id";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseAttdnBonusLeaveType(string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT ABPLT.AttdnBonusPmtPolicyDetailsID, ABPLT.LeaveTypeID, ABPLT.ApprovalType 
                            FROM [dbo].[AttdnBonusPmtPolicyDetails] ABPD
				                            INNER JOIN [dbo].[AttdnBonusPmtPolicyLeaveType] ABPLT ON ABPD.ID = ABPLT.AttdnBonusPmtPolicyDetailsID
				                            INNER JOIN (
							                            SELECT E.SystemId EmpSystemID, D.SalaryRuleMasterId, D.IsOTEntitled, D.AttdnBonusPmtPolicyMasterId, D.PFPolicyMasterID
								                            FROM EmployeeInformation E
												                            INNER JOIN (SELECT dc.plantid,DC.LeavePolicyMasterId,DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.AttdnBonusPmtPolicyMasterId,DC.PFPolicyMasterID,DM.DesignationId
							 FROM MST.DesignationMaster DM
							LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) D ON D.DesignationId = E.GivenDesignationId and D.plantid=e.plantid
							                            WHERE D.AttdnBonusPmtPolicyMasterId IS NOT NULL AND E.SystemId IN (" + sEmpInfo + @") 
							                            ) DM ON ABPD.AttdnBonusPmtPolicyID = DM.AttdnBonusPmtPolicyMasterId
                            ORDER BY ABPLT.AttdnBonusPmtPolicyDetailsID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseOTPolicy(string FromDate, string ToDate, string sEmpInfo, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"
                                            SELECT ee.EmpSystemID
                                            --, OTPD.OverTimePmtPolicyID OverTimePmtPolicyMasterID
                                            , OTPD.ID
                                            ,ee.OverTimePmtPolicyMasterID                                           
                                              ,CAST(ISNULL(ee.IsOTEntitle,0) AS bit) IsOTEntitled

                                            , ee.SalaryRuleMasterId, OTPD.OverTimeDayType, 
                                                                              OTPD.IsFixed, OTPD.FixedValue, OTPD.IsFormula, OTPD.FormulaDes, OTPD.FormulaDesID, SROTM.SalaryHeadID,  
	                                                                          SH.SalaryHead, SH.HeadType, SH.HeadCategory,  	
	                                                                          CRC.AmtEntryCurrency EntryCurrencyID, ECR.Name AS EntryCurrency, CRC.AmtDefinitionCurrency DefineCurrencyID, 
	                                                                          DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
	                                                                          AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
								                                                                        ELSE SROTM.SalaryHeadID END,
	                                                                          CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, 
                                                                              ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo
                                            from
                                            (
                                            select e.SystemId EmpSystemID
                                            --,OTEN.IsOTEntitle,(select ID  from OverTimePmtPolicyMaster where isDefault=1)  OverTimePmtPolicyMasterID
                                            --,D.OverTimePmtPolicyMasterID,D.IsOTEntitled
                                            ,D.SalaryRuleMasterId
                                            ,OverTimePmtPolicyMasterID=case 
                                            when D.IsOTEntitled=1 and D.OverTimePmtPolicyMasterID is not null then D.OverTimePmtPolicyMasterID
                                            when D.IsOTEntitled=1 and D.OverTimePmtPolicyMasterID is null then (select ID  from OverTimePmtPolicyMaster where isDefault=1 and plantid='" + plantid + @"')
                                            when  D.IsOTEntitled=0 and ISNULL(OTEN.IsOTEntitle,0)=1 then  (select ID  from OverTimePmtPolicyMaster where isDefault=1 and plantid='" + plantid + @"')
                                            else null end
                                                                ,IsOTEntitle=case when ISNULL(OTEN.IsOTEntitle,0)=1 then 1
					                                            when ISNULL(D.IsOTEntitled,0)=1 then 1					                        
                                                                else 0 end
                                            from 
	                                            (select * from  [EmployeeInformation] where SystemId IN (" + sEmpInfo + @")
	                                            ) e
	                                            left join (  
	                                            select * from EmployeeOTEntitle
		                                            WHERE  (ISNULL(OTStartDate, GETDATE()) <='" + ToDate + @"'
	                                            AND ISNULL(OTEndDate, GETDATE())>='" + FromDate + @"'   
	                                            AND ISNULL(IsOTEntitle, 0) = 1)
		                                                    ) OTEN on OTEN.EmpSystemID=e.SystemId
	                                            left JOIN  (
				                                            SELECT DC.LeavePolicyMasterId,DC.PlantId,DM.DesignationId,DC.AttdnBonusPmtPolicyMasterId,
				                                            DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.OverTimePmtPolicyMasterID,DC.PFPolicyMasterID 
				                                            FROM MST.DesignationMaster DM
				                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
				                                            ) D ON D.DesignationId = E.GivenDesignationId AND D.PlantId=E.PlantId
	                                            ) ee
	                                            left join [OverTimePmtPolicyDetails] OTPD on otpd.OverTimePmtPolicyID=ee.OverTimePmtPolicyMasterID
	                                             LEFT JOIN [dbo].[SalaryRuleMaster] SRM ON ee.SalaryRuleMasterId = SRM.SystemID
		                                            LEFT JOIN [dbo].[SalaryRuleOT] SROTM ON SRM.SystemID = SROTM.SalaryRuleMasterSystemID AND OTPD.OverTimeDayType = SROTM.OverTimeDayType
		                                            LEFT JOIN [dbo].[SalaryHead] SH ON SROTM.SalaryHeadID = SH.SalaryHeadID
		                                            LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SROTM.SalaryHeadID = CRC.SalaryHeadID
		                                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
		                                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
		                                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                                    where 
		                                             isnull(SROTM.SalaryHeadID,'') <>'' and
                                                    otpd.id is not null and                          
		                                            ee.EmpSystemID not in 
		                                            (
		                                            select EmpSystemID from EmployeeOTEntitle WHERE  (ISNULL(OTStartDate, GETDATE()) <='" + ToDate + @"' AND ISNULL(OTEndDate, GETDATE())>='" + FromDate + @"'   
		                                            AND ISNULL(IsOTEntitle, 0) = 0)
		                                            )
                                            ORDER BY ee.EmpSystemID							
							";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetExistingProcessedSalary(string FromDate, string sEmpInfo, out Dictionary<string, Dictionary<string, DataRow>> salaryInfo)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT c.EmpInfoSystemID, c.SalaryHeadID, c.DisbusmentAmount, c.DisbusmentCurrencyID
                              FROM SalaryProcMaster AS M 
                            JOIN SalaryProcChild C ON m.SystemID=c.SlrProcMstSystemID

                            WHERE c.EmpInfoSystemID IN (" + sEmpInfo + @") AND m.MonthNo=" + Convert.ToDateTime(FromDate).Month.ToString() + @" AND m.YearNo=" + Convert.ToDateTime(FromDate).Year.ToString() + @"
                            ORDER BY c.EmpInfoSystemID, c.SalaryHeadID		
							";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out DataSet dsRef, false, "1");


                salaryInfo = new Dictionary<string, Dictionary<string, DataRow>>();
                Dictionary<string, DataRow> SalaryHeads = new Dictionary<string, DataRow>();
                string empId = "";
                string CurrentEmpId = "";
                foreach (DataRow Row in dsRef.Tables[0].Rows)
                {
                    CurrentEmpId = Row["EmpInfoSystemID"].ToString();
                    if (empId != CurrentEmpId)
                    {
                        SalaryHeads = new Dictionary<string, DataRow>();
                        salaryInfo.Add(CurrentEmpId, SalaryHeads);
                    }

                    SalaryHeads.Add(Row["SalaryHeadID"].ToString(), Row);
                    empId = CurrentEmpId;
                }

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

        public void GetOTHour(string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @" SELECT EmpSystemID,SUM(NormalOTHr) NormalOTHr,SUM(WeekOffOTHr) WeekOffOTHr,SUM(HoliDayOTHr) HoliDayOTHr from
                                (
                                SELECT EmpSystemID, NormalOTHr = CASE WHEN OTDayType = 'NW' THEN NormalOTHr
									                              ELSE 0 END,  
					                         WeekOffOTHr = CASE WHEN OTDayType IN ('W','WL','WLV','WP','CWP') THEN NormalOTHr
									                              ELSE 0 END,  
					                         HoliDayOTHr = CASE WHEN OTDayType IN ('HL','H','HLV','HLV','HP') THEN NormalOTHr
									                              ELSE 0 END 
                           FROM [dbo].[FinalOT]  
                           WHERE WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + "' AND EmpSystemID IN (" + sEmpInfo + @") 
						   ) x
                           GROUP BY EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetLeaveTransactionForAttdnBonus(string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT EmpSystemID, LTSystemID, ComAssignLvSystemID, OffDayMstSystemID, FromDate, ToDate, LeaveDays, IsPostApplied
                             FROM [dbo].[LeaveTransaction]
                           WHERE IsApproved = 1 AND (('" + sFromDate + @"' BETWEEN FromDate AND ToDate) OR ('" + sToDate + @"' BETWEEN FromDate AND ToDate))
	                               AND EmpSystemID IN (" + sEmpInfo + @") 
                           ORDER BY EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetLeaveTransactionForAttdnBonusPRE_POST(string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @" select						   
						   EmpSystemID, LTSystemID, ComAssignLvSystemID, OffDayMstSystemID, FromDate, ToDate, LeaveDays
						   , IsPostApplied=case when m.AppliedDate>m.FromDate then convert(bit,1) else convert(bit,0) end
						    from LeaveTransactionDetails d 
						   left join LeaveTransaction m on m.SystemID=d.LvTrnsSystemID
						   where WorkDate between '" + sFromDate + "' and '" + sToDate + "'  AND EmpSystemID IN (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEarlyOut(string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select count(id) c ,EmpSystemId
                                    from AttendanceInfoExtra 
                                    where EmpSystemId in (" + sEmpInfo + @")
                                    and workdate between '" + sFromDate + @"' and '" + sToDate + @"'
                                    and InfoType='EARLYOUT'
                                    group by EmpSystemId";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWisePFEmployeeVoluntaryValue(string sEmpInfo, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT DM.EmpSystemID, DM.PlantId, DM.PFPolicyMasterID, DM.SalaryRuleMasterId, DM.IsVoluntaryPF, A.VoluntaryPFValue, SRATBM.SalaryHeadID, 
	                              SH.SalaryHead, SH.HeadType, SH.HeadCategory, DM.IsFixedEmp, DM.FixedValueEmp, DM.IsFormulaEmp, DM.IsContributionSlrHDdependOnEarningEmp,
								  DM.FormulaDesEmp, DM.FormulaDesIDEmp, CRC.AmtEntryCurrency EntryCurrencyID, ECR.Name AS EntryCurrency,  	
	                              CRC.AmtDefinitionCurrency DefineCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
	                              AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
								                            ELSE SRATBM.SalaryHeadID END,
	                              CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency
							 FROM [dbo].[PFEmployeeVoluntaryValue] A
										    INNER JOIN (
														 SELECT EmpSystemId, MAX(EffectiveDate) EffectiveDate FROM [dbo].[PFEmployeeVoluntaryValue]
														  WHERE EffectiveDate <= '" + sToDate + @"'
														 GROUP BY EmpSystemId
														) B ON A.EmpSystemId = B.EmpSystemId AND A.EffectiveDate = B.EffectiveDate
										    INNER JOIN (
							                            SELECT E.SystemId EmpSystemID, E.SalaryRuleMasterSystemID SalaryRuleMasterId, D.PFMstID PFPolicyMasterID, E.PlantId,
															   PFD.IsVoluntaryPF, PFD.IsFixedEmp, PFD.FixedValueEmp, PFD.IsFormulaEmp, PFD.IsContributionSlrHDdependOnEarningEmp,
															   PFD.FormulaDesEmp, PFD.FormulaDesIDEmp, PFD.EmpVolunValPer
								                            FROM EmployeeInformation E
												                            INNER JOIN (SELECT * FROM [dbo].[PFEligibleEmployee] WHERE IsActive = 1) D ON D.EmpSystemID = E.SystemId
																			INNER JOIN PFPolicyDetails PFD ON D.PFMstID = PFD.PFPolicyMasterID
							                            WHERE E.SystemId IN (" + sEmpInfo + @") 
							                           ) DM ON A.EmpSystemID = DM.EmpSystemID
				                            INNER JOIN [dbo].[SalaryRuleMaster] SRM ON DM.SalaryRuleMasterId = SRM.SystemID
				                            INNER JOIN [dbo].[SalaryRulePF] SRATBM ON SRM.SystemID = SRATBM.SalaryRuleMasterSystemID
				                            INNER JOIN (
                                                        SELECT * FROM [dbo].[SalaryHead] WHERE HeadCategory = 'PF Voluntary'
                                                       ) SH ON SRATBM.SalaryHeadID = SH.SalaryHeadID
				                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SRATBM.SalaryHeadID = CRC.SalaryHeadID
			                                LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                                LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                                LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                            ORDER BY DM.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetPFStructureData(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @" select 
									e.SystemId EmpSystemID,e.PlantId 
									,e.EmployeeCode,sh.SalaryHead
									,mm.SalaryRuleMasterSystemID,mm.SalaryHeadID,sh.SalaryHead,sh.HeadType,sh.HeadCategory,sh.HeadCategory SlrCate
									,ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,mm.DefineAmount ContributionAmount
									from (select * from EmployeeInformation where SystemId in (" + sEmpInfo + @")) e
										    INNER JOIN (
													        	   SELECT * FROM (  SELECT  *,
				                            DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                                from (
							                                                SELECT SD.SystemID,SDM.PlantID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                from SalaryInfoDefineMaster SDM
								                                                JOIN SalaryInfoDefine AS SD ON sdm.SystemID=SD.SalaryID 
                                                                                WHERE SDM.IsApproved=1
								                                                union ALL
								                                                select SD.SystemID,SDM.PlantID,EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                 from SalaryInfoBackMaster SDM
								                                                JOIN SalaryInfoBack AS SD ON sdm.SystemID=SD.SalaryID 
                                                                                WHERE SDM.IsApproved=1
			                                                ) AS SDM
			
			                                        ) AS SDM 
                                                    WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + sDate + @"' AND rnk=1 
													    ) mm ON e.SystemID = mm.EmpInfoSystemID
										   


								
								LEFT JOIN SalaryHead SH ON mm.SalaryHeadID = SH.SalaryHeadID 
							    LEFT JOIN SalaryRuleMaster SRM ON mm.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND mm.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
								left join SalaryRulePF f on f.SalaryRuleMasterSystemID=mm.SalaryRuleMasterSystemID and f.SalaryHeadID=mm.SalaryHeadID
                                
                                WHERE ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') 
								in ('PF Employer Contribution','PF Employee Contribution','PF Voluntary','Pension')
";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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





        public void GetEmployeeListRetentionAllowMonthWise(string sEmpInfo, string sAllSalaryID, int intMonthNo, int intYearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT RAMW.ID, RAE.EmpSystemID, SLM.SalaryID, RAMW.RetenAllowEmpSystemID, RAMW.SalaryHeadID, RAMW.MonthNo, RAMW.YearNo, RAMW.Amount, RAM.IsAbsentismApplicable, 
								 SLM.PlantID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, ECR.Id EntryCurrencyID, ECR.Name AS EntryCurrency, 
								 DECR.Id DefineCurrencyID, DECR.Name AS DefinitionCurrency, SRM.CurrencyRuleSystemID, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                 AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                    ELSE RAMW.SalaryHeadID END,
			                     CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, IsNetPayEffect = Convert(bit, 'TRUE'), CRC.RoundOption, 
                                 ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo
                            FROM [dbo].[RetentionAllowMonthWise] RAMW
		                            INNER JOIN [dbo].[RetentionAllowEmployee] RAE ON RAMW.RetenAllowEmpSystemID = RAE.ID
		                            INNER JOIN [SCS].[RetentionAllowanceDetail] RAD ON RAE.RetetionAllowID = RAD.Id
		                            INNER JOIN [MST].[RetentionAllowanceMaster] RAM ON RAD.RetentionAllowanceMasterId = RAM.Id
									INNER JOIN (
												SELECT SLM.* FROM 
																(
																 SELECT SystemID SalaryID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																 FROM SalaryInfoDefineMaster
																 UNION 
																(
																 SELECT SystemID SalaryID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																 FROM SalaryInfoBackMaster
																)
																) SLM WHERE (" + sAllSalaryID + @")
											   ) SLM ON RAE.EmpSystemID = SLM.EmpInfoSystemID
									INNER JOIN SalaryHead SH ON RAMW.SalaryHeadID = SH.SalaryHeadID 
			                        INNER JOIN SalaryRuleMaster SRM ON SLM.SalaryRuleMasterSystemID = SRM.SystemID
			                        LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND RAMW.SalaryHeadID = CRC.SalaryHeadID
			                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                            WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @"
	                              AND RAE.EmpSystemID IN (" + sEmpInfo + @") 
                            ORDER BY RAE.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetRetentionAllowMonthWise(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM [dbo].[RetentionAllowMonthWise] 
	                        WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @" 
		                          AND RetenAllowEmpSystemID IN (
		                                                        SELECT ID FROM [dbo].[RetentionAllowEmployee] 
									                             WHERE IsApproved = 1 AND EmpSystemID IN (" + sEmpInfo + @") 
									                           )";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseSalaryValueMontlyBasis(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM (
                                          SELECT BPA.EmpSystemID, E.PlantID, BPA.SystemID, BPA.SalaryHeadID, SH.SalaryHead, 
					                             SH.HeadType, SH.HeadCategory, CRC.AmtEntryCurrency EntryCurrencyID, ECR.Code AS EntryCurrency, 
					                             CRC.AmtDefinitionCurrency AS DefineCurrencyID, DECR.Code AS DefinitionCurrency, 
					                             CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Code AS DisbusmentCurrency, BPA.EntryAmount, BPA.EntryDate,
					                             AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
													                            THEN CRC.AccumulateExchangeSalaryHeadID
											                               ELSE BPA.SalaryHeadID END,
					                             BPA.IsContinued, BPA.PeriodType, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                                 ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo
				                           FROM [dbo].[SalaryValueUploaded] BPA
					                                        INNER JOIN EmployeeInformation E ON BPA.EmpSystemID = E.SystemID
					                                        INNER JOIN SalaryHead SH ON BPA.SalaryHeadID = SH.SalaryHeadID 
					                                        INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
					                                        INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID 
						                                               AND BPA.SalaryHeadID = CRC.SalaryHeadID
					                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
					                                        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
					                                        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                          WHERE Month(BPA.EntryDate) = " + intMonthNo + @" AND Year(BPA.EntryDate) = " + intYearNo + @"
                                                AND BPA.EmpSystemID IN(" + sEmpInfo + @") AND BPA.PeriodType = 'MONTHLY'
												AND ISNULL(BPA.IsContinued, 0) = 0 
                                         ) A
                         ORDER BY EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");//
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseSalaryValueMontlyContinuedBasis(string sDate, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM (
                                          SELECT BPA.EmpSystemID, E.PlantID, BPA.SystemID, BPA.SalaryHeadID, SH.SalaryHead, 
					                             SH.HeadType, SH.HeadCategory, CRC.AmtEntryCurrency EntryCurrencyID, ECR.Code AS EntryCurrency, 
					                             CRC.AmtDefinitionCurrency AS DefineCurrencyID, DECR.Code AS DefinitionCurrency, 
					                             CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Code AS DisbusmentCurrency, BPA.EntryAmount, BPA.EntryDate,
					                             AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
													                            THEN CRC.AccumulateExchangeSalaryHeadID
											                               ELSE BPA.SalaryHeadID END,
					                             BPA.IsContinued, BPA.PeriodType, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                                 ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo
				                           FROM [dbo].[SalaryValueUploaded] BPA
															INNER JOIN (
																		 SELECT EmpSystemID, MAX(EntryDate) EntryDate 
																			FROM [dbo].[SalaryValueUploaded] 
																		 WHERE EntryDate <= '" + sDate + @"' AND EmpSystemID IN(" + sEmpInfo + @")
                                                                         Group By EmpSystemID,EntryDate
																	   ) BPADT ON BPA.EmpSystemID = BPADT.EmpSystemID AND BPA.EntryDate = BPADT.EntryDate
					                                        INNER JOIN EmployeeInformation E ON BPA.EmpSystemID = E.SystemID
					                                        INNER JOIN SalaryHead SH ON BPA.SalaryHeadID = SH.SalaryHeadID 
					                                        INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
					                                        INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID 
						                                               AND BPA.SalaryHeadID = CRC.SalaryHeadID
					                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
					                                        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
					                                        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                          WHERE BPA.EmpSystemID IN(" + sEmpInfo + @") AND BPA.PeriodType = 'MONTHLY'
												AND ISNULL(BPA.IsContinued, 0) = 1 
                                         ) A
                         ORDER BY EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseSalaryValueDailyBasis(string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT EmpSystemID, PlantID, SystemID, SalaryHeadID, SalaryHead, HeadType, HeadCategory, EntryCurrencyID, EntryCurrency, 
					              DefineCurrencyID, DefinitionCurrency, DisbusmentCurrencyID, DisbusmentCurrency, SUM(ISNULL(EntryAmount, 0)) EntryAmount, 
					              AcltExcDisbSlrHDID, RoundOption, IntegerInDisb, IsDecimalInDisb, DecimalNo
                            FROM (
                                    SELECT BPA.EmpSystemID, E.PlantID, BPA.SystemID, BPA.SalaryHeadID, SH.SalaryHead, 
					                        SH.HeadType, SH.HeadCategory, CRC.AmtEntryCurrency EntryCurrencyID, ECR.Code AS EntryCurrency, 
					                        CRC.AmtDefinitionCurrency AS DefineCurrencyID, DECR.Code AS DefinitionCurrency, 
					                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Code AS DisbusmentCurrency, BPA.EntryAmount, BPA.EntryDate,
					                        AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 
													                    THEN CRC.AccumulateExchangeSalaryHeadID
											                        ELSE BPA.SalaryHeadID END,
					                        BPA.IsContinued, BPA.PeriodType, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo
				                    FROM [dbo].[SalaryValueUploaded] BPA
					                                INNER JOIN EmployeeInformation E ON BPA.EmpSystemID = E.SystemID
					                                INNER JOIN SalaryHead SH ON BPA.SalaryHeadID = SH.SalaryHeadID 
					                                INNER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
					                                INNER JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID 
						                                        AND BPA.SalaryHeadID = CRC.SalaryHeadID
					                                LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
					                                LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
					                                LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                    WHERE BPA.EntryDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"' 
                                          AND BPA.EmpSystemID IN(" + sEmpInfo + @") AND BPA.PeriodType = 'DAILY'
										AND ISNULL(BPA.IsContinued, 0) = 0 
                                    ) A
                         GROUP BY EmpSystemID, PlantID, SystemID, SalaryHeadID, SalaryHead, HeadType, HeadCategory, EntryCurrencyID, EntryCurrency, 
					              DefineCurrencyID, DefinitionCurrency, DisbusmentCurrencyID, DisbusmentCurrency, AcltExcDisbSlrHDID, RoundOption, 
                                  IntegerInDisb, IsDecimalInDisb, DecimalNo
                         ORDER BY EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetBonusRetainStructureData(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT FC.*, SH.SalaryHead, SH.HeadType, SH.HeadCategory, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo ,0.0 ContributionAmount 
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT E.SystemID EmpSystemID, rb.SalaryHeadID, SEFD.SalaryRuleMasterSystemID--, PMC.ESICMntEmpWiseCalID, PMC.ESICEligibleEmpID, PMC.ContributionAmount
										, E.PlantId
									    FROM EmployeeInformation E
										    INNER JOIN (
													   SELECT * FROM (  SELECT  *,
				                            DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                                from (
							                                                SELECT SD.SystemID,SDM.PlantID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                from SalaryInfoDefineMaster SDM
								                                                JOIN SalaryInfoDefine AS SD ON sdm.SystemID=SD.SalaryID 
                                                                                  WHERE sdm.EmpInfoSystemID IN (" + sEmpInfo + @") AND SDM.IsApproved=1
								                                                union ALL
								                                                select SD.SystemID,SDM.PlantID,EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                 from SalaryInfoBackMaster SDM
								                                                JOIN SalaryInfoBack AS SD ON sdm.SystemID=SD.SalaryID 
				                                                                 WHERE sdm.EmpInfoSystemID IN (" + sEmpInfo + @") AND SDM.IsApproved=1
			                                                ) AS SDM
			
			                                        ) AS SDM 
                                                    WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + sDate + @"' AND rnk=1 
													    ) SEFD ON E.SystemID = SEFD.EmpInfoSystemID
											INNER join SalaryRuleRetentionPmtMaster rb on rb.SalaryRuleMasterSystemID=SEFD.SalaryRuleMasterSystemID   AND rb.SalaryHeadID=sefd.SalaryHeadID

										   
								    ) AB
						    ) FC 
							    INNER JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'ESIC Voluntary'
							    INNER JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
						    WHERE FC.EmpSystemID IN (" + sEmpInfo + @") 
						    ORDER BY FC.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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


        //====================

        public void GetEmployeeWisePFValueAfterCal(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT FC.EmpSystemID, E.PlantId, FC.SalaryRuleMasterSystemID, FC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, FC.SlrCate, FC.PFMntEmpWiseCalID, 
									FC.PFEligibleEmpID, FC.IsDistribution, FC.ContributionAmount, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT PFE.EmpSystemID, PFSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, PMC.PFMntEmpWiseCalID, PMC.PFEligibleEmpID, PMC.IsDistribution, PMC.ContributionAmount, PMC.SlrCate
									    FROM PFEligibleEmployee PFE
										    INNER JOIN (
													    SELECT SLM.* FROM 
															    (
																    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																    FROM SalaryInfoDefineMaster
																    UNION 
																	    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																			    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																	    )
													    ) SLM 
														    INNER JOIN
																    (
																	    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																	    FROM 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoDefineMaster
																			    UNION 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																		    )
																		    ) A
																	    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																	    GROUP BY EmpInfoSystemID
																    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON PFE.EmpSystemID = SEFD.EmpInfoSystemID
										    INNER JOIN (
													    SELECT *, 'PF Employee Contribution' SlrCate FROM SalaryRulePF WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Employee Contribution')
													    UNION
													    (
													    SELECT *, 'PF Employer Contribution' SlrCate FROM SalaryRulePF WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Employer Contribution')
													    ) 
													    ) PFSlrHd ON SEFD.SalaryRuleMasterSystemID = PFSlrHd.SalaryRuleMasterSystemID 
										    INNER JOIN (
													    SELECT ID PFMntEmpWiseCalID, PFEligibleEmpID, IsDistributionEmp IsDistribution, EmployeeContributionAmount ContributionAmount, 'PF Employee Contribution' SlrCate 
														    FROM [dbo].[PFMonthlyEmpWiseCalculation]
													    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
													    UNION
														    (
														    SELECT ID PFMntEmpWiseCalID, PFEligibleEmpID, IsDistributionEmpr IsDistribution, EmployerContributionAmount ContributionAmount, 'PF Employer Contribution' SlrCate 
															    FROM [dbo].[PFMonthlyEmpWiseCalculation]
															    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
														    )
													    ) PMC  ON PMC.PFEligibleEmpID = PFE.ID AND PFSlrHd.SlrCate = PMC.SlrCate
								    ) AB
								    UNION
								    (
								    SELECT PFE.EmpSystemID, D.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, C.ID PFMntEmpWiseCalID, C.PFEligibleEmpID, CONVERT(BIT, 'False') IsDistribution, D.Amount ContributionAmount, '' SlrCate 
									    FROM [dbo].[PFMonthlyEmpWiseCalculation] C
											    INNER JOIN PFEligibleEmployee PFE ON C.PFEligibleEmpID = PFE.ID
											    INNER JOIN (
														    SELECT SLM.* FROM 
																    (
																	    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																	    FROM SalaryInfoDefineMaster
																	    UNION 
																		    (
																			    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																			    FROM SalaryInfoBackMaster
																		    )
																    ) SLM 
																	    INNER JOIN
																			    (
																				    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																				    FROM 
																					    (
																					    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																							    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																					    FROM SalaryInfoDefineMaster
																						    UNION 
																					    (
																					    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																							    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																					    FROM SalaryInfoBackMaster
																					    )
																					    ) A
																				    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																				    GROUP BY EmpInfoSystemID
																			    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON PFE.EmpSystemID = SEFD.EmpInfoSystemID
											    INNER JOIN
													    (
														    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
																					    (
																						    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
																						    WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0
																						    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
																						    UNION
																						    (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
																						    WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0
																						    GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
																					    ) A 
																					    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
														    UNION
														    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
																					    (
																						    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
																						    WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0
																						    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
																						    UNION
																						    (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
																						    WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0
																						    GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
																					    ) A 
																					    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
													    ) D ON D.PFMntEmpWiseCalID = C.ID 
														    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
								    )
						    ) FC 
								INNER JOIN EmployeeInformation E ON FC.EmpSystemID = E.SystemId
							    INNER JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' --AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary'
							    INNER JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
						    WHERE FC.IsDistribution = 0
								  ------AND FC.SalaryHeadID NOT IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Voluntary')
								  AND 
                                  FC.EmpSystemID IN (" + sEmpInfo + @") 
						    ORDER BY FC.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeWiseESICFValueAfterCal(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT FC.*, E.PlantId, SH.SalaryHead, SH.HeadType, SH.HeadCategory, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                     DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                     AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
                                               ELSE SH.SalaryHeadID END,
                            CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                     ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
           FROM
           (
            SELECT * FROM
             (
         	    SELECT ESICE.EmpSystemID, ESICSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, PMC.ESICMntEmpWiseCalID, PMC.ESICEligibleEmpID, PMC.ContributionAmount, PMC.SlrCate
         	    FROM ESICEligibleEmployee ESICE
         		    INNER JOIN (
         					    SELECT SLM.* FROM 
         							    (
         								    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
         									    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
         								    FROM SalaryInfoDefineMaster
         								    UNION 
         									    (
         										    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
         											    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
         										    FROM SalaryInfoBackMaster
         									    )
         					    ) SLM 
         						    INNER JOIN
         								    (
         									    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
         									    FROM 
         										    (
         										    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
         												    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
         										    FROM SalaryInfoDefineMaster
         											    UNION 
         										    (
         										    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
         												    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
         										    FROM SalaryInfoBackMaster
         										    )
         										    ) A
         									    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
         									    GROUP BY EmpInfoSystemID
         								    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
         					    ) SEFD ON ESICE.EmpSystemID = SEFD.EmpInfoSystemID
         		    INNER JOIN (
         					    SELECT *, 'ESIC Employee Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employee Contribution')
         					    UNION
         					    (
         					    SELECT *, 'ESIC Employer Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employer Contribution')
         					    ) 
         					    ) ESICSlrHd ON SEFD.SalaryRuleMasterSystemID = ESICSlrHd.SalaryRuleMasterSystemID 
         		    INNER JOIN (
         					    SELECT ID ESICMntEmpWiseCalID, ESICEligibleEmpID, EmployeeContributionAmount ContributionAmount, 'ESIC Employee Contribution' SlrCate 
         						    FROM [dbo].[ESICMonthlyEmpWiseCalculation]
         					    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
         					    UNION
         						    (
         						    SELECT ID ESICMntEmpWiseCalID, ESICEligibleEmpID, EmployerContributionAmount ContributionAmount, 'ESIC Employer Contribution' SlrCate 
         							    FROM [dbo].[ESICMonthlyEmpWiseCalculation]
         							    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
         						    )
         					    ) PMC  ON PMC.ESICEligibleEmpID = ESICE.ID AND ESICSlrHd.SlrCate = PMC.SlrCate
             ) AB
           ) FC 
         INNER JOIN EmployeeInformation E ON FC.EmpSystemID = E.SystemId
            INNER JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'ESIC Voluntary'
            INNER JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
           WHERE FC.EmpSystemID IN (" + sEmpInfo + @") 
           ORDER BY FC.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetESICStructureData(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT FC.*, SH.SalaryHead, SH.HeadType, SH.HeadCategory, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,0.0 ContributionAmount 
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT E.SystemID EmpSystemID, ESICSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID--, PMC.ESICMntEmpWiseCalID, PMC.ESICEligibleEmpID, PMC.ContributionAmount
										, ESICSlrHd.SlrCate,E.PlantId
									    FROM EmployeeInformation E
										    INNER JOIN (
													    	   SELECT * FROM (  SELECT  *,
				                                                                DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                                                                    from (
							                                                                                    SELECT SD.SystemID,SDM.PlantID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                                                    SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                                                    from SalaryInfoDefineMaster SDM
								                                                                                    JOIN SalaryInfoDefine AS SD ON sdm.SystemID=SD.SalaryID 
							                                                                                    WHERE sdm.EmpInfoSystemID IN  (" + sEmpInfo + @")  AND SDM.IsApproved=1
								                                                                                    union ALL
								                                                                                    select SD.SystemID,SDM.PlantID,EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                                                    SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                                                     from SalaryInfoBackMaster SDM
								                                                                                    JOIN SalaryInfoBack AS SD ON sdm.SystemID=SD.SalaryID 
				                                                                                                WHERE sdm.EmpInfoSystemID IN  (" + sEmpInfo + @")  AND SDM.IsApproved=1
			                                                                                    ) AS SDM
			
			                                                                            ) AS SDM 
                                                                                        WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + sDate + @"' AND rnk=1 
													    ) SEFD ON E.SystemID = SEFD.EmpInfoSystemID
										    INNER JOIN (
													    SELECT *, 'ESIC Employee Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employee Contribution')
													    UNION
													    (
													    SELECT *, 'ESIC Employer Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employer Contribution')
													    ) 
													    ) ESICSlrHd ON SEFD.SalaryRuleMasterSystemID = ESICSlrHd.SalaryRuleMasterSystemID AND ESICSlrHd.SalaryHeadID=SEFD.SalaryHeadID
										   
								    ) AB
						    ) FC 
							    LEFT JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID 
							    LEFT JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
						    WHERE ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'ESIC Voluntary' AND FC.EmpSystemID IN (" + sEmpInfo + @") 
						    ORDER BY FC.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void LoadEmpSlrDefForSlrProcessList(string sPlantID, string sEmpInfo, string sFromDate, string sToDate, out Dictionary<string, List<dicLocal>> dsDic)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {


                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SEFD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SEFD.SalaryHeadID, SH.SalaryHead
,SequenceNo=case when ISNULL(SlrDis.SequenceNo,0)=0 then SEFD.SequenceNo else ISNULL(SlrDis.SequenceNo,0) end
, SH.HeadType, SH.HeadCategory, SEFD.AmtDefinitionCurrencyID, SEFD.AmtDefinitionRate,	
                                            SEFD.EntryCurrencyID, ECR.Name AS EntryCurrency, SEFD.EntryAmount, SEFD.DefineCurrencyID, SEFD.SalaryID, SRM.CurrencyRuleSystemID,
                                            DECR.Name AS DefinitionCurrency, ISNULL(SEFD.DefineAmount,0) DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SEFD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay
                    ,SlrDis.IsPayOnWeekoffForFixedMonthDay,SlrDis.IsPayOnHolidayForFixedMonthDay

                                            , ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END, 
											SlrDis.FormulaDesID, ISNULL(SlrDis.BaseOnNetPay, Convert(bit, 'FALSE')) BaseOnNetPay, ISNULL(SlrDis.RefAbsentism, Convert(bit, 'FALSE')) RefAbsentism, 
											ISNULL(SlrDis.IsGNRBaseOthSlrHD, Convert(bit, 'FALSE')) IsGNRBaseOthSlrHD, SlrDis.GNRBaseOthSlrHDFormula, SlrDis.GNRApplicableMonthNo,
											SlrDis.IsRetain, SlrDis.IsMinWages
--, SEFD.SequenceNo
, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                            ISNULL(SlrDis.IsWorkDaysInAMonthIncHold, 0) IsWorkDaysInAMonthIncHold, SEFD.SalaryCategory 

                                            --==================
                                            ,   ISNULL(SlrDis.HasMaxLimit, Convert(bit, 'FALSE')) HasMaxLimit
                                            ,	ISNULL(SlrDis.FixedMaxLimit, Convert(bit, 'FALSE')) FixedMaxLimit
                                            ,	ISNULL(SlrDis.PercentageMaxLimit, Convert(bit, 'FALSE')) PercentageMaxLimit
                                            ,	isnull(SlrDis.MaxLimitValue,0) MaxLimitValue,SlrDis.PercentageMaxLimitSalaryHeadId	

                                            ,   ISNULL(SlrDis.HasMinLimit, Convert(bit, 'FALSE')) HasMinLimit
                                            ,	ISNULL(SlrDis.FixedMinLimit, Convert(bit, 'FALSE')) FixedMinLimit
                                            ,	ISNULL(SlrDis.PercentageMinLimit, Convert(bit, 'FALSE')) PercentageMinLimit
                                            ,	isnull(SlrDis.MinLimitValue,0) MinLimitValue,SlrDis.PercentageMinLimitSalaryHeadId	

                                            ,ISNULL(SlrDis.IsDeductionOnGross, 0) IsDeductionOnGross
                                            , FormulaDesID_NewJoin
											--,SlrDis.HasMinLimit,	SlrDis.FixedMinLimit,  SlrDis.PercentageMinLimit,	SlrDis.MinLimitValue,SlrDis.PercentageMinLimitSalaryHeadId
                                            --==================================
		                                             FROM (
		                            	
		                            	SELECT * FROM (  SELECT  *,
				                            DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                                from (
							                                                SELECT SD.SystemID,SDM.PlantID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                from SalaryInfoDefineMaster SDM
								                                                JOIN SalaryInfoDefine AS SD ON sdm.SystemID=SD.SalaryID 
                                                                                WHERE (" + sEmpInfo + @") AND SDM.IsApproved=1
								                                                union ALL
								                                                select SD.SystemID,SDM.PlantID,EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,sd.SequenceNo,sd.SalaryCategory,
								                                                SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                                                 from SalaryInfoBackMaster SDM
								                                                JOIN SalaryInfoBack AS SD ON sdm.SystemID=SD.SalaryID 
                                                                                WHERE (" + sEmpInfo + @") AND SDM.IsApproved=1
							
			                                                ) AS SDM
			
			                                        ) AS SDM 
                                                    WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + sToDate + @"' AND rnk=1 
		                                   ) SEFD 


			                            LEFT JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            LEFT JOIN SalaryHead SH ON SEFD.SalaryHeadID = SH.SalaryHeadID 
			                            LEFT JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SEFD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, g.SalaryHeadID, 'Gen' RuleType, h.PartOfNetPay IsNetPayEffect, FixedMonthDayValue,IsPayOnHolidayForFixedMonthDay,IsPayOnWeekoffForFixedMonthDay, IsMonthDay,  
						                                IsMonthWorkDay, IsFixedDisbus, BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo,                                                        
                                                        FormulaDesID, IsRetain, IsMinWages ,IsWorkDaysInAMonthIncHold

                                                    ,HasMaxLimit,	FixedMaxLimit,	PercentageMaxLimit,	MaxLimitValue,
												   PercentageMaxLimitSalaryHeadId,	
												   HasMinLimit,	FixedMinLimit,  PercentageMinLimit,	MinLimitValue,	
												   PercentageMinLimitSalaryHeadId,isnull(Sequenceno,0) Sequenceno,Convert(bit, 'FALSE') IsDeductionOnGross,'' FormulaDesID_NewJoin

												   FROM SalaryRuleGeneral  g
												   left join SalaryHead h on h.SalaryHeadID=g.SalaryHeadID
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, g.SalaryHeadID, 'Abs' RuleType, h.PartOfNetPay IsNetPayEffect, FixedMonthDayValue,Convert(bit, 'FALSE') IsPayOnHolidayForFixedMonthDay,Convert(bit, 'FALSE') IsPayOnWeekoffForFixedMonthDay, IsMonthDay, 
						                                 IsMonthWorkDay, IsFixedDisbus, Convert(bit, isnull(BaseOnNetPay,0)) BaseOnNetPay, Convert(bit, 'FALSE') RefAbsentism, Convert(bit, 'FALSE') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, '' GNRApplicableMonthNo,
                                                         FormulaDesID, Convert(bit, 'FALSE') IsRetain, Convert(bit, 'FALSE') IsMinWages  ,Convert(bit, 'FALSE') IsWorkDaysInAMonthIncHold

                                                    ,Convert(bit, 'FALSE') HasMaxLimit,	Convert(bit, 'FALSE') FixedMaxLimit,	Convert(bit, 'FALSE') PercentageMaxLimit, 0	MaxLimitValue,
												   '' PercentageMaxLimitSalaryHeadId,	
												   Convert(bit, 'FALSE') HasMinLimit,	Convert(bit, 'FALSE') FixedMinLimit,  Convert(bit, 'FALSE') PercentageMinLimit,	0 MinLimitValue,	
												   '' PercentageMinLimitSalaryHeadId,isnull(Sequenceno,0) Sequenceno, isnull(IsDeductionOnGross,0) IsDeductionOnGross,FormulaDesID_NewJoin

												   FROM SalaryRuleAbsenteeism  g
												   left join SalaryHead h on h.SalaryHeadID=g.SalaryHeadID
                                                 )
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SEFD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SEFD.SalaryHeadID = SRDSM.SalaryHeadID
                                        WHERE E.DOJ <= '" + sToDate + @"' AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                               OR E.DOS = '' OR E.DOS = '01/01/1901')
                                              AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + sToDate + @"'
                                    ) A 
                                  WHERE (" + sEmpInfo + @") ";
                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSql += @"
                                AND PlantID = '" + sPlantID + @"' ";
                }

                strSql += @"
                            ORDER BY EmpInfoSystemID, SequenceNo, HeadType DESC";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out DataSet dsRef, false, "1");

                List<dicLocal> dsRefMain = new List<dicLocal>();
                if (dsRef.Tables[0].Rows.Count > 0)
                    dsRefMain = dsRef.Tables[0].ToList<dicLocal>();

                string EmpInfoSystemID = "";
                dsDic = new Dictionary<string, List<dicLocal>>();
                List<dicLocal> RowData = new List<dicLocal>();
                for (int i = 0; i < dsRefMain.Count; i++)
                {
                    if (EmpInfoSystemID != dsRefMain[i].EmpInfoSystemID)
                    {
                        RowData = new List<dicLocal>();
                        dsDic.Add(dsRefMain[i].EmpInfoSystemID, RowData);
                    }
                    RowData.Add(dsRefMain[i]);


                    EmpInfoSystemID = dsRefMain[i].EmpInfoSystemID;
                }
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

        public void LoadEmpSlrDefForSlrProcess(string sPlantID, string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                                            SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID, SRM.CurrencyRuleSystemID,
                                            DECR.Name AS DefinitionCurrency, ISNULL(SD.DefineAmount,0) DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, ISNULL(SlrDis.IsBankPayment, 'TRUE') IsBankPayment, 
                                            ISNULL(SlrDis.IsCashPayment, 'TRUE') IsCashPayment, ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END, 
											SlrDis.FormulaDesID, ISNULL(SlrDis.BaseOnNetPay, Convert(bit, 'FALSE')) BaseOnNetPay, ISNULL(SlrDis.RefAbsentism, Convert(bit, 'FALSE')) RefAbsentism, 
											ISNULL(SlrDis.IsGNRBaseOthSlrHD, Convert(bit, 'FALSE')) IsGNRBaseOthSlrHD, SlrDis.GNRBaseOthSlrHDFormula, SlrDis.GNRApplicableMonthNo,
											SlrDis.IsRetain, SlrDis.IsMinWages, SD.SequenceNo, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                            ISNULL(SlrDis.IsWorkDaysInAMonthIncHold, 0) IsWorkDaysInAMonthIncHold, SD.SalaryCategory 
		                            FROM (
                                          SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
	                                             AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory
                                          FROM SalaryInfoDefine
                                            UNION
                                          (
                                           SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
		                                          AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory                   
                                           FROM SalaryInfoBack
                                          )
                                         ) SD
										INNER JOIN 
												(
												 SELECT SLM.* FROM 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoDefineMaster
                                                             UNION 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoBackMaster
                                                            )
                                                            ) SLM 
	                                                            INNER JOIN
			                                                            (
			                                                             SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
			                                                             FROM 
				                                                             (
				                                                               SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
						                                                              IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
				                                                               FROM SalaryInfoDefineMaster
						                                                           UNION 
				                                                              (
					                                                            SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
							                                                           IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
					                                                            FROM SalaryInfoBackMaster
				                                                              )
				                                                             ) A
				                                                            WHERE IsApproved = 1 AND EffectiveDate <= '" + sToDate + @"'
				                                                            GROUP BY EmpInfoSystemID
			                                                            ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
												) SEFD ON SD.SalaryID = SEFD.SystemID
			                            INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary'
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Gen' RuleType, IsGNRNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, 
						                                ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment, IsMonthWorkDay, IsFixedDisbus, BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo,
                                                        
                                                        FormulaDesID, IsRetain, IsMinWages ,IsWorkDaysInAMonthIncHold
												   FROM SalaryRuleGeneral
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Abs' RuleType, IsAbsNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
						                                 IsMonthWorkDay, IsFixedDisbus, Convert(bit, 'FALSE') BaseOnNetPay, Convert(bit, 'FALSE') RefAbsentism, Convert(bit, 'FALSE') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, '' GNRApplicableMonthNo,
                                                         FormulaDesID, Convert(bit, 'FALSE') IsRetain, Convert(bit, 'FALSE') IsMinWages  ,Convert(bit, 'FALSE') IsWorkDaysInAMonthIncHold
												   FROM SalaryRuleAbsenteeism
                                                 )
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
                                        WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                               OR E.DOS = '' OR E.DOS = '01/01/1901')
                                              AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + sToDate + @"'
                                    ) A 
                                  WHERE (" + sEmpInfo + @") ";
                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSql += @"
                                AND PlantID = '" + sPlantID + @"' ";
                }

                strSql += @"
                            ORDER BY EmpInfoSystemID, SequenceNo, HeadType DESC";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
}
