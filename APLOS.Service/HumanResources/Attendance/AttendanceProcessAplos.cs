using OTSBD;
//using OTSBD.clsAttendance;
//using OTSBD.clsGeneral;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TBS;
using static OTSBD.clsStaticInfo;

namespace clsAttendance
{
    public class AttendanceProcessAplos
    {
        //AttendanceProcessAplos
        //AttendanceProcessAplosTimeSpan
        string sEmpSystemIDColl = string.Empty;
        string lblAttdnProcBase = string.Empty;
        bool radDwLdEnrollID = false;
        private string sMinOT = "";
        private string sFractionCalculate = "";
        HROTSetting _HROTSetting = null;
        public ReturnType SaveTotal(string _plantid, string sAttnDatex, string _emplist, bool _bReProc)
        {
            int _maxRow = 1000;
            DataSet dsEmployeeList = null;
            ReturnType _result = new ReturnType();
            string strYrSystemID, strYrFromDate, strYrToDate, GroupSysID = string.Empty;
            bool IsLocked = false;
            try
            {
                _result.Status = false;
                _result.Message = "Not Processed";

                if (_emplist.Length > 0)//will check exception
                {
                    LockValidation_Plant_WD_EMP(_plantid, sAttnDatex, _emplist, out IsLocked);
                }
                else
                {
                    LockValidation_Plant_WD(_plantid, sAttnDatex, out IsLocked);
                }


                if (IsLocked == false)
                {
                    if (Convert.ToDateTime(sAttnDatex.Trim()) <= Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                    {
                        PlantNameAndHRMSLocation(_plantid, sAttnDatex, out strYrSystemID, out strYrFromDate, out strYrToDate, out GroupSysID);
                        GetHRsetting(_plantid);
                        //-remote att
                        if (_HROTSetting.IsRemoteAttendanceApprovalRequired == false)
                        {
                            GetRemoteDataInManual(_plantid, sAttnDatex);
                        }
                        if (_emplist.Length == 0)//if user created emplist is not found
                        {
                            AttdnProcBaseOn(GroupSysID, _plantid, sAttnDatex, out dsEmployeeList);
                            string _emps = "''";
                            int _Count = 0;
                            for (int i = 0; i < dsEmployeeList.Tables[0].Rows.Count; i++)
                            {
                                _Count++;
                                if (_emps == "''")
                                {
                                    _emps = "'" + dsEmployeeList.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                                }
                                else
                                {
                                    _emps = _emps.Trim() + ", '" + dsEmployeeList.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                                }

                                ///for each 1000 emp the attn-process will run
                                if (_Count >= _maxRow)
                                {
                                    _emplist = _emps;
                                    CoreProcess(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                                    _emps = "''";
                                    _Count = 0;
                                }
                            }

                            //last portion
                            if (_Count < _maxRow)
                            {
                                _emplist = _emps;
                                CoreProcess(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                                _emps = "''";
                                _Count = 0;
                            }
                        }
                        else
                        {
                            CoreProcess(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                        }
                        ///set auto TBS/LA here                   
                        EmployeeAutoStatusChange(_plantid, sAttnDatex);
                        ///MLV
                        MLVProcess(_plantid, sAttnDatex);
                        FinalOTClearance(_plantid, sAttnDatex);
                    }//for each date
                }//IsLocked
                else
                {
                    _result.Status = false;
                    _result.Message = "Day is locked";
                }
                return _result;
            }
            catch (Exception ex)
            {
                throw new Exception(_result.Message + " because " + ex.Message);
            }
        }//End Function 
        public ReturnType SaveTotal(string _plantid, string sAttnDatex, string _emplist, bool _bReProc, bool ShouldAvoidAttendanceLock)
        {
            int _maxRow = 1000;
            DataSet dsEmployeeList = null;
            ReturnType _result = new ReturnType();
            string strYrSystemID, strYrFromDate, strYrToDate, GroupSysID = string.Empty;
            bool IsLocked = false;
            try
            {
                _result.Status = false;
                _result.Message = "Not Processed";
                if (ShouldAvoidAttendanceLock == false)
                {
                    if (_emplist.Length > 0)//will check exception//
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
                        PlantNameAndHRMSLocation(_plantid, sAttnDatex, out strYrSystemID, out strYrFromDate, out strYrToDate, out GroupSysID);
                        GetHRsetting(_plantid);
                        //-remote att
                        if (_HROTSetting.IsRemoteAttendanceApprovalRequired == false)
                        {
                            GetRemoteDataInManual(_plantid, sAttnDatex);
                        }
                        if (_emplist.Length == 0)//if user created emplist is not found
                        {
                            AttdnProcBaseOn(GroupSysID, _plantid, sAttnDatex, out dsEmployeeList);
                            string _emps = "''";
                            int _Count = 0;
                            for (int i = 0; i < dsEmployeeList.Tables[0].Rows.Count; i++)
                            {
                                _Count++;
                                if (_emps == "''")
                                {
                                    _emps = "'" + dsEmployeeList.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                                }
                                else
                                {
                                    _emps = _emps.Trim() + ", '" + dsEmployeeList.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                                }

                                ///for each 1000 emp the attn-process will run
                                if (_Count >= _maxRow)
                                {
                                    _emplist = _emps;
                                    CoreProcess(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                                    _emps = "''";
                                    _Count = 0;
                                }
                            }

                            //last portion
                            if (_Count < _maxRow)
                            {
                                _emplist = _emps;
                                CoreProcess(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                                _emps = "''";
                                _Count = 0;
                            }
                        }
                        else
                        {
                            CoreProcess(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                        }
                        ///set auto TBS/LA here                   
                        EmployeeAutoStatusChange(_plantid, sAttnDatex);
                        ///MLV
                        MLVProcess(_plantid, sAttnDatex);
                        FinalOTClearance(_plantid, sAttnDatex);
                    }//for each date
                }//IsLocked
                else
                {
                    _result.Status = false;
                    _result.Message = "Day is locked";
                    //throw new Exception(_result.Message);
                }
                return _result;
            }
            catch (Exception ex)
            {
                throw new Exception(_result.Message + " because " + ex.Message);
            }
        }//End Function 

        private void GetRemoteDataInManual(string plantid, string pdate)
        {
            DataSet dsRemote = null;
            DataSet dsManual = null;
            DataView dvRemote = null;
            DataView dvManual = null;
            DataSet dsGroup = null;
            try
            {
                string _CG = string.Empty;
                GetGroupId(plantid, out dsGroup);
                GetRemoteData(plantid, pdate, out dsRemote);
                GetManualData(plantid, pdate, out dsManual);

                if (dsGroup.Tables[0].Rows.Count > 0)
                {
                    _CG = dsGroup.Tables[0].Rows[0]["CompanyGroupId"].ToString();
                }

                for (int i = 0; i < dsRemote.Tables[0].Rows.Count; i++)
                {
                    string _Id = dsRemote.Tables[0].Rows[i]["Id"].ToString();
                    string _emp = dsRemote.Tables[0].Rows[i]["EmployeeId"].ToString();
                    string _pdate = dsRemote.Tables[0].Rows[i]["pdate"].ToString();
                    string _InTime = dsRemote.Tables[0].Rows[i]["InTime"].ToString();
                    string _OutTime = dsRemote.Tables[0].Rows[i]["OutTime"].ToString();
                    bool _isApprovedIN = GetBoolData(dsRemote.Tables[0].Rows[i]["isApprovedIN"].ToString());
                    bool _isApprovedOUT = GetBoolData(dsRemote.Tables[0].Rows[i]["isApprovedOUT"].ToString());
                    dvManual = new DataView(dsManual.Tables[0]);
                    dvManual.RowFilter = "EmpSystemID='" + _emp + "' and WorkDate='" + _pdate + "'";
                    if (dvManual.Count == 0)
                    {
                        DataRow dr = dsManual.Tables[0].NewRow();
                        dr["EmpSystemID"] = _emp;
                        dr["WorkDate"] = _pdate;
                        dr["PlantID"] = plantid;
                        dr["GroupId"] = _CG;
                        if (_isApprovedIN == false && string.IsNullOrEmpty(_InTime) == false)
                        {
                            dr["InTime"] = _InTime;
                        }
                        if (_isApprovedOUT == false && string.IsNullOrEmpty(_OutTime) == false)
                        {
                            dr["OutTime"] = _OutTime;
                        }
                        dr["AddedBy"] = "schedule";
                        dr["DateAdded"] = DateTime.Now;
                        dsManual.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dvManual[0].Row;
                        dr.BeginEdit();
                        if (_isApprovedIN == false && string.IsNullOrEmpty(_InTime) == false)
                        {
                            dr["InTime"] = _InTime;
                        }
                        if (_isApprovedOUT == false && string.IsNullOrEmpty(_OutTime) == false)
                        {
                            dr["OutTime"] = _OutTime;
                        }
                        dr["UpdatedBy"] = "schedule";
                        dr["DateUpdated"] = DateTime.Now;
                        dr.EndEdit();
                    }
                    dvManual.RowFilter = null;

                    dvRemote = new DataView(dsRemote.Tables[0]);
                    dvRemote.RowFilter = "Id='" + _Id + "'";
                    if (dvRemote.Count > 0)
                    {
                        DataRow dr = dvRemote[0].Row;
                        dr.BeginEdit();
                        if (_isApprovedIN == false && string.IsNullOrEmpty(_InTime) == false)
                        {
                            dr["isApprovedIN"] = true;
                            dr["ApprovedByIN"] = "schedule";
                            dr["ApprovalDateIN"] = DateTime.Now;
                        }
                        if (_isApprovedOUT == false && string.IsNullOrEmpty(_OutTime) == false)
                        {
                            dr["isApprovedOUT"] = true;
                            dr["ApprovedByOUT"] = "schedule";
                            dr["ApprovalDateOUT"] = DateTime.Now;
                        }
                        dr["UpdatedBy"] = "schedule";
                        dr["UpdatedDate"] = DateTime.Now;
                        dr.EndEdit();
                    }
                    dvRemote.RowFilter = null;
                }

                //clsStaticInfo obj = new clsStaticInfo();
                SaveDataSets(dsRemote, dsManual);

                //string sql1 = @" INSERT INTO AttdnManualData (EmpSystemID, WorkDate,PlantID,InTime,OutTime,AddedBy,DateAdded)
                //                SELECT EmployeeId, PDate, PlantId, InTime, OutTime,'schedule','"+dt+@"' FROM AttdnRawDataFromApp
                //                where PlantId = '"+plantid+ @"' and(isApprovedIN = 0 and isApprovedOUT = 0) and PDate='"+pdate+ @"'
                //                and EmployeeId not in (select EmpSystemID from AttdnManualData where WorkDate='" + pdate + @"') ";

                //string sql2 = @"update AttdnRawDataFromApp set isApprovedIN = 1, isApprovedOUT = 1 
                //            where PlantId = '" + plantid + @"' and pdate='" + pdate + @"' and (isApprovedIN = 0 and isApprovedOUT = 0)
                //            and EmployeeId in (select EmpSystemID from AttdnManualData where WorkDate = '" + pdate + @"' and addedby='schedule') ";
                //UpdateRemote(sql1, sql2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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

        }//End Function  
        public void UpdateRemote(string sql1, string sql2)
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
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
                objCon.ExecuteNonQueryWrapper(sql2, true, "1");
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
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function

        private void EmployeeAutoStatusChange(string plantid, string adate)
        {
            try
            {
                ///make LA/TBS
                EmployeeAutoStatusChange_LA(plantid, adate);
                EmployeeAutoStatusChange_TBS(plantid, adate);
                ///Reverse LA/TBS
                EmployeeAutoStatusChange_LA_Reverse(plantid, adate);
                //EmployeeAutoStatusChange_TBS_Reverse(plantid, adate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region MLV        
        private void MLVProcess(string plantid, string adate)
        {
            DataSet dsTransaction = null;
            DataSet dsAttendance = null;
            DataView dv = null;
            string EmpIds = string.Empty;
            try
            {
                GetMLVTransaction(plantid, adate, out dsTransaction);//+Policy
                if (dsTransaction.Tables[0].Rows.Count > 0)
                {
                    GetEmpIds(dsTransaction, out EmpIds);
                    GetProcessedAttendanceData(plantid, adate, EmpIds, out dsAttendance);

                    for (int i = 0; i < dsTransaction.Tables[0].Rows.Count; i++)
                    {
                        string _Status = string.Empty;
                        string _FDate = dsTransaction.Tables[0].Rows[i]["FromDate"].ToString();
                        string _ToDate = dsTransaction.Tables[0].Rows[i]["ToDate"].ToString();
                        string _EDD = dsTransaction.Tables[0].Rows[i]["ExpectedDelivaryDate"].ToString();
                        string _EmpSystemid = dsTransaction.Tables[0].Rows[i]["EmpSystemid"].ToString();
                        //string MaternityLeaveEndDay = dsTransaction.Tables[i].Rows[i]["MaternityLeaveEndDay"].ToString();
                        //string MaternityLeaveStartDay = dsTransaction.Tables[i].Rows[i]["MaternityLeaveStartDay"].ToString();//IsBenefitApplicable
                        string MaternityEndDay = dsTransaction.Tables[0].Rows[i]["MaternityEndDay"].ToString();
                        string MaternityStartDay = dsTransaction.Tables[0].Rows[i]["MaternityStartDay"].ToString();
                        bool IsBenefitApplicable = Convert.ToBoolean(dsTransaction.Tables[0].Rows[i]["IsNoBenefit"].ToString());

                        //var _PreDays = Convert.ToDateTime(_EDD).AddDays(-Convert.ToInt32(MaternityStartDay));
                        //var _PostDays = Convert.ToDateTime(_EDD).AddDays(Convert.ToInt32(MaternityEndDay));

                        var _PreDays = Convert.ToDateTime(_FDate).AddDays(-Convert.ToInt32(MaternityStartDay));
                        var _PostDays = Convert.ToDateTime(_ToDate).AddDays(Convert.ToInt32(MaternityEndDay));

                        #region code
                        if (Convert.ToDateTime(adate) >= _PreDays && Convert.ToDateTime(adate) < Convert.ToDateTime(_FDate))//less than fromdate but Maternity started
                        {
                            _Status = "PRE";
                        }
                        else if (Convert.ToDateTime(adate) < _PostDays && Convert.ToDateTime(adate) > Convert.ToDateTime(_ToDate))//less than fromdate but Maternity started
                        {
                            _Status = "POST";
                        }
                        else if (Convert.ToDateTime(adate) >= Convert.ToDateTime(_FDate) && Convert.ToDateTime(adate) <= Convert.ToDateTime(_ToDate))//less than fromdate but Maternity started
                        {
                            _Status = "MLV";
                        }

                        ///filter att and update
                        if (string.IsNullOrEmpty(_Status) == false)
                        {
                            dv = new DataView(dsAttendance.Tables[0]);
                            dv.RowFilter = "EmpSystemid='" + _EmpSystemid + "'";
                            if (dv.Count > 0)
                            {
                                DataRow dr = dv[0].Row;
                                dr.BeginEdit();
                                dr["MaternityStatus"] = _Status;
                                if (_Status == "MLV")
                                {
                                    dr["IsLWP"] = IsBenefitApplicable;
                                }
                                dr.EndEdit();
                            }
                            dv.RowFilter = null;
                        }//_Status 
                        #endregion
                    }//loop
                    //clsStaticInfo obj = new clsStaticInfo();
                    SaveDataSets(dsAttendance);
                }//dsTransaction.Tables[0].Rows.Count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void FinalOTClearance(string plantid, string adate)
        {
            try
            {
                DeleteFinalOTOtherThanPresent(plantid, adate);//+Policy                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        void GetEmpIds(DataSet dsMLVTransac, out string empids)
        {
            try
            {
                empids = string.Empty;
                for (int i = 0; i < dsMLVTransac.Tables[0].Rows.Count; i++)
                {
                    string _EmpSystemid = dsMLVTransac.Tables[0].Rows[i]["EmpSystemid"].ToString();
                    if (empids == "")
                    {
                        empids = "'" + _EmpSystemid + "'";
                    }
                    else
                    {
                        empids += ",'" + _EmpSystemid + "'";

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetMLVTransaction(string PlantId, string WorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"select t.EmpSystemID,t.PlantID,t.FromDate,t.ToDate,t.LeaveDays,t.IsApproved,p.IsNoBenefit
                                   ,t.MaternityLeavePolicyId,p.MaternityStartDay,p.MaternityEndDay,p.MaternityLeaveStartDay,p.MaternityLeaveEndDay,t.ExpectedDelivaryDate
                                    from LeaveTransaction t 
                                    left join [MST].[MaternityLeavePolicy] p on p.Id=t.MaternityLeavePolicyId
                                    where t.LTSystemID in (select id from LeaveType where LeaveType='Maternity')
                                    --and '" + WorkDate + @"'  between DATEADD(DAY,-p.MaternityStartDay,t.ExpectedDelivaryDate) and DATEADD(DAY,p.MaternityEndDay,t.ExpectedDelivaryDate)
                                    and '" + WorkDate + @"'  between DATEADD(DAY,-p.MaternityStartDay,t.FromDate) and DATEADD(DAY,p.MaternityEndDay,t.ToDate)
                                    and t.IsApproved=1 and t.PlantID='" + PlantId + @"' ";

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
        public void DeleteFinalOT(string empid, string fromdate, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                DateTime dt = Convert.ToDateTime(fromdate);
                string FD = "01-" + dt.ToString("MMM") + "-" + dt.ToString("yyyy");
                DateTime dtnextMonth = dt.AddMonths(1);
                string TD = dtnextMonth.AddDays(-1).ToString("dd") + "-" + dt.ToString("MMM") + "-" + dt.ToString("yyyy");


                string _sql = @"delete from FinalOT where EmpSystemID='" + empid + "' and WorkDate between '" + FD + "' and '" + TD + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(_sql, true, "1");
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
        }//End Function
        public void DeleteFinalOTOtherThanPresent(string plantid, string fromdate)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //DateTime dt = Convert.ToDateTime(fromdate);
                //string FD = "01-" + dt.ToString("MMM") + "-" + dt.ToString("yyyy");
                //DateTime dtnextMonth = dt.AddMonths(1);
                //string TD = dtnextMonth.AddDays(-1).ToString("dd") + "-" + dt.ToString("MMM") + "-" + dt.ToString("yyyy");


                string _sql = @"delete from FinalOT where PlantID='" + plantid + @"' and WorkDate='" + fromdate + @"' and EmpSystemID in
                                    (
                                    select EmpSystemID from AttdnProcessData where WorkDate='" + fromdate + @"' and PlantID='" + plantid + @"'
                                    and DayStatus in (select DayType from DayType where Category not in ('Present','Late'))
                                    )";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(_sql, true, "1");
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
        }//End Function

        public void GetProcessedAttendanceData(string PlantId, string WorkDate, string empsids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @" select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantId + "' and EmpSystemID in (" + empsids + ")";

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
        #endregion MLV

        private void EmployeeAutoStatusChange_LA_Reverse(string plantid, string Todate)
        {
            DataSet ds_isauto_LA = null;
            //DataSet ds_isauto_TBS = null;
            DataSet ds_tobe_Active = null;
            try
            {
                GetHRSettingForAutoLA(plantid, out ds_isauto_LA);
                //GetHRSettingForAutoTBS(plantid, out ds_isauto_TBS);

                if (ds_isauto_LA.Tables[0].Rows.Count > 0)//LA
                {
                    string maxDays = GetNumData(ds_isauto_LA.Tables[0].Rows[0]["LongTermAbesnteeism"].ToString());
                    if (Convert.ToInt32(maxDays) > 0)
                    {
                        string FromDate = Convert.ToDateTime(Todate).AddDays(-Convert.ToInt32(maxDays)).ToString("dd-MMM-yyyy");
                        Get_tobe_Active_from_LA(plantid, FromDate, Todate, out ds_tobe_Active);
                        if (ds_tobe_Active.Tables[0].Rows.Count > 0)
                        {
                            UpdateEmpStatus_Reverse(plantid, Todate, ds_tobe_Active);//update these emps as LA
                        }
                    }//>0
                }//LA
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void EmployeeAutoStatusChange_LA(string plantid, string adate)
        {
            DataSet ds_isauto_LA = null;
            //DataSet ds_isauto_TBS = null;
            DataSet ds_tobe_LA = null;
            try
            {
                GetHRSettingForAutoLA(plantid, out ds_isauto_LA);
                //GetHRSettingForAutoTBS(plantid, out ds_isauto_TBS);

                if (ds_isauto_LA.Tables[0].Rows.Count > 0)//LA
                {
                    string maxDays = GetNumData(ds_isauto_LA.Tables[0].Rows[0]["LongTermAbesnteeism"].ToString());
                    if (Convert.ToInt32(maxDays) > 0)
                    {
                        Get_tobe_LA(plantid, adate, maxDays, out ds_tobe_LA);
                        //DataSet dsEffectiveDCount = null;
                        //Get_tobe_LA_days_for_effectiveDate(plantid, adate, maxDays, out dsEffectiveDCount);
                        //if(dsEffectiveDCount.Tables[0].Rows.Count>0)
                        //{
                        //    string v = GetNumData(dsEffectiveDCount.Tables[0].Rows[0]["absentDays"].ToString());
                        //    adate = Convert.ToDateTime(adate).AddDays(-Convert.ToInt32(v)).ToString("dd-MMM-yyyy");
                        //}

                        if (ds_tobe_LA.Tables[0].Rows.Count > 0)
                        {
                            UpdateEmpStatusLA(plantid, adate, ds_tobe_LA);//update these emps as LA
                        }
                    }//>0
                }//LA
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void EmployeeAutoStatusChange_TBS_Reverse(string plantid, string Todate)
        {
            DataSet ds_isauto_TBS = null;
            //DataSet ds_tobe_TBS = null;
            //DataSet ds_tobe_Active_from_LA = null;
            DataSet ds_tobe_Active_from_TBS = null;
            try
            {
                GetHRSettingForAutoTBS(plantid, out ds_isauto_TBS);

                if (ds_isauto_TBS.Tables[0].Rows.Count > 0)//LA
                {
                    string maxDays = GetNumData(ds_isauto_TBS.Tables[0].Rows[0]["TBSDays"].ToString());
                    if (Convert.ToInt32(maxDays) > 0)
                    {
                        string FromDate = Convert.ToDateTime(Todate).AddDays(-Convert.ToInt32(maxDays)).ToString("dd-MMM-yyyy");
                        Get_tobe_Active_from_TBS(plantid, FromDate, Todate, out ds_tobe_Active_from_TBS);
                        if (ds_tobe_Active_from_TBS.Tables[0].Rows.Count > 0)
                        {
                            UpdateEmpStatus_Reverse(plantid, Todate, ds_tobe_Active_from_TBS);//update these emps as LA
                        }
                    }//>0
                }//LA
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void EmployeeAutoStatusChange_TBS(string plantid, string adate)
        {
            DataSet ds_isauto_TBS = null;
            DataSet ds_tobe_TBS = null;
            //DataSet ds_tobe_Active_from_LA = null;
            DataSet ds_tobe_Active_from_TBS = null;
            try
            {
                GetHRSettingForAutoTBS(plantid, out ds_isauto_TBS);

                if (ds_isauto_TBS.Tables[0].Rows.Count > 0)//LA
                {
                    string maxDays = GetNumData(ds_isauto_TBS.Tables[0].Rows[0]["TBSDays"].ToString());
                    if (Convert.ToInt32(maxDays) > 0)
                    {
                        Get_tobe_TBS(plantid, adate, maxDays, out ds_tobe_TBS);
                        //DataSet dsEffectiveDCount = null;
                        //Get_tobe_TBS_days_for_effectiveDate(plantid, adate, maxDays, out dsEffectiveDCount);
                        //if (dsEffectiveDCount.Tables[0].Rows.Count > 0)
                        //{
                        //    string v = GetNumData(dsEffectiveDCount.Tables[0].Rows[0]["absentDays"].ToString());
                        //    adate = Convert.ToDateTime(adate).AddDays(-Convert.ToInt32(v)).ToString("dd-MMM-yyyy");
                        //}
                        if (ds_tobe_TBS.Tables[0].Rows.Count > 0)
                        {
                            UpdateEmpStatusTBS(plantid, adate, ds_tobe_TBS);//update these emps as LA
                        }
                    }//>0
                }//LA
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void UpdateEmpStatus_Reverse(string PlantId, string adate, DataSet dsLA)
        {
            string strSql = string.Empty;
            string _empids = string.Empty;
            try
            {

                for (int i = 0; i < dsLA.Tables[0].Rows.Count; i++)
                {
                    string _empid = dsLA.Tables[0].Rows[i]["systemid"].ToString();
                    if (_empids.Length == 0)
                    {
                        _empids = "'" + _empid + "'";
                    }
                    else
                    {
                        _empids += ",'" + _empid + "'";
                    }
                }//for

                if (_empids.Length == 0)
                {
                    _empids = " ";
                }
                else
                {
                    _empids = " and systemid in (" + _empids + ")";
                }
                strSql = @"update EmployeeInformation set EmployeeCurrentStatus=null,EmployeeCurrentStatusEffectiveDate=null where plantid='" + PlantId + "' " + _empids + "";
                UpdateEmpStatus(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function 

        public void UpdateEmpStatusLA(string PlantId, string adate, DataSet dsLA)
        {
            string strSql = string.Empty;
            string _empids = string.Empty;
            try
            {

                for (int i = 0; i < dsLA.Tables[0].Rows.Count; i++)
                {
                    string _empid = dsLA.Tables[0].Rows[i]["Id"].ToString();

                    //DataView dvemp = new DataView(dsEffectiveDCount.Tables[0]);
                    //dvemp.RowFilter = "EmpSystemID='" + _empid + "'";
                    //if(dvemp.Count>0)
                    //{
                    //    string v = GetNumData(dvemp[0]["absentDays"].ToString());
                    //    adate = Convert.ToDateTime(adate).AddDays(-Convert.ToInt32(v)).ToString("dd-MMM-yyyy");
                    //}
                    string v = dsLA.Tables[0].Rows[i]["FirstAbsentDate"].ToString();
                    adate = Convert.ToDateTime(v).AddDays(-1).ToString("dd-MMM-yyyy");
                    if (strSql.Length == 0)
                    {
                        strSql = @"update EmployeeInformation set EmployeeCurrentStatus='LONG ABSENTEEISM',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                    }
                    else
                    {
                        strSql += Environment.NewLine + @"update EmployeeInformation set EmployeeCurrentStatus='LONG ABSENTEEISM',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                    }


                }//for

                //if (_empids.Length == 0)
                //{
                //    _empids = " ";
                //}
                //else
                //{
                //    _empids = " and systemid in (" + _empids + ")";
                //}

                //strSql = @"update EmployeeInformation set EmployeeCurrentStatus='LONG ABSENTEEISM',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='"+PlantId+"' "+ _empids + "";
                UpdateEmpStatus(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function 
        public void UpdateEmpStatusTBS(string PlantId, string adate, DataSet dsLA)
        {
            string strSql = string.Empty;
            string _empids = string.Empty;
            try
            {

                for (int i = 0; i < dsLA.Tables[0].Rows.Count; i++)
                {
                    string _empid = dsLA.Tables[0].Rows[i]["Id"].ToString();
                    string v = dsLA.Tables[0].Rows[i]["FirstAbsentDate"].ToString();
                    adate = Convert.ToDateTime(v).AddDays(-1).ToString("dd-MMM-yyyy");
                    //DataView dvemp = new DataView(dsEffectiveDCount.Tables[0]);
                    //dvemp.RowFilter = "EmpSystemID='" + _empid + "'";
                    //if (dvemp.Count > 0)
                    //{
                    //    string v = GetNumData(dvemp[0]["absentDays"].ToString());
                    //    adate = Convert.ToDateTime(adate).AddDays(-Convert.ToInt32(v)).ToString("dd-MMM-yyyy");
                    //}
                    if (strSql.Length == 0)
                    {
                        strSql = @"update EmployeeInformation set EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                    }
                    else
                    {
                        strSql += Environment.NewLine + @"update EmployeeInformation set EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                    }
                }//for

                //if (_empids.Length == 0)
                //{
                //    _empids = " ";
                //}
                //else
                //{
                //    _empids = " and systemid in (" + _empids + ")";
                //}
                //strSql = @"update EmployeeInformation set EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "' " + _empids + "";
                UpdateEmpStatus(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function  
        public void UpdateEmpStatus(string sql)
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
                objCon.ExecuteNonQueryWrapper(sql, true, "1");
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
        }//End Function

        public void GetHRSettingForAutoLA(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select  LongTermAbesnteeism from PlantWiseHRMSSetting where PlantId='" + PlantId + "' and IsLongAbsenteeismAuto=1";

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
        public void GetHRSettingForAutoTBS(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select  TBSDays from PlantWiseHRMSSetting where PlantId='" + PlantId + "' and IsTBSAuto=1";

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
        public void Get_tobe_Active_from_LA(string PlantId, string fdate, string tdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select systemid,EmployeeStatus,EmployeeCurrentStatus,EmployeeCurrentStatusEffectiveDate  from EmployeeInformation
                                where EmployeeStatus='Active' and EmployeeCurrentStatus='LONG ABSENTEEISM' and EmployeeCurrentStatusEffectiveDate<='" + tdate + @"'
                                and systemid in
                                (--2
                                select EmpSystemID from AttdnProcessData where PlantId='" + PlantId + "' and WorkDate between  '" + fdate + "'  and '" + tdate + @"' and (DayStatus
                                in (select DayType from DayType where Category in ('Present','Late','Leave','Half Day')) )
                                )--2
                                and systemid not in
                                    (--1
	                                select w.empsystemid from AttdnProcessData w
									left join PlantWiseHRMSSetting p on p.PlantID=w.PlantId
									where w.PlantId='" + PlantId + @"' and WorkDate between  '" + fdate + "'  and '" + tdate + @"' and p.IsAttendanceLockApplicable=1
                                    and IsLock=1
									)--1
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
        public void Get_tobe_Active_from_TBS(string PlantId, string fdate, string tdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select systemid,EmployeeStatus,EmployeeCurrentStatus,EmployeeCurrentStatusEffectiveDate  from EmployeeInformation
                                where EmployeeStatus='Active' and EmployeeCurrentStatus='TBS' and EmployeeCurrentStatusEffectiveDate<'" + tdate + @"'
                                and systemid in
                                (
                                select EmpSystemID from AttdnProcessData where PlantId='" + PlantId + "' and WorkDate between  '" + fdate + "'  and '" + tdate + @"' and (DayStatus
                                in (select DayType from DayType where Category in ('Present','Late','Leave','Half Day')))
                                )
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
        public void xGet_tobe_LA(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"WITH CTE
	                            AS (
		                            SELECT *
			                            ,MAX(CASE 
			                            when Category in ('Present','Late','Leave','Half Day') then 1							
					                            ELSE 0
					                            END) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS mx
				                            -- status of the latest date
			                            ,first_value(DayStatus) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS fv
		                            FROM AttdnProcessData  left join DayType on AttdnProcessData.DayStatus=DayType.daytype 
		                            )
	                            SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS absentDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
	                            FROM cte

	                            LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
	                            WHERE fv = 'A' -- current status = 'A'
		                            AND mx = 0 -- all rows before the 1st 'P'
			                            and DayStatus not in (select distinct DayType from DayType where Category in ('Holiday','Weekend'))
			                            --and OutTime is null
			                            AND (EI.EmployeeStatus = 'Active') AND CONVERT(DATE,CTE.WorkDate) <= '" + adate + @"'  
			                            and ei.EmployeeCurrentStatus is null
	                            GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode
	                            HAVING
		                            -- at least three days absent
		                            COUNT(*) >= " + maxDays + @"
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
        public void Get_tobe_LA(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                                DEP.UserName AS Department,de.UserName AS designation,
                                sec.UserName AS Section,ss.UserName AS SubSection,
                                D.DayStatus,COUNT(d.DayStatus) AS AbsentCount,ab.AbsentDays,ab.FirstAbsentDate
                                FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P
		                                WHERE p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) and p.WorkDate<='" + adate + @"'
	                                ) AS D
                                INNER JOIN (select * from EmployeeInformation) AS E ON e.SystemId=d.EmpSystemID 
                                LEFT OUTER JOIN org.Department AS DEP ON dep.Id=e.DepartmentId
                                LEFT OUTER JOIN hkp.Designation AS DE ON de.Id=e.DesignationSystemID
                                LEFT OUTER JOIN org.section sec ON sec.Id=e.SectionId
                                LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=e.SubSectionId

                                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM (select * from AttdnProcessData where WorkDate<= '" + adate + @"' and PlantID='" + PlantId + @"')  AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeStatus='Active' AND isnull(ei.EmployeeCurrentStatus,'')=''
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>=" + maxDays + @") AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE  e.EmployeeStatus='Active' AND isnull(e.EmployeeCurrentStatus,'')='' AND D.SEQ<=" + maxDays + @" AND D.DayStatus='A'  AND E.PlantId='" + PlantId + @"'
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,D.DayStatus,
                                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath,ab.FirstAbsentDate
                                HAVING COUNT(d.DayStatus)>=" + maxDays + @" ORDER BY AB.AbsentDays DESC";

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
        public void Get_tobe_LA_days_for_effectiveDate(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"WITH CTE
	                            AS (
		                            SELECT *
			                            ,MAX(CASE 
			                            when Category in ('Present','Late','Leave','Half Day') then 1							
					                            ELSE 0
					                            END) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS mx
				                            -- status of the latest date
			                            ,first_value(DayStatus) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS fv
		                            FROM AttdnProcessData  left join DayType on AttdnProcessData.DayStatus=DayType.daytype 
		                            )
	                            SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS absentDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
	                            FROM cte

	                            LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
	                            WHERE fv = 'A' -- current status = 'A'
		                            AND mx = 0 -- all rows before the 1st 'P'
			                            --and DayStatus not in (select distinct DayType from DayType where Category in ('Holiday','Weekend'))
			                            --and OutTime is null
			                            AND (EI.EmployeeStatus = 'Active') AND CONVERT(DATE,CTE.WorkDate) <= '" + adate + @"'  
			                            and ei.EmployeeCurrentStatus is null
	                            GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode
	                            HAVING
		                            -- at least three days absent
		                            COUNT(*) >= " + maxDays + @"
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
        public void xGet_tobe_TBS(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"WITH CTE
	                            AS (
		                            SELECT *
			                            ,MAX(CASE 
			                            when Category in ('Present','Late','Leave','Half Day') then 1							
					                            ELSE 0
					                            END) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS mx
				                            -- status of the latest date
			                            ,first_value(DayStatus) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS fv
		                            FROM AttdnProcessData  left join DayType on AttdnProcessData.DayStatus=DayType.daytype 
		                            )
	                            SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS absentDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
	                            FROM cte

	                            LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
	                            WHERE fv = 'A' -- current status = 'A'
		                            AND mx = 0 -- all rows before the 1st 'P'
			                            and DayStatus not in (select distinct DayType from DayType where Category in ('Holiday','Weekend'))
			                            --and OutTime is null
			                            AND (EI.EmployeeStatus = 'Active' ) AND CONVERT(DATE,CTE.WorkDate) <= '" + adate + @"'  
			                            and (ei.EmployeeCurrentStatus='LONG ABSENTEEISM' or ei.EmployeeCurrentStatus is null)
	                            GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode
	                            HAVING
		                            -- at least three days absent
		                            COUNT(*) >= " + maxDays + @"
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
        public void Get_tobe_TBS(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                                DEP.UserName AS Department,de.UserName AS designation,
                                sec.UserName AS Section,ss.UserName AS SubSection,
                                D.DayStatus,COUNT(d.DayStatus) AS AbsentCount,ab.AbsentDays,ab.FirstAbsentDate
                                FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P
		                                WHERE p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) and p.WorkDate<='" + adate + @"'
	                                ) AS D
                                INNER JOIN (select * from EmployeeInformation) AS E ON e.SystemId=d.EmpSystemID 
                                LEFT OUTER JOIN org.Department AS DEP ON dep.Id=e.DepartmentId
                                LEFT OUTER JOIN hkp.Designation AS DE ON de.Id=e.DesignationSystemID
                                LEFT OUTER JOIN org.section sec ON sec.Id=e.SectionId
                                LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=e.SubSectionId

                                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM (select * from AttdnProcessData where WorkDate<= '" + adate + @"' and PlantID='" + PlantId + @"')  AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeStatus='Active' AND (isnull(ei.EmployeeCurrentStatus,'')='' or isnull(ei.EmployeeCurrentStatus,'')='LONG ABSENTEEISM') 
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>=" + maxDays + @") AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE  e.EmployeeStatus='Active' AND (isnull(e.EmployeeCurrentStatus,'')='' or isnull(e.EmployeeCurrentStatus,'')='LONG ABSENTEEISM') AND D.SEQ<=" + maxDays + @" AND D.DayStatus='A'  AND E.PlantId='" + PlantId + @"'
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,D.DayStatus,
                                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath,ab.FirstAbsentDate
                                HAVING COUNT(d.DayStatus)>=" + maxDays + @" ORDER BY AB.AbsentDays DESC";

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
        public void Get_tobe_TBS_days_for_effectiveDate(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"WITH CTE
	                            AS (
		                            SELECT *
			                            ,MAX(CASE 
			                            when Category in ('Present','Late','Leave','Half Day') then 1							
					                            ELSE 0
					                            END) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS mx
				                            -- status of the latest date
			                            ,first_value(DayStatus) OVER (
				                            PARTITION BY EmpSystemID ORDER BY WorkDate DESC
				                            ) AS fv
		                            FROM AttdnProcessData  left join DayType on AttdnProcessData.DayStatus=DayType.daytype 
		                            )
	                            SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS absentDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
	                            FROM cte

	                            LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
	                            WHERE fv = 'A' -- current status = 'A'
		                            AND mx = 0 -- all rows before the 1st 'P'
			                            --and DayStatus not in (select distinct DayType from DayType where Category in ('Holiday','Weekend'))
			                            --and OutTime is null
			                            AND (EI.EmployeeStatus = 'Active') AND CONVERT(DATE,CTE.WorkDate) <= '" + adate + @"'  
			                            and (ei.EmployeeCurrentStatus='LONG ABSENTEEISM' or ei.EmployeeCurrentStatus is null)
	                            GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode
	                            HAVING
		                            -- at least three days absent
		                            COUNT(*) >= " + maxDays + @"
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
        public void xLockValidation_Plant_FDTD(string _plantid, string _fromDate, string _toDate)
        {
            DataSet dsAttLock = null;
            DataSet dsHRsetting = null;
            try
            {
                GetHRSettingForLock(_plantid, out dsHRsetting);
                if (dsHRsetting.Tables[0].Rows.Count > 0)
                {
                    GetAttendanceLockInfo(_plantid, _fromDate, _toDate, out dsAttLock);
                    //DataView dvAL = new DataView(dsAttLock.Tables[0]);
                    //dvAL.RowFilter = "LockedDate  between '" + _fromDate + "' and '" + _toDate + "'";
                    if (dsAttLock.Tables[0].Rows.Count > 0)
                    {
                        string _ld = string.Empty;
                        for (int i = 0; i < dsAttLock.Tables[0].Rows.Count; i++)
                        {
                            string emp = dsAttLock.Tables[0].Rows[i]["EmployeeCode"].ToString();
                            string dates = dsAttLock.Tables[0].Rows[i]["LockedDate"].ToString();
                            if (_ld.Length == 0)//EmployeeCode
                            {
                                _ld = "[" + dates + "] for (" + emp + ")";
                            }
                            else
                            {
                                _ld += ", [" + dates + "] for (" + emp + ")";
                            }
                        }
                        throw new Exception("Attendance has already been locked on " + _ld + "");
                    }
                }//hr setting
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void xLockValidation(string _plantid, string _fromDate, string _toDate)
        {
            DataSet dsAttLock = null;
            DataSet dsHRsetting = null;
            try
            {
                GetHRSettingForLock(_plantid, out dsHRsetting);
                if (dsHRsetting.Tables[0].Rows.Count > 0)
                {
                    GetAttendanceLockInfo("", _plantid, _fromDate, _toDate, out dsAttLock);
                    //DataView dvAL = new DataView(dsAttLock.Tables[0]);
                    //dvAL.RowFilter = "LockedDate  between '" + _fromDate + "' and '" + _toDate + "'";
                    if (dsAttLock.Tables[0].Rows.Count > 0)
                    {
                        string _ld = string.Empty;
                        for (int i = 0; i < dsAttLock.Tables[0].Rows.Count; i++)
                        {
                            string emp = dsAttLock.Tables[0].Rows[i]["EmployeeCode"].ToString();
                            string dates = dsAttLock.Tables[0].Rows[i]["LockedDate"].ToString();
                            if (_ld.Length == 0)//EmployeeCode
                            {
                                _ld = "[" + dates + "] for (" + emp + ")";
                            }
                            else
                            {
                                _ld += ", [" + dates + "] for (" + emp + ")";
                            }
                        }
                        throw new Exception("Attendance has already been locked on " + _ld + "");
                    }
                }//hr setting
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
                    //DataView dvAL = new DataView(dsAttLock.Tables[0]);
                    //dvAL.RowFilter = "LockedDate  between '" + _fromDate + "' and '" + _toDate + "'";
                    if (dsAttLock.Tables[0].Rows.Count > 0)
                    {
                        //string _ld = string.Empty;
                        //for (int i = 0; i < dsAttLock.Tables[0].Rows.Count; i++)
                        //{
                        //    string emp = dsAttLock.Tables[0].Rows[i]["EmployeeCode"].ToString();
                        //    string dates = dsAttLock.Tables[0].Rows[i]["LockedDate"].ToString();
                        //    if (_ld.Length == 0)//EmployeeCode
                        //    {
                        //        _ld = "[" + dates + "] for (" + emp + ")";
                        //    }
                        //    else
                        //    {
                        //        _ld += ", [" + dates + "] for (" + emp + ")";
                        //    }
                        //}
                        IsLocked = true;
                        // throw new Exception("Attendance has already been locked on " + _ld + "");
                    }
                }//hr setting
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetRemoteData(string PlantId, string pDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select  * from AttdnRawDataFromApp 
                                    where PlantId='" + PlantId + "' and pDate='" + pDate + @"'
                                    and  (isnull(isApprovedIN,0)=0 or isnull(isApprovedOUT,0)=0)
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
        public string xGetWC_Daylock(string PlantId, string pDate)
        {
            string strSql = string.Empty;
            try
            {
                strSql = @" 
									(
	                                select w.empsystemid from AttdnProcessData w
									left join PlantWiseHRMSSetting p on p.PlantID=w.PlantId
									where w.PlantId='" + PlantId + @"' and WorkDate='" + pDate + @"' and p.IsAttendanceLockApplicable=1
                                    and IsLock=1
									)";

                return strSql;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
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
        public void GetManualData(string PlantId, string pDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select  * from AttdnManualData where PlantId='" + PlantId + "' and WorkDate='" + pDate + "'";

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
        public ReturnType SaveTotal_SNA(string _plantid, string sAttnDatex, bool _bReProc)
        {
            int _maxRow = 500;
            DataSet dsEmployeeList = null;
            ReturnType _result = new ReturnType();
            string strYrSystemID, strYrFromDate, strYrToDate, GroupSysID = string.Empty;

            try
            {
                _result.Status = false;
                _result.Message = "Not Processed";
                string _emplist = "''";
                if (Convert.ToDateTime(sAttnDatex.Trim()) <= Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                {
                    PlantNameAndHRMSLocation(_plantid, sAttnDatex, out strYrSystemID, out strYrFromDate, out strYrToDate, out GroupSysID);

                    AttdnProcBaseOn_SNA(GroupSysID, _plantid, sAttnDatex, out dsEmployeeList);
                    string _emps = "''";
                    int _Count = 0;
                    for (int i = 0; i < dsEmployeeList.Tables[0].Rows.Count; i++)
                    {
                        _Count++;
                        if (_emps == "''")
                        {
                            _emps = "'" + dsEmployeeList.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                        }
                        else
                        {
                            _emps = _emps.Trim() + ", '" + dsEmployeeList.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                        }

                        ///for each 1000 emp the attn-process will run
                        if (_Count >= _maxRow)
                        {
                            _emplist = _emps;
                            CoreProcess_SNA(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                            _emps = "''";
                            _Count = 0;
                        }
                    }//for emp

                    //last portion
                    if (_Count < _maxRow)
                    {
                        _emplist = _emps;
                        CoreProcess_SNA(_plantid, sAttnDatex, _emplist, _bReProc, GroupSysID, strYrSystemID, strYrFromDate, strYrToDate);
                        _emps = "''";
                        _Count = 0;
                    }
                }//if less than curr date
                return _result;
            }
            catch (Exception ex)
            {
                throw new Exception(_result.Message + " because " + ex.Message);
            }
        }//End Function  

        void CoreProcess(string _plantid, string sAttnDatex, string _emplist, bool _bReProc, string GroupSysID, string strYrSystemID, string strYrFromDate, string strYrToDate)
        {
            ReturnType _result = new ReturnType();
            try
            {
                _result.Status = false;
                ShiftProcess sp = new ShiftProcess();
                _result.Message = "Not Processed";
                ShiftProcess(_plantid, sAttnDatex, GroupSysID, _emplist);
                sp.ShiftProcessStart(_plantid, sAttnDatex, GroupSysID, _emplist);//_emplist

                if (_emplist.Length > 0)
                {
                    sEmpSystemIDColl = _emplist;
                }
                bool _isin = InDataProcess(GroupSysID, _plantid, sAttnDatex, strYrSystemID, radDwLdEnrollID, strYrFromDate, strYrToDate, _bReProc);
                bool _isout = OutDataProcess(_plantid, sAttnDatex, GroupSysID, sEmpSystemIDColl, sMinOT, sFractionCalculate, radDwLdEnrollID);
                var _issum = SummaryDataProcess(GroupSysID, _plantid, sAttnDatex, sEmpSystemIDColl);
                _result.Status = true;
                _result.Message = "Data processed";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CoreProcess_SNA(string _plantid, string sAttnDatex, string _emplist, bool _bReProc, string GroupSysID, string strYrSystemID, string strYrFromDate, string strYrToDate)
        {
            ReturnType _result = new ReturnType();
            try
            {
                _result.Status = false;
                _result.Message = "Not Processed";
                // ShiftProcess(_plantid, sAttnDatex, GroupSysID, _emplist);
                //if (_emplist.Length > 0)
                //{
                sEmpSystemIDColl = _emplist;
                //}
                bool _isin = InProcess_SNA(GroupSysID, _plantid, sAttnDatex, strYrSystemID, radDwLdEnrollID, strYrFromDate, strYrToDate, _bReProc);
                if (_isin)
                {
                    _result.Message = "'IN' Data processed";
                    bool _isout = OutProcess_SNA(_plantid, sAttnDatex, GroupSysID, sEmpSystemIDColl, sMinOT, sFractionCalculate, radDwLdEnrollID);
                    if (_isout)
                    {
                        _result.Message += Environment.NewLine + "'OUT' Data processed";
                        var _issum = SummaryProcess_SNA(GroupSysID, sAttnDatex, sEmpSystemIDColl);
                        if (_issum)
                        {
                            _result.Message += Environment.NewLine + "'Summary' Data processed";
                            _result.Status = true;
                        }
                        else
                        {
                            _result.Message += Environment.NewLine + "'Summary' Data Not processed";
                        }
                    }
                    else
                    {
                        _result.Message += Environment.NewLine + "'OUT' Data Not processed";
                    }
                }
                else
                {
                    _result.Message = "Data Not processed";
                }
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

        private void GetHRsetting(string _plantid)
        {
            DataSet dsLocal = null;
            try
            {
                _HROTSetting = new global::HROTSetting();
                GetHRSettingPlantWise(_plantid, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    _HROTSetting.MinimumOTMinute = Convert.ToInt32(dsLocal.Tables[0].Rows[0]["MinimumOTMinute"].ToString().Trim());
                    _HROTSetting.OTFractionCalculation = (dsLocal.Tables[0].Rows[0]["OTFractionCalculation"].ToString().Trim());
                    _HROTSetting.OTConsiderOn = (dsLocal.Tables[0].Rows[0]["OTConsiderOn"].ToString().Trim());
                    _HROTSetting.OTBaseOnOuttime = (dsLocal.Tables[0].Rows[0]["OTBaseOnOuttime"].ToString().Trim());

                    _HROTSetting.IsRoundOptionApplicable = GetBoolData(dsLocal.Tables[0].Rows[0]["IsRoundOptionApplicable"].ToString().Trim());
                    _HROTSetting.RoundFigureForOT = Convert.ToInt32(dsLocal.Tables[0].Rows[0]["RoundFigureForOT"].ToString().Trim());

                    _HROTSetting.IsPunchBasedOT = GetBoolData(dsLocal.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());//
                    _HROTSetting.IsRemoteAttendanceApprovalRequired = GetBoolData(dsLocal.Tables[0].Rows[0]["IsRemoteAttendanceApprovalRequired"].ToString().Trim());//IsRemoteAttendanceApprovalRequired

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function

        private void ShiftProcess(string _plantid, string sAttnDate, string GroupSysID, string _emplist)
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

            DataSet dsEmpSftAssBfrEDt = null;
            DataTable dtEmpSftAssBfrEDt = null;
            DataView dvEmpSftAssBfrEDt = null;

            DataSet dsEmpSftAss = null;
            DataTable dtEmpSftAss = null;
            DataView dvEmpSftAss = null;

            DataSet dsSftRstCdl = null;
            DataTable dtSftRstCdl = null;
            DataView dvSftRstCdl = null;

            DataSet dsSftDft = null;
            DataSet dsIdLast = null;
            DataTable dtIdLast = null;
            DataView dvIdLast = null;

            DataSet dsAttdnProc = null;
            DataTable dtAttdnProc = null;
            DataView dvAttdnProc = null;
            DataRow drAttdnProc = null;

            #endregion DataSet Declare
            bool RunRoster = false;
            try
            {
                if (_emplist.Length == 0)
                {
                    GetEmployeeInformationForShiftProcess(_plantid, "", sAttnDate.Trim(), out dsEmpInfoForShiftProc);
                }
                else
                {
                    GetEmployeeInformationForShiftProcess(_plantid, _emplist, sAttnDate.Trim(), out dsEmpInfoForShiftProc);
                }
                if (dsEmpInfoForShiftProc.Tables[0].Rows.Count > 0)
                {
                    string sEmpSysIDCollForSft = "";
                    if (_emplist.Length == 0)
                    {
                        for (int i = 0; i < dsEmpInfoForShiftProc.Tables[0].Rows.Count; i++)
                        {
                            if (sEmpSysIDCollForSft.Trim() == "")
                            {
                                sEmpSysIDCollForSft = "'" + dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                sEmpSysIDCollForSft = sEmpSysIDCollForSft.Trim() + ", '" + dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                        }

                    }//_emplist
                    else
                    {
                        sEmpSysIDCollForSft = _emplist;
                    }
                    #region DataSet

                    string dtLastDt = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
                    GetDayType(out dsDayType);
                    dtDayType = dsDayType.Tables[0];
                    dvDayType = new DataView();

                    List<dicShiftDft> dicShiftDft = new List<global::dicShiftDft>();
                    GetShiftDefination(GroupSysID, _plantid, out dsSftDft);
                    if (dsSftDft.Tables[0].Rows.Count > 0)
                        dicShiftDft = dsSftDft.Tables[0].ToList<dicShiftDft>();

                    GetEmpDateWiseShiftAssignWithDateRange(sEmpSysIDCollForSft.Trim(), dtLastDt, sAttnDate.Trim(), out dsEmpDtWiseSftAss);
                    dtEmpDtWiseSftAss = dsEmpDtWiseSftAss.Tables[0];
                    dvEmpDtWiseSftAss = new DataView();

                    GetEmployeeWeekOffByDay(sAttnDate, sEmpSysIDCollForSft.Trim(), out dsEmpWkOff);
                    dtEmpWkOff = dsEmpWkOff.Tables[0];
                    dvEmpWkOff = new DataView();

                    GetCompanyAssignWeekOffDateRangeWise(GroupSysID, _plantid, sAttnDate.Trim(), out dsComAssWkOff);
                    dtComAssWkOff = dsComAssWkOff.Tables[0];
                    dvComAssWkOff = new DataView();

                    GetUpdatedEmpShiftAssignBeforeFromDate(sEmpSysIDCollForSft.Trim(), sAttnDate.Trim(), out dsEmpSftAssBfrFmDt);
                    dtEmpSftAssBfrFmDt = dsEmpSftAssBfrFmDt.Tables[0];
                    dvEmpSftAssBfrFmDt = new DataView();

                    GetSftRstDayCount(sEmpSysIDCollForSft.Trim(), dtLastDt.Trim(), sAttnDate.Trim(), out dsSftRstDayCnt);
                    dtSftRstDayCnt = dsSftRstDayCnt.Tables[0];
                    dvSftRstDayCnt = new DataView();

                    GetEmployeeShiftAssignBeforeFromDate(sEmpSysIDCollForSft.Trim(), sAttnDate, out dsEmpSftAssBfrEDt);
                    dtEmpSftAssBfrEDt = dsEmpSftAssBfrEDt.Tables[0];
                    dvEmpSftAssBfrEDt = new DataView();

                    GetEmployeeShiftAssignInDateRange(sEmpSysIDCollForSft.Trim(), dtLastDt, sAttnDate.Trim(), out dsEmpSftAss);
                    dtEmpSftAss = dsEmpSftAss.Tables[0];
                    dvEmpSftAss = new DataView();

                    GetShiftRosterChild(GroupSysID.Trim(), out dsSftRstCdl);
                    dtSftRstCdl = dsSftRstCdl.Tables[0];
                    dvSftRstCdl = new DataView();

                    GetAttdnProcessData(sEmpSysIDCollForSft.Trim(), sAttnDate.Trim(), out dsAttdnProc);
                    dtAttdnProc = dsAttdnProc.Tables[0];
                    dvAttdnProc = new DataView();

                    #endregion DataSet

                    for (int i = 0; i < dsEmpInfoForShiftProc.Tables[0].Rows.Count; i++)
                    {
                        #region Declare Variable

                        string sEmpSystemID = dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                        string sPlantID = dsEmpInfoForShiftProc.Tables[0].Rows[i]["PlantID"].ToString().Trim();

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

                        DateTime dtStDt = Convert.ToDateTime(sAttnDate);
                        DateTime dtFrmD = Convert.ToDateTime(sAttnDate);
                        DateTime dtToD = Convert.ToDateTime(sAttnDate);

                        #endregion Declare Variable

                        while (dtStDt <= dtToD)
                        {//check in the table 'EmpDateWiseShiftAssign', EmpSystemID and WorkDate are already available
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

                            #endregion Initialize

                            dvEmpDtWiseSftAss.Table = dtEmpDtWiseSftAss;
                            dvEmpDtWiseSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + strStDt + "'";
                            if (dvEmpDtWiseSftAss.Count > 0)
                            {
                                #region EmpSystemID and WorkDate are already available in the table 'EmpDateWiseShiftAssign'
                                if (dvEmpDtWiseSftAss[0]["ToReprocess"].ToString().Trim().ToUpper() == "YES")
                                {
                                    //Check in the table 'EmpDateWiseShiftAssign' the field 'AttdnLock' is not true
                                    if (Convert.ToBoolean(dvEmpDtWiseSftAss[0]["AttdnLock"].ToString().Trim()) == false)
                                    {
                                        dvEmpSftAss.Table = dtEmpSftAss;
                                        dvEmpSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";

                                        if (dtStDt == dtFrmD || dvEmpSftAss.Count == 0)
                                        {

                                            #region  FromDate & Shift start Date Same and After fromdate to todate not found shift assignment

                                            #region Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                            dvEmpSftAssBfrFmDt.Table = dtEmpSftAssBfrFmDt;
                                            dvEmpSftAssBfrFmDt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                                            if (dvEmpSftAssBfrFmDt.Count > 0)
                                            {
                                                if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsFix"].ToString().Trim()) == true)
                                                {
                                                    #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                                    dvEmpWkOff.Table = dtEmpWkOff;
                                                    //dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim() + "'";
                                                    dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' ";
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

                                                    if (bAlignWithCC == true)
                                                    {
                                                        dvComAssWkOff.Table = dtComAssWkOff;
                                                        dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                        if (dvComAssWkOff.Count > 0)
                                                        {
                                                            sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                            if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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

                                                    if (bIndividualWeekOff == true)
                                                    {
                                                        if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                        {
                                                            sDayLengthType = sFstDayLengthType;
                                                            if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                            if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                    var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim());
                                                    if (dicShiftDft_Sub != null)
                                                    {
                                                        sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                                    }
                                                    #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                    if (sSfTime.Trim().Length > 0)
                                                    {
                                                        drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                        drEmpDtWiseSftAss.BeginEdit();

                                                        drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim();
                                                        drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim();
                                                        drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                        drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                        drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                        drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();
                                                        drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                        drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                        drEmpDtWiseSftAss.EndEdit();
                                                    }
                                                    #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                }
                                                //else if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsRoster"].ToString().Trim()) == true)
                                                else if (RunRoster)
                                                {
                                                    #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                                    //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                                    RosterMstSysID = dvEmpSftAssBfrFmDt[0]["RosterSystemID"].ToString().Trim();

                                                    dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                    dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtStDt.ToString("dd-MMM-yyyy") + "'";
                                                    if (dvSftRstDayCnt.Count > 0)
                                                    {
                                                        bToDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                                        RosterChlTempSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                        RosterChlOldTempSftSysID = dvSftRstDayCnt[0]["RosterShiftSystemId"].ToString().Trim();
                                                    }
                                                    else
                                                    {
                                                        bToDayTempShift = false;
                                                    }

                                                    //Take Last date 'ShiftSystemID' and 'RosterShiftDayCount' from the table 'EmpDateWiseShiftAssign'
                                                    dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");
                                                    dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                    dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtLastDt + "'";
                                                    if (dvSftRstDayCnt.Count > 0)
                                                    {
                                                        RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftDayCount"].ToString().Trim());
                                                        RosterShiftWeekOffCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftWeekOffCount"].ToString().Trim());
                                                        RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                        bPrvDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                                        RosterChlOldSftSysID = dvSftRstDayCnt[0]["RosterShiftSystemId"].ToString().Trim();
                                                        sEmpSftAssTempSysID = dvSftRstDayCnt[0]["EmpSftAssiSystemID"].ToString().Trim();
                                                        bInitialRstSftDyCnt = true;
                                                    }
                                                    else if (bInitialRstSftDyCnt == false)
                                                    {
                                                        RosterShiftDayCount = Convert.ToInt32(dvEmpSftAssBfrFmDt[0]["StartFromDay"].ToString().Trim()) - 1;
                                                        RosterShiftWeekOffCount = Convert.ToInt32(dvEmpSftAssBfrFmDt[0]["StartFromDay"].ToString().Trim()) - 1;
                                                        RosterChlSftSysID = dvEmpSftAssBfrFmDt[0]["RosterStartShiftID"].ToString().Trim();
                                                        bInitialRstSftDyCnt = true;
                                                    }

                                                    if (dvEmpSftAss.Count > 0)
                                                    {
                                                        sEmpSftAssCurntSysID = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                                        if (sEmpSftAssCurntSysID != sEmpSftAssTempSysID)
                                                        {
                                                            bRstSftChange = true;
                                                        }

                                                        if (bRstSftChange)
                                                        {
                                                            RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[0]["StartFromDay"].ToString().Trim()) - 1;
                                                            RosterChlSftSysID = dvEmpSftAss[0]["RosterStartShiftID"].ToString().Trim();
                                                            bRstSftChange = false;
                                                        }
                                                    }
                                                    if (bToDayTempShift == false && bPrvDayTempShift == true)
                                                    {
                                                        RosterChlSftSysID = RosterChlOldSftSysID;
                                                    }
                                                    else if (bToDayTempShift == true && bPrvDayTempShift == false/* && string.IsNullOrEmpty(RosterChlOldTempSftSysID) == true*/)
                                                    {
                                                        RosterChlSftSysID = RosterChlTempSftSysID;
                                                    }

                                                    //Set Roster Child Shift SystemID For Current Date in loop
                                                    dvSftRstCdl.Table = dtSftRstCdl;
                                                    dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                                    if (dvSftRstCdl.Count > 0)
                                                    {
                                                        #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                        {//RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                            if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                            {
                                                                bInitialShift = false;
                                                                ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());

                                                                bIsDaysLengthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthShiftRoster"].ToString().Trim());
                                                                DaysLengthShiftRoster = Convert.ToInt32(dvSftRstCdl[SRC]["DaysLengthShiftRoster"].ToString().Trim());

                                                                bIsFixedDayInMonthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthShiftRoster"].ToString().Trim());
                                                                sFixedDayInMonthShiftRoster = dvSftRstCdl[SRC]["FixedDayInMonthShiftRoster"].ToString().Trim();

                                                                bAlignWithCC = Convert.ToBoolean(dvSftRstCdl[SRC]["IsAlignWithCC"].ToString().Trim());

                                                                bIsFixedDayInMonthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthWeekOff"].ToString().Trim());
                                                                sFixedDayInMonthWeekOff = dvSftRstCdl[SRC]["FixedDayInMonthWeekOff"].ToString().Trim();

                                                                bIsDaysLengthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthWeekOff"].ToString().Trim());
                                                                sWeekOffDay = dvSftRstCdl[SRC]["WeekOffDay"].ToString().Trim();

                                                                bIsWeekOffInShiftLenght = Convert.ToBoolean(dvSftRstCdl[SRC]["IsWeekOffInShiftLenght"].ToString().Trim());
                                                                WeekOffInShiftLenght = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffInShiftLenght"].ToString().Trim());
                                                            }
                                                        }

                                                        if (bInitialShift == true)
                                                        {
                                                            RosterChlSftSysID = dvEmpSftAssBfrFmDt[0]["RosterStartShiftID"].ToString().Trim();
                                                            RosterShiftDayCount = Convert.ToInt32(dvEmpSftAssBfrFmDt[0]["StartFromDay"].ToString().Trim()) - 1;
                                                            RosterShiftWeekOffCount = Convert.ToInt32(dvEmpSftAssBfrFmDt[0]["StartFromDay"].ToString().Trim()) - 1;

                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                            {//RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                                if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                                {
                                                                    ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());

                                                                    bIsDaysLengthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthShiftRoster"].ToString().Trim());
                                                                    DaysLengthShiftRoster = Convert.ToInt32(dvSftRstCdl[SRC]["DaysLengthShiftRoster"].ToString().Trim());

                                                                    bIsFixedDayInMonthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthShiftRoster"].ToString().Trim());
                                                                    sFixedDayInMonthShiftRoster = dvSftRstCdl[SRC]["FixedDayInMonthShiftRoster"].ToString().Trim();

                                                                    bAlignWithCC = Convert.ToBoolean(dvSftRstCdl[SRC]["IsAlignWithCC"].ToString().Trim());

                                                                    bIsFixedDayInMonthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthWeekOff"].ToString().Trim());
                                                                    sFixedDayInMonthWeekOff = dvSftRstCdl[SRC]["FixedDayInMonthWeekOff"].ToString().Trim();

                                                                    bIsDaysLengthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthWeekOff"].ToString().Trim());
                                                                    sWeekOffDay = dvSftRstCdl[SRC]["WeekOffDay"].ToString().Trim();

                                                                    bIsWeekOffInShiftLenght = Convert.ToBoolean(dvSftRstCdl[SRC]["IsWeekOffInShiftLenght"].ToString().Trim());
                                                                    WeekOffInShiftLenght = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffInShiftLenght"].ToString().Trim());
                                                                }
                                                            }
                                                        }

                                                        //Check RosterShiftDayCount & ShiftDays
                                                        #region Days Length For ShiftRoster
                                                        if (bIsDaysLengthShiftRoster == true)
                                                        {
                                                            if (RosterShiftDayCount >= DaysLengthShiftRoster)
                                                            {
                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                    if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                    {
                                                                        RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                                    }
                                                                }
                                                                if (RosterShiftSequence == 0)
                                                                {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                    RosterShiftSequence = 1;
                                                                }
                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                    if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
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
                                                        }
                                                        #endregion Days Length For ShiftRoster
                                                        #region Fixed Date In Month For Shift Roster Change
                                                        else if (bIsFixedDayInMonthShiftRoster == true)
                                                        {
                                                            if (bFstShiftDay == false)
                                                            {
                                                                dtIdList(sFixedDayInMonthShiftRoster.Trim(), out dsIdLast);
                                                                dtIdLast = dsIdLast.Tables[0];
                                                                dvIdLast = new DataView();
                                                                dvIdLast.Table = dtIdLast;
                                                                dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                                if (dvIdLast.Count > 0)
                                                                {
                                                                    //if()
                                                                    for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                    {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                        if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                        {
                                                                            RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                                        }
                                                                    }
                                                                    if (RosterShiftSequence == 0)
                                                                    {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                        RosterShiftSequence = 1;
                                                                    }
                                                                    for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                    {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                        if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                        {
                                                                            RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                            RosterChlSftSysID = RosterChlNewSftSysID;
                                                                            RosterShiftDayCount = 0;
                                                                            RosterShiftSequence = 0;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                                RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                            }
                                                        }
                                                        #endregion Fixed Date In Month For Shift Roster Change
                                                        #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                        if (bIsWeekOffInShiftLenght == true)
                                                        {
                                                            if (RosterShiftWeekOffCount > WeekOffInShiftLenght)
                                                            {
                                                                RosterShiftWeekOffCount = 1;
                                                            }
                                                            else
                                                            {
                                                                RosterShiftWeekOffCount++;
                                                            }
                                                        }
                                                    }

                                                    //Update RosterShiftDayCount 
                                                    RosterShiftDayCount = RosterShiftDayCount + 1;

                                                    #region Week off align with company calendar
                                                    if (bAlignWithCC == true)
                                                    {
                                                        dvComAssWkOff.Table = dtComAssWkOff;
                                                        dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                        if (dvComAssWkOff.Count > 0)
                                                        {
                                                            sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                            if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                    #endregion Week off align with company calendar
                                                    #region Fixed Date In Month For Week Off
                                                    else if (bIsFixedDayInMonthWeekOff == true)
                                                    {
                                                        dtIdList(sFixedDayInMonthWeekOff.Trim(), out dsIdLast);
                                                        dtIdLast = dsIdLast.Tables[0];
                                                        dvIdLast = new DataView();
                                                        dvIdLast.Table = dtIdLast;
                                                        dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                        if (dvIdLast.Count > 0)
                                                        {
                                                            sDayType = "W";
                                                            sDayLengthType = "Week Off";
                                                        }
                                                    }
                                                    #endregion Fixed Date In Month For Week Off
                                                    #region Assign week off day
                                                    else if (bIsDaysLengthWeekOff == true)
                                                    {
                                                        if (sWeekOffDay.Trim() == dtStDt.DayOfWeek.ToString().Trim())
                                                        {
                                                            sDayType = "W";
                                                            sDayLengthType = "Week Off";
                                                        }
                                                    }
                                                    #endregion Assign week off day
                                                    #region Week Off In Shift Lenght
                                                    else if (bIsWeekOffInShiftLenght == true)
                                                    {
                                                        //dtIdList(WeekOffInShiftLenght.Trim(), out dsIdLast);
                                                        //dtIdLast = dsIdLast.Tables[0];
                                                        //dvIdLast = new DataView();
                                                        //dvIdLast.Table = dtIdLast;
                                                        //dvIdLast.RowFilter = "Id = " + RosterShiftDayCount + "";
                                                        //if (dvIdLast.Count > 0)
                                                        if (RosterShiftWeekOffCount == WeekOffInShiftLenght)
                                                        {
                                                            sDayType = "W";
                                                            sDayLengthType = "Week Off";
                                                        }
                                                    }
                                                    #endregion Week Off In Shift Lenght

                                                    var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == RosterChlNewSftSysID.Trim());
                                                    if (dicShiftDft_Sub != null)
                                                    {
                                                        sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                                    }

                                                    if (sSfTime.Trim().Length > 0)
                                                    {
                                                        drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                        drEmpDtWiseSftAss.BeginEdit();

                                                        drEmpDtWiseSftAss["EmpSftAssiSystemID"] = (dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim());
                                                        drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();
                                                        drEmpDtWiseSftAss["RosterShiftDayCount"] = RosterShiftDayCount;
                                                        drEmpDtWiseSftAss["RosterShiftWeekOffCount"] = RosterShiftWeekOffCount;
                                                        drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                        drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                        drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                        drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();
                                                        drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                        drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                        drEmpDtWiseSftAss.EndEdit();
                                                    }
                                                    #endregion if Last updated shift in table 'EmployeeShiftAssign' is roster
                                                }
                                            }
                                            //else
                                            //{

                                            //}
                                            #endregion Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                            #endregion  FromDate & Shift start Date Same and After fromdate to todate not found shift assignment
                                            sEmpSftAssTempSysID = sEmpSftAssCurntSysID;
                                        }
                                        else if (dvEmpSftAss.Count > 0)
                                        {
                                            string strActuEffDt = "";
                                            string strActuEffDtTmp = "";

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
                                            sEmpSftAssCurntSysID = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                            if (sEmpSftAssCurntSysID != sEmpSftAssTempSysID)
                                            {
                                                bRstSftChange = true;
                                            }

                                            if (Convert.ToDateTime(Convert.ToDateTime(dvEmpSftAss[0]["EffectiveDate"].ToString().Trim()).ToString("dd-MMM-yyyy")) < Convert.ToDateTime(dtStDt.ToString("dd-MMM-yyyy")))
                                            {
                                                dvEmpDtWiseSftAss.Table = dtEmpDtWiseSftAss;
                                                dvEmpDtWiseSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate <= '" + dtStDt.ToString("dd-MMM-yyyy") + "'";
                                                if (dvEmpDtWiseSftAss.Count == 0)
                                                {
                                                    bShiftProc = false;
                                                }
                                            }

                                            if (bShiftProc == true)
                                            {
                                                #region Shift start Date is great than FromDate

                                                for (int efDt = 0; efDt < dvEmpSftAss.Count; efDt++)
                                                {
                                                    if (Convert.ToDateTime(dvEmpSftAss[efDt]["EffectiveDate"].ToString().Trim()) == Convert.ToDateTime(strActuEffDt))
                                                    {
                                                        #region Check Last updated shift in table 'EmployeeShiftAssign' after fromdate

                                                        if (Convert.ToBoolean(dvEmpSftAss[efDt]["IsFix"].ToString().Trim()) == true)
                                                        {
                                                            #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                                            dvEmpWkOff.Table = dtEmpWkOff;
                                                            //dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAss[0]["FixSystemID"].ToString().Trim() + "'";
                                                            dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' ";
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

                                                            if (bAlignWithCC == true)
                                                            {
                                                                dvComAssWkOff.Table = dtComAssWkOff;
                                                                dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                                if (dvComAssWkOff.Count > 0)
                                                                {
                                                                    sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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

                                                            if (bIndividualWeekOff == true)
                                                            {
                                                                if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                                {
                                                                    sDayLengthType = sFstDayLengthType;
                                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                            var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == dvEmpSftAss[0]["FixSystemID"].ToString().Trim());
                                                            if (dicShiftDft_Sub != null)
                                                            {
                                                                sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                                            }
                                                            #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                            if (sSfTime.Trim().Length > 0)
                                                            {
                                                                drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                                drEmpDtWiseSftAss.BeginEdit();

                                                                drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[efDt]["SystemID"].ToString().Trim();
                                                                drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAss[efDt]["FixSystemID"].ToString().Trim();
                                                                drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                                drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                                drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();
                                                                drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                drEmpDtWiseSftAss.EndEdit();
                                                            }
                                                            #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                        }
                                                        //else if (Convert.ToBoolean(dvEmpSftAss[efDt]["IsRoster"].ToString().Trim()) == true)
                                                        else if (RunRoster)
                                                        {
                                                            #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                                            //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                                            RosterMstSysID = dvEmpSftAss[efDt]["RosterSystemID"].ToString().Trim();
                                                            string strEmpSftAssiSystemID = dvEmpSftAss[efDt]["SystemID"].ToString().Trim();

                                                            dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                            dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtStDt.ToString("dd-MMM-yyyy") + "'";
                                                            if (dvSftRstDayCnt.Count > 0)
                                                            {
                                                                bToDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                                            }
                                                            else
                                                            {
                                                                bToDayTempShift = false;
                                                            }

                                                            //Take Last date 'ShiftSystemID' and 'RosterShiftDayCount' from the table 'EmpDateWiseShiftAssign'

                                                            dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");
                                                            dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                            dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EmpSftAssiSystemID = '" + strEmpSftAssiSystemID + "' AND WorkDate = '" + dtLastDt + "'";
                                                            if (dvSftRstDayCnt.Count > 0)
                                                            {
                                                                RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftDayCount"].ToString().Trim());
                                                                RosterShiftWeekOffCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftWeekOffCount"].ToString().Trim());
                                                                RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                                bPrvDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                                                RosterChlOldSftSysID = dvSftRstDayCnt[0]["RosterShiftSystemId"].ToString().Trim();
                                                                bInitialRstSftDyCnt = true;
                                                            }
                                                            else if (bInitialRstSftDyCnt == false)
                                                            {
                                                                RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[efDt]["StartFromDay"].ToString().Trim()) - 1;
                                                                RosterShiftWeekOffCount = Convert.ToInt32(dvEmpSftAss[efDt]["StartFromDay"].ToString().Trim()) - 1;
                                                                RosterChlSftSysID = dvEmpSftAss[efDt]["RosterStartShiftID"].ToString().Trim();
                                                                bInitialRstSftDyCnt = true;
                                                            }

                                                            if (bRstSftChange)
                                                            {
                                                                RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[efDt]["StartFromDay"].ToString().Trim()) - 1;
                                                                RosterChlSftSysID = dvEmpSftAss[efDt]["RosterStartShiftID"].ToString().Trim();
                                                                bRstSftChange = false;
                                                            }

                                                            if (bToDayTempShift == false && bPrvDayTempShift == true)
                                                            {
                                                                RosterChlSftSysID = RosterChlOldSftSysID;
                                                            }
                                                            else if (bToDayTempShift == true && bPrvDayTempShift == false/* && string.IsNullOrEmpty(RosterChlOldTempSftSysID) == true*/)
                                                            {
                                                                RosterChlSftSysID = RosterChlTempSftSysID;
                                                            }

                                                            //Set Roster Child Shift SystemID For Current Date in loop
                                                            dvSftRstCdl.Table = dtSftRstCdl;
                                                            dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                                            if (dvSftRstCdl.Count > 0)
                                                            {
                                                                #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                {//RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                                    if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                                    {
                                                                        ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());

                                                                        bIsDaysLengthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthShiftRoster"].ToString().Trim());
                                                                        DaysLengthShiftRoster = Convert.ToInt32(dvSftRstCdl[SRC]["DaysLengthShiftRoster"].ToString().Trim());

                                                                        bIsFixedDayInMonthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthShiftRoster"].ToString().Trim());
                                                                        sFixedDayInMonthShiftRoster = dvSftRstCdl[SRC]["FixedDayInMonthShiftRoster"].ToString().Trim();

                                                                        bAlignWithCC = Convert.ToBoolean(dvSftRstCdl[SRC]["IsAlignWithCC"].ToString().Trim());

                                                                        bIsFixedDayInMonthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthWeekOff"].ToString().Trim());
                                                                        sFixedDayInMonthWeekOff = dvSftRstCdl[SRC]["FixedDayInMonthWeekOff"].ToString().Trim();

                                                                        bIsDaysLengthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthWeekOff"].ToString().Trim());
                                                                        sWeekOffDay = dvSftRstCdl[SRC]["WeekOffDay"].ToString().Trim();

                                                                        bIsWeekOffInShiftLenght = Convert.ToBoolean(dvSftRstCdl[SRC]["IsWeekOffInShiftLenght"].ToString().Trim());
                                                                        WeekOffInShiftLenght = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffInShiftLenght"].ToString().Trim());
                                                                    }
                                                                }

                                                                //Check RosterShiftDayCount & ShiftDays
                                                                #region Days Length For ShiftRoster
                                                                if (bIsDaysLengthShiftRoster == true)
                                                                {
                                                                    if (RosterShiftDayCount >= DaysLengthShiftRoster)
                                                                    {
                                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                        {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                            if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                            {
                                                                                RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                                            }
                                                                        }
                                                                        if (RosterShiftSequence == 0)
                                                                        {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                            RosterShiftSequence = 1;
                                                                        }
                                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                        {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                            if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
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
                                                                }
                                                                #endregion Days Length For ShiftRoster
                                                                #region Fixed Date In Month For Shift Roster Change
                                                                else if (bIsFixedDayInMonthShiftRoster == true)
                                                                {
                                                                    if (bFstShiftDay == false)
                                                                    {
                                                                        dtIdList(sFixedDayInMonthShiftRoster.Trim(), out dsIdLast);
                                                                        dtIdLast = dsIdLast.Tables[0];
                                                                        dvIdLast = new DataView();
                                                                        dvIdLast.Table = dtIdLast;
                                                                        dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                                        if (dvIdLast.Count > 0)
                                                                        {
                                                                            //if()
                                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                            {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                                if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                                {
                                                                                    RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                                                }
                                                                            }
                                                                            if (RosterShiftSequence == 0)
                                                                            {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                                RosterShiftSequence = 1;
                                                                            }
                                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                            {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                                if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                                {
                                                                                    RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                                    RosterChlSftSysID = RosterChlNewSftSysID;
                                                                                    RosterShiftDayCount = 0;
                                                                                    RosterShiftSequence = 0;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                                        RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                                    }
                                                                }
                                                                #endregion Fixed Date In Month For Shift Roster Change
                                                                #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                                if (bIsWeekOffInShiftLenght == true)
                                                                {
                                                                    if (RosterShiftWeekOffCount > WeekOffInShiftLenght)
                                                                    {
                                                                        RosterShiftWeekOffCount = 1;
                                                                    }
                                                                    else
                                                                    {
                                                                        RosterShiftWeekOffCount++;
                                                                    }
                                                                }
                                                            }

                                                            //Update RosterShiftDayCount 
                                                            RosterShiftDayCount = RosterShiftDayCount + 1;

                                                            #region Week off align with company calendar
                                                            if (bAlignWithCC == true)
                                                            {
                                                                dvComAssWkOff.Table = dtComAssWkOff;
                                                                dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                                if (dvComAssWkOff.Count > 0)
                                                                {
                                                                    sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                            #endregion Week off align with company calendar
                                                            #region Fixed Date In Month For Week Off
                                                            else if (bIsFixedDayInMonthWeekOff == true)
                                                            {
                                                                dtIdList(sFixedDayInMonthWeekOff.Trim(), out dsIdLast);
                                                                dtIdLast = dsIdLast.Tables[0];
                                                                dvIdLast = new DataView();
                                                                dvIdLast.Table = dtIdLast;
                                                                dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                                if (dvIdLast.Count > 0)
                                                                {
                                                                    sDayType = "W";
                                                                    sDayLengthType = "Week Off";
                                                                }
                                                            }
                                                            #endregion Fixed Date In Month For Week Off
                                                            #region Assign week off day
                                                            else if (bIsDaysLengthWeekOff == true)
                                                            {
                                                                if (sWeekOffDay.Trim() == dtStDt.DayOfWeek.ToString().Trim())
                                                                {
                                                                    sDayType = "W";
                                                                    sDayLengthType = "Week Off";
                                                                }
                                                            }
                                                            #endregion Assign week off day
                                                            #region Week Off In Shift Lenght
                                                            else if (bIsWeekOffInShiftLenght == true)
                                                            {
                                                                //dtIdList(WeekOffInShiftLenght.Trim(), out dsIdLast);
                                                                //dtIdLast = dsIdLast.Tables[0];
                                                                //dvIdLast = new DataView();
                                                                //dvIdLast.Table = dtIdLast;
                                                                //dvIdLast.RowFilter = "Id = " + RosterShiftDayCount + "";
                                                                //if (dvIdLast.Count > 0)
                                                                if (RosterShiftWeekOffCount == WeekOffInShiftLenght)
                                                                {
                                                                    sDayType = "W";
                                                                    sDayLengthType = "Week Off";
                                                                }
                                                            }
                                                            #endregion Week Off In Shift Lenght

                                                            var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == RosterChlNewSftSysID.Trim());
                                                            if (dicShiftDft_Sub != null)
                                                            {
                                                                sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                                            }

                                                            if (sSfTime.Trim().Length > 0)
                                                            {
                                                                drEmpDtWiseSftAss = dvEmpDtWiseSftAss[0].Row;
                                                                drEmpDtWiseSftAss.BeginEdit();

                                                                drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[efDt]["SystemID"].ToString().Trim();
                                                                drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();
                                                                drEmpDtWiseSftAss["RosterShiftDayCount"] = RosterShiftDayCount;
                                                                drEmpDtWiseSftAss["RosterShiftWeekOffCount"] = RosterShiftWeekOffCount;
                                                                drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                                drEmpDtWiseSftAss["DayType"] = sDayType.Trim();
                                                                drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                                drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();
                                                                drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                                drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                                drEmpDtWiseSftAss.EndEdit();
                                                            }
                                                            #endregion if Last updated shift in table 'EmployeeShiftAssign' is roster
                                                        }

                                                        #endregion Check Last updated shift in table 'EmployeeShiftAssign' after fromdate
                                                    }
                                                }
                                                #endregion Shift start Date is great than FromDate
                                            }
                                            sEmpSftAssTempSysID = sEmpSftAssCurntSysID;
                                        }
                                    }
                                }
                                #endregion EmpSystemID and WorkDate are already available in the table 'EmpDateWiseShiftAssign'
                            }
                            else
                            {
                                #region EmpSystemID and WorkDate not found in the table 'EmpDateWiseShiftAssign'

                                dvEmpSftAss.Table = dtEmpSftAss;
                                dvEmpSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                                if (dtStDt == dtFrmD || dvEmpSftAss.Count == 0)
                                {
                                    dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");

                                    dvEmpSftAssBfrEDt.Table = dtEmpSftAssBfrEDt;
                                    dvEmpSftAssBfrEDt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                                    if (dvEmpSftAssBfrEDt.Count > 0)
                                    {
                                        if (Convert.ToDateTime(Convert.ToDateTime(dvEmpSftAssBfrEDt[0]["EffectiveDate"].ToString().Trim()).ToString("dd-MMM-yyyy")) < Convert.ToDateTime(dtStDt.ToString("dd-MMM-yyyy")))
                                        {
                                            dvEmpDtWiseSftAss.Table = dtEmpDtWiseSftAss;
                                            dvEmpDtWiseSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate <= '" + dtStDt.ToString("dd-MMM-yyyy") + "'";
                                            if (dvEmpDtWiseSftAss.Count == 0)
                                            {
                                                bShiftProc = false;
                                            }
                                        }
                                    }
                                    //bShiftProc = false;

                                    #region FromDate & Shift start Date Same and After fromdate to todate not found shift assignment

                                    if (bShiftProc == true)
                                    {
                                        #region Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                        dvEmpSftAssBfrFmDt.Table = dtEmpSftAssBfrFmDt;
                                        dvEmpSftAssBfrFmDt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND EffectiveDate <= '" + strStDt + "'";
                                        if (dvEmpSftAssBfrFmDt.Count > 0)
                                        {
                                            if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsFix"].ToString().Trim()) == true)
                                            {
                                                #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                                dvEmpWkOff.Table = dtEmpWkOff;
                                                //dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim() + "'";
                                                dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' ";
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

                                                if (bAlignWithCC == true)
                                                {
                                                    dvComAssWkOff.Table = dtComAssWkOff;
                                                    dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                    if (dvComAssWkOff.Count > 0)
                                                    {
                                                        sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                        if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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

                                                if (bIndividualWeekOff == true)
                                                {
                                                    if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                    {
                                                        sDayLengthType = sFstDayLengthType;
                                                        if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                        if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim());
                                                if (dicShiftDft_Sub != null)
                                                {
                                                    sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                                }
                                                #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                                if (sSfTime.Trim().Length > 0)
                                                {
                                                    drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                    drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                    drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                    drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim();
                                                    drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAssBfrFmDt[0]["FixSystemID"].ToString().Trim();
                                                    drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                    drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                    drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                    drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                    drEmpDtWiseSftAss["RosterShiftDayCount"] = 0;
                                                    drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                    drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                    drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                    drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                    drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                    drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                    dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);
                                                }
                                                #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                            }
                                            //else if (Convert.ToBoolean(dvEmpSftAssBfrFmDt[0]["IsRoster"].ToString().Trim()) == true)
                                            else if (RunRoster)
                                            {
                                                #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                                //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                                RosterMstSysID = dvEmpSftAssBfrFmDt[0]["RosterSystemID"].ToString().Trim();

                                                dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtStDt.ToString("dd-MMM-yyyy") + "'";
                                                if (dvSftRstDayCnt.Count > 0)
                                                {
                                                    bToDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                                }
                                                else
                                                {
                                                    bToDayTempShift = false;
                                                }

                                                ////Take Last date 'ShiftSystemID' and 'RosterShiftDayCount' from the table 'EmpDateWiseShiftAssign'
                                                //string dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");
                                                dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                                dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtLastDt + "'";
                                                if (dvSftRstDayCnt.Count > 0)
                                                {
                                                    RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftDayCount"].ToString().Trim());
                                                    RosterShiftWeekOffCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftWeekOffCount"].ToString().Trim());
                                                    RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                    bPrvDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                                    RosterChlOldSftSysID = dvSftRstDayCnt[0]["RosterShiftSystemId"].ToString().Trim();
                                                    sEmpSftAssTempSysID = dvSftRstDayCnt[0]["EmpSftAssiSystemID"].ToString().Trim();

                                                    bInitialRstSftDyCnt = true;
                                                }
                                                else if (bInitialRstSftDyCnt == false)
                                                {
                                                    RosterShiftDayCount = Convert.ToInt32(dvEmpSftAssBfrFmDt[0]["StartFromDay"].ToString().Trim()) - 1;
                                                    RosterShiftWeekOffCount = Convert.ToInt32(dvEmpSftAss[0]["StartFromDay"].ToString().Trim()) - 1;
                                                    RosterChlSftSysID = dvEmpSftAssBfrFmDt[0]["RosterStartShiftID"].ToString().Trim();
                                                    bInitialRstSftDyCnt = true;
                                                }

                                                if (dvEmpSftAss.Count > 0)
                                                {
                                                    sEmpSftAssCurntSysID = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                                    if (sEmpSftAssCurntSysID != sEmpSftAssTempSysID)
                                                    {
                                                        bRstSftChange = true;
                                                    }

                                                    if (bRstSftChange)
                                                    {
                                                        RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[0]["StartFromDay"].ToString().Trim()) - 1;
                                                        RosterChlSftSysID = dvEmpSftAss[0]["RosterStartShiftID"].ToString().Trim();
                                                        bRstSftChange = false;
                                                    }
                                                }
                                                if (bToDayTempShift == false && bPrvDayTempShift == true)
                                                {
                                                    RosterChlSftSysID = RosterChlOldSftSysID;
                                                }
                                                else if (bToDayTempShift == true && bPrvDayTempShift == false/* && string.IsNullOrEmpty(RosterChlOldTempSftSysID) == true*/)
                                                {
                                                    RosterChlSftSysID = RosterChlTempSftSysID;
                                                }

                                                //Set Roster Child Shift SystemID For Current Date in loop
                                                dvSftRstCdl.Table = dtSftRstCdl;
                                                dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                                if (dvSftRstCdl.Count > 0)
                                                {
                                                    #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                    for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                    {//RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                        if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                        {
                                                            ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());

                                                            bIsDaysLengthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthShiftRoster"].ToString().Trim());
                                                            DaysLengthShiftRoster = Convert.ToInt32(dvSftRstCdl[SRC]["DaysLengthShiftRoster"].ToString().Trim());

                                                            bIsFixedDayInMonthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthShiftRoster"].ToString().Trim());
                                                            sFixedDayInMonthShiftRoster = dvSftRstCdl[SRC]["FixedDayInMonthShiftRoster"].ToString().Trim();

                                                            bAlignWithCC = Convert.ToBoolean(dvSftRstCdl[SRC]["IsAlignWithCC"].ToString().Trim());

                                                            bIsFixedDayInMonthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthWeekOff"].ToString().Trim());
                                                            sFixedDayInMonthWeekOff = dvSftRstCdl[SRC]["FixedDayInMonthWeekOff"].ToString().Trim();

                                                            bIsDaysLengthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthWeekOff"].ToString().Trim());
                                                            sWeekOffDay = dvSftRstCdl[SRC]["WeekOffDay"].ToString().Trim();

                                                            bIsWeekOffInShiftLenght = Convert.ToBoolean(dvSftRstCdl[SRC]["IsWeekOffInShiftLenght"].ToString().Trim());
                                                            WeekOffInShiftLenght = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffInShiftLenght"].ToString().Trim());
                                                        }
                                                    }

                                                    //Check RosterShiftDayCount & ShiftDays
                                                    #region Days Length For ShiftRoster
                                                    if (bIsDaysLengthShiftRoster == true)
                                                    {
                                                        if (RosterShiftDayCount >= DaysLengthShiftRoster)
                                                        {
                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                            {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                {
                                                                    RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                                }
                                                            }
                                                            if (RosterShiftSequence == 0)
                                                            {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                RosterShiftSequence = 1;
                                                            }
                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                            {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
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
                                                    }
                                                    #endregion Days Length For ShiftRoster
                                                    #region Fixed Date In Month For Shift Roster Change
                                                    else if (bIsFixedDayInMonthShiftRoster == true)
                                                    {
                                                        if (bFstShiftDay == false)
                                                        {
                                                            dtIdList(sFixedDayInMonthShiftRoster.Trim(), out dsIdLast);
                                                            dtIdLast = dsIdLast.Tables[0];
                                                            dvIdLast = new DataView();
                                                            dvIdLast.Table = dtIdLast;
                                                            dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                            if (dvIdLast.Count > 0)
                                                            {
                                                                //if()
                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                    if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                    {
                                                                        RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                                    }
                                                                }
                                                                if (RosterShiftSequence == 0)
                                                                {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                    RosterShiftSequence = 1;
                                                                }
                                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                                {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                    if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                    {
                                                                        RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                        RosterChlSftSysID = RosterChlNewSftSysID;
                                                                        RosterShiftDayCount = 0;
                                                                        RosterShiftSequence = 0;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                            RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                        }
                                                    }
                                                    #endregion Fixed Date In Month For Shift Roster Change
                                                    #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                    if (bIsWeekOffInShiftLenght == true)
                                                    {
                                                        if (RosterShiftWeekOffCount > WeekOffInShiftLenght)
                                                        {
                                                            RosterShiftWeekOffCount = 1;
                                                        }
                                                        else
                                                        {
                                                            RosterShiftWeekOffCount++;
                                                        }
                                                    }
                                                }

                                                //Update RosterShiftDayCount 
                                                RosterShiftDayCount = RosterShiftDayCount + 1;

                                                #region Week off align with company calendar
                                                if (bAlignWithCC == true)
                                                {
                                                    dvComAssWkOff.Table = dtComAssWkOff;
                                                    dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                    if (dvComAssWkOff.Count > 0)
                                                    {
                                                        sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                        if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                #endregion Week off align with company calendar
                                                #region Fixed Date In Month For Week Off
                                                else if (bIsFixedDayInMonthWeekOff == true)
                                                {
                                                    dtIdList(sFixedDayInMonthWeekOff.Trim(), out dsIdLast);
                                                    dtIdLast = dsIdLast.Tables[0];
                                                    dvIdLast = new DataView();
                                                    dvIdLast.Table = dtIdLast;
                                                    dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                    if (dvIdLast.Count > 0)
                                                    {
                                                        sDayType = "W";
                                                        sDayLengthType = "Week Off";
                                                    }
                                                }
                                                #endregion Fixed Date In Month For Week Off
                                                #region Assign week off day
                                                else if (bIsDaysLengthWeekOff == true)
                                                {
                                                    if (sWeekOffDay.Trim() == dtStDt.DayOfWeek.ToString().Trim())
                                                    {
                                                        sDayType = "W";
                                                        sDayLengthType = "Week Off";
                                                    }
                                                }
                                                #endregion Assign week off day
                                                #region Week Off In Shift Lenght
                                                else if (bIsWeekOffInShiftLenght == true)
                                                {
                                                    //dtIdList(WeekOffInShiftLenght.Trim(), out dsIdLast);
                                                    //dtIdLast = dsIdLast.Tables[0];
                                                    //dvIdLast = new DataView();
                                                    //dvIdLast.Table = dtIdLast;
                                                    //dvIdLast.RowFilter = "Id = " + RosterShiftDayCount + "";
                                                    //if (dvIdLast.Count > 0)
                                                    if (RosterShiftWeekOffCount == WeekOffInShiftLenght)
                                                    {
                                                        sDayType = "W";
                                                        sDayLengthType = "Week Off";
                                                    }
                                                }
                                                #endregion Week Off In Shift Lenght

                                                var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == RosterChlNewSftSysID.Trim());
                                                if (dicShiftDft_Sub != null)
                                                {
                                                    sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                                }
                                                if (sSfTime.Trim().Length > 0)
                                                {
                                                    drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                    drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                    drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                    drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAssBfrFmDt[0]["SystemID"].ToString().Trim();
                                                    drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();
                                                    drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                    drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                    drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                    drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                    drEmpDtWiseSftAss["RosterShiftDayCount"] = RosterShiftDayCount;
                                                    drEmpDtWiseSftAss["RosterShiftWeekOffCount"] = RosterShiftWeekOffCount;
                                                    drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                    drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                    drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                    drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                    drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                    drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                    dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);
                                                }
                                                #endregion if Last updated shift in table 'EmployeeShiftAssign' is roster
                                            }
                                        }

                                        #endregion Check Last updated shift in table 'EmployeeShiftAssign' before fromdate
                                    }

                                    #endregion FromDate & Shift start Date Same and After fromdate to todate not found shift assignment
                                }
                                else if (dvEmpSftAss.Count > 0)
                                {
                                    #region Shift start Date is great than FromDate

                                    dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");

                                    sEmpSftAssCurntSysID = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                    if (sEmpSftAssCurntSysID != sEmpSftAssTempSysID)
                                    {
                                        bRstSftChange = true;
                                    }

                                    if (Convert.ToDateTime(Convert.ToDateTime(dvEmpSftAss[0]["EffectiveDate"].ToString().Trim()).ToString("dd-MMM-yyyy")) < Convert.ToDateTime(dtStDt.ToString("dd-MMM-yyyy")))
                                    {
                                        dvEmpDtWiseSftAss.Table = dtEmpDtWiseSftAss;
                                        dvEmpDtWiseSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate <= '" + dtStDt.ToString("dd-MMM-yyyy") + "'";
                                        if (dvEmpDtWiseSftAss.Count == 0)
                                        {
                                            bShiftProc = false;
                                        }
                                    }

                                    if (bShiftProc == true)
                                    {
                                        #region Check Last updated shift in table 'EmployeeShiftAssign' after fromdate

                                        if (Convert.ToBoolean(dvEmpSftAss[0]["IsFix"].ToString().Trim()) == true)
                                        {
                                            #region Find Fixed Shift Employee's week off align with company calendar or Individual

                                            dvEmpWkOff.Table = dtEmpWkOff;
                                            //dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND FixSystemID = '" + dvEmpSftAss[0]["FixSystemID"].ToString().Trim() + "'";
                                            dvEmpWkOff.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' ";
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

                                            if (bAlignWithCC == true)
                                            {
                                                dvComAssWkOff.Table = dtComAssWkOff;
                                                dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                if (dvComAssWkOff.Count > 0)
                                                {
                                                    sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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

                                            if (bIndividualWeekOff == true)
                                            {
                                                if (sFstOffDay == (dtStDt.DayOfWeek).ToString())
                                                {
                                                    sDayLengthType = sFstDayLengthType;
                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                            var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == dvEmpSftAss[0]["FixSystemID"].ToString().Trim());
                                            if (dicShiftDft_Sub != null)
                                            {
                                                sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                            }
                                            #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                            if (sSfTime.Trim().Length > 0)
                                            {
                                                drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                                drEmpDtWiseSftAss["ShiftSystemID"] = dvEmpSftAss[0]["FixSystemID"].ToString().Trim();
                                                drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                drEmpDtWiseSftAss["RosterShiftDayCount"] = 0;
                                                drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);
                                            }
                                            #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                        }
                                        //else if (Convert.ToBoolean(dvEmpSftAss[0]["IsRoster"].ToString().Trim()) == true)
                                        else if (RunRoster)
                                        {
                                            #region If Last updated shift in table 'EmployeeShiftAssign' is roster

                                            //Take ShiftRosterMasterSystemID in a variable name 'RosterMstSysID'
                                            RosterMstSysID = dvEmpSftAss[0]["RosterSystemID"].ToString().Trim();

                                            dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                            dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtStDt.ToString("dd-MMM-yyyy") + "'";
                                            if (dvSftRstDayCnt.Count > 0)
                                            {
                                                bToDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                            }
                                            else
                                            {
                                                bToDayTempShift = false;
                                            }

                                            //Take Last date 'ShiftSystemID' and 'RosterShiftDayCount' from the table 'EmpDateWiseShiftAssign'
                                            dvSftRstDayCnt.Table = dtSftRstDayCnt;
                                            dvSftRstDayCnt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + dtLastDt + "'";
                                            if (dvSftRstDayCnt.Count > 0)
                                            {
                                                RosterShiftDayCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftDayCount"].ToString().Trim());
                                                RosterShiftWeekOffCount = Convert.ToInt32(dvSftRstDayCnt[0]["RosterShiftWeekOffCount"].ToString().Trim());
                                                RosterChlSftSysID = dvSftRstDayCnt[0]["ShiftSystemID"].ToString().Trim();
                                                bPrvDayTempShift = Convert.ToBoolean(dvSftRstDayCnt[0]["IsManuallyChanged"].ToString().Trim());
                                                RosterChlOldSftSysID = dvSftRstDayCnt[0]["RosterShiftSystemId"].ToString().Trim();
                                                sEmpSftAssTempSysID = dvSftRstDayCnt[0]["EmpSftAssiSystemID"].ToString().Trim();
                                                bInitialRstSftDyCnt = true;
                                            }
                                            else if (bInitialRstSftDyCnt == false)
                                            {
                                                RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[0]["StartFromDay"].ToString().Trim()) - 1;
                                                RosterShiftWeekOffCount = Convert.ToInt32(dvEmpSftAss[0]["StartFromDay"].ToString().Trim()) - 1;
                                                RosterChlSftSysID = dvEmpSftAss[0]["RosterStartShiftID"].ToString().Trim();
                                                bInitialRstSftDyCnt = true;
                                            }

                                            if (dvEmpSftAss.Count > 0)
                                            {
                                                sEmpSftAssCurntSysID = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                                if (sEmpSftAssCurntSysID != sEmpSftAssTempSysID)
                                                {
                                                    bRstSftChange = true;
                                                }

                                                if (bRstSftChange)
                                                {
                                                    RosterShiftDayCount = Convert.ToInt32(dvEmpSftAss[0]["StartFromDay"].ToString().Trim()) - 1;
                                                    RosterChlSftSysID = dvEmpSftAss[0]["RosterStartShiftID"].ToString().Trim();
                                                    bRstSftChange = false;
                                                }
                                            }

                                            if (bToDayTempShift == false && bPrvDayTempShift == true)
                                            {
                                                RosterChlSftSysID = RosterChlOldSftSysID;
                                            }
                                            else if (bToDayTempShift == true && bPrvDayTempShift == false/* && string.IsNullOrEmpty(RosterChlOldTempSftSysID) == true*/)
                                            {
                                                RosterChlSftSysID = RosterChlTempSftSysID;
                                            }

                                            //Set Roster Child Shift SystemID For Current Date in loop
                                            dvSftRstCdl.Table = dtSftRstCdl;
                                            dvSftRstCdl.RowFilter = "SRMasterSystemID = '" + RosterMstSysID.Trim() + "'";
                                            if (dvSftRstCdl.Count > 0)
                                            {
                                                #region Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'

                                                for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                {//RosterChlSftSysID Match with the field 'ShiftDefinationID' of table 'ShiftRosterChild'
                                                    if (dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim() == RosterChlSftSysID.Trim())
                                                    {
                                                        ShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());

                                                        bIsDaysLengthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthShiftRoster"].ToString().Trim());
                                                        DaysLengthShiftRoster = Convert.ToInt32(dvSftRstCdl[SRC]["DaysLengthShiftRoster"].ToString().Trim());

                                                        bIsFixedDayInMonthShiftRoster = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthShiftRoster"].ToString().Trim());
                                                        sFixedDayInMonthShiftRoster = dvSftRstCdl[SRC]["FixedDayInMonthShiftRoster"].ToString().Trim();

                                                        bAlignWithCC = Convert.ToBoolean(dvSftRstCdl[SRC]["IsAlignWithCC"].ToString().Trim());

                                                        bIsFixedDayInMonthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsFixedDayInMonthWeekOff"].ToString().Trim());
                                                        sFixedDayInMonthWeekOff = dvSftRstCdl[SRC]["FixedDayInMonthWeekOff"].ToString().Trim();

                                                        bIsDaysLengthWeekOff = Convert.ToBoolean(dvSftRstCdl[SRC]["IsDaysLengthWeekOff"].ToString().Trim());
                                                        sWeekOffDay = dvSftRstCdl[SRC]["WeekOffDay"].ToString().Trim();

                                                        bIsWeekOffInShiftLenght = Convert.ToBoolean(dvSftRstCdl[SRC]["IsWeekOffInShiftLenght"].ToString().Trim());
                                                        WeekOffInShiftLenght = Convert.ToInt32(dvSftRstCdl[SRC]["WeekOffInShiftLenght"].ToString().Trim());
                                                    }
                                                }

                                                //Check RosterShiftDayCount & ShiftDays
                                                #region Days Length For ShiftRoster
                                                if (bIsDaysLengthShiftRoster == true)
                                                {
                                                    if (RosterShiftDayCount >= DaysLengthShiftRoster)
                                                    {
                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                        {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                            if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                            {
                                                                RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                            }
                                                        }
                                                        if (RosterShiftSequence == 0)
                                                        {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                            RosterShiftSequence = 1;
                                                        }
                                                        for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                        {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                            if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
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
                                                }
                                                #endregion Days Length For ShiftRoster
                                                #region Fixed Date In Month For Shift Roster Change
                                                else if (bIsFixedDayInMonthShiftRoster == true)
                                                {
                                                    if (bFstShiftDay == false)
                                                    {
                                                        dtIdList(sFixedDayInMonthShiftRoster.Trim(), out dsIdLast);
                                                        dtIdLast = dsIdLast.Tables[0];
                                                        dvIdLast = new DataView();
                                                        dvIdLast.Table = dtIdLast;
                                                        dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                        if (dvIdLast.Count > 0)
                                                        {
                                                            //if()
                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                            {//Find Next 'ShiftSequence' in the table 'ShiftRosterChild'
                                                                if ((ShiftSequence + 1) == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                {
                                                                    RosterShiftSequence = Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim());
                                                                }
                                                            }
                                                            if (RosterShiftSequence == 0)
                                                            {//If not found, set the variable 'RosterShiftSequence' value is 1
                                                                RosterShiftSequence = 1;
                                                            }
                                                            for (int SRC = 0; SRC < dvSftRstCdl.Count; SRC++)
                                                            {//Find the 'ShiftDefinationID' depends on RosterShiftSequence in the table 'ShiftRosterChild'
                                                                if (RosterShiftSequence == Convert.ToInt32(dvSftRstCdl[SRC]["ShiftSequence"].ToString().Trim()))
                                                                {
                                                                    RosterChlNewSftSysID = dvSftRstCdl[SRC]["ShiftDefinationID"].ToString().Trim();
                                                                    RosterChlSftSysID = RosterChlNewSftSysID;
                                                                    RosterShiftDayCount = 0;
                                                                    RosterShiftSequence = 0;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {//If last date RosterShiftDayCount is less then ShiftDays from the table 'ShiftRosterChild' than Roster Child shift remain same
                                                        RosterChlNewSftSysID = RosterChlSftSysID.Trim();
                                                    }
                                                }
                                                #endregion Fixed Date In Month For Shift Roster Change
                                                #endregion Find out last date 'ShiftSequence' and 'ShiftDays' in the table 'ShiftRosterChild' using ShiftRosterMasterSystemID 'RosterMstSysID'
                                                if (bIsWeekOffInShiftLenght == true)
                                                {
                                                    if (RosterShiftWeekOffCount > WeekOffInShiftLenght)
                                                    {
                                                        RosterShiftWeekOffCount = 1;
                                                    }
                                                    else
                                                    {
                                                        RosterShiftWeekOffCount++;
                                                    }
                                                }
                                            }

                                            //Update RosterShiftDayCount 
                                            RosterShiftDayCount = RosterShiftDayCount + 1;

                                            #region Week off align with company calendar
                                            if (bAlignWithCC == true)
                                            {
                                                dvComAssWkOff.Table = dtComAssWkOff;
                                                dvComAssWkOff.RowFilter = "OffDayDate = '" + strStDt + "'";
                                                if (dvComAssWkOff.Count > 0)
                                                {
                                                    sDayLengthType = dvComAssWkOff[0]["DayLengthType"].ToString().Trim();

                                                    if (sDayLengthType == "Full Day" || sDayLengthType == "FullDay")
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
                                            #endregion Week off align with company calendar
                                            #region Fixed Date In Month For Week Off
                                            else if (bIsFixedDayInMonthWeekOff == true)
                                            {
                                                dtIdList(sFixedDayInMonthWeekOff.Trim(), out dsIdLast);
                                                dtIdLast = dsIdLast.Tables[0];
                                                dvIdLast = new DataView();
                                                dvIdLast.Table = dtIdLast;
                                                dvIdLast.RowFilter = "Id = " + Convert.ToInt32(dtStDt.Day) + "";
                                                if (dvIdLast.Count > 0)
                                                {
                                                    sDayType = "W";
                                                    sDayLengthType = "Week Off";
                                                }
                                            }
                                            #endregion Fixed Date In Month For Week Off
                                            #region Assign week off day
                                            else if (bIsDaysLengthWeekOff == true)
                                            {
                                                if (sWeekOffDay.Trim() == dtStDt.DayOfWeek.ToString().Trim())
                                                {
                                                    sDayType = "W";
                                                    sDayLengthType = "Week Off";
                                                }
                                            }
                                            #endregion Assign week off day
                                            #region Week Off In Shift Lenght
                                            else if (bIsWeekOffInShiftLenght == true)
                                            {
                                                //dtIdList(WeekOffInShiftLenght.Trim(), out dsIdLast);
                                                //dtIdLast = dsIdLast.Tables[0];
                                                //dvIdLast = new DataView();
                                                //dvIdLast.Table = dtIdLast;
                                                //dvIdLast.RowFilter = "Id = " + RosterShiftDayCount + "";
                                                //if (dvIdLast.Count > 0)
                                                if (RosterShiftWeekOffCount == WeekOffInShiftLenght)
                                                {
                                                    sDayType = "W";
                                                    sDayLengthType = "Week Off";
                                                }
                                            }
                                            #endregion Week Off In Shift Lenght

                                            var dicShiftDft_Sub = dicShiftDft.Find(x => x.SystemID == RosterChlNewSftSysID.Trim());
                                            if (dicShiftDft_Sub != null)
                                            {
                                                sSfTime = strStDt + " " + ((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss");
                                            }
                                            if (sSfTime.Trim().Length > 0)
                                            {
                                                drEmpDtWiseSftAss = dtEmpDtWiseSftAss.NewRow();

                                                drEmpDtWiseSftAss["EmpSystemID"] = sEmpSystemID.Trim();
                                                drEmpDtWiseSftAss["WorkDate"] = strStDt.Trim();
                                                drEmpDtWiseSftAss["EmpSftAssiSystemID"] = dvEmpSftAss[0]["SystemID"].ToString().Trim();
                                                drEmpDtWiseSftAss["ShiftSystemID"] = RosterChlNewSftSysID.Trim();
                                                drEmpDtWiseSftAss["ShiftInTime"] = sSfTime;

                                                drEmpDtWiseSftAss["DayType"] = sDayType.Trim();

                                                drEmpDtWiseSftAss["AddedBy"] = "Schedule";
                                                drEmpDtWiseSftAss["DateAdded"] = DateTime.Now;

                                                drEmpDtWiseSftAss["RosterShiftDayCount"] = RosterShiftDayCount;
                                                drEmpDtWiseSftAss["RosterShiftWeekOffCount"] = RosterShiftWeekOffCount;
                                                drEmpDtWiseSftAss["AttdnLock"] = 0;
                                                drEmpDtWiseSftAss["ToReprocess"] = "No";
                                                drEmpDtWiseSftAss["GroupID"] = GroupSysID.Trim();
                                                drEmpDtWiseSftAss["PlantID"] = sPlantID.Trim();

                                                drEmpDtWiseSftAss["UpdatedBy"] = "Schedule";
                                                drEmpDtWiseSftAss["DateUpdated"] = DateTime.Now;

                                                dtEmpDtWiseSftAss.Rows.Add(drEmpDtWiseSftAss);
                                            }
                                            #endregion if Last updated shift in table 'EmployeeShiftAssign' is roster
                                        }

                                        #endregion Check Last updated shift in table 'EmployeeShiftAssign' after fromdate
                                    }
                                    sEmpSftAssTempSysID = sEmpSftAssCurntSysID;

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

                            dtStDt = dtStDt.AddDays(1);
                        }
                        //}
                    }
                    //clsStaticInfo obs = new clsStaticInfo();
                    SaveDataSets(dsEmpDtWiseSftAss, dsAttdnProc);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsEmpSftAssBfrFmDt = null;
            }
        }//End Function  
        private void AttdnProcBaseOn(string GroupSysID, string _plantid, string sAttnDate, out DataSet dsEmpInfo)
        {
            dsEmpInfo = null;

            try
            {
                GetAllRegsterPersonOnSystemAttdnProc(GroupSysID.Trim(), _plantid, sAttnDate.Trim(), out dsEmpInfo);

                //180607 Pratibha
                radDwLdEnrollID = true;//default
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function  
        private void AttdnProcBaseOn_SNA(string GroupSysID, string _plantid, string sAttnDate, out DataSet dsEmpInfo)
        {
            dsEmpInfo = null;

            try
            {
                GetAllShiftLessEmployees(GroupSysID.Trim(), _plantid, sAttnDate.Trim(), out dsEmpInfo);

                //180607 Pratibha
                radDwLdEnrollID = true;//default
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function  
        string GetOffDay(DataTable dtOffDay, string _plantid, string sDayType)
        {
            string sComHoliDay = string.Empty;
            string sOffDay = string.Empty;
            try
            {
                DataView dvOffDay = new DataView();
                dvOffDay.Table = dtOffDay;
                dvOffDay.RowFilter = "PlantID = '" + _plantid + "'";
                if (dvOffDay.Count > 0)
                {
                    for (int ofd = 0; ofd < dvOffDay.Count; ofd++)
                    {
                        sComHoliDay = sComHoliDay + dvOffDay[ofd]["OffDayType"].ToString().Trim();
                    }
                }

                if (sDayType.ToUpper() == "W")
                {
                    sOffDay = sComHoliDay + sDayType;
                }
                else
                {
                    sOffDay = sComHoliDay;
                }
                return sOffDay;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function  
        void xGetDayStatus(string sDayType, string _maxLateTime, string sOfficeStartTime, string sOfficeInTime, string sBreakStratTime, string sBreakEndTime, int iDeviceID, string sLogDownLoadNum, DataTable dtRawData, ref string sInTime, out string sDayStatus, out bool bMoreInMarg)
        {
            try
            {
                sDayStatus = "";
                bMoreInMarg = false;
                if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) <= Convert.ToDateTime(sOfficeInTime))
                {
                    sDayStatus = "P";
                }
                else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "WA" || sDayType.ToUpper() == "HA" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sOfficeInTime))
                {
                    if (Convert.ToDateTime(_maxLateTime) < Convert.ToDateTime(sInTime))//by monir 180308
                    {
                        sDayStatus = "A";
                    }
                    else
                    {
                        sDayStatus = "L";
                    }
                }
                else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) < Convert.ToDateTime(sOfficeStartTime))
                {
                    sDayStatus = "A";
                    sInTime = "00:00:00";
                    bMoreInMarg = true;
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
                }

                #region if lowest In Time is less than in time Margin and after get another In time after intime margin

                if (bMoreInMarg == true)
                {
                    string sInTimeTmp = "00:00:00";
                    string sInTimeRowIDTmp = "";

                    sInTime = "00:00:00";
                    string sInTimeRowID = "";
                    int iDeviceIDTmp = 0;

                    DataView dvRawData = null;

                    // This is for nornal workday and if employee have 2nd half week end
                    if (sDayType.ToUpper() == "SHW")// if (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW")//by monir 180308
                    {
                        #region P
                        dvRawData.Table = dtRawData;
                        dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                        if (dvRawData.Count > 0)
                        {
                            ///keep the first time as entry time
                            string _intime = "00:00:00";
                            for (int RData = 0; RData < dvRawData.Count; RData++)
                            {
                                if (dvRawData[RData]["PTime"].ToString() != "")
                                {
                                    string sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    //_intime= Convert.ToDateTime(sPInTime).ToString("HH:mm:ss");
                                    if (_intime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(_intime.Trim()))
                                    {
                                        _intime = sPInTime;
                                    }


                                    if (Convert.ToDateTime(sPInTime) >= Convert.ToDateTime(sOfficeStartTime.Trim()))
                                    {
                                        if (sInTime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(sInTime.Trim()))
                                        {
                                            sInTime = _intime;
                                            //sInTime = sPInTime;
                                            sInTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                            iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                            if (sInTimeTmp != "00:00:00" & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp))
                                            {
                                                sInTime = sInTimeTmp;
                                                sInTime = sInTimeTmp;
                                                sInTimeRowID = sInTimeRowIDTmp;
                                                iDeviceID = iDeviceIDTmp;
                                            }
                                            sInTimeTmp = sInTime;
                                            sInTimeRowIDTmp = sInTimeRowID;
                                            iDeviceIDTmp = iDeviceID;
                                        }

                                        sDayStatus = "P";
                                    }//>
                                }//ptime
                            }//for
                        }
                        #endregion
                    }
                    // This is for employee have 1st half week end
                    else if (sDayType.ToUpper() == "FHW") // 
                    {
                        #region P
                        dvRawData.Table = dtRawData;
                        dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                        if (dvRawData.Count > 0)
                        {
                            for (int RData = 0; RData < dvRawData.Count; RData++)
                            {
                                if (dvRawData[RData]["PTime"].ToString() != "")
                                {
                                    string sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");

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
                                    }//>
                                }//ptime
                            }//for
                        }
                        #endregion
                    }
                }

                #endregion if lowest In Time is less than in time Margin and after get another In time after intime margin
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void xxGetDayStatus(string sDate, string sDayType, DateTime _maxLateTime, string sOfficeStartTime, string sOfficeInTime, string sBreakStratTime, string sBreakEndTime, string sLogDownLoadNum, DataTable dtRawData, ref string sInTime, out string sDayStatus, out bool bMoreInMarg)
        {
            try
            {
                sDayStatus = "";
                bMoreInMarg = false;
                if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) <= Convert.ToDateTime(sOfficeInTime))
                {
                    sDayStatus = "P";
                }
                else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "WA" || sDayType.ToUpper() == "HA" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sOfficeInTime))
                {
                    if (_maxLateTime < Convert.ToDateTime(sDate + " " + sInTime))//by monir 180308
                    {
                        sDayStatus = "A";
                    }
                    else
                    {
                        sDayStatus = "L";
                    }
                }
                else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) < Convert.ToDateTime(sOfficeStartTime))
                {
                    sDayStatus = "A";
                    sInTime = "00:00:00";
                    bMoreInMarg = true;
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
                }
                //else if (sInTime == "00:00:00" && sDayType.ToUpper() == "NW")
                //{
                //    sDayStatus = "A";
                //    sInTime = "00:00:00";
                //}

                #region if lowest In Time is less than in time Margin and after get another In time after intime margin

                if (bMoreInMarg == true)
                {
                    string sInTimeTmp = "00:00:00";
                    string sInTimeRowIDTmp = "";

                    sInTime = "00:00:00";
                    string sInTimeRowID = "";
                    int iDeviceIDTmp = 0;

                    DataView dvRawData = null;

                    // This is for nornal workday and if employee have 2nd half week end
                    if (sDayType.ToUpper() == "SHW")// if (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW")//by monir 180308
                    {
                        #region P
                        dvRawData.Table = dtRawData;
                        dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                        if (dvRawData.Count > 0)
                        {
                            ///keep the first time as entry time
                            string _intime = "00:00:00";
                            for (int RData = 0; RData < dvRawData.Count; RData++)
                            {
                                if (dvRawData[RData]["PTime"].ToString() != "")
                                {
                                    string sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    //_intime= Convert.ToDateTime(sPInTime).ToString("HH:mm:ss");
                                    if (_intime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(_intime.Trim()))
                                    {
                                        _intime = sPInTime;
                                    }


                                    if (Convert.ToDateTime(sPInTime) >= Convert.ToDateTime(sOfficeStartTime.Trim()))
                                    {
                                        if (sInTime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(sInTime.Trim()))
                                        {
                                            sInTime = _intime;
                                            //sInTime = sPInTime;
                                            sInTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                            //iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                            if (sInTimeTmp != "00:00:00" & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp))
                                            {
                                                sInTime = sInTimeTmp;
                                                sInTime = sInTimeTmp;
                                                sInTimeRowID = sInTimeRowIDTmp;
                                                //iDeviceID = iDeviceIDTmp;
                                            }
                                            sInTimeTmp = sInTime;
                                            sInTimeRowIDTmp = sInTimeRowID;
                                            //iDeviceIDTmp = iDeviceID;
                                        }

                                        sDayStatus = "P";
                                    }//>
                                }//ptime
                            }//for
                        }
                        #endregion
                    }
                    // This is for employee have 1st half week end
                    else if (sDayType.ToUpper() == "FHW") // 
                    {
                        #region P
                        dvRawData.Table = dtRawData;
                        dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                        if (dvRawData.Count > 0)
                        {
                            for (int RData = 0; RData < dvRawData.Count; RData++)
                            {
                                if (dvRawData[RData]["PTime"].ToString() != "")
                                {
                                    string sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");

                                    if (Convert.ToDateTime(sPInTime) >= Convert.ToDateTime(sBreakEndTime.Trim()))
                                    {
                                        if (sInTime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(sInTime.Trim()))
                                        {
                                            sInTime = sPInTime;
                                            sInTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                            //iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                            if (sInTimeTmp != "00:00:00" & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp))
                                            {
                                                sInTime = sInTimeTmp;
                                                sInTimeRowID = sInTimeRowIDTmp;
                                                //iDeviceID = iDeviceIDTmp;
                                            }
                                            sInTimeTmp = sInTime;
                                            sInTimeRowIDTmp = sInTimeRowID;
                                            //iDeviceIDTmp = iDeviceID;
                                        }
                                        sDayStatus = "P";
                                    }//>
                                }//ptime
                            }//for
                        }
                        #endregion
                    }
                }

                #endregion if lowest In Time is less than in time Margin and after get another In time after intime margin
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void xxxGetDayStatus(string sDate, string sDayType, DateTime _maxLateTime, string sOfficeStartTime, string sOfficeInTime, string sBreakStratTime, string sBreakEndTime, string sLogDownLoadNum, DataTable dtRawData, ref string sInTime, out string sDayStatus, out bool bMoreInMarg)
        {
            try
            {
                sDayStatus = "";
                bMoreInMarg = false;
                if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) <= Convert.ToDateTime(sOfficeInTime))
                {
                    sDayStatus = "P";
                }
                else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "H" || sDayType.ToUpper() == "W" || sDayType.ToUpper() == "WA" || sDayType.ToUpper() == "HA" || sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) >= Convert.ToDateTime(sOfficeStartTime) & Convert.ToDateTime(sInTime) > Convert.ToDateTime(sOfficeInTime))
                {
                    if (_maxLateTime < Convert.ToDateTime(sDate + " " + sInTime))//by monir 180308
                    {
                        sDayStatus = "A";
                    }
                    else
                    {
                        sDayStatus = "L";
                    }
                }
                else if (sInTime != "00:00:00" & (sDayType.ToUpper() == "NW" || sDayType.ToUpper() == "SHW") && Convert.ToDateTime(sInTime) < Convert.ToDateTime(sOfficeStartTime))
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
        void xGetMaxRaw()
        {
            try
            {
                //dvRawData = new DataView();
                //dvRawData.Table = dtRawData;
                //dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                //if (dvRawData.Count > 0)
                //{
                //    for (int RData = 0; RData < dvRawData.Count; RData++)
                //    {
                //        if (dvRawData[RData]["PTime"].ToString() != "")
                //        {
                //            if (sOutDate == Convert.ToDateTime(dvRawData[RData]["PDate"].ToString()).ToString("dd-MMM-yyyy"))
                //            {
                //                string sysOutTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");
                //                if (sOutTime == "00:00:00" || Convert.ToDateTime(sysOutTime.Trim()) > Convert.ToDateTime(sOutTime.Trim()))
                //                {
                //                    sOutTime = sysOutTime;
                //                    sOutTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                //                    //iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                //                    if (sOutTimeTmp != "00:00:00" & Convert.ToDateTime(sOutTime) < Convert.ToDateTime(sOutTimeTmp))
                //                    {
                //                        sOutTime = sOutTimeTmp;
                //                        sOutTimeRowID = sOutTimeRowIDTmp;
                //                        //iDeviceID = iDeviceIDTmp;
                //                    }
                //                    sOutTimeTmp = sOutTime;
                //                    sOutTimeRowIDTmp = sOutTimeRowID;
                //                    //iDeviceIDTmp = iDeviceID;
                //                }
                //            }
                //        }

                //        drRawData = dvRawData[RData].Row;
                //        drRawData.BeginEdit();
                //        drRawData["ProcessedFlag"] = 1;
                //        drRawData.EndEdit();
                //    }//for
                //}//if count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool InDataProcess(string GroupSysID, string _plantid, string sAttnDate, string strYrSystemID, bool radDwLdEnrollID, string strYrFromDate, string strYrToDate, bool _bReProc)//1
        {
            #region Declare variables
            DataSet dsCompensatoryOff = null;
            DataSet dsCompensatoryOffEmpList = null;


            DataSet dsDayType = null;
            DataTable dtDayType = null;
            DataView dvDayType = null;

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

            DataSet dsLvTransDtl = null;
            DataTable dtLvTransDtl = null;
            DataRow drLvTransDtl = null;
            DataView dvLvTransDtl = null;

            DataSet dsODDetail = null;
            DataTable dtODDetail = null;
            DataRow drODDetail = null;
            DataView dvODDetail = null;

            DataSet dsLvTrans = null;
            DataTable dtLvTrans = null;
            DataView dvLvTrans = null;

            DataSet dsLvAvail = null;
            DataTable dtLvAvail = null;
            DataView dvLvAvail = null;

            DataSet dsOffDay = null;
            DataTable dtOffDay = null;
            DataView dvOffDay = null;

            DataSet dsRest = null;
            DataSet dsOD = null;

            string sOfficeStartTime = "";
            string sOfficeInTime = "";
            string sOrgOfficeInTime = "";
            string sLogDownLoadNum = "";

            string sEmpSysID = "";
            string sEmpCode = "";
            string sShiftSystemID = "";
            string sLastProcDate = "";
            string sPlantID = "";

            string sInTime = "";
            string sInTimeRowID = string.Empty;
            string sInTimeRowPunch = string.Empty;
            //int iDeviceID = 0;
            string sInTimeTmp = "";
            //string sInTimeRowIDTmp = string.Empty;
            string sDayStatusTmp = "";
            //int iDeviceIDTmp = 0;
            string sDayStatus = "";
            string sPrvDayStatus = "";
            string sLvTrans = "";
            bool IsHalfDayLeave = false;
            decimal _LeaveDuration = 0;
            bool IsFirstHalfLeave = false;
            string sOffDay = "";
            string sComHoliDay = "";
            string sLvTnsDtlSysID = "";
            string sLvPolDtlSysID = "";
            int sLvAvailed = 0;
            int iInTimeStartMargin = 0;
            int _absentEndMergin = 0;
            //string _maxLateTime = string.Empty;

            string sDayType = "";

            string sBreakStratTime = "";
            string sBreakEndTime = "";

            //string sDate = "";
            string pDate = "";
            //string sPrvDate = "";
            string sWorkingDate = "";
            bool bMoreInMarg = false;

            bool bValid = false;
            bool bAttdnProc = true;
            bool IsOToverHalfDay = false;
            bool IsWeekendFixedAsWeekend = false;
            ShortLeaveSetting _shortleave_setting = null;
            double _PaidHours = 0;
            string _RestPK = string.Empty;
            string _ODPK = string.Empty;
            bool bOTEntitle = false;
            #endregion local variables

            try
            {
                sPlantID = _plantid;
                #region DataSet
                #region get sDate and sPrvDate
                pDate = sAttnDate.Trim();
                //sPrvDate = (Convert.ToDateTime(sAttnDate.Trim()).AddDays(-1)).ToString("dd-MMM-yyyy");
                sWorkingDate = sAttnDate.Trim();

                #endregion
                #region  GetLeaveTrnDetailIds
                DataSet dsLTDIds = null;
                GetLeaveTrnDetailIds(GroupSysID, _plantid, sEmpSystemIDColl.Trim(), sAttnDate, out dsLTDIds);
                string _LTDIds = "''";
                //string _LTDIds = "''";
                for (int i = 0; i < dsLTDIds.Tables[0].Rows.Count; i++)
                {
                    if (_LTDIds == "''")
                    {
                        _LTDIds = "'" + dsLTDIds.Tables[0].Rows[i]["SystemID"].ToString() + "'";
                    }
                    else
                    {
                        _LTDIds += ",'" + dsLTDIds.Tables[0].Rows[i]["SystemID"].ToString() + "'";
                    }
                }
                #endregion
                #region GetODMasterIds
                DataSet dsODIds = null;
                GetODMasterIds(GroupSysID, _plantid, sEmpSystemIDColl.Trim(), sAttnDate, out dsODIds);
                string _ODIds = "''";
                //string _LTDIds = "''";
                for (int i = 0; i < dsODIds.Tables[0].Rows.Count; i++)
                {
                    if (_ODIds == "''")
                    {
                        _ODIds = "'" + dsODIds.Tables[0].Rows[i]["SystemID"].ToString() + "'";
                    }
                    else
                    {
                        _ODIds += ",'" + dsODIds.Tables[0].Rows[i]["SystemID"].ToString() + "'";
                    }
                }
                #endregion
                #region GetShortLeaveSettingPlantWise
                _shortleave_setting = new ShortLeaveSetting();
                DataSet dsShortLeaveSetting = null;
                GetShortLeaveSettingPlantWise(sPlantID, out dsShortLeaveSetting);
                if (dsShortLeaveSetting.Tables[0].Rows.Count > 0)
                {
                    _shortleave_setting.IsHalfDayPresentAllowed = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsHalfDayPresentAllowed"].ToString());
                    _shortleave_setting.IsShortLeaveAllowed = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsShortLeaveAllowed"].ToString());
                    _shortleave_setting.IsTowShortLeaveAllowedInaDay = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsTowShortLeaveAllowedInaDay"].ToString());
                    _shortleave_setting.MaxShortLeaveInaMonth = Convert.ToInt32(GetNumData(dsShortLeaveSetting.Tables[0].Rows[0]["MaxShortLeaveInaMonth"].ToString()));
                    //IsOToverHalfDay = bplib.clsWebLib.GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsOTOverHalfDay"].ToString());
                }
                #endregion
                #region GetHRSettingPlantWise
                DataSet dsHRSetting = null;
                GetHRSettingPlantWise(sPlantID, out dsHRSetting);
                if (dsHRSetting.Tables[0].Rows.Count > 0)
                {
                    IsOToverHalfDay = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsOTOverHalfDay"].ToString());
                    IsWeekendFixedAsWeekend = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsWeekendCount"].ToString());
                }

                #endregion
                #region GetDayType
                GetDayType(out dsDayType);
                dtDayType = dsDayType.Tables[0];
                dvDayType = new DataView();

                #endregion

                #region Raw, Processed, Manual, TobeProcessed
                //GetAttdnRawDataForAttdnProc(GroupSysID.Trim(), sDate.Trim(), "IN", out dsRawData);
                GetRawAll(GroupSysID.Trim(), pDate.Trim(), "IN", sEmpSystemIDColl.Trim(), out dsRawData);
                dtRawData = dsRawData.Tables[0];

                //GetAttdnProcData(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sPrvDate.Trim(), sDate.Trim(), out dsAttnProcData);
                GetProcessedData(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), pDate.Trim(), out dsAttnProcData);
                dtAttnProcData = dsAttnProcData.Tables[0];

                //GetAttdnManualData(GroupSysID.Trim(), _plantid, sEmpSystemIDColl.Trim(), sAttnDate.Trim(), out dsMnAttData);
                GetManualData(_plantid, sEmpSystemIDColl.Trim(), sAttnDate.Trim(), out dsMnAttData);
                dtMnAttData = dsMnAttData.Tables[0];
                dvMnAttData = new DataView();

                GetEmployeeInfo(GroupSysID.Trim(), _plantid, sEmpSystemIDColl.Trim(), pDate.Trim(), out dsEmpInfo);
                #endregion

                #region GetLeaveTransactionDetails
                GetLeaveTransactionDetails(pDate.Trim(), _LTDIds, out dsLvTransDtl);
                dtLvTransDtl = dsLvTransDtl.Tables[0];
                #endregion
                #region GetODDetails
                GetODDetails(pDate.Trim(), _ODIds, out dsODDetail);
                dtODDetail = dsODDetail.Tables[0];
                #endregion
                #region  GetRestInfo
                GetRestInfo(pDate.Trim(), _plantid, sEmpSystemIDColl, out dsRest);
                #endregion
                #region GetODInfo
                GetODInfo(pDate.Trim(), _plantid, sEmpSystemIDColl, out dsOD);
                #endregion
                //dtRest = dsRest.Tables[0];
                #region GetLeaveTransactionInfo
                GetLeaveTransactionInfo(GroupSysID.Trim(), _plantid, sEmpSystemIDColl.Trim(), pDate.Trim(), out dsLvTrans);
                dtLvTrans = dsLvTrans.Tables[0];
                #endregion
                #region GetAvailedLvInfo
                GetAvailedLvInfo(GroupSysID.Trim(), _plantid, strYrSystemID.Trim(), strYrFromDate.Trim(), strYrToDate.Trim(), out dsLvAvail);
                dtLvAvail = dsLvAvail.Tables[0];
                #endregion
                #region GetAllPlantOffDayInformation
                GetAllPlantOffDayInformation(GroupSysID.Trim(), _plantid, pDate.Trim(), out dsOffDay);
                dtOffDay = dsOffDay.Tables[0];
                #endregion
                #region GetPaidHours
                DataSet dsPaidHours = null;
                GetPaidHours(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), out dsPaidHours);
                #endregion

                //============ kabir ==========
                GetCompensatoryOffPlantData(_plantid, pDate.Trim(), out dsCompensatoryOff);
                GetCompensatoryOffEmpListData(_plantid, pDate.Trim(), sEmpSystemIDColl.Trim(), out dsCompensatoryOffEmpList);
                var CompensatoryDateTreatmentType = string.Empty;
                if (dsCompensatoryOff.Tables[0].Rows.Count > 0)
                {
                    CompensatoryDateTreatmentType = dsCompensatoryOff.Tables[0].Rows[0]["CompensatoryDateTreatmentType"].ToString();
                    ////bool IsOriginalDateOTApplicable= Convert.ToBoolean(dsCompensatoryOff.Tables[0].Rows[0]["IsOriginalDateOTApplicable"].ToString());
                }

                #endregion DataSet

                if (dsEmpInfo.Tables[0].Rows.Count > 0)
                {
                    for (int EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                    {
                        #region valiables
                        string flag = "";
                        string ds = "";
                        dicShiftDft _ShiftDft = null;
                        bAttdnProc = true;
                        sComHoliDay = "";
                        sOffDay = "";
                        sLastProcDate = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["LastWorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                        sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();
                        //if (sEmpSysID == "1800529")
                        //{

                        //}

                        DataView dv = new DataView(dsPaidHours.Tables[0]);
                        dv.RowFilter = "EmployeeId='" + sEmpSysID + "'";
                        if (dv.Count > 0)
                        {
                            _PaidHours = Convert.ToDouble(GetNumData(dv[0]["PaidHours"].ToString()));
                        }

                        GetRestPK(dsRest, sEmpSysID, out _RestPK);
                        GetODPK(dsOD, sEmpSysID, out _ODPK);
                        //_PaidHours = Convert.ToDecimal(dsEmpInfo.Tables[0].Rows[EmpCount]["PaidHours"].ToString());
                        bOTEntitle = Convert.ToBoolean(GetBoolData(dsEmpInfo.Tables[0].Rows[EmpCount]["IsOTEntitle"].ToString()));
                        sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();
                        sEmpCode = dsEmpInfo.Tables[0].Rows[EmpCount]["EmployeeCode"].ToString();
                        sOfficeStartTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeStartTime"].ToString().Trim()).ToString("HH:mm:ss");
                        sOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeTime"].ToString().Trim()).AddMinutes(1).AddSeconds(-1).ToString("HH:mm:ss");
                        sOrgOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                        iInTimeStartMargin = Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["InTimeStartMargin"].ToString());

                        _absentEndMergin = Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["AbsentEndMargin"].ToString());

                        //sDate.Trim()
                        var _intime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["InTime"].ToString()).ToString("HH:mm:ss");
                        var _intime_date = Convert.ToDateTime(pDate.Trim() + " " + _intime);

                        var _maxLateTimeDate = _intime_date.AddMinutes(_absentEndMergin);

                        //_maxLateTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["InTime"].ToString().Trim()).AddMinutes(_absentEndMergin).ToString("HH:mm:ss");

                        sShiftSystemID = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftSystemID"].ToString();
                        sDayType = dsEmpInfo.Tables[0].Rows[EmpCount]["DayType"].ToString();

                        sBreakStratTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakStratTime"].ToString().Trim()).ToString("HH:mm:ss");
                        sBreakEndTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakEndTime"].ToString().Trim()).ToString("HH:mm:ss");

                        sOffDay = GetOffDay(dtOffDay, sPlantID, sDayType);
                        sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemId"].ToString();

                        #region plant wise Compensatory

                        if (dsCompensatoryOff.Tables[0].Rows.Count > 0 || dsCompensatoryOffEmpList.Tables[0].Rows.Count > 0)
                        {
                        }



                        if (dsCompensatoryOff.Tables[0].Rows.Count > 0)
                        {
                            flag = dsCompensatoryOff.Tables[0].Rows[0]["flag"].ToString();
                            //190626
                            //if (CompensatoryDateTreatmentType == "W" || CompensatoryDateTreatmentType == "H")
                            //{
                            //    sOffDay = CompensatoryDateTreatmentType;
                            //}
                            //else
                            //{
                            //    sOffDay = "";
                            //}
                            //sDayType = CompensatoryDateTreatmentType;
                        }
                        #endregion

                        #region employe wise Compensatory

                        DataView dvPlantWise = new DataView(dsCompensatoryOffEmpList.Tables[0]);
                        dvPlantWise.RowFilter = "Plantid ='" + _plantid + "' and ForEntirePlant=1";

                        DataView dvEmp = new DataView(dsCompensatoryOffEmpList.Tables[0]);
                        dvEmp.RowFilter = "EmpSystemId ='" + sEmpSysID + "'";

                        if (dvEmp.Count > 0 || dvPlantWise.Count > 0)//either for entire plant or for individual
                        {
                            flag = (dvEmp.Count > 0 ? dvEmp[0]["flag"].ToString() : dvPlantWise[0]["flag"].ToString());
                            CompensatoryDateTreatmentType = (dvEmp.Count > 0 ? dvEmp[0]["CompensatoryDateTreatmentType"].ToString() : dvPlantWise[0]["CompensatoryDateTreatmentType"].ToString());
                        }
                        else
                        {
                            flag = string.Empty;
                            CompensatoryDateTreatmentType = string.Empty;
                        }
                        #endregion

                        //if (dsCompensatoryOff.Tables[0].Rows.Count > 0 || dsCompensatoryOffEmpList.Tables[0].Rows.Count > 0)
                        if (dvEmp.Count > 0 || dvPlantWise.Count > 0)
                        {

                            if (flag == "original")
                            {
                                if (CompensatoryDateTreatmentType == "H")
                                {
                                    ds = "AW";
                                    sOffDay = "";
                                    sDayType = "NW";
                                }
                                else
                                {
                                    sOffDay = "";
                                    sDayType = "NW";
                                    ds = "PW";
                                }

                            }
                            else
                            {
                                if (CompensatoryDateTreatmentType == "H")
                                {
                                    ds = "AH";
                                    sOffDay = "H";
                                    sDayType = "H";
                                }
                                else if (CompensatoryDateTreatmentType == "W")
                                {
                                    ds = "CW";
                                    sOffDay = "W";
                                    sDayType = "W";
                                }
                                else
                                {
                                    ds = "";
                                }
                            }
                        }
                        _ShiftDft = new global::dicShiftDft();
                        GetShiftDefinition(dsEmpInfo.Tables[0].Rows[EmpCount], _ShiftDft);
                        #endregion

                        #region Find InTime from raw Data Table

                        sInTime = "00:00:00";
                        sInTimeRowID = string.Empty;
                        sInTimeRowPunch = string.Empty;
                        //iDeviceID = 0;
                        sInTimeTmp = "00:00:00";
                        //sInTimeRowIDTmp = string.Empty;
                        sDayStatusTmp = "";
                        //iDeviceIDTmp = 0;
                        sDayStatus = "";
                        sPrvDayStatus = "";
                        sLvTrans = "";
                        IsHalfDayLeave = false;
                        _LeaveDuration = 0;
                        sLvPolDtlSysID = "";
                        sLvAvailed = 0;
                        bMoreInMarg = false;


                        GetMinRaw(ref dtRawData, sLogDownLoadNum, _ShiftDft, pDate, out sInTime, out sOfficeStartTime, out sInTimeRowID);
                        string _in_Date = Convert.ToDateTime(sInTime).ToString("dd-MMM-yyyy");
                        sInTimeTmp = Convert.ToDateTime(sInTime).ToString("HH:mm:ss");
                        sInTimeRowPunch = Convert.ToDateTime(sInTime).ToString("dd-MMM-yyyy HH:mm:ss");
                        sInTime = sInTimeTmp;

                        #endregion Find InTime from raw Data Table

                        #region GetDayStatus
                        GetDayStatus(false, pDate.Trim(), sDayType, _maxLateTimeDate, sOfficeStartTime, sOfficeInTime, sLogDownLoadNum, dtRawData, _in_Date, ref sInTime, out sDayStatus, out bMoreInMarg);
                        #endregion

                        #region Leave Transaction

                        sLvTrans = "";
                        bool IsLWP = false;
                        IsHalfDayLeave = false;
                        _LeaveDuration = 0;
                        sLvTnsDtlSysID = "";
                        sLvPolDtlSysID = "";
                        sLvAvailed = 0;

                        string LVDayStatus = "";
                        if (IsWeekendFixedAsWeekend && (sOffDay == "W" || sOffDay == "H"))
                        {
                        }
                        else
                        {
                            dvLvTrans = new DataView();
                            dvLvTrans.Table = dtLvTrans;
                            dvLvTrans.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                            if (dvLvTrans.Count > 0)
                            {
                                LVDayStatus = dvLvTrans[0]["LeaveStatus"].ToString().Trim();
                                sLvTnsDtlSysID = dvLvTrans[0]["LvTrnsSystemID"].ToString().Trim();

                                dvLvTransDtl = new DataView();
                                dvLvTransDtl.Table = dtLvTransDtl;
                                dvLvTransDtl.RowFilter = "LvTrnsSystemID = '" + sLvTnsDtlSysID + "'";
                                if (dvLvTransDtl.Count > 0)
                                {
                                    sLvTrans = dvLvTrans[0]["LTSystemID"].ToString().Trim();
                                    if (dvLvTrans[0]["LWP"].ToString().Trim().ToUpper() == "LEAVE WITHOUT PAY")
                                    {
                                        IsLWP = true;
                                    }
                                    _LeaveDuration = Convert.ToDecimal(GetNumData(dvLvTrans[0]["LeaveDays"].ToString().Trim()));
                                    if (Convert.ToDecimal(GetNumData(dvLvTrans[0]["LeaveDays"].ToString().Trim())) == (decimal)0.5)
                                    {
                                        IsHalfDayLeave = true;
                                        IsFirstHalfLeave = GetBoolData(dvLvTrans[0]["IsFirstHalf"].ToString());
                                        LVDayStatus = "";
                                    }
                                    else if (Convert.ToDecimal(GetNumData(dvLvTrans[0]["LeaveDays"].ToString().Trim())) < 1)
                                    {
                                        LVDayStatus = "";
                                    }
                                }
                            }//count
                        }//IsWeekendFixedAsWeekend

                        #endregion Leave Transaction

                        #region sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                        if (LVDayStatus == "W" || LVDayStatus == "H" || LVDayStatus == "HW" || LVDayStatus == "WH")
                        {
                            LVDayStatus = "";
                        }

                        sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                        if (sDayStatus == "LVA" || sDayStatus == "LVP" || sDayStatus == "LVL"
                            || sDayStatus == "WLVA" || sDayStatus == "WLV" || sDayStatus == "WLVP" || sDayStatus == "WLVL"
                            || sDayStatus == "HLVA" || sDayStatus == "HLV" || sDayStatus == "HLVP" || sDayStatus == "HLVL")
                        {
                            sDayStatus = "LV";
                        }
                        #endregion
                        //************************************************
                        #region dvAttnProcData
                        bool bAttnIsLock = false;
                        bool bManualInTime = false;
                        bool bManualDayStatus = false;
                        bool bToReprocess = true;

                        dvAttnProcData = new DataView();
                        dvAttnProcData.Table = dtAttnProcData;
                        dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + pDate.Trim() + "'";
                        if (dvAttnProcData.Count > 0)
                        {
                            #region edit
                            bAttnIsLock = GetBoolData(dvAttnProcData[0].Row["IsLock"].ToString());
                            if (dvAttnProcData[0].Row["IsLock"].ToString().ToUpper() == "NO")
                            {
                                bToReprocess = false;
                            }

                            if (bAttnIsLock == false)
                            {


                                #region bToReprocess
                                // bToReprocess
                                //data found in process table
                                #region if (dvAttnProcData[0]["InTime"].ToString() != "") -return bToReprocess,sInTimeTmp,sDayStatus
                                if (dvAttnProcData[0]["InTime"].ToString() != "")
                                {


                                    #region  if ((sInTimeTmp != "00:00:00") & (sInTime != "00:00:00"))//old n new---return bToReprocess,sInTimeTmp,sDayStatus
                                    if ((sInTimeTmp != "00:00:00") & (sInTime != "00:00:00"))//old n new

                                    {

                                    }
                                    #endregion
                                    #region  else if ((sInTimeTmp != "00:00:00") & (sInTime == "00:00:00"))------- return bToReprocess = true and sDayStatus
                                    else if ((sInTimeTmp != "00:00:00") & (sInTime == "00:00:00"))
                                    {
                                    }
                                    #endregion

                                    #region else return bToReprocess = false;
                                    else
                                    {
                                        bToReprocess = false;
                                    }
                                    #endregion
                                }
                                #endregion
                                #region else if (dvAttnProcData[0]["InTime"].ToString() == "" && (sInTime != "00:00:00")) ---return bToReprocess = true;
                                else if (dvAttnProcData[0]["InTime"].ToString() == "" && (sInTime != "00:00:00"))
                                {
                                    bToReprocess = true;
                                }

                                #endregion


                                #endregion
                                bToReprocess = true;//always
                                #region bAttdnProc

                                //////Modify Date: 10-May-2018
                                //if (sInTime == "00:00:00" & sInTimeTmp == "00:00:00" & sDayStatus == "")
                                if (sInTime == "00:00:00" & sInTimeTmp == "00:00:00" & sDayStatus == "" & Convert.ToDateTime(System.DateTime.Now) > Convert.ToDateTime(pDate + " " + sOrgOfficeInTime/*sOfficeInTime*/))
                                {
                                    sDayStatus = "A";
                                    bAttdnProc = true;
                                }
                                //else if (sDayStatus == "")
                                //{
                                //    bAttdnProc = false;
                                //}

                                #endregion


                                bool _IsOutToBlank = false;
                                if (bAttdnProc == true && bToReprocess == true)
                                {
                                    var sOutTime = "00:00:00";
                                    if (dvAttnProcData[0]["OutTime"].ToString().Trim() != "")
                                    {
                                        string extOutTime = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                        string extOutTimeDate = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("dd-MMM-yyyy HH:mm:ss");
                                        sOutTime = extOutTime;
                                        if ((sInTime != "00:00:00") & (extOutTime != "00:00:00") & (Convert.ToDateTime(_in_Date + " " + sInTime) > Convert.ToDateTime(extOutTimeDate)))
                                        {
                                            sOutTime = "00:00:00";
                                            _IsOutToBlank = true;
                                        }

                                        if ((sInTime != "00:00:00") & (extOutTime != "00:00:00") & (Convert.ToDateTime(_in_Date + " " + sInTime) > Convert.ToDateTime(extOutTimeDate)) & sDayStatus == "")
                                        {
                                            sInTime = "00:00:00";
                                            sInTimeRowID = "";
                                            //sInTimeRowPunch= "00:00:00";
                                            sDayStatus = "A";
                                        }
                                    }

                                    //var sOutTime = "00:00:00";
                                    //if (dvAttnProcData[0]["OutTime"].ToString().Trim() != "")
                                    //{
                                    //    string extOutTime = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("dd-MMM-yyyy HH:mm:ss");
                                    //    sOutTime = extOutTime;
                                    //    if ((sInTime != "00:00:00") & (extOutTime != "00:00:00") & (Convert.ToDateTime(sDate + " " + sInTime) > Convert.ToDateTime(extOutTime)) & sDayStatus == "")
                                    //    {
                                    //        sInTime = "00:00:00";
                                    //        sInTimeRowID = "";
                                    //        sDayStatus = "A";
                                    //    }
                                    //}

                                    #region Manual Attendance

                                    dvMnAttData.Table = dtMnAttData;
                                    dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + pDate.Trim() + "'";
                                    if (dvMnAttData.Count > 0)
                                    {
                                        if (dvMnAttData[0].Row["DayStatus"].ToString().Trim() == "")
                                        {
                                            if (dvMnAttData[0].Row["InTime"].ToString().Trim() != "")
                                            {
                                                if (Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("HH:mm:ss") != "00:00:00")
                                                {
                                                    sInTime = Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                    _in_Date = Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                                    bManualInTime = true;
                                                }
                                            }
                                            //kabir 
                                            if (dvMnAttData[0].Row["OutTime"].ToString().Trim() == "")
                                            {
                                                if (dvMnAttData[0].Row["DayStatus"].ToString().Trim() != "")
                                                {
                                                    sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();
                                                    bManualDayStatus = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();
                                            bManualDayStatus = true;
                                        }
                                    }
                                    else
                                    {
                                        if (bManualInTime)
                                        {
                                            bManualInTime = false;
                                            //sInTime = "00:00:00";
                                            //sDayStatus = "A";
                                        }
                                    }

                                    if (bManualInTime == true)
                                    {
                                        GetDayStatus(true, pDate, sDayType, _maxLateTimeDate, sOfficeStartTime, sOfficeInTime, sLogDownLoadNum, dtRawData, _in_Date, ref sInTime, out sDayStatus, out bMoreInMarg);
                                        sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                                    }

                                    #endregion Manual Attendance 

                                    #region Least
                                    drAttnProcData = dvAttnProcData[0].Row;
                                    drAttnProcData.BeginEdit();
                                    if (sDayStatus == "LV" || sDayStatus == "LVA" || sDayStatus == "WLV" || sDayStatus == "HLV"
                                        || sDayStatus == "LVL" || sDayStatus == "LVP" || sDayStatus == "WLVP" || sDayStatus == "WLVL"
                                        || sDayStatus == "HLVP" || sDayStatus == "HLVL")
                                    {
                                        sDayStatus = "LV";
                                    }
                                    else
                                    {
                                        //if (IsHalfDayLeave == false)
                                        //{
                                        //    sLvTrans = "";
                                        //}
                                        if (_LeaveDuration == 0)
                                        {
                                            sLvTrans = "";
                                        }
                                    }

                                    //by Kabir to remove WLV/HLV
                                    if (sDayStatus == "HLV")
                                    {
                                        sDayStatus = "LV";
                                    }
                                    if (sDayStatus == "WLV")
                                    {
                                        sDayStatus = "LV";
                                    }
                                    //if (sDayStatus == "HA")
                                    //{
                                    //    sDayStatus = "A";
                                    //}
                                    //if (sDayStatus == "WA")
                                    //{
                                    //    sDayStatus = "A";
                                    //}
                                    //by monir to remove WA/HA
                                    //if (sDayType == "NW")
                                    //{
                                    //    if (sDayStatus == "WA" || sDayStatus == "HA")
                                    //    {
                                    //        sDayStatus = sDayStatus.Substring(1);
                                    //    }
                                    //}

                                    //string _DS_temp = sDayStatus;
                                    bool IsShortLeave = false;
                                    bool IsStatusChanged = false;
                                    string _DayStatus = "";
                                    bool IsReversed = false;
                                    int CountShortLeave = 0;
                                    #endregion

                                    #region ShortLeaveHalfDayAbsent
                                    //#if DEBUG

                                    ParaShortLeaveHalfDayAbsent objSLHD = new global::ParaShortLeaveHalfDayAbsent();
                                    #region set value
                                    objSLHD.sInTime = sInTime;
                                    objSLHD.sOutTime = sOutTime;
                                    objSLHD.sWorkingDate = sWorkingDate;
                                    objSLHD.sDate = pDate;
                                    objSLHD.ManualDate = _in_Date;
                                    objSLHD._ShiftDft = _ShiftDft;
                                    objSLHD.DayStatus = _DayStatus;
                                    objSLHD.IsShortLeave = IsShortLeave;
                                    objSLHD.IsStatusChanged = IsStatusChanged;
                                    objSLHD.IsReversed = IsReversed;
                                    objSLHD.CountShortLeave = CountShortLeave;
                                    objSLHD.IsShortLeaveAllowed = _shortleave_setting.IsShortLeaveAllowed;
                                    objSLHD.IsHalfDayPresentAllowed = _shortleave_setting.IsHalfDayPresentAllowed;
                                    objSLHD.IsTowShortLeaveAllowedInaDay = _shortleave_setting.IsTowShortLeaveAllowedInaDay;
                                    objSLHD.MaxShortLeaveInaMonth = _shortleave_setting.MaxShortLeaveInaMonth;
                                    objSLHD.IsOTOverHalfDay = IsOToverHalfDay;
                                    objSLHD.PaidHours = _PaidHours;
                                    objSLHD.IsOTentitled = bOTEntitle;
                                    #endregion

                                    ShortLeaveHalfDayAbsent(objSLHD);
                                    // ShortLeaveHalfDayAbsent(sInTime, sOutTime, sWorkingDate, sDate, _ShiftDft, IsOToverHalfDay,
                                    //out _DayStatus, out IsShortLeave, out IsStatusChanged, out IsReversed, out CountShortLeave);
                                    _DayStatus = objSLHD.DayStatus;
                                    IsShortLeave = objSLHD.IsShortLeave;
                                    IsStatusChanged = objSLHD.IsStatusChanged;
                                    IsReversed = objSLHD.IsReversed;
                                    CountShortLeave = objSLHD.CountShortLeave;
                                    //#endif 
                                    #endregion

                                    ParaAttendance _paraA = new global::ParaAttendance();
                                    #region set value

                                    _paraA.OPN_FLAG = "EDIT";
                                    _paraA.GroupId = GroupSysID;
                                    _paraA.sType = "IN";
                                    _paraA.sEmpSystemID = sEmpSysID;
                                    _paraA.sPlantID = sPlantID;
                                    _paraA.sWorkingDate = sWorkingDate.Trim();
                                    _paraA.shiftSystemID = sShiftSystemID;
                                    _paraA.sDate = pDate;
                                    _paraA.InDate = _in_Date;
                                    _paraA.IsOutNUll = _IsOutToBlank;
                                    _paraA.sTime = sInTime;
                                    _paraA.bManualTime = bManualInTime;
                                    _paraA.sRowID = sInTimeRowID;
                                    _paraA.sInRawData = sInTimeRowPunch;
                                    _paraA.sDayStatus = sDayStatus;
                                    _paraA.DayStatusInTimeOnly = sDayStatus;
                                    _paraA.bManualDayStatus = bManualDayStatus;
                                    _paraA.iOverTime = 0;
                                    _paraA.sLvTrans = sLvTrans;//
                                    _paraA.IsHalfDayLeave = IsHalfDayLeave;
                                    _paraA.LeaveDuration = _LeaveDuration;
                                    _paraA.iOverTimeIntime = 0;
                                    _paraA.IsFirstHalfLeave = IsFirstHalfLeave;
                                    _paraA.IsLWP = IsLWP;
                                    _paraA.IsOTEntitled = bOTEntitle;


                                    #region  if (IsWeekendFixedAsWeekend && (sOffDay == "W" || sOffDay == "H"))
                                    if (IsWeekendFixedAsWeekend && (sOffDay == "W" || sOffDay == "H"))
                                    {
                                        _paraA.OPN_FLAG = "EDIT";
                                        _paraA.GroupId = GroupSysID;
                                        _paraA.sType = "IN";
                                        _paraA.sEmpSystemID = sEmpSysID;
                                        _paraA.sPlantID = sPlantID;
                                        _paraA.sWorkingDate = sWorkingDate.Trim();
                                        _paraA.shiftSystemID = sShiftSystemID;
                                        _paraA.sDate = pDate;
                                        _paraA.InDate = _in_Date;
                                        //_paraA.InDate = _in_Date;
                                        // _paraA.IsOutNUll = _IsOutToBlank;
                                        _paraA.sTime = "00:00:00";
                                        _paraA.bManualTime = false;
                                        //_paraA.sRowID = null;
                                        _paraA.sRowID = sInTimeRowID;
                                        _paraA.sInRawData = sInTimeRowPunch;
                                        _paraA.sDayStatus = sOffDay;
                                        _paraA.DayStatusInTimeOnly = sOffDay;
                                        _paraA.bManualDayStatus = false;
                                        _paraA.iOverTime = 0;
                                        _paraA.sLvTrans = null;//
                                        _paraA.IsHalfDayLeave = false;//IsHalfDayLeave
                                        _paraA.iOverTimeIntime = 0;
                                        _paraA.IsFirstHalfLeave = IsFirstHalfLeave;
                                        _paraA.IsOTEntitled = bOTEntitle;
                                    }
                                    #endregion
                                    //_paraA.IsHalfDayLeave = IsStatusChanged;
                                    //_paraA.IsShortLeave = IsShortLeave;
                                    //_paraA.IsReversed = IsReversed;
                                    //_paraA.CountedShortLeave = CountShortLeave; 
                                    #endregion


                                    if (ds.Length > 0)
                                    {
                                        if (flag.ToUpper() == "ORIGINAL")
                                        {
                                            if (_paraA.sDayStatus == "P" || _paraA.sDayStatus == "L" || _paraA.sDayStatus == "WP" || _paraA.sDayStatus == "WL" || _paraA.sDayStatus == "WA" || _paraA.sDayStatus == "HP" || _paraA.sDayStatus == "HL" || _paraA.sDayStatus == "HA")//PW,CW/AW/AH will b applicable only when daystatus is P or L
                                            {
                                                _paraA.sDayStatus = ds;
                                            }
                                        }

                                        if (flag.ToUpper() != "ORIGINAL")
                                        {
                                            if (_paraA.sDayStatus == "P" || _paraA.sDayStatus == "L" || _paraA.sDayStatus == "WP" || _paraA.sDayStatus == "WL" || _paraA.sDayStatus == "WA" || _paraA.sDayStatus == "HP" || _paraA.sDayStatus == "HL" || _paraA.sDayStatus == "HA")//PW,CW/AW/AH will b applicable only when daystatus is P or L
                                            {
                                                _paraA.sDayStatus = ds + "P";
                                                //if(_paraA.sDayStatus == "WA")
                                                //{
                                                //    _paraA.sDayStatus = "WL";
                                                //}
                                                //if (_paraA.sDayStatus == "HA")
                                                //{
                                                //    _paraA.sDayStatus = "HL";
                                                //}
                                            }
                                            else
                                            {
                                                _paraA.sDayStatus = ds;
                                            }
                                        }
                                    }


                                    if (bManualDayStatus == false)
                                    {
                                        if (sOffDay == "W")
                                        {
                                            DataView dvDT = new DataView(dsDayType.Tables[0]);
                                            dvDT.RowFilter = "(Category='Late' or Category='Half Day') and daytype='" + _paraA.sDayStatus + "'";
                                            if (dvDT.Count > 0)
                                            {
                                                _paraA.sDayStatus = "WL";
                                            }
                                            else if (_paraA.sDayStatus == "WA")
                                            {
                                                _paraA.sDayStatus = "WL";
                                            }
                                        }

                                        if (sOffDay == "H")
                                        {
                                            DataView dvDT = new DataView(dsDayType.Tables[0]);
                                            dvDT.RowFilter = "(Category='Late' or Category='Half Day') and daytype='" + _paraA.sDayStatus + "'";
                                            if (dvDT.Count > 0)
                                            {
                                                _paraA.sDayStatus = "HL";
                                            }
                                            else if (_paraA.sDayStatus == "HA")
                                            {
                                                _paraA.sDayStatus = "HL";//
                                            }
                                        }
                                    }

                                    UpdateAttdnData(_paraA, ref drAttnProcData);
                                    drAttnProcData.EndEdit();
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region add
                            if (_bReProc == false)
                            {////If Attendance Process Class Not Call From Attendance Process Option Then No Need To Insert In AttdnProcessData 
                                ////////Modify Date: 10-May-2018
                                //if (sInTime == "00:00:00" & sDayStatus == "")
                                if (sInTime == "00:00:00" & sDayStatus == "" & Convert.ToDateTime(System.DateTime.Now) > Convert.ToDateTime(pDate + " " + sOrgOfficeInTime/*sOfficeInTime*/))
                                {
                                    sDayStatus = "A";
                                    bAttdnProc = true;
                                }
                                else if (sDayStatus == "")
                                {
                                    bAttdnProc = false;
                                }
                                if (bAttdnProc == true)
                                {
                                    #region Manual Attendance

                                    dvMnAttData.Table = dtMnAttData;
                                    dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + pDate.Trim() + "'";
                                    if (dvMnAttData.Count > 0)
                                    {
                                        if (dvMnAttData[0].Row["InTime"].ToString().Trim() != "")
                                        {
                                            if (Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("HH:mm:ss") != "00:00:00")
                                            {
                                                sInTime = Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                _in_Date = Convert.ToDateTime(dvMnAttData[0].Row["InTime"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                                //bManualInTime = true;
                                                bManualInTime = true;
                                            }
                                        }
                                        //sInTimeRowID = "";
                                        //sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();

                                        if (dvMnAttData[0].Row["InTime"].ToString().Trim() == "" && dvMnAttData[0].Row["OutTime"].ToString().Trim() == "")
                                        {
                                            if (dvMnAttData[0].Row["DayStatus"].ToString().Trim() != "")
                                            {
                                                sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();
                                                bManualDayStatus = true;
                                            }
                                        }
                                    }//count>0

                                    //*****************************21-May-2018********************************************
                                    if (bManualInTime == true)
                                    {
                                        GetDayStatus(true, pDate, sDayType, _maxLateTimeDate, sOfficeStartTime, sOfficeInTime, sLogDownLoadNum, dtRawData, _in_Date, ref sInTime, out sDayStatus, out bMoreInMarg);
                                        sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                                    }
                                    //*****************************21-May-2018******************************************** 

                                    #endregion Manual Attendance

                                    drAttnProcData = dtAttnProcData.NewRow();

                                    ParaAttendance _paraA = new global::ParaAttendance();
                                    #region para
                                    _paraA.OPN_FLAG = "ADDNEW";
                                    _paraA.GroupId = GroupSysID;
                                    _paraA.sType = "IN";
                                    _paraA.sEmpSystemID = sEmpSysID;
                                    _paraA.sPlantID = sPlantID;
                                    _paraA.sWorkingDate = sWorkingDate.Trim();
                                    _paraA.shiftSystemID = sShiftSystemID;
                                    _paraA.sDate = pDate.Trim();
                                    _paraA.sTime = sInTime;
                                    _paraA.bManualTime = bManualInTime;
                                    _paraA.sRowID = sInTimeRowID;
                                    _paraA.sInRawData = sInTimeRowPunch;
                                    _paraA.sDayStatus = sDayStatus;
                                    _paraA.bManualDayStatus = bManualDayStatus;
                                    _paraA.iOverTime = 0;
                                    _paraA.sLvTrans = sLvTrans;
                                    _paraA.iOverTimeIntime = 0;
                                    _paraA.IsHalfDayLeave = IsHalfDayLeave;
                                    _paraA.IsOTEntitled = bOTEntitle;
                                    _paraA.InDate = _in_Date;

                                    if (IsWeekendFixedAsWeekend && (sOffDay == "W" || sOffDay == "H"))
                                    {
                                        _paraA.OPN_FLAG = "ADDNEW";
                                        _paraA.GroupId = GroupSysID;
                                        _paraA.sType = "IN";
                                        _paraA.sEmpSystemID = sEmpSysID;
                                        _paraA.sPlantID = sPlantID;
                                        _paraA.sWorkingDate = sWorkingDate.Trim();
                                        _paraA.shiftSystemID = sShiftSystemID;
                                        _paraA.sDate = pDate.Trim();
                                        _paraA.sTime = "00:00:00";
                                        //_paraA.sTime = sInTime;
                                        _paraA.bManualTime = false;
                                        _paraA.sRowID = sInTimeRowID;
                                        _paraA.sInRawData = sInTimeRowPunch;
                                        _paraA.sDayStatus = sOffDay;
                                        _paraA.DayStatusInTimeOnly = sOffDay;
                                        _paraA.bManualDayStatus = false;
                                        _paraA.iOverTime = 0;
                                        _paraA.sLvTrans = null;
                                        _paraA.iOverTimeIntime = 0;
                                        _paraA.IsHalfDayLeave = false;
                                        _paraA.IsOTEntitled = bOTEntitle;
                                        _paraA.InDate = _in_Date;
                                    }

                                    if (ds.Length > 0)
                                    {
                                        if (flag.ToUpper() == "ORIGINAL")
                                        {
                                            if (_paraA.sDayStatus == "P" || _paraA.sDayStatus == "L" || _paraA.sDayStatus == "WP" || _paraA.sDayStatus == "WL" || _paraA.sDayStatus == "WA" || _paraA.sDayStatus == "HP" || _paraA.sDayStatus == "HL" || _paraA.sDayStatus == "HA")//PW,CW/AW/AH will b applicable only when daystatus is P or L
                                            {
                                                _paraA.sDayStatus = ds;
                                            }
                                        }

                                        if (flag.ToUpper() != "ORIGINAL")
                                        {
                                            if (_paraA.sDayStatus == "P" || _paraA.sDayStatus == "L" || _paraA.sDayStatus == "WP" || _paraA.sDayStatus == "WL" || _paraA.sDayStatus == "WA" || _paraA.sDayStatus == "HP" || _paraA.sDayStatus == "HL" || _paraA.sDayStatus == "HA")//PW,CW/AW/AH will b applicable only when daystatus is P or L
                                            {
                                                _paraA.sDayStatus = ds + "P";
                                            }
                                            else
                                            {
                                                _paraA.sDayStatus = ds;
                                            }
                                        }
                                    }

                                    if (bManualDayStatus == false)
                                    {
                                        if (sOffDay == "W")
                                        {
                                            DataView dvDT = new DataView(dsDayType.Tables[0]);
                                            dvDT.RowFilter = "(Category='Late' or Category='Half Day') and daytype='" + _paraA.sDayStatus + "'";
                                            if (dvDT.Count > 0)
                                            {
                                                _paraA.sDayStatus = "WL";
                                            }
                                            else if (_paraA.sDayStatus == "WA")
                                            {
                                                _paraA.sDayStatus = "WL";
                                            }
                                        }

                                        if (sOffDay == "H")
                                        {
                                            DataView dvDT = new DataView(dsDayType.Tables[0]);
                                            dvDT.RowFilter = "(Category='Late' or Category='Half Day') and daytype='" + _paraA.sDayStatus + "'";
                                            if (dvDT.Count > 0)
                                            {
                                                _paraA.sDayStatus = "HL";
                                            }
                                            else if (_paraA.sDayStatus == "HA")
                                            {
                                                _paraA.sDayStatus = "HL";
                                            }
                                        }
                                    }

                                    UpdateAttdnData(_paraA, ref drAttnProcData);


                                    #endregion

                                    //UpdateAttdnData("ADDNEW", GroupSysID, "IN", sEmpSysID, sPlantID, sWorkingDate.Trim(), sShiftSystemID, sDate.Trim(), sInTime, bManualInTime, sInTimeRowID, sDayStatus, bManualDayStatus, 0, sLvTrans, ref drAttnProcData);
                                    dtAttnProcData.Rows.Add(drAttnProcData);
                                }//bAttdnProc == true
                            }//_bReProc == false 
                            #endregion
                        }
                        dvAttnProcData.RowFilter = null;
                        #endregion

                        #region Rest
                        bool IsWeekendAllowed = true;
                        if (IsWeekendFixedAsWeekend && (sOffDay == "W" || sOffDay == "H"))//i.e. at weekend or in holiday rest is not allowed
                        {
                            IsWeekendAllowed = false;
                        }
                        if (string.IsNullOrEmpty(_RestPK) == false && bAttnIsLock == false && IsWeekendAllowed)
                        {
                            dvAttnProcData.Table = dtAttnProcData;
                            dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + pDate.Trim() + "'";
                            if (dvAttnProcData.Count > 0)
                            {
                                drAttnProcData = dvAttnProcData[0].Row;
                                drAttnProcData.BeginEdit();
                                drAttnProcData["DayStatus"] = "RST";
                                drAttnProcData["AttendanceRestDetailId"] = _RestPK;
                                if (bOTEntitle)
                                {
                                    drAttnProcData["OTHr"] = _PaidHours * (-60);
                                }
                                else
                                {
                                    drAttnProcData["OTHr"] = _PaidHours * (0);
                                }
                                drAttnProcData.EndEdit();
                            }
                        }//rest found and att is not locked 
                        #endregion

                        //_ODPK
                        #region OD
                        bool IsWeekendAllowed_OD = true;
                        if (IsWeekendFixedAsWeekend && (sOffDay == "W" || sOffDay == "H"))//i.e. at weekend or in holiday rest is not allowed
                        {
                            IsWeekendAllowed_OD = false;
                        }

                        if (string.IsNullOrEmpty(_ODPK) == false && bAttnIsLock == false && IsWeekendAllowed_OD)
                        {
                            dvAttnProcData.Table = dtAttnProcData;
                            dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + pDate.Trim() + "'";
                            if (dvAttnProcData.Count > 0)
                            {
                                drAttnProcData = dvAttnProcData[0].Row;
                                drAttnProcData.BeginEdit();
                                drAttnProcData["DayStatus"] = "OD";
                                drAttnProcData["IsOD"] = true;
                                drAttnProcData.EndEdit();
                            }
                        }//rest found and att is not locked 
                        #endregion

                        #region OD Details Update
                        if (bAttnIsLock == false)
                        {
                            dvODDetail = new DataView();
                            dvODDetail.Table = dtODDetail;
                            dvODDetail.RowFilter = "Id = '" + _ODPK + "'";
                            if (dvODDetail.Count > 0)
                            {
                                drODDetail = dvODDetail[0].Row;
                                drODDetail.BeginEdit();
                                //bool availStatus = Convert.ToBoolean(drODDetail["IsAvailed"]);
                                drODDetail["IsAvailed"] = true;
                                drODDetail.EndEdit();
                            }
                        }
                        #endregion OD Details Update

                        #region Leave Transaction Details Update
                        if (bAttnIsLock == false)
                        {
                            dvLvTransDtl = new DataView();
                            dvLvTransDtl.Table = dtLvTransDtl;
                            dvLvTransDtl.RowFilter = "LvTrnsSystemID = '" + sLvTnsDtlSysID + "'";

                            if (dvLvTransDtl.Count > 0)
                            {
                                drLvTransDtl = dvLvTransDtl[0].Row;
                                drLvTransDtl.BeginEdit();
                                string dayType = drLvTransDtl["DayType"].ToString();
                                bool availStatus = Convert.ToBoolean(drLvTransDtl["IsAvailed"]);
                                if (dayType == "W")
                                {
                                    if (sDayStatus == "WLV")
                                    {
                                        drLvTransDtl["IsAvailed"] = true;
                                    }
                                    else
                                    {
                                        drLvTransDtl["IsAvailed"] = false;
                                    }
                                }
                                else if (dayType == "H")
                                {
                                    if (sDayStatus == "HLV")
                                    {
                                        drLvTransDtl["IsAvailed"] = true;
                                    }
                                    else
                                    {
                                        drLvTransDtl["IsAvailed"] = false;
                                    }
                                }
                                else if (dayType == "NW" && availStatus == false)
                                {
                                    if (sDayStatus == "LV" || sDayStatus == "LVA")
                                    {
                                        drLvTransDtl["IsAvailed"] = true;
                                    }
                                    else
                                    {
                                        drLvTransDtl["IsAvailed"] = false;
                                    }

                                }
                                else
                                {
                                    drLvTransDtl["IsAvailed"] = true;
                                }
                                drLvTransDtl.EndEdit();

                                dvLvAvail = new DataView();
                                dvLvAvail.Table = dtLvAvail;
                                dvLvAvail.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                                if (dvLvAvail.Count > 0)
                                {
                                    for (int LvAllo = 0; LvAllo < dvLvAvail.Count; LvAllo++)
                                    {
                                        sLvPolDtlSysID = dvLvAvail[LvAllo]["LvPolDtlSystemID"].ToString().Trim();
                                        sLvAvailed = Convert.ToInt32(dvLvAvail[LvAllo]["Availed"].ToString().Trim());
                                    }
                                }
                            }
                        }
                        #endregion Leave Transaction Details Update
                    }

                    //clsStaticInfo objs = new clsStaticInfo();

                    SaveDataSets(dsRawData, dsAttnProcData, dsLvTransDtl);

                    //ServiceReference1.HREndpointServiceClient client = new ServiceReference1.HREndpointServiceClient();
                    //client.sendAllNotification(clsRegister.NotificationType.Attendance.ToString());
                }

                bValid = true;
                return bValid;
            }
            catch (Exception ex)
            {
                throw ex;
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

                dsLvAvail = null;
                dtLvAvail = null;
                dvLvAvail = null;

                dsOffDay = null;

                sOfficeStartTime = string.Empty;
                sOfficeInTime = string.Empty;
                sLogDownLoadNum = string.Empty;
                sEmpSysID = string.Empty;

                sInTime = string.Empty;
                //sInTimeRowID = string.Empty;
                sInTimeTmp = string.Empty;
                //sInTimeRowIDTmp = string.Empty;
                sDayStatus = string.Empty;
                sLvTrans = string.Empty;
                sOffDay = string.Empty;
                sLvTnsDtlSysID = string.Empty;
                sLvPolDtlSysID = string.Empty;

                #endregion
            }
        }//End Function 
        void GetRestPK(DataSet dsRest, string empPK, out string restpk)
        {
            restpk = string.Empty;
            try
            {
                DataView dvRest = new DataView(dsRest.Tables[0]);
                dvRest.RowFilter = "EmpSystemId = '" + empPK + "'";
                if (dvRest.Count > 0)
                {
                    restpk = dvRest[0]["RestId"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetODPK(DataSet dsOD, string empPK, out string restpk)
        {
            restpk = string.Empty;
            try
            {
                DataView dvRest = new DataView(dsOD.Tables[0]);
                dvRest.RowFilter = "EmpSystemId = '" + empPK + "'";
                if (dvRest.Count > 0)
                {
                    restpk = dvRest[0]["OdId"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool InProcess_SNA(string GroupSysID, string _plantid, string sAttnDate, string strYrSystemID, bool radDwLdEnrollID, string strYrFromDate, string strYrToDate, bool _bReProc)//1
        {
            #region Declare variables

            //DataSet dsDayType = null;
            //DataTable dtDayType = null;
            //DataView dvDayType = null;

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

            //DataSet dsLvTransDtl = null;
            //DataTable dtLvTransDtl = null;
            //DataRow drLvTransDtl = null;
            //DataView dvLvTransDtl = null;

            //DataSet dsLvTrans = null;
            //DataTable dtLvTrans = null;
            //DataView dvLvTrans = null;

            //DataSet dsLvAvail = null;
            //DataTable dtLvAvail = null;
            //DataView dvLvAvail = null;

            //DataSet dsOffDay = null;
            //DataTable dtOffDay = null;
            //DataView dvOffDay = null;

            //string sOfficeStartTime = "";
            //string sOfficeInTime = "";
            //string sOrgOfficeInTime = "";
            string sLogDownLoadNum = "";

            string sEmpSysID = "";
            string sEmpCode = "";
            //string sShiftSystemID = "";
            string sLastProcDate = "";
            string sPlantID = "";

            string sInTime = "";
            string sInTimeRowID = string.Empty;
            int iDeviceID = 0;
            string sInTimeTmp = "";
            string sInTimeRowIDTmp = string.Empty;
            //string sDayStatusTmp = "";
            int iDeviceIDTmp = 0;
            string sDayStatus = "SNA";
            //string sPrvDayStatus = "";
            //string sLvTrans = "";
            //string sOffDay = "";
            //string sComHoliDay = "";
            //string sLvTnsDtlSysID = "";
            //string sLvPolDtlSysID = "";
            //int sLvAvailed = 0;
            //int iInTimeStartMargin = 0;
            //int _absentEndMergin = 0;
            //string _maxLateTime = string.Empty;

            //string sDayType = "";

            //string sBreakStratTime = "";
            //string sBreakEndTime = "";

            string sDate = "";
            string sPrvDate = "";
            string sWorkingDate = "";
            //bool bMoreInMarg = false;

            bool bValid = false;
            bool bAttdnProc = true;

            #endregion local variables

            try
            {
                #region DataSet

                sDate = sAttnDate.Trim();
                sPrvDate = (Convert.ToDateTime(sAttnDate.Trim()).AddDays(-1)).ToString("dd-MMM-yyyy");
                sWorkingDate = sAttnDate.Trim();

                //DataSet dsLTDIds = null;
                //GetLeaveTrnDetailIds(GroupSysID, _plantid, sEmpSystemIDColl.Trim(), sAttnDate, out dsLTDIds);
                //string _LTDIds = "''";
                //for (int i = 0; i < dsLTDIds.Tables[0].Rows.Count; i++)
                //{
                //    if (_LTDIds == "''")
                //    {
                //        _LTDIds = "'" + dsLTDIds.Tables[0].Rows[i]["SystemID"].ToString() + "'";
                //    }
                //    else
                //    {
                //        _LTDIds += ",'" + dsLTDIds.Tables[0].Rows[i]["SystemID"].ToString() + "'";
                //    }
                //}

                //GetDayType(out dsDayType);
                //dtDayType = dsDayType.Tables[0];
                //dvDayType = new DataView();

                GetAttdnRawDataForAttdnProc(GroupSysID.Trim(), sDate.Trim(), "IN", out dsRawData);
                dtRawData = dsRawData.Tables[0];

                GetAttdnProcData(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sPrvDate.Trim(), sDate.Trim(), out dsAttnProcData);
                dtAttnProcData = dsAttnProcData.Tables[0];

                GetEmployeeInfo_SNA(GroupSysID.Trim(), _plantid, sEmpSystemIDColl.Trim(), sDate.Trim(), out dsEmpInfo);

                GetAttdnManualData(GroupSysID.Trim(), _plantid, sEmpSystemIDColl.Trim(), sAttnDate.Trim(), out dsMnAttData);
                dtMnAttData = dsMnAttData.Tables[0];
                dvMnAttData = new DataView();

                //GetLeaveTransactionDetails(sDate.Trim(), _LTDIds, out dsLvTransDtl);
                //dtLvTransDtl = dsLvTransDtl.Tables[0];

                //GetLeaveTransactionInfo(GroupSysID.Trim(), _plantid, sEmpSystemIDColl.Trim(), sDate.Trim(), out dsLvTrans);
                //dtLvTrans = dsLvTrans.Tables[0];

                //GetAvailedLvInfo(GroupSysID.Trim(), _plantid, strYrSystemID.Trim(), strYrFromDate.Trim(), strYrToDate.Trim(), out dsLvAvail);
                //dtLvAvail = dsLvAvail.Tables[0];

                //GetAllPlantOffDayInformation(GroupSysID.Trim(), _plantid, sDate.Trim(), out dsOffDay);
                //dtOffDay = dsOffDay.Tables[0];

                #endregion DataSet

                if (dsEmpInfo.Tables[0].Rows.Count > 0)
                {
                    for (int EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                    {
                        bAttdnProc = true;
                        //sComHoliDay = "";
                        //sOffDay = "";
                        sLastProcDate = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["LastWorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                        sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();
                        if (sEmpSysID == "1800352")
                        {
                            //sOffDay = "";
                        }
                        sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();
                        sEmpCode = dsEmpInfo.Tables[0].Rows[EmpCount]["EmployeeCode"].ToString();
                        //sOfficeStartTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeStartTime"].ToString().Trim()).ToString("HH:mm:ss");
                        //sOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeTime"].ToString().Trim()).AddMinutes(1).AddSeconds(-1).ToString("HH:mm:ss");
                        //sOrgOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                        //iInTimeStartMargin = Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["InTimeStartMargin"].ToString());

                        //_absentEndMergin = Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["AbsentEndMargin"].ToString());
                        //_maxLateTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["InTime"].ToString().Trim()).AddMinutes(_absentEndMergin).ToString("HH:mm:ss");

                        //sShiftSystemID = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftSystemID"].ToString();
                        //sDayType = dsEmpInfo.Tables[0].Rows[EmpCount]["DayType"].ToString();

                        //sBreakStratTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakStratTime"].ToString().Trim()).ToString("HH:mm:ss");
                        //sBreakEndTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakEndTime"].ToString().Trim()).ToString("HH:mm:ss");

                        //sOffDay = GetOffDay(dtOffDay, sPlantID, sDayType);

                        sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemId"].ToString();

                        #region Find InTime from raw Data Table

                        sInTime = "00:00:00";
                        sInTimeRowID = string.Empty;
                        iDeviceID = 0;
                        sInTimeTmp = "00:00:00";
                        sInTimeRowIDTmp = string.Empty;
                        //sDayStatusTmp = "";
                        iDeviceIDTmp = 0;
                        sDayStatus = "P";
                        //sPrvDayStatus = "";
                        //sLvTrans = "";
                        //sLvPolDtlSysID = "";
                        //sLvAvailed = 0;
                        //bMoreInMarg = false;

                        dvRawData = new DataView();
                        dvRawData.Table = dtRawData;
                        bool HasRawData = false;
                        dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                        if (sLogDownLoadNum == "2018-10538")
                        {

                        }
                        if (dvRawData.Count > 0)
                        {
                            HasRawData = true;
                            for (int RData = 0; RData < dvRawData.Count; RData++)
                            {
                                #region loop
                                if (dvRawData[RData]["PTime"].ToString() != "")
                                {
                                    string sPInTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    if (sInTime == "00:00:00" || Convert.ToDateTime(sPInTime.Trim()) < Convert.ToDateTime(sInTime.Trim()))
                                    {
                                        //if (Convert.ToDateTime(sPInTime.Trim()) >= Convert.ToDateTime(sOfficeStartTime))
                                        //{
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
                                        //}
                                    }
                                }

                                drRawData = dvRawData[RData].Row;
                                drRawData.BeginEdit();
                                drRawData["ProcessedFlag"] = 1;
                                drRawData.EndEdit();
                                #endregion
                            }
                        }
                        else
                        {
                            HasRawData = false;
                        }

                        #endregion Find InTime from raw Data Table

                        // GetDayStatus(sDayType, _maxLateTime, sOfficeStartTime, sOfficeInTime, sBreakStratTime, sBreakEndTime, iDeviceID, sLogDownLoadNum, dtRawData, ref sInTime, out sDayStatus, out bMoreInMarg);



                        //if (LVDayStatus == "W" || LVDayStatus == "H" || LVDayStatus == "HW" || LVDayStatus == "WH")
                        //{
                        //    LVDayStatus = "";
                        //}

                        //sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                        //****************************************************************************************************************************************
                        if (HasRawData)
                        {
                            bool bAttnIsLock = false;
                            bool bManualInTime = false;
                            bool bManualDayStatus = false;

                            dvAttnProcData = new DataView();
                            dvAttnProcData.Table = dtAttnProcData;
                            dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                            if (dvAttnProcData.Count > 0)
                            {
                                #region edit
                                bAttnIsLock = Convert.ToBoolean(dvAttnProcData[0].Row["IsLock"].ToString());

                                if (bAttnIsLock == false)
                                {
                                    if (dvAttnProcData[0]["InTime"].ToString() != "")
                                    {
                                        sInTimeTmp = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                        sInTimeRowIDTmp = dvAttnProcData[0]["InTimeRowID"].ToString().Trim();
                                        //sDayStatusTmp = dvAttnProcData[0]["DayStatus"].ToString().Trim();
                                    }

                                    if ((sInTimeTmp != "00:00:00") & (sInTime == "00:00:00"))
                                    {
                                        sInTime = sInTimeTmp;
                                        sInTimeRowID = sInTimeRowIDTmp;
                                        //sDayStatus = sDayStatusTmp;

                                        //if (sInTime != "00:00:00" && (sDayStatus == "A" || sDayStatus == "L" || sDayStatus == "P"))
                                        //{
                                        //    GetDayStatus(sDayType, _maxLateTime, sOfficeStartTime, sOfficeInTime, sBreakStratTime, sBreakEndTime, iDeviceID, sLogDownLoadNum, dtRawData, ref sInTime, out sDayStatus, out bMoreInMarg);
                                        //    sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                                        //}
                                    }

                                    //sPrvDayStatus = dvAttnProcData[0]["DayStatus"].ToString().Trim();
                                    //////Modify Date: 10-May-2018
                                    //if (sInTime == "00:00:00" & sInTimeTmp == "00:00:00" & sDayStatus == "")
                                    //if (sInTime == "00:00:00" & sInTimeTmp == "00:00:00" & sDayStatus == "" & Convert.ToDateTime(System.DateTime.Now) > Convert.ToDateTime(sDate + " " + sOrgOfficeInTime/*sOfficeInTime*/))
                                    //{
                                    //    sDayStatus = "A";
                                    bAttdnProc = true;
                                    //}
                                    //else if (sDayStatus == "")
                                    //{
                                    //    bAttdnProc = false;
                                    //}
                                    if (bAttdnProc == true)
                                    {
                                        if ((sInTimeTmp != "00:00:00") & (Convert.ToDateTime(sInTime) > Convert.ToDateTime(sInTimeTmp)))
                                        {
                                            sInTime = sInTimeTmp;
                                            sInTimeRowID = sInTimeRowIDTmp;
                                            //sDayStatus = sDayStatusTmp;
                                        }

                                        if (dvAttnProcData[0]["OutTime"].ToString().Trim() != "")
                                        {
                                            string extOutTime = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                            if ((sInTime != "00:00:00") & (extOutTime != "00:00:00") & (Convert.ToDateTime(sInTime) > Convert.ToDateTime(extOutTime)))
                                            {
                                                sInTime = "00:00:00";
                                                sInTimeRowID = "";
                                                //sDayStatus = "A";
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
                                                //bManualInTime = true;
                                            }
                                            sInTimeRowID = "";
                                            //sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();

                                            //if (dvMnAttData[0].Row["InTime"].ToString().Trim() == "" && dvMnAttData[0].Row["OutTime"].ToString().Trim() == "")
                                            //{ bManualDayStatus = true; }
                                        }

                                        //*****************************21-May-2018********************************************
                                        //if (bManualInTime == true)
                                        //{
                                        //    GetDayStatus(sDayType, _maxLateTime, sOfficeStartTime, sOfficeInTime, sBreakStratTime, sBreakEndTime, iDeviceID, sLogDownLoadNum, dtRawData, ref sInTime, out sDayStatus, out bMoreInMarg);
                                        //    sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                                        //}
                                        //*****************************21-May-2018******************************************** 

                                        #endregion Manual Attendance

                                        drAttnProcData = dvAttnProcData[0].Row;
                                        drAttnProcData.BeginEdit();
                                        //if (sDayStatus == "LV" || sDayStatus == "LVA" || sDayStatus == "WLV" || sDayStatus == "HLV")
                                        //{
                                        //}
                                        //else
                                        //{
                                        //    sLvTrans = "";
                                        //}

                                        //by monir to remove WA/HA
                                        //if (sDayType == "NW")
                                        //{
                                        //    if (sDayStatus == "WA" || sDayStatus == "HA")
                                        //    {
                                        //        sDayStatus = sDayStatus.Substring(1);
                                        //    }
                                        //}


                                        UpdateAttdnData_SNA("EDIT", GroupSysID, "IN", sEmpSysID, sPlantID, sWorkingDate.Trim(), sDate, sInTime, bManualInTime, sInTimeRowID, sDayStatus, bManualDayStatus, 0, ref drAttnProcData);
                                        drAttnProcData.EndEdit();
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                #region NEW
                                if (_bReProc == false)
                                {////If Attendance Process Class Not Call From Attendance Process Option Then No Need To Insert In AttdnProcessData 
                                 ////////Modify Date: 10-May-2018
                                 //if (sInTime == "00:00:00" & sDayStatus == "")
                                 //if (sInTime == "00:00:00" & sDayStatus == "" & Convert.ToDateTime(System.DateTime.Now) > Convert.ToDateTime(sDate + " " + sOrgOfficeInTime/*sOfficeInTime*/))
                                 //{
                                 //sDayStatus = "A";
                                    bAttdnProc = true;
                                    //}
                                    //else if (sDayStatus == "")
                                    //{
                                    //    bAttdnProc = false;
                                    //}
                                    if (bAttdnProc == true)
                                    {
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
                                            //sDayStatus = dvMnAttData[0].Row["DayStatus"].ToString().Trim();

                                            //if (dvMnAttData[0].Row["InTime"].ToString().Trim() == "" && dvMnAttData[0].Row["OutTime"].ToString().Trim() == "")
                                            //{ bManualDayStatus = true; }
                                        }

                                        //*****************************21-May-2018********************************************
                                        //if (bManualInTime == true)
                                        //{
                                        //    GetDayStatus(sDayType, _maxLateTime, sOfficeStartTime, sOfficeInTime, sBreakStratTime, sBreakEndTime, iDeviceID, sLogDownLoadNum, dtRawData, ref sInTime, out sDayStatus, out bMoreInMarg);
                                        //    sDayStatus = sOffDay + LVDayStatus + sDayStatus;
                                        //}
                                        //*****************************21-May-2018******************************************** 

                                        #endregion Manual Attendance

                                        drAttnProcData = dtAttnProcData.NewRow();
                                        UpdateAttdnData_SNA("ADDNEW", GroupSysID, "IN", sEmpSysID, sPlantID, sWorkingDate.Trim(), sDate.Trim(), sInTime, bManualInTime, sInTimeRowID, sDayStatus, bManualDayStatus, 0, ref drAttnProcData);
                                        dtAttnProcData.Rows.Add(drAttnProcData);
                                    }
                                }//_bReProc 
                                #endregion
                            }
                        }//HasRawData               
                    }

                    //clsStaticInfo objs = new clsStaticInfo();
                    SaveDataSets(dsRawData, dsAttnProcData);
                }

                bValid = true;
                return bValid;
            }
            catch (Exception ex)
            {
                throw ex;
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
                //sOfficeStartTime = string.Empty;
                //sOfficeInTime = string.Empty;
                sLogDownLoadNum = string.Empty;
                sEmpSysID = string.Empty;

                sInTime = string.Empty;
                sInTimeRowID = string.Empty;
                sInTimeTmp = string.Empty;
                sInTimeRowIDTmp = string.Empty;

                #endregion
            }
        }//End Function 

        private bool OutProcess_SNA(string _plantId, string sAttnDate, string GroupSysID, string sEmpSystemIDColl, string sMinOT, string sFractionCalculate, bool radDwLdEnrollID)
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

            string sLogDownLoadNum = "";
            string sEmpSysID = "";
            string sPlantID = _plantId;
            string sOTStartTime = "";
            decimal iTotalOTHr = 0;

            //string sOfficeInTime = "";
            string sInTime = "";
            string sOutTime = "";
            string sOutTimeRowID = string.Empty;
            int iDeviceID = 0;
            string sOutTimeTmp = "";
            string sOutTimeRowIDTmp = string.Empty;
            int iDeviceIDTmp = 0;


            string sDate = sAttnDate;
            string sPrvDate = "";
            string sWorkingDate = "";
            bool bValid = false;

            string _MaxInTime = "00:00";
            string _BaseDate = sAttnDate;
            int _MaxWorkingHour = 13;
            #endregion local variables

            try
            {
                #region DataSet

                //get plant wise weekoff
                //get min intime of this day
                //get max hour like 13/15
                //calculate max out time for comparison at the time of finding out time

                ////get min intime of this day
                //DataSet dsMaxTime = null;
                //GetMaxInTimeByDay(GroupSysID.Trim(), sDate.Trim(), _plantId, out dsMaxTime);
                //if(dsMaxTime.Tables[0].Rows.Count>0)
                //{
                //    _MaxInTime = Convert.ToDateTime(dsMaxTime.Tables[0].Rows[0]["InTime"].ToString()).ToString("HH:mm");
                //}

                ////calculate max out time for comparison at the time of finding out time
                //string _MinDateTime = _BaseDate + " " + _MaxInTime;
                ////get max hour like 13/15 (this date will be to datetime for selecting rawdata)
                //_MinDateTime = Convert.ToDateTime(_MinDateTime).AddHours(_MaxWorkingHour).ToString("dd-MMM-yyyy HH:mm");



                sDate = sAttnDate.Trim();
                //sPrvDate = (Convert.ToDateTime(sAttnDate.Trim()).AddDays(-1)).ToString("dd-MMM-yyyy");
                sPrvDate = (Convert.ToDateTime(sAttnDate.Trim()).AddDays(-2)).ToString("dd-MMM-yyyy");

                //GetAttdnRawDataForAttdnProc_SNA(GroupSysID.Trim(), sDate.Trim(), _MinDateTime, "OUT", out dsRawData);
                GetAttdnRawDataForAttdnProc(GroupSysID.Trim(), sDate.Trim(), "OUT", out dsRawData);
                dtRawData = dsRawData.Tables[0];

                GetAttdnProcData(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sPrvDate.Trim(), sDate.Trim(), out dsAttnProcData);
                dtAttnProcData = dsAttnProcData.Tables[0];

                //GetFinalOT(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), out dsFinalOT);
                //dtFinalOT = dsFinalOT.Tables[0];

                //GetOTSlabDefineEmployee(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), out dsOTSlabEmp);
                //dtOTSlabEmp = dsOTSlabEmp.Tables[0];

                //GetOTSlabDefineGeneral(GroupSysID.Trim(), sDate.Trim(), out dsOTSlabGen);
                //dtOTSlabGen = dsOTSlabGen.Tables[0];

                GetEmployeeInfo_SNA(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sDate.Trim(), out dsEmpInfo);

                GetAttdnManualData(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sAttnDate.Trim(), out dsMnAttData);
                dtMnAttData = dsMnAttData.Tables[0];
                dvMnAttData = new DataView();

                #endregion DataSet
                bool HasRawData = false;
                if (dsEmpInfo.Tables[0].Rows.Count > 0)
                {
                    for (int EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                    {
                        sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();
                        sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();
                        //sOTStartTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OTStartTime"].ToString().Trim()).ToString("HH:mm:ss");
                        //sOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeTime"].ToString().Trim()).ToString("HH:mm:ss");
                        sInTime = "00:00:00";
                        //bOTEntitle = Convert.ToBoolean(dsEmpInfo.Tables[0].Rows[EmpCount]["IsOTEntitle"].ToString());
                        iTotalOTHr = 0;
                        //sOTDayType = "";
                        //dfirstSlab = 0;
                        //bIsOTExtentNextSlab = false;
                        //bIsTotalWorkTimeAsOT = false;
                        //sShiftSystemID = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftSystemID"].ToString();
                        //sShiftType = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftType"].ToString();
                        //sDayType = dsEmpInfo.Tables[0].Rows[EmpCount]["DayType"].ToString();

                        //sBreakStratTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakStratTime"].ToString().Trim()).ToString("HH:mm:ss");
                        //sBreakEndTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakEndTime"].ToString().Trim()).ToString("HH:mm:ss");

                        if (Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["DateDiffer"].ToString()) <= 1)
                        {
                            sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemId"].ToString();

                            if (sLogDownLoadNum == "2018-10538")
                            {

                            }
                            #region Find InTime from raw Data Table

                            sOutTime = "00:00:00";
                            sOutTimeRowID = string.Empty;
                            iDeviceID = 0;
                            sOutTimeTmp = "00:00:00";
                            sOutTimeRowIDTmp = string.Empty;
                            iDeviceIDTmp = 0;
                            bool IsDateChanged = false;

                            dvRawData = new DataView();
                            dvRawData.Table = dtRawData;
                            dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                            if (dvRawData.Count > 0)
                            {
                                for (int RData = 0; RData < dvRawData.Count; RData++)
                                {
                                    if (dvRawData[RData]["PTime"].ToString() != "")
                                    {
                                        //string sysOutDateTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).AddHours(-_MaxWorkingHour).ToString("dd-MMM-yyyy HH:mm:ss");
                                        //DateTime _PrevDate = Convert.ToDateTime(Convert.ToDateTime(sysOutDateTime).ToString("dd-MMM-yyyy"));
                                        //DateTime _TodaysDate = Convert.ToDateTime(Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("dd-MMM-yyyy"));

                                        string sysOutTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");

                                        //if(_PrevDate <_TodaysDate)
                                        //{
                                        //    //as it goes to previous day the date will b

                                        //}

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
                                        HasRawData = true;
                                    }

                                    drRawData = dvRawData[RData].Row;
                                    drRawData.BeginEdit();
                                    drRawData["ProcessedFlag"] = 1;
                                    drRawData.EndEdit();
                                }

                                //=====add one day starts====================sDate.Trim()
                                string sysOutDateTime = Convert.ToDateTime(sDate.Trim() + " " + sOutTime).AddHours(-_MaxWorkingHour).ToString("dd-MMM-yyyy HH:mm:ss");
                                DateTime _PrevDate = Convert.ToDateTime(Convert.ToDateTime(sysOutDateTime).ToString("dd-MMM-yyyy"));
                                DateTime _TodaysDate = Convert.ToDateTime(sDate.Trim());


                                if (_PrevDate < _TodaysDate)
                                {
                                    // sDate = _PrevDate;
                                    //as it goes to previous day the date will b
                                    sWorkingDate = _PrevDate.ToString("dd-MMM-yyyy");
                                    IsDateChanged = true;
                                }
                                else
                                {
                                    sWorkingDate = sDate.Trim();
                                }
                                //=====add one day ends
                            }
                            else
                            {
                                HasRawData = false;
                            }

                            #endregion Find InTime from raw Data Table

                            //if (sShiftType.ToUpper().Trim() == "DAY SHIFT")
                            //{

                            //}
                            //else if (sShiftType.ToUpper().Trim() == "NIGHT SHIFT")
                            //{
                            //sWorkingDate = sPrvDate.Trim();
                            //}

                            bool bAttnIsLock = false;
                            bool bManualOutTime = false;

                            if (HasRawData)
                            {
                                dvAttnProcData = new DataView();
                                dvAttnProcData.Table = dtAttnProcData;

                                if (IsDateChanged)
                                {
                                    dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                    if (dvAttnProcData.Count > 0)
                                    {
                                        if (dvAttnProcData[0]["InTime"].ToString().Trim() != "")
                                        {
                                            sInTime = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                            string _NextDateTime = Convert.ToDateTime(sDate.Trim() + " " + sInTime).AddHours(_MaxWorkingHour).ToString("dd-MMM-yyyy HH:mm:ss");
                                            DateTime _NextDate = Convert.ToDateTime(Convert.ToDateTime(_NextDateTime).ToString("dd-MMM-yyyy"));
                                            if (_NextDate == Convert.ToDateTime(sDate.Trim()))
                                            {
                                                sWorkingDate = sDate;//it is because the user punch several time in n out machinne almost at the same time.
                                            }
                                        }
                                    }
                                }

                                dvAttnProcData.RowFilter = null;
                                dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sWorkingDate.Trim() + "'";
                                if (dvAttnProcData.Count > 0)
                                {
                                    if (dvAttnProcData[0]["InTime"].ToString().Trim() != "")
                                    {
                                        sInTime = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    }
                                    bAttnIsLock = Convert.ToBoolean(dvAttnProcData[0].Row["IsLock"].ToString());
                                    bManualOutTime = Convert.ToBoolean(dvAttnProcData[0].Row["IsManualOutTime"].ToString());

                                    if (bAttnIsLock == false)
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
                                        }

                                        #endregion Manual Attendance

                                        #region Over Time Calculation

                                        //if (sOutTime != "00:00:00" & Convert.ToDateTime(sOTStartTime) < Convert.ToDateTime(sOutTime)/* & bOTEntitle == true*/)
                                        //{
                                        //    dvOTSlabEmp = new DataView();
                                        //    dvOTSlabEmp.Table = dtOTSlabEmp;
                                        //    dvOTSlabEmp.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                                        //    if (dvOTSlabEmp.Count > 0)
                                        //    {
                                        //        sOTDayType = dvOTSlabEmp[0].Row["DayType"].ToString();
                                        //        dfirstSlab = (Convert.ToDecimal(dvOTSlabEmp[0].Row["firstSlab"].ToString()) * 60);
                                        //        bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsOTExtentNextSlab"].ToString());
                                        //        bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsTotalWorkTimeAsOT"].ToString());
                                        //    }
                                        //    else if (dsOTSlabGen.Tables[0].Rows.Count > 0)
                                        //    {
                                        //        sOTDayType = dsOTSlabGen.Tables[0].Rows[0]["DayType"].ToString();
                                        //        dfirstSlab = (Convert.ToDecimal(dsOTSlabGen.Tables[0].Rows[0]["firstSlab"].ToString()) * 60);
                                        //        bIsOTExtentNextSlab = Convert.ToBoolean(dsOTSlabGen.Tables[0].Rows[0]["IsOTExtentNextSlab"].ToString());
                                        //        bIsTotalWorkTimeAsOT = Convert.ToBoolean(dsOTSlabGen.Tables[0].Rows[0]["IsTotalWorkTimeAsOT"].ToString());
                                        //    }

                                        //    if (bIsTotalWorkTimeAsOT == true)
                                        //    {
                                        //        if (sInTime != "00:00:00")
                                        //        {
                                        //            sInTime = sWorkingDate + " " + sInTime;
                                        //            sOutTime = sDate + " " + sOutTime;

                                        //            TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sInTime);
                                        //            iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                                        //        }
                                        //    }
                                        //    else if (bIsTotalWorkTimeAsOT == false)
                                        //    {
                                        //        TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sOTStartTime);
                                        //        iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                                        //    }

                                        //    int iMinOT = 1;

                                        //    if (string.IsNullOrEmpty(sMinOT.Trim()) == false)
                                        //    {
                                        //        iMinOT = Convert.ToInt32(sMinOT.Trim());
                                        //    }

                                        //    if (sFractionCalculate.ToUpper().Trim() == "ROUND")
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //    else if (sFractionCalculate.ToUpper().Trim() == "ROUND UP")
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Ceiling((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //    else if (sFractionCalculate.ToUpper().Trim() == "ROUND DOWN")
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Floor((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //    else
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //}

                                        #endregion Over Time Calculation

                                        drAttnProcData = dvAttnProcData[0].Row;
                                        drAttnProcData.BeginEdit();
                                        UpdateAttdnData_SNA("EDIT", GroupSysID, "OUT", sEmpSysID, sPlantID, sWorkingDate.Trim(), sDate, sOutTime, bManualOutTime, sOutTimeRowID, "", false, iTotalOTHr, ref drAttnProcData);
                                        drAttnProcData.EndEdit();
                                    }//(bAttnIsLock == false)
                                }//dvAttnProcData.Count 
                            }//HasRawData
                        }
                    }
                    //clsStaticInfo obj = new clsStaticInfo();
                    SaveDataSets(dsRawData, dsAttnProcData);
                }
                bValid = true;
                return bValid;
            }
            catch (Exception ex)
            {
                throw ex;
                //Cursor = Cursors.Default;
                //System.Windows.Forms.MessageBox.Show(this, ex.ToString(), "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return bValid;
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

                #endregion clean variables
            }
        }//End Function 
        private bool SummaryProcess_SNA(string GroupSysID, string sAttnDate, string sEmpSystemIDColl)
        {
            #region declare variables

            DataSet dsAttnDataForTheMonth = null;

            DataSet dsAttnDataMonthSummary = null;
            DataTable dtAttnDataMonthSummary = null;
            DataRow drAttnDataMonthSummary = null;
            DataView dvAttnDataMonthSummary = null;

            bool bValid = false;

            #endregion local variables

            try
            {
                #region DataSet

                GetAttdnDataForMonthlyProc(GroupSysID.Trim(), "", sAttnDate.Trim(), sEmpSystemIDColl.Trim(), out dsAttnDataForTheMonth);
                GetAttdnDataMonthlySummary(GroupSysID.Trim(), Convert.ToDateTime(sAttnDate.Trim()).Month, Convert.ToDateTime(sAttnDate.Trim()).Year, sEmpSystemIDColl.Trim(), out dsAttnDataMonthSummary);
                dtAttnDataMonthSummary = dsAttnDataMonthSummary.Tables[0];

                #endregion DataSet

                for (int i = 0; i < dsAttnDataForTheMonth.Tables[0].Rows.Count; i++)
                {
                    dvAttnDataMonthSummary = new DataView();
                    dvAttnDataMonthSummary.Table = dtAttnDataMonthSummary;
                    dvAttnDataMonthSummary.RowFilter = "EmpSystemID = '" + dsAttnDataForTheMonth.Tables[0].Rows[i]["EmpSystemID"].ToString() + "'";
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

                //clsStaticInfo obj = new clsStaticInfo();
                SaveDataSets(dsAttnDataMonthSummary);

                bValid = true;
                return bValid;
            }
            catch (Exception ex)
            {
                throw ex;
                //Cursor = Cursors.Default;
                //System.Windows.Forms.MessageBox.Show(this, ex.ToString(), "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return bValid;
            }
            finally
            {
                #region clean variable

                dsAttnDataForTheMonth = null;

                dsAttnDataMonthSummary = null;
                dtAttnDataMonthSummary = null;
                drAttnDataMonthSummary = null;
                dvAttnDataMonthSummary = null;

                #endregion
            }
        }//End Function 
        private void UpdateAttdnData(ParaAttendance _paraA, ref DataRow drLocal)
        {
            //if (sShiftType.ToUpper().Trim() == "NIGHT SHIFT")
            //99
            bool IsCurrentShiftNightShift = false;
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
                        drLocal["LTSystemID"] = _paraA.sLvTrans;
                        drLocal["IsLWP"] = _paraA.IsLWP;
                        drLocal["IsHalfDayLeave"] = _paraA.IsHalfDayLeave;
                        drLocal["LeaveDuration"] = _paraA.LeaveDuration;
                    }
                    else
                    {
                        drLocal["LTSystemID"] = DBNull.Value;
                        drLocal["IsLWP"] = false;
                        drLocal["IsHalfDayLeave"] = false;
                        drLocal["LeaveDuration"] = 0;
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
        private void xUpdateAttdnData(string OPN_FLAG, string GroupId, string sType, string sEmpSystemID, string sPlantID, string sWorkingDate, string shiftSystemID, string sDate, string sTime, bool bManualTime, string sRowID, string sDayStatus, bool bManualDayStatus, decimal iOverTime, string sLvTrans, ref DataRow drLocal)
        {
            //if (sShiftType.ToUpper().Trim() == "NIGHT SHIFT")
            //99
            //bool IsCurrentShiftNightShift =false;
            //bool IsNewShiftNightShift = false;
            try
            {

                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["AddedBy"] = "Schedule";
                    drLocal["DateAdded"] = DateTime.Now;
                }
                //else
                //{
                //    //check if it is night shift
                //    DataSet dsST = null;
                //    string sid = drLocal["ShiftSystemID"].ToString();
                //    GetShiftType(sid, out dsST);
                //    if(dsST.Tables[0].Rows.Count>0)
                //    {
                //        IsCurrentShiftNightShift = true;
                //    }

                //    GetShiftType(shiftSystemID, out dsST);
                //    if (dsST.Tables[0].Rows.Count > 0)
                //    {
                //        IsNewShiftNightShift = true;
                //    }
                //}

                drLocal["EmpSystemID"] = sEmpSystemID;
                drLocal["WorkDate"] = sWorkingDate;

                if (sType == "IN")
                {
                    if (shiftSystemID != string.Empty)
                    {
                        drLocal["ShiftSystemID"] = shiftSystemID;
                    }

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
                        //if(IsNewShiftNightShift && IsCurrentShiftNightShift==false)
                        //{

                        //}
                        //else
                        //{
                        drLocal["OutTime"] = sDate + " " + sTime;
                        drLocal["IsManualOutTime"] = bManualTime;
                        //}                       
                    }

                    drLocal["OTHr"] = iOverTime;

                    if (sRowID == string.Empty)
                    {
                        drLocal["OutTimeRowID"] = DBNull.Value;
                    }
                    else
                    {
                        drLocal["OutTimeRowID"] = sRowID;
                    }
                }
                drLocal["ToReprocess"] = "No";

                drLocal["GroupID"] = GroupId;
                drLocal["PlantID"] = sPlantID.Trim();

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
        private void UpdateAttdnData_SNA(string OPN_FLAG, string GroupId, string sType, string sEmpSystemID, string sPlantID, string sWorkingDate, string sDate, string sTime, bool bManualTime, string sRowID, string sDayStatus, bool bManualDayStatus, decimal iOverTime, ref DataRow drLocal)
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

                    //if (sLvTrans != "")
                    //{
                    //    drLocal["LTSystemID"] = sLvTrans;
                    //}
                    //else
                    //{
                    //    drLocal["LTSystemID"] = DBNull.Value;
                    //}
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

                    if (sRowID == string.Empty)
                    {
                        drLocal["OutTimeRowID"] = DBNull.Value;
                    }
                    else
                    {
                        drLocal["OutTimeRowID"] = sRowID;
                    }
                }
                drLocal["ToReprocess"] = "No";

                drLocal["GroupID"] = GroupId;
                drLocal["PlantID"] = sPlantID.Trim();

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

                _ShiftDft.ShortLeaveMaxLimit = Convert.ToDecimal(drSource["ShortLeaveMaxLimit"].ToString());
                _ShiftDft.HalfDayAbsentMaxLimit = Convert.ToDecimal(drSource["HalfDayAbsentMaxLimit"].ToString());
                //_ShiftDft.IncludeBreakTimeInOT = bplib.clsWebLib.GetBoolData(drSource["IncludeBreakTimeInOT"].ToString());

                //Convert.ToInt32(drSource["OutTime"].ToString());BreakStratTime
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool xOutDataProcess(string _plantId, string sAttnDate, string GroupSysID, string sEmpSystemIDColl, string sMinOT, string sFractionCalculate, bool radDwLdEnrollID)
        {
            //sShiftType
            #region declare variables
            DataSet dsCompensatoryOff = null;
            DataSet dsCompensatoryOffEmpList = null;
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

            string sLogDownLoadNum = "";
            string sEmpSysID = "";
            string sPlantID = "";
            string sOTStartTime = "";
            decimal iTotalOTHr = 0;
            decimal _OT_inTime = 0;
            decimal _OT_outTime = 0;
            string sOTDayType = "";
            decimal dfirstSlab = 0;
            bool bIsOTExtentNextSlab = false;
            bool bIsTotalWorkTimeAsOT = false;
            bool bOTEntitle = false;

            string sOfficeInTime = "";
            string sInTime = "";
            string sOutTime = "";
            string sOutTimeRowID = string.Empty;
            // int iDeviceID = 0;
            string sOutTimeTmp = "";
            string sOutTimeRowIDTmp = string.Empty;
            //int iDeviceIDTmp = 0;
            string sShiftSystemID = "";
            string sShiftType = "";
            string sDayType = "";

            string sBreakStratTime = "";
            string sBreakEndTime = "";

            string sDate = "";
            string sPrvDate = "";
            string sWorkingDate = "";
            string sOutDate = "";
            bool bValid = false;
            bool IsOTOverHalfDay = false;
            bool IsWeekendAsFixedWeekend = false;
            bool IsOTBasedOnPerMinute = false;
            //bool OTDeductionForAbsenteeism = false;
            dicShiftDft _ShiftDft = null;
            double _PaidHours = 0;
            ShortLeaveSetting _shortleave_setting = null;
            #endregion local variables

            try
            {
                _shortleave_setting = new ShortLeaveSetting();
                DataSet dsShortLeaveSetting = null;
                GetShortLeaveSettingPlantWise(_plantId, out dsShortLeaveSetting);
                if (dsShortLeaveSetting.Tables[0].Rows.Count > 0)
                {
                    _shortleave_setting.IsHalfDayPresentAllowed = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsHalfDayPresentAllowed"].ToString());
                    _shortleave_setting.IsShortLeaveAllowed = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsShortLeaveAllowed"].ToString());
                    _shortleave_setting.IsTowShortLeaveAllowedInaDay = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsTowShortLeaveAllowedInaDay"].ToString());
                    _shortleave_setting.MaxShortLeaveInaMonth = Convert.ToInt32(GetNumData(dsShortLeaveSetting.Tables[0].Rows[0]["MaxShortLeaveInaMonth"].ToString()));
                    //IsOToverHalfDay = bplib.clsWebLib.GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsOTOverHalfDay"].ToString());
                }

                DataSet dsHRSetting = null;
                GetHRSettingPlantWise(_plantId, out dsHRSetting);//GetHRSettingPlantWise
                if (dsHRSetting.Tables[0].Rows.Count > 0)
                {
                    IsOTOverHalfDay = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsOTOverHalfDay"].ToString());
                    IsWeekendAsFixedWeekend = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsWeekendCount"].ToString());
                    IsOTBasedOnPerMinute = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsOTBasedOnPerMinute"].ToString());
                    //OTDeductionForAbsenteeism = GetBoolData(dsHRSetting.Tables[0].Rows[0]["OTDeductionForAbsenteeism"].ToString());
                }

                DataSet dsOTPerMinPolicy = null;
                if (IsOTBasedOnPerMinute)
                {
                    GetOTPerMinPolicy(_plantId, out dsOTPerMinPolicy);//GetHRSettingPlantWise
                }

                DateTime sFrmDt = Convert.ToDateTime(sAttnDate.Trim());
                DateTime sToDt = Convert.ToDateTime(sAttnDate.Trim()).AddDays(-1);
                while (sToDt <= sFrmDt)
                {
                    #region DataSet

                    sDate = Convert.ToDateTime(sFrmDt).ToString("dd-MMM-yyyy");
                    sPrvDate = (Convert.ToDateTime(sFrmDt).AddDays(-1)).ToString("dd-MMM-yyyy");

                    GetAttdnRawDataForAttdnProc(GroupSysID.Trim(), sDate.Trim(), "OUT", out dsRawData);
                    dtRawData = dsRawData.Tables[0];

                    GetAttdnProcData(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sPrvDate.Trim(), sDate.Trim(), out dsAttnProcData);
                    dtAttnProcData = dsAttnProcData.Tables[0];

                    GetFinalOT(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), out dsFinalOT);
                    dtFinalOT = dsFinalOT.Tables[0];

                    GetOTSlabDefineEmployee(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), out dsOTSlabEmp);
                    dtOTSlabEmp = dsOTSlabEmp.Tables[0];

                    GetOTSlabDefineGeneral(GroupSysID.Trim(), sDate.Trim(), out dsOTSlabGen);
                    dtOTSlabGen = dsOTSlabGen.Tables[0];

                    GetEmployeeInfo_Out(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sDate.Trim(), out dsEmpInfo);

                    GetAttdnManualData(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sAttnDate.Trim(), out dsMnAttData);
                    dtMnAttData = dsMnAttData.Tables[0];
                    dvMnAttData = new DataView();

                    DataSet dsPaidHours = null;
                    GetPaidHours(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), out dsPaidHours);


                    //============ kabir ==========
                    GetCompensatoryOffPlantData(_plantId, sDate.Trim(), out dsCompensatoryOff);
                    GetCompensatoryOffEmpListData(_plantId, sDate.Trim(), sEmpSystemIDColl.Trim(), out dsCompensatoryOffEmpList);
                    var CompensatoryDateTreatmentType = string.Empty;
                    if (dsCompensatoryOff.Tables[0].Rows.Count > 0)
                    {
                        CompensatoryDateTreatmentType = dsCompensatoryOff.Tables[0].Rows[0]["CompensatoryDateTreatmentType"].ToString();
                        //bool IsOriginalDateOTApplicable= Convert.ToBoolean(dsCompensatoryOff.Tables[0].Rows[0]["IsOriginalDateOTApplicable"].ToString());
                    }
                    string flag = "";
                    bool IsOriginalDateOTApplicable = false;
                    #endregion DataSet

                    if (dsEmpInfo.Tables[0].Rows.Count > 0)
                    {
                        for (int EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                        {
                            _ShiftDft = new global::dicShiftDft();
                            GetShiftDefinition(dsEmpInfo.Tables[0].Rows[EmpCount], _ShiftDft);

                            sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();
                            //if (sEmpSysID == "1800529")
                            //{

                            //}

                            DataView dv = new DataView(dsPaidHours.Tables[0]);
                            dv.RowFilter = "EmployeeId='" + sEmpSysID + "'";
                            if (dv.Count > 0)
                            {
                                _PaidHours = Convert.ToDouble(GetNumData(dv[0]["PaidHours"].ToString()));
                            }
                            else
                            {
                                _PaidHours = 0;
                            }

                            sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();
                            sOTStartTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OTStartTime"].ToString().Trim()).ToString("HH:mm:ss");
                            sOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeTime"].ToString().Trim()).ToString("HH:mm:ss");
                            sInTime = "00:00:00";
                            bOTEntitle = Convert.ToBoolean(GetBoolData(dsEmpInfo.Tables[0].Rows[EmpCount]["IsOTEntitle"].ToString()));
                            iTotalOTHr = 0;
                            _OT_inTime = 0;
                            _OT_outTime = 0;
                            sOTDayType = "";
                            dfirstSlab = 0;
                            sOutDate = "";
                            bIsOTExtentNextSlab = false;
                            bIsTotalWorkTimeAsOT = false;
                            sShiftSystemID = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftSystemID"].ToString();
                            sShiftType = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftType"].ToString();
                            sDayType = dsEmpInfo.Tables[0].Rows[EmpCount]["DayType"].ToString();

                            sBreakStratTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakStratTime"].ToString().Trim()).ToString("HH:mm:ss");
                            sBreakEndTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakEndTime"].ToString().Trim()).ToString("HH:mm:ss");
                            #region plant wise Compensatory

                            if (dsCompensatoryOff.Tables[0].Rows.Count > 0)
                            {

                                CompensatoryDateTreatmentType = dsCompensatoryOff.Tables[0].Rows[0]["CompensatoryDateTreatmentType"].ToString();
                                flag = dsCompensatoryOff.Tables[0].Rows[0]["flag"].ToString();
                                IsOriginalDateOTApplicable = Convert.ToBoolean(dsCompensatoryOff.Tables[0].Rows[0]["IsOriginalDateOTApplicable"].ToString());
                                if (flag == "compensatory")
                                {
                                    IsOriginalDateOTApplicable = false;
                                }
                                // sDayType = CompensatoryDateTreatmentType;
                            }
                            #endregion

                            #region employe wise Compensatory
                            DataView dvEmp = new DataView(dsCompensatoryOffEmpList.Tables[0]);
                            dvEmp.RowFilter = "EmpSystemId ='" + sEmpSysID + "'";

                            if (dvEmp.Count > 0)
                            {

                                CompensatoryDateTreatmentType = dvEmp[0]["CompensatoryDateTreatmentType"].ToString();
                                flag = dvEmp[0]["flag"].ToString();
                                IsOriginalDateOTApplicable = Convert.ToBoolean(dsCompensatoryOff.Tables[0].Rows[0]["IsOriginalDateOTApplicable"].ToString());
                                if (flag == "compensatory")
                                {
                                    IsOriginalDateOTApplicable = false;
                                }

                                //sDayType = CompensatoryDateTreatmentType;
                            }
                            #endregion



                            if (Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["DateDiffer"].ToString()) <= 1)
                            {
                                sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemId"].ToString();

                                if (sShiftType.ToUpper().Trim() == "DAY SHIFT")
                                {
                                    sWorkingDate = sDate.Trim();
                                    sOutDate = sWorkingDate.Trim();
                                }
                                else if (sShiftType.ToUpper().Trim() == "NIGHT SHIFT")
                                {
                                    sWorkingDate = sPrvDate.Trim();
                                    sOutDate = (Convert.ToDateTime(sWorkingDate.Trim()).AddDays(1)).ToString("dd-MMM-yyyy");
                                }

                                #region Find InTime from raw Data Table

                                sOutTime = "00:00:00";
                                sOutTimeRowID = string.Empty;
                                //iDeviceID = 0;
                                sOutTimeTmp = "00:00:00";
                                sOutTimeRowIDTmp = string.Empty;
                                //iDeviceIDTmp = 0;

                                dvRawData = new DataView();
                                dvRawData.Table = dtRawData;
                                dvRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "'";
                                if (dvRawData.Count > 0)
                                {
                                    for (int RData = 0; RData < dvRawData.Count; RData++)
                                    {
                                        if (dvRawData[RData]["PTime"].ToString() != "")
                                        {
                                            if (sOutDate == Convert.ToDateTime(dvRawData[RData]["PDate"].ToString()).ToString("dd-MMM-yyyy"))
                                            {
                                                string sysOutTime = Convert.ToDateTime(dvRawData[RData]["PTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                if (sOutTime == "00:00:00" || Convert.ToDateTime(sysOutTime.Trim()) > Convert.ToDateTime(sOutTime.Trim()))
                                                {
                                                    sOutTime = sysOutTime;
                                                    sOutTimeRowID = dvRawData[RData]["RowID"].ToString().Trim();
                                                    //iDeviceID = Convert.ToInt32(dvRawData[RData]["DeviceID"].ToString().Trim());

                                                    if (sOutTimeTmp != "00:00:00" & Convert.ToDateTime(sOutTime) < Convert.ToDateTime(sOutTimeTmp))
                                                    {
                                                        sOutTime = sOutTimeTmp;
                                                        sOutTimeRowID = sOutTimeRowIDTmp;
                                                        //iDeviceID = iDeviceIDTmp;
                                                    }
                                                    sOutTimeTmp = sOutTime;
                                                    sOutTimeRowIDTmp = sOutTimeRowID;
                                                    //iDeviceIDTmp = iDeviceID;
                                                }
                                            }
                                        }

                                        drRawData = dvRawData[RData].Row;
                                        drRawData.BeginEdit();
                                        drRawData["ProcessedFlag"] = 1;
                                        drRawData.EndEdit();
                                    }
                                }

                                #endregion Find InTime from raw Data Table

                                #region by monir for manual
                                bool IsManualDeleted = false;
                                dvMnAttData.Table = dtMnAttData;
                                dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                if (dvMnAttData.Count > 0)
                                {

                                    if (dvMnAttData[0].Row["OutTime"].ToString().Trim() != "")
                                    {
                                        IsManualDeleted = false;
                                        sWorkingDate = sDate;
                                    }
                                    else
                                    {
                                        IsManualDeleted = true;
                                    }
                                }
                                else
                                {
                                    IsManualDeleted = true;
                                }
                                #endregion

                                bool bAttnIsLock = false;
                                bool bManualOutTime = false;

                                dvAttnProcData = new DataView();
                                dvAttnProcData.Table = dtAttnProcData;
                                dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sWorkingDate.Trim() + "'";
                                if (dvAttnProcData.Count > 0)
                                {
                                    if (dvAttnProcData[0]["InTime"].ToString().Trim() != "")
                                    {
                                        sInTime = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                    }
                                    bAttnIsLock = Convert.ToBoolean(dvAttnProcData[0].Row["IsLock"].ToString());
                                    bManualOutTime = Convert.ToBoolean(dvAttnProcData[0].Row["IsManualOutTime"].ToString());

                                    if (bAttnIsLock == false)
                                    {
                                        if (dvAttnProcData[0]["OutTime"].ToString() != "")
                                        {
                                            sOutTimeTmp = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                            sOutTimeRowIDTmp = dvAttnProcData[0]["OutTimeRowID"].ToString().Trim();
                                        }

                                        if (bManualOutTime && IsManualDeleted)
                                        {
                                            sOutTimeTmp = "00:00:00";
                                        }

                                        if (Convert.ToDateTime(sOutTime) < Convert.ToDateTime(sOutTimeTmp))
                                        {
                                            sOutTime = sOutTimeTmp;
                                            sOutTimeRowID = sOutTimeRowIDTmp;
                                        }

                                        #region Manual Attendance

                                        bool HasManualOutTime = false;
                                        string ManualDate = string.Empty;
                                        dvMnAttData.Table = dtMnAttData;
                                        dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                        if (dvMnAttData.Count > 0)
                                        {
                                            if (dvMnAttData[0].Row["OutTime"].ToString().Trim() != "")
                                            {
                                                if (dvMnAttData[0].Row["DayStatus"].ToString().Trim() == "")
                                                {
                                                    HasManualOutTime = true;
                                                    sOutTime = Convert.ToDateTime(dvMnAttData[0].Row["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                    ManualDate = Convert.ToDateTime(dvMnAttData[0].Row["OutTime"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                                    bManualOutTime = true;
                                                }
                                                else
                                                {
                                                    HasManualOutTime = false;
                                                    bManualOutTime = false;
                                                }
                                                //by monir
                                                //if (sShiftType.ToUpper().Trim() == "NIGHT SHIFT")
                                                //{
                                                //    sWorkingDate = sDate;
                                                //}
                                            }
                                            sOutTimeRowID = "";
                                        }
                                        else
                                        {
                                            if (bManualOutTime)
                                            {
                                                bManualOutTime = false;
                                                //sOutTime = "00:00:00";
                                            }
                                        }

                                        #endregion Manual Attendance
                                        //if (bOTEntitle)
                                        //{
                                        // var _OutTimeForShortLeave = sOutTime;
                                        //CalculateOT(IsOriginalDateOTApplicable, dsOTPerMinPolicy, IsOTBasedOnPerMinute, sDayType, bOTEntitle, HasManualOutTime, ManualDate, _PaidHours, _ShiftDft, IsOTOverHalfDay, dtOTSlabEmp, dsOTSlabGen, sEmpSysID, sWorkingDate, sDate, sInTime, sOTStartTime, sMinOT, sOutTime, out iTotalOTHr, out _OT_inTime, out _OT_outTime);
                                        // }//

                                        #region Over Time Calculation //commented

                                        //if (sOutTime != "00:00:00" & Convert.ToDateTime(sOTStartTime) < Convert.ToDateTime(sOutTime)/* & bOTEntitle == true*/)
                                        //{
                                        //    dvOTSlabEmp = new DataView();
                                        //    dvOTSlabEmp.Table = dtOTSlabEmp;
                                        //    dvOTSlabEmp.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                                        //    if (dvOTSlabEmp.Count > 0)
                                        //    {
                                        //        sOTDayType = dvOTSlabEmp[0].Row["DayType"].ToString();
                                        //        dfirstSlab = (Convert.ToDecimal(dvOTSlabEmp[0].Row["firstSlab"].ToString()) * 60);
                                        //        bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsOTExtentNextSlab"].ToString());
                                        //        bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsTotalWorkTimeAsOT"].ToString());
                                        //    }
                                        //    else if (dsOTSlabGen.Tables[0].Rows.Count > 0)
                                        //    {
                                        //        sOTDayType = dsOTSlabGen.Tables[0].Rows[0]["DayType"].ToString();
                                        //        dfirstSlab = (Convert.ToDecimal(dsOTSlabGen.Tables[0].Rows[0]["firstSlab"].ToString()) * 60);
                                        //        bIsOTExtentNextSlab = Convert.ToBoolean(dsOTSlabGen.Tables[0].Rows[0]["IsOTExtentNextSlab"].ToString());
                                        //        bIsTotalWorkTimeAsOT = Convert.ToBoolean(dsOTSlabGen.Tables[0].Rows[0]["IsTotalWorkTimeAsOT"].ToString());
                                        //    }

                                        //    if (bIsTotalWorkTimeAsOT == true)
                                        //    {
                                        //        if (sInTime != "00:00:00")
                                        //        {
                                        //            sInTime = sWorkingDate + " " + sInTime;
                                        //            sOutTime = sDate + " " + sOutTime;

                                        //            TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sInTime);
                                        //            iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                                        //        }
                                        //    }
                                        //    else if (bIsTotalWorkTimeAsOT == false)
                                        //    {
                                        //        //Modify Date:- 21-Jul-2018 By Prodipta
                                        //        TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sOTStartTime);
                                        //        //TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime("00:00:00");
                                        //        iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                                        //    }

                                        //    int iMinOT = 1;

                                        //    if (string.IsNullOrEmpty(sMinOT.Trim()) == false)
                                        //    {
                                        //        iMinOT = Convert.ToInt32(sMinOT.Trim());
                                        //    }

                                        //    if (sFractionCalculate.ToUpper().Trim() == "ROUND")
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //    else if (sFractionCalculate.ToUpper().Trim() == "ROUND UP")
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Ceiling((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //    else if (sFractionCalculate.ToUpper().Trim() == "ROUND DOWN")
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Floor((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //    else
                                        //    {
                                        //        iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                                        //    }
                                        //}

                                        #endregion Over Time Calculation

                                        //sEmpSysID
                                        bool IsShortLeave = false;
                                        bool IsStatusChanged = false;
                                        string _DayStatus = "";
                                        bool IsReversed = false;
                                        int CountShortLeave = 0;
                                        bool ShouldNullifyOTValue = false;
                                        //#if DEBUG
                                        ParaShortLeaveHalfDayAbsent objSLHD = new global::ParaShortLeaveHalfDayAbsent();
                                        #region set value
                                        objSLHD.sInTime = sInTime;
                                        objSLHD.sOutTime = sOutTime;
                                        objSLHD.sWorkingDate = sWorkingDate;
                                        objSLHD.sDate = sDate;
                                        objSLHD._ShiftDft = _ShiftDft;
                                        objSLHD.DayStatus = _DayStatus;
                                        objSLHD.IsShortLeave = IsShortLeave;
                                        objSLHD.IsStatusChanged = IsStatusChanged;
                                        objSLHD.IsReversed = IsReversed;
                                        objSLHD.CountShortLeave = CountShortLeave;
                                        objSLHD.IsShortLeaveAllowed = _shortleave_setting.IsShortLeaveAllowed;
                                        objSLHD.IsHalfDayPresentAllowed = _shortleave_setting.IsHalfDayPresentAllowed;
                                        objSLHD.IsTowShortLeaveAllowedInaDay = _shortleave_setting.IsTowShortLeaveAllowedInaDay;
                                        objSLHD.MaxShortLeaveInaMonth = _shortleave_setting.MaxShortLeaveInaMonth;
                                        objSLHD.IsOTOverHalfDay = IsOTOverHalfDay;
                                        objSLHD.PaidHours = _PaidHours;
                                        objSLHD.IsOTentitled = bOTEntitle;
                                        objSLHD.HasManualOutTime = HasManualOutTime;
                                        objSLHD.ManualDate = ManualDate;
                                        #endregion

                                        ShortLeaveHalfDayAbsent(objSLHD);
                                        ShouldNullifyOTValue = objSLHD.ShouldNullifyOTValue;
                                        //ShortLeaveHalfDayAbsent(sInTime, sOutTime, sWorkingDate, sDate, _ShiftDft, IsOTOverHalfDay,
                                        //out _DayStatus, out IsShortLeave, out IsStatusChanged, out IsReversed,out CountShortLeave);
                                        _DayStatus = objSLHD.DayStatus;
                                        IsShortLeave = objSLHD.IsShortLeave;
                                        IsStatusChanged = objSLHD.IsStatusChanged;
                                        IsReversed = objSLHD.IsReversed;
                                        CountShortLeave = objSLHD.CountShortLeave;
                                        //#endif

                                        #region Para
                                        ParaAttendance _paraA = new global::ParaAttendance();
                                        _paraA.OPN_FLAG = "EDIT";
                                        _paraA.GroupId = GroupSysID;
                                        _paraA.sType = "OUT";
                                        _paraA.sEmpSystemID = sEmpSysID;
                                        _paraA.sPlantID = sPlantID;
                                        _paraA.sWorkingDate = sWorkingDate.Trim();
                                        _paraA.shiftSystemID = sShiftSystemID;
                                        _paraA.sDate = sDate;
                                        _paraA.sTime = sOutTime;
                                        _paraA.bManualTime = bManualOutTime;
                                        _paraA.sRowID = sOutTimeRowID;
                                        _paraA.sDayStatus = _DayStatus;
                                        _paraA.bManualDayStatus = false;
                                        ///at the time of A as per settng
                                        _paraA.iOverTime = iTotalOTHr;
                                        _paraA.sLvTrans = "";
                                        _paraA.iOverTimeIntime = _OT_inTime;
                                        _paraA.iOverTimeOuttime = _OT_outTime;
                                        _paraA.IsStatusChanged = IsStatusChanged;
                                        _paraA.IsShortLeave = IsShortLeave;
                                        _paraA.IsReversed = IsReversed;
                                        _paraA.CountedShortLeave = CountShortLeave;
                                        _paraA.HasManualOutTime = HasManualOutTime;
                                        _paraA.ManualDate = ManualDate;
                                        _paraA.IsOTEntitled = bOTEntitle;



                                        #endregion

                                        drAttnProcData = dvAttnProcData[0].Row;
                                        drAttnProcData.BeginEdit();

                                        if (IsWeekendAsFixedWeekend && (drAttnProcData["DayStatus"].ToString() == "W" || drAttnProcData["DayStatus"].ToString() == "H"))
                                        {
                                            #region WH
                                            _paraA.OPN_FLAG = "EDIT";
                                            _paraA.GroupId = GroupSysID;
                                            _paraA.sType = "OUT";
                                            _paraA.sEmpSystemID = sEmpSysID;
                                            _paraA.sPlantID = sPlantID;
                                            _paraA.sWorkingDate = sWorkingDate.Trim();
                                            _paraA.shiftSystemID = sShiftSystemID;
                                            _paraA.sDate = sDate;
                                            _paraA.sTime = "00:00:00";
                                            //_paraA.sTime = sOutTime;
                                            _paraA.bManualTime = false;
                                            _paraA.sRowID = sOutTimeRowID;
                                            _paraA.sDayStatus = drAttnProcData["DayStatus"].ToString();
                                            _paraA.bManualDayStatus = false;
                                            ///at the time of A as per settng
                                            _paraA.iOverTime = 0;
                                            _paraA.sLvTrans = "";
                                            _paraA.iOverTimeIntime = _OT_inTime;
                                            _paraA.iOverTimeOuttime = _OT_outTime;
                                            _paraA.IsStatusChanged = false;
                                            _paraA.IsShortLeave = false;
                                            _paraA.IsReversed = false;
                                            _paraA.CountedShortLeave = 0;
                                            _paraA.HasManualOutTime = HasManualOutTime;
                                            _paraA.ManualDate = ManualDate;
                                            _paraA.IsOTEntitled = bOTEntitle;
                                            #endregion
                                        }
                                        UpdateAttdnData(_paraA, ref drAttnProcData);
                                        //UpdateAttdnData("EDIT", GroupSysID, "OUT", sEmpSysID, sPlantID, sWorkingDate.Trim(), sShiftSystemID, sDate, sOutTime, bManualOutTime, sOutTimeRowID, "", false, iTotalOTHr, "", ref drAttnProcData);
                                        drAttnProcData.EndEdit();
                                    }//bAttnIsLock
                                }//dvAttnProcData.Count
                            }//DateDiffer
                        }//dsEmpInfo loop
                        //clsStaticInfo obj = new clsStaticInfo();
                        SaveDataSets(dsRawData, dsAttnProcData);
                    }
                    sToDt = sFrmDt.AddDays(1);
                }
                bValid = true;
                return bValid;
            }
            catch (Exception ex)
            {
                throw ex;
                //Cursor = Cursors.Default;
                //System.Windows.Forms.MessageBox.Show(this, ex.ToString(), "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return bValid;
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

                #endregion clean variables
            }
        }//End Function 
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


        void xGetShortLeave(string sWorkingDate, DateTime _empIntime, DateTime _empOuttime, dicShiftDft _ShiftDft, bool IsTowShortLeaveAllowedInaDay, out int _CountShortLeave)
        {
            try
            {
                _CountShortLeave = 0;
                DateTime _cust_empIntime = DateTime.Now;
                DateTime _cust_empOuttime = DateTime.Now;

                var _ShiftInTime = MakeDateTime(sWorkingDate, _ShiftDft.InTime.ToString());
                string OUTDate = _empOuttime.ToString("dd-MMM-yyyy");
                var _ShiftOutTime = MakeDateTime(OUTDate, _ShiftDft.OutTime.ToString());
                if (_ShiftInTime > _empIntime)
                {
                    //_cust_empIntime = _ShiftInTime;
                }
                else//late IN
                {
                    //_cust_empIntime = _empIntime;
                    TimeSpan INlate = _empIntime - _ShiftInTime;
                    var INlateMin = ((INlate.Hours * 60) + INlate.Minutes);
                    if ((INlateMin - _ShiftDft.LateMargin) > 0)//deduct margin
                    {
                        _CountShortLeave++;
                    }
                }

                if (_ShiftOutTime > _empOuttime)//early OUT
                {
                    //_cust_empOuttime = _empOuttime;
                    TimeSpan OUTearly = _ShiftOutTime - _empOuttime;
                    var OUTEarlyMin = ((OUTearly.Hours * 60) + OUTearly.Minutes);
                    if ((OUTEarlyMin) > 0)//deduct margin
                    {
                        _CountShortLeave++;
                    }
                }
                else
                {
                    // _cust_empOuttime = _ShiftOutTime;
                }

                if (IsTowShortLeaveAllowedInaDay == false)
                {
                    if (_CountShortLeave > 1)
                    {
                        _CountShortLeave = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetShortLeave(ParaShortLeaveHalfDayAbsent sldd, DateTime _empIntime, DateTime _empOuttime, out int _CountShortLeave, out bool IsHalfDay)
        {
            try
            {
                IsHalfDay = false;
                _CountShortLeave = 0;
                DateTime _cust_empIntime = DateTime.Now;
                DateTime _cust_empOuttime = DateTime.Now;

                var _ShiftInTime = MakeDateTime(sldd.sWorkingDate, sldd._ShiftDft.InTime.ToString());
                //------------------------
                string _s_end_time_n = string.Empty;
                GetOutTime(sldd._ShiftDft, sldd.sDate, out _s_end_time_n);

                //ShiftOUTTime = Convert.ToDateTime(_s_end_time_n).ToString("dd-MMM-yyyy HH:mm:ss");
                //-------------------------

                // var _s_end_time = Convert.ToDateTime(_ShiftInTime).AddMinutes(sldd._ShiftDft.WorkingHour);
                string OUTDate = Convert.ToDateTime(_s_end_time_n).ToString("dd-MMM-yyyy");

                var _ShiftOutTime = MakeDateTime(OUTDate, sldd._ShiftDft.OutTime.ToString());
                if (_ShiftInTime > _empIntime)
                {
                    //_cust_empIntime = _ShiftInTime;
                }
                else//late IN
                {
                    //_cust_empIntime = _empIntime;
                    TimeSpan INlate = _empIntime - _ShiftInTime;
                    var INlateMin = ((INlate.Hours * 60) + INlate.Minutes);
                    if ((INlateMin - sldd._ShiftDft.LateMargin) > 0)//deduct margin
                    {
                        //is under 120
                        if (INlateMin <= Convert.ToDouble(sldd._ShiftDft.ShortLeaveMaxLimit))
                        {
                            _CountShortLeave++;
                        }
                        else
                        {
                            IsHalfDay = true;
                        }
                    }
                }

                if (_ShiftOutTime > _empOuttime)//early OUT
                {
                    //_cust_empOuttime = _empOuttime;
                    TimeSpan OUTearly = _ShiftOutTime - _empOuttime;
                    var OUTEarlyMin = ((OUTearly.Hours * 60) + OUTearly.Minutes);
                    if ((OUTEarlyMin) > 0)//no early out margin
                    {
                        if (OUTEarlyMin <= Convert.ToDouble(sldd._ShiftDft.ShortLeaveMaxLimit))
                        {
                            _CountShortLeave++;
                        }
                        else
                        {
                            IsHalfDay = true;
                        }
                    }
                }
                else
                {
                    // _cust_empOuttime = _ShiftOutTime;
                }


                if (_CountShortLeave > 1)
                {
                    if (sldd.IsTowShortLeaveAllowedInaDay == false)
                    {
                        _CountShortLeave = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void xShortLeaveHalfDayAbsent(string empIntime, string empOuttime, string sWorkingDate, string sDate, dicShiftDft _ShiftDft, bool IsOTOverHalfDay, out string DayStatus, out bool IsShortLeave, out bool IsStatusChanged, out bool IsReversed, out int CountShortLeave)
        {
            DayStatus = "";
            IsShortLeave = false;
            CountShortLeave = 0;
            //bool IsLunchTimeCrossed = false;//IsOTOverHalfDay
            //bool IsFirstHalfAbsent = false;
            IsStatusChanged = false;
            int _Work_Duration = 0;
            IsReversed = false;//as per updated punch time DayStatus might be changed

            bool IsShortLeaveAllowed = true;
            bool IsHalfDayPresentAllowed = true;
            bool IsTowShortLeaveAllowedInaDay = true;

            try
            {
                if (IsShortLeaveAllowed == false && IsHalfDayPresentAllowed == false)
                {
                    //do nothing
                }
                else
                {
                    if (empIntime != "00:00:00" && empOuttime != "00:00:00")
                    {
                        empIntime = sWorkingDate + " " + empIntime;
                        empOuttime = sDate + " " + empOuttime;
                        DateTime _empIntime = Convert.ToDateTime(empIntime);
                        DateTime _empOuttime = Convert.ToDateTime(empOuttime);

                        DateTime _cust_empIntime = _empIntime;
                        DateTime _cust_empOuttime = _empOuttime;

                        if (_empIntime < _empOuttime)
                        {
                            ///Shift Time
                            double _Shift_Duration = 0;
                            _Shift_Duration = _ShiftDft.WorkingHour;

                            ///Work Time
                            GetWorkDuration(sWorkingDate, _empIntime, _empOuttime, _ShiftDft, out _Work_Duration);
                            ///Short Leave Count                            
                            ///GetShortLeave(sWorkingDate, _empIntime, _empOuttime, _ShiftDft, IsTowShortLeaveAllowedInaDay, out CountShortLeave);


                            double _lack_worktime = 0;
                            _lack_worktime = _Shift_Duration - _Work_Duration;//10-8=2

                            if (_lack_worktime > _ShiftDft.LateMargin)
                            {
                                #region Check
                                if (_lack_worktime <= Convert.ToDouble(_ShiftDft.ShortLeaveMaxLimit))
                                {
                                    //shortleave
                                    if (IsShortLeaveAllowed)//plant wise shortleave policy
                                    {
                                        IsShortLeave = true;
                                    }
                                    IsReversed = true;
                                }
                                else if (_lack_worktime <= Convert.ToDouble(_ShiftDft.HalfDayAbsentMaxLimit))
                                {
                                    //half day Present       
                                    if (IsHalfDayPresentAllowed)//plant wise shortleave policy for HDP
                                    {
                                        if (IsOTOverHalfDay == false)//OT will negetively effect not Half day absenteeism
                                        {
                                            DayStatus = "HDP";
                                            IsStatusChanged = true;
                                        }
                                    }
                                    IsShortLeave = false;
                                    IsReversed = false;
                                }
                                else
                                {
                                    //might be full day absent
                                    //if (_Work_Duration < Convert.ToDouble(_ShiftDft.ShortLeaveMaxLimit + _ShiftDft.HalfDayAbsentMaxLimit))
                                    // {
                                    if (IsShortLeaveAllowed)//plant wise shortleave policy for HDP
                                    {
                                        IsShortLeave = false;
                                    }

                                    if (IsHalfDayPresentAllowed)//plant wise shortleave policy for HDP
                                    {
                                        if (IsOTOverHalfDay == false)//OT will not negetively effect only Half day absenteeism will effect
                                        {
                                            DayStatus = "A";
                                            IsStatusChanged = true;
                                            IsReversed = false;
                                        }
                                    }
                                }//LateInMargin 
                                #endregion

                            }//if has lack
                            else
                            {
                                //if two status is different keep the original
                                IsReversed = true;
                                IsShortLeave = false;
                            }
                        }// from date<todate
                        else
                        {
                            //if two status is different keep the original
                            IsReversed = true;
                            IsShortLeave = false;
                        }
                    }//value not null
                    else
                    {
                        //if two status is different keep the original
                        IsReversed = true;
                        IsShortLeave = false;
                    }
                }//  if (IsShortLeaveAllowed, IsHalfDayPresentAllowed) either or both   
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void ShortLeaveHalfDayAbsent(ParaShortLeaveHalfDayAbsent sldd)
        {
            sldd.DayStatus = "";
            sldd.IsShortLeave = false;
            sldd.CountShortLeave = 0;
            int CountShortLeave = 0;
            bool IsHalfDayPossible = false;
            //bool IsLunchTimeCrossed = false;//IsOTOverHalfDay
            //bool IsFirstHalfAbsent = false;
            sldd.IsStatusChanged = false;
            int _Work_Duration = 0;
            sldd.IsReversed = false;//as per updated punch time DayStatus might be changed



            //sldd.IsShortLeaveAllowed = true;
            //sldd.IsHalfDayPresentAllowed = true;
            //sldd.IsTowShortLeaveAllowedInaDay = true;

            try
            {
                if (sldd.IsShortLeaveAllowed == false && sldd.IsHalfDayPresentAllowed == false)
                {
                    //do nothing
                }
                else
                {
                    if (sldd.sInTime != "00:00:00" && sldd.sOutTime != "00:00:00")
                    {
                        sldd.sInTime = sldd.sWorkingDate + " " + sldd.sInTime;
                        if (sldd.HasManualOutTime)
                        {
                            sldd.sOutTime = sldd.ManualDate + " " + sldd.sOutTime;
                        }
                        else
                        {
                            sldd.sOutTime = sldd.ManualDate + " " + sldd.sOutTime;//by monir
                            //sldd.sOutTime = sldd.sDate + " " + sldd.sOutTime;
                        }
                        DateTime _empIntime = Convert.ToDateTime(sldd.sInTime);
                        DateTime _empOuttime = Convert.ToDateTime(sldd.sOutTime);

                        DateTime _cust_empIntime = _empIntime;
                        DateTime _cust_empOuttime = _empOuttime;

                        if (_empIntime < _empOuttime)
                        {
                            ///Shift Time
                            double _Shift_Duration = 0;
                            if (sldd.PaidHours == 0)//if PaidHours is not found shift working hour will b considered.
                            {
                                _Shift_Duration = sldd._ShiftDft.WorkingHour;
                            }
                            else
                            {
                                _Shift_Duration = sldd.PaidHours * 60;
                            }

                            ///Work Time
                            GetWorkDuration(sldd.sWorkingDate, _empIntime, _empOuttime, sldd._ShiftDft, out _Work_Duration);
                            ///Short Leave Count  (shift time dependant)                           
                            GetShortLeave(sldd, _empIntime, _empOuttime, out CountShortLeave, out IsHalfDayPossible);
                            //GetShortLeave(sldd, sldd.sWorkingDate, _empIntime, _empOuttime, sldd._ShiftDft, sldd.IsTowShortLeaveAllowedInaDay, out CountShortLeave);                            
                            if (sldd.IsShortLeaveAllowed == false)//plant wise shortleave policy
                            {
                                sldd.IsShortLeave = false;
                                CountShortLeave = 0;
                                sldd.CountShortLeave = 0;
                            }
                            else
                            {
                                if (CountShortLeave > 0)
                                {
                                    sldd.IsShortLeave = true;
                                    sldd.CountShortLeave = CountShortLeave;
                                }
                                else
                                {
                                    sldd.IsShortLeave = false;
                                    sldd.CountShortLeave = 0;
                                }
                                //sldd.IsShortLeave = true;
                            }



                            double _lack_worktime = 0;
                            _lack_worktime = _Shift_Duration - _Work_Duration;//10-8=2

                            if (_lack_worktime > sldd._ShiftDft.LateMargin)
                            {
                                if (_Work_Duration < Convert.ToDouble(sldd._ShiftDft.HalfDayAbsentMaxLimit))//work hour is less than 3.5 hrs
                                {
                                    //absent
                                    sldd.IsShortLeave = false;
                                    sldd.CountShortLeave = 0;
                                    if (sldd.IsHalfDayPresentAllowed)//plant wise shortleave policy for HDP
                                    {
                                        //nullify OT value
                                        //sldd.ShouldNullifyOTValue = true;
                                        if (sldd.IsOTentitled && sldd.IsOTOverHalfDay)
                                        {
                                            //ot entitle people will have no effect on daystatus
                                        }
                                        else
                                        {
                                            sldd.DayStatus = "A";
                                            sldd.IsStatusChanged = true;
                                            sldd.IsReversed = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (IsHalfDayPossible)//lac of worktime greater than 2hrs
                                    {
                                        //hdp
                                        if (sldd.IsOTentitled && sldd.IsOTOverHalfDay)
                                        {
                                            //ot entitle people will have no effect on daystatus
                                        }
                                        else
                                        {
                                            if (sldd.IsHalfDayPresentAllowed)//plant wise shortleave policy for HDP
                                            {
                                                sldd.DayStatus = "HDP";
                                                sldd.IsStatusChanged = true;
                                            }
                                        }

                                        sldd.CountShortLeave--;
                                        if (sldd.CountShortLeave > 0)//if tow short leave then reduce one but keep alive other one
                                        {
                                            sldd.IsShortLeave = true;
                                        }
                                        else
                                        {
                                            sldd.IsShortLeave = false;
                                        }
                                        sldd.IsReversed = false;
                                    }
                                    else
                                    {
                                        sldd.IsReversed = true;
                                    }
                                }
                            }//if has lack
                            else
                            {
                                //if two status is different keep the original
                                sldd.IsReversed = true;
                                sldd.IsShortLeave = false;
                            }
                        }// from date<todate
                        else
                        {
                            //if two status is different keep the original
                            sldd.IsReversed = true;
                            sldd.IsShortLeave = false;
                        }
                    }//value not null
                    else if (sldd.sInTime != "00:00:00" && sldd.sOutTime == "00:00:00")
                    {
                        //only intime
                        sldd.sInTime = sldd.sWorkingDate + " " + sldd.sInTime;
                        sldd.sOutTime = sldd.ManualDate + " " + sldd.sOutTime;//by monir 
                        //sldd.sOutTime = sldd.sDate + " " + sldd.sOutTime;
                        DateTime _empIntime = Convert.ToDateTime(sldd.sInTime);
                        DateTime _empOuttime = Convert.ToDateTime(sldd.sOutTime);

                        DateTime _cust_empIntime = _empIntime;
                        DateTime _cust_empOuttime = _empOuttime;
                        GetShortLeave(sldd, _empIntime, _empOuttime, out CountShortLeave, out IsHalfDayPossible);
                        sldd.CountShortLeave = CountShortLeave;

                        if (sldd.IsShortLeaveAllowed)//plant wise shortleave policy
                        {
                            if (sldd.CountShortLeave > 0)//if tow short leave then reduce one but keep alive other one
                            {
                                sldd.IsShortLeave = true;
                            }
                            else
                            {
                                sldd.IsShortLeave = false;
                            }
                        }//allowed
                        else
                        {
                            sldd.IsShortLeave = false;
                            sldd.CountShortLeave = 0;
                        }
                    }
                    else if (sldd.sInTime == "00:00:00" && sldd.sOutTime != "00:00:00")
                    {
                        //only outtime
                        sldd.sInTime = sldd.sWorkingDate + " " + sldd.sInTime;
                        sldd.sOutTime = sldd.ManualDate + " " + sldd.sOutTime;
                        DateTime _empIntime = Convert.ToDateTime(sldd.sInTime);
                        DateTime _empOuttime = Convert.ToDateTime(sldd.sOutTime);

                        DateTime _cust_empIntime = _empIntime;
                        DateTime _cust_empOuttime = _empOuttime;
                        GetShortLeave(sldd, _empIntime, _empOuttime, out CountShortLeave, out IsHalfDayPossible);

                        sldd.CountShortLeave = CountShortLeave;
                        if (sldd.IsShortLeaveAllowed)//plant wise shortleave policy
                        {
                            if (sldd.CountShortLeave > 0)//if tow short leave then reduce one but keep alive other one
                            {
                                sldd.IsShortLeave = true;
                            }
                            else
                            {
                                sldd.IsShortLeave = false;
                            }
                        }//allowed
                        else
                        {
                            sldd.IsShortLeave = false;
                            sldd.CountShortLeave = 0;
                        }
                    }
                    else
                    {
                        //if two status is different keep the original
                        sldd.IsReversed = true;
                        sldd.IsShortLeave = false;
                    }
                }//  if (IsShortLeaveAllowed, IsHalfDayPresentAllowed) either or both   
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void xCalculateOT(bool IsOriginalDateOTApplicable, DataSet dsOTPMPolicy, bool IsOTBasedOnPerMinute, string sDayType, bool bOTEntitle, bool HasManualOutTime, string ManualDate, double _PaidHours, dicShiftDft _ShiftDft, bool IsOTOverHalfDay, DataTable dtOTSlabEmp, DataSet dsOTSlabGen, string sEmpSysID, string sWorkingDate, string sDate, string sInTime, string sOTStartTime, string sMinOT, string sOutTime, out decimal iTotalOTHr, out decimal _OT_inTime, out decimal _OT_outTime)
        {
            DataView dvOTSlabEmp = null;
            iTotalOTHr = 0;
            bool bIsOTExtentNextSlab = false;
            bool bIsTotalWorkTimeAsOT = false;
            decimal dfirstSlab = 0;
            string sOTDayType = "";
            //bool _IsOTbasedonPerMin = false;
            //-------------
            //DataSet dsHRSetting
            _OT_inTime = 0;
            _OT_outTime = 0;
            try
            {
                HROTSetting vHROTSetting = _HROTSetting;
                if (IsOTOverHalfDay == false)//Normal OT (OT based on Plant setting)
                {
                    if (vHROTSetting.IsPunchBasedOT)
                    {
                        // if (sOutTime != "00:00:00" & Convert.ToDateTime(sOTStartTime) < Convert.ToDateTime(sOutTime)/* & bOTEntitle == true*/)
                        bool IsOTShouldBeCalculated = false;

                        string Validate_sOutTime = string.Empty;
                        //string Validate_OTStartTime = string.Empty;
                        if (HasManualOutTime)
                        {
                            Validate_sOutTime = ManualDate + " " + sOutTime;
                            //Validate_OTStartTime = ManualDate + " " + sOTStartTime;
                        }
                        else
                        {
                            Validate_sOutTime = sDate + " " + sOutTime;
                            //Validate_OTStartTime = sDate + " " + sOTStartTime;
                        }

                        if (Convert.ToDateTime(sDate + " " + sOTStartTime) < Convert.ToDateTime(Validate_sOutTime))
                        {
                            IsOTShouldBeCalculated = true;
                        }
                        else if (sDayType == "W" || sDayType == "H")//during w or h early out allowed
                        {
                            IsOTShouldBeCalculated = true;
                        }

                        if (sOutTime != "00:00:00" && (IsOTShouldBeCalculated)/* & bOTEntitle == true*/)
                        {
                            #region Normal OT
                            dvOTSlabEmp = new DataView();
                            dvOTSlabEmp.Table = dtOTSlabEmp;
                            dvOTSlabEmp.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                            if (dvOTSlabEmp.Count > 0)
                            {
                                sOTDayType = dvOTSlabEmp[0].Row["DayType"].ToString();
                                dfirstSlab = (Convert.ToDecimal(dvOTSlabEmp[0].Row["firstSlab"].ToString()) * 60);
                                bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsOTExtentNextSlab"].ToString());
                                bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsTotalWorkTimeAsOT"].ToString());
                            }
                            else
                            {
                                var dvOTSlabgen = new DataView();
                                dvOTSlabgen.Table = dsOTSlabGen.Tables[0];
                                dvOTSlabgen.RowFilter = "DayType = '" + sDayType + "'";

                                if (dvOTSlabgen.Count > 0)
                                {
                                    // sOTDayType = dvOTSlabgen[0]["DayType"].ToString();
                                    dfirstSlab = (Convert.ToDecimal(dvOTSlabgen[0]["firstSlab"].ToString()) * 60);
                                    bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabgen[0]["IsOTExtentNextSlab"].ToString());
                                    bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabgen[0]["IsTotalWorkTimeAsOT"].ToString());
                                }

                            }

                            //compancatory original date full day ot
                            if (IsOriginalDateOTApplicable == true)
                            {
                                bIsTotalWorkTimeAsOT = true;
                            }

                            if (bIsTotalWorkTimeAsOT == true)
                            {
                                if (sInTime != "00:00:00")
                                {
                                    sInTime = sWorkingDate + " " + sInTime;

                                    if (HasManualOutTime)
                                    {
                                        sOutTime = ManualDate + " " + sOutTime;
                                    }
                                    else
                                    {
                                        sOutTime = sDate + " " + sOutTime;
                                    }

                                    TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sInTime);
                                    iTotalOTHr = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
                                    iTotalOTHr = iTotalOTHr - _ShiftDft.BreakPeriod;
                                }
                            }
                            else if (bIsTotalWorkTimeAsOT == false)
                            {
                                if (HasManualOutTime)
                                {
                                    sOutTime = ManualDate + " " + sOutTime;
                                }
                                else
                                {
                                    sOutTime = sDate + " " + sOutTime;
                                }
                                sOTStartTime = sDate + " " + sOTStartTime;
                                //Modify Date:- 21-Jul-2018 By Prodipta
                                TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sOTStartTime);
                                //TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime("00:00:00");
                                iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                            }


                            if (IsOTBasedOnPerMinute)
                            {
                                if (dsOTPMPolicy != null)
                                {
                                    DataView dv = new DataView(dsOTPMPolicy.Tables[0]);
                                    dv.RowFilter = "OverstayOrEarlyOut='" + iTotalOTHr + "'";
                                    if (dv.Count > 0)
                                    {
                                        if (sDayType == "W" || sDayType == "H")
                                        {
                                            iTotalOTHr = Convert.ToDecimal(GetNumData(dv[0]["OffDayAllotedOT"].ToString()));
                                        }
                                        else
                                        {
                                            iTotalOTHr = Convert.ToDecimal(GetNumData(dv[0]["AllotedOT"].ToString()));
                                        }
                                    }//count
                                }//dsOTPMPolicy
                            }//IsOTBasedOnPerMinute
                            else
                            {
                                ////check max limit of ot as per slab
                                if (dfirstSlab > 0)
                                {
                                    if (iTotalOTHr > dfirstSlab)
                                    {
                                        iTotalOTHr = dfirstSlab;
                                    }
                                }
                                /////if min OT is not found s/he will b avoided for OT
                                if (iTotalOTHr < _HROTSetting.MinimumOTMinute)
                                {
                                    bOTEntitle = false;
                                }
                            }




                            #region Commented

                            //int iMinOT = 1;
                            //iMinOT = vHROTSetting.MinimumOTMinute;
                            //sFractionCalculate = vHROTSetting.OTFractionCalculation;

                            //if (string.IsNullOrEmpty(sMinOT.Trim()) == false)
                            //{
                            //    iMinOT = Convert.ToInt32(sMinOT.Trim());
                            //}

                            //if (sFractionCalculate.ToUpper().Trim() == "ROUND")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else if (sFractionCalculate.ToUpper().Trim() == "ROUND UP")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Ceiling((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else if (sFractionCalculate.ToUpper().Trim() == "ROUND DOWN")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Floor((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //} 

                            #endregion

                            ///main OT
                            if (bOTEntitle)
                            {
                                if (IsOTBasedOnPerMinute == false)
                                {
                                    if (_HROTSetting.IsRoundOptionApplicable)
                                    {
                                        iTotalOTHr = RoundedOT(iTotalOTHr, vHROTSetting.RoundFigureForOT, vHROTSetting.OTFractionCalculation);
                                        if (iTotalOTHr < _HROTSetting.MinimumOTMinute)//if calculated value is less make it min OT
                                        {
                                            iTotalOTHr = _HROTSetting.PayableMinimumOT;
                                        }
                                    }
                                    else
                                    {
                                        //iTotalOTHr = iTotalOTHr;
                                    }
                                }
                            }
                            else
                            {
                                iTotalOTHr = 0;
                            }
                            #endregion
                        }
                    }
                    if (iTotalOTHr < 0)
                    {
                        iTotalOTHr = 0;
                    }
                }
                else // IN OUT and Duration consider for OT //jindal
                {
                    if (sOutTime != "00:00:00" && sInTime != "00:00:00")
                    {
                        #region Jindal ot
                        var _sInTime = sWorkingDate + " " + sInTime;
                        var _sOutTime = string.Empty;
                        //var _sOutTime = sDate + " " + sOutTime;
                        if (HasManualOutTime)
                        {
                            _sOutTime = ManualDate + " " + sOutTime;
                        }
                        else
                        {
                            _sOutTime = sDate + " " + sOutTime;
                        }


                        DateTime _xempIntime = Convert.ToDateTime(_sInTime);
                        DateTime _xempOuttime = Convert.ToDateTime(_sOutTime);

                        if (_xempIntime.ToString("dd-MMM-yyyy") == "24-SEP-2018")
                        {

                        }

                        if (_xempIntime < _xempOuttime)
                        {
                            #region +- OT

                            if (bOTEntitle)
                            {
                                _OT_inTime = EarlyInLateIn(sInTime, sWorkingDate, _ShiftDft);
                                _OT_outTime = EarlyOutLateOut(_PaidHours, sOutTime, sDate, _ShiftDft);
                                iTotalOTHr = _OT_outTime + _OT_inTime;

                            }//bOTEntitle
                            else
                            {
                                _OT_inTime = 0;
                                _OT_outTime = 0;
                                iTotalOTHr = 0;
                            }
                            #endregion
                        }
                        #endregion
                    }// IN OUT ok                   
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CalculateOT(ParaOT para, out decimal iTotalOTHr, out decimal _OT_inTime, out decimal _OT_outTime)
        {
            DataView dvOTSlabEmp = null;
            iTotalOTHr = 0;
            bool bIsOTExtentNextSlab = false;
            bool bIsTotalWorkTimeAsOT = false;
            decimal dfirstSlab = 0;
            string sOTDayType = "";
            //bool _IsOTbasedonPerMin = false;
            //-------------
            //DataSet dsHRSetting
            _OT_inTime = 0;
            _OT_outTime = 0;
            try
            {
                bool IsOriginalDateOTApplicable = para.IsOriginalDateOTApplicable;
                DataSet dsOTPMPolicy = para.dsOTPerMinPolicy;
                bool IsOTBasedOnPerMinute = para.IsOTBasedOnPerMinute;
                string sDayType = para.sDayType;
                bool bOTEntitle = para.bOTEntitle;
                bool HasManualOutTime = para.HasManualOutTime;
                string ManualDate = para.ManualDate;
                double _PaidHours = para._PaidHours;
                dicShiftDft _ShiftDft = para._ShiftDft;
                bool IsOTOverHalfDay = para.IsOTOverHalfDay;
                DataTable dtOTSlabEmp = para.dtOTSlabEmp;
                DataSet dsOTSlabGen = para.dsOTSlabGen;
                string sEmpSysID = para.sEmpSysID;
                string sWorkingDate = para.sDate;
                string sDate = para.sDate;
                string sInTime = para.sInTime;
                string sOTStartTime = para.sOTStartTime;
                string sMinOT = para.sMinOT;
                string sOutTime = para.sOutTime;
                int _work_duration = 0;
                //bool IsOriginalDateOTApplicable, DataSet dsOTPMPolicy, bool IsOTBasedOnPerMinute, string sDayType, bool bOTEntitle
                //, bool HasManualOutTime, string ManualDate, double _PaidHours, dicShiftDft _ShiftDft, bool IsOTOverHalfDay,
                //DataTable dtOTSlabEmp, DataSet dsOTSlabGen, string sEmpSysID, string sWorkingDate, string sDate, string sInTime,
                //string sOTStartTime, string sMinOT, string sOutTime, 

                HROTSetting vHROTSetting = _HROTSetting;
                if (IsOTOverHalfDay == false)//Normal OT (OT based on Plant setting)
                {
                    if (vHROTSetting.IsPunchBasedOT)
                    {
                        string _s_end_time = string.Empty;
                        GetOutTime(_ShiftDft, sDate, out _s_end_time);
                        string OT_startTime = Convert.ToDateTime(_s_end_time).ToString("dd-MMM-yyyy") + " " + sOTStartTime;

                        // if (sOutTime != "00:00:00" & Convert.ToDateTime(sOTStartTime) < Convert.ToDateTime(sOutTime)/* & bOTEntitle == true*/)
                        bool IsOTShouldBeCalculated = false;

                        string Validate_sOutTime = string.Empty;
                        //string Validate_OTStartTime = string.Empty;
                        //if (HasManualOutTime)
                        //{
                        Validate_sOutTime = ManualDate + " " + sOutTime;
                        //Validate_OTStartTime = ManualDate + " " + sOTStartTime;
                        //}
                        //else
                        //{
                        //    Validate_sOutTime = sDate + " " + sOutTime;
                        //    //Validate_OTStartTime = sDate + " " + sOTStartTime;
                        //}

                        //if (Convert.ToDateTime(sDate + " " + sOTStartTime) < Convert.ToDateTime(Validate_sOutTime))
                        if (Convert.ToDateTime(OT_startTime) < Convert.ToDateTime(Validate_sOutTime))
                        {
                            IsOTShouldBeCalculated = true;
                        }
                        else if (sDayType == "W" || sDayType == "H")//during w or h early out allowed
                        {
                            IsOTShouldBeCalculated = true;
                            _work_duration = 0;//calculate here
                            //========================================================999
                            //var it = sWorkingDate + " " + sInTime;
                            //var ot = ManualDate + " " + sOutTime;
                            //DateTime _empIntime = Convert.ToDateTime(it);
                            //DateTime _empOuttime = Convert.ToDateTime(ot);

                            //GetWorkDuration(sWorkingDate, _empIntime, _empOuttime, _ShiftDft, out _lunch_time_for_deduction);
                        }

                        if (sOutTime != "00:00:00" && (IsOTShouldBeCalculated)/* & bOTEntitle == true*/)
                        {
                            #region Normal OT
                            dvOTSlabEmp = new DataView();
                            dvOTSlabEmp.Table = dtOTSlabEmp;
                            dvOTSlabEmp.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                            if (dvOTSlabEmp.Count > 0)
                            {
                                sOTDayType = dvOTSlabEmp[0].Row["DayType"].ToString();
                                dfirstSlab = (Convert.ToDecimal(dvOTSlabEmp[0].Row["firstSlab"].ToString()) * 60);
                                bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsOTExtentNextSlab"].ToString());
                                bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsTotalWorkTimeAsOT"].ToString());
                            }
                            else
                            {
                                var dvOTSlabgen = new DataView();
                                dvOTSlabgen.Table = dsOTSlabGen.Tables[0];
                                dvOTSlabgen.RowFilter = "DayType = '" + sDayType + "'";

                                if (dvOTSlabgen.Count > 0)
                                {
                                    // sOTDayType = dvOTSlabgen[0]["DayType"].ToString();
                                    dfirstSlab = (Convert.ToDecimal(dvOTSlabgen[0]["firstSlab"].ToString()) * 60);
                                    bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabgen[0]["IsOTExtentNextSlab"].ToString());
                                    bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabgen[0]["IsTotalWorkTimeAsOT"].ToString());
                                }

                            }

                            //compancatory original date full day ot
                            if (IsOriginalDateOTApplicable == true)
                            {
                                bIsTotalWorkTimeAsOT = true;
                            }

                            if (bIsTotalWorkTimeAsOT == true)
                            {
                                if (sInTime != "00:00:00")
                                {
                                    sInTime = sWorkingDate + " " + sInTime;
                                    sOutTime = ManualDate + " " + sOutTime;

                                    //TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sInTime);
                                    //iTotalOTHr = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
                                    ////iTotalOTHr = iTotalOTHr - _ShiftDft.BreakPeriod;

                                    DateTime _empIntime = Convert.ToDateTime(sWorkingDate + " " + Convert.ToDateTime(sInTime).ToString("HH:mm") + ":00");
                                    DateTime _empOuttime = Convert.ToDateTime(ManualDate + " " + Convert.ToDateTime(sOutTime).ToString("HH:mm") + ":00");

                                    GetWorkDurationIncludingOT(sWorkingDate, _empIntime, _empOuttime, _ShiftDft, out _work_duration);
                                    iTotalOTHr = _work_duration;
                                }
                            }
                            else if (bIsTotalWorkTimeAsOT == false)
                            {
                                //if (HasManualOutTime)
                                //{
                                sOutTime = ManualDate + " " + sOutTime;
                                //}
                                //else
                                //{
                                //    sOutTime = sDate + " " + sOutTime;
                                //}
                                //sOTStartTime = sDate + " " + sOTStartTime;
                                //Modify Date:- 21-Jul-2018 By Prodipta
                                TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(OT_startTime);
                                //TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime("00:00:00");
                                iTotalOTHr = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);

                            }


                            if (IsOTBasedOnPerMinute)
                            {
                                if (dsOTPMPolicy != null)
                                {
                                    DataView dv = new DataView(dsOTPMPolicy.Tables[0]);
                                    dv.RowFilter = "OverstayOrEarlyOut='" + iTotalOTHr + "'";
                                    if (dv.Count > 0)
                                    {
                                        if (sDayType == "W" || sDayType == "H")
                                        {
                                            iTotalOTHr = Convert.ToDecimal(GetNumData(dv[0]["OffDayAllotedOT"].ToString()));
                                        }
                                        else
                                        {
                                            iTotalOTHr = Convert.ToDecimal(GetNumData(dv[0]["AllotedOT"].ToString()));
                                        }
                                    }//count
                                }//dsOTPMPolicy
                            }//IsOTBasedOnPerMinute
                            else
                            {
                                ////check max limit of ot as per slab
                                if (dfirstSlab > 0)
                                {
                                    if (iTotalOTHr > dfirstSlab)
                                    {
                                        iTotalOTHr = dfirstSlab;
                                    }
                                }
                                /////if min OT is not found s/he will b avoided for OT
                                if (iTotalOTHr < _HROTSetting.MinimumOTMinute)
                                {
                                    bOTEntitle = false;
                                }
                            }




                            #region Commented

                            //int iMinOT = 1;
                            //iMinOT = vHROTSetting.MinimumOTMinute;
                            //sFractionCalculate = vHROTSetting.OTFractionCalculation;

                            //if (string.IsNullOrEmpty(sMinOT.Trim()) == false)
                            //{
                            //    iMinOT = Convert.ToInt32(sMinOT.Trim());
                            //}

                            //if (sFractionCalculate.ToUpper().Trim() == "ROUND")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else if (sFractionCalculate.ToUpper().Trim() == "ROUND UP")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Ceiling((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else if (sFractionCalculate.ToUpper().Trim() == "ROUND DOWN")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Floor((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //} 

                            #endregion

                            ///main OT
                            if (bOTEntitle)
                            {
                                if (IsOTBasedOnPerMinute == false)
                                {
                                    if (_HROTSetting.IsRoundOptionApplicable)
                                    {
                                        iTotalOTHr = RoundedOT(iTotalOTHr, vHROTSetting.RoundFigureForOT, vHROTSetting.OTFractionCalculation);
                                        if (iTotalOTHr < _HROTSetting.MinimumOTMinute)//if calculated value is less make it min OT
                                        {
                                            iTotalOTHr = _HROTSetting.PayableMinimumOT;
                                        }
                                    }
                                    else
                                    {
                                        //iTotalOTHr = iTotalOTHr;
                                    }
                                }
                            }
                            else
                            {
                                iTotalOTHr = 0;
                            }
                            #endregion
                        }
                    }
                    if (iTotalOTHr < 0)
                    {
                        iTotalOTHr = 0;
                    }
                }
                else // IN OUT and Duration consider for OT //jindal
                {
                    if (sOutTime != "00:00:00" && sInTime != "00:00:00")
                    {
                        #region Jindal ot
                        var _sInTime = sWorkingDate + " " + sInTime;
                        var _sOutTime = string.Empty;
                        //var _sOutTime = sDate + " " + sOutTime;
                        //if (HasManualOutTime)
                        //{
                        _sOutTime = ManualDate + " " + sOutTime;
                        //}
                        //else
                        //{
                        //    _sOutTime = sDate + " " + sOutTime;
                        //}


                        DateTime _xempIntime = Convert.ToDateTime(_sInTime);
                        DateTime _xempOuttime = Convert.ToDateTime(_sOutTime);

                        if (_xempIntime.ToString("dd-MMM-yyyy") == "24-SEP-2018")
                        {

                        }

                        if (_xempIntime < _xempOuttime)
                        {
                            #region +- OT

                            if (bOTEntitle)
                            {
                                _OT_inTime = EarlyInLateIn(sInTime, sWorkingDate, _ShiftDft);
                                _OT_outTime = EarlyOutLateOut(_PaidHours, sOutTime, sDate, _ShiftDft);
                                iTotalOTHr = _OT_outTime + _OT_inTime;

                            }//bOTEntitle
                            else
                            {
                                _OT_inTime = 0;
                                _OT_outTime = 0;
                                iTotalOTHr = 0;
                            }
                            #endregion
                        }
                        #endregion
                    }// IN OUT ok                   
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void xCalculateOT(DataSet dsOTPMPolicy, bool IsOTBasedOnPerMinute, string sDayType, bool bOTEntitle, bool HasManualOutTime, string ManualDate, double _PaidHours, dicShiftDft _ShiftDft, bool IsOTOverHalfDay, DataTable dtOTSlabEmp, DataSet dsOTSlabGen, string sEmpSysID, string sWorkingDate, string sDate, string sInTime, string sOTStartTime, string sMinOT, string sOutTime, out decimal iTotalOTHr, out decimal _OT_inTime, out decimal _OT_outTime)
        {
            DataView dvOTSlabEmp = null;
            iTotalOTHr = 0;
            bool bIsOTExtentNextSlab = false;
            bool bIsTotalWorkTimeAsOT = false;
            decimal dfirstSlab = 0;
            string sOTDayType = "";
            //bool _IsOTbasedonPerMin = false;
            //-------------
            //DataSet dsHRSetting
            _OT_inTime = 0;
            _OT_outTime = 0;
            try
            {
                HROTSetting vHROTSetting = _HROTSetting;
                if (IsOTOverHalfDay == false)//Normal OT (OT based on Plant setting)
                {
                    if (vHROTSetting.IsPunchBasedOT)
                    {
                        // if (sOutTime != "00:00:00" & Convert.ToDateTime(sOTStartTime) < Convert.ToDateTime(sOutTime)/* & bOTEntitle == true*/)
                        bool IsOTShouldBeCalculated = false;

                        string Validate_sOutTime = string.Empty;
                        //string Validate_OTStartTime = string.Empty;
                        if (HasManualOutTime)
                        {
                            Validate_sOutTime = ManualDate + " " + sOutTime;
                            //Validate_OTStartTime = ManualDate + " " + sOTStartTime;
                        }
                        else
                        {
                            Validate_sOutTime = sDate + " " + sOutTime;
                            //Validate_OTStartTime = sDate + " " + sOTStartTime;
                        }

                        if (Convert.ToDateTime(sDate + " " + sOTStartTime) < Convert.ToDateTime(Validate_sOutTime))
                        {
                            IsOTShouldBeCalculated = true;
                        }
                        else if (sDayType == "W" || sDayType == "H")//during w or h early out allowed
                        {
                            IsOTShouldBeCalculated = true;
                        }

                        if (sOutTime != "00:00:00" && (IsOTShouldBeCalculated)/* & bOTEntitle == true*/)
                        {
                            #region Normal OT
                            dvOTSlabEmp = new DataView();
                            dvOTSlabEmp.Table = dtOTSlabEmp;
                            dvOTSlabEmp.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                            if (dvOTSlabEmp.Count > 0)
                            {
                                sOTDayType = dvOTSlabEmp[0].Row["DayType"].ToString();
                                dfirstSlab = (Convert.ToDecimal(dvOTSlabEmp[0].Row["firstSlab"].ToString()) * 60);
                                bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsOTExtentNextSlab"].ToString());
                                bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsTotalWorkTimeAsOT"].ToString());
                            }
                            else
                            {
                                var dvOTSlabgen = new DataView();
                                dvOTSlabgen.Table = dsOTSlabGen.Tables[0];
                                dvOTSlabgen.RowFilter = "DayType = '" + sDayType + "'";

                                if (dvOTSlabgen.Count > 0)
                                {
                                    // sOTDayType = dvOTSlabgen[0]["DayType"].ToString();
                                    dfirstSlab = (Convert.ToDecimal(dvOTSlabgen[0]["firstSlab"].ToString()) * 60);
                                    bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabgen[0]["IsOTExtentNextSlab"].ToString());
                                    bIsTotalWorkTimeAsOT = Convert.ToBoolean(dvOTSlabgen[0]["IsTotalWorkTimeAsOT"].ToString());
                                }
                            }

                            if (bIsTotalWorkTimeAsOT == true)
                            {
                                if (sInTime != "00:00:00")
                                {
                                    sInTime = sWorkingDate + " " + sInTime;

                                    if (HasManualOutTime)
                                    {
                                        sOutTime = ManualDate + " " + sOutTime;
                                    }
                                    else
                                    {
                                        sOutTime = sDate + " " + sOutTime;
                                    }

                                    TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sInTime);
                                    iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                                }
                            }
                            else if (bIsTotalWorkTimeAsOT == false)
                            {
                                if (HasManualOutTime)
                                {
                                    sOutTime = ManualDate + " " + sOutTime;
                                }
                                else
                                {
                                    sOutTime = sDate + " " + sOutTime;
                                }
                                sOTStartTime = sDate + " " + sOTStartTime;
                                //Modify Date:- 21-Jul-2018 By Prodipta
                                TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime(sOTStartTime);
                                //TimeSpan tsOT = Convert.ToDateTime(sOutTime) - Convert.ToDateTime("00:00:00");
                                iTotalOTHr = ((tsOT.Hours * 60) + tsOT.Minutes);
                            }


                            if (IsOTBasedOnPerMinute)
                            {
                                if (dsOTPMPolicy != null)
                                {
                                    DataView dv = new DataView(dsOTPMPolicy.Tables[0]);
                                    dv.RowFilter = "OverstayOrEarlyOut='" + iTotalOTHr + "'";
                                    if (dv.Count > 0)
                                    {
                                        if (sDayType == "W" || sDayType == "H")
                                        {
                                            iTotalOTHr = Convert.ToDecimal(GetNumData(dv[0]["OffDayAllotedOT"].ToString()));
                                        }
                                        else
                                        {
                                            iTotalOTHr = Convert.ToDecimal(GetNumData(dv[0]["AllotedOT"].ToString()));
                                        }
                                    }//count
                                }//dsOTPMPolicy
                            }//IsOTBasedOnPerMinute
                            else
                            {
                                ////check max limit of ot as per slab
                                if (dfirstSlab > 0)
                                {
                                    if (iTotalOTHr > dfirstSlab)
                                    {
                                        iTotalOTHr = dfirstSlab;
                                    }
                                }
                                /////if min OT is not found s/he will b avoided for OT
                                if (iTotalOTHr < _HROTSetting.MinimumOTMinute)
                                {
                                    bOTEntitle = false;
                                }
                            }




                            #region Commented

                            //int iMinOT = 1;
                            //iMinOT = vHROTSetting.MinimumOTMinute;
                            //sFractionCalculate = vHROTSetting.OTFractionCalculation;

                            //if (string.IsNullOrEmpty(sMinOT.Trim()) == false)
                            //{
                            //    iMinOT = Convert.ToInt32(sMinOT.Trim());
                            //}

                            //if (sFractionCalculate.ToUpper().Trim() == "ROUND")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else if (sFractionCalculate.ToUpper().Trim() == "ROUND UP")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Ceiling((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else if (sFractionCalculate.ToUpper().Trim() == "ROUND DOWN")
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Floor((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //}
                            //else
                            //{
                            //    iTotalOTHr = Convert.ToInt32(Math.Round((double)iTotalOTHr / iMinOT)) * iMinOT;
                            //} 

                            #endregion

                            ///main OT
                            if (bOTEntitle)
                            {
                                if (IsOTBasedOnPerMinute == false)
                                {
                                    if (_HROTSetting.IsRoundOptionApplicable)
                                    {
                                        iTotalOTHr = RoundedOT(iTotalOTHr, vHROTSetting.RoundFigureForOT, vHROTSetting.OTFractionCalculation);
                                        if (iTotalOTHr < _HROTSetting.MinimumOTMinute)//if calculated value is less make it min OT
                                        {
                                            iTotalOTHr = _HROTSetting.PayableMinimumOT;
                                        }
                                    }
                                    else
                                    {
                                        //iTotalOTHr = iTotalOTHr;
                                    }
                                }
                            }
                            else
                            {
                                iTotalOTHr = 0;
                            }
                            #endregion
                        }
                    }
                }
                else // IN OUT and Duration consider for OT //jindal
                {
                    if (sOutTime != "00:00:00" && sInTime != "00:00:00")
                    {
                        #region Jindal ot
                        var _sInTime = sWorkingDate + " " + sInTime;
                        var _sOutTime = string.Empty;
                        //var _sOutTime = sDate + " " + sOutTime;
                        if (HasManualOutTime)
                        {
                            _sOutTime = ManualDate + " " + sOutTime;
                        }
                        else
                        {
                            _sOutTime = sDate + " " + sOutTime;
                        }


                        DateTime _xempIntime = Convert.ToDateTime(_sInTime);
                        DateTime _xempOuttime = Convert.ToDateTime(_sOutTime);

                        if (_xempIntime.ToString("dd-MMM-yyyy") == "24-SEP-2018")
                        {

                        }

                        if (_xempIntime < _xempOuttime)
                        {
                            #region +- OT

                            if (bOTEntitle)
                            {
                                _OT_inTime = EarlyInLateIn(sInTime, sWorkingDate, _ShiftDft);
                                _OT_outTime = EarlyOutLateOut(_PaidHours, sOutTime, sDate, _ShiftDft);
                                iTotalOTHr = _OT_outTime + _OT_inTime;

                            }//bOTEntitle
                            else
                            {
                                _OT_inTime = 0;
                                _OT_outTime = 0;
                                iTotalOTHr = 0;
                            }
                            #endregion
                        }
                        #endregion
                    }// IN OUT ok                   
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        decimal EarlyInLateIn(string sInTime, string sWorkingDate, dicShiftDft _ShiftDft)
        {
            decimal _OT_inTime = 0;
            decimal _OT_outTime_returnValue = 0;
            try
            {
                //get shift in time =>sInTime
                //get emp intime
                //deduct
                //check with margin
                DateTime _Emp_Intime = Convert.ToDateTime(sWorkingDate + " " + sInTime);//date and time
                var _ShiftTime = Convert.ToDateTime(_ShiftDft.InTime).ToString("HH:mm:ss");
                var _ShiftDateTime = sWorkingDate + " " + _ShiftTime;
                DateTime _Shift_Intime = Convert.ToDateTime(_ShiftDateTime); //only time

                TimeSpan _inOT = Convert.ToDateTime(_Shift_Intime) - Convert.ToDateTime(_Emp_Intime);
                _OT_inTime = ((_inOT.Hours * 60) + _inOT.Minutes);

                //int _deducted_value = 0;
                if (_OT_inTime > 0)//positive/early
                {
                    if (_ShiftDft.EarlyIn)
                    {
                        //check late margin
                        if (_ShiftDft.EarlyInMargin > _OT_inTime)
                        {
                            _OT_inTime = 0;
                        }
                        _OT_outTime_returnValue = RoundedOT(_OT_inTime, _ShiftDft.EarlyInRoundMargin, _ShiftDft.EarlyInRoundMarginType);

                        //deduct latein
                    }
                    else
                    {
                        _OT_inTime = 0;
                        _OT_outTime_returnValue = 0;
                    }
                }
                else//late
                {
                    if (_ShiftDft.LateIn)
                    {
                        if (_ShiftDft.LateInMargin >= Math.Abs(_OT_inTime))
                        {
                            _OT_inTime = 0;
                        }
                        // if (_ShiftDft.LateInMargin<=Math.Abs(_deducted_value))
                        // {
                        _OT_outTime_returnValue = RoundedOT(Math.Abs(_OT_inTime), _ShiftDft.LateInRoundMargin, _ShiftDft.LateInRoundMarginType) * (-1);
                        // }
                    }
                    else
                    {
                        _OT_inTime = 0;
                        _OT_outTime_returnValue = 0;
                    }
                }
                return _OT_outTime_returnValue;
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
        decimal EarlyOutLateOut(double _PaidHours, string sOutTime, string sWorkingDate, dicShiftDft _ShiftDft)
        {
            decimal _OT_outTime = 0;
            decimal _OT_outTime_returnValue = 0;
            try
            {
                //get shift in time =>sInTime
                //get emp intime
                //deduct
                //check with margin
                if (_ShiftDft.EarlyOut || _ShiftDft.LateOut)
                {
                    DateTime _Emp_Outtime = Convert.ToDateTime(sWorkingDate + " " + sOutTime);//date and time
                    var _ShiftEndTime = Convert.ToDateTime(_ShiftDft.OutTime).ToString("HH:mm:ss");
                    var _ShiftDateTime = sWorkingDate + " " + _ShiftEndTime;
                    DateTime _Shift_Outtime = Convert.ToDateTime(_ShiftDateTime); //only time



                    //bool HasEarlyOut = false;
                    //if(_OT_outTime<=0)
                    //{
                    //    HasEarlyOut = true;
                    //}
                    //int _deducted_value = 0;
                    bool IsPositiveOT = false;
                    _OT_outTime = 0;
                    if (_ShiftDft.EarlyOut)
                    {
                        TimeSpan _out_margin = Convert.ToDateTime(_Emp_Outtime) - Convert.ToDateTime(_Shift_Outtime);
                        var _out_margin_duration = ((_out_margin.Hours * 60) + _out_margin.Minutes);
                        if (_out_margin_duration < 0)//early out (neg value)
                        {
                            if (_ShiftDft.EarlyOutMargin >= Math.Abs(_out_margin_duration))//no neg out time only braekperiod time considering
                            {
                                _OT_outTime = 0;
                                if (_ShiftDft.IncludeBreakTimeInOT == false)
                                {
                                    _OT_outTime -= _ShiftDft.BreakPeriod;//as BreakPeriod is deducted twice
                                }
                            }
                            else//huge negetive ot found
                            {
                                //need to define out time
                                GetEmpOutTime(_ShiftDft, ref _Emp_Outtime, sWorkingDate);
                                //calculate OT
                                TimeSpan _inOT = Convert.ToDateTime(_Emp_Outtime) - Convert.ToDateTime(_Shift_Outtime);
                                _OT_outTime = ((_inOT.Hours * 60) + _inOT.Minutes);
                            }

                            //_OT_outTime_returnValue = RoundedOT(_OT_outTime, _ShiftDft.EarlyOutRoundMargin, _ShiftDft.EarlyOutRoundMarginType) * (-1);
                        }
                        else
                        {
                            //calculate OT
                            IsPositiveOT = true;
                            TimeSpan _inOT = Convert.ToDateTime(_Emp_Outtime) - Convert.ToDateTime(_Shift_Outtime);
                            _OT_outTime = ((_inOT.Hours * 60) + _inOT.Minutes);
                        }
                        //deduct latein
                    }


                    if (_ShiftDft.LateOut)
                    {
                        if (_OT_outTime > 0)//positiove ot will b calculated
                        {
                            if (_ShiftDft.IsGapInclude == false)
                            {
                                _OT_outTime = Math.Abs(_OT_outTime) - _ShiftDft.LateOutMargin;

                                if (_OT_outTime < 0)
                                {
                                    _OT_outTime = 0;
                                }
                            }
                            //_OT_outTime_returnValue = RoundedOT(Math.Abs(_OT_outTime), _ShiftDft.LateOutRoundMargin, _ShiftDft.LateOutRoundMarginType);
                        }
                    }

                    ///adjust paidhours 8/10 hours shift
                    if (_PaidHours > 0)//iTotalOTHr=iTotalOTHr+(WorkingHour-PaidHour)
                    {
                        _OT_outTime = _OT_outTime + (Convert.ToDecimal(_ShiftDft.WorkingHour - (_PaidHours * 60)));

                        if (_ShiftDft.IncludeBreakTimeInOT == false)
                        {
                            if (IsPositiveOT == false)//bcoz at the time of negetive ot outitme can be less than breakend time but during positive no need
                            {
                                _OT_outTime += _ShiftDft.BreakPeriod;//as BreakPeriod is deducted twice
                            }
                        }

                    }


                    if (_OT_outTime <= 0)
                    {
                        _OT_outTime_returnValue = RoundedOT(Math.Abs(_OT_outTime), _ShiftDft.EarlyOutRoundMargin, _ShiftDft.EarlyOutRoundMarginType);
                        _OT_outTime_returnValue = _OT_outTime_returnValue * (-1);
                    }
                    else
                    {
                        _OT_outTime_returnValue = RoundedOT(Math.Abs(_OT_outTime), _ShiftDft.LateOutRoundMargin, _ShiftDft.LateOutRoundMarginType);
                    }


                }//both check

                return _OT_outTime_returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        decimal RoundedOT(decimal iTotalOTHr, int MinOT, string sFractionCalculate)
        {
            try
            {
                int iMinOT = 1;
                if (MinOT > 0)
                {
                    iMinOT = MinOT;
                }

                if (string.IsNullOrEmpty(sMinOT.Trim()) == false)
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

                return iTotalOTHr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool SummaryDataProcess(string GroupSysID, string splantid, string sAttnDate, string sEmpSystemIDColl)
        {
            #region declare variables

            DataSet dsAttnDataForTheMonth = null;

            DataSet dsAttnDataMonthSummary = null;
            DataTable dtAttnDataMonthSummary = null;
            DataRow drAttnDataMonthSummary = null;
            DataView dvAttnDataMonthSummary = null;

            bool bValid = false;

            #endregion local variables

            try
            {
                #region DataSet

#if DEBUG
                GetAttdnDataForMonthlyProcNew(GroupSysID.Trim(), splantid, sAttnDate.Trim(), sEmpSystemIDColl.Trim(), out dsAttnDataForTheMonth);
#else
                GetAttdnDataForMonthlyProc(GroupSysID.Trim(), splantid, sAttnDate.Trim(), sEmpSystemIDColl.Trim(), out dsAttnDataForTheMonth);
#endif
                GetAttdnDataMonthlySummary(GroupSysID.Trim(), Convert.ToDateTime(sAttnDate.Trim()).Month, Convert.ToDateTime(sAttnDate.Trim()).Year, sEmpSystemIDColl.Trim(), out dsAttnDataMonthSummary);
                dtAttnDataMonthSummary = dsAttnDataMonthSummary.Tables[0];

                #endregion DataSet

                for (int i = 0; i < dsAttnDataForTheMonth.Tables[0].Rows.Count; i++)
                {
                    dvAttnDataMonthSummary = new DataView();
                    dvAttnDataMonthSummary.Table = dtAttnDataMonthSummary;
                    dvAttnDataMonthSummary.RowFilter = "EmpSystemID = '" + dsAttnDataForTheMonth.Tables[0].Rows[i]["EmpSystemID"].ToString() + "'";
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
                        drAttnDataMonthSummary["TotalLWP"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalLWP"].ToString();

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
                        drAttnDataMonthSummary["TotalLWP"] = dsAttnDataForTheMonth.Tables[0].Rows[i]["TotalLWP"].ToString();

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

                //clsStaticInfo obj = new clsStaticInfo();
                SaveDataSets(dsAttnDataMonthSummary);

                bValid = true;
                return bValid;
            }
            catch (Exception ex)
            {
                throw ex;
                //Cursor = Cursors.Default;
                //System.Windows.Forms.MessageBox.Show(this, ex.ToString(), "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return bValid;
            }
            finally
            {
                #region clean variable

                dsAttnDataForTheMonth = null;

                dsAttnDataMonthSummary = null;
                dtAttnDataMonthSummary = null;
                drAttnDataMonthSummary = null;
                dvAttnDataMonthSummary = null;

                #endregion
            }
        }//End Function 

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
        private void xGetHRSetting(string Plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM PlantWiseHRMSSetting where plantid='" + Plantid + "'";

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
        private void GetEmpDateWiseShiftAssign(string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT *
                            FROM dbo.EmpDateWiseShiftAssign 
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @")";

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
        private void GetEmployeeWeekOffByDay(string sAttnDate, string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                //strSql = @"SELECT *
                //            FROM dbo.EmployeeWeekOffByDay 
                //             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @")";
                ///by monir 190706
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
                //                ISNULL(SRM.IsWeekOffInShiftLenght, 0) IsWeekOffInShiftLenght, SRM.WeekOffInShiftLenght 
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
        private void GetRawAll(string sGroupID, string sAttnDate, string sType, string empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                string FromDdate = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
                string ToDdate = Convert.ToDateTime(sAttnDate).AddDays(+1).ToString("dd-MMM-yyyy");
                strSql = @"SELECT * FROM AttdnRawData
                           WHERE isnull(PType,'')<>'' and PDate between '" + FromDdate + @"' and '" + ToDdate + @"' AND GroupID = '" + sGroupID + @"'  AND LogDownLoadNum IN (
                                                     " + empids + @"
                                                    )";

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
        private void xGetEmployeeInfo(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT E.*, ES.*, ISNULL(DATEDIFF(D, Atd.LastWorkDate, '" + sAttnDate + @"'), 0) DateDiffer
                            , ISNULL(Atd.LastWorkDate, GETDATE()) LastWorkDate, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle, EmOT.OTStartDate, EmOT.OTEndDate,
                                  ISNULL(AttDt.ToReprocess, 'YES') ToReprocess
	                        FROM 
                            (
                             SELECT * FROM EmployeeInformation WHERE 
                                    SystemID IN (" + sEmpSysIdColl + @")
                            ) AS E 
		                        INNER JOIN (
											SELECT * FROM
														(
														 SELECT ES.EmpSystemID, ES.ShiftSystemID, ES.DayType, S.ShiftType, 
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
																OfficeEndTime = CASE WHEN ISNULL(C.OutTimeEndMargin, '') != '' THEN DATEADD(MI, C.OutTimeEndMargin, S.OutTime)
																					  ELSE DATEADD(MI, S.OutTimeEndMargin, S.OutTime) END,
																OTStartTime = CASE WHEN S.IsGapInclude = 1 AND ISNULL(C.OutTime, '') != '' THEN C.OutTime
																				   WHEN S.IsGapInclude = 1 AND ISNULL(C.OutTime, '') = '' THEN S.OutTime
																				   WHEN S.IsGapInclude = 0 AND ISNULL(C.OutTime, '') != '' THEN DATEADD(MI, C.OTStartTime, C.OutTime)
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
        void GetEarlyOut(dicShiftDft dicshiftdft, string workdate, string Punch_outtime, out bool IsEarlyOut, out decimal EOValue)
        {
            IsEarlyOut = false;
            EOValue = 0;
            int _EarlyOutMargin = 0;
            try
            {
                var Calculated_OUT_Time = Convert.ToDateTime(Punch_outtime).AddMinutes(_EarlyOutMargin);
                string _s_end_time = string.Empty;
                GetOutTime(dicshiftdft, workdate, out _s_end_time);
                if (Calculated_OUT_Time < Convert.ToDateTime(_s_end_time))
                {
                    var x = Convert.ToDateTime(_s_end_time) - Calculated_OUT_Time;
                    EOValue = (((x.Days * 60) * 24) + (x.Hours * 60) + x.Minutes);
                    //_Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
                    IsEarlyOut = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetLunchOut(dicShiftDft dicshiftdft, DataTable dtRaw, string workdate, out object intime_obj, out object outtime_obj)
        {
            string _lunctimeStart = string.Empty;
            string _lunctimeEnd = string.Empty;
            outtime_obj = null;
            intime_obj = null;
            try
            {
                //lunch start time + margin 15

                string _lst = workdate + " " + Convert.ToDateTime(dicshiftdft.BreakStratTime).AddMinutes(-15).ToString("HH:mm:ss");
                string _led = workdate + " " + Convert.ToDateTime(dicshiftdft.BreakEndTime).AddMinutes(15).ToString("HH:mm:ss");
                intime_obj = dtRaw.Compute("min(ptime)", "where PTime >= '" + _lst + "' and PTime<='" + _led + "'");
                outtime_obj = dtRaw.Compute("max(ptime)", "where PTime  PTime >=  '" + _lst + "' and PTime<='" + _led + "'");
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
        private bool OutDataProcess(string _plantId, string sAttnDate, string GroupSysID, string sEmpSystemIDColl, string sMinOT, string sFractionCalculate, bool radDwLdEnrollID)
        {
            //sShiftType
            #region declare variables
            DataSet dsCompensatoryOff = null;
            DataSet dsCompensatoryOffEmpList = null;
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

            string sLogDownLoadNum = "";
            string sEmpSysID = "";
            string sPlantID = "";
            string sOTStartTime = "";
            decimal iTotalOTHr = 0;
            decimal _OT_inTime = 0;
            decimal _OT_outTime = 0;
            string sOTDayType = "";
            decimal dfirstSlab = 0;
            bool bIsOTExtentNextSlab = false;
            bool bIsTotalWorkTimeAsOT = false;
            bool bOTEntitle = false;

            string sOfficeInTime = "";
            string sInTime = "";
            string sOutTime = "";
            string sOutTimeRowID = string.Empty;
            string sOutTimeRowData = string.Empty;
            // int iDeviceID = 0;
            string sOutTimeTmp = "";
            //string sOutTimeRowIDTmp = string.Empty;
            //int iDeviceIDTmp = 0;
            string sShiftSystemID = "";
            string sShiftType = "";
            string sDayType = "";

            string sBreakStratTime = "";
            string sBreakEndTime = "";

            string sDate = "";
            //string sPrvDate = "";
            //string sWorkingDate = "";
            string sOutDate = "";
            bool bValid = false;
            bool IsOTOverHalfDay = false;
            bool IsWeekendAsFixedWeekend = false;
            bool IsOTBasedOnPerMinute = false;
            //bool OTDeductionForAbsenteeism = false;
            dicShiftDft _ShiftDft = null;
            double _PaidHours = 0;
            ShortLeaveSetting _shortleave_setting = null;
            DataSet dsDayType = null;
            #endregion local variables

            try
            {
                #region Dataset
                GetDayType(out dsDayType);
                _shortleave_setting = new ShortLeaveSetting();
                DataSet dsShortLeaveSetting = null;
                GetShortLeaveSettingPlantWise(_plantId, out dsShortLeaveSetting);
                if (dsShortLeaveSetting.Tables[0].Rows.Count > 0)
                {
                    _shortleave_setting.IsHalfDayPresentAllowed = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsHalfDayPresentAllowed"].ToString());
                    _shortleave_setting.IsShortLeaveAllowed = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsShortLeaveAllowed"].ToString());
                    _shortleave_setting.IsTowShortLeaveAllowedInaDay = GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsTowShortLeaveAllowedInaDay"].ToString());
                    _shortleave_setting.MaxShortLeaveInaMonth = Convert.ToInt32(GetNumData(dsShortLeaveSetting.Tables[0].Rows[0]["MaxShortLeaveInaMonth"].ToString()));
                    //IsOToverHalfDay = bplib.clsWebLib.GetBoolData(dsShortLeaveSetting.Tables[0].Rows[0]["IsOTOverHalfDay"].ToString());
                }

                DataSet dsHRSetting = null;
                GetHRSettingPlantWise(_plantId, out dsHRSetting);//GetHRSettingPlantWise
                if (dsHRSetting.Tables[0].Rows.Count > 0)
                {
                    IsOTOverHalfDay = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsOTOverHalfDay"].ToString());
                    IsWeekendAsFixedWeekend = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsWeekendCount"].ToString());
                    IsOTBasedOnPerMinute = GetBoolData(dsHRSetting.Tables[0].Rows[0]["IsOTBasedOnPerMinute"].ToString());
                    //OTDeductionForAbsenteeism = GetBoolData(dsHRSetting.Tables[0].Rows[0]["OTDeductionForAbsenteeism"].ToString());
                }

                DataSet dsOTPerMinPolicy = null;
                if (IsOTBasedOnPerMinute)
                {
                    GetOTPerMinPolicy(_plantId, out dsOTPerMinPolicy);//GetHRSettingPlantWise
                }
                #endregion

                DateTime sFrmDt = Convert.ToDateTime(sAttnDate.Trim());
                DateTime sToDt = Convert.ToDateTime(sAttnDate.Trim()).AddDays(-1);
                int DateCount = 0;
                while (sToDt <= sFrmDt)
                {
                    #region DataSet

                    sDate = sToDt.ToString("dd-MMM-yyyy");
                    //sDate = Convert.ToDateTime(sFrmDt).ToString("dd-MMM-yyyy");
                    //sPrvDate = (Convert.ToDateTime(sFrmDt).AddDays(-1)).ToString("dd-MMM-yyyy");                    

                    //GetAttdnRawDataForAttdnProc(GroupSysID.Trim(), sDate.Trim(), "OUT", out dsRawData);
                    GetRawAll(GroupSysID.Trim(), sDate.Trim(), "OUT", sEmpSystemIDColl.Trim(), out dsRawData);//GetRawAll
                    dtRawData = dsRawData.Tables[0];

                    GetAttdnProcData(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), sDate.Trim(), out dsAttnProcData);
                    dtAttnProcData = dsAttnProcData.Tables[0];

                    GetFinalOT(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), out dsFinalOT);
                    dtFinalOT = dsFinalOT.Tables[0];

                    GetOTSlabDefineEmployee(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), sDate.Trim(), out dsOTSlabEmp);
                    dtOTSlabEmp = dsOTSlabEmp.Tables[0];

                    GetOTSlabDefineGeneral(GroupSysID.Trim(), sDate.Trim(), out dsOTSlabGen);
                    dtOTSlabGen = dsOTSlabGen.Tables[0];

                    GetEmployeeInfo_Out(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sDate.Trim(), out dsEmpInfo);

                    DataSet dsDayTypePrev = null;
                    GetDayTypePrev(_plantId, sEmpSystemIDColl.Trim(), sDate.Trim(), out dsDayTypePrev);
                    DataSet dsHoliday = null;
                    GetHoliday(GroupSysID.Trim(), _plantId, sDate.Trim(), out dsHoliday);


                    ////GetAttdnManualData(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sAttnDate.Trim(), out dsMnAttData);
                    GetAttdnManualData(GroupSysID.Trim(), _plantId, sEmpSystemIDColl.Trim(), sDate, out dsMnAttData);
                    dtMnAttData = dsMnAttData.Tables[0];
                    dvMnAttData = new DataView();

                    DataSet dsPaidHours = null;
                    GetPaidHours(GroupSysID.Trim(), sEmpSystemIDColl.Trim(), out dsPaidHours);


                    //============ kabir ==========
                    GetCompensatoryOffPlantData(_plantId, sDate.Trim(), out dsCompensatoryOff);
                    GetCompensatoryOffEmpListData(_plantId, sDate.Trim(), sEmpSystemIDColl.Trim(), out dsCompensatoryOffEmpList);
                    var CompensatoryDateTreatmentType = string.Empty;
                    if (dsCompensatoryOff.Tables[0].Rows.Count > 0)
                    {
                        CompensatoryDateTreatmentType = dsCompensatoryOff.Tables[0].Rows[0]["CompensatoryDateTreatmentType"].ToString();
                        //bool IsOriginalDateOTApplicable= Convert.ToBoolean(dsCompensatoryOff.Tables[0].Rows[0]["IsOriginalDateOTApplicable"].ToString());
                    }
                    string flag = "";
                    bool IsOriginalDateOTApplicable = false;
                    #endregion DataSet

                    if (dsEmpInfo.Tables[0].Rows.Count > 0)
                    {
                        for (int EmpCount = 0; EmpCount < dsEmpInfo.Tables[0].Rows.Count; EmpCount++)
                        {
                            #region Variables
                            IsOriginalDateOTApplicable = false;
                            flag = "";
                            _ShiftDft = new global::dicShiftDft();
                            GetShiftDefinition(dsEmpInfo.Tables[0].Rows[EmpCount], _ShiftDft);
                            sEmpSysID = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemID"].ToString();

                            DataView dv = new DataView(dsPaidHours.Tables[0]);
                            dv.RowFilter = "EmployeeId='" + sEmpSysID + "'";
                            if (dv.Count > 0)
                            {
                                _PaidHours = Convert.ToDouble(GetNumData(dv[0]["PaidHours"].ToString()));
                            }
                            else
                            {
                                _PaidHours = 0;
                            }

                            sPlantID = dsEmpInfo.Tables[0].Rows[EmpCount]["PlantID"].ToString();
                            sOTStartTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OTStartTime"].ToString().Trim()).ToString("HH:mm:ss");
                            sOfficeInTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["OfficeTime"].ToString().Trim()).ToString("HH:mm:ss");
                            sInTime = "00:00:00";
                            bOTEntitle = Convert.ToBoolean(GetBoolData(dsEmpInfo.Tables[0].Rows[EmpCount]["IsOTEntitle"].ToString()));
                            iTotalOTHr = 0;
                            _OT_inTime = 0;
                            _OT_outTime = 0;
                            sOTDayType = "";
                            dfirstSlab = 0;
                            sOutDate = "";
                            bIsOTExtentNextSlab = false;
                            bIsTotalWorkTimeAsOT = false;
                            sShiftSystemID = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftSystemID"].ToString();
                            sShiftType = dsEmpInfo.Tables[0].Rows[EmpCount]["ShiftType"].ToString();
                            //==========

                            //======================================================================
                            sDayType = dsEmpInfo.Tables[0].Rows[EmpCount]["DayType"].ToString();

                            sBreakStratTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakStratTime"].ToString().Trim()).ToString("HH:mm:ss");
                            sBreakEndTime = Convert.ToDateTime(dsEmpInfo.Tables[0].Rows[EmpCount]["BreakEndTime"].ToString().Trim()).ToString("HH:mm:ss");
                            #endregion

                            #region plant wise Compensatory
                            if (dsCompensatoryOff.Tables[0].Rows.Count > 0)
                            {

                                CompensatoryDateTreatmentType = dsCompensatoryOff.Tables[0].Rows[0]["CompensatoryDateTreatmentType"].ToString();
                                flag = dsCompensatoryOff.Tables[0].Rows[0]["flag"].ToString();
                                IsOriginalDateOTApplicable = Convert.ToBoolean(dsCompensatoryOff.Tables[0].Rows[0]["IsOriginalDateOTApplicable"].ToString());
                                if (flag == "compensatory")
                                {
                                    IsOriginalDateOTApplicable = false;
                                }
                            }
                            #endregion

                            #region employe wise Compensatory
                            DataView dvEmp = new DataView(dsCompensatoryOffEmpList.Tables[0]);
                            dvEmp.RowFilter = "EmpSystemId ='" + sEmpSysID + "'";

                            if (dvEmp.Count > 0)
                            {
                                CompensatoryDateTreatmentType = dvEmp[0]["CompensatoryDateTreatmentType"].ToString();
                                flag = dvEmp[0]["flag"].ToString();
                                IsOriginalDateOTApplicable = Convert.ToBoolean(dvEmp[0]["IsOriginalDateOTApplicable"].ToString());
                                if (flag == "compensatory")
                                {
                                    IsOriginalDateOTApplicable = false;
                                }
                            }
                            #endregion

                            if (Convert.ToInt32(dsEmpInfo.Tables[0].Rows[EmpCount]["DateDiffer"].ToString()) <= 1)
                            {
                                sLogDownLoadNum = dsEmpInfo.Tables[0].Rows[EmpCount]["SystemId"].ToString();
                                DataView dvHoliday = new DataView(dsHoliday.Tables[0]);
                                dvHoliday.RowFilter = "OffDayDate='" + sDate + "' ";
                                if (dvHoliday.Count > 0)
                                {
                                    sDayType = dvHoliday[0]["OffDayType"].ToString();
                                }

                                if (flag.ToUpper() == "ORIGINAL")
                                {
                                    sDayType = "NW";
                                }

                                if (flag.ToUpper() == "COMPENSATORY")
                                {
                                    sDayType = CompensatoryDateTreatmentType;
                                }

                                #region Find raw Data Table

                                sOutTime = "00:00:00";
                                sOutTimeRowID = string.Empty;
                                sOutTimeRowData = string.Empty;
                                //iDeviceID = 0;
                                sOutTimeTmp = "00:00:00";
                                //sOutTimeRowIDTmp = string.Empty;
                                //iDeviceIDTmp = 0;
                                GetMaxRaw(ref dtRawData, sLogDownLoadNum, _ShiftDft, sDate, out sOutTime, out sOutTimeRowID);
                                //999
                                //var xx = false;
                                //if (xx)
                                //{
                                //    object inttimeobj = null;
                                //    object outtimeobj = null;
                                //    GetLunchOut(_ShiftDft, dtRawData, sDate, out inttimeobj, out outtimeobj); 
                                //}

                                string _out_Date = Convert.ToDateTime(sOutTime).ToString("dd-MMM-yyyy");
                                sOutTimeTmp = Convert.ToDateTime(sOutTime).ToString("HH:mm:ss");
                                sOutTimeRowData = Convert.ToDateTime(sOutTime).ToString("dd-MMM-yyyy HH:mm:ss");
                                sOutTime = sOutTimeTmp;

                                #endregion Find InTime from raw Data Table

                                #region by monir for manual
                                bool IsManualDeleted = false;
                                //bool bManualOutTime = false;
                                dvMnAttData.Table = dtMnAttData;
                                dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                if (dvMnAttData.Count > 0)
                                {

                                    if (dvMnAttData[0].Row["OutTime"].ToString().Trim() != "")
                                    {
                                        IsManualDeleted = false;
                                        //sWorkingDate = sDate;
                                    }
                                    else
                                    {
                                        IsManualDeleted = true;
                                    }
                                }
                                else
                                {
                                    IsManualDeleted = true;
                                }
                                #endregion

                                bool bAttnIsLock = false;
                                bool bManualOutTime = false;

                                dvAttnProcData = new DataView();
                                dvAttnProcData.Table = dtAttnProcData;
                                dvAttnProcData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                if (dvAttnProcData.Count > 0)
                                {
                                    string _inDate = "00:00:00";
                                    if (dvAttnProcData[0]["InTime"].ToString().Trim() != "")
                                    {
                                        sInTime = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("HH:mm:ss");
                                        _inDate = Convert.ToDateTime(dvAttnProcData[0]["InTime"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                    }
                                    bAttnIsLock = Convert.ToBoolean(dvAttnProcData[0].Row["IsLock"].ToString());
                                    bManualOutTime = Convert.ToBoolean(dvAttnProcData[0].Row["IsManualOutTime"].ToString());

                                    if (bAttnIsLock == false)
                                    {
                                        //string sOut_Date = "00:00:00";
                                        //bool IsProcessed = false;
                                        //if (dvAttnProcData[0]["OutTime"].ToString() != "")
                                        //{
                                        //    sOutTimeTmp = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                        //    sOut_Date = Convert.ToDateTime(dvAttnProcData[0]["OutTime"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                        //    sOutTimeRowIDTmp = dvAttnProcData[0]["OutTimeRowID"].ToString().Trim();
                                        //    IsProcessed = true;
                                        //}

                                        if (bManualOutTime && IsManualDeleted)
                                        {
                                            sOutTimeTmp = "00:00:00";
                                        }

                                        #region Compare DateTime for Max  
                                        string Punch_sDateTime = "00:00:00";
                                        if (sOutTime == "00:00:00")
                                        {
                                            Punch_sDateTime = sDate + " " + sOutTime;//if no raw data set process date else punch date
                                        }
                                        else
                                        {
                                            Punch_sDateTime = _out_Date + " " + sOutTime;
                                        }

                                        if (sInTime != "00:00:00")//intime avai but out time <intime
                                        {
                                            if (Convert.ToDateTime(Punch_sDateTime) < Convert.ToDateTime(_inDate + " " + sInTime))
                                            {
                                                sOutTime = "00:00:00";
                                                sOutTimeTmp = "00:00:00";
                                                sOutTimeRowID = string.Empty;
                                                //sOutTimeRowData= "00:00:00";
                                            }
                                        }
                                        //string Processed_sDateTime = "00:00:00";
                                        //if (IsProcessed==false)
                                        //{
                                        //    Processed_sDateTime = sDate + " " + sOutTimeTmp;//if no raw data set process date else punch date
                                        //    IsProcessed = false;
                                        //}
                                        //else
                                        //{
                                        //    Processed_sDateTime = sOut_Date + " " + sOutTimeTmp;
                                        //} 
                                        #endregion

                                        //if (IsProcessed && Convert.ToDateTime(Punch_sDateTime) < Convert.ToDateTime(Processed_sDateTime))
                                        //{
                                        //    sOutTime = sOutTimeTmp;
                                        //    sOutTimeRowID = sOutTimeRowIDTmp;
                                        //    _out_Date = sOut_Date;
                                        //}

                                        #region Manual Attendance
                                        bool HasManualOutTime = false;
                                        bool HasManualDayStatus = false;
                                        //string ManualDate = string.Empty;
                                        dvMnAttData.Table = dtMnAttData;
                                        dvMnAttData.RowFilter = "EmpSystemID = '" + sEmpSysID + "' AND WorkDate = '" + sDate.Trim() + "'";
                                        if (dvMnAttData.Count > 0)
                                        {
                                            if (dvMnAttData[0].Row["OutTime"].ToString().Trim() != "" && Convert.ToDateTime(dvMnAttData[0].Row["OutTime"].ToString().Trim()).ToString("HH:mm:ss") != "00:00:00")
                                            {
                                                if (dvMnAttData[0].Row["DayStatus"].ToString().Trim() == "")
                                                {
                                                    sOutTimeRowID = "";
                                                    HasManualOutTime = true;
                                                    sOutTime = Convert.ToDateTime(dvMnAttData[0].Row["OutTime"].ToString().Trim()).ToString("HH:mm:ss");
                                                    _out_Date = Convert.ToDateTime(dvMnAttData[0].Row["OutTime"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                                    bManualOutTime = true;
                                                    if (sInTime != "00:00:00")
                                                    {
                                                        if (Convert.ToDateTime(dvMnAttData[0].Row["OutTime"].ToString().Trim()) < Convert.ToDateTime(_inDate + " " + sInTime))
                                                        {
                                                            HasManualOutTime = false;
                                                            bManualOutTime = false;
                                                            sOutTime = "00:00:00";
                                                            sOutTimeTmp = "00:00:00";
                                                        }
                                                    }//intime
                                                }
                                                else
                                                {
                                                    HasManualDayStatus = true;
                                                    HasManualOutTime = false;
                                                    bManualOutTime = false;
                                                    sOutTime = "00:00:00";
                                                    sOutTimeTmp = "00:00:00";
                                                }
                                            }

                                            if (dvMnAttData[0].Row["DayStatus"].ToString().Trim() != "")
                                            {
                                                HasManualDayStatus = true;
                                                HasManualOutTime = false;
                                                bManualOutTime = false;
                                                sOutTime = "00:00:00";
                                                sOutTimeTmp = "00:00:00";
                                            }
                                        }
                                        else
                                        {
                                            if (bManualOutTime)
                                            {
                                                bManualOutTime = false;
                                                HasManualOutTime = false;
                                                //sOutTime = "00:00:00";
                                            }
                                        }
                                        #endregion Manual Attendance

                                        #region OT
                                        ParaOT para = new ParaOT();
                                        para.IsOriginalDateOTApplicable = IsOriginalDateOTApplicable;
                                        para.dsOTPerMinPolicy = dsOTPerMinPolicy;
                                        para.IsOTBasedOnPerMinute = IsOTBasedOnPerMinute;
                                        para.sDayType = sDayType;
                                        para.bOTEntitle = bOTEntitle;
                                        para.HasManualOutTime = HasManualOutTime;
                                        para.ManualDate = _out_Date;
                                        para._PaidHours = _PaidHours;
                                        para._ShiftDft = _ShiftDft;
                                        para.IsOTOverHalfDay = IsOTOverHalfDay;
                                        para.dtOTSlabEmp = dtOTSlabEmp;
                                        para.dsOTSlabGen = dsOTSlabGen;
                                        para.sEmpSysID = sEmpSysID;
                                        para.sDate = sDate;
                                        para.sInTime = sInTime;
                                        para.sOTStartTime = sOTStartTime;
                                        para.sMinOT = sMinOT;
                                        para.sOutTime = sOutTime;

                                        if (sEmpSysID == "1900023")
                                        {

                                        }
                                        CalculateOT(para, out iTotalOTHr, out _OT_inTime, out _OT_outTime);
                                        ///(if an emp was ot enttile but now he is not 
                                        ///so approved ot will be deleted based on current status for the current month)
                                        if (DateCount > 0 && para.bOTEntitle == false)//DateCount=0 == prev Date 
                                        {
                                            DeleteFinalOT(sEmpSysID, _out_Date);
                                        }
                                        #endregion

                                        //sEmpSysID
                                        bool IsShortLeave = false;
                                        bool IsStatusChanged = false;
                                        string _DayStatus = "";
                                        bool IsReversed = false;
                                        int CountShortLeave = 0;
                                        bool ShouldNullifyOTValue = false;
                                        //#if DEBUG
                                        ParaShortLeaveHalfDayAbsent objSLHD = new global::ParaShortLeaveHalfDayAbsent();
                                        #region set value
                                        objSLHD.sInTime = sInTime;
                                        objSLHD.sOutTime = sOutTime;
                                        objSLHD.sWorkingDate = sDate;
                                        objSLHD.sDate = sDate;
                                        objSLHD._ShiftDft = _ShiftDft;
                                        objSLHD.DayStatus = _DayStatus;
                                        objSLHD.IsShortLeave = IsShortLeave;
                                        objSLHD.IsStatusChanged = IsStatusChanged;
                                        objSLHD.IsReversed = IsReversed;
                                        objSLHD.CountShortLeave = CountShortLeave;
                                        objSLHD.IsShortLeaveAllowed = _shortleave_setting.IsShortLeaveAllowed;
                                        objSLHD.IsHalfDayPresentAllowed = _shortleave_setting.IsHalfDayPresentAllowed;
                                        objSLHD.IsTowShortLeaveAllowedInaDay = _shortleave_setting.IsTowShortLeaveAllowedInaDay;
                                        objSLHD.MaxShortLeaveInaMonth = _shortleave_setting.MaxShortLeaveInaMonth;
                                        objSLHD.IsOTOverHalfDay = IsOTOverHalfDay;
                                        objSLHD.PaidHours = _PaidHours;
                                        objSLHD.IsOTentitled = bOTEntitle;
                                        objSLHD.HasManualOutTime = HasManualOutTime;
                                        objSLHD.ManualDate = _out_Date;//_out_Date
                                        #endregion

                                        if (HasManualDayStatus == false)
                                        {
                                            ShortLeaveHalfDayAbsent(objSLHD);
                                            ShouldNullifyOTValue = objSLHD.ShouldNullifyOTValue;
                                            //ShortLeaveHalfDayAbsent(sInTime, sOutTime, sWorkingDate, sDate, _ShiftDft, IsOTOverHalfDay,
                                            //out _DayStatus, out IsShortLeave, out IsStatusChanged, out IsReversed,out CountShortLeave);
                                            _DayStatus = objSLHD.DayStatus;
                                            IsShortLeave = objSLHD.IsShortLeave;
                                            IsStatusChanged = objSLHD.IsStatusChanged;
                                            IsReversed = objSLHD.IsReversed;
                                            CountShortLeave = objSLHD.CountShortLeave;
                                        }
                                        //#endif

                                        #region Para
                                        ParaAttendance _paraA = new global::ParaAttendance();
                                        _paraA.OPN_FLAG = "EDIT";
                                        _paraA.GroupId = GroupSysID;
                                        _paraA.sType = "OUT";
                                        _paraA.sEmpSystemID = sEmpSysID;
                                        _paraA.sPlantID = sPlantID;
                                        _paraA.sWorkingDate = sDate.Trim();
                                        _paraA.shiftSystemID = sShiftSystemID;
                                        _paraA.sDate = sDate;
                                        _paraA.sTime = sOutTime;
                                        _paraA.bManualTime = bManualOutTime;
                                        _paraA.sRowID = sOutTimeRowID;
                                        _paraA.sOutRawData = sOutTimeRowData;
                                        _paraA.sDayStatus = _DayStatus;
                                        _paraA.bManualDayStatus = HasManualDayStatus;
                                        ///at the time of A as per settng
                                        _paraA.iOverTime = iTotalOTHr;
                                        _paraA.sLvTrans = "";
                                        _paraA.iOverTimeIntime = _OT_inTime;
                                        _paraA.iOverTimeOuttime = _OT_outTime;
                                        _paraA.IsStatusChanged = IsStatusChanged;
                                        _paraA.IsShortLeave = IsShortLeave;
                                        _paraA.IsReversed = IsReversed;
                                        _paraA.CountedShortLeave = CountShortLeave;
                                        _paraA.HasManualOutTime = HasManualOutTime;
                                        _paraA.ManualDate = _out_Date;
                                        _paraA.IsOTEntitled = bOTEntitle;
                                        _paraA.OutDate = _out_Date;



                                        #endregion

                                        drAttnProcData = dvAttnProcData[0].Row;
                                        drAttnProcData.BeginEdit();

                                        if (IsWeekendAsFixedWeekend && (drAttnProcData["DayStatus"].ToString() == "W" || drAttnProcData["DayStatus"].ToString() == "H"))
                                        {
                                            #region WH
                                            _paraA.OPN_FLAG = "EDIT";
                                            _paraA.GroupId = GroupSysID;
                                            _paraA.sType = "OUT";
                                            _paraA.sEmpSystemID = sEmpSysID;
                                            _paraA.sPlantID = sPlantID;
                                            _paraA.sWorkingDate = sDate.Trim();
                                            _paraA.shiftSystemID = sShiftSystemID;
                                            _paraA.sDate = sDate;
                                            _paraA.sTime = "00:00:00";
                                            //_paraA.sTime = sOutTime;
                                            _paraA.bManualTime = false;
                                            _paraA.sRowID = sOutTimeRowID;
                                            _paraA.sOutRawData = sOutTimeRowData;
                                            _paraA.sDayStatus = drAttnProcData["DayStatus"].ToString();
                                            _paraA.bManualDayStatus = HasManualDayStatus;
                                            ///at the time of A as per settng
                                            _paraA.iOverTime = 0;
                                            _paraA.sLvTrans = "";
                                            _paraA.iOverTimeIntime = _OT_inTime;
                                            _paraA.iOverTimeOuttime = _OT_outTime;
                                            _paraA.IsStatusChanged = false;
                                            _paraA.IsShortLeave = false;
                                            _paraA.IsReversed = false;
                                            _paraA.CountedShortLeave = 0;
                                            _paraA.HasManualOutTime = HasManualOutTime;
                                            _paraA.ManualDate = _out_Date;
                                            _paraA.IsOTEntitled = bOTEntitle;
                                            _paraA.OutDate = _out_Date;
                                            #endregion
                                        }

                                        _paraA.DayType = sDayType;

                                        if (_paraA.bManualDayStatus == false)
                                        {
                                            //if (sOffDay == "W" || sOffDay == "H")
                                            if (sDayType == "W" || sDayType == "H")
                                            {
                                                DataView dvDT = new DataView(dsDayType.Tables[0]);
                                                dvDT.RowFilter = "(Category='Late' or Category='Half Day') and daytype='" + _paraA.sDayStatus + "'";
                                                if (dvDT.Count > 0)
                                                {
                                                    _paraA.sDayStatus = "WL";
                                                }
                                            }
                                        }

                                        //var xxx = false;
                                        //if (xxx)
                                        //{
                                        //    bool IsEarlyOut;
                                        //    decimal EOValue;
                                        //    GetEarlyOut(_ShiftDft, _paraA.sDate, _paraA.sTime, out IsEarlyOut, out EOValue);
                                        //}
                                        //bool IsEarlyOut;
                                        //GetEarlyOut(_ShiftDft, _paraA.sDate, _paraA.sTime, out IsEarlyOut);
                                        UpdateAttdnData(_paraA, ref drAttnProcData);
                                        //UpdateAttdnData("EDIT", GroupSysID, "OUT", sEmpSysID, sPlantID, sWorkingDate.Trim(), sShiftSystemID, sDate, sOutTime, bManualOutTime, sOutTimeRowID, "", false, iTotalOTHr, "", ref drAttnProcData);
                                        drAttnProcData.EndEdit();
                                    }//bAttnIsLock
                                }//dvAttnProcData.Count
                            }//DateDiffer
                        }//dsEmpInfo loop
                        //clsStaticInfo obj = new clsStaticInfo();
                        SaveDataSets(dsRawData, dsAttnProcData);
                    }
                    //sToDt = sFrmDt.AddDays(1);
                    sToDt = sToDt.AddDays(1);
                    DateCount++;
                }
                bValid = true;
                return bValid;
            }
            catch (Exception ex)
            {
                throw ex;
                //Cursor = Cursors.Default;
                //System.Windows.Forms.MessageBox.Show(this, ex.ToString(), "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return bValid;
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
                //sOutTimeRowIDTmp = string.Empty;

                #endregion clean variables
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
                                                                ,LateOutRoundMargin
                                                                ,isnull(LateOutRoundMarginType, 'ROUND') LateOutRoundMarginType
                                                                 ,HalfDayAbsentMaxLimit,ShortLeaveMaxLimit,S.IsGapInclude,hrset.IsOTOverHalfDay

																,OfficeStartTime = CASE WHEN ISNULL(C.InTimeStartMargin, '') != '' THEN DATEADD(MI, -C.InTimeStartMargin, C.InTime)
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
                                                               
                                                                WorkingHour=CASE WHEN ISNULL(C.WorkingHour, '') != '' THEN isnull(c.BreakPeriod,0)+isnull(C.WorkingHour,0) 
                                                                WHEN S.IsGapInclude = 1 AND ISNULL(C.WorkingHour, '') = '' THEN isnull(s.BreakPeriod,0)+ isnull(s.WorkingHour,0)
                                                                ELSE S.WorkingHour END,

																OfficeEndTime = CASE WHEN ISNULL(C.OutTimeEndMargin, '') != '' THEN DATEADD(MI, C.OutTimeEndMargin, S.OutTime)
																					  ELSE DATEADD(MI, S.OutTimeEndMargin, S.OutTime) END,
                                                                OutTime = CASE WHEN ISNULL(C.OutTime, '') != '' THEN C.OutTime
																					  ELSE S.OutTime END,
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
        private void xGetEmployeeInfo_Out(string sGroupID, string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string sAttnDatePrev = string.Empty;//vv
            try
            {//kk

                sAttnDatePrev = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSql = @"SELECT E.*, ES.*, ISNULL(DATEDIFF(D, Atd.LastWorkDate, '" + sAttnDate + @"'), 0) DateDiffer, ISNULL(Atd.LastWorkDate, GETDATE()) LastWorkDate
                            , ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle, EmOT.OTStartDate, EmOT.OTEndDate,
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
                                                                ,LateOutRoundMargin
                                                                ,isnull(LateOutRoundMarginType, 'ROUND') LateOutRoundMarginType
                                                                 ,HalfDayAbsentMaxLimit,ShortLeaveMaxLimit,S.IsGapInclude,hrset.IsOTOverHalfDay

																,OfficeStartTime = CASE WHEN ISNULL(C.InTimeStartMargin, '') != '' THEN DATEADD(MI, -C.InTimeStartMargin, C.InTime)
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
                                                                WorkingHour=CASE WHEN ISNULL(C.WorkingHour, '') != '' THEN C.WorkingHour
																					  ELSE S.WorkingHour END,
																OfficeEndTime = CASE WHEN ISNULL(C.OutTimeEndMargin, '') != '' THEN DATEADD(MI, C.OutTimeEndMargin, S.OutTime)
																					  ELSE DATEADD(MI, S.OutTimeEndMargin, S.OutTime) END,
                                                                OutTime = CASE WHEN ISNULL(C.OutTime, '') != '' THEN C.OutTime
																					  ELSE S.OutTime END,
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
											--WHERE --CONVERT(DATETIME, CONVERT(VARCHAR(5), InTime, 108)) < CONVERT(DATETIME, CONVERT(VARCHAR(5), GETDATE(), 108))
                                                 -- CONVERT(DATETIME, CONVERT(VARCHAR(11), '" + sAttnDate + @"', 101) + ' ' + CONVERT(VARCHAR(5), InTime, 108)) < CONVERT(DATETIME, CONVERT(VARCHAR(11), GETDATE(), 101) + ' ' + CONVERT(VARCHAR(5), GETDATE(), 108))
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
                                LEFT JOIN dbo.AttdnProcessData AS AttDt ON E.SystemID = AttDt.EmpSystemID AND (AttDt.WorkDate = '" + sAttnDate + @"' OR (AttDt.WorkDate = '" + sAttnDatePrev + @"' and attdt.OutTime is null))
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
        public void xGetAttdnDataForMonthlyProc(string sGroupID, string sAttnDate, string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate, 
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
                                                       WHEN DayStatus = 'HDP' THEN 0.5	 
                                                       ELSE 0 END,
			                            TotalLate = CASE WHEN DayStatus = 'L' THEN 1
                                                       WHEN DayStatus = 'WL' THEN 1 
						                               WHEN DayStatus = 'HL' THEN 1  
                                                       WHEN DayStatus = 'WHL' THEN 1 
						                               WHEN DayStatus = 'HWL' THEN 1  
                                                       ELSE 0 END,
			                            TotalAbsent = CASE WHEN DayStatus = 'A' THEN 1 
                                                        WHEN DayStatus = 'HDP' and LTSystemID is null THEN 0.5
                                                        ELSE 0 END,
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
                                                       WHEN DayStatus = 'HDP' and LTSystemID is not null THEN 0.5  
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

        public void GetAttdnDataForMonthlyProcNew(string sGroupID, string plantid, string sAttnDate, string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                clsCrossModule ob = new clsCrossModule();
                strSql = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(TotalPresent, 0)) TotalPresent, SUM(ISNULL(TotalLate, 0)) TotalLate, 
		                            SUM(ISNULL(TotalAbsent, 0)) TotalAbsent, SUM(ISNULL(TotalLv, 0)) TotalLv, 
		                            SUM(ISNULL(TotalMLv, 0)) TotalMLv, SUM(ISNULL(TotalWeekOff, 0)) TotalWeekOff, SUM(ISNULL(TotalCompAssignLv, 0)) TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay, 0)) TotalHoliDay, SUM(ISNULL(TotalWeekOffHoliDay, 0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(OTHr, 0)) TotalOTHr, PlantID ,
                                    SUM(ISNULL(TotalLWP, 0)) TotalLWP 
                            FROM (SELECT EmpSystemID, WorkDate, PlantID,

			                           " + ob.GetAttSum() + @"

                                        OTHr
	                             FROM dbo.AttdnProcessData left join daytype p on AttdnProcessData.DayStatus=p.DayType
                                WHERE GroupID = '" + sGroupID + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @") 
                                    AND MONTH(WorkDate) = MONTH('" + sAttnDate + @"')
                                    AND YEAR(WorkDate) = YEAR('" + sAttnDate + @"')) A
                            GROUP BY EmpSystemID, PlantID";

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
        public void GetAttdnDataForMonthlyProc(string sGroupID, string plantid, string sAttnDate, string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                clsCrossModule ob = new clsCrossModule();
                strSql = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(TotalPresent, 0)) TotalPresent, SUM(ISNULL(TotalLate, 0)) TotalLate, 
		                            SUM(ISNULL(TotalAbsent, 0)) TotalAbsent, SUM(ISNULL(TotalLv, 0)) TotalLv, 
		                            SUM(ISNULL(TotalMLv, 0)) TotalMLv, SUM(ISNULL(TotalWeekOff, 0)) TotalWeekOff, SUM(ISNULL(TotalCompAssignLv, 0)) TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay, 0)) TotalHoliDay, SUM(ISNULL(TotalWeekOffHoliDay, 0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(OTHr, 0)) TotalOTHr, PlantID ,
                                    SUM(ISNULL(TotalLWP, 0)) TotalLWP 
                            FROM (SELECT EmpSystemID, WorkDate, PlantID,

			                           " + ob.xxGetAttSum() + @"

                                        OTHr
	                             FROM dbo.AttdnProcessData 
                                WHERE GroupID = '" + sGroupID + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @") 
                                    AND MONTH(WorkDate) = MONTH('" + sAttnDate + @"')
                                    AND YEAR(WorkDate) = YEAR('" + sAttnDate + @"')) A
                            GROUP BY EmpSystemID, PlantID";

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
        public void xxGetAttdnDataForMonthlyProc(string sGroupID, string sAttnDate, string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(TotalPresent, 0)) TotalPresent, SUM(ISNULL(TotalLate, 0)) TotalLate, 
		                            SUM(ISNULL(TotalAbsent, 0)) TotalAbsent, SUM(ISNULL(TotalLv, 0)) TotalLv, 
		                            SUM(ISNULL(TotalMLv, 0)) TotalMLv, SUM(ISNULL(TotalWeekOff, 0)) TotalWeekOff, SUM(ISNULL(TotalCompAssignLv, 0)) TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay, 0)) TotalHoliDay, SUM(ISNULL(TotalWeekOffHoliDay, 0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(OTHr, 0)) TotalOTHr, PlantID   
                            FROM (SELECT EmpSystemID, WorkDate, PlantID,

			                            TotalPresent = CASE WHEN DayStatus = 'P' and LTSystemID is null THEN 1 
                                                       WHEN DayStatus = 'WP' and LTSystemID is null THEN 1  
						                               WHEN DayStatus = 'HP' and LTSystemID is null THEN 1  
                                                       WHEN DayStatus = 'WHP' and LTSystemID is null THEN 1 
						                               WHEN DayStatus = 'HWP' and LTSystemID is null THEN 1 

                                                       WHEN DayStatus = 'P' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5  
                                                       WHEN DayStatus = 'WP' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5   
						                               WHEN DayStatus = 'HP' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5    
                                                       WHEN DayStatus = 'WHP' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5   
						                               WHEN DayStatus = 'HWP' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5   

						                               WHEN DayStatus = 'RST' THEN 1 
						                               WHEN DayStatus = 'OD' THEN 1 
                                                       WHEN DayStatus = 'HDP'  THEN 0.5                                                      
                                                       WHEN DayStatus = 'HDA' and LTSystemID is null THEN 0.5 

                                                       ELSE 0 END,
			                            TotalLate = CASE WHEN DayStatus = 'L' and LTSystemID is null THEN 1 
                                                       WHEN DayStatus = 'WL' and LTSystemID is null THEN 1 
						                               WHEN DayStatus = 'HL' and LTSystemID is null THEN 1  
                                                       WHEN DayStatus = 'WHL' and LTSystemID is null THEN 1 
						                               WHEN DayStatus = 'HWL' and LTSystemID is null THEN 1 
                                                       WHEN DayStatus = 'L' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5 
                                                       WHEN DayStatus = 'WL' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5  
						                               WHEN DayStatus = 'HL' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5   
                                                       WHEN DayStatus = 'WHL' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5  
						                               WHEN DayStatus = 'HWL' and LTSystemID is not null and IsHalfDayLeave=1  THEN 0.5                                                       

                                                       ELSE 0 END,
			                            TotalAbsent = CASE WHEN DayStatus = 'A' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'WA' and LTSystemID is null THEN 1  
                                                        WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5  
                                                        WHEN DayStatus = 'A' and LTSystemID is not null and IsHalfDayLeave=1 THEN 0.5 
                                                        WHEN DayStatus = 'HDP' and LTSystemID is null THEN 0.5
                                                        WHEN DayStatus = 'HDA' THEN 0.5                                                      
                                                        ELSE 0 END,
			                            TotalLv = CASE WHEN LTSystemID is not null  and IsHalfDayLeave=1 THEN 0.5 
														  WHEN LTSystemID is not null  and IsHalfDayLeave=0  THEN 1
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
        private string CardNumConvert(string strRawCardNum, int i)
        {
            string TmpNum = "";
            string TmpNum1 = "";
            string TmpNum2 = "";
            string Hex1 = "";
            string Hex2 = "";

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
        private string CardDecimal(string ProxcardNo, int i)
        {
            string Idcrd = "";

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


        public void GetCompensatoryOffPlantData(string PlantId, string wDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select *,'original' flag from[MST].[CompensatoryOff] where PlantId = '" + PlantId + @"' and OriginalDate = '" + wDate + @"' and ForEntirePlant=1
                union
                select*,'compensatory' flag from[MST].[CompensatoryOff] where PlantId = '" + PlantId + @"' and CompensatoryDate = '" + wDate + @"' and ForEntirePlant = 1";

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
        public void GetCompensatoryOffEmpListData(string PlantId, string wdate, string empList, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select Co.*,col.EmpSystemId,'original' flag from [MST].[CompensatoryOff] co
                            left join [MST].[CompensatoryOffEmpList] col on co.Id=col.CompensatoryOffId
                            where co.PlantId='" + PlantId + @"' and (co.ForEntirePlant=1 or col.EmpSystemId in(" + empList + @")) and co.OriginalDate='" + wdate + @"' 
                            union
                            select Co.*,col.EmpSystemId,'compensatory' flag from [MST].[CompensatoryOff] co
                            left join [MST].[CompensatoryOffEmpList] col on co.Id=col.CompensatoryOffId
                            where co.PlantId='" + PlantId + @"' and (co.ForEntirePlant=1 or col.EmpSystemId in(" + empList + @")) and co.CompensatoryDate='" + wdate + @"' ";

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
    public class ReturnType
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
