using Library.Model.Finances;
using Library.Model.Systems;
using Library.ViewModel.Vouchers;

namespace Library.Service.Finances
{
    public interface IFinancingService
    {
        Financing Insert(Financing financing);
        void UpdateFinancing(Financing financing);
        void UpdateFinancingDetail(FinancingDetail financingDetail);

        Financing InsertFinancing(Financing financing);

        PKGenerator GetMaxNumber();

        void InsertFinancingDetail(Financing financing, FinancingDetail financingDetail);

        void InsertFinancingSchedule(Financing financing, FinancingSchedule financingSchedule);
        void DeleteLoan(string companyId, string plantId, string voucherId);
        void DeleteAutoloanPost(string companyId, string plantId, string voucherId);
        void Post(string financingId);
        void PostFinancingWriteOff(string financingWriteOffId);
        void DeleteLoanPayment(string companyId, string plantId, string voucherId);
        Financing FindFinancing(string financingId);
        FinancingDetail FindFinancingDetail(string financingDetailId);
        FinancingWriteOff InsertFinancingWriteOff(FinancingWriteOff invoiceWriteOff);
        void InsertFinancingWriteOffDetail(FinancingWriteOff invoiceWriteOff, FinancingDetailWriteOff invoiceWriteOffDetail, int currentId);
        void PostLoanInterestPayable(string voucherId);
        void DeleteLoanInterestPayable(string companyId, string plantId, string LoanIntPayableId, string voucherId);
        void DeleteLoanInterestPayableReverse(string companyId, string plantId, string loanIntPayableId, string voucherId);
        void DeleteInvestment(string companyId, string plantId, string voucherId);
        void InsertFinancingMasterOrder(Financing financing, FinancingMasterOrder financingMasterOrder, int currentId);

    }
}