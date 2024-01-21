using Library.Accounting.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Calendars;
using Library.Model.Currencies;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Model.Invoices;
using Library.Model.Organizations;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class FiscalYearCloseService
    {
        private readonly ISqlRepository _sqlRepository;
        public FiscalYearCloseService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        
        public void InsertFiscalYearClose(FiscalYearClose fiscalYearCloseVM)
        {
            try
            {
                var fiscalYearClose = new FiscalYearClose
                {
                   
                    CompanyGroupId = fiscalYearCloseVM.CompanyGroupId,
                    CompanyId = fiscalYearCloseVM.CompanyId,
                    PlantId = fiscalYearCloseVM.PlantId,
                    FiscalYearId = fiscalYearCloseVM.FiscalYearId
                   
                };
                InsertFiscalYearCloseData(fiscalYearClose, out DataSet _fiscalYearCloseData);

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_fiscalYearCloseData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public FiscalYearClose InsertFiscalYearCloseData(FiscalYearClose fiscalYearClose, out DataSet dsData)
        {
            AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
            
           
            if (!string.IsNullOrEmpty(fiscalYearClose.FiscalYearId))
            {
                DataTable Qry = _sqlRepository.GetDataTable("select * from [SCS].[FiscalYearClose] where FiscalYearId='" + fiscalYearClose.FiscalYearId + "' AND CompanyId='" + fiscalYearClose.CompanyId + "' AND PlantId='" + fiscalYearClose.PlantId + "' AND Id<>''");
                if (Qry.Rows.Count > 0)
                    throw new Exception("Data already exists!!!");

            }
            fiscalYearClose.Id = _accountsCommonService.GetAutoNumber(nameof(FiscalYearClose), PKGeneratorEnum.Yearly, null, DateTime.Now);
          
            if (string.IsNullOrEmpty(fiscalYearClose.AddedBy))
                AuditService.AddedLog(fiscalYearClose);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from [SCS].[FiscalYearClose] where 1=2", out dsData);

            AddNewRow<FiscalYearClose>(dsData.Tables[0], fiscalYearClose);

            return fiscalYearClose;
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();

        }
        private void EditRow(DataSet ds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].DefaultView[0].Row;

                dr.BeginEdit();
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr.EndEdit();
            }
            clsStaticInfo obj = new clsStaticInfo();
            obj.SaveDataSets(ds);

        }

        public GridModel GetFiscalYearCloseList(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FYC.Id,FY.Id Sequence,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName
                                FROM [SCS].[FiscalYearClose] As FYC
                                LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
                                LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
                                LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel GetPFESICDisbursementList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT V.Id, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode, VD.DrAmount, V.VoucherNo, V.IsPark, V.Narration
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 GROUP BY VD.VoucherId
                                    ) AS VD ON VD.VoucherId=V.Id
                                    WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "'AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + SourceType.PFESICDisbursement + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> CheckYearClosedByDate(System.DateTime date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [SCS].[FiscalYearClose] where  VoucherId is not null AND CompanyId='" + identity.CompanyId + "' AND PlantId='" + identity.PlantId + "' AND FiscalYearId in(select Id from [SCS].[FiscalYear] where '" + date + "' between StartDate and EndDate) ";
            return _sqlRepository.GetDataCollection(sql);
        }
        #region
        public List<Dictionary<string, object>> GetFiscalYearClosePostedList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (SELECT V.Id,V.VoucherNo,V.DocRefNo,FORMAT(V.PostingDate, 'dd-MMM-yyyy') PostingDate
				,FYC.Id FiscalYearCloseId,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName,FYC.AdjustmentAmount Amount
				FROM TRN.Voucher V 
				LEFT JOIN [SCS].[FiscalYearClose] FYC ON FYC.VoucherId=V.Id
				LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
				LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
                LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId
                WHERE V.CompanyId='" + companyId + @"' AND V.Archive=0 AND V.SourceType='YearCloseJournal' 
                ) AS TEMP WHERE " + strkey + " order by PostingDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFiscalYearCloseListForPosting()
        {
            string sql = @"SELECT FYC.Id,FY.Id Sequence,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName
    ,(SELECT ROUND(SUM(RevenueBalance)-SUM(ExpenseBalance),2)
	FROM(SELECT
                sum(VDC.DrAmount) as DrAmount,
                sum(VDC.CrAmount) as CrAmount
                ,ACT.BalanceType
                ,ACT.Id AS [MainHead]
				,CASE WHEN ACT.Id='Expense' THEN sum(VDC.DrAmount)-sum(VDC.CrAmount) ELSE 0 END ExpenseBalance
				,CASE WHEN ACT.Id='Revenue' THEN sum(VDC.CrAmount)-sum(VDC.DrAmount) ELSE 0 END RevenueBalance
                FROM TRN.VoucherDetailCurrency AS VDC
                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                where act.IsBalanceSheet=0 AND v.IsPark=0 AND v.PostingDate between FY.StartDate AND FY.EndDate AND V.CompanyId=FYC.CompanyId AND V.PlantId=FYC.PlantId
                group by ACT.BalanceType,ACT.Id )X)Amount
                                FROM [SCS].[FiscalYearClose] As FYC
                                LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
                                LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
                                LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId
                                Where FYC.VoucherId IS NULL";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFiscalYearClosedListForReporting()
        {
            string sql = @"SELECT FYC.Id,FY.Id Sequence,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName
                                FROM [SCS].[FiscalYearClose] As FYC
                                LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
                                LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
                                LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId
                                Where FYC.VoucherId IS NOT NULL";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFiscalYearCloseSingleJVList(string fiscalYearCloseId, string companyId, string plantId)
        {
            var sql = @"DECLARE @fiscalYearCloseId varchar(50)='" + fiscalYearCloseId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"', @startDate varchar(50), @endDate varchar(50)

						SELECT @startDate=FORMAT(FY.StartDate, 'dd-MMM-yyyy') ,@endDate=FORMAT(FY.EndDate, 'dd-MMM-yyyy') 
                                FROM [SCS].[FiscalYearClose] As FYC
                                LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
								WHERE FYC.Id=@fiscalYearCloseId
CREATE TABLE TempProfitandLoss(
	[ProfitLoss] [varchar](10)  NULL,
	[Amount] [decimal](18, 2) NULL
	)
	INSERT INTO TempProfitandLoss(ProfitLoss,Amount)
	SELECT CASE WHEN SUM(RevenueBalance)>SUM(ExpenseBalance) THEN 'Profit' ELSE 'Loss' END
	,CASE WHEN SUM(RevenueBalance)>SUM(ExpenseBalance) THEN SUM(RevenueBalance)-SUM(ExpenseBalance) ELSE SUM(ExpenseBalance)-SUM(RevenueBalance) END 
	FROM(SELECT
                sum(VDC.DrAmount) as DrAmount,
                sum(VDC.CrAmount) as CrAmount
                ,ACT.BalanceType
                ,ACT.Id AS [MainHead]
				,CASE WHEN ACT.Id='Expense' THEN sum(VDC.DrAmount)-sum(VDC.CrAmount) ELSE 0 END ExpenseBalance
				,CASE WHEN ACT.Id='Revenue' THEN sum(VDC.CrAmount)-sum(VDC.DrAmount) ELSE 0 END RevenueBalance
                FROM TRN.VoucherDetailCurrency AS VDC
                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                where act.IsBalanceSheet=0 AND v.IsPark=0 AND v.PostingDate between @startDate AND  @endDate AND V.CompanyId=@companyId AND V.PlantId=@plantId
                group by ACT.BalanceType,ACT.Id )X

						SELECT X.* FROM(
						SELECT  'GainOnIncomeStatement' AS OtherName, 'Dr' AS TrnType,'Profit' TransactionTypeId
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = BMA.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Profit') AS Dr
							, NULL Cr
							, (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Profit') AS Amount
					    FROM [HKP].[GeneralAccountDeterminate] GAD
						LEFT JOIN [MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN (SELECT Id,BudgetMasterId,ActivityId FROM [MST].[BudgetMasterActivity] WHERE Isdefault=1 ) AS BMA ON BMA.BudgetMasterId= GAD.BudgetMasterId 
						LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
					    WHERE GAD.Id='GainOnIncomeStatement' 
						GROUP BY BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,GAD.BudgetMasterId,BMA.ActivityId,BMA.Id
						
						UNION
						SELECT  'GainOnRetainedEarning' AS OtherName, 'Cr' AS TrnType,'Profit' TransactionTypeId
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = BMA.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, NULL Dr
							,  (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Profit') AS Cr
							,  (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Profit') AS Amount
						FROM [HKP].[GeneralAccountDeterminate] GAD
						LEFT JOIN [MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN (SELECT Id,BudgetMasterId,ActivityId FROM [MST].[BudgetMasterActivity] WHERE Isdefault=1 ) AS BMA ON BMA.BudgetMasterId= GAD.BudgetMasterId 
						LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
					    WHERE GAD.Id='GainOnRetainedEarning' 
						GROUP BY BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,GAD.BudgetMasterId,BMA.ActivityId,BMA.Id

						UNION
						SELECT  'LossOnRetainedEarning' AS OtherName, 'Dr' AS TrnType,'Loss' TransactionTypeId
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = BMA.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Loss') AS Dr
							, NULL Cr
							, (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Loss') AS Amount
					    FROM [HKP].[GeneralAccountDeterminate] GAD
						LEFT JOIN [MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN (SELECT Id,BudgetMasterId,ActivityId FROM [MST].[BudgetMasterActivity] WHERE Isdefault=1 ) AS BMA ON BMA.BudgetMasterId= GAD.BudgetMasterId 
						LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
					    WHERE GAD.Id='LossOnRetainedEarning' 
						GROUP BY BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,GAD.BudgetMasterId,BMA.ActivityId,BMA.Id
						
						UNION
						SELECT  'LossOnIncomeStatement' AS OtherName, 'Cr' AS TrnType,'Loss' TransactionTypeId
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = BMA.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, NULL Dr
							,  (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Loss') AS Cr
							,  (SELECT Amount FROM TempProfitandLoss WHERE ProfitLoss='Loss') AS Amount
						FROM [HKP].[GeneralAccountDeterminate] GAD
						LEFT JOIN [MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN (SELECT Id,BudgetMasterId,ActivityId FROM [MST].[BudgetMasterActivity] WHERE Isdefault=1 ) AS BMA ON BMA.BudgetMasterId= GAD.BudgetMasterId 
						LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
					    WHERE GAD.Id='LossOnIncomeStatement' 
						GROUP BY BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,GAD.BudgetMasterId,BMA.ActivityId,BMA.Id
						) X  
                        WHERE X.Amount>0
						ORDER BY 2 DESC

						DROP TABLE TempProfitandLoss ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public void InsertFiscalYearClosePosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, Dictionary<string, object> fiscalYearClosedata)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                var voucherDrId = "";
                decimal totalDrAmount = 0;
                decimal totalCrAmount = 0;

                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.YearCloseJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;


                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            BudgetMasterActivityId = voucherDetailVM.BudgetMasterActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = voucherDr.DrAmount
                        }, ref _drvDetailCurrencyData);

                        totalDrAmount += voucherDr.DrAmount;
                        voucherDrId = voucherDr.Id;
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            BudgetMasterActivityId = voucherDetailVM.BudgetMasterActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };

                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            CrAmount = voucherCr.CrAmount
                        }, ref _crvDetailCurrencyData);

                        totalCrAmount += voucherCr.CrAmount;
                    }
                }
                if (totalCrAmount != totalDrAmount)
                    throw new CustomException("Dr Cr Amount not match !.");

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData);
                if (fiscalYearClosedata != null)
                {
                    builderSql = "";
                    builderSql = @"DECLARE @fiscalYearCloseId varchar(50)='" + fiscalYearClosedata["Id"].ToString() + "' , @companyGroupId varchar(10)='" + identity.CompanyGroupId + "', @companyId varchar(10)='" + identity.CompanyId + "', @plantId varchar(30)='" + identity.PlantId + "', @addedBy varchar(100)='" + identity.Name + "', @addedFromIP varchar(30)='" + identity.IPAddress + @"', @startDate varchar(50), @endDate varchar(50)
                    SELECT @startDate=FORMAT(FY.StartDate, 'dd-MMM-yyyy') ,@endDate=FORMAT(FY.EndDate, 'dd-MMM-yyyy') 
                                                    FROM [SCS].[FiscalYearClose] As FYC
                                                    LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
								                    WHERE FYC.Id=@fiscalYearCloseId

                    INSERT INTO [TRN].[FiscalYearCloseTrialBalance]( [AccountCodeId], [ParallelCurrencyId], [CurrencyCode], [OBDRcumulative], [OBCRcumulative], [DRcumulative], [CRcumulative], [CBDRcumulative], [CBCRcumulative], [PDRcumulative], [PCRcumulative], [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode], [Budget], [BudgetMasterId], [Activity], [Particulars], [ActivityId], [BankMasterId], [CashMasterId], [PartyId], [PartyPlantId], [CompanyGroupId], [CompanyId], [PlantId], [FiscalYearCloseId], [AddedBy], [AddedFromIP], [AddedDate])

                    SELECT ttd.*,@companyGroupId,@companyId,@plantId,@fiscalYearCloseId,@addedBy,@addedFromIP,getdate()
					FROM(SELECT  AccountCodeId,ParallelCurrencyId,CurrencyCode,
		                                  SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
										, SUM(DRcumulative) DRcumulative, SUM(CRcumulative) CRcumulative
                                            , SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                                           , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
										   ,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
										 ,ISNULL(BudgetMasterId,'') BudgetMasterId
										 ,Activity,Particulars,ISNULL(ActivityId,'') ActivityId,ISNULL(BankMasterId,'') BankMasterId
										 ,ISNULL(CashMasterId,'') CashMasterId,ISNULL(PartyId,'') PartyId,ISNULL(PartyPlantId,'') PartyPlantId
		                                 FROM
		                                ( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative,0 PDRcumulative,0 PCRcumulative,       
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN PP.UserName
											ELSE ''	END,

                                            A.Id AS ActivityId, VD.BankMasterId, VD.CashMasterId, VD.PartyId, VD.PartyPlantId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE v.PostingDate < @startDate and v.CompanyId =@companyId AND V.PlantId=@plantId
                                            AND  v.IsPark=0
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId

											UNION 

											   SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,0 OBDRcumulative,0 OBCRcumulative,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS CRcumulative
                                 
                                           , 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative   
										    , SUM(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                           PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, SUM(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative,
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle 
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN PP.UserName
											ELSE ''	END,

                                            A.Id AS ActivityId, VD.BankMasterId, VD.CashMasterId, VD.PartyId, VD.PartyPlantId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, @startDate) AND CONVERT(DATE, @endDate) AND SourceType!='OpeningBalance' AND v.CompanyId =@companyId AND V.PlantId=@plantId
                                            AND  V.IsPark=0
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId
											 
                                            UNION
                                                        SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBCRcumulative,0 PDRcumulative,0 PCRcumulative
															, ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN PP.UserName
											ELSE ''	END,

                                            A.Id AS ActivityId, VD.BankMasterId, VD.CashMasterId, VD.PartyId, VD.PartyPlantId
	                                                FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
	                                               
                                                    WHERE V.PostingDate = @startDate AND V.CompanyId = @companyId AND V.PlantId = @plantId AND v.IsPark = 0 and v.SourceType='OpeningBalance'

                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId
											) TOTAL

											GROUP BY AccountCodeId,ParallelCurrencyId,CurrencyCode,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
		                                    ,BudgetMasterId,Activity,Particulars,ActivityId,BankMasterId,CashMasterId,PartyId,PartyPlantId
                                           
                                            )ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR
											ISNULL(OBDRcumulative,0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR
											ISNULL(CBDRcumulative,0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00
                                            OR ISNULL(PDRcumulative,0.00) <> 0.00 OR ISNULL(PCRcumulative,0) <> 0.00 ";
                    rdBuilder.Append(builderSql);
                    builderSql = @"  
UPDATE [SCS].[FiscalYearClose] SET VoucherId='" + voucher.Id + "' ,AdjustmentAmount = '" + totalDrAmount + "',TransactionType = '" + voucherDetailVMList.FirstOrDefault().TransactionTypeId + "'  WHERE Id='" + fiscalYearClosedata["Id"].ToString() + "'  ";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IWorkbook FiscalYearClosePostVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "FiscalYearClosePost";
            FixedAssetDisposeService _fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);
            var header = GetFiscalYearClosePostHeader(companyGroupId, companyId, plantId, voucherId, SourceType.YearCloseJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetVoucherCommonData(companyGroupId, companyId, plantId, voucherId, SourceType.YearCloseJournal);

            var transcationCurrency = header["CurrencyId"].ToString();
           
            _fixedAssetDisposeService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;


            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());



            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Fiscal Year");
            reportUtility.SetText(ref sheet, row, 2, header["FiscalYearName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Company");
            reportUtility.SetText(ref sheet, row, 5, header["CompanyName"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Plant");
            reportUtility.SetText(ref sheet, row, 2, header["PlantName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 5;
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;
            row++;  //10


            reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet[row, 4, row, 5].Merge();

            sheet[row, 6].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, 7].ColumnWidth = 15;
            sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; xlsCol++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
            colLast = xlsCol;
            sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);




            int formulaStartRow = 0;
            int formulaEndRow = 0;
            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                    reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));

                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                //reportUtility.CompanyPlantHeader(ref sheet, colLast, "Capitalize Asset Register", companyId, plantName, null);
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Fiscal Year Close Posting", companyId, plantId, plantName, null);

                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);


            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, "Fiscal Year Close Posting", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        private Dictionary<string, object> GetFiscalYearClosePostHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
			, V.CurrencyId, CR.Code AS CurrencyCode,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName
            FROM TRN.Voucher V 
		    INNER JOIN [SCS].[FiscalYearClose] FYC ON FYC.VoucherId=V.Id
			LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
			LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
            LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [SCS].[Currency] AS CR ON CR.Id=V.CurrencyId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
            WHERE v.Archive=0 AND v.CompanyGroupId='" + companyGroupId + "' AND v.CompanyId='" + companyId + "' AND v.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND v.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetVoucherCommonData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget

                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IV.Id=IVD.InvoiceWriteOffId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        #endregion
    }
}
