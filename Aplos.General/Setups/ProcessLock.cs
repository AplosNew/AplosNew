using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Setups
{
    //tarek talukder
    public enum ProcessLockId
    {
        SalaryProcess, ArrearProcess,RosterProcess,DOJProcess, AttendanceProcess, PlanningType1, TNA, TODOScheduler
    }
    public class ProcessLock
    {
        SqlRepository _sqlRepository = new SqlRepository();
        string _userid = ""; string _processid = ""; int _maxduration = 0;
        public ProcessLock(string UserId, ProcessLockId ProcessLockid, string ProcessIdSuffix = "", int MaxDuration = 15)
        {
            _userid = UserId;
            _processid = ProcessLockid.ToString() + "-" + ProcessIdSuffix;
            _maxduration = MaxDuration;
        }



        public void LockProcess()
        {
            try
            {


                _sqlRepository.ExecuteSqlCommand(@"IF OBJECT_ID('ProcessLock', 'U') IS NULL
                                                CREATE TABLE ProcessLock
                                                (
	                                                ProcessId VARCHAR(300) PRIMARY KEY,
	                                                ProcessStartTime DATETIME,
	                                                MaxDurationInMinutes INT,
	                                                UserId VARCHAR(100)
                                                )");

                DataTable dt = _sqlRepository.GetDataTable("select * from ProcessLock where ProcessId='" + _processid + "'");
                if (dt.Rows.Count > 0)
                {
                    //already running the process
                    System.DateTime ProcessStartTime = Convert.ToDateTime(dt.Rows[0]["ProcessStartTime"].ToString());
                    int MaxDurationInMinutes = (int)OTSBD.clsStaticInfo.dbl(dt.Rows[0]["MaxDurationInMinutes"].ToString());
                    DateTime ProcessEndTime = ProcessStartTime.AddMinutes(MaxDurationInMinutes);

                    if (ProcessEndTime < System.DateTime.Now)
                    {
                        //delete the lock and insert the parameters for lock request to server
                        _sqlRepository.ExecuteSqlCommand(@"DELETE FROM ProcessLock WHERE ProcessId='" + _processid + @"'
                                                        INSERT INTO ProcessLock(ProcessId,ProcessStartTime,MaxDurationInMinutes,UserId)
                                                        VALUES('" + _processid + "','" + System.DateTime.Now.ToString() + "'," + _maxduration + ",'" + _userid + @"')");
                    }
                    else
                    {
                        //running process found, throw exception
                        throw new Exception(string.Format("System is running the same process from [{0}] requested by [{1}]. Max. completion time [{2}]"
                            , ProcessStartTime.ToString("dd-MMM-yyyy hh:mm:ss tt"), dt.Rows[0]["UserId"].ToString(), ProcessEndTime.ToString("dd-MMM-yyyy hh:mm:ss tt")));
                    }

                }
                else
                {
                    //not running, insert the parameters and lock request to server
                    _sqlRepository.ExecuteSqlCommand(@"INSERT INTO ProcessLock(ProcessId,ProcessStartTime,MaxDurationInMinutes,UserId)
                                                        VALUES('" + _processid + "','" + System.DateTime.Now.ToString() + "'," + _maxduration + ",'" + _userid + @"')");

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void UnlockProcess()
        {
            try
            {
                //delete the lock 
                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM ProcessLock WHERE ProcessId='" + _processid + @"'");
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
