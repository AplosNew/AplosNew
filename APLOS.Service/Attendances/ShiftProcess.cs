using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
//using System.Collections.Generic;

namespace TBS
{
    public class ShiftProcess
    {
        void GetEmpList(string _plantid, string sAttnDate, string GroupSysID, string _emplist, out DataSet dsEmpMainLoop, out string sEmpSysIDCollForSft)
        {
            try
            {
                sEmpSysIDCollForSft = "";
                dsEmpMainLoop = null;
                if (_emplist.Length == 0)
                {
                    GetEmployeeInformationForShiftProcess(_plantid, "", sAttnDate.Trim(), out dsEmpMainLoop);
                }
                else
                {
                    GetEmployeeInformationForShiftProcess(_plantid, _emplist, sAttnDate.Trim(), out dsEmpMainLoop);
                }
                if (dsEmpMainLoop.Tables[0].Rows.Count > 0)
                {

                    if (_emplist.Length == 0)
                    {
                        for (int i = 0; i < dsEmpMainLoop.Tables[0].Rows.Count; i++)
                        {
                            if (sEmpSysIDCollForSft.Trim() == "")
                            {
                                sEmpSysIDCollForSft = "'" + dsEmpMainLoop.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                sEmpSysIDCollForSft = sEmpSysIDCollForSft.Trim() + ", '" + dsEmpMainLoop.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                        }

                    }//_emplist
                    else
                    {
                        sEmpSysIDCollForSft = _emplist;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        ShiftDateWise GetShiftInfo(DataSet ds, string pDate, string _empid)
        {
            ShiftDateWise sdw = null;
            try
            {
                DataView dv = new DataView(ds.Tables[0]);
                dv.RowFilter = "Empsystemid='" + _empid + "' and workdate='" + pDate + "'";
                if (dv.Count > 0)
                {
                    sdw = new ShiftDateWise();
                    sdw.DayType = dv[0]["DayType"].ToString();
                    sdw.EmpSftAssiSystemID = dv[0]["EmpSftAssiSystemID"].ToString();
                    sdw.EmpSystemID = dv[0]["EmpSystemID"].ToString();
                    sdw.RosterShiftDayCount = Convert.ToInt32(dv[0]["RosterShiftDayCount"].ToString());
                    sdw.ShiftSystemID = dv[0]["ShiftSystemID"].ToString();
                    sdw.ToReprocess = dv[0]["ToReprocess"].ToString();
                    sdw.WorkDate = dv[0]["WorkDate"].ToString();
                    //sdw.IsRoster = Convert.ToBoolean(dv[0]["IsRoster"].ToString());
                    //sdw.RosterStartShiftID = dv[0]["RosterStartShiftID"].ToString();
                    //888
                }
                return sdw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        ShiftDateWise GetShiftInfo_Prev(DataSet ds, string pDate, string _empid)
        {
            ShiftDateWise sdw = null;
            try
            {
                DataView dv = new DataView(ds.Tables[0]);
                dv.RowFilter = "Empsystemid='" + _empid + "' and workdate='" + pDate + "'";
                if (dv.Count > 0)
                {
                    sdw = new ShiftDateWise();
                    sdw.DayType = dv[0]["DayType"].ToString();
                    sdw.EmpSftAssiSystemID = dv[0]["EmpSftAssiSystemID"].ToString();
                    sdw.EmpSystemID = dv[0]["EmpSystemID"].ToString();
                    sdw.RosterShiftDayCount = Convert.ToInt32(dv[0]["RosterShiftDayCount"].ToString());
                    sdw.ShiftSystemID = dv[0]["ShiftSystemID"].ToString();
                    sdw.ToReprocess = dv[0]["ToReprocess"].ToString();
                    sdw.WorkDate = dv[0]["WorkDate"].ToString();
                    sdw.IsRoster = Convert.ToBoolean(dv[0]["IsRoster"].ToString());
                    sdw.RosterSystemID = dv[0]["RosterSystemID"].ToString();
                    sdw.RosterStartShiftID = dv[0]["RosterStartShiftID"].ToString();
                    //888
                }
                return sdw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ShiftProcessStart(string _plantid, string WorkingDate, string GroupSysID, string _emplist)
        {
            #region DataSet Declare

            DataSet dsEmpMainLoop = null;

            DataSet dsDateWiseShiftSave = null;
            DataTable dtDateWiseShiftSave = null;
            //DataRow drDateWiseShiftSave = null;
            DataView dvDateWiseShiftSave = null;

            DataSet dsDateWiseShift_Prev = null;
            DataTable dtDateWiseShift_Prev = null;
            //DataRow drDateWiseShift_Prev = null;
            DataView dvDateWiseShift_Prev = null;

            DataSet dsEmpWkOff = null;
            DataTable dtEmpWkOff = null;
            DataView dvEmpWkOff = null;

            DataSet dsComAssWkOff = null;
            DataTable dtComAssWkOff = null;
            DataView dvComAssWkOff = null;

            DataSet dsDayType = null;
            DataTable dtDayType = null;
            DataView dvDayType = null;

            //DataSet dsDateWiseTwoDays = null;
            //DataTable dtDateWiseTwoDays = null;
            //DataView dvDateWiseTwoDays = null;

            DataSet dsAssignedCurrent = null;
            DataTable dtAssignedCurrent = null;
            DataView dvAssignedCurrent = null;

            //DataSet dsAssignedBefore = null;
            //DataTable dtAssignedBefore = null;
            //DataView dvAssignedBefore = null;

            //DataSet dsAssignedBetween = null;
            //DataTable dtAssignedBetween = null;
            //DataView dvAssignedBetween = null;

            DataSet dsSftRstCdl = null;
            DataTable dtSftRstCdl = null;
            DataView dvSftRstCdl = null;

            DataSet dsSftDft = null;
            DataSet dsIdLast = null;
            DataTable dtIdLast = null;
            DataView dvIdLast = null;

            //DataSet dsAttdnProc = null;
            //DataTable dtAttdnProc = null;
            //DataView dvAttdnProc = null;
            //DataRow drAttdnProc = null;

            #endregion DataSet Declare

            try
            {
                string sEmpSysIDCollForSft = "";
                GetEmpList(_plantid, WorkingDate, GroupSysID, _emplist, out dsEmpMainLoop, out sEmpSysIDCollForSft);

                if (dsEmpMainLoop.Tables[0].Rows.Count > 0)
                {
                    #region DataSet

                    string dtLastDt = Convert.ToDateTime(WorkingDate).AddDays(-1).ToString("dd-MMM-yyyy");
                    GetDayType(out dsDayType);
                    dtDayType = dsDayType.Tables[0];
                    dvDayType = new DataView();

                    List<dicShiftDft> dicShiftDft = new List<dicShiftDft>();
                    GetShiftDefination(GroupSysID, _plantid, out dsSftDft);
                    if (dsSftDft.Tables[0].Rows.Count > 0)
                        dicShiftDft = dsSftDft.Tables[0].ToList<dicShiftDft>();

                    GetDateWiseShift_Prev(sEmpSysIDCollForSft.Trim(), dtLastDt, out dsDateWiseShift_Prev);
                    dtDateWiseShift_Prev = dsDateWiseShift_Prev.Tables[0];
                    dvDateWiseShift_Prev = new DataView();
                    //GetEmpDateWiseShiftAssignWithDateRange(sEmpSysIDCollForSft.Trim(), dtLastDt, sAttnDate.Trim(), out dsEmpDtWiseSftAss);
                    GetDateWiseShift_Save(sEmpSysIDCollForSft.Trim(), WorkingDate.Trim(), out dsDateWiseShiftSave);
                    dtDateWiseShiftSave = dsDateWiseShiftSave.Tables[0];
                    dvDateWiseShiftSave = new DataView();

                    DataSet dsCurrWeekOff = null;
                    GetEmpCurrWeekOff(WorkingDate, sEmpSysIDCollForSft.Trim(), out dsCurrWeekOff);

                    GetEmployeeWeekOffByDay(WorkingDate, sEmpSysIDCollForSft.Trim(), out dsEmpWkOff);
                    dtEmpWkOff = dsEmpWkOff.Tables[0];
                    dvEmpWkOff = new DataView();

                    GetCompanyAssignWeekOffDateRangeWise(GroupSysID, _plantid, WorkingDate.Trim(), out dsComAssWkOff);
                    dtComAssWkOff = dsComAssWkOff.Tables[0];
                    dvComAssWkOff = new DataView();

                    GetUpdatedEmpShiftAssignBeforeFromDate(sEmpSysIDCollForSft.Trim(), WorkingDate.Trim(), out dsAssignedCurrent);
                    dtAssignedCurrent = dsAssignedCurrent.Tables[0];
                    dvAssignedCurrent = new DataView();

                    //GetSftRstDayCount(sEmpSysIDCollForSft.Trim(), dtLastDt.Trim(), WorkingDate.Trim(), out dsDateWiseTwoDays);
                    //dtDateWiseTwoDays = dsDateWiseTwoDays.Tables[0];
                    //dvDateWiseTwoDays = new DataView();

                    //GetEmployeeShiftAssignBeforeFromDate(sEmpSysIDCollForSft.Trim(), WorkingDate, out dsAssignedBefore);
                    //dtAssignedBefore = dsAssignedBefore.Tables[0];
                    //dvAssignedBefore = new DataView();

                    //GetEmployeeShiftAssignInDateRange(sEmpSysIDCollForSft.Trim(), dtLastDt, WorkingDate.Trim(), out dsAssignedBetween);
                    //dtAssignedBetween = dsAssignedBetween.Tables[0];
                    //dvAssignedBetween = new DataView();

                    GetShiftRosterChild(GroupSysID.Trim(), out dsSftRstCdl);
                    dtSftRstCdl = dsSftRstCdl.Tables[0];
                    dvSftRstCdl = new DataView();

                    //GetAttdnProcessData(sEmpSysIDCollForSft.Trim(), WorkingDate.Trim(), out dsAttdnProc);
                    //dtAttdnProc = dsAttdnProc.Tables[0];
                    //dvAttdnProc = new DataView();

                    #endregion DataSet

                    for (int i = 0; i < dsEmpMainLoop.Tables[0].Rows.Count; i++)
                    {
                        #region Declare Variable

                        string sEmpSystemID = dsEmpMainLoop.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                        string sPlantID = dsEmpMainLoop.Tables[0].Rows[i]["PlantID"].ToString().Trim();
                        string _DOJ = dsEmpMainLoop.Tables[0].Rows[i]["DOJ"].ToString().Trim();

                        int ShiftSequence = 0;
                        int DaysLengthShiftRoster = 0;
                        int WeekOffInShiftLenght = 0;

                        int RosterShiftDayCount = 0;
                        int RosterShiftSequence = 0;
                        int RosterShiftWeekOffCount = 0;
                        string RosterMstSysID = "";
                        string RosterChlSftSysID = "";
                        string RosterChlNewSftSysID = "";
                        string RosterChlOldSftSysID = "";
                        string RosterChlTempSftSysID = "";
                        string RosterChlOldTempSftSysID = "";
                        string sFixedDayInMonthShiftRoster = "";
                        string sFixedDayInMonthWeekOff = "";
                        string sWeekOffDay = "";
                        bool bInitialRstSftDyCnt = false;
                        bool bRstSftChange = false;
                        bool bIsFixedDayInMonthShiftRoster = false;
                        bool bIsDaysLengthShiftRoster = false;

                        bool bAlignWithCC = false;
                        bool bIsFixedDayInMonthWeekOff = false;
                        bool bIsDaysLengthWeekOff = false;
                        bool bIsWeekOffInShiftLenght = false;
                        bool bIndividualWeekOff = false;
                        bool bFstShiftDay = true;
                        bool bToDayTempShift = false;
                        bool bPrvDayTempShift = false;
                        bool bShiftProc = true;

                        string sFstOffDay = "";
                        string sFstDayLengthType = "";
                        string sSndOffDay = "";
                        string sSndDayLengthType = "";
                        string sEmpSftAssCurntSysID = "";
                        string sEmpSftAssTempSysID = "";

                        string sDayType = "NW";
                        string sDayLengthType = "Normal Workday";
                        string sSfTime = "00:00:00";

                        DateTime dtStDt = Convert.ToDateTime(WorkingDate);
                        //DateTime dtFrmD = Convert.ToDateTime(WorkingDate);
                        //DateTime dtToD = Convert.ToDateTime(WorkingDate);

                        #endregion Declare Variable

                        //while (dtStDt <= dtToD)
                        //{//check in the table 'EmpDateWiseShiftAssign', EmpSystemID and WorkDate are already available
                        #region Initialize

                        bRstSftChange = false;
                        string strStDt = dtStDt.ToString("dd-MMM-yyyy");
                        bool bInitialShift = true;
                        bAlignWithCC = false;
                        bIndividualWeekOff = false;
                        bToDayTempShift = false;
                        bPrvDayTempShift = false;
                        bShiftProc = true;

                        sFstOffDay = "";
                        sFstDayLengthType = "";
                        sSndOffDay = "";
                        sSndDayLengthType = "";
                        sSfTime = "";

                        sDayType = "NW";
                        sDayLengthType = "Normal Workday";
                        var _PrevDate = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");

                        #endregion Initialize

                        GetOffDay(dtComAssWkOff, dsCurrWeekOff.Tables[0], strStDt, sEmpSystemID, out sDayType);
                        ShiftDateWise Shift_Current = GetShiftInfo(dsDateWiseShiftSave, strStDt, sEmpSystemID);
                        ShiftDateWise Shift_PrevDay = GetShiftInfo_Prev(dsDateWiseShift_Prev, _PrevDate, sEmpSystemID);

                        string _effectiveDate = DateTime.Now.ToString("dd-MMM-yyyy");
                        DataView dvAssign = new DataView(dtAssignedCurrent);
                        dvAssign.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                        if (dvAssign.Count > 0)
                        {
                            _effectiveDate = Convert.ToDateTime(dvAssign[0]["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy");
                            if (_effectiveDate != WorkingDate && Shift_PrevDay == null)//today is not first day and last day was not processed
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        dvDateWiseShiftSave.Table = dtDateWiseShiftSave;
                        dvDateWiseShiftSave.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + strStDt + "'";
                        if (dvDateWiseShiftSave.Count > 0)//update
                        {
                            bool IsManuallyChanged = Convert.ToBoolean(dvDateWiseShiftSave[0]["IsManuallyChanged"].ToString().Trim().ToUpper());
                            if (dvDateWiseShiftSave[0]["ToReprocess"].ToString().Trim().ToUpper() == "YES" && IsManuallyChanged == false)//IsManuallyChanged add also ismanually updated valida here
                            {
                                dvAssignedCurrent.Table = dtAssignedCurrent;
                                dvAssignedCurrent.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                                //if (dtStDt == dtFrmD || dvAssignedBetween.Count == 0)
                                if (dvAssignedCurrent.Count == 0)
                                {
                                    ///delete already inserted date wise shift innfo     
                                    var dr = dvDateWiseShiftSave[0].Row;
                                    dr.BeginEdit();
                                    dr.Delete();
                                    dr.EndEdit();
                                }
                                else if (dvAssignedCurrent.Count > 0)
                                {
                                    if (Convert.ToBoolean(dvAssignedCurrent[0]["IsRoster"].ToString().Trim()) == true)
                                    {
                                        dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");
                                        ShiftDateWise objS = new ShiftDateWise();
                                        bool IsOk = false;
                                        //core
                                        CoreProcess(out IsOk, out objS, Shift_PrevDay, sEmpSystemID, sDayType, _effectiveDate, WorkingDate, dvAssignedCurrent, dtSftRstCdl);

                                        if (IsOk)
                                        {
                                            SetRowValueUpdate(ref dvDateWiseShiftSave, objS, sSfTime, GroupSysID, sPlantID);
                                        }
                                    }//if                                                                                                          
                                }//dvAssignedCurrent.Count > 0
                            }//ToReprocess
                        }
                        else//insert
                        {
                            #region Insert
                            dvAssignedCurrent.Table = dtAssignedCurrent;
                            dvAssignedCurrent.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                            if (dvAssignedCurrent.Count > 0)
                            {
                                var _dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");
                                if (Convert.ToBoolean(dvAssignedCurrent[0]["IsRoster"].ToString().Trim()) == true)
                                {
                                    ShiftDateWise objS = new ShiftDateWise();
                                    bool IsOk = false;
                                    CoreProcess(out IsOk, out objS, Shift_PrevDay, sEmpSystemID, sDayType, _effectiveDate, WorkingDate, dvAssignedCurrent, dtSftRstCdl);

                                    if (IsOk)
                                    {
                                        SetRowValue(ref dtDateWiseShiftSave, objS, sSfTime, GroupSysID, sPlantID);
                                    }
                                }//if roster                                                         
                            }//if (dvAssignedCurrent.Count > 0) 
                            #endregion
                        } //insert                           
                    }//for                    
                    SaveDataSets(dsDateWiseShiftSave);
                }//count
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsAssignedCurrent = null;
            }
        }//End Function  
        void ShouldGoForRostering(RosterDetailInfo rdi, string PrevDayType, string curr_day, string curr_date, out bool IsRostering)
        {
            IsRostering = false;
            try
            {
                if (rdi.RosteringPattern.ToUpper() == "INDIVIDUALWEEKOFF")
                {
                    if (PrevDayType == "W")//yestarday was off so today wil b new start
                    {
                        IsRostering = true;
                    }
                }
                else if (rdi.RosteringPattern.ToUpper() == "WEEKDAYS")
                {
                    if (curr_day.ToUpper() == rdi.DayName.ToUpper())//TBD (current dayname)
                    {
                        IsRostering = true;
                    }
                }
                else if (rdi.RosteringPattern.ToUpper() == "MULTIDATE")
                {
                    if (rdi.Dates.Contains(Convert.ToInt32(curr_date).ToString()))//TBD (current date)
                    {
                        IsRostering = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CoreProcess(out bool IsOk, out ShiftDateWise objS, ShiftDateWise Shift_PrevDay, string sEmpSystemID, string sDayType, string _effectiveDate, string WorkingDate, DataView dvAssignedCurrent, DataTable dtSftRstCdl)
        {
            objS = null;
            IsOk = false;
            try
            {
                objS = new ShiftDateWise();
                if (_effectiveDate == WorkingDate)//first time
                {
                    //get shift assign 
                    //set value
                    GetShiftNew(dvAssignedCurrent, ref objS);
                    IsOk = true;
                }
                else if (Shift_PrevDay != null)//and if old is roster
                {
                    if (Shift_PrevDay.IsRoster)
                    {
                        //List<string> _Dates = new List<string>();
                        //string _DAY_NAME = string.Empty;
                        //string RosteringPattern = string.Empty;
                        bool IsRostering = false;
                        RosterDetailInfo rdi = null;
                        GetRosteringPattern(dtSftRstCdl, Shift_PrevDay.RosterSystemID, out rdi);
                        //if pattern ok go for roster
                        DateTime _wd = Convert.ToDateTime(WorkingDate);
                        ShouldGoForRostering(rdi, Shift_PrevDay.DayType, _wd.ToString("dddd"), _wd.ToString("dd"), out IsRostering);

                        if (IsRostering)//yestarday was off so today wil b new start (Shift_PrevDay.DayType == "W")
                        {
                            string _newShift = string.Empty;
                            GetShiftNew(dvAssignedCurrent, ref objS);
                            //GetRostedOrFresh(objS.RosterShiftDayCount, dtSftRstCdl, dvAssignedCurrent[0]["RosterSystemID"].ToString(), objS.ShiftSystemID, out _newShift);
                            GetRostedOrFresh(objS.RosterShiftDayCount, dtSftRstCdl, Shift_PrevDay.RosterSystemID, Shift_PrevDay.ShiftSystemID, out _newShift);
                            objS.ShiftSystemID = _newShift;
                        }
                        else//roster continues
                        {
                            GetShiftOld(Shift_PrevDay, ref objS);
                        }
                    }
                    else//yestarday was fix and today wil b new start
                    {
                        GetShiftNew(dvAssignedCurrent, ref objS);
                    }
                    IsOk = true;
                }
                //else
                //{
                //    continue;
                //}
                objS.DayType = sDayType;
                objS.EmpSystemID = sEmpSystemID;
                objS.WorkDate = WorkingDate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetRostedOrFresh(int DayCount, DataTable dtRoster, string RosterId, string ShiftIdOld, out string ShiftIdNew)
        {
            int _seq = 0;
            ShiftIdNew = string.Empty;
            //int RosterLength = 0;
            try
            {
                DataView dv = new DataView(dtRoster);
                dv.RowFilter = "SRMasterSystemID='" + RosterId + "' and ShiftDefinationID='" + ShiftIdOld + "'";
                if (dv.Count > 0)
                {
                    _seq = Convert.ToInt32(dv[0]["ShiftSequence"].ToString());
                    //RosterLength = Convert.ToInt32(dv[0]["DaysLengthShiftRoster"].ToString());
                }
                dv.RowFilter = null;

                object maxObject;
                maxObject = dtRoster.Compute("max(ShiftSequence)", "SRMasterSystemID='" + RosterId + "'");
                //if(Convert.ToInt32(maxObject)>)

                //if (DayCount<=RosterLength)//next roster
                if (Convert.ToInt32(maxObject) > _seq)
                {
                    _seq++;
                    DataView dvNew = new DataView(dtRoster);
                    dvNew.RowFilter = "SRMasterSystemID='" + RosterId + "' and ShiftSequence=" + _seq + "";
                    if (dvNew.Count > 0)
                    {
                        ShiftIdNew = dvNew[0]["ShiftDefinationID"].ToString();
                    }
                }
                else//Fresh Start of roster
                {
                    DataView dvNew = new DataView(dtRoster);
                    dvNew.RowFilter = "SRMasterSystemID='" + RosterId + "' and ShiftSequence=1";
                    if (dvNew.Count > 0)
                    {
                        ShiftIdNew = dvNew[0]["ShiftDefinationID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetRosteringPattern(DataTable dtRoster, string RosterId, out RosterDetailInfo RosteringPattern)
        {
            //int _seq = 0;
            RosteringPattern = new RosterDetailInfo();
            //int RosterLength = 0;
            try
            {
                DataView dv = new DataView(dtRoster);
                dv.RowFilter = "SRMasterSystemID='" + RosterId + "'";
                //DataTable dtR = dv.ToTable(true, "RosteringPattern");
                if (dv.Count > 0)
                {
                    var v = dv[0]["RosteringPattern"].ToString();
                    RosteringPattern.RosteringPattern = v;
                    RosteringPattern.DayName = dv[0]["WeekDays"].ToString();
                    RosteringPattern.Dates = dv[0]["MultiDate"].ToString().Split(',');
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetShiftOld(ShiftDateWise Shift_PrevDay, ref ShiftDateWise objS)
        {
            try
            {
                objS.EmpSftAssiSystemID = Shift_PrevDay.EmpSftAssiSystemID;
                objS.RosterShiftDayCount = Shift_PrevDay.RosterShiftDayCount + 1;
                objS.ShiftSystemID = Shift_PrevDay.ShiftSystemID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetShiftNew(DataView dvAssigned, ref ShiftDateWise objS)
        {
            try
            {
                objS = new ShiftDateWise();
                objS.EmpSftAssiSystemID = dvAssigned[0]["SystemId"].ToString();
                objS.RosterShiftDayCount = 1;
                objS.ShiftSystemID = dvAssigned[0]["RosterStartShiftID"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetOffDay(DataTable dtComAssWkOff, DataTable dtEmpWkOff, string Workdate, string _empSystemid, out string sDayType)
        {
            sDayType = "NW";
            DataView dvComAssWkOff = null;
            bool IsIndividualWeekOff = false;
            bool AlignWithCC = false;
            try
            {
                DataView dvWeekInfo = new DataView(dtEmpWkOff);
                dvWeekInfo.RowFilter = "EmpSystemID='" + _empSystemid + "'";
                if (dvWeekInfo.Count > 0)
                {
                    IsIndividualWeekOff = Convert.ToBoolean(dvWeekInfo[0]["IndividualWeekOff"].ToString().Trim());
                    AlignWithCC = Convert.ToBoolean(dvWeekInfo[0]["AlignWithCC"].ToString().Trim());
                }

                if (IsIndividualWeekOff == false)
                {
                    dvComAssWkOff = new DataView();
                    dvComAssWkOff.Table = dtComAssWkOff;
                    dvComAssWkOff.RowFilter = "OffDayDate = '" + Workdate + "' ";
                    if (dvComAssWkOff.Count > 0)
                    {
                        var sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();
                        if (sDayLengthType.ToUpper() == "FULL DAY" || sDayLengthType.ToUpper() == "FULLDAY")
                        {
                            sDayType = "W";
                        }
                    }
                }
                else
                {
                    var _FstOffDay = dvWeekInfo[0]["FstOffDay"].ToString().Trim();
                    var d = Convert.ToDateTime(Workdate).ToString("dddd");
                    if (_FstOffDay.ToUpper() == d.ToUpper())
                    {
                        sDayType = "W";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataTable dtDateWiseShiftSave, ShiftDateWise sw, string sSfTime, string GroupSysID, string sPlantID)
        {
            DataRow dr = null;
            try
            {
                dr = dtDateWiseShiftSave.NewRow();
                dr["EmpSystemID"] = sw.EmpSystemID;
                dr["WorkDate"] = sw.WorkDate;
                dr["EmpSftAssiSystemID"] = sw.EmpSftAssiSystemID;
                //dr["EmpSftAssiSystemID"] = dvAssignedBetween[0]["SystemID"].ToString().Trim();
                dr["ShiftSystemID"] = sw.ShiftSystemID;
                //dr["ShiftInTime"] = sSfTime;
                dr["DayType"] = sw.DayType;
                dr["AddedBy"] = "Schedule";
                dr["DateAdded"] = DateTime.Now;
                dr["RosterShiftDayCount"] = sw.RosterShiftDayCount;
                //dr["RosterShiftWeekOffCount"] = RosterShiftWeekOffCount;
                dr["AttdnLock"] = 0;
                dr["ToReprocess"] = "No";
                dr["GroupID"] = GroupSysID.Trim();
                dr["PlantID"] = sPlantID.Trim();
                dr["UpdatedBy"] = "Schedule";
                dr["DateUpdated"] = DateTime.Now;
                dtDateWiseShiftSave.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValueUpdate(ref DataView dtDateWiseShiftSave, ShiftDateWise sw, string sSfTime, string GroupSysID, string sPlantID)
        {
            //DataRow dr = null;
            try
            {
                var dr = dtDateWiseShiftSave[0].Row;
                dr.BeginEdit();
                dr["EmpSystemID"] = sw.EmpSystemID;
                dr["WorkDate"] = sw.WorkDate;
                dr["EmpSftAssiSystemID"] = sw.EmpSftAssiSystemID;
                //dr["EmpSftAssiSystemID"] = dvAssignedBetween[0]["SystemID"].ToString().Trim();
                dr["ShiftSystemID"] = sw.ShiftSystemID;
                //dr["ShiftInTime"] = sSfTime;
                dr["DayType"] = sw.DayType;
                dr["AddedBy"] = "Schedule";
                dr["DateAdded"] = DateTime.Now;
                dr["RosterShiftDayCount"] = sw.RosterShiftDayCount;
                //dr["RosterShiftWeekOffCount"] = RosterShiftWeekOffCount;
                dr["AttdnLock"] = 0;
                dr["ToReprocess"] = "No";
                dr["GroupID"] = GroupSysID.Trim();
                dr["PlantID"] = sPlantID.Trim();
                dr["UpdatedBy"] = "Schedule";
                dr["DateUpdated"] = DateTime.Now;
                dr.EndEdit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataTable dtDateWiseShiftSave, string sEmpSystemID, string strStDt,
            string RosterChlNewSftSysID, string sSfTime, string EmpSftAssiSystemID, string sDayType, string RosterShiftDayCount, string GroupSysID, string sPlantID)
        {
            DataRow dr = null;
            try
            {
                dr = dtDateWiseShiftSave.NewRow();
                dr["EmpSystemID"] = sEmpSystemID.Trim();
                dr["WorkDate"] = strStDt.Trim();
                dr["EmpSftAssiSystemID"] = EmpSftAssiSystemID;
                //dr["EmpSftAssiSystemID"] = dvAssignedBetween[0]["SystemID"].ToString().Trim();
                dr["ShiftSystemID"] = RosterChlNewSftSysID.Trim();
                dr["ShiftInTime"] = sSfTime;
                dr["DayType"] = sDayType.Trim();
                dr["AddedBy"] = "Schedule";
                dr["DateAdded"] = DateTime.Now;
                dr["RosterShiftDayCount"] = RosterShiftDayCount;
                //dr["RosterShiftWeekOffCount"] = RosterShiftWeekOffCount;
                dr["AttdnLock"] = 0;
                dr["ToReprocess"] = "No";
                dr["GroupID"] = GroupSysID.Trim();
                dr["PlantID"] = sPlantID.Trim();
                dr["UpdatedBy"] = "Schedule";
                dr["DateUpdated"] = DateTime.Now;
                dtDateWiseShiftSave.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private void SaveDataSets(params DataSet[] dsRef)
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
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        private void GetEmployeeInformationForShiftProcess(string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                string sEmpId = "";

                if (sEmpSysIdColl.Trim() != "")
                {
                    sEmpId = " AND SystemID IN (" + sEmpSysIdColl + @") ";
                }

                strSql = @"SELECT * FROM 
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
		                                        WHERE (E.DOS >= '" + sAttnDate + @"' OR E.DOS IS NULL or e.EmployeeStatus<>'Separated')
                                         ) A WHERE ToReprocess = 'Yes'  " + sEmpId + @"
                                         ORDER BY EmployeeCode";

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
        private void GetDayType(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.DayType";

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
        private void GetShiftDefination(string sGroupID, string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.ShiftDefination
                            WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

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
        private void dtIdList(string strIdCollection, out DataSet dsLocal)
        {
            dsLocal = new DataSet();

            strIdCollection = strIdCollection.Replace("'", "");
            string[] strIdCol = strIdCollection.Split(',');

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
        }//End Function
        private void GetDateWiseShift_Save(string sEmpSystemIDColl, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT *
                            FROM dbo.EmpDateWiseShiftAssign 
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @") 
                                   AND WorkDate ='" + sDate + @"'";

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
        private void GetDateWiseShift_Prev(string sEmpSystemIDColl, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT s.IsRoster,s.RosterSystemID,s.RosterStartShiftID,d.*  FROM dbo.EmpDateWiseShiftAssign d
                                left join EmployeeShiftAssign s on s.SystemID=d.EmpSftAssiSystemID
                             WHERE d.EmpSystemID IN (" + sEmpSystemIDColl + @") 
                                   AND d.WorkDate ='" + sDate + @"'";

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
        private void xGetEmpDateWiseShiftAssignWithDateRange(string sEmpSystemIDColl, string dtLastDt, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT *
                            FROM dbo.EmpDateWiseShiftAssign 
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @") 
                                   AND WorkDate BETWEEN '" + dtLastDt + @"' AND '" + sDate + @"'";

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
        private void GetEmployeeWeekOffByDay(string sAttnDate, string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"select * from 
                            (
                            SELECT max(EffectiveDate) EffectiveDate, EmpSystemID
                            FROM dbo.EmployeeWeekOffByDay
                            where EffectiveDate <= '" + sAttnDate + @"' 
                            group by EmpSystemID
                            ) d
                            left join EmployeeWeekOffByDay dd on dd.EffectiveDate = d.EffectiveDate and dd.EmpSystemID = d.EmpSystemID
                             WHERE d.EmpSystemID IN(" + sEmpSystemIDColl + @")";

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
        private void GetEmpCurrWeekOff(string sAttnDate, string empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT 
                                      [EmpSystemID]
                                      ,[FixSystemID]
                                      ,max(EffectiveDate) EffectiveDate
                                      ,[AlignWithCC]
                                      ,[IndividualWeekOff]
                                      ,[FstOffDay]
                                      ,[FstDayLengthType]
                                      ,[SndOffDay]
                                      ,[SndDayLengthType]     
                                  FROM
                                  --------------tables starts
                                   [dbo].[EmployeeWeekOffByDay] d
                                  inner join 
                                  (--1
                                  select max(EffectiveDate) ed,EmpSystemID emp from [EmployeeWeekOffByDay] 
                                  where EmpSystemID in 
                                  (
                                  " + empids + @"
                                  )
                                    and EffectiveDate<='" + sAttnDate + @"'
                                  group by EmpSystemID
                                  )--1 
                                  m on m.ed=d.EffectiveDate and m.emp=d.EmpSystemID
                                ------------tables ends

                                  where EmpSystemID in (
                                   " + empids + @"
                                  )
                                  group by 
                                  EmpSystemID,FixSystemID,AlignWithCC,IndividualWeekOff
                                  ,FstOffDay,FstDayLengthType,SndOffDay,SndDayLengthType";

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
        private void GetCompanyAssignWeekOffDateRangeWise(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT A.* FROM scs.OffDayDetail A
			                            INNER JOIN (SELECT * FROM scs.OffDayMaster WHERE OffDayType = 'W') B ON A.OffDayMasterId = B.Id
                            WHERE A.OffDayDate = '" + sAttnDate + @"'
                                  AND A.CompanyGroupId = '" + sGroupID + @"' AND A.PlantID = '" + sPlantID + "'";

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
        private void GetUpdatedEmpShiftAssignBeforeFromDate(string sEmpSystemIDColl, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT A.* FROM dbo.EmployeeShiftAssign A
                            INNER JOIN (
                                         SELECT EmpSystemID, MAX(EffectiveDate) EffectiveDate FROM dbo.EmployeeShiftAssign
                                            WHERE EffectiveDate <= '" + sAttnDate + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")
                                            GROUP BY EmpSystemID
                                        ) B ON A.EmpSystemID = B.EmpSystemID AND A.EffectiveDate = B.EffectiveDate";

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
        private void GetSftRstDayCount(string sEmpSystemIDColl, string dtLastDt, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT *
                            FROM dbo.EmpDateWiseShiftAssign 
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @") AND
                                    WorkDate BETWEEN '" + dtLastDt + "' AND '" + sAttnDate + @"'";

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
        private void GetEmployeeShiftAssignBeforeFromDate(string sEmpSystemIDColl, string sFromDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeShiftAssign 
                                WHERE EffectiveDate < '" + sFromDate + @"'
                                      AND EmpSystemID IN (" + sEmpSystemIDColl + @")";

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
        private void GetEmployeeShiftAssignInDateRange(string sEmpSystemIDColl, string lstDate, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.EmployeeShiftAssign 
                                WHERE EffectiveDate between  '" + lstDate + @"' and '" + sAttnDate + @"'
                                            AND EmpSystemID IN (" + sEmpSystemIDColl + @")";

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
        private void GetShiftRosterChild(string sGroupID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                //strSql = @"SELECT SRM.SystemID SRMasterSystemID, SRM.ShiftRosterName, SRM.ShiftRosterDescription, SRC.ShiftDefinationID, SRC.ShiftSequence,
                //                ISNULL(SRM.IsFixedDayInMonthShiftRoster, 0) IsFixedDayInMonthShiftRoster, SRM.FixedDayInMonthShiftRoster,
                //                   ISNULL(SRM.IsDaysLengthShiftRoster, 0) IsDaysLengthShiftRoster, ISNULL(SRM.DaysLengthShiftRoster, 0) DaysLengthShiftRoster, 
                //                ISNULL(SRM.IsAlignWithCC, 0) IsAlignWithCC, ISNULL(SRM.IsFixedDayInMonthWeekOff, 0) IsFixedDayInMonthWeekOff, 
                //                   SRM.FixedDayInMonthWeekOff, ISNULL(SRM.IsDaysLengthWeekOff, 0) IsDaysLengthWeekOff, SRM.WeekOffDay, 
                //                ISNULL(SRM.IsWeekOffInShiftLenght, 0) IsWeekOffInShiftLenght, SRM.WeekOffInShiftLenght,SRM.IsChangeAfterIndividualWeekoff 
                //            FROM [dbo].[ShiftRosterMaster] SRM
                //               LEFT JOIN [dbo].[ShiftRosterChild] SRC ON SRM.SystemID = SRC.SRMasterSystemID
                //            WHERE SRM.GroupID = '" + sGroupID + @"'";

                strSql = @"	SELECT srm.Systemid,srm.PlantID,srm.ChangeAfterDayLength,srm.ShiftRosterName
										,srm.RosteringPattern	,srm.WeekDays	,srm.MultiDate
										,src.SRMasterSystemID,src.ShiftDefinationID,src.ShiftSequence
                            FROM [dbo].[ShiftRosterMaster] SRM
			                            LEFT JOIN [dbo].[ShiftRosterChild] SRC ON SRM.SystemID = SRC.SRMasterSystemID
                                        WHERE SRM.GroupID = '" + sGroupID + @"'";

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
        private void GetAttdnProcessData(string sEmpSystemIDColl, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.AttdnProcessData 
                            WHERE WorkDate = '" + sDate + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")";

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

    public class dicShiftDft
    {
        public string SystemID { get; set; } = string.Empty;
        public string GroupID { get; set; } = string.Empty;
        public string PlantID { get; set; } = string.Empty;
        public string ShiftDefinationName { get; set; } = string.Empty;
        public string ShiftDefinationDescription { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int SequenceNo { get; set; } = 0;
        public bool IsActive { get; set; } = false;
        public bool DefaultShift { get; set; } = false;
        public string ShiftType { get; set; } = string.Empty;
        public DateTime? InTime { get; set; }
        public int InTimeStartMargin { get; set; } = 0;
        public int LateMargin { get; set; } = 0;
        public int AbsentEndMargin { get; set; } = 0;
        public DateTime? OutTime { get; set; }
        public int OutTimeEndMargin { get; set; } = 0;
        public int OTStartTime { get; set; } = 0;
        public bool IsGapInclude { get; set; } = false;
        public DateTime? BreakStratTime { get; set; }
        public DateTime? BreakEndTime { get; set; }
        public int BreakPeriod { get; set; } = 0;
        public double WorkingHour { get; set; } = 0.0;

        public bool EarlyIn { get; set; } = false;
        public int EarlyInMargin { get; set; } = 0;
        public int EarlyInRoundMargin { get; set; } = 0;
        public string EarlyInRoundMarginType { get; set; } = string.Empty;

        public bool LateIn { get; set; } = false;
        public int LateInMargin { get; set; } = 0;
        public int LateInRoundMargin { get; set; } = 0;
        public string LateInRoundMarginType { get; set; } = string.Empty;

        public bool EarlyOut { get; set; } = false;
        public int EarlyOutMargin { get; set; } = 0;
        public int EarlyOutRoundMargin { get; set; } = 0;
        public string EarlyOutRoundMarginType { get; set; } = string.Empty;

        public bool LateOut { get; set; } = false;
        public int LateOutMargin { get; set; } = 0;
        public int LateOutRoundMargin { get; set; } = 0;
        public string LateOutRoundMarginType { get; set; } = string.Empty;

        public decimal ShortLeaveMaxLimit { get; set; } = 0;
        public decimal HalfDayAbsentMaxLimit { get; set; } = 0;
        public bool IncludeBreakTimeInOT { get; set; } = false;
        //public bool IsOTOverHalfDay { get; set; } = false;


    }

    //public class RosterDetail
    //{
    //    public string sFixedDayInMonthShiftRoster { get; set; }
    //    public string sFixedDayInMonthWeekOff { get; set; }
    //    public string sWeekOffDay { get; set; }
    //    public int ShiftSequence { get; set; }
    //    public bool bIsDaysLengthShiftRoster { get; set; }
    //    public bool bIsFixedDayInMonthShiftRoster { get; set; }
    //    public bool bAlignWithCC { get; set; }
    //    public bool bIsFixedDayInMonthWeekOff { get; set; }
    //    public bool bIsDaysLengthWeekOff { get; set; }
    //    public int DaysLengthShiftRoster { get; set; }
    //    public int WeekOffInShiftLenght { get; set; }
    //    public bool bIsWeekOffInShiftLenght { get; set; }
       
    //}
    public class ShiftDateWise
    {
        public string EmpSftAssiSystemID { get; set; }
        public string ShiftSystemID { get; set; }
        public int RosterShiftDayCount { get; set; }
        public string DayType { get; set; }
        public string ToReprocess { get; set; }
        public string WorkDate { get; set; }
        public string EmpSystemID { get; set; }
        public string RosterStartShiftID { get; set; }
        public string RosterSystemID { get; set; }
        public bool IsRoster { get; set; }
    }
    public class RosterDetailInfo
    {
        public string RosteringPattern { get; set; }
        public string DayName { get; set; }
        public string[] Dates { get; set; }
    }
    public static class Extensions
    {
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            try
            {
                IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
                List<T> result = new List<T>();

                foreach (var row in table.Rows)
                {
                    var item = CreateItemFromRow<T>((DataRow)row, properties);
                    result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            try
            {
                T item = new T();
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(System.DayOfWeek))
                    {
                        DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), row[property.Name].ToString());
                        property.SetValue(item, day, null);
                    }
                    else
                    {
                        if (row[property.Name] == DBNull.Value)
                            property.SetValue(item, null, null);
                        else
                            property.SetValue(item, row[property.Name], null);
                    }
                }
                return item;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
