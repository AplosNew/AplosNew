using SetINOUT;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;

namespace Attendance
{
    public class DownloadApi
    {
        public DownloadApi()
        {

        }
        public void GetRawData(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                @strSQL = @"   select * from  AttdnRawData   where  pdate='" + DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.getDataSet(strSQL, out dsRef);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function 
        public void GetAccessControllerList(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                @strSQL = @"   select Id DeviceSystemid,MachineID,PlantId,  IsDeviceBasedInOut,  DeviceInOutFlag from  mst.AccessControllerList d   where  d.IsActive=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.getDataSet(strSQL, out dsRef);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        string GetMonthName(string monthValue)
        {
            string r = "Jan";
            try
            {

                switch (Convert.ToInt32(monthValue))
                {
                    case 1:
                        r = "Jan";
                        break;
                    case 2:
                        r = "Feb";
                        break;
                    case 3:
                        r = "Mar";
                        break;
                    case 4:
                        r = "Apr";
                        break;
                    case 5:
                        r = "May";
                        break;
                    case 6:
                        r = "Jun";
                        break;
                    case 7:
                        r = "Jul";
                        break;
                    case 8:
                        r = "Aug";
                        break;
                    case 9:
                        r = "Sep";
                        break;
                    case 10:
                        r = "Oct";
                        break;
                    case 11:
                        r = "Nov";
                        break;
                    case 12:
                        r = "Dec";
                        break;
                    default:
                        r = "Jan";
                        break;
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetEmployeePK(string cardno, DataSet dtwithempsystemid, out string employeeid, out string plantid)
        {
            employeeid = string.Empty;
            plantid = string.Empty;
            try
            {
                DataView dv = new DataView(dtwithempsystemid.Tables[0]);
                dv.RowFilter = "cardnumber='" + cardno + "' and EmployeeStatus='Active' ";
                if (dv.Count > 0)
                {
                    employeeid = dv[0]["employeeid"].ToString();
                    plantid = dv[0]["Plantid"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetDate(string v)
        {
            try
            {
                if (v != null && v.Trim().Length == 8)
                {
                    return v.Substring(6, 2) + "-" + GetMonthName(v.Substring(4, 2)) + "-" + v.Substring(0, 4);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string xGetMonthName(string monthValue)
        {
            string r = "Jan";
            try
            {

                switch (Convert.ToInt32(monthValue))
                {
                    case 1:
                        r = "Jan";
                        break;
                    case 2:
                        r = "Feb";
                        break;
                    case 3:
                        r = "Mar";
                        break;
                    case 4:
                        r = "Apr";
                        break;
                    case 5:
                        r = "May";
                        break;
                    case 6:
                        r = "Jun";
                        break;
                    case 7:
                        r = "Jul";
                        break;
                    case 8:
                        r = "Aug";
                        break;
                    case 9:
                        r = "Sep";
                        break;
                    case 10:
                        r = "Oct";
                        break;
                    case 11:
                        r = "Nov";
                        break;
                    case 12:
                        r = "Dec";
                        break;
                    default:
                        r = "Jan";
                        break;
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetTime(string v)
        {
            try
            {
                if (v != null && v.Trim().Length == 6)
                {
                    return v.Substring(0, 2) + ":" + v.Substring(2, 2) + ":" + v.Substring(4, 2);
                }
                return "00:00:00";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void FromDataTableToList(List<Rfc> RfcList, out List<RawData> EmpList)
        {
            EmpList = new List<RawData>();
            try
            {

                //"Provider=Microsoft.JET.OLEDB.4.0;" + "data source=C:\\menus\\newmenus\\menu.mdb;Password=****"
                //var _FileLocation = ConfigurationManager.AppSettings["FileLocation"];
                //if (File.Exists(_FileLocation) == false)
                //{
                //    throw new Exception("No file found in [" + _FileLocation + "]");
                //}
                //var lines = File.ReadLines(_FileLocation);
                foreach (var item in RfcList)
                {
                    RawData rd = new RawData();
                    var dr = item;
                    //003 2020 0110 1107 0100000000621
                    string _cardno = item.card_no;// dr["card_no"].ToString();
                    string _deviceid = item.node_no;// dr["node_no"].ToString();
                    string t_card = item.t_card;// dr["t_card"].ToString();
                    string d_card = item.d_card;// dr["d_card"].ToString();

                    string _date = GetDate(d_card);
                    string _time = GetTime(t_card);
                    if (string.IsNullOrEmpty(_date) == false)
                    {
                        rd.Date = _date;
                        try
                        {
                            rd.DateTime = Convert.ToDateTime(_date + " " + _time).ToString("dd-MMM-yyyy HH:mm:ss");
                        }
                        catch (Exception exx)
                        {
                            throw new Exception("DateConvert: " + exx);
                        }
                        rd.DeviceId = _deviceid;
                        rd.EmpCard = _cardno.ToString();
                        rd.Flag = item.Flag;
                        EmpList.Add(rd);
                    }//if
                }//for
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetEmployeeInfo(string CardNumber, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                @strSQL = @" select e.SystemId,e.GroupId ,e.EmployeeCode,e.PlantId,e.CardNumber
                            ,e.EmployeeStatus ,isnull(h.ShiftBasedPunchFlag,0) ShiftBasedPunchFlag
                            from EmployeeInformation e
                            left join PlantWiseHRMSSetting h on h.PlantID=e.PlantId
                            where e.CardNumber in (" + CardNumber + @") and e.EmployeeStatus='Active'
                                ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.getDataSet(strSQL, out dsRef);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function 
        void GetDevicePK(DataSet dsAccessController, string _DeviceId, out string DevicePK, out bool IsOK)
        {
            try
            {
                IsOK = false;
                DevicePK = string.Empty;
                DataView dvAC = new DataView(dsAccessController.Tables[0]);
                dvAC.RowFilter = "MachineID=" + _DeviceId + "";
                if (dvAC.Count > 0)
                {
                    DevicePK = dvAC[0]["DeviceSystemid"].ToString();
                    IsOK = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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
        public static string getUserDateFormat()
        {
            System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString();
        }
        void GetEmpInfo(DataSet dsEmpInfo, string cardno, out string groupId, out string Plantid, out string EmpPK, out bool IsShiftBasedPunchFlag, out bool IsOK)
        {
            try
            {
                EmpPK = string.Empty;
                Plantid = string.Empty;
                groupId = string.Empty;
                IsShiftBasedPunchFlag = false;
                IsOK = false;
                DataView dvAC = new DataView(dsEmpInfo.Tables[0]);
                dvAC.RowFilter = "CardNumber='" + cardno + "' and EmployeeStatus='Active' ";
                //dvAC.RowFilter = "EmployeeCode=" + EmployeeCode + "";
                if (dvAC.Count > 0)
                {
                    EmpPK = dvAC[0]["SystemId"].ToString();
                    Plantid = dvAC[0]["PlantId"].ToString();
                    groupId = dvAC[0]["groupId"].ToString();
                    IsShiftBasedPunchFlag = Convert.ToBoolean(dvAC[0]["ShiftBasedPunchFlag"].ToString());
                    IsOK = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void UpdateAttdnRawData(string sPType, string OPN_FLAG, string _pk, string GroupSysID, string iDeviceID, string sDevSystemID, string sCardNumber, string sDate, string sTime, string sPlantID, ref System.Data.DataRow drLocal)
        {
            //string _pk = string.Empty;
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["AddedBy"] = "Schedule";
                    drLocal["DateAdded"] = System.DateTime.Now;
                }

                drLocal["Id"] = _pk;
                drLocal["DeviceID"] = iDeviceID;
                if (string.IsNullOrEmpty(sDevSystemID) == false)
                {
                    drLocal["DevSystemID"] = sDevSystemID;
                }
                else
                {
                    drLocal["DevSystemID"] = DBNull.Value;
                }
                drLocal["LogDownLoadNum"] = sCardNumber;
                drLocal["PDate"] = sDate;
                drLocal["PTime"] = sTime;

                if (string.IsNullOrEmpty(sPType))
                {
                    drLocal["PType"] = DBNull.Value;
                }
                else
                {
                    drLocal["PType"] = sPType;
                }

                drLocal["GroupID"] = GroupSysID.Trim();
                drLocal["PlantID"] = sPlantID.Trim();

                drLocal["UpdatedBy"] = "Schedule";
                drLocal["DateUpdated"] = System.DateTime.Now;
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
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

                    LastNumber = Convert.ToDecimal(("" + drLocal["LastNumber"].ToString()));
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
        public void GetEmpCodes(List<RawData> EmpList, out string empcodes)
        {
            empcodes = "''";
            try
            {
                foreach (var emp in EmpList)
                {
                    empcodes += ",'" + emp.EmpCard + "'";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetPlant(string CompanyGroupId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT p.CompanyGroupId, p.Id ,isnull(s.ShiftBasedPunchFlag,0) ShiftBasedPunchFlag
                                            FROM ORG.Plant p
                                            left join PlantWiseHRMSSetting s on s.Plantid=p.id
                                            WHERE p.CompanyGroupId = '" + CompanyGroupId + @"' AND  p.Active = 1 AND p.Archive = 0";

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
        public void SaveData(List<Rfc> rfcList)
        {
            DataSet dsAccessController = null;
            DataSet dsEmpInfo = null;
            string EmpCards = string.Empty;
            bool IsDeviceOK = false;
            bool IsEmpOK = false;
            string DevicePK = string.Empty;
            string _DateTime = string.Empty;
            string _EmpPK = string.Empty;
            string _PlantId = string.Empty;
            string _groupId = string.Empty;
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataRow drLocal = null;
            List<RawData> EmpList = null;
            DateTime _minDate = DateTime.Now;
            DateTime _maxDate = DateTime.Now;
            try
            {
                //var GetCompanyGroupId = ConfigurationManager.AppSettings["GROUPID"];
                //var plantids = _plantId;// ConfigurationManager.AppSettings["PlantId"];

                //List<RawData> EmpList = null;
                //ReadAllFile(out EmpList);
                FromDataTableToList(rfcList, out EmpList);
                GetEmpCodes(EmpList, out EmpCards);//comma separated empCodes
                GetAccessControllerList(out dsAccessController);
                GetEmployeeInfo(EmpCards, out dsEmpInfo);
                GetRawData(out dsLocal);
                //get device pk,
                //get emp pk
                //get plantid
                int Count = 0;
                string _pk = string.Empty;
                try
                {
                    GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_ATT_RAW", out _pk);
                }
                catch (Exception exx)
                {
                    throw new Exception("PK: " + exx);
                }
                dvLocal = new DataView(dsLocal.Tables[0]);
                bool IsFirst = true;
                bool IsShiftBasedPunchFlag = false;
                foreach (var punch in EmpList)
                {
                    IsShiftBasedPunchFlag = false;
                    try
                    {
                        if (IsFirst)
                        {
                            _maxDate = Convert.ToDateTime(punch.Date);
                            IsFirst = false;
                        }
                        if (_minDate > Convert.ToDateTime(punch.Date))
                        {
                            _minDate = Convert.ToDateTime(punch.Date);
                        }
                        if (_maxDate < Convert.ToDateTime(punch.Date))
                        {
                            _maxDate = Convert.ToDateTime(punch.Date);
                        }
                    }
                    catch (Exception eex)
                    {
                        throw new Exception("DateConvert2: " + eex);
                    }

                    _DateTime = punch.DateTime;
                    GetDevicePK(dsAccessController, punch.DeviceId, out DevicePK, out IsDeviceOK);
                    GetEmpInfo(dsEmpInfo, punch.EmpCard, out _groupId, out _PlantId, out _EmpPK, out IsShiftBasedPunchFlag, out IsEmpOK);
                    if (IsShiftBasedPunchFlag)//as per hr setting
                    {
                        punch.Flag = string.Empty;
                    }

                    dvLocal.RowFilter = "LogDownLoadNum = '" + _EmpPK + "' and DeviceID='" + punch.DeviceId + "' AND PDate = '" + punch.Date + "' AND PTime = '" + punch.DateTime + "' ";
                    if (dvLocal.Count == 0 && IsEmpOK == true)
                    {
                        Count++;
                        drLocal = dsLocal.Tables[0].NewRow();
                        string _systemid = _pk + "-" + Count.ToString();
                        UpdateAttdnRawData(punch.Flag, "ADDNEW", _systemid, _groupId, punch.DeviceId, DevicePK, _EmpPK, punch.Date, punch.DateTime, _PlantId, ref drLocal);
                        dsLocal.Tables[0].Rows.Add(drLocal);
                    }
                    dvLocal.RowFilter = null;
                }//foreach
                try
                {
                    SaveDataSets(dsLocal);
                }
                catch (Exception s)
                {
                    throw new Exception("Save: " + s);
                }
                //call setINOUT flagging func
                ExecuteFlagSetting(_groupId, _minDate, _maxDate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void ExecuteFlagSetting(string GetCompanyGroupId, DateTime FromDate, DateTime ToDate)
        {
            DataSet dsPlant = null;
            try
            {
                clsSetInOut sio = new clsSetInOut();
                GetPlant(GetCompanyGroupId, out dsPlant);
                DateTime _fromD = FromDate.AddDays(-1);
                FromDate = _fromD;
                while (FromDate <= ToDate)
                {
                    for (int i = 0; i < dsPlant.Tables[0].Rows.Count; i++)
                    {
                        var _plantid = dsPlant.Tables[0].Rows[i][@"Id"].ToString();//ShiftBasedPunchFlag
                        if (Convert.ToBoolean(dsPlant.Tables[0].Rows[i]["ShiftBasedPunchFlag"].ToString()) == true)
                        {
                            sio.SetRawINOUT(_plantid, GetCompanyGroupId, ToDate.ToString("dd-MMM-yyyy"), "");
                        }
                    }
                    ToDate = ToDate.AddDays(-1);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error (Flag setting): " + ex);
            }
        }
        public void Execute(DateTime FromDate, DateTime ToDate, string _plantId, string _companyGroupId)
        {
            try
            {
                clsSetInOut sio = new clsSetInOut();
                DateTime _fromD = FromDate.AddDays(-1);
                FromDate = _fromD;
                while (FromDate <= ToDate)
                {
                    sio.SetRawINOUT(_plantId, _companyGroupId, ToDate.ToString("dd-MMM-yyyy"), "");
                    ToDate = ToDate.AddDays(-1);
                }
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
        public void SetRawINOUT(string plantid, string workdate, string sEmpSystemIDColl)
        {
            DataSet dsEmpShift = null;
            DataSet dsRaw = null;
            try
            {
                GetEmpDateWise(plantid, workdate, out dsEmpShift);
                GetTypeLessRawData(plantid, workdate, out dsRaw);
                //calculation
                for (int i = 0; i < dsRaw.Tables[0].Rows.Count; i++)
                {
                    string _rid = dsRaw.Tables[0].Rows[i]["Id"].ToString();
                    if (_rid == "TX238610")
                    {

                    }
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
                string punchDT = Convert.ToDateTime(workdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(worktime).ToString("HH:mm");
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
        private void GetEmpDateWise(string plantid, string workdate, out DataSet dsRef)
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
                        where WorkDate = '" + workdate + @"'  and e.plantid='" + plantid + "'";

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
        private void GetTypeLessRawData(string plantid, string workdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = "select * from AttdnRawData where pdate='" + workdate + "' and plantid='" + plantid + "' and ptype is null";

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

    public class RawData
    {
        public string DeviceId { get; set; }
        public string EmpCard { get; set; }
        public string Date { get; set; }
        public string DateTime { get; set; }
        public string Flag { get; set; }
    }

    public class Rfc
    {
        public string node_no { get; set; }
        public string d_card { get; set; }
        public string t_card { get; set; }
        public string card_no { get; set; }
        public string Flag { get; set; }
    }
    public class vmPlant
    {
        public string CompanyGroupId { get; set; }
        public string Id { get; set; }
    }
}
