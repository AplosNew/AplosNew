using clsAttendance;
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

namespace Library.HumanResource.Attendance.Manual
{
    public class MT
    {
        public bool IsError = false;
        public List<ManualAttendanceWShift> data = null;
        public string msg = string.Empty;
    }
    public class clsManualAttendanceWithShift
    {
        ISqlRepository _sqlRepository;
        public clsManualAttendanceWithShift()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetEmpData(string plantId, string companyId, string date)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT Emp.SystemID,EMP.EmployeeCode,s.SystemID ShiftId, s.UserName ShiftName,EMP.CompanyId,EMP.GroupID,EMP.PlantId
                                        FROM EmployeeInformation EMP
										Left join JobLocation jl on jl.SystemID=EMP.JobLocationID
										left join ShiftDefination s on s.PlantID = jl.PlantID
                                        WHERE emp.PlantID='" + plantId + @"'  
										and EMP.CompanyId='" + companyId + @"' 
										and EMP.EmployeeStatus='Active' 
										and EMP.DOJ <= ( '" + Convert.ToDateTime(date).ToString("dd-MMM-yyyy") + @"') 
										and EMP.EmployeeStatus = 'Active' OR COnvert(date,DOS) >= ( '" + Convert.ToDateTime(date).ToString("dd-MMM-yyyy") + "')";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public string stringAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (string.IsNullOrEmpty(employeeid) == false)
                employeeid = " AND emp.SystemId='" + employeeid + @"' ";
            else
            {
                todate = fromdate;
            }
            return @" SELECT convert(bit, 0) AS Active,
                            kk.Id,kk.EmployeeCode,E.UserName as Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(KK.WorkDate,'ddd') AS DayName, 
                            format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 

                            KK.ShiftSystemID,kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
                            format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                     	    format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,


                            format(KK.InTime,'dd-MMM-yyyy') AS  InDate,--format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
                            format(KK.InTime,'hh:mm tt') AS  InTime,-- format(KK.InTime,'hh:mm tt') AS  InTimeOriginal, 

                            --KK.IsManualInTime, 


						
                            format(KK.OutTime,'dd-MMM-yyyy') AS  OutDate,
                            format(KK.OutTime,'hh:mm tt') AS  OutTime, 

                            KK.DayStatus 
                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, --O.IsManualInTime,
		                            O.OutTime, --O.IsManualOutTime, O.IsManualDayStatus,
       
		                            --O.PunchInTime,O.PunchOutTime,
                                    --O.Reason,O.ProposedIntime,O.ProposedOutTime,
		                            O.DayStatus-- O.OTHr, O.IsOTComfirm,
		                            --O.IsOTEntitled

