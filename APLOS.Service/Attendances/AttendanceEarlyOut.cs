using GenericAttendance;
using System;
using System.Data;
using TBS;


public class xAttendanceEarlyOut
{
    //AttendanceProcessAplos
    //AttendanceProcessAplosTimeSpan
    string sEmpSystemIDColl = string.Empty;
    string lblAttdnProcBase = string.Empty;

    public void Execute(string GroupId, string _plantid, string sAttnDatex, string _emplist = "", bool ShouldAvoidAttendanceLock = false)
    {
        int _maxRow = 1000;
        DataSet dsEmployeeList = null;
        string GroupSysID = string.Empty;
        bool IsLocked = false;
        try
        {
            if (ShouldAvoidAttendanceLock == false)
            {
                if (_emplist.Length > 0)//will check exception
                {
                    LockValidation_Plant_WD_EMP(_plantid, sAttnDatex, _emplist, out IsLocked);
                }
                else
                {
                    LockValidation_Plant_WD(_plantid, sAttnDatex, out IsLocked);
                }
            }
            else
            {
                IsLocked = false;
            }


            if (IsLocked == false)
            {
                if (Convert.ToDateTime(sAttnDatex.Trim()) <= Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                {
                    if (_emplist.Length == 0)
                    {
                        //AttdnProcBaseOn(GroupSysID, _plantid, sAttnDatex, out dsEmployeeList);---------------------TBD
                        GetProcessedData(_plantid, sAttnDatex, out dsEmployeeList);
                        string _emps = "''";
                        int _Count = 0;
                        for (int i = 0; i < dsEmployeeList.Tables[0].Rows.Count; i++)
                        {
                            _Count++;
                            if (_emps == "''")
                            {
                                _emps = "'" + dsEmployeeList.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                _emps = _emps.Trim() + ", '" + dsEmployeeList.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + "'";
                            }

                            ///for each 1000 emp the attn-process will run
                            if (_Count >= _maxRow)
                            {
                                _emplist = _emps;
                                CoreProcess(GroupId, _plantid, sAttnDatex, _emplist);
                                _emps = "''";
                                _Count = 0;
                            }
                        }

                        //last portion
                        if (_Count < _maxRow)
                        {
                            _emplist = _emps;
                            CoreProcess(GroupId, _plantid, sAttnDatex, _emplist);
                            _emps = "''";
                            _Count = 0;
                        }
                    }
                    else
                    {
                        CoreProcess(GroupId, _plantid, sAttnDatex, _emplist);
                    }
                }//for each date
            }//IsLocked                
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }//End Function 
    public void LockValidation_Plant_WD(string _plantid, string ProcessDate, out bool IsLocked)
    {
        IsLocked = false;
        DataSet dsAttLock = null;
        DataSet dsHRsetting = null;
        try
        {
            GetHRSettingForLock(_plantid, out dsHRsetting);
            if (dsHRsetting.Tables[0].Rows.Count > 0)
            {
                GetAttendanceLockInfo(_plantid, ProcessDate, ProcessDate, out dsAttLock);
                DataView dvAL = new DataView(dsAttLock.Tables[0]);
                dvAL.RowFilter = "LockedDate>='" + ProcessDate + "'";
                if (dvAL.Count > 0)
                {
                    IsLocked = true;
                    //throw new Exception("Attendance has already been locked on ["+ ProcessDate + "]...");
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public void LockValidation(string _plantid, string _fromDate, string _toDate, string empids)
    {
        DataSet dsAttLock = null;
        DataSet dsHRsetting = null;
        try
        {
            GetHRSettingForLock(_plantid, out dsHRsetting);
            if (dsHRsetting.Tables[0].Rows.Count > 0)
            {
                DateTime _fd = Convert.ToDateTime(_fromDate);
                DateTime _td = Convert.ToDateTime(_toDate);
                while (_fd <= _td)
                {
                    GetAttendanceLockWithException(empids, _plantid, _fd.ToString("dd-MMM-yyyy"), out dsAttLock);
                    if (dsAttLock.Tables[0].Rows.Count > 0)
                    {
                        //string _ld = string.Empty;
                        //for (int i = 0; i < dsAttLock.Tables[0].Rows.Count; i++)
                        //{
                        //    string emp = dsAttLock.Tables[0].Rows[i]["EmployeeCode"].ToString();
                        //    //string dates = dsAttLock.Tables[0].Rows[i]["LockedDate"].ToString();
                        //    if (_ld.Length == 0)//EmployeeCode
                        //    {
                        //        _ld = "[" + emp + "]";
                        //    }
                        //    else
                        //    {
                        //        _ld += ", [" + emp + "]";
                        //    }
                        //}//for
                        throw new Exception("Attendance has already been locked on " + _fd.ToString("dd-MMM-yyyy") + "");
                    } //count
                    _fd = _fd.AddDays(1);
                }//while
            }//hr setting
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public void LockValidation_Plant_WD_EMP(string _plantid, string _fromDate, string empids, out bool IsLocked)
    {
        DataSet dsAttLock = null;
        DataSet dsHRsetting = null;
        IsLocked = false;
        try
        {
            GetHRSettingForLock(_plantid, out dsHRsetting);
            if (dsHRsetting.Tables[0].Rows.Count > 0)
            {
                GetAttendanceLockWithException(empids, _plantid, _fromDate, out dsAttLock);
                if (dsAttLock.Tables[0].Rows.Count > 0)
                {
                    IsLocked = true;
                }
            }//hr setting
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public void GetGroupId(string PlantId, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"select  CompanyGroupId from org.plant where Id='" + PlantId + "' ";

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

    public void GetHRSettingForLock(string PlantId, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"select  systemid from PlantWiseHRMSSetting where PlantId='" + PlantId + "' and IsAttendanceLockApplicable=1";

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
    public void GetProcessedData(string PlantId, string workdate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"select * from AttdnProcessData where WorkDate='" + workdate + "'  and PlantId='" + PlantId + "' ";

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

    void CoreProcess(string Groupid, string _plantid, string sAttnDatex, string _emplist)
    {
        try
        {
            if (_emplist.Length > 0)
            {
                sEmpSystemIDColl = _emplist;
            }
            EarlyOutDataProcess(Groupid, _plantid, sAttnDatex, sEmpSystemIDColl);
            // SetRawINOUT(_plantid, sAttnDatex, sEmpSystemIDColl);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    private void PlantNameAndHRMSLocation(string _plantid, string sAttnDate, out string strYrSystemID, out string strYrFromDate, out string strYrToDate, out string GroupSysID)
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

            GetPlantInformation(_plantid, out dsLocal);
            if (dsLocal.Tables[0].Rows.Count > 0)
            {
                GroupSysID = dsLocal.Tables[0].Rows[0]["CompanyGroupId"].ToString().Trim();
            }

            GetYearlyCalender(GroupSysID, _plantid, sAttnDate.Trim(), out dsYrCal);
            if (dsYrCal.Tables[0].Rows.Count > 0)
            {
                strYrSystemID = dsYrCal.Tables[0].Rows[0]["Id"].ToString();
                strYrFromDate = Convert.ToDateTime(dsYrCal.Tables[0].Rows[0]["FromDate"]).ToString("dd-MMM-yyyy");
                strYrToDate = Convert.ToDateTime(dsYrCal.Tables[0].Rows[0]["ToDate"]).ToString("dd-MMM-yyyy");
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }//End Function
    string GetINOUTType(DataSet dsEmpShift, DataRowView drRaw)
    {
        string res = string.Empty;
        try
        {
            string empid = drRaw["LogDownLoadNum"].ToString();
            string workdate = drRaw["pdate"].ToString();
            string worktime = drRaw["ptime"].ToString();

            DataView dvEmpShift = new DataView(dsEmpShift.Tables[0]);
            dvEmpShift.RowFilter = "EmpSystemId='" + empid + "' and workdate='" + workdate + "'";
            if (dvEmpShift.Count > 0)
            {
                res = GetTypeINOUT(workdate, worktime, dvEmpShift[0]);
            }//if

            return res;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    string GetTypeINOUT(string workdate, string worktime, DataRowView drEmpShift)
    {
        string res = string.Empty;
        try
        {
            string punchDT = Convert.ToDateTime(workdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(workdate).ToString("HH:mm");
            DateTime _punchDT = Convert.ToDateTime(punchDT);

            string ShiftInTime = drEmpShift["InTime"].ToString();
            string ShiftOutTime = drEmpShift["OutTime"].ToString();
            int RawINDefinitionFrom = Convert.ToInt32(drEmpShift["RawINDefinitionFrom"].ToString());
            int RawINDefinitionTo = Convert.ToInt32(drEmpShift["RawINDefinitionTo"].ToString());
            int RawOUTDefinitionFrom = Convert.ToInt32(drEmpShift["RawOUTDefinitionFrom"].ToString());
            int RawOUTDefinitionTo = Convert.ToInt32(drEmpShift["RawOUTDefinitionTo"].ToString());

            string ShiftIn = Convert.ToDateTime(workdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(ShiftInTime).ToString("HH:mm");
            string ShiftOut = Convert.ToDateTime(workdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(ShiftOutTime).ToString("HH:mm");//TBD
            if (Convert.ToDateTime(ShiftOut) < Convert.ToDateTime(ShiftIn))
            {
                ShiftOut = Convert.ToDateTime(ShiftOut).AddDays(1).ToString("dd-MMM-yyyy HH:mm");
            }

            string IN_From = Convert.ToDateTime(ShiftIn).AddMinutes(-RawINDefinitionFrom).ToString("dd-MMM-yyyy HH:mm");
            string IN_To = Convert.ToDateTime(ShiftIn).AddMinutes(RawINDefinitionTo).ToString("dd-MMM-yyyy HH:mm");

            string OUT_From = Convert.ToDateTime(ShiftOut).AddMinutes(-RawOUTDefinitionFrom).ToString("dd-MMM-yyyy HH:mm");
            string OUT_To = Convert.ToDateTime(ShiftOut).AddMinutes(RawOUTDefinitionTo).ToString("dd-MMM-yyyy HH:mm");

            if (_punchDT >= Convert.ToDateTime(IN_From) && _punchDT <= Convert.ToDateTime(IN_To))
            {
                res = "IN";
            }

            if (_punchDT >= Convert.ToDateTime(OUT_From) && _punchDT <= Convert.ToDateTime(OUT_To))
            {
                res = "OUT";
            }

            return res;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    private void SetRawINOUT(string plantid, string workdate, string sEmpSystemIDColl)
    {
        DataSet dsEmpShift = null;
        DataSet dsRaw = null;
        try
        {
            GetEmpDateWise(plantid, workdate, sEmpSystemIDColl, out dsEmpShift);
            GetTypeLessRawData(plantid, workdate, sEmpSystemIDColl, out dsRaw);
            //calculation
            for (int i = 0; i < dsRaw.Tables[0].Rows.Count; i++)
            {
                string _rid = dsRaw.Tables[0].Rows[i]["Id"].ToString();
                DataView dvRaw = new DataView(dsRaw.Tables[0]);
                dvRaw.RowFilter = "Id='" + _rid + "'";
                if (dvRaw.Count > 0)
                {
                    string _Type = GetINOUTType(dsEmpShift, dvRaw[0]);
                    if (string.IsNullOrEmpty(_Type) == false)
                    {
                        DataRow drRaw = dvRaw[0].Row;
                        drRaw.BeginEdit();
                        drRaw["PType"] = _Type;
                        drRaw.EndEdit();
                    }//type found                      
                }//if
            }//for
            SaveDataSets(dsRaw);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }//End Function     
    private void GetEmpDateWise(string plantid, string workdate, string sEmpSystemIDColl, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"select
                        s.InTime,s.OutTime,s.[RawINDefinitionFrom]
                              ,s.[RawINDefinitionTo]
                              ,s.[RawOUTDefinitionFrom]
                              ,s.[RawOUTDefinitionTo]
	                          ,a.EmpSystemID
                              ,a.WorkDate
                         from EmpDateWiseShiftAssign a
                        left join ShiftDefination s on s.SystemID = a.ShiftSystemID
                        left join EmployeeInformation e on e.SystemId = a.EmpSystemID
                        where WorkDate = '" + workdate + @"' and EmpSystemID in (" + sEmpSystemIDColl + @") and e.plantid='" + plantid + "'";

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
    private void GetTypeLessRawData(string plantid, string workdate, string sEmpSystemIDColl, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = "select * from AttdnRawData where LogDownLoadNum in (" + sEmpSystemIDColl + ") and pdate='" + workdate + "' and plantid='" + plantid + "' and ptype is null";

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

    void GetDayStatus(bool Ismanual, string sDate, string sDayType, DateTime _maxLateTime, string sOfficeStartTime, string sOfficeInTime, string sLogDownLoadNum, DataTable dtRawData, string InDate, ref string sInTime, out string sDayStatus, out bool bMoreInMarg)
    {
        try
        {
            sDayStatus = "";
            bMoreInMarg = false;

            string _office_start_margin = string.Empty;

            bool _GTOST = false;//officeStartTime
            if (Ismanual == false)
            {
                if (Convert.ToDateTime(InDate + " " + sInTime) >= Convert.ToDateTime(sOfficeStartTime))
                {
                    _GTOST = true;
                }
            }
            else
            {
                _GTOST = true;//during manual att get any inpunch
            }

            if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && _GTOST && Convert.ToDateTime(InDate + " " + sInTime) <= Convert.ToDateTime(sDate + " " + sOfficeInTime))
            {
                sDayStatus = "P";
            }
            else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "WA" || sDayType.ToUpper() == "HA" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && _GTOST & Convert.ToDateTime(InDate + " " + sInTime) > Convert.ToDateTime(sDate + " " + sOfficeInTime))
            {
                if (_maxLateTime < Convert.ToDateTime(InDate + " " + sInTime))//by monir 180308
                {
                    sDayStatus = "A";
                }
                else
                {
                    sDayStatus = "L";
                }
            }
            else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && _GTOST)
            {
                sDayStatus = "A";
                sInTime = "00:00:00";
                bMoreInMarg = true;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    void GetMinRaw(ref DataTable dtRawData, string empPK, dicShiftDft _dicShift, string pDate, out string sInTime, out string ShiftInTimeWithEarlyInMargin, out string pk)
    {
        string ShiftOUTTime = string.Empty;
        ShiftInTimeWithEarlyInMargin = string.Empty;
        pk = string.Empty;
        try
        {
            #region Find InTime from raw Data Table

            sInTime = "00:00:00";
            //sInTimeTmp= "00:00:00";
            var _time = Convert.ToDateTime(_dicShift.InTime).ToString("HH:mm:ss");
            string _s_start_time = pDate + " " + _time;
            var _dt = Convert.ToDateTime(_s_start_time).AddMinutes(-_dicShift.InTimeStartMargin);
            ShiftInTimeWithEarlyInMargin = _dt.ToString("dd-MMM-yyyy HH:mm:ss");

            //var _s_end_time = Convert.ToDateTime(_s_start_time).AddMinutes(_dicShift.WorkingHour);
            //ShiftOUTTime = _s_end_time.ToString("dd-MMM-yyyy HH:mm:ss");
            string _s_end_time = string.Empty;
            GetOutTime(_dicShift, pDate, out _s_end_time);

            ShiftOUTTime = Convert.ToDateTime(_s_end_time).ToString("dd-MMM-yyyy HH:mm:ss");

            DataView dvRawData = new DataView(dtRawData);
            dvRawData.RowFilter = "LogDownLoadNum = '" + empPK + "' and PTime>='" + ShiftInTimeWithEarlyInMargin + "' and PTime<='" + ShiftOUTTime + "'";
            DataTable dtInTime = dvRawData.ToTable();

            if (dvRawData.Count > 0)
            {
                object minDate = dtInTime.Compute("MIN(PTime)", "LogDownLoadNum = '" + empPK + "' and PTime >= '" + ShiftInTimeWithEarlyInMargin + "' and PTime <= '" + ShiftOUTTime + "'");
                sInTime = Convert.ToDateTime(minDate).ToString("dd-MMM-yyyy HH:mm:ss");
                //sInTimeTmp = sInTime;
                //========pk
                DataView dv = new DataView(dtRawData);
                dv.RowFilter = "LogDownLoadNum = '" + empPK + "' and PTime>='" + ShiftInTimeWithEarlyInMargin + "' and PTime<='" + ShiftOUTTime + "' and PTime<='" + sInTime + "'";
                if (dv.Count > 0)
                {
                    pk = dv[0]["RowID"].ToString();
                }
            }//if count
            else
            {
                sInTime = Convert.ToDateTime(pDate + " 00:00:00").ToString("dd-MMM-yyyy HH:mm:ss");
            }


            for (int rc = 0; rc < dvRawData.Count; rc++)
            {
                #region update raw flag
                DataRow dr = dvRawData[rc].Row;
                dr.BeginEdit();
                dr["ProcessedFlag"] = 1;
                dr.EndEdit();
                #endregion
            }

            #endregion Find InTime from raw Data Table
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    void GetOutTime(dicShiftDft _dicShift, string pdate, out string OUTTime)
    {

        try
        {
            string ppDate = Convert.ToDateTime(pdate).ToString("dd-MMM-yyyy");
            string it = ppDate + " " + Convert.ToDateTime(_dicShift.InTime).ToString("HH:mm:ss");
            string ot = ppDate + " " + Convert.ToDateTime(_dicShift.OutTime).ToString("HH:mm:ss");
            OUTTime = ot;
            if (Convert.ToDateTime(ot) < Convert.ToDateTime(it))
            {
                OUTTime = Convert.ToDateTime(pdate).AddDays(1).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(_dicShift.OutTime).ToString("HH:mm:ss");
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    void GetINTime(dicShiftDft _dicShift, string pdate, out string INTime)
    {

        try
        {
            string ppDate = Convert.ToDateTime(pdate).ToString("dd-MMM-yyyy");
            string it = ppDate + " " + Convert.ToDateTime(_dicShift.InTime).ToString("HH:mm:ss");
            //string ot = ppDate + " " + Convert.ToDateTime(_dicShift.OutTime).ToString("HH:mm:ss");
            INTime = it;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    void GetMaxRaw(ref DataTable dtRawData, string empPK, dicShiftDft _dicShift, string pDate, out string sInTime, out string pk)
    {
        string ShiftOUTTime = string.Empty;
        string ShiftInTimeWithEarlyInMargin = string.Empty;
        int _Minutes = 720;
        //int _Minutes = 480;//191117
        pk = string.Empty;
        try
        {
            #region Find InTime from raw Data Table

            sInTime = "00:00:00";
            //sInTimeTmp= "00:00:00";
            var _time = Convert.ToDateTime(_dicShift.InTime).ToString("HH:mm:ss");
            string _s_start_time = pDate + " " + _time;
            var _dt = Convert.ToDateTime(_s_start_time).AddMinutes(-_dicShift.InTimeStartMargin);
            ShiftInTimeWithEarlyInMargin = _dt.ToString("dd-MMM-yyyy HH:mm:ss");

            string _s_end_time = string.Empty;
            GetOutTime(_dicShift, pDate, out _s_end_time);

            ShiftOUTTime = Convert.ToDateTime(_s_end_time).AddMinutes(_Minutes).ToString("dd-MMM-yyyy HH:mm:ss");
            //ShiftOUTTime = _s_end_time.ToString("dd-MMM-yyyy HH:mm:ss");

            DataView dvRawData = new DataView(dtRawData);
            dvRawData.RowFilter = "LogDownLoadNum = '" + empPK + "' and PTime>='" + ShiftInTimeWithEarlyInMargin + "' and PTime<='" + ShiftOUTTime + "'";
            DataTable dtInTime = dvRawData.ToTable();
            if (dvRawData.Count > 0)
            {
                object minDate = dtInTime.Compute("MAX(PTime)", "LogDownLoadNum = '" + empPK + "' and PTime >= '" + ShiftInTimeWithEarlyInMargin + "' and PTime <= '" + ShiftOUTTime + "'");
                sInTime = Convert.ToDateTime(minDate).ToString("dd-MMM-yyyy HH:mm:ss");
                //========pk
                DataView dv = new DataView(dtRawData);
                dv.RowFilter = "LogDownLoadNum = '" + empPK + "' and PTime>='" + ShiftInTimeWithEarlyInMargin + "' and PTime<='" + ShiftOUTTime + "' and PTime>='" + sInTime + "'";
                if (dv.Count > 0)
                {
                    pk = dv[0]["RowID"].ToString();
                }
                //sInTimeTmp = sInTime;
            }//if count
            else
            {
                //sInTime = Convert.ToDateTime(pDate+" "+sInTime).ToString("dd-MMM-yyyy HH:mm:ss");
                sInTime = Convert.ToDateTime(pDate + " 00:00:00").ToString("dd-MMM-yyyy HH:mm:ss");
            }

            for (int rc = 0; rc < dvRawData.Count; rc++)
            {
                #region update raw flag
                DataRow dr = dvRawData[rc].Row;
                dr.BeginEdit();
                dr["ProcessedFlag"] = 1;
                dr.EndEdit();
                #endregion
            }

            #endregion Find InTime from raw Data Table
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    private void UpdateAttdnData(ParaAttendance _paraA, ref DataRow drLocal)
    {
        //if (sShiftType.ToUpper().Trim() == "NIGHT SHIFT")
        //99
       // bool IsCurrentShiftNightShift = false;
        bool IsNewShiftNightShift = false;
        try
        {
            //ManualDate
            //else
            //{
            //    //check if it is night shift
            //DataSet dsST = null;
            //string sid = drLocal["ShiftSystemID"].ToString();
            //GetShiftType(sid, out dsST);
            //if (dsST.Tables[0].Rows.Count > 0)
            //{
            //    IsCurrentShiftNightShift = true;
            //}

            //GetShiftType(_paraA.shiftSystemID, out dsST);
            //if (dsST.Tables[0].Rows.Count > 0)
            //{
            //    IsNewShiftNightShift = true;
            //}
            //}

            //var x = _paraA.shiftSystemID;
            //var y = drLocal["ShiftSystemID"].ToString();

            if (string.IsNullOrEmpty(_paraA.sDayStatus))
            {
                _paraA.sDayStatus = "A";
            }

            if (_paraA.sDayStatus == "RST" || _paraA.sDayStatus == "OD")
            {
                _paraA.sLvTrans = "";
                _paraA.IsHalfDayLeave = false;
                _paraA.IsShortLeave = false;
                _paraA.CountedShortLeave = 0;
                _paraA.IsShortLeave = false;
                _paraA.IsLWP = false;
                _paraA.bManualDayStatus = false;
                _paraA.bManualTime = false;
            }



            if (string.IsNullOrEmpty(_paraA.sLvTrans) == false && _paraA.IsHalfDayLeave == false)//full leave
            {
                if (_paraA.IsShortLeave)
                {
                    _paraA.CountedShortLeave = 0;
                    _paraA.IsShortLeave = false;
                }
            }
            else if (string.IsNullOrEmpty(_paraA.sLvTrans) == false && _paraA.IsHalfDayLeave)//half leave
            {
                if (_paraA.IsShortLeave)
                {

                    _paraA.CountedShortLeave = _paraA.CountedShortLeave - 1;
                    if (_paraA.CountedShortLeave <= 0)
                    {
                        _paraA.CountedShortLeave = 0;
                        _paraA.IsShortLeave = false;
                    }
                }
            }



            if (_paraA.OPN_FLAG == "ADDNEW")
            {
                drLocal["AddedBy"] = "Schedule";
                drLocal["DateAdded"] = DateTime.Now;
            }


            //if (_paraA.sDayStatus != "RST")
            //{
            //    drLocal["AttendanceRestDetailId"] = DBNull.Value;
            //    // drAttnProcData["AttendanceRestDetailId"]
            //}

            if (_paraA.IsFirstHalfLeave)
            {
                if (_paraA.sDayStatus == "L")
                {
                    _paraA.sDayStatus = "P";
                }
            }

            drLocal["EmpSystemID"] = _paraA.sEmpSystemID;
            drLocal["WorkDate"] = _paraA.sWorkingDate;

            if (_paraA.sType == "IN")
            {
                if (_paraA.shiftSystemID != string.Empty)
                {
                    drLocal["ShiftSystemID"] = _paraA.shiftSystemID;
                }

                if (_paraA.sTime == string.Empty || _paraA.sTime == "00:00:00")
                {
                    drLocal["InTime"] = DBNull.Value;
                    drLocal["IsManualInTime"] = false;
                }
                else
                {
                    drLocal["InTime"] = _paraA.InDate + " " + _paraA.sTime;
                    drLocal["IsManualInTime"] = _paraA.bManualTime;
                }

                if (_paraA.IsOutNUll)
                {
                    drLocal["OutTime"] = DBNull.Value;
                    drLocal["IsManualOutTime"] = 0;
                    drLocal["OTHr"] = 0;
                    drLocal["IsOTComfirm"] = 0;
                }
                if (_paraA.sRowID == string.Empty)
                {
                    drLocal["InTimeRowID"] = DBNull.Value;
                    drLocal["PunchInTime"] = DBNull.Value;
                }
                else
                {
                    //drLocal["InTimeRowID"] = DBNull.Value;
                    drLocal["InTimeRowID"] = _paraA.sRowID;
                    drLocal["PunchInTime"] = _paraA.sInRawData;
                }
                drLocal["DayStatus"] = (string.IsNullOrEmpty(_paraA.sDayStatus) == true ? "A" : _paraA.sDayStatus);
                drLocal["DayStatusInTimeOnly"] = _paraA.sDayStatus;
                drLocal["IsManualDayStatus"] = _paraA.bManualDayStatus;

                if (_paraA.sLvTrans != "")
                {
                    //if (_paraA.OPN_FLAG == "ADDNEW")
                    //{
                    drLocal["LTSystemID"] = _paraA.sLvTrans;
                    drLocal["IsLWP"] = _paraA.IsLWP;
                    drLocal["IsHalfDayLeave"] = _paraA.IsHalfDayLeave;
                    //}
                    //else
                    //{
                    //    if(drLocal["DayStatus"].ToString()=="HDP")
                    //    {
                    //        drLocal["DayStatus"] = "HDP";
                    //    }                            
                    //    drLocal["LTSystemID"] = _paraA.sLvTrans;
                    //}
                }
                else
                {
                    drLocal["LTSystemID"] = DBNull.Value;
                    drLocal["IsLWP"] = false;
                    drLocal["IsHalfDayLeave"] = false;
                }


            }
            else if (_paraA.sType == "OUT")//od will b considered here edit time
            {
                //if(_paraA.DayType=="W" && (_paraA.sDayStatus=="W" || _paraA.sDayStatus=="" || _paraA.sDayStatus=="A"))
                //{
                //    DataSet dsST = null;
                //    GetShiftType(_paraA.shiftSystemID, out dsST);
                //    if (dsST.Tables[0].Rows.Count > 0)
                //    {
                //        _paraA.sTime = "00:00:00";
                //    }
                //}
                if (_paraA.sTime == string.Empty || _paraA.sTime == "00:00:00")
                {
                    drLocal["OutTime"] = DBNull.Value;
                    drLocal["IsManualOutTime"] = false;
                }
                else
                {
                    if (_paraA.bManualDayStatus)
                    {
                        //drLocal["OutTime"] = _paraA.ManualDate + " " + _paraA.sTime;
                        //drLocal["IsManualOutTime"] = false;
                    }
                    else
                    {
                        if (_paraA.HasManualOutTime)
                        {
                            drLocal["OutTime"] = _paraA.ManualDate + " " + _paraA.sTime;
                            drLocal["IsManualOutTime"] = _paraA.bManualTime;
                        }
                        else
                        {
                            drLocal["OutTime"] = _paraA.ManualDate + " " + _paraA.sTime;
                            //drLocal["OutTime"] = _paraA.OutDate + " " + _paraA.sTime;
                            drLocal["IsManualOutTime"] = _paraA.bManualTime;
                        }
                    }
                }

                if (_paraA.IsStatusChanged)/// out time HDL or A et
                {
                    //if (string.IsNullOrEmpty(drLocal["LTSystemID"].ToString()) && Convert.ToBoolean(drLocal["IsOD"].ToString())==false)//if no leave application found
                    if (Convert.ToBoolean(drLocal["IsOD"].ToString()) == false)//if no leave application found
                    {
                        if (_paraA.bManualDayStatus == false)
                        {
                            if (string.IsNullOrEmpty(drLocal["AttendanceRestDetailId"].ToString()))//AttendanceRestDetailId
                            {
                                drLocal["DayStatus"] = _paraA.sDayStatus;
                                if (string.IsNullOrEmpty(drLocal["LTSystemID"].ToString()) == false && _paraA.sDayStatus == "A")//if present for just an hour and applied for leave
                                {
                                    if (GetBoolData(drLocal["IsHalfDayLeave"].ToString()))
                                    {
                                        //_paraA.sDayStatus = "LV";
                                        drLocal["DayStatus"] = _paraA.sDayStatus;
                                    }
                                    else
                                    {
                                        _paraA.sDayStatus = "LV";
                                        drLocal["DayStatus"] = _paraA.sDayStatus;
                                    }
                                }
                            }
                        }//  if (_paraA.bManualDayStatus == false)
                    }// if (Convert.ToBoolean(drLocal["IsOD"].ToString()) == false)
                }// if (_paraA.IsStatusChanged)

                //else//for normal absent ot will b nullify if hr setting allow
                //{
                //    if(drLocal["DayStatus"].ToString()=="A")
                //    {
                //        if(_paraA.OTDeductionForAbsenteeism==false)
                //        {
                //            _paraA.iOverTime = 0;
                //            drLocal["OTHr"] = _paraA.iOverTime;
                //        }
                //    }
                //}
                if (Convert.ToBoolean(drLocal["IsOD"].ToString()) == false)
                {
                    if (_paraA.IsReversed)/// out time HDP or A et
                    {
                        if (string.IsNullOrEmpty(drLocal["LTSystemID"].ToString()))//if 0.5 leave is not explicitly taken
                        {
                            if (string.IsNullOrEmpty(drLocal["DayStatusInTimeOnly"].ToString()) == false)//if DayStatusInTimeOnly is null , no need to update
                            {
                                if (string.IsNullOrEmpty(drLocal["AttendanceRestDetailId"].ToString()))//AttendanceRestDetailId
                                {
                                    drLocal["DayStatus"] = drLocal["DayStatusInTimeOnly"];
                                }

                                //if (drLocal["DayStatus"].ToString() == "A")//for normal absent ot will b nullify if hr setting allow
                                //{
                                //    if (_paraA.iso)
                                //    {
                                //        _paraA.iOverTime = 0;
                                //        drLocal["OTHr"] = _paraA.iOverTime;
                                //    }
                                //}
                            }
                        }
                        else
                        {
                            //as Half day leave taken all short leave is discarded
                            _paraA.IsShortLeave = false;
                        }
                    }
                }//if not od

                //190628
                //if (GetBoolData(drLocal["IsOTComfirm"].ToString()) == false)
                //{
                if (string.IsNullOrEmpty(drLocal["AttendanceRestDetailId"].ToString()) && Convert.ToBoolean(drLocal["IsOD"].ToString()) == false)//AttendanceRestDetailId
                {
                    string _oth = GetNumData(drLocal["OTHr"].ToString());
                    if (Convert.ToDecimal(_oth) != _paraA.iOverTime)
                    {
                        drLocal["IsOTComfirm"] = 0;
                    }
                    if (_paraA.IsOTEntitled == false)
                    {
                        _paraA.iOverTime = 0;
                        drLocal["IsOTComfirm"] = 0;
                    }
                    //if (drLocal["DayStatus"].ToString() == "CW" || drLocal["DayStatus"].ToString() == "AH" || drLocal["DayStatus"].ToString() == "W" || drLocal["DayStatus"].ToString() == "H")
                    if (drLocal["DayStatus"].ToString() == "W" || drLocal["DayStatus"].ToString() == "H")
                    {
                        _paraA.iOverTime = 0;
                    }

                    drLocal["OTHr"] = _paraA.iOverTime;
                }

                drLocal["OTIntime"] = _paraA.iOverTimeIntime;
                drLocal["OTOuttime"] = _paraA.iOverTimeOuttime;
                if (string.IsNullOrEmpty(drLocal["LTSystemID"].ToString()) == false && GetBoolData(drLocal["IsHalfDayLeave"].ToString()))//if half leave 
                {
                    if (_paraA.iOverTime < 0)
                    {
                        _paraA.iOverTime = 0;
                    }
                    drLocal["OTHr"] = _paraA.iOverTime;
                }
                else if (string.IsNullOrEmpty(drLocal["LTSystemID"].ToString()) == false && GetBoolData(drLocal["IsHalfDayLeave"].ToString()) == false)//if full leave 
                {
                    _paraA.iOverTime = 0;
                    drLocal["OTHr"] = _paraA.iOverTime;
                }
                //}//if ot not confirmed

                //if(bplib.clsWebLib.GetBoolData(drLocal["IsShortLeave"].ToString()) && _paraA.IsShortLeave==false)
                //{
                //    drLocal["DayStatus"] = _paraA.sDayStatus;
                //}

                if (Convert.ToBoolean(drLocal["IsOD"].ToString()))
                {
                    _paraA.IsShortLeave = false;
                }

                drLocal["IsShortLeave"] = _paraA.IsShortLeave;
                if (_paraA.IsShortLeave)
                {
                    drLocal["CountedShortLeave"] = _paraA.CountedShortLeave;
                }
                else
                {
                    drLocal["CountedShortLeave"] = 0;
                }

                if (_paraA.sRowID == string.Empty)
                {
                    drLocal["OutTimeRowID"] = DBNull.Value;
                    //drLocal["PunchOutTime"] = DBNull.Value;
                    //drLocal["PunchOutTime"] = _paraA.sOutRawData;
                }
                else
                {
                    drLocal["OutTimeRowID"] = _paraA.sRowID;
                    drLocal["PunchOutTime"] = _paraA.sOutRawData;
                }
            }//out


            //if (string.IsNullOrEmpty(drLocal["AttendanceRestDetailId"].ToString()) == false)//AttendanceRestDetailId
            //{
            //    drLocal["DayStatus"] = "RST";
            //}
            drLocal["IsOTEntitled"] = _paraA.IsOTEntitled;
            drLocal["ToReprocess"] = "No";

            drLocal["GroupID"] = _paraA.GroupId;
            drLocal["PlantID"] = _paraA.sPlantID.Trim();

            drLocal["UpdatedBy"] = "Schedule";
            drLocal["DateUpdated"] = DateTime.Now;
        }
        catch (Exception ex)
        {
            throw (ex);
        }
        finally
        {
            //
        }
    }//End Function 
    void GetShiftDefinition(DataRow drSource, dicShiftDft _ShiftDft)
    {
        try
        {
            //#if DEBUG
            //                // _ShiftDft.LateIn = bplib.clsWebLib.GetBoolData(drSource["LateIn"].ToString());
            //#else

            //#endif
            _ShiftDft.PlantID = drSource["PlantID"].ToString();

            _ShiftDft.InTimeStartMargin = Convert.ToInt32(drSource["InTimeStartMargin"].ToString());

            _ShiftDft.LateMargin = Convert.ToInt32(drSource["LateMargin"].ToString());
            _ShiftDft.LateIn = GetBoolData(drSource["LateIn"].ToString());
            _ShiftDft.LateInMargin = Convert.ToInt32(drSource["LateInMargin"].ToString());
            _ShiftDft.LateInRoundMargin = Convert.ToInt32(drSource["LateInRoundMargin"].ToString());
            _ShiftDft.LateInRoundMarginType = drSource["LateInRoundMarginType"].ToString();

            _ShiftDft.EarlyIn = GetBoolData(drSource["EarlyIn"].ToString());
            _ShiftDft.EarlyInMargin = Convert.ToInt32(drSource["EarlyInMargin"].ToString());
            _ShiftDft.EarlyInRoundMargin = Convert.ToInt32(drSource["EarlyInRoundMargin"].ToString());
            _ShiftDft.EarlyInRoundMarginType = drSource["EarlyInRoundMarginType"].ToString();

            _ShiftDft.IsGapInclude = GetBoolData(drSource["IsGapInclude"].ToString());


            _ShiftDft.EarlyOut = GetBoolData(drSource["EarlyOut"].ToString());
            _ShiftDft.EarlyOutMargin = Convert.ToInt32(drSource["EarlyOutMargin"].ToString());
            _ShiftDft.EarlyOutRoundMargin = Convert.ToInt32(drSource["EarlyOutRoundMargin"].ToString());
            _ShiftDft.EarlyOutRoundMarginType = drSource["EarlyOutRoundMarginType"].ToString();

            _ShiftDft.InTime = Convert.ToDateTime(drSource["InTime"].ToString().Trim());
            _ShiftDft.OutTime = Convert.ToDateTime(drSource["OutTime"].ToString().Trim());

            _ShiftDft.LateOut = GetBoolData(drSource["LateOut"].ToString());
            _ShiftDft.LateOutMargin = Convert.ToInt32(drSource["LateOutMargin"].ToString());
            _ShiftDft.LateOutRoundMargin = Convert.ToInt32(drSource["LateOutRoundMargin"].ToString());
            _ShiftDft.LateOutRoundMarginType = drSource["LateOutRoundMarginType"].ToString();
            _ShiftDft.BreakStratTime = Convert.ToDateTime(drSource["BreakStratTime"].ToString());
            _ShiftDft.BreakEndTime = Convert.ToDateTime(drSource["BreakEndTime"].ToString());
            _ShiftDft.BreakPeriod = Convert.ToInt32(drSource["BreakPeriod"].ToString());
            _ShiftDft.WorkingHour = Convert.ToDouble(drSource["WorkingHour"].ToString());

            //_ShiftDft.ShortLeaveMaxLimit = Convert.ToDecimal(drSource["ShortLeaveMaxLimit"].ToString());
            _ShiftDft.HalfDayAbsentMaxLimit = Convert.ToDecimal(drSource["HalfDayAbsentMaxLimit"].ToString());
            _ShiftDft.EarlyOutToleranceMargin = Convert.ToInt32(drSource["EarlyOutToleranceMargin"].ToString());
            _ShiftDft.EarlyOutMaxLimit = Convert.ToInt32(drSource["EarlyOutMaxLimit"].ToString());
            _ShiftDft.LateInMaxLimit = Convert.ToInt32(drSource["LateInMaxLimit"].ToString());

            _ShiftDft.IsLunchOutApplicable = GetBoolData(drSource["IsLunchOutApplicable"].ToString());
            _ShiftDft.IsLateInApplicable = GetBoolData(drSource["IsLateInApplicable"].ToString());
            _ShiftDft.IsEarlyOutApplicable = GetBoolData(drSource["IsEarlyOutApplicable"].ToString());

            //_ShiftDft.LateInToleranceMargin = Convert.ToInt32(drSource["LateInToleranceMargin"].ToString());

            //Convert.ToInt32(drSource["OutTime"].ToString());BreakStratTime
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    DateTime MakeDateTime(string sDate, string sTime)
    {
        try
        {
            var LT = Convert.ToDateTime(sTime).ToString("HH:mm:ss");
            var LunchTime = sDate + " " + LT;
            return Convert.ToDateTime(LunchTime);
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }
    void GetWorkDuration(string sWorkingDate, DateTime _empIntime, DateTime _empOuttime, dicShiftDft _ShiftDft, out int _Work_Duration)
    {
        try
        {
            _Work_Duration = 0;
            DateTime _cust_empIntime = DateTime.Now;
            DateTime _cust_empOuttime = DateTime.Now;

            var _ShiftInTime = MakeDateTime(sWorkingDate, _ShiftDft.InTime.ToString());

            //--
            //var _time = Convert.ToDateTime(_ShiftDft.InTime).ToString("HH:mm:ss");
            //string _s_start_time = pDate + " " + _time;
            //var _dt = Convert.ToDateTime(_ShiftInTime).AddMinutes(-_dicShift.InTimeStartMargin);
            //ShiftInTimeWithEarlyInMargin = _dt.ToString("dd-MMM-yyyy HH:mm:ss");

            var _s_end_time = Convert.ToDateTime(_ShiftInTime).AddMinutes(_ShiftDft.WorkingHour);
            //ShiftOUTTime = _s_end_time.ToString("dd-MMM-yyyy HH:mm:ss");
            //----------------------------------------------------------------
            string OUTDate = _s_end_time.ToString("dd-MMM-yyyy");
            var _ShiftOutTime = MakeDateTime(OUTDate, _ShiftDft.OutTime.ToString());
            if (_ShiftInTime > _empIntime)
            {
                _cust_empIntime = _ShiftInTime;
            }
            else
            {
                _cust_empIntime = _empIntime;
            }

            if (_ShiftOutTime > _empOuttime)
            {
                _cust_empOuttime = _empOuttime;
            }
            else
            {
                _cust_empOuttime = _ShiftOutTime;
            }


            TimeSpan tsOT = _cust_empOuttime - _cust_empIntime;
            //_Work_Duration = ((tsOT.Hours * 60) + tsOT.Minutes);
            _Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);

            //if (_ShiftDft.IncludeBreakTimeInOT == false)
            if (_ShiftDft.IsGapInclude == false)
            {

                //if (_ShiftDft.ShiftType.ToUpper()!="NIGHT")
                //{
                var _break_start = MakeDateTime(OUTDate, _ShiftDft.BreakStratTime.ToString());
                var _break_end = MakeDateTime(OUTDate, _ShiftDft.BreakEndTime.ToString());

                if (_cust_empOuttime > _break_start)
                {
                    _Work_Duration = 0;//1st part and 2nd part both will b calculated individually


                    if (_cust_empIntime > _break_end)//intime 2:30
                    {
                        var first_part = _break_end - _cust_empIntime;
                        //_Work_Duration = ((first_part.Hours * 60) + first_part.Minutes);
                        _Work_Duration = (((first_part.Days * 60) * 24) + (first_part.Hours * 60) + first_part.Minutes);
                    }
                    else if (_cust_empIntime > _break_start)//intime 1:30
                    {
                        //no imtime
                    }
                    else//intime 12:30
                    {
                        var first_part = _break_start - _cust_empIntime;
                        //_Work_Duration = ((first_part.Hours * 60) + first_part.Minutes);
                        _Work_Duration = (((first_part.Days * 60) * 24) + (first_part.Hours * 60) + first_part.Minutes);
                    }

                    if (_cust_empOuttime > _break_end)
                    {
                        var second_part = _cust_empOuttime - _break_end;
                        // _Work_Duration += ((second_part.Hours * 60) + second_part.Minutes);
                        _Work_Duration += (((second_part.Days * 60) * 24) + (second_part.Hours * 60) + second_part.Minutes);
                    }
                }
                //}                

                // _Work_Duration -= _ShiftDft.BreakPeriod;
            }//break time included
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    void GetWorkDurationIncludingOT(string sWorkingDate, DateTime _empIntime, DateTime _empOuttime, dicShiftDft _ShiftDft, out int _Work_Duration)
    {
        try
        {
            _Work_Duration = 0;
            DateTime _cust_empIntime = DateTime.Now;
            DateTime _cust_empOuttime = DateTime.Now;

            var _ShiftInTime = MakeDateTime(sWorkingDate, _ShiftDft.InTime.ToString());

            //--
            //var _time = Convert.ToDateTime(_ShiftDft.InTime).ToString("HH:mm:ss");
            //string _s_start_time = pDate + " " + _time;
            //var _dt = Convert.ToDateTime(_ShiftInTime).AddMinutes(-_dicShift.InTimeStartMargin);
            //ShiftInTimeWithEarlyInMargin = _dt.ToString("dd-MMM-yyyy HH:mm:ss");

            var _s_end_time = Convert.ToDateTime(_ShiftInTime).AddMinutes(_ShiftDft.WorkingHour);
            //ShiftOUTTime = _s_end_time.ToString("dd-MMM-yyyy HH:mm:ss");
            //----------------------------------------------------------------
            string OUTDate = _s_end_time.ToString("dd-MMM-yyyy");
            var _ShiftOutTime = MakeDateTime(OUTDate, _ShiftDft.OutTime.ToString());
            if (_ShiftInTime > _empIntime)
            {
                _cust_empIntime = _ShiftInTime;
            }
            else
            {
                _cust_empIntime = _empIntime;
            }

            _cust_empOuttime = _empOuttime;
            //if (_ShiftOutTime > _empOuttime)
            //{
            //    _cust_empOuttime = _empOuttime;
            //}
            //else
            //{
            //    _cust_empOuttime = _ShiftOutTime;
            //}


            TimeSpan tsOT = _cust_empOuttime - _cust_empIntime;
            //_Work_Duration = ((tsOT.Hours * 60) + tsOT.Minutes);
            _Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);

            //if (_ShiftDft.IncludeBreakTimeInOT == false)
            if (_ShiftDft.IsGapInclude == false)
            {

                //if (_ShiftDft.ShiftType.ToUpper()!="NIGHT")
                //{
                var _break_start = MakeDateTime(OUTDate, _ShiftDft.BreakStratTime.ToString());
                var _break_end = MakeDateTime(OUTDate, _ShiftDft.BreakEndTime.ToString());

                if (_cust_empOuttime > _break_start)
                {
                    _Work_Duration = 0;//1st part and 2nd part both will b calculated individually


                    if (_cust_empIntime > _break_end)//intime 2:30
                    {
                        var first_part = _break_end - _cust_empIntime;
                        //_Work_Duration = ((first_part.Hours * 60) + first_part.Minutes);
                        _Work_Duration = (((first_part.Days * 60) * 24) + (first_part.Hours * 60) + first_part.Minutes);
                    }
                    else if (_cust_empIntime > _break_start)//intime 1:30
                    {
                        //no imtime
                    }
                    else//intime 12:30
                    {
                        var first_part = _break_start - _cust_empIntime;
                        //_Work_Duration = ((first_part.Hours * 60) + first_part.Minutes);
                        _Work_Duration = (((first_part.Days * 60) * 24) + (first_part.Hours * 60) + first_part.Minutes);
                    }

                    if (_cust_empOuttime > _break_end)
                    {
                        var second_part = _cust_empOuttime - _break_end;
                        // _Work_Duration += ((second_part.Hours * 60) + second_part.Minutes);
                        _Work_Duration += (((second_part.Days * 60) * 24) + (second_part.Hours * 60) + second_part.Minutes);
                    }
                }
                //}                

                // _Work_Duration -= _ShiftDft.BreakPeriod;
            }//break time included
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }



    void GetEmpOutTime(dicShiftDft _ShiftDft, ref DateTime _cust_empOuttime, string OUTDate)
    {
        try
        {
            if (_ShiftDft.IncludeBreakTimeInOT == false)
            {
                //_Emp_Outtime

                //if (_ShiftDft.ShiftType.ToUpper()!="NIGHT")
                //{
                var _break_start = MakeDateTime(OUTDate, _ShiftDft.BreakStratTime.ToString());
                var _break_end = MakeDateTime(OUTDate, _ShiftDft.BreakEndTime.ToString());

                if (_cust_empOuttime > _break_start && _cust_empOuttime < _break_end)//13:30
                {
                    _cust_empOuttime = _break_start;
                }
                else if (_cust_empOuttime > _break_end)//13:30
                {
                    _cust_empOuttime = _cust_empOuttime.AddMinutes(-_ShiftDft.BreakPeriod);
                }
                //}                

                // _Work_Duration -= _ShiftDft.BreakPeriod;
            }//break time
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }


    private void GetPlantInformation(string _plantid, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = "SELECT * FROM org.Plant WHERE Id = '" + _plantid + "'";

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
    private void GetHRSettingPlantWise(string _plantid, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = "SELECT * FROM PlantWiseHRMSSetting WHERE Plantid = '" + _plantid + "'";

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
    private void GetAttendanceInfoExtra(string _plantid, string workdate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = "SELECT * FROM AttendanceInfoExtra WHERE Plantid = '" + _plantid + "' and workdate='" + workdate + "'";

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
    private void GetOTPerMinPolicy(string _plantid, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = "SELECT * FROM OTPerMinutePolicy WHERE Plantid = '" + _plantid + "'";

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
    private void GetShortLeaveSettingPlantWise(string _plantid, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = "SELECT * FROM ShortLeavePolicy WHERE Plantid = '" + _plantid + "'";

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
    private void GetShiftType(string Shiftid, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = "select ShiftType from ShiftDefination where systemid='" + Shiftid + @"' and ShiftType='Night Shift'";

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
    private void GetYearlyCalender(string sGroupID, string sPlantID, string sDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.YearlyCalendar 
                                    WHERE CompanyGroupId = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                            AND '" + sDate + @"' BETWEEN FromDate AND ToDate";

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
    public void GetEmpDateWiseShiftAssignWithDateRange(string sEmpSystemIDColl, string dtLastDt, string sDate, out DataSet dsRef)
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

    //based on
    private void GetAllRegsterPersonOnSystemAttdnProc(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT SystemID, EmployeeCode EnrollID, EmployeeName EnrollName, CardNumber, PlantID
	                                        FROM (
                                                  SELECT * FROM EmployeeInformation WHERE 
                                                            SystemID IN (
                                                                         SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"') 
                                                                        )
                                                ) AS E 
		                                        WHERE GroupID = '" + sGroupID + @"' AND (DOS >= '" + sAttnDate + @"' OR DOS IS NULL) 
                            ";

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
    private void GetAllShiftLessEmployees(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT SystemID, EmployeeCode EnrollID, EmployeeName EnrollName, CardNumber, PlantID
                                FROM (
                                        SELECT * FROM EmployeeInformation	 WHERE PlantId='" + sPlantID + @"' and  SystemID not IN (select EmpSystemID from EmployeeShiftAssign )
                                    ) AS E 
                                WHERE GroupID = '" + sGroupID + @"' AND (DOS > '" + sAttnDate + @"' OR DOS IS NULL)  ";

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

    //in
    private void GetAttdnRawDataForAttdnProc(string sGroupID, string sAttnDate, string sType, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM AttdnRawData
                           WHERE PDate = '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"' 
                                 AND ProcessedFlag = 0";

            if (sType != "")
            {
                strSql = strSql + @" AND PType = '" + sType + @"'";
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
    private void GetRawAll(string sGroupID, string sAttnDate, string empids, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            string FromDdate = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
            string ToDdate = Convert.ToDateTime(sAttnDate).AddDays(+1).ToString("dd-MMM-yyyy");
            strSql = @"SELECT * FROM AttdnRawData
                           WHERE PDate between '" + FromDdate + @"' and '" + ToDdate + @"' AND GroupID = '" + sGroupID + @"'  AND LogDownLoadNum IN (
                                                     " + empids + @"
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
    }//End Function 
    private void GetAttdnRawDataForAttdnProc_SNA(string sGroupID, string sAttnDate, string sAttnDateTime, string sType, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM AttdnRawData
                           WHERE PDate >= '" + sAttnDate + @"' and PTime<='" + sAttnDateTime + "' AND GroupID = '" + sGroupID + @"' 
                                 AND ProcessedFlag = 0";

            if (sType != "")
            {
                strSql = strSql + @" AND PType = '" + sType + @"'";
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
    private void GetMaxInTimeByDay(string sGroupID, string sAttnDate, string sPlant, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT isnull(max(InTime),'00:00') InTime
                          FROM [dbo].[AttdnProcessData] where GroupID='" + sGroupID + @"' and PlantID='" + sPlant + @"' and WorkDate='" + sAttnDate + @"'
                          ";

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
    public void GetAttdnProcData(string sGroupID, string empids, string strPrvAttnDate, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.AttdnProcessData
                           WHERE WorkDate BETWEEN '" + strPrvAttnDate + @"' 
                                 AND '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
                                 AND daystatus in ( select DayType from DayType where Category in ('Present','Late','Absent'))
                                 AND EmpSystemID IN (
                                                     " + empids + @"
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
    }//End Function  
    public void GetProcessedData(string sGroupID, string empids, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.AttdnProcessData
                           WHERE WorkDate = '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
                                 AND EmpSystemID IN (
                                                     " + empids + @"
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
    }//End Function  
    private void GetPaidHours(string sGroupID, string sEmpSysIdColl, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"select * from mst.PaidHoursEmployeeAssign where 
                                   employeeid IN (" + sEmpSysIdColl + @")
                            ORDER BY employeeid";

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
    private void GetEmployeeInfo(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT E.*, ES.*, ISNULL(DATEDIFF(D, Atd.LastWorkDate, '" + sAttnDate + @"'), 0) DateDiffer
                            , ISNULL(Atd.LastWorkDate, GETDATE()) LastWorkDate
                            --, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle
                             ,IsOTEntitle=case when (ISNULL(OTX.IsOTEntitle, 0)=0 and isnull(OTX.EmpSystemID,'')<>'') then 0
					                                                    when ISNULL(EmOT.IsOTEntitle, 0)=1 then 1
					                                                     when ISNULL(d.IsOTEntitled, 0)=1 then 1
					                                                     else 0 end
                            , EmOT.OTStartDate, EmOT.OTEndDate,
                                  ISNULL(AttDt.ToReprocess, 'YES') ToReprocess
	                        FROM 
                            (
                             SELECT * FROM EmployeeInformation WHERE 
                                    SystemID IN (" + sEmpSysIdColl + @")                                    
                            ) AS E 
		                        INNER JOIN (
											SELECT * FROM
														(
														 SELECT ES.EmpSystemID, ES.ShiftSystemID, ES.DayType, S.ShiftType

                                                                ,EarlyIn
                                                                ,EarlyInMargin
                                                                ,EarlyInRoundMargin
                                                                ,isnull(EarlyInRoundMarginType, 'ROUND') EarlyInRoundMarginType

                                                                ,S.LateIn
                                                                ,S.LateInMargin
                                                                ,LateInRoundMargin
                                                                ,isnull(LateInRoundMarginType, 'ROUND') LateInRoundMarginType

                                                                ,S.EarlyOut
                                                                ,S.EarlyOutMargin
                                                                ,EarlyOutRoundMargin
                                                                ,isnull(EarlyOutRoundMarginType, 'ROUND') EarlyOutRoundMarginType
                                                               
                                                                ,LateOut
                                                                ,LateOutMargin
                                                                ,LateOutRoundMargin
                                                                ,isnull(LateOutRoundMarginType, 'ROUND') LateOutRoundMarginType
                                                                 ,HalfDayAbsentMaxLimit,ShortLeaveMaxLimit,S.IsGapInclude,hrset.IsOTOverHalfDay,
 
																OfficeStartTime = CASE WHEN ISNULL(C.InTimeStartMargin, '') != '' THEN DATEADD(MI, -C.InTimeStartMargin, C.InTime)
																					  ELSE DATEADD(MI, -S.InTimeStartMargin, S.InTime) END, 
																OfficeTime = CASE WHEN ISNULL(C.LateMargin, '') != '' THEN DATEADD(MI, C.LateMargin, C.InTime)
																					  ELSE DATEADD(MI, S.LateMargin, S.InTime) END,
																InTime = CASE WHEN ISNULL(C.InTime, '') != '' THEN C.InTime
																					  ELSE S.InTime END, 

                                                                AbsentEndMargin = CASE WHEN ISNULL(C.AbsentEndMargin, '') != '' THEN C.AbsentEndMargin
																					  ELSE S.AbsentEndMargin END,

																InTimeStartMargin = CASE WHEN ISNULL(C.InTimeStartMargin, '') != '' THEN C.InTimeStartMargin
																					  ELSE S.InTimeStartMargin END, 
																BreakStratTime = CASE WHEN ISNULL(C.BreakStratTime, '') != '' THEN C.BreakStratTime
																					  ELSE S.BreakStratTime END, 
																BreakEndTime = CASE WHEN ISNULL(C.BreakEndTime, '') != '' THEN C.BreakEndTime
																					  ELSE S.BreakEndTime END,
                                                                BreakPeriod=CASE WHEN ISNULL(C.BreakPeriod, '') != '' THEN C.BreakPeriod
																					  ELSE S.BreakPeriod END,
                                                                WorkingHour=CASE WHEN ISNULL(C.WorkingHour, '') != '' THEN isnull(c.BreakPeriod,0)+isnull(C.WorkingHour,0) 
                                                                                 WHEN S.IsGapInclude = 1 AND ISNULL(C.WorkingHour, '') = '' THEN isnull(s.BreakPeriod,0)+ isnull(s.WorkingHour,0)
																			     ELSE S.WorkingHour END,
																OfficeEndTime = CASE WHEN ISNULL(C.OutTimeEndMargin, '') != '' THEN DATEADD(MI, C.OutTimeEndMargin, S.OutTime)
																					  ELSE DATEADD(MI, S.OutTimeEndMargin, S.OutTime) END,
                                                                OutTime = CASE WHEN ISNULL(C.OutTime, '') != '' THEN C.OutTime
																					  ELSE S.OutTime END,
                                                                LateMargin = CASE WHEN ISNULL(C.LateMargin, '') != '' THEN C.LateMargin
																					  ELSE S.LateMargin END,
																OTStartTime = CASE WHEN S.IsGapInclude = 1 AND ISNULL(C.OutTime, '') != '' THEN C.OutTime
																				   WHEN S.IsGapInclude = 1 AND ISNULL(C.OutTime, '') = '' THEN S.OutTime
																				   WHEN S.IsGapInclude = 0 AND ISNULL(C.OutTime, '') != '' THEN DATEADD(MI, C.OTStartTime, C.OutTime)
																				   ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
														 FROM dbo.EmpDateWiseShiftAssign ES
																	LEFT JOIN dbo.ShiftDefination S ON ES.ShiftSystemID = S.SystemID
                                                                    left join PlantWiseHRMSSetting hrset on hrset.plantid=S.plantid
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
                               
	                        -------------------------------------OT entitle starts-----------------------------------------------------------
                                LEFT JOIN (
											SELECT * FROM dbo.EmployeeOTEntitle 
													WHERE '" + sAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
													AND ISNULL(IsOTEntitle, 0) = 1
										   ) EmOT ON E.SystemID = EmOT.EmpSystemID
								 left JOIN  (
				                                            SELECT DC.LeavePolicyMasterId,DC.PlantId,DM.DesignationId,DC.AttdnBonusPmtPolicyMasterId,
				                                            DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.OverTimePmtPolicyMasterID,DC.PFPolicyMasterID 
				                                            FROM MST.DesignationMaster DM
				                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
				                             ) D ON D.DesignationId = E.GivenDesignationId AND D.PlantId=E.PlantId
								 LEFT JOIN (
											SELECT * FROM dbo.EmployeeOTEntitle 
													WHERE '" + sAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
													AND ISNULL(IsOTEntitle, 0) = 0
										   ) OTX ON E.SystemID = OTX.EmpSystemID
								-------------------------------------OT entitle ends-----------------------------------------------------------

								LEFT JOIN 
                                        (
                                            SELECT EmpSystemID, MAX(WorkDate) LastWorkDate
	                                            FROM dbo.AttdnProcessData
                                            WHERE GroupID = '" + sGroupID + @"'
                                            GROUP BY EmpSystemID
                                        ) AS Atd ON E.SystemID = Atd.EmpSystemID
                                LEFT JOIN dbo.AttdnProcessData AS AttDt ON E.SystemID = AttDt.EmpSystemID AND AttDt.WorkDate = '" + sAttnDate + @"'
                            WHERE (E.DOS >= '" + sAttnDate + @"' OR DOS IS NULL) AND E.DOJ <= '" + sAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                  AND E.SystemID IN (" + sEmpSysIdColl + @")
                            ORDER BY E.EmployeeCode";

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
    void GetEarlyOut(dicShiftDft dicshiftdft, string workdate, string Punch_outtime, bool IsHDPApplicable, out bool IsEarlyOut, out decimal EOValue)
    {
        IsEarlyOut = false;
        EOValue = 0;
        try
        {
            var Calculated_OUT_Time = Convert.ToDateTime(Punch_outtime);
            string _s_end_time = string.Empty;
            GetOutTime(dicshiftdft, workdate, out _s_end_time);
            if (Calculated_OUT_Time < Convert.ToDateTime(_s_end_time))
            {
                string _ott = Calculated_OUT_Time.ToString("dd-MMM-yyyy HH:mm");
                var x = Convert.ToDateTime(_s_end_time) - Convert.ToDateTime(_ott);
                EOValue = (((x.Days * 60) * 24) + (x.Hours * 60) + x.Minutes);
                //_Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
                if (EOValue > 0)
                {
                    if (dicshiftdft.EarlyOutToleranceMargin < EOValue)
                    {
                        if (IsHDPApplicable)
                        {
                            if (dicshiftdft.EarlyOutMaxLimit >= EOValue)
                            {
                                IsEarlyOut = true;
                            }
                        }
                        else
                        {
                            IsEarlyOut = true;
                        }
                    }
                    else
                    {
                        EOValue = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    void GetLateIn(dicShiftDft dicshiftdft, string workdate, string Punch_intime, bool IsHDPApplicable, out bool IsLateIN, out decimal EOValue)
    {
        IsLateIN = false;
        EOValue = 0;
        try
        {
            var Calculated_IN_Time = Convert.ToDateTime(Punch_intime);
            string _s_in_time = string.Empty;
            GetINTime(dicshiftdft, workdate, out _s_in_time);
            if (Calculated_IN_Time > Convert.ToDateTime(_s_in_time))
            {
                string _ott = Calculated_IN_Time.ToString("dd-MMM-yyyy HH:mm");
                var x = Convert.ToDateTime(_ott) - Convert.ToDateTime(_s_in_time);
                EOValue = (((x.Days * 60) * 24) + (x.Hours * 60) + x.Minutes);
                //_Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
                if (EOValue > 0)
                {
                    if (dicshiftdft.LateMargin < EOValue)
                    {
                        if (IsHDPApplicable)
                        {
                            if (dicshiftdft.LateInMaxLimit >= EOValue)
                            {
                                IsLateIN = true;
                            }
                        }
                        else
                        {
                            IsLateIN = true;
                        }
                    }
                    else
                    {
                        EOValue = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    void GetLunchOut(string sEmpSysID, dicShiftDft dicshiftdft, DataTable dtRaw, string workdate, out object intime_obj, out object outtime_obj)
    {
        string _lunctimeStart = string.Empty;
        string _lunctimeEnd = string.Empty;
        outtime_obj = null;
        intime_obj = null;
        try
        {
            //lunch start time + margin 15

            string _lst = workdate + " " + Convert.ToDateTime(dicshiftdft.BreakStratTime).AddMinutes(-15).ToString("HH:mm:ss");
            string _led = workdate + " " + Convert.ToDateTime(dicshiftdft.BreakEndTime).AddMinutes(30).ToString("HH:mm:ss");
            intime_obj = dtRaw.Compute("max(ptime)", "PTime >=  '" + _lst + "' and PTime<='" + _led + "' and ptype='IN' and LogDownLoadNum='" + sEmpSysID + "'");
            outtime_obj = dtRaw.Compute("min(ptime)", "PTime >= '" + _lst + "' and PTime<='" + _led + "' and ptype='OUT' and LogDownLoadNum='" + sEmpSysID + "'");
            //select min(ptime) it from AttdnRawData where PTime between '' and ''

            //var Calculated_OUT_Time = Convert.ToDateTime(Punch_outtime).AddMinutes(_EarlyOutMargin);
            //string _s_end_time = string.Empty;
            //GetOutTime(dicshiftdft, workdate, out _s_end_time);
            //if (Calculated_OUT_Time < Convert.ToDateTime(_s_end_time))
            //{
            //    var x = Convert.ToDateTime(_s_end_time) - Calculated_OUT_Time;
            //    EOValue = (((x.Days * 60) * 24) + (x.Hours * 60) + x.Minutes);
            //    //_Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
            //    IsEarlyOut = true;
            //}
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    private void EarlyOutDataProcess(string GroupSysID, string _plantId, string sAttnDate, string sEmpSystemIDColl)
    {
        //sShiftType
        #region declare variables

        DataSet dsRawData = null;
        DataTable dtRawData = null;
        //DataView dvRawData = null;
        //DataRow drRawData = null;
        DataSet dsAttnProcData = null;
        DataTable dtAttnProcData = null;
        //DataRow drAttnProcData = null;
        DataView dvAttnProcData = null;
        DataSet dsEmpInfo = null;
        string sLogDownLoadNum = "";
        string sEmpSysID = "";
        string sPlantID = "";
        string sOTStartTime = "";
        string sOutTime = "";
        string sOutTimeRowID = string.Empty;
        string sOutTimeRowData = string.Empty;
        string sOutTimeTmp = "";
        //string sDayType = "";
        //string sBreakStratTime = "";
        //string sBreakEndTime = "";
        string sDate = "";
        //bool IsOTOverHalfDay = false;
        //bool IsWeekendAsFixedWeekend = false;
        //bool IsOTBasedOnPerMinute = false;
        dicShiftDft _ShiftDft = null;
        //DataSet dsDayType = null;

        #endregion local variables

        try
        {
            #region Dataset
            //GetDayType(out dsDayType);
            //DataSet dsHRSetting = null;
            //GetHRSettingPlantWise(_plantId, out dsHRSetting);
            DataSet dsAttExtra = null;
            DataView dvAttExtra = null;
            DataRow drAttExtra = null;
            GetAttendanceInfoExtra(_plantId, sAttnDate, out dsAttExtra);

            //if (dsHRSetting.Tables[0].Rows.Count > 0)
            //{
            //    IsOTOverHalfDay = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsOTOverHalfDay"].ToString());
            //    IsWeekendAsFixedWeekend = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsWeekendCount"].ToString());
            //    IsOTBasedOnPerMinute = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsOTBasedOnPerMinute"].ToString());
            //}
            #endregion

            DateTime sToDt = Convert.ToDateTime(sAttnDate.Trim());
            #region DataSet

            sDate = sToDt.ToString("dd-MMM-yyyy");
            GetRawAll(GroupSysID.Trim(), sDate.Trim(), sEmpSystemIDColl.Trim(), out dsRawData);
            dtRawData = dsRawData.Tables[0];
            GetAttdnProcData(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), sDate.Trim(), out dsAttnProcData);
            dtAttnProcData = dsAttnProcData.Tables[0];
            GetEmployeeInfo_Out(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sDate.Trim(), out dsEmpInfo);//TBD
            //DataSet dsOffDuty = null;
            bool IsHDPApplicable = false;
            //DataView dvOffDuty = null;
            //GetOffDuty(_plantId, sDate.Trim(), out dsOffDuty);
            //DataSet dsShortLeaveSetting = null;
            //GetShortLeaveSettingPlantWise(_plantId, out dsShortLeaveSetting);
            //if (dsShortLeaveSetting.Tables[0].Rows.Count > 0)
            //{
            //    IsHDPApplicable = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsHalfDayPresentAllowed"].ToString());               
            //}

            #endregion DataSet

            if (dsEmpInfo.Tables[0].Rows.Count > 0)
            {
                string Pk = "";
                GenID(DateTime.Now.ToShortDateString().ToString(), "EARLYOUT", out Pk);
                int _count = 0;
                #region Loop
                for (int EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                {
                    #region Variables

                    _ShiftDft = new dicShiftDft();
                    GetShiftDefinition(dsEmpInfo.Tables[0].Rows[EmpCount], _ShiftDft);
                    IsHDPApplicable = (_ShiftDft.HalfDayAbsentMaxLimit > 0 ? true : false);
                    sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();
                    sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();

                    #endregion

                    //dvOffDuty = new DataView(dsOffDuty.Tables[0]);
                    //dvOffDuty.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                    //if (dvAttnProcData.Count > 0)
                    //{
                    //    var d = dvAttnProcData[0]["Duration"].ToString();
                    //}//dvAttnProcData.Count 

                    #region LI

                    string _pit = string.Empty;
                    dvAttnProcData = new DataView();
                    dvAttnProcData.Table = dtAttnProcData;
                    dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                    if (dvAttnProcData.Count > 0)
                    {
                        _pit = dvAttnProcData[0]["intime"].ToString();

                    }//dvAttnProcData.Count 

                    var ls_li = _ShiftDft.IsLateInApplicable;
                    bool IsLateIn = false;
                    decimal LIValue = 0;
                    if (ls_li && string.IsNullOrEmpty(_pit) == false)
                    {
                        GetLateIn(_ShiftDft, sDate, _pit, IsHDPApplicable, out IsLateIn, out LIValue);
                    }

                    if (LIValue > 0)
                    {
                        UpdateInsert(dsAttExtra, sPlantID, Pk, _ShiftDft, sEmpSysID, sDate, LIValue, "LATEIN", null, null, ref _count);
                    }
                    #endregion

                    #region LO

                    if (sEmpSysID == "1900336")
                    {

                    }
                    object inttimeobj = null;
                    object outtimeobj = null;
                    if (_ShiftDft.IsLunchOutApplicable)
                    {
                        GetLunchOut(sEmpSysID, _ShiftDft, dtRawData, sDate, out inttimeobj, out outtimeobj);
                    UpdateInsert(dsAttExtra, sPlantID, Pk, _ShiftDft, sEmpSysID, sDate, 0, "LUNCHOUT", inttimeobj, outtimeobj, ref _count);
                    }
                    #endregion

                    #region EO

                    string _pt = string.Empty;
                    dvAttnProcData = new DataView();
                    dvAttnProcData.Table = dtAttnProcData;
                    dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                    if (dvAttnProcData.Count > 0)
                    {
                        _pt = dvAttnProcData[0]["outtime"].ToString();

                    }//dvAttnProcData.Count 

                    var xxx = _ShiftDft.IsEarlyOutApplicable;
                    bool IsEarlyOut = false;
                    decimal EOValue = 0;
                    if (xxx && string.IsNullOrEmpty(_pt) == false)
                    {
                        GetEarlyOut(_ShiftDft, sDate, _pt, IsHDPApplicable, out IsEarlyOut, out EOValue);
                    }

                    if (EOValue > 0)
                    {
                        UpdateInsert(dsAttExtra, sPlantID, Pk, _ShiftDft, sEmpSysID, sDate, EOValue, "EARLUOUT", null, null, ref _count);
                    }
                    #endregion                   

                    #region commented
                    //if ((_ShiftDft.IsEarlyOutApplicable || _ShiftDft.IsLunchOutApplicable || _ShiftDft.IsLateInApplicable) && dvAttnProcData.Count > 0)
                    //{                       
                    //        dvAttExtra = new DataView(dsAttExtra.Tables[0]);
                    //        dvAttExtra.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                    //        if (dvAttExtra.Count > 0)
                    //        {
                    //            drAttExtra = dvAttExtra[0].Row;
                    //            drAttExtra.BeginEdit();
                    //        //---------------------------------------------------------
                    //        if(_ShiftDft.IsEarlyOutApplicable)
                    //        {
                    //            drAttExtra["IsEarlyOut"] = IsEarlyOut.ToString();
                    //            drAttExtra["EarlyOutValue"] = EOValue.ToString();
                    //        }
                    //        else
                    //        {
                    //            drAttExtra["IsEarlyOut"] = DBNull.Value;
                    //            drAttExtra["EarlyOutValue"] = DBNull.Value;
                    //        }
                    //        //--------------------------------------------------------
                    //        if (_ShiftDft.IsLunchOutApplicable)
                    //        {
                    //            drAttExtra["LunchInTime"] = inttimeobj;
                    //            drAttExtra["LunchOutTime"] = outtimeobj;
                    //        }
                    //        else
                    //        {
                    //            drAttExtra["LunchInTime"] = DBNull.Value;
                    //            drAttExtra["LunchOutTime"] = DBNull.Value;
                    //        }
                    //        //--------------------------------------------------------
                    //        if (_ShiftDft.IsLateInApplicable)
                    //        {
                    //            drAttExtra["IsLateIn"] = IsLateIn;
                    //            drAttExtra["LateInValue"] = LIValue;
                    //        }
                    //        else
                    //        {
                    //            drAttExtra["IsLateIn"] = DBNull.Value;
                    //            drAttExtra["LateInValue"] = DBNull.Value;
                    //        }
                    //        //--------------------------------------------------------
                    //        drAttExtra["UpdatedBy"] = "auto";
                    //            drAttExtra["UpdatedDate"] = DateTime.Now;
                    //            drAttExtra.EndEdit();
                    //        }
                    //        else
                    //        {
                    //            _count++;
                    //            drAttExtra = dsAttExtra.Tables[0].NewRow();
                    //            drAttExtra["Id"] = "AX" + Pk + "-" + _count;
                    //            drAttExtra["WorkDate"] = sDate;
                    //            drAttExtra["EmpSystemID"] = sEmpSysID;
                    //            drAttExtra["IsEarlyOut"] = IsEarlyOut.ToString();
                    //            drAttExtra["EarlyOutValue"] = EOValue.ToString();
                    //            drAttExtra["LunchInTime"] = inttimeobj;
                    //            drAttExtra["LunchOutTime"] = outtimeobj;
                    //            drAttExtra["PlantId"] = sPlantID;
                    //            drAttExtra["AddedBy"] = "auto";
                    //            drAttExtra["AddedDate"] = DateTime.Now;
                    //            dsAttExtra.Tables[0].Rows.Add(drAttExtra);
                    //        }
                    //}//(_ShiftDft.IsEarlyOutApplicable || _ShiftDft.IsLunchOutApplicable)
                    #endregion
                }//dsEmpInfo loop
                SaveDataSets(dsAttExtra);
                #endregion
            }//if
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            #region clean variables

            dsRawData = null;
            dtRawData = null;
            //dvRawData = null;
            //drRawData = null;
            dsAttnProcData = null;
            dtAttnProcData = null;
            //drAttnProcData = null;
            dvAttnProcData = null;
            dsEmpInfo = null;
            sLogDownLoadNum = string.Empty;
            sEmpSysID = string.Empty;
            sOTStartTime = string.Empty;
            sOutTime = string.Empty;
            sOutTimeRowID = string.Empty;
            sOutTimeTmp = string.Empty;

            #endregion clean variables
        }
    }//End Function
    void UpdateInsert(DataSet dsAttExtra, string Plantid, string Pk, dicShiftDft _ShiftDft, string sEmpSysID, string sDate, decimal Duration, string InfoType, object intime, object outtime, ref int _count)
    {
        DataRow drAttExtra = null;
        DataView dvAttExtra = null;
        string _id = string.Empty;
        try
        {
            dvAttExtra = new DataView(dsAttExtra.Tables[0]);
            dvAttExtra.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "' and InfoType='" + InfoType + "'";
            if (dvAttExtra.Count > 0)
            {
                _id = dvAttExtra[0]["Id"].ToString();
            }
            dvAttExtra.RowFilter = null;

            dvAttExtra.RowFilter = "Id='" + _id + "'";
            if (dvAttExtra.Count > 0)
            {
                drAttExtra = dvAttExtra[0].Row;
                drAttExtra.BeginEdit();
                drAttExtra["InfoType"] = InfoType;
                if (InfoType == "LUNCHOUT")
                {
                    drAttExtra["InTime"] = intime;//InTime
                    drAttExtra["OutTime"] = outtime;//InTime
                }
                else
                {
                    drAttExtra["InTime"] = DBNull.Value;
                    drAttExtra["OutTime"] = DBNull.Value;
                    drAttExtra["OffDuration"] = Duration;
                }
                drAttExtra["UpdatedBy"] = "auto";
                drAttExtra["UpdatedDate"] = DateTime.Now;
                drAttExtra.EndEdit();
            }
            else
            {
                _count++;
                drAttExtra = dsAttExtra.Tables[0].NewRow();
                drAttExtra["Id"] = "AX" + Pk + "-" + _count;
                drAttExtra["WorkDate"] = sDate;
                drAttExtra["EmpSystemID"] = sEmpSysID;

                drAttExtra["InfoType"] = InfoType;
                if (InfoType == "LUNCHOUT")
                {
                    drAttExtra["OffDuration"] = Duration;
                    if (intime is null)
                    {
                        drAttExtra["InTime"] = DBNull.Value;
                    }
                    else
                    {
                        drAttExtra["InTime"] = intime;//InTime
                    }

                    if (outtime is null)
                    {
                        drAttExtra["OutTime"] = DBNull.Value;
                    }
                    else
                    {
                        drAttExtra["OutTime"] = outtime;//InTime
                    }


                }
                else
                {
                    drAttExtra["OffDuration"] = Duration;
                    drAttExtra["InTime"] = DBNull.Value;
                    drAttExtra["OutTime"] = DBNull.Value;
                }
                drAttExtra["PlantId"] = Plantid;
                drAttExtra["AddedBy"] = "auto";
                drAttExtra["AddedDate"] = DateTime.Now;
                dsAttExtra.Tables[0].Rows.Add(drAttExtra);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    #region GEN ID
    private static bool DateOkCheck(string strdate)
    {
        try
        {
            DateTime myDt = Convert.ToDateTime(strdate);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            //
        }
    }// end function
    public static object chk_NullDateData(object dateValue)
    {
        if (DateOkCheck("" + dateValue.ToString()) == false)
        {
            dateValue = "";
        }

        if (("" + dateValue.ToString()) == "")
        {
            DateTime dt = new DateTime(1901, 1, 1);
            dateValue = (object)dt;
        }
        return (object)dateValue;
    }
    public static DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
    {
        string strDate = null;
        dateValue = chk_NullDateData(dateValue);
        strDate = dateValue.ToString();
        if (strDate != "")
        {
            if (input_date_format.Trim() != "")
            {
                if (output_date_format.Trim() != "")
                {
                    System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                    InputFormat.ShortDatePattern = input_date_format;
                    DateTime myDt = Convert.ToDateTime(strDate, InputFormat);
                    strDate = myDt.ToString(output_date_format);
                }
            }
        }
        return Convert.ToDateTime(strDate);
    }// End of function
    public void GenID(string strEntryDate, string strFieldName, out string strID)
    {
        ConnectionManager.DAL.ConManager objCoManager;
        string strSql = "";
        DataSet dsLocal = null;
        DataTable dtLocal = null;
        DataRow drLocal = null;
        DataView dvLocal = null;
        // System.Text.StringBuilder SB = null;
        decimal LastNumber = 0;

        try
        {
            //by Monir
            strEntryDate = AppDateConvert(strEntryDate, "MM/dd/yyyy", getUserDateFormat()).ToShortDateString();
            //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
            strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";

            //SB = new System.Text.StringBuilder(strEntryDate);
            // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();

            strID = Convert.ToDateTime(strEntryDate).ToString("yy");

            objCoManager = new ConnectionManager.DAL.ConManager("1");
            objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
            dtLocal = dsLocal.Tables[0];
            dvLocal = new DataView();
            dvLocal.Table = dtLocal;
            dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
            if (dvLocal.Count == 0)
            {// Add data
                drLocal = dtLocal.NewRow();
                drLocal["Field"] = strFieldName;
                drLocal["Dates"] = strEntryDate.Trim();
                drLocal["LastNumber"] = 1;
                LastNumber = 1;
                dtLocal.Rows.Add(drLocal);
            }
            else if (dvLocal.Count == 1)
            {
                drLocal = dvLocal[0].Row;

                LastNumber = Convert.ToDecimal(GetNumData(("" + drLocal["LastNumber"].ToString())));
                LastNumber = LastNumber + 1;

                drLocal.BeginEdit();
                drLocal["Dates"] = strEntryDate.Trim();
                drLocal["LastNumber"] = LastNumber;
                drLocal.EndEdit();
            }
            objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
            //strID = strID + "-" + (int)LastNumber;
            strID = strID + (int)LastNumber;

        }
        catch (Exception ex)
        {
            throw (ex);
        }
        finally
        {
            dtLocal = null;
            dvLocal = null;
            drLocal = null;
        }
    }

    //public static string GetNumData(string strNumber)
    //{
    //    double d;
    //    strNumber = strNumber.Replace(",", "");
    //    System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
    //    if (strNumber.Trim() == "")
    //    { return "0"; }
    //    else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
    //    {
    //        return strNumber;
    //    }
    //    else
    //    {
    //        return "0";
    //    }
    //}// end function
    public static string getUserDateFormat()
    {
        System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
        return USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString();
    }
    #endregion

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
    private void GetEmployeeInfo_Out(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;
        string sAttnDatePrev = string.Empty;
        try
        {

            sAttnDatePrev = sAttnDate;
            //sAttnDatePrev = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
            strSql = @"SELECT E.*, ES.*, ISNULL(DATEDIFF(D, Atd.LastWorkDate, '" + sAttnDate + @"'), 0) DateDiffer, ISNULL(Atd.LastWorkDate, GETDATE()) LastWorkDate
                        --, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle
                        ,IsOTEntitle=case when (ISNULL(OTX.IsOTEntitle, 0)=0 and isnull(OTX.EmpSystemID,'')<>'') then 0
					                        when ISNULL(EmOT.IsOTEntitle, 0)=1 then 1
					                         when ISNULL(d.IsOTEntitled, 0)=1 then 1
					                         else 0 end
                        , EmOT.OTStartDate, EmOT.OTEndDate,
                                  ISNULL(AttDt.ToReprocess, 'YES') ToReprocess,attdt.WorkDate
	                        FROM 
                            (
                             SELECT * FROM EmployeeInformation WHERE 
                                    SystemID IN (" + sEmpSysIdColl + @")                                   
                            ) AS E 
		                        INNER JOIN (
											SELECT * FROM
														(
														 SELECT ES.EmpSystemID, ES.ShiftSystemID, ES.DayType, S.ShiftType
                                                                ,EarlyIn
                                                                ,EarlyInMargin
                                                                ,EarlyInRoundMargin
                                                                ,isnull(EarlyInRoundMarginType, 'ROUND') EarlyInRoundMarginType

                                                                ,LateIn
                                                                ,LateInMargin
                                                                ,LateInRoundMargin
                                                                ,isnull(LateInRoundMarginType, 'ROUND') LateInRoundMarginType

                                                                ,EarlyOut
                                                                ,EarlyOutMargin
                                                                ,EarlyOutRoundMargin
                                                                ,isnull(EarlyOutRoundMarginType, 'ROUND') EarlyOutRoundMarginType
                                                               
                                                                ,LateOut
                                                                ,LateOutMargin
                                                               
                                                                --,S.IsEarlyOutApplicable
                                                                --,S.LateInToleranceMargin
                                                                --,S.IsLateInApplicable
                                                                --S.IsLunchOutApplicable
                                                                ,LateOutRoundMargin
                                                                ,isnull(LateOutRoundMarginType, 'ROUND') LateOutRoundMarginType
                                                                ,hrset.IsOTOverHalfDay,

                                                              ---200420
                                                                                      HalfDayAbsentMaxLimit = CASE WHEN ISNULL(C.HalfDayAbsentMaxLimit, 0) != 0 THEN isnull(C.HalfDayAbsentMaxLimit,0)
																					  ELSE isnull(S.HalfDayAbsentMaxLimit,0) END,

																					  EarlyOutToleranceMargin = CASE WHEN ISNULL(C.EarlyOutToleranceMargin, 0) != 0 THEN isnull(c.EarlyOutToleranceMargin,0)
																					  ELSE isnull(S.EarlyOutToleranceMargin,0) END,
																					  LateInToleranceMargin = CASE WHEN ISNULL(C.LateInToleranceMargin,0) != 0 THEN ISNULL(C.LateInToleranceMargin,0)
																					  ELSE ISNULL(s.LateInToleranceMargin,0) END,

																					  LateInMaxLimit = CASE WHEN ISNULL(C.LateInMaxLimit, 0) != 0 THEN ISNULL(C.LateInMaxLimit, 0)
																					  ELSE ISNULL(s.LateInMaxLimit, 0) END,
																					  EarlyOutMaxLimit = CASE WHEN ISNULL(C.EarlyOutMaxLimit, 0) != 0 THEN ISNULL(C.EarlyOutMaxLimit, 0)
																					  ELSE ISNULL(s.EarlyOutMaxLimit, 0) END,

																					  IsGapInclude = CASE WHEN ISNULL(C.IsGapInclude, 0) !=0 THEN ISNULL(C.IsGapInclude, 0)
																					  ELSE ISNULL(s.IsGapInclude, 0) END,
																					  IncludeBreakTimeInOT = CASE WHEN ISNULL(C.IncludeBreakTimeInOT, 0) != 0 THEN ISNULL(C.IncludeBreakTimeInOT, 0)
																					  ELSE ISNULL(s.IncludeBreakTimeInOT, 0) END,

																					  IsLateInApplicable = CASE WHEN ISNULL(C.IsLateInApplicable, 0) != 0 THEN ISNULL(C.IsLateInApplicable, 0)
																					  ELSE ISNULL(s.IsLateInApplicable, 0) END,
																					  IsEarlyOutApplicable = CASE WHEN ISNULL(C.IsEarlyOutApplicable, 0) != 0 THEN ISNULL(C.IsEarlyOutApplicable, 0)
																					  ELSE ISNULL(s.IsEarlyOutApplicable, 0) END,
                                                                                      IsLunchOutApplicable = CASE WHEN ISNULL(C.IsLunchOutApplicable, 0) != 0 THEN ISNULL(C.IsLunchOutApplicable, 0)
																					  ELSE ISNULL(s.IsLunchOutApplicable, 0) END,
                                                                    ---200420

																OfficeStartTime = CASE WHEN ISNULL(C.InTimeStartMargin, '') != '' THEN DATEADD(MI, -C.InTimeStartMargin, C.InTime)
																					  ELSE DATEADD(MI, -S.InTimeStartMargin, S.InTime) END, 
																OfficeTime = CASE WHEN ISNULL(C.LateMargin, '') != '' THEN DATEADD(MI, C.LateMargin, C.InTime)
																					  ELSE DATEADD(MI, S.LateMargin, S.InTime) END,
                                                                LateMargin = CASE WHEN ISNULL(C.LateMargin, '') != '' THEN C.LateMargin
																					  ELSE S.LateMargin END,
																InTime = CASE WHEN ISNULL(C.InTime, '') != '' THEN C.InTime
																					  ELSE S.InTime END, 

                                                                AbsentEndMargin = CASE WHEN ISNULL(C.AbsentEndMargin, '') != '' THEN C.AbsentEndMargin
																					  ELSE S.AbsentEndMargin END,

																InTimeStartMargin = CASE WHEN ISNULL(C.InTimeStartMargin, '') != '' THEN C.InTimeStartMargin
																					  ELSE S.InTimeStartMargin END, 
																BreakStratTime = CASE WHEN ISNULL(C.BreakStratTime, '') != '' THEN C.BreakStratTime
																					  ELSE S.BreakStratTime END, 
																BreakEndTime = CASE WHEN ISNULL(C.BreakEndTime, '') != '' THEN C.BreakEndTime
																					  ELSE S.BreakEndTime END,
                                                                BreakPeriod=CASE WHEN ISNULL(C.BreakPeriod, '') != '' THEN C.BreakPeriod
																					  ELSE S.BreakPeriod END,
                                                               
                                                                --WorkingHour=CASE WHEN ISNULL(C.WorkingHour, '') != '' THEN isnull(c.BreakPeriod,0)+isnull(C.WorkingHour,0) 
                                                                --WHEN S.IsGapInclude = 1 AND ISNULL(C.WorkingHour, '') = '' THEN isnull(s.BreakPeriod,0)+ isnull(s.WorkingHour,0)
                                                                --ELSE S.WorkingHour END,

                                                                    WorkingHour=CASE WHEN ISNULL(C.WorkingHour, '') != '' and c.IncludeBreakTimeInOT = 1 THEN isnull(c.BreakPeriod,0)+isnull(C.WorkingHour,0) 
																				 WHEN ISNULL(C.WorkingHour, '') != '' and c.IncludeBreakTimeInOT = 0 THEN isnull(C.WorkingHour,0) 
                                                                                 WHEN c.IncludeBreakTimeInOT = 1 AND ISNULL(c.WorkingHour, '') = '' THEN isnull(s.BreakPeriod,0)+ isnull(s.WorkingHour,0)
                                                                                 WHEN c.IncludeBreakTimeInOT = 0 AND ISNULL(c.WorkingHour, '') = '' THEN isnull(s.WorkingHour,0)
																			     ELSE S.WorkingHour END,

																OfficeEndTime = CASE WHEN ISNULL(C.OutTimeEndMargin, '') != '' THEN DATEADD(MI, C.OutTimeEndMargin, S.OutTime)
																					  ELSE DATEADD(MI, S.OutTimeEndMargin, S.OutTime) END,
                                                                OutTime = CASE WHEN ISNULL(C.OutTime, '') != '' THEN C.OutTime
																					  ELSE S.OutTime END,
																--OTStartTime = CASE WHEN S.IsGapInclude = 1 AND ISNULL(C.OutTime, '') != '' THEN C.OutTime
																				  -- WHEN S.IsGapInclude = 1 AND ISNULL(C.OutTime, '') = '' THEN S.OutTime
																				  -- WHEN S.IsGapInclude = 0 AND ISNULL(C.OutTime, '') != '' THEN DATEADD(MI, C.OTStartTime, C.OutTime)
																				  -- ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END

                                                                    OTStartTime = CASE WHEN c.IsGapInclude = 0 AND ISNULL(C.OutTime, '') != '' THEN C.OutTime
																				   WHEN c.IsGapInclude = 0 AND ISNULL(C.OutTime, '') = '' THEN S.OutTime
																				   WHEN c.IsGapInclude = 1 AND ISNULL(C.OutTime, '') = '' THEN S.OutTime
																				   WHEN S.IsGapInclude = 1 AND ISNULL(C.OutTime, '') != '' THEN DATEADD(MI, C.OTStartTime, C.OutTime)
																				   ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END


														 FROM dbo.EmpDateWiseShiftAssign ES
																	LEFT JOIN dbo.ShiftDefination S ON ES.ShiftSystemID = S.SystemID
                                                                    left join PlantWiseHRMSSetting hrset on hrset.plantid=S.plantid
																	LEFT JOIN (
																				SELECT SCM.*, SCC.ShiftDate FROM [dbo].[ShiftTimeChgMaster] SCM
																						INNER JOIN [dbo].[ShiftTimeChgChild] SCC ON SCM.SystemID = STCMasterSystemID
																				WHERE SCC.ShiftDate = '" + sAttnDate + @"'
																			  ) C ON ES.ShiftSystemID = C.ShiftDefinationID
														 WHERE ES.WorkDate = '" + sAttnDate + @"' AND ES.GroupID = '" + sGroupID + @"'
														) A
											--WHERE --CONVERT(DATETIME, CONVERT(VARCHAR(5), InTime, 108)) < CONVERT(DATETIME, CONVERT(VARCHAR(5), GETDATE(), 108))
                                                 -- CONVERT(DATETIME, CONVERT(VARCHAR(11), '" + sAttnDate + @"', 101) + ' ' + CONVERT(VARCHAR(5), InTime, 108)) < CONVERT(DATETIME, CONVERT(VARCHAR(11), GETDATE(), 101) + ' ' + CONVERT(VARCHAR(5), GETDATE(), 108))
                                           ) ES ON E.SystemID = ES.EmpSystemID

                                -------------------------------------OT entitle starts-----------------------------------------------------------
                                LEFT JOIN (
											SELECT * FROM dbo.EmployeeOTEntitle 
													WHERE '" + sAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
													AND ISNULL(IsOTEntitle, 0) = 1
										   ) EmOT ON E.SystemID = EmOT.EmpSystemID
								 left JOIN  (
				                                            SELECT DC.LeavePolicyMasterId,DC.PlantId,DM.DesignationId,DC.AttdnBonusPmtPolicyMasterId,
				                                            DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.OverTimePmtPolicyMasterID,DC.PFPolicyMasterID 
				                                            FROM MST.DesignationMaster DM
				                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
				                             ) D ON D.DesignationId = E.GivenDesignationId AND D.PlantId=E.PlantId
								 LEFT JOIN (
											SELECT * FROM dbo.EmployeeOTEntitle 
													WHERE '" + sAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
													AND ISNULL(IsOTEntitle, 0) = 0
										   ) OTX ON E.SystemID = OTX.EmpSystemID
								-------------------------------------OT entitle ends-----------------------------------------------------------

								LEFT JOIN 
                                        (
                                            SELECT EmpSystemID, MAX(WorkDate) LastWorkDate
	                                            FROM dbo.AttdnProcessData
                                            WHERE GroupID = '" + sGroupID + @"'
                                            GROUP BY EmpSystemID
                                        ) AS Atd ON E.SystemID = Atd.EmpSystemID
                                LEFT JOIN dbo.AttdnProcessData AS AttDt ON E.SystemID = AttDt.EmpSystemID AND (AttDt.WorkDate = '" + sAttnDate + @"' OR (AttDt.WorkDate = '" + sAttnDatePrev + @"' ))
                            WHERE (E.DOS >= '" + sAttnDate + @"' OR DOS IS NULL) AND E.DOJ <= '" + sAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                  AND E.SystemID IN (" + sEmpSysIdColl + @")
                            ORDER BY E.EmployeeCode";//and attdt.OutTime is null

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
    private void GetShiftType_Last(string empid, string workdate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;
        string sAttnDatePrev = string.Empty;
        try
        {
            strSql = @"select * from EmpDateWiseShiftAssign e
                                left join ShiftDefination s on e.ShiftSystemID=s.SystemID
                                where WorkDate='" + workdate + @"' and EmpSystemID='" + empid + @"' and s.ShiftType='Day Shift'";

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
    private void GetOffDuty(string plantid, string workdate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;
        string sAttnDatePrev = string.Empty;
        try
        {
            strSql = @"select 
                            Id,EmpSystemId,FromDate,ToDate,Duration,ApproveType
                            from HourlyOffDuty 
                            where IsApprove=1 
                                    and ApproveType='Deducation'
                                    and WorkDate='" + workdate + @"'
                                    and plantid='" + plantid + "'";

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
    private void GetDayTypePrev(string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;
        string sAttnDatePrev = string.Empty;
        try
        {

            //sAttnDatePrev = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
            strSql = @"SELECT * FROM dbo.EmpDateWiseShiftAssign 
						WHERE (WorkDate = '" + sAttnDate + @"' or WorkDate = '" + sAttnDate + @"') AND plantid = '" + sPlantID + @"' and EmpSystemID IN (" + sEmpSysIdColl + @")";

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
    private void GetHoliday(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;
        string sAttnDatePrev = string.Empty;
        sAttnDatePrev = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
        try
        {
            strSql = @"SELECT OFM.CldDescription, OFM.FromDate, OFM.ToDate, OFM.OffDayType, OFM.TotalDay, OFD.DayName, OFM.PlantID  ,OFD.OffDayDate 
	                            FROM scs.OffDayMaster OFM
			                            INNER JOIN scs.OffDayDetail OFD ON OFM.Id = OFD.OffDayMasterId 
                                                                    AND (OFD.OffDayDate = '" + sAttnDatePrev + @"' or OFD.OffDayDate = '" + sAttnDate + @"')
                                WHERE OFM.CompanyGroupId = '" + sGroupID + @"' AND OFM.PlantID = '" + sPlantID + @"'
									  AND OFM.OffDayType = 'H'";

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
    private void GetEmployeeInfo_SNA(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT E.*, ES.*, ISNULL(DATEDIFF(D, Atd.LastWorkDate, '" + sAttnDate + @"'), 0) DateDiffer, ISNULL(Atd.LastWorkDate, GETDATE()) LastWorkDate, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle, EmOT.OTStartDate, EmOT.OTEndDate,
                                  ISNULL(AttDt.ToReprocess, 'YES') ToReprocess
	                        FROM 
                            (
                             SELECT * FROM EmployeeInformation WHERE 
                                    SystemID IN (" + sEmpSysIdColl + @")
                            ) AS E 
		                        left JOIN (
											SELECT * FROM
														(
														 SELECT ES.EmpSystemID, ES.ShiftSystemID, ES.DayType, S.ShiftType, 
																OfficeStartTime = CASE WHEN C.InTimeStartMargin != '' THEN DATEADD(MI, -C.InTimeStartMargin, C.InTime)
																					  ELSE DATEADD(MI, -S.InTimeStartMargin, S.InTime) END, 
																OfficeTime = CASE WHEN C.LateMargin != '' THEN DATEADD(MI, C.LateMargin, C.InTime)
																					  ELSE DATEADD(MI, S.LateMargin, S.InTime) END,
																InTime = CASE WHEN C.InTime != '' THEN C.InTime
																					  ELSE S.InTime END, 

                                                                AbsentEndMargin = CASE WHEN C.AbsentEndMargin != '' THEN C.AbsentEndMargin
																					  ELSE S.AbsentEndMargin END,

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
                                LEFT JOIN dbo.AttdnProcessData AS AttDt ON E.SystemID = AttDt.EmpSystemID AND AttDt.WorkDate = '" + sAttnDate + @"'
                            WHERE (E.DOS > '" + sAttnDate + @"' OR DOS IS NULL) AND E.DOJ <= '" + sAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                  AND E.SystemID IN (" + sEmpSysIdColl + @")
                            ORDER BY E.EmployeeCode";

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
    private void GetAttdnManualData(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            //by monir 190205
            //strSql = @"SELECT * FROM AttdnManualData
            //           WHERE WorkDate = '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
            //                 AND EmpSystemID IN (" + sEmpSysIdColl + @"                                                     
            //                                    )";

            //by monnir 190227
            //strSql = @"select * from 
            //                    (
            //                    select EmpSystemID,WorkDate,PlantID,DayStatus,InTime,OutTime from AttdnManualData
            //                    union
            //                    select EmployeeId EmpSystemID,pDate WorkDate,PlantId PlantID,null DayStatus,InTime,OutTime from [AttdnRawDataFromApp]
            //                    ) x where EmpSystemID IN (" + sEmpSysIdColl + @")  and WorkDate='" + sAttnDate + @"' 
            //                   ";

            //       strSql = @"select * from 
            //                           (
            //                           select EmpSystemID,WorkDate,PlantID,DayStatus,InTime,OutTime, DateUpdated udate,DateAdded adate from AttdnManualData
            //                           union
            //                           select EmployeeId EmpSystemID,pDate WorkDate,PlantId PlantID,null DayStatus,InTime,OutTime
            //                           ,UpdatedDate uDate,AddedDate adate from [AttdnRawDataFromApp]
            //                           ) x where EmpSystemID IN (" + sEmpSysIdColl + @")  and WorkDate='" + sAttnDate + @"' 
            //order by udate desc,adate desc";

            strSql = @"select * from 
                                    (
                                    select EmpSystemID,WorkDate,PlantID,DayStatus,InTime,OutTime, DateUpdated udate,DateAdded adate from AttdnManualData                                   
                                    ) x where EmpSystemID IN (" + sEmpSysIdColl + @")  and WorkDate='" + sAttnDate + @"' 
									order by udate desc,adate desc";

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
    private void GetManualData(string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;
        try
        {
            string FromDdate = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
            string ToDdate = Convert.ToDateTime(sAttnDate).AddDays(+1).ToString("dd-MMM-yyyy");

            strSql = @"select * from 
                                    (
                                    select EmpSystemID,WorkDate,PlantID,DayStatus,InTime,OutTime, DateUpdated udate,DateAdded adate from AttdnManualData                                    
                                    ) x where EmpSystemID IN (" + sEmpSysIdColl + @")  and WorkDate between '" + FromDdate + @"' and '" + ToDdate + @"'
                                    and plantid='" + sPlantID + @"'
									order by udate desc,adate desc";

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
    private void GetODMasterIds(string sGroupID, string sPlantID, string sEmpSystemIDColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT Id SystemID FROM EmployeeOnDuty 
                                    WHERE GroupID = '" + sGroupID + @"' and IsNull(IsApproved,0) = 1
                                        AND EmpSystemID IN (                                                                                      
                                                            " + sEmpSystemIDColl + @"                                                                                       
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
    }//End Function 
    private void GetLeaveTrnDetailIds(string sGroupID, string sPlantID, string sEmpSystemIDColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT SystemID FROM LeaveTransaction 
                                                             WHERE GroupID = '" + sGroupID + @"' and IsNull(IsApproved,0) = 1
                                                                   AND EmpSystemID IN (
                                                                                       --SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', '" + sPlantID + @"') 
                                                                                        --    WHERE JobLcSystemID IN (
                                                                                         --                           SELECT SystemID FROM [dbo].[JobLocation] 
                                                                                         --                               WHERE PlantID = '" + sPlantID + @"'
                                                                                          --                         ) AND EmpSystemID IN (
                                                                                        " + sEmpSystemIDColl + @"
                                                                                        --)
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
    }//End Function 
    private void GetLeaveTransactionDetails(string sAttnDate, string sLeaveDetailIds, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM LeaveTransactionDetails 
                                WHERE WorkDate = '" + sAttnDate + @"' 
                                    AND LvTrnsSystemID IN (
                                                           " + sLeaveDetailIds + @"
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
    }//End Function 
    private void GetODDetails(string sAttnDate, string odmasterids, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM EmployeeOnDutyDetails 
                                WHERE WorkDate = '" + sAttnDate + @"' 
                                    AND OnDutyId IN (
                                                           " + odmasterids + @"
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
    }//End Function 
    private void GetRestInfo(string sAttnDate, string plantid, string emplist, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT d.Id RestId,d.AttendanceRestId,d.PlantId,d.EmpSystemId,m.Id MasterId,m.AttendanceRestDate FROM AttendanceRest m
                            LEFT JOIN AttendanceRestDetail d on d.AttendanceRestId=m.Id
                                    WHERE m.AttendanceRestDate='" + sAttnDate + "' and d.PlantId='" + plantid + "' and d.EmpSystemId in (" + emplist + ")";

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
    private void GetODInfo(string sAttnDate, string plantid, string emplist, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT d.Id OdId,m.PlantId,m.EmpSystemId,m.Id MasterId,d.Workdate FROM EmployeeOnDuty m
                            LEFT JOIN EmployeeOnDutyDetails d on d.OnDutyId=m.Id
                                    WHERE d.Workdate='" + sAttnDate + @"' and m.PlantId='" + plantid + @"'
									 and m.EmpSystemId in (" + emplist + @") and m.IsApproved=1";

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
    private void GetLeaveTransactionInfo(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT LTD.SystemID, LTD.LvTrnsSystemID, LT.EmpSystemID, LT.LTSystemID, LT.FromDate, LT.ToDate, LTD.LeaveDuration LeaveDays, LT.LvReason,
                             LTD.WorkDate, LTD.DayType, LTD.LeaveStatus, LTD.IsAvailed,LTD.IsFirstHalf,tt.LeaveType LWP
                            FROM LeaveTransaction LT
		                        INNER JOIN LeaveTransactionDetails LTD ON LT.SystemID = LTD.LvTrnsSystemID 
                                INNER join LeaveType tt on tt.id=LT.LTSystemID
                                        AND LTD.WorkDate = '" + sAttnDate + @"'
                            WHERE LT.GroupID = '" + sGroupID + @"' AND LT.IsApproved = 1 AND LT.EmpSystemID IN (" + sEmpSysIdColl + @")";

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
    private void GetAvailedLvInfo(string sGroupID, string sPlantID, string strYrSystemID, string strFromDate, string strToDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT LT.EmpSystemID, LPD.SystemID LvPolDtlSystemID, LT.LTSystemID, ISNULL(Count(LTD.SystemID), 0) Availed
                            FROM (select * from LeaveTransaction where IsApproved=1) LT
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
    private void GetAllPlantOffDayInformation(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT OFM.CldDescription, OFM.FromDate, OFM.ToDate, OFM.OffDayType, OFM.TotalDay, OFD.DayName, OFM.PlantID  
	                            FROM scs.OffDayMaster OFM
			                            INNER JOIN scs.OffDayDetail OFD ON OFM.Id = OFD.OffDayMasterId 
                                                                    AND OFD.OffDayDate = '" + sAttnDate + @"'
                                WHERE OFM.CompanyGroupId = '" + sGroupID + @"' AND OFM.PlantID = '" + sPlantID + @"'
									  AND OFM.OffDayType = 'H'";

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
    public void GetFinalOT(string sGroupID, string sEmpSystemIDColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.FinalOT
                                    WHERE WorkDate = '" + sAttnDate + @"' AND GroupID = '" + sGroupID + @"'
                                          AND EmpSystemID IN (" + sEmpSystemIDColl + @"
                                                             --SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', ) 
                                                             --   WHERE JobLcSystemID IN (
                                                             --                           SELECT SystemID FROM [dbo].[JobLocation] 
                                                             --                               WHERE PlantID = 
                                                             --                          )
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
    }//End Function 
    public void GetOTSlabDefineEmployee(string sGroupID, string sEmpSystemIDColl, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.OTSlabDefineEmployee
                           WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate 
                                AND GroupID = '" + sGroupID + @"'
                                AND EmpSystemID IN (" + sEmpSystemIDColl + @"
                                                    --SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', ) 
                                                    --    WHERE JobLcSystemID IN (
                                                    --                            SELECT SystemID FROM [dbo].[JobLocation] 
                                                    --                                WHERE PlantID = 
                                                    --                            )
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
    }//End Function 
    public void GetOTSlabDefineGeneral(string sGroupID, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.OTSlabDefineGeneral
                           WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate AND GroupID = '" + sGroupID + @"'";

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


    public void GetAttdnDataMonthlySummary(string sGroupID, int MonthNo, int YearNo, string sEmpSystemIDColl, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.AttdnDataMonthlySummary
                           WHERE GroupID = '" + sGroupID + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")
                                    AND MonthNo = " + MonthNo + @" AND YearNo = " + YearNo + @"";

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
    public void GetShiftDefination(string sGroupID, string sPlantID, out DataSet dsRef)
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

    public bool GetBoolData(string inputData)
    {
        /*
         * Added By Shohel
         * return false for null, empty, 0, false, no
         * return true for yes, true, 1
         * */

        bool FLAG = false;
        if (string.IsNullOrEmpty(inputData) == true)//null or empty
        {
            FLAG = false;
        }
        else if (string.IsNullOrEmpty(inputData.Trim()) == true)//null or empty
        {
            FLAG = false;
        }
        else if (string.Compare(inputData.Trim(), "0", true) == 0)
        {
            FLAG = false;
        }
        else if (string.Compare(inputData.Trim(), "NO", true) == 0)
        {
            FLAG = false;
        }
        else if (string.Compare(inputData.Trim(), "FALSE", true) == 0)
        {
            FLAG = false;
        }
        else if (string.Compare(inputData.Trim(), "YES", true) == 0)
        {
            FLAG = true;
        }
        else if (string.Compare(inputData.Trim(), "TRUE", true) == 0)
        {
            FLAG = true;
        }
        else if (string.Compare(inputData.Trim(), "1", true) == 0)
        {
            FLAG = true;
        }

        return FLAG;
    } // End Function
    public static string GetNumData(string strNumber)
    {
        double d;
        strNumber = strNumber.Replace(",", "");
        System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
        if (strNumber.Trim() == "")
        { return "0"; }
        else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
        {
            return strNumber;
        }
        else
        {
            return "0";
        }
    }// end function

    public void GetRemoteAttdnProcData(string sGroupID, string empids, string strPrvAttnDate, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT * FROM dbo.AttdnRawDataFromApp
                           WHERE PDate BETWEEN '" + strPrvAttnDate + @"' 
                                 AND '" + sAttnDate + @"' 
                                 AND EmployeeId IN (
                                                     " + empids + @"
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
    }//End Function  

    public void GetAttendanceLockInfo(string PlantId, string _fromDate, string _toDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"select  FORMAT(LockedDate,'dd-MMM-yyyy') LockedDate from PlantWiseAttendanceLock where PlantId='" + PlantId + @"' 
                                and LockedDate  between '" + _fromDate + "' and '" + _toDate + "' and IsActive=1";

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
    public void GetAttendanceLockWithException(string empids, string PlantId, string _fromDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"select distinct EmployeeCode from EmployeeInformation 
                                    where PlantId in
                                    (
                                    select  p.PlantId
                                     from PlantWiseAttendanceLock p
                                    where p.PlantId='" + PlantId + @"' 
                                    and p.LockedDate  ='" + _fromDate + @"' 
                                    and p.IsActive=1 
                                    )
                                    and systemid not in
                                    (
                                    select EmpSystemId from ExceptionEmployeeAttendanceUnlock where EmpSystemId in (" + empids + ") and workdate='" + _fromDate + @"'
                                    )
                                    and systemid in (" + empids + ")";

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
    public void GetAttendanceLockInfo(string empids, string PlantId, string _fromDate, string _toDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            //string _wc = "";
            //if(empids.Length>0)
            //{
            //    _wc = " and empsystemid in (" + empids + ")";
            //}
            //strSql = @"select  FORMAT(WorkDate,'dd-MMM-yyyy') LockedDate ,e.EmployeeCode from AttdnProcessData w
            //                        left join EmployeeInformation e on e.systemid=w.empsystemid
            //            where w.PlantId='" + PlantId + @"' "+_wc+ @" 
            //    and WorkDate  between '" + _fromDate + "' and '" + _toDate + "' and w.Islock=1";

            if (empids.Length == 0)
            {
                empids = "''";
            }
            strSql = @"select distinct EmployeeCode from EmployeeInformation 
                                    where PlantId in
                                    (
                                    select  p.PlantId
                                     from PlantWiseAttendanceLock p
                                    where p.PlantId='" + PlantId + @"' 
                                    and p.LockedDate  ='" + _fromDate + @"' 
                                    and p.IsActive=1 
                                    )
                                    and systemid not in
                                    (
                                    select EmpSystemId from ExceptionEmployeeAttendanceUnlock where EmpSystemId in (" + empids + ") and workdate='" + _fromDate + @"'
                                    )
                                    and systemid in (" + empids + ")";

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
     //End Function  
     //ooo
}

