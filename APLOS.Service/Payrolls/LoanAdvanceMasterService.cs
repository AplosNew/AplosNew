using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.Payrolls
{
    public class LoanAdvanceMasterService : Service<LoanAdvanceMaster>, ILoanAdvanceMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ILoanAdvanceChildService _loanAdvanceChildService;

        public LoanAdvanceMasterService(
            IRepositoryAsync<LoanAdvanceMaster> loanAdvanceMasterRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , ILoanAdvanceChildService loanAdvanceChildService) : base(loanAdvanceMasterRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _loanAdvanceChildService = loanAdvanceChildService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(LoanAdvanceMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private List<Dictionary<string, object>> CheckEmployee(string empSystemId, string taxYearId)
        {
            var sql = @"SELECT EI.EmployeeCode,LAM.EmpInfoSystemID,LAM.FromYearNo FROM LoanAdvanceMaster LAM
                        LEFT JOIN dbo.EmployeeInformation EI ON LAM.EmpInfoSystemID = EI.SystemId
                        WHERE LAM.EmpInfoSystemID = '" + empSystemId + "' AND LAM.FromYearNo = '" + taxYearId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        private void Check(LoanAdvanceMaster entity)
        {
            var empCode = "";
            var empSystemId = "";
            var Year = "";
            foreach (var item in CheckEmployee(entity.EmpInfoSystemID, entity.FromYearNo))
            {
                var dic = (Dictionary<string, object>)item;
                empCode = dic["EmployeeCode"].ToString();
                empSystemId = dic["EmpInfoSystemID"].ToString();
                Year = dic["FromYearNo"].ToString();

                if (entity.EmpInfoSystemID == empSystemId && entity.FromYearNo == Year)
                {
                    throw new CustomException("This Employee [" + empCode + "] already exists for [" + Year + "] Year.");
                }
            }
        }

        public void InsertOrUpdate(LoanAdvanceMaster entity, IEnumerable<LoanAdvanceChild> loanAdvanceChild)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.SystemID))
                {
                    //Check(entity);
                    entity.SystemID = GetPK();
                    DateTime st = Convert.ToDateTime(entity.StartDate);
                    entity.FromMonthNo = st.Month.ToString();
                    entity.FromYearNo = st.Year.ToString();
                    entity.ModelState = ModelState.Added;
                    AuditService.Log(entity);
                    Insert(entity);
                    //_loanAdvanceChildService.InsertOrUpdateGraph(loanAdvanceChild, entity.SystemID);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.Log(entity);
                    Update(entity);
                    // _loanAdvanceChildService.InsertOrUpdateGraph(loanAdvanceChild, entity.SystemID);
                }
                _loanAdvanceChildService.InsertOrUpdateGraph(loanAdvanceChild, entity.SystemID);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdateOpeningBalance(LoanAdvanceMaster entity, IEnumerable<LoanAdvanceChild> loanAdvanceChild)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.SystemID))
                {
                    //Check(entity);
                    entity.SystemID = GetPK();
                    DateTime st = Convert.ToDateTime(entity.StartDate);
                    entity.FromMonthNo = st.Month.ToString();
                    entity.FromYearNo = st.Year.ToString();
                    entity.ModelState = ModelState.Added;
                    AuditService.Log(entity);
                    Insert(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.Log(entity);
                    Update(entity);
                }
                _loanAdvanceChildService.InsertOrUpdateGraphOpeningBalance(loanAdvanceChild, entity.SystemID);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetCbo(string currencyRuleSystemID)
        {
            try
            {
                var sql = @"SELECT SH.SalaryHeadID AS [Value], SH.SalaryHead AS [Text] FROM SalaryHead SH
                            INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                            AND CRC.MstSystemID = '" + currencyRuleSystemID + "' AND  HeadCategory = 'Advance'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetSalaryHeadCbo(string currencyRuleSystemID)
        {
            try
            {
                var sql = @"SELECT DISTINCT SH.SalaryHeadID , SH.SalaryHead,HasGL=CASE WHEN G.Id IS NOT NULL THEN 1 ELSE 0 END FROM SalaryHead SH
                            INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
							LEFT JOIN(Select * from MST.SalaryHeadGL Where (DrDirectGLId IS NOT NULL OR CrDirectGLId IS NOT NULL OR DrInDirectGLId IS NOT NULL OR CrInDirectGLId IS NOT NULL)) G ON G.SalaryHeadID=SH.SalaryHeadID
                            AND CRC.MstSystemID = '" + currencyRuleSystemID + "' ORDER BY SH.SalaryHead";
                return _sqlRepository.GetCombo(sql, "SalaryHeadID", "SalaryHead");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IEnumerable<object> GetLoanMasterByEmployee(string employeeId)
        {
            string sql = @"SELECT LAM.*
                                       ,StartingMonthName = DATENAME(MONTH, LAM.FromMonthNo + '-01-' + LAM.FromYearNo)
                                       ,ECR.Code AS EntryCurrency
                                       ,ECR.Code AS DefinitionCurrency
                                       ,ECR.Code AS DisbustCurrency
                                      FROM LoanAdvanceMaster LAM
                                      LEFT JOIN SCS.Currency ECR ON LAM.EntryCurrencyID = ECR.Id
                                      LEFT JOIN SCS.Currency DECR ON LAM.DefineCurrencyID = DECR.Id
                                      LEFT JOIN SCS.Currency DICR ON LAM.DisbustCurrencyID = DICR.Id
                                      WHERE LAM.EmpInfoSystemID = '" + employeeId + "'";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetOpeningBalanceByEmployee(string employeeId)
        {
            //string sql = @"SELECT LAM.SystemID, LAM.EmpInfoSystemID, LAM.StartDate, LAM.PaidAmount,
            //                      LAM.EntryCurrencyID,
            //                      ECR.Code AS EntryCurrency, LAM.AdvanceAmount, LAM.DefineCurrencyID AS DefinitionCurrencyID,
            //                      ECR.Code AS DefinitionCurrency, LAM.DefineAmount, LAM.DisbustCurrencyID,
            //                      ECR.Code AS DisbustCurrency, LAM.PaidAmount, LAM.IsFixedAmount,
            //                      LAM.IsEqualMonthAmount, LAM.IsInterestApplicable, LAM.InterestPercentageAmount,
            //                      LAM.InstallmentAmount, LAM.InstallmentMonth, LAM.IsDisbusted
            //                FROM LoanAdvanceMaster LAM
            //                     LEFT JOIN SCS.Currency ECR ON LAM.EntryCurrencyID = ECR.Id
            //                           LEFT JOIN SCS.Currency DECR ON LAM.DefineCurrencyID = DECR.Id
            //                           LEFT JOIN SCS.Currency DICR ON LAM.DisbustCurrencyID = DICR.Id
            //              WHERE LAM.EmpInfoSystemID ='" + employeeId + "'";

            string sql = @"SELECT LAM.*
                                       ,StartingMonthName = DATENAME(MONTH, LAM.FromMonthNo + '-01-' + LAM.FromYearNo)
                                       ,ECR.Code AS EntryCurrency
                                       ,ECR.Code AS DefinitionCurrency
                                       ,ECR.Code AS DisbustCurrency
                                      FROM LoanAdvanceMaster LAM
                                      LEFT JOIN SCS.Currency ECR ON LAM.EntryCurrencyID = ECR.Id
                                      LEFT JOIN SCS.Currency DECR ON LAM.DefineCurrencyID = DECR.Id
                                      LEFT JOIN SCS.Currency DICR ON LAM.DisbustCurrencyID = DICR.Id
                                      WHERE LAM.EmpInfoSystemID = '" + employeeId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetYear(string plantId)
        {
            try
            {
                string sql = @"SELECT CutOffDate FROM SCS.OpeningBalanceCutOffDate WHERE PlantId='" + plantId + @"' AND ModuleName='HR'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetLoanAdvanceInfoPlantWise(GridParameter parameters, string plantId, bool isControlAdmin, bool isSysAdmin, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                    str = @" AND EI.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE SalaryAdvanceApproval='" + employeeId + "'))";
                parameters.CmdText = @"SELECT LAM.*,EI.EmployeeName,0 Active FROM dbo.LoanAdvanceMaster LAM
                                      LEFT JOIN dbo.EmployeeInformation EI ON LAM.EmpInfoSystemID=EI.SystemId
                                      WHERE LAM.PlantId = '" + plantId + @"' AND (LAM.ApprovalStatus != 'Approved' AND LAM.ApprovalStatus != 'Rejected') OR ISNULL(ApprovalStatus, '') = ''"
                                     + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateSalApprovals(IEnumerable<LoanAdvanceMaster> entities, string name)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pks = entities.Select(t => t.SystemID);
                var from_db = base.Query(t => pks.Contains(t.SystemID)).Select().AsEnumerable();
                foreach (var item in entities)
                {
                    if (item.Active)
                    {
                        if (!from_db.Any(t => t.SystemID == item.SystemID))
                            throw new CustomException(ServiceResources.RecordNoLonger.ToString());
                        if (item.ApprovalStatus == EnumSalaryApprovalStatus.Approved.ToString())
                        {
                            item.ApprovedDate = DateTime.Now;
                            item.ApprovedBy = name;
                        }
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                        base.UpdateGraph(item);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}