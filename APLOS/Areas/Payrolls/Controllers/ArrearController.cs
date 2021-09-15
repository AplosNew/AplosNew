using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Arrear;
using System.Threading.Tasks;
using Library.Service.TaskScheduler;
using Library.Service.Payrolls.SalaryProcessActive;
using Library.Service.Payrolls.SalaryProcess;
using Library.HumanResource.Payroll.SalaryProcess;
using Library.HumanResource.Payroll.Allowance;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class ArrearController : BaseController
    {


        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public string PlantId { get; private set; }

        public ArrearController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
            //return await Task.Factory.StartNew(() =>
            //{
            //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //    clsMobileNotification.SendData(identity.CompanyGroupId);

            //});
        }

        private void GetDateFromMonth(ref string FromDate, ref string ToDate)
        {
            try
            {
                if (string.IsNullOrEmpty(FromDate))
                    throw new Exception("Invalid from date");

                if (string.IsNullOrEmpty(ToDate))
                    throw new Exception("Invalid to date");



                DateTime dtFromDateTemp = Convert.ToDateTime(FromDate);
                FromDate = new DateTime(dtFromDateTemp.Year, dtFromDateTemp.Month, 1).ToString("dd-MMM-yyyy");

                DateTime dtToDateTemp = Convert.ToDateTime(ToDate);
                ToDate = new DateTime(dtToDateTemp.Year, dtToDateTemp.Month, DateTime.DaysInMonth(dtToDateTemp.Year, dtToDateTemp.Month)).ToString("dd-MMM-yyyy");


                if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                    throw new Exception("To date is earlier than from date");

            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetEmpList(string FromDate, string ToDate)
        {

            try
            {
                GetDateFromMonth(ref FromDate, ref ToDate);
                string sql = string.Empty;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ArrearProcess obj = new ArrearProcess();

                JsonResult json = Json(obj.GetEmployee(FromDate, ToDate, identity.PlantId));
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }

        bool requestCancelled = false;
        private async void OnAbortSalaryProcess(CancellationTokenSource source)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {


                    while (!requestCancelled)
                    {
                        if (source.IsCancellationRequested)
                        {
                            try
                            {
                                requestCancelled = true;
                                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                                Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.SalaryProcess, "", 300);
                                _lock.UnlockProcess();
                                source.Token.ThrowIfCancellationRequested();
                                //throw new Exception();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                }
            });

        }
        private void SendNotification(string Message)
        {
            try
            {
                var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, Message);

            }
            catch (Exception ex)
            {

            }

        }

        private void ValidateAndClearArrear(string ArrearDesc, string ArrearFromDate, string ArrearToDate, string AllEmployees)
        {
            try
            {


                DataSet dsLocal;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

                string sql = @"Select top 1 * FROM ArrearSummaryBatchWise WHERE isnull(IsApproved,0)=1 AND EmployeeSystemId IN (" + AllEmployees + @") AND ArrearProcessBatchId IN (SELECT ArrearProcessBatchId FROM ArrearProcMaster WHERE ('" + ArrearFromDate + @"' BETWEEN FromDate AND ToDate) OR  ('" + ArrearToDate + "' BETWEEN FromDate AND ToDate)OR  (FromDate BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "') OR  (ToDate  BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "'))";
                con.getDataSet(sql, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                    throw new Exception(string.Format("Arrear has been approved between the period from {0} and {1} for the selected employee(s), please unapprove the arrear then reprocess", Convert.ToDateTime(ArrearFromDate).ToString("MMMM yyyy"), Convert.ToDateTime(ArrearToDate).ToString("MMMM yyyy")));


                con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery(@"DELETE FROM ArrearSummaryBatchWise WHERE EmployeeSystemId IN (" + AllEmployees + @") AND ArrearProcessBatchId IN (SELECT ArrearProcessBatchId FROM ArrearProcMaster WHERE ('" + ArrearFromDate + @"' BETWEEN FromDate AND ToDate) OR  ('" + ArrearToDate + "' BETWEEN FromDate AND ToDate)OR  (FromDate BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "') OR  (ToDate  BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "'))");
                con.executeQuery(@"DELETE FROM ArrearSummaryMonthWise WHERE EmployeeSystemId IN (" + AllEmployees + @") AND ArrearProcessBatchId IN (SELECT ArrearProcessBatchId FROM ArrearProcMaster WHERE ('" + ArrearFromDate + @"' BETWEEN FromDate AND ToDate) OR  ('" + ArrearToDate + "' BETWEEN FromDate AND ToDate)OR  (FromDate BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "') OR  (ToDate  BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "'))");
                con.executeQuery(@"DELETE FROM ArrearProcChild WHERE EmpInfoSystemID IN (" + AllEmployees + @") AND SlrProcMstSystemID IN (SELECT SystemID FROM ArrearProcMaster WHERE ('" + ArrearFromDate + @"' BETWEEN FromDate AND ToDate) OR  ('" + ArrearToDate + "' BETWEEN FromDate AND ToDate)OR  (FromDate BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "') OR  (ToDate  BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "'))");
                con.executeQuery(@"DELETE FROM ArrearProcMaster WHERE SystemID IN (
                                    SELECT APM.SystemID FROM ArrearProcMaster AS apm
                                    LEFT JOIN ArrearProcChild AS apc ON apm.SystemID=apc.SlrProcMstSystemID AND apc.SystemID=(SELECT TOP 1 SystemId FROM ArrearProcChild AS apc2 WHERE apc2.SlrProcMstSystemID=apm.SystemID)
                                    WHERE ISNULL(apc.SystemID,'')=''
                                    )");

                con.executeQuery(@"DELETE FROM ArrearProcessBatch WHERE Id IN (
                                    SELECT APM.Id FROM ArrearProcessBatch AS apm
                                    LEFT JOIN ArrearProcMaster AS apc ON apm.Id=apc.ArrearProcessBatchId 
                                    AND apc.SystemID=(SELECT TOP 1 apc2.SystemID FROM ArrearProcMaster AS apc2 WHERE apc2.ArrearProcessBatchId=apm.Id)
                                    WHERE ISNULL(apc.SystemID,'')=''
                                    )");

                //con.executeQuery(@"DELETE FROM ArrearProcMaster WHERE ('" + ArrearFromDate + @"' BETWEEN FromDate AND ToDate) OR  ('" + ArrearToDate + "' BETWEEN FromDate AND ToDate) OR  (FromDate BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "') OR  (ToDate  BETWEEN '" + ArrearFromDate + @"' AND '" + ArrearToDate + "')");

                con.CommitTransaction();

                con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from ArrearProcessBatch M where M.ArrearDesc='" + ArrearDesc.Trim() + @"'", out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                    throw new Exception("Same description has been used in another arrear process. Please change your description");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        private void CreateArrearBatch(string BatchId, string Description, string FromDate, string ToDate)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from ArrearProcessBatch M where 1=2", out DataSet dsLocal);
                DataRow dr = dsLocal.Tables[0].NewRow();

                dr["Id"] = bplib.clsWebLib.RetValidLen(BatchId);
                dr["ArrearDesc"] = Description;
                dr["ArrearFromDate"] = FromDate;
                dr["ArrearToDate"] = ToDate;

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;


                dsLocal.Tables[0].Rows.Add(dr);


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsLocal);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        [HttpPost, Authorize]
        public async Task<JsonResult> ProcessAll(string FromDate, string ToDate, string pDescription, AllDataset palldataset)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.ArrearProcess, "", 60);
            _lock.LockProcess();
            try
            {
                return await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        GetDateFromMonth(ref FromDate, ref ToDate);

                        var alldataset = palldataset;
                        string _active_emps = string.Empty;
                        string _all_emps = string.Empty;
                        GetEmpDelimitedActiveAndNewlyJoined(alldataset.dtActive, alldataset.dtNewlyJoined, out _active_emps);
                        GetEmpDelimited(alldataset.dtPresetZero, ref _active_emps);
                        _all_emps = _active_emps;

                        List<Tuple<string, string>> MonthList = new List<Tuple<string, string>>();
                        //construct fromtodates for each month
                        //16-Jan-2020 to 23-May-2020

                        string ArrearFromDate = FromDate;
                        string ArrearToDate = ToDate;

                        ValidateAndClearArrear(pDescription, ArrearFromDate, ArrearToDate, _all_emps);

                        //there will be a loop for months and years
                        string ProcessMonths = "''";
                        do
                        {
                            DateTime dtFromDateTemp = Convert.ToDateTime(FromDate);
                            MonthList.Add(Tuple.Create(
                                new DateTime(dtFromDateTemp.Year, dtFromDateTemp.Month, 1).ToString("dd-MMM-yyyy"),
                                new DateTime(dtFromDateTemp.Year, dtFromDateTemp.Month, DateTime.DaysInMonth(dtFromDateTemp.Year, dtFromDateTemp.Month)).ToString("dd-MMM-yyyy")));

                            ProcessMonths += dtFromDateTemp.Year.ToString() + dtFromDateTemp.Month.ToString();

                            dtFromDateTemp = new DateTime(dtFromDateTemp.Year, dtFromDateTemp.Month, 1);
                            FromDate = dtFromDateTemp.AddMonths(1).ToString("dd-MMM-yyyy");



                        } while (Convert.ToDateTime(FromDate) < Convert.ToDateTime(ToDate));



                        #region MLV RETURN
                        SendNotification("MLV Return");

                        GetEmpDelimitedMLVR(alldataset.dtMaternityReturn, ref _all_emps);
                        #endregion
                        #region SALARY LOCK Prev Month
                        SendNotification("Validating Salary Lock for selected date range");

                        clsSalaryProcessUI objel = new clsSalaryProcessUI();
                        for (int i = 0; i < MonthList.Count; i++)
                        {
                            try
                            {
                                string _yearnoP = Convert.ToDateTime(MonthList[i].Item1).ToString("yyyy");
                                string _monthnoP = Convert.ToDateTime(MonthList[i].Item1).ToString("MM");
                                objel.ValidationSalaryLockForArrear(_all_emps, _yearnoP, _monthnoP);
                            }
                            catch (Exception ex)
                            {
                                string _errorMessage = "Process was interrupted for the month " + Convert.ToDateTime(MonthList[i].Item1).ToString("MMM") + "/" + Convert.ToDateTime(MonthList[i].Item1).ToString("yyyy");
                                _errorMessage += ". Reason [" + ex.Message + @"]";
                                throw new Exception(_errorMessage);
                            }

                        }

                        #endregion


                        string BatchNo = System.DateTime.Now.Ticks.ToString();
                        CreateArrearBatch(BatchNo, pDescription, ArrearFromDate, ArrearToDate);

                        for (int i = 0; i < MonthList.Count; i++)
                        {
                            FromDate = MonthList[i].Item1;
                            ToDate = MonthList[i].Item2;

                            palldataset.FromDate = FromDate;
                            palldataset.ToDate = ToDate;
                            palldataset.PlantId = identity.PlantId;


                            clsSalaryProcessQuery objQ = null;
                            DataSet dsGrid = null;
                            string _currencyId = string.Empty;

                            var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                            SendNotification("Starting arrear Process");

                            try
                            {
                                //OnAbortSalaryProcess(source);

                                alldataset = palldataset;

                                objQ = new clsSalaryProcessQuery();

                                #region Validation


                                DateValidation(FromDate, ToDate);
                                if (pDescription.Trim().Length == 0)
                                {
                                    throw new Exception("'Description' can not be blank...");
                                }
                                #endregion
                                SendNotification("Validating Salary Structure");
                                //clsSalaryProcessUI objel = new clsSalaryProcessUI();
                                //objel.SSValida(identity.PlantId, FromDate, ToDate);

                                SendNotification("Getting Currency");
                                GetCurrency(identity.CompanyGroupId, identity.PlantId, out _currencyId);

                                SendNotification("Getting Employee List");

                                #region Active and other
                                //string _active_emps = string.Empty;
                                //string _all_emps = string.Empty;
                                //GetEmpDelimitedActiveAndNewlyJoined(alldataset.dtActive, alldataset.dtNewlyJoined, out _active_emps);
                                //GetEmpDelimited(alldataset.dtPresetZero, ref _active_emps);
                                //_all_emps = _active_emps;

                                //#region MLV RETURN
                                //SendNotification("MLV Return");

                                //GetEmpDelimitedMLVR(alldataset.dtMaternityReturn, ref _all_emps);
                                //#endregion

                                #region SALARY LOCK Current Month
                                //clsSalaryProcessUI objel = new clsSalaryProcessUI();
                                SendNotification("Validating Salary Lock");

                                string _yearno = Convert.ToDateTime(FromDate).ToString("yyyy");
                                string _monthno = Convert.ToDateTime(FromDate).ToString("MM");
                                //objel.ValidationSalaryLock(_all_emps, _yearno, _monthno);

                                clsSalaryProcessUI _lockCheck = new clsSalaryProcessUI();
                                _lockCheck.GetSalaryProcessedUnLockedEmp(_all_emps, _yearno, _monthno, out DataSet dsSalaryLocked);
                                if (dsSalaryLocked.Tables[0].Rows.Count > 0)
                                {
                                    string UnlockedSalaryEmployeeCode = "";
                                    foreach (DataRow item in dsSalaryLocked.Tables[0].Rows)
                                    {
                                        if (UnlockedSalaryEmployeeCode == "")
                                            UnlockedSalaryEmployeeCode = item["EmployeeCode"].ToString();
                                        else
                                            UnlockedSalaryEmployeeCode += "," + item["EmployeeCode"].ToString();

                                    }
                                    throw new Exception("Following employee(s) have salary unlocked for the month " + _monthno + "/" + _yearno + " " + UnlockedSalaryEmployeeCode);
                                }
                                #endregion

                                //#region SALARY LOCK Prev Month
                                //SendNotification("Validating Salary Lock for Previous Month");

                                ////clsSalaryProcessUI objel = new clsSalaryProcessUI();
                                ////string dtFD = Convert.ToDateTime(FromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                                ////string _yearnoP = Convert.ToDateTime(dtFD).ToString("yyyy");
                                ////string _monthnoP = Convert.ToDateTime(dtFD).ToString("MM");
                                ////objel.ValidationSalaryLockPreviousMonth(_all_emps, _yearnoP, _monthnoP);
                                //#endregion


                                //SendNotification("Validating Bank Accounts");
                                //ValidationBank(_all_emps, identity.PlantId);

                                SendNotification("Validating Attendance Lock");
                                ValidationAttendance(_all_emps, identity.PlantId, FromDate, ToDate);

                                SendNotification("Fetching Employee List");
                                objQ.GetEmpListAllForArrear(_active_emps, FromDate, ToDate, identity.PlantId, out dsGrid);


                                //string _spid = string.Empty;
                                FunctionPara para = new FunctionPara();
                                para.FromDate = FromDate;
                                para.ToDate = ToDate;
                                para.PlantId = identity.PlantId;
                                para.GroupId = identity.CompanyGroupId;
                                para.txtDescription = pDescription;
                                para.USER = identity.Name;
                                para.dsGrid = dsGrid;
                                //get ds
                                #endregion

                                ProcessMain(para, BatchNo, ArrearFromDate, ArrearToDate, _currencyId, alldataset);//pass ds


                                #region MLVR
                                string _mlvr_emps = string.Empty;
                                GetEmpDelimitedMLVR(alldataset.dtMaternityReturn, ref _mlvr_emps);

                                if (_mlvr_emps.Length > 0)
                                {
                                    SendNotification("Checking MLV Return");
                                    objQ.GetEmpListAllMLVReturn(_mlvr_emps, FromDate, ToDate, identity.PlantId, out dsGrid);

                                    FunctionPara para2 = new FunctionPara();
                                    para2.FromDate = FromDate;
                                    para2.ToDate = ToDate;
                                    para2.PlantId = identity.PlantId;
                                    para2.GroupId = identity.CompanyGroupId;
                                    para2.txtDescription = pDescription;
                                    para2.USER = identity.Name;
                                    para2.dsGrid = dsGrid;

                                    SendNotification("Processing MLV");
                                    ProcessMLV(para2, BatchNo, ArrearFromDate, ArrearToDate, _mlvr_emps, _currencyId, alldataset);
                                }
                                #endregion


                                SendNotification("Status: Process Completed");
                                requestCancelled = true;

                            }
                            catch (Exception ex)
                            {

                                SendNotification(ex.Message);
                                throw ex;
                                //requestCancelled = true;
                                //_lock.UnlockProcess();
                                //return Json(new { Error = true, Message = ex.Message });
                            }
                        }

                        FinalizingProcessUpdateArrearSummary(BatchNo);

                        _lock.UnlockProcess();
                        SendNotification("Status: Process Completed");
                        JsonResult json = Json(new { Error = false, Message = AplosMessage.Success });
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
                    catch (Exception ex)
                    {
                        SendNotification(ex.Message);
                        requestCancelled = true;
                        _lock.UnlockProcess();
                        return Json(new { Error = true, Message = ex.Message });
                    }
                    //JsonResult jsontemp = Json(new { Error = false, Message = AplosMessage.Success });
                    //jsontemp.MaxJsonLength = int.MaxValue;
                    //return jsontemp;
                });
            }
            catch (Exception ex)
            {
                SendNotification(ex.Message);
                requestCancelled = true;
                _lock.UnlockProcess();
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        private void FinalizingProcessUpdateArrearSummary(string BatchNo)
        {
            try
            {
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();

                connection.BeginTransaction();

                connection.executeQuery(@"INSERT INTO ArrearSummaryBatchWise(
                                     ArrearProcessBatchId,EmployeeSystemId,Diff,AddedBy,DateAdded,UpdatedBy,DateUpdated
                                )

                                    SELECT am.ArrearProcessBatchId,ei.SystemId,
                                    SUM(AC.Diff) Diff,'ArrearProcess',GETDATE(),'ArrearProcess',GETDATE()


                                    FROM ArrearProcMaster AS AM
                                    JOIN ArrearProcChild AS AC ON am.SystemID=ac.SlrProcMstSystemID
                                  
                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=ac.EmpInfoSystemID
                                    LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID=ac.SalaryHeadID

                                    WHERE sh.HeadCategory='Net Payable' AND AM.ArrearProcessBatchId='" + BatchNo + @"'
                                    GROUP BY am.ArrearProcessBatchId,ei.SystemId");

                connection.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw (ex);
            }


        }
        #region SP related func

        void ValidationAttendance(string emplist, string plantid, string fromdate, string todate)
        {
            clsEmployeeLoad objel = null;
            try
            {
                objel = new clsEmployeeLoad();
                DataSet dsAttInfo;
                objel.GetAttendanceLockInfo(plantid, fromdate, todate, emplist, out dsAttInfo);
                string r = GetAttendanceTobelocked(dsAttInfo);
                if (r.Length > 0)
                {
                    throw new Exception(r);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetAttendanceTobelocked(DataSet ds)
        {
            string r = string.Empty;
            try
            {
                DataTable dt = new DataView(ds.Tables[0]).ToTable(true, "EmployeeCode");
                //DataTable dt = new DataView(ds.Tables[0]).ToTable(true, "workdate");
                //DataTable dtTab = new DataView(ds.Tables[0]).ToTable(true, "SystemId", "EmployeeCode", "EmployeeName", "DOJ", "DOS", "GivenDesignation", "LegalDesignation", "EmployeeStatus", "Subsection", "Section");
                // DGAttendanceNotLocked.DataSource = dtTab;
                // DGAttendanceNotLocked.DataBind();
                // tabAttNotLocked.Text = "Att. Not processed (" + dtTab.Rows.Count + ")";

                //int cc = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //cc++;
                    //if (cc < 10)
                    //{
                    if (r.Length == 0)
                    {
                        r = "Attendance is not locked (individual) for the following Employees:-" + Environment.NewLine;
                        r += " Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }
                    else
                    {
                        r += ", Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }
                    //}
                    //else
                    //{

                    //}
                }
                //if(r.Length>0)
                //{
                //    r += "...see employee list in [Attendance not processed] Tab...";
                //    return r;
                //}
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void DateValidation(string fromdate, string todate)
        {
            try
            {
                if (string.IsNullOrEmpty(fromdate))
                {
                    throw new Exception("'From Date' can not be blank...");
                }

                if (string.IsNullOrEmpty(todate))
                {
                    throw new Exception("'To Date' can not be blank...");
                }

                if (bplib.clsWebLib.IsDateOK(fromdate) == false)
                {
                    throw new Exception("From Date is not in right format; Expected Date fromat is 'dd-MMM-yyyy'; ex:'07-Feb-2010'");
                }
                if (bplib.clsWebLib.IsDateOK(todate) == false)
                {
                    throw new Exception("To Date is not in right format; Expected Date fromat is 'dd-MMM-yyyy'; ex:'07-Feb-2010'");
                }

                if (Convert.ToDateTime(fromdate) > Convert.ToDateTime(todate))
                {
                    throw new Exception("'To Date' can not be less than from date...");
                }

                if (Convert.ToDateTime(fromdate).ToString("yyyy") != Convert.ToDateTime(todate).ToString("yyyy"))
                {
                    throw new Exception("'Year' must be same in both FromDate and ToDate...");
                }

                if (Convert.ToDateTime(fromdate).ToString("MMM") != Convert.ToDateTime(todate).ToString("MMM"))
                {
                    throw new Exception("'Month' must be same in both FromDate and ToDate...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetEmpDelimited(string[] eList, out string _emps)
        {
            _emps = string.Empty;
            try
            {
                foreach (var item in eList)
                {
                    if (_emps.Length == 0)
                    {
                        _emps = "'" + item + "'";
                    }
                    else
                    {
                        _emps += ", '" + item + "'";
                    }
                }
                if (_emps.Length == 0)
                {
                    throw new Exception("No employee is selected...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetCurrency(string groupid, string plantid, out string _currencyId)
        {
            clsSalaryInfo objSal = null;
            DataSet dsCurrency = null;
            _currencyId = string.Empty;
            try
            {
                objSal = new clsSalaryInfo();
                objSal.GetLocalCurrency(groupid, plantid, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _currencyId = dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetEmpDelimited(List<ActiveEmp> List, ref string empids)
        {
            try
            {
                if (List != null)
                {
                    foreach (var obj in List)
                    {
                        if (obj.IsSelectSlrProc)
                        {
                            if (empids.Length == 0)
                            {
                                empids = "'" + obj.EmpSystemID + "'";
                            }
                            else
                            {
                                empids += ",'" + obj.EmpSystemID + "'";
                            }
                        }//if
                    }
                }//null
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetEmpDelimited(List<ExceptionEmp> List, ref string empids)
        {
            try
            {
                if (List != null)
                {
                    foreach (var obj in List)
                    {
                        //if (obj.IsSelectSlrProc)
                        //{
                        if (empids.Length == 0)
                        {
                            empids = "'" + obj.SystemID + "'";
                        }
                        else
                        {
                            empids += ",'" + obj.SystemID + "'";
                        }
                        //}//if
                    }
                }//null
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetEmpDelimitedMLVR(List<MaternityRetun> List, ref string empids)
        {
            try
            {
                if (List != null)
                {
                    foreach (var obj in List)
                    {
                        if (obj.IsSelectSlrProc)
                        {
                            if (empids.Length == 0)
                            {
                                empids = "'" + obj.EmpSystemID + "'";
                            }
                            else
                            {
                                empids += ",'" + obj.EmpSystemID + "'";
                            }
                        }//if
                    }
                }//null
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetEmpDelimitedActiveAndNewlyJoined(List<ActiveEmp> ListActive, List<ActiveEmp> ListNJ, out string empids)
        {
            empids = string.Empty;
            try
            {
                if (ListActive != null)
                {
                    foreach (var obj in ListActive)
                    {
                        if (obj.IsSelectSlrProc)
                        {
                            if (empids.Length == 0)
                            {
                                empids = "'" + obj.EmpSystemID + "'";
                            }
                            else
                            {
                                empids += ",'" + obj.EmpSystemID + "'";
                            }
                        }//if
                    }
                }//null
                if (ListNJ != null)
                {
                    foreach (var obj in ListNJ)
                    {
                        if (obj.IsSelectSlrProc)
                        {
                            if (empids.Length == 0)
                            {
                                empids = "'" + obj.EmpSystemID + "'";
                            }
                            else
                            {
                                empids += ",'" + obj.EmpSystemID + "'";
                            }
                        }//if
                    }
                }//null
                if (empids.Length == 0)
                {
                    throw new Exception("No employee is selected...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void ValidationBank(string emplist, string plantid)
        {
            clsEmployeeLoad objel = null;
            try
            {
                objel = new clsEmployeeLoad();
                DataSet dsBankInof;
                objel.GetBankInfo(plantid, emplist, out dsBankInof);

                string r = GetBankInfo(dsBankInof);
                if (r.Length > 0)
                {
                    throw new Exception(r);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetBankInfo(DataSet ds)
        {
            string r = string.Empty;
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (r.Length == 0)
                    {
                        r = "Employee [" + ds.Tables[0].Rows[i]["EmployeeCode"].ToString() + "] " + ds.Tables[0].Rows[i]["Remark"].ToString() + Environment.NewLine;
                    }
                    else
                    {
                        r += ", Employee [" + ds.Tables[0].Rows[i]["EmployeeCode"].ToString() + "] " + ds.Tables[0].Rows[i]["Remark"].ToString() + Environment.NewLine;
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void ProcessMain(FunctionPara para, string BatchNo, string ArrearFromDate, string ArrearToDate, string _currencyId, AllDataset allds)
        {
            try
            {

                para.txtForeignCurRate = "1";
                para.lblLocalCurRate = "1";
                para.lblLocalCurrencyID = _currencyId;
                para.lblForeignCurrencyID = _currencyId;
                para.lblUseFrgCurID = _currencyId;
                para.lblEmpCount = "0";
                para.IsNegativeSalaryApplicable = allds.IsNegativeSalaryApplicable;
                para.NegativeSalaryHeadId = allds.NegativeSalaryHeadId;

                para.ParaclsAdvanceProcess = (IclsAdvanceProcess)new clsAdvanceProcess();
                para.ParaSalaryHeadWiseAmountTransaction = (ISalaryHeadWiseAmountTransaction)new SalaryHeadWiseAmountTransaction();
                para.ParaSalaryHeadWiseFixedService = (ISalaryHeadWiseFixedService)new SalaryHeadWiseFixedService();
                para.ParaSalaryHeadWiseDailyService = (ISalaryHeadWiseDailyService)new SalaryHeadWiseDailyService();

                clsSalaryProcessAplosArrear obj = new clsSalaryProcessAplosArrear();
                FunctionPara m = obj.SalaryProcess(para, BatchNo, ArrearFromDate, ArrearToDate);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        List<ActiveEmp> ReSetDT(FunctionPara para, List<ActiveEmp> list)
        {
            List<ActiveEmp> r = null;
            try
            {
                clsSalaryProcessUI ui = new clsSalaryProcessUI();
                DataSet dsactive = null;
                string emp_pk = string.Empty;
                GetEmpDelimited(list, ref emp_pk);
                ui.LoadEmp_For_LOG(para.PlantId, para.FromDate, para.ToDate, emp_pk, out dsactive);
                r = dsactive.Tables[0].ToList<ActiveEmp>();
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        List<ExceptionEmp> ReSetDT(FunctionPara para, List<ExceptionEmp> list)
        {
            List<ExceptionEmp> r = null;
            try
            {
                clsSalaryProcessUI ui = new clsSalaryProcessUI();
                DataSet dsactive = null;
                string emp_pk = string.Empty;
                GetEmpDelimited(list, ref emp_pk);
                ui.LoadEmp_For_LOG(para.PlantId, para.FromDate, para.ToDate, emp_pk, out dsactive);
                r = dsactive.Tables[0].ToList<ExceptionEmp>();
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        List<MaternityRetun> ReSetDT(FunctionPara para, List<MaternityRetun> list)
        {
            List<MaternityRetun> r = null;
            try
            {
                //clsSalaryProcessQuery objQ = null;
                clsSalaryProcessQuery ui = new clsSalaryProcessQuery();
                DataSet dsactive = null;
                string emp_pk = string.Empty;
                GetEmpDelimitedMLVR(list, ref emp_pk);
                //ui.LoadEmp_For_LOG(para.PlantId, para.FromDate, para.ToDate, emp_pk, out dsactive);
                ui.GetEmpList_MLV_Going(emp_pk, para.FromDate, para.ToDate, para.PlantId, out dsactive);

                r = dsactive.Tables[0].ToList<MaternityRetun>();
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void ProcessMLV(FunctionPara para, string BatchNo, string ArrearFromDate, string ArrearToDate, string _mlvr_emps, string _currencyId, AllDataset allds)
        {
            try
            {
                #region Process
                para.txtForeignCurRate = "1";
                para.lblLocalCurRate = "1";
                para.lblLocalCurrencyID = _currencyId;
                para.lblForeignCurrencyID = _currencyId;
                para.lblUseFrgCurID = _currencyId;

                para.ParaclsAdvanceProcess = (IclsAdvanceProcess)new clsAdvanceProcess();
                para.ParaSalaryHeadWiseAmountTransaction = (ISalaryHeadWiseAmountTransaction)new SalaryHeadWiseAmountTransaction();
                para.ParaSalaryHeadWiseFixedService = (ISalaryHeadWiseFixedService)new SalaryHeadWiseFixedService();
                para.ParaSalaryHeadWiseDailyService = (ISalaryHeadWiseDailyService)new SalaryHeadWiseDailyService();

                para.lblEmpCount = "0";
                para.IsMaternityReturn = true;
                //para. = lblEmpCount.Text;
                clsSalaryProcessAplosArrear obj = new clsSalaryProcessAplosArrear();
                FunctionPara m = obj.SalaryProcess(para, BatchNo, ArrearFromDate, ArrearToDate);
                #endregion

                #region Save Log TBD
                clsSalaryProcessLog spl = new clsSalaryProcessLog();
                ParaLog paralog = new ParaLog();
                paralog.CompanyGroupId = para.GroupId;
                paralog.UserId = para.USER;
                paralog.PlantId = para.PlantId;
                paralog.SalaryProcessId = m.lblSalaryProcSystemId;

                paralog.MaternityReturn = ReSetDT(para, allds.dtMaternityReturn);

                paralog.YearNo = Convert.ToDateTime(para.FromDate).Year;
                paralog.MonthNo = Convert.ToDateTime(para.ToDate).Month;
                spl.SaveSalaryLogMLVReturn(paralog);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}