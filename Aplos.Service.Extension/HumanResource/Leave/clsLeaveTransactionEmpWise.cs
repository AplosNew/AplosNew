using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace OTSBD
{
    public class clsLeaveTransactionEmpWise
    {
        public clsLeaveTransactionEmpWise()
        {
            // TODO: Add constructor logic here
        }

        public void GetFiscalYearlCmb(string sGroupID, string sCompanyId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT [FiscalYearId],FY.FiscalYearCode,FY.FiscalYearName,FY.StartDate,FY.EndDate
                                    ,convert(varchar,FY.StartDate)+','+convert(varchar,FY.EndDate) CombinedDateRange
                                  ,[CompanyId],CFY.[Active]     
                                  FROM [SCS].[CompanyFiscalYear] AS CFY INNER JOIN  
                                  SCS.FiscalYear FY ON CFY.FiscalYearId = FY.Id 
                                  WHERE CFY.CompanyId = '" + sCompanyId + @"' AND CFy.Active =1";

                strSQL = strSQL + "   ORDER BY FY.StartDate desc";

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

        public void GetYearlyCalendarInfoCmb(string sGroupID, string sPlantID, string sSystemID, out DataSet dsRef)
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

                if (sSystemID.Trim() != "")
                {
                    strSQL = strSQL + " WHERE Id = '" + sSystemID + @"'";
                }

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

        public void GetYearlyCalendarInfoCmbDateWise(string sGroupID, string sPlantID, string sFromDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (
                                     SELECT Id, YearNo, REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate,
                                        REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate
                                      FROM YearlyCalendar
                                     WHERE PlantID = '" + sPlantID + @"' AND CompanyGroupId = '" + sGroupID + @"'
                                           AND '" + sFromDate + @"' BETWEEN FromDate AND ToDate
                                    ) AS A";

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

        public void GetYearlyCalendarWiseLeavePolicyNameCmb(string designationID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT SystemID, PolicyName, DefaultPolicy FROM dbo.LeavePolicyMaster
                //            WHERE SystemID IN (SELECT LvPolMstSystemID FROM dbo.LvPolMstYearCalendar WHERE YrCalSystemID = '" + strYrCalSystemID + @"')
                //            ORDER BY PolicyName";
                strSQL = @"SELECT SystemID, PolicyName, DefaultPolicy FROM dbo.LeavePolicyMaster
                            ORDER BY PolicyName";

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

        public void XLoadLvPolicyWiseLeaveTypeCmb(string sGroupID, string sPlantID, string strLvPolSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LT.SystemID, LT.LeaveName FROM LeavePolicyDetail LPD
                                LEFT JOIN LeaveType LT ON LPD.LTSystemID = LT.SystemID
                            WHERE LPD.PlantID = '" + sPlantID + @"' AND LPD.GroupID = '" + sGroupID + @"' AND LPD.LPMSystemID = '" + strLvPolSysID + @"' AND LPD.IsActive = 1
                            UNION
                            (SELECT SystemID, LeaveName FROM LeaveType
                                    WHERE LeaveType IN ('Leave Without Pay','Encash') AND IsActive = 1)
                            ORDER BY LT.LeaveName";

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

        public void XLoadLvPolicyWiseLeaveTypeCmb(string sGroupID, string sPlantID, string strLvPolSysID, string empSystemId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LT.Id, LT.UserName LeaveName FROM LeavePolicyDetail LPD
                                LEFT JOIN LeaveType LT ON LPD.LTSystemID = LT.Id
                            WHERE LPD.PlantID = '" + sPlantID + @"' AND LPD.GroupID = '" + sGroupID + @"' AND LPD.LPMSystemID = '" + strLvPolSysID + @"' AND LPD.IsActive = 1
							AND  LT.Id NOT IN (Select PLT.LeaveTypeID From [dbo].[ESICEligibleEmployee] AS E
							LEFT JOIN [dbo].[ESICPolicyLeaveType] AS PLT  ON E.ESICMstID=PLT.ESICPolicyMasterID
							Where EmpSystemID='" + empSystemId + @"')
							--UNION
							--(SELECT Id, UserName LeaveName FROM LeaveType
							--        WHERE LeaveType IN('Leave Without Pay', 'Earn'))
							--ORDER BY LT.UserName
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

        public void GetESICEligibleEmployee(string empSystemId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ESICEligibleEmployee WHERE EmpSystemID='" + empSystemId + "'  AND IsActive=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function
        public void GetCalYearInfo(string CalYearId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"  select * from YearlyCalendar WHERE ID='" + CalYearId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function

        public void XXLoadLvPolicyWiseLeaveTypeCmb(string sGroupID, string sPlantID, string strLvPolSysID, string empSystemId, out DataSet dsRef)
        {
            DataSet dataSet = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                GetESICEligibleEmployee(empSystemId, out dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    strSQL = @"SELECT LT.ID, LT.UserName LeaveName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID FROM dbo.ESICPolicyLeaveType AS EPLT
						   LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
						   WHERE
						   EPLT.LeaveTypeID IN
						    (
						      SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
							  LEFT JOIN MST.DesignationMaster AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
							  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
							  WHERE EI.SystemID='" + empSystemId + @"' AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"'
						    )
							AND
							EPLT.ESICPolicyMasterID IN (
							 SELECT DM.ESICPolicyMasterID FROM MST.DesignationMaster DM
							 WHERE DM.DesignationId IN (
							  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + empSystemId + @"'
							  )
							)";
                }
                else
                {
                    strSQL = @"SELECT LT.ID, LT.UserName LeaveName FROM LeaveType LT
                           LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                           LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                           LEFT JOIN MST.DesignationMaster DM ON DM.LeavePolicyMasterId=LPM.SystemID
                           LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId
                           LEFT JOIN ESICEligibleEmployee EE ON EE.EmpSystemID=EI.SystemId
                           WHERE EI.SystemID='" + empSystemId + @"' AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"' AND LT.IsGeneral = 1";
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

        public void LoadLvPolicyWiseLeaveTypeCmb(string sGroupID, string sPlantID, string strLvPolSysID, string empSystemId, out DataSet dsRef)
        {
            DataSet dataSet = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                GetESICEligibleEmployee(empSystemId, out dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    strSQL = @"SELECT LT.ID, LT.UserName LeaveName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID FROM dbo.ESICPolicyLeaveType AS EPLT
                  LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                  WHERE
                  EPLT.LeaveTypeID IN
                   (
                     SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                  LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                  LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                  WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                  WHERE EI.SystemID='" + empSystemId + @"' AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"'
                   )
                AND
                EPLT.ESICPolicyMasterID IN (
                 SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                 LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                 WHERE DC.PlantId='" + sPlantID + @"') DM
                 WHERE DM.DesignationId IN (
                  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + empSystemId + @"'
                  )
                )";
                }
                else
                {
                    strSQL = @"SELECT LT.ID, LT.UserName LeaveName FROM LeaveType LT
                                    LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                    LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + sPlantID + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                    LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId
                                    LEFT JOIN ESICEligibleEmployee EE ON EE.EmpSystemID=EI.SystemId
                                    WHERE EI.SystemID='" + empSystemId + @"' AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"' AND LT.IsGeneral = 1";
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

        public void XLoadLeaveTypeCmb(string sGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID, LeaveName FROM LeaveType
                                    WHERE GroupID = '" + sGroupID + @"' AND IsActive = 1 AND
                                            LeaveType IN ('Leave Without Pay','Encash','General')
                            ORDER BY LeaveName";

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

        public void LoadLeaveTypeCmb(string sGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Id, UserName LeaveName FROM LeaveType
                                    WHERE CompanyGroupId = '" + sGroupID + @"'
                                            AND LeaveType IN ('Leave Without Pay','Encash','General')
                            ORDER BY UserName";

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
        public void GetLeaveAllocat(string sGroupID, string sPlantID, string EmpSystemID, string calYearId, out DataSet dsRef)
        {
            DataSet dataSet = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsCalYear = null;
            try
            {
               // string pYear = string.Empty;
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;
                GetESICEligibleEmployee(EmpSystemID, out dataSet);
                GetCalYearInfo(calYearId, out dsCalYear);
                if(dsCalYear.Tables[0].Rows.Count>0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                    
                }
                else
                {
                    throw new Exception("No Year found...");
                }


                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    strSQL = @"SELECT	els.CalanderYearID,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                        ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId='" + calYearId + @"') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m 
where  (FromDate between '"+_FromDate+ @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																			select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1  and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                            group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + calYearId + @"'
                                             AND els.LeaveTypeId IN ( --IN
                                            
                                            
                                            SELECT LT.ID--, LT.UserName LeaveName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID 
                                            FROM dbo.ESICPolicyLeaveType AS EPLT
                                                              LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                                              WHERE
                                                              EPLT.LeaveTypeID IN
                                                               (
                                                                 SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                                              LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                                              LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                                              WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                                              LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                                              WHERE EI.SystemID= '" + EmpSystemID + @"'     AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"'
                                                               )
                                                            AND
                                                            EPLT.ESICPolicyMasterID IN (
                                                             SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                                             LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                                             WHERE DC.PlantId='" + sPlantID + @"') DM
                                                             WHERE DM.DesignationId IN (
                                                              SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID= '" + EmpSystemID + @"'    
                                                              )
                                                            )
                                            				)--IN";
                }
                else
                {
                    strSQL = @"SELECT	els.CalanderYearID,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         --ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId='" + calYearId + @"') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                            where  (FromDate between '" + _FromDate + @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
                                                    group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + calYearId + @"'
                                              AND els.LeaveTypeId not IN 
                                            (select id from LeaveType where IsESIC=1 and IsGeneral=0)
                                                    ";
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
        public void xxGetLeaveAllocat(string sGroupID, string sPlantID, string EmpSystemID, string strSystemID, out DataSet dsRef)
        {
            DataSet dataSet = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                GetESICEligibleEmployee(EmpSystemID, out dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    strSQL = @"SELECT	els.CalanderYearID,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         --ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         ISNULL(els.CarryForward, 0)+isnull(els.CarryForwardOpeningBalance,0) PreviousYearCarryForward,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                         FROM trn.EmployeeLeaveSummary els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																			select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + strSystemID + @"'
                                             AND els.LeaveTypeId IN ( --IN
                                            
                                            
                                            SELECT LT.ID--, LT.UserName LeaveName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID 
                                            FROM dbo.ESICPolicyLeaveType AS EPLT
                                                              LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                                              WHERE
                                                              EPLT.LeaveTypeID IN
                                                               (
                                                                 SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                                              LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                                              LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                                              WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                                              LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                                              WHERE EI.SystemID= '" + EmpSystemID + @"'     AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"'
                                                               )
                                                            AND
                                                            EPLT.ESICPolicyMasterID IN (
                                                             SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                                             LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                                             WHERE DC.PlantId='" + sPlantID + @"') DM
                                                             WHERE DM.DesignationId IN (
                                                              SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID= '" + EmpSystemID + @"'    
                                                              )
                                                            )
                                            				)--IN";
                }
                else
                {
                    strSQL = @"SELECT	els.CalanderYearID,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         --ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         ISNULL(els.CarryForward, 0)+isnull(els.CarryForwardOpeningBalance,0) PreviousYearCarryForward,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                         FROM trn.EmployeeLeaveSummary els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + strSystemID + @"'";
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
        public void xGetLeaveAllocat(string sGroupID, string sPlantID, string EmpSystemID, string strSystemID, out DataSet dsRef)
        {
            DataSet dataSet = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                GetESICEligibleEmployee(EmpSystemID, out dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                { 
                    strSQL = @"SELECT	els.CalanderYearID,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                         ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
                                         ISNULL(ltrn.ldays, 0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
                                         ISNULL(tav.av, 0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                         FROM trn.EmployeeLeaveSummary els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																			select COUNT(d.systemId) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + strSystemID + @"'
                                             AND els.LeaveTypeId IN ( --IN
                                            
                                            
                                            SELECT LT.ID--, LT.UserName LeaveName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID 
                                            FROM dbo.ESICPolicyLeaveType AS EPLT
                                                              LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                                              WHERE
                                                              EPLT.LeaveTypeID IN
                                                               (
                                                                 SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                                              LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                                              LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                                              WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                                              LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                                              WHERE EI.SystemID= '" + EmpSystemID + @"'     AND EI.GroupID='"+sGroupID+@"' AND EI.PlantID='" + sPlantID + @"'
                                                               )
                                                            AND
                                                            EPLT.ESICPolicyMasterID IN (
                                                             SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                                             LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                                             WHERE DC.PlantId='" + sPlantID + @"') DM
                                                             WHERE DM.DesignationId IN (
                                                              SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID= '" + EmpSystemID + @"'    
                                                              )
                                                            )
                                            				)--IN";
                }
                else
                {
                    strSQL = @"SELECT	els.CalanderYearID,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                         ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
                                         ISNULL(ltrn.ldays, 0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
                                         ISNULL(tav.av, 0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                         FROM trn.EmployeeLeaveSummary els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																			select COUNT(d.systemId) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + strSystemID + @"'";
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

        //public void GetLeaveAllocat(string sGroupID, string sPlantID, string EmpSystemID, string strSystemID, out System.Data.DataSet dsRef)
        //{
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"SELECT YrCalSystemID, EmpSystemID, LvPolDetailsSystemID, ISNULL(LeaveDays, 0) LeaveDays,
        //                            ISNULL(AppliedLeave, 0) Applied, ISNULL(AvailedLeave, 0) Availed
        //                    FROM dbo.LeaveAllocation
        //                    WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
        //                          AND EmpSystemID = '" + EmpSystemID + @"' AND YrCalSystemID = '" + strSystemID + @"'";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        public void XGetLvPolicyWiseLeaveType(string sGroupID, string sPlantID, string strLvPolSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LPM.SystemID, LPD.SystemID LvPolDetailsSystemID, LPD.LTSystemID, LT.LeaveName, LT.LeaveDescription, LPD.LeaveDays, 0 Applied,
                                        0 AppliedBalance, 0 Availed, 0 Balance
                                FROM dbo.LeavePolicyMaster LPM
                                    LEFT JOIN dbo.LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID AND LPD.IsActive = 1
                                    LEFT JOIN dbo.LeaveType LT ON LPD.LTSystemID = LT.SystemID
                                WHERE LPM.PlantID = '" + sPlantID + @"' AND LPM.GroupID = '" + sGroupID + @"'
                                        AND LPM.SystemID = '" + strLvPolSysID + @"'";

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

        public void GetEmployeeInfo(string sGroupID, string sPlantID, string strLvPolSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * FROM dbo.EmployeeInformation
                                    WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                    --AND LPM.SystemID = '" + strLvPolSysID + @"'
                                        ";

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

        public void GetLvPolicyWiseLeaveType(string sGroupID, string sPlantID, string strLvPolSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT LPM.SystemID, LPD.SystemID LvPolDetailsSystemID, LPD.LTSystemID, LT.UserName LeaveName, LT.Description LeaveDescription, LPD.LeaveDays, 0 Applied,
                //                    0 AppliedBalance, 0 Availed, 0 Balance , 0 CurrentAllocation , 0 PreviousYearCarryForward
                //                    FROM dbo.LeavePolicyMaster LPM
                //                    LEFT JOIN dbo.LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID AND LPD.IsActive = 1
                //                    LEFT JOIN dbo.LeaveType LT ON LPD.LTSystemID = LT.Id
                //                    WHERE LPM.PlantID = '" + sPlantID + @"' AND LPM.GroupID = '" + sGroupID + @"'
                //                    AND LPM.SystemID = '" + strLvPolSysID + @"'";

                strSQL = @"select		 els.CalanderYearID,
                                         els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
                                         LT.UserName LeaveName,
										 --ltrn.LeaveDays appliedLeaveDays,
                                         LT.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
                                         ISNULL(els.AppliedDays, 0) Applied,
                                         ISNULL(els.AvailedDays, 0) Availed,
                                         ISNULL(els.DaysCanBeSanctioned, 0) - ISNULL(els.AppliedDays, 0) Balance
                                         FROM trn.EmployeeLeaveSummary els
										 --left outer join dbo.LeaveTransaction ltrn on ltrn.EmpSystemID = els.EmployeeId
                                         left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
                                         left outer join dbo.LeavePolicyDetail ltd on ltd.LTSystemID = lt.Id
										 left outer join dbo.LeavePolicyMaster LPM on  lpm.SystemID = ltd.LPMSystemID
                                    WHERE LPM.PlantID = '" + sPlantID + @"' AND LPM.GroupID = '" + sGroupID + @"'
                                    AND LPM.SystemID = '" + strLvPolSysID + @"'";

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

        public void XLoadLvTransInfoForEmpGrd(string sGroupID, string sPlantID, string strEmpSysID, string strFromDate, string strToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LvT.SystemID, LT.LeaveName, LT.LeaveDescription,
                                    REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                                    REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID
                             FROM LeaveTransaction LvT
                                    LEFT JOIN LeaveType LT ON LvT.LTSystemID = LT.SystemID
                            WHERE LvT.PlantID = '" + sPlantID + @"' AND LvT.GroupID = '" + sGroupID + @"' AND LvT.EmpSystemID = '" + strEmpSysID + @"'
                                    AND (LvT.FromDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'
                                        OR LvT.ToDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                            ORDER BY LvT.FromDate DESC, LvT.ToDate DESC, LT.LeaveName";

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

        //public void GetOffDate(string sGroupID, string sPlantID, string strSystemID, string strLPMSysterID, out System.Data.DataSet dsRef)
        //{
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"SELECT * FROM SCS.OffDayDetail
        //                    WHERE YrCalSystemID = '" + strSystemID + @"' AND
        //                            AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + "'";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        public void LoadLvTransInfoForEmpGrd(string sGroupID, string sPlantID, string strEmpSysID, string strFromDate, string strToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LvT.SystemID, LT.UserName LeaveName, LT.Description LeaveDescription, LvT.IsApproved,
                                    REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                                    REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID,Lvt.LeaveDayType,Lvt.IsCancel
                                    ,LvT.IsCancel
                             FROM LeaveTransaction LvT
                                    LEFT JOIN LeaveType LT ON LvT.LTSystemID = LT.Id
                            WHERE LvT.PlantID = '" + sPlantID + @"' AND LvT.GroupID = '" + sGroupID + @"' AND LvT.EmpSystemID = '" + strEmpSysID + @"'
                                    AND (LvT.FromDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'
                                        OR LvT.ToDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                            AND LeaveDays<>0.5 AND LT.LeaveType<>'Maternity'
                            ORDER BY LvT.FromDate DESC, LvT.ToDate DESC, LT.UserName";

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

        public void LoadLvTransInfoForEmpGrdForHalfDay(string sGroupID, string sPlantID, string strEmpSysID, string strFromDate, string strToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LvT.SystemID, LT.UserName LeaveName, LT.Description LeaveDescription, LvT.IsApproved,
                                    REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                                    REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID,Lvt.LeaveDayType,Lvt.IsCancel
                             FROM LeaveTransaction LvT
                                    LEFT JOIN LeaveType LT ON LvT.LTSystemID = LT.Id
                            WHERE LvT.PlantID = '" + sPlantID + @"' AND LvT.GroupID = '" + sGroupID + @"' AND LvT.EmpSystemID = '" + strEmpSysID + @"'
                                    AND (LvT.FromDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'
                                        OR LvT.ToDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                            AND LeaveDays=0.5 AND LT.LeaveType<>'Maternity'
                            ORDER BY LvT.FromDate DESC, LvT.ToDate DESC, LT.UserName";

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

        public void GetSysIdWiseEmpBasicInfoInformationForLeave(string sGroupID, string sCompanyID, string sPlantID, string strKey, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                //str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                //                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                //                            where ProbationRP='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSql = @"SELECT * FROM ( select emp.SystemId EmployeeID
                                                ,EmployeeName
                                                ,EmployeeCode
                                                --,NationalID
                                                ,REPLACE(CONVERT(VARCHAR(11), DOJ, 113), ' ', '-') DOJ
                                                ,Dsgg.UserName GivenDesignation
                                                ,E.UserName as Entity
							                    ,LT.UserName LeaveName
                                                --, LT.Description LeaveDescription
                                                ,REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate
                                                ,REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate
                                                ,LvT.LeaveDays
                                                ,Lvt.SystemID LvTrnMsID
                                                --,LvT.LvReason AS Reason
                                                --,LvT.ComAssignLvSystemID
                                                 FROM
							                     dbo.EmployeeInformation emp
							                     LEFT outer JOIN dbo.LeaveTransaction LvT on LvT.EmpSystemID = emp.SystemId
                                                 LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
							                     LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                 LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
                                                 LEFT outer JOIN dbo.LeaveType LT ON LvT.LTSystemID = LT.Id
							                     LEFT outer JOIN [HKP].Designation AS Dsg ON Dsg.ID = Emp.DesignationSystemID
							                     LEFT outer JOIN [HKP].Designation AS Dsgg ON Dsgg.ID = Emp.GivenDesignationID
                                                 WHERE emp.GroupID = '" + sGroupID + @"'
                                                  AND IsNull(Lvt.IsApproved,0) = 0
							                     AND isnull(LvT.SystemID,'')<> ''
                                                 AND LvT.IsCancel=0
                                                 AND emp.PlantID = '" + sPlantID + @"' ) A";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " where " + strKey;
                }

                strSql = strSql + " Order By EmployeeName";

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

        //public void GetSysIdWiseEmpBasicInfoInformationForLeave(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        //{
        //    ConnectionManager.DAL.ConManager objCon;
        //    string strSql = "";
        //    try
        //    {
        //        var str = "";
        //        //str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
        //        //                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
        //        //                            where ProbationRP='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

        //        strSql = @"SELECT emp.SystemId,Lvt.SystemID LvTrnMsID, emp.EmployeeCode,emp.BudgetCode,emp.EmployeeName,emp.EmpType,emp.NationalID,Dsgg.UserName GivenDesignation,
        //                     REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 113), ' ', '-') DOJ,
        //LT.UserName LeaveName, LT.Description LeaveDescription,
        //                     REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
        //                     REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID
        //                     FROM
        //dbo.EmployeeInformation emp
        //LEFT outer JOIN dbo.LeaveTransaction LvT on LvT.EmpSystemID = emp.SystemId
        //                     LEFT outer JOIN dbo.LeaveType LT ON LvT.LTSystemID = LT.Id
        //LEFT outer JOIN [HKP].Designation AS Dsg ON Dsg.ID = Emp.DesignationSystemID
        //LEFT outer JOIN [HKP].Designation AS Dsgg ON Dsgg.ID = Emp.GivenDesignationID
        //                     WHERE  IsNull(Lvt.IsApproved,0) = 0
        //and isnull(LvT.SystemID,'')<> ''
        //AND emp.GroupID = '" + sGroupID + @"'
        //                     AND emp.PlantID = '" + sPlantID + @"'
        //                     --Order LvT.FromDate DESC, LvT.ToDate DESC, LT.UserName";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function

        public void GetSysIdWiseEmpBasicInfoInformationForLeave(string sGroupID, string sPlantID, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where LeaveApproval='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSql = @"SELECT emp.SystemId EmployeeID,Lvt.SystemID LvTrnMsID, emp.EmployeeCode,emp.BudgetCode,emp.EmployeeName,emp.EmpType,emp.NationalID,Dsgg.UserName GivenDesignation,E.UserName as Entity,
                             REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 113), ' ', '-') DOJ,
							 LT.UserName LeaveName, LT.Description LeaveDescription,
                             REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                             REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID,LVT.LTSystemID
                             FROM
							 dbo.EmployeeInformation emp
							 LEFT outer JOIN dbo.LeaveTransaction LvT on LvT.EmpSystemID = emp.SystemId
                             LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
							 LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                             LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
                             LEFT outer JOIN dbo.LeaveType LT ON LvT.LTSystemID = LT.Id
							 LEFT outer JOIN [HKP].Designation AS Dsg ON Dsg.ID = Emp.DesignationSystemID
							 LEFT outer JOIN [HKP].Designation AS Dsgg ON Dsgg.ID = Emp.GivenDesignationID
                             WHERE  IsNull(Lvt.IsApproved,0) = 0
							 AND ISNULL(LvT.SystemID,'')<> ''
                             AND LvT.IsCancel=0
							 AND emp.GroupID = '" + sGroupID + @"'
                             AND emp.PlantID = '" + sPlantID + @"'" + str;

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

        public void LoadLvTransInfoDetailsForEmpGrd(string sGroupID, string sPlantID, string strLvTrnsSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT  B.LvTrnsSystemID LV, B.WorkDate, B.WeekOff, B.HoliDay, B.LeaveStatus, B.AttendanceStatus
                            FROM
                                (SELECT LV.EmpSystemID, LVD.SystemID, LVD.LvTrnsSystemID, REPLACE(CONVERT(VARCHAR(11), LVD.WorkDate, 113),' ','-') WorkDate,
				                                                            WeekOff = CASE WHEN LVD.DayType = 'W' THEN 'YES'
								                                                            WHEN LVD.DayType = 'WH' THEN 'YES'
								                                                            WHEN LVD.DayType = 'HW' THEN 'YES'
								                                                            ELSE '' END,
				                                                            HoliDay = CASE WHEN LVD.DayType = 'H' THEN 'YES'
								                                                            WHEN LVD.DayType = 'WH' THEN 'YES'
								                                                            WHEN LVD.DayType = 'HW' THEN 'YES'
								                                                            ELSE '' END,
				                                                            LVD.LeaveStatus,
				                                                            ISNULL(AD.DayStatus, '') AS AttendanceStatus
                                FROM dbo.LeaveTransaction LV
                                    INNER JOIN dbo.LeaveTransactionDetails LVD ON LV.SystemID = LVD.LvTrnsSystemID
                                    LEFT JOIN dbo.AttdnProcessData AD ON LV.EmpSystemID = AD.EmpSystemID AND LVD.WorkDate = AD.WorkDate
                                WHERE LV.PlantID = '" + sPlantID + @"' AND LV.GroupID = '" + sGroupID + @"') B ";

                if (strLvTrnsSystemID != "")
                {
                    strSQL = strSQL + @"WHERE " + strLvTrnsSystemID + @" ";
                }

                strSQL = strSQL + @"ORDER BY B.WorkDate";

                //WHERE B.LvTrnsSystemID = '" + strLvTrnsSystemID + @"'
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

        public void LoadSelectedLeaveInfo(string sGroupID, string strLTSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM LeaveType
                                WHERE CompanyGroupId = '" + sGroupID + @"' AND Id = '" + strLTSysID + @"'
                            ORDER BY UserName";

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

        public bool CheckLvTransactionInSameDate(string sGroupID, string sPlantID, string strLvTransSysID, string strEmpSysID, string strFromDate, string strToDate)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            DataSet dsRef = null;

            bool blnStatus = false;

            try
            {
                if (strEmpSysID != "")
                {
                    strSql = @"SELECT * FROM dbo.LeaveTransaction
                               WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                    AND (SystemID <> '" + strLvTransSysID + @"') AND (EmpSystemID = '" + strEmpSysID + @"')
                                        AND ((FromDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                                            OR (ToDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'))";
                }
                else
                {
                    strSql = @"SELECT * FROM dbo.LeaveTransaction
                               WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                        AND ((FromDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                                            OR (ToDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'))";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public bool CheckCurrentDateLeaveTypeEntry(string sGroupID, string sPlantID, string strLvTransSysID, string strLvTypeId, string strEmpSysID)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            DataSet dsRef = null;

            bool blnStatus = false;

            try
            {
                if (strEmpSysID != "")
                {
                    strSql = @"SELECT * FROM dbo.LeaveTransaction
                               WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                    AND (SystemID <> '" + strLvTransSysID + @"') AND (LTSystemID = '" + strLvTypeId + @"') AND (EmpSystemID = '" + strEmpSysID + @"')
                                        AND CONVERT(DATE, DateAdded) ='" + DateTime.Now.ToString("yyyy-MM-dd") + @"'";
                }
                else
                {
                    strSql = @"SELECT * FROM dbo.LeaveTransaction
                               WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                        AND CONVERT(DATE, DateAdded) ='" + DateTime.Now.ToString("yyyy-MM-dd") + @"'";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public void GetMaxLeaveAtaTime(string sGroupID, string sPlantID, string strLPMSystemID, string strLvTypeId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            dsRef = null;

            try
            {
                if (strLPMSystemID != "")
                {
                    strSql = @"SELECT ISNULL(IsExcessAllow,0) IsExcessAllow, ISNULL(IsSubjectToApproval,0)IsSubjectToApproval, ISNULL(MaxAllocationLimit,0)MaxAllocationLimit
                               FROM dbo.LeavePolicyDetail
                               WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                               AND (LPMSystemID = '" + strLPMSystemID + @"') AND (LTSystemID = '" + strLvTypeId + @"')";
                }
                //else
                //{
                //    strSql = @"SELECT * FROM dbo.LeaveTransaction
                //               WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                //                        AND CONVERT(DATE, DateAdded) ='" + DateTime.Now.ToString("yyyy-MM-dd") + @"'";
                //}

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

        public bool CheckLvTransactionInSameDateCompAssi(string sGroupID, string sPlantID, string strFromDate, string strToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            bool blnStatus = false;

            try
            {
                strSql = @"SELECT * FROM dbo.LeaveTransaction
                                WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                        AND ((FromDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                                            OR (ToDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'))";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public bool CheckLvLeaveTransIsAvailedInTheDateRangeForEmp(string sGroupID, string sPlantID, string strEmpSysID, string strFromDate, string strToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSQL = string.Empty;

            bool blnStatus = false;

            try
            {
                if (strEmpSysID != "")
                {
                    strSQL = @"SELECT LT.SystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate FROM LeaveTransactionDetails LTD
		                            INNER JOIN dbo.LeaveTransaction LT ON LTD.LvTrnsSystemID = LT.SystemID
                            WHERE LT.PlantID = '" + sPlantID + @"' AND LT.GroupID = '" + sGroupID + @"' AND LTD.IsAvailed = 1 AND LT.EmpSystemID = '" + strEmpSysID + @"'
                                   AND IsCancel=0 AND LTD.WorkDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'
                            GROUP BY LT.SystemID";
                }
                else
                {
                    strSQL = @"SELECT LT.SystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate FROM LeaveTransactionDetails LTD
		                            INNER JOIN dbo.LeaveTransaction LT ON LTD.LvTrnsSystemID = LT.SystemID
                            WHERE LT.PlantID = '" + sPlantID + @"' AND LT.GroupID = '" + sGroupID + @"' AND LTD.IsAvailed = 1
                                  AND IsCancel=0 AND LTD.WorkDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'
                            GROUP BY LT.SystemID";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public void LeavePolicyDetailInforForSelectedLeaveType(string sGroupID, string sPlantID, string strGLTSysID, string strLvPolSysID, string strFrmDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM LeavePolicyDetail
                                WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"' AND LPMSystemID = '" + strLvPolSysID + @"' AND LTSystemID = '" + strGLTSysID + @"'
                                         --AND '" + strFrmDate + @"' BETWEEN ISNULL(StartDate, GETDATE()) AND ISNULL(EndDate, GETDATE())
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

        public void XGetYearlyOffDayDetails(string sGroupID, string sPlantID, string strYearSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT OFD.OffDayMstSystemID, OFD.OffDayDate, OFD.DayName, ODM.OffDayType FROM OffDayDetail OFD
                                INNER JOIN OffDayMaster ODM ON OFD.OffDayMstSystemID = ODM.SystemID
                            WHERE OFD.PlantID = '" + sPlantID + @"' AND OFD.GroupID = '" + sGroupID + @"' AND ODM.YrSystemID = '" + strYearSystemID + @"'";

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

        public void GetYearlyOffDayDetails(string sGroupID, string sPlantID, string strYearSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT OFD.OffDayMasterId, OFD.OffDayDate, OFD.DayName, ODM.OffDayType FROM SCS.OffDayDetail OFD
                                INNER JOIN SCS.OffDayMaster ODM ON OFD.OffDayMasterId = ODM.Id
                            WHERE OFD.PlantID = '" + sPlantID + @"' AND OFD.CompanyGroupId = '" + sGroupID + @"' AND ODM.YearlyCalendarId = '" + strYearSystemID + @"'";

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

        public void GetLvTransInfo(string sGroupID, string sPlantID, string strLvTrnSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from dbo.LeaveTransaction
                            WHERE PlantID = '" + sPlantID + @"' AND GroupId = '" + sGroupID + @"' AND SystemID = '" + strLvTrnSystemID + @"'";

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

        public void GetRestData(string sEmployeeId, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select D.EmpSystemId,R.AttendanceRestDate from AttendanceRest R 
                        LEFT JOIN AttendanceRestDetail D ON D.AttendanceRestId=R.Id 
                        Where D.EmpSystemId='"+ sEmployeeId + "' AND R.AttendanceRestDate='"+sDate+"'";

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

        public void GetEmpWeekendData(string sEmployeeId, string fromDate, string toDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT WorkDate FROM EmpDateWiseShiftAssign WHERE EmpSystemID='" + sEmployeeId + @"' and DayType='W' and WorkDate between '"+ fromDate + @"' and '"+ toDate + "' ";

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
        public void GetHRSettinng(string plantid, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from PlantWiseHRMSSetting WHERE plantid='" + plantid + @"' ";

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
        public void getAllLeavesDetail(string sEmployeeId, string fromDate, string toDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM LeaveTransactionDetails where 
                                    WorkDate between '" + fromDate + @"' and '" + toDate + @"' and 
                                    LvTrnsSystemID in (select SystemID from LeaveTransaction where EmpSystemID='" + sEmployeeId + @"') ";

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

        public void GetEmpHoliDayData(string sGroupID, string sPlantID, string fromDate, string toDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT OFM.CldDescription, OFM.FromDate, OFM.ToDate, OFM.OffDayType, OFM.TotalDay, OFD.DayName, OFM.PlantID  
	                            FROM scs.OffDayMaster OFM
			                            INNER JOIN scs.OffDayDetail OFD ON OFM.Id = OFD.OffDayMasterId 
                                                                    AND OFD.OffDayDate between '"+ fromDate + @"' AND '"+ toDate + @"'
                                WHERE OFM.CompanyGroupId = '"+ sGroupID + @"' AND OFM.PlantID = '"+ sPlantID + @"'
									  AND OFM.OffDayType = 'H'";

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

        public void ISEmpOnDuty(string EmpSystemId, string WDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from EmployeeOnDuty O
                      left join EmployeeOnDutyDetails OD  ON OD.OnDutyId=O.Id
               Where O.EmpSystemId='"+EmpSystemId+@"' AND  OD.Workdate ='"+WDate+@"'";

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
        public void ISEmpRest(string EmpSystemId, string WDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select *  from  AttendanceRest R
				  left join AttendanceRestDetail RD on RD.AttendanceRestId=R.Id
               Where EmpSystemId='" + EmpSystemId + @"' AND  R.AttendanceRestDate ='" + WDate + @"'";

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

        public void GetWeekOffData(string sEmployeeId, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DayType,EmpSystemID FROM [dbo].[EmpDateWiseShiftAssign] Where EmpSystemID='" + sEmployeeId + @"' AND WorkDate='"+sDate+@"' AND DayType='W'";

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

        public void GetLvTransInfo(string strSysTemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT SystemID, ComAssignLvSystemID, LTSystemID, REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate,
                //                  REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate, LeaveDays, LvReason AS Reason, ApprovedDate,
                //                  REPLACE(CONVERT(VARCHAR(11), AppliedDate, 113), ' ', '-')  AppliedDate
                //            FROM LeaveTransaction
                //            WHERE SystemID = '" + strSysTemID + @"'";

                strSQL = @"SELECT SystemID, ComAssignLvSystemID, LTSystemID, REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate, lt.UserName,ltrn.LTSystemID,
                                  REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate, LeaveDays, LvReason AS Reason, ApprovedDate,
                                  REPLACE(CONVERT(VARCHAR(11), AppliedDate, 113), ' ', '-')  AppliedDate
                            FROM dbo.LeaveTransaction ltrn
							left outer join dbo.LeaveType lt on lt.Id = ltrn.LTSystemID
                            WHERE  SystemID = '" + strSysTemID + @"'";

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
        public void GetODApprInfo( out DataSet dsRef)
        {
            //var FromDate = "01-jan-" + stYearId;
            //var ToDate = "31-dec-" + stYearId;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT E.EmployeeName, D.* FROM EmployeeOnDuty D
			                LEFT JOIN EmployeeInformation E on E.SystemId =D.EmpSystemId WHERE D.IsApproved=0";
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
        public void GetEntityByEmployee(string tableName, string fieldName, string employeeId, bool isControlAdmin, bool isSysAdmin, string companyId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where ProbationRP='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSQL = @"SELECT E.Code , E.UserName AS EntityName, C.UserName AS CompanyName FROM ORG.Entity AS E
                            LEFT OUTER JOIN ORG.Company AS C ON E.CompanyId=C.Id WHERE E.Id IN(
                            SELECT EntityId FROM " + tableName + " WHERE " + fieldName + "='" + employeeId + "')";

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

        public void GetLvTransInfo(string sGroupID, string sPlantID, string strEmpSysID, string strlblComAssSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                if (strEmpSysID != "" & strlblComAssSysID == "")
                {
                    strSQL = @"SELECT * FROM LeaveTransaction 
                               
                                    WHERE --PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                           -- AND 
                                        EmpSystemID = '" + strEmpSysID + @"'";
                }
                else if (strEmpSysID == "" & strlblComAssSysID != "")
                {
                    strSQL = @"SELECT * FROM LeaveTransaction 
                                    WHERE --PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                          --  AND 
                                        ComAssignLvSystemID = '" + strlblComAssSysID + "'";
                }
                else
                {
                    strSQL = @"SELECT * FROM LeaveTransaction 
                                    WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'";
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
        public void GetLeaveTypeCategory(string LeaveTypeID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                    strSQL = @"SELECT * FROM LeaveType WHERE Id = '" + LeaveTypeID + @"' ";
               
                

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

        public void GetLvTransDetInfo(string strSysID, string strlblComAssSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strSysID != "" & strlblComAssSysID == "")
                {
                    strSQL = @"SELECT * FROM LeaveTransactionDetails
                            WHERE LvTrnsSystemID = '" + strSysID + @"'";
                }
                else if (strSysID == "" & strlblComAssSysID != "")
                {
                    strSQL = @"SELECT * FROM LeaveTransactionDetails
                                    WHERE LvTrnsSystemID IN
                                        (SELECT SystemID FROM LeaveTransaction
                                                WHERE ComAssignLvSystemID = '" + strlblComAssSysID + "')";
                }
                else
                {
                    strSQL = @"SELECT * FROM LeaveTransactionDetails";
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

        public void GetLvTransDetInfoByDay(string sLvTranSysId, string sFromDate, string sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT REPLACE(CONVERT(VARCHAR(11), WorkDate, 113),' ','-') WorkDate,DayType,LeaveStatus FROM dbo.LeaveTransactionDetails
                //                    WHERE LvTrnsSystemID = '" + sLvTranSysId + @"'
                //                            AND WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'";

                strSQL = @"SELECT REPLACE(CONVERT(VARCHAR(11),ltd.WorkDate, 113),' ','-') WorkDate,
                                           datename(dw,ltd.WorkDate) Day,
                                           ltd.DayType,
                                           ltd.LeaveStatus
                                           --atd.DayStatus DayType
                                           FROM dbo.LeaveTransactionDetails ltd
                                           left outer join dbo.LeaveTransaction lt on lt.SystemID = ltd.LvTrnsSystemID
                                           left outer join [dbo].[AttdnProcessData] atd on atd.EmpSystemID = lt.EmpSystemID
                                           and ltd.WorkDate = atd.WorkDate
                                            WHERE LvTrnsSystemID = '" + sLvTranSysId + @"'
                                            --AND ltd.WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
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

        public void GetAttdnData(string sGroupID, string sPlantID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM AttdnProcessData
                                    WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                            AND WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'";

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

        public void GetAttdnData(string sGroupID, string sPlantID, DateTime sFromDate, DateTime sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM AttdnProcessData
                                    WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                                            AND WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'";

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

        public void SelectedLvLeaveDaysCountForEmp(string sGroupID, string sPlantID, string sEmpSysID, string sFromDate, string sToDate, string strMstSystemID, string strLTSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSysID != "")
                {
                    strSQL = @"SELECT ISNULL(SUM(LT.LeaveDays), 0) LeaveDays FROM LeaveTransaction LT
                                WHERE LT.PlantID = '" + sPlantID + @"' AND LT.GroupID = '" + sGroupID + @"' AND LT.EmpSystemID = '" + sEmpSysID + @"'
                                    AND (LT.FromDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
                                    OR LT.ToDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"')
                                    AND LT.SystemID <> '" + strMstSystemID + @"' AND LTSystemID = '" + strLTSysID + @"'";
                }
                else
                {
                    strSQL = @"SELECT ISNULL(SUM(LT.LeaveDays), 0) LeaveDays FROM LeaveTransaction LT
                                WHERE LT.PlantID = '" + sPlantID + @"' AND LT.GroupID = '" + sGroupID + @"' AND (LT.FromDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
                                    OR LT.ToDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"')
                                    AND LT.SystemID <> '" + strMstSystemID + @"' AND LTSystemID = '" + strLTSysID + @"'";
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
        public void LeaveDayCount( string strLTSysID, out DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {                
                    strSQL = @"SELECT ISNULL(SUM(LT.LeaveDays), 0) LeaveDays FROM LeaveTransaction LT
                                WHERE SystemID = '" + strLTSysID + @"'";               
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

        public void GetLvPolMstTagEmp(string strEmpSysID, string strLvPolSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strEmpSysID != "")
                {
                    strSQL = @"SELECT * FROM LvPolMstTagEmp
                            WHERE EmpSystemID = '" + strEmpSysID + @"' AND LvPolMstSystemID = '" + strLvPolSysID + @"'";
                }
                else
                {
                    strSQL = @"SELECT * FROM LvPolMstTagEmp
                            WHERE LvPolMstSystemID = '" + strLvPolSysID + @"'";
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

        public void GetYearlyAvailedCAL(string sEmpSysID, string sFromDate, string sToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Count(*) AvailedCAL
                                FROM [dbo].[LeaveTransactionDetails]
                            WHERE WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"' AND DayType = 'CAL'
	                              AND LeaveStatus = 'LV' AND IsAvailed = 1
                                  AND LvTrnsSystemID IN (
                                                         SELECT SystemID FROM [dbo].[LeaveTransaction]
                                                            WHERE EmpSystemID = '" + sEmpSysID + @"'
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

        public void SaveDataSets(bool bAdddEdit, string strMstSystemID, params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

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
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void SaveDeleteDataSets(bool bAdddEdit, string strMstSystemID, params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.LeaveTransactionDetails WHERE LvTrnsSystemID = '" + strMstSystemID + "'", true, "1");
                if (bAdddEdit == false)
                {
                    objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.LeaveTransaction WHERE (SystemID = '" + strMstSystemID + "')", true, "1");
                }

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
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void LeaveDetailsDataSetsDelete(string strMstSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.LeaveTransactionDetails WHERE LvTrnsSystemID = '" + strMstSystemID + "'", true, "1");

               
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void SaveDataSets(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.LeaveTransactionDetails WHERE LvTrnsSystemID = '" + strMstSystemID + "'", true, "1");
                //if (bAdddEdit == false)
                //{
                //    objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.LeaveTransaction WHERE (SystemID = '" + strMstSystemID + "')", true, "1");
                //}

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
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void SaveDataSets(string SystemID, string FromDate, string ToDate, params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper(" delete from [dbo].[FinalOT] where  EmpSystemID='" + SystemID + "' and WorkDate between '" + FromDate + "' and '" + ToDate + "'", true, "1");


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
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void LoadLvPolicyWiseLeaveTypeGrdForCompanyAssigned(string sGroupID, string sPlantID, string strLvPolSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LT.SystemID, LT.LeaveName, LT.LeaveDescription, LPD.IsExcessAllow FROM LeavePolicyDetail LPD
                                LEFT JOIN LeaveType LT ON LPD.LTSystemID = LT.SystemID
                            WHERE LPD.GroupID = '" + sGroupID + @"' AND LPD.PlantID = '" + sPlantID + @"'
								  AND LPD.LPMSystemID = '" + strLvPolSysID + @"' AND LPD.IsActive = 1
                            --UNION
                            --(SELECT SystemID, LeaveName, LeaveDescription, '' IsExcessAllow FROM LeaveType
                            --        WHERE LeaveType IN ('Leave Without Pay','Encash') AND IsActive = 1)
                            --ORDER BY LT.LeaveName
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

        public void XLoadLeaveTypeWiseGrdForCompanyAssigned(string sGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LPD.SystemID LvPolDetailsSystemID, LPD.LPMSystemID, LPD.LTSystemID, LT.LeaveName, LT.LeaveDescription,
	                              LPD.IsExcessAllow, LPD.PlantID
                            FROM LeavePolicyDetail LPD
                                LEFT JOIN LeaveType LT ON LPD.LTSystemID = LT.SystemID
                            WHERE LPD.GroupID = '" + sGroupID + @"' AND LPD.IsActive = 1
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

        public void LoadLeaveTypeWiseGrdForCompanyAssigned(string sGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LPD.SystemID LvPolDetailsSystemID, LPD.LPMSystemID, LPD.LTSystemID, LT.UserName LeaveName, LT.Description LeaveDescription,
	                              LPD.IsExcessAllow, LPD.PlantID
                            FROM LeavePolicyDetail LPD
                                LEFT JOIN LeaveType LT ON LPD.LTSystemID = LT.ID
                            WHERE LPD.GroupID = '" + sGroupID + @"' AND LPD.IsActive = 1
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

        public void GetEmployeeIDNameBaseOnDOJandDOS(string sGroupID, string sPlantID, string strYrCalSysID, string strLPMSysterID, string strFrmDate, string strToDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT [CheckBoxSelect] = Case WHEN LA.EmpSystemID IS NULL THEN Convert(bit, 'False')
                                            ELSE Convert(bit, 'True') END, E.SystemID, E.EmployeeCode,
                                E.EmployeeName
                            FROM EmployeeInformation E
                                    LEFT JOIN (SELECT DISTINCT EmpSystemID FROM dbo.LeaveAllocation WHERE YrCalSystemID = '" + strYrCalSysID + @"'
                                                            AND LvPolDetailsSystemID IN (SELECT SystemID FROM LeavePolicyDetail
                                                        WHERE LPMSystemID = '" + strLPMSysterID + @"' AND IsActive = 1 GROUP BY SystemID)
                                                GROUP BY EmpSystemID) LA ON E.SystemID = LA.EmpSystemID
                                WHERE (E.DOS > '" + strToDate + @"' OR E.DOS IS NULL) AND (E.DOJ < '" + strFrmDate + @"' OR E.DOJ IS NULL)
                                        AND E.SystemID NOT IN (SELECT EmpSystemID
                                                                    FROM dbo.LeaveAllocation
                                                                WHERE YrCalSystemID = '" + strYrCalSysID + @"'
                                                                    AND LvPolDetailsSystemID IN
                                                                       (SELECT SystemID
                                                                        FROM LeavePolicyDetail
                                                                        WHERE LPMSystemID <> '" + strLPMSysterID + @"' AND IsActive = 1
                                                                        GROUP BY SystemID)
                                                                GROUP BY EmpSystemID)
                                        AND E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"'
                                ORDER BY E.EmployeeCode";

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

        public void GetLeaveAllocatLPMSysterIDWise(string sGroupID, string sPlantID, string strYrCalSysID, string strLPMSysterID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveAllocation
                            WHERE YrCalSystemID = '" + strYrCalSysID + @"'
                                   AND LvPolDetailsSystemID IN (SELECT SystemID FROM LeavePolicyDetail
						                                    WHERE LPMSystemID = '" + strLPMSysterID + @"' AND IsActive = 1
                                                            GROUP BY SystemID)
                                   AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

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

        public void XGetLeaveTypeSelectedLvType(string sGroupID, string sLvType, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveType
                            WHERE LeaveType = '" + sLvType + @"' AND IsActive = 1 AND GroupID = '" + sGroupID + @"'
                            ORDER BY LeaveName";

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

        public void GetLeaveTypeSelectedLvType(string sGroupID, string sLvType, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveType
                            WHERE LeaveType = '" + sLvType + @"' AND CompanyGroupID = '" + sGroupID + @"'
                            ORDER BY UserName";

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

        public void XGetLeaveType(string strSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LPD.SystemID LvPolDetailsSystemID, LPD.LTSystemID, GLT.LeaveName, LPD.LeaveDays, LPD.IsExcessAllow FROM dbo.LeavePolicyMaster LPM
                                LEFT JOIN dbo.LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID AND LPD.IsActive = 1
                                LEFT JOIN dbo.LeaveType GLT ON LPD.LTSystemID = GLT.SystemID
                            WHERE LPM.SystemID = '" + strSystemID + @"'
                            ORDER BY GLT.LeaveName";

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

        public void GetLeaveType(string strSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LPD.SystemID LvPolDetailsSystemID, LPD.LTSystemID, GLT.UserName LeaveName, LPD.LeaveDays, LPD.IsExcessAllow FROM dbo.LeavePolicyMaster LPM
                                LEFT JOIN dbo.LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID AND LPD.IsActive = 1
                                LEFT JOIN dbo.LeaveType GLT ON LPD.LTSystemID = GLT.ID
                            WHERE LPM.SystemID = '" + strSystemID + @"'
                            ORDER BY GLT.UserName";

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

        public void GetLeaveTypeWise(string sGroupID, string sType, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = "SELECT * FROM LeaveType WHERE GroupID = '" + sGroupID + @"' AND
                                LeaveType = '" + sType + @"'
                           ORDER BY LeaveName";
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
        }//End function

        public void GetLeaveTransCompAssign(string sGroupID, string sPlantID, string strSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveTransactionCompanyAssign
                            WHERE SystemID = '" + strSystemID.Trim() + @"'
                                  AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

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

        public void GetLeaveTransCompAssignChd(string strSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveTransactionCompanyAssignChd
                                WHERE ComAssignLvSystemID = '" + strSystemID.Trim() + "'";

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

        public void GetLvPolMst(string sGroupID, string sPlantID, string strLvPolSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM LeavePolicyMaster
                            WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
                                    AND SystemID = '" + strLvPolSysID + @"'";

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

        public void GetLvPolChd(string sGroupID, string sPlantID, string strLvPolSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM LeavePolicyDetail
                            WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
                                   AND LPMSystemID = '" + strLvPolSysID + @"'";

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

        public void SearchLeaveTransCompAssign(string sGroupID, string sPlantID, string strKey, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT REPLACE(CONVERT(VARCHAR(11), LTCA.FromDate, 113),' ','-') FromDate,
                                REPLACE(CONVERT(VARCHAR(11), LTCA.ToDate, 113),' ','-') ToDate,
                                LTCA.LeaveDays, LTCA.LvReason, LPM.PolicyName, YC.YearNo, LTCA.SystemID,
                                LTCA.LvPolMstSystemID, LTCA.YrCalSystemID
                            FROM dbo.LeaveTransactionCompanyAssign LTCA
	                            LEFT JOIN dbo.LeavePolicyMaster LPM ON LTCA.LvPolMstSystemID = LPM.SystemID
	                            LEFT JOIN dbo.YearlyCalendar YC ON LTCA.YrCalSystemID = YC.ID
                            WHERE LTCA.GroupID = '" + sGroupID + @"' AND LTCA.PlantID = '" + sPlantID + @"'";

                if (strKey != "")
                {
                    strSQL = strSQL + @"
                            AND " + strKey + "";
                }

                strSQL = strSQL + @"
                            ORDER BY LTCA.FromDate DESC, LTCA.ToDate DESC";

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

        public void SelectLeaveTransCompAssignChd(string strSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LTCAC.SystemID, LTCAC.ComAssignLvSystemID, LTCAC.SeqNo, LTCAC.LTSystemID, LT.LeaveName, LT.LeaveDescription, LTCAC.IsExcessAllow
		                    FROM dbo.LeaveTransactionCompanyAssignChd LTCAC
		                            LEFT JOIN LeaveType LT ON LTCAC.LTSystemID = LT.SystemID
                            WHERE LTCAC.ComAssignLvSystemID = '" + strSystemID.Trim() + "'";

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

        public void SelectCompAssignLvTransInfoWithDateAndEmpWise(string sGroupID, string sPlantID, string strSystemID, string strLvPolSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LT.SystemID, LTD.SystemID LTDSystemID, LT.EmpSystemID, LTD.WorkDate, LPD.SystemID LvPolDetailsSystemID, LT.LTSystemID,
	                            LT.LeaveDays, LTD.DayType, LTD.LeaveStatus, LeaveCount = CASE WHEN LTD.LeaveStatus = 'LV' THEN 1 ELSE 0 END
                            FROM dbo.LeaveTransaction LT
		                            LEFT JOIN dbo.LeaveTransactionDetails LTD ON LT.SystemID = LTD.LvTrnsSystemID
		                            LEFT JOIN LeavePolicyDetail LPD ON LT.LTSystemID = LPD.LTSystemID AND IsActive = 1
													                            AND LPMSystemID = '" + strLvPolSysID + @"'
                            WHERE LT.GroupID = '" + sGroupID + @"' AND LT.PlantID = '" + sPlantID + @"'
                                  AND LT.ComAssignLvSystemID = '" + strSystemID.Trim() + "'";

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

        public void GetValue(string childNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT
                                            CASE
                                              WHEN
                                                MaternityLeavePolicy.DurationType = 'Day'
                                                  THEN
                                                    MaternityLeavePolicy.DurationValue
                                              WHEN
                                                MaternityLeavePolicy.DurationType = 'Week'
                                                  THEN
                                                   MaternityLeavePolicy.DurationValue * 7
                                              ELSE
                                                  MaternityLeavePolicy.DurationValue * 30
                                            END AS DurationValue
                                           FROM
                                           MST.MaternityLeavePolicy Where ChildNo=" + childNo + " ";

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

        public void GetCheck(string childNo, string empId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT BabyNo FROM LeaveTransaction Where  BabyNo =" + childNo + " AND EmpSystemId <>'" + empId + "' ";

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
        public void GetExceptionAllowed(string LPMSystemId, string LTSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsAvailExceptionAllowedOnSpecialAppeal FROM LeavePolicyDetail WHERE LPMSystemId='" + LPMSystemId + @"' AND LTSystemID='"+ LTSystemID + "' AND IsAvailExceptionAllowedOnSpecialAppeal=1";

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
        public void GetExceptionAllowedA(string LPMSystemId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsAvailExceptionAllowedOnSpecialAppeal FROM LeavePolicyDetail WHERE LPMSystemId='" + LPMSystemId + @"'  AND IsAvailExceptionAllowedOnSpecialAppeal=1";

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
        public void GetGender(string empId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT GenderID FROM EmployeeInformation WHERE SystemId='" + empId + "' ";

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

        public void GetYearlyCalendarForLeaveYearEndProcess(string sGroupID, string sPlantID, string sSystemID, out DataSet dsRef)
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

                if (sSystemID.Trim() != "")
                {
                    strSQL = strSQL + " WHERE Id = '" + sSystemID + @"'";
                }

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
        public void GetYearlyCalendarForValidation(string sGroupID, string sPlantID, string sSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (
                                        SELECT Id
                                        ,IsYearEndClosed
                                        , YearNo
                                        , REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate
                                        ,REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate
                                        ,(select Id from dbo.YearlyCalendar where '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate AND PlantId='" + sPlantID + @"') CalendarYear
                                        FROM YearlyCalendar WHERE PlantID = '" + sPlantID + @"' AND CompanyGroupId = '" + sGroupID + @"'
                                    ) AS A";

                if (sSystemID.Trim() != "")
                {
                    strSQL = strSQL + " WHERE Id = '" + sSystemID + @"'";
                }

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



    }
}