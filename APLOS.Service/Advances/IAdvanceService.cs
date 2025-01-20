using Library.Core;
using Library.Data.Repositories;
using Library.Model.Advances;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Service.Core;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Library.Service.Advances  
{
    public interface IAdvanceService : IService<Advance>
    {
        Dictionary<string, object> GetReportHeader(string voucherId);

        void Post(string advanceId, string entityId, string voucherId);
        void PostEmployeeAdvanceHR(string voucherId,string requisitionId);
        void PostCustomerAdvanceGroupWise(string advanveGroupNo);
        void UnPost(string advanceId);

        List<Dictionary<string, object>> GetDetail(string advanceId);

        Dictionary<string, object> GetAvailableJournal(string companyGroupId, string companyId, string plantId, string advanceId);

        List<Dictionary<string, object>> GetPartyWiseOutstandingAdvance(string companyGroupId, string companyId, string plantId, string partyId, SourceType sourceType);
        List<Dictionary<string, object>> GetPartyWiseOutstandingDebitNote(string companyGroupId, string companyId, string plantId, string partyId, SourceType sourceType);

        Dictionary<string, object> GetAdvance(string companyGroupId, string companyId, string plantId, string advanceId);

        GridModel GetAvailableJournal(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId, SourceType sourceType);

        string MakeAdvanceDetailPK(string masterId, int currentId);

        Advance InsertAdvance(Advance advance);

        Advance InsertAdvance(VoucherViewModel voucherVM);

        AdvanceDetail InsertAdvanceDetail(AdvanceDetail advanceDetail);

        AdvanceDetail InsertAdvanceDetail(Advance advance, AdvanceDetail advanceDetail, int currentAdvanceDetaiId);

        AdvanceDetail InsertAdvanceDetail(Advance advance, int currentAdvanceDetaiId, VoucherDetailViewModel advanceDetailVM);

        AdvanceDetail FindAdvanceDetail(string advanceDetailId);

        Dictionary<string, object> GetById(string companyGroupId, string companyId, string plantId, string id);

        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        GridModel GetCustomerPaymentList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);


        Dictionary<string, object> Query(string companyGroupId, string companyId, string plantId, string partyId, string advanceId, SourceType sourceType);

        string InsertCrAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);

        string InsertDrAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        string InsertCustomerAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertMultiBankCustomerAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);
        string InsertEmployeeAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);
        string InsertEmployeeAdvanceRequisition(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);
        string CreateEmployeeAdvanceHRPark(VoucherViewModel voucherVM, Dictionary<string, object> data, List<Dictionary<string, object>> advanceDetail, IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist);
        string UpdateCrAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);

        string UpdateCustomerAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        string UpdateDrAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);

        string UpdateEmployeeAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<AdvanceReqSchedule> DetailsList);

        string InsertCustomerPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);

        decimal GetCustomerTotalAdvanceAmount(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId);

        GridModel GetEmployeeAvilabePayableList(GridParameter parameters, string companyGroupId, string companyId, string employeeId);

        IQueryFluent<AdvanceDetail> GetAdvanceDetailList(Expression<Func<AdvanceDetail, bool>> query);

        AdvanceDetail UpdateAdvanceDetail(AdvanceDetail advanceDetail);

        string UpdateCustomerPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);

        string InsertInterTransaction(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> NoteSetOffList, IEnumerable<VoucherDetailViewModel> employeePayableVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        void DeleteInterTransaction(string advanceId, string voucherId);
        void DeleteVendorAdvance(string companyId, string plantId, string voucherId);
        void DeleteMultiVendorAdvance(string companyId, string plantId, string voucherId, string advanceGroupNo);
        void DeleteEmployeeAdvanceWriteOff(string advanceWriteOffId, string voucherId);
        void DeleteEmployeeTotalAdvanceWriteOff(string advanceWriteOffId, string voucherId);
        void PostEmployeeAdvanceRequisition(string advanceId, string voucherId);
        void DeleteEmployeeAdvance(string companyId, string plantId, string voucherId);
        void DeleteEmployeeAdvanceHR(string employeeAdvanceId, string voucherId);

    }
}