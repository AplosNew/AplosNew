#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.HumanResource.Payroll.SalaryProcess;
using Library.Model.Setups;
using Library.Service.Payrolls.SalaryProcess;
using Library.Service.Payrolls.SalaryProcessActive;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryProcessOtherStatusNewController : BaseController
    {
        //authentication for
        //GetList Create Delete


        #region Constructor
        //string TableName = "HKP.TaskCategory";
        private readonly ISqlRepository _sqlRepository;//
        public SalaryProcessOtherStatusNewController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> ProcessSalarySep(string FromDate, string ToDate, string pDescription, string[] eList)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.SalaryProcess, "", 60);
            _lock.LockProcess();

            return await Task.Factory.StartNew(() =>
            {
                string _currencyId = "";
                //string _EmpCount = "0";
                clsSalaryProcessQuery objQ = null;
                DataSet dsGrid = null;
                clsSalaryInfo objSal = null;
                DataSet dsCurrency = null;
                try
                {
                    objQ = new clsSalaryProcessQuery();
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

                    #region SALARY LOCK Current Month
                    clsSalaryProcessUI objel = new clsSalaryProcessUI();
                    string _yearno = Convert.ToDateTime(FromDate).ToString("yyyy");
                    string _monthno = Convert.ToDateTime(FromDate).ToString("MM");
                    objel.ValidationSalaryLock(_emps, _yearno, _monthno);
                    #endregion

                    #region SALARY LOCK Prev Month
                    //clsSalaryProcessUI objel = new clsSalaryProcessUI();
                    string dtFD = Convert.ToDateTime(FromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                    string _yearnoP = Convert.ToDateTime(dtFD).ToString("yyyy");
                    string _monthnoP = Convert.ToDateTime(dtFD).ToString("MM");
                    objel.ValidationSalaryLockPreviousMonth(_emps, _yearnoP, _monthnoP);
                    #endregion

                    #region allowance
                    //try
                    //{
                    //    DateTime fd = Convert.ToDateTime(FromDate);
                    //    DateTime td = Convert.ToDateTime(ToDate);
                    //    clsDailyAllowance odailyAllowance = new clsDailyAllowance();                    
                    //    odailyAllowance.UpdateDailyAllowanceSummaryData(identity, fd.ToString("dd-MMM-yyyy"), td.ToString("dd-MMM-yyyy"), _emps);
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new Exception("Allowance issue: " + ex.Message);
                    //}
                    #endregion

                    #region Advance
                    //try
                    //{
                    //    DateTime fd = Convert.ToDateTime(FromDate);
                    //    DateTime td = Convert.ToDateTime(ToDate);
                    //    clsAdvanceProcess oAdvProc = new clsAdvanceProcess();
                    //    oAdvProc.ProcessEmployeeAdvance(identity, fd.ToString("dd-MMM-yyyy"), td.ToString("dd-MMM-yyyy"), _emps);
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new Exception("Allowance issue: " + ex.Message);
                    //}
                    #endregion



                    #region Daily/Monthly
                    //try
                    //{
                    //    DateTime fd = Convert.ToDateTime(FromDate);
                    //    DateTime td = Convert.ToDateTime(ToDate);
                    //    //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    //    SalaryHeadWiseAmountTransaction o = new SalaryHeadWiseAmountTransaction();
                    //    o.SalaryHeadWiseAmountCalculation(identity, fd.ToString("dd-MMM-yyyy"), td.ToString("dd-MMM-yyyy"), _emps);
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new Exception("Salary-Head-Wise-Amount issue: " + ex.Message);
                    //}
                    #endregion

                    #region MonthlyFixedService
                    try
                    {
                        //DateTime fd = Convert.ToDateTime(FromDate);
                        //DateTime td = Convert.ToDateTime(ToDate);
                        ////var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        //SalaryHeadWiseFixedService o = new SalaryHeadWiseFixedService();
                        //o.SalaryHeadWiseMonthlyFixedAmountCalculation(identity, fd.ToString("dd-MMM-yyyy"), td.ToString("dd-MMM-yyyy"), _emps);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Monthly Fixed Service issue: " + ex.Message);
                    }
                    #endregion MonthlyFixedService

                    // ValidationBank(_emps);
                    ValidationAttendance(_emps, identity.PlantId, FromDate, ToDate);


                    //string _flag = GetFlag();
                    ///GetDataSet(_flag, out dsGrid);//get ds from db
                    ///para.dsGrid = dsGrid;
                    //objQ.GetEmpList(eList, FromDate, ToDate, identity.PlantId, out dsGrid);
                    objQ.GetEmpListSepa(eList, FromDate, ToDate, identity.PlantId, out dsGrid);

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

                    para.ParaclsAdvanceProcess = (IclsAdvanceProcess)new clsAdvanceProcess();
                    para.ParaSalaryHeadWiseAmountTransaction = (ISalaryHeadWiseAmountTransaction)new SalaryHeadWiseAmountTransaction();
                    para.ParaSalaryHeadWiseFixedService = (ISalaryHeadWiseFixedService)new SalaryHeadWiseFixedService();
                    para.ParaSalaryHeadWiseDailyService = (ISalaryHeadWiseDailyService)new SalaryHeadWiseDailyService();

                    clsSalaryProcessAplosR obj = new clsSalaryProcessAplosR();
                    FunctionPara m = obj.SalaryProcess(para);
                    objQ.DeleteExceptionEmpsForSalaryProcess(_emps, para.PlantId, Convert.ToDateTime(FromDate).ToString("yyyy"), Convert.ToDateTime(FromDate).ToString("MM"));

                    //log
                    #region Save Log TBD
                    clsSalaryProcessLog spl = new clsSalaryProcessLog();
                    ParaLog paralog = new ParaLog();
                    paralog.CompanyGroupId = para.GroupId;
                    paralog.UserId = para.USER;
                    paralog.PlantId = para.PlantId;
                    paralog.SalaryProcessId = m.lblSalaryProcSystemId;

                    //paralog.ActiveEmp = allds.dtActive;
                    //paralog.NewlyJoinedEmp = allds.dtNewlyJoined;
                    //paralog.PresentDaysZero = allds.dtPresetZero;

                    //paralog.SalaryStructureNotDefined = allds.dtSND;
                    //paralog.ssna = allds.dtSNA;
                    //paralog.ApprovedSalary = allds.dtApprovedSalary;
                    //paralog.DifferentStatus = allds.dtDifferentStatus;
                    //paralog.SeparatedEmp = allds.dtSeparated;
                    //paralog.AttNotLocked = allds.dtAttNotProcessed;
                    paralog.SeparatedEmp = dsGrid.Tables[0].ToList<ActiveEmp>();

                    paralog.YearNo = Convert.ToDateTime(para.FromDate).Year;
                    paralog.MonthNo = Convert.ToDateTime(para.ToDate).Month;
                    spl.SaveSalaryLogSeparated(paralog);
                    #endregion

                    _lock.UnlockProcess();
                    JsonResult json = Json(new { Error = false, Message = AplosMessage.Success });
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                }
                catch (Exception ex)
                {
                    _lock.UnlockProcess();
                    return Json(new { Error = true, Message = ex.Message });
                }
            });
        }
        [HttpPost]
        public async Task<JsonResult> ProcessSalaryMLV(string FromDate, string ToDate, string pDescription, string[] eList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.SalaryProcess, "", 60);
            _lock.LockProcess();


            return await Task.Factory.StartNew(() =>
            {
                string _currencyId = "";
                //string _EmpCount = "0";
                clsSalaryProcessQuery objQ = null;
                DataSet dsGrid = null;
                clsSalaryInfo objSal = null;
                DataSet dsCurrency = null;
                try
                {
                    if (string.IsNullOrEmpty(pDescription))
                    {
                        throw new Exception("'Description' can not be blank...");
                    }
                    objQ = new clsSalaryProcessQuery();
                    FunctionPara para = new FunctionPara();

                    objSal = new clsSalaryInfo();

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

                    if (pDescription.Length == 0)
                    {
                        throw new Exception("'Description' can not be blank...");
                    }

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

                    #region SALARY LOCK
                    clsSalaryProcessUI objel = new clsSalaryProcessUI();
                    string _yearno = Convert.ToDateTime(FromDate).ToString("yyyy");
                    string _monthno = Convert.ToDateTime(FromDate).ToString("MM");
                    objel.ValidationSalaryLock(_emps, _yearno, _monthno);
                    #endregion



                    #region SALARY LOCK Prev Month
                    //clsSalaryProcessUI objel = new clsSalaryProcessUI();
                    string dtFD = Convert.ToDateTime(FromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                    string _yearnoP = Convert.ToDateTime(dtFD).ToString("yyyy");
                    string _monthnoP = Convert.ToDateTime(dtFD).ToString("MM");
                    objel.ValidationSalaryLockPreviousMonth(_emps, _yearnoP, _monthnoP);
                    #endregion

                    #region allowance
                    try
                    {
                        //DateTime fd = Convert.ToDateTime(FromDate);
                        //DateTime td = Convert.ToDateTime(ToDate);
                        //clsDailyAllowance odailyAllowance = new clsDailyAllowance();
                        //odailyAllowance.UpdateDailyAllowanceSummaryData(identity, fd.ToString("dd-MMM-yyyy"), td.ToString("dd-MMM-yyyy"), _emps);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Allowance issue: " + ex.Message);
                    }
                    #endregion
                    // ValidationBank(_emps);
                    ValidationAttendance(_emps, identity.PlantId, FromDate, ToDate);

                    //string _flag = GetFlag();
                    ///GetDataSet(_flag, out dsGrid);//get ds from db
                    ///para.dsGrid = dsGrid;
                    //objQ.GetEmpList(eList, FromDate, ToDate, identity.PlantId, out dsGrid); 
                    objQ.GetEmpList_MLV_Going(eList, FromDate, ToDate, identity.PlantId, out dsGrid);

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

                    para.ParaclsAdvanceProcess = (IclsAdvanceProcess)new clsAdvanceProcess();
                    para.ParaSalaryHeadWiseAmountTransaction = (ISalaryHeadWiseAmountTransaction)new SalaryHeadWiseAmountTransaction();
                    para.ParaSalaryHeadWiseFixedService = (ISalaryHeadWiseFixedService)new SalaryHeadWiseFixedService();
                    para.ParaSalaryHeadWiseDailyService = (ISalaryHeadWiseDailyService)new SalaryHeadWiseDailyService();

                    clsSalaryProcessAplosR obj = new clsSalaryProcessAplosR();
                    FunctionPara m = obj.SalaryProcess(para);
                    objQ.DeleteExceptionEmpsForSalaryProcess(_emps, para.PlantId, Convert.ToDateTime(FromDate).ToString("yyyy"), Convert.ToDateTime(FromDate).ToString("MM"));

                    //log
                    #region Save Log TBD
                    clsSalaryProcessLog spl = new clsSalaryProcessLog();
                    ParaLog paralog = new ParaLog();
                    paralog.CompanyGroupId = para.GroupId;
                    paralog.UserId = para.USER;
                    paralog.PlantId = para.PlantId;
                    paralog.SalaryProcessId = m.lblSalaryProcSystemId;

                    //paralog.ActiveEmp = allds.dtActive;
                    //paralog.NewlyJoinedEmp = allds.dtNewlyJoined;
                    //paralog.PresentDaysZero = allds.dtPresetZero;

                    //paralog.SalaryStructureNotDefined = allds.dtSND;
                    //paralog.ssna = allds.dtSNA;
                    //paralog.ApprovedSalary = allds.dtApprovedSalary;
                    //paralog.DifferentStatus = allds.dtDifferentStatus;
                    //paralog.SeparatedEmp = allds.dtSeparated;
                    //paralog.AttNotLocked = allds.dtAttNotProcessed;
                    paralog.MaternityGoing = dsGrid.Tables[0].ToList<MaternityRetun>();

                    paralog.YearNo = Convert.ToDateTime(para.FromDate).Year;
                    paralog.MonthNo = Convert.ToDateTime(para.ToDate).Month;
                    spl.SaveSalaryLogMLVGoing(paralog);
                    #endregion

                    _lock.UnlockProcess();
                    JsonResult json = Json(new { Error = false, Message = AplosMessage.Success });
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                    //return Json(new { Error = false, Message = AplosMessage.Success });
                }
                catch (Exception ex)
                {
                    _lock.UnlockProcess();
                    return Json(new { Error = true, Message = ex.Message });
                }
            });
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
                DataTable dt = new DataView(ds.Tables[0]).ToTable(true, "workdate");
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
                        r = "Attendance is not locked on the following days:-" + Environment.NewLine;
                        r += " Work Date [" + dt.Rows[i]["workdate"].ToString() + "]" + Environment.NewLine;
                    }
                    else
                    {
                        r += ", Work Date [" + dt.Rows[i]["workdate"].ToString() + "]" + Environment.NewLine;
                    }
                    //}
                    //else
                    //{
                    r += "...see employee list in [Attendance not processed] Tab...";
                    return r;
                    //}
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


        [HttpPost, Authorize]
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        [HttpPost, Authorize]
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }


        #region SP
        [HttpPost]
        public ActionResult GetSeparatedEmpInfo(string FromDate, string ToDate)
        {
            string sql = string.Empty;


            try
            {
                DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(new { data = d.GetSeparatedEmpInfo(FromDate, ToDate, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetSeparatedEmpZeroPresentInfo(string FromDate, string ToDate)
        {
            string sql = string.Empty;


            try
            {
                //DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(new { data = d.GetSeparatedEmpPresentZeroInfo(FromDate, ToDate, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetSeparatedApprovedEmpInfo(string FromDate, string ToDate)
        {
            string sql = string.Empty;


            try
            {
                //DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(new { data = d.GetSeparatedApprovedEmpInfo(FromDate, ToDate, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
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
                return Json(new { data = d.GetMLVEmpInfo(FromDate, ToDate, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetMLVApprovedEmpInfo(string FromDate, string ToDate)
        {
            try
            {

                //if (FromDate ==null)
                //{
                //    Exception ex = new Exception("Please Select From Date");
                //    throw (ex);
                //}
                //if (ToDate == null)
                //{
                //    Exception ex = new Exception("Please Select TO Date");
                //    throw (ex);
                //}
                //DateValidation(FromDate, ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
                return Json(new { data = d.GetMLVProcessedEmpInfo(FromDate, ToDate, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GettbsEmpInfo(string FromDate, string ToDate)
        {
            string sql = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SalaryProcessDic d = new SalaryProcessDic(_sqlRepository);
            //return Json(_sqlRepository.GetDataCollection(identity.PlantId, FromDate, ToDate, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
            return Json(d.GettbsmpInfo(FromDate, ToDate, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}