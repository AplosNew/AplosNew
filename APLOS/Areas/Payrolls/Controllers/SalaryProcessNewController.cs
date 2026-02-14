#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.HumanResource.Payroll.SalaryProcess;
using Library.HumanResource.Payroll.SalaryProcessActive;
using Library.Service.TaskScheduler;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryProcessNewController : BaseController
    {
        //authentication for//
        //GetList Create Delete


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public SalaryProcessNewController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public async Task<ActionResult> Aplos()
        {
            return await Task.Factory.StartNew(() =>
            {

                var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SendNotification("Status: Ready To Process");

                return View();
            });

        }//
        [HttpGet, Authorize]
        public ActionResult GetNegativeSalaryHead()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT SalaryHeadID,SalaryHead,HeadCategory FROM SalaryHead where HeadCategory='Negative Salary' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult ProcessSalarySep(string FromDate, string ToDate, string pDescription, string[] eList)
        {
            string _currencyId = "";
            //string _EmpCount = "0";
            Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessQuery objQ = null;
            DataSet dsGrid = null;
            clsSalaryInfo objSal = null;
            DataSet dsCurrency = null;
            try
            {
                objQ = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessQuery();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                FunctionPara para = new FunctionPara();
                objSal = new clsSalaryInfo();

                if (string.IsNullOrEmpty(pDescription))
                {
                    throw new Exception("'Description' can not be blank...");
                }
                DateValidation(FromDate, ToDate);

                objSal.GetLocalCurrency(identity.CompanyGroupId, identity.PlantId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    //lblLocalCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }



                string _emps = string.Empty;
                GetEmpDelimited(eList, out _emps);
                // ValidationBank(_emps);
                ValidationAttendance(_emps, identity.PlantId, FromDate, ToDate);


                //string _flag = GetFlag();
                ///GetDataSet(_flag, out dsGrid);//get ds from db
                ///para.dsGrid = dsGrid;
                objQ.GetEmpList(eList, FromDate, ToDate, identity.PlantId, out dsGrid);

                para.dsGrid = dsGrid;
                para.FromDate = FromDate;
                para.ToDate = ToDate;
                para.GroupId = identity.CompanyGroupId;
                para.PlantId = identity.PlantId;

                para.USER = identity.UserId;
                para.txtForeignCurRate = "1";
                para.lblLocalCurRate = "1";
                para.lblLocalCurrencyID = _currencyId;
                para.lblForeignCurrencyID = _currencyId;
                para.lblUseFrgCurID = _currencyId;


                //para.lblTaxYearID = lblTaxYearID.Text;
                //para.lblTaxPeriod = lblTaxPeriod.Text;

                para.txtDescription = pDescription;
                //para.lblSalaryProcSystemId = lblSalaryProcSystemId.Text;
                //para.lblSalaryProcId = lblSalaryProcId.Text;
                para.lblEmpCount = eList.Length.ToString();
                para.IsSeparated = true;

                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR obj = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR();
                FunctionPara m = obj.SalaryProcess(para);
                objQ.DeleteExceptionEmpsForSalaryProcess(_emps, para.PlantId, Convert.ToDateTime(FromDate).ToString("yyyy"), Convert.ToDateTime(FromDate).ToString("MM"));

                #region Save Log TBD
                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog spl = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog();
                Library.HumanResource.Payroll.SalaryProcessActive.ParaLog paralog = new Library.HumanResource.Payroll.SalaryProcessActive.ParaLog();
                paralog.CompanyGroupId = para.GroupId;
                paralog.UserId = para.USER;
                paralog.PlantId = para.PlantId;
                paralog.SalaryProcessId = m.lblSalaryProcSystemId;
                var kk = dsGrid.Tables[0].ToList<Library.HumanResource.Payroll.SalaryProcessActive.ActiveEmp>();//List<ActiveEmp>
                paralog.SeparatedEmp = kk;

                paralog.YearNo = Convert.ToDateTime(para.FromDate).Year;
                paralog.MonthNo = Convert.ToDateTime(para.ToDate).Month;
                spl.SaveSalaryLogSeparated(paralog);
                #endregion

                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public JsonResult ProcessSalaryMLV(string FromDate, string ToDate, string pDescription, string[] eList)
        {
            string _currencyId = "";
            //string _EmpCount = "0";
            Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessQuery objQ = null;
            DataSet dsGrid = null;
            clsSalaryInfo objSal = null;
            DataSet dsCurrency = null;
            try
            {
                objQ = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessQuery();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                FunctionPara para = new FunctionPara();

                objSal = new clsSalaryInfo();

                DateValidation(FromDate, ToDate);
                if (pDescription.Trim().Length == 0)
                {
                    throw new Exception("'Description' can not be blank...");
                }

                GetCurrency(identity.CompanyGroupId, identity.PlantId, out _currencyId);

                string _emps = string.Empty;
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
                    _emps = "''";
                }


                // ValidationBank(_emps);
                ValidationAttendance(_emps, identity.PlantId, FromDate, ToDate);

                //string _flag = GetFlag();
                ///GetDataSet(_flag, out dsGrid);//get ds from db
                ///para.dsGrid = dsGrid;
                objQ.GetEmpList(eList, FromDate, ToDate, identity.PlantId, out dsGrid);

                para.dsGrid = dsGrid;
                para.FromDate = FromDate;
                para.ToDate = ToDate;
                para.GroupId = identity.CompanyGroupId;
                para.PlantId = identity.PlantId;

                para.USER = identity.UserId;
                para.txtForeignCurRate = "1";
                para.lblLocalCurRate = "1";
                para.lblLocalCurrencyID = _currencyId;
                para.lblForeignCurrencyID = _currencyId;
                para.lblUseFrgCurID = _currencyId;


                //para.lblTaxYearID = lblTaxYearID.Text;
                //para.lblTaxPeriod = lblTaxPeriod.Text;

                para.txtDescription = pDescription;
                //para.lblSalaryProcSystemId = lblSalaryProcSystemId.Text;
                //para.lblSalaryProcId = lblSalaryProcId.Text;
                para.lblEmpCount = eList.Length.ToString();
                para.IsMaternity = true;

                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR obj = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR();
                FunctionPara m = obj.SalaryProcess(para);
                objQ.DeleteExceptionEmpsForSalaryProcess(_emps, para.PlantId, Convert.ToDateTime(FromDate).ToString("yyyy"), Convert.ToDateTime(FromDate).ToString("MM"));

                #region Save Log TBD
                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog spl = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog();
                Library.HumanResource.Payroll.SalaryProcessActive.ParaLog paralog = new Library.HumanResource.Payroll.SalaryProcessActive.ParaLog();
                paralog.CompanyGroupId = para.GroupId;
                paralog.UserId = para.USER;
                paralog.PlantId = para.PlantId;
                paralog.SalaryProcessId = m.lblSalaryProcSystemId;
                var kk = dsGrid.Tables[0].ToList<Library.HumanResource.Payroll.SalaryProcessActive.MaternityRetun>();//List<ActiveEmp>
                paralog.MaternityGoing = kk;

                paralog.YearNo = Convert.ToDateTime(para.FromDate).Year;
                paralog.MonthNo = Convert.ToDateTime(para.ToDate).Month;
                spl.SaveSalaryLogMLVGoing(paralog);
                #endregion

                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
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
        [HttpPost]
        public async Task<JsonResult> ProcessAll(string FromDate, string ToDate, string pDescription, AllDataset palldataset)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.SalaryProcess, "", 60);
                _lock.LockProcess();

                //CancellationToken disconnectedToken = Response.ClientDisconnectedToken;
                //var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, disconnectedToken);
                //source.CancelAfter(43200000);
                return await Task.Factory.StartNew(() =>
                  {


                      Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessQuery objQ = null;
                      DataSet dsGrid = null;
                      string _currencyId = string.Empty;

                      var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                      SendNotification("Starting Salary Process");

                      try
                      {
                          //OnAbortSalaryProcess(source);

                          var alldataset = palldataset;

                          objQ = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessQuery();

                          #region Validation


                          DateValidation(FromDate, ToDate);
                          if (pDescription.Trim().Length == 0)
                          {
                              throw new Exception("'Description' can not be blank...");
                          }
                          #endregion
                          SendNotification("Validating Salary Structure");
                          clsSalaryProcessUI objel = new clsSalaryProcessUI();
                          objel.SSValida(identity.PlantId, FromDate, ToDate);

                          SendNotification("Getting Currency");
                          GetCurrency(identity.CompanyGroupId, identity.PlantId, out _currencyId);

                          SendNotification("Getting Employee List");

                          #region Active and other
                          string _active_emps = string.Empty;
                          string _all_emps = string.Empty;
                          GetEmpDelimitedActiveAndNewlyJoined(alldataset.dtActive, alldataset.dtNewlyJoined, alldataset.dtDifferentStatus,out _active_emps);
                          GetEmpDelimited(alldataset.dtPresetZero, ref _active_emps);
                          _all_emps = _active_emps;

                          #region MLV RETURN
                          SendNotification("MLV Return");

                          GetEmpDelimitedMLVR(alldataset.dtMaternityReturn, ref _all_emps);
                          #endregion

                          #region SALARY LOCK Current Month
                          //clsSalaryProcessUI objel = new clsSalaryProcessUI();
                          SendNotification("Validating Salary Lock");

                          string _yearno = Convert.ToDateTime(FromDate).ToString("yyyy");
                          string _monthno = Convert.ToDateTime(FromDate).ToString("MM");
                          objel.ValidationSalaryLock(_all_emps, _yearno, _monthno);
                          #endregion

                          #region SALARY LOCK Prev Month
                          SendNotification("Validating Salary Lock for Previous Month");

                          //clsSalaryProcessUI objel = new clsSalaryProcessUI();
                          string dtFD = Convert.ToDateTime(FromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                          string _yearnoP = Convert.ToDateTime(dtFD).ToString("yyyy");
                          string _monthnoP = Convert.ToDateTime(dtFD).ToString("MM");
                          objel.ValidationSalaryLockPreviousMonth(_all_emps, _yearnoP, _monthnoP);
                          #endregion


                          SendNotification("Validating Bank Accounts");
                          ValidationBank(_all_emps, identity.PlantId);

                          SendNotification("Validating Attendance Lock");
                          ValidationAttendance(_all_emps, identity.PlantId, FromDate, ToDate);

                          SendNotification("Fetching Employee List");
                          objQ.GetEmpListAll(_active_emps, FromDate, ToDate, identity.PlantId, out dsGrid);


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

                          /////////////////////////MAIN SALARY PROCESS////////////////////////
                          ProcessMain(para, _currencyId, alldataset);//pass ds

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
                              ProcessMLV(para2, _mlvr_emps, _currencyId, alldataset);
                          }
                          #endregion


                          SendNotification("Status: Process Completed");
                          requestCancelled = true;
                          _lock.UnlockProcess();
                          JsonResult json = Json(new { Error = false, Message = AplosMessage.Success });
                          json.MaxJsonLength = int.MaxValue;
                          return json;
                      }
                      catch (Exception ex)
                      {
                          requestCancelled = true;
                          _lock.UnlockProcess();
                          return Json(new { Error = true, Message = ex.Message });
                      }
                  });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #region SP related func
        void xValidationSalaryLock(string emplist, string yearno, string monthno)
        {
            clsSalaryProcessUI objel = null;
            DataSet dsSalaryLocked;
            try
            {
                objel = new clsSalaryProcessUI();
                objel.GetSalaryProcessedLockedEmp(emplist, yearno, monthno, out dsSalaryLocked);
                string r = objel.GetSalaryProcessedLockedMSG(dsSalaryLocked);
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
               
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    
                    if (r.Length == 0)
                    {
                        r = "Attendance is not locked (individual) for the following Employees:-" + Environment.NewLine;
                        r += " Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }
                    else
                    {
                        r += ", Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }
                  
                }
              
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
        void GetEmpDelimitedActiveAndNewlyJoined(List<ActiveEmp> ListActive, List<ActiveEmp> ListNJ, List<ActiveEmp> ListDS, out string empids)
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
                if (ListDS != null)
                {
                    foreach (var obj in ListDS)
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
                    throw new Exception("No active or newly joined employee is selected...");
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
        void ProcessMain(FunctionPara para, string _currencyId, AllDataset allds)
        {
            try
            {
                #region Process
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

                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR obj = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR();
                FunctionPara m = obj.SalaryProcess(para);

                #endregion

                #region Save Log TBD
                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog spl = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog();
                Library.HumanResource.Payroll.SalaryProcessActive.ParaLog paralog = new Library.HumanResource.Payroll.SalaryProcessActive.ParaLog();
                paralog.CompanyGroupId = para.GroupId;
                paralog.UserId = para.USER;
                paralog.PlantId = para.PlantId;
                paralog.SalaryProcessId = m.lblSalaryProcSystemId;
                //===================
                SendNotification("Creating Log [Active]");
                paralog.ActiveEmp = ReSetDT(para, allds.dtActive); //allds.dtActive;

                SendNotification("Creating Log [Newly Joined]");
                paralog.NewlyJoinedEmp = ReSetDT(para, allds.dtNewlyJoined);
                SendNotification("Creating Log [Present Days Zero]");
                paralog.PresentDaysZero = ReSetDT(para, allds.dtPresetZero);

                SendNotification("Creating Log [Salary Structure Not Defined]");
                paralog.SalaryStructureNotDefined = ReSetDT(para, allds.dtSND);
                SendNotification("Creating Log [New]");
                paralog.ssna = ReSetDT(para, allds.dtSNA);
                SendNotification("Creating Log [Approved Salary]");
                paralog.ApprovedSalary = ReSetDT(para, allds.dtApprovedSalary);
                SendNotification("Creating Log [Different Status]");
                paralog.DifferentStatus = ReSetDT(para, allds.dtDifferentStatus);
                SendNotification("Creating Log [Separated]");
                paralog.SeparatedEmp = ReSetDT(para, allds.dtSeparated);
                SendNotification("Creating Log [Attendance Not Locked]");
                paralog.AttNotLocked = ReSetDT(para, allds.dtAttNotProcessed);
                //paralog.ExcepEmp = ReSetDT(para, allds.dtEXemp);

                paralog.YearNo = Convert.ToDateTime(para.FromDate).Year;
                paralog.MonthNo = Convert.ToDateTime(para.ToDate).Month;
                SendNotification("Saving Salary Log");
                spl.SaveSalaryLog(paralog);
                #endregion
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
        void ProcessMLV(FunctionPara para, string _mlvr_emps, string _currencyId, AllDataset allds)
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
                clsSalaryProcessAplosR obj = new clsSalaryProcessAplosR();
                FunctionPara m = obj.SalaryProcess(para);
                #endregion

                #region Save Log TBD
                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog spl = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessLog();
                Library.HumanResource.Payroll.SalaryProcessActive.ParaLog paralog = new Library.HumanResource.Payroll.SalaryProcessActive.ParaLog();
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

        #region SP
        [HttpPost, Authorize]
        public ActionResult GetSeparatedEmpInfo(string FromDate, string ToDate)
        {
            string sql = string.Empty;


            try
            {
                DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(d.GetSeparatedEmpInfo(FromDate, ToDate, identity.PlantId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetSeparatedApprovedEmpInfo(string FromDate, string ToDate)
        {
            string sql = string.Empty;


            try
            {
                DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(d.GetSeparatedApprovedEmpInfo(FromDate, ToDate, identity.PlantId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetMLVEmpInfo(string FromDate, string ToDate)
        {
            try
            {
                DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(d.GetMLVEmpInfo(FromDate, ToDate, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetMLVApprovedEmpInfo(string FromDate, string ToDate)
        {
            try
            {
                DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(d.GetMLVProcessedEmpInfo(FromDate, ToDate, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetEmpList(string Description, string FromDate, string ToDate, string plantId)
        {
            return await Task.Factory.StartNew(() =>
            {

                try
                {
                    string sql = string.Empty;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    clsSalaryProcessUI obj = new clsSalaryProcessUI();
                    AllDataset ads = new AllDataset();
                    DateValidation(FromDate, ToDate);
                    if (plantId==null)
                    {
                        plantId = identity.PlantId;
                    }
                    obj.LoadEmpSalaryProcGrid(Description, FromDate, ToDate, plantId, out ads);


                    //JsonResult json = Json(_employeePromotionService.GetSalaryStrcApprovedEmployee(identity.CompanyGroupId, identity.PlantId) , JsonRequestBehavior.AllowGet);
                    //json.MaxJsonLength = int.MaxValue;
                    //return json;

                    JsonResult json = Json(new
                    {
                        Error = false
                        ,
                        Active = ads.dtActive
                        ,
                        Separated = ads.dtSeparated
                        ,
                        ApprovedSalary = ads.dtApprovedSalary
                        ,
                        AttNotProcessed = ads.dtAttNotProcessed
                        ,
                        DifferentStatus = ads.dtDifferentStatus
                        ,
                        ExcepEmp = ads.dtEXemp
                        ,
                        MaternityReturn = ads.dtMaternityReturn
                        ,
                        NewlyJoined = ads.dtNewlyJoined
                        ,
                        PresetZero = ads.dtPresetZero
                        ,
                        SNA = ads.dtSNA
                        ,
                        SND = ads.dtSND
                        ,
                        Message = AplosMessage.Updated
                    });
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                }
                catch (Exception ex)
                {
                    return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

                }
            });
        }

        [HttpPost]
        public async Task<ActionResult> GetEmpListNew(string Description, string FromDate, string ToDate, string plantId)
        {
            return await Task.Factory.StartNew(() =>
            {

                try
                {
                    string sql = string.Empty;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    clsSalaryProcessUI obj = new clsSalaryProcessUI();
                    AllDataset ads = new AllDataset();
                    DateValidation(FromDate, ToDate);
                    if (plantId == null)
                    {
                        plantId = identity.PlantId;
                    }
                    obj.LoadEmpSalaryProcGridNew(Description, FromDate, ToDate, plantId, out ads);


                    //JsonResult json = Json(_employeePromotionService.GetSalaryStrcApprovedEmployee(identity.CompanyGroupId, identity.PlantId) , JsonRequestBehavior.AllowGet);
                    //json.MaxJsonLength = int.MaxValue;
                    //return json;

                    JsonResult json = Json(new
                    {
                        Error = false
                        ,
                        Active = ads.dtActive
                        ,
                        Separated = ads.dtSeparated
                        ,
                        ApprovedSalary = ads.dtApprovedSalary
                        ,
                        AttNotProcessed = ads.dtAttNotProcessed
                        ,
                        DifferentStatus = ads.dtDifferentStatus
                        ,
                        ExcepEmp = ads.dtEXemp
                        ,
                        MaternityReturn = ads.dtMaternityReturn
                        ,
                        NewlyJoined = ads.dtNewlyJoined
                        ,
                        PresetZero = ads.dtPresetZero
                        ,
                        SNA = ads.dtSNA
                        ,
                        SND = ads.dtSND
                        ,
                        Message = AplosMessage.Updated
                    });
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                }
                catch (Exception ex)
                {
                    return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

                }
            });
        }
        #endregion
    }
}