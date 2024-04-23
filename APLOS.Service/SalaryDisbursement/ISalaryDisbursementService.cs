using Library.Core;
using Library.Data.Repositories;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Systems;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Library.Service.SalaryDisbursement
{
    public interface ISalaryDisbursementService
    {

        string ParkSalaryPayable(VoucherViewModel voucherVM, string yearNo, string monthNo,string monthName
            , IEnumerable<VoucherDetailViewModel> directJVList, IEnumerable<VoucherDetailViewModel> inDirectJVList, IEnumerable<VoucherDetailViewModel> directSalaryLockList, IEnumerable<VoucherDetailViewModel> indirectSalaryLockList);
        string ParkSalaryPayableDisbursement(VoucherViewModel voucherVM, string yearNo, string monthNo, string monthName, string pMode, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string empSystemIds);
        string ParkGoodWorkPaymentAdviseDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string goodWorkPaymentAdviseDetailIds);
        string ParkEmployeeMultipleAdvanceDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string goodWorkPaymentAdviseDetailIds);
        string ParkFinalSettlementDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId);
        string SaveBonusDisbursementPosting(VoucherViewModel voucherVM, string fromDate, string toDate, string pMode, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string empSystemIds);
         GridModel GetSalaryPayableVoucherList(GridParameter parameters);

        IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, string voucherId);

        IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWiseSalaryPayableInVoucher(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, string voucherId);
        void DeleteSalaryPayable(string plantId, string voucherId, string monthNo, string yearNo);
        void DeleteSalaryDisbursementVoucher(string plantId, string voucherId, string monthNo, string yearNo);
        void DeleteGoodWorkPaymentAdviseDisbursement(string plantId, string voucherId);
        void DeleteEmployeeMultipleAdvanceDisbursement(string plantId, string voucherId);
        void DeleteFinalSettlementDisbursementVoucher(string plantId, string voucherId);
        void DeleteBonusDisbursementVoucher(string plantId, string voucherId);
        void PostSalarydisbursement(string voucherId);
    }
}