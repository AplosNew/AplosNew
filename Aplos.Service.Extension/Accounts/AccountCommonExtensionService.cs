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

        public Dictionary<string, object> GetMaterialHSNCodeId(string materialMasterId)
        {
            var cmdText = @"select HSNCodeId from mst.MaterialMaster where Id = '" + materialMasterId + "'";
            return _sqlRepository.GetData(cmdText);
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
            var sql = @"select TOP(1) * from hkp.CompanyParty where CompanyId='"+ companyId + "' PlantId='" + plantId + "' and PartyId='" + partyId + "' and PartyType='"+ partyType + @"'";
            var partyPlantTemp = _sqlRepository.GetData(sql);

            if (null == sql || partyPlantTemp.Count == 0)
                throw new CustomException("Plant party mapping not found.");
            return partyPlantTemp;
        }

        public Dictionary<string, object> GetCompanyPartyGL(string partyId,string companyPartyId,string partyGLType)
        {
            var sql = @"select TOP(1) * from hkp.CompanyPartyGL where PartyId='" + partyId + "' CompanyPartyId='" + companyPartyId + "'  and PartyGLType='" + partyGLType + @"'";
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

        public Dictionary<string, object> GetTaxCodeGL(string id)
        {
            var sql = @"SELECT TOP(1) TCGL.* FROM [MST].[TaxCodeGL] AS TCGL
                        WHERE TCGL.Id='" + id + "' ";
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

    }
}
