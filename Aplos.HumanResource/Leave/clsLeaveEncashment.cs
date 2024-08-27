using bplib;
using Library.Crosscutting.Security;
using OTSBD.clsLeave;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace OTSBD
{
    public class clsLeaveEncashment
    {


        public clsLeaveEncashment()
        {
            // TODO: Add constructor logic here
        }//End Function


        public void GetSalaryDataEmpWise(string sEmpSystemId, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"  SELECT * FROM ( SELECT (x.EffectiveDate) EffectiveDate,m.SystemID from (
select max(	EffectiveDate) 	EffectiveDate FROM (
                        SELECT  max(EffectiveDate)EffectiveDate FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
                        union
                        SELECT  Max(EffectiveDate)EffectiveDate  FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
 	 	) zz						
) x
						
						INNER JOIN (
							 SELECT  EffectiveDate,SystemID
							   FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 
                        union
                        SELECT  EffectiveDate,SystemID FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 
						) m ON m.EffectiveDate=x.EffectiveDate ) mas
						INNER JOIN (
						SELECT s.SystemID,	s.SalaryID,	s.SalaryHeadID,	s.EntryCurrencyID,	s.EntryAmount,	s.DefineCurrencyID,	s.DefineAmount,	s.AmtDefinitionCurrencyID,	s.AmtDefinitionRate,	s.AddedBy,	s.DateAdded,	s.UpdatedBy,	s.DateUpdated,	s.SequenceNo,	s.SalaryCategory ,sh.HeadCategory,sh.SalaryHead  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.SystemID,	sb.SalaryID,	sb.SalaryHeadID,	sb.EntryCurrencyID,	sb.EntryAmount,	sb.DefineCurrencyID,	sb.DefineAmount,	sb.AmtDefinitionCurrencyID,	sb.AmtDefinitionRate,	sb.AddedBy,	sb.DateAdded,	sb.UpdatedBy,	sb.DateUpdated,	sb.SequenceNo,	sb.SalaryCategory ,sh.HeadCategory,sh.SalaryHead FROM  SalaryInfoBack sb
						LEFT JOIN SalaryHead AS sh on sb.SalaryHeadID=sh.SalaryHeadID
                        ) d ON mas.SystemID=d.SalaryID";

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

        public void GetLeaveBalance(string EmpSystemId, string YearId, string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType,s.LeaveTypeId,e.LegalDesignationId
                            ,BroughtForward=isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,s.CarryForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed,s.EncashedInbetween ,s.YearEndEncash
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave

                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									ELSE  
                        ---isnull(s.BroughtForward,0) 
                        CASE WHEN s.IsEncashed =1 THEN ISNULL(s.CarryForward, 0)+ISNULL(s.EncashedInbetween, 0) ELSE ISNULL(s.BroughtForward, 0)+isnull(s.CarryForwardOpeningBalance,0) END
                        +isnull(s.DaysCanBeSanctioned,0)
                        -isnull(kk.LeaveDuration,0)-
                        isnull(s.EncashedInbetween,0) END---Yes
							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No,
	                        ,DOJorDOC=CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ )
                                                                                                    WHEN ltd.CanAvailUOM is null
                                                                                                    						THEN DateAdd(DAY, LvCanAvailAfter, e.DOJ)
                                                                                                    END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
                                                                                                        WHEN ltd.CanAvailUOM is null
                                                                                                        						THEN DateAdd(DAY, LvCanAvailAfter, e.DOC)
										   													END
																   END
                            from trn.EmployeeLeaveSummary s 
                            INNER join LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            left join EmployeeInformation e on e.SystemId=s.EmployeeId
                            left join (
                            select 
                            tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
                            from 
                            LeaveTransaction t 
                            left join 
                            (--detail
                            select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                            where IsAvailed=1
                            and WorkDate between
                            (select FromDate from YearlyCalendar where Id=" + YearId + @" and PlantId='" + PlantId + @"')
                            and (select ToDate from YearlyCalendar where Id= " + YearId + @" and PlantId='" + PlantId + @"')
                            group by LvTrnsSystemID
                            )--detail 
                            d on t.SystemID=d.LvTrnsSystemID

                            left join LeaveType tt on tt.id=t.LTSystemID
                            where t.IsApproved=1  
                            group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                        -----------------------------------------------------------
						   left outer join (select * from dbo.LeavePolicyDetail
											where LPMSystemID =
											(--w
											select LeavePolicyMasterId from 
													(
														SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																				FROM MST.DesignationMaster DM
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId
																where dc.plantid='" + PlantId + @"'

													) dm where dm.DesignationId =(select givendesignationId 
																				from dbo.EmployeeInformation 
																				where SystemId='" + EmpSystemId + @"')
											)--w
							) ltd on ltd.LTSystemID = t.Id
						   --------------------------------------------------------------------------
                            where s.CalanderYearId=(select id from YearlyCalendar where Id=" + YearId + @" and PlantId='" + PlantId + @"') AND E.SystemId ='" + EmpSystemId + @"'
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric
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
        public void GetLeaveBalanceFinalSettlement(string EmpSystemId, string DOS, out System.Data.DataSet dsRef)
        {
            string strSQL;
            string fromDate = null;
            string toDate = null;

            string _fromDate = null;
            string _toDate = null;
            DataSet dsFromTo;
            ConnectionManager.DAL.ConManager objCon;
            try
            {


                objCon = new ConnectionManager.DAL.ConManager("1");
                //                objCon.OpenDataSetThroughAdapter("select top(1) a.Fromdate,a.Todate from trn.EmployeeLeaveSummary a LEFT JOIN LeaveType t on t.Id=a.LeaveTypeID where EmployeeId='" + EmpSystemId + @"' AND t.LeaveType='Earn' order by fromdate desc", out dsFromTo, false, "1");
                //                if (dsFromTo.Tables[0].Rows.Count > 0)
                //                {
                //                    fromDate = dsFromTo.Tables[0].Rows[0]["Fromdate"].ToString();
                //                    toDate = dsFromTo.Tables[0].Rows[0]["Todate"].ToString();
                //                }
                //                string sql = @"select e.SystemID,e.EmployeeCode,e.EmployeeName,t.UserName LeaveType,t.Id LeaveTypeId,ISNULL(S.BroughtForward,0)BroughtForward,CONVERT(NUMERIC(10,2),count(CONVERT(NUMERIC(10,2),a.WorkDate))/CONVERT(NUMERIC(10,2),dp.EncashWorkingDaysQty)) DaysCanBeSanctioned
                //,ISNULL(B.Availed,0)AvailedLeave,Balance=ISNULL((CONVERT(NUMERIC(10,2),count(CONVERT(NUMERIC(10,2),a.WorkDate))/CONVERT(NUMERIC(10,2),dp.EncashWorkingDaysQty))+S.BroughtForward-ISNULL(B.Availed,0)),0)
                //,ISNULL(S.EncashedInbetween,0) EncashedInbetween,LeaveDaysAllowed=ISNULL((CONVERT(NUMERIC(10,2),count(CONVERT(NUMERIC(10,2),a.WorkDate))/CONVERT(NUMERIC(10,2),dp.EncashWorkingDaysQty))+S.BroughtForward-ISNULL(B.Availed,0)),0)
                //from AttdnProcessData a
                //left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                //left join mst.DesignationMasterLegalDesignation d on d.LegalDesignationId=e.LegalDesignationId
                //left join SCS.DesignationMasterConfiguration c on c.DesignationMasterId=d.DesignationMasterId and c.PlantId=e.PlantId
                //left join LeavePolicyDetail dp on dp.LPMSystemID=c.LeavePolicyMasterId
                //join LeavePolicyWorkingDays p on  p.LPDetailID=dp.SystemID and a.DayStatus=p.DayType  
                //left join LeaveType t on t.Id=dp.LTSystemID 
                //LEFT JOIN(
                //Select COUNT(a.EmpSystemID)Availed,a.EmpSystemID from AttdnProcessData a  
                //LEFT JOIN LeaveType t on t.Id=a.LTSystemID 
                //where a.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND EmpSystemID='" + EmpSystemId + @"' AND t.LeaveType='Earn'
                //Group By a.EmpSystemID
                //) B ON a.EmpSystemID=B.EmpSystemID
                //LEFT JOIN(
                //--select top(1) BroughtForward=isnull(a.BroughtForward,0)+isnull(a.CarryForwardOpeningBalance,0),a.EmployeeId,a.EncashedInbetween 
                //--from trn.EmployeeLeaveSummary a
                //--		LEFT JOIN LeaveType t on t.Id=a.LeaveTypeID 
                //--		where EmployeeId='" + EmpSystemId + @"' AND t.LeaveType='Earn' 
                //--		order by fromdate desc
                //select top(1) BroughtForward=CASE WHEN A.Closing>A.CarryForward THEN A.Closing ELSE A.CarryForward END,A.EmployeeId,0 EncashedInbetween from dbo.AnnualLeaveDataPast A
                //left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                //Where EmployeeId='" + EmpSystemId + @"' order by A.AddedDate desc
                //) S ON a.EmpSystemID=S.EmployeeId
                //where a.WorkDate between '" + fromDate + @"' and '" + toDate + @"' and e.SystemID='" + EmpSystemId + @"' and t.LeaveType='Earn'
                //group by E.SystemID,e.EmployeeCode,e.EmployeeName,t.UserName,t.id,dp.EncashWorkingDaysQty,B.Availed,S.BroughtForward,S.EncashedInbetween";

                string year = DateTime.Now.Year.ToString();

                string sql = @"Select E.SystemID,E.EmployeeCode,E.EmployeeName,t.UserName LeaveType,t.Id LeaveTypeId,S.BroughtForward,DaysCanBeSanctioned=CONVERT(NUMERIC(10,0),CONVERT(NUMERIC(10,2),P.PayDays)/CONVERT(NUMERIC(10,2),dp.EncashWorkingDaysQty)),ISNULL(B.Availed,0)AvailedLeave
,Balance=ROUND(ISNULL((CONVERT(NUMERIC(10,2),CONVERT(NUMERIC(10,2),P.PayDays)/CONVERT(NUMERIC(10,2),dp.EncashWorkingDaysQty))+S.BroughtForward-ISNULL(B.Availed,0)),0),0)
,ISNULL(S.EncashedInbetween,0) EncashedInbetween,LeaveDaysAllowed=ROUND(ISNULL((CONVERT(NUMERIC(10,2),CONVERT(NUMERIC(10,2),P.PayDays)/CONVERT(NUMERIC(10,2),dp.EncashWorkingDaysQty))+S.BroughtForward-ISNULL(B.Availed,0)),0),0)
FROM EmployeeInformation E
 LEFT JOIN (SELECT SUM(ISNULL(TotalPresent,0) + ISNULL(TotalLate,0))PayDays, EmpSystemID
 FROM [dbo].[SalaryProceAttdnData] WHERE EmpSystemID='" + EmpSystemId + @"' and YearNo='"+ year + @"' Group By EmpSystemID
) P ON P.EmpSystemID=E.SystemId
left join mst.DesignationMasterLegalDesignation d on d.LegalDesignationId=e.LegalDesignationId
left join SCS.DesignationMasterConfiguration c on c.DesignationMasterId=d.DesignationMasterId and c.PlantId=e.PlantId
left join LeavePolicyDetail dp on dp.LPMSystemID=c.LeavePolicyMasterId
left join LeaveType t on t.Id=dp.LTSystemID 
LEFT JOIN(
Select COUNT(a.EmpSystemID)Availed,a.EmpSystemID from AttdnProcessData a  
LEFT JOIN LeaveType t on t.Id=a.LTSystemID 
where a.WorkDate between '01-JAN-" + year + @"' AND  '31-DEC-" + year + @"' AND EmpSystemID='" + EmpSystemId + @"' AND t.LeaveType='Earn'
Group By a.EmpSystemID
) B ON E.SystemID=B.EmpSystemID
LEFT JOIN(
select top(1) BroughtForward=CASE WHEN A.Closing>A.CarryForward THEN A.Closing ELSE A.CarryForward END,A.EmployeeId,0 EncashedInbetween from dbo.AnnualLeaveDataPast A
left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
Where EmployeeId='" + EmpSystemId + @"' AND (A.Closing<>0 OR A.CarryForward<>0) order by A.AddedDate desc
) S ON E.SystemID=S.EmployeeId
where e.SystemID='" + EmpSystemId + @"' AND dp.EncashWorkingDaysQty<>0";


                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
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
        public void GetLeaveBalanceFinalSettlementNew(string EmpSystemId, string DOS, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType,s.LeaveTypeId,e.LegalDesignationId
                            ,BroughtForward=isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,s.CarryForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed,s.EncashedInbetween ,s.YearEndEncash
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,ISNULL( (SELECT sum(m.LvValue) FROM AttdnProcessData m 
                                        where m.WorkDate BETWEEN S.FromDate and S.ToDate
                                AND m.EmpSystemID=S.EmployeeId AND m.LTSystemID=S.LeaveTypeId ),0) AS AvailedLeave
            
            				,Balance=ISNULL(s.CurrentYearAllocation,0)+ISNULL(s.BroughtForward,0)+ISNULL(s.CarryForwardOpeningBalance,0)
                            from trn.EmployeeLeaveSummary s 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId AND s.PlantId=e.PlantId
                            
						   --------------------------------------------------------------------------
                            where  E.SystemId ='" + EmpSystemId + @"' AND e.DOS BETWEEN s.FromDate AND s.ToDate
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric
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

        public void GetEarnLeavePolicy(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT LPM.PolicyName,LPD.* FROM [dbo].[LeavePolicyMaster] AS LPM
                            LEFT JOIN [dbo].[LeavePolicyDetail] AS LPD ON LPD.LPMSystemID = LPM.SystemID
                            INNER JOIN LeaveType AS lt ON lt.Id = LPD.LTSystemID AND lt.LeaveType='Earn'
                            WHERE LPM.PlantID='" + PlantId + @"'";
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
        public void GetEarnLeavePolicy(string PlantId, string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT LPM.PolicyName,LPD.*	from EmployeeInformation e
                            left join mst.DesignationMasterLegalDesignation Ld on Ld.LegalDesignationId=e.LegalDesignationId
                            left join scs.DesignationMasterConfiguration c on c.DesignationMasterId=ld.DesignationMasterId and c.PlantId='" + PlantId + @"'
                            left join LeavePolicyDetail LPD on LPD.LPMSystemID=c.LeavePolicyMasterId
                            left join [dbo].[LeavePolicyMaster] AS LPM  ON LPD.LPMSystemID = LPM.SystemID
                            INNER JOIN LeaveType AS lt ON lt.Id = LPD.LTSystemID AND lt.LeaveType='Earn'
                             WHERE LPM.PlantID='" + PlantId + @"' and e.SystemId='" + EmpSystemId + @"'";
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

        public void GetAvailedEncashmentBalance(string EmpSystemId, string YearNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT SUM(Days) Days FROM LeaveEncashmentTransaction WHERE EmpSystemId='" + EmpSystemId + @"' AND YearlyCalendarId='" + YearNo + @"'";

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

        public LeaveEncashmentViewModel GetLeaveEncashmentData(string EmpSystemId, string LeaveEncashmentDate, string YearNo, string PlantId)
        {
            DataSet dsLvEncashment = null;
            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataSet dsAvailedEncashmentBalance = null;

            DataSet dsSalHd = null;
            DataTable dtSlrHd = null;
            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            LeaveEncashmentViewModel ob = new LeaveEncashmentViewModel();


            GetEarnLeavePolicy(PlantId, out dsEarnLeavePolicy);
            GetLeaveBalance(EmpSystemId, YearNo, PlantId, out dsLvEncashment);
            GetSalaryDataEmpWise(EmpSystemId, LeaveEncashmentDate, out dsSalaryDataEmpWise);
            //GetAvailedEncashmentBalance(EmpSystemId, YearNo, out dsAvailedEncashmentBalance);

            if (dsLvEncashment.Tables[0].Rows.Count > 0)
            {
                ob.EmpSystemId = EmpSystemId;
                ob.EncashmentDate = LeaveEncashmentDate;
                ob.EmployeeCode = dsLvEncashment.Tables[0].Rows[0]["EmployeeCode"].ToString();
                ob.EmployeeName = dsLvEncashment.Tables[0].Rows[0]["EmployeeName"].ToString();
                ob.LeaveType = dsLvEncashment.Tables[0].Rows[0]["LeaveType"].ToString();
                if (!string.IsNullOrEmpty(dsLvEncashment.Tables[0].Rows[0]["BroughtForward"].ToString()))
                {
                    ob.BroughtForward = Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["BroughtForward"].ToString());
                }
                if (!string.IsNullOrEmpty(dsLvEncashment.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString()))
                {
                    ob.DaysCanBeSanctioned = Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                }
                if (!string.IsNullOrEmpty(dsLvEncashment.Tables[0].Rows[0]["CurrentYearAllocation"].ToString()))
                {
                    ob.CurrentYearAllocation = Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["CurrentYearAllocation"].ToString());
                }
                if (!string.IsNullOrEmpty(dsLvEncashment.Tables[0].Rows[0]["LeaveDaysAllowed"].ToString()))
                {
                    ob.LeaveDaysAllowed = Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["LeaveDaysAllowed"].ToString());
                }
                if (!string.IsNullOrEmpty(dsLvEncashment.Tables[0].Rows[0]["AvailedLeave"].ToString()))
                {
                    ob.AvailedLeave = Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["AvailedLeave"].ToString());
                }
                if (!string.IsNullOrEmpty(dsLvEncashment.Tables[0].Rows[0]["YearEndEncash"].ToString()))
                {
                    ob.Balance = Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["YearEndEncash"].ToString());
                }
                //if (!string.IsNullOrEmpty(dsLvEncashment.Tables[0].Rows[0]["Balance"].ToString()))
                //{
                //    ob.Days = Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["Balance"].ToString());
                //}

                //if (dsAvailedEncashmentBalance.Tables[0].Rows.Count > 0)
                //{

                //    if (!string.IsNullOrEmpty(dsAvailedEncashmentBalance.Tables[0].Rows[0]["Days"].ToString()))
                //    {
                //        ob.AvailedEncashment = Convert.ToDecimal(dsAvailedEncashmentBalance.Tables[0].Rows[0]["Days"].ToString());

                //    }
                //}


                ob.Days = ob.Balance;



                if (dsEarnLeavePolicy.Tables[0].Rows.Count > 0)
                {
                    DataView dv = new DataView(dsSalaryDataEmpWise.Tables[0]);
                    if (!string.IsNullOrEmpty(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString()))
                    {


                        //all head and Salary info
                        GetSalaryHead(out dsSalHd);
                        dtSlrHd = dsSalHd.Tables[0];


                        //GetSalaryDataEmpWise(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                        if (dsSalaryDataEmpWise.Tables[0].Rows.Count == 0)
                        {
                            throw new Exception("This Employee has no Approved Salary Structure.");
                        }



                        DataTable dtValue = new DataTable();
                        dtValue.TableName = "TempTable";
                        dtValue.Columns.Add("SalaryHeadID");
                        dtValue.Columns.Add("EntryCurrencyID");
                        dtValue.Columns.Add("Amount");


                        for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                        {
                            DataRow dtValueRow = dtValue.NewRow();
                            dtValueRow["SalaryHeadID"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                            dtValueRow["EntryCurrencyID"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                            dtValueRow["Amount"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                            dtValue.Rows.Add(dtValueRow);
                        }
                        obSSrecal.ReLoadFormulaWithValue(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString(), ref dtValue, dsSalaryDataEmpWise.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                        ob.Rate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));














                        //dsSalaryDataEmpWise.Tables[0].DefaultView.RowFilter= "SalaryHeadId=" + dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentSalaryHeadID"].ToString();
                        //dv.RowFilter = "SalaryHeadId='" + dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentSalaryHeadID"].ToString() + "'";
                        //if (dv.Count > 0)
                        //{
                        //    if (!string.IsNullOrEmpty(dv[0]["EntryAmount"].ToString()))
                        //    {
                        //        ob.Rate = Convert.ToDecimal(dv[0]["EntryAmount"].ToString()) / 30;
                        //    }

                        //}
                    }
                    else
                    {
                        throw new Exception("No Salary Head defined on Earn Leave Policy.");
                    }


                }
                else
                {
                    throw new Exception("Earn Leave Policy not found.");
                }


            }


            return ob;
        }



        public LeaveEncashmentViewModel GetLeaveEncashmentDataForFinalSettlement(string EmpSystemId, string LeaveEncashmentDate, string YearNo, string PlantId)
        {
            DataSet dsLvEncashment = null;
            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataSet dsAvailedEncashmentBalance = null;

            DataSet dsSalHd = null;
            DataTable dtSlrHd = null;
            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            LeaveEncashmentViewModel ob = new LeaveEncashmentViewModel();


            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            string EarnleaveID = string.Empty;
            EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();

            GetEarnLeavePolicy(PlantId, EmpSystemId, out dsEarnLeavePolicy);
            GetLeaveBalanceFinalSettlement(EmpSystemId, LeaveEncashmentDate, out dsLvEncashment);
            GetSalaryDataEmpWise(EmpSystemId, LeaveEncashmentDate, out dsSalaryDataEmpWise);
            //GetAvailedEncashmentBalance(EmpSystemId, YearNo, out dsAvailedEncashmentBalance);

            if (dsLvEncashment.Tables[0].Rows.Count > 0)
            {
                ob.EmpSystemId = EmpSystemId;
                ob.EncashmentDate = LeaveEncashmentDate;
                ob.EmployeeCode = dsLvEncashment.Tables[0].Rows[0]["EmployeeCode"].ToString();
                ob.EmployeeName = dsLvEncashment.Tables[0].Rows[0]["EmployeeName"].ToString();
                ob.LeaveType = dsLvEncashment.Tables[0].Rows[0]["LeaveType"].ToString();
                ob.LeaveTypeId = dsLvEncashment.Tables[0].Rows[0]["LeaveTypeId"].ToString();


                ob.BroughtForward = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["BroughtForward"].ToString());
                ob.DaysCanBeSanctioned = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                //ob.CurrentYearAllocation = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["CurrentYearAllocation"].ToString());
                ob.LeaveDaysAllowed = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["LeaveDaysAllowed"].ToString());
                ob.AvailedLeave = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["AvailedLeave"].ToString());
                ob.Days = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["Balance"].ToString());


                CarryForword objCarryForword = CheckLeavePolicyDetails(PlantId, EmpSystemId, EarnleaveID, Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["Balance"].ToString()));




                if (dsEarnLeavePolicy.Tables[0].Rows.Count > 0)
                {
                    DataView dv = new DataView(dsSalaryDataEmpWise.Tables[0]);
                    if (!string.IsNullOrEmpty(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString()))
                    {


                        //all head and Salary info
                        GetSalaryHead(out dsSalHd);
                        dtSlrHd = dsSalHd.Tables[0];


                        //GetSalaryDataEmpWise(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                        if (dsSalaryDataEmpWise.Tables[0].Rows.Count == 0)
                        {
                            throw new Exception("This Employee has no Approved Salary Structure.");
                        }



                        DataTable dtValue = new DataTable();
                        dtValue.TableName = "TempTable";
                        dtValue.Columns.Add("SalaryHeadID");
                        dtValue.Columns.Add("EntryCurrencyID");
                        dtValue.Columns.Add("Amount");


                        for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                        {
                            DataRow dtValueRow = dtValue.NewRow();
                            dtValueRow["SalaryHeadID"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                            dtValueRow["EntryCurrencyID"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                            dtValueRow["Amount"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                            dtValue.Rows.Add(dtValueRow);
                        }
                        obSSrecal.ReLoadFormulaWithValue(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString(), ref dtValue, dsSalaryDataEmpWise.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                        ob.Rate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));


                    }
                    else
                    {
                        throw new Exception("No Salary Head defined on Earn Leave Policy.");
                    }


                }
                else
                {
                    throw new Exception("Earn Leave Policy not found.");
                }


            }


            return ob;
        }
        public LeaveEncashmentViewModel GetLeaveEncashmentDataForFinalSettlementNew(string EmpSystemId, string LeaveEncashmentDate, string YearNo, string PlantId)
        {
            DataSet dsLvEncashment = null;
            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataSet dsAvailedEncashmentBalance = null;

            DataSet dsSalHd = null;
            DataTable dtSlrHd = null;
            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            LeaveEncashmentViewModel ob = new LeaveEncashmentViewModel();


            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            string EarnleaveID = string.Empty;
            EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();

            GetEarnLeavePolicy(PlantId, EmpSystemId, out dsEarnLeavePolicy);
            GetLeaveBalanceFinalSettlementNew(EmpSystemId, LeaveEncashmentDate, out dsLvEncashment);
            GetSalaryDataEmpWise(EmpSystemId, LeaveEncashmentDate, out dsSalaryDataEmpWise);
            //GetAvailedEncashmentBalance(EmpSystemId, YearNo, out dsAvailedEncashmentBalance);

            if (dsLvEncashment.Tables[0].Rows.Count > 0)
            {
                ob.EmpSystemId = EmpSystemId;
                ob.EncashmentDate = LeaveEncashmentDate;
                ob.EmployeeCode = dsLvEncashment.Tables[0].Rows[0]["EmployeeCode"].ToString();
                ob.EmployeeName = dsLvEncashment.Tables[0].Rows[0]["EmployeeName"].ToString();
                ob.LeaveType = dsLvEncashment.Tables[0].Rows[0]["LeaveType"].ToString();
                ob.LeaveTypeId = dsLvEncashment.Tables[0].Rows[0]["LeaveTypeId"].ToString();


                ob.BroughtForward = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["BroughtForward"].ToString());
                ob.DaysCanBeSanctioned = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                ob.CurrentYearAllocation = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["CurrentYearAllocation"].ToString());
                ob.LeaveDaysAllowed = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["LeaveDaysAllowed"].ToString());
                ob.AvailedLeave = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["AvailedLeave"].ToString());
                ob.Days = (decimal)clsStaticInfo.dbl(dsLvEncashment.Tables[0].Rows[0]["Balance"].ToString());


                CarryForword objCarryForword = CheckLeavePolicyDetails(PlantId, EmpSystemId, EarnleaveID, Convert.ToDecimal(dsLvEncashment.Tables[0].Rows[0]["Balance"].ToString()));




                if (dsEarnLeavePolicy.Tables[0].Rows.Count > 0)
                {
                    DataView dv = new DataView(dsSalaryDataEmpWise.Tables[0]);
                    if (!string.IsNullOrEmpty(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString()))
                    {


                        //all head and Salary info
                        GetSalaryHead(out dsSalHd);
                        dtSlrHd = dsSalHd.Tables[0];


                        //GetSalaryDataEmpWise(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                        if (dsSalaryDataEmpWise.Tables[0].Rows.Count == 0)
                        {
                            throw new Exception("This Employee has no Approved Salary Structure.");
                        }



                        DataTable dtValue = new DataTable();
                        dtValue.TableName = "TempTable";
                        dtValue.Columns.Add("SalaryHeadID");
                        dtValue.Columns.Add("EntryCurrencyID");
                        dtValue.Columns.Add("Amount");


                        for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                        {
                            DataRow dtValueRow = dtValue.NewRow();
                            dtValueRow["SalaryHeadID"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                            dtValueRow["EntryCurrencyID"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                            dtValueRow["Amount"] = dsSalaryDataEmpWise.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                            dtValue.Rows.Add(dtValueRow);
                        }
                        obSSrecal.ReLoadFormulaWithValue(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString(), ref dtValue, dsSalaryDataEmpWise.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                        ob.Rate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));


                    }
                    else
                    {
                        throw new Exception("No Salary Head defined on Earn Leave Policy.");
                    }


                }
                else
                {
                    throw new Exception("Earn Leave Policy not found.");
                }


            }


            return ob;
        }

        public void GetMultipleEmployeeLeaveBalance(string PlantId, string YearId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                             , EC1.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,BroughtForward=isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            ,Balance=isnull(s.CarryForwardOpeningBalance,0) +isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0),av.LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.LeaveTypeId,ltd.PolicyName
                            , BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo, BI.SalaryPercentage
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode,dm.EmployeeCategoryId
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where Id=" + YearId + @" and PlantId='" + PlantId + @"')
                                    and (select ToDate from YearlyCalendar where Id= " + YearId + @" and PlantId='" + PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            
                          
                             LEFT JOIN 
                            (
                            	SELECT EmpSystemId,YearlyCalendarId ,SUM(Days) LeaveEncashmentDayNo FROM LeaveEncashmentTransaction WHERE  YearlyCalendarId='" + YearId + @"' GROUP BY EmpSystemId,YearlyCalendarId
                            ) as 
                            av ON av.EmpSystemId=s.EmployeeId
                            left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
																				LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId		
																where dc.plantid='" + PlantId + @"' 
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId



                            LEFT JOIN HKP.EmployeeCategory AS EC1 ON E.EmployeeCategorySystemID = EC1.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
	                        LEFT JOIN HKP.EmployeeCategory AS EC ON dm.EmployeeCategoryId = EC.Id
							LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
                            where s.CalanderYearId=(select id from YearlyCalendar where Id=" + YearId + @" and PlantId='" + PlantId + @"')  AND s.IsYearlyProcessed=1 
                            AND s.EmployeeId IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + PlantId + @"' )
                            AND  ltd.EncashmentBasis='CalanderYear'

                                            AND (CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END)<= (select ToDate from YearlyCalendar where Id= " + YearId + @" and PlantId='" + PlantId + @"')
                            AND s.EmployeeId NOT IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId='" + YearId + @"' AND LeaveEncashmentType='Year End Leave Encashment' and PlantId='" + PlantId + @"' )
                           ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";

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
        public void GetMultipleEmployeeSalaryData(string PlantId, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM ( SELECT (x.EffectiveDate) EffectiveDate,m.SystemID,m.EmpInfoSystemID from (
												select max(	EffectiveDate) 	EffectiveDate,EmpInfoSystemID FROM (
																	SELECT   EffectiveDate   ,EmpInfoSystemID
																	FROM SalaryInfoDefineMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"'  AND PlantID='" + PlantId + @"'
																	union
																	SELECT  EffectiveDate  ,EmpInfoSystemID
																	FROM SalaryInfoBackMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"' AND PlantID='" + PlantId + @"'
 	 												) zz GROUP BY EmpInfoSystemID		
											) x
						
						INNER JOIN (
							 SELECT  EffectiveDate,SystemID,EmpInfoSystemID
							   FROM SalaryInfoDefineMaster  
							  WHERE    IsApproved =1 
                        union
                        SELECT  EffectiveDate,SystemID ,EmpInfoSystemID
								FROM SalaryInfoBackMaster  
                                WHERE IsApproved =1 
						) m ON m.EffectiveDate=x.EffectiveDate AND m.EmpInfoSystemID= x.EmpInfoSystemID ) mas
						INNER JOIN (
						SELECT s.SystemID,s.SalaryID,s.SalaryHeadID,s.EntryCurrencyID,s.EntryAmount,s.DefineCurrencyID,s.DefineAmount,s.AmtDefinitionCurrencyID,s.AmtDefinitionRate,s.SequenceNo,s.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.SystemID,sb.SalaryID,sb.SalaryHeadID,sb.EntryCurrencyID,sb.EntryAmount,sb.DefineCurrencyID,sb.DefineAmount,sb.AmtDefinitionCurrencyID,sb.AmtDefinitionRate,sb.SequenceNo,sb.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead FROM  SalaryInfoBack sb
						LEFT JOIN SalaryHead AS sh on sb.SalaryHeadID=sh.SalaryHeadID
                        ) d ON mas.SystemID=d.SalaryID   ORDER BY mas.EmpInfoSystemID ";

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

        public void GetMultipleEmployeeSalaryDataOnYearEnd(string PlantId, out System.Data.DataSet dsRef)
        {
            dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM (SELECT dense_rank() OVER (PARTITION BY K.EmpInfoSystemID ORDER BY K.EffectiveDate DESC) AS RNK,K.*
                                   FROM (SELECT M.EmpInfoSystemID,M.EffectiveDate,els.ToDate, s.SystemID,s.SalaryID,s.SalaryHeadID,s.EntryCurrencyID,s.EntryAmount,s.DefineCurrencyID,s.DefineAmount,s.AmtDefinitionCurrencyID,s.AmtDefinitionRate,s.SequenceNo,s.SalaryCategory
,sh.HeadCategory,sh.SalaryHead  FROM SalaryInfoDefine s
JOIN SalaryInfoDefineMaster M ON M.SystemID=s.SalaryID
JOIN trn.EmployeeLeaveSummary AS els ON els.EmployeeId=M.EmpInfoSystemID
JOIN trn.EmployeeLeaveSummary AS SX ON els.Id=SX.Id AND sX.Id=(SELECT TOP 1 x.Id FROM trn.EmployeeLeaveSummary X 
WHERE x.EmployeeId=els.EmployeeId AND x.LeaveTypeId=els.LeaveTypeId AND x.PlantId=els.PlantId  AND ISNULL(X.IsYearlyProcessed,0)=0 ORDER BY x.fromDate  ASC)
JOIN LeaveType AS lt ON lt.Id=els.LeaveTypeId
LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID --AND M.PlantID='" + PlantId + @"'
WHERE m.EffectiveDate<=els.ToDate AND lt.LeaveType='Earn' AND M.IsApproved=1 AND m.PlantID='" + PlantId + @"'

UNION

SELECT M.EmpInfoSystemID,M.EffectiveDate,els.ToDate, s.SystemID,s.SalaryID,s.SalaryHeadID,s.EntryCurrencyID,s.EntryAmount,s.DefineCurrencyID,s.DefineAmount,s.AmtDefinitionCurrencyID,s.AmtDefinitionRate,s.SequenceNo,s.SalaryCategory
,sh.HeadCategory,sh.SalaryHead  FROM SalaryInfoBack s
JOIN SalaryInfoBackMaster M ON M.SystemID=s.SalaryID
JOIN trn.EmployeeLeaveSummary AS els ON els.EmployeeId=M.EmpInfoSystemID
JOIN trn.EmployeeLeaveSummary AS SX ON ELS.Id=SX.Id AND sX.Id=(SELECT TOP 1 x.Id FROM trn.EmployeeLeaveSummary X 
WHERE x.EmployeeId=els.EmployeeId AND x.LeaveTypeId=els.LeaveTypeId AND x.PlantId=els.PlantId  AND ISNULL(X.IsYearlyProcessed,0)=0 ORDER BY x.fromDate  ASC)
JOIN LeaveType AS lt ON lt.Id=els.LeaveTypeId
LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
WHERE m.EffectiveDate<=els.ToDate AND lt.LeaveType='Earn' AND m.PlantID='" + PlantId + @"'
) AS K
) AS K WHERE K.RNK=1 ORDER BY k.EmpInfoSystemID";

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

        public void GetwithInYearEmployeeLeaveBalance(string PlantId, string FromDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (string.IsNullOrEmpty(FromDate))
                {
                    FromDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    //ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    strSQL = @" SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                             , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									--ELSE  isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
									ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes

							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No
                            ,'' LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.CarryForwardOpeningBalance,s.LeaveTypeId,yc.Id YearlyCalendarId,ltd.PolicyName
                            , BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo, BI.SalaryPercentage
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode,dm.EmployeeCategoryId
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    and (select ToDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
																				LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId	
																where dc.plantid='" + PlantId + @"' 
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                         
                          
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            LEFT JOIN YearlyCalendar yc ON  yc.Id=s.CalanderYearId
	                        LEFT JOIN HKP.EmployeeCategory AS EC ON dm.EmployeeCategoryId = EC.Id
							LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
                            where s.CalanderYearId=(select id from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')  --AND s.IsYearlyProcessed=1 
                           
                            AND  ltd.EncashmentBasis='DOJ'
                            AND s.EmployeeId NOT IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId=yc.Id AND LeaveEncashmentType='Encashment Within Year' and PlantId='" + PlantId + @"' )
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";
                }
                else
                {
                    strSQL = @" SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                             , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									--ELSE  isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
									  ELSE isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes

							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No
                            ,'' LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.CarryForwardOpeningBalance,s.LeaveTypeId,yc.Id YearlyCalendarId,ltd.PolicyName
                            , BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo, BI.SalaryPercentage
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode,dm.EmployeeCategoryId
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    and (select ToDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
																				LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId			
																where dc.plantid='" + PlantId + @"' 
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                         
                            
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            LEFT JOIN YearlyCalendar yc ON  yc.Id=s.CalanderYearId
	                        LEFT JOIN HKP.EmployeeCategory AS EC ON dm.EmployeeCategoryId = EC.Id
							LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
                            where s.CalanderYearId=(select id from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')  --AND s.IsYearlyProcessed=1 
                            AND s.EmployeeId IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + PlantId + @"'  
                                            AND DAY(DOJ)<=DAY('" + FromDate + @"') AND DAY(DOJ)>=DAY(yc.FromDate) 
                                            AND month(DOJ)<=month('" + FromDate + @"') AND month(DOJ)>=month(yc.FromDate)
                                            --AND DOJ <=  DATEADD(YEAR,-1, '" + FromDate + @"')
                                            AND  ltd.EncashmentBasis='DOJ' AND (CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END)<= Convert(date,'" + FromDate + @"')
                                           
                            )
                            AND s.EmployeeId NOT IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId=yc.Id AND LeaveEncashmentType='Encashment Within Year' and PlantId='" + PlantId + @"' )
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric ";
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
        public void GetSpecificDateEmployeeLeaveBalance(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @" SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 

                         FORMAT(CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
								   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
	 														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
	 													END
							    END,'dd-MMM-yyyy') AS MaturityDate,
							    FORMAT(DATEFROMPARTS( YEAR(getdate()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay)),'dd-MMM-yyyy') AS EncashmentDate,
   


                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                            ,EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									--ELSE  isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
									  ELSE isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes

							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No
                            ,'' LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.CarryForwardOpeningBalance,s.LeaveTypeId,yc.Id YearlyCalendarId,FORMAT(DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay)),'dd-MMM-yyyy') EncashmentSpecificDate,ltd.PolicyName
                            , BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo, BI.SalaryPercentage
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode,dm.EmployeeCategoryId
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where  '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    and (select ToDate from YearlyCalendar where '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId	
                                                                                LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId
																where dc.plantid='" + PlantId + @"' 
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                         
                            
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            LEFT JOIN YearlyCalendar yc ON  yc.Id=s.CalanderYearId
	                        LEFT JOIN HKP.EmployeeCategory AS EC ON dm.EmployeeCategoryId = EC.Id
							LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
                            where s.CalanderYearId=(select id from YearlyCalendar where DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay)) BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')  --AND s.IsYearlyProcessed=1 
                            AND s.EmployeeId IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + PlantId + @"'  )
                            AND   ltd.EncashmentBasis='EncashmentDate' 
                        --                          AND (CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                        --    																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
						--																			  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
						--																			  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
						--											   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
						--				   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
						--																			  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
						--																			  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
						--				   													END
						--										    END)<=DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay))
                                                                    AND DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay))<=GETDATE() 
                            AND s.EmployeeId NOT IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId=yc.Id AND LeaveEncashmentType='Specific Date Leave Encashment' and PlantId='" + PlantId + @"' )
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric ";



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

        public void GetYearlyEmployeeLeaveBalance(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select 
                
                           s.EmployeeId,  E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ


                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                             ,ISNULL( (SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                        INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                        where D.WorkDate BETWEEN S.FromDate and S.ToDate
                                AND m.EmpSystemID=S.EmployeeId AND m.LTSystemID=S.LeaveTypeId ),0) AS AvailedLeave
                            ,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull((SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                  INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                  where D.WorkDate BETWEEN s.FromDate and s.ToDate
                                AND m.EmpSystemID=s.EmployeeId AND m.LTSystemID=S.LeaveTypeId),0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							
                            ,'' LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.CarryForwardOpeningBalance,s.LeaveTypeId

                            , BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo, BI.SalaryPercentage
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode
            				
            				
            				
                            from trn.EmployeeLeaveSummary s 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId AND s.PlantId=e.PlantId
                            JOIN trn.EmployeeLeaveSummary AS Sx ON s.Id=sx.Id AND sx.Id=(SELECT TOP 1 x.Id FROM trn.EmployeeLeaveSummary X 
                                WHERE x.EmployeeId=S.EmployeeId AND x.LeaveTypeId=S.LeaveTypeId AND x.PlantId=S.PlantId  AND ISNULL(X.IsYearlyProcessed,0)=0 ORDER BY x.fromDate  ASC)

							LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
						   --------------------------------------------------------------------------
                            where  s.plantid ='" + PlantId + @"' 
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                            
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
        public void GetYearlyEmployeeLeaveBalanceNew(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select 
                
                           s.EmployeeId,  E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ


                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                             ,ISNULL( (SELECT sum(M.LvValue) FROM AttdnProcessData m 
                                        where m.WorkDate BETWEEN S.FromDate and S.ToDate
                                AND m.EmpSystemID=S.EmployeeId AND m.LTSystemID=S.LeaveTypeId ),0) AS AvailedLeave
                            ,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-ISNULL( (SELECT sum(M.LvValue) FROM AttdnProcessData m 
                                        where m.WorkDate BETWEEN S.FromDate and S.ToDate
                                AND m.EmpSystemID=S.EmployeeId AND m.LTSystemID=S.LeaveTypeId ),0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							                                   
							
                            ,'' LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.CarryForwardOpeningBalance,s.LeaveTypeId

                            , BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo, BI.SalaryPercentage
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode
            				
            				
            				
                            from trn.EmployeeLeaveSummary s 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId AND s.PlantId=e.PlantId
                            JOIN trn.EmployeeLeaveSummary AS Sx ON s.Id=sx.Id AND sx.Id=(SELECT TOP 1 x.Id FROM trn.EmployeeLeaveSummary X 
                                WHERE x.EmployeeId=S.EmployeeId AND x.LeaveTypeId=S.LeaveTypeId AND x.PlantId=S.PlantId  AND ISNULL(X.IsYearlyProcessed,0)=0 ORDER BY x.fromDate  ASC)

							LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
						   --------------------------------------------------------------------------
                            where  s.plantid ='" + PlantId + @"' 
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                            
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

        public void xGetwithInYearEmployeeLeaveBalance(string PlantId, string FromDate, string ToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (string.IsNullOrEmpty(FromDate))
                {
                    FromDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    strSQL = @" SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                             , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									ELSE  isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No
                            ,'' LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.LeaveTypeId
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    and (select ToDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId			
																where dc.plantid='20171'
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                         
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            where s.CalanderYearId=(select id from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')  --AND s.IsYearlyProcessed=1 
                           
                           
                            --AND s.EmployeeId NOT IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId='' AND LeaveEncashmentType='Year End Leave Encashment' and PlantId='" + PlantId + @"' )
                            order by CONVERT(INT, e.EmployeeCode) ";
                }
                else
                {
                    strSQL = @" SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                             , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									ELSE  isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No
                            ,'' LeaveEncashmentDayNo,s.EncashedInbetween,s.YearEndEncash,s.YearEndLapse,s.CarryForward,s.LeaveTypeId
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    and (select ToDate from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId			
																where dc.plantid='20171'
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                         
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            where s.CalanderYearId=(select id from YearlyCalendar where '" + FromDate + @"' BETWEEN FromDate AND ToDate and PlantId='" + PlantId + @"')  --AND s.IsYearlyProcessed=1 
                            AND s.EmployeeId IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + PlantId + @"'  
                                            AND DAY(DOJ)>=DAY('" + FromDate + @"') AND month(DOJ)>=month('" + FromDate + @"')
                                            AND DAY(DOJ)<=DAY('" + ToDate + @"') AND month(DOJ)<=month('" + ToDate + @"')
                                            AND YEAR(DOJ)<YEAR('" + ToDate + @"')
                            )
                            --AND s.EmployeeId NOT IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId='' AND LeaveEncashmentType='Year End Leave Encashment' and PlantId='" + PlantId + @"' )
                            order by CONVERT(INT, e.EmployeeCode) ";
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

        public List<MultipleLeaveEncashmentViewModel> GetMultipleLeaveEncashmentData(string LeaveEncashmentDate, string YearNo, string PlantId)
        {
            DataSet dsLvEncashment = null;
            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataSet dsAvailedEncashmentBalance = null;
            bool IsEarnLeavePolicyCount = false;
            decimal MaxEncashment = 0;
            DataSet dsSalHd = null;
            DataTable dtSlrHd = null;
            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            List<MultipleLeaveEncashmentViewModel> ob = new List<MultipleLeaveEncashmentViewModel>();







            List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
            GetSalaryHead(out dsSalHd);
            DataView dvsh = new DataView(dsSalHd.Tables[0]);
            DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

            if (dtSalHdx.Rows.Count > 0)
                dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();



            GetAllLeavePolicyDetails(PlantId, out dsEarnLeavePolicy);
            Dictionary<string, DataRow> DicEarnLeavePolicy = new Dictionary<string, DataRow>();
            for (int i = 0; i < dsEarnLeavePolicy.Tables[0].Rows.Count; i++)
            {
                DicEarnLeavePolicy.Add(dsEarnLeavePolicy.Tables[0].Rows[i]["EmpSystemId"].ToString(), dsEarnLeavePolicy.Tables[0].Rows[i]);
            }



            //GetEarnLeavePolicy(PlantId, out dsEarnLeavePolicy);

            //if (dsEarnLeavePolicy.Tables[0].Rows.Count > 0)
            //{
            //    //DataView dv = new DataView(dsSalaryDataEmpWise.Tables[0]);
            //    if (!string.IsNullOrEmpty(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString()))
            //    {
            //        IsEarnLeavePolicyCount = true;
            //        MaxEncashment = Convert.ToDecimal(dsEarnLeavePolicy.Tables[0].Rows[0]["MaxEncashment"].ToString());
            //    }
            //    else
            //    {
            //        throw new Exception("Earn Leave Policy not found.");
            //    }

            //}
            //else
            //{
            //    throw new Exception("No Salary Head defined on Earn Leave Policy.");
            //}





            GetMultipleEmployeeLeaveBalance(PlantId, YearNo, out dsLvEncashment);
            Dictionary<string, DataRow> DicLvEncashment = new Dictionary<string, DataRow>();
            for (int i = 0; i < dsLvEncashment.Tables[0].Rows.Count; i++)
            {
                DicLvEncashment.Add(dsLvEncashment.Tables[0].Rows[i]["SystemID"].ToString(), dsLvEncashment.Tables[0].Rows[i]);
            }


            GetMultipleEmployeeSalaryData(PlantId, LeaveEncashmentDate, out dsSalaryDataEmpWise);
            Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

            string _empId = "";
            List<DataRow> _data = new List<DataRow>();
            for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
            {
                if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                }
                _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
            }





            foreach (string key in DicLvEncashment.Keys)
            {
                List<SPvalueHeadWise> dtValue = null;
                DataRow dr = DicLvEncashment[key];
                IsEarnLeavePolicyCount = false;
                if (DicEarnLeavePolicy.ContainsKey(key) == true)
                {
                    DataRow drDicEarnLeavePolicy = DicEarnLeavePolicy[key];
                    if (drDicEarnLeavePolicy["EncashmentBasis"].ToString() == "CalanderYear" && !string.IsNullOrEmpty(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString()))
                    {
                        IsEarnLeavePolicyCount = true;

                        MultipleLeaveEncashmentViewModel o = new MultipleLeaveEncashmentViewModel();
                        o.EmpSystemId = dr["SystemID"].ToString();
                        o.EncashmentDate = LeaveEncashmentDate;
                        o.EmployeeCode = dr["EmployeeCode"].ToString();
                        o.EmployeeName = dr["EmployeeName"].ToString();
                        o.LeaveType = dr["LeaveType"].ToString();

                        o.CheckBoxSelect = Convert.ToBoolean(dr["CheckBoxSelect"].ToString());
                        o.Department = dr["Department"].ToString();
                        o.Designation = dr["Designation"].ToString();
                        o.EmpCategoryName = dr["EmpCategoryName"].ToString();
                        o.DOJ = dr["DOJ"].ToString();
                        o.Line = dr["Line"].ToString();

                        o.Section = dr["Section"].ToString();
                        o.SubSection = dr["SubSection"].ToString();
                        o.Unit = dr["Unit"].ToString();

                        o.Division = dr["Division"].ToString();



                        o.BankSystemID = dr["BankSystemID"].ToString();
                        o.BankBranchId = dr["BankBranchId"].ToString();
                        o.BankAccNo = dr["BankAccNo"].ToString();
                        //o.SalaryPercentage = dr["SalaryPercentage"].ToString();
                        //o.GivenDesignationId = dr["GivenDesignationId"].ToString();
                        o.PaymentMode = dr["PaymentMode"].ToString();
                        o.LegalDesignationId = dr["LegalDesignationId"].ToString();
                        //o.BudgetCode = dr["BudgetCode"].ToString();
                        //o.EmployeeCategoryId = dr["EmployeeCategoryId"].ToString();

                        if (!string.IsNullOrEmpty(dr["BroughtForward"].ToString()))
                        {
                            o.BroughtForward = Convert.ToDecimal(dr["BroughtForward"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["DaysCanBeSanctioned"].ToString()))
                        {
                            o.DaysCanBeSanctioned = Convert.ToDecimal(dr["DaysCanBeSanctioned"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["CurrentYearAllocation"].ToString()))
                        {
                            o.CurrentYearAllocation = Convert.ToDecimal(dr["CurrentYearAllocation"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["LeaveDaysAllowed"].ToString()))
                        {
                            o.LeaveDaysAllowed = Convert.ToDecimal(dr["LeaveDaysAllowed"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["AvailedLeave"].ToString()))
                        {
                            o.AvailedLeave = Convert.ToDecimal(dr["AvailedLeave"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["Balance"].ToString()))
                        {
                            o.Balance = Convert.ToDecimal(dr["Balance"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["EncashedInbetween"].ToString()))
                        {
                            o.EncashedInbetween = Convert.ToDecimal(dr["EncashedInbetween"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["YearEndEncash"].ToString()))
                        {
                            o.YearEndEncash = Convert.ToDecimal(dr["YearEndEncash"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["YearEndLapse"].ToString()))
                        {
                            o.YearEndLapse = Convert.ToDecimal(dr["YearEndLapse"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["CarryForward"].ToString()))
                        {
                            o.CarryForward = Convert.ToDecimal(dr["CarryForward"].ToString());
                        }


                        if (!string.IsNullOrEmpty(dr["LeaveTypeId"].ToString()))
                        {
                            o.LeaveTypeId = dr["LeaveTypeId"].ToString();
                        }


                        if (!string.IsNullOrEmpty(dr["PolicyName"].ToString()))
                        {
                            o.PolicyName = dr["PolicyName"].ToString();
                        }


                        if (!string.IsNullOrEmpty(dr["YearEndEncash"].ToString()))
                        {
                            o.Days = Convert.ToDecimal(dr["YearEndEncash"].ToString());
                        }

                        //if (dsAvailedEncashmentBalance.Tables[0].Rows.Count > 0)
                        //{

                        if (!string.IsNullOrEmpty(dr["LeaveEncashmentDayNo"].ToString()))
                        {
                            o.AvailedEncashment = Convert.ToDecimal(dr["LeaveEncashmentDayNo"].ToString());

                        }

                        if (IsEarnLeavePolicyCount == true)
                        {
                            if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                                continue;

                            List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                            #region Create Table                    
                            dtValue = new List<SPvalueHeadWise>();
                            #endregion Create Table
                            for (int j = 0; j < salaryStructure.Count; j++)
                            {
                                SPvalueHeadWise sp = new SPvalueHeadWise();
                                sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                dtValue.Add(sp);
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                                {
                                    o.BasicAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                }
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                                {
                                    o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                }
                            }
                            try
                            {
                                ReLoadFormulaWithGrossValueNew(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                //obSSrecal.ReLoadFormulaWithValue(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString(), ref dtValue, dsSalaryDataEmpWise.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                o.Rate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                        ob.Add(o);///


                    }


                }






            }




            return ob;
        }


        public List<MultipleLeaveEncashmentViewModel> GetWithInYearLeaveEncashmentData(string FromDate, string PlantId)
        {
            DataSet dsLvEncashment = null;
            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataSet dsAvailedEncashmentBalance = null;
            bool IsEarnLeavePolicyCount = false;
            decimal MaxEncashment = 0;
            DataSet dsSalHd = null;
            DataTable dtSlrHd = null;
            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            List<MultipleLeaveEncashmentViewModel> ob = new List<MultipleLeaveEncashmentViewModel>();



            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            string EarnleaveID = string.Empty;
            EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();



            List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
            GetSalaryHead(out dsSalHd);
            DataView dvsh = new DataView(dsSalHd.Tables[0]);
            DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

            if (dtSalHdx.Rows.Count > 0)
                dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();

            GetAllLeavePolicyDetails(PlantId, out dsEarnLeavePolicy);
            Dictionary<string, DataRow> DicEarnLeavePolicy = new Dictionary<string, DataRow>();
            for (int i = 0; i < dsEarnLeavePolicy.Tables[0].Rows.Count; i++)
            {
                DicEarnLeavePolicy.Add(dsEarnLeavePolicy.Tables[0].Rows[i]["EmpSystemId"].ToString(), dsEarnLeavePolicy.Tables[0].Rows[i]);
            }



            //GetEarnLeavePolicy(PlantId, out dsEarnLeavePolicy);

            //if (dsEarnLeavePolicy.Tables[0].Rows.Count > 0)
            //{
            //    //DataView dv = new DataView(dsSalaryDataEmpWise.Tables[0]);
            //    if (!string.IsNullOrEmpty(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString()))
            //    {
            //        IsEarnLeavePolicyCount = true;
            //        MaxEncashment = Convert.ToDecimal(dsEarnLeavePolicy.Tables[0].Rows[0]["MaxEncashment"].ToString());
            //    }
            //    else
            //    {
            //        throw new Exception("Earn Leave Policy not found.");
            //    }

            //}
            //else
            //{
            //    throw new Exception("No Salary Head defined on Earn Leave Policy.");
            //}





            GetwithInYearEmployeeLeaveBalance(PlantId, FromDate, out dsLvEncashment);
            Dictionary<string, DataRow> DicLvEncashment = new Dictionary<string, DataRow>();
            for (int i = 0; i < dsLvEncashment.Tables[0].Rows.Count; i++)
            {
                DicLvEncashment.Add(dsLvEncashment.Tables[0].Rows[i]["SystemID"].ToString(), dsLvEncashment.Tables[0].Rows[i]);
            }

            if (string.IsNullOrEmpty(FromDate))
            {
                FromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            }

            GetMultipleEmployeeSalaryData(PlantId, FromDate, out dsSalaryDataEmpWise);
            Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

            string _empId = "";
            List<DataRow> _data = new List<DataRow>();
            for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
            {
                if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                }
                _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
            }





            foreach (string key in DicLvEncashment.Keys)
            {
                if (key == "1800085")
                {

                }
                List<SPvalueHeadWise> dtValue = null;
                DataRow dr = DicLvEncashment[key];

                IsEarnLeavePolicyCount = false;
                if (DicEarnLeavePolicy.ContainsKey(key) == true)
                {
                    DataRow drDicEarnLeavePolicy = DicEarnLeavePolicy[key];
                    if (drDicEarnLeavePolicy["EncashmentBasis"].ToString() == "DOJ" && !string.IsNullOrEmpty(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString()))
                    {
                        IsEarnLeavePolicyCount = true;

                        MultipleLeaveEncashmentViewModel o = new MultipleLeaveEncashmentViewModel();
                        o.EmpSystemId = dr["SystemID"].ToString();
                        o.EncashmentDate = FromDate;
                        o.EmployeeCode = dr["EmployeeCode"].ToString();
                        o.EmployeeName = dr["EmployeeName"].ToString();
                        o.LeaveType = dr["LeaveType"].ToString();

                        o.CheckBoxSelect = Convert.ToBoolean(dr["CheckBoxSelect"].ToString());
                        o.Department = dr["Department"].ToString();
                        o.Designation = dr["Designation"].ToString();
                        o.EmpCategoryName = dr["EmpCategoryName"].ToString();
                        o.DOJ = dr["DOJ"].ToString();
                        o.Line = dr["Line"].ToString();

                        o.Section = dr["Section"].ToString();
                        o.SubSection = dr["SubSection"].ToString();
                        o.Unit = dr["Unit"].ToString();

                        o.Division = dr["Division"].ToString();
                        o.YearlyCalendarId = dr["YearlyCalendarId"].ToString();



                        o.BankSystemID = dr["BankSystemID"].ToString();
                        o.BankBranchId = dr["BankBranchId"].ToString();
                        o.BankAccNo = dr["BankAccNo"].ToString();
                        //o.SalaryPercentage = dr["SalaryPercentage"].ToString();
                        //o.GivenDesignationId = dr["GivenDesignationId"].ToString();
                        o.PaymentMode = dr["PaymentMode"].ToString();
                        o.LegalDesignationId = dr["LegalDesignationId"].ToString();
                        //o.BudgetCode = dr["BudgetCode"].ToString();
                        //o.EmployeeCategoryId = dr["EmployeeCategoryId"].ToString();
                        if (!string.IsNullOrEmpty(dr["BroughtForward"].ToString()))
                        {
                            o.BroughtForward = Convert.ToDecimal(dr["BroughtForward"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["CarryForwardOpeningBalance"].ToString()))
                        {
                            o.CarryForwardOpeningBalance = Convert.ToDecimal(dr["CarryForwardOpeningBalance"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["DaysCanBeSanctioned"].ToString()))
                        {
                            o.DaysCanBeSanctioned = Convert.ToDecimal(dr["DaysCanBeSanctioned"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["CurrentYearAllocation"].ToString()))
                        {
                            o.CurrentYearAllocation = Convert.ToDecimal(dr["CurrentYearAllocation"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["LeaveDaysAllowed"].ToString()))
                        {
                            o.LeaveDaysAllowed = Convert.ToDecimal(dr["LeaveDaysAllowed"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["AvailedLeave"].ToString()))
                        {
                            o.AvailedLeave = Convert.ToDecimal(dr["AvailedLeave"].ToString());
                        }
                        o.BroughtForward = o.BroughtForward + o.CarryForwardOpeningBalance;
                        if (!string.IsNullOrEmpty(dr["Balance"].ToString()))
                        {
                            o.Balance = Convert.ToDecimal(dr["Balance"].ToString()) + o.BroughtForward;
                        }

                        if (!string.IsNullOrEmpty(dr["EncashedInbetween"].ToString()))
                        {
                            o.EncashedInbetween = Convert.ToDecimal(dr["EncashedInbetween"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["YearEndEncash"].ToString()))
                        {
                            o.YearEndEncash = Convert.ToDecimal(dr["YearEndEncash"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["YearEndLapse"].ToString()))
                        {
                            o.YearEndLapse = Convert.ToDecimal(dr["YearEndLapse"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["CarryForward"].ToString()))
                        {
                            o.CarryForward = Convert.ToDecimal(dr["CarryForward"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["LeaveTypeId"].ToString()))
                        {
                            o.LeaveTypeId = dr["LeaveTypeId"].ToString();
                        }


                        CarryForword objCarryForword = CheckLeavePolicyDetails(PlantId, dr["SystemID"].ToString(), EarnleaveID, o.Balance);
                        //leaveResult = CalculateLeave(EmpSystemId, LeaveTypeId, sdsLeaveTranInfo);
                        if (objCarryForword != null)
                        {
                            o.Days = objCarryForword.CarryForwordEncash;
                            o.NewBroughtForward = objCarryForword.CarryForward;
                            o.NewYearEndEncash = objCarryForword.CarryForwordEncash;
                            o.NewYearEndLapse = objCarryForword.CarryForwordLapse;
                        }




                        if (!string.IsNullOrEmpty(dr["LeaveEncashmentDayNo"].ToString()))
                        {
                            o.AvailedEncashment = Convert.ToDecimal(dr["LeaveEncashmentDayNo"].ToString());

                        }


                        if (!string.IsNullOrEmpty(dr["PolicyName"].ToString()))
                        {
                            o.PolicyName = dr["PolicyName"].ToString();

                        }



                        if (IsEarnLeavePolicyCount == true)
                        {

                            if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                                continue;
                            List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                            #region Create Table                    
                            dtValue = new List<SPvalueHeadWise>();
                            #endregion Create Table

                            for (int j = 0; j < salaryStructure.Count; j++)
                            {
                                SPvalueHeadWise sp = new SPvalueHeadWise();
                                sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                dtValue.Add(sp);
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                                {
                                    o.BasicAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                }
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                                {
                                    o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                }
                            }
                            try
                            {
                                ReLoadFormulaWithGrossValueNew(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                //obSSrecal.ReLoadFormulaWithValue(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString(), ref dtValue, dsSalaryDataEmpWise.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                o.Rate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        ob.Add(o);
                    }
                }
            }




            return ob;
        }

        public List<MultipleLeaveEncashmentViewModel> GetSpecificDateLeaveEncashmentData(string PlantId)
        {
            DataSet dsLvEncashment = null;
            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataTable dtSlrHd = null;
            bool IsEarnLeavePolicyCount = false;

            DataSet dsSalHd = null;

            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            List<MultipleLeaveEncashmentViewModel> ob = new List<MultipleLeaveEncashmentViewModel>();



            clsLeaveYearEndProcess objLeaveYearEndProcessData;
            objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
            string EarnleaveID = string.Empty;
            EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();



            List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
            GetSalaryHead(out dsSalHd);
            DataView dvsh = new DataView(dsSalHd.Tables[0]);
            DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

            if (dtSalHdx.Rows.Count > 0)
                dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();

            GetAllLeavePolicyDetails(PlantId, out dsEarnLeavePolicy);
            Dictionary<string, DataRow> DicEarnLeavePolicy = new Dictionary<string, DataRow>();
            for (int i = 0; i < dsEarnLeavePolicy.Tables[0].Rows.Count; i++)
            {
                DicEarnLeavePolicy.Add(dsEarnLeavePolicy.Tables[0].Rows[i]["EmpSystemId"].ToString(), dsEarnLeavePolicy.Tables[0].Rows[i]);
            }








            GetSpecificDateEmployeeLeaveBalance(PlantId, out dsLvEncashment);
            Dictionary<string, DataRow> DicLvEncashment = new Dictionary<string, DataRow>();
            for (int i = 0; i < dsLvEncashment.Tables[0].Rows.Count; i++)
            {
                DicLvEncashment.Add(dsLvEncashment.Tables[0].Rows[i]["SystemID"].ToString(), dsLvEncashment.Tables[0].Rows[i]);
            }

            //if (string.IsNullOrEmpty(FromDate))
            //{
            //    FromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            //}

            GetMultipleEmployeeSalaryData(PlantId, DateTime.Now.ToString("dd-MMM-yyyy"), out dsSalaryDataEmpWise);
            Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

            string _empId = "";
            List<DataRow> _data = new List<DataRow>();
            for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
            {
                if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                }
                _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
            }





            foreach (string key in DicLvEncashment.Keys)
            {
                if (key == "1900391")
                {

                }
                List<SPvalueHeadWise> dtValue = null;
                DataRow dr = DicLvEncashment[key];

                IsEarnLeavePolicyCount = false;
                if (DicEarnLeavePolicy.ContainsKey(key) == true)
                {
                    DataRow drDicEarnLeavePolicy = DicEarnLeavePolicy[key];
                    if (drDicEarnLeavePolicy["EncashmentBasis"].ToString() == "EncashmentDate" && !string.IsNullOrEmpty(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString()))
                    {
                        IsEarnLeavePolicyCount = true;

                        MultipleLeaveEncashmentViewModel o = new MultipleLeaveEncashmentViewModel();
                        o.EmpSystemId = dr["SystemID"].ToString();
                        o.EncashmentDate = DateTime.Now.ToString("dd-MMM-yyyy");
                        o.EmployeeCode = dr["EmployeeCode"].ToString();
                        o.EmployeeName = dr["EmployeeName"].ToString();
                        o.LeaveType = dr["LeaveType"].ToString();

                        o.CheckBoxSelect = Convert.ToBoolean(dr["CheckBoxSelect"].ToString());
                        o.Department = dr["Department"].ToString();
                        o.Designation = dr["Designation"].ToString();
                        o.EmpCategoryName = dr["EmpCategoryName"].ToString();
                        o.DOJ = dr["DOJ"].ToString();
                        o.Line = dr["Line"].ToString();

                        o.Section = dr["Section"].ToString();
                        o.SubSection = dr["SubSection"].ToString();
                        o.Unit = dr["Unit"].ToString();

                        o.Division = dr["Division"].ToString();
                        o.YearlyCalendarId = dr["YearlyCalendarId"].ToString();

                        o.BankSystemID = dr["BankSystemID"].ToString();
                        o.BankBranchId = dr["BankBranchId"].ToString();
                        o.BankAccNo = dr["BankAccNo"].ToString();
                        //o.SalaryPercentage = dr["SalaryPercentage"].ToString();
                        //o.GivenDesignationId = dr["GivenDesignationId"].ToString();
                        o.PaymentMode = dr["PaymentMode"].ToString();
                        o.LegalDesignationId = dr["LegalDesignationId"].ToString();
                        //o.BudgetCode = dr["BudgetCode"].ToString();
                        //o.EmployeeCategoryId = dr["EmployeeCategoryId"].ToString();


                        if (!string.IsNullOrEmpty(dr["BroughtForward"].ToString()))
                        {
                            o.BroughtForward = Convert.ToDecimal(dr["BroughtForward"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["CarryForwardOpeningBalance"].ToString()))
                        {
                            o.CarryForwardOpeningBalance = Convert.ToDecimal(dr["CarryForwardOpeningBalance"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["DaysCanBeSanctioned"].ToString()))
                        {
                            o.DaysCanBeSanctioned = Convert.ToDecimal(dr["DaysCanBeSanctioned"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["CurrentYearAllocation"].ToString()))
                        {
                            o.CurrentYearAllocation = Convert.ToDecimal(dr["CurrentYearAllocation"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["LeaveDaysAllowed"].ToString()))
                        {
                            o.LeaveDaysAllowed = Convert.ToDecimal(dr["LeaveDaysAllowed"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["AvailedLeave"].ToString()))
                        {
                            o.AvailedLeave = Convert.ToDecimal(dr["AvailedLeave"].ToString());
                        }

                        o.BroughtForward = o.BroughtForward + o.CarryForwardOpeningBalance;
                        if (!string.IsNullOrEmpty(dr["Balance"].ToString()))
                        {
                            o.Balance = Convert.ToDecimal(dr["Balance"].ToString()) + o.BroughtForward;
                        }

                        if (!string.IsNullOrEmpty(dr["EncashedInbetween"].ToString()))
                        {
                            o.EncashedInbetween = Convert.ToDecimal(dr["EncashedInbetween"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["YearEndEncash"].ToString()))
                        {
                            o.YearEndEncash = Convert.ToDecimal(dr["YearEndEncash"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["YearEndLapse"].ToString()))
                        {
                            o.YearEndLapse = Convert.ToDecimal(dr["YearEndLapse"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["CarryForward"].ToString()))
                        {
                            o.CarryForward = Convert.ToDecimal(dr["CarryForward"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["LeaveTypeId"].ToString()))
                        {
                            o.LeaveTypeId = dr["LeaveTypeId"].ToString();
                        }
                        if (!string.IsNullOrEmpty(dr["EncashmentSpecificDate"].ToString()))
                        {
                            o.EncashmentSpecificDate = dr["EncashmentSpecificDate"].ToString();
                        }
                        if (!string.IsNullOrEmpty(dr["PolicyName"].ToString()))
                        {
                            o.PolicyName = dr["PolicyName"].ToString();
                        }
                        CarryForword objCarryForword = CheckLeavePolicyDetails(PlantId, dr["SystemID"].ToString(), EarnleaveID, o.Balance);
                        //leaveResult = CalculateLeave(EmpSystemId, LeaveTypeId, sdsLeaveTranInfo);
                        o.FixedEncashmentDate = dr["EncashmentDate"].ToString();
                        o.MaturityDate = dr["MaturityDate"].ToString();
                        if (objCarryForword != null)
                        {



                            if (Convert.ToDateTime(dr["EncashmentDate"].ToString()) < Convert.ToDateTime(dr["MaturityDate"].ToString()))
                            {
                                o.Days = 0;
                                o.NewBroughtForward = objCarryForword.CarryForwordEncash + objCarryForword.CarryForward;
                                o.NewYearEndEncash = 0;
                                o.NewYearEndLapse = 0;
                            }
                            else
                            {
                                o.Days = objCarryForword.CarryForwordEncash;
                                o.NewBroughtForward = objCarryForword.CarryForward;
                                o.NewYearEndEncash = objCarryForword.CarryForwordEncash;
                                o.NewYearEndLapse = objCarryForword.CarryForwordLapse;
                            }
                        }




                        if (!string.IsNullOrEmpty(dr["LeaveEncashmentDayNo"].ToString()))
                        {
                            o.AvailedEncashment = Convert.ToDecimal(dr["LeaveEncashmentDayNo"].ToString());

                        }



                        if (IsEarnLeavePolicyCount == true)
                        {

                            if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                                continue;
                            List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                            #region Create Table                    
                            dtValue = new List<SPvalueHeadWise>();
                            #endregion Create Table

                            for (int j = 0; j < salaryStructure.Count; j++)
                            {
                                SPvalueHeadWise sp = new SPvalueHeadWise();
                                sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                dtValue.Add(sp);
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                                {
                                    o.BasicAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                }
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                                {
                                    o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                }
                            }
                            try
                            {
                                //o.FixedEncashmentDate = dr["EncashmentDate"].ToString();
                                //o.MaturityDate = dr["MaturityDate"].ToString();


                                //if (Convert.ToDateTime(dr["EncashmentDate"].ToString()) < Convert.ToDateTime(dr["MaturityDate"].ToString()))
                                //    o.Rate = 0;
                                //else
                                //{
                                ReLoadFormulaWithGrossValueNew(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                                o.Rate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                                //}
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }




                        }
                        ob.Add(o);
                    }
                }
            }




            return ob;
        }


        public DataSet SaveYearEndDateLeaveEncashmentData(string PlantId, DataSet dsLvEncashment)
        {

            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataTable dtSlrHd = null;
            bool IsEarnLeavePolicyCount = false;
            DataSet dsEncashment = null;
            DataSet dsSalHd = null;

            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            List<MultipleLeaveEncashmentViewModel> ob = new List<MultipleLeaveEncashmentViewModel>();

            try
            {



                clsLeaveYearEndProcess objLeaveYearEndProcessData;
                objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
                string EarnleaveID = string.Empty;
                EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();



                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();

                GetAllLeavePolicyDetails(PlantId, out dsEarnLeavePolicy);
                Dictionary<string, DataRow> DicEarnLeavePolicy = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsEarnLeavePolicy.Tables[0].Rows.Count; i++)
                {
                    DicEarnLeavePolicy.Add(dsEarnLeavePolicy.Tables[0].Rows[i]["EmpSystemId"].ToString(), dsEarnLeavePolicy.Tables[0].Rows[i]);
                }


                Dictionary<string, DataRow> DicLvEncashment = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsLvEncashment.Tables[0].Rows.Count; i++)
                {
                    DicLvEncashment.Add(dsLvEncashment.Tables[0].Rows[i]["EmployeeId"].ToString(), dsLvEncashment.Tables[0].Rows[i]);
                }

                GetYearlyEmployeeLeaveBalance(PlantId, out DataSet dsEmployeeInformation);
                Dictionary<string, DataRow> DicEmployeeInformation = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsEmployeeInformation.Tables[0].Rows.Count; i++)
                {
                    DicEmployeeInformation.Add(dsEmployeeInformation.Tables[0].Rows[i]["EmployeeId"].ToString(), dsEmployeeInformation.Tables[0].Rows[i]);
                }


                GetMultipleEmployeeSalaryDataOnYearEnd(PlantId, out dsSalaryDataEmpWise);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                {
                    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                }





                foreach (string key in DicLvEncashment.Keys)
                {
                    if (key == "1900391")
                    {

                    }
                    List<SPvalueHeadWise> dtValue = null;
                    DataRow dr = DicEmployeeInformation[key];
                    DataRow drOldSummary = DicLvEncashment[key];

                    IsEarnLeavePolicyCount = false;
                    if (DicEarnLeavePolicy.ContainsKey(key) == true)
                    {
                        DataRow drDicEarnLeavePolicy = DicEarnLeavePolicy[key];
                        if (!string.IsNullOrEmpty(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString()))
                        {
                            IsEarnLeavePolicyCount = true;

                            MultipleLeaveEncashmentViewModel o = new MultipleLeaveEncashmentViewModel();
                            o.EmpSystemId = dr["EmployeeId"].ToString();
                            o.EmployeeCode = dr["EmployeeCode"].ToString();
                            o.EmployeeName = dr["EmployeeName"].ToString();
                            o.LeaveType = dr["LeaveType"].ToString();
                            o.LeaveTypeId = dr["LeaveTypeId"].ToString();

                            o.DOJ = dr["DOJ"].ToString();

                            o.AvailedLeave = (decimal)clsStaticInfo.dbl(dr["AvailedLeave"].ToString());
                            o.BankSystemID = dr["BankSystemID"].ToString();
                            o.BankBranchId = dr["BankBranchId"].ToString();
                            o.BankAccNo = dr["BankAccNo"].ToString();
                            o.PaymentMode = dr["PaymentMode"].ToString();
                            o.LegalDesignationId = dr["LegalDesignationId"].ToString();

                            o.EncashmentDate = drOldSummary["ToDate"].ToString();
                            o.Days = (decimal)clsStaticInfo.dbl(drOldSummary["YearEndEncash"].ToString());
                            o.BroughtForward = (decimal)clsStaticInfo.dbl(drOldSummary["BroughtForward"].ToString());
                            o.CarryForward = (decimal)clsStaticInfo.dbl(drOldSummary["CarryForward"].ToString());
                            o.YearEndLapse = (decimal)clsStaticInfo.dbl(drOldSummary["YearEndLapse"].ToString());
                            o.CurrentYearAllocation = (decimal)clsStaticInfo.dbl(drOldSummary["CurrentYearAllocation"].ToString());
                            o.DaysCanBeSanctioned = (decimal)clsStaticInfo.dbl(drOldSummary["DaysCanBeSanctioned"].ToString());
                            o.AvailedEncashment = (decimal)clsStaticInfo.dbl(drOldSummary["CalculatedEarningDays"].ToString());

                            if (IsEarnLeavePolicyCount == true)
                            {

                                if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                                    continue;
                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table

                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                    if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                                    {
                                        o.BasicAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    }
                                    if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                                    {
                                        o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    }
                                }
                                try
                                {

                                    ReLoadFormulaWithGrossValueNew(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                                    o.Rate = (decimal)clsStaticInfo.dbl(string.Format("{0:F2}", sFormulaResult));

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }




                            }
                            ob.Add(o);
                        }
                    }
                }



                //SAVE leaveencashment data
                GetYearEndLeaveTransaction(PlantId, out dsEncashment);
                Dictionary<string, DataRow> dicEncashment = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsEncashment.Tables[0].Rows.Count; i++)
                    dicEncashment.Add(dsEncashment.Tables[0].Rows[i]["EmpSystemId"].ToString() + "-" + dsEncashment.Tables[0].Rows[i]["LeaveTypeSystemId"].ToString(), dsEncashment.Tables[0].Rows[i]);

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string Id = "";
                for (int i = 0; i < ob.Count; i++)
                {
                    string Key = ob[i].EmpSystemId + "-" + ob[i].LeaveTypeId;

                    if (dicEncashment.ContainsKey(Key) == false)
                    {
                        //add new
                        if (Id == "")
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "YEAR END PROCESS ENCASHMENT", out sID);
                            Id = "Y-" + sID;
                        }
                        DataRow dr = dsEncashment.Tables[0].NewRow();
                        dr["Id"] = Id + "-" + (i + 1).ToString();
                        dr["PlantId"] = PlantId.ToString();
                        dr["EmpSystemId"] = ob[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Specific Date Leave Encashment";
                        dr["EncashmentDate"] = ob[i].EncashmentDate;
                        dr["Days"] = clsStaticInfo.dbl(ob[i].Days);
                        dr["LeaveTypeSystemId"] = ob[i].LeaveTypeId;
                        dr["Rate"] = clsStaticInfo.dbl(ob[i].Rate);

                        dr["PaymentMode"] = ob[i].PaymentMode;
                        dr["BasicAmmount"] = clsStaticInfo.dbl(ob[i].BasicAmmount);
                        dr["GrossAmmount"] = clsStaticInfo.dbl(ob[i].GrossAmmount);
                        dr["BankSystemID"] = ob[i].BankSystemID;
                        dr["BankBranchId"] = ob[i].BankBranchId;
                        dr["BankAccNo"] = ob[i].BankAccNo;
                        dr["LegalDesignationId"] = ob[i].LegalDesignationId;
                        dr["BroughtForward"] = clsStaticInfo.dbl(ob[i].BroughtForward);
                        dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(ob[i].DaysCanBeSanctioned);
                        dr["AvailedLeave"] = clsStaticInfo.dbl(ob[i].AvailedLeave);
                        dr["CarryForward"] = clsStaticInfo.dbl(ob[i].CarryForward);
                        dr["Isdisburse"] = false;
                        dr["YearlyCalendarId"] = DBNull.Value;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsEncashment.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dicEncashment[Key];
                        if (bplib.clsWebLib.GetBoolData(dr["isApproved"].ToString()))
                            continue;

                        dr.BeginEdit();

                        dr["PlantId"] = PlantId.ToString();
                        dr["EmpSystemId"] = ob[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Specific Date Leave Encashment";
                        dr["EncashmentDate"] = ob[i].EncashmentDate;
                        dr["Days"] = clsStaticInfo.dbl(ob[i].Days);
                        dr["LeaveTypeSystemId"] = ob[i].LeaveTypeId;
                        dr["Rate"] = clsStaticInfo.dbl(ob[i].Rate);

                        dr["PaymentMode"] = ob[i].PaymentMode;
                        dr["BasicAmmount"] = clsStaticInfo.dbl(ob[i].BasicAmmount);
                        dr["GrossAmmount"] = clsStaticInfo.dbl(ob[i].GrossAmmount);
                        dr["BankSystemID"] = ob[i].BankSystemID;
                        dr["BankBranchId"] = ob[i].BankBranchId;
                        dr["BankAccNo"] = ob[i].BankAccNo;
                        dr["LegalDesignationId"] = ob[i].LegalDesignationId;
                        dr["BroughtForward"] = clsStaticInfo.dbl(ob[i].BroughtForward);
                        dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(ob[i].DaysCanBeSanctioned);
                        dr["AvailedLeave"] = clsStaticInfo.dbl(ob[i].AvailedLeave);
                        dr["CarryForward"] = clsStaticInfo.dbl(ob[i].CarryForward);
                        dr["Isdisburse"] = false;
                        dr["YearlyCalendarId"] = DBNull.Value;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return dsEncashment;
        }

        public DataSet SaveYearEndDateLeaveEncashmentDataForNewAttendance(string PlantId, DataSet dsLvEncashment)
        {

            DataSet dsEarnLeavePolicy = null;
            DataSet dsSalaryDataEmpWise = null;
            DataTable dtSlrHd = null;
            bool IsEarnLeavePolicyCount = false;
            DataSet dsEncashment = null;
            DataSet dsSalHd = null;

            string _formulaValue = string.Empty;
            string sFormulaResult = string.Empty;
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            List<MultipleLeaveEncashmentViewModel> ob = new List<MultipleLeaveEncashmentViewModel>();

            try
            {



                clsLeaveYearEndProcess objLeaveYearEndProcessData;
                objLeaveYearEndProcessData = new clsLeaveYearEndProcess();
                string EarnleaveID = string.Empty;
                EarnleaveID = objLeaveYearEndProcessData.GetEarnLeaveID();



                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();

                GetAllLeavePolicyDetails(PlantId, out dsEarnLeavePolicy);
                Dictionary<string, DataRow> DicEarnLeavePolicy = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsEarnLeavePolicy.Tables[0].Rows.Count; i++)
                {
                    DicEarnLeavePolicy.Add(dsEarnLeavePolicy.Tables[0].Rows[i]["EmpSystemId"].ToString(), dsEarnLeavePolicy.Tables[0].Rows[i]);
                }


                Dictionary<string, DataRow> DicLvEncashment = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsLvEncashment.Tables[0].Rows.Count; i++)
                {
                    DicLvEncashment.Add(dsLvEncashment.Tables[0].Rows[i]["EmployeeId"].ToString(), dsLvEncashment.Tables[0].Rows[i]);
                }

                GetYearlyEmployeeLeaveBalanceNew(PlantId, out DataSet dsEmployeeInformation);
                Dictionary<string, DataRow> DicEmployeeInformation = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsEmployeeInformation.Tables[0].Rows.Count; i++)
                {
                    DicEmployeeInformation.Add(dsEmployeeInformation.Tables[0].Rows[i]["EmployeeId"].ToString(), dsEmployeeInformation.Tables[0].Rows[i]);
                }


                GetMultipleEmployeeSalaryDataOnYearEnd(PlantId, out dsSalaryDataEmpWise);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                {
                    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                }





                foreach (string key in DicLvEncashment.Keys)
                {
                    if (key == "1900391")
                    {

                    }
                    List<SPvalueHeadWise> dtValue = null;
                    DataRow dr = DicEmployeeInformation[key];
                    DataRow drOldSummary = DicLvEncashment[key];

                    IsEarnLeavePolicyCount = false;
                    if (DicEarnLeavePolicy.ContainsKey(key) == true)
                    {
                        DataRow drDicEarnLeavePolicy = DicEarnLeavePolicy[key];
                        if (!string.IsNullOrEmpty(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString()))
                        {
                            IsEarnLeavePolicyCount = true;

                            MultipleLeaveEncashmentViewModel o = new MultipleLeaveEncashmentViewModel();
                            o.EmpSystemId = dr["EmployeeId"].ToString();
                            o.EmployeeCode = dr["EmployeeCode"].ToString();
                            o.EmployeeName = dr["EmployeeName"].ToString();
                            o.LeaveType = dr["LeaveType"].ToString();
                            o.LeaveTypeId = dr["LeaveTypeId"].ToString();

                            o.DOJ = dr["DOJ"].ToString();

                            o.AvailedLeave = (decimal)clsStaticInfo.dbl(dr["AvailedLeave"].ToString());
                            o.BankSystemID = dr["BankSystemID"].ToString();
                            o.BankBranchId = dr["BankBranchId"].ToString();
                            o.BankAccNo = dr["BankAccNo"].ToString();
                            o.PaymentMode = dr["PaymentMode"].ToString();
                            o.LegalDesignationId = dr["LegalDesignationId"].ToString();

                            o.EncashmentDate = drOldSummary["ToDate"].ToString();
                            o.Days = (decimal)clsStaticInfo.dbl(drOldSummary["YearEndEncash"].ToString());
                            o.BroughtForward = (decimal)clsStaticInfo.dbl(drOldSummary["BroughtForward"].ToString());
                            o.CarryForward = (decimal)clsStaticInfo.dbl(drOldSummary["CarryForward"].ToString());
                            o.YearEndLapse = (decimal)clsStaticInfo.dbl(drOldSummary["YearEndLapse"].ToString());
                            o.CurrentYearAllocation = (decimal)clsStaticInfo.dbl(drOldSummary["CurrentYearAllocation"].ToString());
                            o.DaysCanBeSanctioned = (decimal)clsStaticInfo.dbl(drOldSummary["DaysCanBeSanctioned"].ToString());
                            o.AvailedEncashment = (decimal)clsStaticInfo.dbl(drOldSummary["CalculatedEarningDays"].ToString());

                            if (IsEarnLeavePolicyCount == true)
                            {

                                if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                                    continue;
                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table

                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                    if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                                    {
                                        o.BasicAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    }
                                    if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                                    {
                                        o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    }
                                }
                                try
                                {

                                    ReLoadFormulaWithGrossValueNew(drDicEarnLeavePolicy["LvEncashmentFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                                    o.Rate = (decimal)clsStaticInfo.dbl(string.Format("{0:F2}", sFormulaResult));

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }




                            }
                            ob.Add(o);
                        }
                    }
                }



                //SAVE leaveencashment data
                GetYearEndLeaveTransaction(PlantId, out dsEncashment);
                Dictionary<string, DataRow> dicEncashment = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsEncashment.Tables[0].Rows.Count; i++)
                    dicEncashment.Add(dsEncashment.Tables[0].Rows[i]["EmpSystemId"].ToString() + "-" + dsEncashment.Tables[0].Rows[i]["LeaveTypeSystemId"].ToString(), dsEncashment.Tables[0].Rows[i]);

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string Id = "";
                for (int i = 0; i < ob.Count; i++)
                {
                    string Key = ob[i].EmpSystemId + "-" + ob[i].LeaveTypeId;

                    if (dicEncashment.ContainsKey(Key) == false)
                    {
                        //add new
                        if (Id == "")
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "YEAR END PROCESS ENCASHMENT", out sID);
                            Id = "Y-" + sID;
                        }
                        DataRow dr = dsEncashment.Tables[0].NewRow();
                        dr["Id"] = Id + "-" + (i + 1).ToString();
                        dr["PlantId"] = PlantId.ToString();
                        dr["EmpSystemId"] = ob[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Specific Date Leave Encashment";
                        dr["EncashmentDate"] = ob[i].EncashmentDate;
                        dr["Days"] = clsStaticInfo.dbl(ob[i].Days);
                        dr["LeaveTypeSystemId"] = ob[i].LeaveTypeId;
                        dr["Rate"] = clsStaticInfo.dbl(ob[i].Rate);

                        dr["PaymentMode"] = ob[i].PaymentMode;
                        dr["BasicAmmount"] = clsStaticInfo.dbl(ob[i].BasicAmmount);
                        dr["GrossAmmount"] = clsStaticInfo.dbl(ob[i].GrossAmmount);
                        dr["BankSystemID"] = ob[i].BankSystemID;
                        dr["BankBranchId"] = ob[i].BankBranchId;
                        dr["BankAccNo"] = ob[i].BankAccNo;
                        dr["LegalDesignationId"] = ob[i].LegalDesignationId;
                        dr["BroughtForward"] = clsStaticInfo.dbl(ob[i].BroughtForward);
                        dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(ob[i].DaysCanBeSanctioned);
                        dr["AvailedLeave"] = clsStaticInfo.dbl(ob[i].AvailedLeave);
                        dr["CarryForward"] = clsStaticInfo.dbl(ob[i].CarryForward);
                        dr["Isdisburse"] = false;
                        dr["YearlyCalendarId"] = DBNull.Value;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsEncashment.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dicEncashment[Key];
                        dr.BeginEdit();

                        dr["PlantId"] = PlantId.ToString();
                        dr["EmpSystemId"] = ob[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Specific Date Leave Encashment";
                        dr["EncashmentDate"] = ob[i].EncashmentDate;
                        dr["Days"] = clsStaticInfo.dbl(ob[i].Days);
                        dr["LeaveTypeSystemId"] = ob[i].LeaveTypeId;
                        dr["Rate"] = clsStaticInfo.dbl(ob[i].Rate);

                        dr["PaymentMode"] = ob[i].PaymentMode;
                        dr["BasicAmmount"] = clsStaticInfo.dbl(ob[i].BasicAmmount);
                        dr["GrossAmmount"] = clsStaticInfo.dbl(ob[i].GrossAmmount);
                        dr["BankSystemID"] = ob[i].BankSystemID;
                        dr["BankBranchId"] = ob[i].BankBranchId;
                        dr["BankAccNo"] = ob[i].BankAccNo;
                        dr["LegalDesignationId"] = ob[i].LegalDesignationId;
                        dr["BroughtForward"] = clsStaticInfo.dbl(ob[i].BroughtForward);
                        dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(ob[i].DaysCanBeSanctioned);
                        dr["AvailedLeave"] = clsStaticInfo.dbl(ob[i].AvailedLeave);
                        dr["CarryForward"] = clsStaticInfo.dbl(ob[i].CarryForward);
                        dr["Isdisburse"] = false;
                        dr["YearlyCalendarId"] = DBNull.Value;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return dsEncashment;
        }


        public CarryForword CheckLeavePolicyDetails(string sPlantID, string sEmployeeId, string LeaveTypeId, decimal LeaveBalance)
        {
            CarryForword objCarryForword = new CarryForword();
            DataSet dsLeavePld = null;

            bool IsCFFixed = false;
            bool IsCarryforward = false;
            decimal CarryForwardMaxDay = 0;
            decimal MaxAllocationLimit = 0;

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
                dv.RowFilter = "LTSystemID='" + LeaveTypeId + "'";
                if (dv.Count > 0)
                {
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


                    //LeaveTran = CalculateLeave(sEmployeeId, LeaveTypeId, sdsLeaveTranInfo);
                    //newCarryForward = DaysCanBeSanctioned + CarryforwardOB - (LeaveTran + CurrentYearAvailedOpeningBalance);
                    if (LeaveBalance > 0)
                    {
                        objCarryForword = GetCarryforwardQnty(IsCarryforward, IsCFFixed, CarryForwardRoundupOption, LeaveBalance, CarryForwardMaxDay, IsCFRestEncash, IsMaxEncashment, MaxEncashment);
                    }
                    else
                    {
                        objCarryForword = null;
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

        public CarryForword GetCarryforwardQnty(bool IsCarryForward, bool IsCFFixed, string CFRoundupOption, decimal newCarryForward, decimal CarryForwardMaxDay, bool IsCFRestEncash, bool IsCFCRestEncashMaxLimit, decimal CFEncashMaxLimit)
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
                    CarryforwardTemp = (newCarryForward * CarryForwardMaxDay) / 100;
                    CarryforwardResult = GetRoundupOption(CFRoundupOption, CarryforwardTemp);
                    var rest = newCarryForward - CarryforwardResult;
                    //var rest = GetRoundupOptionForEncashment(CFRoundupOption, resttemp);

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
                        CarryForwordEncash = 0;
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
        public void GetLeavePolicyDetails(string sPlantID, string sEmployeeId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @"select lpd.* from dbo.LeavePolicyDetail as lpd
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





        public void ReLoadFormulaWithGrossValueNew(string strFormulaID, string sLocalCurrencyID, string sForeignCurRate,
        out string sFormulaValue, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
        {
            DataSet dsLocal = null;
            //DataView dvLocal = null;
            //DataView dvSlrHd = null;
            string strTemp = "";

            try
            {

                dsLocal = new DataSet();
                string strFormulaIDTemp = strFormulaID.Trim();
                //string sLocalCurrencyID = para.lblLocalCurrencyID;
                //string sForeignCurRate = para.lblLocalCurRate;

                if (sForeignCurRate == "")
                { sForeignCurRate = "1"; }

                sFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == ">" || strTemp.Trim() == "<" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {

                        var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                        if (dtv.Count() > 0)
                        {

                            if (dtv[0].EntryCurrencyID == sLocalCurrencyID)
                            {
                                strTemp = dtv[0].EntryAmount;
                                strTemp = GetAbsValue(strTemp);
                            }
                            else
                            {
                                strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                strTemp = " " + GetAbsValue(strTemp) + " ";
                            }


                        }
                        else
                        {
                            var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                            if (dicsh.Count() > 0)
                            {
                                strTemp = "0.00";
                            }
                        }


                    }


                    sFormulaValue += strTemp.Trim();
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


        string GetAbsValue(string strTemp)
        {
            try
            {
                var vv = Math.Abs(Convert.ToDecimal(strTemp.Trim()));
                string _vv = vv.ToString();
                return _vv;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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


        public void GetAllLeavePolicyDetails(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @" SELECT emp.SystemId EmpSystemId, lpd.*  from dbo.LeavePolicyDetail as lpd
                            LEFT JOIN dbo.LeavePolicyMaster as lpm on lpd.LPMSystemID = lpm.SystemID
                            LEFT JOIN (select * from SCS.DesignationMasterConfiguration where PlantId='" + sPlantID + @"') DC on   lpm.SystemID = DC.LeavePolicyMasterId
                            LEFT JOIN MST.DesignationMaster DM on  DC.DesignationMasterId=DM.Id
                            LEFT JOIN dbo.EmployeeInformation emp on emp.GivenDesignationId=DM.DesignationId
                            where lpd.IsCarryForward=1 AND emp.SystemId IS NOT NULL AND lpd.LTSystemID IN (SELECT id FROM LeaveType WHERE LeaveType='Earn') 
							ORDER BY emp.SystemId ";



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



        public void GetYearEndLeaveTransaction(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @"SELECT * FROM LeaveEncashmentTransaction AS let WHERE let.Id IN (

                            SELECT let.Id from trn.EmployeeLeaveSummary s 
                                                        INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                                                        INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId AND s.PlantId=e.PlantId
                                                        JOIN trn.EmployeeLeaveSummary AS Sx ON S.Id=sx.Id AND sx.Id=(SELECT TOP 1 x.Id FROM trn.EmployeeLeaveSummary X 
                                                            WHERE x.EmployeeId=S.EmployeeId AND x.LeaveTypeId=S.LeaveTypeId AND x.PlantId=S.PlantId  AND ISNULL(X.IsYearlyProcessed,0)=0 ORDER BY x.fromDate  ASC)
                            JOIN LeaveEncashmentTransaction AS let ON let.EmpSystemId=s.EmployeeId AND let.LeaveTypeSystemId=s.LeaveTypeId
                            AND let.EncashmentDate=s.ToDate
                            AND e.PlantId='" + sPlantID + @"'
                            )";



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








































        public string sFormulaValue = "";
        public EmployeeFinalSettlement CalculateFinalSettlementValue(string sEmpSystemId)
        {

            EmployeeFinalSettlement obj = new EmployeeFinalSettlement();
            DataTable dtSlrHd = null;
            DataSet dsSalHd = null;
            DataSet dsSalaryData = null;
            DataSet dsProcSalaryData = null;
            DataSet dsSeparationType = null;
            DataSet dsSeparationTypeDetails = null;
            DataSet dsTenure = null;
            bool isFixedDayAmountApplicable = false;
            bool isGratuityApplicable = false;
            string _formulaValue = "0";
            string sFormulaResult = "0";
            decimal sTotalAmount = 0;
            decimal sGratuityAmount = 0;
            decimal sFixedDayAmount = 0;
            decimal sGrossAmount = 0;
            decimal sBasicAmount = 0;
            decimal sOTRate = 0;
            decimal sSalaryRate = 0;
            decimal NumberOfDays = 0;
            decimal NumberOfYears = 0;
            decimal NumberOfFixedDays = 0;


            try
            {
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();

                //Separation Type and Employee info
                GetSeparationTypeByEmpId(sEmpSystemId, out dsSeparationType);
                if (dsSeparationType.Tables[0].Rows.Count > 0)
                {
                    GetSeparationTypeDetailsById(dsSeparationType.Tables[0].Rows[0]["Id"].ToString(), out dsSeparationTypeDetails);
                    obj.SeparationTypeId = dsSeparationType.Tables[0].Rows[0]["Id"].ToString();
                    obj.SeparationTypeName = dsSeparationType.Tables[0].Rows[0]["UserName"].ToString();
                    isGratuityApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsGratuityApplicable"].ToString());
                    isFixedDayAmountApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsFixedDayAmountApplicable"].ToString());
                }
                else
                {
                    throw new Exception("This Employee has no Separation Type.");
                }

                GetTenureByEmpId(sEmpSystemId, out dsTenure);

                //all head and Salary info
                GetSalaryHead(out dsSalHd);
                dtSlrHd = dsSalHd.Tables[0];


                GetSalaryDataEmpWise(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                if (dsSalaryData.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("This Employee has no Approved Salary Structure.");
                }



                DataTable dtValue = new DataTable();
                dtValue.TableName = "TempTable";
                dtValue.Columns.Add("SalaryHeadID");
                dtValue.Columns.Add("EntryCurrencyID");
                dtValue.Columns.Add("Amount");


                for (int i = 0; i < dsSalaryData.Tables[0].Rows.Count; i++)
                {
                    DataRow dtValueRow = dtValue.NewRow();
                    dtValueRow["SalaryHeadID"] = dsSalaryData.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                    dtValueRow["EntryCurrencyID"] = dsSalaryData.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                    dtValueRow["Amount"] = dsSalaryData.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                    dtValue.Rows.Add(dtValueRow);
                }
                obSSrecal.ReLoadFormulaWithValue(dsSeparationType.Tables[0].Rows[0]["FormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                //calculation
                if (dsTenure.Tables[0].Rows.Count > 0)
                {
                    DataView dvSeparationTypeDetails = new DataView(dsSeparationTypeDetails.Tables[0]);
                    TimeSpan DaysNo = TimeSpan.FromDays(Convert.ToInt32(dsTenure.Tables[0].Rows[0]["TenureInDays"].ToString()));
                    DateTime zeroTime = new DateTime(1, 1, 1);
                    //int years = (zeroTime + DaysNo).Year - 1;
                    //int month = (zeroTime + DaysNo).Month - 1;
                    //int days = (zeroTime + DaysNo).Day-1;

                    DateTime Now = Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"].ToString());
                    int years = new DateTime(DateTime.Now.Subtract(Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).Ticks).Year - 1;
                    DateTime PastYearDate = (Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).AddYears(years);
                    int month = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        if (PastYearDate.AddMonths(i) == Now)
                        {
                            month = i;
                            break;
                        }
                        else if (PastYearDate.AddMonths(i) >= Now)
                        {
                            month = i - 1;
                            break;
                        }
                    }
                    int days = Now.Subtract(PastYearDate.AddMonths(month)).Days + 1;
                    int Hours = Now.Subtract(PastYearDate).Hours;
                    int Minutes = Now.Subtract(PastYearDate).Minutes;
                    int Seconds = Now.Subtract(PastYearDate).Seconds;








                    obj.TenureDayNo = days;
                    obj.TenureMonthNo = month;
                    obj.TenureYearNo = years;
                    obj.OTRate = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["OTRate"].ToString()));
                    obj.LastMonthProcDay = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["TotalProcDate"].ToString()));
                    obj.LastMonthAbsentDay = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["TotalAbsent"].ToString()));
                    obj.LastMonthOTHour = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["TotalOTHr"].ToString()));

                    //Count Days
                    if (years > 0)
                    {
                        dvSeparationTypeDetails.RowFilter = "Yearno='" + years + "'";
                        if (dvSeparationTypeDetails.Count > 0)
                        {
                            if (Convert.ToBoolean(dvSeparationTypeDetails[0]["RoundUp"]) == true)
                            {

                                if (month > 6)
                                {
                                    NumberOfYears = years + 1;
                                }
                                else if (month == 6 && days > 0)
                                {
                                    NumberOfYears = years + 1;
                                }
                                else
                                {
                                    NumberOfYears = years;
                                }
                                dvSeparationTypeDetails.RowFilter = null;
                                dvSeparationTypeDetails.RowFilter = "Yearno='" + NumberOfYears + "'";
                                if (dvSeparationTypeDetails.Count > 0)
                                {
                                    NumberOfDays = Convert.ToInt32(dvSeparationTypeDetails[0]["DayNo"]);
                                }
                                else
                                {
                                    throw new Exception("Policy  was not defined for this year.");
                                }

                            }
                            else
                            {
                                NumberOfYears = years;
                                NumberOfDays = Convert.ToInt32(dvSeparationTypeDetails[0]["DayNo"]);
                            }
                        }
                        else
                        {
                            //throw new Exception("Policy  was not defined for this year.");
                        }
                    }
                    // calculate total
                    sTotalAmount = (Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) / 30) * NumberOfDays * NumberOfYears;





                    //Calculate Gratuity
                    if (isGratuityApplicable)
                    {
                        DataView dvSalaryData = new DataView(dsSalaryData.Tables[0]);

                        dvSalaryData.RowFilter = "HeadCategory='Basic'";
                        if (dvSalaryData.Count > 0)
                        {
                            int GratuityNumberOfYears = 0;
                            if (month > 6)
                            {
                                GratuityNumberOfYears = years + 1;
                            }
                            else if (month == 6 && days > 0)
                            {
                                GratuityNumberOfYears = years + 1;
                            }
                            else
                            {
                                GratuityNumberOfYears = years;
                            }
                            if (GratuityNumberOfYears >= 5 && GratuityNumberOfYears < 10)
                            {
                                sGratuityAmount = (Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) / 2) * GratuityNumberOfYears;
                            }
                            if (GratuityNumberOfYears >= 10)
                            {
                                sGratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) * GratuityNumberOfYears;
                            }

                        }
                    }


                    if (isFixedDayAmountApplicable)// Fixed Day Amount
                    {
                        DataSet dsSeparationTypeFixedDayAmount = null;
                        GetSeparationTypeFixedDayAmountById(dsSeparationType.Tables[0].Rows[0]["Id"].ToString(), out dsSeparationTypeFixedDayAmount);
                        DataView dv = new DataView(dsSeparationTypeFixedDayAmount.Tables[0]);
                        dv.RowFilter = "EmploymentType='" + dsTenure.Tables[0].Rows[0]["EmploymentType"].ToString() + "'";

                        if (dv.Count > 0)
                        {
                            sFixedDayAmount = (Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) / 30) * Convert.ToDecimal(dv[0]["DayNo"].ToString());
                            NumberOfFixedDays = Convert.ToDecimal(dv[0]["DayNo"].ToString());
                        }

                    }




                    //Salary Info

                    //ss basd
                    DataView dvBasicData = new DataView(dsSalaryData.Tables[0]);
                    dvBasicData.RowFilter = "HeadCategory='Basic'";
                    if (dvBasicData.Count > 0)
                    {
                        sBasicAmount = Convert.ToDecimal(dvBasicData[0]["EntryAmount"].ToString());
                    }

                    DataView dvGrossData = new DataView(dsSalaryData.Tables[0]);
                    dvGrossData.RowFilter = "HeadCategory='GROSS'";
                    if (dvGrossData.Count > 0)
                    {
                        sGrossAmount = Convert.ToDecimal(dvGrossData[0]["EntryAmount"].ToString());
                    }
                    // proc data
                    GetLastMonthSalaryInfoByEmpId(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsProcSalaryData);
                    if (dsProcSalaryData.Tables[0].Rows.Count > 0)
                    {
                        DataView dvSPAprovedData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPAprovedData.RowFilter = "IsApproved=" + true;
                        if (dvSPAprovedData.Count == 0)
                        {
                            throw new Exception("This Employee's last month[" + Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] Salary was not Approved.");
                        }



                        DataView dvSPGData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPGData.RowFilter = "HeadCategory='GROSS'";
                        if (dvSPGData.Count > 0)
                        {
                            obj.LastMonthGrossAmount = Convert.ToDecimal(dvSPGData[0]["DisbusmentAmount"].ToString());
                        }


                        DataView dvSPAData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPAData.RowFilter = "HeadCategory='Absenteeism'";
                        if (dvSPAData.Count > 0)
                        {
                            obj.LastMonthAbsenteeismAmount = Convert.ToDecimal(dvSPAData[0]["DisbusmentAmount"].ToString()) * (-1);
                        }


                        DataView dvSPOTData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPOTData.RowFilter = "HeadCategory='OverTime'";
                        if (dvSPOTData.Count > 0)
                        {
                            obj.LastMonthOTAmount = Convert.ToDecimal(dvSPOTData[0]["DisbusmentAmount"].ToString());
                        }

                    }
                    else
                    {
                        throw new Exception("This Employee's last month[" + Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] Salary was not processed. ");
                    }




                }
                sSalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) / 30;
                obj.EmpSystemId = sEmpSystemId;
                obj.FormulaDes = dsSeparationType.Tables[0].Rows[0]["FormulaDes"].ToString();
                obj.SeparationTypeAmount = Convert.ToDecimal(string.Format("{0:F2}", sTotalAmount));
                obj.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sGratuityAmount));
                obj.FixedDayAmount = Convert.ToDecimal(string.Format("{0:F2}", sFixedDayAmount));
                obj.BasicAmount = Convert.ToDecimal(string.Format("{0:F2}", sBasicAmount));
                obj.GrossAmount = Convert.ToDecimal(string.Format("{0:F2}", sGrossAmount));
                obj.SalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sSalaryRate));
                obj.PolicyYearNo = NumberOfYears;
                obj.PolicyDayNo = NumberOfDays;
                obj.PolicyFixedDayNo = NumberOfFixedDays;
                obj.IsGratuityApplicable = isGratuityApplicable;
                obj.IsFixedDayApplicable = isFixedDayAmountApplicable;
                return obj;
            }


            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void NaturalLength(int length, ref int years, ref int months, ref int days)
        {
            double remain = 0;
            double amount = 0;
            remain = Convert.ToDouble(length);
            amount = remain / 365.25;
            years = (int)Math.Truncate(amount);
            remain = remain - years * 365.25;
            amount = remain / 30.4375;
            months = (int)Math.Truncate(amount);
            remain = remain - months * 30.4375;
            days = (int)Math.Truncate(remain);
        }
        // Just a test
        //public void Main()
        //{
        //    int length = 396;
        //    int y = 0;
        //    int m = 0;
        //    int d = 0;
        //    NaturalLength(length, ref y, ref m, ref d);
        //    Console.WriteLine("Years: {0}, months: {0}, days = {0}", y, m, d);
        //}
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryHead";

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

        public void GetSeparationTypeByEmpId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [HKP].[SeparationType] WHERE Id=(SELECT TOP 1 SeparationTypeId
                           FROM [TRN].[Resignation] WHERE EmployeeId='" + EmployeeId + @"' ORDER BY UpdatedDate DESC)";

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
        public void GetSeparationTypeDetailsById(string SeparationTypeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SeparationTypeDetails WHERE SeparationTypeId='" + SeparationTypeId + @"'";

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
        public void GetSeparationTypeFixedDayAmountById(string SeparationTypeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SeparationTypeFixedDayAmount WHERE SeparationTypeId='" + SeparationTypeId + @"'";

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

        public void GetTenureByEmpId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EI.SystemId,EI.DOS,EI.DOJ 
                                ,DateDiff(day,EI.doj, EI.dos)+1 TenureInDays
                                --,DateDiff(MONTH,EI.doj, EI.dos) TenureInMonths
                                --,DateDiff(YEAR,EI.doj, EI.dos) TenureInYears
                                ,EI.EmploymentType
                                ,ISNULL(SPAD.OTRate,0) OTRate,SPAD.MonthNo,SPAD.YearNo
                                ,ISNULL(SPAD.TotalOTHr,0) TotalOTHr
                                ,ISNULL(SPAD.TotalProcDate,0) TotalProcDate
                                 ,ISNULL(SPAD.TotalPresent,0) TotalPresent 
                                 ,ISNULL(SPAD.TotalAbsent,0) TotalAbsent
                                FROM EmployeeInformation AS EI
                                LEFT JOIN  SalaryProceAttdnData AS SPAD ON SPAD.EmpSystemID = EI.SystemId AND SPAD.MonthNo=MONTH(EI.DOS) AND SPAD.YearNo=YEAR(EI.DOS)
                                WHERE EI.SystemId='" + EmployeeId + @"'";

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
        public void GetLastMonthSalaryInfoByEmpId(string EmployeeId, string dos, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"     SELECT * FROM SalaryProcChild as sps
                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = sps.SalaryHeadID
                                WHERE sps.SlrProcMstSystemID IN (Select SystemID  from SalaryProcMaster WHERE MonthNo=MONTH('" + dos + @"') AND YearNo=YEAR('" + dos + @"'))
                                AND sps.EmpInfoSystemID='" + EmployeeId + @"'";


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





        public void GetDailyAllowanceSummaryData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"DECLARE @dtDate DATETIME
                                SET @dtDate = '" + sWorkDate + @"'
                                SELECT --DAT.WorkDate
                                 DAT.EmpSystemId
                                ,DAT.AllowanceDailyId
                                ,sum( DAT.Quantity) TotalQuantity
                                ,da.UserName
                                ,da.SalaryHeadId
                                ,DAR.Rate 
                                ,sum( DAT.Quantity)* DAR.Rate  Totalvalue,MONTH( @dtDate) MonthNo,YEAR( @dtDate) YearNo
                                FROM DailyAllowanceTransaction AS DAT
                                LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = DAT.EmpSystemId
                                LEFT JOIN hkp.AllowanceDaily AS DA ON DA.Id=dat.AllowanceDailyId
                                LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=da.Id AND dar.EmployeeCategoryId=ei.EmployeeCategorySystemID
                                WHERE DAT.WorkDate 
                                BETWEEN Replace(CONVERT(VARCHAR(25),DATEADD(dd,-(DAY(@dtDate)-1),@dtDate),106), ' ', '-')---from date
                                AND FORMAT(DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,@dtDate)+1,0)),'dd-MMM-yyyy') ---to date
                                AND DAT.PlantID='" + sPlantID + @"'
                                GROUP BY 
                                 DAT.EmpSystemId
                                ,DAT.AllowanceDailyId
                                ,da.SalaryHeadId
                                ,da.UserName
                                ---,DAT.Quantity 
                                ,DAR.Rate 
                                ---,DAT.WorkDate";

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
        public void UpdateDailyAllowanceSummaryData(CustomIdentity identity, string sWorkDate)
        {
            clsSalaryInfo objSal = new clsSalaryInfo();
            DataSet dsCurrency = null;
            DataSet dsCurrencyRule = null;
            DataSet dsDailyAllowanceSummary = null;
            DataSet dsMonthWiseExtraSalaryAmtMaster = null;
            DataSet dsMonthWiseExtraSalaryAmtChild = null;

            string _currencyId = string.Empty;


            try
            {
                objSal.GetLocalCurrency(identity.CompanyGroupId, identity.PlantId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    //lblLocalCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }
                GetCurrencyRuleId(identity.PlantId, out dsCurrencyRule);


                GetDailyAllowanceSummaryData(identity.PlantId, sWorkDate, out dsDailyAllowanceSummary);
                GetMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sWorkDate, out dsMonthWiseExtraSalaryAmtMaster);
                GetMonthWiseExtraSalaryAmtChildData(identity.PlantId, sWorkDate, out dsMonthWiseExtraSalaryAmtChild);

                if (dsDailyAllowanceSummary.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsDailyAllowanceSummary.Tables[0].Rows.Count; i++)
                    {
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString();
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString() + "' and YearNo='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND EmpInfoSystemID='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString() + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            dr["SystemID"] = "DAM" + sID;
                            MasterId = "DAM" + sID;
                            dr["EmpInfoSystemID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString();

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();

                            //dr["UpdatedBy"] = identity.Name;
                            //dr["DateUpdated"] = System.DateTime.Now.ToString();

                            dsMonthWiseExtraSalaryAmtMaster.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvMonthWiseExtraSalaryAmtMaster[0].Row;

                            MasterId = dr["SystemID"].ToString();
                            dr.BeginEdit();
                            dr["EmpInfoSystemID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString();


                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = null;





                        DataView dvMonthWiseExtraSalaryAmtChild = new DataView(dsMonthWiseExtraSalaryAmtChild.Tables[0]);
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = "mwesamastersystemid='" + MasterId + "' AND SalaryHeadID='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString() + "'";
                        if (dvMonthWiseExtraSalaryAmtChild.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            dr["SystemID"] = "DAC" + sID;
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();

                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            }
                            else
                            {
                                dr["EntryAmount"] = 0;
                                dr["DefineAmount"] = 0;
                            }


                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";

                            dr["EntryCurrencyID"] = _currencyId;
                            dr["DefineCurrencyID"] = _currencyId;
                            dr["AmtDefinitionCurrencyID"] = _currencyId;

                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString());
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();

                            //dr["UpdatedBy"] = identity.Name;
                            //dr["DateUpdated"] = System.DateTime.Now.ToString();

                            dsMonthWiseExtraSalaryAmtChild.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvMonthWiseExtraSalaryAmtChild[0].Row;

                            dr.BeginEdit();
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();
                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            }
                            else
                            {
                                dr["EntryAmount"] = 0;
                                dr["DefineAmount"] = 0;
                            }
                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString());
                            dr["EntryCurrencyID"] = _currencyId;
                            dr["DefineCurrencyID"] = _currencyId;
                            dr["AmtDefinitionCurrencyID"] = _currencyId;


                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = null;


                    }

                }





                clsStaticInfo objt = new clsStaticInfo();
                objt.SaveDataSets(dsMonthWiseExtraSalaryAmtMaster, dsMonthWiseExtraSalaryAmtChild);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function
        public void GetMonthWiseExtraSalaryAmtMasterData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select * from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"'";


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
        public void GetMonthWiseExtraSalaryAmtChildData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select * from dbo.MonthWiseExtraSalaryAmtChild where mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"')";
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

        public void GetCurrencyRuleId(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT MstSystemID,SalaryHeadID  FROM  [dbo].[CurrencyRuleChild] 			                      
			                      WHERE MstSystemID IN (SELECT SystemId FROM [dbo].[CurrencyRuleMaster] WHERE PlantID='" + sPlantID + @"')";

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

        private string GetCurrencyRuleIdBySalaryHead(DataSet ds, string salaryHeadid)
        {
            string CurrencyRuleId = string.Empty;
            DataView dv = new DataView(ds.Tables[0]);
            dv.RowFilter = "SalaryHeadID='" + salaryHeadid + "'";
            if (dv.Count > 0)
            {
                CurrencyRuleId = dv[0]["MstSystemID"].ToString();

            }
            return CurrencyRuleId;
        }




    }

    public class LeaveEncashmentViewModel
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public decimal BroughtForward { get; set; } = 0;
        public decimal DaysCanBeSanctioned { get; set; } = 0;
        public decimal CurrentYearAllocation { get; set; } = 0;
        public string IsYearlyProcessed { get; set; }
        public decimal LeaveDaysAllowed { get; set; } = 0;
        public decimal AvailedLeave { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public decimal EncashedInbetween { get; set; } = 0;
        public decimal YearEndEncash { get; set; } = 0;
        public decimal YearEndLapse { get; set; } = 0;
        public decimal CarryForward { get; set; } = 0;
        public string Id { get; set; }
        public string LeaveTypeId { get; set; }
        public string PlantId { get; set; }
        public string YearlyCalendarId { get; set; }
        public string EncashmentDate { get; set; }
        public string LeaveEncashmentType { get; set; }
        public string EmpSystemId { get; set; }
        public decimal Days { get; set; } = 0;
        //public decimal Rest { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal AvailedEncashment { get; set; } = 0;
        public bool? Isdisburse { get; set; } = false;


    }


    public class MultipleLeaveEncashmentViewModel
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public decimal BroughtForward { get; set; } = 0;
        public decimal NewBroughtForward { get; set; } = 0;
        public decimal DaysCanBeSanctioned { get; set; } = 0;
        public decimal CurrentYearAllocation { get; set; } = 0;
        public string IsYearlyProcessed { get; set; }
        public decimal LeaveDaysAllowed { get; set; } = 0;
        public decimal AvailedLeave { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public decimal EncashedInbetween { get; set; } = 0;
        public decimal YearEndEncash { get; set; } = 0;
        public decimal YearEndLapse { get; set; } = 0;
        public decimal NewYearEndEncash { get; set; } = 0;
        public decimal NewYearEndLapse { get; set; } = 0;
        public decimal CarryForward { get; set; } = 0;
        public decimal CarryForwardOpeningBalance { get; set; } = 0;
        public string LeaveTypeId { get; set; }
        public string Id { get; set; }
        public string PlantId { get; set; }
        public string YearlyCalendarId { get; set; }
        public string EncashmentDate { get; set; }
        public string LeaveEncashmentType { get; set; }
        public string EmpSystemId { get; set; }
        public decimal Days { get; set; } = 0;
        //public decimal Rest { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal AvailedEncashment { get; set; } = 0;
        public bool? Isdisburse { get; set; } = false;
        public bool CheckBoxSelect { get; set; } = false;
        //public string SystemId { get; set; }

        public string EmpCategoryName { get; set; }
        public string Designation { get; set; }
        public string Unit { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string Line { get; set; }
        public string DOJ { get; set; }
        public string EncashmentSpecificDate { get; set; }
        public string PolicyName { get; set; }

        public string PaymentMode { get; set; }
        public string BasicAmmount { get; set; }
        public string GrossAmmount { get; set; }
        //public string GivenDesignationId { get; set; }
        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
        public string BankAccNo { get; set; }
        //public string SalaryPercentage { get; set; }

        //public string BudgetCode { get; set; }
        //public string EmployeeCategoryId { get; set; }

        public string LegalDesignationId { get; set; }
        public string FixedEncashmentDate { get; set; }
        public string MaturityDate { get; set; }

    }


    public class MultipleLeaveEncashmentViewModelNew
    {

        public string LeaveType { get; set; }

        public decimal NewBroughtForward { get; set; } = 0;
        public decimal NewYearEndEncash { get; set; } = 0;
        public decimal NewYearEndLapse { get; set; } = 0;

        public string LeaveTypeId { get; set; }
        public string YearlyCalendarId { get; set; }
        public string EmpSystemId { get; set; }
        public decimal Days { get; set; } = 0;
        public decimal Rate { get; set; } = 0;

        public bool CheckBoxSelect { get; set; } = false;
        public string EmployeeCode { get; set; }



        public string PaymentMode { get; set; }
        public string BasicAmmount { get; set; }
        public string GrossAmmount { get; set; }

        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
        public string BankAccNo { get; set; }


        public string LegalDesignationId { get; set; }
        public string EncashmentDate { get; set; }



        public decimal DaysCanBeSanctioned { get; set; } = 0;
        public decimal AvailedLeave { get; set; } = 0;
        //public decimal Balance { get; set; } = 0;
        public decimal CarryForward { get; set; } = 0;
        public decimal BroughtForward { get; set; } = 0;

    }

}