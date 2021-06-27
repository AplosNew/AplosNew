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
    public class EmployeeNotificationsService : Service<EmployeeNotifications>, IEmployeeNotificationsService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IAccessControllerDeleteRequestService _d;

        public EmployeeNotificationsService(
            IRepositoryAsync<EmployeeNotifications> PreRecruitmentEmpReferenceRepository
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

        private DataSet GetAttdnRawDataForAttdnProc(string sGroupID, string sAttnDate, string sType)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM AttdnRawData
                           WHERE PDate = '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
                                 AND ProcessedFlag = 0";

                if (sType != "")
                {
                    parameters.CmdText = parameters.CmdText + @" AND PType = '" + sType + @"'";
                }
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetAttdnProcData(string sGroupID, string sPlantID, string strPrvAttnDate, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters.CmdText = @"SELECT * FROM dbo.AttdnProcessData
                           WHERE WorkDate BETWEEN '" + strPrvAttnDate + @"'
                                 AND '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
                                 AND EmpSystemID IN (
                                                     SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                        WHERE JobLcSystemID IN (
                                                                                SELECT SystemID FROM [dbo].[JobLocation]
                                                                                    WHERE PlantID = '" + sPlantID + @"'
                                                                                )
                                                    )";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetFinalOT(string sGroupID, string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.FinalOT
                                    WHERE WorkDate = '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
                                          AND EmpSystemID IN (
                                                             SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                                WHERE JobLcSystemID IN (
                                                                                        SELECT SystemID FROM [dbo].[JobLocation]
                                                                                            WHERE PlantID = '" + sPlantID + @"'
                                                                                       )
                                                            )";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetOTSlabDefineEmployee(string sGroupID, string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.OTSlabDefineEmployee
                           WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate
                                AND GroupID = '" + sGroupID + @"'
                                AND EmpSystemID IN (
                                                    SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                        WHERE JobLcSystemID IN (
                                                                                SELECT SystemID FROM [dbo].[JobLocation]
                                                                                    WHERE PlantID = '" + sPlantID + @"'
                                                                                )
                                                    )";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetOTSlabDefineGeneral(string sGroupID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.OTSlabDefineGeneral
                           WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate AND GroupID = '" + sGroupID + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmployeeInfo(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT E.*, ES.*, ISNULL(DATEDIFF(D, Atd.LastWorkDate, '" + sAttnDate + @"'), 0) DateDiffer, ISNULL(Atd.LastWorkDate, GETDATE()) LastWorkDate, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle, EmOT.OTStartDate, EmOT.OTEndDate
	                        FROM
                            (
                             SELECT * FROM EmployeeInformation WHERE
                                    SystemID IN (
                                                 SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                )
                            ) AS E
		                        INNER JOIN (
											SELECT * FROM
														(
														 SELECT ES.EmpSystemID, ES.ShiftSystemID, ES.DayType, S.ShiftType,
																OfficeStartTime = CASE WHEN C.InTimeStartMargin != '' THEN DATEADD(MI, -C.InTimeStartMargin, C.InTime)
																					  ELSE DATEADD(MI, -S.InTimeStartMargin, S.InTime) END,
																OfficeTime = CASE WHEN C.LateMargin != '' THEN DATEADD(MI, C.LateMargin, C.InTime)
																					  ELSE DATEADD(MI, S.LateMargin, S.InTime) END,
																InTime = CASE WHEN C.InTime != '' THEN C.InTime
																					  ELSE S.InTime END,
																InTimeStartMargin = CASE WHEN C.InTimeStartMargin != '' THEN C.InTimeStartMargin
																					  ELSE S.InTimeStartMargin END,
																BreakStratTime = CASE WHEN C.BreakStratTime != '' THEN C.BreakStratTime
																					  ELSE S.BreakStratTime END,
																BreakEndTime = CASE WHEN C.BreakEndTime != '' THEN C.BreakEndTime
																					  ELSE S.BreakEndTime END,
																OfficeEndTime = CASE WHEN C.OutTimeEndMargin != '' THEN DATEADD(MI, C.OutTimeEndMargin, S.OutTime)
																					  ELSE DATEADD(MI, S.OutTimeEndMargin, S.OutTime) END,
																OTStartTime = CASE WHEN S.IsGapInclude = 1 AND C.OutTime != '' THEN C.OutTime
																				   WHEN S.IsGapInclude = 1 AND C.OutTime = '' THEN S.OutTime
																				   WHEN S.IsGapInclude = 0 AND C.OutTime != '' THEN DATEADD(MI, C.OTStartTime, C.OutTime)
																				   ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
														 FROM dbo.EmpDateWiseShiftAssign ES
																	LEFT JOIN dbo.ShiftDefination S ON ES.ShiftSystemID = S.SystemID
																	LEFT JOIN (
																				SELECT SCM.*, SCC.ShiftDate FROM [dbo].[ShiftTimeChgMaster] SCM
																						INNER JOIN [dbo].[ShiftTimeChgChild] SCC ON SCM.SystemID = STCMasterSystemID
																				WHERE SCC.ShiftDate = '" + sAttnDate + @"'
																			  ) C ON ES.ShiftSystemID = C.ShiftDefinationID
														 WHERE ES.WorkDate = '" + sAttnDate + @"' AND ES.GroupID = '" + sGroupID + @"'
														) A
											WHERE --CONVERT(DATETIME, CONVERT(VARCHAR(5), InTime, 108)) < CONVERT(DATETIME, CONVERT(VARCHAR(5), GETDATE(), 108))
                                                  CONVERT(DATETIME, CONVERT(VARCHAR(11), '" + sAttnDate + @"', 101) + ' ' + CONVERT(VARCHAR(5), InTime, 108)) < CONVERT(DATETIME, CONVERT(VARCHAR(11), GETDATE(), 101) + ' ' + CONVERT(VARCHAR(5), GETDATE(), 108))
                                           ) ES ON E.SystemID = ES.EmpSystemID
                                LEFT JOIN (
											SELECT * FROM dbo.EmployeeOTEntitle
													WHERE '" + sAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE())
																					AND ISNULL(OTEndDate, GETDATE())
										   ) EmOT ON E.SystemID = EmOT.EmpSystemID
								LEFT JOIN
                                        (
                                            SELECT EmpSystemID, MAX(WorkDate) LastWorkDate
	                                            FROM dbo.AttdnProcessData
                                            WHERE GroupID = '" + sGroupID + @"'
                                            GROUP BY EmpSystemID
                                        ) AS Atd ON E.SystemID = Atd.EmpSystemID
                            WHERE (E.DOS > '" + sAttnDate + @"' OR DOS IS NULL) AND E.DOJ <= '" + sAttnDate + @"' AND E.GroupID = '" + sGroupID + @"'
                                  AND E.SystemID IN (" + sEmpSysIdColl + @")
                            ORDER BY E.EmployeeCode";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetAttdnManualData(string sGroupID, string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM AttdnManualData
                           WHERE WorkDate = '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
                                 AND EmpSystemID IN (
                                                     SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                        WHERE JobLcSystemID IN (
                                                                                SELECT SystemID FROM [dbo].[JobLocation]
                                                                                    WHERE PlantID = '" + sPlantID + @"'
                                                                                )
                                                    )";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateAttdnData(string OPN_FLAG, string GroupSysID, string sType, string sEmpSystemID, string sPlantID, string sWorkingDate, string shiftSystemID, string sDate, string sTime, bool bManualTime, string sRowID, string sDayStatus, bool bManualDayStatus, decimal iOverTime, string sLvTrans, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["AddedBy"] = "Schedule";
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["EmpSystemID"] = sEmpSystemID;
                drLocal["WorkDate"] = sWorkingDate;
                if (shiftSystemID != string.Empty)
                {
                    drLocal["ShiftSystemID"] = shiftSystemID;
                }

                if (sType == "IN")
                {
                    if (sTime == string.Empty || sTime == "00:00:00")
                    {
                        drLocal["InTime"] = DBNull.Value;
                        drLocal["IsManualInTime"] = false;
                    }
                    else
                    {
                        drLocal["InTime"] = sDate + " " + sTime;
                        drLocal["IsManualInTime"] = bManualTime;
                    }

                    //drLocal["InTime"] = sTime;
                    if (sRowID == string.Empty)
                    {
                        drLocal["InTimeRowID"] = DBNull.Value;
                    }
                    else
                    {
                        drLocal["InTimeRowID"] = sRowID;
                    }
                    drLocal["DayStatus"] = sDayStatus;
                    drLocal["IsManualDayStatus"] = bManualDayStatus;

                    if (sLvTrans != "")
                    {
                        drLocal["LTSystemID"] = sLvTrans;
                    }
                    else
                    {
                        drLocal["LTSystemID"] = DBNull.Value;
                    }
                }
                else if (sType == "OUT")
                {
                    if (sTime == string.Empty || sTime == "00:00:00")
                    {
                        drLocal["OutTime"] = DBNull.Value;
                        drLocal["IsManualOutTime"] = false;
                    }
                    else
                    {
                        drLocal["OutTime"] = sDate + " " + sTime;
                        drLocal["IsManualOutTime"] = bManualTime;
                    }

                    drLocal["OTHr"] = iOverTime;
                    //drLocal["IsManualOTHr"] = bManualTime;

                    if (sRowID == string.Empty)
                    {
                        drLocal["OutTimeRowID"] = DBNull.Value;
                    }
                    else
                    {
                        drLocal["OutTimeRowID"] = sRowID;
                    }
                    //if (sDayStatus != string.Empty)
                    //{
                    //    drLocal["DayStatus"] = sDayStatus;
                    //}
                }
                drLocal["ToReprocess"] = "No";

                drLocal["GroupID"] = GroupSysID.ToString().Trim();
                drLocal["PlantID"] = sPlantID.Trim();

                drLocal["UpdatedBy"] = "Schedule";
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool AttdnDateProcessForOutData(string _plantId, string sAttnDate, string GroupSysID, string sEmpSystemIDColl, string sMinOT, string sFractionCalculate, bool radDwLdEnrollID, bool radDwLdScanNumber)
        {
            #region declare variables

            DataSet dsRawData = null;
            DataTable dtRawData = null;
            DataView dvRawData = null;
            DataRow drRawData = null;

            DataSet dsMnAttData = null;
            DataTable dtMnAttData = null;
            DataView dvMnAttData = null;

            DataSet dsAttnProcData = null;
            DataTable dtAttnProcData = null;
            DataRow drAttnProcData = null;
            DataView dvAttnProcData = null;

            DataSet dsEmpInfo = null;

            DataSet dsFinalOT = null;
            DataTable dtFinalOT = null;
            DataSet dsOTSlabEmp = null;
            DataTable dtOTSlabEmp = null;
            DataView dvOTSlabEmp = null;

            DataSet dsOTSlabGen = null;
            DataTable dtOTSlabGen = null;
            var sLogDownLoadNum = "";
            var sEmpSysID = "";
            var sPlantID = "";
            var sOTStartTime = "";
            decimal iTotalOTHr = 0;
            //decimal iNormalOTHr = 0;
            //decimal iExtraOTHr = 0;
            var sOTDayType = "";
            decimal dfirstSlab = 0;
            var bIsOTExtentNextSlab = false;
            var bIsTotalWorkTimeAsOT = false;
            var bOTEntitle = false;

            var sOfficeInTime = "";
            var sInTime = "";
            var sOutTime = "";
            var sOutTimeRowID = string.Empty;
            var iDeviceID = 0;
            var sOutTimeTmp = "";
            var sOutTimeRowIDTmp = string.Empty;
            var iDeviceIDTmp = 0;
            //string sDayStatus = "";
            var sShiftSystemID = "";
            var sShiftType = "";
            var sDayType = "";

            var sBreakStratTime = "";
            var sBreakEndTime = "";

            var sDate = "";
            var sPrvDate = "";
            var sWorkingDate = "";
            var bValid = false;

            #endregion declare variables

            try
            {
                #region DataSet

                sDate = sAttnDate.Trim();
                sPrvDate = (Convert.ToDateTime(sAttnDate.Trim()).AddDays(-1)).ToString("dd-MMM-yyyy");

                dsRawData = GetAttdnRawDataForAttdnProc(GroupSysID.Trim(), sDate.Trim(), "OUT");
                dtRawData = dsRawData.Tables[0];

                dsAttnProcData = GetAttdnProcData(GroupSysID.Trim(), _plantId, sPrvDate.Trim(), sDate.Trim());
                dtAttnProcData = dsAttnProcData.Tables[0];

                dsFinalOT = GetFinalOT(GroupSysID.Trim(), _plantId, sDate.Trim());
                dtFinalOT = dsFinalOT.Tables[0];

                dsOTSlabEmp = GetOTSlabDefineEmployee(GroupSysID.Trim(), _plantId, sDate.Trim());
                dtOTSlabEmp = dsOTSlabEmp.Tables[0];

                dsOTSlabGen = GetOTSlabDefineGeneral(GroupSysID.Trim(), sDate.Trim());
                dtOTSlabGen = dsOTSlabGen.Tables[0];

                dsEmpInfo = GetEmployeeInfo(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sDate.Trim());

                dsMnAttData = GetAttdnManualData(GroupSysID.Trim(), _plantId, sAttnDate.Trim());
                dtMnAttData = dsMnAttData.Tables[0];
                dvMnAttData = new DataView();

                #endregion DataSet

                if (dsEmpInfo.Tables[0].Rows.Count > 0)
                {
                    for (var EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                    {
                        sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();
                        sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();
                        sOTStartTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OTStartTime"].ToString().Trim()).ToString("HH:mm:ss");
                        sOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeTime"].ToString().Trim()).ToString("HH:mm:ss");
                        sInTime = "00:00:00";
                        bOTEntitle = Convert.ToBoolean(dsEmpInfo.Tables[0].Rows[EmpCount]["IsOTEntitle"].ToString());
                        iTotalOTHr = 0;
                        ////iNormalOTHr = 0;
                        ////iExtraOTHr = 0;
                        sOTDayType = "";
                        dfirstSlab = 0;
                        bIsOTExtentNextSlab = false;
                        bIsTotalWorkTimeAsOT = false;
                        sShiftSystemID = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftSystemID"].ToString();
                        sShiftType = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftType"].ToString();
                        sDayType = dsEmpInfo.Tables[0].Rows[EmpCount]["DayType"].ToString();

                        sBreakStratTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakStratTime"].ToString().Trim()).ToString("HH:mm:ss");
                        sBreakEndTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakEndTime"].ToString().Trim()).ToString("HH:mm:ss");

                        if (Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["DateDiffer"].ToString()) <= 1)
                        {
                            if (radDwLdEnrollID)
                            {
                                sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["EmployeeCode"].ToString();
                            }
                            else if (radDwLdScanNumber)
                            {
                                sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["CardNumber"].ToString();
                            }

                            #region Find InTime from raw Data Table

                            sOutTime = "00:00:00";
                            sOutTimeRowID = string.Empty;
                            iDeviceID = 0;
                            sOutTimeTmp = "00:00:00";
                            sOutTimeRowIDTmp = string.Empty;
                            iDeviceIDTmp = 0;
                            //sDayStatus = "";

                            dvRawData = new DataView
                            {
                                Table = dtRawData,
                                RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'"
                            };
                            if (dvRawData.Count > 0)
                            {
                                for (var RData = 0; RData < dvRawData.Count; RData++)
                                {
                                    if (dvRawData[RData]["PTime"].ToString() != "")
                                    {
                                        var sysOutTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");
                                        if (sOutTime == "00:00:00" || Convert.ToDateTime(sysOutTime.Trim()) > Convert.ToDateTime(sOutTime.Trim()))
                                        {
                                            sOutTime = sysOutTime;
                                            sOutTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                            iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                            if (sOutTimeTmp != "00:00:00" & Convert.ToDateTime(sOutTime) < Convert.ToDateTime(sOutTimeTmp))
                                            {
                                                sOutTime = sOutTimeTmp;
                                                sOutTimeRowID = sOutTimeRowIDTmp;
                                                iDeviceID = iDeviceIDTmp;
                                            }
                                            sOutTimeTmp = sOutTime;
                                            sOutTimeRowIDTmp = sOutTimeRowID;
                                            iDeviceIDTmp = iDeviceID;
                                        }
                                    }

                                    drRawData = dvRawData[RData].Row;
                                    drRawData.BeginEdit();
                                    drRawData["ProcessedFlag"] = 1;
                                    drRawData.EndEdit();
                                }
                            }

                            #endregion Find InTime from raw Data Table

                            if (sShiftType.ToUpper().Trim() == "DAY SHIFT")
                            {
                                sWorkingDate = sDate.Trim();
                            }
                            else if (sShiftType.ToUpper().Trim() == "NIGHT SHIFT")
                            {
                                sWorkingDate = sPrvDate.Trim();
                            }

                            var bAttnIsLock = false;
                            var bManualOutTime = false;

                            dvAttnProcData = new DataView
                            {
                                Table = dtAttnProcData,
                                RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sWorkingDate.Trim() + "'"
                            };
                            if (dvAttnProcData.Count > 0)
                            {
                                if (dvAttnProcData[0]["InTime"].ToString().Trim() != "")
                                {
                                    sInTime = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                }
                                bAttnIsLock = Convert.ToBoolean(dvAttnProcData[0].Row["IsLock"].ToString());
                                bManualOutTime = Convert.ToBoolean(dvAttnProcData[0].Row["IsManualOutTime"].ToString());
                                //if (iDeviceID == 0)
                                //{
                                //    bManualOutTime = true;
                                //}

                                if (!bAttnIsLock)
                                {
                                    if (dvAttnProcData[0]["OutTime"].ToString() != "")
                                    {
                                        sOutTimeTmp = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                        sOutTimeRowIDTmp = dvAttnProcData[0]["OutTimeRowID"].ToString().Trim();
                                    }

                                    if (Convert.ToDateTime(sOutTime) < Convert.ToDateTime(sOutTimeTmp))
                                    {
                                        sOutTime = sOutTimeTmp;
                                        sOutTimeRowID = sOutTimeRowIDTmp;
                                    }

                                    //string sexieInTime = "00:00:00";
                                    //if (dvAttnProcData[0]["InTime"].ToString() != "")
                                    //{
                                    //    sexieInTime = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    //}
                                    //sDayStatus = dvAttnProcData[0]["DayStatus"].ToString().Trim();

                                    //if ((sexieInTime == "00:00:00") & (sOutTime != "00:00:00") & sDayStatus == "A")
                                    //{
                                    //    if (Convert.ToDateTime(sOutTime) <= Convert.ToDateTime(sOfficeInTime))
                                    //    {
                                    //        sDayStatus = "P";
                                    //    }
                                    //    else
                                    //    {
                                    //        sDayStatus = "L";
                                    //    }
                                    //}

                                    #region Manual Attendance

                                    dvMnAttData.Table = dtMnAttData;
                                    dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                    if (dvMnAttData.Count > 0)
                                    {
                                        if (dvMnAttData[0].Row["OutTime"].ToString().Trim() != "")
                                        {
                                            sOutTime = Convert.ToDateTime(dvMnAttData[0].Row["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                            bManualOutTime = true;
                                        }
                                        sOutTimeRowID = "";
                                        //sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();
                                    }

                                    #endregion Manual Attendance

                                    #region Over Time Calculation

                                    //dvAttnProcData = new DataView();
                                    //dvAttnProcData.Table = dtAttnProcData;
                                    //dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sWorkingDate.Trim() + "'";
                                    //if (dvAttnProcData.Count > 0)
                                    //{
                                    //if (dvAttnProcData[0]["InTime"].ToString().Trim() != "")
                                    //{
                                    //    sInTime = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    //}
                                    //}

                                    if (sOutTime != "00:00:00" & Convert.ToDateTime(sOTStartTime) < Convert.ToDateTime(sOutTime) & bOTEntitle)
                                    {
                                        dvOTSlabEmp = new DataView
                                        {
                                            Table = dtOTSlabEmp,
                                            RowFilter = "EmpSystemID = '" + sEmpSysID + "'"
                                        };
                                        if (dvOTSlabEmp.Count > 0)
                                        {
                                            sOTDayType = dvOTSlabEmp[0].Row["DayType"].ToString();
                                            dfirstSlab = (Convert.ToDecimal(dvOTSlabEmp[0].Row["firstSlab"].ToString()) * 60);
                                            bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsOTExtentNextSlab"].ToString());
                                            bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsTotalWorkTimeAsOT"].ToString());
                                        }
                                        else if (dsOTSlabGen.Tables[0].Rows.Count > 0)
                                        {
                                            sOTDayType = dsOTSlabGen.Tables[0].Rows[0]["DayType"].ToString();
                                            dfirstSlab = (Convert.ToDecimal(dsOTSlabGen.Tables[0].Rows[0]["firstSlab"].ToString()) * 60);
                                            bIsOTExtentNextSlab = Convert.ToBoolean(dsOTSlabGen.Tables[0].Rows[0]["IsOTExtentNextSlab"].ToString());
                                            bIsTotalWorkTimeAsOT = Convert.ToBoolean(dsOTSlabGen.Tables[0].Rows[0]["IsTotalWorkTimeAsOT"].ToString());
                                        }

                                        if (bIsTotalWorkTimeAsOT)
                                        {
                                            if (sInTime != "00:00:00")
                                            {
                                                sInTime = sWorkingDate + " " + sInTime;
                                                sOutTime = sDate + " " + sOutTime;

                                                var tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sInTime);
                                                iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                                            }
                                        }
                                        else if (!bIsTotalWorkTimeAsOT)
                                        {
                                            var tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sOTStartTime);
                                            iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                                        }

                                        var iMinOT = 1;

                                        if (!string.IsNullOrEmpty(sMinOT.Trim()))
                                        {
                                            iMinOT = Convert.ToInt32(sMinOT.Trim());
                                        }

                                        if (sFractionCalculate.ToUpper().Trim() == "ROUND")
                                        {
                                            iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        }
                                        else if (sFractionCalculate.ToUpper().Trim() == "ROUND UP")
                                        {
                                            iTotalOTHr = Convert.ToInt32(Math.Ceiling((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        }
                                        else if (sFractionCalculate.ToUpper().Trim() == "ROUND DOWN")
                                        {
                                            iTotalOTHr = Convert.ToInt32(Math.Floor((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        }
                                        else
                                        {
                                            iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        }
                                    }

                                    #endregion Over Time Calculation

                                    drAttnProcData = dvAttnProcData[0].Row;
                                    drAttnProcData.BeginEdit();
                                    UpdateAttdnData("EDIT", GroupSysID, "OUT", sEmpSysID, sPlantID, sWorkingDate.Trim(), sShiftSystemID, sDate, sOutTime, bManualOutTime, sOutTimeRowID, "", false, iTotalOTHr, "", ref drAttnProcData);
                                    drAttnProcData.EndEdit();
                                }
                            }
                        }
                    }

                    //  objReg.SaveDataSets(dsRawData, dsAttnProcData);
                }
                bValid = true;
                return bValid;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                #region clean variables

                dsRawData = null;
                dtRawData = null;
                dvRawData = null;
                drRawData = null;

                dsAttnProcData = null;
                dtAttnProcData = null;
                drAttnProcData = null;
                dvAttnProcData = null;

                dsEmpInfo = null;

                sLogDownLoadNum = string.Empty;
                sEmpSysID = string.Empty;
                sOTStartTime = string.Empty;

                sOutTime = string.Empty;
                sOutTimeRowID = string.Empty;
                sOutTimeTmp = string.Empty;
                sOutTimeRowIDTmp = string.Empty;
                //sDayStatus = string.Empty;

                #endregion clean variables
            }
        }//End Function

        private void SaveDataSets(DataSet dsRawData, DataSet dsAttnProcData)
        {
            try
            {
                InitAttdnRawData(dsRawData, out var AttdnRawDataList);
                InitAttdnProcessData(dsAttnProcData, out var AttdnProcessDataList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitAttdnRawData(DataSet dsRawData, out List<AttdnRawData> AttdnRawDataList)
        {
            AttdnRawDataList = new List<AttdnRawData>();
            try
            {
                for (var i = 0; i < dsRawData.Tables[0].Rows.Count; i++)
                {
                    if (dsRawData.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                    }
                    else
                    {
                        var ob = new AttdnRawData
                        {
                            ProcessedFlag = Convert.ToBoolean(dsRawData.Tables[0].Rows[i]["ProcessedFlag"].ToString()),
                            AddedBy = dsRawData.Tables[0].Rows[i]["AddedBy"].ToString(),
                            ModelState = ModelState.Modified
                        };
                        AttdnRawDataList.Add(ob);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitAttdnProcessData(DataSet dsAttnProcData, out List<AttdnProcessData> AttdnRawDataList)
        {
            AttdnRawDataList = new List<AttdnProcessData>();
            try
            {
                for (var i = 0; i < dsAttnProcData.Tables[0].Rows.Count; i++)
                {
                    if (dsAttnProcData.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                    }
                    else
                    {
                        var ob = new AttdnProcessData
                        {
                            AddedBy = dsAttnProcData.Tables[0].Rows[i]["AddedBy"].ToString(),
                            ModelState = ModelState.Modified
                        };
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