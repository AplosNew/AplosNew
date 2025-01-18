using ConnectionManager;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.PayrollManagementDashboard
{
    public class PayrollManagementDashboard
    {
        SqlRepository _sqlRepository = new SqlRepository();
        public enum ReportParticulars
        {
            AbsentNoPunchTime, AbsentWithsinglePunch, LeaveWithPunch, ShortDurationAbsent, ShortDuration, OTApplicableAndOutMissing,
            OTNotApplicableAndOutMissing, OTNotConfirm, AttendanceNotLock, OffdayMissingPunch, OffdayWithPunch, AbsentWithWrongShift, NoOfAbsent,
            UnApprovedProfile, NoSalaryStructure, SalaryStructureNotApprove, LongAbsenteeism, TBS, BankStatus, SalaryNotApproved, ShiftNotAssign
        }
        public Dictionary<ReportParticulars, string> dicKeyMapping = new Dictionary<ReportParticulars, string>();

        public PayrollManagementDashboard()
        {
            _sqlRepository = new SqlRepository();

            dicKeyMapping = new Dictionary<ReportParticulars, string>();
            dicKeyMapping.Add(ReportParticulars.AbsentNoPunchTime, "Absent No Punch Time");
            dicKeyMapping.Add(ReportParticulars.AbsentWithsinglePunch, "Absent With single Punch");
            dicKeyMapping.Add(ReportParticulars.LeaveWithPunch, "Leave With Punch");
            dicKeyMapping.Add(ReportParticulars.ShortDurationAbsent, "Short Duration Absent");
            dicKeyMapping.Add(ReportParticulars.ShortDuration, "Short Duration");
            dicKeyMapping.Add(ReportParticulars.OTApplicableAndOutMissing, "OT Applicable And Out Missing");

            dicKeyMapping.Add(ReportParticulars.OTNotApplicableAndOutMissing, "OT Not Applicable And Out Missing");
            dicKeyMapping.Add(ReportParticulars.OTNotConfirm, "OT Not Confirm");
            dicKeyMapping.Add(ReportParticulars.AttendanceNotLock, "Attendance Not Lock");
            dicKeyMapping.Add(ReportParticulars.OffdayMissingPunch, "Off Day Missing Punch");
            dicKeyMapping.Add(ReportParticulars.OffdayWithPunch, "Off Day With Punch");
            dicKeyMapping.Add(ReportParticulars.AbsentWithWrongShift, "Absent With Wrong Shift");
            dicKeyMapping.Add(ReportParticulars.NoOfAbsent, "No Of Absent");

            dicKeyMapping.Add(ReportParticulars.UnApprovedProfile, "Un Approved Profile");
            dicKeyMapping.Add(ReportParticulars.NoSalaryStructure, "No Salary Structure");
            dicKeyMapping.Add(ReportParticulars.SalaryStructureNotApprove, "Salary Structure Not Approve");
            dicKeyMapping.Add(ReportParticulars.LongAbsenteeism, "Long Absenteeism");
            dicKeyMapping.Add(ReportParticulars.TBS, "TBS");
            dicKeyMapping.Add(ReportParticulars.BankStatus, "Bank Status");
            dicKeyMapping.Add(ReportParticulars.SalaryNotApproved, "Salary Not Approved");
            dicKeyMapping.Add(ReportParticulars.ShiftNotAssign, "Shift Not Assigned");

        }
        private string getCaption(string Particulars)
        {
            foreach (var item in dicKeyMapping)
            {
                if (item.Key.ToString().ToUpper() == Particulars.ToUpper())
                    return item.Value;
            }

            return "";
        }

        public void MakeSummary(string FromDate, string ToDate, string companyId, string companyGroupId, out DataTable dtFinalTable)
        {
            dtFinalTable = null;
            try
            {


                GetAttendanceAndOTUpToDateSummary(FromDate, ToDate, companyId, ref dtFinalTable, out DataTable dtPlant);
                GetAttendanceAndOTyesterdaySummary(FromDate, ToDate, companyId, ref dtFinalTable);


                GetUNApprovedProfileSummary(companyId, out DataSet dsLocal);
                MakeOtherSummary("UnApprovedProfile", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);

                GetProfileNoSalarySummary(FromDate, companyId, companyGroupId, ToDate, out dsLocal);
                MakeOtherSummary("NoSalaryStructure", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);

                GetNoSalaryStructureApproveSummary(FromDate, companyId, companyGroupId, out dsLocal);
                MakeOtherSummary("SalaryStructureNotApprove", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);

                GetLongAbsentisomSummary(companyId, out dsLocal);
                MakeOtherSummary("LongAbsenteeism", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);

                GetTBSSummary(companyId, out dsLocal);
                MakeOtherSummary("TBS", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);


                GetBankRemarkSummary(FromDate, companyId, companyGroupId, ToDate, out dsLocal);
                MakeOtherSummary("BankStatus", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);


                GetSalaryNotApprovedSummary(FromDate, companyId, out dsLocal);
                MakeOtherSummary("SalaryNotApproved", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);

                GetShiftNotAssignSummary(FromDate, companyId, ToDate, out dsLocal);
                MakeOtherSummary("ShiftNotAssign", dsLocal.Tables[0], companyId, dtFinalTable, dtPlant);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetAttendanceDetail(string FromDate, string ToDate, string PlantId, string ParticularsKey, out List<Dictionary<string, object>> data)
        {

            try
            {
                string sql = GetAttendanceAndOTSQLDetail(FromDate, ToDate, PlantId, ParticularsKey);
                SqlRepository repo = new SqlRepository();
                data = repo.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void MakeOtherSummary(string CaptionName, DataTable dtCurrentData, string companyId, DataTable dtFinal, DataTable dtPlant)
        {
            try
            {
                for (int PL = 0; PL < dtPlant.Rows.Count; PL++)
                {
                    dtCurrentData.DefaultView.RowFilter = "PlantId='" + dtPlant.Rows[PL]["Id"].ToString() + "'";

                    if (dtCurrentData.DefaultView.Count == 0)
                        continue;

                    DataRow dr = dtFinal.NewRow();
                    dr["Particulars"] = getCaption(CaptionName);
                    dr["ParticularsKey"] = CaptionName;
                    dr["PlantId"] = dtPlant.Rows[PL]["Id"].ToString();
                    dr["PlantName"] = dtPlant.Rows[PL]["UserName"].ToString();

                    dr["UpToDate"] = clsStaticInfo.dbl(dtCurrentData.DefaultView[0]["CNT"].ToString());

                    dtFinal.Rows.Add(dr);


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetAttendanceAndOTSQL(string FromDate, string ToDate, string companyId)
        {
            return @"SELECT 

                        PlantId,SUM(AbsentNoPunchTime)AS AbsentNoPunchTime,	SUM(AbsentWithsinglePunch)AS  AbsentWithsinglePunch,SUM(LeaveWithPunch)AS	LeaveWithPunch,
                        SUM(ShortDurationAbsent)AS ShortDurationAbsent,	SUM(ShortDuration)AS ShortDuration,	SUM(OTApplicableAndOutMissing)AS OTApplicableAndOutMissing,
                        SUM(OTNotApplicableAndOutMissing)AS	OTNotApplicableAndOutMissing,
                        SUM(OTNotConfirm)AS OTNotConfirm,SUM(AttendanceNotLock)AS AttendanceNotLock,SUM(OffdayMissingPunch)AS	OffdayMissingPunch,
                        SUM(OffdayWithPunch)AS OffdayWithPunch,	SUM(AbsentWithWrongShift)AS AbsentWithWrongShift,SUM(NoOfAbsent)AS NoOfAbsent


                        FROM (SELECT  
                        ei.PlantId,

                        AbsentNoPunchTime=CASE WHEN isnull(ei.EmployeeCurrentStatus,'') not in('TBS','LONG ABSENTEEISM') AND ISNULL(apd.PunchInTime,'')='' AND ISNULL(apd.PunchOutTime,'')='' THEN 1 ELSE 0 END 
                        ,AbsentWithsinglePunch=CASE WHEN  (ISNULL(apd.PunchInTime,'')<>'' AND ISNULL(apd.PunchOutTime,'')='') OR (ISNULL(apd.PunchOutTime,'')<>'' AND ISNULL(apd.PunchInTime,'')='') THEN 1 ELSE 0 END
                        ,LeaveWithPunch=CASE WHEN APD.DayStatus in (select daytype from daytype where category='Leave')  AND (ISNULL(apd.PunchInTime,'')<>'' OR ISNULL(apd.PunchOutTime,'')<>'') THEN 1 ELSE 0 END
                        ,ShortDurationAbsent=CASE WHEN apd.DayStatus='A' AND (ISNULL(apd.PunchInTime,'')<>'' AND ISNULL(apd.PunchOutTime,'')<>'') THEN 1 ELSE 0 END
                        ,ShortDuration= CASE WHEN APD.IsHalfDayLeave <> 1 AND datediff(minute,apd.InTime ,apd.OutTime )<datediff(minute,sft.ShiftInTime ,CASE WHEN sft.ShiftInTime>sft.ShiftOutTime THEN DATEADD(DAY,1,sft.ShiftOutTime) ELSE sft.ShiftOutTime END ) THEN 1 ELSE 0 END
                        ,OTApplicableAndOutMissing=CASE WHEN APD.IsOTEntitled = 1 AND APD.DayStatus in (select daytype from daytype where category IN ('Present','Late'))  AND (ISNULL(apd.InTime,'')<>'' AND ISNULL(apd.OutTime,'')='') THEN 1 ELSE 0 END
                        ,OTNotApplicableAndOutMissing=CASE WHEN ISNULL(APD.IsOTEntitled,0) = 0 AND APD.DayStatus in (select daytype from daytype where category IN ('Present','Late'))  AND (ISNULL(apd.InTime,'')<>'' AND ISNULL(apd.OutTime,'')='') THEN 1 ELSE 0 END
                        ,OTNotConfirm=CASE WHEN APD.IsOTEntitled = 1 and APD.OTHr >0 AND  APD.IsOTComfirm = 0 and ISNULL (oa.OThour,0)=0  AND APD.DayStatus in (select daytype from daytype where category IN ('Present','Late'))  THEN 1 ELSE 0 END
                        ,AttendanceNotLock=CASE WHEN ISNULL(plk.IsActive,0)=0 OR( ISNULL(ulk.Id,'')<>'' AND ISNULL(ulk.IsActive,0)=0) THEN 1 ELSE 0 END                 
                        ,OffdayMissingPunch=CASE WHEN APD.DayStatus in (select daytype from daytype where OriginalDayType IN ('W','H'))  and (( (APD.InTime IS NULL and APD.PunchInTime Is Null)	AND (APD.OutTime IS not NULL or APD.PunchOutTime Is NOT NULL)) or ( (APD.InTime IS Not NULL or APD.PunchInTime Is Not Null)	AND (APD.OutTime IS NULL and APD.PunchOutTime Is NULL)))THEN 1 ELSE 0 END 
                        ,OffdayWithPunch=CASE WHEN APD.DayStatus in (select daytype from daytype where OriginalDayType IN ('W','H')) AND ( APD.InTime IS Not NULL or APD.PunchInTime Is Not Null)THEN 1 ELSE 0 END 
                        ,AbsentWithWrongShift=CASE WHEN APD.DayStatus ='A' and ISNULL(rd.LogDownLoadNum,'')<>'' THEN 1 ELSE 0 END
                        ,NoOfAbsent=CASE WHEN APD.DayStatus ='A' THEN 1 ELSE 0 END

                        FROM AttdnProcessData AS apd
                         LEFT JOIN EmployeeInformation EI ON apd.EmpSystemID = EI.SystemId
                         left join OTfromApp oa on oa.EmpSystemId = APD.EmpSystemID and oa.WorkDate=APD.WorkDate
 
                         LEFT JOIN PlantWiseAttendanceLock PLK ON  plk.PlantId=apd.PlantID AND plk.LockedDate=apd.WorkDate
                         LEFT JOIN ExceptionEmployeeAttendanceUnlock ULK ON ULK.EmpSystemId=ei.SystemId AND ulk.WorkDate=apd.WorkDate
 
                        left join SCS.OpeningBalanceCutOffDate c on c.PlantId=eI.PlantId and c.ModuleName='HR'
                        left join (select MIN(EffectiveDate)EffectiveDate,EmpSystemID from EmployeeShiftAssign where IsSingleDayShift=0 group by EmpSystemID) es on es.EmpSystemID = eI.SystemId
 
                         LEFT join (select LogDownLoadNum,PDate,min(PTime)ptime from AttdnRawData where isnull(ptype,'')='' group by LogDownLoadNum,PDate) rd on rd.LogDownLoadNum = APD.EmpSystemID and rd.PDate = APD.WorkDate
                    
                         LEFT JOIN (SELECT o.EmpSystemID ,o.WorkDate,
								                            DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                                                    DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime
		                         
		                                                    FROM  AttdnProcessData O
		                                                    LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                                                    LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID    ) SFT 
		                                                    ON sft.EmpSystemID=ei.SystemId AND sft.WorkDate=apd.WorkDate
                        WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
                        AND ei.CompanyId='" + companyId + @"'
                        ) AS K GROUP BY PlantId";


        }
        private string GetAttendanceAndOTSQLDetail(string FromDate, string ToDate, string PlantId, string ParticularsKey)
        {
            return @"SELECT 
                       K.*
                        FROM (SELECT  
                        ei.PlantId,apd.EmpSystemID, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection,apd.InTime, apd.OutTime, apd.DayStatus, apd.OTHr,
                            apd.IsOTComfirm, apd.PunchInTime, apd.PunchOutTime,

                        AbsentNoPunchTime=CASE WHEN isnull(ei.EmployeeCurrentStatus,'') not in('TBS','LONG ABSENTEEISM') AND ISNULL(apd.PunchInTime,'')='' AND ISNULL(apd.PunchOutTime,'')='' THEN 1 ELSE 0 END 
                        ,AbsentWithsinglePunch=CASE WHEN  (ISNULL(apd.PunchInTime,'')<>'' AND ISNULL(apd.PunchOutTime,'')='') OR (ISNULL(apd.PunchOutTime,'')<>'' AND ISNULL(apd.PunchInTime,'')='') THEN 1 ELSE 0 END
                        ,LeaveWithPunch=CASE WHEN APD.DayStatus in (select daytype from daytype where category='Leave')  AND (ISNULL(apd.PunchInTime,'')<>'' OR ISNULL(apd.PunchOutTime,'')<>'') THEN 1 ELSE 0 END
                        ,ShortDurationAbsent=CASE WHEN apd.DayStatus='A' AND (ISNULL(apd.PunchInTime,'')<>'' AND ISNULL(apd.PunchOutTime,'')<>'') THEN 1 ELSE 0 END
                        ,ShortDuration= CASE WHEN APD.IsHalfDayLeave <> 1 AND datediff(minute,apd.InTime ,apd.OutTime )<datediff(minute,sft.ShiftInTime ,CASE WHEN sft.ShiftInTime>sft.ShiftOutTime THEN DATEADD(DAY,1,sft.ShiftOutTime) ELSE sft.ShiftOutTime END ) THEN 1 ELSE 0 END
                        ,OTApplicableAndOutMissing=CASE WHEN APD.IsOTEntitled = 1 AND APD.DayStatus in (select daytype from daytype where category IN ('Present','Late'))  AND (ISNULL(apd.InTime,'')<>'' AND ISNULL(apd.OutTime,'')='') THEN 1 ELSE 0 END
                        ,OTNotApplicableAndOutMissing=CASE WHEN ISNULL(APD.IsOTEntitled,0) = 0 AND APD.DayStatus in (select daytype from daytype where category IN ('Present','Late'))  AND (ISNULL(apd.InTime,'')<>'' AND ISNULL(apd.OutTime,'')='') THEN 1 ELSE 0 END
                        ,OTNotConfirm=CASE WHEN APD.IsOTEntitled = 1 and APD.OTHr >0 AND  APD.IsOTComfirm = 0 and ISNULL (oa.OThour,0)=0  AND APD.DayStatus in (select daytype from daytype where category IN ('Present','Late'))  THEN 1 ELSE 0 END
                        ,AttendanceNotLock=CASE WHEN ISNULL(plk.IsActive,0)=0 OR( ISNULL(ulk.Id,'')<>'' AND ISNULL(ulk.IsActive,0)=0) THEN 1 ELSE 0 END                 
                        ,OffdayMissingPunch=CASE WHEN APD.DayStatus in (select daytype from daytype where OriginalDayType IN ('W','H'))  and (( (APD.InTime IS NULL and APD.PunchInTime Is Null)	AND (APD.OutTime IS not NULL or APD.PunchOutTime Is NOT NULL)) or ( (APD.InTime IS Not NULL or APD.PunchInTime Is Not Null)	AND (APD.OutTime IS NULL and APD.PunchOutTime Is NULL)))THEN 1 ELSE 0 END 
                        ,OffdayWithPunch=CASE WHEN APD.DayStatus in (select daytype from daytype where OriginalDayType IN ('W','H')) AND ( APD.InTime IS Not NULL or APD.PunchInTime Is Not Null)THEN 1 ELSE 0 END 
                        ,AbsentWithWrongShift=CASE WHEN APD.DayStatus ='A' and ISNULL(rd.LogDownLoadNum,'')<>'' THEN 1 ELSE 0 END
                        ,NoOfAbsent=CASE WHEN APD.DayStatus ='A' THEN 1 ELSE 0 END

                        FROM AttdnProcessData AS apd
                         LEFT JOIN EmployeeInformation EI ON apd.EmpSystemID = EI.SystemId

                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id


                         left join OTfromApp oa on oa.EmpSystemId = APD.EmpSystemID and oa.WorkDate=APD.WorkDate
 
                         LEFT JOIN PlantWiseAttendanceLock PLK ON  plk.PlantId=apd.PlantID AND plk.LockedDate=apd.WorkDate
                         LEFT JOIN ExceptionEmployeeAttendanceUnlock ULK ON ULK.EmpSystemId=ei.SystemId AND ulk.WorkDate=apd.WorkDate
 
                        left join SCS.OpeningBalanceCutOffDate c on c.PlantId=eI.PlantId and c.ModuleName='HR'
                        left join (select MIN(EffectiveDate)EffectiveDate,EmpSystemID from EmployeeShiftAssign where IsSingleDayShift=0 group by EmpSystemID) es on es.EmpSystemID = eI.SystemId
 
                         LEFT join (select LogDownLoadNum,PDate,min(PTime)ptime from AttdnRawData where isnull(ptype,'')='' group by LogDownLoadNum,PDate) rd on rd.LogDownLoadNum = APD.EmpSystemID and rd.PDate = APD.WorkDate
                    
                         LEFT JOIN (SELECT o.EmpSystemID ,o.WorkDate,
								                            DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                                                    DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime
		                         
		                                                    FROM  AttdnProcessData O
		                                                    LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                                                    LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID    ) SFT 
		                                                    ON sft.EmpSystemID=ei.SystemId AND sft.WorkDate=apd.WorkDate
                        WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
                        AND ei.PlantId='" + PlantId + @"'
                        ) AS K WHERE " + ParticularsKey + ">0";


        }

        private void GetAttendanceAndOTUpToDateSummary(string FromDate, string ToDate, string companyId, ref DataTable dtFinal, out DataTable dtPlant)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {

                strSql = GetAttendanceAndOTSQL(FromDate, ToDate, companyId);
                con.getDataSet(strSql, out DataSet dsRef);

                SqlRepository _sqlRepo = new SqlRepository();
                dtPlant = _sqlRepo.GetDataTable("select * from org.plant where companyid='" + companyId + @"'");

                if (dtFinal == null)
                {
                    dtFinal = new DataTable("FinalTable");
                    dtFinal.Columns.Add("Particulars");
                    dtFinal.Columns.Add("ParticularsKey");
                    dtFinal.Columns.Add("PlantId");
                    dtFinal.Columns.Add("PlantName");
                    dtFinal.Columns.Add("Yesterday", typeof(double));
                    dtFinal.Columns.Add("UpToDate", typeof(double));

                }


                int DataColumnStart = 1;
                for (int PL = 0; PL < dsRef.Tables[0].Rows.Count; PL++)
                {
                    dtPlant.DefaultView.RowFilter = "Id='" + dsRef.Tables[0].Rows[PL]["PlantId"].ToString() + "'";

                    for (int COL = DataColumnStart; COL < dsRef.Tables[0].Columns.Count; COL++)
                    {
                        DataRow dr = dtFinal.NewRow();
                        dr["ParticularsKey"] = dsRef.Tables[0].Columns[COL].ColumnName;
                        dr["Particulars"] = getCaption(dsRef.Tables[0].Columns[COL].ColumnName);
                        dr["PlantId"] = dtPlant.DefaultView[0]["Id"].ToString();
                        dr["PlantName"] = dtPlant.DefaultView[0]["UserName"].ToString();

                        dr["UpToDate"] = dsRef.Tables[0].Rows[PL][COL].ToString();

                        dtFinal.Rows.Add(dr);
                    }

                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        private void GetAttendanceAndOTyesterdaySummary(string FromDate, string ToDate, string companyId, ref DataTable dtFinal)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {

                strSql = GetAttendanceAndOTSQL(ToDate, ToDate, companyId);
                con.getDataSet(strSql, out DataSet dsRef);




                int DataColumnStart = 1;
                for (int PL = 0; PL < dsRef.Tables[0].Rows.Count; PL++)
                {

                    for (int COL = DataColumnStart; COL < dsRef.Tables[0].Columns.Count; COL++)
                    {
                        dtFinal.DefaultView.RowFilter = "ParticularsKey='" + dsRef.Tables[0].Columns[COL].ToString() + @"' and PlantId='" + dsRef.Tables[0].Rows[PL]["PlantId"].ToString() + @"'";


                        if (dtFinal.DefaultView.Count > 0)
                            dtFinal.DefaultView[0]["Yesterday"] = dsRef.Tables[0].Rows[PL][COL].ToString();

                    }

                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 


        private void GetUNApprovedProfileSummary(string companyId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {

                strSql = @"SELECT EI.PlantId, COUNT(*) AS CNT
                        FROM EmployeeInformation EI WHERE EI.EmployeeStatus = 'Active'                        
                        	and ISNULL(EI.IsApproved,0)=0 
                        
                    and ei.companyId='" + companyId + @"' GROUP BY EI.PlantId";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        private void GetProfileNoSalarySummary(string FromDate, string companyId, string companyGroupId, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT  EI.PlantId, COUNT(*) AS CNT
                        FROM EmployeeInformation EI
                        WHERE EI.EmployeeStatus='Active' and EI.SystemId NOT IN (
                        SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster 
                        UNION 
                        SELECT EmpInfoSystemID FROM SalaryInfoBackMaster 
                        )                          
                        and EI.DOJ <= '" + FromDate + @"'  
                 and  ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                 and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + ToDate + @"')
                        GROUP BY EI.PlantId";
                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        private void GetNoSalaryStructureApproveSummary(string FromDate, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EI.PlantId, COUNT(*) AS CNT
                        FROM EmployeeInformation EI
                        WHERE EI.EmployeeStatus = 'Active' AND EI.SystemId  IN (
                             SELECT EmpInfoSystemID FROM (                        
                        SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID,IsApproved FROM (                        
                        SELECT  EffectiveDate,EmpInfoSystemID,IsApproved FROM SalaryInfoDefineMaster  WHERE EffectiveDate<='" + FromDate + @"' 
                        union
                        SELECT  EffectiveDate,EmpInfoSystemID,IsApproved FROM SalaryInfoBackMaster  WHERE  EffectiveDate<='" + FromDate + @"'
                        ) x GROUP BY EmpInfoSystemID,IsApproved 
                        ) r WHERE IsApproved=0
                        ) 
                        and EI.DOJ <= '" + FromDate + @"'
                  and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'                        
                        GROUP BY EI.PlantId";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        private void GetLongAbsentisomSummary(string companyId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EI.PlantId,COUNT(*) AS CNT

                        FROM EmployeeInformation EI
                        WHERE 
                         ei.companyId='" + companyId + @"' 
                            AND isnull(EI.EmployeeCurrentStatus,'')='LONG ABSENTEEISM' 

                            group by EI.PlantId
                        ";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        private void GetTBSSummary(string companyId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EI.PlantId,COUNT(*) AS CNT

                FROM EmployeeInformation EI
                WHERE
                ei.companyId='" + companyId + @"' 
                AND EI.EmployeeCurrentStatus='TBS'   group by EI.PlantId";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        private void GetBankRemarkSummary(string FromDate, string companyId, string companyGroupId, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @" select K.PlantId,COUNT(*) AS CNT FROM (   select distinct EI.SystemId,EI.PlantId
							
                            from EmployeeInformation EI
                        
                            left join EmployeeBankInfo b on ei.SystemId=b.EmpSystemID
                            where 
	                    	 EI.DOJ<='" + FromDate + @"' AND (EI.DOS is null OR EI.DOS>= '" + ToDate + @"') 
                         and EI.CompanyId='" + companyId + @"' and EI.GroupID='" + companyGroupId + @"'
                         and EI.DOJ<='" + FromDate + @"' AND (EI.DOS is null OR EI.DOS>= '" + ToDate + @"')
                            and                          
                            (--plant
                            (isnull(EI.PaymentMode,'')='Bank' and ISNULL(b.BankAccNo,'')='') 
                            or (isnull(EI.PaymentMode,'')='Cash' and ISNULL(b.BankAccNo,'')<>'') 
                            or (isnull(EI.PaymentMode,'')='Transfer' and ISNULL(b.BankAccNo,'')='') 
                            --or (isnull(EI.PaymentMode,'')='Bank' and ISNULL(b.BankAccNo,'')<>'' 
                            or b.IsApproved=0)--plant 
                            ) AS K group by PlantId";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        private void GetSalaryNotApprovedSummary(string FromDate, string companyId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            DateTime NewMonth;
            string otFutureDate = Convert.ToDateTime(FromDate).ToString("dd-MMM-yyyy");
            NewMonth = Convert.ToDateTime(otFutureDate).AddMonths(-1);
            try
            {
                strSql = @" select EI.PlantId, COUNT(*) AS CNT
                            
                            from (select distinct spc.EmpInfoSystemID,spc.SlrProcMstSystemID,spm.YearNo,spm.MonthNo from SalaryProcChild spc
							left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
							) c
                            inner join SalaryLock sl on sl.EmpSystemId=c.EmpInfoSystemID and sl.MonthNo=c.MonthNo and sl.YearNo=c.YearNo and isnull(IsLocked,0)=1
                            LEFT JOIN EmployeeInformation EI ON c.EmpInfoSystemID = EI.SystemId
                            where isnull(sl.EmpSystemId,'')='' AND EI.CompanyId='" + companyId + @"'
                            group by  EI.PlantId  ";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        private void GetShiftNotAssignSummary(string FromDate, string companyId, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
            string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                strSql = @"select x.PlantId,count(*) AS CNT
                                    from org.Position p
                                    left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
                                    left join 
                                    
                                    (	select  e.EmployeeCode,e.EmployeeName,e.PlantId,E.CompanyId
									,FORMAT( EffectiveDate,'dd-MMM-yyyy')EffectiveDate
									,FORMAT(e.DOJ,'dd-MMM-yyyy')DOJ
									,FORMAT(c.CutOffDate,'dd-MMM-yyyy')CutOffDate
									,e.BudgetCode,e.LegalDesignationId
									,flag= case when isnull(es.EmpSystemID,'')='' then 'Shift Not Assign'
									when e.DOJ<es.EffectiveDate and c.CutOffDate<es.EffectiveDate then 'Wrong Effective Date'
									else ''									end
									from EmployeeInformation e
									left join SCS.OpeningBalanceCutOffDate c on c.PlantId=e.PlantId and c.ModuleName='HR'
									left join (select MIN(EffectiveDate)EffectiveDate,EmpSystemID from EmployeeShiftAssign where IsSingleDayShift=0
									group by EmpSystemID
									) es on es.EmpSystemID = e.SystemId
									
									) x on x.BudgetCode = mpb.Id									
									
									where x.flag != '' and isnull(EmployeeCode,'') != ''
									and x.EffectiveDate >= '" + ToDate + @"' and x.CompanyId = '" + companyId + "' GROUP BY X.PlantId";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function




        public void GetUNApprovedProfileEmployeeDetail(string plantId, out List<Dictionary<string, object>> dsRef)
        {
            string strSql = string.Empty;

            try
            {

                strSql = @"SELECT EI.SystemId, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                        FROM EmployeeInformation EI 

                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id


                        WHERE EI.EmployeeStatus = 'Active'                        
                        	and ISNULL(EI.IsApproved,0)=0 
                        
                    and ei.plantId='" + plantId + @"' ";

                dsRef = _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
               
            }
        }//End Function 
        public void GetProfileNoSalaryEmployeeDetail(string FromDate, string plantId, string companyGroupId, string ToDate, out List<Dictionary<string, object>> dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT  EI.SystemId, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                        FROM EmployeeInformation EI

                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id



                        WHERE EI.EmployeeStatus='Active' and EI.SystemId NOT IN (
                        SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster 
                        UNION 
                        SELECT EmpInfoSystemID FROM SalaryInfoBackMaster 
                        )                          
                        and EI.DOJ <= '" + FromDate + @"'  
                 and  ei.plantId='" + plantId + @"' and ei.GroupID='" + companyGroupId + @"'
                 and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + ToDate + @"')
                       ";
                dsRef = _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        public void GetNoSalaryStructureApproveEmployeeDetail(string FromDate, string plantId, string companyGroupId, out List<Dictionary<string, object>> dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EI.SystemId, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            EI.SectionId,SS.UserName SubSection
                        FROM EmployeeInformation EI

                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id


                        WHERE EI.EmployeeStatus = 'Active' AND EI.SystemId  IN (
                             SELECT EmpInfoSystemID FROM (                        
                        SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID,IsApproved FROM (                        
                        SELECT  EffectiveDate,EmpInfoSystemID,IsApproved FROM SalaryInfoDefineMaster  WHERE EffectiveDate<='" + FromDate + @"' 
                        union
                        SELECT  EffectiveDate,EmpInfoSystemID,IsApproved FROM SalaryInfoBackMaster  WHERE  EffectiveDate<='" + FromDate + @"'
                        ) x GROUP BY EmpInfoSystemID,IsApproved 
                        ) r WHERE IsApproved=0
                        ) 
                        and EI.DOJ <= '" + FromDate + @"'
                  and ei.plantId='" + plantId + @"' and ei.GroupID='" + companyGroupId + @"'                        
                        ";
                dsRef = _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        public void GetLongAbsentisomEmployeeDetail(string plantId, out List<Dictionary<string, object>> dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  EI.SystemId, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection

                        FROM EmployeeInformation EI

                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id


                        WHERE 
                         ei.plantId='" + plantId + @"' 
                            AND isnull(EI.EmployeeCurrentStatus,'')='LONG ABSENTEEISM' 

                        ";
                dsRef = _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetTBSEmployeeDetail(string plantId, out List<Dictionary<string, object>> dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  EI.SystemId, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection

                        FROM EmployeeInformation EI

                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id


                WHERE
                ei.plantId='" + plantId + @"' 
                AND EI.EmployeeCurrentStatus='TBS'";
                dsRef = _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetBankRemarkEmployeeDetail(string FromDate, string plantId, string companyGroupId, string ToDate, out List<Dictionary<string, object>> dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @" select K.* FROM (   select distinct  EI.SystemId, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
							
                            from EmployeeInformation EI
                        
                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id


                            left join EmployeeBankInfo b on ei.SystemId=b.EmpSystemID
                            where 
	                    	 EI.DOJ<='" + FromDate + @"' AND (EI.DOS is null OR EI.DOS>= '" + ToDate + @"') 
                         and EI.plantId='" + plantId + @"' and EI.GroupID='" + companyGroupId + @"'
                         and EI.DOJ<='" + FromDate + @"' AND (EI.DOS is null OR EI.DOS>= '" + ToDate + @"')
                            and                          
                            (--plant
                            (isnull(EI.PaymentMode,'')='Bank' and ISNULL(b.BankAccNo,'')='') 
                            or (isnull(EI.PaymentMode,'')='Cash' and ISNULL(b.BankAccNo,'')<>'') 
                            or (isnull(EI.PaymentMode,'')='Transfer' and ISNULL(b.BankAccNo,'')='') 
                            --or (isnull(EI.PaymentMode,'')='Bank' and ISNULL(b.BankAccNo,'')<>'' 
                            or b.IsApproved=0)--plant 
                            ) AS K ";
                dsRef = _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetSalaryNotApprovedEmployeeDetail(string FromDate, string plantId, out List<Dictionary<string, object>> dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            DateTime NewMonth;
            string otFutureDate = Convert.ToDateTime(FromDate).ToString("dd-MMM-yyyy");
            NewMonth = Convert.ToDateTime(otFutureDate).AddMonths(-1);
            try
            {
                strSql = @" select EI.SystemId, EI.EmployeeName
                        ,EI.EmployeeCode,EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric
                        ,EI.EmpPicPath,
                        EI.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            
                            from (select distinct spc.EmpInfoSystemID,spc.SlrProcMstSystemID,spm.YearNo,spm.MonthNo from SalaryProcChild spc
							left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
							) c
                            inner join SalaryLock sl on sl.EmpSystemId=c.EmpInfoSystemID and sl.MonthNo=c.MonthNo and sl.YearNo=c.YearNo and isnull(IsLocked,0)=1
                            LEFT JOIN EmployeeInformation EI ON c.EmpInfoSystemID = EI.SystemId
                            
                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id

                            where isnull(sl.EmpSystemId,'')='' AND EI.plantId='" + plantId + @"'
                            ";
                dsRef = _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetShiftNotAssignEmployeeDetail(string FromDate, string plantId, string ToDate, out List<Dictionary<string, object>> dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
            string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                strSql = @"SELECT X.*
                                    from org.Position p
                                    left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
                                    left join 
                                    
                                    (	select  EI.EmployeeCode,EI.EmployeeName,EI.PlantId,EI.CompanyId
									,FORMAT( es.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
									,FORMAT(EI.DOJ,'dd-MMM-yyyy')DOJ
									,FORMAT(c.CutOffDate,'dd-MMM-yyyy')CutOffDate
									,EI.BudgetCode,EI.LegalDesignationId
									,flag= case when isnull(es.EmpSystemID,'')='' then 'Shift Not Assign'
									when EI.DOJ<es.EffectiveDate and c.CutOffDate<es.EffectiveDate then 'Wrong Effective Date'
									else ''									end
									FROM EmployeeInformation EI 

								LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
								LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
								LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
								LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
								LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EI.LegalDesignationId
								LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
								LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
								LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
                            
									left join SCS.OpeningBalanceCutOffDate c on c.PlantId=ei.PlantId and c.ModuleName='HR'
									left join (select MIN(EffectiveDate)EffectiveDate,EmpSystemID from EmployeeShiftAssign where IsSingleDayShift=0
									group by EmpSystemID
									) es on es.EmpSystemID = ei.SystemId
									
									) x on x.BudgetCode = mpb.Id									
									
									where x.flag != '' and isnull(EmployeeCode,'') != ''
									and x.EffectiveDate >= '" + ToDate + @"' and x.plantId = '" + plantId + "'";

                dsRef = _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
    
    
    }
}
