using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Banks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Expenses;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Vouchers;
using Library.Model.Commercial;
using Library.Service.Banks;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Expenses
{
    public class ExpenseBookingService : Service<ExpenseBooking>, IExpenseBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IInvoiceService _invoiceService;
        private readonly IRepositoryAsync<ExpenseBookingApprovalHistory> _expenseBookingApprovalHistoryRepository;
        private readonly IRepositoryAsync<ExpenseBookingDetail> _expenseBookingDetailRepository;
        private readonly IRepositoryAsync<ApprovalConfiguration> _approvalConfigurationrepository;
        private readonly IRepositoryAsync<ExpenseBooking> _expenseBookingRepository;
        private readonly IRepositoryAsync<ExpenseActivity> _expenseActivityRepository;
        private readonly IBankJournalNewService _bankJournalService;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;
        private readonly IRepositoryAsync<InvoiceDetailCharges> _invoiceDetailChargesRepository;

        public ExpenseBookingService(
              IRepositoryAsync<ExpenseBooking> expenseBookingRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IRepositoryAsync<ExpenseBookingApprovalHistory> expenseBookingApprovalHistoryRepository
            , IRepositoryAsync<ExpenseBookingDetail> expenseBookingDetailRepository
            , IVoucherService voucherService
            , IRepositoryAsync<ApprovalConfiguration> approvalConfigurationrepository
            , IEmployeePayableService employeePayableService
            , IRepositoryAsync<ExpenseActivity> expenseActivityRepository
            , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository
        , IBankJournalNewService bankJournalService

            , IInvoiceService invoiceService
            , IRepositoryAsync<InvoiceDetailCharges> invoiceDetailChargesRepository
            ) : base(expenseBookingRepository, unitOfWork, pkGeneratorService)
        {
            _expenseBookingRepository = expenseBookingRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _expenseBookingApprovalHistoryRepository = expenseBookingApprovalHistoryRepository;
            _expenseBookingDetailRepository = expenseBookingDetailRepository;
            _voucherService = voucherService;
            _approvalConfigurationrepository = approvalConfigurationrepository;
            _employeePayableService = employeePayableService;
            _bankJournalService = bankJournalService;
            _invoiceService = invoiceService;
            _expenseActivityRepository = expenseActivityRepository;
            _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
            _invoiceDetailChargesRepository = invoiceDetailChargesRepository;
        }

        private static string MakePKExpenseBookingDetail(string masterId, int currentId)
        {
            return MakePK(masterId, currentId, 2);
        }

        private static string MakePKApprovalHistoryDetail(string masterId, int currentId)
        {
            return MakePK(masterId, currentId, 2);
        }

        public void InsertOrUpdateGraph(IEnumerable<ExpenseBookingDetail> entities, IEnumerable<ExpenseActivity> expActdetails, ExpenseBooking expenseBooking, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList)
        {
            AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
            if (entities != null)
            {
                var budgetTransactionDetailDb_list = _expenseBookingDetailRepository.Query(r => r.ExpenseBookingId == expenseBooking.Id).Select();
                var currentRecord = _expenseBookingDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM TRN.ExpenseBookingDetail WHERE ExpenseBookingId='{expenseBooking.Id}'").First();
                foreach (var entity in entities)
                {
                    var gl = _accountsCommonService.GetGLByBudgetMasterId(entity.BudgetMasterId);
                    entity.GLGeneralInfoId = gl["GLGeneralInfoId"].ToString();
                    if (!string.IsNullOrEmpty(entity.Id))
                    {
                        var budgetTransactionDetailDb = budgetTransactionDetailDb_list.FirstOrDefault(r => r.Id == entity.Id);
                        if (budgetTransactionDetailDb != null)
                        {
                            entity.UpdatedBy = expenseBooking.UpdatedBy;
                            entity.UpdatedDate = expenseBooking.UpdatedDate;
                            entity.UpdatedFromIP = expenseBooking.UpdatedFromIP;
                            entity.DocDate = expenseBooking.InvoiceDate;
                            entity.IsPosted = false;
                            budgetTransactionDetailDb.ModelState = ModelState.Modified;
                            _expenseBookingDetailRepository.Update(entity);
                        }
                        if (expActdetails != null) {
                            var expActivity = expActdetails.Where(r => r.ExpenseBookingDetailId == entity.Id).FirstOrDefault();
                            if (expActivity != null)
                            {
                                expActivity.UpdatedBy = expenseBooking.UpdatedBy;
                                expActivity.UpdatedDate = expenseBooking.UpdatedDate;
                                expActivity.UpdatedFromIP = expenseBooking.UpdatedFromIP;
                                expActivity.ExpenseBookingDetailId = entity.Id;
                                expActivity.ExpenseBookingId = entity.ExpenseBookingId;
                                expActivity.GLGeneralInfoId = entity.GLGeneralInfoId;
                                expActivity.BudgetMasterId = entity.BudgetMasterId;
                                expActivity.ActivityId = entity.ActivityId;
                                expActivity.FixedAssetRegisterId = entity.FixedAssetRegisterId;
                                expActivity.ExpenseBookingDetailId = entity.Id;
                                expActivity.ModelState = ModelState.Modified;
                                _expenseActivityRepository.Update(expActivity);
                            }
                        }
                        if (null != invoiceDetailChargesList && invoiceDetailChargesList.Count() > 0)
                        {

                            foreach (var item in invoiceDetailChargesList.Where(r => r.GLGeneralInfoId == entity.GLGeneralInfoId && r.BudgetMasterId == entity.BudgetMasterId && r.ActivityId == entity.ActivityId))
                            {
                                var invoiceDetailChargesId = base.GetAutoNumber(nameof(InvoiceDetailCharges), PKGeneratorEnum.Yearly, null, DateTime.Now);
                                var invoiceChargesId = 0;
                                if (item.Id == null)
                                {
                                    invoiceChargesId++;
                                    var invoiceCharges = new InvoiceDetailCharges
                                    {
                                        Id = MakePK(invoiceDetailChargesId, invoiceChargesId, 2),
                                        InvoiceDetailId = item.InvoiceDetailId,
                                        InvoiceId = item.InvoiceId,
                                        DistributedAmount = item.DistributedAmount,
                                        InvoiceServiceMasterChargesId = null,
                                        VoucherDetailId = null,
                                        Amount = item.Amount,
                                        InvoiceType = item.InvoiceType,
                                        MasterOrderId = item.MasterOrderId,
                                        ContractId = item.ContractId,
                                        ExpenseBookingDetailId = entity.Id
                                    };
                                    AuditService.AddedLog(invoiceCharges);
                                    _invoiceDetailChargesRepository.Insert(invoiceCharges);
                                }
                                else
                                {
                                    var invoiceCharges = _invoiceDetailChargesRepository.Find(item.Id);
                                    invoiceCharges.DistributedAmount = item.DistributedAmount;
                                    AuditService.UpdatedLog(invoiceCharges);
                                    _invoiceDetailChargesRepository.Update(invoiceCharges);
                                }
                            }
                        }

                    }
                    else
                    {
                        currentRecord++;
                        entity.Id = MakePKExpenseBookingDetail(expenseBooking.Id, currentRecord);
                        entity.ExpenseBookingId = expenseBooking.Id;
                        entity.ApprovalStatus = ApprovalStatus.ToBeChecked.ToString();
                        entity.IsPosted = false;
                        entity.AddedBy = expenseBooking.AddedBy;
                        entity.AddedDate = expenseBooking.AddedDate;
                        entity.AddedFromIP = expenseBooking.AddedFromIP;
                        _expenseBookingDetailRepository.Insert(entity);

                        //Insert ExpenseActivity
                        if(expActdetails != null)
                        {
                            var expActivity = expActdetails.Where(r => r.GLGeneralInfoId == entity.GLGeneralInfoId && r.BudgetMasterId == entity.BudgetMasterId
                         && r.ActivityId == entity.ActivityId && r.FixedAssetRegisterId == entity.FixedAssetRegisterId).FirstOrDefault();
                            if (expActivity != null)
                            {
                                expActivity.Id = entity.Id;
                                expActivity.ExpenseBookingDetailId = entity.Id;
                                expActivity.ExpenseBookingId = entity.ExpenseBookingId;
                                expActivity.GLGeneralInfoId = entity.GLGeneralInfoId;
                                expActivity.BudgetMasterId = entity.BudgetMasterId;
                                expActivity.ActivityId = entity.ActivityId;
                                expActivity.FixedAssetRegisterId = entity.FixedAssetRegisterId;
                                expActivity.AddedBy = expenseBooking.AddedBy;
                                expActivity.AddedDate = expenseBooking.AddedDate;
                                expActivity.AddedFromIP = expenseBooking.AddedFromIP;
                                _expenseActivityRepository.Insert(expActivity);
                            }
                        }
                        if (null != invoiceDetailChargesList && invoiceDetailChargesList.Count() > 0 )
                        {

                            foreach (var item in invoiceDetailChargesList.Where(r => r.GLGeneralInfoId == entity.GLGeneralInfoId && r.BudgetMasterId == entity.BudgetMasterId && r.ActivityId == entity.ActivityId))
                            {
                                var invoiceDetailChargesId = base.GetAutoNumber(nameof(InvoiceDetailCharges), PKGeneratorEnum.Yearly, null, DateTime.Now);
                                var invoiceChargesId = 0;
                                if (item.Id == null)
                                {
                                    invoiceChargesId++;
                                    var invoiceCharges = new InvoiceDetailCharges
                                    {
                                        Id = MakePK(invoiceDetailChargesId, invoiceChargesId, 2),
                                        InvoiceDetailId = item.InvoiceDetailId,
                                        InvoiceId = item.InvoiceId,
                                        DistributedAmount = item.DistributedAmount,
                                        InvoiceServiceMasterChargesId = null,
                                        VoucherDetailId = null,
                                        Amount = item.Amount,
                                        InvoiceType = item.InvoiceType,
                                        MasterOrderId = item.MasterOrderId,
                                        ContractId = item.ContractId,
                                        ExpenseBookingDetailId = entity.Id
                                    };
                                    AuditService.AddedLog(invoiceCharges);
                                    _invoiceDetailChargesRepository.Insert(invoiceCharges);
                                }
                                else
                                {
                                    var invoiceCharges = _invoiceDetailChargesRepository.Find(item.Id);
                                    invoiceCharges.DistributedAmount = item.DistributedAmount;
                                    AuditService.UpdatedLog(invoiceCharges);
                                    _invoiceDetailChargesRepository.Update(invoiceCharges);
                                }
                            }
                        }

                    }
                }
            }
        }

        public void EntityInsertOrUpdateGraph(IEnumerable<ExpenseBookingDetail> entities, ExpenseBooking expenseBooking)
        {
            AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();

            if (entities != null)
            {
                var budgetTransactionDetailDb_list = _expenseBookingDetailRepository.Query(r => r.ExpenseBookingId == expenseBooking.Id).Select();
                var currentRecord = _expenseBookingDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM TRN.ExpenseBookingDetail WHERE ExpenseBookingId='{expenseBooking.Id}'").First();
                foreach (var entity in entities)
                {
                    entity.GLGeneralInfoId = _accountsCommonService.GetGLByBudgetMasterId(entity.BudgetMasterId).ToString();

                    if (!string.IsNullOrEmpty(entity.Id))
                    {
                        var budgetTransactionDetailDb = budgetTransactionDetailDb_list.FirstOrDefault(r => r.Id == entity.Id);
                        if (budgetTransactionDetailDb != null)
                        {
                            entity.UpdatedBy = expenseBooking.UpdatedBy;
                            entity.UpdatedDate = expenseBooking.UpdatedDate;
                            entity.UpdatedFromIP = expenseBooking.UpdatedFromIP;
                            entity.DocDate = expenseBooking.InvoiceDate;
                            entity.IsPosted = false;
                            entity.ApprovalStatus = "Submitted";
                            budgetTransactionDetailDb.ModelState = ModelState.Modified;
                            _expenseBookingDetailRepository.Update(entity);
                        }
                    }
                    else
                    {
                        currentRecord++;
                        entity.Id = MakePKExpenseBookingDetail(expenseBooking.Id, currentRecord);
                        entity.ExpenseBookingId = expenseBooking.Id;
                        entity.ApprovalStatus = "Submitted";
                        entity.IsPosted = false;
                        entity.AddedBy = expenseBooking.AddedBy;
                        entity.AddedDate = expenseBooking.AddedDate;
                        entity.AddedFromIP = expenseBooking.AddedFromIP;
                        _expenseBookingDetailRepository.Insert(entity);
                    }
                }
            }
        }

        public void ApprovalGraph(IEnumerable<ExpenseBookingDetail> expenseBookingDetails, ExpenseBooking expenseBooking, string responsiblePersonId)
        {
            if (expenseBookingDetails != null)
            {
                var currentRecord = _expenseBookingDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM TRN.ExpenseBookingApprovalHistory WHERE ExpenseBookingId='{expenseBooking.Id}'").First();
                foreach (var expenseBookingDetail in expenseBookingDetails)
                {
                    var expenseBookingApprovalHistory_list = _expenseBookingApprovalHistoryRepository.Query(r => r.ExpenseBookingDetailId == expenseBookingDetail.Id).Select().FirstOrDefault();
                    if (!string.IsNullOrEmpty(expenseBookingDetail.Id))
                    {
                        expenseBookingDetail.IsPosted = false;
                        expenseBookingDetail.ApprovalStatus = expenseBooking.ApprovalStatus;
                        _expenseBookingDetailRepository.Update(expenseBookingDetail);
                        if (expenseBookingApprovalHistory_list != null)
                        {
                            AuditService.UpdatedLog(expenseBookingApprovalHistory_list);
                            expenseBookingApprovalHistory_list.ApprovalStatus = expenseBooking.ApprovalStatus;
                            _expenseBookingApprovalHistoryRepository.Update(expenseBookingApprovalHistory_list);
                        }
                        else
                        {
                            currentRecord++;
                            var Expen = new ExpenseBookingApprovalHistory
                            {
                                Id = MakePKApprovalHistoryDetail(expenseBookingDetail.ExpenseBookingId, currentRecord),
                                ExpenseBookingDetailId = expenseBookingDetail.Id,
                                ExpenseBookingId = expenseBookingDetail.ExpenseBookingId,
                                EmployeeId = responsiblePersonId,
                                ApprovalStatus = expenseBooking.ApprovalStatus
                            };
                            AuditService.AddedLog(Expen);
                            Expen.ApprovalStatusDate = Expen.AddedDate;
                            _expenseBookingApprovalHistoryRepository.Insert(Expen);
                        }
                    }
                }
            }
        }

        public void CheckedGraph(IEnumerable<ExpenseBookingDetail> expenseBookingDetails, ExpenseBooking expenseBooking, string responsiblePersonId)
        {
            if (expenseBookingDetails != null)
            {
                var currentRecord = _expenseBookingDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM TRN.ExpenseBookingApprovalHistory WHERE ExpenseBookingId='{expenseBooking.Id}'").First();
                foreach (var expenseBookingDetail in expenseBookingDetails)
                {
                    var expenseBookingApprovalHistory_list = _expenseBookingApprovalHistoryRepository.Query(r => r.ExpenseBookingDetailId == expenseBookingDetail.Id).Select().FirstOrDefault();
                    if (!string.IsNullOrEmpty(expenseBookingDetail.Id))
                    {
                        expenseBookingDetail.IsPosted = false;
                        expenseBookingDetail.ApprovalStatus = expenseBooking.ApprovalStatus;
                        _expenseBookingDetailRepository.Update(expenseBookingDetail);
                        if (expenseBookingApprovalHistory_list != null)
                        {
                            expenseBookingApprovalHistory_list.ApprovalStatus = expenseBookingDetail.ApprovalStatus;
                            expenseBookingApprovalHistory_list.EmployeeId = responsiblePersonId;
                            AuditService.UpdatedLog(expenseBookingApprovalHistory_list);
                            _expenseBookingApprovalHistoryRepository.Update(expenseBookingApprovalHistory_list);
                        }
                        else
                        {
                            currentRecord++;
                            var Expen = new ExpenseBookingApprovalHistory
                            {
                                Id = MakePKApprovalHistoryDetail(expenseBookingDetail.ExpenseBookingId, currentRecord),
                                ExpenseBookingDetailId = expenseBookingDetail.Id,
                                ExpenseBookingId = expenseBookingDetail.ExpenseBookingId,
                                EmployeeId = responsiblePersonId,
                                ApprovalStatus = expenseBooking.ApprovalStatus
                            };
                            AuditService.AddedLog(Expen);
                            Expen.ApprovalStatusDate = Expen.AddedDate;
                            _expenseBookingApprovalHistoryRepository.Insert(Expen);
                        }
                    }
                }
            }
        }

        public GridModel Query(GridParameter parameters, string Id)
        {
            try
            {
                parameters.CmdText = @"SELECT EB.Id, EB.EmployeeId, EB.InvoiceDate, EB.InvoiceNumber, EB.CompanyGroupId, EI.EmployeeCode+' - '+EI.EmployeeName AS EmployeeCodeName, EB.CurrencyId, CU.Code As CurrencyCode
                                        FROM TRN.ExpenseBooking EB
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                        LEFT JOIN [SCS].Currency AS CU ON CU.Id=EB.CurrencyId
                                        WHERE EB.Id='" + Id + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetExpenseBookingApprovedData(GridParameter parameters, string companyId, string plantId, string expensesBookingId)
        {
            try
            {
                parameters.CmdText = @" SELECT DISTINCT EB.EmployeeId, EI.EmployeeCode+' - '+EI.EmployeeName AS EmployeeCodeName, EB.EntityId, EB.PlantId, P.UserName AS PartyName, EB.PartyId, EB.PartyPlantId,
										EB.InvoiceDate, EB.InvoiceNumber, EB.CompanyGroupId, EB.BeneficiaryType,
										EB.CurrencyId, CU.Code As CurrencyCode, EB.Remarks AS Narration,
                                        GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName,EBD.Id,EBD.ExpenseBookingId,EBD.MaterialMasterId,EBD.CostCenterId
										,EBD.FixedAssetRegisterId, EBD.GLGeneralInfoId, EBD.BudgetMasterId, EBD.ActivityId, EBD.DocRefNo, EBD.DocDate, EBD.Amount, EBD.ApprovalStatus,
                                        B.UserName AS BudgetName, A.UserName AS ActivityName, NULL TrnType, EBD.Id AS ExpenseBookingDetailId,
										CPC.CurrencyId AS ToCurrencyCode, CPC.CurrencyId AS companyCurrencyId, CU.Code AS companyCurrencyName, CPC.CurrencyId AS ToCurrencyId, 1 ToCurrencyRate, CPC.CurrencyId AS FromCurrencyId
										,FixedAsset=CASE WHEN EBD.MaterialMasterId IS NOT NULL THEN MM.UserName ELSE (CASE WHEN EBD.MaterialMasterId IS NOT NULL THEN FAR.SerialNo ELSE NULL END) END
                                        FROM [TRN].[ExpenseBookingDetail] AS EBD
                                        LEFT JOIN [TRN].[ExpenseBooking] AS EB ON EB.Id=EBD.ExpenseBookingId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                                        LEFT JOIN [HKP].GLGeneralInfo AS GLGI ON GLGI.Id=EBD.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON EBD.BudgetMasterId=BM.Id
                                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId=B.Id
                                        LEFT JOIN [HKP].[Activity] AS A ON EBD.ActivityId=A.Id
										LEFT JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=EB.CurrencyId
										LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=EB.CurrencyId
										LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=EBD.MaterialMasterId
										LEFT JOIN [MST].[BudgetMasterActivityFixedAsset] AS FAM ON FAM.MaterialMasterId=MM.Id
										LEFT JOIN [TRN].[FixedAssetRegister] AS FAR ON FAM.Id=EBD.FixedAssetRegisterId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
										AND EB.CompanyId='" + companyId + @"' AND EB.PlantId='" + plantId + @"' AND EB.Id='" + expensesBookingId + @"' AND EB.ApprovalStatus='Approved' AND EB.IsPosted=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetEntityExpenseBookingSubmittedData(GridParameter parameters, string companyId, string plantId, string expensesBookingId)
        {
            try
            {
                parameters.CmdText = @" SELECT DISTINCT EB.EmployeeId, EI.EmployeeCode+' - '+EI.EmployeeName AS EmployeeCodeName, EB.EntityId, EB.PlantId, P.UserName AS PartyName, EB.PartyId,
										EB.InvoiceDate, EB.InvoiceNumber, EB.CompanyGroupId, EB.BeneficiaryType,
										EB.CurrencyId, CU.Code As CurrencyCode, EB.Remarks AS Narration,
                                        GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName,EBD.Id,EBD.ExpenseBookingId,EBD.MaterialMasterId
										,EBD.FixedAssetRegisterId, EBD.GLGeneralInfoId, EBD.BudgetMasterId, EBD.ActivityId, EBD.DocRefNo, EBD.DocDate, EBD.Amount, EBD.ApprovalStatus,
                                        B.UserName AS BudgetName, A.UserName AS ActivityName, NULL TrnType, EBD.Id AS ExpenseBookingDetailId,
										CPC.CurrencyId AS ToCurrencyCode, CPC.CurrencyId AS companyCurrencyId, CU.Code AS companyCurrencyName, CPC.CurrencyId AS ToCurrencyId, 1 ToCurrencyRate, CPC.CurrencyId AS FromCurrencyId
										,FixedAsset=CASE WHEN EBD.MaterialMasterId IS NOT NULL THEN MM.UserName ELSE (CASE WHEN EBD.MaterialMasterId IS NOT NULL THEN FAR.SerialNo ELSE NULL END) END
                                        FROM [TRN].[ExpenseBookingDetail] AS EBD
                                        LEFT JOIN [TRN].[ExpenseBooking] AS EB ON EB.Id=EBD.ExpenseBookingId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                                        LEFT JOIN [HKP].GLGeneralInfo AS GLGI ON GLGI.Id=EBD.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON EBD.BudgetMasterId=BM.Id
                                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId=B.Id
                                        LEFT JOIN [HKP].[Activity] AS A ON EBD.ActivityId=A.Id
										LEFT JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=EB.CurrencyId
										LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=EB.CurrencyId
										LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=EBD.MaterialMasterId
										LEFT JOIN [MST].[BudgetMasterActivityFixedAsset] AS FAM ON FAM.MaterialMasterId=MM.Id
										LEFT JOIN [TRN].[FixedAssetRegister] AS FAR ON FAM.Id=EBD.FixedAssetRegisterId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
										AND EB.CompanyId='" + companyId + @"' AND EB.PlantId='" + plantId + @"' AND EB.Id='" + expensesBookingId + @"' AND EB.ApprovalStatus='Submitted'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<ExpenseBookingViewModel> GetExpenseBookingPendingList(string employeeId)
        {
            try
            {
                var sql = @"SELECT EB.Id, EB.EmployeeId, EB.CurrencyId, C.Code AS CurrencyName, EB.InvoiceNumber, EB.InvoiceDate, EB.ApprovalStatus, EB.Remarks
                            FROM [TRN].[ExpenseBooking] AS EB
                            LEFT JOIN [SCS].[Currency] C ON EB.CurrencyId=C.Id
                            WHERE EB.Archive=0 AND EB.EmployeeId='" + employeeId + "' AND EB.ApprovalStatus='" + ApprovalStatus.Pending + "' AND EB.IsPosted=0 ";
                return _sqlRepository.GetModelCollection<ExpenseBookingViewModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetExpenseBookingPendingList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT EB.Id, EB.EmployeeId,EB.PartyId,EB.FileName, EI.EmployeeCode, EI.EmployeeName , EB.PartyPlantId, P.UserName AS PartyName,EBD.Amount,EB.BeneficiaryType,
					                EIH.EmployeeCode AS ApproverCode, EIH.EmployeeName AS ApprovedBy, EB.CurrencyId, C.Code AS CurrencyName, EB.InvoiceNumber, EB.InvoiceDate, EB.ApprovalStatus, EB.Remarks
                                    ,EIR.EmployeeCode +'-'+EIR.EmployeeName CheckedBy
                                    FROM [TRN].[ExpenseBooking] AS EB
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                    LEFT JOIN [SCS].[Currency] C ON EB.CurrencyId=C.Id
							        LEFT JOIN [TRN].[ExpenseBookingApprovalHistory] AS EAH ON EAH.ExpenseBookingId=EB.Id
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EIH ON EIH.SystemId=EAH.EmployeeId
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
									LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                    WHERE EB.Archive=0 AND EB.CompanyGroupId='" + companyGroupId + "' AND EB.CompanyId='" + companyId + "' AND EB.PlantId='" + plantId + "' AND EB.ApprovalStatus='" + ApprovalStatus.Approved + "' AND EB.IsPosted=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetEntityExpenseBookingPendingList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT EB.Id, EB.EmployeeId,EB.PartyId, EI.EmployeeCode, EI.EmployeeName , P.UserName AS PartyName,EBD.Amount,EB.BeneficiaryType,
					                EIH.EmployeeCode AS ApproverCode, EIH.EmployeeName AS ApprovedBy, EB.CurrencyId, C.Code AS CurrencyName, EB.InvoiceNumber, EB.InvoiceDate, EB.ApprovalStatus, EB.Remarks
                                    FROM [TRN].[ExpenseBooking] AS EB
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                    LEFT JOIN [SCS].[Currency] C ON EB.CurrencyId=C.Id
							        LEFT JOIN [TRN].[ExpenseBookingApprovalHistory] AS EAH ON EAH.ExpenseBookingId=EB.Id
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EIH ON EIH.SystemId=EAH.EmployeeId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
									LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                    WHERE EB.Archive=0 AND EB.CompanyGroupId='" + companyGroupId + "' AND EB.CompanyId='" + companyId + "' AND EB.PlantId='" + plantId + "' AND EB.IsPosted=0 AND EB.AppliedBy='Entity'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetEntityExpenseBookingSubmittedList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT EB.Id, EB.EmployeeId,EB.VoucherId, EB.PartyId, EB.CashMasterId, EB.EntityId, EI.EmployeeCode, EI.EmployeeName , P.UserName AS PartyName, EBD.Amount, EB.BeneficiaryType,
					                EIH.EmployeeCode AS ApproverCode, EIH.EmployeeName AS ApprovedBy, EB.CurrencyId, C.Code AS CurrencyName
									, EB.InvoiceNumber, EB.InvoiceDate, EB.ApprovalStatus, EB.Remarks, EB.IsPosted, V.VoucherNo
                                    FROM [TRN].[ExpenseBooking] AS EB
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                    LEFT JOIN [SCS].[Currency] C ON EB.CurrencyId=C.Id
							        LEFT JOIN [TRN].[ExpenseBookingApprovalHistory] AS EAH ON EAH.ExpenseBookingId=EB.Id
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EIH ON EIH.SystemId=EAH.EmployeeId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
									LEFT JOIN [TRN].[Voucher] AS V ON V.Id=EB.VoucherId
									LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                    WHERE EB.Archive=0 AND EB.CompanyGroupId='" + companyGroupId + "' AND EB.CompanyId='" + companyId + "' AND EB.PlantId='" + plantId + "' AND EB.ApprovalStatus='Submitted'  AND EB.AppliedBy='Entity'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetExpenseBookingApprovedList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT DISTINCT EB.Id, EI.EmployeeCode, EI.EmployeeName, EB.EmployeeId, EB.CurrencyId, C.Code AS CurrencyCode, EB.InvoiceNumber, EB.InvoiceDate, EB.ApprovalStatus, EB.Remarks
                                , [Post]=CASE WHEN EB.IsPosted=1 THEN 'Posted' ELSE 'Approved' END, EBD.Amount, EB.BeneficiaryType, ISNULL(EP.VoucherId, I.VoucherId) AS VoucherId
								,EIH.EmployeeCode AS ApproverCode, EIH.EmployeeName AS ApprovedBy,V.IsPark,V.VoucherNo,V.PostingDate,EIR.EmployeeCode+'-'+EIR.EmployeeName CheckedBy
                                FROM [TRN].[ExpenseBooking] AS EB
                                LEFT JOIN [TRN].[EmployeePayable] AS EP ON EP.ExpenseBookingId=EB.Id
                                LEFT JOIN [TRN].[Invoice] AS I ON I.ExpenseBookingId=EB.Id
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
							    LEFT JOIN [TRN].[ExpenseBookingApprovalHistory] AS EAH ON EAH.ExpenseBookingId=EB.Id
                                LEFT JOIN [dbo].[EmployeeInformation] AS EIH ON EIH.SystemId=EAH.EmployeeId
                                LEFT JOIN [SCS].[Currency] C ON EB.CurrencyId=C.Id
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=EB.VoucherId
                                LEFT JOIN (SELECT ExpenseBookingId, SUM(Amount) AS Amount FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                WHERE EB.Archive=0 AND V.Archive=0 AND EB.CompanyGroupId='" + companyGroupId + "' AND EB.CompanyId='" + companyId + "' AND EB.PlantId='" + plantId + "' AND EB.ApprovalStatus='" + ApprovalStatus.Approved + "' AND EB.IsPosted=1";
            return _sqlRepository.GetGridData(parameters);
        }

        public IEnumerable<object> GetExpenseBookingDetail(string expenseBookingId)
        {
            try
            {
                var cmdText = @"SELECT DISTINCT EB.EmployeeId, EI.EmployeeCode+' - '+EI.EmployeeName AS EmployeeCodeName, EB.ResponsiblePersonId, EIR.EmployeeName AS ResponsiblePersonName, EB.CurrencyId, EB.EntityId, EB.PlantId
                                , EBD.GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, NULL TrnType, EBD.Id AS ExpenseBookingDetailId, REPLACE(CONVERT(VARCHAR(11), EBD.DocDate, 106), ' ', '-') AS DocDate
                                , EBD.Id, EBD.ExpenseBookingId, EBD.PartyId, EBD.BudgetMasterId, B.UserName AS BudgetName, EBD.ActivityId, A.UserName AS ActivityName, EBD.ActivityPhoneId, EBD.DocRefNo, EBD.Amount
                                , EBD.ApprovalStatus, EBD.ApprovalStatusDate, EBD.ActivityType, EBD.IsPosted, EBD.MaterialMasterId, MM.UserName AS FixedAsset, EBD.FixedAssetRegisterId, FAR.SerialNo,EBD.CostCenterId
                                FROM [TRN].[ExpenseBookingDetail] AS EBD
                                LEFT JOIN [TRN].[ExpenseBooking] AS EB ON EB.Id=EBD.ExpenseBookingId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=EBD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON EBD.BudgetMasterId=BM.Id
                                LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId=B.Id
                                LEFT JOIN [HKP].[Activity] AS A ON EBD.ActivityId=A.Id
                                LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=EBD.MaterialMasterId
                                LEFT JOIN [MST].[BudgetMasterActivityFixedAsset] AS FAM ON FAM.MaterialMasterId=MM.Id
                                LEFT JOIN [TRN].[FixedAssetRegister] AS FAR ON FAM.Id=EBD.FixedAssetRegisterId
                                WHERE EBD.ExpenseBookingId='" + expenseBookingId + "' ORDER BY Id";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void Insert(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details,IEnumerable<ExpenseActivity> expActdetails, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList)
        {
            var flag = false;
            try
            {
                Check(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = base.GetAutoNumber("ExpenseBooking", PKGeneratorEnum.Yearly, null, DateTime.Now);
                entity.ApprovalStatus = ApprovalStatus.ToBeChecked.ToString();
                entity.IsPosted = false;
                entity.InvoiceDate = entity.InvoiceDate;
                AuditService.AddedLog(entity);
                
                InsertGraph(entity);
                if (identity.EmployeeId != null)
                    entity.AddedBy = identity.EmployeeId;
                else
                    entity.AddedBy = identity.UserId;
                InsertOrUpdateGraph(details, expActdetails, entity, invoiceDetailChargesList);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Update(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> expenseBookingDetails, IEnumerable<ExpenseActivity> expActdetails, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList)
        {
            var flag = false;
            try
            {
                Check(entity);
                Validation(entity.Id);
                _unitOfWork.BeginTransaction();
                flag = true;
                AuditService.UpdatedLog(entity);
                entity.ApprovalStatus = ApprovalStatus.ToBeChecked.ToString();
                InsertOrUpdateGraph(expenseBookingDetails, expActdetails, entity, invoiceDetailChargesList);
                UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void EntityExpenseBookingSubmit(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details)
        {
            var flag = false;
            try
            {
                Check(entity);
                Validation(entity.Id);
                _unitOfWork.BeginTransaction();
                flag = true;
                AuditService.UpdatedLog(entity);
                entity.ApprovalStatus = "Submitted";
                EntityInsertOrUpdateGraph(details, entity);
                UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void Check(ExpenseBooking entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.InvoiceNumber, r => r.Id != entity.Id && r.InvoiceNumber == entity.InvoiceNumber);
        }

        public void ExpenseBookingCheckedPotal(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details, string responsiblePersonId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var expenseBookingDb = Find(entity.Id);
                CheckedValidation(expenseBookingDb);

                expenseBookingDb.IsPosted = false;
                expenseBookingDb.ApprovalStatus = entity.ApprovalStatus;
                _expenseBookingRepository.Update(entity);

                CheckedGraph(details, entity, responsiblePersonId);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void ExpenseBookingApprovalPotal(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details, string responsiblePersonId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var expenseBookingDb = Find(entity.Id);
                ApprovedValidation(expenseBookingDb);

                expenseBookingDb.IsPosted = false;
                expenseBookingDb.ApprovalStatus = entity.ApprovalStatus;
                _expenseBookingRepository.Update(entity);

                ApprovalGraph(details, entity, responsiblePersonId);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        private static void ApprovedValidation(ExpenseBooking entity)
        {
            if (entity.ApprovalStatus == ApprovalStatus.Approved.ToString())
                throw new CustomException("Update is not allowed after Approved.");
        }
        private static void CheckedValidation(ExpenseBooking entity)
        {
            if (entity.ApprovalStatus == ApprovalStatus.Checked.ToString())
                throw new CustomException("Update is not allowed after Checked.");
        }

        private void Validation(string id)
        {
            var entity = Find(id);
            if (entity.ApprovalStatus == ApprovalStatus.Approved.ToString())
                throw new CustomException("Update is not allowed after Approved.");
            if (entity.ApprovalStatus == ApprovalStatus.Holded.ToString())
                throw new CustomException("Update is not allowed after Holden.");
            if (entity.ApprovalStatus == ApprovalStatus.Rejected.ToString())
                throw new CustomException("Update is not allowed after Rejected.");
        }

        public GridModel QueryPoatal(GridParameter parameters, string status)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT EB.*, C.Code AS CurrencyCode,EI.EmployeeCode+' - '+EI.EmployeeName AS [EmployeeName], EIR.EmployeeName AS ResponsiblePersonName, EBD.Amount, P.UserName AS PartyName
										FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
										LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        WHERE EB.Archive=0 AND EB.EmployeeId='" + identity.EmployeeId + "' AND EB.ApprovalStatus='" + status + @"'  AND EB.IsPosted=0 AND EB.AppliedBy!='Entity'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> QueryPoatal(string status)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT EB.Id,EB.CompanyGroupId,EB.EmployeeId,EB.PlantId,EB.EntityId,EB.CurrencyId,EB.InvoiceNumber,REPLACE(CONVERT(VARCHAR(11), EB.InvoiceDate, 106), ' ', '-') AS InvoiceDate
										,EB.ApprovalStatus,EB.Remarks,EB.Archive,EB.AddedBy,EB.AddedDate,EB.AddedFromIP,EB.CompanyId,EB.IsPosted,EB.ResponsiblePersonId,EB.PartyId,EB.PartyPlantId
										,EB.BeneficiaryType,EB.VoucherId,EB.AppliedBy,EB.[FileName],EB.CashMasterId, C.Code AS CurrencyCode,EI.EmployeeCode+' - '+EI.EmployeeName AS [EmployeeName], EIR.EmployeeName AS ResponsiblePersonName
                                        , EBD.Amount, P.UserName AS PartyName,EIRA.EmployeeName AddedByName,EIA.EmployeeName ApprovedBy
										FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
										LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        left JOIN (SELECT distinct ExpenseBookingId,EmployeeId,ApprovalStatus FROM TRN.ExpenseBookingApprovalHistory WHERE  ApprovalStatus='" + status + @"' ) 
										EBAH ON EBAH.ExpenseBookingId=EB.Id AND EB.ApprovalStatus=EBAH.ApprovalStatus
										left join dbo.EmployeeInformation EIA on EIA.SystemId=EBAH.EmployeeId
                                WHERE EB.Archive=0 AND EB.EmployeeId='" + identity.EmployeeId + "' AND EB.ApprovalStatus='" + status + @"'  AND EB.IsPosted=0 AND EB.AppliedBy!='Entity'
                                ORDER BY EB.InvoiceDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> QueryPoatalPostedList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT EB.Id,EB.CompanyGroupId,EB.EmployeeId,EB.PlantId,EB.EntityId,EB.CurrencyId,EB.InvoiceNumber,REPLACE(CONVERT(VARCHAR(11), EB.InvoiceDate, 106), ' ', '-') AS InvoiceDate
										,EB.ApprovalStatus,EB.Remarks,EB.Archive,EB.AddedBy,EB.AddedDate,EB.AddedFromIP,EB.CompanyId,EB.IsPosted,EB.ResponsiblePersonId,EB.PartyId,EB.PartyPlantId
										,EB.BeneficiaryType,EB.VoucherId,V.VoucherNo,EB.AppliedBy,EB.[FileName],EB.CashMasterId, C.Code AS CurrencyCode,EI.EmployeeCode+' - '+EI.EmployeeName AS [EmployeeName], EIR.EmployeeName AS ResponsiblePersonName
                                        , EBD.Amount, P.UserName AS PartyName,EIRA.EmployeeName AddedByName,EIA.EmployeeName ApprovedBy
										FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
										LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        left JOIN (SELECT distinct ExpenseBookingId,EmployeeId,ApprovalStatus FROM TRN.ExpenseBookingApprovalHistory WHERE  ApprovalStatus='Approved' ) 
										EBAH ON EBAH.ExpenseBookingId=EB.Id AND EB.ApprovalStatus=EBAH.ApprovalStatus
										left join dbo.EmployeeInformation EIA on EIA.SystemId=EBAH.EmployeeId
                                        LEFT JOIN TRN.Voucher V ON V.Id=EB.VoucherId
                                WHERE EB.Archive=0 AND EB.EmployeeId='" + identity.EmployeeId + @"' AND EB.ApprovalStatus='Approved'  AND EB.IsPosted=1 AND EB.AppliedBy!='Entity' AND EB.VoucherId<>''
                                ORDER BY EB.InvoiceDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> QueryCheckedByPoatal(string status)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT EB.Id,EB.CompanyGroupId,EB.EmployeeId,EB.PlantId,EB.EntityId,EB.CurrencyId,EB.InvoiceNumber,REPLACE(CONVERT(VARCHAR(11), EB.InvoiceDate, 106), ' ', '-') AS InvoiceDate
										,EB.ApprovalStatus,EB.Remarks,EB.Archive,EB.AddedBy,EB.AddedDate,EB.AddedFromIP,EB.CompanyId,EB.IsPosted,EB.ResponsiblePersonId,EB.PartyId,EB.PartyPlantId
										,EB.BeneficiaryType,EB.VoucherId,EB.AppliedBy,EB.[FileName],EB.CashMasterId
										, C.Code AS CurrencyCode,EI.EmployeeCode+' - '+EI.EmployeeName AS [EmployeeName], EIR.EmployeeName AS ResponsiblePersonName
                                        , EBD.Amount, P.UserName AS PartyName,EIRA.EmployeeName AddedByName,EIA.EmployeeName ApprovedBy
										FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
										LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        left JOIN (SELECT distinct ExpenseBookingId,EmployeeId,ApprovalStatus FROM TRN.ExpenseBookingApprovalHistory WHERE  ApprovalStatus='ToBeApproved' ) 
										EBAH ON EBAH.ExpenseBookingId=EB.Id AND EB.ApprovalStatus=EBAH.ApprovalStatus
										LEFT JOIN dbo.EmployeeInformation EIA on EIA.SystemId=EBAH.EmployeeId
                                        WHERE EB.Archive=0 AND EB.ResponsiblePersonId='" + identity.EmployeeId + "' AND EB.ApprovalStatus='" + status + @"'  AND EB.IsPosted=0 AND EB.AppliedBy!='Entity'
                                        ORDER BY EB.InvoiceDate DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> CheckedQueryByCheckedBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT EB.Id,EB.CompanyGroupId,EB.EmployeeId,EB.PlantId,EB.EntityId,EB.CurrencyId,EB.InvoiceNumber,REPLACE(CONVERT(VARCHAR(11), EB.InvoiceDate, 106), ' ', '-') AS InvoiceDate
										,EB.ApprovalStatus,EB.Remarks,EB.Archive,EB.AddedBy,EB.AddedDate,EB.AddedFromIP,EB.CompanyId,EB.IsPosted,EB.ResponsiblePersonId,EB.PartyId,EB.PartyPlantId
										,EB.BeneficiaryType,EB.VoucherId,EB.AppliedBy,EB.[FileName],EB.CashMasterId
										, C.Code AS CurrencyCode,EI.EmployeeCode+' - '+EI.EmployeeName AS [EmployeeName], EIR.EmployeeName AS ResponsiblePersonName
                                        , EBD.Amount, P.UserName AS PartyName,EIRA.EmployeeName AddedByName,EIA.EmployeeName ApprovedBy,[ParkedPosted]=case when eb.IsPosted=1 then 'Posted' else 'Parked' End
										FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
										LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
										LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
										LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        left JOIN (SELECT distinct ExpenseBookingId,EmployeeId,ApprovalStatus FROM TRN.ExpenseBookingApprovalHistory WHERE  ApprovalStatus='ToBeApproved' ) 
										EBAH ON EBAH.ExpenseBookingId=EB.Id AND EB.ApprovalStatus=EBAH.ApprovalStatus
										LEFT JOIN dbo.EmployeeInformation EIA on EIA.SystemId=EBAH.EmployeeId
                                        WHERE EB.Archive=0 AND EB.ResponsiblePersonId='" + identity.EmployeeId + @"' AND EB.ApprovalStatus not in ('ToBeChecked','CheckedRejected','CheckedHolded')   AND EB.AppliedBy!='Entity'
                                        ORDER BY EB.InvoiceDate DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel QueryAdmin(GridParameter parameters, string status)
        {
            try
            {
                parameters.CmdText = @"SELECT EB.*, C.Code AS CurrencyCode, EI.EmployeeCode, EI.EmployeeName, P.UserName AS PartyName, EBD.Amount FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
									    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=EB.VoucherId
                                        LEFT JOIN (SELECT ExpenseBookingId, SUM(Amount) AS Amount FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        WHERE EB.Archive=0 AND EB.ApprovalStatus='" + status + @"' AND EB.IsPosted=0 AND EB.AppliedBy!='Entity'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetEntityExpenseBooking(GridParameter parameters, string status)
        {
            try
            {
                parameters.CmdText = @"SELECT EB.*, C.Code AS CurrencyCode, EI.EmployeeCode, EI.EmployeeName, P.UserName AS PartyName, EBD.Amount FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
									    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                                        LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                        WHERE EB.Archive=0 AND EB.IsPosted=0 AND EB.AppliedBy='Entity'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetListForApproval(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var employeeId = _approvalConfigurationrepository.SqlQuery<string>(@"SELECT ExpanseBookingRP FROM [HKP].[ApprovalConfiguration] WHERE ExpanseBookingRP='" + identity.EmployeeId + "'").FirstOrDefault();
                if (!string.IsNullOrEmpty(employeeId))
                {
                    parameters.CmdText = @"SELECT EB.*, C.Code AS CurrencyCode,EI.EmployeeCode+' - '+EI.EmployeeName AS [EmployeeName] FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
										LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                        WHERE EB.Archive=0 AND EB.ApprovalStatus <>'Approved' AND EB.IsPosted=0 ";
                    return _sqlRepository.GetGridData(parameters);
                }
                else
                    parameters.CmdText = @"";
                return null;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public IEnumerable<object> GetListForDepartmentApproval(string approvalStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT top(300) EI.DepartmentId, EB.*, C.Code AS CurrencyCode, EI.EmployeeCode, EI.EmployeeName, EBD.Amount, P.UserName AS PartyName
                                        , EIR.EmployeeName AS ResponsiblePersonName
                                        ,AddedByName= case when EIRA.EmployeeName<>'' then  EIRA.EmployeeName else '' end
                                        FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                                        LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
										 JOIN (SELECT  distinct ExpenseBookingId,EmployeeId,ApprovalStatus FROM TRN.ExpenseBookingApprovalHistory WHERE EmployeeId='" + identity.EmployeeId + @"' AND ApprovalStatus='" + approvalStatus + @"' ) 
										EBAH ON EBAH.ExpenseBookingId=EB.Id AND EB.ApprovalStatus=EBAH.ApprovalStatus
                                        order by EB.AddedDate desc ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetListForDepartmentApprovedHoldReject()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT EI.DepartmentId, EB.*, C.Code AS CurrencyCode, EI.EmployeeCode, EI.EmployeeName, EBD.Amount, P.UserName AS PartyName
                                        , EIR.EmployeeName AS ResponsiblePersonName,EIRA.EmployeeName AddedByName
                                        FROM [TRN].[ExpenseBooking] AS EB
                                        JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EIRA ON EIRA.SystemId=EB.AddedBy
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                                        LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
										 JOIN (SELECT  distinct ExpenseBookingId,EmployeeId,ApprovalStatus FROM TRN.ExpenseBookingApprovalHistory WHERE EmployeeId='" + identity.EmployeeId + @"' AND ApprovalStatus IN ('"+ApprovalStatus.ApprovedHolded.ToString()+ "','" + ApprovalStatus.ApprovedRejected.ToString() + @"') ) 
                                        EBAH ON EBAH.ExpenseBookingId=EB.Id AND EB.ApprovalStatus=EBAH.ApprovalStatus";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        private string GetEmployeeSubsequentTransactionPK()
        {
            return GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public void InsertExpenseBookingApproved(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;

                voucherVM.CompanyCurrencyRate = 1;
                voucherVM.SourceType = SourceType.EmployeePayable.ToString();
                voucherVM.EntityId = _expenseBookingDetailRepository.SqlQuery<string>(@"SELECT MB.EntityId FROM dbo.EmployeeInformation AS EI LEFT JOIN MST.ManpowerBudget AS MB ON MB.Id=EI.BudgetCode WHERE EI.SystemId='" + voucherVM.EmployeeId + "'").First(); 
                voucherVM.ExpenseBookingId = voucherVM.ExpenseBookingId;
                // INSERT INTO EmployeePayable TABLE
                var invoice = new Invoice();
                var employeePayable = new EmployeePayable();
                if (voucherVM.BeneficiaryType == BeneficiaryType.Self.ToString())
                {
                    employeePayable = _employeePayableService.InsertEmployeePayable(voucherVM);
                }
                if (voucherVM.BeneficiaryType == BeneficiaryType.Vendor.ToString())
                {
                    invoice = _invoiceService.InsertInvoice(voucherVM);
                }

                // INSERT INTO Voucher TABLE

                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Update Posting Flag
                AuditService.PostedLog(voucher);

                // Set to EmployeePayable
                if (voucherVM.BeneficiaryType == BeneficiaryType.Self.ToString())
                {
                    employeePayable.VoucherId = voucher.Id;
                    employeePayable.AddedBy = voucher.AddedBy;
                    employeePayable.AddedDate = voucher.AddedDate;
                    employeePayable.AddedFromIP = voucher.AddedFromIP;
                }
                if (voucherVM.BeneficiaryType == BeneficiaryType.Vendor.ToString())
                {
                    invoice.VoucherId = voucher.Id;
                    invoice.AddedBy = voucher.AddedBy;
                    invoice.AddedDate = voucher.AddedDate;
                    invoice.AddedFromIP = voucher.AddedFromIP;
                    invoice.RevisedDueDate = null;
                }

                var employeePayableDetailId = 0;
                var invoiceDetailId = 0;
                //Expenses Booking Update
                var expenseBookingData = base.Find(voucherVM.ExpenseBookingId);
                expenseBookingData.IsPosted = true;
                expenseBookingData.VoucherId = voucher.Id;
                UpdateGraph(expenseBookingData);
                var currentVoucherDetailId = 0;

                //Expenses VoucherDetail and VoucherDetailCurrency Insert
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        var expenseBookingDetailData = _expenseBookingDetailRepository.Find(voucherDetailVM.ExpenseBookingDetailId);
                        expenseBookingDetailData.Amount = voucherDetailVM.Amount;
                        expenseBookingDetailData.IsPosted = true;
                        if (voucherDetailVM.ActivityId!= expenseBookingDetailData.ActivityId)
                        {
                            expenseBookingDetailData.OldGLGeneralInfoId = expenseBookingDetailData.GLGeneralInfoId;
                            expenseBookingDetailData.OldBudgetMasterId = expenseBookingDetailData.BudgetMasterId;
                            expenseBookingDetailData.OldActivityId = expenseBookingDetailData.ActivityId;
                            expenseBookingDetailData.ChangedBy = voucher.AddedBy;

                            expenseBookingDetailData.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                            expenseBookingDetailData.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                            expenseBookingDetailData.ActivityId = voucherDetailVM.ActivityId;
                        }
                        _expenseBookingDetailRepository.Update(expenseBookingDetailData);

                        // in liability side Cr.
                        var voucherDr = new VoucherDetail
                        {
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CostCenterId = voucherDetailVM.CostCenterId,
                            CurrencyId = voucher.CurrencyId,
                            EntityId = voucherVM.EntityId,
                            FiscalYearId = voucher.FiscalYearId,
                            FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                            ExpenseBookingDetailId = voucherDetailVM.ExpenseBookingDetailId,
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP,
                            Archive = voucher.Archive,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate,
                            FromCurrencyId = companyCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            ToCurrencyId = voucherVM.CurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate
                        });
                        
                       var invoiceDetailChargesData = _invoiceDetailChargesRepository.Query(x=> x.ExpenseBookingDetailId==voucherDetailVM.ExpenseBookingDetailId).Select().ToList();
                       if(invoiceDetailChargesData !=null && invoiceDetailChargesData.Count() > 0)
                        {
                            foreach (var invoiceDetailCharges in invoiceDetailChargesData)
                            {
                                invoiceDetailCharges.VoucherDetailId = voucherDr.Id;
                                _invoiceDetailChargesRepository.Update(invoiceDetailCharges);
                            }
                        }
                        
                     }
                }
                if (voucherVM.BeneficiaryType == BeneficiaryType.Vendor.ToString())
                {
                    var companyParty = _accountsCommonService.GetCompanyParty(voucherVM.CompanyId, voucherVM.PlantId, voucherVM.PartyId, "Vendor");
                    var regularGL = _accountsCommonService.GetCompanyPartyGL(voucherVM.PartyId,companyParty["Id"].ToString(), PartyGLType.ReconciliationGL.ToString());
                   ;
                    if (null == regularGL)
                        throw new CustomException("Party Reconciliation GL not found!");

                    voucherVM.GLGeneralInfoId = regularGL["GLGeneralInfoId"].ToString();
                    voucherVM.BudgetMasterId = regularGL["BudgetMasterId"].ToString();
                    voucherVM.ActivityId = regularGL["ActivityId"].ToString();

                    var invoiceDetail = new InvoiceDetail
                    {
                        GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                        BudgetMasterId = voucherVM.BudgetMasterId,
                        ActivityId = voucherVM.ActivityId,
                        Amount = voucherVM.Amount,
                        NetAmount = voucherVM.Amount
                    };
                    invoiceDetailId++;
                    _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, invoiceDetailId);

                    var voucherCr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                        BudgetMasterId = invoiceDetail.BudgetMasterId,
                        ActivityId = invoiceDetail.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        EntityId = voucherVM.EntityId,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        EmployeeId = null,
                        PartyType = BeneficiaryType.Vendor.ToString(),
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        VoucherId = voucher.Id,
                        InvoiceDetailId = invoiceDetail.Id,
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                    {
                        FromCurrencyId = companyCurrencyId,
                        ParallelCurrencyId = companyCurrencyId,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        ToCurrencyId = voucherVM.CurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        CrAmount = voucherVM.Amount * voucherVM.CompanyCurrencyRate
                    });
                }
                if (voucherVM.BeneficiaryType == BeneficiaryType.Self.ToString())
                {
                    var employeepayableDetail = new EmployeePayableDetail
                    {
                        GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                        BudgetMasterId = voucherVM.BudgetMasterId,
                        ActivityId = voucherVM.ActivityId,
                        Amount = voucherVM.Amount,
                        NetAmount = voucherVM.Amount
                    };
                    employeePayableDetailId++;
                    _employeePayableService.InsertEmployeePayableDetail(employeePayable, employeepayableDetail, employeePayableDetailId);
                    var voucherCr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                        BudgetMasterId = employeepayableDetail.BudgetMasterId,
                        ActivityId = employeepayableDetail.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        EntityId = voucherVM.EntityId,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        EmployeeId = voucherVM.EmployeeId,
                        PartyType = employeePayable.PartyType,
                        PartyId = null,
                        VoucherId = voucher.Id,
                        EmployeePayableDetailId = employeepayableDetail.Id,
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                    {
                        FromCurrencyId = companyCurrencyId,
                        ParallelCurrencyId = companyCurrencyId,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        ToCurrencyId = voucherVM.CurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        CrAmount = voucherVM.Amount * voucherVM.CompanyCurrencyRate
                    });

                    var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        AdvanceId = null,
                        EmployeeId = employeePayable.EmployeeId,
                        EmployeeTransactionTypeId = employeePayable.EmployeeTransactionTypeId,
                        AdvanceWriteOffId = null,
                        EmployeePayableId = employeePayable.Id,
                        PartyType = employeePayable.PartyType,
                        CurrencyId = employeePayable.CurrencyId,
                        Amount = voucherCr.CrAmount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        JournalType = AdvanceType.General.ToString(),
                        TransactionType = EmployeeSubsequentTranEnum.Payable.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType.ToString(),
                        IsPark = voucherVM.IsPark,
                        Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                        VoucherId = voucher.Id,
                        VoucherDetailId = voucherCr.Id,
                        PaymentSource = voucherVM.PaymentSource,
                    };
                    AuditService.AddedLog(EmployeeSubsequentAdvance);
                    _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);


                }
                //Employee Payable voucher Detail and voucher DetailCurrency

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void DeleteApprovedExpenseBooking(string expensesBookingId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var expensesBooking = _expenseBookingRepository.Find(expensesBookingId);
                var expensesActivity = _expenseActivityRepository.Query(r => r.ExpenseBookingId == expensesBookingId).Select().ToList();
                var expensesBookingDetail = _expenseBookingDetailRepository.Query(r => r.ExpenseBookingId == expensesBookingId).Select().ToList();
                var expensesBookingHistory = _expenseBookingApprovalHistoryRepository.Query(r => r.ExpenseBookingId == expensesBookingId).Select().ToList();
                if(expensesActivity != null)
                {
                    foreach (var item in expensesActivity)
                    {
                        _expenseActivityRepository.Delete(item.Id);
                    }
                }
               
                foreach (var item in expensesBookingDetail)
                    {
                        _expenseBookingDetailRepository.Delete(item.Id);
                    }
                    foreach (var item in expensesBookingHistory)
                    {
                        _expenseBookingApprovalHistoryRepository.Delete(item.Id);
                    }
                    _expenseBookingRepository.Delete(expensesBooking);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel QueryExpenseBookingApproved(GridParameter parameters, string companyGroupId, string companyId)
        {
            parameters.CmdText = @"SELECT A.Id, P.Code +' - ' + P.UserName AS PartyName,  A.DocDate, A.DocRefNo,A.SourceType
									, C.Code AS CurrencyName, A.Amount, A.PostingDate, V.VoucherNo
                                    FROM [TRN].[Advance] AS A
                                    INNER JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                    INNER JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
									LEFT JOIN [TRN].Voucher AS V ON V.Id=A.VoucherId
                                    WHERE A.OpeningBalanceId IS NULL AND A.Archive=0
                                    AND A.[SourceType]='" + SourceType.EmployeePayable + "' AND A.CompanyGroupId='" + companyGroupId + "' AND A.CompanyId='" + companyId + "' ";
            return _sqlRepository.GetGridData(parameters);
        }

        public string GetEmployeeTransactionNo(string employeeId)
        {
            var dayprefix = DateTime.Now.Year.ToString().Substring(2) + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour;
            return dayprefix + employeeId + new Random().Next(10, 100);
        }

        public Dictionary<string, object> GetExpenseBookingReportHeader(string companyGroupId, string companyId, string plantId, string expensesBookingId)
        {
            var sql = @"SELECT REPLACE(CONVERT(VARCHAR(11), EB.InvoiceDate, 106), ' ', '-') AS InvoiceDate, EB.InvoiceNumber, E.UserName AS EntityName,  C.Code AS CurrencyCode, UPPER(EB.Remarks) AS Narration
                        , EI.EmployeeCode, EI.EmployeeName, EB.BeneficiaryType, EBAH.EmployeeId, EIA.EmployeeCode AS ApprovedByCode, EIA.EmployeeName AS ApprovedByName, P.Code AS PartyCode, P.UserName AS PartyName
                        , REPLACE(CONVERT(VARCHAR(11), EB.AddedDate, 106), ' ', '-') AS VoucherDate, EB.ApprovalStatus, EIR.EmployeeName AS ResponsiblePersonName
                        FROM [TRN].[ExpenseBooking] AS EB
                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=EB.EntityId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
                        LEFT JOIN [TRN].[ExpenseBookingApprovalHistory] AS EBAH ON EBAH.ExpenseBookingId=EB.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EIA ON EIA.SystemId=EBAH.EmployeeId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                        WHERE EB.CompanyGroupId='" + companyGroupId + "' AND EB.CompanyId='" + companyId + "' AND EB.PlantId='" + plantId + "' AND EB.Id='" + expensesBookingId + "'";
            return _sqlRepository.GetData(sql);
        }

        public IEnumerable<object> GetCboCostCenterIdByEntity(string entityId)
        {
            var sql = @"select DISTINCT CC.UserName Text,ECC.CostCenterId [Value] from [ORG].[EntityCostCenter] ECC 
                        JOIN ORG.CostCenter CC ON CC.Id=ECC.CostCenterId
                        WHERE ECC.EntityId='" + entityId + "' ";
            return _sqlRepository.GetDataCollection(sql);
        }


        public List<Dictionary<string, object>> GetExpenseBookingReportData(string expensesBookingId)
        {
            var sql = @"SELECT GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, B.UserName AS BudgetCode, B.UserName AS BudgetName, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , EBD.Amount, MM.UserName AS AssetItem, FAR.InvoiceNo
                        FROM [TRN].[ExpenseBookingDetail] AS EBD
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=EBD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=EBD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=EBD.ActivityId
						LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=EBD.MaterialMasterId
						LEFT JOIN [TRN].[FixedAssetRegister] AS FAR ON FAR.Id=EBD.FixedAssetRegisterId
                        WHERE EBD.ExpenseBookingId='" + expensesBookingId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public DataTable GetEmployeeExpenseBookingData(string companyGroupId, string companyId, string plantId, string employeeId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT EB.CurrencyId, C.Code AS CurrencyCode, EB.InvoiceNumber, REPLACE(CONVERT(VARCHAR(11), EB.InvoiceDate, 106), ' ', '-') AS InvoiceDate, EB.Remarks, EB.BeneficiaryType
                            , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
                            , REPLACE(CONVERT(VARCHAR(11), EBD.DocDate, 106), ' ', '-') AS DocDate, EBD.DocRefNo, EBD.Amount
                            FROM [TRN].[ExpenseBooking] AS EB
                            LEFT JOIN [TRN].[ExpenseBookingDetail] AS EBD on EBD.ExpenseBookingId=EB.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=EBD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=EBD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=EBD.ActivityId
                            WHERE EB.Archive=0 AND EB.CompanyGroupId='" + companyGroupId + "' AND EB.CompanyId=@companyId AND EB.PlantId='" + plantId + "' AND EB.EmployeeId='" + employeeId + "' AND EB.InvoiceDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate.ToDbDate() + @"'
                            ORDER BY EB.InvoiceDate ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public DataTable GetAssetRegisterExpenseBookingData(string companyGroupId, string companyId, string plantId, string fixedAssetRegisterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT EB.CurrencyId, C.Code AS CurrencyCode, EB.InvoiceNumber, REPLACE(CONVERT(VARCHAR(11), EB.InvoiceDate, 106), ' ', '-') AS InvoiceDate, EB.Remarks, EB.BeneficiaryType
                            , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
                            , REPLACE(CONVERT(VARCHAR(11), EBD.DocDate, 106), ' ', '-') AS DocDate, EBD.DocRefNo, EBD.Amount
                            FROM [TRN].[ExpenseBooking] AS EB
                            LEFT JOIN [TRN].[ExpenseBookingDetail] AS EBD on EBD.ExpenseBookingId=EB.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=EB.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=EBD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=EBD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=EBD.ActivityId
                            WHERE EB.Archive=0 AND EB.CompanyGroupId='" + companyGroupId + "' AND EB.CompanyId=@companyId AND EB.PlantId='" + plantId + "' AND EBD.FixedAssetRegisterId='" + fixedAssetRegisterId + "' AND EB.InvoiceDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate.ToDbDate() + @"'
                            ORDER BY EB.InvoiceDate ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        #region EntityExpensesBooking
        public string InsertEntityExpenses(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                    throw new CustomException("Cash Id not found!");
                if (voucherVM.CashMasterId == voucherVM.OtherCashMasterId)
                    throw new CustomException("Same to same cash transfer is not allowed.");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                var bankJournal = _bankJournalService.InsertBankJournal(new BankJournal
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    CashMasterId = voucherVM.CashMasterId,
                    IsPark = voucherVM.IsPark,
                    SourceType = SourceType.CashExpenses.ToString(),
                    PaymentSource = PaymentSource.Cash.ToString(),
                    BankJournalType = voucherVM.BankJournalType,
                    Amount = voucherVM.Amount
                });

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(new Voucher
                {
                    CompanyGroupId = bankJournal.CompanyGroupId,
                    CompanyId = bankJournal.CompanyId,
                    PlantId = bankJournal.PlantId,
                    EntityId = bankJournal.EntityId,
                    CurrencyId = bankJournal.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = bankJournal.PostingDate,
                    DocDate = bankJournal.DocDate,
                    DocRefNo = bankJournal.DocRefNo,
                    Narration = bankJournal.Narration,
                    SourceType = bankJournal.SourceType,
                    AddedBy = bankJournal.AddedBy,
                    AddedFromIP = bankJournal.AddedFromIP,
                    AddedDate = bankJournal.AddedDate,
                    Archive = bankJournal.Archive,
                    IsPark = bankJournal.IsPark,
                    VoucherTypeId = voucherVM.VoucherTypeId
                }, voucherVM.FiscalYearPrefix);

                // Set Voucher Id to BankJournal Table.
                bankJournal.VoucherId = voucher.Id;

                var cashMaster = _accountsCommonService.GetCashMaster(bankJournal.CashMasterId);

                // INSERT INTO VoucherDetail Credit
                var currentVoucherDetailId = 1;
                var voucherDetailCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                {
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    CurrencyId = voucher.CurrencyId,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = voucher.Narration,
                    CrAmount = bankJournal.Amount,
                    CashMasterId = bankJournal.CashMasterId,
                    PaymentSource = bankJournal.PaymentSource,
                    PartyType = bankJournal.PaymentSource,
                    GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString(),
                    BudgetMasterId = cashMaster["BudgetMasterId"].ToString(),
                    ActivityId = cashMaster["ActivityId"].ToString()
                }, currentVoucherDetailId);

                // INSRT INTO GLTransactionDetail
                _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                {
                    SourceType = voucherDetailCr.PaymentSource,
                    CashMasterId = voucherDetailCr.CashMasterId,
                    CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate
                });

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailCr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                });

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetailCr.DrAmount;
                var totalAmountCr = voucherDetailCr.CrAmount;

                // INSERT INTO Debit Side
                var currentBankJournalDetailId = 0;

                if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                    throw new CustomException("Expense GL list not found!");

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.Amount < 0)
                        throw new CustomException("Please ensure all line item have amount.");

                    // INSERT INTO BankJournalDetail
                    currentBankJournalDetailId++;
                    var bankJournalDetail = _bankJournalService.InsertBankJournalDetail(bankJournal, new BankJournalDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        Amount = voucherDetailVM.Amount
                    }, currentBankJournalDetailId);

                    currentVoucherDetailId++;
                    var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                    {
                        BankJournalDetailId = bankJournalDetail.Id,
                        GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                        BudgetMasterId = bankJournalDetail.BudgetMasterId,
                        ActivityId = bankJournalDetail.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = bankJournalDetail.Amount,
                        PaymentSource = bankJournal.PaymentSource,
                        PartyType = bankJournal.PaymentSource,
                        Narration = voucherVM.Narration,
                        CostCenterId = voucherDetailVM.CostCenterId,
                        TrnNature = TransactionNature.ToExpense.ToString()
                    }, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                }
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public List<Dictionary<string, object>> GetEntityExpensesBookingDetail(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                              SELECT GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,VD.GLGeneralInfoId, BGM.RefNo, B.UserName AS BudgetName,VD.BudgetMasterId
							,VD.ActivityId , A.UserName AS ActivityName, VD.CurrencyId, VD.CostCenterId, VD.DrAmount AS Amount, VD.CrAmount
                            , BM.AccountTitle AS BankName, CM.UserName AS CashName, CC.CompanyCurrencyId, CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount,BJ.Id AS BankJournalId
                            , GC.CompanyGroupCurrencyId, GC.CompanyGroupCurrencyDrAmount, GC.CompanyGroupCurrencyCrAmount, P.UserName AS PartyName,VD.BankJournalDetailId AS Id,VD.Id AS VoucherDetailId
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [TRN].[BankJournalDetail] AS BJD ON BJD.Id=VD.BankJournalDetailId
                            LEFT JOIN [TRN].[BankJournal] AS BJ ON BJ.Id=BJD.BankJournalId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS B ON B.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=BJD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherId=V.Id AND CC.VoucherDetailId=VD.Id
                            LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                            ) AS GC ON GC.VoucherId=V.Id AND GC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND VD.CashMasterId IS NULL AND V.SourceType='" + sourceType + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataCollection(cmdText);
        }
        public string UpdateCashJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                    throw new CustomException("Cash Id not found!");
                if (voucherVM.CashMasterId == voucherVM.OtherCashMasterId)
                    throw new CustomException("Same to same cash transfer is not allowed.");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                var bankJournal = _bankJournalService.FindBankJournal(voucherVM.Id);
                if (null == bankJournal)
                    throw new CustomException("Bank journal master data not found.");

                bankJournal.EntityId = voucherVM.EntityId;
                bankJournal.DocRefNo = voucherVM.DocRefNo;
                bankJournal.Narration = voucherVM.Narration;
                bankJournal.Amount = voucherDetailVMList.Sum(r => r.Amount);
                _bankJournalService.UpdateBankJournal(bankJournal);

                var voucher = _voucherService.FindVoucher(bankJournal.VoucherId);
                voucher.EntityId = bankJournal.EntityId;
                voucher.DocRefNo = bankJournal.DocRefNo;
                voucher.Narration = bankJournal.Narration;
                voucher.UpdatedBy = bankJournal.UpdatedBy;
                voucher.UpdatedDate = bankJournal.UpdatedDate;
                voucher.UpdatedFromIP = bankJournal.UpdatedFromIP;
                _voucherService.UpdateVoucher(voucher);

                var voucherDetailList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                var voucherDetailCurrencyList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();

                var voucherDetailCr = voucherDetailList.FirstOrDefault(r => r.CashMasterId == bankJournal.CashMasterId);
                voucherDetailCr.DocRefNo = voucher.DocRefNo;
                voucherDetailCr.Narration = voucher.Narration;
                voucherDetailCr.CrAmount = voucherDetailVMList.Sum(r=>r.Amount);
                _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);

                var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetailCr.Id);
                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                _voucherService.UpdateGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                var voucherDetailCompanyCurrency = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailCr.Id && r.ParallelCurrencyId == companyCurrencyId);
                voucherDetailCompanyCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                voucherDetailCompanyCurrency.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                voucherDetailCompanyCurrency.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCompanyCurrency);

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetailCr.DrAmount;
                var totalAmountCr = voucherDetailCr.CrAmount;


                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("Expense GL list not found!");
                    var currentBankJournalDetailId = _bankJournalService.GetBankJournalDetailPK(bankJournal.Id);
                    var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);

                foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        var bankJournalDetail = _bankJournalService.FindBankJournalDetail(voucherDetailVM.Id);
                        if (null == bankJournalDetail)
                        {
                            // INSERT INTO BankJournalDetail
                            currentBankJournalDetailId++;
                             var bankJDetail=_bankJournalService.InsertBankJournalDetail(bankJournal, new BankJournalDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount
                            }, currentBankJournalDetailId);
                        currentVoucherDetailId++;
                            var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankJournalDetailId = bankJDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.Amount,
                                PaymentSource = bankJournal.PaymentSource,
                                PartyType = bankJournal.PaymentSource,
                                Narration = voucherVM.Narration,
                                TrnNature = TransactionNature.ToExpense.ToString()
                            }, currentVoucherDetailId);

                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                            });

                            totalAmountDr += voucherDetailDr.DrAmount;
                            totalAmountCr += voucherDetailDr.CrAmount;
                        }
                        else
                        {
                            bankJournalDetail.Amount = bankJournal.Amount;
                            bankJournalDetail.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                            bankJournalDetail.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                            bankJournalDetail.ActivityId = voucherDetailVM.ActivityId;
                            bankJournalDetail.Amount = voucherDetailVM.Amount;
                            _bankJournalService.UpdateBankJournalDetail(bankJournal, bankJournalDetail);

                            var voucherDetailDr = voucherDetailList.FirstOrDefault(r => r.Id == voucherDetailVM.VoucherDetailId);
                            if (null == voucherDetailDr)
                                throw new CustomException("Voucher Detail(For Expense) data not found!");

                            voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                            voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                            voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                            voucherDetailDr.DrAmount = bankJournalDetail.Amount;
                            voucherDetailDr.Narration = voucherVM.Narration;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);

                            var voucherDetailCompanyCurrencyDr = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailDr.Id && r.ParallelCurrencyId == companyCurrencyId);
                            voucherDetailCompanyCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                            voucherDetailCompanyCurrencyDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                            voucherDetailCompanyCurrencyDr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                            _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCompanyCurrencyDr);

                            totalAmountDr += voucherDetailDr.DrAmount;
                            totalAmountCr += voucherDetailDr.CrAmount;
                        }
                    }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public Dictionary<string, object> GetExpenseBookingFile(string id)
        {
            try
            {
                var sql = @"Select Id, FileName From [TRN].[ExpenseBooking]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion
    }
}