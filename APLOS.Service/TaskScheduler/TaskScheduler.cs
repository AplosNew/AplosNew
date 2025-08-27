#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.TaskScheduler;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Microsoft.AspNet.SignalR.Client;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace Library.Service.TaskScheduler
{

    //this bullshit has been written by tarek talukder
    //if you are wasting time on reading this library
    //i would suggest you to take a deep breathe and go somewhere in north/south pole to cool-down your brain
    //please don't blame me if you are failing to understand this crap correctly
    //I would suggest you to understand project management(theory) and science of task dependency
    //for better understanding, please never call me
    public class TaskScheduler
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public TaskScheduler(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        private DataSet getDataset(string sql)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

            return dsMaster;
        }
        #endregion Constructor
        public void ProcessAllPendingSchedule()
        {
            string sql = @"SELECT tm.* FROM TaskManagerMaster AS tm
                            INNER JOIN TaskSchedulerMaster AS A ON tm.TaskSchedulerMasterId=a.Id

                            WHERE isnull(tm.IsExpiredSchedule,0)<>1 
                            AND convert(date,A.StartDate)<=convert(date,'" + DateTime.Now.ToString("dd-MMM-yyyy") + @"') 
                                    AND tm.Id NOT IN (SELECT tm.Id
                                                        FROM TaskManagerMaster AS tm
                                    INNER JOIN TaskSchedulerMaster AS A ON tm.TaskSchedulerMasterId=a.Id
                                                      WHERE A.OnPreviousAccomplishment=1 AND tm.CurrentStatus<>'Closed' 
                                    UNION
                                    SELECT tm.Id
                                                        FROM TaskManagerMaster AS tm
								        INNER JOIN TaskManagerMaster AS tm2 ON tm2.ParentTaskManagerMasterId=tm.Id
                                    INNER JOIN TaskSchedulerMaster AS A ON tm.TaskSchedulerMasterId=a.Id
                                              WHERE isnull(A.OnPreviousAccomplishment,0)=1 AND tm2.CurrentStatus<>'Closed'       
                            )";
            DataTable dtTasks = _sqlRepository.GetDataTable(sql);

            for (int i = 0; i < dtTasks.Rows.Count; i++)
            {
                GetScheduler(dtTasks.Rows[i]["Id"].ToString());
            }

        }
        public void GetScheduler(string TaskManagerMasterId)
        {
            //await Task.Factory.StartNew(() =>
            // {
            DataSet dtTasks = getDataset("SELECT * FROM TaskManagerMaster AS tmm WHERE Id='" + TaskManagerMasterId + "'");

            DataSet dtSchedule = getDataset("SELECT * FROM TaskSchedulerMaster AS tsm WHERE Id=(SELECT TaskSchedulerMasterId FROM TaskManagerMaster WHERE Id='" + TaskManagerMasterId + "')");

            string LastExecutionDate = dtTasks.Tables[0].Rows[0]["LastExecutionDate"].ToString();
            if (string.IsNullOrEmpty(LastExecutionDate))
            {

                LastExecutionDate = dtSchedule.Tables[0].Rows[0]["StartDate"].ToString();
                if (Convert.ToDateTime(LastExecutionDate) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                    LastExecutionDate = DateTime.Now.ToString("dd-MMM-yyyy");

                dtTasks.Tables[0].Rows[0]["NextExecutionDate"] = LastExecutionDate;

            }


            //check whether the service expired or not: service end date,no of occurences etc

            NextOnDaily(dtSchedule, dtTasks, LastExecutionDate);


            //});
        }
        private string NextOnDaily(DataSet dtSchedule, DataSet dtTask, string LastExecutionDate)
        {



            //scheduler will copy the task accordingly and update LastExecutionDate,Next Execution date,NoOfOccurences
            string _LastExecutionDate = DateTime.Now.ToString("dd-MMM-yyyy");


            //calculate next execution date block
            //here code will be changed
            string _NextExecutionDate = "";
            int days = clsStaticInfo.dateDiff(LastExecutionDate, DateTime.Now.ToString("dd-MMM-yyyy"));
            int RepeatEvery = (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["EveryInterval"].ToString());
            int _NoOfOccurence = (int)clsStaticInfo.dbl(dtTask.Tables[0].Rows[0]["NoOfOccurences"].ToString());
            if (dtSchedule.Tables[0].Rows[0]["RepeatType"].ToString().ToUpper() == "DAILY")
            {

                _NextExecutionDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddDays(RepeatEvery).ToString("dd-MMM-yyyy");
                _NoOfOccurence += 1;

            }
            else if (dtSchedule.Tables[0].Rows[0]["RepeatType"].ToString().ToUpper() == "WEEKLY")
            {
                _NextExecutionDate = NextScheduleDateWeekly(dtTask, dtSchedule, ref _NoOfOccurence);
            }
            else if (dtSchedule.Tables[0].Rows[0]["RepeatType"].ToString().ToUpper() == "MONTHLY")
            {
                _NextExecutionDate = NextScheduleDateMonthly(dtTask, dtSchedule);
                _NoOfOccurence += 1;
            }
            else if (dtSchedule.Tables[0].Rows[0]["RepeatType"].ToString().ToUpper() == "YEARLY")
            {
                _NextExecutionDate = NextScheduleDateYearly(dtTask, dtSchedule);
                _NoOfOccurence += 1;
            }
            else if (dtSchedule.Tables[0].Rows[0]["RepeatType"].ToString().ToUpper() == "EVERY")
            {
                _NextExecutionDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddDays(RepeatEvery * 7).ToString("dd-MMM-yyyy");
                _NoOfOccurence += 1;
            }



            //daily validations and checkings
            string currenttExecutionDate = dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString();

            //stop the service if:::
            #region stop the service




            #endregion stop the service

            if (Convert.ToDateTime(Convert.ToDateTime(currenttExecutionDate).ToString("dd-MMM-yyyy")) <= Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
            {
                dtTask.Tables[0].Rows[0].BeginEdit();
                dtTask.Tables[0].Rows[0]["LastExecutionDate"] = _LastExecutionDate;
                dtTask.Tables[0].Rows[0]["NextExecutionDate"] = _NextExecutionDate;
                dtTask.Tables[0].Rows[0]["NoOfOccurences"] = _NoOfOccurence;
                if (bplib.clsWebLib.GetBoolData(dtSchedule.Tables[0].Rows[0]["IsOn"].ToString()) == true)
                {
                    if (Convert.ToDateTime(_NextExecutionDate) > Convert.ToDateTime(dtSchedule.Tables[0].Rows[0]["EndDate"].ToString()))
                    {
                        dtTask.Tables[0].Rows[0]["IsExpiredSchedule"] = true;
                    }
                }
                else if (bplib.clsWebLib.GetBoolData(dtSchedule.Tables[0].Rows[0]["isAfter"].ToString()) == true)
                {
                    if (_NoOfOccurence > clsStaticInfo.dbl(dtTask.Tables[0].Rows[0]["NoOfOccurences"].ToString()))
                    {
                        dtTask.Tables[0].Rows[0]["IsExpiredSchedule"] = true;
                    }
                }

                dtTask.Tables[0].Rows[0].EndEdit();



                string TaskDueDate = _NextExecutionDate;
                //create task here

                CopyTask(TaskDueDate, dtTask);
            }




            return "";
        }


        private void CopyTask(string DueDate, DataSet dsTaskMain)
        {
            string TaskMasterId = dsTaskMain.Tables[0].Rows[0]["Id"].ToString();

            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsMasterSource, dsMasterDestination;
            DataSet dsAuditSource, dsAuditDestination;
            DataSet dsSubTasksSource, dsSubTasksDestination;
            DataSet dsAttachmentsSource, dsAttachmentsDestination;

            string Id = "";
            genid.GenID("TODO", out Id);
            Id = "SHD" + Id;


            #region Master

            GetDataSet("Select * from TaskManagerMaster where id='" + TaskMasterId + "'", out dsMasterSource);
            GetDataSet("Select * from TaskManagerMaster where 1=2", out dsMasterDestination);

            DataRow drMaster = dsMasterDestination.Tables[0].NewRow();

            CopyRow(dsMasterSource.Tables[0].Rows[0], ref drMaster);
            drMaster["Id"] = Id;
            drMaster["ParentTaskManagerMasterId"] = TaskMasterId;
            drMaster["TaskSchedulerMasterId"] = DBNull.Value;
            drMaster["AddedDate"] = System.DateTime.Now.ToString();
            drMaster["UpdatedDate"] = System.DateTime.Now.ToString();

            drMaster["CurrentStatus"] = "ToStart";
            drMaster["LastExecutionDate"] = DBNull.Value;
            drMaster["NextExecutionDate"] = DBNull.Value;
            drMaster["NoOfOccurences"] = DBNull.Value;
            drMaster["IsExpiredSchedule"] = DBNull.Value;
            dsMasterDestination.Tables[0].Rows.Add(drMaster);

            #endregion Master


            #region audit
            GetDataSet("Select * from TaskAudit where TaskManagerMasterId='" + TaskMasterId + "'", out dsAuditSource);
            GetDataSet("Select * from TaskAudit where 1=2", out dsAuditDestination);

            for (int i = 0; i < dsAuditSource.Tables[0].Rows.Count; i++)
            {
                DataRow drLocal = dsAuditDestination.Tables[0].NewRow();

                CopyRow(dsAuditSource.Tables[0].Rows[i], ref drLocal);
                drLocal["Id"] = Id + "-" + (i + 1).ToString();
                drLocal["TaskManagerMasterId"] = Id;
                drLocal["AddedDate"] = System.DateTime.Now.ToString();
                drLocal["UpdatedDate"] = System.DateTime.Now.ToString();
                drLocal["DueDate"] = DueDate;

                drLocal["isRead"] = false;
                drLocal["isDone"] = false;
                drLocal["RevisedCommitmentDate"] = DBNull.Value;
                drLocal["CommitmentDate"] = DBNull.Value;

                dsAuditDestination.Tables[0].Rows.Add(drLocal);

            }
            #endregion audit


            #region TaskManagerSubTasks
            GetDataSet("Select * from TaskManagerSubTasks where TaskManagerMasterId='" + TaskMasterId + "'", out dsSubTasksSource);
            GetDataSet("Select * from TaskManagerSubTasks where 1=2", out dsSubTasksDestination);

            for (int i = 0; i < dsSubTasksSource.Tables[0].Rows.Count; i++)
            {
                DataRow drLocal = dsSubTasksDestination.Tables[0].NewRow();

                CopyRow(dsSubTasksSource.Tables[0].Rows[i], ref drLocal);
                drLocal["Id"] = Id + "-" + (i + 1).ToString();
                drLocal["TaskManagerMasterId"] = Id;
                drLocal["AddedDate"] = System.DateTime.Now.ToString();
                drLocal["UpdatedDate"] = System.DateTime.Now.ToString();
                drLocal["isDone"] = false;
                dsSubTasksDestination.Tables[0].Rows.Add(drLocal);

            }
            #endregion audit


            #region TaskAttachments
            GetDataSet("Select * from TaskAttachments where TaskManagerMasterId='" + TaskMasterId + "'", out dsAttachmentsSource);
            GetDataSet("Select * from TaskAttachments where 1=2", out dsAttachmentsDestination);

            for (int i = 0; i < dsAttachmentsSource.Tables[0].Rows.Count; i++)
            {
                DataRow drLocal = dsAttachmentsDestination.Tables[0].NewRow();

                CopyRow(dsAttachmentsSource.Tables[0].Rows[i], ref drLocal);
                drLocal["Id"] = Id + "-" + (i + 1).ToString();
                drLocal["TaskManagerMasterId"] = Id;
                drLocal["AddedDate"] = System.DateTime.Now.ToString();
                drLocal["UpdatedDate"] = System.DateTime.Now.ToString();

                dsAttachmentsDestination.Tables[0].Rows.Add(drLocal);

            }
            #endregion audit


            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsTaskMain, dsMasterDestination, dsAuditDestination, dsSubTasksDestination, dsAttachmentsDestination);
        }
        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];
                }
                catch (Exception)
                {


                }

            }

        }

        private void GetDataSet(string sql, out DataSet ds)
        {
            ds = new DataSet();
            try
            {
                ConnectionManager.clsConnection _con = new ConnectionManager.clsConnection();
                _con.BeginTransaction();
                _con.getDataSet(sql, out ds);
                _con.CommitTransaction();
                //ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter(sql, out ds, false, "1");
            }
            catch (Exception ex)
            {

            }
        }

        private string NextScheduleDateWeekly(DataSet dtTask, DataSet dtSchedule, ref int Count)
        {
            int RepeatEvery = (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["EveryInterval"].ToString());

            string _NextExecutionDate = "";
            DateTime dtNextExecDateWeekly = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString());
            if (dtSchedule.Tables[0].Rows[0]["WeeklyRepeatationBycommaSepDayName"].ToString() == "")
            {
                _NextExecutionDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddDays(RepeatEvery * 7).ToString("dd-MMM-yyyy");

                Count++;
            }
            else
            {

                string WeeklyRepeatationBycommaSepDayName = dtSchedule.Tables[0].Rows[0]["WeeklyRepeatationBycommaSepDayName"].ToString();// "Sat,Mon,Wed";
                //string _NextExecutionDate = "";
                //DateTime dtNextExecDateWeekly = Convert.ToDateTime("13-Nov-2019");
                DateTime Now = DateTime.Now;

                string[] weekdays = WeeklyRepeatationBycommaSepDayName.Split(',');
                DateTime dtFirstDateOfThisWeek = Convert.ToDateTime(Now.AddDays((int)dtNextExecDateWeekly.DayOfWeek * -1).ToString("dd-MMM-yyyy"));

                List<DateTime> CurrentWeekDates = new List<DateTime>();

                for (int i = 0; i < 7; i++)
                {

                    if (WeeklyRepeatationBycommaSepDayName.ToUpper().Contains(dtFirstDateOfThisWeek.ToString("ddd").ToUpper()))
                        CurrentWeekDates.Add(dtFirstDateOfThisWeek);

                    dtFirstDateOfThisWeek = dtFirstDateOfThisWeek.AddDays(1);
                }

                List<DateTime> dtTemp = CurrentWeekDates.Where(ee => ee > Convert.ToDateTime(Now.ToString("dd-MMM-yyyy"))).ToList();
                if (dtTemp.Count == 0)
                {
                    //reset the first day of week
                    dtFirstDateOfThisWeek = Convert.ToDateTime(Now.AddDays((int)dtNextExecDateWeekly.DayOfWeek * -1).ToString("dd-MMM-yyyy"));
                    _NextExecutionDate = Convert.ToDateTime(dtFirstDateOfThisWeek.ToString("dd-MMM-yyyy")).AddDays(RepeatEvery * 7).ToString("dd-MMM-yyyy");
                    DateTime dtFirstDateOfThatWeek = Convert.ToDateTime(Convert.ToDateTime(_NextExecutionDate).AddDays((int)Convert.ToDateTime(_NextExecutionDate).DayOfWeek * -1).ToString("dd-MMM-yyyy"));
                    for (int i = 0; i < 7; i++)
                    {

                        if (WeeklyRepeatationBycommaSepDayName.ToUpper().Contains(dtFirstDateOfThatWeek.ToString("ddd").ToUpper()))
                        {
                            _NextExecutionDate = dtFirstDateOfThatWeek.ToString("dd-MMM-yyyy");
                            Count++;
                            break;
                        }
                        dtFirstDateOfThatWeek = dtFirstDateOfThatWeek.AddDays(1);
                    }
                }
                else
                {
                    _NextExecutionDate = dtTemp.Min().ToString("dd-MMM-yyyy");

                }




            }


            return _NextExecutionDate;

        }
        private string NextScheduleDateMonthly(DataSet dtTask, DataSet dtSchedule)
        {
            int RepeatEvery = (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["EveryInterval"].ToString());


            string _NextExecutionDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).ToString("dd-MMM-yyyy");
            DateTime dtNextExecDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString());
            if (bplib.clsWebLib.GetBoolData(dtSchedule.Tables[0].Rows[0]["isRepeatByDay"].ToString()) == false
                && bplib.clsWebLib.GetBoolData(dtSchedule.Tables[0].Rows[0]["isRepeatByTheNthWeekForMonthly"].ToString()) == false)
            {

                _NextExecutionDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddMonths(RepeatEvery).ToString("dd-MMM-yyyy");
            }
            else
            {
                if (bplib.clsWebLib.GetBoolData(dtSchedule.Tables[0].Rows[0]["isRepeatByDay"].ToString()) == true)
                {
                    int RepeatByDayNumber = (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["RepeatByDayNumber"].ToString());
                    DateTime dtLocalDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddMonths(RepeatEvery);

                    if (DateTime.DaysInMonth(dtLocalDate.Year, dtLocalDate.Month) < RepeatByDayNumber)
                        RepeatByDayNumber = DateTime.DaysInMonth(dtLocalDate.Year, dtLocalDate.Month);


                    _NextExecutionDate = new DateTime(dtLocalDate.Year, dtLocalDate.Month, RepeatByDayNumber).ToString("dd-MMM-yyyy");
                    return _NextExecutionDate;
                }
                else if (bplib.clsWebLib.GetBoolData(dtSchedule.Tables[0].Rows[0]["isRepeatByTheNthWeekForMonthly"].ToString()) == true)
                {
                    int RepeatByDayNumber = (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["RepeatByDayNumber"].ToString());
                    int weekNo = 1;

                    if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "FIRST")
                        weekNo = 0;
                    else if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "SECOND")
                        weekNo = 1;
                    else if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "THIRD")
                        weekNo = 2;
                    else if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "FOURTH")
                        weekNo = 3;
                    else
                        weekNo = 5;

                    if (weekNo <= 4)
                    {
                        DateTime dtLocalDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddMonths(RepeatEvery);
                        dtLocalDate = new DateTime(dtLocalDate.Year, dtLocalDate.Month, 1);
                        dtLocalDate = dtLocalDate.AddDays(weekNo * 7);
                        dtLocalDate = dtLocalDate.AddDays((int)dtLocalDate.DayOfWeek * -1);
                        for (int i = 0; i < 7; i++)
                        {
                            if (dtLocalDate.ToString("dddd").ToUpper() == dtSchedule.Tables[0].Rows[0]["RepeatByWeek"].ToString().ToUpper())
                            {
                                _NextExecutionDate = dtLocalDate.ToString("dd-MMM-yyyy");
                                return _NextExecutionDate;
                            }
                            dtLocalDate = dtLocalDate.AddDays(1);

                        }

                    }
                    else
                    {
                        DateTime dtLocalDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddMonths(RepeatEvery);
                        dtLocalDate = new DateTime(dtLocalDate.Year, dtLocalDate.Month, DateTime.DaysInMonth(dtLocalDate.Year, dtLocalDate.Month));
                        for (int i = 0; i < 7; i++)
                        {
                            if (dtLocalDate.ToString("dddd").ToUpper() == dtSchedule.Tables[0].Rows[0]["RepeatByWeek"].ToString().ToUpper())
                            {
                                _NextExecutionDate = dtLocalDate.ToString("dd-MMM-yyyy");
                                return _NextExecutionDate;
                            }
                            dtLocalDate = dtLocalDate.AddDays(-1);

                        }


                    }


                }





            }


            return _NextExecutionDate;

        }
        private string NextScheduleDateYearly(DataSet dtTask, DataSet dtSchedule)
        {
            List<string> Months = new List<string>();
            foreach (MonthEnum item in Enum.GetValues(typeof(MonthEnum)))
                Months.Add(item.ToString());

            int RepeatEvery = (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["EveryInterval"].ToString());


            string _NextExecutionDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).ToString("dd-MMM-yyyy");
            DateTime dtNextExecDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString());
            if (bplib.clsWebLib.GetBoolData(dtSchedule.Tables[0].Rows[0]["isRepeatByTheMonth"].ToString()))
            {

                DateTime dtTemp = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddYears(RepeatEvery);
                dtTemp = new DateTime(dtTemp.Year,
                    Months.IndexOf(dtSchedule.Tables[0].Rows[0]["RepeatByMonth"].ToString()) + 1,
                    (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["RepeatByDayNumber"].ToString()));



                return _NextExecutionDate = dtTemp.ToString("dd-MMM-yyyy");
            }
            else
            {
                //isRepeatByTheNthWeek


                int RepeatByDayNumber = (int)clsStaticInfo.dbl(dtSchedule.Tables[0].Rows[0]["RepeatByDayNumber"].ToString());
                int weekNo = 1;

                if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "FIRST")
                    weekNo = 0;
                else if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "SECOND")
                    weekNo = 1;
                else if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "THIRD")
                    weekNo = 2;
                else if (dtSchedule.Tables[0].Rows[0]["RepeatbyNthWeek"].ToString().ToUpper() == "FOURTH")
                    weekNo = 3;
                else
                    weekNo = 5;

                if (weekNo <= 4)
                {

                    DateTime dtLocalDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddYears(RepeatEvery);
                    dtLocalDate = new DateTime(dtLocalDate.Year, Months.IndexOf(dtSchedule.Tables[0].Rows[0]["RepeatbyOfEarly"].ToString()) + 1, 1);
                    dtLocalDate = dtLocalDate.AddDays(weekNo * 7);
                    dtLocalDate = dtLocalDate.AddDays((int)dtLocalDate.DayOfWeek * -1);
                    for (int i = 0; i < 7; i++)
                    {
                        if (dtLocalDate.ToString("dddd").ToUpper() == dtSchedule.Tables[0].Rows[0]["RepeatByWeek"].ToString().ToUpper())
                        {
                            _NextExecutionDate = dtLocalDate.ToString("dd-MMM-yyyy");
                            return _NextExecutionDate;
                        }
                        dtLocalDate = dtLocalDate.AddDays(1);

                    }

                }
                else
                {
                    DateTime dtLocalDate = Convert.ToDateTime(dtTask.Tables[0].Rows[0]["NextExecutionDate"].ToString()).AddYears(RepeatEvery);
                    dtLocalDate = new DateTime(dtLocalDate.Year, Months.IndexOf(dtSchedule.Tables[0].Rows[0]["RepeatbyOfEarly"].ToString()) + 1, DateTime.DaysInMonth(dtLocalDate.Year, Months.IndexOf(dtSchedule.Tables[0].Rows[0]["RepeatbyOfEarly"].ToString()) + 1));
                    for (int i = 0; i < 7; i++)
                    {
                        if (dtLocalDate.ToString("dddd").ToUpper() == dtSchedule.Tables[0].Rows[0]["RepeatByWeek"].ToString().ToUpper())
                        {
                            _NextExecutionDate = dtLocalDate.ToString("dd-MMM-yyyy");
                            return _NextExecutionDate;
                        }
                        dtLocalDate = dtLocalDate.AddDays(-1);

                    }


                }








            }


            return _NextExecutionDate;

        }


        public class Data
        {
            public static int Count { get; set; }
            public int TaskId { get; set; } = 0;
            public string TaskName { get; set; } = "";
            public string StartDate { get; set; } = "";
            public string EndDate { get; set; } = "";
            public int Duration { get; set; } = 0;
            public string Predecessor { get; set; } = "";
            public string Criteria { get; set; } = "";

        }


        bool PushDates = true;

        public DataTable GetDataSourceMasterOrderNew(string TransactionId, TaskAppliedOnEnum ScheduleFor)
        {
            string MasterOrderId = "";

            DataTable dtDependentDates = null;
            if (ScheduleFor == TaskAppliedOnEnum.MasterOrder)
            {
                MasterOrderId = TransactionId;
                dtDependentDates = getDependentDatesMasterOrderNew(TransactionId);
            }
            else if (ScheduleFor == TaskAppliedOnEnum.Style)
                dtDependentDates = getDependentDatesStyleNew(TransactionId, out MasterOrderId);
            else if (ScheduleFor == TaskAppliedOnEnum.SalesOrder)
                dtDependentDates = getDependentDatesSalesOrderNew(TransactionId, out MasterOrderId);
            else if (ScheduleFor == TaskAppliedOnEnum.ProductionOrder)
                dtDependentDates = getDependentDatesProductionOrderNew(TransactionId, out MasterOrderId);


            Library.Service.Extension.TaskScheduler.TaskScheduler scheduler = new Extension.TaskScheduler.TaskScheduler();
            scheduler.GetDataSourceMasterOrderNew(MasterOrderId, out DataTable dtData, out DataTable dtRelations, out DataTable dtTaskDelayedEndDate, out DataTable dtCalendar);


            DataTable dtOriginalData = dtData.DefaultView.ToTable();
            DataTable dtOriginalRelation = dtRelations.DefaultView.ToTable();

            PushDates = true;
            generateDatesNew(MasterOrderId, dtRelations, dtCalendar, dtData, dtDependentDates, dtTaskDelayedEndDate);

            PushDates = false;
            generateDatesNew(MasterOrderId, dtOriginalRelation, dtCalendar, dtOriginalData, dtDependentDates, dtTaskDelayedEndDate);

            for (int i = 0; i < dtOriginalData.Rows.Count; i++)
            {
                dtData.DefaultView.RowFilter = "Id='" + dtOriginalData.Rows[i]["Id"].ToString() + "'";
                if (dtData.DefaultView.Count > 0)
                {
                    dtData.DefaultView[0]["OriginalSequentialStartDate"] = dtOriginalData.Rows[i]["TempStartDate"].ToString();
                    dtData.DefaultView[0]["OriginalSequentialEndDate"] = dtOriginalData.Rows[i]["TempEndDate"].ToString();
                }
            }
            if (dtData.Rows.Count > 0)
            {

            }
            return dtData;
        }
        public void generateDatesNew(string MasterOrderId, DataTable dtTemplateData, DataTable dtCalendar, DataTable dtTaskData, DataTable dtDependentDates, DataTable dtTaskDelayedEndDate)
        {

            //DataTable dtDependentDates = getDependentDatesNew(MasterOrderId);


            //plot dependent dates
            dtTemplateData.DefaultView.RowFilter = "isnull(TempStartDate,'')=''";
            while (dtTemplateData.DefaultView.Count > 0)
            {

                string pre = dtTemplateData.DefaultView[0]["PreTaskTemplateId"].ToString();
                string cur = dtTemplateData.DefaultView[0]["TaskTemplateId"].ToString();
                DataRow drPresceding = getPrecedingDateNew(pre, cur, dtTemplateData);


                if (drPresceding["Criteria"].ToString() == "" && drPresceding["TempStartDate"].ToString() == "")
                {
                    int duration = (int)clsStaticInfo.dbl(drPresceding["duration"].ToString());
                    int LagDays = (int)clsStaticInfo.dbl(drPresceding["OwnLagDays"].ToString());

                    pre = drPresceding["PreTaskTemplateId"].ToString();
                    cur = drPresceding["TaskTemplateId"].ToString();

                    dtDependentDates.DefaultView.RowFilter = "DependentDatesEnum='" + drPresceding["DependentDatesEnum"].ToString() + "'";
                    CalculatGeneralDateNew(dtDependentDates.DefaultView[0].Row, duration, LagDays, drPresceding, dtCalendar, dtTaskData, dtTaskDelayedEndDate);
                }
                else
                {
                    dtTemplateData.DefaultView.RowFilter = "isnull(TempStartDate,'')='' AND PreTaskTemplateId='" + drPresceding["TaskTemplateId"].ToString() + "'";
                    if (dtTemplateData.DefaultView.Count > 0)
                    {
                        pre = dtTemplateData.DefaultView[0]["PreTaskTemplateId"].ToString();
                        cur = dtTemplateData.DefaultView[0]["TaskTemplateId"].ToString();

                    }
                }
                getSubsceedingDateNew(drPresceding["TaskTemplateId"].ToString(), cur, dtCalendar, dtTemplateData, dtDependentDates, dtTaskData, dtTaskDelayedEndDate);
                dtTemplateData.DefaultView.RowFilter = "isnull(TempStartDate,'')=''";
                continue;

            }


        }

        private DataRow getPrecedingDateNew(string Pre, string Cur, DataTable dtTemplateData)
        {
            dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Pre + "'";
            if (dtTemplateData.DefaultView.Count > 0)
            {
                if (dtTemplateData.DefaultView[0]["TempStartDate"].ToString() != "")
                {
                    return dtTemplateData.DefaultView[0].Row;
                }
                else
                {
                    if (dtTemplateData.DefaultView[0]["PreTaskTemplateId"].ToString() == "")
                    {
                        return dtTemplateData.DefaultView[0].Row;
                    }
                    else
                    {
                        //return dtTemplateData.DefaultView[0].Row;
                        string pre = dtTemplateData.DefaultView[0]["PreTaskTemplateId"].ToString();
                        string cur = dtTemplateData.DefaultView[0]["TaskTemplateId"].ToString();

                        DataRow dr = getPrecedingDateNew(pre, cur, dtTemplateData);
                        if (dr == null)
                            getPrecedingDateNew(pre, cur, dtTemplateData);
                        else
                            return dr;
                    }
                }

            }
            else
            {
                dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Cur + "'";
                return dtTemplateData.DefaultView[0].Row;
            }
            return null;
        }
        private void getSubsceedingDateNew(string Pre, string Cur, DataTable dtCalendar, DataTable dtTemplateData, DataTable dtDependentDates, DataTable dtTaskData, DataTable dtTaskDelayedEndDate)
        {
            try
            {


                dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Pre + "'";
                DataRow drParent = dtTemplateData.DefaultView[0].Row;
                if (drParent["TempStartDate"].ToString() == "")
                {

                    Pre = drParent["PreTaskTemplateId"].ToString();
                    Cur = drParent["TaskTemplateId"].ToString();
                    DataRow drPresceding = getPrecedingDateNew(Pre, Cur, dtTemplateData);
                    if (drPresceding["Criteria"].ToString() == "" && drPresceding["TempStartDate"].ToString() == "")
                    {
                        int duration = (int)clsStaticInfo.dbl(drPresceding["duration"].ToString());
                        int LagDays = (int)clsStaticInfo.dbl(drPresceding["OwnLagDays"].ToString());

                        Pre = drPresceding["PreTaskTemplateId"].ToString();
                        Cur = drPresceding["TaskTemplateId"].ToString();

                        dtDependentDates.DefaultView.RowFilter = "DependentDatesEnum='" + drPresceding["DependentDatesEnum"].ToString() + "'";
                        CalculatGeneralDateNew(dtDependentDates.DefaultView[0].Row, duration, LagDays, drPresceding, dtCalendar, dtTaskData, dtTaskDelayedEndDate);
                    }
                    else
                    {
                        dtTemplateData.DefaultView.RowFilter = "isnull(TempStartDate,'')='' AND PreTaskTemplateId='" + drPresceding["TaskTemplateId"].ToString() + "'";
                        if (dtTemplateData.DefaultView.Count > 0)
                        {
                            Pre = dtTemplateData.DefaultView[0]["PreTaskTemplateId"].ToString();
                            Cur = dtTemplateData.DefaultView[0]["TaskTemplateId"].ToString();

                        }
                    }
                    getSubsceedingDateNew(drPresceding["TaskTemplateId"].ToString(), Cur, dtCalendar, dtTemplateData, dtDependentDates, dtTaskData, dtTaskDelayedEndDate);

                    return;
                }
                if (Cur == "202010119")
                {

                }


                dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Cur + "' AND isnull(TempStartDate,'')=''";
                for (int i = 0; i < dtTemplateData.DefaultView.Count; i++)
                {
                    DataRow drCurrentDateRowToBeEdited = dtTemplateData.DefaultView[i].Row;

                    if (dtTemplateData.DefaultView[i]["TempStartDate"].ToString() == "")
                    {

                        //add date here

                        int Duration = (int)clsStaticInfo.dbl(drCurrentDateRowToBeEdited["Duration"].ToString());
                        int LagDays = (int)clsStaticInfo.dbl(drCurrentDateRowToBeEdited["LagDays"].ToString());
                        int OwnLagDays = (int)clsStaticInfo.dbl(drCurrentDateRowToBeEdited["OwnLagDays"].ToString());
                        DateTime PrecedingStartDate = Convert.ToDateTime(drParent["TempStartDate"].ToString());
                        DateTime PrecedingEndDate = Convert.ToDateTime(drParent["TempEndDate"].ToString());



                        //DateTime TempActualStartDate = Convert.ToDateTime(drParent["ActualStartDate"].ToString());
                        string SequentialStartDate = ""; string SequentialEndDate = "";
                        string ActualStartDate = GetDependentDate(dtTemplateData.DefaultView[i]["DependentDatesEnum"].ToString(), dtDependentDates); string ActualEndDate = "";
                        string DependentDate = ActualStartDate;
                        //ActualStartDate = Convert.ToDateTime(ActualStartDate).AddDays(OwnLagDays).ToString("dd-MMM-yyyy");
                        //string ActualStartDate = PrecedingStartDate.ToString("dd-MMM-yyyy"); string ActualEndDate = "";


                        #region calculations
                        if (drCurrentDateRowToBeEdited["Criteria"].ToString() == "FS")
                        {
                            if (drCurrentDateRowToBeEdited["TaskTemplateId"].ToString() == "2020898")
                            {


                            }

                            //sequential date
                            if (clsStaticInfo.dbl(drParent["Duration"].ToString()) == 0)
                            {
                                if (LagDays >= 0)
                                {
                                    CalendarRowFilter("WorkingDate>=#" + PrecedingEndDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                                }
                                else
                                {
                                    CalendarRowFilter("WorkingDate<=#" + PrecedingEndDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                                    dtCalendar.DefaultView.Sort = "WorkingDate DESC";

                                    CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(LagDays + 1).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                                }
                            }
                            else
                            {
                                if (LagDays >= 0)
                                {
                                    CalendarRowFilter("WorkingDate>=#" + PrecedingEndDate.AddDays(LagDays + 1).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                                }
                                else
                                {
                                    CalendarRowFilter("WorkingDate<=#" + PrecedingEndDate.AddDays(LagDays + 1).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                                    dtCalendar.DefaultView.Sort = "WorkingDate DESC";

                                    CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(LagDays + 1).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                                }
                            }


                            dtCalendar.DefaultView.Sort = "WorkingDate ASC";
                            SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");

                            if (Duration > 0)
                                SequentialEndDate = Convert.ToDateTime(dtCalendar.DefaultView[Duration - 1]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
                            else
                                SequentialEndDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");
                        }
                        else if (drCurrentDateRowToBeEdited["Criteria"].ToString() == "SS")
                        {
                            if (LagDays >= 0)
                            {
                                CalendarRowFilter("WorkingDate>=#" + PrecedingStartDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                            }
                            else
                            {
                                CalendarRowFilter("WorkingDate<=#" + PrecedingStartDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                                dtCalendar.DefaultView.Sort = "WorkingDate DESC";

                                CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(0).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                            }
                            dtCalendar.DefaultView.Sort = "WorkingDate ASC";
                            SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");


                            if (Duration > 0)
                                SequentialEndDate = Convert.ToDateTime(dtCalendar.DefaultView[Duration - 1]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
                            else
                                SequentialEndDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");

                        }
                        else if (drCurrentDateRowToBeEdited["Criteria"].ToString() == "FF")
                        {

                            //CalendarRowFilter("WorkingDate<=#" + PrecedingEndDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                            //dtCalendar.DefaultView.Sort = "WorkingDate DESC";

                            //SequentialEndDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");

                            //if (Duration > 0)
                            //    SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[Duration - 1]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
                            //else
                            //    SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");

                            if (LagDays >= 0)
                            {
                                CalendarRowFilter("WorkingDate>=#" + PrecedingEndDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                            }
                            else
                            {
                                CalendarRowFilter("WorkingDate<=#" + PrecedingEndDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                                dtCalendar.DefaultView.Sort = "WorkingDate DESC";

                                CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(0).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                            }
                            dtCalendar.DefaultView.Sort = "WorkingDate ASC";
                            SequentialEndDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");

                            CalendarRowFilter("WorkingDate<=#" + SequentialEndDate + "#", drCurrentDateRowToBeEdited, dtCalendar);
                            dtCalendar.DefaultView.Sort = "WorkingDate DESC";

                            if (Duration > 0)
                                SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[Duration - 1]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
                            else
                                SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");

                        }
                        else if (drCurrentDateRowToBeEdited["Criteria"].ToString() == "SF")
                        {
                            if (clsStaticInfo.dbl(drParent["Duration"].ToString()) == 0)
                            {
                                CalendarRowFilter("WorkingDate<=#" + PrecedingStartDate.AddDays(LagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                            }
                            else
                            {
                                CalendarRowFilter("WorkingDate<=#" + PrecedingStartDate.AddDays(LagDays - 1).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                            }
                            dtCalendar.DefaultView.Sort = "WorkingDate DESC";
                            SequentialEndDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");


                            if (Duration > 0)
                                SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[Duration - 1]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
                            else
                                SequentialStartDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");

                        }

                        //actualDate
                        //if (OwnLagDays == 0)
                        //{
                        //    CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(ActualStartDate).AddDays(OwnLagDays + 0).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                        //}
                        //else 
                        if (OwnLagDays >= 0)
                        {
                            CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(ActualStartDate).AddDays(OwnLagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                        }
                        else
                        {
                            CalendarRowFilter("WorkingDate<=#" + Convert.ToDateTime(ActualStartDate).AddDays(OwnLagDays).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);
                            dtCalendar.DefaultView.Sort = "WorkingDate DESC";

                            CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(0).ToString("dd-MMM-yyyy") + "#", drCurrentDateRowToBeEdited, dtCalendar);

                        }
                        dtCalendar.DefaultView.Sort = "WorkingDate ASC";
                        ActualStartDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");

                        if (Duration > 0)
                            ActualEndDate = Convert.ToDateTime(dtCalendar.DefaultView[Duration - 1]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
                        else
                            ActualEndDate = Convert.ToDateTime(dtCalendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");



                        drCurrentDateRowToBeEdited["SequentialStartDate"] = SequentialStartDate;
                        drCurrentDateRowToBeEdited["SequentialEndDate"] = SequentialEndDate;
                        drCurrentDateRowToBeEdited["ActualStartDate"] = ActualStartDate;
                        drCurrentDateRowToBeEdited["ActualEndDate"] = ActualEndDate;

                        if (Convert.ToDateTime(SequentialStartDate) >= Convert.ToDateTime(ActualStartDate))
                        {
                            drCurrentDateRowToBeEdited["TempStartDate"] = SequentialStartDate;
                            drCurrentDateRowToBeEdited["TempEndDate"] = SequentialEndDate;
                        }
                        else
                        {
                            drCurrentDateRowToBeEdited["TempStartDate"] = ActualStartDate;
                            drCurrentDateRowToBeEdited["TempEndDate"] = ActualEndDate;
                        }

                        drCurrentDateRowToBeEdited["DependentDate"] = DependentDate;
                        #endregion calculations
                    }
                    dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Cur + "' AND isnull(TempStartDate,'')=''";
                    if (dtTemplateData.DefaultView.Count > 0)
                    {
                        getSubsceedingDateNew(dtTemplateData.DefaultView[0]["PreTaskTemplateId"].ToString(), dtTemplateData.DefaultView[0]["TaskTemplateId"].ToString(), dtCalendar, dtTemplateData, dtDependentDates, dtTaskData, dtTaskDelayedEndDate);
                        return;
                    }


                }

                dtTaskData.DefaultView.RowFilter = "TaskTemplateId='" + Cur + "'";
                dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Cur + "'";
                DateTime dtStartDate = Convert.ToDateTime(dtTemplateData.DefaultView[0]["TempStartDate"].ToString());
                DateTime dtEndDate = Convert.ToDateTime(dtTemplateData.DefaultView[0]["TempEndDate"].ToString());

                string Id = dtTemplateData.DefaultView[0]["Id"].ToString();


                for (int i = 0; i < dtTemplateData.DefaultView.Count; i++)
                {
                    Id = dtTemplateData.DefaultView[i]["Id"].ToString();
                    if (dtEndDate < Convert.ToDateTime(dtTemplateData.DefaultView[i]["TempEndDate"].ToString()))
                    {
                        //Id = dtTemplateData.DefaultView[i]["Id"].ToString();
                        dtStartDate = Convert.ToDateTime(dtTemplateData.DefaultView[i]["TempStartDate"].ToString());
                        dtEndDate = Convert.ToDateTime(dtTemplateData.DefaultView[i]["TempEndDate"].ToString());

                    }
                }
                dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Cur + "' AND Id<>'" + Id + "'";
                while (dtTemplateData.DefaultView.Count > 0)
                    dtTemplateData.DefaultView[0].Delete();


                dtTemplateData.DefaultView.RowFilter = "TaskTemplateId='" + Cur + "' AND Id='" + Id + "'";

                //HasPredecessorActualDate


                dtTaskData.DefaultView[0]["HasPredecessorActualDate"] = true;
                dtTaskData.DefaultView[0]["isPredecessorDelayed"] = true;
                if (dtTemplateData.DefaultView[0]["PreTaskTemplateId"].ToString() != "")
                {
                    if (bplib.clsWebLib.GetBoolData(drParent["HasActualDate"].ToString()) == false)
                        dtTaskData.DefaultView[0]["HasPredecessorActualDate"] = false;
                }

                dtTaskData.DefaultView[0]["TempStartDate"] = dtStartDate.ToString("dd-MMM-yyyy");
                dtTaskData.DefaultView[0]["TempEndDate"] = SetDelyedTaskEndDate(dtEndDate.ToString("dd-MMM-yyyy"), dtTemplateData.DefaultView[0].Row, drParent, dtTaskDelayedEndDate);

                dtTaskData.DefaultView[0]["ActualStartDate"] = Convert.ToDateTime(dtTemplateData.DefaultView[0]["ActualStartDate"].ToString()).ToString("dd-MMM-yyyy");
                dtTaskData.DefaultView[0]["ActualEndDate"] = Convert.ToDateTime(dtTemplateData.DefaultView[0]["ActualEndDate"].ToString()).ToString("dd-MMM-yyyy");

                dtTaskData.DefaultView[0]["SequentialStartDate"] = Convert.ToDateTime(dtTemplateData.DefaultView[0]["SequentialStartDate"].ToString()).ToString("dd-MMM-yyyy");
                dtTaskData.DefaultView[0]["SequentialEndDate"] = Convert.ToDateTime(dtTemplateData.DefaultView[0]["SequentialEndDate"].ToString()).ToString("dd-MMM-yyyy");

                dtTaskData.DefaultView[0]["DependentDate"] = Convert.ToDateTime(dtTemplateData.DefaultView[0]["DependentDate"].ToString()).ToString("dd-MMM-yyyy");

                for (int i = 0; i < dtTemplateData.DefaultView.Count; i++)
                {
                    dtTemplateData.DefaultView[i]["TempStartDate"] = dtTaskData.DefaultView[0]["TempStartDate"];
                    dtTemplateData.DefaultView[i]["TempEndDate"] = dtTaskData.DefaultView[0]["TempEndDate"];
                }


            }
            catch (Exception ex)
            {
                throw (ex);

            }
        }
        private void CalculatGeneralDateNew(DataRow Originaldate, int duration, int lagDays, DataRow drTemplate, DataTable Calendar, DataTable dtTaskData, DataTable dtTaskDelayedEndDate)
        {
            CalendarRowFilter("WorkingDate>=#" + Convert.ToDateTime(Originaldate["ActualDate"].ToString()).AddDays(clsStaticInfo.dbl(lagDays)).ToString("dd-MMM-yyyy") + "#", drTemplate, Calendar);


            Calendar.DefaultView.Sort = "WorkingDate ASC";
            drTemplate["TempStartDate"] = Convert.ToDateTime(Calendar.DefaultView[0]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");

            if (duration == 0)
                drTemplate["TempEndDate"] = Convert.ToDateTime(Calendar.DefaultView[duration]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
            else if (duration > 0)
                drTemplate["TempEndDate"] = Convert.ToDateTime(Calendar.DefaultView[duration - 1]["WorkingDate"].ToString()).ToString("dd-MMM-yyyy");
            else
                drTemplate["TempEndDate"] = Convert.ToDateTime(Calendar.DefaultView[0]["WorkingDate"].ToString()).AddDays(duration - 1).ToString("dd-MMM-yyyy");

            //main dependent date (eg. masterordercreationdate or deliveryDate)


            drTemplate["ActualStartDate"] = drTemplate["TempStartDate"].ToString();
            drTemplate["ActualEndDate"] = drTemplate["TempEndDate"].ToString();

            drTemplate["SequentialStartDate"] = drTemplate["TempStartDate"].ToString();
            drTemplate["SequentialEndDate"] = drTemplate["TempEndDate"].ToString();


            drTemplate["HasActualDate"] = false;
            if (Originaldate["HasActualDate"].ToString().ToUpper() == "YES")
                drTemplate["HasActualDate"] = true;

            drTemplate["DependentDate"] = Convert.ToDateTime(Originaldate["ActualDate"].ToString()).ToString("dd-MMM-yyyy");

            dtTaskData.DefaultView.RowFilter = "TaskTemplateId='" + drTemplate["TaskTemplateId"].ToString() + "'";
            //HasPredecessorActualDate
            dtTaskData.DefaultView[0]["HasPredecessorActualDate"] = true;
            if (drTemplate["PreTaskTemplateId"].ToString() != "")
            {
                if (bplib.clsWebLib.GetBoolData(drTemplate["HasActualDate"].ToString()) == false)
                    dtTaskData.DefaultView[0]["HasPredecessorActualDate"] = false;
            }

            dtTaskData.DefaultView[0]["TempStartDate"] = Convert.ToDateTime(drTemplate["TempStartDate"].ToString()).ToString("dd-MMM-yyyy");
            dtTaskData.DefaultView[0]["TempEndDate"] = SetDelyedTaskEndDate(
                Convert.ToDateTime(drTemplate["TempEndDate"].ToString()).ToString("dd-MMM-yyyy"),
                drTemplate, drTemplate, dtTaskDelayedEndDate);

            dtTaskData.DefaultView[0]["ActualStartDate"] = Convert.ToDateTime(drTemplate["ActualStartDate"].ToString()).ToString("dd-MMM-yyyy");
            dtTaskData.DefaultView[0]["ActualEndDate"] = Convert.ToDateTime(drTemplate["ActualEndDate"].ToString()).ToString("dd-MMM-yyyy");

            dtTaskData.DefaultView[0]["SequentialStartDate"] = Convert.ToDateTime(drTemplate["SequentialStartDate"].ToString()).ToString("dd-MMM-yyyy");
            dtTaskData.DefaultView[0]["SequentialEndDate"] = Convert.ToDateTime(drTemplate["SequentialEndDate"].ToString()).ToString("dd-MMM-yyyy");


            dtTaskData.DefaultView[0]["DependentDate"] = Convert.ToDateTime(drTemplate["DependentDate"].ToString()).ToString("dd-MMM-yyyy");

        }

        public DataTable getDependentDatesMasterOrderNew(string MasterOrderId)
        {
            string mm = @"SELECT k.Enum as DependentDatesEnum,convert(date,mo.AddedDate) as  ActualDate
                                    FROM (SELECT 'MasterOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id='" + MasterOrderId + @"' 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'MaterialCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrderItem AS mo ON mo.MasterOrderId='" + MasterOrderId + @"' GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'SOCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.MasterOrderItemId IN (SELECT Id
                                                                                                       FROM trn.MasterOrderItem WHERE MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'SOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.MasterOrderItemId IN (SELECT Id
                                                                                                       FROM trn.MasterOrderItem WHERE MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'FirstSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum
             
                                    UNION ALL
                                    SELECT k.Enum,convert(date,Max(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'LastSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.LSD)) AS Dates 
                                    FROM (SELECT 'LatestStartDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.MainRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'MainRawmaterialinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.OtherRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'OtherRMinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum
             	                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.AddedDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.ProductionOrder AS P1
                                    ON p1.Id IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                     SELECT k.Enum,convert(date,MIN(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderFirstOutputDate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MIN(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' ) AS D ON 1=1 GROUP BY k.Enum
                                                 
                                                                UNION ALL
                                    SELECT k.Enum,convert(date,MAX(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderLastoutputdate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MAX(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' ) AS D ON 1=1 GROUP BY k.Enum";
            DataTable dtData = _sqlRepository.GetDataTable(mm);
            dtData.Columns.Add("HasActualDate");
            // DependentDatesEnum.
            dtData.DefaultView.RowFilter = "DependentDatesEnum='" + DependentDatesEnum.MasterOrderCreationDate.ToString() + "'";
            string OrderCreationDate = dtData.DefaultView[0]["ActualDate"].ToString();
            //assume that we have a data for sequential date
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                dtData.Rows[i]["HasActualDate"] = "YES";
                if (dtData.Rows[i]["ActualDate"].ToString() == "")
                {
                    dtData.Rows[i]["ActualDate"] = OrderCreationDate;
                    dtData.Rows[i]["HasActualDate"] = "NO";
                }

            }



            return dtData;
        }
        public DataTable getDependentDatesStyleNew(string MasterOrderItemId, out string MasterOrderId)
        {
            MasterOrderId = "";
            string SQL = @"SELECT MasterOrderId FROM trn.MasterOrderItem WHERE id = '" + MasterOrderItemId + "'";
            DataTable dtRefData = _sqlRepository.GetDataTable(SQL);
            MasterOrderId = dtRefData.Rows[0]["MasterOrderId"].ToString();


            DataTable dtData = _sqlRepository.GetDataTable(@"SELECT k.Enum as DependentDatesEnum,convert(date,mo.AddedDate) as  ActualDate
                                    FROM (SELECT 'MasterOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id='" + MasterOrderId + @"' 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'MaterialCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrderItem AS mo 
                                    ON mo.MasterOrderId='" + MasterOrderId + @"'  AND id='" + MasterOrderItemId + @"' GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'SOCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.MasterOrderItemId IN (SELECT Id
                                                                                                       FROM trn.MasterOrderItem WHERE MasterOrderId='" + MasterOrderId + @"' AND id='" + MasterOrderItemId + @"') GROUP BY k.Enum 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'SOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.MasterOrderItemId IN (SELECT Id
                                                                                                       FROM trn.MasterOrderItem WHERE MasterOrderId='" + MasterOrderId + @"' AND id='" + MasterOrderItemId + @"') GROUP BY k.Enum 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'FirstSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND moi.id='" + MasterOrderItemId + @"') GROUP BY k.Enum
             
                                    UNION ALL
                                    SELECT k.Enum,convert(date,Max(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'LastSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND moi.id='" + MasterOrderItemId + @"') GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.LSD)) AS Dates 
                                    FROM (SELECT 'LatestStartDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND moi.id='" + MasterOrderItemId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.MainRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'MainRawmaterialinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND moi.id='" + MasterOrderItemId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.OtherRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'OtherRMinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND moi.id='" + MasterOrderItemId + @"') GROUP BY k.Enum
             	                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.AddedDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.ProductionOrder AS P1
                                    ON p1.Id IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND moi.id='" + MasterOrderItemId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                     SELECT k.Enum,convert(date,MIN(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderFirstOutputDate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MIN(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND moi.id='" + MasterOrderItemId + @"' ) AS D ON 1=1 GROUP BY k.Enum
                                                 
                                                                UNION ALL
                                    SELECT k.Enum,convert(date,MAX(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderLastoutputdate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MAX(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                                 WHERE moi.MasterOrderId='" + MasterOrderId + @"'  AND moi.id='" + MasterOrderItemId + @"') AS D ON 1=1 GROUP BY k.Enum");

            dtData.Columns.Add("HasActualDate");
            // DependentDatesEnum.
            dtData.DefaultView.RowFilter = "DependentDatesEnum='" + DependentDatesEnum.MasterOrderCreationDate.ToString() + "'";
            string OrderCreationDate = dtData.DefaultView[0]["ActualDate"].ToString();
            //assume that we have a data for sequential date
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                dtData.Rows[i]["HasActualDate"] = "YES";
                if (dtData.Rows[i]["ActualDate"].ToString() == "")
                {
                    dtData.Rows[i]["ActualDate"] = OrderCreationDate;
                    dtData.Rows[i]["HasActualDate"] = "NO";
                }

            }



            return dtData;
        }
        public DataTable getDependentDatesSalesOrderNew(string SalesOrderId, out string MasterOrderId)
        {
            string MasterOrderItemId = "";
            string SQL = @"SELECT MasterOrderItemId FROM trn.SalesOrder WHERE id = '" + SalesOrderId + "'";
            DataTable dtRefData = _sqlRepository.GetDataTable(SQL);
            MasterOrderItemId = dtRefData.Rows[0]["MasterOrderItemId"].ToString();


            MasterOrderId = "";
            SQL = @"SELECT MasterOrderId FROM trn.MasterOrderItem WHERE id = '" + MasterOrderItemId + "'";
            dtRefData = _sqlRepository.GetDataTable(SQL);
            MasterOrderId = dtRefData.Rows[0]["MasterOrderId"].ToString();

            SQL = @"SELECT k.Enum as DependentDatesEnum,convert(date,mo.AddedDate) as  ActualDate
                                    FROM (SELECT 'MasterOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id='" + MasterOrderId + @"' 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'MaterialCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrderItem AS mo 
                                    ON MO.id='" + MasterOrderItemId + @"' GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'SOCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.Id='" + SalesOrderId + @"' GROUP BY k.Enum 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'SOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.Id='" + SalesOrderId + @"' GROUP BY k.Enum 
                                    
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'FirstSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') GROUP BY k.Enum
             
                                    UNION ALL
                                    SELECT k.Enum,convert(date,Max(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'LastSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.LSD)) AS Dates 
                                    FROM (SELECT 'LatestStartDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.MainRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'MainRawmaterialinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.OtherRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'OtherRMinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') GROUP BY k.Enum
             	                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.AddedDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.ProductionOrder AS P1
                                    ON p1.Id IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                     SELECT k.Enum,convert(date,MIN(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderFirstOutputDate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MIN(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') AS D ON 1=1 GROUP BY k.Enum
                                                 
                                                                UNION ALL
                                    SELECT k.Enum,convert(date,MAX(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderLastoutputdate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MAX(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE so.id='" + SalesOrderId + @"') AS D ON 1=1 GROUP BY k.Enum";

            DataTable dtData = _sqlRepository.GetDataTable(SQL);

            dtData.Columns.Add("HasActualDate");
            // DependentDatesEnum.
            dtData.DefaultView.RowFilter = "DependentDatesEnum='" + DependentDatesEnum.MasterOrderCreationDate.ToString() + "'";
            string OrderCreationDate = dtData.DefaultView[0]["ActualDate"].ToString();
            //assume that we have a data for sequential date
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                dtData.Rows[i]["HasActualDate"] = "YES";
                if (dtData.Rows[i]["ActualDate"].ToString() == "")
                {
                    dtData.Rows[i]["ActualDate"] = OrderCreationDate;
                    dtData.Rows[i]["HasActualDate"] = "NO";
                }

            }



            return dtData;
        }
        public DataTable getDependentDatesProductionOrderNew(string ProductionOrderId, out string MasterOrderSingleId)
        {
            MasterOrderSingleId = "";

            string MasterOrderItemId = "";
            string SQL = @"select STUFF((select distinct ','+XMOI.Id from 
                            trn.SalesOrder XSO 
	                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
	                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
	                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
	                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
		                            where Xpod.ProductionOrderId='" + ProductionOrderId + @"'	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') AS MasterOrderItemId";
            DataTable dtRefData = _sqlRepository.GetDataTable(SQL);
            MasterOrderItemId = "'" + dtRefData.Rows[0]["MasterOrderItemId"].ToString().Replace(",", "','") + "'";


            string SalesOrderId = "";
            SQL = @"select STUFF((select distinct ','+XSO.Id from 
                            trn.SalesOrder XSO 
	                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
	                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
	                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
	                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
		                            where Xpod.ProductionOrderId='" + ProductionOrderId + @"'	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') AS SalesOrderId";
            dtRefData = _sqlRepository.GetDataTable(SQL);
            SalesOrderId = "'" + dtRefData.Rows[0]["SalesOrderId"].ToString().Replace(",", "','") + "'";


            string MasterOrderId = "";
            SQL = @"select STUFF((select distinct ','+XMO.Id from 
                            trn.SalesOrder XSO 
	                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
	                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
	                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
	                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
		                            where Xpod.ProductionOrderId='" + ProductionOrderId + @"'	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') AS MasterOrderId";
            dtRefData = _sqlRepository.GetDataTable(SQL);
            MasterOrderId = "'" + dtRefData.Rows[0]["MasterOrderId"].ToString().Replace(",", "','") + "'";

            string[] MasterOrderIds = dtRefData.Rows[0]["MasterOrderId"].ToString().Split(',');
            if (MasterOrderIds.Length > 1)
                throw new Exception("Multiple master order found. Cannot process TNA");

            MasterOrderSingleId = dtRefData.Rows[0]["MasterOrderId"].ToString();


            string trn = @"SELECT k.Enum as DependentDatesEnum,convert(date,mo.AddedDate) as  ActualDate
                                    FROM (SELECT 'MasterOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id IN (" + MasterOrderId + @") 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'MaterialCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.MasterOrderItem AS mo 
                                    ON id IN (" + MasterOrderItemId + @") GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.AddedDate)) AS Dates 
                                    FROM (SELECT 'SOCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.Id IN (" + SalesOrderId + @")  GROUP BY k.Enum 
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'SOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo ON mo.Id IN (" + SalesOrderId + @")  GROUP BY k.Enum 
                                    
                                    UNION ALL
                                    SELECT k.Enum,convert(date,min(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'FirstSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') GROUP BY k.Enum
             
                                    UNION ALL
                                    SELECT k.Enum,convert(date,Max(mo.DeliveryDate)) AS Dates 
                                    FROM (SELECT 'LastSOShipmentDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.SalesOrder AS mo
                                    ON mo.Id IN (SELECT so.Id FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') GROUP BY k.Enum
                                    UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.LSD)) AS Dates 
                                    FROM (SELECT 'LatestStartDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.MainRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'MainRawmaterialinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.OtherRawMaterialInhouseDate)) AS Dates 
                                    FROM (SELECT 'OtherRMinhouseDate' AS Enum) AS K
                                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS P1
                                    ON p1.ProductionOrderID IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') GROUP BY k.Enum
             	                                     UNION ALL
                                    SELECT k.Enum,convert(date,MIN(p1.AddedDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderCreationDate' AS Enum) AS K
                                    LEFT OUTER JOIN trn.ProductionOrder AS P1
                                    ON p1.Id IN (SELECT pod.ProductionOrderId
                                                                  FROM trn.ProductionOrderDetail AS pod
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') GROUP BY k.Enum

			                                     UNION ALL
                                     SELECT k.Enum,convert(date,MIN(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderFirstOutputDate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MIN(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                   INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') AS D ON 1=1 GROUP BY k.Enum
                                                 
                                                                UNION ALL
                                    SELECT k.Enum,convert(date,MAX(d.ProductionDate)) AS Dates 
                                    FROM (SELECT 'ProductionOrderLastoutputdate' AS Enum) AS K
                                    LEFT OUTER JOIN (SELECT MAX(ppt.ProductionDate) AS ProductionDate
                                                                  FROM trn.ProductionOrderDetail AS pod
									INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=pod.ProductionOrderId
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                 WHERE POD.ProductionOrderId='" + ProductionOrderId + @"') AS D ON 1=1 GROUP BY k.Enum";

            DataTable dtData = _sqlRepository.GetDataTable(trn);
            dtData.Columns.Add("HasActualDate");
            // DependentDatesEnum.
            dtData.DefaultView.RowFilter = "DependentDatesEnum='" + DependentDatesEnum.MasterOrderCreationDate.ToString() + "'";
            string OrderCreationDate = dtData.DefaultView[0]["ActualDate"].ToString();
            //assume that we have a data for sequential date
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                dtData.Rows[i]["HasActualDate"] = "YES";
                if (dtData.Rows[i]["ActualDate"].ToString() == "")
                {
                    dtData.Rows[i]["ActualDate"] = OrderCreationDate;
                    dtData.Rows[i]["HasActualDate"] = "NO";
                }

            }



            return dtData;
        }

        private string SetDelyedTaskEndDate(string EndDate, DataRow currentRow, DataRow ParentRow, DataTable dtTaskDelayedEndDate)
        {
            if (PushDates == false)
                return EndDate;

            if (clsStaticInfo.dbl(currentRow["Duration"].ToString()) == 0)
                return EndDate;

            currentRow["isCurrentDelayed"] = "NO";
            if (ParentRow["isCurrentDelayed"].ToString() == "YES")
            {
                currentRow["isCurrentDelayed"] = "YES";
                return EndDate;
            }

            dtTaskDelayedEndDate.DefaultView.RowFilter = "Id='" + currentRow["TaskTemplateId"].ToString() + "'";
            if (dtTaskDelayedEndDate.DefaultView.Count > 0)
            {

                if (Convert.ToDateTime(dtTaskDelayedEndDate.DefaultView[0]["TaskNewEndDate"].ToString()) >= Convert.ToDateTime(EndDate))
                {
                    currentRow["isCurrentDelayed"] = "YES";
                    currentRow["TempEndDate"] = Convert.ToDateTime(dtTaskDelayedEndDate.DefaultView[0]["TaskNewEndDate"].ToString()).ToString("dd-MMM-yyyy");
                    return Convert.ToDateTime(dtTaskDelayedEndDate.DefaultView[0]["TaskNewEndDate"].ToString()).ToString("dd-MMM-yyyy");
                }
            }



            return EndDate;

        }

        public string GetDependentDate(string DependentDateName, DataTable dtDependentDate)
        {
            dtDependentDate.DefaultView.RowFilter = "DependentDatesEnum='" + DependentDateName + "'";
            return Convert.ToDateTime(dtDependentDate.DefaultView[0]["ActualDate"].ToString()).ToString("dd-MMM-yyyy");
        }
        public void CalendarRowFilter(string FilterString, DataRow drTemplateDateToBeEdited, DataTable dtCalendar)
        {

            if (bplib.clsWebLib.GetBoolData(drTemplateDateToBeEdited["ConsiderOffDays"].ToString()) == true)
                dtCalendar.DefaultView.RowFilter = FilterString + " AND ISNULL(DayType,'')=''";
            else
                dtCalendar.DefaultView.RowFilter = FilterString;

        }
        #region MasterOrderTaskTemplates

        public void CopyTaskTemplate(string MasterOrderId)
        {

            DataTable dt = _sqlRepository.GetDataTable("SELECT * FROM trn.MasterOrder AS mo WHERE mo.Id='" + MasterOrderId + "'");
            if (dt.Rows.Count > 0)
            {
                DataTable dtMasterTask = _sqlRepository.GetDataTable("SELECT * FROM MasterOrderTaskTemplate AS mo WHERE mo.MasterOrderId='" + MasterOrderId + "'");

                if (dt.Rows[0]["TaskTemplateMasterId"].ToString() == "")
                    return;

                if (dtMasterTask.Rows.Count == 0)
                    CopyTask(MasterOrderId, dt.Rows[0]["TaskTemplateMasterId"].ToString());
            }

        }

        public void CopyTask(string MasterOrderId, string TemplateMasterId)
        {
            //copy with subtasks
            try
            {

                DataSet dsTaskSource, dsTaskDependencySource, dsSubTaskSource, dsTaskDestination, dsTaskDependencyDestination, dsSubTaskDestination;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskTemplate where TaskTemplateMasterId='" + TemplateMasterId + "' ORDER BY Id", out dsTaskSource, false, "1");
                con.OpenDataSetThroughAdapter(@"SELECT * FROM TaskTemplateDependency WHERE TaskTemplateId IN (SELECT TaskTemplate.Id
                                                                FROM TaskTemplate WHERE TaskTemplateMasterId='" + TemplateMasterId + "')", out dsTaskDependencySource, false, "1");
                con.OpenDataSetThroughAdapter(@"SELECT * FROM TaskTemplateSubTasks WHERE TaskTemplateId IN (SELECT TaskTemplate.Id
                                                                FROM TaskTemplate WHERE TaskTemplateMasterId='" + TemplateMasterId + "')", out dsSubTaskSource, false, "1");


                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where 1=2", out dsTaskDestination, false, "1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplateDependency where 1=2", out dsTaskDependencyDestination, false, "1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplateSubTasks where 1=2", out dsSubTaskDestination, false, "1");

                #region Master
                string _TaskMId = "";
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Template Task Creation", out _TaskMId);

                _TaskMId = _TaskMId.Replace("-", "");
                DataRow dr = null;
                for (int i = 0; i < dsTaskSource.Tables[0].Rows.Count; i++)
                {
                    string CurrentTaskTemplateId = _TaskMId + (i + 1).ToString();

                    dr = dsTaskDestination.Tables[0].NewRow();
                    dr["Id"] = CurrentTaskTemplateId;
                    dr["RefTaskTemplateId"] = dsTaskSource.Tables[0].Rows[i]["Id"].ToString();
                    dr["TaskDescription"] = dsTaskSource.Tables[0].Rows[i]["TaskDescription"].ToString();
                    dr["MasterOrderId"] = MasterOrderId;
                    dr["TaskMasterId"] = dsTaskSource.Tables[0].Rows[i]["TaskMasterId"].ToString();

                    dr["Active"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["Active"].ToString());
                    dr["Remarks"] = dsTaskSource.Tables[0].Rows[i]["Remarks"].ToString();
                    dr["Sequence"] = clsStaticInfo.dbl(dsTaskSource.Tables[0].Rows[i]["Sequence"].ToString());
                    dr["IsFirstTask"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["IsFirstTask"].ToString());
                    dr["IsLastTask"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["IsLastTask"].ToString());



                    dr["ForNewOrder"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["ForNewOrder"].ToString());
                    dr["IsMandatory"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["IsMandatory"].ToString());
                    dr["TaskType"] = dsTaskSource.Tables[0].Rows[i]["TaskType"].ToString();
                    dr["IsTaskMilestone"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["IsTaskMilestone"].ToString());
                    dr["TaskDependentDatesId"] = dsTaskSource.Tables[0].Rows[i]["TaskDependentDatesId"].ToString();
                    dr["TaskAppliedOnId"] = dsTaskSource.Tables[0].Rows[i]["TaskAppliedOnId"].ToString();
                    dr["WillSendEmail"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["WillSendEmail"].ToString());
                    dr["WillSendSMS"] = bplib.clsWebLib.GetBoolData(dsTaskSource.Tables[0].Rows[i]["WillSendSMS"].ToString());
                    dr["ResponsiblePersonCategory"] = dsTaskSource.Tables[0].Rows[i]["ResponsiblePersonCategory"].ToString();
                    dr["predecessor"] = dsTaskSource.Tables[0].Rows[i]["predecessor"].ToString();
                    dr["EmployeeId"] = bplib.clsWebLib.RetValidLen(dsTaskSource.Tables[0].Rows[i]["EmployeeId"].ToString());

                    dr["LagDays"] = clsStaticInfo.dbl(dsTaskSource.Tables[0].Rows[i]["LagDays"].ToString());


                    dr["Duration"] = clsStaticInfo.dbl(dsTaskSource.Tables[0].Rows[i]["Duration"].ToString());
                    dr["startDate"] = dsTaskSource.Tables[0].Rows[i]["startDate"].ToString();
                    dr["endDate"] = dsTaskSource.Tables[0].Rows[i]["startDate"].ToString();

                    dr["AddedBy"] = "Scheduler";
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = "";
                    dr["UpdatedBy"] = "Scheduler";
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = "";


                    dsTaskDestination.Tables[0].Rows.Add(dr);


                    #region subtasks
                    dsSubTaskSource.Tables[0].DefaultView.RowFilter = "TaskTemplateId='" + dsTaskSource.Tables[0].Rows[i]["Id"].ToString() + "'";
                    for (int j = 0; j < dsSubTaskSource.Tables[0].DefaultView.Count; j++)
                    {
                        dr = dsSubTaskDestination.Tables[0].NewRow();
                        dr["MasterOrderTaskTemplateId"] = CurrentTaskTemplateId;
                        dr["SubTaskDescription"] = dsSubTaskSource.Tables[0].DefaultView[j]["SubTaskDescription"].ToString();

                        dr["AddedBy"] = "Scheduler";
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = "";
                        dr["UpdatedBy"] = "Scheduler";
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = "";

                        dsSubTaskDestination.Tables[0].Rows.Add(dr);
                    }
                    #endregion subtasks
                }

                #endregion Master

                #region Task Dependencies


                for (int i = 0; i < dsTaskDependencySource.Tables[0].Rows.Count; i++)
                {
                    dr = dsTaskDependencyDestination.Tables[0].NewRow();

                    dr["PreTaskTemplateId"] = ReferenceTaskTemplateIdForDependency(dsTaskDependencySource.Tables[0].Rows[i]["PreTaskTemplateId"].ToString(), dsTaskDestination);
                    dr["TaskTemplateId"] = ReferenceTaskTemplateIdForDependency(dsTaskDependencySource.Tables[0].Rows[i]["TaskTemplateId"].ToString(), dsTaskDestination);
                    dr["Criteria"] = dsTaskDependencySource.Tables[0].Rows[i]["Criteria"].ToString();
                    dr["LagDays"] = dsTaskDependencySource.Tables[0].Rows[i]["LagDays"].ToString();

                    dr["AddedBy"] = "Scheduler";
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = "";
                    dr["UpdatedBy"] = "Scheduler";
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = "";

                    dsTaskDependencyDestination.Tables[0].Rows.Add(dr);
                }


                for (int i = 0; i < dsTaskDestination.Tables[0].Rows.Count; i++)
                {
                    string _pre = "";
                    dsTaskDependencyDestination.Tables[0].DefaultView.RowFilter = "TaskTemplateId='" + dsTaskDestination.Tables[0].Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < dsTaskDependencyDestination.Tables[0].DefaultView.Count; K++)
                    {
                        string _s = dsTaskDependencyDestination.Tables[0].DefaultView[K]["PreTaskTemplateId"].ToString() + dsTaskDependencyDestination.Tables[0].DefaultView[K]["Criteria"].ToString();

                        if (clsStaticInfo.dbl(dsTaskDependencyDestination.Tables[0].DefaultView[K]["LagDays"].ToString()) > 0)
                            _s += "+" + Math.Abs(clsStaticInfo.dbl(dsTaskDependencyDestination.Tables[0].DefaultView[K]["LagDays"].ToString()));

                        if (clsStaticInfo.dbl(dsTaskDependencyDestination.Tables[0].DefaultView[K]["LagDays"].ToString()) < 0)
                            _s += "-" + Math.Abs(clsStaticInfo.dbl(dsTaskDependencyDestination.Tables[0].DefaultView[K]["LagDays"].ToString()));

                        if (_pre == "")
                            _pre = _s;
                        else
                            _pre += "," + _s;

                    }
                    dsTaskDestination.Tables[0].Rows[i].BeginEdit();
                    dsTaskDestination.Tables[0].Rows[i]["predecessor"] = _pre;
                    dsTaskDestination.Tables[0].Rows[i].EndEdit();

                }
                #endregion Task Dependencies

                dsTaskDestination.Tables[0].DefaultView.RowFilter = null;
                dsTaskDependencyDestination.Tables[0].DefaultView.RowFilter = null;
                dsSubTaskDestination.Tables[0].DefaultView.RowFilter = null;

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTaskDestination, dsTaskDependencyDestination, dsSubTaskDestination);


                UpdateTaskStatus();
                //master order tasks
                string sql = @"SELECT MO.* FROM trn.MasterOrder AS mo WHERE mo.Id='"+MasterOrderId+"' AND ISNULL(mo.TaskTemplateMasterId,'')='"+TemplateMasterId+"'";

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {

                        DataTable dt = GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.MasterOrder);
                        if (dt.Rows.Count > 0)
                            MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.MasterOrder);

                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            catch (Exception ex)
            {

            }

        }

        private string ReferenceTaskTemplateIdForDependency(string OriginalTemplateId, DataSet dsTemplate)
        {
            dsTemplate.Tables[0].DefaultView.RowFilter = "RefTaskTemplateId='" + OriginalTemplateId + "'";
            return dsTemplate.Tables[0].DefaultView[0]["Id"].ToString();
        }


        #endregion MasterOrderTaskTemplates


        public void UpdateTaskStatus()
        {
            try

            {
                _sqlRepository.ExecuteSqlCommand(@"UPDATE TaskManagerMaster SET CurrentStatus = 'Closed',ClosingDate = GETDATE(),ClosedBy='System'
                                                            FROM  TaskManagerMaster TM 
                                                            INNER JOIN TNATasks AS t ON t.Id=tm.TNATasksId
                                                            INNER JOIN TNAMaster AS t2 ON t2.Id=t.TNAMasterId
                                                            INNER JOIN trn.MasterOrder AS mo ON mo.Id=t2.MasterOrderId
                                                            WHERE isnull(mo.OrderStatusId,'')<>'Active' AND isnull(tm.CurrentStatus,'')<>'Closed'");

                _sqlRepository.ExecuteSqlCommand(@"UPDATE TaskManagerMaster SET CurrentStatus = 'Closed',ClosingDate = GETDATE(),ClosedBy='System'
                                                            FROM  TaskManagerMaster TM 
                                                            INNER JOIN TNATasks AS t ON t.Id=tm.TNATasksId
                                                            INNER JOIN TNAMaster AS t2 ON t2.Id=t.TNAMasterId
                                                            INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=t2.MasterOrderItemId
                                                            INNER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
                                                            WHERE isnull(mo.OrderStatusId,'')<>'Active' AND isnull(tm.CurrentStatus,'')<>'Closed'");

                _sqlRepository.ExecuteSqlCommand(@"UPDATE TaskManagerMaster SET CurrentStatus = 'Closed',ClosingDate = GETDATE(),ClosedBy='System'
                                                            FROM  TaskManagerMaster TM 
                                                            INNER JOIN TNATasks AS t ON t.Id=tm.TNATasksId
                                                            INNER JOIN TNAMaster AS t2 ON t2.Id=t.TNAMasterId
                                                            INNER JOIN trn.SalesOrder mo ON mo.Id=t2.SalesOrderId
                                                            WHERE isnull(mo.OrderStatusId,'')<>'Active' AND isnull(tm.CurrentStatus,'')<>'Closed'");

                _sqlRepository.ExecuteSqlCommand(@"UPDATE TaskManagerMaster SET CurrentStatus = 'Closed',ClosingDate = GETDATE(),ClosedBy='System'
                                                            FROM  TaskManagerMaster TM 
                                                            INNER JOIN TNATasks AS t ON t.Id=tm.TNATasksId
                                                            INNER JOIN TNAMaster AS t2 ON t2.Id=t.TNAMasterId
                                                            INNER JOIN trn.ProductionOrder mo ON mo.Id=t2.ProductionOrderId
                                                            INNER JOIN hkp.ProductionStatus AS ps ON ps.Id=mo.ProductionStatusId
                                                            WHERE isnull(ps.UserName,'')='Closed' AND isnull(tm.CurrentStatus,'')<>'Closed'");
            }
            catch (Exception ex)
            {


            }
        }

        public void RunTNASchedule()
        {
            try
            {
                UpdateTaskStatus();
                //master order tasks
                string sql = @"SELECT MO.* FROM trn.MasterOrder AS mo WHERE mo.OrderStatusId<>'Closed' AND ISNULL(mo.TaskTemplateMasterId,'')<>''";

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {

                        DataTable dt = GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.MasterOrder);
                        if (dt.Rows.Count > 0)
                            MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.MasterOrder);

                    }
                    catch (Exception ex)
                    {

                    }
                }



                //line item related tasks
                sql = @"SELECT MOI.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                WHERE os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ISNULL(mo.TaskTemplateMasterId,'')<>''";

                dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {
                        DataTable dt = GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.Style);
                        if (dt.Rows.Count > 0)
                            MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.Style);
                    }
                    catch (Exception ex)
                    {

                    }
                }


                //Sales Order Related Tasks
                sql = @"SELECT SO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ISNULL(mo.TaskTemplateMasterId,'')<>''";

                dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {
                        DataTable dt = GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                        if (dt.Rows.Count > 0)
                            MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                    }
                    catch (Exception ex)
                    {

                    }
                }


                //Production Order Related Tasks
                sql = @"SELECT DISTINCT  PO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                INNER JOIN trn.SalesOrder AS so2 ON so2.Id=pod.SalesOrderId
                                INNER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                    WHERE ISNULL(mo.TaskTemplateMasterId,'')<>'' AND os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ps.StandardName<>'CLOSED'";

                dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {

                        DataTable dt = GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                        if (dt.Rows.Count > 0)
                            MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                    }
                    catch (Exception ex)
                    {


                    }

                }

                ConnectionManager.clsConnection ConManager = new ConnectionManager.clsConnection();
                ConManager.BeginTransaction();
                ConManager.executeQuery("delete from tnalog");
                ConManager.CommitTransaction();
            }
            catch (Exception ex)
            {


            }
        }
        public void MakeTNAMaster(DataTable dtData, string TransactionId, TaskAppliedOnEnum ScheduleFor)
        {
            try
            {

                bplib.clsGenID genId = new bplib.clsGenID();


                dtData.DefaultView.RowFilter = "TaskAppliedOnEnum='" + ScheduleFor.ToString() + "'";
                dtData = dtData.DefaultView.ToTable();


                //creating Master
                string TNAMasterSystemID = "";
                string columnname = ColumnName(ScheduleFor);
                string sql = "select * from TNAMaster where " + columnname + "='" + TransactionId + "' AND TNAAppliedOn='" + ScheduleFor.ToString() + "'";
                GetDataSet(sql, out DataSet dsMaster);
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genId.GenID("TNA MASTER", out TNAMasterSystemID);
                    TNAMasterSystemID = "TM" + TNAMasterSystemID;
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = TNAMasterSystemID;
                    dr[columnname] = TransactionId;
                    dr["TNAAppliedOn"] = ScheduleFor.ToString();
                    dr["AddedBy"] = "Scheduler";
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = "";
                    dr["UpdatedBy"] = "Scheduler";
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = "";

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].Rows[0];
                    TNAMasterSystemID = dr["Id"].ToString();
                    dr.BeginEdit();
                    dr["UpdatedBy"] = "Scheduler";
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = "";
                    dr.EndEdit();
                }

                if (TNAMasterSystemID == "TM81")
                {

                }

                sql = "Select * from TNATasks where TNAMasterId='" + TNAMasterSystemID + "'";
                GetDataSet(sql, out DataSet dsChild);
                string ChildSystemId = "";
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "TaskTemplateId='" + dtData.Rows[i]["TaskTemplateId"].ToString() + "'";
                    if (dsChild.Tables[0].DefaultView.Count == 0)
                    {
                        if (ChildSystemId == "")
                        {
                            genId.GenID("TNA CHILD", out ChildSystemId);
                            ChildSystemId = "TC" + ChildSystemId;
                        }
                        DataRow dr = dsChild.Tables[0].NewRow();

                        dr["Id"] = ChildSystemId + "-" + (i + 1).ToString();
                        dr["TNAMasterId"] = TNAMasterSystemID;
                        dr["TaskTemplateId"] = dtData.Rows[i]["TaskTemplateId"].ToString();
                        dr["HasActualDate"] = dtData.Rows[i]["HasActualDate"].ToString();
                        dr["HasPredecessorActualDate"] = dtData.Rows[i]["HasPredecessorActualDate"].ToString();
                        dr["ACTIVE"] = dtData.Rows[i]["ACTIVE"].ToString();
                        dr["TempStartDate"] = dtData.Rows[i]["TempStartDate"].ToString();
                        dr["TempEndDate"] = dtData.Rows[i]["TempEndDate"].ToString();
                        dr["ActualStartDate"] = dtData.Rows[i]["ActualStartDate"].ToString();
                        dr["ActualEndDate"] = dtData.Rows[i]["ActualEndDate"].ToString();
                        dr["SequentialStartDate"] = dtData.Rows[i]["SequentialStartDate"].ToString();
                        dr["SequentialEndDate"] = dtData.Rows[i]["SequentialEndDate"].ToString();
                        dr["OriginalSequentialStartDate"] = dtData.Rows[i]["OriginalSequentialStartDate"].ToString();
                        dr["OriginalSequentialEndDate"] = dtData.Rows[i]["OriginalSequentialEndDate"].ToString();
                        dr["EmployeeId"] = bplib.clsWebLib.RetValidLen(dtData.Rows[i]["EmployeeId"].ToString());

                        dr["DependentDate"] = dtData.Rows[i]["DependentDate"];


                        dr["AddedBy"] = "Scheduler";
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = "";
                        dr["UpdatedBy"] = "Scheduler";
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = "";

                        dsChild.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["HasActualDate"] = dtData.Rows[i]["HasActualDate"].ToString();
                        dr["HasPredecessorActualDate"] = dtData.Rows[i]["HasPredecessorActualDate"].ToString();
                        dr["ACTIVE"] = dtData.Rows[i]["ACTIVE"].ToString();
                        dr["TempStartDate"] = dtData.Rows[i]["TempStartDate"].ToString();
                        dr["TempEndDate"] = dtData.Rows[i]["TempEndDate"].ToString();
                        dr["ActualStartDate"] = dtData.Rows[i]["ActualStartDate"].ToString();
                        dr["ActualEndDate"] = dtData.Rows[i]["ActualEndDate"].ToString();
                        dr["SequentialStartDate"] = dtData.Rows[i]["SequentialStartDate"].ToString();
                        dr["SequentialEndDate"] = dtData.Rows[i]["SequentialEndDate"].ToString();
                        dr["OriginalSequentialStartDate"] = dtData.Rows[i]["OriginalSequentialStartDate"].ToString();
                        dr["OriginalSequentialEndDate"] = dtData.Rows[i]["OriginalSequentialEndDate"].ToString();
                        ////this line should be deleted
                        dr["DependentDate"] = dtData.Rows[i]["DependentDate"];


                        if (string.IsNullOrEmpty(dr["EmployeeId"].ToString()))
                            dr["EmployeeId"] = bplib.clsWebLib.RetValidLen(dtData.Rows[i]["EmployeeId"].ToString());


                        dr["UpdatedBy"] = "Scheduler";
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = "";
                        dr.EndEdit();
                    }
                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster, dsChild);

                CopyTNATasks(TNAMasterSystemID);
            }
            catch (Exception ex)
            {

            }
        }
        private string ColumnName(TaskAppliedOnEnum ScheduleFor)
        {
            if (ScheduleFor == TaskAppliedOnEnum.MasterOrder)
                return "MasterOrderId";
            if (ScheduleFor == TaskAppliedOnEnum.SalesOrder)
                return "SalesOrderId";
            if (ScheduleFor == TaskAppliedOnEnum.Style)
                return "MasterOrderItemId";
            if (ScheduleFor == TaskAppliedOnEnum.ProductionOrder)
                return "ProductionOrderId";
            return "";
        }

        private void CopyTNATasks(string TNAMasterId)
        {

            try
            {
                bplib.clsGenID genid = new bplib.clsGenID();

                string strTaskMangerMasterId = "";




                //string sql = @"SELECT TT.Id,MT.TaskDescription,MT.StoryPoint,TT.OriginalSequentialStartDate AS TempStartDate,TT.OriginalSequentialEndDate AS TempEndDate,tm.TaskCategoryId,tm.TaskSubCategoryId,
                //                    ISNULL(mo.ResponsiblePersonId,ttm.EmployeeId) AS AssignedBy,tt.EmployeeId AS AssignTo,t.MasterOrderId,
                //                    t.MasterOrderItemId, t.SalesOrderId, t.ProductionOrderId, t.TNAAppliedOn,
                //                    MASO.MDesc,li.STDesc,so.SODesc,po.PODesc

                //                      FROM TNATasks TT
                //                    INNER JOIN MasterOrderTaskTemplate AS MT ON tt.TaskTemplateId=mt.Id
                //                    INNER JOIN trn.MasterOrder AS mo ON mo.Id=mt.MasterOrderId
                //                    INNER JOIN TaskTemplateMaster AS ttm ON ttm.Id=mo.TaskTemplateMasterId
                //                    INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tt.EmployeeId
                //                    INNER JOIN TaskMaster AS tm ON tm.Id=mt.TaskMasterId
                //                    INNER JOIN TNAMaster AS t ON t.Id=tt.TNAMasterId

                //                    LEFT OUTER JOIN (
                //                    SELECT mo.Id, CONCAT(' Master Order#',mo.Id,'(',b.UserName,')',' ,SO Desc:',STUFF((select distinct ','+XSO.[Description] from 
                //                                                                     trn.SalesOrder XSO 
                //                                                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //                                                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //                                                         where Mo.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')) AS MDesc
                //                      FROM trn.MasterOrder AS mo	
                //                    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                //                    ) AS MASO ON maso.Id=t.MasterOrderId

                //                    LEFT OUTER JOIN (
                //                    SELECT moi.Id, CONCAT('Buyer Item#',moi.BuyerReferenceNo, ', Master Order#',mo.Id,'(',b.UserName,')',' ,SO Desc:',STUFF((select distinct ','+XSO.[Description] from 
                //                                                                     trn.SalesOrder XSO 
                //                                                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //                                                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //                                                         where Moi.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')) AS STDesc
                //                      FROM trn.MasterOrder AS mo	
                //                      INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                //                      LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                //                    ) AS LI ON li.Id=t.MasterOrderItemId
                //                    LEFT OUTER JOIN (
                //                     SELECT SO.Id, CONCAT( 'LineItem#',SO.LineItemReference, ' SO Id:',so.Id,
                //                     CASE WHEN ISNULL(cp.PONumber,'')<>'' THEN CONCAT(', PO#',cp.PONumber,format(cp.PODate,'dd-MMM-yyyy'),' ') ELSE '' END,
                //                     ', Del. Date ',format(so.DeliveryDate,'dd-MMM-yyyy'), ', Buyer Item#',moi.BuyerReferenceNo, ', Master Order#',mo.Id,'(',b.UserName,')',', SO Desc:',so.[Description]) AS SODesc
                //                      FROM trn.MasterOrder AS mo	
                //                      INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                //                      INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                //                      LEFT JOIN  trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                //                      LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                //                    ) AS SO ON So.Id=t.SalesOrderId

                //                    LEFT OUTER JOIN (
                //                   SELECT PO.Id, CONCAT('Prod Order#', po.Id,', ',
                //                            'Buyer ',STUFF((select distinct ','+XB.UserName from 
                //                                                                     trn.SalesOrder XSO 
                //                                                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //                                                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //                                                                      left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                //                                                                      left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                //                                                                       where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                //                            ', Buyer Item#',STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                //                                                                     trn.SalesOrder XSO 
                //                                                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //                                                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //                                                                       where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                //                            ', Master Order#',STUFF((select distinct ','+XMO.MasterOrderNo from 
                //                                                                     trn.SalesOrder XSO 
                //                                                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //                                                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //                                                                      left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                //                                                         where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                //                   ', SO Desc:',STUFF((select distinct ','+XSO.[Description] from 
                //                                                                     trn.SalesOrder XSO 
                //                                                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //                                                                      --left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //                                                                      --left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                //                                                         where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                //                            ) AS PODesc
                //                      FROM trn.MasterOrder AS mo	
                //                      INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                //                      INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                //                      INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                //                      INNER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                //                      LEFT JOIN  trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                //                      LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                //                    ) AS PO ON PO.Id=t.ProductionOrderId

                //                WHERE tt.TNAMasterId='" + TNAMasterId + @"' AND isnull(tt.[ACTIVE],'')=1 ";


                string sql = @"SELECT TT.Id,MT.TaskDescription,MT.StoryPoint,TT.SequentialStartDate AS TempStartDate,TT.SequentialEndDate AS TempEndDate,tm.TaskCategoryId,tm.TaskSubCategoryId,
                                    ISNULL(mo.ResponsiblePersonId,ttm.EmployeeId) AS AssignedBy,tt.EmployeeId AS AssignTo,t.MasterOrderId,
                                    t.MasterOrderItemId, t.SalesOrderId, t.ProductionOrderId, t.TNAAppliedOn,
                                    MASO.MDesc,li.STDesc,so.SODesc,po.PODesc

                                      FROM TNATasks TT
                                    INNER JOIN MasterOrderTaskTemplate AS MT ON tt.TaskTemplateId=mt.Id
                                    INNER JOIN trn.MasterOrder AS mo ON mo.Id=mt.MasterOrderId
                                    INNER JOIN TaskTemplateMaster AS ttm ON ttm.Id=mo.TaskTemplateMasterId
                                    INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tt.EmployeeId
                                    INNER JOIN TaskMaster AS tm ON tm.Id=mt.TaskMasterId
                                    INNER JOIN TNAMaster AS t ON t.Id=tt.TNAMasterId

                                    LEFT OUTER JOIN (
                                    SELECT mo.Id, CONCAT(' Master Order#',mo.Id,'(',b.UserName,')',' ,SO Desc:',STUFF((select distinct ','+XSO.[Description] from 
	                                                                                    trn.SalesOrder XSO 
		                                                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                                                      where Mo.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')) AS MDesc
                                      FROM trn.MasterOrder AS mo	
                                    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                    ) AS MASO ON maso.Id=t.MasterOrderId

                                    LEFT OUTER JOIN (
                                    SELECT moi.Id, CONCAT('Buyer Item#',moi.BuyerReferenceNo, ', Master Order#',mo.Id,'(',b.UserName,')',' ,SO Desc:',STUFF((select distinct ','+XSO.[Description] from 
	                                                                                    trn.SalesOrder XSO 
		                                                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                                                      where Moi.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')) AS STDesc
                                      FROM trn.MasterOrder AS mo	
                                      INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                      LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                    ) AS LI ON li.Id=t.MasterOrderItemId
                                    LEFT OUTER JOIN (
                                     SELECT SO.Id, CONCAT( 'LineItem#',SO.LineItemReference, ' SO Id:',so.Id,
	                                    CASE WHEN ISNULL(cp.PONumber,'')<>'' THEN CONCAT(', PO#',cp.PONumber,format(cp.PODate,'dd-MMM-yyyy'),' ') ELSE '' END,
	                                    ', Del. Date ',format(so.DeliveryDate,'dd-MMM-yyyy'), ', Buyer Item#',moi.BuyerReferenceNo, ', Master Order#',mo.Id,'(',b.UserName,')',', SO Desc:',so.[Description]) AS SODesc
                                      FROM trn.MasterOrder AS mo	
                                      INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                      INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                                      LEFT JOIN  trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                                      LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                    ) AS SO ON So.Id=t.SalesOrderId

                                    LEFT OUTER JOIN (
                                   SELECT PO.Id, CONCAT('Prod Order#', po.Id,', ',
                                            'Buyer ',STUFF((select distinct ','+XB.UserName from 
	                                                                                    trn.SalesOrder XSO 
		                                                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            ', Buyer Item#',STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
	                                                                                    trn.SalesOrder XSO 
		                                                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                                            ', Master Order#',STUFF((select distinct ','+XMO.MasterOrderNo from 
	                                                                                    trn.SalesOrder XSO 
		                                                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                                      where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                ', SO Desc:',STUFF((select distinct ','+XSO.[Description] from 
	                                                                                    trn.SalesOrder XSO 
		                                                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                    --left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                                    --left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                                      where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                            ) AS PODesc
                                      FROM trn.MasterOrder AS mo	
                                      INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                      INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                                      INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                      INNER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                                      LEFT JOIN  trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                                      LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                    ) AS PO ON PO.Id=t.ProductionOrderId

                                WHERE tt.TNAMasterId='" + TNAMasterId + @"' AND isnull(tt.[ACTIVE],'')=1 ";

                DataTable dtRefTaskMaster = _sqlRepository.GetDataTable(sql);

                sql = @"SELECT TT.Id,ST.SubTaskDescription
                                  FROM TNATasks TT
                                INNER JOIN MasterOrderTaskTemplateSubTasks AS ST ON st.MasterOrderTaskTemplateId=tt.TaskTemplateId
                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tt.EmployeeId
                                WHERE tt.TNAMasterId='" + TNAMasterId + @"'";
                DataTable dtRefSubTasks = _sqlRepository.GetDataTable(sql);



                //get all existing tasks
                sql = @"SELECT * FROM TaskManagerMaster AS tmm WHERE tmm.Id IN (
                        SELECT tmm2.Id FROM TaskManagerMaster AS tmm2
                        INNER JOIN TNATasks AS t ON t.Id=tmm2.TNATasksId
                        WHERE t.TNAMasterId='" + TNAMasterId + @"'	
                        ) ";
                DataSet dsTaskManagerMaster = getDataset(sql);

                GetDataSet(@"Select * from TaskAudit  AS tmm WHERE tmm.TaskManagerMasterId IN (
                        SELECT tmm2.Id FROM TaskManagerMaster AS tmm2
                        INNER JOIN TNATasks AS t ON t.Id=tmm2.TNATasksId
                        WHERE t.TNAMasterId='" + TNAMasterId + @"'	
                        )", out DataSet dsAuditDestination);

                GetDataSet("Select * from TaskManagerSubTasks where 1=2", out DataSet dsSubTasksDestination);

                for (int i = 0; i < dtRefTaskMaster.Rows.Count; i++)
                {
                    dsTaskManagerMaster.Tables[0].DefaultView.RowFilter = "TNATasksId='" + dtRefTaskMaster.Rows[i]["Id"].ToString() + "'";
                    if (dsTaskManagerMaster.Tables[0].DefaultView.Count == 0)
                    {

                        if (strTaskMangerMasterId == "")
                        {
                            genid.GenID("TNA", out strTaskMangerMasterId);
                            strTaskMangerMasterId = "TNA" + strTaskMangerMasterId;
                        }

                        #region Task Manager Master
                        DataRow dr = dsTaskManagerMaster.Tables[0].NewRow();

                        dr["Id"] = strTaskMangerMasterId + "-" + (i + 1).ToString();
                        dr["TaskType"] = TaskTypeEnum.TNA.ToString();
                        string RefTaskMangerMasterId = dr["Id"].ToString();

                        if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.MasterOrder.ToString().ToUpper())
                            dr["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["MDesc"].ToString();

                        else if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.Style.ToString().ToUpper())
                            dr["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["STDesc"].ToString();

                        else if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.SalesOrder.ToString().ToUpper())
                            dr["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["SODesc"].ToString();

                        else if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.ProductionOrder.ToString().ToUpper())
                            dr["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["PODesc"].ToString();


                        dr["CurrentStatus"] = CurrentStatusEnum.ToStart.ToString();
                        dr["TaskTypeGroup"] = TaskCategoryFlagEnum.TNA.ToString();
                        dr["TaskCategoryId"] = bplib.clsWebLib.RetValidLen(dtRefTaskMaster.Rows[i]["TaskCategoryId"].ToString());
                        dr["TaskSubCategoryId"] = bplib.clsWebLib.RetValidLen(dtRefTaskMaster.Rows[i]["TaskSubCategoryId"].ToString());
                        dr["TNATasksId"] = dtRefTaskMaster.Rows[i]["Id"].ToString();
                        dr["TaskPriority"] = "4";
                        dr["StoryPoint"] = clsStaticInfo.dbl(dtRefTaskMaster.Rows[i]["StoryPoint"].ToString());

                        dr["AddedBy"] = "Scheduler";
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = "";
                        dr["UpdatedBy"] = "Scheduler";
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = "";

                        dsTaskManagerMaster.Tables[0].Rows.Add(dr);
                        #endregion Task Manager Master

                        dtRefSubTasks.DefaultView.RowFilter = "Id='" + dtRefTaskMaster.Rows[i]["Id"].ToString() + "'";
                        for (int K = 0; K < dtRefSubTasks.DefaultView.Count; K++)
                        {


                            DataRow drSub = dsSubTasksDestination.Tables[0].NewRow();
                            drSub["Id"] = RefTaskMangerMasterId + "-" + (K + 1).ToString();
                            drSub["TaskManagerMasterId"] = RefTaskMangerMasterId;
                            drSub["TaskDetail"] = dtRefSubTasks.DefaultView[K]["SubTaskDescription"].ToString();

                            drSub["AddedBy"] = "Scheduler";
                            drSub["AddedDate"] = System.DateTime.Now.ToString();
                            drSub["AddedFromIP"] = "";
                            drSub["UpdatedBy"] = "Scheduler";
                            drSub["UpdatedDate"] = System.DateTime.Now.ToString();
                            drSub["UpdatedFromIP"] = "";

                            dsSubTasksDestination.Tables[0].Rows.Add(drSub);
                        }


                        DataRow drAudit = dsAuditDestination.Tables[0].NewRow();
                        drAudit["Id"] = RefTaskMangerMasterId + "-1";
                        drAudit["TaskManagerMasterId"] = RefTaskMangerMasterId;
                        drAudit["AuthorizationType"] = AuthorizationTypeEnum.CreatedBy.ToString();
                        drAudit["ResponsiblePersonId"] = dtRefTaskMaster.Rows[i]["AssignedBy"].ToString();
                        drAudit["DueDate"] = dtRefTaskMaster.Rows[i]["TempEndDate"].ToString();
                        drAudit["isRead"] = true;

                        drAudit["AddedBy"] = "Scheduler";
                        drAudit["AddedDate"] = System.DateTime.Now.ToString();
                        drAudit["AddedFromIP"] = "";
                        drAudit["UpdatedBy"] = "Scheduler";
                        drAudit["UpdatedDate"] = System.DateTime.Now.ToString();
                        drAudit["UpdatedFromIP"] = "";
                        dsAuditDestination.Tables[0].Rows.Add(drAudit);


                        drAudit = dsAuditDestination.Tables[0].NewRow();
                        drAudit["Id"] = RefTaskMangerMasterId + "-2";
                        drAudit["TaskManagerMasterId"] = RefTaskMangerMasterId;
                        drAudit["AuthorizationType"] = AuthorizationTypeEnum.AssignTo.ToString();
                        drAudit["ResponsiblePersonId"] = dtRefTaskMaster.Rows[i]["AssignTo"].ToString();
                        drAudit["DueDate"] = dtRefTaskMaster.Rows[i]["TempEndDate"].ToString();
                        drAudit["isRead"] = false;

                        drAudit["AddedBy"] = "Scheduler";
                        drAudit["AddedDate"] = System.DateTime.Now.ToString();
                        drAudit["AddedFromIP"] = "";
                        drAudit["UpdatedBy"] = "Scheduler";
                        drAudit["UpdatedDate"] = System.DateTime.Now.ToString();
                        drAudit["UpdatedFromIP"] = "";
                        dsAuditDestination.Tables[0].Rows.Add(drAudit);
                    }
                    else
                    {
                        dsAuditDestination.Tables[0].DefaultView.RowFilter = "TaskManagerMasterId='" + dsTaskManagerMaster.Tables[0].DefaultView[0]["Id"].ToString() + "' AND AuthorizationType='" + AuthorizationTypeEnum.AssignTo.ToString() + "'";
                        if (dsAuditDestination.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow drEdit = dsAuditDestination.Tables[0].DefaultView[0].Row;
                            drEdit.BeginEdit();
                            drEdit["DueDate"] = dtRefTaskMaster.Rows[i]["TempEndDate"].ToString();
                            drEdit.EndEdit();
                        }

                        DataRow drEditMaster = dsTaskManagerMaster.Tables[0].DefaultView[0].Row;
                        drEditMaster.BeginEdit();


                        if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.MasterOrder.ToString().ToUpper())
                            drEditMaster["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["MDesc"].ToString();

                        else if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.Style.ToString().ToUpper())
                            drEditMaster["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["STDesc"].ToString();

                        else if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.SalesOrder.ToString().ToUpper())
                            drEditMaster["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["SODesc"].ToString();

                        else if (dtRefTaskMaster.Rows[i]["TNAAppliedOn"].ToString().ToUpper() == TaskAppliedOnEnum.ProductionOrder.ToString().ToUpper())
                            drEditMaster["TaskDescription"] = dtRefTaskMaster.Rows[i]["TaskDescription"].ToString() + " " + dtRefTaskMaster.Rows[i]["PODesc"].ToString();


                        drEditMaster["TaskCategoryId"] = bplib.clsWebLib.RetValidLen(dtRefTaskMaster.Rows[i]["TaskCategoryId"].ToString());
                        drEditMaster["TaskSubCategoryId"] = bplib.clsWebLib.RetValidLen(dtRefTaskMaster.Rows[i]["TaskSubCategoryId"].ToString());

                        drEditMaster["UpdatedBy"] = "Scheduler";
                        drEditMaster["UpdatedDate"] = System.DateTime.Now.ToString();
                        drEditMaster["UpdatedFromIP"] = "";

                        drEditMaster.EndEdit();
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTaskManagerMaster, dsSubTasksDestination, dsAuditDestination);
            }
            catch (Exception ex)
            {


            }
        }


        public void TaskNotification(string CompanyGroupId)
        {

            try
            {
                int NotificationId = 0;
                #region Expense Booking
                try
                {
                    string strSqlCheckedBy = @"INSERT  TaskNotifications 
                                    SELECT  EB.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.ExpenseBookingCheck).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.ExpenseBookingCheck.ToString() + @"' AS NotificationName, 'Expense Booking' [TaskType], 'Checked By' [TaskUserRole],
                                    CONCAT('Please Check and Confirm the Expense Booking Id(',EB.Id,') Prepared by [',EI.EmployeeCode,' - ',EI.EmployeeName,'] amount ',ROUND(EBD.Amount,0),' ',c.Code) [TaskDesc],
                                      EB.AddedBy [TaskAssignBy],eb.ResponsiblePersonId [TaskAssignTo], CONVERT(Date,EB.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										INNER JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										INNER JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        INNER JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
										LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                     
                                WHERE EB.CompanyGroupId='" + CompanyGroupId + "' AND ISNULL(EB.ToBeCheckedRetrieve,0) = 0 AND EB.ApprovalStatus = '" + ApprovalStatus.ToBeChecked.ToString() + @"' AND ISNULL(eb.ResponsiblePersonId,'')<>''
           

							 UPDATE [TRN].[ExpenseBooking] SET ToBeCheckedRetrieve = 1  WHERE Isnull(ToBeCheckedRetrieve,0) = 0  AND CompanyGroupId='" + CompanyGroupId + "'";

                    _sqlRepository.ExecuteSqlCommand(strSqlCheckedBy);


                    NotificationId++;
                    string strSqlApprovedBy = @"INSERT INTO TaskNotifications 
                                    SELECT  EB.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.ExpenseBookingApprove).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.ExpenseBookingApprove.ToString() + @"' AS NotificationName, 'Expense Booking' [TaskType], 'Approve By' [TaskUserRole],
                                    CONCAT('Please Approve the Expense Booking Id(',EB.Id,') Prepared by [',EI.EmployeeCode,' - ',EI.EmployeeName,'] amount ',ROUND(EBD.Amount,0),' ',c.Code) [TaskDesc],
                                      EB.ResponsiblePersonId [TaskAssignBy],EIA.SystemId [TaskAssignTo], COnvert(Date,EB.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										Inner JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										Inner JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        Inner JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
										Left JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
										Left JOIN (SELECT ExpenseBookingId,SUM(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        Left JOIN (SELECT distinct ExpenseBookingId,EmployeeId,ApprovalStatus FROM TRN.ExpenseBookingApprovalHistory WHERE  ApprovalStatus= '" + ApprovalStatus.ToBeApproved.ToString() + @"' ) 
										EBAH ON EBAH.ExpenseBookingId=EB.Id AND EB.ApprovalStatus=EBAH.ApprovalStatus
										LEFT JOIN DBO.EmployeeInformation EIA on EIA.SystemId=EBAH.EmployeeId
                               
							    WHERE  EB.CompanyGroupId='" + CompanyGroupId + "' AND ISNULL(EB.ToBeApporvedRetrieve,0) = 0 AND  EB.ApprovalStatus = '" + ApprovalStatus.ToBeApproved.ToString() + @"' and  EB.ResponsiblePersonId <>''							
							
							 UPDATE [TRN].[ExpenseBooking] SET ToBeApporvedRetrieve = 1  WHERE Isnull(ToBeApporvedRetrieve,0) = 0   AND CompanyGroupId='" + CompanyGroupId + "'";
                    _sqlRepository.ExecuteSqlCommand(strSqlApprovedBy);
                }
                catch (Exception ex)
                {


                }
                #endregion

                #region Advance Booking

                try
                {
                    NotificationId++;
                    string strSqlAdvCheckedBy = @" INSERT  TaskNotifications 
	                               SELECT  EADV.SystemId [TaskId],'" + ((int)TaskNotificationTypesEnum.AdvanceBookingCheck).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.AdvanceBookingCheck.ToString() + @"' AS NotificationName, 'Advance Booking' [TaskType], 'Checked By' [TaskUserRole],
                                    CONCAT('Please Check and Confirm the Advance RequestId(',EADV.SystemId,') Prepared by [',EI.EmployeeCode,' - ',EI.EmployeeName,'] amount ',ROUND(EADV.Amount,2),' ',Curr.Code) [TaskDesc],
                                      EADV.AddedBy [TaskAssignBy],EADV.CheckedBy [TaskAssignTo], COnvert(Date,EADV.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM [TRN].EmployeeAdvanceRequisition AS EADV
										Left join EmployeeInformation EI ON EI.SystemId  = EADV.EmpSystemId
										left join scs.Currency Curr ON curr.Id = EADV.CurrencyId

										Where  EI.GroupId='" + CompanyGroupId + "' AND ISNULL(EADV.ToBeCheckedRetrieve,0) = 0 and EADV.ApprovalStatus = '" + ApprovalStatus.ToBeChecked.ToString() + @"' AND ISNULL(EADV.CheckedBy,'')<>''
										UPDATE [TRN].EmployeeAdvanceRequisition SET ToBeCheckedRetrieve = 1
										 from  [TRN].EmployeeAdvanceRequisition EADV 
										 left join EmployeeInformation EI ON EI.SystemId  = EADV.EmpSystemId  WHERE Isnull(ToBeCheckedRetrieve,0) = 0  AND EI.GroupID='" + CompanyGroupId + @"'";
                    _sqlRepository.ExecuteSqlCommand(strSqlAdvCheckedBy);

                    NotificationId++;
                    string strSqlAdvApproveddBy = @" INSERT  TaskNotifications
									SELECT  EADV.SystemId [TaskId],'" + ((int)TaskNotificationTypesEnum.AdvanceBookingApprove).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.AdvanceBookingApprove.ToString() + @"' AS NotificationName, 'Advance Booking' [TaskType], 'Approved By' [TaskUserRole],
                                    CONCAT('Please Approve  the Advance RequestId(',EADV.SystemId,') Prepared by [',EI.EmployeeCode,' - ',EI.EmployeeName,'] amount ',ROUND(EADV.Amount,0),' ',curr.Code) [TaskDesc],
                                      EADV.CheckedBy [TaskAssignBy],EADV.ApprovedBy [TaskAssignTo], COnvert(Date,EADV.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM [TRN].EmployeeAdvanceRequisition AS EADV
										
										LEFT JOIN EmployeeInformation EI ON EI.SystemId  = EADV.EmpSystemId

										LEFT JOIN scs.Currency Curr ON curr.Id = EADV.CurrencyId
										WHERE  EADV.CompanyGroupId='" + CompanyGroupId + "' AND ISNULL(EADV.ToBeApporvedRetrieve,0) = 0 and EADV.ApprovalStatus = '" + ApprovalStatus.ToBeApproved.ToString() + @"' AND ISNULL(EADV.ApprovedBy,'')<>''

										  UPDATE [TRN].EmployeeAdvanceRequisition SET ToBeApporvedRetrieve = 1
										 from  [TRN].EmployeeAdvanceRequisition EADV 
										 left join EmployeeInformation EI ON EI.SystemId  = EADV.EmpSystemId  WHERE Isnull(ToBeApporvedRetrieve,0) = 0  AND EI.GroupID='" + CompanyGroupId + @"'";
                    _sqlRepository.ExecuteSqlCommand(strSqlAdvCheckedBy);
                }
                catch (Exception ex)
                {


                }

                #endregion

                #region Material Booking
                try
                {
                    NotificationId++;
                    string strMatCheckedBy = @"INSERT  TaskNotifications
									SELECT  MRM.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.MaterialBookingCheck).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.MaterialBookingCheck.ToString() + @"' AS NotificationName, 'Material Booking' [TaskType], 'Checked By' [TaskUserRole],
                                    CONCAT('Please Check and Confirm  the Requisition RequestId(',MRM.id,') Prepared by [',EI.EmployeeCode,' - ',EI.EmployeeName,'] ') [TaskDesc],
                                      MRM.ReqEmpId [TaskAssignBy],MRM.CheckedBy [TaskAssignTo], COnvert(Date,MRM.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM [TRN].MaterialRequsitionMaster AS MRM
										LEFT JOIN EmployeeInformation EI ON EI.SystemId  = MRM.ReqEmpId									
										WHERE  MRM.CompanyGroupId='" + CompanyGroupId + @"' AND ISNULL(MRM.ToBeCheckedRetrieve,0) = 0 and MRM.CheckedByStatus = 'For Checking' AND ISNULL(MRM.CheckedBy,'')<>''


                                         UPDATE [TRN].MaterialRequsitionMaster  SET ToBeCheckedRetrieve = 1  WHERE Isnull(ToBeCheckedRetrieve,0) = 0  AND CompanyGroupId='" + CompanyGroupId + "'";

                    _sqlRepository.ExecuteSqlCommand(strMatCheckedBy);


                    NotificationId++;
                    string strMatApprovedBy = @" INSERT  TaskNotifications
									SELECT  MRM.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.MaterialBookingApprove).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.MaterialBookingApprove.ToString() + @"' AS NotificationName, 'Material Booking' [TaskType], 'Approved By' [TaskUserRole],
                                    CONCAT('Please Approved  the Requisition RequestId(',MRM.id,') Prepared by [',EI.EmployeeCode,' - ',EI.EmployeeName,'] ') [TaskDesc],
                                      MRM.ReqEmpId [TaskAssignBy],MRM.CheckedBy [TaskAssignTo], COnvert(Date,MRM.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM [TRN].MaterialRequsitionMaster AS MRM
										LEFT JOIN EmployeeInformation EI ON EI.SystemId  = MRM.ReqEmpId									
										WHERE  MRM.CompanyGroupId='" + CompanyGroupId + @"' AND ISNULL(MRM.ToBeApporvedRetrieve,0) = 0 AND ISNULL(MRM.AuthorizedBy,'')<>''
										 UPDATE [TRN].MaterialRequsitionMaster  SET ToBeApporvedRetrieve = 1  WHERE Isnull(ToBeApporvedRetrieve,0) = 0  AND CompanyGroupId='" + CompanyGroupId + "'";

                    _sqlRepository.ExecuteSqlCommand(strMatApprovedBy);
                }
                catch (Exception ex)
                {


                }
                #endregion

                #region Purchase Order 
                try
                {
                    NotificationId++;
                    string strPOCheckedBy = @"  INSERT  TaskNotifications
									SELECT  PO.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.PurchaseOrderCheck).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.PurchaseOrderCheck.ToString() + @"' AS NotificationName, 'Purchase Order' [TaskType], 'Checked By' [TaskUserRole],
                                    CONCAT('Please Check and Confirm  the Purchase Order RequestId(',PO.id,') Prepared by [',PO.AddedBy,'] ') [TaskDesc],
                                        null [TaskAssignBy],PO.AuthorizedBy [TaskAssignTo], COnvert(Date,PO.AddedDate) TaskCreatedDate,0 as isRead,getdate(),PO.CompanyGroupId
										FROM TRN.PurchaseOrder AS PO
											Left Join SEC.[User] USR ON USR.UserId = PO.AddedBy 
										--Left join EmployeeInformation EI ON EI.SystemId  = USR.EmployeeId									
										Where  PO.CompanyGroupId='" + CompanyGroupId + @"' AND ISNULL(PO.ToBeCheckedRetrieve,0) = 0 and PO.CheckedByStatus = 'For Checking' AND ISNULL(PO.CheckedBy,'')<>''

										 UPDATE [TRN].PurchaseOrder  SET ToBeCheckedRetrieve = 1  WHERE Isnull(ToBeCheckedRetrieve,0) = 0 AND CompanyGroupId='" + CompanyGroupId + "'";
                    _sqlRepository.ExecuteSqlCommand(strPOCheckedBy);
                    NotificationId++;
                    string strPOApprovedBy = @" INSERT  TaskNotifications
									SELECT  PO.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.PurchaseOrderApprove).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.PurchaseOrderApprove.ToString() + @"' AS NotificationName, 'Purchase Order' [TaskType], 'Approved By' [TaskUserRole],
                                    CONCAT('Please Approved  the Purchase Order RequestId(',PO.id,') Prepared by [',PO.AddedBy,'] ') [TaskDesc],                                    
                                   null  [TaskAssignBy],PO.AuthorizedBy [TaskAssignTo], COnvert(Date,PO.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM TRN.PurchaseOrder AS PO
											Left Join SEC.[User] USR ON USR.UserId = PO.AddedBy 
										Left join EmployeeInformation EI ON EI.SystemId  = USR.EmployeeId								
										WHERE  PO.CompanyGroupId='" + CompanyGroupId + @"' AND ISNULL(PO.ToBeApporvedRetrieve,0) = 0 and ISNULL(PO.AuthorizedBy,'')<>''

										 UPDATE [TRN].PurchaseOrder  SET ToBeApporvedRetrieve = 1  WHERE Isnull(ToBeApporvedRetrieve,0) = 0 AND CompanyGroupId='" + CompanyGroupId + "'";

                    _sqlRepository.ExecuteSqlCommand(strPOApprovedBy);
                }
                catch (Exception ex)
                {


                }
                #endregion

                #region Goods Receive Notes
                try
                {
                    NotificationId++;
                    string strGRNCheckedBy = @"	 INSERT  TaskNotifications
									SELECT  GRN.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.GoodsReceiveNotesCheck).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.GoodsReceiveNotesCheck.ToString() + @"' AS NotificationName, 'Goods Receive Notes' [TaskType], 'Checked By' [TaskUserRole],
                                    CONCAT('Please Check and Confirm  the Purchase Order RequestId(',GRN.id,') Prepared by [',GRN.AddedBy,'] ') [TaskDesc],
                                     null	 [TaskAssignBy],GRN.CheckedBy [TaskAssignTo], COnvert(Date,GRN.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM TRN.InventoryReceive AS GRN
										Left Join SEC.[User] USR ON USR.UserId = GRN.AddedBy 								

										Left join EmployeeInformation EI ON EI.SystemId  = USR.EmployeeId								
										Where  GRN.CompanyGroupId='" + CompanyGroupId + @"' AND ISNULL(GRN.ToBeCheckedRetrieve,0) = 0 and GRN.CheckedByStatus = 'For Checking' AND ISNULL(GRN.CheckedBy,'')<>''

										 UPDATE TRN.InventoryReceive  SET ToBeCheckedRetrieve = 1  WHERE Isnull(ToBeCheckedRetrieve,0) = 0 AND CompanyGroupId='" + CompanyGroupId + "'";
                    _sqlRepository.ExecuteSqlCommand(strGRNCheckedBy);

                    NotificationId++;
                    string strGRNApprovedBy = @" INSERT  TaskNotifications
									SELECT  GRN.Id [TaskId],'" + ((int)TaskNotificationTypesEnum.GoodsReceiveNotesApprove).ToString() + @"' AS NotificationId,'" + TaskNotificationTypesEnum.GoodsReceiveNotesApprove.ToString() + @"' AS NotificationName,
                                    'Goods Receive Notes' [TaskType], 'Approved By' [TaskUserRole],
                                    CONCAT('Please Approved  the Purchase Order RequestId(',GRN.id,') Prepared by [',GRN.AddedBy,'] ') [TaskDesc],
                                   null [TaskAssignBy],GRN.AuthorizedBy [TaskAssignTo], COnvert(Date,GRN.AddedDate) TaskCreatedDate,0 as isRead,getdate(),ei.GroupID
										FROM TRN.InventoryReceive AS GRN
											Left Join SEC.[User] USR ON USR.UserId = GRN.AddedBy 
										Left join EmployeeInformation EI ON EI.SystemId  = USR.EmployeeId									
										WHERE  GRN.CompanyGroupId='" + CompanyGroupId + @"' AND ISNULL(GRN.ToBeApporvedRetrieve,0) = 0 and ISNULL(GRN.AuthorizedBy,'')<>''
										 UPDATE TRN.InventoryReceive  SET ToBeApporvedRetrieve = 1  WHERE Isnull(ToBeApporvedRetrieve,0) = 0 AND CompanyGroupId='" + CompanyGroupId + "'";

                    _sqlRepository.ExecuteSqlCommand(strGRNApprovedBy);
                }
                catch (Exception ex)
                {


                }
                #endregion



                #region Tasks

                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                //assigned to
                string sql = @"INSERT TaskNotifications
                                SELECT TMM.Id," + ((int)TaskNotificationTypesEnum.ToDoAssignTo).ToString() + @" AS NotificationId,
                                '" + TaskNotificationTypesEnum.ToDoAssignTo.ToString() + @"' AS NotificationName,tmm.TaskType,'Assignment',
                                concat(tmm.TaskDescription,' Assigned By '+eita.EmployeeName),ta.ResponsiblePersonId,tao.ResponsiblePersonId,
                                tmm.AddedDate,0 AS IsRead,GETDATE() AS AddedDate,eita.GroupID
                                 FROM TaskManagerMaster AS tmm
                                LEFT OUTER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN EmployeeInformation AS eita ON eita.SystemId=ta.ResponsiblePersonId
                                LEFT OUTER JOIN TaskAudit AS tao ON tao.TaskManagerMasterId=tmm.Id AND tao.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS eitao ON eitao.SystemId=tao.ResponsiblePersonId
                                WHERE eita.GroupID='" + CompanyGroupId + @"' AND tmm.TaskTypeGroup='ToDo' AND  ta.ResponsiblePersonId<>tao.ResponsiblePersonId AND isnull(tao.TakenForNotification,0)=0";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");

                sql = @"INSERT TaskNotifications
                                SELECT TMM.Id," + ((int)TaskNotificationTypesEnum.TNAAssignTo).ToString() + @" AS NotificationId,
                                '" + TaskNotificationTypesEnum.TNAAssignTo.ToString() + @"' AS NotificationName,tmm.TaskType,'TNA Assignment',
                                concat(tmm.TaskDescription,' Assigned By '+eita.EmployeeName),ta.ResponsiblePersonId,tao.ResponsiblePersonId,
                                tmm.AddedDate,0 AS IsRead,GETDATE() AS AddedDate,eita.GroupID
                                 FROM TaskManagerMaster AS tmm
                                LEFT OUTER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN EmployeeInformation AS eita ON eita.SystemId=ta.ResponsiblePersonId
                                LEFT OUTER JOIN TaskAudit AS tao ON tao.TaskManagerMasterId=tmm.Id AND tao.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS eitao ON eitao.SystemId=tao.ResponsiblePersonId
                                WHERE eita.GroupID='" + CompanyGroupId + @"' AND tmm.TaskTypeGroup='TNA' AND  ta.ResponsiblePersonId<>tao.ResponsiblePersonId AND isnull(tao.TakenForNotification,0)=0";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");

                sql = @"INSERT TaskNotifications
                                SELECT TMM.Id," + ((int)TaskNotificationTypesEnum.Issue).ToString() + @" AS NotificationId,
                                '" + TaskNotificationTypesEnum.Issue.ToString() + @"' AS NotificationName,tmm.TaskType,'Issue Assignment',
                                concat(tmm.TaskDescription,' Assigned By '+eita.EmployeeName),ta.ResponsiblePersonId,tao.ResponsiblePersonId,
                                tmm.AddedDate,0 AS IsRead,GETDATE() AS AddedDate,eita.GroupID
                                 FROM TaskManagerMaster AS tmm
                                LEFT OUTER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN EmployeeInformation AS eita ON eita.SystemId=ta.ResponsiblePersonId
                                LEFT OUTER JOIN TaskAudit AS tao ON tao.TaskManagerMasterId=tmm.Id AND tao.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS eitao ON eitao.SystemId=tao.ResponsiblePersonId
                                WHERE eita.GroupID='" + CompanyGroupId + @"' AND tmm.TaskTypeGroup='Issue' AND  ta.ResponsiblePersonId<>tao.ResponsiblePersonId AND isnull(tao.TakenForNotification,0)=0";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");


                //check by
                sql = @"INSERT TaskNotifications
                                SELECT TMM.Id," + ((int)TaskNotificationTypesEnum.ToDoToCheck).ToString() + @" AS NotificationId,
                                '" + TaskNotificationTypesEnum.ToDoToCheck.ToString() + @"' AS NotificationName,tmm.TaskType,'Check',
                                concat(tmm.TaskDescription,' Assigned To '+eitao.EmployeeName),tao.ResponsiblePersonId,tax.ResponsiblePersonId,
                                tmm.AddedDate,0 AS IsRead,GETDATE() AS AddedDate,eita.GroupID
                                 FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN EmployeeInformation AS eita ON eita.SystemId=ta.ResponsiblePersonId

                                INNER JOIN TaskAudit AS tao ON tao.TaskManagerMasterId=tmm.Id AND tao.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS eitao ON eitao.SystemId=tao.ResponsiblePersonId

                                INNER JOIN TaskAudit AS tax ON tax.TaskManagerMasterId=tmm.Id AND tax.AuthorizationType='CheckBy'
                                INNER JOIN EmployeeInformation AS eitx ON eitx.SystemId=tax.ResponsiblePersonId

                                WHERE eita.GroupID='" + CompanyGroupId + @"' AND tmm.TaskTypeGroup='ToDo' AND  ta.ResponsiblePersonId<>tax.ResponsiblePersonId AND isnull(tax.TakenForNotification,0)=0";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");

                //cross check by
                sql = @"INSERT TaskNotifications
                                SELECT TMM.Id," + ((int)TaskNotificationTypesEnum.ToDoToCrossCheck).ToString() + @" AS NotificationId,
                                '" + TaskNotificationTypesEnum.ToDoToCrossCheck.ToString() + @"' AS NotificationName,tmm.TaskType,'Cross Check',
                                concat(tmm.TaskDescription,' Assigned To '+eitao.EmployeeName),tao.ResponsiblePersonId,tax.ResponsiblePersonId,
                                tmm.AddedDate,0 AS IsRead,GETDATE() AS AddedDate,eita.GroupID
                                 FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN EmployeeInformation AS eita ON eita.SystemId=ta.ResponsiblePersonId

                                INNER JOIN TaskAudit AS tao ON tao.TaskManagerMasterId=tmm.Id AND tao.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS eitao ON eitao.SystemId=tao.ResponsiblePersonId

                                INNER JOIN TaskAudit AS tax ON tax.TaskManagerMasterId=tmm.Id AND tax.AuthorizationType='CrossCheckBy'
                                INNER JOIN EmployeeInformation AS eitx ON eitx.SystemId=tax.ResponsiblePersonId

                                WHERE eita.GroupID='" + CompanyGroupId + @"' AND tmm.TaskTypeGroup='ToDo' AND  ta.ResponsiblePersonId<>tax.ResponsiblePersonId AND isnull(tax.TakenForNotification,0)=0";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");

                //cross check by
                sql = @"INSERT TaskNotifications
                                SELECT TMM.Id," + ((int)TaskNotificationTypesEnum.ToDoToApprove).ToString() + @" AS NotificationId,
                                '" + TaskNotificationTypesEnum.ToDoToApprove.ToString() + @"' AS NotificationName,tmm.TaskType,'Approve',
                                concat(tmm.TaskDescription,' Assigned To '+eitao.EmployeeName),tao.ResponsiblePersonId,tax.ResponsiblePersonId,
                                tmm.AddedDate,0 AS IsRead,GETDATE() AS AddedDate,eita.GroupID
                                 FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN EmployeeInformation AS eita ON eita.SystemId=ta.ResponsiblePersonId

                                INNER JOIN TaskAudit AS tao ON tao.TaskManagerMasterId=tmm.Id AND tao.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS eitao ON eitao.SystemId=tao.ResponsiblePersonId

                                INNER JOIN TaskAudit AS tax ON tax.TaskManagerMasterId=tmm.Id AND tax.AuthorizationType='ApproveBy'
                                INNER JOIN EmployeeInformation AS eitx ON eitx.SystemId=tax.ResponsiblePersonId

                                WHERE eita.GroupID='" + CompanyGroupId + @"' AND tmm.TaskTypeGroup='ToDo' AND  ta.ResponsiblePersonId<>tax.ResponsiblePersonId AND isnull(tax.TakenForNotification,0)=0";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");


                //To Review
                sql = @"INSERT TaskNotifications
                                SELECT TMM.Id," + ((int)TaskNotificationTypesEnum.ToReview).ToString() + @" AS NotificationId,
                                '" + TaskNotificationTypesEnum.ToReview.ToString() + @"' AS NotificationName,tmm.TaskType,'Review',
                                concat(tmm.TaskDescription,' Waiting for Approval; Assigned to '+eitao.EmployeeName),ta.ResponsiblePersonId,ta.ResponsiblePersonId,
                                tmm.AddedDate,0 AS IsRead,GETDATE() AS AddedDate,eita.GroupID
                                 FROM TaskManagerMaster AS tmm
                                LEFT OUTER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN EmployeeInformation AS eita ON eita.SystemId=ta.ResponsiblePersonId
                                LEFT OUTER JOIN TaskAudit AS tao ON tao.TaskManagerMasterId=tmm.Id AND tao.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS eitao ON eitao.SystemId=tao.ResponsiblePersonId
                                WHERE eita.GroupID='" + CompanyGroupId + @"' AND tmm.CurrentStatus='ToClose' AND  ta.ResponsiblePersonId<>tao.ResponsiblePersonId AND isnull(tmm.TakenForNotification,0)=0";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");


                objCon.ExecuteNonQueryWrapper(@"Update TaskAudit set TakenForNotification=1 FROM 
                                                TaskAudit AS ta
                                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=ta.ResponsiblePersonId
                                                WHERE ei.GroupID='" + CompanyGroupId + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"Update TaskManagerMaster set TakenForNotification=1 
                                                FROM TaskManagerMaster MM
                                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=mm.UpdatedBy
                                                where CurrentStatus='ToClose' AND isnull(TakenForNotification,0)=0 AND  
                                                ei.GroupID='" + CompanyGroupId + @"'", true, "1");

                objCon.CommitTransaction();

                #endregion


                clsMobileNotification.SendData(CompanyGroupId);
            }
            catch (Exception ex)
            {


            }
        }
    }
    public static class clsMobileNotification
    {
        /*
         this library has been brutally implemented by: tarek talukder, tarektalukder@gmail.com
        For enquiry, no reason to contact
        write your own code plz
         */

        public static string hubProxyName = "aplosbroadcasthub";
        public static IHubProxy HubProxy;
        public static HubConnection hubConnection;
        private static string HubAddress = "";
        public static void createProxy(string CompanyGroupId)
        {
            try
            {
                //TODO: Will implement dynamic address from DB; Company Group Wise
                if (string.IsNullOrEmpty(HubAddress))
                    HubAddress = GetNotificationLink(CompanyGroupId);


                if (HubProxy != null)
                    return;

                hubConnection = new HubConnection(HubAddress, new Dictionary<string, string> {
                    { "UserToken", "MobileNotification" }
                });
                hubConnection.StateChanged += HubConnection_StateChanged;
                HubProxy = hubConnection.CreateHubProxy(hubProxyName);

            }
            catch (Exception ex)
            {

            }

        }
        public static async Task createConnection()
        {
            try
            {
                if (hubConnection.State != Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                    await hubConnection.Start();
               
            }
            catch (Exception ex)
            {

            }

        }

        private static void HubConnection_StateChanged(StateChange obj)
        {
            if (obj.OldState != obj.NewState)
            {
                if (obj.NewState == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                {
                    try
                    {
                        var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, "Ready");
                    }
                    catch (Exception ex)
                    {

                      
                    }
                }
            }
        }

        public static string GetNotificationLink(string CompanyGroupId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.getDataSet("select * from [NotificationURL] where [CompanyGroupId]='" + CompanyGroupId + "'", out DataSet dsLocal);
                con.CommitTransaction();
                if (dsLocal.Tables[0].Rows.Count > 0)
                    return dsLocal.Tables[0].Rows[0]["URL"].ToString();
            }
            catch (Exception)
            {


            }
            return "about:blank";
        }
        public static async void SendData(string CompanyGroupId)
        {
            try
            {
                createProxy(CompanyGroupId);
                await createConnection();

                try
                {
                    string sql = @"SELECT DISTINCT TaskAssignTo FROM TaskNotifications WHERE isnull(isRead,0)=0 AND CompanyGroupId='" + CompanyGroupId + "'";
                    ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager();
                    con.getDataSet(sql, out DataSet dsLocal);

                    for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                    {
                        try
                        {
                            await HubProxy.Invoke("SendTaskNotification", new object[] { dsLocal.Tables[0].Rows[i]["TaskAssignTo"].ToString() }); ;
                        }
                        catch (Exception)
                        {
                        }

                    }

                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception ex)
            {

            }

        }
        public static async void SendMessage(string CompanyGroupId, string PlantId, string UserId, string Message)
        {
            try
            {
                createProxy(CompanyGroupId);
                await createConnection();


                try
                {
                    await HubProxy.Invoke("SendProgressPercentage", new object[] { PlantId + UserId, Message });
                }
                catch (Exception ex)
                {

                }


            }
            catch (Exception ex)
            {

            }

        }
        public static bool isLogOut = false;
    }
}