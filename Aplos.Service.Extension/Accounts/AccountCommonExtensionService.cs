using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.Model.Accounts;
using Library.Service.Core;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Data;
using Library.ViewModel.Vouchers;
using Library.Model.Currencies;
using Library.Service.Properties;
using Library.Core;
using Library.Model.Finances;
using Library.Service.Logs;
using Library.Service.Enums;
using Library.Model.Vouchers;

namespace Library.Service.Extension.Accounts
{
    public class AccountCommonExtensionService
    {
        SqlRepository _sqlRepository;
        public AccountCommonExtensionService()
        {
            _sqlRepository = new SqlRepository();
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
        public void CheckingFiscalYearClose(Voucher voucher)
        {
            if (voucher.PostingDate != null)
            {
                DataTable QryFiscalYearClose = _sqlRepository.GetDataTable("select * from [SCS].[FiscalYearClose] where  CompanyId='" + voucher.CompanyId + "' AND PlantId='" + voucher.PlantId + "' AND FiscalYearId in(select Id from [SCS].[FiscalYear] where '" + voucher.PostingDate.Date + "' between StartDate and EndDate) ");
                if (QryFiscalYearClose.Rows.Count > 0)
                    throw new Exception("Fiscal Year already closed!!!");

            }
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


        public AdditionalTax InsertAdditionalTax(AdditionalTax additionalTax, out DataSet dsData)
        {
            if (string.IsNullOrEmpty(additionalTax.AddedBy))
                AuditService.AddedLog(additionalTax);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.AdditionalTax where 1=2", out dsData);

            AddNewRow<AdditionalTax>(dsData.Tables[0], additionalTax);

            return additionalTax;
        }
        public AdditionalTaxDetail InsertAdditionalTaxDetail(AdditionalTaxDetail additionalTaxDetail, ref  DataSet dsData)
        {
            if (string.IsNullOrEmpty(additionalTaxDetail.AddedBy))
                AuditService.AddedLog(additionalTaxDetail);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.AdditionalTaxDetail where 1=2", out dsData);

            AddNewRow<AdditionalTaxDetail>(dsData.Tables[0], additionalTaxDetail);

            return additionalTaxDetail;
        }
        public GRNAcceptanceMap InsertGRNAcceptanceMap(GRNAcceptanceMap gRNAcceptanceMap, out DataSet dsData)
        {
            if (string.IsNullOrEmpty(gRNAcceptanceMap.AddedBy))
                AuditService.AddedLog(gRNAcceptanceMap);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from TRN.GRNAcceptanceMap where 1=2", out dsData);

            AddNewRow<GRNAcceptanceMap>(dsData.Tables[0], gRNAcceptanceMap);

            return gRNAcceptanceMap;
        }

        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
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
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        public void EditRow(DataRow dr, Dictionary<string, object> sourceData)
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


        public void AddNewRow<T>(DataTable dt, T Data)
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
        public void EditRow<T>(DataRow dr, T Data)
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

        public void AddNewRowD(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow(); foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress; dt.Rows.Add(dr);
        }

        public void EditRowD(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit(); foreach (var item in sourceData.Keys)
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
        public Dictionary<string, object> GetInvestmentGL(string companyId, string financingTypeId)
        {
           
            var sql = @"SELECT TOP(1) FTGL.* FROM [HKP].[FinancingTypeGL] AS FTGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=FTGL.COAId
                        WHERE C.Id='" + companyId + "' AND FTGL.FinancingTypeId='" + financingTypeId + "'";
            var glTemp = _sqlRepository.GetData(sql);
                if (null == glTemp || glTemp.Count==0)
                throw new CustomException("This Transaction Type GL not Found!");

            return glTemp;
        }
        public Dictionary<string, object> GetGLByBudgetMasterId(string budgetmasterid)
        {

            var sql = @"SELECT TOP(1) GLGeneralInfoId FROM [MST].[BudgetMaster]  
                        WHERE Id='" + budgetmasterid + "'";
            var budgetmasterTemp = _sqlRepository.GetData(sql);
            if (null == budgetmasterTemp || budgetmasterTemp.Count == 0)
                throw new CustomException("Budget Master  not Found!");

            return budgetmasterTemp;
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
        public Dictionary<string, object> GetIncentiveMaster(string incentiveMasterId)
        {

            var sql = @"SELECT TOP(1) * FROM [MST].[IncentiveMaster]  
                        WHERE Id='" + incentiveMasterId + "'";
            var incentiveMasterTemp = _sqlRepository.GetData(sql);
            if (null == incentiveMasterTemp || incentiveMasterTemp.Count == 0)
                throw new CustomException("Incentive Master  not Found!");

            return incentiveMasterTemp;
        }
        public Dictionary<string, object> GetReverseGL(string voucherId)
        {

            var sql = @"SELECT TOP(1) * FROM trn.VoucherDetail  
                        WHERE VoucherId='" + voucherId + "' and DrAmount>0";
            var ReverseGLTemp = _sqlRepository.GetData(sql);
            if (null == ReverseGLTemp || ReverseGLTemp.Count == 0)
                throw new CustomException("Reverse GL  not Found!");

            return ReverseGLTemp;
        }

        public IList<Dictionary<string, object>> GetMaterialHSNCodeId(string materialMasterId)
        {
            var cmdText = @"select HSN.* from mst.HSNTaxPercentage HSN left join mst.MaterialMaster mm on mm.HSNCodeId=HSN.Id
						where mm.Id='' = '" + materialMasterId + "'";
            return _sqlRepository.GetDataCollection(cmdText);
        }


        public Dictionary<string, object> GetCompanyAddressStateId(string companyId)
        {
            var cmdText = @"select AM.StateId from MST.AddressMaster AM left join ORG.Company C ON C.AddressMasterId=AM.Id
            where C.Id= '" + companyId + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public Dictionary<string, object> GetPartyAddressStateId(string partyId)
        {
            var cmdText = @"select AM.StateId from MST.AddressMaster AM left join HKP.Party P ON P.AddressMasterId=AM.Id
            where P.Id= '" + partyId + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public Dictionary<string, object> GetPartyByCompany(string companyGroupId, string companyId)
        {
            var sql = @"select TOP(1) Id from hkp.Party where CompanyGroupId='"+ companyGroupId + @"' and CompanyId='"+ companyId + "'";
            var partyTemp = _sqlRepository.GetData(sql);
            if (null == sql || partyTemp.Count==0)
                throw new CustomException("This Company is not created as InterCompany Party.");
            return partyTemp;
        }

        public Dictionary<string, object> GetPartyPlantByPlant(string partyId, string plantId)
        {
            var sql = @"select TOP(1) Id from hkp.PartyPlant where PlantId='" + plantId + @"' and PartyId='" + partyId + "'";
            var partyPlantTemp = _sqlRepository.GetData(sql);

            if (null == sql || partyPlantTemp.Count==0)
                throw new CustomException("This Company is not created as InterCompany Party Plant.");
            return partyPlantTemp;
        }

        public Dictionary<string, object> GetCompanyParty(string companyId, string plantId,string partyId,string partyType)
        {
            var sql = @"select TOP(1) * from hkp.CompanyParty where CompanyId='"+ companyId + "'  and PartyId='" + partyId + "' and PartyType='"+ partyType + @"'";
            var partyPlantTemp = _sqlRepository.GetData(sql);

            if (null == sql || partyPlantTemp.Count == 0)
                throw new CustomException("Plant party mapping not found.");
            return partyPlantTemp;
        }

        public Dictionary<string, object> GetCompanyPartyGL(string partyId,string companyPartyId,string partyGLType)
        {
            var sql = @"select TOP(1) * from hkp.CompanyPartyGL where PartyId='" + partyId + "' and CompanyPartyId='" + companyPartyId + "'  and PartyGLType='" + partyGLType + @"'";
            var partyPlantTemp = _sqlRepository.GetData(sql);

            if (null == sql || partyPlantTemp.Count == 0)
                throw new CustomException("Party DownPayment GL not found!.");
            return partyPlantTemp;
        }

        public Dictionary<string, object> GetFinancingTypeGL(string companyId, string financingTypeId)
        {
            var sql = @"SELECT TOP(1) FTGL.* FROM [HKP].[FinancingTypeGL] AS FTGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=FTGL.COAId
                        WHERE C.Id='" + companyId + "' AND FTGL.FinancingTypeId='" + financingTypeId + "'";
            var glTemp = _sqlRepository.GetData(sql);

            if (null == sql || glTemp.Count == 0)
                throw new CustomException("This Transaction Type GL not Found!.");
            return glTemp;
        }
        public Dictionary<string, object> GetDiscountGL(string companyId, string sourceType)
        {
            var sql = @"SELECT TOP(1) FTGL.* FROM [HKP].[FinancingTypeGL] AS FTGL JOIN [HKP].[FinancingType] FT ON FT.Id=FTGL.FinancingTypeId
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=FTGL.COAId
                        WHERE C.Id='"+ companyId + "' AND FT.SourceType='" + sourceType + "'";
            var glTemp = _sqlRepository.GetData(sql);

            if (null == sql || glTemp.Count == 0)
                throw new CustomException("There is no  Discount GL Found!.");
            return glTemp;
        }
        public Dictionary<string, object> GetTaxCode(string id)
        {
            var sql = @"SELECT TOP(1) TCGL.*,TC.TaxCategoryType  FROM [MST].[TaxCode] AS TCGL 
						JOIN mst.TaxCategory TC ON TC.Id=TCGL.TaxCategoryId
                        WHERE TCGL.Id='" + id + "' ";
            var glTemp = _sqlRepository.GetData(sql);

            if (null == sql || glTemp.Count == 0)
                throw new CustomException("Tax code  not found!.");
            return glTemp;
        }
        public Dictionary<string, object> GetTaxCategoryByCircle( string circle)
        {
            var sql = @"SELECT TOP(1) TCGL.* FROM [MST].[TaxCategory] AS TCGL
                        WHERE TCGL.Active=1 and TCGL.TaxCircle='"+ circle + "'";
            var glTemp = _sqlRepository.GetData(sql);

            if (null == sql || glTemp.Count == 0)
                throw new CustomException("Tax code  not found!.");
            return glTemp;
        }
        public Dictionary<string, object> GetTaxCodeGL(string id)
        {
            var sql = @"SELECT TOP(1) TCGL.* FROM [MST].[TaxCodeGL] AS TCGL
                        WHERE TCGL.TaxCodeId='" + id + "' ";
            var glTemp = _sqlRepository.GetData(sql);

            if (null == sql || glTemp.Count == 0)
                throw new CustomException("Tax code GL not found!.");
            return glTemp;
        }

        public Dictionary<string, object> GetTaxCategoryGL(string id)
        {
            var sql = @"SELECT TOP(1) TCGL.* FROM [MST].[TaxCategory] AS TCGL
                        WHERE TCGL.Id='" + id + "' ";
            var glTemp = _sqlRepository.GetData(sql);

            if (null == sql || glTemp.Count == 0)
                throw new CustomException("Tax Category GL not found!.");
            return glTemp;
        }
        public Dictionary<string, object> GetTaxCategoryInputGL(string id)
        {
            var sql = @"SELECT TOP(1) TCGL.* FROM [MST].[TaxCategory] AS TCGL
                        WHERE TCGL.Id='" + id + "' and TCGL.InputTaxOutPutTax = 'Input' ";
            var glTemp = _sqlRepository.GetData(sql);

            if (null == sql || glTemp.Count == 0)
                throw new CustomException("Input Tax Category GL not found!.");
            return glTemp;
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

        
        public string MakePK(string masterId, int currentId, int padLeft)
        {
            return masterId + currentId.ToString().PadLeft(padLeft, '0');
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
        #region Deleted/Parked Log 
        public void InsertVoucherLogDeleted(string voucherId, string VoucherNo, string financingId, string financingWriteOffId, string invoiceId, string invoiceWriteOffId, string advanceId, string advanceWriteOffId, string adjustmentNoteId, string bankJournalId, string employeePayableId, string employeePayableWriteOffId, string salesId, string remarks)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = @"DECLARE @VoucherLogId AS VARCHAR(80)='',@VoucherId AS VARCHAR(80)='" + voucherId + "',@FinancingId AS VARCHAR(10)='" + financingId + "',@FinancingWriteOffId AS VARCHAR(80)='" + financingWriteOffId + "',@InvoiceId AS VARCHAR(80)='" + invoiceId + "',@InvoiceWriteOffId AS VARCHAR(80)='" + invoiceWriteOffId + "',@AdvanceId AS VARCHAR(80)='" + advanceId + "',@AdvanceWriteOffId AS VARCHAR(80)='" + advanceWriteOffId + "',@AdjustmentNoteId AS VARCHAR(80)='" + adjustmentNoteId + "',@BankJournalId AS VARCHAR(80)='" + bankJournalId + "',@EmployeePayableId AS VARCHAR(80)='" + employeePayableId + "',@EmployeePayableWriteOffId AS VARCHAR(80)='" + employeePayableWriteOffId + "',@SalesId AS VARCHAR(10)='" + salesId + "',@ActivityType AS VARCHAR(100)='Delete',@Status AS VARCHAR(200)='',@Remarks AS VARCHAR(500)='" + remarks + "',@AddedBy AS VARCHAR(30)='" + identity.Name + @"',@AddedFromIP AS VARCHAR(15)='" + identity.IPAddress + @"';
                
                select @VoucherLogId=ISNULL(MAX(CAST(Id AS INT)), 0)+1  from [TRN].[VoucherLog]
                SET @Status='VoucherNo " + VoucherNo + @" is Deleted by " + identity.Name + @"'

                INSERT INTO [TRN].[VoucherLog](Id, VoucherId, FinancingId, FinancingWriteOffId, InvoiceId, InvoiceWriteOffId, AdvanceId, AdvanceWriteOffId, AdjustmentNoteId, BankJournalId, EmployeePayableId, EmployeePayableWriteOffId, ActivityType, Status, Remarks, AddedBy, AddedDate, AddedFromIP,SalesId)
                VALUES(@VoucherLogId,@VoucherId, @FinancingId, @FinancingWriteOffId, @InvoiceId, @InvoiceWriteOffId, @AdvanceId, @AdvanceWriteOffId, @AdjustmentNoteId, @BankJournalId, @EmployeePayableId, @EmployeePayableWriteOffId, @ActivityType, @Status, @Remarks, @AddedBy, GETDATE(), @AddedFromIP,@SalesId) 

                INSERT INTO [TRN].[VoucherDeleted](Id, CompanyGroupId, CompanyId, PlantId, EntityId, FiscalYearId, FiscalYearPeriodId, TaxYearId, TaxYearPeriodId, VoucherTypeId, CurrencyId, SourceType, VoucherNo, VoucherDate, TransactionRefNo, PostingDate, DocRefNo, DocDate, Narration, IsPark, Archive, AddedBy, AddedDate, AddedFromIP, UpdatedBy, UpdatedDate, UpdatedFromIP, PostedBy, PostedDate, PostedFromIP, ExchangeType)
                SELECT Id, CompanyGroupId, CompanyId, PlantId, EntityId, FiscalYearId, FiscalYearPeriodId, TaxYearId, TaxYearPeriodId, VoucherTypeId, CurrencyId, SourceType, VoucherNo, VoucherDate, TransactionRefNo, PostingDate, DocRefNo, DocDate, Narration, IsPark, Archive, AddedBy, AddedDate, AddedFromIP, UpdatedBy, UpdatedDate, UpdatedFromIP, PostedBy, PostedDate, PostedFromIP, ExchangeType FROM [TRN].[Voucher] WHERE Id=@VoucherId

                INSERT INTO [TRN].[VoucherDetailDeleted](Id, PlantId, EntityId, FiscalYearId, FiscalYearPeriodId, VoucherId, GLGeneralInfoId, BudgetMasterId, ActivityId, CheckLotDetailId, CurrencyId, BankMasterId, BankChargeId, CashMasterId, CostCenterId, FinancingDetailId, InvoiceTaxWriteOffDetailId, EmployeeId, PartyId, PartyPlantId, InvoiceDetailId, AdvanceDetailId, AdvanceWriteOffDetailId, InvoiceWriteOffDetailId, SecurityDepositDetailId, SecurityDepositWriteOffDetailId, EmployeePayableDetailId, EmployeePayableWriteOffDetailId, ExpenseBookingDetailId, PartyType, DocRefNo, DocDate, RefCode, Narration, DrAmount, CrAmount, TotalTaxAmount, PostingWithoutTaxAllow, IsExcludingTax, IsPark, Archive, AddedBy, AddedDate, AddedFromIP, UpdatedBy, UpdatedDate, UpdatedFromIP, CompanyId, PaymentSource, JournalId, InterTransactionDetailId, BankJournalDetailId, InvoiceTaxDetailId, TrnNature, AdjustmentNoteDetailId, InventoryIssueDetailId, OpeningBalanceDetailId, FinancingDetailWriteOffId, FixedAssetMasterId, FAType, IsCapitalizeExpenseRegister, LoanSetOffGroupNo, SalaryType, SalaryHeadId)
                SELECT Id, PlantId, EntityId, FiscalYearId, FiscalYearPeriodId, VoucherId, GLGeneralInfoId, BudgetMasterId, ActivityId, CheckLotDetailId, CurrencyId, BankMasterId, BankChargeId, CashMasterId, CostCenterId, FinancingDetailId, InvoiceTaxWriteOffDetailId, EmployeeId, PartyId, PartyPlantId, InvoiceDetailId, AdvanceDetailId, AdvanceWriteOffDetailId, InvoiceWriteOffDetailId, SecurityDepositDetailId, SecurityDepositWriteOffDetailId, EmployeePayableDetailId, EmployeePayableWriteOffDetailId, ExpenseBookingDetailId, PartyType, DocRefNo, DocDate, RefCode, Narration, DrAmount, CrAmount, TotalTaxAmount, PostingWithoutTaxAllow, IsExcludingTax, IsPark, Archive, AddedBy, AddedDate, AddedFromIP, UpdatedBy, UpdatedDate, UpdatedFromIP, CompanyId, PaymentSource, JournalId, InterTransactionDetailId, BankJournalDetailId, InvoiceTaxDetailId, TrnNature, AdjustmentNoteDetailId, InventoryIssueDetailId, OpeningBalanceDetailId, FinancingDetailWriteOffId, FixedAssetMasterId, FAType, IsCapitalizeExpenseRegister, LoanSetOffGroupNo, SalaryType, SalaryHeadId FROM [TRN].[VoucherDetail] WHERE VoucherId=@VoucherId

                INSERT INTO [TRN].[VoucherDetailCurrencyDeleted] (Id, VoucherId, VoucherDetailId, ParallelCurrencyId, FromCurrencyId, ToCurrencyId, ToCurrencyRate, ToCurrencyConversion, DrAmount, CrAmount, AddedBy, AddedDate, AddedFromIP, UpdatedBy, UpdatedDate, UpdatedFromIP)
                SELECT Id, VoucherId, VoucherDetailId, ParallelCurrencyId, FromCurrencyId, ToCurrencyId, ToCurrencyRate, ToCurrencyConversion, DrAmount, CrAmount, AddedBy, AddedDate, AddedFromIP, UpdatedBy, UpdatedDate, UpdatedFromIP FROM [TRN].[VoucherDetailCurrency] WHERE VoucherId=@VoucherId
                ";
                rdBuilder.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
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
                //
            }
        }
        #endregion
        #region Deleted/Parked Log 
        public void InsertVoucherLogParked(string voucherId, string VoucherNo, string financingId, string financingWriteOffId, string invoiceId, string invoiceWriteOffId, string advanceId, string advanceWriteOffId, string adjustmentNoteId, string bankJournalId, string employeePayableId, string employeePayableWriteOffId, string salesId, string remarks)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = @"DECLARE @VoucherLogId AS VARCHAR(80)='',@VoucherId AS VARCHAR(80)='" + voucherId + "',@FinancingId AS VARCHAR(10)='" + financingId + "',@FinancingWriteOffId AS VARCHAR(80)='" + financingWriteOffId + "',@InvoiceId AS VARCHAR(80)='" + invoiceId + "',@InvoiceWriteOffId AS VARCHAR(80)='" + invoiceWriteOffId + "',@AdvanceId AS VARCHAR(80)='" + advanceId + "',@AdvanceWriteOffId AS VARCHAR(80)='" + advanceWriteOffId + "',@AdjustmentNoteId AS VARCHAR(80)='" + adjustmentNoteId + "',@BankJournalId AS VARCHAR(80)='" + bankJournalId + "',@EmployeePayableId AS VARCHAR(80)='" + employeePayableId + "',@EmployeePayableWriteOffId AS VARCHAR(80)='" + employeePayableWriteOffId + "',@SalesId AS VARCHAR(10)='" + salesId + "',@ActivityType AS VARCHAR(100)='Parked',@Status AS VARCHAR(200)='',@Remarks AS VARCHAR(500)='" + remarks + "',@AddedBy AS VARCHAR(30)='" + identity.Name + @"',@AddedFromIP AS VARCHAR(15)='" + identity.IPAddress + @"';
                
                select @VoucherLogId=ISNULL(MAX(CAST(Id AS INT)), 0)+1  from [TRN].[VoucherLog]
                SET @Status='VoucherNo " + VoucherNo + @" is Parked by " + identity.Name + @"'

                INSERT INTO [TRN].[VoucherLog](Id, VoucherId, FinancingId, FinancingWriteOffId, InvoiceId, InvoiceWriteOffId, AdvanceId, AdvanceWriteOffId, AdjustmentNoteId, BankJournalId, EmployeePayableId, EmployeePayableWriteOffId, ActivityType, Status, Remarks, AddedBy, AddedDate, AddedFromIP,SalesId)
                VALUES(@VoucherLogId,@VoucherId, @FinancingId, @FinancingWriteOffId, @InvoiceId, @InvoiceWriteOffId, @AdvanceId, @AdvanceWriteOffId, @AdjustmentNoteId, @BankJournalId, @EmployeePayableId, @EmployeePayableWriteOffId, @ActivityType, @Status, @Remarks, @AddedBy, GETDATE(), @AddedFromIP,@SalesId) 

                 ";
                rdBuilder.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
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
                //
            }
        }
        #endregion

        public Dictionary<string, object> GetEmployeeSalaryAdvane(string id)
        {
            var sql = @"select top(1) esa.*,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId from TRN.EmployeeSalaryAdvance esa 
                left join trn.voucherdetail vd on vd.id=esa.VoucherDetailId where esa.Id='" + id + "'";
            var customerAdvanceTemp = _sqlRepository.GetData(sql);

            return customerAdvanceTemp;
        }
        public Dictionary<string, object> GetEmployeeSalaryMultipleAdvane(string workerAdvanceDetailId)
        {
            var sql = @"select top(1) est.*,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId from TRN.EmployeeSubsequentTransaction est 
                left join trn.voucherdetail vd on vd.id=est.VoucherDetailId where est.WorkerAdvanceDetailId='" + workerAdvanceDetailId + @"' ";
            var customerAdvanceTemp = _sqlRepository.GetData(sql);

            return customerAdvanceTemp;
        }
    }
}
