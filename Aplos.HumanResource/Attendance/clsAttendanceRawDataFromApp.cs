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
                    if (string.IsNullOrEmpty(item.InDate) == false)
                        if (bplib.clsWebLib.IsDateOK(item.InDate) == false)
                            item.ErrorMessage = "Invalid in date";


                    if (string.IsNullOrEmpty(item.OutDate) == false)
                        if (bplib.clsWebLib.IsDateOK(item.OutDate) == false)
                            item.ErrorMessage = "Invalid out date";

                    if (item.InTime != null && item.OutTime != null)
                    {
                        if (Convert.ToDateTime(item.InDate + " " + item.InTime) > Convert.ToDateTime(item.OutDate + " " + item.OutTime))
                        {
                            item.IsError = true;
                            item.ErrorMessage = "Out time is earlier than In time";
                        }

                        TimeSpan ts = Convert.ToDateTime(item.OutDate + " " + item.OutTime).Subtract(Convert.ToDateTime(item.InDate + " " + item.InTime));
                        if (Math.Abs(ts.TotalHours) > 24)
                        {
                            item.IsError = true;
                            item.ErrorMessage = "Time span cannot be greater than 24 hours between in and out time";
                        }

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

                        DataRow dr = dsManualAttendance.Tables[0].Rows[0];

                        dr.BeginEdit();
                        if (data[i].InDate != null && data[i].InTime == null)
                        {
                            dr["InTime"] = data[i].InDate;
                        }
                        else
                        {
                            if (data[i].InDate == null && data[i].InTime == null)
                            {
                                dr["InTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["InTime"] = data[i].InDate + " " + data[i].InTime;
                            }

                        }
                        if (data[i].OutDate != null && data[i].OutTime == null)
                        {
                            dr["OutTime"] = data[i].OutDate;
                        }
                        else
                        {
                            if (data[i].OutDate == null && data[i].OutTime == null)
                            {
                                dr["OutTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["OutTime"] = data[i].OutDate + " " + data[i].OutTime;
                            }

                        }
                        dr["UpdatedBy"] = identity.Name;


                        dr["UpdatedDate"] = System.DateTime.Now;


                        dr.EndEdit();
                    }
                    else
                    {

                        DataRow dr = dsManualAttendance.Tables[0].NewRow();

                        count++;
                        string pk = "A" + seed_detail + "_" + count;
                        //dr = dtMSave.NewRow();
                        dr["Id"] = pk;

                        dr["EmployeeId"] = data[i].Id;
                        if (data[i].WorkDate == null)
                        {
                            dr["PDate"] = data[i].InDate;
                        }
                        else
                        {
                            dr["PDate"] = data[i].WorkDate;
                        }



                        if (data[i].InDate != null && data[i].InTime == null)
                        {
                            dr["InTime"] = data[i].InDate;
                        }
                        else
                        {
                            if (data[i].InDate == null && data[i].InTime == null)
                            {
                                dr["InTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["InTime"] = data[i].InDate + " " + data[i].InTime;
                            }

                        }
                        if (data[i].OutDate != null && data[i].OutTime == null)
                        {
                            dr["OutTime"] = data[i].OutDate;
                        }
                        else
                        {
                            if (data[i].OutDate == null && data[i].OutTime == null)
                            {
                                dr["OutTime"] = DBNull.Value;
                            }
                            else
                            {
                                dr["OutTime"] = data[i].OutDate + " " + data[i].OutTime;
                            }

                        }

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
    public string InDate { get; set; } = "";
    public string InTime { get; set; } = "";
    public string OutDate { get; set; } = "";
    public string OutTime { get; set; } = "";
    public bool IsError { get; set; } = false;
    public string ErrorMessage { get; set; } = "";
}

public class AttendanceRawDataFromApp
{
    public string Id { get; set; } = "";
    public string EmployeeId { get; set; } = "";
    public string WorkDate { get; set; } = "";
    public string InDate { get; set; } = "";
    public string InTime { get; set; } = "";
    public string OutDate { get; set; } = "";
    public string OutTime { get; set; } = "";
    public bool isApprovedIN { get; set; }
    public bool isApprovedOUT { get; set; }
}