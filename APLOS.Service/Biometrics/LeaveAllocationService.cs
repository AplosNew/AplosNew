#region Using

using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;

#endregion Using

namespace Library.Service.Biometrics
{
    public class LeaveAllocationService : Service<LeaveAllocation>, ILeaveAllocationService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IAccessControllerDeleteRequestService _d;

        public LeaveAllocationService(
            IRepositoryAsync<LeaveAllocation> PreRecruitmentEmpReferenceRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IAccessControllerDeleteRequestService d
            , IEmployeeInformationService employeeInformationService) :
            base(PreRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _d = d;
            _employeeInformationService = employeeInformationService;
        }

        #endregion Constructor

        private DataSet GetAttdnDataForMonthlyProc(string sGroupID, string sAttnDate, string sEmpSystemIDColl)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate,
		                            SUM(ISNULL(TotalPresent, 0)) TotalPresent, SUM(ISNULL(TotalLate, 0)) TotalLate,
		                            SUM(ISNULL(TotalAbsent, 0)) TotalAbsent, SUM(ISNULL(TotalLv, 0)) TotalLv,
		                            SUM(ISNULL(TotalMLv, 0)) TotalMLv, SUM(ISNULL(TotalWeekOff, 0)) TotalWeekOff, SUM(ISNULL(TotalCompAssignLv, 0)) TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay, 0)) TotalHoliDay, SUM(ISNULL(TotalWeekOffHoliDay, 0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(OTHr, 0)) TotalOTHr, PlantID
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
                                        TotalCompAssignLv = CASE WHEN DayStatus = 'CAL' THEN 1
                                                        WHEN DayStatus = 'CALP' THEN 1
						                                WHEN DayStatus = 'CALL' THEN 1
						                                WHEN DayStatus = 'WCAL' THEN 1
						                                WHEN DayStatus = 'HCAL' THEN 1
						                                WHEN DayStatus = 'WCALP' THEN 1
						                                WHEN DayStatus = 'HCALP' THEN 1
						                                WHEN DayStatus = 'WCALL' THEN 1
						                                WHEN DayStatus = 'HCALL' THEN 1
                                                        WHEN DayStatus = 'WHCAL' THEN 1
                                                        WHEN DayStatus = 'WHCALP' THEN 1
                                                        WHEN DayStatus = 'WHCALL' THEN 1
						                                WHEN DayStatus = 'HWCAL' THEN 1
						                                WHEN DayStatus = 'HWCALP' THEN 1
						                                WHEN DayStatus = 'HWCALL' THEN 1
						                                ELSE 0 END,
			                            TotalWeekOff = CASE WHEN DayStatus = 'W' THEN 1
						                                ELSE 0 END,
			                            TotalHoliDay = CASE WHEN DayStatus = 'H' THEN 1
						                               ELSE 0 END,
                                        TotalWeekOffHoliDay = CASE WHEN DayStatus = 'WH' THEN 1
							                            WHEN DayStatus = 'HW' THEN 1
						                               ELSE 0 END,
                                        OTHr
	                             FROM dbo.AttdnProcessData
                                WHERE GroupID = '" + sGroupID + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")
                                    AND MONTH(WorkDate) = MONTH('" + sAttnDate + @"')
                                    AND YEAR(WorkDate) = YEAR('" + sAttnDate + @"')) A
                            GROUP BY EmpSystemID, PlantID";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetAttdnDataMonthlySummary(string sGroupID, int MonthNo, int YearNo, string sEmpSystemIDColl)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.AttdnDataMonthlySummary
                           WHERE GroupID = '" + sGroupID + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")
                                    AND MonthNo = " + MonthNo + @" AND YearNo = " + YearNo + @"";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool AttdnDateMonthlySummaryProcess(string GroupSysID, string sAttnDate, string sEmpSystemIDColl)
        {
            #region declare variables

            DataSet dsAttnDataForTheMonth = null;

            DataSet dsAttnDataMonthSummary = null;
            DataTable dtAttnDataMonthSummary = null;
            DataRow drAttnDataMonthSummary = null;
            DataView dvAttnDataMonthSummary = null;

            //clsRegister objReg;
            //objReg = new clsRegister();
            var bValid = false;

            #endregion declare variables

            try
            {
                #region DataSet

                //string strFromDate = FirstDayOfMonthFromDateTime(Convert.ToDateTime(sWorkDate.Trim())).ToString();

                dsAttnDataForTheMonth = GetAttdnDataForMonthlyProc(GroupSysID.Trim(), sAttnDate.Trim(), sEmpSystemIDColl.Trim());
                dsAttnDataMonthSummary = GetAttdnDataMonthlySummary(GroupSysID.Trim(), Convert.ToDateTime(sAttnDate.Trim()).Month, Convert.ToDateTime(sAttnDate.Trim()).Year, sEmpSystemIDColl.Trim());
                dtAttnDataMonthSummary = dsAttnDataMonthSummary.Tables[0];

                #endregion DataSet

                for (int i = 0; i < dsAttnDataForTheMonth.Tables[0].Rows.Count; i++)
                {
                    dvAttnDataMonthSummary = new DataView();
                    dvAttnDataMonthSummary.Table = dtAttnDataMonthSummary;
                    dvAttnDataMonthSummary.RowFilter = "EmpSystemID = '" + dsAttnDataForTheMonth.Tables[0].Rows[i]["EmpSystemID"] + "'";
                    if (dvAttnDataMonthSummary.Count == 0)
                    {
                        drAttnDataMonthSummary = dtAttnDataMonthSummary.NewRow();
                        drAttnDataMonthSummary["EmpSystemID"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["EmpSystemID"].ToString();
                        drAttnDataMonthSummary["AddedBy"] = "Schedule";
                        drAttnDataMonthSummary["DateAdded"] = DateTime.Now;

                        drAttnDataMonthSummary["MonthNo"] = Convert.ToDateTime(sAttnDate.Trim()).Month.ToString();
                        drAttnDataMonthSummary["YearNo"] = Convert.ToDateTime(sAttnDate.Trim()).Year.ToString();

                        drAttnDataMonthSummary["FromDate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["FromDate"].ToString();
                        drAttnDataMonthSummary["ToDate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["ToDate"].ToString();

                        drAttnDataMonthSummary["TotalProcDate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalProcDate"].ToString();
                        drAttnDataMonthSummary["TotalPresent"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalPresent"].ToString();

                        drAttnDataMonthSummary["TotalLate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalLate"].ToString();
                        drAttnDataMonthSummary["TotalAbsent"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalAbsent"].ToString();
                        drAttnDataMonthSummary["TotalLv"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalLv"].ToString();

                        drAttnDataMonthSummary["TotalMLv"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalMLv"].ToString();
                        drAttnDataMonthSummary["TotalCompAssignLv"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalCompAssignLv"].ToString();
                        drAttnDataMonthSummary["TotalWeekOff"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalWeekOff"].ToString();
                        drAttnDataMonthSummary["TotalHoliDay"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalHoliDay"].ToString();
                        drAttnDataMonthSummary["TotalWeekOffHoliDay"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalWeekOffHoliDay"].ToString();

                        drAttnDataMonthSummary["TotalOTHr"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalOTHr"].ToString();
                        drAttnDataMonthSummary["TotalNormalOTHr"] = 0;
                        drAttnDataMonthSummary["TotalExtraOTHr"] = 0;

                        drAttnDataMonthSummary["GroupID"] = GroupSysID.ToString().Trim();
                        drAttnDataMonthSummary["PlantID"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["PlantID"].ToString();

                        drAttnDataMonthSummary["UpdatedBy"] = "Schedule";
                        drAttnDataMonthSummary["DateUpdated"] = DateTime.Now;
                        dtAttnDataMonthSummary.Rows.Add(drAttnDataMonthSummary);
                    }
                    else
                    {
                        drAttnDataMonthSummary = dvAttnDataMonthSummary[0].Row;
                        drAttnDataMonthSummary.BeginEdit();
                        drAttnDataMonthSummary["FromDate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["FromDate"].ToString();
                        drAttnDataMonthSummary["ToDate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["ToDate"].ToString();

                        drAttnDataMonthSummary["TotalProcDate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalProcDate"].ToString();
                        drAttnDataMonthSummary["TotalPresent"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalPresent"].ToString();

                        drAttnDataMonthSummary["TotalLate"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalLate"].ToString();
                        drAttnDataMonthSummary["TotalAbsent"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalAbsent"].ToString();
                        drAttnDataMonthSummary["TotalLv"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalLv"].ToString();

                        drAttnDataMonthSummary["TotalMLv"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalMLv"].ToString();
                        drAttnDataMonthSummary["TotalCompAssignLv"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalCompAssignLv"].ToString();
                        drAttnDataMonthSummary["TotalWeekOff"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalWeekOff"].ToString();
                        drAttnDataMonthSummary["TotalHoliDay"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalHoliDay"].ToString();
                        drAttnDataMonthSummary["TotalWeekOffHoliDay"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalWeekOffHoliDay"].ToString();

                        drAttnDataMonthSummary["TotalOTHr"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalOTHr"].ToString();
                        drAttnDataMonthSummary["TotalNormalOTHr"] = 0;
                        drAttnDataMonthSummary["TotalExtraOTHr"] = 0;

                        drAttnDataMonthSummary["GroupID"] = GroupSysID.ToString().Trim();
                        drAttnDataMonthSummary["PlantID"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["PlantID"].ToString();

                        drAttnDataMonthSummary["UpdatedBy"] = "Schedule";
                        drAttnDataMonthSummary["DateUpdated"] = DateTime.Now;
                        drAttnDataMonthSummary.EndEdit();
                    }
                }
                SaveDataSets(dsAttnDataMonthSummary);

                bValid = true;
                return bValid;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                #region clean variable

                dsAttnDataForTheMonth = null;

                dsAttnDataMonthSummary = null;
                dtAttnDataMonthSummary = null;
                drAttnDataMonthSummary = null;
                dvAttnDataMonthSummary = null;

                #endregion clean variable
            }
        }//End Function

        private void SaveDataSets(DataSet dsAttnDataMonthSummary)
        {
            List<AttdnRawData> AttdnRawDataList = null;
            InitAttdnRawData(dsAttnDataMonthSummary, out AttdnRawDataList);
            //SaveAttdnRawData(AttdnRawDataList);
        }

        private void InitAttdnRawData(DataSet dsRawData, out List<AttdnRawData> AttdnRawDataList)
        {
            AttdnRawDataList = new List<AttdnRawData>();
            try
            {
                for (int i = 0; i < dsRawData.Tables[0].Rows.Count; i++)
                {
                    if (dsRawData.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                    }
                    else
                    {
                        var ob = new AttdnRawData();
                        ob.ProcessedFlag = Convert.ToBoolean(dsRawData.Tables[0].Rows[i]["ProcessedFlag"].ToString());
                        ob.AddedBy = dsRawData.Tables[0].Rows[i]["AddedBy"].ToString();
                        ob.ModelState = ModelState.Modified;
                        AttdnRawDataList.Add(ob);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}