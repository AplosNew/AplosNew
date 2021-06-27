using ConnectionManager;
using HtmlAgilityPack;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Employees;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace Library.Service.Helpers
{
    public enum ProcessFlag
    {
        AttendanceLock,
        SalaryProcess,
        AttendanceProcess,
        DailyAllowanceCalculation,
        DailyAllowanceTransaction,
        Type1PlanSimulator,
        Type1PlanSnapshot1,
        Type1PlanSnapshot2,
        ToDoScheduler,
        TNAScheduler

    }
    public static class ProcessUtility
    {
        public static double TimeoutMinutes = 30;

        public static bool ProcessLocked(ProcessFlag processFlag)
        {
            try
            {
                clsConnectionManager con = new clsConnectionManager();
                
                con.BeginTransaction();
                con.getDataSet("select * from PROCESSLOCK WHERE LOCKTYPE='" + processFlag.ToString() + "'", out DataSet dsProcessLock);
                con.CommitTransaction();

                DateTime dtNow = DateTime.Now;

                if (dsProcessLock.Tables[0].Rows.Count > 0)
                {
                    DateTime dtDBLockTime = Convert.ToDateTime(dsProcessLock.Tables[0].Rows[0]["LockTime"].ToString());
                    TimeSpan ts = dtNow.Subtract(dtDBLockTime);
                    if (Math.Abs(ts.TotalMinutes) < TimeoutMinutes)
                    {
                        return true;
                    }
                    else
                    {
                        ProcessUnlock(processFlag);
                    }
                }


                con.BeginTransaction();
                con.executeQuery(@"INSERT INTO PROCESSLOCK(LOCKTYPE,LockTime,LockedBy)
                                SELECT '" + processFlag.ToString() + "'," + dtNow + ",'System'");
                con.CommitTransaction();

                return false;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public static void ProcessUnlock(ProcessFlag processFlag)
        {
            try
            {
                clsConnectionManager con = new clsConnectionManager();

                con.BeginTransaction();
                con.executeQuery(@"DELETE from PROCESSLOCK where LOCKTYPE='" + processFlag.ToString() + "'");
                con.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}