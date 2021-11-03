#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Payrolls;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.HR;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Setups
{
    public class SalaryFixationService : Service<SalaryFixation>, ISalaryFixationService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISalaryFixationSettingDetailsService _salaryFixationSettingDetailsService;
        private readonly ISalaryFixationMailService _sfm;
        private readonly IRepositoryAsync<SalaryFixationSettingDetails> _salaryFixationSettingDetailsRepository;
        private readonly IRepositoryAsync<SalaryFixation> _salaryFixationRepository;
        private readonly IRepositoryAsync<EmployeeWiseTermsAndConditions> _employeeWiseTermsAndConditionsRepository;

        public SalaryFixationService(
            IRepositoryAsync<SalaryFixation> salaryFixationRepository,
            IPKGeneratorService pkGeneratorService,
            ISalaryFixationMailService sfm,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , ISalaryFixationSettingDetailsService salaryFixationSettingDetailsService
            , IRepositoryAsync<SalaryFixationSettingDetails> salaryFixationSettingDetailsRepository
            , IRepositoryAsync<EmployeeWiseTermsAndConditions> employeeWiseTermsAndConditionsRepository)
            : base(salaryFixationRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _sfm = sfm;
            _salaryFixationSettingDetailsService = salaryFixationSettingDetailsService;
            _salaryFixationSettingDetailsRepository = salaryFixationSettingDetailsRepository;
            _salaryFixationRepository = salaryFixationRepository;
            _employeeWiseTermsAndConditionsRepository = employeeWiseTermsAndConditionsRepository;
        }

        #endregion Constructor

        #region ---Actions
        public GridModel GetEmployees(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT PRE.Id AS PreRecruitmentEmployeeID
                                        	,PRE.EmployeeCode
                                        	,PRE.FullName
                                        	,PRE.BudgetId
                                        	,PRE.Email
                                        	,DEG.UserName GivenDesignation
                                        	,DEPT.UserName AS Department
											,PRE.GivenDesignationId
											,PRE.Image
                                            ,sr.SalaryRuleName
                                            ,dm.SalaryRuleMasterId SalaryRuleId
											,pre.TotalSalary
                                            ,f.k Formula
                                            ,pre.Submitted,PRE.PlantId,PMB.Code
                                        FROM PreRecruitmentEmployee PRE
                                        LEFT JOIN MST.ManpowerBudget PMB ON PRE.BudgetId = PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
                                        LEFT JOIN HKP.Designation DEG ON DEG.Id = PRE.GivenDesignationId
                                        left outer join (SELECT DM.DesignationId,DC.SalaryRuleMasterId,DC.PlantId 
										FROM MST.DesignationMaster DM
                                        LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                        )  dm on dm.DesignationId=pre.GivenDesignationId
										left outer join SalaryRuleMaster sr on sr.SystemID=dm.SalaryRuleMasterId

                                        left outer join
										(
										SELECT
											   SS.SalaryRuleName,SS.SystemID,
											   STUFF((SELECT  '# ' +h.SalaryHead+' = '+   sh.FormulaDes

											   from SalaryRuleGeneral sh
											left outer join SalaryHead h on h.SalaryHeadID=sh.SalaryHeadId

											where isnull(sh.FormulaDes,'')<>'' and ss.SystemID=sh.SalaryRuleMasterSystemID
											ORDER BY ss.SystemID,sh.SequenceNo
													  FOR XML PATH('')), 1, 1, '') [k]
											FROM SalaryRuleMaster SS
										) f on f.SystemID=dm.SalaryRuleMasterId

                                        WHERE PRE.GroupID = '" + companyGroupId + @"'
                                        AND PRE.CompanyId = '" + companyId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }
        public GridModel xxxGetEmployees(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT PRE.Id AS PreRecruitmentEmployeeID
                                        	,PRE.EmployeeCode
                                        	,PRE.FullName
                                        	,PRE.BudgetId
                                        	,PRE.Email
                                        	,DEG.UserName GivenDesignation
                                        	,DEPT.UserName AS Department
											,PRE.GivenDesignationId
											,PRE.Image
                                            ,sr.SalaryRuleName
                                            ,dm.SalaryRuleMasterId SalaryRuleId
											,pre.TotalSalary
                                            ,f.k Formula
                                            ,pre.Submitted,PRE.PlantId,PMB.Code
                                        FROM PreRecruitmentEmployee PRE
                                        LEFT JOIN MST.ManpowerBudget PMB ON PRE.BudgetId = PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
                                        LEFT JOIN HKP.Designation DEG ON DEG.Id = PRE.GivenDesignationId
                                        left outer join mst.DesignationMaster dm on dm.DesignationId=pre.GivenDesignationId
										left outer join SalaryRuleMaster sr on sr.SystemID=dm.SalaryRuleMasterId

                                        left outer join
										(
										SELECT
											   SS.SalaryRuleName,SS.SystemID,
											   STUFF((SELECT  '# ' +h.SalaryHead+' = '+   sh.FormulaDes

											   from SalaryRuleGeneral sh
											left outer join SalaryHead h on h.SalaryHeadID=sh.SalaryHeadId

											where isnull(sh.FormulaDes,'')<>'' and ss.SystemID=sh.SalaryRuleMasterSystemID
											ORDER BY ss.SystemID,sh.SequenceNo
													  FOR XML PATH('')), 1, 1, '') [k]
											FROM SalaryRuleMaster SS
										) f on f.SystemID=dm.SalaryRuleMasterId

                                        WHERE PRE.GroupID = '" + companyGroupId + @"'
                                        AND PRE.CompanyId = '" + companyId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        /// <summary>
        /// For PreRecruitment Employee
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetSalaryHeadDataList()
        {
            try
            {
                var sql = @"SELECT SH.SalaryHeadID
                            	,SH.SalaryHead
                            	,SFD.IsAnnualCash
                            	,SFD.IsMonthly
                            	,SFD.IsLeave
                            	,SFD.IsAnnualNonCash
                                ,'' PreRecruitmentEmployeeID
                            FROM SCS.SalaryFixationSettingDetails SFD
                            LEFT JOIN SalaryHead SH ON SFD.SalaryHeadID = SH.SalaryHeadID
                            WHERE IsMonthly = 1
                            	OR IsAnnualCash = 1
                            UNION
                            SELECT SH.Id SalaryHeadID
                            	,SH.LeaveType SalaryHead
                            	,SFD.IsAnnualCash
                            	,SFD.IsMonthly
                            	,SFD.IsLeave
                            	,SFD.IsAnnualNonCash
                                ,'' PreRecruitmentEmployeeID
                            FROM SCS.SalaryFixationSettingDetails SFD
                            LEFT JOIN LeaveType SH ON SFD.LeaveTypeId = SH.Id
                            WHERE IsLeave = 1
                            UNION
                            SELECT SFD.Id SalaryHeadID
                            	,SFD.YearlyNonCash SalaryHead
                            	,SFD.IsAnnualCash
                            	,SFD.IsMonthly
                            	,SFD.IsLeave
                            	,SFD.IsAnnualNonCash
                                ,'' PreRecruitmentEmployeeID
                            FROM SCS.SalaryFixationSettingDetails SFD
                            WHERE IsAnnualNonCash = 1";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetSalaryHeadList(string preRecEmpId)
        {
            try
            {
                var sql = @"SELECT isnull(SH.SalaryHead, '') + isnull(LT.Description, '') SalaryHead
                            	,isnull(SH.SalaryHeadID, '') + isnull(SF.LeaveTypeID, '') SalaryHeadId
                            	,SF.CurrentAmount
                            	,SF.ExpectedAmount
                                ,SF.Id,SF.PreRecruitmentEmployeeID
								,SF.IsMonthly,SF.IsYearlyCash,SF.IsYearlyNonCash,SF.IsLeave
                            FROM SCS.SalaryFixation SF
                            LEFT JOIN dbo.SalaryHead SH ON SF.SalaryHeadID = SH.SalaryHeadID
                            LEFT JOIN dbo.LeaveType LT ON SF.LeaveTypeID = LT.Id
                            WHERE SF.PreRecruitmentEmployeeID = '" + preRecEmpId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        private decimal GetFixationAmount(DataTable dt, string SalaryHeadId, out bool IsOpen)
        {
            decimal _r = 0;
            IsOpen = false;
            DataView dv = null;
            try
            {
                dv = new DataView(dt)
                {
                    RowFilter = "SalaryHeadID='" + SalaryHeadId + "'"
                };
                if (dv.Count > 0)
                {
                    _r = Convert.ToDecimal(dv[0]["EntryAmount"].ToString());
                    IsOpen = Convert.ToBoolean(dv[0][nameof(IsOpen)].ToString());
                }
                return _r;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dv = null;
                dt = null;
                SalaryHeadId = null;
            }
        }

        private void GetOPH(DataTable dt, string SalaryHeadId, out bool IsOpen)
        {
            IsOpen = false;
            DataView dv = null;
            try
            {
                dv = new DataView(dt)
                {
                    RowFilter = "SalaryHeadID='" + SalaryHeadId + "'"
                };
                if (dv.Count > 0)
                {
                    IsOpen = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dv = null;
                dt = null;
                SalaryHeadId = null;
            }
        }

        private void GetFixationAmount(IEnumerable<SalaryFixation> entities, ref DataTable dtOpenHead)
        {
            DataView dv = null;
            DataRow dr = null;
            try
            {
                dv = new DataView(dtOpenHead);
                for (int i = 0; i < dtOpenHead.Rows.Count; i++)
                {
                    var _id = dtOpenHead.Rows[i]["SalaryHeadID"].ToString();
                    var db = entities.FirstOrDefault(a => a.SalaryHeadID == _id);
                    dv.RowFilter = "SalaryHeadID='" + _id + "'";
                    if (dv.Count > 0 && db != null)
                    {
                        dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["Amount"] = db.FixationAmount;
                        dr.EndEdit();
                    }
                    dv.RowFilter = null;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        private void ValidateSalaryHead(IEnumerable<SalaryFixationVM> sfVM, DataTable dtSSSH)
        {
            DataView dv = null;
            try
            {
                dv = new DataView(dtSSSH);
                foreach (var item in sfVM)
                {
                    if (item.IsMonthly)
                    {
                        dv.RowFilter = "SalaryHeadId='" + item.SalaryHeadId + "'";
                        if (dv.Count == 0)
                        {
                            throw new Exception("Salary Head [" + item.SalaryHead + "] is not found in the Salary Rule ...");
                        }
                        dv.RowFilter = null;
                    }//monthly
                }
                //for (int i = 0; i < dtSSSH.Rows.Count; i++)
                //{
                //    string _shid = dtSSSH.Rows[i]["SalaryHeadId"].ToString();
                //    string _sh = dtSSSH.Rows[i]["SalaryHead"].ToString();
                //    var db = sfVM.Where(a => a.SalaryHeadId == _shid).FirstOrDefault();
                //    if(db==null || db.SalaryHeadId==null)
                //    {
                //        throw new Exception("Salary Head ["+_sh+"] is not found in the salary fixation setting...");
                //    }
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCalculationInfo(string preRecruitmentEmployeeId, string givenDesignationId, string plantId)
        {
            decimal _FAmount = 0;
            var _IsOpen = false;
            var _SalaryRuleMasterSystemID = string.Empty;
            var _iscalculated = false;
            try
            {
                var _SalaryHeadList = GetHeadList(preRecruitmentEmployeeId, givenDesignationId, plantId);//existing query
                foreach (var item in _SalaryHeadList)
                {
                    if (item.IsCalculated)
                    {
                        _iscalculated = true;
                        break;
                    }
                }

                GetOpenHead(preRecruitmentEmployeeId, out DataTable dtOpenVal); ;

                //ValidateSalaryHead(_SalaryHeadList, _SalaryStructure);
                foreach (var item in _SalaryHeadList)
                {
                    GetOPH(dtOpenVal, item.SalaryHeadId, out _IsOpen);
                    item.IsOpen = _IsOpen;
                    item.SalaryRuleId = _SalaryRuleMasterSystemID;
                    if (!_iscalculated)
                    {
                        item.FixationAmount = _FAmount;
                    }
                }
                return _SalaryHeadList;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void GetCalculationInfoFinal(IEnumerable<SalaryFixation> entities, string TotalSalary, string preRecruitmentEmployeeId, string givenDesignationId, string plantId, out IEnumerable<SalaryFixationVM> list)
        {
            decimal _FAmount = 0;
            var _IsOpen = false;
            list = null;
            var _fs = false;
            var _SalaryRuleMasterSystemID = string.Empty;

            //list =IEnumerable<SalaryFixationVM>();
            try
            {
                var _SalaryHeadList = GetHeadList(preRecruitmentEmployeeId, givenDesignationId, plantId);//existing query

                var _SalaryStructure = EmpSalaryDefine(preRecruitmentEmployeeId, TotalSalary, entities, out _SalaryRuleMasterSystemID);
                ValidateSalaryHead(_SalaryHeadList, _SalaryStructure);
                foreach (var item in _SalaryHeadList)
                {
                    if (item.IsAnnualNonCash)
                    {
                        var db = _SalaryHeadList.FirstOrDefault(a => a.SalaryHeadId == item.SalaryHeadId);
                        if (db != null && db.SalaryHeadId != null)
                        {
                            _fs = db.FixationStatusN;
                        }
                        item.FixationStatus = _fs;
                        item.IsOpen = false;  //_SalaryRuleMasterSystemID
                        item.SalaryRuleId = _SalaryRuleMasterSystemID;
                    }
                    else if (item.IsLeave)
                    {
                        var db = _SalaryHeadList.FirstOrDefault(a => a.SalaryHeadId == item.SalaryHeadId);
                        if (db != null && db.SalaryHeadId != null)
                        {
                            _fs = db.FixationStatusL;
                        }
                        item.FixationStatus = _fs;
                        item.IsOpen = false;  //_SalaryRuleMasterSystemID
                        item.SalaryRuleId = _SalaryRuleMasterSystemID;
                    }
                    else
                    {
                        _FAmount = GetFixationAmount(_SalaryStructure, item.SalaryHeadId, out _IsOpen);
                        item.FixationAmount = _FAmount;
                        item.IsOpen = _IsOpen;  //_SalaryRuleMasterSystemID
                        item.SalaryRuleId = _SalaryRuleMasterSystemID;
                    }
                }
                list = _SalaryHeadList;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        private void CheckAllowanceIsFoundInSalaryrule(DataTable dsSalaryHead, DataTable dsAllowance, DataTable dsLocationAllowance, string lblEmpName)
        {
            //this salary rule is found by given designation id>designation group>salary rule ap plant.
            var dvLocal = new DataView(dsSalaryHead);
            try
            {
                if (dsLocationAllowance.Rows.Count > 0)
                {
                    //Location Allowance
                    if (Convert.ToDecimal(dsLocationAllowance.Rows[0]["Allowance"].ToString()) > 0)
                    {
                        dvLocal.RowFilter = "HeadCategory='Location Allowance'";
                        if (dvLocal.Count == 0)
                        {
                            Throw(MSG_Salary_Calculate("Location Allowance", lblEmpName));
                        }
                    }
                    else
                    {
                        dvLocal.RowFilter = "HeadCategory='Location Allowance'";
                        if (dvLocal.Count == 0)
                        {
                            Throw(MSG_Salary_Calculate("Location Allowance", lblEmpName));
                        }
                    }
                }//dsLocationAllowance

                if (dsAllowance.Rows.Count > 0)
                {
                    //SkillAllowance
                    if (Convert.ToDecimal(dsAllowance.Rows[0]["SkillAllowance"].ToString()) > 0)
                    {
                        dvLocal.RowFilter = "HeadCategory='Skill Allowance'";
                        if (dvLocal.Count == 0)
                        {
                            Throw(MSG_Salary_Calculate("Skill Allowance", lblEmpName));
                        }
                    }
                    else
                    {
                        dvLocal.RowFilter = "HeadCategory='Skill Allowance'";
                        if (dvLocal.Count == 0)
                        {
                            Throw(MSG_Salary_Calculate("Skill Allowance", lblEmpName));
                        }
                    }

                    //ResponsibilityAllowance
                    dvLocal.RowFilter = null;
                    if (Convert.ToDecimal(dsAllowance.Rows[0]["ResponsibilityAllowance"].ToString()) > 0)
                    {
                        dvLocal.RowFilter = "HeadCategory='Responsibility Allowance'";
                        if (dvLocal.Count == 0)
                        {
                            Throw(MSG_Salary_Calculate("Responsibility Allowance", lblEmpName));
                        }
                    }
                    else
                    {
                        dvLocal.RowFilter = "HeadCategory='Responsibility Allowance'";
                        if (dvLocal.Count == 0)
                        {
                            Throw(MSG_Salary_Calculate("Responsibility Allowance", lblEmpName));
                        }
                    }

                    //SpecialAllowance
                    //dvLocal.RowFilter = null;
                    //if (Convert.ToDecimal(dsAllowance.Tables[0].Rows[0]["SpecialAllowance"].ToString()) > 0)
                    //{
                    //    dvLocal.RowFilter = "HeadCategory='Special Allowance'";
                    //    if (dvLocal.Count == 0)
                    //    {
                    //        bplib.clsWebLib.Throw(bplib.clsWebLib.MSG_Salary_Calculate("Special Allowance", lblEmpName.Text));
                    //    }
                    //}
                    //else
                    //{
                    //    dvLocal.RowFilter = "HeadCategory='Special Allowance'";
                    //    if (dvLocal.Count == 0)
                    //    {
                    //        bplib.clsWebLib.Throw(bplib.clsWebLib.MSG_Salary_Calculate("Special Allowance", lblEmpName.Text));
                    //    }
                    //}
                }//dsAllowance
                else
                {
                    dvLocal.RowFilter = "HeadCategory='Special Allowance'";
                    if (dvLocal.Count > 0)
                    {
                        Throw(lblEmpName + " has [Splecial allownace] in his Salary Rule but his Maximum salary is not defined...");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void Throw(string msg)
        {
            try
            {
                throw new Exception(msg);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string MSG_Salary_Calculate(string SalaryHead, string emp)
        {
            try
            {
                return " <b>" + SalaryHead + "</b> is not found in the applied Salary Rule for <b>" + emp + "</b>,</br> but as per BudgetCode he must have <b>" + SalaryHead + "</b>";
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GetAmountHeadCategoryWise(string HeadCategory, DataTable dsEntityAllowance, DataTable dsMBallowance, DataTable dsSalaryHeadId, DataTable dsCTCAmount, out string _allowance, out string _currency)
        {
            var _FORMAT_DECIMAL_2 = "0.00";
            _allowance = string.Empty;
            _currency = string.Empty;
            try
            {
                switch (HeadCategory)
                {
                    case "Skill Allowance":
                        if (dsMBallowance.Rows.Count > 0)
                        {
                            // _allowance = Convert.ToDecimal(dsMBallowance.Tables[0].Rows[0]["SkillAllowance"].ToString());
                            _allowance = dsMBallowance.Rows[0]["SkillAllowance"].ToString();
                            _currency = (dsMBallowance.Rows[0]["CurrencyId"].ToString());
                        }

                        break;

                    case "Responsibility Allowance":
                        if (dsMBallowance.Rows.Count > 0)
                        {
                            //_allowance = Convert.ToDecimal(dsMBallowance.Tables[0].Rows[0]["ResponsibilityAllowance"].ToString());
                            _allowance = dsMBallowance.Rows[0]["ResponsibilityAllowance"].ToString();
                            _currency = (dsMBallowance.Rows[0]["CurrencyId"].ToString());
                        }
                        break;

                    case "Special Allowance":
                        if (dsMBallowance.Rows.Count > 0)
                        {
                            decimal _ctc = 0;
                            var dv = new DataView(dsSalaryHeadId)
                            {
                                RowFilter = "HeadCategory='CTC'"
                            };
                            if (dv.Count > 0)
                            {
                                var headid = dv[0]["SalaryHeadID"].ToString();

                                var dva = new DataView(dsCTCAmount)
                                {
                                    RowFilter = "SalaryHeadID='" + headid + "'"
                                };
                                if (dva.Count > 0)
                                {
                                    _ctc = Convert.ToDecimal(dva[0]["Amount"]);
                                }
                            }

                            ///ctc-maxSal-3allowance
                            //_allowance = Convert.ToDecimal(dsMBallowance.Tables[0].Rows[0]["MaximumSalary"].ToString());
                            var vv = _ctc - Convert.ToDecimal(dsMBallowance.Rows[0]["MaximumSalary"].ToString());
                            _allowance = vv > 0 ? vv.ToString(_FORMAT_DECIMAL_2) : "0.00";
                            _currency = (dsMBallowance.Rows[0]["CurrencyId"].ToString());
                        }

                        break;

                    case "Location Allowance":
                        if (dsEntityAllowance.Rows.Count > 0)
                        {
                            //_allowance = Convert.ToDecimal(dsEntityAllowance.Tables[0].Rows[0]["Allowance"].ToString());
                            _allowance = (dsEntityAllowance.Rows[0]["Allowance"].ToString());
                            _currency = (dsEntityAllowance.Rows[0]["CurrencyId"].ToString());
                        }

                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetMBAllowance(string ManpowerBudgetId, string EffectiveDate, out DataTable dt)
        {
            string strSQL;
            dt = null;

            try
            {
                strSQL = @"SELECT top 1 *
                                FROM mst.manpowerbudgetallowance a
                                WHERE a.ManpowerBudgetId = '" + ManpowerBudgetId + @"'
	                                AND a.EffectiveDate<='" + EffectiveDate + @"'
									order by a.EffectiveDate desc";
                dt = _sqlRepository.GetDataTable(strSQL);
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        public void GetEntityAllowance(string EffectiveDate, string BudgetCodeId, string DesignationGroupId, string CompanyGroupId, out DataTable dt)
        {
            string strSQL;
            dt = null;
            try
            {
                strSQL = @" SELECT top 1*
                            FROM org.entityallowance a
                            WHERE a.DesignationGroupId = '" + DesignationGroupId + @"'
	                            AND entityid in (select EntityId from mst.ManpowerBudget where Id='" + BudgetCodeId + @"')
	                            AND CompanyGroupId = '" + CompanyGroupId + @"'
	                             AND a.EffectiveDate<='" + EffectiveDate + @"'
								 order by a.EffectiveDate desc";
                dt = _sqlRepository.GetDataTable(strSQL);
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        public void GetOpenHead(string sEmpSystemID, out DataTable dt)
        {
            string strSQL;
            dt = null;
            try
            {
                strSQL = @"
                        SELECT SG.SalaryHeadID
	                        , SH.SalaryHead
	                        , SH.Description
	                        , SM.SalaryRuleDescription
	                        , HeadType = CASE
                                WHEN HeadType = 'D' THEN 'Deduction'
                                WHEN HeadType = 'E' THEN 'Earning'
                                ELSE ''  END
	                        , SH.HeadCategory,0 Amount
                        FROM SalaryRuleGeneral SG
                        INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
                        LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
                        WHERE SG.IsOpen = 1
                            AND SG.SalaryRuleMasterSystemID = (
									                                select d.SalaryRuleMasterId
									                                from (SELECT DC.SalaryRuleMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                                                    WHERE DC.PlantId=(select PlantId from dbo.EmployeeInformation='"+ sEmpSystemID + @"')) d
									                                left outer join PreRecruitmentEmployee e on e.GivenDesignationId=d.DesignationId
									                                where e.id='" + sEmpSystemID + @"'
										                                )
                            AND SM.PlantID in (select plantid from PreRecruitmentEmployee where id='" + sEmpSystemID + @"')
                        GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, HeadType, SH.HeadCategory
                        ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                        ";
                dt = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        public void xxxGetOpenHead(string sEmpSystemID, out DataTable dt)
        {
            string strSQL;
            dt = null;
            try
            {
                strSQL = @"
                        SELECT SG.SalaryHeadID
	                        , SH.SalaryHead
	                        , SH.Description
	                        , SM.SalaryRuleDescription
	                        , HeadType = CASE
                                WHEN HeadType = 'D' THEN 'Deduction'
                                WHEN HeadType = 'E' THEN 'Earning'
                                ELSE ''  END
	                        , SH.HeadCategory,0 Amount
                        FROM SalaryRuleGeneral SG
                        INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
                        LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
                        WHERE SG.IsOpen = 1
                            AND SG.SalaryRuleMasterSystemID = (
									                                select d.SalaryRuleMasterId
									                                from mst.designationmaster d
									                                left outer join PreRecruitmentEmployee e on e.GivenDesignationId=d.DesignationId
									                                where e.id='" + sEmpSystemID + @"'
										                                )
                            AND SM.PlantID in (select plantid from PreRecruitmentEmployee where id='" + sEmpSystemID + @"')
                        GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, HeadType, SH.HeadCategory
                        ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                        ";
                dt = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        public void SalaryStructureAPHeadOnGrid(string PlantId, string sEmpSystemID, out DataTable dt)
        {
            string strSQL;
            dt = null;
            try
            {
                strSQL = @"   SELECT
	                                 A.SalaryRuleMasterSystemID
	                                , A.SalaryRuleName
	                                , A.SalaryHeadID
	                                , A.SalaryHead
	                                , HeadType = CASE
		                                WHEN A.HeadType = 'D'
			                                THEN 'Deduction'
		                                WHEN A.HeadType = 'E'
			                                THEN 'Earning'
		                                ELSE ''
		                                END

	                                , A.FormulaDes
	                                , A.FormulaDesID
	                                , A.FixedValue
	                                , A.IsOpen
	                                , A.IsNA
	                                , 0 EntryAmount
	                                , 0 DefineAmount
	                                , A.SequenceNo
	                                , HeadCategory

                                FROM (
	                                SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName
		                                , SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID
		                                , TagAndUnTag = CASE
			                                WHEN Fml.TagAndUnTag = '1'
				                                THEN Fml.TagAndUnTag
			                                WHEN Fxd.TagAndUnTag = '1'
				                                THEN Fxd.TagAndUnTag
			                                WHEN SG.IsGNRTagAndUnTag = '1'
				                                THEN SG.IsGNRTagAndUnTag
			                                ELSE Convert(BIT, 'False')
			                                END
		                                , SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID
		                                , CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, Fml.FormulaDes, Fml.FormulaDesID, ISNULL(Fxd.FixedValue, 0) FixedValue
		                                , ISNULL(SG.IsOpen, 0) IsOpen, isnull(SG.IsNA, 0) IsNA, SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0))
		                                , SH.HeadCategory

	                                FROM SalaryRuleMaster SM
	                                LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
	                                LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID

	                                LEFT JOIN (
		                                SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral	WHERE IsFormula = 1
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo
			                                FROM SalaryRuleAbsenteeism	WHERE IsFormula = 1)
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes
				                                , ('( ' + PerSalaryHeadID + ' * ' + Convert(VARCHAR(10), PerValue) + ' ) / 100') AS FormulaDesID
				                                , SequenceNo
			                                FROM SalaryRuleDayStatusMaster	WHERE IsPercemtage = 1)
		                                ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID

	                                LEFT JOIN (
		                                SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleGeneral	WHERE (	IsFixed = 1	OR IsNA = 1	)
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism	WHERE IsFixed = 1)
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster	WHERE IsFixed = 1)
		                                ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID	AND CRC.SalaryHeadID = Fxd.SalaryHeadID

	                                LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID
		                                AND CRC.SalaryHeadID = SG.SalaryHeadID
		                                AND (
			                                SG.IsOpen = 1
			                                OR SG.IsNA = 1
			                                )
	                                ) A
                                WHERE A.SequenceNo > 0
	                                AND A.SalaryRuleMasterSystemID = (
									                                select d.SalaryRuleMasterId
									                                from (
									                                select d.SalaryRuleMasterId
									                                from   (SELECT DC.SalaryRuleMasterId,DM.DesignationId FROM MST.DesignationMaster DM
LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
WHERE DC.PlantId='"+PlantId+@"') d
									                                left outer join PreRecruitmentEmployee e on e.GivenDesignationId=d.DesignationId
									                                where e.id='" + sEmpSystemID + @"' and e.PlantId='" + PlantId + @"'
										                                )

	                                AND ISNULL(A.HeadCategory, '') != 'Tax'
	                                AND A.PlantID = '" + PlantId + @"'

                                ORDER BY A.SequenceNo ASC";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                dt = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        public void xxxSalaryStructureAPHeadOnGrid(string PlantId, string sEmpSystemID, out DataTable dt)
        {
            string strSQL;
            dt = null;
            try
            {
                strSQL = @"   SELECT
	                                 A.SalaryRuleMasterSystemID
	                                , A.SalaryRuleName
	                                , A.SalaryHeadID
	                                , A.SalaryHead
	                                , HeadType = CASE
		                                WHEN A.HeadType = 'D'
			                                THEN 'Deduction'
		                                WHEN A.HeadType = 'E'
			                                THEN 'Earning'
		                                ELSE ''
		                                END

	                                , A.FormulaDes
	                                , A.FormulaDesID
	                                , A.FixedValue
	                                , A.IsOpen
	                                , A.IsNA
	                                , 0 EntryAmount
	                                , 0 DefineAmount
	                                , A.SequenceNo
	                                , HeadCategory

                                FROM (
	                                SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName
		                                , SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID
		                                , TagAndUnTag = CASE
			                                WHEN Fml.TagAndUnTag = '1'
				                                THEN Fml.TagAndUnTag
			                                WHEN Fxd.TagAndUnTag = '1'
				                                THEN Fxd.TagAndUnTag
			                                WHEN SG.IsGNRTagAndUnTag = '1'
				                                THEN SG.IsGNRTagAndUnTag
			                                ELSE Convert(BIT, 'False')
			                                END
		                                , SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID
		                                , CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, Fml.FormulaDes, Fml.FormulaDesID, ISNULL(Fxd.FixedValue, 0) FixedValue
		                                , ISNULL(SG.IsOpen, 0) IsOpen, isnull(SG.IsNA, 0) IsNA, SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0))
		                                , SH.HeadCategory

	                                FROM SalaryRuleMaster SM
	                                LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
	                                LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID

	                                LEFT JOIN (
		                                SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral	WHERE IsFormula = 1
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo
			                                FROM SalaryRuleAbsenteeism	WHERE IsFormula = 1)
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes
				                                , ('( ' + PerSalaryHeadID + ' * ' + Convert(VARCHAR(10), PerValue) + ' ) / 100') AS FormulaDesID
				                                , SequenceNo
			                                FROM SalaryRuleDayStatusMaster	WHERE IsPercemtage = 1)
		                                ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID

	                                LEFT JOIN (
		                                SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleGeneral	WHERE (	IsFixed = 1	OR IsNA = 1	)
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism	WHERE IsFixed = 1)
		                                UNION
		                                (SELECT SalaryRuleMasterSystemID	, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster	WHERE IsFixed = 1)
		                                ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID	AND CRC.SalaryHeadID = Fxd.SalaryHeadID

	                                LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID
		                                AND CRC.SalaryHeadID = SG.SalaryHeadID
		                                AND (
			                                SG.IsOpen = 1
			                                OR SG.IsNA = 1
			                                )
	                                ) A
                                WHERE A.SequenceNo > 0
	                                AND A.SalaryRuleMasterSystemID = (
									                                select d.SalaryRuleMasterId
									                                from mst.designationmaster d
									                                left outer join PreRecruitmentEmployee e on e.GivenDesignationId=d.DesignationId
									                                where e.id='" + sEmpSystemID + @"' and e.PlantId='" + PlantId + @"'
										                                )

	                                AND ISNULL(A.HeadCategory, '') != 'Tax'
	                                AND A.PlantID = '" + PlantId + @"'

                                ORDER BY A.SequenceNo ASC";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                dt = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        private void SetValue(DataRow drSource, ref DataRow dr)
        {
            try
            {
                // dr["IsSelectSlrHd"] = Convert.ToBoolean(drSource["IsSelectSlrHd"].ToString().Trim());
                //dr["SlrInfoDefSystemID"] = drSource["SlrInfoDefSystemID"].ToString().Trim();
                //dr["CurrencyRuleChildSystemID"] = drSource["CurrencyRuleChildSystemID"].ToString().Trim();
                dr["SalaryHeadID"] = drSource["SalaryHeadID"].ToString().Trim();
                dr["SalaryHead"] = drSource["SalaryHead"].ToString().Trim();
                dr["HeadCategory"] = drSource["HeadCategory"].ToString().Trim();
                dr["HeadType"] = drSource["HeadType"].ToString().Trim();
                dr["FormulaDesID"] = drSource["FormulaDesID"].ToString().Trim();
                dr["FixedValue"] = drSource["FixedValue"].ToString().Trim();
                dr["IsOpen"] = Convert.ToBoolean(drSource["IsOpen"].ToString().Trim());
                dr["IsNA"] = Convert.ToBoolean(drSource["IsNA"].ToString().Trim());
                //dr["EntryCurrencyID"] = drSource["EntryCurrencyID"].ToString().Trim();
                // dr["EntryCurrency"] = drSource["EntryCurrency"].ToString().Trim();
                //dr["DefinitionCurrencyID"] = drSource["DefinitionCurrencyID"].ToString().Trim();
                //dr["DefinitionCurrency"] = drSource["DefinitionCurrency"].ToString().Trim();
                dr["HeadCategory"] = drSource["HeadCategory"].ToString().Trim();

                //string strTempEntryAmt = "0.0";
                //if (drSource["HeadCategory"].ToString().Trim().ToUpper() == "CTC")
                //{
                //    strTempEntryAmt = _CTC.ToString();
                //}
                //else
                //{
                //    strTempEntryAmt = "0.0";
                //}

                //dr["EntryAmount"] = strTempEntryAmt;
                //dr["TagAndUnTag"] = drSource["TagAndUnTag"].ToString().Trim();
                //dr["MonthPeriod"] = drSource["MonthPeriod"].ToString().Trim();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable EmpSalaryDefine(string EmployeeSystemId, string TotalSalary, IEnumerable<SalaryFixation> entities, out string _SalaryRuleMasterSystemID)
        {
            DataTable dsLocal = null;
            DataTable dsOpenVal = null;
            DataView dvOpenValFil = null;
            DataView dv = null;
            DataTable dsMBAllowance = null;
            DataTable dsEntityAllowance = null;
            var _FORMAT_DECIMAL_2 = "0.00";

            var _PlantId = string.Empty;
            var _EmployeeName = string.Empty;//name
            var _TextEffectiveDate = string.Empty;//doj
            var _lblBudgetCodeId = string.Empty;//budgetcode
            var _CG = string.Empty;
            var _DG = string.Empty;//designation
            decimal _CTC = 0;//totalsalary
            _SalaryRuleMasterSystemID = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _CG = identity.CompanyGroupId;
                //objsi = new clsSalaryInfo();
                var emp = GetEmpinfo(EmployeeSystemId);
                _PlantId = emp.PlantId;
                _EmployeeName = emp.EmployeeName;
                if (emp.AgreedDOJ != null)
                {
                    _TextEffectiveDate = Convert.ToDateTime(emp.AgreedDOJ).ToString("dd-MMM-yyyy");
                }
                else
                {
                    _TextEffectiveDate = "01-Jan-1980";
                }
                _lblBudgetCodeId = emp.BudgetId;
                _DG = emp.GivenDesignationId;
                _CTC = Convert.ToDecimal(TotalSalary);

                if (string.IsNullOrEmpty(_DG))
                {
                    throw new Exception("'Given Designation' is not found...");
                }
                if (string.IsNullOrEmpty(_lblBudgetCodeId))
                {
                    throw new Exception("'Budget Code' is not found...");
                }
                if (string.IsNullOrEmpty(_PlantId))
                {
                    throw new Exception("'Plant' is not found...");
                }
                if (string.IsNullOrEmpty(_TextEffectiveDate))
                {
                    throw new Exception("'DOJ' is not found...");
                }

                SalaryStructureAPHeadOnGrid(_PlantId, EmployeeSystemId, out dsLocal);
                if (dsLocal.Rows.Count > 0)
                {
                    _SalaryRuleMasterSystemID = dsLocal.Rows[0]["SalaryRuleMasterSystemID"].ToString();
                }
                ///LoadDataSetFromDataGrid(ref dgEmpSalaryDefine, out dsLocal);
                GetOpenHead(EmployeeSystemId, out dsOpenVal);

                if (entities != null)
                {
                    GetFixationAmount(entities, ref dsOpenVal);
                }

                /// LoadDataSetFromDataGrid(ref dgSalaryOpen, out dsOpenVal);

                GetMBAllowance(_lblBudgetCodeId, _TextEffectiveDate, out dsMBAllowance);
                GetEntityAllowance(_TextEffectiveDate, _lblBudgetCodeId, _DG, _CG, out dsEntityAllowance);

                CheckAllowanceIsFoundInSalaryrule(dsLocal, dsMBAllowance, dsEntityAllowance, _EmployeeName);

                var dtValue = new DataTable
                {
                    TableName = "TempTable"
                };
                dtValue.Columns.Add("SalaryHeadID");
                dtValue.Columns.Add("EntryCurrencyID");
                dtValue.Columns.Add("Amount");

                var strTempEntryAmt = "0.0";
                var strTempDefineAmt = "0.0";

                var strFormulaID = "";
                var strFormulaResult = "0.0";
                var strFormulaResultEnt = "";

                var dt = new DataTable
                {
                    TableName = "TempTable"
                };
                dt.Columns.Add("IsSelectSlrHd", typeof(bool));                  //0
                dt.Columns.Add("SlrInfoDefSystemID");                           //1
                dt.Columns.Add("CurrencyRuleChildSystemID");                    //2
                dt.Columns.Add("SalaryHeadID");                                 //3
                dt.Columns.Add("SalaryHead");                                   //4
                dt.Columns.Add("HeadType");                                     //5
                dt.Columns.Add("FormulaDesID");                                 //6
                dt.Columns.Add("FixedValue");                                   //7
                dt.Columns.Add("IsOpen");        //8
                dt.Columns.Add("EntryCurrencyID");                              //9
                dt.Columns.Add("EntryCurrency");                                //10
                dt.Columns.Add("DefinitionCurrencyID");                         //11
                dt.Columns.Add("DefinitionCurrency");                           //12
                dt.Columns.Add("EntryAmount");                                  //13
                dt.Columns.Add("DefineAmount");                                 //14
                dt.Columns.Add("TagAndUnTag");                                  //15
                dt.Columns.Add("MonthPeriod");
                dt.Columns.Add("IsNA");    //16
                dt.Columns.Add("HeadCategory");    //16

                if (dsLocal.Rows.Count > 0)
                {
                    //ISNA loop

                    for (int i = 0; i < dsLocal.Rows.Count; i++)//non formula
                    {
                        if ((Convert.ToBoolean(dsLocal.Rows[i]["IsNA"].ToString().Trim())) || (Convert.ToBoolean(dsLocal.Rows[i]["IsOpen"].ToString().Trim())) || (Convert.ToDecimal(dsLocal.Rows[i]["FixedValue"].ToString().Trim()) > 0))
                        {
                            //lblDefine.Text = "1";

                            strFormulaID = "";
                            strFormulaResult = "0.0";
                            strFormulaResultEnt = "";

                            strTempEntryAmt = "0.0";
                            strTempDefineAmt = "0.0";

                            dvOpenValFil = new DataView(dsOpenVal);

                            var dtRow = dt.NewRow();
                            SetValue(dsLocal.Rows[i], ref dtRow);

                            #region For Open Value

                            if (Convert.ToBoolean(dsLocal.Rows[i]["IsOpen"].ToString().Trim()))
                            {
                                dvOpenValFil.RowFilter = "SalaryHeadID = '" + dsLocal.Rows[i]["SalaryHeadID"].ToString().Trim() + "'";
                                if (dvOpenValFil.Count == 1)
                                {
                                    strTempEntryAmt = dsLocal.Rows[i]["HeadCategory"].ToString().Trim().ToUpper() == "CTC" ? _CTC.ToString() : dvOpenValFil[0]["Amount"].ToString().Trim();
                                    if (string.IsNullOrEmpty(strTempEntryAmt.Trim()))
                                    { strTempEntryAmt = "0.0"; }

                                    strTempDefineAmt = strTempEntryAmt;
                                }

                                if (entities == null)//no user given amount. i.e. calculating by pre-emp totalSalary as per G.designation
                                {
                                    if (dsLocal.Rows[i]["HeadCategory"].ToString().Trim().ToUpper() == "CTC")
                                    {
                                        strTempEntryAmt = _CTC.ToString();
                                    }
                                }
                                dtRow["EntryAmount"] = strTempEntryAmt;

                                #region For SalaryHead Wise Amount In Virtual 2nd Table

                                var dtValueRow = dtValue.NewRow();

                                dtValueRow["SalaryHeadID"] = dsLocal.Rows[i]["SalaryHeadID"].ToString().Trim();
                                dtValueRow["Amount"] = strTempEntryAmt;

                                dtValue.Rows.Add(dtValueRow);

                                #endregion For SalaryHead Wise Amount In Virtual 2nd Table
                            }

                            #endregion For Open Value

                            #region For Fixed Value

                            else if (Convert.ToDecimal(dsLocal.Rows[i]["FixedValue"].ToString().Trim()) > 0)
                            {
                                dtRow["EntryAmount"] = dsLocal.Rows[i]["FixedValue"].ToString().Trim();

                                #region For SalaryHead Wise Amount In Virtual 2nd Table

                                var dtValueRow = dtValue.NewRow();

                                dtValueRow["SalaryHeadID"] = dsLocal.Rows[i]["SalaryHeadID"].ToString().Trim();
                                dtValueRow["Amount"] = dsLocal.Rows[i]["FixedValue"].ToString().Trim();

                                dtValue.Rows.Add(dtValueRow);

                                #endregion For SalaryHead Wise Amount In Virtual 2nd Table
                            }

                            #endregion For Fixed Value

                            #region For NA

                            if (Convert.ToBoolean(dsLocal.Rows[i]["IsNA"].ToString().Trim()))
                            {
                                var _currency = string.Empty;
                                GetAmountHeadCategoryWise(dsLocal.Rows[i]["HeadCategory"].ToString(), dsEntityAllowance, dsMBAllowance, dsLocal, dsOpenVal, out strTempEntryAmt, out _currency);

                                if (string.IsNullOrEmpty(strTempEntryAmt.Trim()))
                                { strTempEntryAmt = "0.0"; }

                                if (string.IsNullOrEmpty(strTempDefineAmt.Trim()))
                                { strTempDefineAmt = "0.0"; }

                                dtRow["EntryAmount"] = strTempEntryAmt;

                                #region For SalaryHead Wise Amount In Virtual 2nd Table

                                var dtValueRow = dtValue.NewRow();

                                dtValueRow["SalaryHeadID"] = dsLocal.Rows[i]["SalaryHeadID"].ToString().Trim();
                                dtValueRow["Amount"] = strTempEntryAmt;

                                dtValue.Rows.Add(dtValueRow);

                                #endregion For SalaryHead Wise Amount In Virtual 2nd Table
                            }

                            #endregion For NA

                            dt.Rows.Add(dtRow);
                        }
                    }

                    for (int i = 0; i < dsLocal.Rows.Count; i++)//for Formula
                    {
                        var sh = dsLocal.Rows[i]["SalaryHead"].ToString();
                        if ((!Convert.ToBoolean(dsLocal.Rows[i]["IsNA"].ToString().Trim())) && (!Convert.ToBoolean(dsLocal.Rows[i]["IsOpen"].ToString().Trim())) && (Convert.ToDecimal(dsLocal.Rows[i]["FixedValue"].ToString().Trim()) == 0))
                        {
                            strFormulaID = "";
                            strFormulaResult = "0.0";
                            strFormulaResultEnt = "";

                            strTempEntryAmt = "0.0";
                            strTempDefineAmt = "0.0";

                            var dtRow = dt.NewRow();
                            SetValue(dsLocal.Rows[i], ref dtRow);

                            #region For Formula Value

                            if (!string.IsNullOrEmpty(dsLocal.Rows[i]["FormulaDesID"].ToString().Trim()))
                            {
                                strFormulaID = dsLocal.Rows[i]["FormulaDesID"].ToString().Trim();
                                var lblFormulaValue = string.Empty;
                                ReLoadFormulaWithValue(strFormulaID, ref dtValue, out lblFormulaValue);

                                strFormulaResult = Evaluate(lblFormulaValue.Trim()).ToString();
                                strFormulaResultEnt = strFormulaResult;

                                if (Convert.ToDecimal(strFormulaResultEnt) < 0)
                                {
                                    throw new Exception("'Open value' must be increased...otherwise [" + dsLocal.Rows[i]["SalaryHead"].ToString().Trim() + "] would be negetive (" + Convert.ToDecimal(strFormulaResultEnt).ToString(_FORMAT_DECIMAL_2) + ")...");
                                }
                                dtRow["EntryAmount"] = Convert.ToDecimal(strFormulaResultEnt).ToString(_FORMAT_DECIMAL_2);

                                #region For SalaryHead Wise Amount In Virtual 2nd Table

                                var dtValueRow = dtValue.NewRow();

                                dtValueRow["SalaryHeadID"] = dsLocal.Rows[i]["SalaryHeadID"].ToString().Trim();
                                dtValueRow["Amount"] = strFormulaResultEnt;

                                dtValue.Rows.Add(dtValueRow);

                                #endregion For SalaryHead Wise Amount In Virtual 2nd Table
                            }

                            #endregion For Formula Value

                            #region For NULL Value

                            else
                            {
                                dtRow["EntryAmount"] = dsLocal.Rows[i]["EntryAmount"].ToString().Trim();
                            }

                            #endregion For NULL Value

                            dt.Rows.Add(dtRow);
                        }
                    }

                    //ValidationOpenvalue(dt);
                    dv = new DataView
                    {
                        Table = dt
                    };
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        private void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            var strTemp = "";

            try
            {
                dsLocal = new DataSet();

                var strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                var strIdCol = strFormulaIDTemp.Split(' ');

                var dt = new DataTable
                {
                    TableName = "IDLIST"
                };
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView
                        {
                            Table = dtValue,

                            RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'"
                        };
                        if (dvLocal.Count == 1)
                        {
                            //if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == lblLocalCurrencyID.Text.Trim())
                            //{
                            strTemp = dvLocal[0]["Amount"].ToString().Trim();
                            //}
                            //else
                            //{
                            //    strTemp = (Convert.ToDecimal(dvLocal[0]["Amount"].ToString().Trim()) * Convert.ToDecimal(txtForeignCurRate.Text.Trim())).ToString();
                            //}
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        public static double Evaluate(string expression)
        {
            // That is some code instruction, is'nt it?
            return (double)new System.Xml.XPath.XPathDocument
            (new StringReader("<r/>")).CreateNavigator().Evaluate
            ($"number({new System.Text.RegularExpressions.Regex(@"([\+\-\*])").Replace(expression, " ${1} ").Replace("/", " div ").Replace("%", " mod ")})");
        }//End Function

        public IEnumerable<object> GetGDAndEmpWiseSalaryHeadList(string preRecruitmentEmployeeId, string givenDesignationId)
        {
            try
            {
                var sql = @"SELECT ISNULL(SH.SalaryHead, '') + ISNULL(LT.Description, '')+ ISNULL(ANC.UserName, '') SalaryHead
								,ISNULL(D.SalaryHeadID, '') + ISNULL(D.LeaveTypeID, '')+ ISNULL(D.AnnualNonCashId, '') SalaryHeadId

								,ISNULL(SFSH.CurrentAmount, 0)  CurrentAmount
								,ISNULL(SFSH.ExpectedAmount, 0)  ExpectedAmount
                        		,ISNULL(SFSH.FixationAmount, 0)  FixationAmount

                                ,ISNULL(SFLT.CurrentStatus, 0)+ ISNULL(SFync.CurrentStatus, 0) CurrentStatus
								,ISNULL(SFLT.ExpectedStatus, 0)+ ISNULL(SFync.ExpectedStatus, 0) ExpectedStatus
                        		,ISNULL(SFLT.FixationStatus, 0)+ ISNULL(SFync.FixationStatus, 0) FixationStatus

								,D.SalFixSetId,D.IsMonthly,D.IsAnnualCash,D.IsAnnualNonCash,D.IsLeave,isnull(SFSH.IsCalculated,0) IsCalculated
							    ,ISNULL(SFSH.Id, '') + ISNULL(SFLT.Id, '') + ISNULL(SFync.Id, '') Id,SFSH.SalaryRuleID

							FROM SCS.SalaryFixationSettingDetails D
							LEFT JOIN (SELECT DC.SalaryFixationSettingId,DM.DesignationId FROM MST.DesignationMaster DM LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) DM ON D.SalFixSetId = DM.SalaryFixationSettingId
							LEFT JOIN dbo.SalaryHead SH ON D.SalaryHeadID = SH.SalaryHeadID
							LEFT JOIN dbo.LeaveType LT ON D.LeaveTypeID = LT.Id
							LEFT JOIN hkp.AnnualNonCash ANC ON D.AnnualNonCashId = ANC.Id
							LEFT JOIN (SELECT * FROM SCS.SalaryFixation
								WHERE PreRecruitmentEmployeeID = '" + preRecruitmentEmployeeId + @"'
								) SFSH ON SFSH.SalaryHeadID = D.SalaryHeadID

							LEFT JOIN (SELECT * FROM SCS.SalaryFixation
								WHERE PreRecruitmentEmployeeID = '" + preRecruitmentEmployeeId + @"'
								) SFLT ON SFLT.LeaveTypeID = D.LeaveTypeId

								LEFT JOIN (SELECT * FROM SCS.SalaryFixation
								WHERE PreRecruitmentEmployeeID = '" + preRecruitmentEmployeeId + @"'
								) SFync ON SFync.AnnualNonCashId = D.AnnualNonCashId

							WHERE DM.DesignationId = '" + givenDesignationId + "' order by D.SequenceNo";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<SalaryFixationVM> GetHeadList(string preRecruitmentEmployeeId, string givenDesignationId, string plantId)
        {
            IEnumerable<SalaryFixationVM> _returnlist = null;
            try
            {
                var sql = @"SELECT ISNULL(SH.SalaryHead, '') + ISNULL(LT.Description, '')+ ISNULL(ANC.UserName, '') SalaryHead
								,ISNULL(D.SalaryHeadID, '') + ISNULL(D.LeaveTypeID, '')+ ISNULL(D.AnnualNonCashId, '') SalaryHeadId

								,ISNULL(SFSH.CurrentAmount, 0)  CurrentAmount
								,ISNULL(SFSH.ExpectedAmount, 0)  ExpectedAmount
                        		,ISNULL(SFSH.FixationAmount, 0)  FixationAmount

                                --,ISNULL(SFLT.CurrentStatus, false)+ ISNULL(SFync.CurrentStatus, false) CurrentStatus
								--,ISNULL(SFLT.ExpectedStatus, false)+ ISNULL(SFync.ExpectedStatus, false) ExpectedStatus
                        		--,ISNULL(SFLT.FixationStatus, false)+ ISNULL(SFync.FixationStatus, false) FixationStatus

                                ,ISNULL(SFLT.CurrentStatus, 0) CurrentStatusL
								,ISNULL(SFLT.ExpectedStatus, 0) ExpectedStatusL
                        		,ISNULL(SFLT.FixationStatus, 0) FixationStatusL

								  ,ISNULL(SFync.CurrentStatus, 0) CurrentStatusN
								,ISNULL(SFync.ExpectedStatus, 0) ExpectedStatusN
                        		,ISNULL(SFync.FixationStatus, 0) FixationStatusN

								,D.SalFixSetId,D.IsMonthly,D.IsAnnualCash,D.IsAnnualNonCash,D.IsLeave,isnull(SFSH.IsCalculated,0) IsCalculated
							    ,ISNULL(SFSH.Id, '') + ISNULL(SFLT.Id, '') + ISNULL(SFync.Id, '') Id,SFSH.SalaryRuleId

							FROM SCS.SalaryFixationSettingDetails D
                            LEFT JOIN (SELECT DC.LeavePolicyMasterId,DC.SalaryFixationSettingId,DM.DesignationId FROM MST.DesignationMaster DM 
                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId 
                            WHERE DC.PlantId='" + plantId+@"')DM ON D.SalFixSetId = DM.SalaryFixationSettingId
							--LEFT JOIN MST.DesignationMaster DM ON D.SalFixSetId = DM.SalaryFixationSettingId
							LEFT JOIN dbo.SalaryHead SH ON D.SalaryHeadID = SH.SalaryHeadID
							LEFT JOIN dbo.LeaveType LT ON D.LeaveTypeID = LT.Id
							LEFT JOIN SCS.AnnualNonCash ANC ON D.AnnualNonCashId = ANC.Id
							LEFT JOIN (SELECT * FROM SCS.SalaryFixation
								WHERE PreRecruitmentEmployeeID = '" + preRecruitmentEmployeeId + @"' AND PlantId='" + plantId + @"'
								) SFSH ON SFSH.SalaryHeadID = D.SalaryHeadID

							LEFT JOIN (SELECT * FROM SCS.SalaryFixation
								WHERE PreRecruitmentEmployeeID = '" + preRecruitmentEmployeeId + @"' AND PlantId='" + plantId + @"'
								) SFLT ON SFLT.LeaveTypeID = D.LeaveTypeId

								LEFT JOIN (SELECT * FROM SCS.SalaryFixation
								WHERE PreRecruitmentEmployeeID = '" + preRecruitmentEmployeeId + @"' AND PlantId='" + plantId + @"'
								) SFync ON SFync.AnnualNonCashId = D.AnnualNonCashId

							WHERE DM.DesignationId = '" + givenDesignationId + @"' order by D.SequenceNo";
                // return _sqlRepository.ModelDataCollection(sql, null);
                _returnlist = _sqlRepository.GetModelCollection<SalaryFixationVM>(sql, null);
                return _returnlist;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public PreRecruitmentEmployee GetEmpinfo(string preRecruitmentEmployeeId)
        {
            try
            {
                var sql = @"SELECT * FROM PreRecruitmentEmployee WHERE Id='" + preRecruitmentEmployeeId + @"'";
                // return _sqlRepository.ModelDataCollection(sql, null);
                return _sqlRepository.GetModelCollection<PreRecruitmentEmployee>(sql, null).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        private string GetPK()
        {
            return base.GetAutoNumber(nameof(SalaryFixation), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private string GetTCPK()
        {
            return base.GetAutoNumber(nameof(EmployeeWiseTermsAndConditions), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void XInsertOrUpdateGraph(IEnumerable<SalaryFixation> entities, string companyGroupId)
        {
            List<SalaryFixation> from_db = null;
            var flag = false;
            var _empid = string.Empty;

            try
            {
                foreach (SalaryFixation item in entities)
                {
                    _empid = item.PreRecruitmentEmployeeID;
                    break;
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                from_db = GetDetailList(_empid).ToList<SalaryFixation>();
                var pk = GetPK();
                var count = 0;
                foreach (SalaryFixation salaryFixations in entities)
                {
                    count++;

                    var salaryFixationDb = from_db.FirstOrDefault(r => r.Id == salaryFixations.Id);
                    if (from_db.Any(r => r.Id == salaryFixations.Id))
                    {
                        if (salaryFixations.IsLeave)
                        {
                            salaryFixations.LeaveTypeID = salaryFixations.SalaryHeadID;
                            salaryFixations.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixations.ExpectedStatus = salaryFixations.ExpectedStatus;
                            //salaryFixations.FixationStatus = salaryFixations.FixationStatusL;
                        }
                        else if (salaryFixations.IsAnnualNonCash)
                        {
                            salaryFixations.AnnualNonCashId = salaryFixations.SalaryHeadID;
                            salaryFixations.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixations.ExpectedStatus = salaryFixations.ExpectedStatus;
                            //salaryFixations.FixationStatus = salaryFixations.FixationStatusN;
                        }
                        else
                        {
                            salaryFixations.SalaryHeadID = salaryFixations.SalaryHeadID;
                            salaryFixations.CurrentAmount = salaryFixations.CurrentAmount;
                            salaryFixations.ExpectedAmount = salaryFixations.ExpectedAmount;
                            //salaryFixations.FixationAmount = salaryFixations.FixationAmount;
                        }
                        salaryFixations.IsCalculated = false;
                        //salaryFixations.FixationSetID = salaryFixations.SalFixSetId;

                        salaryFixations.ModelState = ModelState.Modified;
                        AuditService.Log(salaryFixations);
                        base.InsertOrUpdateGraph(salaryFixations);
                    }
                    else
                    {
                        salaryFixations.Id = "F" + pk + "-" + count;

                        if (salaryFixations.IsLeave)
                        {
                            salaryFixations.LeaveTypeID = salaryFixations.SalaryHeadID;
                            salaryFixations.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixations.ExpectedStatus = salaryFixations.ExpectedStatus;
                            //salaryFixations.FixationStatus = salaryFixations.FixationStatus;
                        }
                        else if (salaryFixations.IsAnnualNonCash)
                        {
                            salaryFixations.AnnualNonCashId = salaryFixations.SalaryHeadID;
                            salaryFixations.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixations.ExpectedStatus = salaryFixations.ExpectedStatus;
                            //salaryFixations.FixationStatus = salaryFixations.FixationStatus;
                        }
                        else
                        {
                            salaryFixations.SalaryHeadID = salaryFixations.SalaryHeadID;
                            salaryFixations.CurrentAmount = salaryFixations.CurrentAmount;
                            salaryFixations.ExpectedAmount = salaryFixations.ExpectedAmount;
                            //salaryFixations.FixationAmount = salaryFixations.FixationAmount;
                        }

                        //salaryFixations.FixationSetID = salaryFixations.SalFixSetId;
                        salaryFixations.PlantId = salaryFixations.PlantId;
                        salaryFixations.CompanyGroupID = companyGroupId;
                        salaryFixations.IsCalculated = false;
                        AuditService.Log(salaryFixations);
                        Insert(salaryFixations);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdateGraph(IEnumerable<SalaryFixation> entities, string companyGroupId, string plantid)
        {
            List<SalaryFixation> from_db = null;
            var flag = false;
            var _empid = string.Empty;
            try
            {
                foreach (SalaryFixation item in entities)
                {
                    _empid = item.PreRecruitmentEmployeeID;
                    break;
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                from_db = GetDetailList(_empid).ToList<SalaryFixation>();
                var pk = GetPK();
                var count = 0;
                foreach (SalaryFixation salaryFixations in entities)
                {
                    var salaryFixationDb = from_db.FirstOrDefault(r => r.Id == salaryFixations.Id);
                    if (from_db.Any(r => r.Id == salaryFixations.Id))
                    {
                        salaryFixationDb.CurrentAmount = salaryFixations.CurrentAmount;
                        salaryFixationDb.ExpectedAmount = salaryFixations.ExpectedAmount;

                        if (salaryFixations.IsLeave)
                        {
                            salaryFixationDb.LeaveTypeID = salaryFixations.SalaryHeadID;
                            salaryFixationDb.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixationDb.ExpectedStatus = salaryFixations.ExpectedStatus;
                            salaryFixationDb.IsLeave = salaryFixations.IsLeave;
                        }
                        else if (salaryFixations.IsAnnualNonCash)
                        {
                            salaryFixationDb.AnnualNonCashId = salaryFixations.SalaryHeadID;
                            salaryFixationDb.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixationDb.ExpectedStatus = salaryFixations.ExpectedStatus;
                            salaryFixationDb.IsAnnualNonCash = salaryFixations.IsAnnualNonCash;
                        }
                        else
                        {
                            salaryFixationDb.SalaryHeadID = salaryFixations.SalaryHeadID;
                            //salaryFixationDb.FixationStatus = salaryFixations.FixationStatus;
                            salaryFixationDb.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixationDb.ExpectedStatus = salaryFixations.ExpectedStatus;
                            salaryFixationDb.IsMonthly = salaryFixations.IsMonthly;
                            salaryFixationDb.IsAnnualCash = salaryFixations.IsAnnualCash;
                        }

                        salaryFixationDb.SalaryRuleId = salaryFixations.SalaryRuleId;

                        salaryFixationDb.ModelState = ModelState.Modified;
                        AuditService.Log(salaryFixationDb);
                        base.InsertOrUpdateGraph(salaryFixationDb);
                    }
                    else
                    {
                        count++;
                        salaryFixationDb = new SalaryFixation
                        {
                            Id = "F" + pk + "-" + count
                        };

                        if (salaryFixations.IsLeave)
                        {
                            salaryFixationDb.LeaveTypeID = salaryFixations.SalaryHeadID;
                            salaryFixationDb.IsLeave = salaryFixations.IsLeave;
                            salaryFixationDb.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixationDb.ExpectedStatus = salaryFixations.ExpectedStatus;
                        }
                        else if (salaryFixations.IsAnnualNonCash)
                        {
                            salaryFixationDb.AnnualNonCashId = salaryFixations.SalaryHeadID;
                            salaryFixationDb.IsAnnualNonCash = salaryFixations.IsAnnualNonCash;
                            salaryFixationDb.CurrentStatus = salaryFixations.CurrentStatus;
                            salaryFixationDb.ExpectedStatus = salaryFixations.ExpectedStatus;
                        }
                        else
                        {
                            salaryFixationDb.SalaryHeadID = salaryFixations.SalaryHeadID;
                            salaryFixationDb.IsMonthly = salaryFixations.IsMonthly;
                            salaryFixationDb.IsAnnualCash = salaryFixations.IsAnnualCash;
                            salaryFixationDb.CurrentAmount = salaryFixations.CurrentAmount;
                            salaryFixationDb.ExpectedAmount = salaryFixations.ExpectedAmount;
                        }

                        salaryFixationDb.PreRecruitmentEmployeeID = salaryFixations.PreRecruitmentEmployeeID;
                        salaryFixationDb.SalaryRuleId = salaryFixations.SalaryRuleId;
                        salaryFixationDb.PlantId = plantid;
                        salaryFixationDb.CompanyGroupID = companyGroupId;
                        salaryFixationDb.ModelState = ModelState.Added;
                        AuditService.Log(salaryFixationDb);
                        base.InsertOrUpdateGraph(salaryFixationDb);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdateGraphFromFixation(IEnumerable<SalaryFixation> entities, string companyGroupId, string plantid, EmployeeWiseTermsAndConditions employeeWiseTermsAndConditions, bool IsMail)
        {
            List<SalaryFixation> from_db = null;
            var flag = false;
            var _empid = string.Empty;
            try
            {
                foreach (SalaryFixation item in entities)
                {
                    _empid = item.PreRecruitmentEmployeeID;
                    break;
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                from_db = GetDetailList(_empid).ToList<SalaryFixation>();
                var pk = GetPK();
                var count = 0;
                foreach (SalaryFixation salaryFixations in entities)
                {
                    var salaryFixationDb = from_db.FirstOrDefault(r => r.Id == salaryFixations.Id);
                    if (from_db.Any(r => r.Id == salaryFixations.Id))
                    {
                        if (salaryFixations.IsLeave)
                        {
                            salaryFixationDb.LeaveTypeID = salaryFixations.SalaryHeadID;
                            salaryFixationDb.FixationStatus = salaryFixations.FixationStatusL;
                            salaryFixationDb.IsLeave = salaryFixations.IsLeave;
                        }
                        else if (salaryFixations.IsAnnualNonCash)
                        {
                            salaryFixationDb.AnnualNonCashId = salaryFixations.SalaryHeadID;
                            salaryFixationDb.FixationStatus = salaryFixations.FixationStatusN;
                            salaryFixationDb.IsAnnualNonCash = salaryFixations.IsAnnualNonCash;
                        }
                        else
                        {
                            salaryFixationDb.SalaryHeadID = salaryFixations.SalaryHeadID;
                            salaryFixationDb.FixationAmount = salaryFixations.FixationAmount;
                            salaryFixationDb.IsMonthly = salaryFixations.IsMonthly;
                            salaryFixationDb.IsAnnualCash = salaryFixations.IsAnnualCash;
                        }

                        salaryFixationDb.SalaryRuleId = salaryFixations.SalaryRuleId;
                        salaryFixationDb.IsCalculated = true;
                        salaryFixationDb.ModelState = ModelState.Modified;
                        AuditService.Log(salaryFixationDb);
                        base.InsertOrUpdateGraph(salaryFixationDb);
                    }
                    else
                    {
                        count++;
                        salaryFixationDb = new SalaryFixation
                        {
                            Id = "F" + pk + "-" + count
                        };

                        if (salaryFixations.IsLeave)
                        {
                            salaryFixationDb.LeaveTypeID = salaryFixations.SalaryHeadID;
                            salaryFixationDb.IsLeave = salaryFixations.IsLeave;
                            salaryFixationDb.FixationStatus = salaryFixations.FixationStatusL;
                        }
                        else if (salaryFixations.IsAnnualNonCash)
                        {
                            salaryFixationDb.AnnualNonCashId = salaryFixations.SalaryHeadID;
                            salaryFixationDb.IsAnnualNonCash = salaryFixations.IsAnnualNonCash;
                            salaryFixationDb.FixationStatus = salaryFixations.FixationStatusN;
                        }
                        else
                        {
                            salaryFixationDb.SalaryHeadID = salaryFixations.SalaryHeadID;
                            salaryFixationDb.IsMonthly = salaryFixations.IsMonthly;
                            salaryFixationDb.IsAnnualCash = salaryFixations.IsAnnualCash;
                            salaryFixationDb.FixationAmount = salaryFixations.FixationAmount;
                        }

                        salaryFixationDb.PreRecruitmentEmployeeID = salaryFixations.PreRecruitmentEmployeeID;
                        salaryFixationDb.SalaryRuleId = salaryFixations.SalaryRuleId;
                        salaryFixationDb.PlantId = plantid;
                        salaryFixationDb.CompanyGroupID = companyGroupId;
                        salaryFixationDb.IsCalculated = true;
                        salaryFixationDb.ModelState = ModelState.Added;
                        AuditService.Log(salaryFixationDb);
                        base.InsertOrUpdateGraph(salaryFixationDb);
                    }
                }
                if (employeeWiseTermsAndConditions != null)
                {
                    if (string.IsNullOrEmpty(employeeWiseTermsAndConditions.Id))
                    {
                        employeeWiseTermsAndConditions.Id = GetTCPK();
                        employeeWiseTermsAndConditions.ModelState = ModelState.Added;
                        AuditService.Log(employeeWiseTermsAndConditions);
                        _employeeWiseTermsAndConditionsRepository.Insert(employeeWiseTermsAndConditions);
                    }
                    else
                    {
                        employeeWiseTermsAndConditions.ModelState = ModelState.Modified;
                        AuditService.Log(employeeWiseTermsAndConditions);
                        _employeeWiseTermsAndConditionsRepository.Update(employeeWiseTermsAndConditions);
                    }
                }
                if (IsMail)
                {
                    _sfm.SaveSFM(_empid, plantid);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<SalaryFixation> GetDetailList(string _empid)
        {
            try
            {
                var _sql = "SELECT * FROM SCS.SalaryFixation where PreRecruitmentEmployeeID='" + _empid + "'";
                return _salaryFixationRepository.SqlQuery<SalaryFixation>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetTermsAndConditionsByPlant(string plantId)
        {
            try
            {
                var sql = @"SELECT '' Id, Description1,Description2 FROM [SCS].[PlantWiseTermsAndConditions]  Where PlantId ='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetTermsAndConditionsByEmployee(string preRecruitmentEmployeeid)
        {
            try
            {
                var sql = @"SELECT * FROM [TRN].[EmployeeWiseTermsAndConditions]  Where PreRecruitmentEmployeeId ='" + preRecruitmentEmployeeid + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCbo(string companyGroupId)
        {
            var _sql = @"SELECT SF.Id Value, SF.UserName Text
                            FROM SCS.SalaryFixationSetting SF";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        public override void Update(SalaryFixation entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        #endregion ---Actions
    }
}