using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Accounts;
using Library.Model.Advances;
using Library.Model.Banks;
using Library.Model.Currencies;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.FixedAssets;
using Library.Model.Invoices;
using Library.Model.Organizations;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Systems;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class AccountsCommonService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsCommonService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public void GetParallelCurrency(string companyId, out string companyCurrencyId, out string companyCurrencyCode)
        {
            var companyParallelCurrency = GetCompanyCurrencyId(companyId);
            if (null == companyParallelCurrency["CurrencyId"].ToString())
                throw new CustomException(ResourcesCore.CompanyParallelCurrencyNotConfigured);
            companyCurrencyId = companyParallelCurrency["CurrencyId"].ToString();
            companyCurrencyCode = companyParallelCurrency["CurrencyCode"].ToString();
        }
        private Dictionary<string, object> GetCompanyCurrencyId(string companyId)
        {
            var cmdText = @"select cpc.CurrencyId,C.Code CurrencyCode from SCS.CompanyParallelCurrency cpc
                            LEFT JOIN SCS.Currency C ON C.Id = CPC.CurrencyId where cpc.ParallelCurrencyType = '" + ParallelCurrencyType.CompanyCurrency.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public bool GetPlantIsShowFCInWord(string plantId)
        {
            return bplib.clsWebLib.GetBoolData(_sqlRepository.GetDataCollection(@"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='" + plantId + "'")[0]["IsShowFCInWord"].ToString());
        }

        public void CheckingFiscalYearPeriod(VoucherViewModel voucherVM)
        {
            var sql = @"SELECT CFY.FiscalYearId, FY.FiscalYearName, CFYP.FiscalYearPeriodId, FYP.PeriodName, FYP.StartDate, FYP.EndDate, CFYP.IsBudgetLocked
                        , CFYP.IsTransationLocked, CFYP.IsExchangeRateConfirmed, FY.YearPrefix
                        FROM [SCS].[CompanyFiscalYearPeriod] AS CFYP
                        INNER JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=CFYP.FiscalYearPeriodId
                        INNER JOIN [SCS].[CompanyFiscalYear] AS CFY ON CFY.Id=CFYP.CompanyFiscalYearId
                        INNER JOIN [SCS].[FiscalYear] AS FY ON FY.Id=CFY.FiscalYearId
                        WHERE CFY.CompanyId='" + voucherVM.CompanyId + "' AND FYP.StartDate <= '" + voucherVM.PostingDate.ToDbDate() + "' AND FYP.EndDate >= '" + voucherVM.PostingDate.ToDbDate() + "' ";
            var fiscalYear = _sqlRepository.GetData(sql);
            if (null == fiscalYear || fiscalYear.Count == 0)
                throw new CustomException(ResourcesCore.FYNotFound);
            if (Convert.ToBoolean(fiscalYear["IsTransationLocked"].ToString()))
                throw new CustomException($"This period ({fiscalYear["PeriodName"]}) transation is locked! Please contact with Administrator.");
            if (!Convert.ToBoolean(fiscalYear["IsExchangeRateConfirmed"].ToString()))
                throw new CustomException($"This period ({fiscalYear["PeriodName"]}) exchange rate is not confirmed! Please contact with Administrator.");
            // Set data into .....
            voucherVM.FiscalYearId = fiscalYear["FiscalYearId"].ToString();
            voucherVM.FiscalYearPeriodId = fiscalYear["FiscalYearPeriodId"].ToString();
            voucherVM.FiscalYearPrefix = fiscalYear["YearPrefix"].ToString();
        }

        public void CheckingTaxYearPeriod(VoucherViewModel voucherVM)
        {
            var sql = @"SELECT CFY.TaxYearId, FY.TaxYearName, CFYP.TaxYearPeriodId, FYP.PeriodName, FYP.StartDate, FYP.EndDate, CFYP.IsBudgetLocked, CFYP.IsTransationLocked, CFYP.IsExchangeRateConfirmed
                        FROM [SCS].[CompanyTaxYearPeriod] AS CFYP
                        INNER JOIN [SCS].[TaxYearPeriod] AS FYP ON FYP.Id = CFYP.TaxYearPeriodId
                        INNER JOIN [SCS].[CompanyTaxYear] AS CFY ON CFY.Id = CFYP.CompanyTaxYearId
                        INNER JOIN [SCS].[TaxYear] AS FY ON FY.Id = CFY.TaxYearId
                        WHERE CFY.CompanyId='" + voucherVM.CompanyId + @"' AND FYP.StartDate <= '" + voucherVM.PostingDate.ToDbDate() + "' AND FYP.EndDate >= '" + voucherVM.PostingDate.ToDbDate() + "' ";
            var taxYear = _sqlRepository.GetData(sql);
            if (null == taxYear || taxYear.Count == 0)
                throw new CustomException(ResourcesCore.FYNotFound);
            // Set data into ....
            voucherVM.TaxYearId = taxYear["TaxYearId"].ToString();
            voucherVM.TaxYearPeriodId = taxYear["TaxYearPeriodId"].ToString();
        }
        public GridModel VoucherTypeConfigQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT VTC.Id, VTC.CompanyGroupId, VTC.CompanyId, VTC.PlantId, VTC.VoucherTypeId, VT.Code AS VoucherTypeCode, VT.UserName AS VoucherTypeName, VTC.Prefix, VTC.[Period], VTC.PadLeftWidth
                                    , VTC.PadLeftChar, VTC.IsBackDatePostingAllow
                                    FROM [SCS].[VoucherTypeConfig] AS VTC
                                    LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=VTC.VoucherTypeId
                                    WHERE VTC.CompanyGroupId='" + companyGroupId + "' AND VTC.CompanyId='" + companyId + "' AND VTC.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public Dictionary<string, object> VoucherTypeConfigFind(string companyGroupId, string companyId, string plantId, string voucherTypeId)
        {
            var voucherTypeConfig = _sqlRepository.GetData(@"SELECT * FROM SCS.VoucherTypeConfig WHERE CompanyGroupId='" + companyGroupId + "' AND CompanyId='" + companyId + @"' 
                            AND PlantId='" + plantId + "' AND  VoucherTypeId='" + voucherTypeId + "'");
            if (null == voucherTypeConfig["Id"])
                throw new CustomException("Plant voucher type config not found!");
            return voucherTypeConfig;
        }
        private static string MakePeriodAuto()
        {
            return PKGeneratorEnum.Auto.ToString();
        }
        private static string MakePeriodDaily(DateTime date)
        {
            return date.Year + date.Month.ToString().PadLeft(2, '0') + date.Day.ToString().PadLeft(2, '0');
        }

        private static string MakePeriodMonthly(DateTime date)
        {
            return date.Year + date.Month.ToString().PadLeft(2, '0');
        }

        private static string MakePeriodYearly(DateTime date)
        {
            return date.Year.ToString();
        }
        private static string MakePeriodFiscalYear(string fiscalYearPrefix)
        {
            if (string.IsNullOrEmpty(fiscalYearPrefix))
                throw new CustomException("FiscalYear prefix not found!");
            return fiscalYearPrefix;
        }
        private Dictionary<string, object> GetfiscalYearfind(string fiscalYearId)
        {
            var cmdText = @"select * from scs.FiscalYear where Id= '" + fiscalYearId + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private VoucherTypeNumber GetAuto(string voucherTypeConfigId, string registerName, string period)
        {
            List<VoucherTypeNumber> data = _sqlRepository.GetModelCollection<VoucherTypeNumber>(@"select * from scs.VoucherTypeNumber where VoucherTypeConfigId = '" + voucherTypeConfigId + "' and RegisterName = '" + registerName + "' and [Period] ='" + period + "'");
            if (data.Count > 0)
                return data[0];
            return null;

        }
        public string GetNumber(string companyGroupId, string companyId, string plantId, string voucherTypeId, string registerName, string fiscalYearPrefix, DateTime date)
        {
            var voucherTypeConfig = VoucherTypeConfigFind(companyGroupId, companyId, plantId, voucherTypeId);

            if (null == voucherTypeConfig["Id"])
                throw new CustomException("Plant voucher type config not found!");
            var pkgenerator = GetPeriod(voucherTypeConfig["Id"].ToString(), registerName, voucherTypeConfig["Period"].ToString(), fiscalYearPrefix, out string prefix, date);

            DataSet dsVoucher;
            if (pkgenerator == null)
            {
                pkgenerator = new VoucherTypeNumber
                {
                    VoucherTypeConfigId = voucherTypeConfig["Id"].ToString(),
                    RegisterName = registerName,
                    Period = prefix,
                    MaxNumber = 1,
                    UpdatedDate = date,
                    ModelState = ModelState.Added
                };
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from scs.VoucherTypeNumber where 1=2", out dsVoucher);
                AddNewRow<VoucherTypeNumber>(dsVoucher.Tables[0], pkgenerator);
            }
            else
            {
                pkgenerator.MaxNumber += 1;
                pkgenerator.UpdatedDate = DateTime.Now;
                pkgenerator.ModelState = ModelState.Modified;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from scs.VoucherTypeNumber where VoucherTypeConfigId='" + pkgenerator.VoucherTypeConfigId + "' AND RegisterName='" + pkgenerator.RegisterName + "' AND Period='" + pkgenerator.Period + "'", out dsVoucher);

                VoucherTypeNumberEditRow<VoucherTypeNumber>(dsVoucher.Tables[0].Rows[0], pkgenerator);
            }
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsVoucher);

            var voucherNumber = voucherTypeConfig["Prefix"].ToString();
            if (voucherTypeConfig["Period"].ToString() != PKGeneratorEnum.Auto.ToString())
                voucherNumber += "-" + prefix;
            voucherNumber += "-" + pkgenerator.MaxNumber.ToString().PadLeft(int.Parse(voucherTypeConfig["PadLeftWidth"].ToString()), voucherTypeConfig["PadLeftChar"].ToString()[0]);
            return voucherNumber;
        }

        private VoucherTypeNumber GetPeriod(string voucherTypeConfigId, string registerName, string period, string fiscalYearPrefix, out string prefix, DateTime date)
        {
            VoucherTypeNumber pkgenerator = null;
            switch (period)
            {
                case "Auto":
                    period = MakePeriodAuto();
                    break;

                case "FiscalYear":
                    period = MakePeriodFiscalYear(fiscalYearPrefix);
                    break;

                case "Yearly":
                    period = MakePeriodYearly(date);
                    break;

                case "Monthly":
                    period = MakePeriodMonthly(date);
                    break;

                case "Daily":
                    period = MakePeriodDaily(date);
                    break;

                default:
                    throw new CustomException("VoucherType config period not found!");
            }
            pkgenerator = GetAuto(voucherTypeConfigId, registerName, period);
            prefix = period;
            return pkgenerator;
        }

        private DateTime? GetLastYearEndDate(Voucher voucher)
        {
            try
            {
                var cmdText = @"SELECT MAX(PostingDate) as PostingDate  FROM TRN.Voucher WHERE CompanyId='" + voucher.CompanyId + @"' 
                        AND PlantId='" + voucher.PlantId + "' and VoucherTypeId='" + voucher.VoucherTypeId + "' and FiscalYearId='" + voucher.FiscalYearId + "'";
                //return _voucherRepository.SqlQuery<DateTime?>(cmdText).FirstOrDefault();
                List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(cmdText);
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data[0]["PostingDate"].ToString()))
                        return Convert.ToDateTime(data[0]["PostingDate"].ToString());
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        private void CheckPostingDate(Voucher voucher)
        {
            var lastPostingDate = GetLastYearEndDate(voucher);
            var voucherTypeConfig = VoucherTypeConfigFind(voucher.CompanyGroupId, voucher.CompanyId, voucher.PlantId, voucher.VoucherTypeId);
            if (null != lastPostingDate && Convert.ToBoolean(voucherTypeConfig["IsBackDatePostingAllow"]) == false)
            {
                if (voucher.PostingDate.Date < lastPostingDate.Value.Date)
                    throw new CustomException($"Posting date cannot be less than last ({lastPostingDate.Value.Date.ToString("dd-MMM-yyyy")}) posting date!");
            }

        }
        private string GetVoucherNo(Voucher entity, string fiscalYearPrefix)
        {
            if (string.IsNullOrEmpty(fiscalYearPrefix))
                throw new CustomException("Fiscal year prefix is null.");
            return GetNumber(entity.CompanyGroupId, entity.CompanyId, entity.PlantId, entity.VoucherTypeId, nameof(Voucher), fiscalYearPrefix, entity.PostingDate);
        }

        public string GetAutoNumber(string fieldName, PKGeneratorEnum period, string companyGroupId, DateTime date)
        {
            string prefix = null; var condition = "";
            switch (period.ToString())
            {
                case "Auto":
                    prefix = MakePeriodAuto();
                    condition = $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                case "Yearly":
                    prefix = MakePeriodYearly(date);
                    condition = $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                case "Monthly":
                    prefix = MakePeriodMonthly(date);
                    condition = $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                case "Daily":
                    prefix = MakePeriodDaily(date);
                    condition += $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                default:
                    break;
            }
            var cId = companyGroupId == null ? "null" : (object)$"'{companyGroupId}'";
            var sql = "DECLARE @lastNumber AS BIGINT=0; " +
                   $"SELECT @lastNumber=MaxNumber FROM [ACS].[PKGenerator] {condition} " +
                   "IF @lastNumber > 0  " +
                   "BEGIN  " +
                       $"UPDATE [ACS].[PKGenerator] SET UpdatedDate=GETDATE(), MaxNumber=@lastNumber + 1 {condition} " +
                   "END " +
                   "ELSE    " +
                       $"INSERT INTO [ACS].[PKGenerator](FieldName, [Period], CompanyGroupId, MaxNumber, UpdatedDate) VALUES('{fieldName}', '{prefix}', {cId}, 1, GETDATE()); " +
                   "SELECT @lastNumber + 1 AS MaxNumber";


            var number = _sqlRepository.GetDataCollection(sql)[0]["MaxNumber"].ToString();
            return period == PKGeneratorEnum.Auto ? number : prefix + number;
        }
        public string MakePK(string masterId, int currentId, int padLeft)
        {
            return masterId + currentId.ToString().PadLeft(padLeft, '0');
        }
        public string MakePK(string masterId, int currentId)
        {
            return masterId + currentId.ToString();
        }
        public Voucher InsertVoucher(Voucher voucher, string fiscalYearPrefix, out DataSet dsData)
        {
            return InsertVoucher(voucher, fiscalYearPrefix, true, out dsData);
        }

        public Voucher InsertVoucher(Voucher voucher, string fiscalYearPrefix, bool flag, out DataSet dsData)
        {

            if (flag)
                CheckPostingDate(voucher);
            voucher.VoucherDate = DateTime.Now;
            voucher.VoucherNo = GetVoucherNo(voucher, fiscalYearPrefix);
            if (!string.IsNullOrEmpty(voucher.VoucherNo))
            {
                DataTable Qry = _sqlRepository.GetDataTable("select * from TRN.Voucher where VoucherNo='" + voucher.VoucherNo + "' AND PlantId='" + voucher.PlantId + "' AND Id<>''");
                if (Qry.Rows.Count > 0)
                    throw new Exception("Same voucher no. already exists!!!");

            }
            if (voucher.PostingDate != null)
            {
                DataTable QryFiscalYearClose = _sqlRepository.GetDataTable("select * from [SCS].[FiscalYearClose] where  VoucherId is not null AND CompanyId='" + voucher.CompanyId + "' AND PlantId='" + voucher.PlantId + "' AND FiscalYearId in(select Id from [SCS].[FiscalYear] where '" + voucher.PostingDate.Date + "' between StartDate and EndDate) ");
                if (QryFiscalYearClose.Rows.Count > 0)
                    throw new Exception("Fiscal Year already closed!!!");

            }
            voucher.Id = GetAutoNumber(nameof(Voucher), PKGeneratorEnum.Yearly, null, DateTime.Now);
            if (string.IsNullOrEmpty(voucher.VoucherNo))
            {
                var fiscalYear = GetfiscalYearfind(voucher.FiscalYearId);
                voucher.VoucherNo = GetVoucherNo(voucher, fiscalYear["YearPrefix"].ToString());
            }
            if (string.IsNullOrEmpty(voucher.TransactionRefNo))
                voucher.TransactionRefNo = voucher.PostingDate.Year.ToString().Substring(2) + voucher.Id;
            voucher.Narration = voucher.Narration?.ToUpper();
            if (string.IsNullOrEmpty(voucher.AddedBy))
                AuditService.AddedLog(voucher);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.Voucher where 1=2", out dsData);

            AddNewRow<Voucher>(dsData.Tables[0], voucher);

            return voucher;
        }


        private void AddNewRow<T>(DataTable dt, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));
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

            dt.Rows.Add(dr);
        }
        private void EditRow<T>(DataRow dr, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item.ToUpper() == "ID")
                        continue;

                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr.EndEdit();
        }
        private void EditRow_Dictionary(DataRow dr, Dictionary<string, object> sourceData)
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
        private void VoucherTypeNumberEditRow<T>(DataRow dr, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item.ToUpper() == "ID")
                        continue;

                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr.EndEdit();
        }

        public Voucher InsertVoucher(VoucherViewModel voucherVM)
        {
            return InsertVoucher(new Voucher
            {
                CompanyGroupId = voucherVM.CompanyGroupId,
                CompanyId = voucherVM.CompanyId,
                PlantId = voucherVM.PlantId,
                EntityId = voucherVM.EntityId,
                FiscalYearId = voucherVM.FiscalYearId,
                FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                TaxYearId = voucherVM.TaxYearId,
                TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                VoucherTypeId = voucherVM.VoucherTypeId,
                CurrencyId = voucherVM.CurrencyId,
                AddedBy = voucherVM.AddedBy,
                AddedDate = voucherVM.AddedDate,
                AddedFromIP = voucherVM.AddedFromIP,
                PostingDate = voucherVM.PostingDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                SourceType = voucherVM.SourceType,
                ExchangeType = voucherVM.ExchangeType,
                IsPark = voucherVM.IsPark,
                Archive = false
            }, voucherVM.FiscalYearPrefix, out DataSet _voucherdataset);
        }
        public VoucherDetail InsertVoucherDetail(Voucher voucher, VoucherDetail voucherDetail, int currentId, ref DataSet vDetailData)
        {
            voucherDetail.Id = MakePK(voucher.Id, currentId, 4);
            voucherDetail.VoucherId = voucher.Id;
            voucherDetail.EntityId = voucher.EntityId;
            voucherDetail.FiscalYearId = voucher.FiscalYearId;
            voucherDetail.FiscalYearPeriodId = voucher.FiscalYearPeriodId;
            voucherDetail.CurrencyId = voucher.CurrencyId;
            voucherDetail.Archive = voucher.Archive;
            voucherDetail.IsPark = voucher.IsPark;
            voucherDetail.AddedBy = voucher.AddedBy;
            voucherDetail.AddedDate = voucher.AddedDate;
            voucherDetail.AddedFromIP = voucher.AddedFromIP;
            voucherDetail.DocDate = voucher.DocDate;
            voucherDetail.DocRefNo = voucher.DocRefNo;
            voucherDetail.Narration = string.IsNullOrEmpty(voucherDetail.Narration) ? voucher.Narration : voucherDetail.Narration;

            if (vDetailData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.VoucherDetail where 1=2", out vDetailData);
            }


            AddNewRow<VoucherDetail>(vDetailData.Tables[0], voucherDetail);
            return voucherDetail;
        }

        public VoucherDetailCurrency InsertVoucherDetailCompanyCurrency(VoucherDetail voucherDetail, VoucherDetailCurrency voucherDetailCurrency, ref DataSet vDetailCurrencyData)
        {
            voucherDetailCurrency.Id = MakePK(voucherDetail.Id, 1, 1);
            voucherDetailCurrency.VoucherId = voucherDetail.VoucherId;
            voucherDetailCurrency.VoucherDetailId = voucherDetail.Id;
            voucherDetailCurrency.AddedBy = voucherDetail.AddedBy;
            voucherDetailCurrency.AddedDate = voucherDetail.AddedDate;
            voucherDetailCurrency.AddedFromIP = voucherDetail.AddedFromIP;
            if (vDetailCurrencyData == null || vDetailCurrencyData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.VoucherDetailCurrency where 1=2", out vDetailCurrencyData);
            }

            AddNewRow<VoucherDetailCurrency>(vDetailCurrencyData.Tables[0], voucherDetailCurrency);
            return voucherDetailCurrency;
        }

        public void InsertGLTransactionDetail(VoucherDetail voucherDetail, GLTransactionDetail glTransactionDetail, out DataSet glTransactionData)
        {
            glTransactionDetail.Id = voucherDetail.Id;
            glTransactionDetail.VoucherDetailId = voucherDetail.Id;
            glTransactionDetail.AddedBy = voucherDetail.AddedBy;
            glTransactionDetail.AddedDate = voucherDetail.AddedDate;
            glTransactionDetail.AddedFromIP = voucherDetail.AddedFromIP;
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.GLTransactionDetail where 1=2", out glTransactionData);
            AddNewRow<GLTransactionDetail>(glTransactionData.Tables[0], glTransactionDetail);
        }

        public decimal GetCompanyCurrencyExchange(string transactionCurrencyId, string companyCurrencyId, decimal companyCurrencyRate)
        {
            return transactionCurrencyId == companyCurrencyId ? (decimal)1 : 1 / companyCurrencyRate;
        }
        public void InsertBankJournal(BankJournal bankJournal, ref DataSet dsData)
        {
            bankJournal.Id = GetAutoNumber(nameof(BankJournal), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(bankJournal);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[BankJournal] where 1=2", out dsData);
            }
            AddNewRow<BankJournal>(dsData.Tables[0], bankJournal);
        }
        public void InsertBankJournalDetail(BankJournal bankJournal, BankJournalDetail bankJournalDetail, int currentId, ref DataSet dsData)
        {
            bankJournalDetail.Id = MakePK(bankJournal.Id, currentId, 1);
            bankJournalDetail.BankJournalId = bankJournal.Id;
            bankJournalDetail.AddedBy = bankJournal.AddedBy;
            bankJournalDetail.AddedDate = bankJournal.AddedDate;
            bankJournalDetail.AddedFromIP = bankJournal.AddedFromIP;
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[BankJournalDetail] where 1=2", out dsData);
            }
            AddNewRow<BankJournalDetail>(dsData.Tables[0], bankJournalDetail);
        }
       
        public Dictionary<string, object> GetBankMaster(string bankMasterId)
        {

            var sql = @"SELECT TOP(1) * FROM [MST].[BankMaster]  
                        WHERE Id='" + bankMasterId + "'";
            var bankTemp = _sqlRepository.GetData(sql);
            if (null == bankTemp || bankTemp.Count == 0)
                throw new CustomException("Bank Master  not Found!");

            return bankTemp;
        }
        public Dictionary<string, object> GetCashMaster(string cashMasterId)
        {

            var sql = @"SELECT TOP(1) * FROM [MST].[CashMaster]  
                        WHERE Id='" + cashMasterId + "'";
            var cashTemp = _sqlRepository.GetData(sql);
            if (null == cashTemp || cashTemp.Count == 0)
                throw new CustomException("Cash Master  not Found!");

            return cashTemp;
        }
        #region Invoice
        public Invoice InsertInvoice(Invoice invoice, out DataSet dsData)
        {
            return InsertInvoice(invoice, true, out dsData);
        }
        public Invoice InsertInvoice(Invoice invoice, bool flag, out DataSet dsData)
        {
            invoice.Id = GetAutoNumber(nameof(Invoice), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoice.Narration = invoice.Narration?.ToUpper();
            invoice.AdditionalAmount = 0;
            if (string.IsNullOrEmpty(invoice.AddedBy))
                AuditService.AddedLog(invoice);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.Invoice where 1=2", out dsData);

            AddNewRow<Invoice>(dsData.Tables[0], invoice);

            return invoice;
        }

        public void UpdateInvoice(Dictionary<string, object>  invoice, ref DataSet _invoice)
        {
            if (_invoice == null || _invoice.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.Invoice where Id='" + invoice["Id"].ToString() + "'", out _invoice);

            }
            EditRow_Dictionary(_invoice.Tables[0].Rows[0], invoice);
        }
        public void UpdateInvoiceDetails(InvoiceDetail invoiceDetail, ref DataSet _invoiceDetail)
        {
            if (_invoiceDetail == null || _invoiceDetail.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceDetail where InvoiceId='" + invoiceDetail.InvoiceId + "'", out _invoiceDetail);
            }
            EditRow<InvoiceDetail>(_invoiceDetail.Tables[0].Rows[0], invoiceDetail);
        }
        public void UpdateInvoiceDetail(Dictionary<string, object> invoiceDetail, ref DataSet _invoiceDetail)
        {
            if (_invoiceDetail == null || _invoiceDetail.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceDetail where InvoiceId='" + invoiceDetail["InvoiceId"].ToString()  + "'", out _invoiceDetail);
            }
            EditRow_Dictionary(_invoiceDetail.Tables[0].Rows[0], invoiceDetail);
        }


        public Advance InsertAdvance(Advance advance, out DataSet dsData)
        {
            if (advance.PaymentSource == PaymentSource.Bank.ToString())
                if (string.IsNullOrEmpty(advance.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                else
                    advance.CashMasterId = null;
            else if (advance.PaymentSource == PaymentSource.Cash.ToString())
                if (string.IsNullOrEmpty(advance.CashMasterId))
                    throw new CustomException("Cash Id not found!");
                else
                    advance.BankMasterId = null;
            return InsertAdvance(advance, true, out dsData);
        }
        public Advance InsertAdvance(Advance advance, bool flag, out DataSet dsData)
        {
            advance.Id = GetAutoNumber(nameof(Advance), PKGeneratorEnum.Yearly, null, DateTime.Now);
            advance.Narration = advance.Narration?.ToUpper();
            if (string.IsNullOrEmpty(advance.AddedBy))
                AuditService.AddedLog(advance);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.Advance where 1=2", out dsData);

            AddNewRow<Advance>(dsData.Tables[0], advance);
            return advance;
        }

        public AdvanceDetail InsertAdvanceDetail(AdvanceDetail advanceDetail, int currentId, out DataSet dsData)
        {
            return InsertInvoiceWriteOffDetail(advanceDetail, currentId, true, out dsData);
        }
        public AdvanceDetail InsertInvoiceWriteOffDetail(AdvanceDetail advanceDetail, int currentId, bool flag, out DataSet dsData)
        {
            advanceDetail.Id = MakePK(advanceDetail.AdvanceId, currentId);
            advanceDetail.Narration = advanceDetail.Narration?.ToUpper();
            if (string.IsNullOrEmpty(advanceDetail.AddedBy))
                AuditService.AddedLog(advanceDetail);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.AdvanceDetail where 1=2", out dsData);

            AddNewRow<AdvanceDetail>(dsData.Tables[0], advanceDetail);
            return advanceDetail;
        }

        public InvoiceWriteOff InsertInvoiceWriteOff(InvoiceWriteOff invoiceWriteOff, out DataSet dsData)
        {
            if (invoiceWriteOff.PaymentSource == PaymentSource.Bank.ToString())
                if (string.IsNullOrEmpty(invoiceWriteOff.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                else
                    invoiceWriteOff.CashMasterId = null;
            else if (invoiceWriteOff.PaymentSource == PaymentSource.Cash.ToString())
                if (string.IsNullOrEmpty(invoiceWriteOff.CashMasterId))
                    throw new CustomException("Cash Id not found!");
                else
                    invoiceWriteOff.BankMasterId = null;

           
            //if (voucherVM.SourceType != "CustomerBanksReceipt")
            //{
            //    Check(invoiceWriteOff);
            //}
            return InsertInvoiceWriteOff(invoiceWriteOff,true, out dsData);
        }
        public InvoiceWriteOff InsertInvoiceWriteOff(InvoiceWriteOff invoiceWriteOff,bool flag  ,out DataSet dsData)
        {
            invoiceWriteOff.Id = GetAutoNumber(nameof(InvoiceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoiceWriteOff.Narration = invoiceWriteOff.Narration?.ToUpper();
            if (string.IsNullOrEmpty(invoiceWriteOff.AddedBy))
                AuditService.AddedLog(invoiceWriteOff);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.InvoiceWriteOff where 1=2", out dsData);

            AddNewRow<InvoiceWriteOff>(dsData.Tables[0], invoiceWriteOff);
            return invoiceWriteOff;
        }

        public InvoiceWriteOffDetail InsertInvoiceWriteOffDetail(InvoiceWriteOffDetail invoiceWriteOffDetail,int currentId, out DataSet dsData)
        {
            return InsertInvoiceWriteOffDetail(invoiceWriteOffDetail, currentId, true, out dsData);
        }
        public InvoiceWriteOffDetail InsertInvoiceWriteOffDetail(InvoiceWriteOffDetail invoiceWriteOffDetail, int currentId, bool flag, out DataSet dsData)
        {
            invoiceWriteOffDetail.Id = MakePK(invoiceWriteOffDetail.InvoiceWriteOffId, currentId, 2);
            invoiceWriteOffDetail.Narration = invoiceWriteOffDetail.Narration?.ToUpper();
            if (string.IsNullOrEmpty(invoiceWriteOffDetail.AddedBy))
                AuditService.AddedLog(invoiceWriteOffDetail);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.InvoiceWriteOffDetail where 1=2", out dsData);

            AddNewRow<InvoiceWriteOffDetail>(dsData.Tables[0], invoiceWriteOffDetail);
            return invoiceWriteOffDetail;
        }
        //private void CheckInvoiceWriteOff(InvoiceWriteOff entity)
        //{
        //    CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        //}
        public InvoiceDetail InsertInvoiceDetail(Invoice invoice, InvoiceDetail invoiceDetail, int currentId, ref DataSet vDetailData)
        {
            invoiceDetail.Id = "IND" + MakePK(invoice.Id, currentId, 1);
            invoiceDetail.InvoiceId = invoice.Id;
            invoiceDetail.AdditionalAmount = 0;
            invoiceDetail.Archive = invoice.Archive;
            invoiceDetail.AddedBy = invoice.AddedBy;
            invoiceDetail.AddedDate = invoice.AddedDate;
            invoiceDetail.AddedFromIP = invoice.AddedFromIP;
            if (vDetailData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceDetail where 1=2", out vDetailData);
            }

            AddNewRow<InvoiceDetail>(vDetailData.Tables[0], invoiceDetail);
            return invoiceDetail;
        }
        public Dictionary<string, object> GetInvoiceDetail(string invoiceDetailId)
        {
            var sql = @"SELECT TOP(1) * FROM TRN.InvoiceDetail  WHERE  Id='" + invoiceDetailId + "'";
            var gainTemp = _sqlRepository.GetData(sql);

            if (null == sql || gainTemp.Count == 0)
                throw new CustomException("Invoice Detail not found!.");
            return gainTemp;
        }
        public Dictionary<string, object> GetInvoice(string invoiceId)
        {
            var sql = @"SELECT TOP(1) * FROM TRN.Invoice  WHERE  Id='" + invoiceId + "'";
            var gainTemp = _sqlRepository.GetData(sql);

            if (null == sql || gainTemp.Count == 0)
                throw new CustomException("Invoice  not found!.");
            return gainTemp;
        }

        public Dictionary<string, object> GetRoundingGL(string companyId)
        {
            var sql = @"SELECT TOP(1) FTGL.* FROM [HKP].[FinancingTypeGL] AS FTGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=FTGL.COAId
                        LEFT JOIN [HKP].[FinancingType] AS FT ON FT.Id=FTGL.FinancingTypeId
                        WHERE C.Id='" + companyId + "' AND FT.SourceType='" + FinancingTypeEnum.Rounding + "'";
            var gainTemp = _sqlRepository.GetData(sql);

            if (null == gainTemp)
                throw new CustomException("This Transaction Type GL not Found!");
          
            return gainTemp;
        }

        public Dictionary<string, object> GetExchangeGainGL(FinancingTypeEnum sourceType)
        {
            var st = sourceType.ToString();
            var sql = @"SELECT TOP(1) * FROM SCS.ExchangeGainLossGL 
                        WHERE  SourceType='" + st + "' and ExchangeStatus='ExchangeGain'";
            var gainTemp = _sqlRepository.GetData(sql);

            if (null == sql || gainTemp.Count == 0)
                throw new CustomException("Exchange Gain GL not found!.");
            return gainTemp;
        }

        public Dictionary<string, object> GetExchangeLossGL(FinancingTypeEnum sourceType)
        {
            var st = sourceType.ToString();
            var sql = @"SELECT TOP(1) * FROM SCS.ExchangeGainLossGL 
                        WHERE  SourceType='" + st + "' and ExchangeStatus='ExchangeLoss'";
            var gainTemp = _sqlRepository.GetData(sql);

            if (null == sql || gainTemp.Count == 0)
                throw new CustomException("Exchange Loss GL not found!.");
            return gainTemp;
        }

        public void InsertAdvanceReqSchedule(AdvanceReqSchedule financingSchedule, string requisitionId, ref DataSet dsData)
        {
            financingSchedule.Id = MakePK(requisitionId, financingSchedule.InstallmentNo, 3);
            financingSchedule.RequisitionId = requisitionId;

            AuditService.AddedLog(financingSchedule);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [dbo].[AdvanceReqSchedule] where 1=2", out dsData);
            }
            AddNewRow<AdvanceReqSchedule>(dsData.Tables[0], financingSchedule);
        }

        public IEnumerable<object> GetcustomerInvoiceList(string companyGroupId, string companyId, string plantId, string customerSelectedList, string fromDate, string toDate, string paymentStatus)
        {
            var status = "";
            if (paymentStatus == "Pending")
            {
                status = "AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0";
            }
            var sql = @"SELECT Id,VoucherNo,PostingDate, DocRefNo,DocRefNo,BuyerRefNo,CustomerRemarks,ExpectedPaymentReceivedDate
                    ,ISNULL( X.PartyId,'')PartyId,ISNULL( X.PartyPlantId,'')PartyPlantId,ISNULL( X.PartyCode,'')PartyCode
                    ,ISNULL( X.PartyName,'')PartyName,ISNULL( X.PartyPlantName,'')PartyPlantName,ISNULL( x.CurrencyCode,'')CurrencyCode
				 ,ISNULL(X.GrossSales,0 )GrossSales 
				,ISNULL(X.Receipts,0 )Receipts
				,ISNULL(X.Balance,0) Balance
                ,ISNULL( X.BooksGrossSales ,0)BooksGrossSales
				,ISNULL( X.BooksReceipts ,0)BooksReceipts
				,X.BooksBalance
				
                FROM (
                SELECT IV.Id,V.VoucherNo,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo
                ,IV.[BuyerRefNo] ,IV.[CustomerRemarks],Replace(CONVERT(VARCHAR(11), IV.ExpectedPaymentReceivedDate, 106), ' ', '-') ExpectedPaymentReceivedDate
                ,ISNULL( IV.PartyId,'') NoOfInvoice,ISNULL( IV.PartyId,'')PartyId--,cc.CompanyCurrencyRate
				, ISNULL( IV.PartyPlantId,'')PartyPlantId,ISNULL( P.Code,'') PartyCode
				,ISNULL( P.UserName,'') PartyName,ISNULL( PP.UserName,'') AS PartyPlantName ,ISNULL( c.Code,'') CurrencyCode

                ,ISNULL(IVD.Amount,0) AS GrossSales
				,ISNULL(IVD.WrittenOffAmount ,0) AS Receipts
				,ISNULL(IVD.Amount-IVD.WrittenOffAmount,0) AS Balance

                ,ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS BooksGrossSales
				,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0) AS BooksReceipts
				,ISNULL((IVD.Amount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0) AS BooksBalance
				, ISNULL(IVD.Amount,0) AS GrossTranAmount
				, ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS GrossAmount
                FROM [TRN].[InvoiceDetail] AS IVD
                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                ) AS CC ON CC.VoucherDetailId=VD.Id
                
                WHERE IV.Archive=0  AND V.Archive=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice')
                and IV.PartyId in 	(" + customerSelectedList + @")  AND IV.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'  and  IV.CompanyGroupId='" + companyGroupId + "'   AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"' " + status + @"
                UNION ALL
                SELECT IV.Id,V.VoucherNo,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo
                ,IV.[BuyerRefNo] ,IV.[CustomerRemarks],Replace(CONVERT(VARCHAR(11), IV.ExpectedPaymentReceivedDate, 106), ' ', '-') ExpectedPaymentReceivedDate
                ,ISNULL( IV.PartyId,'') NoOfInvoice,ISNULL( IV.PartyId,'')PartyId
				, ISNULL( IV.PartyPlantId,'')PartyPlantId,ISNULL( P.Code,'') PartyCode
				,ISNULL( P.UserName,'') PartyName,ISNULL( PP.UserName,'') AS PartyPlantName ,ISNULL( c.Code,'') CurrencyCode
			      ,ISNULL(IVD.Amount,0) AS GrossSales
				,ISNULL(IVD.WrittenOffAmount ,0) AS Receipts
				 , ISNULL(IVD.Amount-IVD.WrittenOffAmount,0) AS Balance

                 ,ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS BooksGrossSales
				,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0) AS BooksReceipts
				, ISNULL((IVD.Amount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0) AS BooksBalance
				, ISNULL(IVD.Amount,0) AS GrossTranAmount
				, ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS GrossAmount
                FROM [TRN].[InvoiceDetail] AS IVD
                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId

							LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id

								
						LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) DiscountAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Discount'
								group by wd.InvoiceDetailId
								) DIWD ON DIWD.InvoiceDetailId=IVD.Id

               -- LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IV.InventoryReceiveId
                   LEFT JOIN TRN.InventorySales IVS ON IVS.Id=IV.InventorySalesId

				
                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                ) AS CC ON CC.VoucherDetailId=VD.Id
                
                WHERE IV.Archive=0  AND V.Archive=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('InventorySales')
                  and IV.PartyId in 	(" + customerSelectedList + @") AND IV.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' and  IV.CompanyGroupId='" + companyGroupId + "'   AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"' " + status + @"
               -- AND IR.PurchaseDocumentAcceptanceId IS NULL
                
                union all


				 SELECT IV.Id,V.VoucherNo,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo
                ,'' [BuyerRefNo] ,'' [CustomerRemarks],''  ExpectedPaymentReceivedDate
                ,ISNULL( IV.PartyId,'') NoOfInvoice,ISNULL( IV.PartyId,'')PartyId
				, ISNULL( IV.PartyPlantId,'')PartyPlantId,ISNULL( P.Code,'') PartyCode
				,ISNULL( P.UserName,'') PartyName,ISNULL( PP.UserName,'') AS PartyPlantName ,ISNULL( c.Code,'') CurrencyCode
                   ,ISNULL(IVD.Amount,0) AS GrossSales
				,ISNULL(IVD.WrittenOffAmount ,0) AS Receipts
				, ISNULL(IVD.Amount-IVD.WrittenOffAmount,0) AS Balance

                 ,ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS BooksGrossSales
				,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0) AS BooksReceipts
				, ISNULL((IVD.Amount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0) AS BooksBalance
				, ISNULL(IVD.Amount,0) AS GrossTranAmount
				, ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS GrossAmount

                FROM [TRN].[AdjustmentNoteDetail] AS IVD
                LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                ) AS CC ON CC.VoucherDetailId=VD.Id
                
                WHERE IV.Archive=0 AND V.Archive=0 AND V.IsPark=0  AND IV.SourceType in ('CustomerReceipt')
                 and IV.PartyId in 	(" + customerSelectedList + @") AND IV.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' and  IV.CompanyGroupId='" + companyGroupId + "'   AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"' " + status + @"
                
				)
                X
				--where x.PartyCode='2020100'
                ";
            return _sqlRepository.GetDataCollection(sql);

        }

        #endregion

        #region AdjustmentNote
        public AdjustmentNote InsertAdjustmentNote(AdjustmentNote adjustmentNote, out DataSet dsData)
        {
            return InsertAdjustmentNote(adjustmentNote, true, out dsData);
        }

        public AdjustmentNote InsertAdjustmentNote(AdjustmentNote adjustmentNote, bool flag, out DataSet dsData)
        {
            adjustmentNote.Id = GetAutoNumber(nameof(AdjustmentNote), PKGeneratorEnum.Yearly, null, DateTime.Now);
            adjustmentNote.Narration = adjustmentNote.Narration?.ToUpper();
            if (string.IsNullOrEmpty(adjustmentNote.AddedBy))
                AuditService.AddedLog(adjustmentNote);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.AdjustmentNote where 1=2", out dsData);

            AddNewRow<AdjustmentNote>(dsData.Tables[0], adjustmentNote);

            return adjustmentNote;
        }


        private AdjustmentNoteDetail InsertAdjustmentNoteDetail(AdjustmentNote adjustmentNote, AdjustmentNoteDetail adjustmentNoteDetail, int currentId)
        {
            adjustmentNoteDetail.Id = MakePK(adjustmentNote.Id, currentId, 1);
            adjustmentNoteDetail.AdjustmentNoteId = adjustmentNote.Id;
            adjustmentNoteDetail.InvoiceId = adjustmentNote.InvoiceId;
            adjustmentNoteDetail.AddedBy = adjustmentNote.AddedBy;
            adjustmentNoteDetail.AddedDate = adjustmentNote.AddedDate;
            adjustmentNoteDetail.AddedFromIP = adjustmentNote.AddedFromIP;
            adjustmentNoteDetail.Archive = adjustmentNote.Archive;
            //_AdjustmentNoteDetailRepository.Insert(adjustmentNoteDetail);
            return adjustmentNoteDetail;
        }

        public AdjustmentNoteDetail InsertAdjustmentNoteDetail(AdjustmentNote adjustmentNote, AdjustmentNoteDetail adjustmentNoteDetail, int currentId, ref DataSet ajNDetailData)
        {
            adjustmentNoteDetail.Id = MakePK(adjustmentNote.Id, currentId, 1);
            adjustmentNoteDetail.AdjustmentNoteId = adjustmentNote.Id;
            adjustmentNoteDetail.Archive = adjustmentNote.Archive;
            adjustmentNoteDetail.AddedBy = adjustmentNote.AddedBy;
            adjustmentNoteDetail.AddedDate = adjustmentNote.AddedDate;
            adjustmentNoteDetail.AddedFromIP = adjustmentNote.AddedFromIP;
            if (ajNDetailData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.AdjustmentNoteDetail where 1=2", out ajNDetailData);
            }

            AddNewRow<AdjustmentNoteDetail>(ajNDetailData.Tables[0], adjustmentNoteDetail);
            return adjustmentNoteDetail;
        }


        public InvoiceTax InsertInvoiceTax(InvoiceTax invoiceTax, ref DataSet dsData)
        {
            return InsertInvoiceTax(invoiceTax, true, ref dsData);
        }

        public InvoiceTax InsertInvoiceTax(InvoiceTax invoiceTax, bool flag, ref DataSet dsData)
        {
            invoiceTax.Id = GetAutoNumber(nameof(InvoiceTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            if (string.IsNullOrEmpty(invoiceTax.AddedBy))
                AuditService.AddedLog(invoiceTax);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.InvoiceTax where 1=2", out dsData);

            AddNewRow<InvoiceTax>(dsData.Tables[0], invoiceTax);

            return invoiceTax;
        }



        public InvoiceTax InsertInvoiceTax(Invoice invoice, InvoiceTax invoiceTax, ref DataSet dsData)
        {
            invoiceTax.Id = GetAutoNumber(nameof(InvoiceTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoiceTax.InvoiceId = invoice.Id;
            invoiceTax.TaxYearId = invoice.TaxYearId;
            invoiceTax.TaxYearPeriodId = invoice.TaxYearPeriodId;
            invoiceTax.VoucherId = invoice.VoucherId;
            invoiceTax.PartyId = invoice.PartyId;
            invoiceTax.PartyPlantId = invoice.PartyPlantId;
            invoiceTax.SourceType = invoice.SourceType;
            invoiceTax.Archive = invoice.Archive;
            invoiceTax.AddedBy = invoice.AddedBy;
            invoiceTax.AddedDate = invoice.AddedDate;
            invoiceTax.AddedFromIP = invoice.AddedFromIP;
            if (dsData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceTax where 1=2", out dsData);
            }
            AddNewRow<InvoiceTax>(dsData.Tables[0], invoiceTax);
            return invoiceTax;
        }
        public InvoiceTax InsertInvoiceTax(EmployeePayable employeePayable, InvoiceTax invoiceTax, ref DataSet dsData)
        {
            invoiceTax.Id = GetAutoNumber(nameof(InvoiceTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoiceTax.EmployeePayableId = employeePayable.Id;
            invoiceTax.TaxYearId = employeePayable.TaxYearId;
            invoiceTax.TaxYearPeriodId = employeePayable.TaxYearPeriodId;
            invoiceTax.VoucherId = employeePayable.VoucherId;
            invoiceTax.PartyId = employeePayable.PartyId;
            invoiceTax.PartyPlantId = employeePayable.PartyPlantId;
            invoiceTax.SourceType = employeePayable.SourceType;
            invoiceTax.Archive = employeePayable.Archive;
            invoiceTax.AddedBy = employeePayable.AddedBy;
            invoiceTax.AddedDate = employeePayable.AddedDate;
            invoiceTax.AddedFromIP = employeePayable.AddedFromIP;
            if (dsData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceTax where 1=2", out dsData);
            }
            AddNewRow<InvoiceTax>(dsData.Tables[0], invoiceTax);
            return invoiceTax;
        }
        public InvoiceTax InsertInvoiceTax(InvoiceWriteOff invoicewriteoff, InvoiceTax invoiceTax, ref DataSet dsData)
        {
            invoiceTax.Id = GetAutoNumber(nameof(InvoiceTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoiceTax.InvoiceWriteOffId = invoicewriteoff.Id;
            invoiceTax.TaxYearId = invoicewriteoff.TaxYearId;
            invoiceTax.TaxYearPeriodId = invoicewriteoff.TaxYearPeriodId;
            invoiceTax.VoucherId = invoicewriteoff.VoucherId;
            invoiceTax.PartyId = invoicewriteoff.PartyId;
            invoiceTax.PartyPlantId = invoicewriteoff.PartyPlantId;
            invoiceTax.SourceType = invoicewriteoff.SourceType;
            invoiceTax.Archive = invoicewriteoff.Archive;
            invoiceTax.AddedBy = invoicewriteoff.AddedBy;
            invoiceTax.AddedDate = invoicewriteoff.AddedDate;
            invoiceTax.AddedFromIP = invoicewriteoff.AddedFromIP;
            if (dsData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceTax where 1=2", out dsData);
            }
            AddNewRow<InvoiceTax>(dsData.Tables[0], invoiceTax);
            return invoiceTax;
        }

        public InvoiceTax InsertInvoiceTax(Advance advance, InvoiceTax invoiceTax, ref DataSet dsData)
        {
            invoiceTax.Id = GetAutoNumber(nameof(InvoiceTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoiceTax.AdvanceId = advance.Id;
            invoiceTax.TaxYearId = advance.TaxYearId;
            invoiceTax.TaxYearPeriodId = advance.TaxYearPeriodId;
            invoiceTax.VoucherId = advance.VoucherId;
            invoiceTax.PartyId = advance.PartyId;
            invoiceTax.PartyPlantId = advance.PartyPlantId;
            invoiceTax.SourceType = advance.SourceType;
            invoiceTax.Archive = advance.Archive;
            invoiceTax.AddedBy = advance.AddedBy;
            invoiceTax.AddedDate = advance.AddedDate;
            invoiceTax.AddedFromIP = advance.AddedFromIP;
            if (dsData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceTax where 1=2", out dsData);
            }
            AddNewRow<InvoiceTax>(dsData.Tables[0], invoiceTax);
            return invoiceTax;
        }

        public InvoiceTax InsertInvoiceTax(AdjustmentNote adjustmentNote, InvoiceTax invoiceTax, ref DataSet dsData)
        {
            invoiceTax.Id = GetAutoNumber(nameof(InvoiceTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoiceTax.InvoiceId = adjustmentNote.InvoiceId;
            invoiceTax.TaxYearId = adjustmentNote.TaxYearId;
            invoiceTax.TaxYearPeriodId = adjustmentNote.TaxYearPeriodId;
            invoiceTax.VoucherId = adjustmentNote.VoucherId;
            invoiceTax.PartyId = adjustmentNote.PartyId;
            invoiceTax.PartyPlantId = adjustmentNote.PartyPlantId;
            invoiceTax.SourceType = adjustmentNote.SourceType;
            invoiceTax.Archive = adjustmentNote.Archive;
            invoiceTax.AddedBy = adjustmentNote.AddedBy;
            invoiceTax.AddedDate = adjustmentNote.AddedDate;
            invoiceTax.AddedFromIP = adjustmentNote.AddedFromIP;
            if (dsData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceTax where 1=2", out dsData);
            }
            AddNewRow<InvoiceTax>(dsData.Tables[0], invoiceTax);
            return invoiceTax;
        }

        public InvoiceTax InsertInvoiceTax(VoucherViewModel voucherVM, InvoiceTax invoiceTax, ref DataSet dsData)
        {
            invoiceTax.Id = GetAutoNumber(nameof(InvoiceTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            invoiceTax.TaxYearId = voucherVM.TaxYearId;
            invoiceTax.TaxYearPeriodId = voucherVM.TaxYearPeriodId;
            invoiceTax.VoucherId = voucherVM.VoucherId;
            invoiceTax.PartyId = voucherVM.PartyId;
            invoiceTax.PartyPlantId = voucherVM.PartyPlantId;
            invoiceTax.SourceType = voucherVM.SourceType;
            invoiceTax.Archive = false;
            invoiceTax.AddedBy = voucherVM.AddedBy;
            invoiceTax.AddedDate = voucherVM.AddedDate;
            invoiceTax.AddedFromIP = voucherVM.AddedFromIP;

            if (dsData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceTax where 1=2", out dsData);
            }
            AddNewRow<InvoiceTax>(dsData.Tables[0], invoiceTax);
            return invoiceTax;
        }

        public InvoiceTaxDetail InsertInvoiceTaxDetail(InvoiceTax invoiceTax, InvoiceTaxDetail invoiceTaxDetail, ref DataSet ivTaxDetailData)
        {
            invoiceTaxDetail.Archive = invoiceTax.Archive;
            invoiceTaxDetail.AddedBy = invoiceTax.AddedBy;
            invoiceTaxDetail.AddedDate = invoiceTax.AddedDate;
            invoiceTaxDetail.AddedFromIP = invoiceTax.AddedFromIP;
            if (ivTaxDetailData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.InvoiceTaxDetail where 1=2", out ivTaxDetailData);
            }

            AddNewRow<InvoiceTaxDetail>(ivTaxDetailData.Tables[0], invoiceTaxDetail);
            return invoiceTaxDetail;
        }


        public AdditionalTax InsertAddtionalTax(AdjustmentNote adjustmentNote, AdditionalTax additionalTax, ref DataSet dsData)
        {
            additionalTax.Id = GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
            additionalTax.TaxYearId = adjustmentNote.TaxYearId;
            additionalTax.TaxYearPeriodId = adjustmentNote.TaxYearPeriodId;
            //additionalTax.VoucherId = adjustmentNote.VoucherId;
            additionalTax.PartyId = adjustmentNote.PartyId;
            additionalTax.PartyPlantId = adjustmentNote.PartyPlantId;
            additionalTax.SourceType = adjustmentNote.SourceType;
            additionalTax.Archive = false;
            additionalTax.AddedBy = adjustmentNote.AddedBy;
            additionalTax.AddedDate = adjustmentNote.AddedDate;
            additionalTax.AddedFromIP = adjustmentNote.AddedFromIP;

            if (dsData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.AdditionalTax where 1=2", out dsData);
            }
            AddNewRow<AdditionalTax>(dsData.Tables[0], additionalTax);
            return additionalTax;
        }

        public AdditionalTaxDetail InsertAddtionalTaxDetail(AdditionalTax invoiceTax, AdditionalTaxDetail invoiceTaxDetail, ref DataSet ivTaxDetailData)
        {
            invoiceTaxDetail.Archive = invoiceTax.Archive;
            invoiceTaxDetail.AddedBy = invoiceTax.AddedBy;
            invoiceTaxDetail.AddedDate = invoiceTax.AddedDate;
            invoiceTaxDetail.AddedFromIP = invoiceTax.AddedFromIP;
            if (ivTaxDetailData == null)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.AdditionalTaxDetail where 1=2", out ivTaxDetailData);
            }

            AddNewRow<AdditionalTaxDetail>(ivTaxDetailData.Tables[0], invoiceTaxDetail);
            return invoiceTaxDetail;
        }

        #endregion
        #region VoucherGLUpdate
        public void UpdateVoucherGl(IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet _VoucherGLUpdateLogData = null;
                DataSet _drvDetailData = null;
                DataSet _drvDetailDataset = null;
                var OldGLGeneralInfoId = "";
                var OldBudgetMasterId = "";
                var OldActivityId = "";
                con.OpenDataSetThroughAdapter(@"SELECT * FROM [TRN].[VoucherDetail] WHERE VoucherId='" + voucherDetailVMList.FirstOrDefault().VoucherId + "' and DrAmount > 0", out _drvDetailDataset, false, "1");
                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.DrAmount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not update Voucher GL!");

                        con.OpenDataSetThroughAdapter(@"SELECT * FROM [TRN].[VoucherDetail] WHERE Id='" + voucherDetailVM.Id + "'", out _drvDetailData, false, "1");
                        OldGLGeneralInfoId = _drvDetailData.Tables[0].DefaultView[0].Row["GLGeneralInfoId"].ToString();
                        OldBudgetMasterId = _drvDetailData.Tables[0].DefaultView[0].Row["BudgetMasterId"].ToString();
                        OldActivityId = _drvDetailData.Tables[0].DefaultView[0].Row["ActivityId"].ToString();

                        DataView dv = new DataView(_drvDetailDataset.Tables[0]);
                        dv.RowFilter = "Id='" + voucherDetailVM.Id + "'";
                        if (dv.Count > 0)
                        {
                            DataRow dr = dv[0].Row;

                            dr.BeginEdit();

                            dr["GLGeneralInfoId"] = voucherDetailVM.GLGeneralInfoId;
                            dr["BudgetMasterId"] = voucherDetailVM.BudgetMasterId;
                            dr["ActivityId"] = voucherDetailVM.ActivityId;

                            dr.EndEdit();

                        }

                        var voucherGLUpdateLog = new VoucherGLUpdateLog
                        {
                            VoucherId = voucherDetailVM.VoucherId,
                            VoucherDetailId = voucherDetailVM.Id,
                            OldGLGeneralInfoId = OldGLGeneralInfoId,
                            OldBudgetMasterId = OldBudgetMasterId,
                            OldActivityId = OldActivityId,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,

                        };

                        InsertVoucherGLUpdateLogData(voucherGLUpdateLog, ref _VoucherGLUpdateLogData);

                        if (voucherDetailVM.SourceType == "IssueJournal")
                        {
                            var rdBuilder = new System.Text.StringBuilder();
                            var builderSql = @"UPDATE ID SET ID.PostDrGLGeneralInfoId='" + voucherDetailVM.GLGeneralInfoId + "' , ID.PostDrBudgetMasterId='" + voucherDetailVM.BudgetMasterId + "' , ID.PostDrActivityId='" + voucherDetailVM.ActivityId + "'  FROM TRN.InventoryIssueDetail  ID INNER JOIN TRN.InventoryIssue I ON I.Id=ID.InventoryIssueId WHERE I.VoucherId='" + voucherDetailVM.VoucherId + "' AND ID.PostDrGLGeneralInfoId='" + OldGLGeneralInfoId + "' AND ID.PostDrBudgetMasterId='" + OldBudgetMasterId + "' AND ID.PostDrActivityId='" + OldActivityId + "'";
                            rdBuilder.Append(builderSql);
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        }
                        if (voucherDetailVM.SourceType == "InventoryPayable")
                        {
                            var rdBuilder = new System.Text.StringBuilder();
                            var builderSql = @"UPDATE ID SET ID.PostDrGLGeneralInfoId='" + voucherDetailVM.GLGeneralInfoId + "' , ID.PostDrBudgetMasterId='" + voucherDetailVM.BudgetMasterId + "' , ID.PostDrActivityId='" + voucherDetailVM.ActivityId + "'  FROM TRN.InventoryReceiveDetail  ID INNER JOIN TRN.InventoryReceive I ON I.Id=ID.InventoryReceiveId WHERE I.VoucherId='" + voucherDetailVM.VoucherId + "' AND ID.PostDrGLGeneralInfoId='" + OldGLGeneralInfoId + "' AND ID.PostDrBudgetMasterId='" + OldBudgetMasterId + "' AND ID.PostDrActivityId='" + OldActivityId + "'";
                            rdBuilder.Append(builderSql);
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        }

                    }
                }

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_VoucherGLUpdateLogData, _drvDetailDataset);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public void InsertVoucherGLUpdateLogData(VoucherGLUpdateLog voucherGLUpdateLog, ref DataSet dsData)
        {
            voucherGLUpdateLog.Id = GetAutoNumber(nameof(VoucherGLUpdateLog), PKGeneratorEnum.Yearly, null, DateTime.Now);

            if (string.IsNullOrEmpty(voucherGLUpdateLog.AddedBy))
                AuditService.AddedLog(voucherGLUpdateLog);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[VoucherGLUpdateLog] where 1=2", out dsData);
            }
            AddNewRow<VoucherGLUpdateLog>(dsData.Tables[0], voucherGLUpdateLog);

        }

        #endregion

        #region Bank Reconciliation Data Upload
        public void SaveBankReconciliationUploadData(BankReconciliationUpload bankReconciliationUploadvm, IEnumerable<BankReconciliationUploadedData> bankReconciliationUploadedDataList)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet _BankReconciliationUploadedData = null;
                DataSet _BankReconciliationUpload = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var bankReconciliationUpload = new BankReconciliationUpload
                {
                    BankMasterId = bankReconciliationUploadvm.BankMasterId,
                    OpeningBlance = bankReconciliationUploadvm.OpeningBlance,
                    ClosingBalance = bankReconciliationUploadvm.ClosingBalance,
                    BankStatementNo = bankReconciliationUploadvm.BankStatementNo,
                    FromDate = bankReconciliationUploadvm.FromDate,
                    ToDate = bankReconciliationUploadvm.ToDate,
                    EmployeeId = bankReconciliationUploadvm.EmployeeId,
                    Remarks = bankReconciliationUploadvm.Remarks,
                    CompanyGroupId = identity.CompanyGroupId,
                    CompanyId = identity.CompanyId,
                    PlantId = identity.PlantId,

                };

                InserBankReconciliationUpload(bankReconciliationUpload, ref _BankReconciliationUpload);

                foreach (var item in bankReconciliationUploadedDataList)
                {
                    var bankReconciliationUploadedData = new BankReconciliationUploadedData
                    {
                        BankReconciliationUploadId = bankReconciliationUpload.Id,
                        BankStatementDate = item.BankStatementDate,
                        BankRefNo = item.BankRefNo,
                        BankParticulars = item.BankParticulars,
                        DrAmount = item.DrAmount,
                        CrAmount = item.CrAmount,
                        Remarks = item.Remarks,
                        OwnRefNo = item.OwnRefNo,
                        CompanyGroupId = identity.CompanyGroupId,
                        CompanyId = identity.CompanyId,
                        PlantId = identity.PlantId,

                    };

                    InserBankReconciliationUploadedData(bankReconciliationUploadedData, ref _BankReconciliationUploadedData);
                }

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_BankReconciliationUpload, _BankReconciliationUploadedData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public void InserBankReconciliationUpload(BankReconciliationUpload bankReconciliationUpload, ref DataSet dsData)
        {
            bankReconciliationUpload.Id = GetAutoNumber(nameof(BankReconciliationUpload), PKGeneratorEnum.Yearly, null, DateTime.Now);

            if (string.IsNullOrEmpty(bankReconciliationUpload.AddedBy))
                AuditService.AddedLog(bankReconciliationUpload);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[BankReconciliationUpload] where 1=2", out dsData);
            }
            AddNewRow<BankReconciliationUpload>(dsData.Tables[0], bankReconciliationUpload);

        }
        public void InserBankReconciliationUploadedData(BankReconciliationUploadedData bankReconciliationUploadedData, ref DataSet dsData)
        {
            bankReconciliationUploadedData.Id = GetAutoNumber(nameof(BankReconciliationUploadedData), PKGeneratorEnum.Yearly, null, DateTime.Now);

            if (string.IsNullOrEmpty(bankReconciliationUploadedData.AddedBy))
                AuditService.AddedLog(bankReconciliationUploadedData);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[BankReconciliationUploadedData] where 1=2", out dsData);
            }
            AddNewRow<BankReconciliationUploadedData>(dsData.Tables[0], bankReconciliationUploadedData);

        }
        public void SaveBankReconciliationMap(BankReconciliation bankReconciliation, IEnumerable<BankReconciliationUploadedDataViewModel> bankReconciliationList)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet _BankReconciliationUploadedData = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                foreach (var item in bankReconciliationList)
                {
                    var bankReconciliationMap = new BankReconciliationMap
                    {
                        BankReconciliationUploadedDataId = item.BankReconciliationUploadedDataId,
                        VoucherDetailId = item.VoucherDetailId,
                        GLTransactionDetailId = item.GLTransactionDetailId,
                    };

                    InsertBankReconciliationMap(bankReconciliationMap, ref _BankReconciliationUploadedData);
                }

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_BankReconciliationUploadedData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public void InsertBankReconciliationMap(BankReconciliationMap bankReconciliationMap, ref DataSet dsData)
        {
            bankReconciliationMap.Id = GetAutoNumber(nameof(BankReconciliationMap), PKGeneratorEnum.Yearly, null, DateTime.Now);

            if (string.IsNullOrEmpty(bankReconciliationMap.AddedBy))
                AuditService.AddedLog(bankReconciliationMap);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[BankReconciliationMap] where 1=2", out dsData);
            }
            AddNewRow<BankReconciliationMap>(dsData.Tables[0], bankReconciliationMap);

        }

        #endregion

        #region Bank Reconciliation Data Upload
        public void SaveBalanceSheetSchedulingUploadedData(IEnumerable<BalanceSheetSchedulingUploadedData> balanceSheetSchedulingUploadedDataList)
        {
            try
            {
                var uploadedData = new System.Text.StringBuilder();
                var uploadedDatasql = "";
                foreach (var item in balanceSheetSchedulingUploadedDataList)
                {
                    if (item.BalanceSheetSchedulingId != null && item.BalanceSheetSchedulingId != "null" && item.BalanceSheetSchedulingId != "")
                    {
                        uploadedDatasql = @"UPDATE BMA SET  BMA.BalanceSheetSchedulingId='" + item.BalanceSheetSchedulingId + @"'
                                    ,BMA.TaxApplicable= '" + item.TaxApplicable + "',BMA.TaxType= '" + item.TaxType + @"'
                                    ,BMA.UserCategory= '" + item.UserCategory + "',BMA.UserSubCategory= '" + item.UserSubCategory + @"'
                                    ,BMA.UserItem= '" + item.UserItem + "',BMA.UserReport= '" + item.UserReport + @"'
                                    ,BMA.IsAllowed='" + item.IsAllowed + "',BMA.AllowedDays=" + item.AllowedDays + ",BMA.MonthDay=" + item.MonthDay + @"
                                    FROM [MST].[BudgetMasterActivity] BMA
                                    WHERE BMA.Id='" + item.BudgetMasterActivityId + @"'";
                        uploadedData.Append(uploadedDatasql);
                    }
                    if (item.BalanceSheetSchedulingId == null || item.BalanceSheetSchedulingId == "null" || item.BalanceSheetSchedulingId == "")
                    {
                        uploadedDatasql = @"UPDATE BMA SET BMA.TaxApplicable= '" + item.TaxApplicable + "',BMA.TaxType= '" + item.TaxType + @"'
                                    ,BMA.UserCategory= '" + item.UserCategory + "',BMA.UserSubCategory= '" + item.UserSubCategory + @"'
                                    ,BMA.UserItem= '" + item.UserItem + "',BMA.UserReport= '" + item.UserReport + @"'
                                    ,BMA.IsAllowed='" + item.IsAllowed + "',BMA.AllowedDays=" + item.AllowedDays + ",BMA.MonthDay=" + item.MonthDay + @"
                                    FROM [MST].[BudgetMasterActivity] BMA
                                    WHERE BMA.Id='" + item.BudgetMasterActivityId + @"'";
                        uploadedData.Append(uploadedDatasql);
                        
                    }
                }
                _sqlRepository.ExecuteSqlCommand(uploadedData.ToString());

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public void InserBalanceSheetSchedulingUploadedData(BalanceSheetSchedulingUploadedData balanceSheetSchedulingUploadedData, ref DataSet dsData)
        {
            balanceSheetSchedulingUploadedData.Id = GetAutoNumber(nameof(BalanceSheetSchedulingUploadedData), PKGeneratorEnum.Yearly, null, DateTime.Now);

            if (string.IsNullOrEmpty(balanceSheetSchedulingUploadedData.AddedBy))
                AuditService.AddedLog(balanceSheetSchedulingUploadedData);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[BalanceSheetSchedulingUploadedData] where 1=2", out dsData);
            }
            AddNewRow<BalanceSheetSchedulingUploadedData>(dsData.Tables[0], balanceSheetSchedulingUploadedData);

        }
        #endregion

        #region Update Invoice for Confirm
        public void UpdateInvoiceforConfirm(IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            try
            {
                //ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //DataSet _drvDetailDataset = null;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = @"UPDATE [TRN].[Invoice] SET ExpectedPaymentReceivedDate='" + voucherDetailVM.ExpectedPaymentReceivedDate + "' , BuyerRefNo='" + voucherDetailVM.BuyerRefNo + "' , CustomerRemarks='" + voucherDetailVM.CustomerRemarks + "', BankRefNo='" + voucherDetailVM.BankRefNo + "' , BankMasterId='" + voucherDetailVM.BankMasterId + "'    WHERE Id='" + voucherDetailVM.Id + "' ";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    //con.OpenDataSetThroughAdapter(@"SELECT * FROM [TRN].[Invoice] WHERE Id='" + voucherDetailVM.Id + "'", out _drvDetailDataset, false, "1");

                    //DataView dv = new DataView(_drvDetailDataset.Tables[0]);
                    //dv.RowFilter = "Id='" + voucherDetailVM.Id + "'";
                    //if (dv.Count > 0)
                    //{
                    //    DataRow dr = dv[0].Row;

                    //    dr.BeginEdit();

                    //    dr["ExpectedPaymentReceivedDate"] = voucherDetailVM.ExpectedPaymentReceivedDate;
                    //    dr["BuyerRefNo"] = voucherDetailVM.BuyerRefNo;
                    //    dr["CustomerRemarks"] = voucherDetailVM.CustomerRemarks;

                    //    dr.EndEdit();

                    //}  
                }

                //clsStaticInfo objApp = new clsStaticInfo();
                //objApp.SaveDataSets(_drvDetailDataset);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #endregion

        public GridModel GetVoucherListForCashCheckPrinting(GridParameter parameters)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT V.Id VoucherId ,VD.Id AS VoucherDetailId, VD.GLGeneralInfoId, V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
							  , V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                              ,B.UserName Bank , BM.AccountTitle BankAccountTitle,p.UserName Party
                              , CU.Code AS CurrencyCode, VD.CurrencyId, REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                              ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS CheckDate
                              , VD.BankMasterId, CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
							  , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                                 ,BB.UserName BankBranch, B.CheckTemplate
                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                            LEFT JOIN hkp.BankBranch BB ON BB.Id=bm.BankBranchId
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
							LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
                            WHERE  VD.BankMasterId IS NOT NULL
                           AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
							 AND VD.CrAmount>0 AND CLH.VoucherDetailId iS NULL ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public GridModel Getvoucherlistforcheckprinting(GridParameter parameters)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT V.Id VoucherId ,VD.Id AS VoucherDetailId, VD.GLGeneralInfoId, V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
                , V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                ,B.UserName Bank , BM.AccountTitle BankAccountTitle,p.UserName Party
                , CU.Code AS CurrencyCode, VD.CurrencyId, REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                --,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS CheckDate
                , VD.BankMasterId, CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
                , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                ,BB.UserName BankBranch, B.CheckTemplate
                ,p.Id PartyId
                FROM [TRN].[GLTransactionDetail] AS SD
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
                LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                LEFT JOIN hkp.BankBranch BB ON BB.Id=bm.BankBranchId
                left join TRN.VoucherDetail vdd on vdd.VoucherId=v.Id and vdd.id=(
                select top 1 VD.Id from TRN.VoucherDetail VD where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
                left join HKP.Party p on p.Id=vdd.PartyId
                LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
                LEFT JOIN TRN.BankJournal BJ ON BJ.VoucherId=V.Id AND BJ.BankJournalType!='BankToCash' 
                WHERE VD.BankMasterId IS NOT NULL
                           AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
							 AND VD.CrAmount>0 AND CLH.VoucherDetailId iS NULL ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetVoucherListForCheckVoidPrinting(GridParameter parameters)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT V.Id VoucherId ,VD.Id AS VoucherDetailId, VD.GLGeneralInfoId, V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
                , V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                ,B.UserName Bank , BM.AccountTitle BankAccountTitle,p.UserName Party
                , CU.Code AS CurrencyCode, VD.CurrencyId, REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS CheckDate
                , VD.BankMasterId, CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
                , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                ,BB.UserName BankBranch, B.CheckTemplate
                FROM [TRN].[GLTransactionDetail] AS SD
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
                LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                LEFT JOIN hkp.BankBranch BB ON BB.Id=bm.BankBranchId
                left join TRN.VoucherDetail vdd on vdd.VoucherId=v.Id and vdd.id=(
                select top 1 VD.Id from TRN.VoucherDetail VD where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
                left join HKP.Party p on p.Id=vdd.PartyId
                LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
                LEFT JOIN TRN.BankJournal BJ ON BJ.VoucherId=V.Id AND BJ.BankJournalType!='BankToCash' 
                WHERE VD.BankMasterId IS NOT NULL
                           AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
							 AND VD.CrAmount>0 AND CLH.VoucherDetailId iS NULL ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel getvoucherlistforcheckReprinting(GridParameter parameters)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
	                                SELECT V.Id VoucherId ,VD.Id AS VoucherDetailId, VD.GLGeneralInfoId, V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
							  , V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                              ,B.UserName Bank , BM.AccountTitle BankAccountTitle,p.UserName Party
                              , CU.Code AS CurrencyCode, VD.CurrencyId, REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                            	 ,REPLACE(CONVERT(CHAR(11), CDH.CheckDate, 106),' ','-') AS CheckDate ,CDH.CheckNo
                              , VD.BankMasterId, CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
							  , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                                 ,BB.UserName BankBranch,  B.CheckTemplate ,p.Id PartyId

                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                             LEFT JOIN hkp.BankBranch BB ON BB.Id=bm.BankBranchId
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
							--LEFT JOIN (SELECT DISTINCT VoucherDetailId FROM TRN.CheckLotDetailHistory ) CDH ON CDH.VoucherDetailId=VD.Id
							--LEFT JOIN (sELECT DISTINCT VoucherDetailId,max(CheckDate) CheckDate  FROM TRN.CheckLotDetailHistory group by VoucherDetailId) CDH ON CDH.VoucherDetailId=VD.Id

							--LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
							LEFT JOIN (sELECT DISTINCT C.VoucherDetailId,max(C.CheckDate) CheckDate  
							,CheckNo = STUFF((select distinct ','+  Convert(VARCHAR, XIRD.CheckNumber)  from
									TRN.CheckLotDetailHistory AS XIH
									join TRN.CheckLotDetail XIRD ON XIRD.Id=XIH.CheckLotDetailId
									where XIH.VoucherDetailId=C.VoucherDetailId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1,'')

							FROM TRN.CheckLotDetailHistory C group by C.VoucherDetailId) CDH ON CDH.VoucherDetailId=VD.Id


                            WHERE  VD.BankMasterId IS NOT NULL
                            AND V.CompanyGroupId='" + identity.CompanyGroupId + "' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
							 AND VD.CrAmount>0 AND CDH.VoucherDetailId <>'' 
                               

									";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public GridModel getvoucherlistforCashchequeReprinting(GridParameter parameters)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT V.Id VoucherId ,VD.Id AS VoucherDetailId, VD.GLGeneralInfoId, V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
							  , V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                              ,B.UserName Bank , BM.AccountTitle BankAccountTitle,p.UserName Party
                              , CU.Code AS CurrencyCode, VD.CurrencyId, REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                              ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS CheckDate
                              , VD.BankMasterId, CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
							  , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                                 ,BB.UserName BankBranch, B.CheckTemplate

                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                             LEFT JOIN hkp.BankBranch BB ON BB.Id=bm.BankBranchId
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
							LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
                            WHERE  VD.BankMasterId IS NOT NULL
                         AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
							 AND VD.CrAmount>0 AND CLH.VoucherDetailId <>'' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        #region getVoucherDataList
        public List<Dictionary<string, object>> getVoucherDataList(string companyGroupId, string companyId, string plantId, string voucherNo)
        {
            var sql = @"select * from trn.Voucher where VoucherNo='" + voucherNo + "' and CompanyGroupId='" + companyGroupId + "' and CompanyId='" + companyId + "' and PlantId='" + plantId + @"'";
            return _sqlRepository.GetDataCollection(sql);

        }
        #endregion getVoucherDataList

        #region getVoucherGLDataList
        public List<Dictionary<string, object>> getVoucherGLDataList(string companyGroupId, string companyId, string plantId, string voucherNo)
        {
            var sql = @"SELECT
                        V.Id,FORMAT (V.VoucherDate,'dd-MMM-yyyy') VoucherDate,FORMAT (V.PostingDate,'dd-MMM-yyyy') PostingDate, V.DocRefNo
                        , V.VoucherTypeId,vt.UserName VoucherType
                        , V.CurrencyId,FORMAT (V.DocDate,'dd-MMM-yyyy') DocDate, V.EntityId,
                        C.Code AS CurrencyCode, VD.DrAmount, V.VoucherNo, V.IsPark, V.Narration,e.UserName Entity,V.SourceType
                        ,CASE WHEN  II.IssueType='Capital' AND  II.CapitalizeVoucherId is not null THEN 'Yes' ELSE 'No' END Capitalize
                        ,II.Id InventoryIssueId,IR.Id InventoryReceiveId
                          FROM TRN.[Voucher] AS V
						  LEFT JOIN TRN.InventoryIssue AS II ON II.VoucherId = V.Id
                          LEFT JOIN TRN.InventoryReceive AS IR ON IR.VoucherId = V.Id
                          LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                          LEFT JOIN SCS.VoucherType AS vt ON vt.Id=v.VoucherTypeId
                          LEFT JOIN ORG.Entity AS e ON e.Id=v.EntityId
                          LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 GROUP BY VD.VoucherId
                          ) AS VD ON VD.VoucherId=V.Id
            WHERE V.Archive=0 AND V.VoucherNo='" + voucherNo + "' and V.CompanyGroupId='" + companyGroupId + "' and V.CompanyId='" + companyId + "' and V.PlantId='" + plantId + @"' AND V.SourceType IN ('VendorInvoice','EmployeePayable','IssueJournal','JournalVoucher','InventoryPayable') ";
            return _sqlRepository.GetDataCollection(sql);

        }


        public List<Dictionary<string, object>> getVoucherData(string voucherId)
        {
            var sql = @"SELECT VD.Id, DrAmount, CrAmount, CrAmount AS Amount, VD.GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                , VD.BudgetMasterId, B.UserName AS BudgetName, VD.ActivityId, A.UserName AS ActivityName, P.Code AS PartyCode
                                , P.UserName AS PartyName, VD.PartyType,E.UserName Entity,VD.VoucherId,V.SourceType
                                FROM [TRN].[VoucherDetail] AS VD
                                LEFT JOIN [TRN].[Voucher]  AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                                LEFT JOIN ORG.Entity AS e ON e.Id=VD.EntityId
								WHERE VD.VoucherId='" + voucherId + @"' ORDER BY DrAmount DESC";
            return _sqlRepository.GetDataCollection(sql);

        }


        #endregion getVoucherGLDataList

        public Dictionary<string, object> CheckParkCustomerAdvancePending()
        {

            var sql = @"SELECT TOP(1) * FROM [TRN].[BankReconciliationMap] BRM 
                        JOIN TRN.VoucherDetail VD ON VD.Id=BRM.VoucherDetailId 
                        JOIN TRN.Voucher V ON V.Id=VD.VoucherId WHERE V.IsPark=1 AND V.SourceType='CustomerAdvance'";
            var customerAdvanceTemp = _sqlRepository.GetData(sql);
            if (customerAdvanceTemp.Count > 0)
                throw new CustomException("Please post voucher No "+customerAdvanceTemp["VoucherNo"]+" First!");

            return customerAdvanceTemp;
        }
        public Dictionary<string, object> CheckParkCustomerInvoiceSetOffPending()
        {

            var sql = @"SELECT TOP(1) * FROM [TRN].[BankReconciliationMap] BRM 
                        JOIN TRN.VoucherDetail VD ON VD.Id=BRM.VoucherDetailId 
                        JOIN TRN.Voucher V ON V.Id=VD.VoucherId WHERE V.IsPark=1 AND V.SourceType='CustomerReceipt'";
            var customerAdvanceTemp = _sqlRepository.GetData(sql);
            if (customerAdvanceTemp.Count > 0)
                throw new CustomException("Please post voucher No " + customerAdvanceTemp["VoucherNo"] + " First!");

            return customerAdvanceTemp;
        }
        public Dictionary<string, object> CheckParkExpensesPaymentPending()
        {

            var sql = @"SELECT TOP(1) * FROM [TRN].[BankReconciliationMap] BRM 
                        JOIN TRN.VoucherDetail VD ON VD.Id=BRM.VoucherDetailId 
                        JOIN TRN.Voucher V ON V.Id=VD.VoucherId WHERE V.IsPark=1 AND V.SourceType='BankJournal' and Convert(Date,V.VoucherDate) < Convert(Date,GetDate())";
            var customerAdvanceTemp = _sqlRepository.GetData(sql);
            if (customerAdvanceTemp.Count > 0)
                throw new CustomException("Please post voucher No " + customerAdvanceTemp["VoucherNo"] + " First!");

            return customerAdvanceTemp;
        }
    }
}
