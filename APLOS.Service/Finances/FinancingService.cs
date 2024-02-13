using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Systems;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System;
using System.Linq;
using System.Reflection;

namespace Library.Service.Finances
{
    public class FinancingService : IFinancingService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<Financing> _financingRepository;
        private readonly IRepositoryAsync<FinancingDetail> _investmentDetailRepository;
        private readonly IRepositoryAsync<FinancingSchedule> _financingScheduleRepository;
        private readonly IRepositoryAsync<FinancingWriteOff> _financingWriteOffRepository;
        private readonly IRepositoryAsync<FinancingDetailWriteOff> _financingDetailWriteOffRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IVoucherService _voucherService;
        private readonly IRepositoryAsync<FinancingSubsequentTransaction> _loanInterestPayableRepository;
        private readonly IRepositoryAsync<FinancingMasterOrder> _financingMasterOrderRepository;

        public FinancingService(
              IRepositoryAsync<Financing> financingRepository
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<FinancingDetail> investmentDetailRepository
            , IRepositoryAsync<FinancingSchedule> financingScheduleRepository
            , IRepositoryAsync<FinancingWriteOff> financingWriteOffRepository
            , IRepositoryAsync<FinancingDetailWriteOff> financingDetailWriteOffRepository
            , IVoucherService voucherService
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<FinancingSubsequentTransaction> loanInterestPayableRepository
            , IRepositoryAsync<FinancingMasterOrder> financingMasterOrderRepository
            )
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _financingRepository = financingRepository;
            _financingWriteOffRepository = financingWriteOffRepository;
            _financingDetailWriteOffRepository = financingDetailWriteOffRepository;
            _investmentDetailRepository = investmentDetailRepository;
            _financingScheduleRepository = financingScheduleRepository;
            _pkGeneratorService = pkGeneratorService;
            _voucherService = voucherService;
            _voucherRepository = voucherRepository;
            _loanInterestPayableRepository = loanInterestPayableRepository;
            _financingMasterOrderRepository = financingMasterOrderRepository;
        }

        #endregion Constructor

        public Financing InsertFinancing(Financing financing)
        {
            financing.Id = _pkGeneratorService.GetAutoNumber(nameof(Financing), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(financing);
            _financingRepository.Insert(financing);
            return financing;
        }

        /// <summary>
        /// This is for opening balance
        /// </summary>
        /// <param name="financing"></param>
        /// <returns></returns>
        public Financing Insert(Financing financing)
        {
            _financingRepository.Insert(financing);
            return financing;
        }

        public void UpdateFinancing(Financing financing)
        {
            if (string.IsNullOrEmpty(financing.AddedBy))
                AuditService.AddedLog(financing);
            _financingRepository.Update(financing);
        }

        public void UpdateFinancingDetail(FinancingDetail financingDetail)
        {
            if (string.IsNullOrEmpty(financingDetail.AddedBy))
                AuditService.AddedLog(financingDetail);
            _investmentDetailRepository.Update(financingDetail);
        }



        public PKGenerator GetMaxNumber()
        {
            return _pkGeneratorService.GetMaxNumber(nameof(Financing), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertFinancingDetail(Financing financing, FinancingDetail financingDetail)
        {
            financingDetail.Id = _pkGeneratorService.MakePK(financing.Id, 1, 2);
            financingDetail.FinancingId = financing.Id;
            financingDetail.AddedBy = financing.AddedBy;
            financingDetail.AddedDate = financing.AddedDate;
            financingDetail.AddedFromIP = financing.AddedFromIP;
            _investmentDetailRepository.Insert(financingDetail);
        }

        public void InsertFinancingSchedule(Financing financing, FinancingSchedule financingSchedule)
        {
            financingSchedule.Id = _pkGeneratorService.MakePK(financing.Id, financingSchedule.InstallmentNo, 3);
            financingSchedule.FinancingId = financing.Id;
            financingSchedule.AddedBy = financing.AddedBy;
            financingSchedule.AddedDate = financing.AddedDate;
            financingSchedule.AddedFromIP = financing.AddedFromIP;
            _financingScheduleRepository.Insert(financingSchedule);
        }
        public void InsertFinancingMasterOrder(Financing financing, FinancingMasterOrder financingMasterOrder, int currentId)
        {
            financingMasterOrder.Id = _pkGeneratorService.MakePK(financing.Id, currentId, 3);
            financingMasterOrder.AddedBy = financing.AddedBy;
            financingMasterOrder.AddedDate = financing.AddedDate;
            financingMasterOrder.AddedFromIP = financing.AddedFromIP;
            _financingMasterOrderRepository.Insert(financingMasterOrder);
        }

        public void Post(string financingId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var financing = _financingRepository.Find(financingId);

                var financingwriteOff = _financingWriteOffRepository.Query(r => r.VoucherId == financing.VoucherId).Select().FirstOrDefault();
                var loanInterestPayable = _loanInterestPayableRepository.Query(r => r.FinancingId == financingId).Select().ToList();
                var financingSubsequentTransaction = _loanInterestPayableRepository.Query(r => r.SetOffFinancingId == financingId).Select().ToList();

                CheckIsPosted(financing);

                if(financingwriteOff != null)
                {
                    financingwriteOff.IsPark = false;
                    AuditService.UpdatedLog(financingwriteOff);
                    _financingWriteOffRepository.Update(financingwriteOff);
                }
                if (loanInterestPayable != null)
                {
                    foreach (var item in loanInterestPayable)
                    {
                        item.IsPark = false;
                        AuditService.UpdatedLog(item);
                        _loanInterestPayableRepository.Update(item);
                    }
                    
                }
                if (financingSubsequentTransaction != null)
                {
                    foreach (var item in financingSubsequentTransaction)
                    {
                        item.IsPark = false;
                        AuditService.UpdatedLog(item);
                        _loanInterestPayableRepository.Update(item);
                    }
                   
                }
                financing.IsPark = false;
                AuditService.UpdatedLog(financing);
                _financingRepository.Update(financing);
                _voucherService.PostVoucher(financing.VoucherId);
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



        public Financing FindFinancing(string financingId)
        {
            return _financingRepository.Find(financingId);
        }

        public FinancingDetail FindFinancingDetail(string financingDetailId)
        {
            return _investmentDetailRepository.Find(financingDetailId);
        }

        private static void CheckIsPosted(Financing financing)
        {
            if (financing.IsPosted)
                throw new CustomException("Update is not allowed after Posted.");
            if (!financing.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        public FinancingWriteOff InsertFinancingWriteOff(FinancingWriteOff invoiceWriteOff)
        {
            invoiceWriteOff.Id = _pkGeneratorService.GetAutoNumber(nameof(FinancingWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
            if (string.IsNullOrEmpty(invoiceWriteOff.AddedBy))
                AuditService.AddedLog(invoiceWriteOff);
            _financingWriteOffRepository.Insert(invoiceWriteOff);
            return invoiceWriteOff;
        }

        public void InsertFinancingWriteOffDetail(FinancingWriteOff invoiceWriteOff, FinancingDetailWriteOff invoiceWriteOffDetail, int currentId)
        {
            invoiceWriteOffDetail.AddedBy = invoiceWriteOff.AddedBy;
            invoiceWriteOffDetail.AddedDate = invoiceWriteOff.AddedDate;
            invoiceWriteOffDetail.AddedFromIP = invoiceWriteOff.AddedFromIP;
            invoiceWriteOffDetail.Archive = invoiceWriteOff.Archive;
            invoiceWriteOffDetail.FinancingWriteOffId = invoiceWriteOff.Id;
            invoiceWriteOffDetail.Id = _pkGeneratorService.MakePK(invoiceWriteOff.Id, currentId, 2);
            _financingDetailWriteOffRepository.Insert(invoiceWriteOffDetail);
        }

        private static void CheckIsPostedFinanceWriteOff(FinancingWriteOff financing)
        {
            if (financing.IsPosted)
                throw new CustomException("Update is not allowed after Posted.");
            if (!financing.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
       
        public void PostFinancingWriteOff(string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var financingwriteOff = _financingWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var loanInterestPayable = _loanInterestPayableRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                
                if (financingwriteOff != null)
                {
                    CheckIsPostedFinanceWriteOff(financingwriteOff);
                    financingwriteOff.IsPark = false;
                    AuditService.UpdatedLog(financingwriteOff);
                    _financingWriteOffRepository.Update(financingwriteOff);
                }
                if (loanInterestPayable != null)
                {
                    foreach (var item in loanInterestPayable)
                    {
                        CheckIsPostedLoanInterestPayable(item);
                        item.IsPark = false;
                        AuditService.UpdatedLog(item);
                        _loanInterestPayableRepository.Update(item);
                    }
                   
                }

                _voucherService.PostVoucher(voucherId);
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

        public void DeleteLoan(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"delete from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                
                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete from trn.FinancingDetailWriteOff where FinancingWriteOffId in(select Id from trn.FinancingWriteOff where voucherId= '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from trn.FinancingWriteOff  where voucherId= '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.FinancingSchedule where FinancingId in (select Id from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete TRN.BankCharge where FinancingId in (select Id from trn.Financing where   VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.FinancingDetail where FinancingId in (select Id from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
               
                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Loan.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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

        public void DeleteAutoloanPost(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"delete from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);


                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"update TRN.InvoiceDetail set WrittenOffAmount=0,IsWrittenOff=0 where InvoiceId in(select InvoiceId from TRN.InvoiceWriteOffDetail where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"update TRN.Invoice set WrittenOffAmount=0,IsWrittenOff=0 where Id in(select InvoiceId from TRN.InvoiceWriteOffDetail where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.InvoiceWriteOffDetail where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.InvoiceWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete from TRN.FinancingSchedule where FinancingId in (select Id from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.FinancingDetail where FinancingId in (select Id from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"update LoanAgainstAcceptanceMaster set VoucherId=null where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"update InvoiceTaggingWithLCMaster set VoucherId=null where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.AutoLoan.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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

        public void DeleteLoanPayment(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                var laonIntPayable = _loanInterestPayableRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var financingWriteOff = _financingWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                if (financingWriteOff != null)
                {
                    vendorAdWrsql = @"declare @writeOffAmount decimal(18,2)=(select Amount from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND VoucherId = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);

                    vendorAdWrsql = @"update TRN.Financing set WrittenOffAmount=(WrittenOffAmount - @writeOffAmount),IsWrittenOff= 0 
                                where Id in (select FinancingId from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND VoucherId = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update TRN.FinancingDetail set WrittenOffAmount=(WrittenOffAmount - @writeOffAmount)
                                where Id in (select FinancingDetailId from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND VoucherId = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                if (laonIntPayable != null)
                {
                    vendorAdWrsql = @"delete from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from [TRN].[CheckLotDetailHistory] where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                if (financingWriteOff != null)
                {
                    vendorAdWrsql = @"delete from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete from TRN.FinancingWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                

                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanPayment.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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

        //loan interest payable delete option
        public void DeleteLoanInterestPayable(string companyId, string plantId, string loanIntPayableId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan interest payable
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                var laonIntPayable = _loanInterestPayableRepository.Query(r => r.Id == loanIntPayableId).Select().FirstOrDefault();
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                if (voucher.SourceType.ToString() == "AdditionalLoanPayable")
                {
                    vendorAdWrsql = @"declare @additionalLoanPayableAmount decimal(18,2)=(select Amount from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from TRN.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + voucher.SourceType.ToString() + "' AND Id = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"declare @financingId varchar(10)=(select FinancingId from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from TRN.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + voucher.SourceType.ToString() + "' AND Id = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);

                    vendorAdWrsql = @"update TRN.Financing set AdditionalLoanAmount=(AdditionalLoanAmount - @additionalLoanPayableAmount) where Id=@financingId ";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update TRN.FinancingDetail set AdditionalLoanAmount=(AdditionalLoanAmount - @additionalLoanPayableAmount) where FinancingId=@financingId ";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                if (laonIntPayable != null)
                {
                    vendorAdWrsql = @"delete from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + voucher.SourceType.ToString() + "' AND Id = '" + voucherId + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + voucher.SourceType.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + voucher.SourceType.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + voucher.SourceType.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + voucher.SourceType.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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
        public void DeleteLoanInterestPayableReverse(string companyId, string plantId, string loanIntPayableId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan interest payable
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                var laonIntPayable = _loanInterestPayableRepository.Query(r => r.Id == loanIntPayableId).Select().FirstOrDefault();
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var laonIntPayableReverseWr = new System.Text.StringBuilder();
                var laonIntPayableReversesql = "";

                if (laonIntPayable != null)
                {
                    laonIntPayableReversesql = @"delete from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanInterestPayableReverse.ToString() + "' AND Id = '" + voucherId + "')";
                    laonIntPayableReverseWr.Append(laonIntPayableReversesql);
                }
                //vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanInterestPayable.ToString() + "' AND Id = '" + voucherId + "'))";
                //vendorAdWr.Append(vendorAdWrsql);
                laonIntPayableReversesql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanInterestPayableReverse.ToString() + "' AND Id = '" + voucherId + "')";
                laonIntPayableReverseWr.Append(laonIntPayableReversesql);
                laonIntPayableReversesql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanInterestPayableReverse.ToString() + "' AND Id = '" + voucherId + "')";
                laonIntPayableReverseWr.Append(laonIntPayableReversesql);
                laonIntPayableReversesql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanInterestPayableReverse.ToString() + "' AND Id = '" + voucherId + "'";
                laonIntPayableReverseWr.Append(laonIntPayableReversesql);
                _sqlRepository.ExecuteSqlCommand(laonIntPayableReverseWr.ToString());
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

        public void DeleteInvestmentPayment(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                var laonIntPayable = _loanInterestPayableRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var financingWriteOff = _financingWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                if (financingWriteOff != null)
                {
                    vendorAdWrsql = @"declare @writeOffAmount decimal(18,2)=(select Amount from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND VoucherId = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);

                    vendorAdWrsql = @"update TRN.Financing set WrittenOffAmount=(WrittenOffAmount - @writeOffAmount),IsWrittenOff= 0 
                                where Id in (select FinancingId from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND VoucherId = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update TRN.FinancingDetail set WrittenOffAmount=(WrittenOffAmount - @writeOffAmount)
                                where Id in (select FinancingDetailId from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND VoucherId = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                if (laonIntPayable != null)
                {
                    vendorAdWrsql = @"delete from TRN.FinancingSubsequentTransaction where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from [TRN].[CheckLotDetailHistory] where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                if (financingWriteOff != null)
                {
                    vendorAdWrsql = @"delete from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete from TRN.FinancingWriteOff where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                }


                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.InvestmentSetOff.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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

        //DeleteSalaryJournal

        private static void CheckIsPostedLoanInterestPayable(FinancingSubsequentTransaction loanInterestPayable)
        {

            if (!loanInterestPayable.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
        public void PostLoanInterestPayable(string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var loanInterestPayable = _loanInterestPayableRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                if (loanInterestPayable!=null)
                {
                    foreach (var item in loanInterestPayable)
                    {
                        CheckIsPostedLoanInterestPayable(item);

                        item.IsPark = false;
                        AuditService.UpdatedLog(item);
                        _loanInterestPayableRepository.Update(item);
                        _voucherService.PostVoucher(item.VoucherId);
                    }
                }
                
                
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
        public void DeleteInvestment(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);


                vendorAdWrsql = @"delete from TRN.FinancingSchedule where FinancingId in (select Id from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.FinancingSubsequentTransaction where FinancingId in (select Id from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete from TRN.FinancingDetail where FinancingId in (select Id from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.Financing where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.Investment.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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

    }
}