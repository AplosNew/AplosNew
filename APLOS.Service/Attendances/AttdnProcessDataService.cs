#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Service.Biometrics;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

#endregion Using

namespace Library.Service.Attendances
{
    public class AttdnProcessDataService : Service<AttdnProcessData>, IAttdnProcessDataService
    {
       
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;//
        private readonly IAttdnRawDataService _rs;//
        private readonly IEmployeeNotificationsService _en;//
        private readonly ILeaveAllocationService _la;//
        private readonly ILeaveTransactionDetailsService _ltd;//
        private readonly IEmpDateWiseShiftAssignService _eds;//IEmpDateWiseShiftAssignService
        private string sAttnDate = DateTime.Now.ToString("dd-MMM-yyyy");
        private string sEmpSystemIDColl = string.Empty;
        private string lblAttdnProcBase = string.Empty;
        private bool radDwLdEnrollID;

        public AttdnProcessDataService(
             IRepositoryAsync<AttdnProcessData> attdnProcessDataRepository
            , IPKGeneratorService pkGeneratorService
            , IAttdnRawDataService rs
            , IEmployeeNotificationsService en
            , ILeaveAllocationService la
            , ILeaveTransactionDetailsService ltd
            , IEmpDateWiseShiftAssignService eds
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(attdnProcessDataRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pk = pkGeneratorService;
            _rs = rs;
            _en = en;
            _la = la;
            _ltd = ltd;
            _eds = eds;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
        public IWorkbook AttndReport(string fromDate, string toDate, string companyGroupId ,string companyId, string plantId)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                //DataSet dsLocal = GetJobCardInfo(employeeId, fromDate, toDate);

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_Attendance(ref sheet1, oRU, "Attendance ", "Attendance",  fromDate,  toDate, companyGroupId,companyId ,plantId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        private void CreateSheet_Attendance (ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string SheetName, string fromDate, string toDate, string companyGroupId, string companyId, string plantId)
        {
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            try
            {
                var dtEmp = GetEmpAttdcInfo(fromDate, toDate, companyGroupId, companyId, plantId);

                xlsRow = 4;

                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    xlsRow += 1;
                    int c_ec = 0;
                    int E_Name = 0;
                    int E_FName = 0;
                    int E_DJ = 0;
                    int E_WD = 0;
                    int E_DS = 0;
                    
                  
                  
                    sheet1.Range[5, xlsCol].RowHeight = 20;
                    c_ec = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Code"); xlsCol += 1;
                    E_Name = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Name"); xlsCol += 1;
                    E_FName =xlsCol;
                   oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, " Father Name"); xlsCol += 1;
                    E_DJ = xlsCol;
                   oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOJ"); xlsCol += 1;
                    E_WD = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ToDay"); xlsCol += 1;
                    E_DS = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "YesterDay"); 
                  
                    

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightGreen;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header-----------------

                    for (int i = 0; i < dtEmp.Rows.Count; i++)//e lt
                    {
                        #region --------data----------
                        xlsRow += 1;
                        xlsCol = 1;
                        oRU.SetCellText(sheet1, xlsRow, c_ec, dtEmp.Rows[i]["EmployeeCode"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_Name, dtEmp.Rows[i]["EmployeeName"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_FName, dtEmp.Rows[i]["FatherName"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_DJ, dtEmp.Rows[i]["DOJ"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_WD, dtEmp.Rows[i]["ToDay"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_DS, dtEmp.Rows[i]["Yestarday"].ToString());


                        #endregion --------data----------
                    }// emp + ltype
                    xlsCol = 2;
                    xlsRow += 5;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.UsedRange.WrapText = true;
                    sheet1.Name = SheetName;

                    sheet1.Range[xlsRow, xlsCol].NumberFormat = oRU.NumberFormatDecimalTwo();

                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].Merge();
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                    oRU.CompanyGroupHeader(ref sheet1, endXlsCol, "Absent Employee List",companyGroupId);
                    oRU.PageSetup(ref sheet1, 4, ExcelPageOrientation.Portrait);


                    #region UsedRange Alignment
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetEmpAttdcInfo(string fromDate, string toDate, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                var todayDate = DateTime.Now.ToString("dd-MMM-yyyy");
                var preDate = Convert.ToDateTime(todayDate).AddDays(-1).ToString("dd-MMM-yyyy");

                var sql = @"select e.SystemId,e.EmployeeCode,REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
                                    ,REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS
                                    ,e.EmployeeName,E.FatherName,
							isnull(t.DS,'') Today,isnull (p.DS ,'')Yestarday
							 from EmployeeInformation e
							left join (
																	select * from 
										(
										select em.EmpSystemID,d.DS from
										(
										select * from AttdnProcessData  WHERE DayStatus ='A' AND WorkDate = '" + todayDate+@"'
										) em
										 left join (
													 select EmpSystemID,COUNT(DayStatus) DS from AttdnProcessData 
													 where WorkDate between '"+fromDate+ @"' and '" + toDate + @"'
													 and DayStatus='A'
													 group by EmpSystemID
													) d on d.EmpSystemID=em.EmpSystemID
								) t1
							) t on t.EmpSystemID=e.SystemId

						left join (
																	select * from 
										(
										select em.EmpSystemID,d.DS from
										(
										select * from AttdnProcessData  WHERE DayStatus ='A' AND WorkDate ='" + preDate + @"'
										--yestarday absent make sure today it is not absent
										and EmpSystemID not in
										(
										select EmpSystemID from AttdnProcessData  WHERE DayStatus ='A' AND WorkDate = '" + todayDate + @"'
										)
										) em
										 left join (
													 select EmpSystemID,COUNT(DayStatus) DS from AttdnProcessData 
													 where WorkDate between '" + fromDate + @"' and '" + toDate + @"'
													 and DayStatus='A'
													 group by EmpSystemID
													) d on d.EmpSystemID=em.EmpSystemID
								) t1
							) p
							on p.EmpSystemID=e.SystemId where t.DS is not null or p.DS is not null";

                var list = _sqlRepository.GetDataTable(sql);
                
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string GetPK()
        {
            return _pk.GetAutoNumber(nameof(AttdnProcessData), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private IEnumerable<AttdnProcessData> LoadAttdnProcessData(string sPlantid, string sDevSystemID, string sMinDate, string sMaxDate)//TBT
        {
            try
            {
                var _sql = @"SELECT * FROM AttdnProcessData
                            WHERE PlantID = '" + sPlantid + @"' AND DevSystemID = '" + sDevSystemID + @"'
                                  AND PDate BETWEEN '" + sMinDate + @"' AND '" + sMaxDate + @"'";
                return _sqlRepository.GetModelCollection<AttdnProcessData>(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void InitData(string plantid, string deviceid, string sMinDate, string sMaxDate, string groupid, List<AttdnProcessData> from_ui, out List<AttdnProcessData> from_db)
        {
            from_db = null;
            try
            {
                var _pks = GetPK();
                from_db = LoadAttdnProcessData(plantid, deviceid, sMinDate, sMaxDate).ToList<AttdnProcessData>();
                var _count = 0;
                foreach (var ui in from_ui)
                {
                    //dvAttnRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "' AND PDate = '" + sDate + "' AND PTime >= '" + dtTime.AddSeconds(-10) + "' AND PTime <= '" + dtTime + "'";
                    var db = from_db.FirstOrDefault(a => a.GroupID == ui.GroupID);
                    if (db == null)
                    {
                        _count++;
                        db = new AttdnProcessData
                        {
                            AddedBy = ui.AddedBy,
                            // db.DateAdded = DateTime.Now;
                            // db.GroupId = groupid;
                            // db.Id = "R" + _pks + "-" + _count;
                            //db.RowId = ui.RowId;
                            //db.DateUpdated = DateTime.Now;
                            UpdatedBy = ui.UpdatedBy,
                            ModelState = ModelState.Added
                        };
                        from_db.Add(db);
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveAttdnRawData(string plantid, string deviceid, string sMinDate, string sMaxDate, string groupid, List<AttdnProcessData> fromui)
        {
            List<AttdnProcessData> from_db = null;
            var flag = false;
            try
            {
                InitData(plantid, deviceid, sMinDate, sMaxDate, groupid, fromui, out from_db);
                foreach (var item in from_db)
                {
                    InsertOrUpdateGraph(item);
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SaveTotal(string _plantid)
        {
            var GroupSysID = string.Empty;
            try
            {
                PlantNameAndHRMSLocation(_plantid, out string strYrSystemID, out string strYrFromDate, out string strYrToDate, out GroupSysID);
                AttdnProcBaseOn(GroupSysID, _plantid);
                ShiftProcess(_plantid, GroupSysID);
                xAttdnDateProcessForInData(GroupSysID, _plantid, strYrSystemID, radDwLdEnrollID, strYrFromDate, strYrToDate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void PlantNameAndHRMSLocation(string _plantid, out string strYrSystemID, out string strYrFromDate, out string strYrToDate, out string GroupSysID)
        {
            DataSet dsLocal = null;
            DataSet dsYrCal = null;
            strYrSystemID = string.Empty;
            strYrFromDate = string.Empty;
            strYrToDate = string.Empty;
            GroupSysID = string.Empty;
            try
            {
                //clsRegister objReg;
                //objReg = new clsRegister();

                dsLocal = GetPlantInformation(_plantid);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    GroupSysID = dsLocal.Tables[0].Rows[0]["CompanyGroupId"].ToString().Trim();
                }

                //objReg.GetCompanyAndGroupID(_plantid, out dsCmpIDGrpID);
                //if (dsCmpIDGrpID.Tables[0].Rows.Count > 0)
                //{
                //objReg.GetHRMSLocation(this.lblPlantID.Text.Trim(), out dsComSerIP);
                //if (dsComSerIP.Tables[0].Rows.Count > 0)
                //{
                //    CompServerIP = dsComSerIP.Tables[0].Rows[0]["HRMSSERVER"].ToString().Trim();
                //    CompHRDB = dsComSerIP.Tables[0].Rows[0]["HRMSDB"].ToString().Trim();
                //    bSameServer = Convert.ToBoolean(dsComSerIP.Tables[0].Rows[0]["SameServer"].ToString().Trim());
                //}

                dsYrCal = GetYearlyCalendar(GroupSysID, _plantid, sAttnDate.Trim());
                if (dsYrCal.Tables[0].Rows.Count > 0)
                {
                    strYrSystemID = dsYrCal.Tables[0].Rows[0]["SystemID"].ToString();
                    strYrFromDate = Convert.ToDateTime(dsYrCal.Tables[0].Rows[0]["FromDate"]).ToString("dd-MMM-yyyy");
                    strYrToDate = Convert.ToDateTime(dsYrCal.Tables[0].Rows[0]["ToDate"]).ToString("dd-MMM-yyyy");
                }

                //objReg.GetPlantWiseHRMSSetting(GroupSysID.Trim(), this.lblPlantID.Text.Trim(), out dsLocal);
                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    sMinOT = dsLocal.Tables[0].Rows[0]["MinimumOTMinute"].ToString().Trim();
                //    sFractionCalculate = dsLocal.Tables[0].Rows[0]["OTFractionCalculation"].ToString().Trim();
                //    sOTConsiderOn = dsLocal.Tables[0].Rows[0]["OTConsiderOn"].ToString().Trim();
                //}
                //else
                //{
                //    sMinOT = "";
                //    sFractionCalculate = "";
                //    sOTConsiderOn = "";
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }//End Function

        private void ShiftProcess(string _plantid, string GroupSysID)
        {
            #region DataSet Declare

            DataSet dsEmpInfoForShiftProc = null;

            DataSet dsEmpDtWiseSftAss = null;
            DataTable dtEmpDtWiseSftAss = null;
            DataRow drEmpDtWiseSftAss = null;
            DataView dvEmpDtWiseSftAss = null;

            DataSet dsEmpWkOff = null;
            DataTable dtEmpWkOff = null;
            DataView dvEmpWkOff = null;

            DataSet dsComAssWkOff = null;
            DataTable dtComAssWkOff = null;
            DataView dvComAssWkOff = null;

            DataSet dsDayType = null;
            DataTable dtDayType = null;
            DataView dvDayType = null;

            DataSet dsSftRstDayCnt = null;
            DataTable dtSftRstDayCnt = null;
            DataView dvSftRstDayCnt = null;

            DataSet dsEmpSftAssBfrFmDt = null;
            DataTable dtEmpSftAssBfrFmDt = null;
            DataView dvEmpSftAssBfrFmDt = null;

            DataSet dsEmpSftAss = null;
            DataTable dtEmpSftAss = null;
            DataView dvEmpSftAss = null;

            DataSet dsSftRstCdl = null;
            DataTable dtSftRstCdl = null;
            DataView dvSftRstCdl = null;

            DataSet dsAttdnProc = null;
            DataTable dtAttdnProc = null;
            DataView dvAttdnProc = null;
            DataRow drAttdnProc = null;

            #endregion DataSet Declare

            //clsRegister objReg = null;
            //objReg = new clsRegister();

            try
            {
                dsEmpInfoForShiftProc = GetEmployeeInformationForShiftProcess(_plantid, sAttnDate.Trim());

                if (dsEmpInfoForShiftProc.Tables[0].Rows.Count > 0)
                {
                    var sEmpSysIDCollForSft = "";

                    for (int i = 0; i < dsEmpInfoForShiftProc.Tables[0].Rows.Count; i++)
                    {
                        sEmpSysIDCollForSft = sEmpSysIDCollForSft.Trim() == "" ? "'" + dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'" : sEmpSysIDCollForSft.Trim() + ", '" + dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                    }

                    #region DataSet

                    dsDayType = GetDayType();
                    dtDayType = dsDayType.Tables[0];
                    using (dvDayType = new DataView())
                    {
                        dsEmpDtWiseSftAss = GetEmpDateWiseShiftAssign(sEmpSysIDCollForSft.Trim());
                        dtEmpDtWiseSftAss = dsEmpDtWiseSftAss.Tables[0];
                        using (dvEmpDtWiseSftAss = new DataView())
                        {
                            dsEmpWkOff = GetEmployeeWeekOffByDay(sEmpSysIDCollForSft.Trim());
                            dtEmpWkOff = dsEmpWkOff.Tables[0];
                            using (dvEmpWkOff = new DataView())
                            {
                                dsComAssWkOff = GetCompanyAssignWeekOffDateRangeWise(GroupSysID.Trim(), sAttnDate.Trim());
                                dtComAssWkOff = dsComAssWkOff.Tables[0];
                                using (dvComAssWkOff = new DataView())
                                {
                                    var dtLastDt = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
                                    dsEmpSftAssBfrFmDt = GetUpdatedEmpShiftAssignBeforeFromDate(sEmpSysIDCollForSft.Trim(), sAttnDate.Trim());
                                    dtEmpSftAssBfrFmDt = dsEmpSftAssBfrFmDt.Tables[0];
                                    using (dvEmpSftAssBfrFmDt = new DataView())
                                    {
                                        dsSftRstDayCnt = GetSftRstDayCount(sEmpSysIDCollForSft.Trim(), dtLastDt.Trim(), sAttnDate.Trim());
                                        dtSftRstDayCnt = dsSftRstDayCnt.Tables[0];
                                        using (dvSftRstDayCnt = new DataView())
                                        {
                                            dsEmpSftAss = GetEmployeeShiftAssignInDateRange(sEmpSysIDCollForSft.Trim(), sAttnDate.Trim());
                                            dtEmpSftAss = dsEmpSftAss.Tables[0];
                                            using (dvEmpSftAss = new DataView())
                                            {
                                                dsSftRstCdl = GetShiftRosterChild(GroupSysID.Trim());
                                                dtSftRstCdl = dsSftRstCdl.Tables[0];
                                                using (dvSftRstCdl = new DataView())
                                                {
                                                    dsAttdnProc = GetAttdnProcessData(sEmpSysIDCollForSft.Trim(), sAttnDate.Trim());
                                                    dtAttdnProc = dsAttdnProc.Tables[0];
                                                    using (dvAttdnProc = new DataView())
                                                    {
                                                        #endregion DataSet

                                                        for (int i = 0; i < dsEmpInfoForShiftProc.Tables[0].Rows.Count; i++)
                                                        {
                                                            #region Declare Variable

                                                            var ShiftDays = 0;
                                                            var ShiftSequence = 0;

                                                            var sEmpSystemID = dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                                                            var sPlantID = dsEmpInfoForShiftProc.Tables[0].Rows[i]["PlantID"].ToString().Trim();

                                                            var RosterShiftDayCount = 0;
                                                            var RosterShiftSequence = 0;
                                                            var RosterMstSysID = "";
                                                            var RosterChlSftSysID = "";
                                                            var RosterChlNewSftSysID = "";
                                                            var bInitialRstSftDyCnt = false;

                                                            var bAlignWithCC = false;
                                                            var bIndividualWeekOff = false;

                                                            var sFstOffDay = "";
                                                            var sFstDayLengthType = "";
                                                            var sSndOffDay = "";
                                                            var sSndDayLengthType = "";

                                                            var sDayType = "NW";
                                                            var sDayLengthType = "Normal Workday";

                                                            var iWeekOffDayInRoster = 0;

                                                            #endregion Declare Variable

                                                            var strStDt = sAttnDate.Trim();
                                                            var dtStDt = Convert.ToDateTime(strStDt);

                                                            bAlignWithCC = false;
                                                            bIndividualWeekOff = false;

                                                            sFstOffDay = "";
                                                            sFstDayLengthType = "";
                                                            sSndOffDay = "";
                                                            sSndDayLengthType = "";

                                                            sDayType = "NW";
                                                            sDayLengthType = "Normal Workday";

                                                            dvEmpDtWiseSftAss.Table = dtEmpDtWiseSftAss;
                                                            dvEmpDtWiseSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + strStDt + "'";

                                                            if (dvEmpDtWiseSftAss.Count > 0)
                                                            {
                                                                #region EmpSystemID and WorkDate are already available in the table 'EmpDateWiseShiftAssign'

                                                                //Check in the table 'EmpDateWiseShiftAssign' the field 'AttdnLock' is not true
                                                                if (!Convert.ToBoolean(dvEmpDtWiseSftAss[0]["AttdnLock"].ToString().Trim()))
                                                                {
                                                                    dvEmpSftAss.Table = dtEmpSftAss;
                                                                    dvEmpSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";

                                                                    if (dvEmpSftAss.Count == 0)
                                                                    {
                                                                        #region FromDate & Shift start Date Same and After fromdate to todate not found shift assignment

                                                                        #region Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                                                        dvEmpSftAssBfrFmDt.Table = dtEmpSftAssBfrFmDt;
                                                                        dvEmpSftAssBfrFmDt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                                                                        if (dvEmpSftAssBfrFmDt.Count > 0)
                                                                        {
                                                                            if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsFix"].ToString().Trim()))
                                                                            {
                                                                                #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                                dvEmpWkOff.Table = dtEmpWkOff;
                                                                                dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim() + "'";
                                                                                if (dvEmpWkOff.Count > 0)
                                                                                {
                                                                                    bAlignWithCC = Convert.ToBoolean(dvEmpWkOff[0]["AlignWithCC"].ToString().Trim());
                                                                                    bIndividualWeekOff = Convert.ToBoolean(dvEmpWkOff[0]["IndividualWeekOff"].ToString().Trim());

                                                                                    sFstOffDay = dvEmpWkOff[0]["FstOffDay"].ToString().Trim();
                                                                                    sFstDayLengthType = dvEmpWkOff[0]["FstDayLengthType"].ToString().Trim();
                                                                                    sSndOffDay = dvEmpWkOff[0]["SndOffDay"].ToString().Trim();
                                                                                    sSndDayLengthType = dvEmpWkOff[0]["SndDayLengthType"].ToString().Trim();
                                                                                }

                                                                                #endregion Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                                #region If Employee's week off align with company calendar

                                                                                if (bAlignWithCC)
                                                                                {
                                                                                    dvComAssWkOff.Table = dtComAssWkOff;
                                                                                    dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                                                    if (dvComAssWkOff.Count > 0)
                                                                                    {
                                                                                        sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                                                        if (sDayLengthType == "Full Day")
                                                                                        {
                                                                                            sDayType = "W";
                                                                                            sDayLengthType = "Week Off";
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            dvDayType.Table = dtDayType;
                                                                                            dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                            if (dvDayType.Count > 0)
                                                                                            {
                                                                                                sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }

                                                                                #endregion If Employee's week off align with company calendar

                                                                                #region If Employee's week off align Individualy

                                                                                if (bIndividualWeekOff)
                                                                                {
                                                                                    if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                                                    {
                                                                                        sDayLengthType = sFstDayLengthType;
                                                                                        if (sDayLengthType == "Full Day")
                                                                                        {
                                                                                            sDayType = "W";
                                                                                            sDayLengthType = "Week Off";
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            dvDayType.Table = dtDayType;
                                                                                            dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                            if (dvDayType.Count > 0)
                                                                                            {
                                                                                                sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    if (sSndOffDay == (dtStDt.DayOfWeek).ToString())
                                                                                    {
                                                                                        sDayLengthType = sSndDayLengthType;
                                                                                        if (sDayLengthType == "Full Day")
                                                                                        {
                                                                                            sDayType = "W";
                                                                                            sDayLengthType = "Week Off";
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            dvDayType.Table = dtDayType;
                                                                                            dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                            if (dvDayType.Count > 0)
                                                                                            {
                                                                                                sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }

                                                                                #endregion If Employee's week off align Individualy

                                                                                #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'

                                                                                drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                                                drEmpDtWiseSftAss.BeginEdit();

                                                                                drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim();
                                                                                drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim();

                                                                                drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                                                drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                                drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                                drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                                drEmpDtWiseSftAss.EndEdit();

                                                                                #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                                            }
                                                                            else if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsRoster"].ToString().Trim()))
                                                                            {
                                                                                #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                                                                //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                                                                RosterMstSysID = dvEmpSftAssBfrFmDt[0]["RosterSystemID"].ToString().Trim();

                                                                                //Take Last date 'ShiftSystemID' and 'RosterShiftDayCount' from the table 'EmpDateWiseShiftAssign'

                                                                                dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                                                dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtLastDt + "'";
                                                                                if (dvSftRstDayCnt.Count > 0)
                                                                                {
                                                                                    RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0][nameof(RosterShiftDayCount)].ToString().Trim());
                                                                                    RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                                                    bInitialRstSftDyCnt = true;
                                                                                }
                                                                                else if (!bInitialRstSftDyCnt)
                                                                                {
                                                                                    RosterShiftDayCount = Convert.ToInt32(dvEmpSftAssBfrFmDt[0]["StartFromDay"].ToString().Trim()) - 1;
                                                                                    RosterChlSftSysID = dvEmpSftAssBfrFmDt[0]["RosterStartShiftID"].ToString().Trim();
                                                                                    bInitialRstSftDyCnt = true;
                                                                                }

                                                                                //Set Roster Child Shift SystemID For Current Date in loop
                                                                                dvSftRstCdl.Table = dtSftRstCdl;
                                                                                dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                                                                if (dvSftRstCdl.Count > 0)
                                                                                {
                                                                                    #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                                                    for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                    {// RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                                                        if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                                                        {
                                                                                            ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                            ShiftDays = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftDays)].ToString().Trim());
                                                                                            iWeekOffDayInRoster = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffDayInRoster"].ToString().Trim());
                                                                                        }
                                                                                    }

                                                                                    //Check RosterShiftDayCount & ShiftDays
                                                                                    if (RosterShiftDayCount >= ShiftDays)
                                                                                    {
                                                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                        {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                                            if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                            {
                                                                                                RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                            }
                                                                                        }
                                                                                        if (RosterShiftSequence == 0)
                                                                                        {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                                            RosterShiftSequence = 1;
                                                                                        }
                                                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                        {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                                            if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                            {
                                                                                                RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                                                RosterChlSftSysID = RosterChlNewSftSysID;
                                                                                                RosterShiftDayCount = 0;
                                                                                                RosterShiftSequence = 0;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                                                        RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                                                    }

                                                                                    #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                                                }

                                                                                //Update RosterShiftDayCount
                                                                                RosterShiftDayCount = RosterShiftDayCount + 1;

                                                                                if (iWeekOffDayInRoster == RosterShiftDayCount)
                                                                                {
                                                                                    sDayType = "W";
                                                                                    sDayLengthType = "Week Off";
                                                                                }

                                                                                drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                                                drEmpDtWiseSftAss.BeginEdit();

                                                                                drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim();
                                                                                drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();
                                                                                drEmpDtWiseSftAss[nameof(RosterShiftDayCount)] = RosterShiftDayCount;

                                                                                drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                                                drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                                drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                                drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                                drEmpDtWiseSftAss.EndEdit();

                                                                                #endregion If Last updated shift in table 'EmployeeShiftAssign' is roster
                                                                            }
                                                                        }

                                                                        #endregion Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                                                        #endregion FromDate & Shift start Date Same and After fromdate to todate not found shift assignment
                                                                    }
                                                                    else if (dvEmpSftAss.Count > 0)
                                                                    {
                                                                        var strActuEffDt = "";
                                                                        var strActuEffDtTmp = "";

                                                                        if (dvEmpSftAss.Count > 1)
                                                                        {
                                                                            for (int efDt = 0; efDt < dvEmpSftAss.Count; efDt++)
                                                                            {
                                                                                if (Convert.ToDateTime(dvEmpSftAss[efDt]["EffectiveDate"].ToString().Trim()) <= Convert.ToDateTime(strStDt))
                                                                                {
                                                                                    strActuEffDtTmp = dvEmpSftAss[efDt]["EffectiveDate"].ToString().Trim();
                                                                                }
                                                                                if (strActuEffDt == "")
                                                                                { strActuEffDt = strActuEffDtTmp; }

                                                                                if (Convert.ToDateTime(strActuEffDtTmp) > Convert.ToDateTime(strActuEffDt))
                                                                                {
                                                                                    strActuEffDt = strActuEffDtTmp;
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            strActuEffDt = dvEmpSftAss[0]["EffectiveDate"].ToString().Trim();
                                                                        }

                                                                        #region Shift start Date is great than FromDate

                                                                        for (int efDt = 0; efDt < dvEmpSftAss.Count; efDt++)
                                                                        {
                                                                            if (Convert.ToDateTime(dvEmpSftAss[efDt]["EffectiveDate"].ToString().Trim()) == Convert.ToDateTime(strActuEffDt))
                                                                            {
                                                                                #region Check Last updated shift in table 'EmployeeShiftAssign' after fromdate

                                                                                if (Convert.ToBoolean(dvEmpSftAss[efDt]["IsFix"].ToString().Trim()))
                                                                                {
                                                                                    #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                                    dvEmpWkOff.Table = dtEmpWkOff;
                                                                                    dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAss[0]["FixSystemID"].ToString().Trim() + "'";
                                                                                    if (dvEmpWkOff.Count > 0)
                                                                                    {
                                                                                        bAlignWithCC = Convert.ToBoolean(dvEmpWkOff[0]["AlignWithCC"].ToString().Trim());
                                                                                        bIndividualWeekOff = Convert.ToBoolean(dvEmpWkOff[0]["IndividualWeekOff"].ToString().Trim());

                                                                                        sFstOffDay = dvEmpWkOff[0]["FstOffDay"].ToString().Trim();
                                                                                        sFstDayLengthType = dvEmpWkOff[0]["FstDayLengthType"].ToString().Trim();
                                                                                        sSndOffDay = dvEmpWkOff[0]["SndOffDay"].ToString().Trim();
                                                                                        sSndDayLengthType = dvEmpWkOff[0]["SndDayLengthType"].ToString().Trim();
                                                                                    }

                                                                                    #endregion Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                                    #region If Employee's week off align with company calendar

                                                                                    if (bAlignWithCC)
                                                                                    {
                                                                                        dvComAssWkOff.Table = dtComAssWkOff;
                                                                                        dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                                                        if (dvComAssWkOff.Count > 0)
                                                                                        {
                                                                                            sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                                                            if (sDayLengthType == "Full Day")
                                                                                            {
                                                                                                sDayType = "W";
                                                                                                sDayLengthType = "Week Off";
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                dvDayType.Table = dtDayType;
                                                                                                dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                                if (dvDayType.Count > 0)
                                                                                                {
                                                                                                    sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    #endregion If Employee's week off align with company calendar

                                                                                    #region If Employee's week off align Individualy

                                                                                    if (bIndividualWeekOff)
                                                                                    {
                                                                                        if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                                                        {
                                                                                            sDayLengthType = sFstDayLengthType;
                                                                                            if (sDayLengthType == "Full Day")
                                                                                            {
                                                                                                sDayType = "W";
                                                                                                sDayLengthType = "Week Off";
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                dvDayType.Table = dtDayType;
                                                                                                dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                                if (dvDayType.Count > 0)
                                                                                                {
                                                                                                    sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                                }
                                                                                            }
                                                                                        }

                                                                                        if (sSndOffDay == (dtStDt.DayOfWeek).ToString())
                                                                                        {
                                                                                            sDayLengthType = sSndDayLengthType;
                                                                                            if (sDayLengthType == "Full Day")
                                                                                            {
                                                                                                sDayType = "W";
                                                                                                sDayLengthType = "Week Off";
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                dvDayType.Table = dtDayType;
                                                                                                dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                                if (dvDayType.Count > 0)
                                                                                                {
                                                                                                    sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    #endregion If Employee's week off align Individualy

                                                                                    #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'

                                                                                    drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                                                    drEmpDtWiseSftAss.BeginEdit();

                                                                                    drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[efDt]["SystemID"].ToString().Trim();
                                                                                    drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAss[efDt]["FixSystemID"].ToString().Trim();

                                                                                    drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                                                    drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                                    drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                                    drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                                    drEmpDtWiseSftAss.EndEdit();

                                                                                    #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                                                }
                                                                                else if (Convert.ToBoolean(dvEmpSftAss[efDt]["IsRoster"].ToString().Trim()))
                                                                                {
                                                                                    #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                                                                    //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                                                                    RosterMstSysID = dvEmpSftAss[efDt]["RosterSystemID"].ToString().Trim();
                                                                                    var strEmpSftAssiSystemID = dvEmpSftAss[efDt]["SystemID"].ToString().Trim();

                                                                                    dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                                                    dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EmpSftAssiSystemID = '" + strEmpSftAssiSystemID + "' AND WorkDate = '" + dtLastDt + "'";
                                                                                    if (dvSftRstDayCnt.Count > 0)
                                                                                    {
                                                                                        RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0][nameof(RosterShiftDayCount)].ToString().Trim());
                                                                                        RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                                                        bInitialRstSftDyCnt = true;
                                                                                    }
                                                                                    else if (!bInitialRstSftDyCnt)
                                                                                    {
                                                                                        RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[efDt]["StartFromDay"].ToString().Trim()) - 1;
                                                                                        RosterChlSftSysID = dvEmpSftAss[efDt]["RosterStartShiftID"].ToString().Trim();
                                                                                        bInitialRstSftDyCnt = true;
                                                                                    }

                                                                                    //Set Roster Child Shift SystemID For Current Date in loop
                                                                                    dvSftRstCdl.Table = dtSftRstCdl;
                                                                                    dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                                                                    if (dvSftRstCdl.Count > 0)
                                                                                    {
                                                                                        #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                        {// RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                                                            if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                                                            {
                                                                                                ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                                ShiftDays = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftDays)].ToString().Trim());
                                                                                                iWeekOffDayInRoster = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffDayInRoster"].ToString().Trim());
                                                                                            }
                                                                                        }

                                                                                        //Check RosterShiftDayCount & ShiftDays
                                                                                        if (RosterShiftDayCount >= ShiftDays)
                                                                                        {
                                                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                            {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                                                if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                                {
                                                                                                    RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                                }
                                                                                            }
                                                                                            if (RosterShiftSequence == 0)
                                                                                            {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                                                RosterShiftSequence = 1;
                                                                                            }
                                                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                            {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                                                if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                                {
                                                                                                    RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                                                    RosterChlSftSysID = RosterChlNewSftSysID;
                                                                                                    RosterShiftDayCount = 0;
                                                                                                    RosterShiftSequence = 0;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        else
                                                                                        {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                                                            RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                                                        }

                                                                                        #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                                                    }

                                                                                    //Update RosterShiftDayCount
                                                                                    RosterShiftDayCount = RosterShiftDayCount + 1;

                                                                                    if (iWeekOffDayInRoster == RosterShiftDayCount)
                                                                                    {
                                                                                        sDayType = "W";
                                                                                        sDayLengthType = "Week Off";
                                                                                    }

                                                                                    drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                                                    drEmpDtWiseSftAss.BeginEdit();

                                                                                    drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[efDt]["SystemID"].ToString().Trim();
                                                                                    drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();
                                                                                    drEmpDtWiseSftAss[nameof(RosterShiftDayCount)] = RosterShiftDayCount;

                                                                                    drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                                                    drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                                    drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                                    drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                                    drEmpDtWiseSftAss.EndEdit();

                                                                                    #endregion If Last updated shift in table 'EmployeeShiftAssign' is roster
                                                                                }

                                                                                #endregion Check Last updated shift in table 'EmployeeShiftAssign' after fromdate
                                                                            }
                                                                        }

                                                                        #endregion Shift start Date is great than FromDate
                                                                    }
                                                                }

                                                                #endregion EmpSystemID and WorkDate are already available in the table 'EmpDateWiseShiftAssign'
                                                            }
                                                            else
                                                            {
                                                                #region EmpSystemID and WorkDate not found in the table 'EmpDateWiseShiftAssign'

                                                                dvEmpSftAss.Table = dtEmpSftAss;
                                                                dvEmpSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";

                                                                if (dvEmpSftAss.Count == 0)
                                                                {
                                                                    #region FromDate & Shift start Date Same and After fromdate to todate not found shift assignment

                                                                    #region Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                                                    dvEmpSftAssBfrFmDt.Table = dtEmpSftAssBfrFmDt;
                                                                    dvEmpSftAssBfrFmDt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                                                                    if (dvEmpSftAssBfrFmDt.Count > 0)
                                                                    {
                                                                        if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsFix"].ToString().Trim()))
                                                                        {
                                                                            #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                            dvEmpWkOff.Table = dtEmpWkOff;
                                                                            dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim() + "'";
                                                                            if (dvEmpWkOff.Count > 0)
                                                                            {
                                                                                bAlignWithCC = Convert.ToBoolean(dvEmpWkOff[0]["AlignWithCC"].ToString().Trim());
                                                                                bIndividualWeekOff = Convert.ToBoolean(dvEmpWkOff[0]["IndividualWeekOff"].ToString().Trim());

                                                                                sFstOffDay = dvEmpWkOff[0]["FstOffDay"].ToString().Trim();
                                                                                sFstDayLengthType = dvEmpWkOff[0]["FstDayLengthType"].ToString().Trim();
                                                                                sSndOffDay = dvEmpWkOff[0]["SndOffDay"].ToString().Trim();
                                                                                sSndDayLengthType = dvEmpWkOff[0]["SndDayLengthType"].ToString().Trim();
                                                                            }

                                                                            #endregion Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                            #region If Employee's week off align with company calendar

                                                                            if (bAlignWithCC)
                                                                            {
                                                                                dvComAssWkOff.Table = dtComAssWkOff;
                                                                                dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                                                if (dvComAssWkOff.Count > 0)
                                                                                {
                                                                                    sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                                                    if (sDayLengthType == "Full Day")
                                                                                    {
                                                                                        sDayType = "W";
                                                                                        sDayLengthType = "Week Off";
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        dvDayType.Table = dtDayType;
                                                                                        dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                        if (dvDayType.Count > 0)
                                                                                        {
                                                                                            sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }

                                                                            #endregion If Employee's week off align with company calendar

                                                                            #region If Employee's week off align Individualy

                                                                            if (bIndividualWeekOff)
                                                                            {
                                                                                if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                                                {
                                                                                    sDayLengthType = sFstDayLengthType;
                                                                                    if (sDayLengthType == "Full Day")
                                                                                    {
                                                                                        sDayType = "W";
                                                                                        sDayLengthType = "Week Off";
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        dvDayType.Table = dtDayType;
                                                                                        dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                        if (dvDayType.Count > 0)
                                                                                        {
                                                                                            sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                        }
                                                                                    }
                                                                                }

                                                                                if (sSndOffDay == (dtStDt.DayOfWeek).ToString())
                                                                                {
                                                                                    sDayLengthType = sSndDayLengthType;
                                                                                    if (sDayLengthType == "Full Day")
                                                                                    {
                                                                                        sDayType = "W";
                                                                                        sDayLengthType = "Week Off";
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        dvDayType.Table = dtDayType;
                                                                                        dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                        if (dvDayType.Count > 0)
                                                                                        {
                                                                                            sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }

                                                                            #endregion If Employee's week off align Individualy

                                                                            #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'

                                                                            drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                                            drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                                            drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                                            drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim();
                                                                            drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim();

                                                                            drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                                            drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                                            drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                                            drEmpDtWiseSftAss[nameof(RosterShiftDayCount)] = 0;
                                                                            drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                                            drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                            drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                                            drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                                            drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                            drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                            dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);

                                                                            #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                                        }
                                                                        else if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsRoster"].ToString().Trim()))
                                                                        {
                                                                            #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                                                            //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                                                            RosterMstSysID = dvEmpSftAssBfrFmDt[0]["RosterSystemID"].ToString().Trim();

                                                                            dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                                            dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtLastDt + "'";
                                                                            if (dvSftRstDayCnt.Count > 0)
                                                                            {
                                                                                RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0][nameof(RosterShiftDayCount)].ToString().Trim());
                                                                                RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                                                bInitialRstSftDyCnt = true;
                                                                            }
                                                                            else if (!bInitialRstSftDyCnt)
                                                                            {
                                                                                RosterShiftDayCount = Convert.ToInt32(dvEmpSftAssBfrFmDt[0]["StartFromDay"].ToString().Trim()) - 1;
                                                                                RosterChlSftSysID = dvEmpSftAssBfrFmDt[0]["RosterStartShiftID"].ToString().Trim();
                                                                                bInitialRstSftDyCnt = true;
                                                                            }

                                                                            //Set Roster Child Shift SystemID For Current Date in loop
                                                                            dvSftRstCdl.Table = dtSftRstCdl;
                                                                            dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                                                            if (dvSftRstCdl.Count > 0)
                                                                            {
                                                                                #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                {// RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                                                    if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                                                    {
                                                                                        ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                        ShiftDays = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftDays)].ToString().Trim());
                                                                                        iWeekOffDayInRoster = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffDayInRoster"].ToString().Trim());
                                                                                    }
                                                                                }

                                                                                //Check RosterShiftDayCount & ShiftDays
                                                                                if (RosterShiftDayCount >= ShiftDays)
                                                                                {
                                                                                    for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                    {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                                        if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                        {
                                                                                            RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                        }
                                                                                    }
                                                                                    if (RosterShiftSequence == 0)
                                                                                    {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                                        RosterShiftSequence = 1;
                                                                                    }
                                                                                    for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                    {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                                        if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                        {
                                                                                            RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                                            RosterChlSftSysID = RosterChlNewSftSysID;
                                                                                            RosterShiftDayCount = 0;
                                                                                            RosterShiftSequence = 0;
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                                                    RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                                                }

                                                                                #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                                            }

                                                                            //Update RosterShiftDayCount
                                                                            RosterShiftDayCount = RosterShiftDayCount + 1;

                                                                            if (iWeekOffDayInRoster == RosterShiftDayCount)
                                                                            {
                                                                                sDayType = "W";
                                                                                sDayLengthType = "Week Off";
                                                                            }

                                                                            drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                                            drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                                            drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                                            drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim();
                                                                            drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();

                                                                            drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                                            drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                                            drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                                            drEmpDtWiseSftAss[nameof(RosterShiftDayCount)] = RosterShiftDayCount;
                                                                            drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                                            drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                            drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                                            drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                                            drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                            drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                            dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);

                                                                            #endregion If Last updated shift in table 'EmployeeShiftAssign' is roster
                                                                        }
                                                                    }

                                                                    #endregion Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                                                    #endregion FromDate & Shift start Date Same and After fromdate to todate not found shift assignment
                                                                }
                                                                else if (dvEmpSftAss.Count > 0)
                                                                {
                                                                    #region Shift start Date is great than FromDate

                                                                    #region Check Last updated shift in table 'EmployeeShiftAssign' after fromdate

                                                                    if (Convert.ToBoolean(dvEmpSftAss[0]["IsFix"].ToString().Trim()))
                                                                    {
                                                                        #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                        dvEmpWkOff.Table = dtEmpWkOff;
                                                                        dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAss[0]["FixSystemID"].ToString().Trim() + "'";
                                                                        if (dvEmpWkOff.Count > 0)
                                                                        {
                                                                            bAlignWithCC = Convert.ToBoolean(dvEmpWkOff[0]["AlignWithCC"].ToString().Trim());
                                                                            bIndividualWeekOff = Convert.ToBoolean(dvEmpWkOff[0]["IndividualWeekOff"].ToString().Trim());

                                                                            sFstOffDay = dvEmpWkOff[0]["FstOffDay"].ToString().Trim();
                                                                            sFstDayLengthType = dvEmpWkOff[0]["FstDayLengthType"].ToString().Trim();
                                                                            sSndOffDay = dvEmpWkOff[0]["SndOffDay"].ToString().Trim();
                                                                            sSndDayLengthType = dvEmpWkOff[0]["SndDayLengthType"].ToString().Trim();
                                                                        }

                                                                        #endregion Find Fixed Shift Employee's week off align with company calendar or Individual

                                                                        #region If Employee's week off align with company calendar

                                                                        if (bAlignWithCC)
                                                                        {
                                                                            dvComAssWkOff.Table = dtComAssWkOff;
                                                                            dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                                            if (dvComAssWkOff.Count > 0)
                                                                            {
                                                                                sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                                                if (sDayLengthType == "Full Day")
                                                                                {
                                                                                    sDayType = "W";
                                                                                    sDayLengthType = "Week Off";
                                                                                }
                                                                                else
                                                                                {
                                                                                    dvDayType.Table = dtDayType;
                                                                                    dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                    if (dvDayType.Count > 0)
                                                                                    {
                                                                                        sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                    }
                                                                                }
                                                                            }
                                                                        }

                                                                        #endregion If Employee's week off align with company calendar

                                                                        #region If Employee's week off align Individualy

                                                                        if (bIndividualWeekOff)
                                                                        {
                                                                            if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                                            {
                                                                                sDayLengthType = sFstDayLengthType;
                                                                                if (sDayLengthType == "Full Day")
                                                                                {
                                                                                    sDayType = "W";
                                                                                    sDayLengthType = "Week Off";
                                                                                }
                                                                                else
                                                                                {
                                                                                    dvDayType.Table = dtDayType;
                                                                                    dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                    if (dvDayType.Count > 0)
                                                                                    {
                                                                                        sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                    }
                                                                                }
                                                                            }

                                                                            if (sSndOffDay == (dtStDt.DayOfWeek).ToString())
                                                                            {
                                                                                sDayLengthType = sSndDayLengthType;
                                                                                if (sDayLengthType == "Full Day")
                                                                                {
                                                                                    sDayType = "W";
                                                                                    sDayLengthType = "Week Off";
                                                                                }
                                                                                else
                                                                                {
                                                                                    dvDayType.Table = dtDayType;
                                                                                    dvDayType.RowFilter = "Description = '" + sDayLengthType + "'";
                                                                                    if (dvDayType.Count > 0)
                                                                                    {
                                                                                        sDayType = dvDayType[0]["DayType"].ToString().Trim();
                                                                                    }
                                                                                }
                                                                            }
                                                                        }

                                                                        #endregion If Employee's week off align Individualy

                                                                        #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'

                                                                        drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                                        drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                                        drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                                        drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                                                        drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAss[0]["FixSystemID"].ToString().Trim();

                                                                        drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                                        drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                                        drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                                        drEmpDtWiseSftAss[nameof(RosterShiftDayCount)] = 0;
                                                                        drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                                        drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                        drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                                        drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                                        drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                        drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                        dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);

                                                                        #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                                    }
                                                                    else if (Convert.ToBoolean(dvEmpSftAss[0]["IsRoster"].ToString().Trim()))
                                                                    {
                                                                        #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                                                        //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                                                        RosterMstSysID = dvEmpSftAss[0]["RosterSystemID"].ToString().Trim();

                                                                        dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                                        dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtLastDt + "'";
                                                                        if (dvSftRstDayCnt.Count > 0)
                                                                        {
                                                                            RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0][nameof(RosterShiftDayCount)].ToString().Trim());
                                                                            RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                                            bInitialRstSftDyCnt = true;
                                                                        }
                                                                        else if (!bInitialRstSftDyCnt)
                                                                        {
                                                                            RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[0]["StartFromDay"].ToString().Trim()) - 1;
                                                                            RosterChlSftSysID = dvEmpSftAss[0]["RosterStartShiftID"].ToString().Trim();
                                                                            bInitialRstSftDyCnt = true;
                                                                        }

                                                                        //Set Roster Child Shift SystemID For Current Date in loop
                                                                        dvSftRstCdl.Table = dtSftRstCdl;
                                                                        dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                                                        if (dvSftRstCdl.Count > 0)
                                                                        {
                                                                            #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                            {// RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                                                if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                                                {
                                                                                    ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                    ShiftDays = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftDays)].ToString().Trim());
                                                                                    iWeekOffDayInRoster = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffDayInRoster"].ToString().Trim());
                                                                                }
                                                                            }

                                                                            //Check RosterShiftDayCount & ShiftDays
                                                                            if (RosterShiftDayCount >= ShiftDays)
                                                                            {
                                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                                    if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                    {
                                                                                        RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim());
                                                                                    }
                                                                                }
                                                                                if (RosterShiftSequence == 0)
                                                                                {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                                    RosterShiftSequence = 1;
                                                                                }
                                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                                {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                                    if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC][nameof(ShiftSequence)].ToString().Trim()))
                                                                                    {
                                                                                        RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                                        RosterChlSftSysID = RosterChlNewSftSysID;
                                                                                        RosterShiftDayCount = 0;
                                                                                        RosterShiftSequence = 0;
                                                                                    }
                                                                                }
                                                                            }
                                                                            else
                                                                            {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                                                RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                                            }

                                                                            #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                                        }

                                                                        //Update RosterShiftDayCount
                                                                        RosterShiftDayCount = RosterShiftDayCount + 1;

                                                                        if (iWeekOffDayInRoster == RosterShiftDayCount)
                                                                        {
                                                                            sDayType = "W";
                                                                            sDayLengthType = "Week Off";
                                                                        }

                                                                        drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                                        drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                                        drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                                        drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                                                        drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();

                                                                        drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                                        drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                                        drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                                        drEmpDtWiseSftAss[nameof(RosterShiftDayCount)] = RosterShiftDayCount;
                                                                        drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                                        drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                        drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                                        drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                                        drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                        drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                        dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);

                                                                        #endregion If Last updated shift in table 'EmployeeShiftAssign' is roster
                                                                    }

                                                                    #endregion Check Last updated shift in table 'EmployeeShiftAssign' after fromdate

                                                                    #endregion Shift start Date is great than FromDate
                                                                }

                                                                #endregion EmpSystemID and WorkDate not found in the table 'EmpDateWiseShiftAssign'
                                                            }

                                                            dvAttdnProc.Table = dtAttdnProc;
                                                            dvAttdnProc.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + strStDt + "'";
                                                            if (dvAttdnProc.Count > 0)
                                                            {
                                                                drAttdnProc = dvAttdnProc[0].Row;
                                                                drAttdnProc.BeginEdit();
                                                                drAttdnProc["ToReprocess"] = "Yes";
                                                                drAttdnProc.EndEdit();
                                                            }
                                                        }

                                                        SaveDataSetsShift(dsEmpDtWiseSftAss, dsAttdnProc);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dsEmpSftAssBfrFmDt = null;
            }
        }//End Function

        private void SaveDataSetsShift(DataSet dsEmpDtWiseSftAss, DataSet dsAttdnProc)
        {
            List<AttdnProcessData> AttdnProcessDataList = null;
            List<EmpDateWiseShiftAssign> EmpDateWiseShiftAssignList = null;
            var flag = false;
            try
            {
                InitAttdnProcessDataShift(dsAttdnProc, out AttdnProcessDataList);
                InitEmpDtWiseSftAss(dsEmpDtWiseSftAss, out EmpDateWiseShiftAssignList);

                foreach (var item in EmpDateWiseShiftAssignList)
                {
                    _eds.InsertOrUpdateGraph(item);
                }
                foreach (var item in AttdnProcessDataList)
                {
                    InsertOrUpdateGraph(item);
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private static void InitAttdnProcessDataShift(DataSet dsAttnProcData, out List<AttdnProcessData> AttdnRawDataList)
        {
            AttdnRawDataList = new List<AttdnProcessData>();
            try
            {
                for (int i = 0; i < dsAttnProcData.Tables[0].Rows.Count; i++)
                {
                    if (dsAttnProcData.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                        var ob = new AttdnProcessData
                        {
                            EmpSystemID = dsAttnProcData.Tables[0].Rows[i]["EmpSystemID"].ToString(),
                            WorkDate = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["WorkDate"].ToString()),
                            GroupID = dsAttnProcData.Tables[0].Rows[i]["GroupID"].ToString(),
                            PlantID = dsAttnProcData.Tables[0].Rows[i]["PlantID"].ToString(),
                            ShiftSystemID = dsAttnProcData.Tables[0].Rows[i]["ShiftSystemID"].ToString(),
                            InTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["InTime"].ToString()),
                            IsManualInTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualInTime"].ToString()),
                            OutTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["OutTime"].ToString()),
                            IsManualOutTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualOutTime"].ToString()),
                            DayStatus = dsAttnProcData.Tables[0].Rows[i]["DayStatus"].ToString(),
                            IsManualDayStatus = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualDayStatus"].ToString()),
                            OTHr = Convert.ToDecimal(dsAttnProcData.Tables[0].Rows[i]["OTHr"].ToString()),
                            IsOTComfirm = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsOTComfirm"].ToString()),
                            DateOTComfirm = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["DateOTComfirm"].ToString()),
                            OTComfirmBy = dsAttnProcData.Tables[0].Rows[i]["OTComfirmBy"].ToString(),
                            LTSystemID = dsAttnProcData.Tables[0].Rows[i]["LTSystemID"].ToString(),
                            IsLock = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsLock"].ToString()),
                            ToReprocess = dsAttnProcData.Tables[0].Rows[i]["ToReprocess"].ToString(),
                            InTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["InTimeRowID"].ToString()),
                            OutTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["OutTimeRowID"].ToString()),

                            ModelState = ModelState.Added
                        };
                        AttdnRawDataList.Add(ob);
                    }
                    else
                    {
                        var ob = new AttdnProcessData
                        {
                            // ob.EmpSystemID = dsAttnProcData.Tables[0].Rows[i]["EmpSystemID"].ToString();
                            WorkDate = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["WorkDate"].ToString()),
                            //ob.GroupID = dsAttnProcData.Tables[0].Rows[i]["GroupID"].ToString();
                            //ob.PlantID = dsAttnProcData.Tables[0].Rows[i]["PlantID"].ToString();
                            ShiftSystemID = dsAttnProcData.Tables[0].Rows[i]["ShiftSystemID"].ToString(),
                            InTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["InTime"].ToString()),
                            IsManualInTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualInTime"].ToString()),
                            OutTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["OutTime"].ToString()),
                            IsManualOutTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualOutTime"].ToString()),
                            DayStatus = dsAttnProcData.Tables[0].Rows[i]["DayStatus"].ToString(),
                            IsManualDayStatus = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualDayStatus"].ToString()),
                            OTHr = Convert.ToDecimal(dsAttnProcData.Tables[0].Rows[i]["OTHr"].ToString()),
                            IsOTComfirm = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsOTComfirm"].ToString()),
                            DateOTComfirm = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["DateOTComfirm"].ToString()),
                            OTComfirmBy = dsAttnProcData.Tables[0].Rows[i]["OTComfirmBy"].ToString(),
                            LTSystemID = dsAttnProcData.Tables[0].Rows[i]["LTSystemID"].ToString(),
                            IsLock = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsLock"].ToString()),
                            ToReprocess = dsAttnProcData.Tables[0].Rows[i]["ToReprocess"].ToString(),
                            InTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["InTimeRowID"].ToString()),
                            OutTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["OutTimeRowID"].ToString()),

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

        private static void InitEmpDtWiseSftAss(DataSet dsAttnProcData, out List<EmpDateWiseShiftAssign> EmpDateWiseShiftAssignList)
        {
            EmpDateWiseShiftAssignList = new List<EmpDateWiseShiftAssign>();
            try
            {
                for (int i = 0; i < dsAttnProcData.Tables[0].Rows.Count; i++)
                {
                    if (dsAttnProcData.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                        var ob = new EmpDateWiseShiftAssign
                        {
                            EmpSystemID = dsAttnProcData.Tables[0].Rows[i]["EmpSystemID"].ToString(),
                            WorkDate = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["WorkDate"].ToString()),
                            GroupID = dsAttnProcData.Tables[0].Rows[i]["GroupID"].ToString(),
                            PlantID = dsAttnProcData.Tables[0].Rows[i]["PlantID"].ToString(),
                            ShiftSystemID = dsAttnProcData.Tables[0].Rows[i]["ShiftSystemID"].ToString(),

                            ModelState = ModelState.Added
                        };
                        EmpDateWiseShiftAssignList.Add(ob);
                    }
                    else
                    {
                        var ob = new EmpDateWiseShiftAssign
                        {
                            // ob.EmpSystemID = dsAttnProcData.Tables[0].Rows[i]["EmpSystemID"].ToString();
                            WorkDate = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["WorkDate"].ToString()),
                            //ob.GroupID = dsAttnProcData.Tables[0].Rows[i]["GroupID"].ToString();
                            //ob.PlantID = dsAttnProcData.Tables[0].Rows[i]["PlantID"].ToString();
                            ShiftSystemID = dsAttnProcData.Tables[0].Rows[i]["ShiftSystemID"].ToString(),

                            ModelState = ModelState.Modified
                        };
                        EmpDateWiseShiftAssignList.Add(ob);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void AttdnProcBaseOn(string GroupSysID, string _plantid)
        {
            DataSet dsLocal = null;
            DataSet dsEmpInfo = null;
            // clsRegister objReg = null;
            //objReg = new clsRegister();

            try
            {
                dsEmpInfo = GetAllRegsterPersonOnSystemAttdnProc(GroupSysID.Trim(), _plantid, sAttnDate.Trim());
                sEmpSystemIDColl = "";
                for (int i = 0; i < dsEmpInfo.Tables[0].Rows.Count; i++)
                {
                    sEmpSystemIDColl = sEmpSystemIDColl.Trim() == "" ? "'" + dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'" : sEmpSystemIDColl.Trim() + ", '" + dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                }

                dsLocal = GetPlantWiseHRMSSetting(GroupSysID.Trim(), _plantid);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    lblAttdnProcBase = "As per  plant wise HRMS setting, attendance process will be based on '" + dsLocal.Tables[0].Rows[0]["AttdnProcBase"].ToString().Trim() + "'";
                    if (dsLocal.Tables[0].Rows[0]["AttdnProcBase"].ToString().ToUpper().Trim() == "ENROLLMENTID")
                    {
                        radDwLdEnrollID = true;
                        //radDwLdScanNumber = false;
                    }
                    else if (dsLocal.Tables[0].Rows[0]["AttdnProcBase"].ToString().ToUpper().Trim() == "SCANNUMBER")
                    {
                        radDwLdEnrollID = false;
                        //radDwLdScanNumber = true;
                    }
                    else
                    {
                        lblAttdnProcBase = "";
                        var ex = new Exception("Please configure plant wise HRMS setting for attendance data download...!");
                        throw (ex);
                    }
                }
                else
                {
                    throw new Exception("Please configure plant wise HRMS setting for attendance data download...!");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool xAttdnDateProcessForInData(string GroupSysID, string _plantid, string strYrSystemID, bool radDwLdEnrollID, string strYrFromDate, string strYrToDate)//1
        {
            #region Declare variables

            DataSet dsDayType = null;
            DataTable dtDayType = null;
            DataView dvDayType = null;

            DataSet dsRawData = null;
            DataTable dtRawData = null;
            DataView dvRawData = null;
            DataRow drRawData = null;

            DataSet dsEmpNotiData = null;
            DataTable dtEmpNotiData = null;
            DataView dvEmpNotiData = null;
            DataRow drEmpNotiData = null;

            DataSet dsMnAttData = null;
            DataTable dtMnAttData = null;
            DataView dvMnAttData = null;

            DataSet dsAttnProcData = null;
            DataTable dtAttnProcData = null;
            DataRow drAttnProcData = null;
            DataView dvAttnProcData = null;

            DataSet dsEmpInfo = null;

            DataSet dsLvTransDtl = null;
            DataTable dtLvTransDtl = null;
            DataRow drLvTransDtl = null;
            DataView dvLvTransDtl = null;

            DataSet dsLvTrans = null;
            DataTable dtLvTrans = null;
            DataView dvLvTrans = null;

            DataSet dsLvAllo = null;
            DataTable dtLvAllo = null;
            DataView dvLvAllo = null;
            DataRow drLvAllo = null;

            DataSet dsLvAvail = null;
            DataTable dtLvAvail = null;
            DataView dvLvAvail = null;

            DataSet dsOffDay = null;
            DataTable dtOffDay = null;
            DataView dvOffDay = null;

            // clsRegister objReg;
            //objReg = new clsRegister();

            var sOfficeStartTime = "";
            var sOfficeInTime = "";
            var sOrgOfficeInTime = "";
            var sLogDownLoadNum = "";

            var sEmpSysID = "";
            var sEmpCode = "";
            var sShiftSystemID = "";
            var sLastProcDate = "";
            var sPlantID = "";

            var sInTime = "";
            var sInTimeRowID = string.Empty;
            var iDeviceID = 0;
            var sInTimeTmp = "";
            var sInTimeRowIDTmp = string.Empty;
            var sDayStatusTmp = "";
            var iDeviceIDTmp = 0;
            var sDayStatus = "";
            var sPrvDayStatus = "";
            var sLvTrans = "";
            var sOffDay = "";
            var sComHoliDay = "";
            var sLvTnsDtlSysID = "";
            var sLvPolDtlSysID = "";
            var sLvAvailed = 0;
            var iInTimeStartMargin = 0;

            var sDayType = "";

            var sBreakStratTime = "";
            var sBreakEndTime = "";

            var sDate = "";
            var sPrvDate = "";
            var sWorkingDate = "";
            var bMoreInMarg = false;
            sAttnDate = DateTime.Now.ToString("dd-MMM-yyyy");
            var bValid = false;

            #endregion Declare variables

            try
            {
                #region DataSet

                sDate = sAttnDate.Trim();
                sPrvDate = (Convert.ToDateTime(sAttnDate.Trim()).AddDays(-1)).ToString("dd-MMM-yyyy");
                sWorkingDate = sAttnDate.Trim();

                dsDayType = GetDayType();
                dtDayType = dsDayType.Tables[0];
                using (dvDayType = new DataView())
                {
                    dsRawData = GetAttdnRawDataForAttdnProc(GroupSysID.Trim(), sDate.Trim(), "IN");
                    dtRawData = dsRawData.Tables[0];

                    dsAttnProcData = GetAttdnProcData(GroupSysID.Trim(), _plantid, sPrvDate.Trim(), sDate.Trim());
                    dtAttnProcData = dsAttnProcData.Tables[0];

                    dsEmpInfo = GetEmployeeInfo(GroupSysID.Trim(), _plantid, sEmpSystemIDColl.Trim(), sDate.Trim());

                    dsMnAttData = GetAttdnManualData(GroupSysID.Trim(), _plantid, sAttnDate.Trim());
                    dtMnAttData = dsMnAttData.Tables[0];
                    using (dvMnAttData = new DataView())
                    {
                        dsLvTransDtl = GetLeaveTransactionDetails(GroupSysID.Trim(), _plantid, sDate.Trim());
                        dtLvTransDtl = dsLvTransDtl.Tables[0];

                        dsLvTrans = GetLeaveTransactionInfo(GroupSysID.Trim(), _plantid, sDate.Trim());
                        dtLvTrans = dsLvTrans.Tables[0];

                        dsLvAllo = GetLeaveAllocation(GroupSysID.Trim(), _plantid, sDate.Trim(), strYrSystemID.Trim());
                        dtLvAllo = dsLvAllo.Tables[0];

                        dsLvAvail = GetAvailedLvInfo(GroupSysID.Trim(), _plantid, strYrSystemID.Trim(), strYrFromDate.Trim(), strYrToDate.Trim());
                        dtLvAvail = dsLvAvail.Tables[0];

                        dsOffDay = GetAllPlantOffDayInformation(GroupSysID.Trim(), _plantid, sDate.Trim());
                        dtOffDay = dsOffDay.Tables[0];

                        dsEmpNotiData = GetEmployeeNotifications(sDate.Trim());
                        dtEmpNotiData = dsEmpNotiData.Tables[0];
                        using (dvEmpNotiData = new DataView())
                        {
                            #endregion DataSet

                            if (dsEmpInfo.Tables[0].Rows.Count > 0)
                            {
                                for (int EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                                {
                                    sComHoliDay = "";
                                    sOffDay = "";
                                    sLastProcDate = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["LastWorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                    sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();
                                    sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();
                                    sEmpCode = dsEmpInfo.Tables[0].Rows[EmpCount]["EmployeeCode"].ToString();
                                    sOfficeStartTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeStartTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    sOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    sOrgOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    iInTimeStartMargin = Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["InTimeStartMargin"].ToString());
                                    sShiftSystemID = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftSystemID"].ToString();
                                    sDayType = dsEmpInfo.Tables[0].Rows[EmpCount]["DayType"].ToString();

                                    sBreakStratTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakStratTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    sBreakEndTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakEndTime"].ToString().Trim()).ToString("HH:mm:ss");

                                    using (dvOffDay = new DataView
                                    {
                                        Table = dtOffDay,
                                        RowFilter = "PlantID = '" + sPlantID + "'"
                                    })
                                    {
                                        if (dvOffDay.Count > 0)
                                        {
                                            var builder = new StringBuilder();
                                            builder.Append(sComHoliDay);
                                            for (int ofd = 0; ofd < dvOffDay.Count; ofd++)
                                            {
                                                builder.Append(dvOffDay[ofd]["OffDayType"].ToString().Trim());
                                            }
                                            sComHoliDay = builder.ToString();
                                        }

                                        sOffDay = sDayType.ToUpper() == "W" ? sComHoliDay + sDayType : sComHoliDay;

                                        if (radDwLdEnrollID)
                                        {
                                            sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["EmployeeCode"].ToString();
                                        }
                                        else// if (this.radDwLdScanNumber.Checked == true)
                                        {
                                            sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["CardNumber"].ToString();
                                            sLogDownLoadNum = CardNumConvert(sLogDownLoadNum, 0);
                                        }

                                        //if (sLogDownLoadNum == "07696" || sLogDownLoadNum == "26698")
                                        //{
                                        //    string a = "";
                                        //}

                                        #region Find InTime from raw Data Table

                                        sInTime = "00:00:00";
                                        sInTimeRowID = string.Empty;
                                        iDeviceID = 0;
                                        sInTimeTmp = "00:00:00";
                                        sInTimeRowIDTmp = string.Empty;
                                        sDayStatusTmp = "";
                                        iDeviceIDTmp = 0;
                                        sDayStatus = "";
                                        sPrvDayStatus = "";
                                        sLvTrans = "";
                                        sLvPolDtlSysID = "";
                                        sLvAvailed = 0;
                                        bMoreInMarg = false;

                                        using (dvRawData = new DataView
                                        {
                                            Table = dtRawData,
                                            RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'"
                                        })
                                        {
                                            if (dvRawData.Count > 0)
                                            {
                                                for (int RData = 0; RData < dvRawData.Count; RData++)
                                                {
                                                    if (dvRawData[RData]["PTime"].ToString() != "")
                                                    {
                                                        var sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                        if (sInTime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(sInTime.Trim()))
                                                        {
                                                            sInTime = sPInTime;
                                                            sInTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                                            iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                                            if (sInTimeTmp != "00:00:00" & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp))
                                                            {
                                                                sInTime = sInTimeTmp;
                                                                sInTimeRowID = sInTimeRowIDTmp;
                                                                iDeviceID = iDeviceIDTmp;
                                                            }
                                                            sInTimeTmp = sInTime;
                                                            sInTimeRowIDTmp = sInTimeRowID;
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

                                            #region DayStatus Base On InTime

                                            if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) <= Convert.ToDateTime(sOfficeInTime))
                                            {
                                                sDayStatus = "P";
                                            }
                                            else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sOfficeInTime))
                                            {
                                                sDayStatus = "L";
                                            }
                                            else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) < Convert.ToDateTime(sOfficeStartTime))
                                            {
                                                sDayStatus = "A";
                                                sInTime = "00:00:00";
                                                bMoreInMarg = true;

                                                //sDayStatus = "P";
                                                //TimeSpan tsEarlyCome = Convert.ToDateTime(sOfficeStartTime) - Convert.ToDateTime(sInTime);
                                                //int iEarlyCome = tsEarlyCome.Minutes;
                                                //if (iEarlyCome > iInTimeStartMargin & iInTimeStartMargin > 0)
                                                //{
                                                //    while (iEarlyCome > iInTimeStartMargin)
                                                //    {
                                                //        iEarlyCome = iEarlyCome / 4;
                                                //        sInTime = Convert.ToDateTime(sOrgOfficeInTime).AddMinutes(-iEarlyCome).ToString("HH:mm:ss");
                                                //    }
                                                //}
                                                //else if (iEarlyCome < iInTimeStartMargin & iInTimeStartMargin > 0)
                                                //{
                                                //    sInTime = Convert.ToDateTime(sOfficeStartTime).AddMinutes(iEarlyCome).ToString("HH:mm:ss");
                                                //}
                                                //else
                                                //{
                                                //    sInTime = Convert.ToDateTime(sInTime).ToString("HH:mm:ss");
                                                //}
                                            }
                                            else if (sInTime != "00:00:00" & sDayType.ToUpper() == "FHW" && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sBreakStratTime) & Convert.ToDateTime(sInTime) <= Convert.ToDateTime(sBreakEndTime))
                                            {
                                                sDayStatus = "P";
                                            }
                                            else if (sInTime != "00:00:00" & sDayType.ToUpper() == "FHW" && Convert.ToDateTime(sInTime) > Convert.ToDateTime(sBreakStratTime) & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sBreakEndTime))
                                            {
                                                sDayStatus = "L";
                                            }
                                            else if (sInTime != "00:00:00" & sDayType.ToUpper() == "FHW" && Convert.ToDateTime(sInTime) < Convert.ToDateTime(sBreakStratTime))
                                            {
                                                sDayStatus = "A";
                                                sInTime = "00:00:00";
                                                bMoreInMarg = true;

                                                //sDayStatus = "P";
                                                //TimeSpan tsEarlyCome = Convert.ToDateTime(sBreakStratTime) - Convert.ToDateTime(sInTime);
                                                //TimeSpan tsInTimeStartMargin = Convert.ToDateTime(sBreakEndTime) - Convert.ToDateTime(sBreakStratTime);
                                                //iInTimeStartMargin = tsInTimeStartMargin.Minutes;
                                                //int iEarlyCome = tsEarlyCome.Minutes;
                                                //if (iEarlyCome > iInTimeStartMargin & iInTimeStartMargin > 0)
                                                //{
                                                //    while (iEarlyCome > iInTimeStartMargin)
                                                //    {
                                                //        iEarlyCome = iEarlyCome / 4;
                                                //        sInTime = Convert.ToDateTime(sBreakEndTime).AddMinutes(-iEarlyCome).ToString("HH:mm:ss");
                                                //    }
                                                //}
                                                //else if (iEarlyCome < iInTimeStartMargin & iInTimeStartMargin > 0)
                                                //{
                                                //    sInTime = Convert.ToDateTime(sBreakStratTime).AddMinutes(iEarlyCome).ToString("HH:mm:ss");
                                                //}
                                                //else
                                                //{
                                                //    sInTime = Convert.ToDateTime(sInTime).ToString("HH:mm:ss");
                                                //}
                                            }

                                            #region if lowest In Time is less than in time Margin and after get another In time after intime margin

                                            if (bMoreInMarg)
                                            {
                                                sInTimeTmp = "00:00:00";
                                                sInTimeRowIDTmp = "";
                                                sDayStatusTmp = "";

                                                sInTime = "00:00:00";
                                                sInTimeRowID = "";

                                                // This is for nornal workday and if employee have 2nd half week end
                                                if (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW")
                                                {
                                                    dvRawData.Table = dtRawData;
                                                    dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                                                    if (dvRawData.Count > 0)
                                                    {
                                                        for (int RData = 0; RData < dvRawData.Count; RData++)
                                                        {
                                                            if (dvRawData[RData]["PTime"].ToString() != "")
                                                            {
                                                                var sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");

                                                                if (Convert.ToDateTime(sPInTime) >= Convert.ToDateTime(sOfficeStartTime.Trim()))
                                                                {
                                                                    if (sInTime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(sInTime.Trim()))
                                                                    {
                                                                        sInTime = sPInTime;
                                                                        sInTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                                                        iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                                                        if (sInTimeTmp != "00:00:00" & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp))
                                                                        {
                                                                            sInTime = sInTimeTmp;
                                                                            sInTimeRowID = sInTimeRowIDTmp;
                                                                            iDeviceID = iDeviceIDTmp;
                                                                        }
                                                                        sInTimeTmp = sInTime;
                                                                        sInTimeRowIDTmp = sInTimeRowID;
                                                                        iDeviceIDTmp = iDeviceID;
                                                                    }

                                                                    sDayStatus = "P";
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                // This is for employee have 1st half week end
                                                else if (sDayType.ToUpper() == "FHW") //
                                                {
                                                    dvRawData.Table = dtRawData;
                                                    dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                                                    if (dvRawData.Count > 0)
                                                    {
                                                        for (int RData = 0; RData < dvRawData.Count; RData++)
                                                        {
                                                            if (dvRawData[RData]["PTime"].ToString() != "")
                                                            {
                                                                var sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");

                                                                if (Convert.ToDateTime(sPInTime) >= Convert.ToDateTime(sBreakEndTime.Trim()))
                                                                {
                                                                    if (sInTime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(sInTime.Trim()))
                                                                    {
                                                                        sInTime = sPInTime;
                                                                        sInTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                                                        iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                                                        if (sInTimeTmp != "00:00:00" & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp))
                                                                        {
                                                                            sInTime = sInTimeTmp;
                                                                            sInTimeRowID = sInTimeRowIDTmp;
                                                                            iDeviceID = iDeviceIDTmp;
                                                                        }
                                                                        sInTimeTmp = sInTime;
                                                                        sInTimeRowIDTmp = sInTimeRowID;
                                                                        iDeviceIDTmp = iDeviceID;
                                                                    }

                                                                    sDayStatus = "P";
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion if lowest In Time is less than in time Margin and after get another In time after intime margin

                                            #endregion DayStatus Base On InTime

                                            #region Leave Transaction

                                            sLvTrans = "";
                                            sLvTnsDtlSysID = "";
                                            sLvPolDtlSysID = "";
                                            sLvAvailed = 0;

                                            var LVDayStatus = "";

                                            using (dvLvTrans = new DataView
                                            {
                                                Table = dtLvTrans,
                                                RowFilter = "EmpSystemID = '" + sEmpSysID + "'"
                                            })
                                            {
                                                if (dvLvTrans.Count > 0)
                                                {
                                                    LVDayStatus = dvLvTrans[0]["LeaveStatus"].ToString().Trim();
                                                    sLvTrans = dvLvTrans[0]["LTSystemID"].ToString().Trim();
                                                    sLvTnsDtlSysID = dvLvTrans[0]["SystemID"].ToString().Trim();
                                                }

                                                #endregion Leave Transaction

                                                if (LVDayStatus == "W" || LVDayStatus == "H" || LVDayStatus == "HW" || LVDayStatus == "WH")
                                                {
                                                    LVDayStatus = "";
                                                }

                                                sDayStatus = sOffDay + LVDayStatus + sDayStatus;

                                                var bAttnIsLock = false;
                                                var bManualInTime = false;
                                                var bManualDayStatus = false;

                                                //if (iDeviceID == 0)
                                                //{
                                                //    bManualInTime = true;
                                                //}

                                                using (

                                                //if (iDeviceID == 0)
                                                //{
                                                //    bManualInTime = true;
                                                //}

                                                dvAttnProcData = new DataView
                                                {
                                                    Table = dtAttnProcData,
                                                    RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'"
                                                })
                                                {
                                                    if (dvAttnProcData.Count > 0)
                                                    {
                                                        bAttnIsLock = Convert.ToBoolean(dvAttnProcData[0].Row["IsLock"].ToString());
                                                        //bManualInTime = Convert.ToBoolean(dvAttnProcData[0].Row["IsManualInTime"].ToString());

                                                        if (!bAttnIsLock)
                                                        {
                                                            if (dvAttnProcData[0]["InTime"].ToString() != "")
                                                            {
                                                                sInTimeTmp = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                                sInTimeRowIDTmp = dvAttnProcData[0]["InTimeRowID"].ToString().Trim();
                                                                sDayStatusTmp = dvAttnProcData[0]["DayStatus"].ToString().Trim();
                                                            }

                                                            if ((sInTimeTmp != "00:00:00") & (sInTime == "00:00:00"))
                                                            {
                                                                sInTime = sInTimeTmp;
                                                                sInTimeRowID = sInTimeRowIDTmp;
                                                                sDayStatus = sDayStatusTmp;
                                                            }

                                                            sPrvDayStatus = dvAttnProcData[0]["DayStatus"].ToString().Trim();

                                                            if (sInTime == "00:00:00" & sInTimeTmp == "00:00:00" & sDayStatus == "")
                                                            {
                                                                sDayStatus = "A";
                                                            }

                                                            if ((sInTimeTmp != "00:00:00") & (Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp)))
                                                            {
                                                                sInTime = sInTimeTmp;
                                                                sInTimeRowID = sInTimeRowIDTmp;
                                                                sDayStatus = sDayStatusTmp;
                                                            }

                                                            if (dvAttnProcData[0]["OutTime"].ToString().Trim() != "")
                                                            {
                                                                var extOutTime = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                                if ((sInTime != "00:00:00") & (extOutTime != "00:00:00") & (Convert.ToDateTime(sInTime) > Convert.ToDateTime(extOutTime)) & sDayStatus == "")
                                                                {
                                                                    sInTime = "00:00:00";
                                                                    sInTimeRowID = "";
                                                                    sDayStatus = "A";
                                                                }
                                                            }

                                                            #region Manual Attendance

                                                            dvMnAttData.Table = dtMnAttData;
                                                            dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                                            if (dvMnAttData.Count > 0)
                                                            {
                                                                if (dvMnAttData[0].Row["InTime"].ToString().Trim() != "")
                                                                {
                                                                    sInTime = Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                                    bManualInTime = true;
                                                                }
                                                                sInTimeRowID = "";
                                                                sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();

                                                                if (dvMnAttData[0].Row["InTime"].ToString().Trim() == "" && dvMnAttData[0].Row["OutTime"].ToString().Trim() == "")
                                                                { bManualDayStatus = true; }
                                                            }

                                                            #endregion Manual Attendance

                                                            if (sPrvDayStatus.Trim() != sDayStatus.Trim())
                                                            {
                                                                using (dvEmpNotiData = new DataView
                                                                {
                                                                    Table = dtEmpNotiData,
                                                                    RowFilter = "EmpInfoSystemID = '" + sEmpSysID + "'"
                                                                })
                                                                {
                                                                    {
                                                                        drEmpNotiData = dtEmpNotiData.NewRow();
                                                                        drEmpNotiData["EmpInfoSystemID"] = sEmpSysID.Trim();
                                                                        drEmpNotiData["EventSourceTableSystemID"] = sWorkingDate.Trim();
                                                                        drEmpNotiData["EventDate"] = sWorkingDate.Trim();
                                                                        drEmpNotiData["EventRaisedBy"] = sEmpSysID.Trim();
                                                                        drEmpNotiData["EventType"] = NotificationType.Attendance.ToString();
                                                                        drEmpNotiData["IsDelivered"] = false;
                                                                        drEmpNotiData["WorkDate"] = sWorkingDate.Trim();
                                                                        dtEmpNotiData.Rows.Add(drEmpNotiData);
                                                                    }
                                                                }
                                                            }

                                                            drAttnProcData = dvAttnProcData[0].Row;
                                                            drAttnProcData.BeginEdit();
                                                            UpdateAttdnData("EDIT", GroupSysID, "IN", sEmpSysID, sPlantID, sWorkingDate.Trim(), sShiftSystemID, sDate, sInTime, bManualInTime, sInTimeRowID, sDayStatus, bManualDayStatus, 0, sLvTrans, ref drAttnProcData);
                                                            drAttnProcData.EndEdit();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (sInTime == "00:00:00" & sDayStatus == "")
                                                        {
                                                            sDayStatus = "A";
                                                        }

                                                        #region Manual Attendance

                                                        dvMnAttData.Table = dtMnAttData;
                                                        dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                                        if (dvMnAttData.Count > 0)
                                                        {
                                                            if (dvMnAttData[0].Row["InTime"].ToString().Trim() != "")
                                                            {
                                                                sInTime = Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                                bManualInTime = true;
                                                            }
                                                            sInTimeRowID = "";
                                                            sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();

                                                            if (dvMnAttData[0].Row["InTime"].ToString().Trim() == "" && dvMnAttData[0].Row["OutTime"].ToString().Trim() == "")
                                                            { bManualDayStatus = true; }
                                                        }

                                                        #endregion Manual Attendance

                                                        using (dvEmpNotiData = new DataView
                                                        {
                                                            Table = dtEmpNotiData,
                                                            RowFilter = "EmpInfoSystemID = '" + sEmpSysID + "'"
                                                        })
                                                        {
                                                            {
                                                                drEmpNotiData = dtEmpNotiData.NewRow();
                                                                drEmpNotiData["EmpInfoSystemID"] = sEmpSysID.Trim();
                                                                drEmpNotiData["EventSourceTableSystemID"] = sWorkingDate.Trim();
                                                                drEmpNotiData["EventDate"] = sWorkingDate.Trim();
                                                                drEmpNotiData["EventRaisedBy"] = sEmpSysID.Trim();
                                                                drEmpNotiData["EventType"] = NotificationType.Attendance.ToString();
                                                                drEmpNotiData["IsDelivered"] = false;
                                                                drEmpNotiData["WorkDate"] = sWorkingDate.Trim();
                                                                dtEmpNotiData.Rows.Add(drEmpNotiData);
                                                            }

                                                            drAttnProcData = dtAttnProcData.NewRow();
                                                            UpdateAttdnData("ADDNEW", GroupSysID, "IN", sEmpSysID, sPlantID, sWorkingDate.Trim(), sShiftSystemID, sDate.Trim(), sInTime, bManualInTime, sInTimeRowID, sDayStatus, bManualDayStatus, 0, sLvTrans, ref drAttnProcData);
                                                            dtAttnProcData.Rows.Add(drAttnProcData);
                                                        }
                                                    }

                                                    #region Leave Transaction Details Update

                                                    if (!bAttnIsLock)
                                                    {
                                                        using (dvLvTransDtl = new DataView
                                                        {
                                                            Table = dtLvTransDtl,
                                                            RowFilter = "SystemID = '" + sLvTnsDtlSysID + "'"
                                                        })
                                                        {
                                                            if (dvLvTransDtl.Count > 0)
                                                            {
                                                                drLvTransDtl = dvLvTransDtl[0].Row;
                                                                drLvTransDtl.BeginEdit();
                                                                drLvTransDtl["IsAvailed"] = 1;
                                                                drLvTransDtl.EndEdit();

                                                                using (
                                                                dvLvAvail = new DataView
                                                                {
                                                                    Table = dtLvAvail,
                                                                    RowFilter = "EmpSystemID = '" + sEmpSysID + "'"
                                                                })
                                                                {
                                                                    if (dvLvAvail.Count > 0)
                                                                    {
                                                                        for (int LvAllo = 0; LvAllo < dvLvAvail.Count; LvAllo++)
                                                                        {
                                                                            sLvPolDtlSysID = dvLvAvail[LvAllo]["LvPolDtlSystemID"].ToString().Trim();
                                                                            sLvAvailed = Convert.ToInt32(dvLvAvail[LvAllo]["Availed"].ToString().Trim());

                                                                            using (
                                                                            dvLvAllo = new DataView
                                                                            {
                                                                                Table = dtLvAllo,
                                                                                RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND LvPolDetailsSystemID = '" + sLvPolDtlSysID + "'"
                                                                            })
                                                                            {
                                                                                if (dvLvAllo.Count == 1)
                                                                                {
                                                                                    drLvAllo = dvLvAllo[0].Row;
                                                                                    drLvAllo.BeginEdit();

                                                                                    if (dvLvAvail[LvAllo]["LTSystemID"].ToString().Trim() == sLvTrans.Trim())
                                                                                    {
                                                                                        drLvAllo["AvailedLeave"] = sLvAvailed + 1;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        drLvAllo["AvailedLeave"] = sLvAvailed;
                                                                                    }
                                                                                    drLvAllo["UpdatedBy"] = "Schedule";
                                                                                    drLvAllo["DateUpdated"] = DateTime.Now;

                                                                                    drLvAllo.EndEdit();
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    #endregion Leave Transaction Details Update
                                }
                                //}

                                SaveDataSets(dsRawData, dsAttnProcData, dsEmpNotiData, dsLvTransDtl, dsLvAllo);

                                //ServiceReference1.HREndpointServiceClient client = new ServiceReference1.HREndpointServiceClient();
                                //client.sendAllNotification(clsRegister.NotificationType.Attendance.ToString());
                            }

                            //for (int i = 0; i < dsAttnProcData.Tables[0].Rows.Count; i++)
                            //{
                            //await proxy.Invoke("SendAttendanceNotificationIndividual", new object[] { _s.ToString(), System.DateTime.Now.ToString("dd-MMM-yyyy h:mm:ss tt"), "P", System.DateTime.Now.ToString("dd-MMM-yyyy h:mm:ss tt"), System.DateTime.Now.ToString("dd-MMM-yyyy h:mm:ss tt"), });

                            //}
                            bValid = true;
                            return bValid;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
                //Cursor = Cursors.Default;
                //System.Windows.Forms.MessageBox.Show(this, ex.ToString(), "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return bValid;
            }
            finally
            {
                #region clean variable

                dsRawData = null;
                dtRawData = null;
                dvRawData = null;
                drRawData = null;

                dsAttnProcData = null;
                dtAttnProcData = null;
                drAttnProcData = null;
                dvAttnProcData = null;

                dsEmpInfo = null;

                dsLvTransDtl = null;
                dtLvTransDtl = null;
                drLvTransDtl = null;
                dvLvTransDtl = null;

                dsLvTrans = null;
                dtLvTrans = null;
                dvLvTrans = null;

                dsLvAllo = null;
                dtLvAllo = null;
                dvLvAllo = null;
                drLvAllo = null;

                dsLvAvail = null;
                dtLvAvail = null;
                dvLvAvail = null;

                dsOffDay = null;

                sOfficeStartTime = string.Empty;
                sOfficeInTime = string.Empty;
                sLogDownLoadNum = string.Empty;
                sEmpSysID = string.Empty;

                sInTime = string.Empty;
                sInTimeRowID = string.Empty;
                sInTimeTmp = string.Empty;
                sInTimeRowIDTmp = string.Empty;
                sDayStatus = string.Empty;
                sLvTrans = string.Empty;
                sOffDay = string.Empty;
                sLvTnsDtlSysID = string.Empty;
                sLvPolDtlSysID = string.Empty;

                #endregion clean variable
            }
        }//End Function

        private static void UpdateAttdnData(string OPN_FLAG, string GroupId, string sType, string sEmpSystemID, string sPlantID, string sWorkingDate, string shiftSystemID, string sDate, string sTime, bool bManualTime, string sRowID, string sDayStatus, bool bManualDayStatus, decimal iOverTime, string sLvTrans, ref DataRow drLocal)
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

                drLocal["GroupID"] = GroupId;
                drLocal["PlantID"] = sPlantID.Trim();

                drLocal["UpdatedBy"] = "Schedule";
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //
            }
        }//End Function

        public DataSet GetAllRegsterPersonOnSystemAttdnProc(string sGroupID, string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT SystemID, EmployeeCode EnrollID, EmployeeName EnrollName, CardNumber, PlantID
	                                        FROM (
                                                  SELECT * FROM EmployeeInformation WHERE
                                                            SystemID IN (
                                                                         SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                                        )
                                                ) AS E
		                                        WHERE GroupID = '" + sGroupID + @"' AND (DOS > '" + sAttnDate + @"' OR DOS IS NULL)
                            UNION
                            (SELECT SystemID, VisitorID EnrollID, VisitorName EnrollName, CardNumber, PlantID
		                            FROM dbo.VisitorEntry WHERE GroupID = '" + sGroupID + @"' AND VisitorStatue = 'Active')";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet GetPlantWiseHRMSSetting(string sGroupID, string sPlantID)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.PlantWiseHRMSSetting
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetDayType()
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT* FROM dbo.DayType";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

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
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
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

        private DataSet GetLeaveTransactionDetails(string sGroupID, string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM LeaveTransactionDetails
                                WHERE WorkDate = '" + sAttnDate + @"'
                                    AND LvTrnsSystemID IN (
                                                           SELECT SystemID FROM LeaveTransaction
                                                             WHERE GroupID = '" + sGroupID + @"'
                                                                   AND EmpSystemID IN (
                                                                                       SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                                                            WHERE JobLcSystemID IN (
                                                                                                                    SELECT SystemID FROM [dbo].[JobLocation]
                                                                                                                        WHERE PlantID = '" + sPlantID + @"'
                                                                                                                   )
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

        private DataSet GetLeaveTransactionInfo(string sGroupID, string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT LTD.SystemID, LTD.LvTrnsSystemID, LT.EmpSystemID, LT.LTSystemID, LT.FromDate, LT.ToDate, LT.LeaveDays, LT.LvReason,
                             LTD.WorkDate, LTD.DayType, LTD.LeaveStatus, LTD.IsAvailed
                            FROM LeaveTransaction LT
		                        INNER JOIN LeaveTransactionDetails LTD ON LT.SystemID = LTD.LvTrnsSystemID
                                        AND LTD.WorkDate = '" + sAttnDate + @"'
                            WHERE LT.GroupID = '" + sGroupID + @"' AND LT.EmpSystemID IN (
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

        private DataSet GetLeaveAllocation(string sGroupID, string sPlantID, string sAttnDate, string strYrSystemID)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM LeaveAllocation
                                WHERE GroupID = '" + sGroupID + @"'
                                      AND YrCalSystemID  = '" + strYrSystemID + @"'
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

        private DataSet GetAvailedLvInfo(string sGroupID, string sPlantID, string strYrSystemID, string strFromDate, string strToDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT LT.EmpSystemID, LPD.SystemID LvPolDtlSystemID, LT.LTSystemID, ISNULL(Count(LTD.SystemID), 0) Availed
                            FROM LeaveTransaction LT
	                            LEFT JOIN LeaveTransactionDetails LTD ON LT.SystemID = LTD.LvTrnsSystemID AND LTD.IsAvailed = 1 AND LTD.LeaveStatus LIKE ('%LV%')
	                            LEFT JOIN (SELECT * FROM dbo.LeavePolicyDetail
						                            WHERE LPMSystemID IN (SELECT LvPolMstSystemID FROM dbo.LvPolMstYearCalendar
													                            WHERE YrCalSystemID = '" + strYrSystemID + @"')) LPD ON LT.LTSystemID = LPD.LTSystemID
                            WHERE LT.GroupID = '" + sGroupID + @"'
					                            AND (LT.FromDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'
						                            OR LT.ToDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                                 AND LT.EmpSystemID IN (SELECT SystemID FROM EmployeeInformation
                                                            WHERE JobLocationID IN (SELECT SystemID FROM
                                                                                        [dbo].[JobLocation]
                                                                                    WHERE PlantID = '" + sPlantID + @"'))
                            GROUP BY LT.EmpSystemID, LPD.SystemID, LT.LTSystemID";

                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetAllPlantOffDayInformation(string sGroupID, string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT OFM.CldDescription, OFM.FromDate, OFM.ToDate, OFM.OffDayType, OFM.TotalDay, OFD.DayName, OFM.PlantID
	                            FROM OffDayMaster OFM
			                            INNER JOIN OffDayDetail OFD ON OFM.SystemID = OFD.OffDayMstSystemID
                                                                    AND OFD.OffDayDate = '" + sAttnDate + @"'
                                WHERE OFM.GroupID = '" + sGroupID + @"' AND OFM.PlantID = '" + sPlantID + @"'
									  AND OFM.OffDayType = 'H'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmployeeNotifications(string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM [dbo].[EmployeeNotifications]
                           WHERE WorkDate = '" + sAttnDate + @"' AND EventSourceTableSystemID IS NULL";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetPlantInformation(string sPlantID)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM org.Plant WHERE Id = '" + sPlantID + "'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetYearlyCalendar(string sGroupID, string sPlantID, string sDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.YearlyCalendar
                                    WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
                                            AND '" + sDate + @"' BETWEEN FromDate AND ToDate";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmployeeInformationForShiftProcess(string sPlantID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM
                                        (
                                         SELECT E.*, ToReprocess = CASE WHEN EDS.ToReprocess IS NOT NULL THEN EDS.ToReprocess ELSE 'Yes' END
	                                        FROM
                                                (
                                                  SELECT * FROM EmployeeInformation WHERE --JobLocationID IN (SELECT SystemID FROM [dbo].[JobLocation] WHERE PlantID = '" + sPlantID + @"')
                                                            SystemID IN (
                                                                         SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"')
                                                                            WHERE JobLcSystemID IN (
                                                                                                    SELECT SystemID FROM [dbo].[JobLocation]
                                                                                                        WHERE PlantID = '" + sPlantID + @"'
                                                                                                   )
                                                                        )
                                                ) AS E
			                                        LEFT JOIN (SELECT * FROM dbo.EmpDateWiseShiftAssign
                                                                    WHERE WorkDate = '" + sAttnDate + @"') EDS ON E.SystemID = EDS.EmpSystemID
		                                        WHERE (E.DOS > '" + sAttnDate + @"' OR E.DOS IS NULL)
                                         ) A WHERE ToReprocess = 'Yes'
                                         ORDER BY EmployeeCode";
                //var x = _sqlRepository.GetGridData(parameters, strSQL).Source;
                //return x;
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmpDateWiseShiftAssign(string sEmpSystemIDColl)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT *
                            FROM dbo.EmpDateWiseShiftAssign
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @")";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmployeeWeekOffByDay(string sEmpSystemIDColl)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT *
                            FROM dbo.EmployeeWeekOffByDay
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @")";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetCompanyAssignWeekOffDateRangeWise(string sGroupID, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT A.* FROM dbo.OffDayDetail A
			                            INNER JOIN (SELECT * FROM dbo.OffDayMaster WHERE OffDayType = 'W') B ON A.OffDayMstSystemID = B.SystemID
                            WHERE A.OffDayDate = '" + sAttnDate + @"'
                                  AND A.GroupID = '" + sGroupID + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetUpdatedEmpShiftAssignBeforeFromDate(string sEmpSystemIDColl, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT A.* FROM dbo.EmployeeShiftAssign A
                            INNER JOIN (
                                         SELECT EmpSystemID, MAX(EffectiveDate) EffectiveDate FROM dbo.EmployeeShiftAssign
                                            WHERE EffectiveDate <= '" + sAttnDate + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")
                                            GROUP BY EmpSystemID
                                        ) B ON A.EmpSystemID = B.EmpSystemID AND A.EffectiveDate = B.EffectiveDate";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetSftRstDayCount(string sEmpSystemIDColl, string dtLastDt, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT *
                            FROM dbo.EmpDateWiseShiftAssign
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @") AND
                                    WorkDate BETWEEN '" + dtLastDt + "' AND '" + sAttnDate + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmployeeShiftAssignInDateRange(string sEmpSystemIDColl, string sAttnDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.EmployeeShiftAssign
                                WHERE EffectiveDate = '" + sAttnDate + @"'
                                            AND EmpSystemID IN (" + sEmpSystemIDColl + @")";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetShiftRosterChild(string sGroupID)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.ShiftRosterChild
                            WHERE GroupID = '" + sGroupID + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetAttdnProcessData(string sEmpSystemIDColl, string sDate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT * FROM dbo.AttdnProcessData
                            WHERE WorkDate = '" + sDate + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SaveDataSets(DataSet dsRawData, DataSet dsAttnProcData, DataSet dsEmpNotiData, DataSet dsLvTransDtl, DataSet dsLvAllo)
        {
            List<AttdnRawData> AttdnRawDataList = null;
            List<AttdnProcessData> AttdnProcessDataList = null;
            List<EmployeeNotifications> EmployeeNotificationsList = null;
            List<LeaveTransactionDetails> LeaveTransactionDetailsList = null;
            List<LeaveAllocation> LeaveAllocationList = null;

            var flag = false;
            try
            {
                InitAttdnRawData(dsRawData, out AttdnRawDataList);
                InitAttdnProcessData(dsAttnProcData, out AttdnProcessDataList);
                InitEmployeeNotifications(dsEmpNotiData, out EmployeeNotificationsList);
                InitLeaveAllocation(dsLvAllo, out LeaveAllocationList);
                InitLeaveTransactionDetails(dsLvTransDtl, out LeaveTransactionDetailsList);

                foreach (var item in AttdnRawDataList)
                {
                    _rs.InsertOrUpdateGraph(item);
                }
                foreach (var item in AttdnProcessDataList)
                {
                    InsertOrUpdateGraph(item);
                }
                foreach (var item in EmployeeNotificationsList)
                {
                    _en.InsertOrUpdateGraph(item);
                }
                foreach (var item in LeaveAllocationList)
                {
                    _la.InsertOrUpdateGraph(item);
                }
                foreach (var item in LeaveTransactionDetailsList)
                {
                    _ltd.InsertOrUpdateGraph(item);
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private static void InitAttdnRawData(DataSet dsRawData, out List<AttdnRawData> AttdnRawDataList)
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
                        var ob = new AttdnRawData
                        {
                            Id = dsRawData.Tables[0].Rows[i]["Id"].ToString(),
                            RowId = Convert.ToInt32(dsRawData.Tables[0].Rows[i]["RowId"].ToString()),
                            DeviceId = Convert.ToInt32(dsRawData.Tables[0].Rows[i]["AddedBy"].ToString()),
                            DevSystemId = dsRawData.Tables[0].Rows[i]["DevSystemId"].ToString(),
                            LogDownLoadNum = dsRawData.Tables[0].Rows[i]["LogDownLoadNum"].ToString(),
                            PDate = Convert.ToDateTime(dsRawData.Tables[0].Rows[i]["PDate"].ToString()),
                            PTime = Convert.ToDateTime(dsRawData.Tables[0].Rows[i]["PTime"].ToString()),
                            PType = dsRawData.Tables[0].Rows[i]["PType"].ToString(),
                            ProcessedFlag = Convert.ToBoolean(dsRawData.Tables[0].Rows[i]["ProcessedFlag"].ToString()),
                            AddedBy = dsRawData.Tables[0].Rows[i]["AddedBy"].ToString(),
                            DateAdded = Convert.ToDateTime(dsRawData.Tables[0].Rows[i]["DateAdded"].ToString()),
                            DateUpdated = Convert.ToDateTime(dsRawData.Tables[0].Rows[i]["DateUpdated"].ToString()),
                            UpdatedBy = dsRawData.Tables[0].Rows[i]["UpdatedBy"].ToString(),
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

        private static void InitAttdnProcessData(DataSet dsAttnProcData, out List<AttdnProcessData> AttdnRawDataList)
        {
            AttdnRawDataList = new List<AttdnProcessData>();
            try
            {
                for (int i = 0; i < dsAttnProcData.Tables[0].Rows.Count; i++)
                {
                    if (dsAttnProcData.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                        var ob = new AttdnProcessData
                        {
                            EmpSystemID = dsAttnProcData.Tables[0].Rows[i]["EmpSystemID"].ToString(),
                            WorkDate = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["WorkDate"].ToString()),
                            GroupID = dsAttnProcData.Tables[0].Rows[i]["GroupID"].ToString(),
                            PlantID = dsAttnProcData.Tables[0].Rows[i]["PlantID"].ToString(),
                            ShiftSystemID = dsAttnProcData.Tables[0].Rows[i]["ShiftSystemID"].ToString(),
                            InTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["InTime"].ToString()),
                            IsManualInTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualInTime"].ToString()),
                            OutTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["OutTime"].ToString()),
                            IsManualOutTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualOutTime"].ToString()),
                            DayStatus = dsAttnProcData.Tables[0].Rows[i]["DayStatus"].ToString(),
                            IsManualDayStatus = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualDayStatus"].ToString()),
                            OTHr = Convert.ToDecimal(dsAttnProcData.Tables[0].Rows[i]["OTHr"].ToString()),
                            IsOTComfirm = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsOTComfirm"].ToString()),
                            DateOTComfirm = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["DateOTComfirm"].ToString()),
                            OTComfirmBy = dsAttnProcData.Tables[0].Rows[i]["OTComfirmBy"].ToString(),
                            LTSystemID = dsAttnProcData.Tables[0].Rows[i]["LTSystemID"].ToString(),
                            IsLock = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsLock"].ToString()),
                            ToReprocess = dsAttnProcData.Tables[0].Rows[i]["ToReprocess"].ToString(),
                            InTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["InTimeRowID"].ToString()),
                            OutTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["OutTimeRowID"].ToString()),

                            ModelState = ModelState.Added
                        };
                        AttdnRawDataList.Add(ob);
                    }
                    else
                    {
                        var ob = new AttdnProcessData
                        {
                            // ob.EmpSystemID = dsAttnProcData.Tables[0].Rows[i]["EmpSystemID"].ToString();
                            WorkDate = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["WorkDate"].ToString()),
                            //ob.GroupID = dsAttnProcData.Tables[0].Rows[i]["GroupID"].ToString();
                            //ob.PlantID = dsAttnProcData.Tables[0].Rows[i]["PlantID"].ToString();
                            ShiftSystemID = dsAttnProcData.Tables[0].Rows[i]["ShiftSystemID"].ToString(),
                            InTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["InTime"].ToString()),
                            IsManualInTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualInTime"].ToString()),
                            OutTime = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["OutTime"].ToString()),
                            IsManualOutTime = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualOutTime"].ToString()),
                            DayStatus = dsAttnProcData.Tables[0].Rows[i]["DayStatus"].ToString(),
                            IsManualDayStatus = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsManualDayStatus"].ToString()),
                            OTHr = Convert.ToDecimal(dsAttnProcData.Tables[0].Rows[i]["OTHr"].ToString()),
                            IsOTComfirm = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsOTComfirm"].ToString()),
                            DateOTComfirm = Convert.ToDateTime(dsAttnProcData.Tables[0].Rows[i]["DateOTComfirm"].ToString()),
                            OTComfirmBy = dsAttnProcData.Tables[0].Rows[i]["OTComfirmBy"].ToString(),
                            LTSystemID = dsAttnProcData.Tables[0].Rows[i]["LTSystemID"].ToString(),
                            IsLock = Convert.ToBoolean(dsAttnProcData.Tables[0].Rows[i]["IsLock"].ToString()),
                            ToReprocess = dsAttnProcData.Tables[0].Rows[i]["ToReprocess"].ToString(),
                            InTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["InTimeRowID"].ToString()),
                            OutTimeRowID = Convert.ToInt32(dsAttnProcData.Tables[0].Rows[i]["OutTimeRowID"].ToString()),

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

        private static void InitEmployeeNotifications(DataSet dsEmpNotiData, out List<EmployeeNotifications> EmployeeNotificationsList)
        {
            EmployeeNotificationsList = new List<EmployeeNotifications>();
            try
            {
                for (int i = 0; i < dsEmpNotiData.Tables[0].Rows.Count; i++)
                {
                    if (dsEmpNotiData.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                        var ob = new EmployeeNotifications
                        {
                            SystemID = Convert.ToDecimal(dsEmpNotiData.Tables[0].Rows[i]["SystemID"].ToString()),
                            EmpInfoSystemID = dsEmpNotiData.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(),
                            EventSourceTableSystemID = dsEmpNotiData.Tables[0].Rows[i]["EventSourceTableSystemID"].ToString(),
                            EventDate = Convert.ToDateTime(dsEmpNotiData.Tables[0].Rows[i]["EventDate"].ToString()),
                            EventRaisedBy = dsEmpNotiData.Tables[0].Rows[i]["EventRaisedBy"].ToString(),
                            EventType = dsEmpNotiData.Tables[0].Rows[i]["EventType"].ToString(),
                            IsDelivered = Convert.ToBoolean(dsEmpNotiData.Tables[0].Rows[i]["IsDelivered"].ToString()),
                            WorkDate = Convert.ToDateTime(dsEmpNotiData.Tables[0].Rows[i]["WorkDate"].ToString()),
                            ModelState = ModelState.Modified
                        };
                        EmployeeNotificationsList.Add(ob);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void InitLeaveTransactionDetails(DataSet dsLvTransDtl, out List<LeaveTransactionDetails> LeaveTransactionDetailsList)
        {
            LeaveTransactionDetailsList = new List<LeaveTransactionDetails>();
            try
            {
                for (int i = 0; i < dsLvTransDtl.Tables[0].Rows.Count; i++)
                {
                    if (dsLvTransDtl.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                    }
                    else
                    {
                        var ob = new LeaveTransactionDetails
                        {
                            SystemID = dsLvTransDtl.Tables[0].Rows[i]["SystemID"].ToString(),
                            LvTrnsSystemID = dsLvTransDtl.Tables[0].Rows[i]["LvTrnsSystemID"].ToString(),
                            WorkDate = Convert.ToDateTime(dsLvTransDtl.Tables[0].Rows[i]["WorkDate"].ToString()),
                            DayType = dsLvTransDtl.Tables[0].Rows[i]["DayType"].ToString(),
                            LeaveStatus = dsLvTransDtl.Tables[0].Rows[i]["LeaveStatus"].ToString(),
                            IsAvailed = Convert.ToBoolean(dsLvTransDtl.Tables[0].Rows[i]["IsAvailed"].ToString()),
                            ModelState = ModelState.Modified
                        };
                        LeaveTransactionDetailsList.Add(ob);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void InitLeaveAllocation(DataSet dsLvTransDtl, out List<LeaveAllocation> LeaveAllocationList)
        {
            LeaveAllocationList = new List<LeaveAllocation>();
            try
            {
                for (int i = 0; i < dsLvTransDtl.Tables[0].Rows.Count; i++)
                {
                    if (dsLvTransDtl.Tables[0].Rows[i].RowState == DataRowState.Added)
                    {
                    }
                    else
                    {
                        var ob = new LeaveAllocation
                        {
                            YrCalSystemID = dsLvTransDtl.Tables[0].Rows[i]["YrCalSystemID"].ToString(),
                            EmpSystemID = dsLvTransDtl.Tables[0].Rows[i]["EmpSystemID"].ToString(),
                            LvPolDetailsSystemID = dsLvTransDtl.Tables[0].Rows[i]["LvPolDetailsSystemID"].ToString(),
                            LeaveDays = Convert.ToInt32(dsLvTransDtl.Tables[0].Rows[i]["LeaveDays"].ToString()),
                            AppliedLeave = Convert.ToInt32(dsLvTransDtl.Tables[0].Rows[i]["AppliedLeave"].ToString()),
                            AvailedLeave = Convert.ToInt32(dsLvTransDtl.Tables[0].Rows[i]["AvailedLeave"].ToString()),
                            GroupID = dsLvTransDtl.Tables[0].Rows[i]["GroupID"].ToString(),
                            PlantID = dsLvTransDtl.Tables[0].Rows[i]["PlantID"].ToString(),
                            ModelState = ModelState.Modified
                        };
                        LeaveAllocationList.Add(ob);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string CardNumConvert(string strRawCardNum, int i)
        {
            var TmpNum = "";
            var TmpNum1 = "";
            var TmpNum2 = "";
            var Hex1 = "";
            var Hex2 = "";

            if (strRawCardNum.Length < 8)
            {
                for (int j = strRawCardNum.Length; j < 8; j++)
                {
                    strRawCardNum = "0" + strRawCardNum;
                }
            }
            if (strRawCardNum.Length == 8)
            {
                TmpNum = strRawCardNum;
                Hex1 = Convert.ToInt32(strRawCardNum.Substring(0, 3)).ToString("X");
                Hex2 = Convert.ToInt32(strRawCardNum.Substring(3, 5)).ToString("X");

                if (Hex1.Length == 2 & Hex2.Length < 4)
                {
                    Hex1 = Hex1 + "0";
                }
                TmpNum1 = Hex1 + Hex2;
                TmpNum2 = CardDecimal(TmpNum1, i);

                //this.txtCardNumberDec.Text = TmpNum2;
                //this.txtCardNumberHexDec.Text = CardHex(TmpNum2, i);
            }
            return TmpNum2;
        }//End Function

        private static string CardDecimal(string ProxcardNo, int i)
        {
            var Idcrd = "";
            Idcrd = (TableConvert.Convert(ProxcardNo)).ToString();
            //this.lblCardNumberDec.Text = Idcrd.Trim();

            if (i > Idcrd.Length)
            {
                for (int j = Idcrd.Length; j < i; j++)
                {
                    Idcrd = "0" + Idcrd;
                }
            }
            return Idcrd;
        }//End Function

        public enum NotificationType
        {
            Attendance,

            Salary,
            SalaryDisbursement,
            SalaryApproval,
            SalaryApprovalRollback,

            Promotion,
            PromotionRollback,

            Increment,
            IncrementRollback,

            GeneralAnnouncement,
            Holiday,
            Birthday
        }
    }

    public static class DecimalHelper
    {
        public static string ToHexString(this Decimal dec)
        {
            var sb = new StringBuilder();
            while (dec > 1)
            {
                var r = dec % 16;
                dec /= 16;
                sb.Insert(0, ((int)r).ToString("X"));
            }
            return sb.ToString();
        }
    }//End Function

    public class TableConvert
    {
        private static readonly sbyte[] unhex_table =
      { -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       , 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,-1,-1,-1,-1,-1,-1
       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
      };

        public static int Convert(string hexNumber)
        {
            int decValue = unhex_table[(byte)hexNumber[0]];
            for (int i = 1; i < hexNumber.Length; i++)
            {
                decValue *= 16;
                decValue += unhex_table[(byte)hexNumber[i]];
            }
            return decValue;
        }
    }//End Function
   
}