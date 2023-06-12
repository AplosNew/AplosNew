using bplib;
using Library.Service.Payrolls.SalaryProcessActive;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Library.HumanResource.Payroll.SalaryProcessActive
{
    public class clsSalaryProcessLog
    {
        public void SaveSalaryLog(ParaLog para)
        {
            DataSet dsSaveSummaryLog = null;
            DataSet dsSaveDetailLog = null;
            List<SalaryProcessLogDetail> list_spd = null;
            //clsStaticInfo objs = null;
            string _empids = string.Empty;
            try
            {
                //objs = new clsStaticInfo();
                CreateList(para, out list_spd);
                SaveSummaryLog(para, out dsSaveSummaryLog, out _empids);
                SaveDetailLog(list_spd, para, out dsSaveDetailLog);
                //objs.SaveDataSets(dsSaveSummaryLog, dsSaveDetailLog);
                SaveSalaryProcessLog(_empids, para, dsSaveSummaryLog, dsSaveDetailLog);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveSalaryLogMLVReturn(ParaLog para)
        {
            DataSet dsSaveSummaryLog = null;
            DataSet dsSaveDetailLog = null;
            List<SalaryProcessLogDetail> list_spd = null;
            //clsStaticInfo objs = null;
            string _empids = string.Empty;
            try
            {
                //objs = new clsStaticInfo();
                CreateListMLVReturn(para, out list_spd);
                SaveSummaryLogMLV_Return(para, out dsSaveSummaryLog, out _empids);
                SaveDetailLog(list_spd, para, out dsSaveDetailLog);
                //objs.SaveDataSets(dsSaveSummaryLog, dsSaveDetailLog);
                SaveSalaryProcessLog(_empids, para, dsSaveSummaryLog, dsSaveDetailLog);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SaveSalaryLogMLVGoing(ParaLog para)
        {
            DataSet dsSaveSummaryLog = null;
            DataSet dsSaveDetailLog = null;
            List<SalaryProcessLogDetail> list_spd = null;
            //clsStaticInfo objs = null;
            string _empids = string.Empty;
            try
            {
                //objs = new clsStaticInfo();
                CreateListMLVGoing(para, out list_spd);
                SaveSummaryLogMLV_Going(para, out dsSaveSummaryLog, out _empids);
                SaveDetailLog(list_spd, para, out dsSaveDetailLog);
                //objs.SaveDataSets(dsSaveSummaryLog, dsSaveDetailLog);
                SaveSalaryProcessLog(_empids, para, dsSaveSummaryLog, dsSaveDetailLog);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SaveSalaryLogSeparated(ParaLog para)
        {
            DataSet dsSaveSummaryLog = null;
            DataSet dsSaveDetailLog = null;
            List<SalaryProcessLogDetail> list_spd = null;
            //clsStaticInfo objs = null;
            string _empids = string.Empty;
            try
            {
                //objs = new clsStaticInfo();
                CreateListSeparated(para, out list_spd);
                SaveSummaryLogSeparated(para, out dsSaveSummaryLog, out _empids);
                SaveDetailLog(list_spd, para, out dsSaveDetailLog);
                //objs.SaveDataSets(dsSaveSummaryLog, dsSaveDetailLog);
                SaveSalaryProcessLog(_empids, para, dsSaveSummaryLog, dsSaveDetailLog);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        void InitList_MLV_Return(List<MaternityRetun> lists, string flag, ref List<SalaryProcessLogDetail> list_spd)
        {
            string _status = string.Empty;
            try
            {
                if (lists != null)
                {
                    foreach (var li in lists)
                    {
                        SalaryProcessLogDetail sd = new SalaryProcessLogDetail();
                        string _empid = li.EmpSystemID;// ds.Tables[0].Rows[i]["EmpSystemId"].ToString();
                        string _GivenDesignationId = li.GivenDesignationId;// ds.Tables[0].Rows[i]["GivenDesignationId"].ToString();
                        if (flag.Length == 0)//LA/TBS emp wise status will be dynamic 
                        {
                            _status = li.EmployeeStatus;// ds.Tables[0].Rows[i]["EmployeeStatus"].ToString();
                            sd.Flag = _status;
                            sd.EmpSystemId = _empid;
                            sd.DesignationId = _GivenDesignationId;

                            sd.LegalDesignationId = li.LegalDesignationId;
                            sd.LegalSalaryGradeId = li.LegalSalaryGradeId;
                            sd.BudgetCode = li.BudgetCode;
                            sd.BankAccNo = li.BankAccNo;
                            sd.BankBranchId = li.BankBranchId;
                            sd.BankSystemID = li.BankSystemID;
                            sd.EmployeeCategoryId = li.EmployeeCategoryId;
                            sd.PaymentMode = li.PaymentMode;
                            sd.SalaryPercentage = li.SalaryPercentage;
                            sd.IFSCCode = li.IFSCCode;
                            sd.MICRCode = li.MICRCode;

                            list_spd.Add(sd);
                        }
                        else
                        {
                            if (li.IsSelectSlrProc)
                            {
                                sd.Flag = flag;
                                sd.EmpSystemId = _empid;
                                sd.DesignationId = _GivenDesignationId;

                                sd.LegalDesignationId = li.LegalDesignationId;
                                sd.LegalSalaryGradeId = li.LegalSalaryGradeId;
                                sd.BudgetCode = li.BudgetCode;
                                sd.BankAccNo = li.BankAccNo;
                                sd.BankBranchId = li.BankBranchId;
                                sd.BankSystemID = li.BankSystemID;
                                sd.EmployeeCategoryId = li.EmployeeCategoryId;
                                sd.PaymentMode = li.PaymentMode;
                                sd.SalaryPercentage = li.SalaryPercentage;
                                sd.IFSCCode = li.IFSCCode;
                                sd.MICRCode = li.MICRCode;

                                list_spd.Add(sd);
                            }
                        }
                    }//foreach
                }//list null
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void InitList(List<ActiveEmp> lists, string flag, ref List<SalaryProcessLogDetail> list_spd)
        {
            string _status = string.Empty;
            try
            {
                if (lists != null)
                {
                    foreach (var li in lists)
                    {
                        SalaryProcessLogDetail sd = new SalaryProcessLogDetail();
                        string _empid = li.EmpSystemID;// ds.Tables[0].Rows[i]["EmpSystemId"].ToString();
                        string _GivenDesignationId = li.GivenDesignationId;// ds.Tables[0].Rows[i]["GivenDesignationId"].ToString();
                        if (flag.Length == 0)//LA/TBS emp wise status will be dynamic 
                        {
                            _status = li.EmployeeStatus;// ds.Tables[0].Rows[i]["EmployeeStatus"].ToString();
                            sd.Flag = _status;
                            sd.EmpSystemId = _empid;
                            sd.DesignationId = _GivenDesignationId;

                            sd.LegalDesignationId = li.LegalDesignationId;
                            sd.LegalSalaryGradeId = li.LegalSalaryGradeId;
                            sd.BudgetCode = li.BudgetCode;
                            sd.BankAccNo = li.BankAccNo;
                            sd.BankBranchId = li.BankBranchId;
                            sd.BankSystemID = li.BankSystemID;
                            sd.EmployeeCategoryId = li.EmployeeCategoryId;
                            sd.PaymentMode = li.PaymentMode;
                            sd.SalaryPercentage = li.SalaryPercentage;
                            sd.IFSCCode = li.IFSCCode;
                            sd.MICRCode = li.MICRCode;

                            list_spd.Add(sd);
                        }
                        else
                        {
                            if (li.IsSelectSlrProc)
                            {
                                sd.Flag = flag;
                                sd.EmpSystemId = _empid;
                                sd.DesignationId = _GivenDesignationId;

                                sd.LegalDesignationId = li.LegalDesignationId;
                                sd.LegalSalaryGradeId = li.LegalSalaryGradeId;
                                sd.BudgetCode = li.BudgetCode;
                                sd.BankAccNo = li.BankAccNo;
                                sd.BankBranchId = li.BankBranchId;
                                sd.BankSystemID = li.BankSystemID;
                                sd.EmployeeCategoryId = li.EmployeeCategoryId;
                                sd.PaymentMode = li.PaymentMode;
                                sd.SalaryPercentage = li.SalaryPercentage;
                                sd.IFSCCode = li.IFSCCode;
                                sd.MICRCode = li.MICRCode;

                                list_spd.Add(sd);
                            }
                        }
                    }//foreach
                }//list null
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void InitList(List<ExceptionEmp> lists, string flag, ref List<SalaryProcessLogDetail> list_spd)
        {
            string _status = string.Empty;
            try
            {
                foreach (var li in lists)
                {
                    SalaryProcessLogDetail sd = new SalaryProcessLogDetail();
                    string _empid = li.SystemID;// ds.Tables[0].Rows[i]["EmpSystemId"].ToString();
                    string _GivenDesignationId = li.LegalDesignation;// ds.Tables[0].Rows[i]["GivenDesignationId"].ToString();
                    if (flag.Length == 0)//LA/TBS emp wise status will be dynamic 
                    {
                        _status = li.EmployeeStatus;// ds.Tables[0].Rows[i]["EmployeeStatus"].ToString();
                        sd.Flag = _status;
                    }
                    sd.EmpSystemId = _empid;
                    sd.DesignationId = _GivenDesignationId;
                    sd.Flag = flag;
                    list_spd.Add(sd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CreateList(ParaLog para, out List<SalaryProcessLogDetail> list_spd)
        {
            list_spd = null;
            try
            {
                list_spd = new List<SalaryProcessLogDetail>();
                InitList(para.ActiveEmp, "Active", ref list_spd);
                InitList(para.NewlyJoinedEmp, "NewlyJoined", ref list_spd);
                InitList(para.PresentDaysZero, "Present Days Zero", ref list_spd);
                InitList(para.DifferentStatus, "", ref list_spd);
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CreateListMLVReturn(ParaLog para, out List<SalaryProcessLogDetail> list_spd)
        {
            list_spd = null;
            try
            {
                list_spd = new List<SalaryProcessLogDetail>();
                //LoadDataSetFromDataGrid(para.SalaryStructureNotDefined, out dsSSND);
                //LoadDataSetFromDataGrid(para.PresentDaysZero, out dsPZ);
                //LoadDataSetFromDataGrid(para.DifferentStatus, out dsDS);

                //InitList(para.SalaryStructureNotDefined, "Salary Structure Not defined", ref list_spd);
                InitList_MLV_Return(para.MaternityReturn, "MLV_RETURN", ref list_spd);
                //InitList(para.NewlyJoinedEmp, "NewlyJoined", ref list_spd);
                //InitList(para.PresentDaysZero, "Present Days Zero", ref list_spd);
                //InitList(para.DifferentStatus, "", ref list_spd);
                //InitList(para.AttNotLocked, "AttendanceNotLocked", ref list_spd);
                //InitList(para.ExcepEmp, "ExceptionEmployee", ref list_spd);
                //InitList(para.ApprovedSalary, "ApprovedSalary", ref list_spd);
                //InitList(para.ssna, "SalaryStructureNotApproved", ref list_spd);
                //InitList(para.SeparatedEmp, "SeparatedEmp", ref list_spd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CreateListMLVGoing(ParaLog para, out List<SalaryProcessLogDetail> list_spd)
        {
            list_spd = null;
            try
            {
                list_spd = new List<SalaryProcessLogDetail>();
                //LoadDataSetFromDataGrid(para.SalaryStructureNotDefined, out dsSSND);
                //LoadDataSetFromDataGrid(para.PresentDaysZero, out dsPZ);
                //LoadDataSetFromDataGrid(para.DifferentStatus, out dsDS);

                //InitList(para.SalaryStructureNotDefined, "Salary Structure Not defined", ref list_spd);
                InitList_MLV_Return(para.MaternityGoing, "MLV_GOING", ref list_spd);
                //InitList(para.NewlyJoinedEmp, "NewlyJoined", ref list_spd);
                //InitList(para.PresentDaysZero, "Present Days Zero", ref list_spd);
                //InitList(para.DifferentStatus, "", ref list_spd);
                //InitList(para.AttNotLocked, "AttendanceNotLocked", ref list_spd);
                //InitList(para.ExcepEmp, "ExceptionEmployee", ref list_spd);
                //InitList(para.ApprovedSalary, "ApprovedSalary", ref list_spd);
                //InitList(para.ssna, "SalaryStructureNotApproved", ref list_spd);
                //InitList(para.SeparatedEmp, "SeparatedEmp", ref list_spd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CreateListSeparated(ParaLog para, out List<SalaryProcessLogDetail> list_spd)
        {
            list_spd = null;
            try
            {
                list_spd = new List<SalaryProcessLogDetail>();
                //LoadDataSetFromDataGrid(para.SalaryStructureNotDefined, out dsSSND);
                //LoadDataSetFromDataGrid(para.PresentDaysZero, out dsPZ);
                //LoadDataSetFromDataGrid(para.DifferentStatus, out dsDS);

                //InitList(para.SalaryStructureNotDefined, "Salary Structure Not defined", ref list_spd);
                //InitList_MLV_Return(para.MaternityReturn, "MLV_RETURN", ref list_spd);
                //InitList(para.NewlyJoinedEmp, "NewlyJoined", ref list_spd);
                //InitList(para.PresentDaysZero, "Present Days Zero", ref list_spd);
                //InitList(para.DifferentStatus, "", ref list_spd);
                //InitList(para.AttNotLocked, "AttendanceNotLocked", ref list_spd);
                //InitList(para.ExcepEmp, "ExceptionEmployee", ref list_spd);
                //InitList(para.ApprovedSalary, "ApprovedSalary", ref list_spd);
                //InitList(para.ssna, "SalaryStructureNotApproved", ref list_spd);
                InitList(para.SeparatedEmp, "SeparatedEmp", ref list_spd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getEmpids(List<SalaryProcessLogDetail> list_spd, out string empids)
        {
            empids = "''";
            try
            {
                foreach (var item in list_spd)
                {
                    if (empids == "''")
                    {
                        empids = "'" + item.EmpSystemId + "'";
                    }
                    else
                    {
                        empids += ",'" + item.EmpSystemId + "'";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SaveDetailLog(List<SalaryProcessLogDetail> list_spd, ParaLog para, out DataSet dsSaveDetailLog)
        {
            dsSaveDetailLog = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            bool DATA_OK = false;
            string _empids = string.Empty;
            try
            {
                if (DATA_OK == false)
                {
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    _getEmpids(list_spd, out _empids);
                    DeleteLogDetail(para, _empids);
                    SelectSalaryProcessLogDetail(para.SalaryProcessId, para.PlantId, out dsSaveDetailLog);
                    dtLocal = dsSaveDetailLog.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;
                    int _pk_count = 0;
                    string idFromDB = string.Empty;
                    clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARY_LOG_DETAIL", out idFromDB);

                    for (int i = 0; i < list_spd.Count(); i++)
                    {
                        SalaryProcessLogDetail sd = list_spd[i];
                        sd.AddedBy = para.UserId;
                        sd.CompanyGroupId = para.CompanyGroupId;
                        sd.PlantId = para.PlantId;
                        sd.SalaryProcessId = para.SalaryProcessId;
                        sd.UpdatedBy = para.UserId;                        

                        dvLocal.RowFilter = "Id=''";//ever insert
                        if (dvLocal.Count == 0)
                        { // Add new block
                            _pk_count++;
                            drLocal = dtLocal.NewRow();
                            DetailRow("ADDNEW", _pk_count, idFromDB, sd, ref drLocal);
                            dtLocal.Rows.Add(drLocal);
                        }
                        else
                        {//edit block
                            drLocal = dvLocal[0].Row;
                            drLocal.BeginEdit();
                            DetailRow("EDIT", _pk_count, idFromDB, sd, ref drLocal);
                            drLocal.EndEdit();
                        }
                        dvLocal.RowFilter = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
            }
        }//end of function
        int GetCountedEmp(List<ActiveEmp> dg)
        {
            int _r = 0;
            try
            {
                _r = 0;
                if (dg != null)
                {
                    for (int i = 0; i < dg.Count; i++)
                    {
                        //CheckBox chkBox = (CheckBox)dg.Items[i].FindControl("chkSelectSlrProc");
                        if (dg[i].IsSelectSlrProc)
                        {
                            _r++;
                        }
                    }
                }
                return _r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        int GetCountedEmp(List<MaternityRetun> dg)
        {
            int _r = 0;
            try
            {
                _r = 0;
                if (dg != null)
                {
                    for (int i = 0; i < dg.Count; i++)
                    {
                        //CheckBox chkBox = (CheckBox)dg.Items[i].FindControl("chkSelectSlrProc");
                        if (dg[i].IsSelectSlrProc)
                        {
                            _r++;
                        }
                    }
                }
                return _r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetSelectedEmp(List<ActiveEmp> dg, ref string empids)
        {
            try
            {
                if (dg != null)
                {
                    for (int i = 0; i < dg.Count; i++)
                    {
                        //CheckBox chkBox = (CheckBox)dg.Items[i].FindControl("chkSelectSlrProc");
                        if (dg[i].IsSelectSlrProc)
                        {
                            if (empids.Length == 0)
                            {
                                empids = "'" + dg[i].EmpSystemID + "'";
                            }
                            else
                            {
                                empids += ", '" + dg[i].EmpSystemID + "'";
                            }
                        }//if
                    }//for
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetSelectedEmp(List<MaternityRetun> dg, ref string empids)
        {
            try
            {
                if (dg != null)
                {
                    for (int i = 0; i < dg.Count; i++)
                    {
                        //CheckBox chkBox = (CheckBox)dg.Items[i].FindControl("chkSelectSlrProc");
                        if (dg[i].IsSelectSlrProc)
                        {
                            if (empids.Length == 0)
                            {
                                empids = "'" + dg[i].EmpSystemID + "'";
                            }
                            else
                            {
                                empids += ", '" + dg[i].EmpSystemID + "'";
                            }
                        }//if
                    }//for
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void InitObj(ParaLog para, int se, SalaryProcessLogSummary sp)
        {
            try
            {
                sp.AddedBy = para.UserId;
                sp.CompanyGroupId = para.CompanyGroupId;
                sp.DifferentStatus = (para.DifferentStatus == null ? 0 : para.DifferentStatus.Count);
                sp.PlantId = para.PlantId;
                sp.PresentDaysZero = (para.PresentDaysZero == null ? 0 : para.PresentDaysZero.Count);// para.PresentDaysZero.Count;
                sp.ActiveEmployees = (para.ActiveEmp == null ? 0 : para.ActiveEmp.Count);//para.ActiveEmp.Count;
                sp.SeparatedEmployees = (para.SeparatedEmp == null ? 0 : para.SeparatedEmp.Count);//para.SeparatedEmp.Count;
                sp.NewlyJoinedEmployees = (para.NewlyJoinedEmp == null ? 0 : para.NewlyJoinedEmp.Count);//para.NewlyJoinedEmp.Count;
                sp.SalaryProcessId = para.SalaryProcessId;
                sp.SalaryStrucNotDefined = (para.SalaryStructureNotDefined == null ? 0 : para.SalaryStructureNotDefined.Count);//para.SalaryStructureNotDefined.Count;
                sp.SelectedEmployees = se;
                sp.UpdatedBy = para.UserId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SaveSummaryLog(ParaLog para, out DataSet dsSaveSummaryLog, out string _empids)
        {
            dsSaveSummaryLog = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            bool DATA_OK = false;
            SalaryProcessLogSummary sp = null;
            _empids = string.Empty;
            try
            {
                sp = new SalaryProcessLogSummary();
                if (DATA_OK == false)
                {
                    int Selected_emp_count = GetCountedEmp(para.ActiveEmp);
                    Selected_emp_count += GetCountedEmp(para.PresentDaysZero);
                    Selected_emp_count += GetCountedEmp(para.NewlyJoinedEmp);
                    //Selected_emp_count += GetCountedEmp(para.MaternityReturn);

                    //
                    GetSelectedEmp(para.ActiveEmp, ref _empids);
                    GetSelectedEmp(para.NewlyJoinedEmp, ref _empids);
                    GetSelectedEmp(para.PresentDaysZero, ref _empids);
                    //
                    InitObj(para, Selected_emp_count, sp);

                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    SelectSummaryLog(sp.SalaryProcessId, sp.PlantId, out dsSaveSummaryLog);
                    dtLocal = dsSaveSummaryLog.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;
                    dvLocal.RowFilter = "Id=''";//ever insert
                    if (dvLocal.Count == 0)
                    { // Add new block
                        drLocal = dtLocal.NewRow();
                        SummaryRow("ADDNEW", sp, ref drLocal);
                        dtLocal.Rows.Add(drLocal);
                    }
                    else
                    {//edit block
                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        SummaryRow("EDIT", sp, ref drLocal);
                        drLocal.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
            }
        }//end of function
        private void SaveSummaryLogMLV_Return(ParaLog para, out DataSet dsSaveSummaryLog, out string _empids)
        {
            dsSaveSummaryLog = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            bool DATA_OK = false;
            SalaryProcessLogSummary sp = null;
            _empids = string.Empty;
            try
            {
                sp = new SalaryProcessLogSummary();
                if (DATA_OK == false)
                {
                    int Selected_emp_count = GetCountedEmp(para.MaternityReturn);
                    //Selected_emp_count += GetCountedEmp(para.PresentDaysZero);
                    //Selected_emp_count += GetCountedEmp(para.NewlyJoinedEmp);
                    //Selected_emp_count += GetCountedEmp(para.MaternityReturn);

                    //
                    //GetSelectedEmp(para.ActiveEmp, ref _empids);
                    //GetSelectedEmp(para.NewlyJoinedEmp, ref _empids);
                    GetSelectedEmp(para.MaternityReturn, ref _empids);
                    //
                    InitObj(para, Selected_emp_count, sp);

                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    SelectSummaryLog(sp.SalaryProcessId, sp.PlantId, out dsSaveSummaryLog);
                    dtLocal = dsSaveSummaryLog.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;
                    dvLocal.RowFilter = "Id=''";//ever insert
                    if (dvLocal.Count == 0)
                    { // Add new block
                        drLocal = dtLocal.NewRow();
                        SummaryRow("ADDNEW", sp, ref drLocal);
                        dtLocal.Rows.Add(drLocal);
                    }
                    else
                    {//edit block
                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        SummaryRow("EDIT", sp, ref drLocal);
                        drLocal.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
            }
        }//end of function
        private void SaveSummaryLogSeparated(ParaLog para, out DataSet dsSaveSummaryLog, out string _empids)
        {
            dsSaveSummaryLog = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            bool DATA_OK = false;
            SalaryProcessLogSummary sp = null;
            _empids = string.Empty;
            try
            {
                sp = new SalaryProcessLogSummary();
                if (DATA_OK == false)
                {
                    int Selected_emp_count = GetCountedEmp(para.SeparatedEmp);
                    //Selected_emp_count += GetCountedEmp(para.PresentDaysZero);
                    //Selected_emp_count += GetCountedEmp(para.NewlyJoinedEmp);
                    //Selected_emp_count += GetCountedEmp(para.MaternityReturn);

                    //
                    //GetSelectedEmp(para.ActiveEmp, ref _empids);
                    //GetSelectedEmp(para.NewlyJoinedEmp, ref _empids);
                    GetSelectedEmp(para.SeparatedEmp, ref _empids);
                    //
                    InitObj(para, Selected_emp_count, sp);

                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    SelectSummaryLog(sp.SalaryProcessId, sp.PlantId, out dsSaveSummaryLog);
                    dtLocal = dsSaveSummaryLog.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;
                    dvLocal.RowFilter = "Id=''";//ever insert
                    if (dvLocal.Count == 0)
                    { // Add new block
                        drLocal = dtLocal.NewRow();
                        SummaryRow("ADDNEW", sp, ref drLocal);
                        dtLocal.Rows.Add(drLocal);
                    }
                    else
                    {//edit block
                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        SummaryRow("EDIT", sp, ref drLocal);
                        drLocal.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
            }
        }//end of function
        private void SaveSummaryLogMLV_Going(ParaLog para, out DataSet dsSaveSummaryLog, out string _empids)
        {
            dsSaveSummaryLog = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            bool DATA_OK = false;
            SalaryProcessLogSummary sp = null;
            _empids = string.Empty;
            try
            {
                sp = new SalaryProcessLogSummary();
                if (DATA_OK == false)
                {
                    int Selected_emp_count = GetCountedEmp(para.MaternityGoing);
                    //Selected_emp_count += GetCountedEmp(para.PresentDaysZero);
                    //Selected_emp_count += GetCountedEmp(para.NewlyJoinedEmp);
                    //Selected_emp_count += GetCountedEmp(para.MaternityReturn);

                    //
                    //GetSelectedEmp(para.ActiveEmp, ref _empids);
                    //GetSelectedEmp(para.NewlyJoinedEmp, ref _empids);
                    GetSelectedEmp(para.MaternityGoing, ref _empids);
                    //
                    InitObj(para, Selected_emp_count, sp);

                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    SelectSummaryLog(sp.SalaryProcessId, sp.PlantId, out dsSaveSummaryLog);
                    dtLocal = dsSaveSummaryLog.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;
                    dvLocal.RowFilter = "Id=''";//ever insert
                    if (dvLocal.Count == 0)
                    { // Add new block
                        drLocal = dtLocal.NewRow();
                        SummaryRow("ADDNEW", sp, ref drLocal);
                        dtLocal.Rows.Add(drLocal);
                    }
                    else
                    {//edit block
                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        SummaryRow("EDIT", sp, ref drLocal);
                        drLocal.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
            }
        }//end of function
        private void SummaryRow(string OPN_FLAG, SalaryProcessLogSummary sps, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;
            string idFromDB = "";
            string systemID = "";
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARY_LOG_SUMMARY", out idFromDB);
                    systemID = "LS" + idFromDB;
                    drLocal["Id"] = systemID;
                    drLocal["AddedBy"] = sps.AddedBy;
                    drLocal["AddedDate"] = DateTime.Now;
                }
                drLocal["SalaryProcessId"] = sps.SalaryProcessId;
                drLocal["ActiveEmployees"] = sps.ActiveEmployees;
                drLocal["SeparatedEmployees"] = sps.SeparatedEmployees;
                drLocal["NewlyJoinedEmployees"] = sps.NewlyJoinedEmployees;
                drLocal["SelectedEmployees"] = sps.SelectedEmployees;
                drLocal["SalaryStrucNotDefined"] = sps.SalaryStrucNotDefined;
                drLocal["PresentDaysZero"] = sps.PresentDaysZero;
                drLocal["DifferentStatus"] = sps.DifferentStatus;
                drLocal["CompanyGroupId"] = sps.CompanyGroupId;
                drLocal["PlantId"] = sps.PlantId;
                drLocal["UpdatedBy"] = sps.UpdatedBy;
                drLocal["UpdatedDate"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        } // end function

        //void SetValue(ref DataRow dr,object obj)
        //{
        //    try
        //    {
        //        if (sps.BankAccNo == null)
        //        {
        //            drLocal["BankAccNo"] = DBNull.Value;
        //        }
        //        else
        //        {
        //            drLocal["BankAccNo"] = sps.BankAccNo;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}
        private void DetailRow(string OPN_FLAG, int pkCount, string pk_seed, SalaryProcessLogDetail sps, ref DataRow drLocal)
        {
            string systemID = "";
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {

                    systemID = "LD" + pk_seed + "_" + pkCount;
                    drLocal["Id"] = systemID;
                    drLocal["AddedBy"] = sps.AddedBy;
                    drLocal["AddedDate"] = DateTime.Now;
                }
                drLocal["SalaryProcessId"] = sps.SalaryProcessId;
                drLocal["EmpSystemId"] = sps.EmpSystemId;
                drLocal["DesignationId"] = sps.DesignationId;

                drLocal["LegalDesignationId"] = sps.LegalDesignationId;
                if (sps.LegalSalaryGradeId == null)
                {
                    drLocal["LegalSalaryGradeId"] = DBNull.Value;
                }
                else
                {
                    drLocal["LegalSalaryGradeId"] = sps.LegalSalaryGradeId;
                }
                drLocal["BudgetCode"] = sps.BudgetCode;

                if (sps.BankAccNo == null)
                {
                    drLocal["BankAccNo"] = DBNull.Value;
                }
                else
                {
                    drLocal["BankAccNo"] = sps.BankAccNo;
                }

                if (sps.IFSCCode == null)
                {
                    drLocal["IFSCCode"] = DBNull.Value;
                }
                else
                {
                    drLocal["IFSCCode"] = sps.IFSCCode;
                }

                if (sps.MICRCode == null)
                {
                    drLocal["MICRCode"] = DBNull.Value;
                }
                else
                {
                    drLocal["MICRCode"] = sps.MICRCode;
                }

                //            ,EBI.IFSCCode
                //,EBI.MICRCode

                //if (sps.BankBranchId == null)
                //{
                //    drLocal["BankBranchId"] = DBNull.Value;
                //}
                //else
                //{
                //    drLocal["BankBranchId"] = sps.BankBranchId;
                //}

                //if (sps.BankSystemID == null)
                //{
                //    drLocal["BankSystemID"] = DBNull.Value;
                //}
                //else
                //{
                //    drLocal["BankSystemID"] = sps.BankSystemID;
                //}
                //drLocal["BankAccNo"] = sps.BankAccNo;

                //drLocal["BankBranchId"] = sps.BankBranchId;
                //drLocal["BankSystemID"] = sps.BankSystemID;


                drLocal["EmployeeCategoryId"] = sps.EmployeeCategoryId;
                drLocal["PaymentMode"] = sps.PaymentMode;
                drLocal["SalaryPercentage"] = sps.SalaryPercentage;

                if (sps.BankBranchId == null)
                {
                    drLocal["BankBranchId"] = DBNull.Value;
                }
                else
                {
                    drLocal["BankBranchId"] = sps.BankBranchId;
                }

                if (sps.BankSystemID == null)
                {
                    drLocal["BankSystemID"] = DBNull.Value;
                }
                else
                {
                    drLocal["BankSystemID"] = sps.BankSystemID;
                }


                //sd.LegalDesignationId = li.LegalDesignationId;
                //sd.LegalSalaryGradeId = li.LegalSalaryGradeId;
                //sd.BudgetCode = li.BudgetCode;
                //sd.BankAccNo = li.BankAccNo;
                //sd.BankBranchId = li.BankBranchId;
                //sd.BankSystemID = li.BankSystemID;
                //sd.EmployeeCategoryId = li.EmployeeCategoryId;
                //sd.PaymentMode = li.PaymentMode;
                //sd.SalaryPercentage = li.SalaryPercentage;


                drLocal["Flag"] = sps.Flag;
                drLocal["CompanyGroupId"] = sps.CompanyGroupId;
                drLocal["PlantId"] = sps.PlantId;
                drLocal["UpdatedBy"] = sps.UpdatedBy;
                drLocal["UpdatedDate"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        } // end function
        public void SelectSummaryLog(string SalaryProcessId, string PlantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryProcessLogSummary WHERE SalaryProcessId = '" + SalaryProcessId + "' AND PlantId = '" + PlantId + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public void SelectSalaryProcessLogDetail(string SalaryProcessId, string PlantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryProcessLogDetail WHERE SalaryProcessId = '" + SalaryProcessId + "' AND PlantId = '" + PlantId + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function



        private void LoadDataSetFromDataGrid(DataGrid dgSource, out DataSet dsDest)
        {
            Type T = null;
            DataRow drLocal = null;
            try
            {
                dsDest = new DataSet();
                dsDest.Tables.Add(new DataTable("dsFromDg"));

                //Adding Column Name To DataSource
                for (int ColCount = 0; ColCount < dgSource.Columns.Count; ColCount++)
                {
                    T = dgSource.Columns[ColCount].GetType();
                    //dsDest.Tables[0].Columns.Add(((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString());
                    if (T.Name == "BoundColumn")
                    {
                        dsDest.Tables[0].Columns.Add(((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString());
                    }
                    else if (T.Name == "TemplateColumn")
                    {
                        dsDest.Tables[0].Columns.Add(((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString());
                    }
                    //
                }

                //Adding Row To DataSource
                for (int rowCount = 0; rowCount < dgSource.Items.Count; rowCount++)
                {
                    drLocal = dsDest.Tables[0].NewRow();

                    for (int ColCount = 0; ColCount < dgSource.Columns.Count; ColCount++)
                    {
                        T = dgSource.Columns[ColCount].GetType();
                        if (T.Name == "BoundColumn")
                        {
                            if ((dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim() != "&nbsp;") && (dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim() != ""))
                            {
                                drLocal[((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString()] = dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim();
                            }
                            else
                            {
                                drLocal[((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString()] = DBNull.Value;
                            }
                        }
                        else if (T.Name == "TemplateColumn")
                        {
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsSelectSlrProc")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("chkSelectSlrProc")).Checked.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Amount")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtAmount")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsApproved")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("chkApproved")).Checked.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsDisbursed")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("chkDisbursed")).Checked.ToString().Trim();
                            }
                        }
                    }

                    dsDest.Tables[0].Rows.Add(drLocal);
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                T = null;
                drLocal = null;
            }
        }//End Function
        public void DeleteLogDetail(ParaLog para, string _empids)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            string _LogDetail = @"
                            delete from SalaryProcessLogDetail where SalaryProcessId in 
                            (
                            select systemid from SalaryProcMaster where YearNo=" + para.YearNo + @" 
                                                                        and MonthNo=" + para.MonthNo + @" 
                                                                        
                            )
                            and EmpSystemId in
                            (
	                            " + _empids + @"
                            )
                            ";
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(_LogDetail, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void DeleteSPMasterWithoutChild(ParaLog para)
        {
            ConnectionManager.DAL.ConManager objCon = null;

            string _common_sql = @" select SystemID from SalaryProcMaster m
										   WHERE NOT EXISTS(SELECT * FROM SalaryProcChild AS spc 
                                            WHERE spc.SlrProcMstSystemID=m.SystemID AND spc.PlantID='" + para.PlantId + @"') 
                                          AND m.YearNo=" + para.YearNo + @" AND m.MonthNo=" + para.MonthNo + @""
    ;
            string _LogSum = @"delete from SalaryProcessLogSummary where SalaryProcessId in 
                                    (
                                    " + _common_sql + @"
                                    )";
            string _LogDetail = @"delete from SalaryProcessLogDetail where SalaryProcessId in 
                                    (
                                   " + _common_sql + @"
                                    )";
            string _SpMaster = @"delete from SalaryProcMaster where YearNo=" + para.YearNo + @" and monthno=" + para.MonthNo + @" and systemid in
                                    (
                                   " + _common_sql + @"
                                    )";

            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(_LogSum, true, "1");
                objCon.ExecuteNonQueryWrapper(_LogDetail, true, "1");
                objCon.ExecuteNonQueryWrapper(_SpMaster, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                //throw (ex);
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }

                throw (ex);
            }
            finally
            {
                //objCon = null;
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void SaveSalaryProcessLog(string empids, ParaLog para, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                string _ss = @"delete from ExceptionEmployeeSalaryReprocess where EmpSystemId in (" + empids + @") and plantid = '" + para.PlantId + @"' 
                                    and Yearno = " + para.YearNo + @" and monthno = " + para.MonthNo + @"";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                if (!string.IsNullOrEmpty(empids))
                {
                    objCon.ExecuteNonQueryWrapper(_ss, true, "1"); 
                }
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
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
    }
    public class SalaryProcessLogSummary
    {
        public string Id { get; set; }
        public string UpdatedBy { get; set; }
        public string AddedBy { get; set; }

        public string SalaryProcessId { get; set; }
        public int SeparatedEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int NewlyJoinedEmployees { get; set; }
        public int SelectedEmployees { get; set; }
        public int SalaryStrucNotDefined { get; set; }
        public string CompanyGroupId { get; set; }
        public int PresentDaysZero { get; set; }
        public string PlantId { get; set; }
        public int DifferentStatus { get; set; }
    }
    public class SalaryProcessLogDetail
    {
        public string Id { get; set; }
        public string UpdatedBy { get; set; }
        public string AddedBy { get; set; }

        public string SalaryProcessId { get; set; }
        public string DesignationId { get; set; }
        public string EmpSystemId { get; set; }
        public string Flag { get; set; }
        public string CompanyGroupId { get; set; }
        public string PlantId { get; set; }

        public string LegalDesignationId { get; set; }
        public string PaymentMode { get; set; }
        public string BudgetCode { get; set; }
        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
        public string BankAccNo { get; set; }
        public string IFSCCode { get; set; }
        public string MICRCode { get; set; }
        public decimal SalaryPercentage { get; set; }
        public string EmployeeCategoryId { get; set; }
        public string LegalSalaryGradeId { get; set; }
    }
    public class ParaLog
    {
        public string SalaryProcessId { get; set; }
        public int YearNo { get; set; }
        public int MonthNo { get; set; }
        public string UserId { get; set; }
        public string CompanyGroupId { get; set; }
        public string PlantId { get; set; }
        public List<ActiveEmp> ActiveEmp { get; set; }
        public List<ActiveEmp> NewlyJoinedEmp { get; set; }
        public List<ActiveEmp> PresentDaysZero { get; set; }
        public List<MaternityRetun> MaternityReturn { get; set; }
        public List<MaternityRetun> MaternityGoing { get; set; }

        public List<ActiveEmp> SeparatedEmp { get; set; }
        public List<ActiveEmp> SalaryStructureNotDefined { get; set; }
        public List<ActiveEmp> ssna { get; set; }
        public List<ExceptionEmp> ExcepEmp { get; set; }

        public List<ActiveEmp> ApprovedSalary { get; set; }
        public List<ActiveEmp> AttNotLocked { get; set; }
        public List<ActiveEmp> DifferentStatus { get; set; }
    }
}
