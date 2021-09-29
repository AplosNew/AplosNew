using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;


namespace Library.HumanResource.NewAttendanceProcess
{
    public class NewJobCardReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewJobCardReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public void GetEmpJobCardInfoWithInDateTimes(string EmpIdLoop, string FromDate, string ToDate, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT A.EmployeeCode,A.EmployeeCodeNumeric
                            	,A.EmployeeName,A.OutTime punchTime,A.firstSlab
                                ,A.EmployeeStatus
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,REPLACE(CONVERT(VARCHAR(11), A.PDate, 113), ' ', '-') PDate
                                ,PDay
                            	,DayStatus
                                
                                ,A.IsHalfDayLeave
                            	,A.InTime
                                ,ShiftInTimeShow
								 ,ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,OverStay
                                ,A.TotalOTHr FinalOT
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,A.OTConsiderOn
                                ,ShiftTime = CASE WHEN ShiftChangeInTime IS NULL THEN ShiftInTime ELSE ShiftChangeInTime END
                                ,ShiftInTimeC = CASE WHEN ShiftChangeInTime IS NULL THEN ShiftInTimecc ELSE ShiftChangeInTime END
                                ,ShiftName
								,ShiftType
							    ,ShiftOutTime
                                ,A.IsManualDayStatus,A.IsManualInTime,A.IsManualOutTime, A.ShortLeave,A.IsOTEntitled,A.IsOTComfirm,A.pdate  WorkDate,
                                ReConfirm = CASE  WHEN A.IsOTComfirm=0 AND A.WorkDate IS NOT NULL  THEN 1   ELSE 0  END,A.DayCategory
                                ,A.InTimelate,A.OutTimelate
                                ,A.ShiftInTimeLate
                                ,A.GradeCode
	                            ,A.LeaveDuration                               
								,A.DurationInMin

	                                ,A.EO 
									,A.LIN
									,A.LO
                                    ,A.Line
                            FROM(
                                SELECT E.EmployeeCode,e.EmployeeCodeNumeric,g.firstSlab
                                    , E.EmployeeName
                                    ,E.EmployeeStatus
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS
                                    , D.UserName GivenDesignation
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    ,ar.IsHalfDayLeave
                                    , SB.UserName SubSection
                                    ,datename(dw,AR.WorkDate) as PDay
                                    , AR.WorkDate PDate
                                    --, AR.DayStatus
                                    , LSalGr.Code GradeCode
                                    , HR.OTConsiderOn
                                    , AR.InTime InTime
                                    --, AR.InTime InTimeShow
                                   	,l.UserName as Line
                                    ,OverStay = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then 0.00
									when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then 0.00 else AR.OTHr end
                                    ,DayStatus = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then 'H' 
									when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then 'W'
									else AR.DayStatus end
									,InTimeShow = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then null
									 when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then null else FORMAT( AR.InTime,'HH:mm') end
									,OutTimeShow = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then Null
								 when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then Null	else FORMAT( AR.OutTime,'HH:mm') end
                            ,ShiftInTimeLate=CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),108)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 108)
						     END
                                    , CONVERT(VARCHAR(5), AR.InTime, 108) InTimelate
                             ,ShiftInTimeShow = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END
                                    , ARIN.DeviceID InDeviceID
                                    , AR.OutTime OutTime
                                    --, AR.OutTime OutTimeShow
                                    , CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate
                                    , AROUT.DeviceID OutDeviceID
                                    , AR.IsManualInTime IsManual
                                    --, AR.OTHr 
                                    ,OT.TotalOTHr
                                    , LT.UserName LvShortName
                                    , LT.Description LvDescrip
                                    , LT.LeaveType
                                    , LT.Code
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , AR.InTime dti, AR.OutTime dto
                                    , CONVERT(VARCHAR(5), cs.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
									,sd.ShiftType
                                    ,LEAVE.LeaveDuration	                            
									,HODD.DurationInMin

		                            ,EO.OffDuration AS EO
									,EIN.OffDuration AS LIN
									,LO= Case when LO.InfoType='LUNCHOUT' THEN 'YES' ELSE 'NO' END

						   ,ShiftOutTime = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                     ,ShiftInTime = Format(AR.InTime, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END

                                    ,ShiftInTimecc = Format(AR.WorkDate, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END

                                    , AR.IsManualDayStatus, AR.IsManualInTime, AR.IsManualOutTime, ar.CountedShortLeave ShortLeave,AR.IsOTEntitled,AR.IsOTComfirm,OT.WorkDate,dt.Category DayCategory
                                FROM dbo.EmployeeInformation E

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
	                           LEFT JOIN (select LET.SystemID,LTD.LeaveDuration,LTD.WorkDate,LET.EmpSystemID from  LeaveTransaction LET 
										    left join LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID=LET.SystemID	
                                        where ltd.WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
								         ) LEAVE ON LEAVE.EmpSystemID=E.SystemId and LEAVE.WorkDate= AR.WorkDate

                                left join (select EmpSystemID,WorkDate,SUM(DurationInMin)AS DurationInMin
		                    From  [dbo].[HourlyOffDuty] 
	                        WHERE  ApproveType='Deducation' AND WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
		                    Group BY  EmpSystemID,WorkDate)as HODD on HODD.EmpSystemID=E.SystemId and HODD.WorkDate=AR.WorkDate

                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + FromDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
                                ON AR.ShiftSystemID = SFCG.ShiftDefinationID
                                LEFT JOIN dbo.AttdnRawData ARIN ON AR.InTimeRowID = ARIN.RowID
                                LEFT JOIN dbo.AttdnRawData AROUT ON AR.OutTimeRowID = AROUT.RowID
                                LEFT JOIN dbo.LeaveType LT ON AR.LTSystemID = LT.Id
                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id

                                  LEFT JOIN ORG.Section S ON PO.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PO.SubSectionID = SB.Id
								left join org.Line l on l.Id=mpb.LineId

                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and LSGD.PlantId='" + plantId + @"' and LSGD.LegalSalaryGradeId is not null
                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId
                                left join(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = AR.ShiftSystemID and cs.ShiftDate = ar.WorkDate
                                left join[ShiftDefination] sd on sd.SystemID = AR.ShiftSystemID
                                LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                                LEFT JOIN FinalOT OT ON E.SystemId = OT.EmpSystemID and ot.WorkDate=ar.WorkDate
                                LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=E.PlantId
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus
                            
							inner join OTSlabDefineGeneral g on 
							'" + ToDate + @"' between g.FromDate and g.ToDate 
							and g.PlantID=ar.PlantID 
							and g.DayType=dt.OriginalDayType

                                left join AttendanceInfoExtra LO on LO.EmpSystemId=e.SystemId and LO.WorkDate=ar.WorkDate and LO.InfoType='LUNCHOUT'
								left join AttendanceInfoExtra EO on EO.EmpSystemId=e.SystemId and EO.WorkDate=ar.WorkDate and EO.InfoType='EARLYOUT'
								left join AttendanceInfoExtra EIN on EIN.EmpSystemId=e.SystemId and EIN.WorkDate=ar.WorkDate and EIN.InfoType='EARLYIN'

                                WHERE E.SystemID in (" + EmpIdLoop + @")
                                    AND AR.WorkDate BETWEEN '" + FromDate + @"'
                                        AND '" + ToDate + @"' AND (EmployeeStatus = 'Active' OR COnvert(date,DOS) >= Convert(Date,'" + FromDate + @"'))
                                ) A
                            
                            ORDER BY A.EmployeeCode
                            	,A.PDate
                                ";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
                objCon.CommitTransaction();
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

