using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance.Compliance;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class clsFinalAttendanceProcess
    {
        ISqlRepository _sqlRepository;
        public clsFinalAttendanceProcess()
        {
            _sqlRepository = new SqlRepository();
        }
        public void FinalAttendance(string fromDate, string toDate, string PlantId)
        {
            DataSet dsAttdnFnl = null;
            DataTable dtAttdnFnl = null;
            DataView dvAttdnFnl = null;
            DataRow drAttdnFnl = null;

            DataSet dsAttdn = null;

            DataSet dsDaySt = null;
            DataView dvDaySt = null;
            DataTable dtDaySt = null;

            StringCollection sEmpSysID = new StringCollection();
            clsStaticInfo objStc = new clsStaticInfo();
            string sEmpSysIDColl = "";
            List<EMPDateFinalProcess> _list = new List<EMPDateFinalProcess>();
            try
            {
                if (string.IsNullOrEmpty(PlantId) == true)
                {                    
                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (fromDate == "" || bplib.clsWebLib.IsDateOK(fromDate) == false)
                {
                    //txtProcFromDate.Focus();
                    Exception ex = new Exception("Please define From date .... (allowed format is  dd-MMM-yyyy ex: '01-APR-2021')");
                    throw (ex);
                }
                if (toDate == "" || bplib.clsWebLib.IsDateOK(toDate) == false)
                {
                    //txtProcToDate.Focus();
                    Exception ex = new Exception("Please define To date .... (allowed format is  dd-MMM-yyyy ex: '01-APR-2021')");
                    throw (ex);
                }

                if (Convert.ToDateTime(toDate) < Convert.ToDateTime(fromDate))
                {
                    //txtProcToDate.Focus();
                    Exception ex = new Exception("'From Date' can not be greater than 'To Date'");
                    throw (ex);
                }

                if (DateTime.Now < Convert.ToDateTime(fromDate))
                {
                    //txtProcFromDate.Focus();
                    Exception ex = new Exception("'From Date' can not be greater than 'Current Date'");
                    throw (ex);
                }

                var ob = new clsStaticInfo();
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);

                AttendanceProcessFinalAplos obj = new AttendanceProcessFinalAplos();

                DateTime FromDate = Convert.ToDateTime(fromDate);
                DateTime ToDate = Convert.ToDateTime(toDate);

                objStc.GetDayTypeLeast(out dsDaySt);
                dtDaySt = dsDaySt.Tables[0];

                while (FromDate <= ToDate)
                {
                    obj.ShiftProcess(PlantId, FromDate.ToString("dd-MMM-yyyy"));
                    FromDate = FromDate.AddDays(1);
                }

                FromDate = Convert.ToDateTime(fromDate);
                ToDate = Convert.ToDateTime(toDate);

                obj.GetAttdnProcessDataForFinalProcess(fromDate, toDate, PlantId, out dsAttdn);

                if (dsAttdn.Tables[0].Rows.Count > 0)
                {
                    for (int EmpCnt = 0; EmpCnt < dsAttdn.Tables[0].Rows.Count; EmpCnt++)
                    {
                        #region loop
                        if (dsAttdn.Tables[0].Rows[EmpCnt]["EmpSystemID"].ToString() == "1800372")
                        {

                        }
                        //by monir for real time process
                        var _wdate = Convert.ToDateTime(dsAttdn.Tables[0].Rows[EmpCnt]["WorkDate"].ToString());
                        var _intime = DateTime.Now.ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(dsAttdn.Tables[0].Rows[EmpCnt]["ComShiftInTime"].ToString().Trim()).ToString("HH:mm");
                        var _outime = DateTime.Now.ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(dsAttdn.Tables[0].Rows[EmpCnt]["ComShiftOutTime"].ToString().Trim()).ToString("HH:mm");
                        bool IsValidEmpForShift = false;
                        if (_wdate < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                        {
                            IsValidEmpForShift = true;
                        }
                        else if (_wdate > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                        {
                            IsValidEmpForShift = false;
                        }
                        else
                        {
                            if (Convert.ToDateTime(_intime) <= Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy HH:mm")))
                            {
                                IsValidEmpForShift = true;
                            }
                        }


                        if (IsValidEmpForShift)
                        {
                            var empid = dsAttdn.Tables[0].Rows[EmpCnt]["EmpSystemID"].ToString().Trim();
                            // var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                            var x = _list.Where(r => r.Empid == empid).FirstOrDefault();
                            if (x == null)//new 
                            {
                                EMPDateFinalProcess edob = new EMPDateFinalProcess();
                                edob.Empid = empid;
                                List<string> lob = new List<string>();
                                lob.Add(_wdate.ToString("dd-MMM-yyyy"));
                                edob.ListOfDates = lob;
                                _list.Add(edob);
                            }
                            else
                            {
                                x.ListOfDates.Add(_wdate.ToString("dd-MMM-yyyy"));
                            }

                            if (sEmpSysID.Contains(dsAttdn.Tables[0].Rows[EmpCnt]["EmpSystemID"].ToString().Trim()) == false)
                            {
                                sEmpSysID.Add(empid);
                            }
                        }

                        #endregion
                    }//for dsAttdn
                }

                for (int c = 0; c < sEmpSysID.Count; c++)
                {
                    if (sEmpSysIDColl.Trim() == "")
                    {
                        sEmpSysIDColl = "'" + sEmpSysID[c].ToString() + "'";
                    }
                    else
                    {
                        sEmpSysIDColl += ",'" + sEmpSysID[c].ToString() + "'";
                    }
                }

                obj.GetAttdnProcessFinalData(fromDate, toDate, PlantId, sEmpSysIDColl, out dsAttdnFnl);
                dtAttdnFnl = dsAttdnFnl.Tables[0];

                #region Processing on Processed Data

                if (dsAttdn.Tables[0].Rows.Count > 0)
                {
                    for (int EmpCnt = 0; EmpCnt < dsAttdn.Tables[0].Rows.Count; EmpCnt++)
                    {
                        #region Init
                        bool _IsValidEmp = false;
                        string sEmpSecSysID = dsAttdn.Tables[0].Rows[EmpCnt]["EmpSystemID"].ToString().Trim();
                        string sWorkDate = dsAttdn.Tables[0].Rows[EmpCnt]["WorkDate"].ToString().Trim();

                        if (dsAttdn.Tables[0].Rows[EmpCnt]["EmpSystemID"].ToString() == "1800009")
                        {

                        }
                        if (sEmpSysID.Contains(sEmpSecSysID))
                        {
                            var x = _list.Where(r => r.Empid == sEmpSecSysID).FirstOrDefault();
                            string _date = Convert.ToDateTime(sWorkDate).ToString("dd-MMM-yyyy");
                            if (x != null)
                            {
                                if (x.ListOfDates.Contains(_date))
                                {
                                    _IsValidEmp = true;
                                }
                            }
                        }

                        if (_IsValidEmp)
                        {
                            string sDayStatus = string.Empty;
                            sDayStatus = dsAttdn.Tables[0].Rows[EmpCnt]["DayStatus"].ToString().Trim();
                            //GetDayStatus(dsAttdn, sEmpSecSysID, sWorkDate, out sDayStatus);
                            string sDayCategory = "";
                            string sIntime = "";

                            dvDaySt = new DataView();
                            dvDaySt.Table = dtDaySt;
                            dvDaySt.RowFilter = "DayType = '" + sDayStatus + "'";
                            if (dvDaySt.Count > 0)
                            {
                                sDayCategory = dvDaySt[0].Row["Category"].ToString();
                            }

                            if (sDayCategory == "Present" || sDayCategory == "Late" || sDayCategory == "Half Day")
                            {
                                sIntime = dsAttdn.Tables[0].Rows[EmpCnt]["ComInTime"].ToString().Trim();
                            }

                            //?? 
                            #endregion
                            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                            dvAttdnFnl = new DataView();
                            dvAttdnFnl.Table = dtAttdnFnl;
                            dvAttdnFnl.RowFilter = "EmpSystemID = '" + sEmpSecSysID + "' AND WorkDate = '" + sWorkDate + "'";
                            if (dvAttdnFnl.Count > 0)
                            {
                                #region Edit
                                drAttdnFnl = dvAttdnFnl[0].Row;
                                drAttdnFnl.BeginEdit();


                                drAttdnFnl["EmpSystemID"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["EmpSystemID"].ToString().Trim());
                                drAttdnFnl["WorkDate"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["WorkDate"].ToString().Trim());
                                drAttdnFnl["ShiftID"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["CompliedShiftId"].ToString().Trim());
                                drAttdnFnl["InTime"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["ComInTime"].ToString().Trim());
                                drAttdnFnl["OutTime"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["ComOutTime"].ToString().Trim());
                                drAttdnFnl["DayStatus"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["DayStatus"].ToString().Trim());
                                drAttdnFnl["GroupID"] = bplib.clsWebLib.RetValidLen(identity.CompanyGroupId);
                                drAttdnFnl["PlantID"] = bplib.clsWebLib.RetValidLen(PlantId);

                                drAttdnFnl["UpdatedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drAttdnFnl["DateUpdated"] = DateTime.Now;

                                drAttdnFnl.EndEdit();
                                #endregion
                            }
                            else
                            {
                                #region Add
                                drAttdnFnl = dtAttdnFnl.NewRow();

                                drAttdnFnl["EmpSystemID"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["EmpSystemID"].ToString().Trim());
                                drAttdnFnl["WorkDate"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["WorkDate"].ToString().Trim());
                                drAttdnFnl["ShiftID"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["CompliedShiftId"].ToString().Trim());
                                drAttdnFnl["InTime"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["ComInTime"].ToString().Trim());
                                drAttdnFnl["OutTime"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["ComOutTime"].ToString().Trim());
                                drAttdnFnl["DayStatus"] = bplib.clsWebLib.RetValidLen(dsAttdn.Tables[0].Rows[EmpCnt]["DayStatus"].ToString().Trim());
                                drAttdnFnl["GroupID"] = bplib.clsWebLib.RetValidLen(identity.CompanyGroupId);
                                drAttdnFnl["PlantID"] = bplib.clsWebLib.RetValidLen(PlantId);

                                drAttdnFnl["AddedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drAttdnFnl["DateAdded"] = DateTime.Now;

                                dtAttdnFnl.Rows.Add(drAttdnFnl);
                                #endregion
                            }
                        }//valid
                    }//for
                }//count 
                #endregion

                objStc.SaveDataSets(dsAttdnFnl);

                //ShowLog("Process completed!!!");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

class EMPDateFinalProcess
{
    public string Empid { get; set; }
    public List<string> ListOfDates { get; set; }
}
