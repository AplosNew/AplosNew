using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Employees;
using Library.Model.FixedAssets;
using Library.Model.Organizations;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
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
        private Dictionary<string, object> GetCompanyCurrencyId( string companyId)        {            var cmdText =@"select cpc.CurrencyId,C.Code CurrencyCode from SCS.CompanyParallelCurrency cpc
                            LEFT JOIN SCS.Currency C ON C.Id = CPC.CurrencyId where cpc.ParallelCurrencyType = '"+ ParallelCurrencyType.CompanyCurrency.ToString() + "'";            return _sqlRepository.GetData(cmdText);        }
        private bool GetPlantIsShowFCInWord(string plantId)
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
        private Dictionary<string, object> GetfiscalYearfind(string fiscalYearId)        {            var cmdText = @"select * from scs.FiscalYear where Id= '" + fiscalYearId + "'";            return _sqlRepository.GetData(cmdText);        }
        private VoucherTypeNumber GetAuto(string voucherTypeConfigId, string registerName, string period)
        {
          List<VoucherTypeNumber> data= _sqlRepository.GetModelCollection<VoucherTypeNumber>(@"select * from scs.VoucherTypeNumber where VoucherTypeConfigId = '" + voucherTypeConfigId + "' and RegisterName = '" + registerName + "' and [Period] ='"+ period + "'");
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
                con.getDataSet("Select * from scs.VoucherTypeNumber where VoucherTypeConfigId='" + pkgenerator.VoucherTypeConfigId + "' AND RegisterName='"+pkgenerator.RegisterName+"' AND Period='"+pkgenerator.Period+"'", out dsVoucher);

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
                 List<Dictionary<string,object>> data =  _sqlRepository.GetDataCollection(cmdText);
                if (data.Count > 0)
                    return Convert.ToDateTime(data[0]["PostingDate"].ToString());
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
            if (null != lastPostingDate && voucherTypeConfig["IsBackDatePostingAllow"] !=null)
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
        public Voucher InsertVoucher(Voucher voucher, string fiscalYearPrefix,out DataSet dsData)
        {
            return InsertVoucher(voucher, fiscalYearPrefix, true,out  dsData);
        }

        public Voucher InsertVoucher(Voucher voucher, string fiscalYearPrefix, bool flag,out DataSet dsData)
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
            }, voucherVM.FiscalYearPrefix,out DataSet _voucherdataset);
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
            if (vDetailCurrencyData == null || vDetailCurrencyData.Tables.Count==0)
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
                            AND V.CompanyGroupId='" + identity.CompanyGroupId+"' AND V.CompanyId='"+identity.CompanyId+@"'  AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
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
            var sql = @"select * from trn.Voucher where VoucherNo='"+voucherNo+"' and CompanyGroupId='" + companyGroupId + "' and CompanyId='" + companyId + "' and PlantId='" + plantId + @"'";
            return _sqlRepository.GetDataCollection(sql);

        }

        #endregion getVoucherDataList
    }
}
