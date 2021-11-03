using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class ARFA
    {
        public bool IsError = false;
        public List<AttendanceFromApp> data = null;
        public string msg = string.Empty;
    }
    public class clsAttendanceRawDataFromApp
    {
        ISqlRepository _sqlRepository;
        public clsAttendanceRawDataFromApp()
        {
            _sqlRepository = new SqlRepository();
        }
        public ARFA Save(List<AttendanceFromApp> data)
        {
            try
            {
                List<AttendanceFromApp> DataToBeSaved = new List<AttendanceFromApp>();

                if (data == null)
                    throw new Exception("No new data has been updated");

                for (int i = 0; i < data.Count; i++)
                {
                    DataToBeSaved.Add(data[i]);
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                try
                {
                    string inDates = "";
                    string inEmployeeIds = "";
                    foreach (AttendanceFromApp item in DataToBeSaved)
                    {
                        if (inDates == "")
                            inDates = "'" + item.WorkDate + "'";
                        else
                            inDates += ",'" + item.WorkDate + "'";


                        if (inEmployeeIds == "")
                            inEmployeeIds = "'" + item.Id + "'";
                        else
                            inEmployeeIds += ",'" + item.Id + "'";
                    }

                    if (inDates != "")
                    {
                        DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE isActive=1 AND pwal.LockedDate IN (" + inDates + ") AND pwal.PlantId='" + identity.PlantId + "'");
                        DataTable dtLockEmployee = _sqlRepository.GetDataTable("SELECT * FROM ExceptionEmployeeAttendanceUnlock WHERE EmpSystemId IN (" + inEmployeeIds + @")");
                        for (int i = 0; i < dtLock.Rows.Count; i++)
                        {
                            var k = DataToBeSaved.Where(ee => ee.WorkDate.ToUpper() == Convert.ToDateTime(dtLock.Rows[i]["LockedDate"].ToString()).ToString("dd-MMM-yyyy").ToUpper());
                            foreach (var item in k)
                            {
                                dtLockEmployee.DefaultView.RowFilter = "EmpSystemId='" + item.Id + "' AND WorkDate=#" + item.WorkDate + "#";
                                if (dtLockEmployee.DefaultView.Count == 0)
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "Day locked";
                                }
                            }
                        }

                        if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                        {
                            return new ARFA { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                        }
                    }
                }
                catch (Exception)
                {
                }
                foreach (AttendanceFromApp item in DataToBeSaved)
                {
                    if (string.IsNullOrEmpty(item.InDateApp) == false)
                        if (bplib.clsWebLib.IsDateOK(item.InDateApp) == false)
                            item.ErrorMessage = "Invalid in date";


                    if (string.IsNullOrEmpty(item.OutDateApp) == false)
                        if (bplib.clsWebLib.IsDateOK(item.OutDateApp) == false)
                            item.ErrorMessage = "Invalid out date";

                    if (item.InTimeApp != null && item.OutTimeApp != null)
                    {
                        //if (Convert.ToDateTime(item.InDateApp + " " + item.InTimeApp) > Convert.ToDateTime(item.OutDateApp + " " + item.OutTimeApp))
                        //{
                        //    item.IsError = true;
                        //    item.ErrorMessage = "Out time is earlier than In time";
                        //}

                        //TimeSpan ts = Convert.ToDateTime(item.OutDateApp + " " + item.OutTimeApp).Subtract(Convert.ToDateTime(item.InDateApp + " " + item.InTimeApp));
                        //if (Math.Abs(ts.TotalHours) > 24)
                        //{
                        //    item.IsError = true;
                        //    item.ErrorMessage = "Time span cannot be greater than 24 hours between in and out time";
                        //}

                    }
                }

                if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                {
                    return new ARFA { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                }
                //operations
                saveData(DataToBeSaved);
                return new ARFA { data = data, IsError = false, msg = "Time updated successfully" };
            }
            catch (Exception ex)
            {
                return new ARFA { data = data, IsError = true, msg = ex.Message };
            }
        }
        private void saveData(List<AttendanceFromApp> data)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID objId = new bplib.clsGenID();
                DataView dvMSave = null;
                DataTable dtMSave = null;
                DataRow drMSave = null;
                #region manual Attendance
                int count = 0;
                string seed_detail = string.Empty;
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "AttdnRawDataFromApp", out seed_detail);

                DataSet dsManualAttendance = null;
                DataSet dsDateWise = null;
                string inEmployeeIds = "";
                string inDates = "";
                foreach (AttendanceFromApp item in data)
                {
                    if (inDates == "")
                        inDates = "'" + item.WorkDate + "'";
                    else
                        inDates += ",'" + item.WorkDate + "'";


                    if (inEmployeeIds == "")
                        inEmployeeIds = "'" + item.Id + "'";
                    else
                        inEmployeeIds += ",'" + item.Id + "'";
                }

                con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.getDataSet(@"SELECT * FROM AttdnRawDataFromApp AS SA WHERE SA.EmployeeId in (" + inEmployeeIds + ") AND sa.PDate in (" + inDates + " )", out dsManualAttendance);
                con.CommitTransaction();

                var CurrentDate = DateTime.Now.ToString("dd-MMM-yyyy");
                for (int i = 0; i < data.Count; i++)
                {
                    dtMSave = dsManualAttendance.Tables[0];
                    dvMSave = new DataView();
                    dvMSave.Table = dtMSave;
                    dvMSave.RowFilter = "EmployeeId ='" + data[i].Id + "' and PDate = '" + data[i].WorkDate + "' ";
                    if (dvMSave.Count > 0)
                    {

                        DataRow dr = dvMSave[0].Row;

                        dr.BeginEdit();
                        if (!string.IsNullOrEmpty(data[i].InDateApp) && string.IsNullOrEmpty(data[i].InTimeApp))
                        {
                            dr["InTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(data[i].InDateApp) && string.IsNullOrEmpty(data[i].InTimeApp))
                            {
                                dr["InTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["InTime"] = data[i].InDateApp + " " + data[i].InTimeApp;
                            }

                        }
                        if (!string.IsNullOrEmpty(data[i].OutDateApp) && string.IsNullOrEmpty(data[i].OutTimeApp))
                        {
                            dr["OutTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(data[i].OutDateApp) && string.IsNullOrEmpty(data[i].OutTimeApp))
                            {
                                dr["OutTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["OutTime"] = Convert.ToDateTime(data[i].OutDateApp + " " + data[i].OutTimeApp);
                            }

                        }
                        dr["SourceFlag"] = "ManualAttendance";

                        dr["UpdatedBy"] = identity.Name;


                        dr["UpdatedDate"] = System.DateTime.Now;


                        dr.EndEdit();
                    }
                    else
                    {

                        DataRow dr = dsManualAttendance.Tables[0].NewRow();

                        count++;
                        string pk = seed_detail + "_" + count;
                        //dr = dtMSave.NewRow();
                        dr["Id"] = pk;

                        dr["EmployeeId"] = data[i].Id;
                        if (string.IsNullOrEmpty(data[i].WorkDate))
                        {
                            dr["PDate"] = data[i].InDateApp;
                        }
                        else
                        {
                            dr["PDate"] = data[i].WorkDate;
                        }



                        if (!string.IsNullOrEmpty(data[i].InDateApp) && string.IsNullOrEmpty(data[i].InTimeApp))
                        {
                            dr["InTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(data[i].InDateApp) && string.IsNullOrEmpty(data[i].InTimeApp))
                            {
                                dr["InTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["InTime"] = data[i].InDateApp + " " + data[i].InTimeApp;
                            }

                        }
                        if (!string.IsNullOrEmpty(data[i].OutDateApp) && string.IsNullOrEmpty(data[i].OutTimeApp))
                        {
                            dr["OutTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(data[i].OutDateApp) && string.IsNullOrEmpty(data[i].OutTimeApp))
                            {
                                dr["OutTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["OutTime"] = data[i].OutDateApp + " " + data[i].OutTimeApp;
                            }

                        }

                        dr["SourceFlag"] = "ManualAttendance";
                        dr["PlantId"] = identity.PlantId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now;

                        dsManualAttendance.Tables[0].Rows.Add(dr);
                    }
                }
                #endregion manual Attendance


                SaveDataSets(dsManualAttendance);

                    try
                    {

                    }
                    catch (Exception ex)
                    {

                        throw new Exception("Error occured while processing attendance " + ex.Message);
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
                        if (dsRef[i].Tables.Count > 0)
                            objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                    i++;
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
    }
}
public class AttendanceFromApp
{
    public string Id { get; set; } = "";
    public string WorkDate { get; set; } = "";
    public string InDateApp { get; set; } = "";
    public string InTimeApp { get; set; } = "";
    public string OutDateApp { get; set; } = "";
    public string OutTimeApp { get; set; } = "";
    public bool IsError { get; set; } = false;
    public string ErrorMessage { get; set; } = "";
    public bool isApprovedIN { get; set; }
    public bool isApprovedOUT { get; set; }
}

public class AttendanceRawDataFromApp : BaseModel
{
    public string Id { get; set; } = "";
    public string WorkDate { get; set; } = "";
    public string InDateApp { get; set; } = "";
    public string InTimeApp { get; set; } = "";
    public string OutDateApp { get; set; } = "";
    public string OutTimeApp { get; set; } = "";
    public bool isApprovedIN { get; set; }
    public bool isApprovedOUT { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string Section { get; set; } = "";
    public string SubSection { get; set; } = "";
    public string Department { get; set; } = "";
    public string Designation { get; set; } = "";

    public string DayName { get; set; } = "";
    public string ShiftSystemIDOriginal { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public string ShiftInTime { get; set; } = "";
    public string ShiftOutTime { get; set; } = "";
    public string InDateOriginal { get; set; } = "";
    public string InTimeOriginal { get; set; } = "";
    public bool IsManualInTime { get; set; } = false;
    public string OutDateOriginal { get; set; } = "";
    public string OutTimeOriginal { get; set; } = "";
    public bool IsManualOutTime { get; set; } = false;
    public string PunchInTime { get; set; } = "";
    public string PunchOutTime { get; set; } = "";
    public string DayStatus { get; set; } = "";
    public string OTHr { get; set; } = "";
    public bool IsOTComfirm { get; set; } = false;
    public bool IsOTEntitled { get; set; } = false;
    public bool IsManualDayStatus { get; set; } = false;


}

public class AttendanceRawFromApp
{
    public string Id { get; set; } = "";
    public string WorkDate { get; set; } = "";
    public string InDateApp { get; set; } = "";
    public string InTime { get; set; } = "";
    public string OutDateApp { get; set; } = "";
    public string OutTime { get; set; } = "";
    public bool isApprovedIN { get; set; } = true;
    public bool isApprovedOUT { get; set; } = true;
}