		                            FROM EmployeeInformation EMP
		                            LEFT JOIN [dbo].[AttndManualDataFromApp] O ON EMP.SystemID=o.EmpSystemID and o.WorkDate BETWEEN   '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            --WHERE o.WorkDate BETWEEN   '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
                        ) AS KK
                        LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=kk.ShiftSystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
						    LEFT OUTER JOIN EmployeeInformation EMP ON KK.Id=EMP.SystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                        where emp.plantid='" + identity.PlantId + @"' and emp.employeeStatus='Active'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";
        }

        public MT Save(List<ManualAttendanceWShift> data)
        {
            try
            {
                List<ManualAttendanceWShift> DataToBeSaved = new List<ManualAttendanceWShift>();

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
                    foreach (ManualAttendanceWShift item in DataToBeSaved)
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
                            return new MT { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                        }
                    }
                }
                catch (Exception)
                {
                }
                foreach (ManualAttendanceWShift item in DataToBeSaved)
                {
                    if (string.IsNullOrEmpty(item.InDate) == false)
                        if (bplib.clsWebLib.IsDateOK(item.InDate) == false)
                            item.ErrorMessage = "Invalid in date";


                    if (string.IsNullOrEmpty(item.OutDate) == false)
                        if (bplib.clsWebLib.IsDateOK(item.OutDate) == false)
                            item.ErrorMessage = "Invalid out date";

                    if (item.InTime != null && item.OutTime != null)
                    {
                        if (item.InDate + item.InTime != item.InDateOriginal + item.InTimeOriginal
                            || item.OutDate + item.OutTime != item.OutDateOriginal + item.OutTimeOriginal)
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
                }

                if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                {
                    return new MT { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                }
                //operations
                saveData(DataToBeSaved);
                return new MT { data = data, IsError = false, msg = "Time updated successfully" };
            }
            catch (Exception ex)
            {
                return new MT { data = data, IsError = true, msg = ex.Message };
            }
        }

        private void saveData(List<ManualAttendanceWShift> data)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID objId = new bplib.clsGenID();
                for (int i = 0; i < data.Count; i++)
                {
                    #region manual Attendance

                    DataSet dsManualAttendance = null;
                    DataSet dsDateWise = null;


                    con = new ConnectionManager.clsConnection();
                    con.BeginTransaction();
                    con.getDataSet(@"SELECT * FROM AttndManualDataFromApp AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsManualAttendance);
                    con.getDataSet(@"SELECT * FROM EmpDateWiseShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsDateWise);
                    con.CommitTransaction();

                    var CurrentDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    if (data[i].WorkDate == CurrentDate)
                    {
                        if (dsDateWise.Tables[0].Rows.Count > 0)
                        {
                            DataRow dx = dsDateWise.Tables[0].Rows[0];
                            dx.BeginEdit();
                            dx["ManualShiftId"] = data[i].ShiftSystemID;
                            dx["ShiftSystemID"] = data[i].ShiftSystemID;
                            dx["UpdatedBy"] = identity.Name;
                            dx["DateUpdated"] = System.DateTime.Now;
                            dx.EndEdit();
                        }
                        else
                        {
                            DataRow dx = dsDateWise.Tables[0].NewRow();
                            dx["ManualShiftId"] = data[i].ShiftSystemID;
                            dx["ShiftSystemID"] = data[i].ShiftSystemID;
                            dx["UpdatedBy"] = identity.Name;
                            dx["DateUpdated"] = System.DateTime.Now;
                            dsDateWise.Tables[0].Rows.Add(dx);
                        }
                    }

                    if (dsManualAttendance.Tables[0].Rows.Count > 0)
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

                        dr["ShiftSystemId"] = data[i].ShiftSystemID;
                        dr["UpdatedBy"] = identity.Name;


                        dr["DateUpdated"] = System.DateTime.Now;


                        dr.EndEdit();
                    }
                    else
                    {

                        DataRow dr = dsManualAttendance.Tables[0].NewRow();

                        dr["EmpSystemID"] = data[i].Id;
                        if (data[i].WorkDate == null)
                        {
                            dr["WorkDate"] = data[i].InDate;
                        }
                        else
                        {
                            dr["WorkDate"] = data[i].WorkDate;
                        }

                        dr["ShiftSystemId"] = data[i].ShiftSystemID;
                        dr["GroupID"] = identity.CompanyGroupId;

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
                        dr["DateUpdated"] = System.DateTime.Now;
                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = System.DateTime.Now;

                        dsManualAttendance.Tables[0].Rows.Add(dr);
                    }
                    #endregion manual Attendance

                    if (dsManualAttendance != null)
                    {
                        if (dsManualAttendance.Tables[0].DefaultView.Count > 0)
                        {
                            if (string.IsNullOrEmpty(dsManualAttendance.Tables[0].DefaultView[0]["DayStatus"].ToString()) == true
                                && string.IsNullOrEmpty(dsManualAttendance.Tables[0].DefaultView[0]["InTime"].ToString()) == true
                                 && string.IsNullOrEmpty(dsManualAttendance.Tables[0].DefaultView[0]["OutTime"].ToString()) == true)
                            {
                                //dsManualAttendance.Tables[0].DefaultView[0].Delete();
                            }
                        }
                    }

                    SaveDataSets(dsManualAttendance, dsDateWise);

                    try
                    {

                    }
                    catch (Exception ex)
                    {

                        throw new Exception("Error occured while processing attendance " + ex.Message);
                    }



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


public class ManualAttendanceWShift : BaseModel
{
    public string Id { get; set; } = "";
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string Section { get; set; } = "";
    public string SubSection { get; set; } = "";
    public string Department { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Entity { get; set; } = "";
    public string LTSystemID { get; set; } = "";
    public string LTSystemIDOriginal { get; set; } = "";

    public bool IsOD { get; set; } = false;
    public string AttendanceRestDetailId { get; set; } = "";


    public string DayName { get; set; } = "";
    public string WorkDate { get; set; } = "";
    public string ShiftSystemID { get; set; } = "";
    public string Reason { get; set; } = "";
    public string pindate { get; set; }
    public string pindateG { get; set; } = "";
    public string pintime { get; set; } = "";
    public string poutdate { get; set; }
    public string poutdateG { get; set; } = "";
    public string pouttime { get; set; } = "";
    public string EntryFlag { get; set; } = "";
    //public string ShiftSystemID { get; set; } = "";
    public string ShiftSystemIDOriginal { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public string ShiftInTime { get; set; } = "";
    public string ShiftOutTime { get; set; } = "";
    public string InDate { get; set; } = "";
    public string InTime { get; set; } = "";
    public string InDateOriginal { get; set; } = "";
    public string InTimeOriginal { get; set; } = "";
    public bool IsManualInTime { get; set; } = false;
    public string OutDate { get; set; } = "";
    public string OutTime { get; set; } = "";
    public string OutDateOriginal { get; set; } = "";
    public string OutTimeOriginal { get; set; } = "";
    public bool IsManualOutTime { get; set; } = false;
    public string PunchInTime { get; set; } = "";
    public string PunchOutTime { get; set; } = "";
    public string DayStatus { get; set; } = "";
    public string DayStatusNew { get; set; } = "";
    public bool IsManualDayStatus { get; set; } = false;
    public string OTHr { get; set; } = "";
    public bool IsOTComfirm { get; set; } = false;
    public bool IsOTEntitled { get; set; } = false;
    public bool IsError { get; set; } = false;
    public string ErrorMessage { get; set; } = "";
}

public class ManualAtdnWithShift
{
    public string Id { get; set; } = "";
    public string WorkDate { get; set; } = "";
    public string ShiftSystemID { get; set; } = "";
    public string InDate { get; set; } = "";
    public string InTime { get; set; } = "";
    public string OutDate { get; set; } = "";
    public string OutTime { get; set; } = "";


}
